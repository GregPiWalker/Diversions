﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading.Tasks;

namespace Diversions
{
    /// <summary>
    /// This class has an invocation list and acts like a MultiCastDelegate, so multiple delegates can
    /// be added to it where each delegate marshals its work according to its diversion marshal option.
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    public class DiversionDelegate<TArg>
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(DiversionDelegate<TArg>));
        private readonly List<DelegateBase<TArg>> _invocationList = new List<DelegateBase<TArg>>();

        public DiversionDelegate()
        { }

        public bool HasListeners { get { return _invocationList.Any(); } }

        /// <summary>
        /// Synchronously invoke this <see cref="DiversionDelegate{TArg}"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        public void Invoke(object sender, TArg arg)
        {
            foreach (var target in _invocationList)
            {
                target.Invoke(sender, arg);
            }
        }

        /// <summary>
        /// Asynchronously invoke this <see cref="DiversionDelegate{TArg}"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        /// <returns>A task that invokes this delegate.</returns>
        public async Task InvokeAsync(object sender, TArg arg)
        {
            await Task.Run(() => Invoke(this, arg));
        }

        public void InvokeParallel(object sender, TArg arg)
        {
            _invocationList.AsParallel().ForAll(target => target.Invoke(this, arg));
        }

        /// <summary>
        /// Returns the invocation list of the delegate.
        /// </summary>
        /// <returns>An array of delegates representing the invocation list of the current delegate.</returns>
        public DelegateBase<TArg>[] GetInvocationList()
        {
            return _invocationList.ToArray();
        }

        public void Add(DelegateBase<TArg> toAdd)
        {
            lock (_invocationList)
            {
                _invocationList.Add(toAdd);
            }
        }

        public DelegateBase<TArg> Add(Delegate toAdd)
        {
            var paramType = toAdd.Method.GetParameters().Last().ParameterType;
            if (paramType != typeof(TArg))
            {
                throw new ArgumentException($"Wrong delegate type supplied.  Got {paramType.Name}, expected {typeof(TArg).Name}.", "toAdd");
            }

            DelegateBase<TArg> newDel = null;

            // First look for a method-level diversion override.
            var diversion = toAdd.Method.GetCustomAttribute(typeof(DiversionAttribute)) as DiversionAttribute;
            if (diversion == null)
            {
                // If method-level didn't exist, look for a class-level diversion.
                if (toAdd.Target != null)
                {
                    // If there's an instance, find the Type from it.
                    diversion = toAdd.Target.GetType().GetCustomAttribute(typeof(DiversionAttribute)) as DiversionAttribute;
                }
                else
                {
                    // In the static case, find the Type from the MethodInfo.
                    diversion = toAdd.Method.DeclaringType.GetCustomAttribute(typeof(DiversionAttribute)) as DiversionAttribute;
                }
            }

            if (diversion == null)
            {
                // If neither a class nor a method Diversion were defined, then just go with the global default.
                diversion = new DiversionAttribute();
            }

            //TODO: move this to Factory or another method ?
            switch (diversion.SelectedDiverter)
            {
                case MarshalOption.CurrentThread:
                    newDel = new CurrentThreadDelegate<TArg>(toAdd);
                    break;

                //case MarshalOption.StartNewTask:
                case MarshalOption.RunTask:
                    newDel = new TaskDelegate<TArg>(toAdd);
                    break;

                case MarshalOption.Dispatcher:
                    newDel = new DispatcherDelegate<TArg>(toAdd);
                    break;

                case MarshalOption.UserDefined:
                    // Search for expected user-defined types.
                    if (diversion.MarshalInfo.Marshaller is IScheduler || (Type)diversion.MarshalInfo.Marshaller == typeof(Scheduler))
                    {
                        newDel = new SchedulerDelegate<TArg>(toAdd);
                    }
                    else
                    {
                        throw new Exception("Unknown user-defined delegate type.  Make sure a Diverter was defined for it in Diversion's static Diverter set.");
                    }
                    break;
            }

            newDel.MarshalInfo = diversion.MarshalInfo;
            Add(newDel);
            return newDel;
        }

        public void Remove(DelegateBase<TArg> toRemove)
        {
            lock (_invocationList)
            {
                _invocationList.Remove(toRemove);
            }
        }

        public DelegateBase<TArg> Remove(Delegate toRemove)
        {
            if (toRemove.Method.GetParameters().Last().ParameterType != typeof(TArg))
            {
                throw new ArgumentException("toRemove: wrong delegate type");
            }

            DelegateBase<TArg> removed = null;
            lock (_invocationList)
            {
                foreach (var redirect in _invocationList)
                {
                    if ((redirect.DirectTarget == toRemove.Target && redirect.DirectMethod == toRemove.Method)
                        || (redirect.MarshalInfo != null && redirect.MarshalInfo.Marshaller == toRemove.Target && redirect.MarshalInfo.MarshalMethod == toRemove.Method))
                    {
                        removed = redirect;
                        break;
                    }
                }

                if (removed != null)
                {
                    _invocationList.Remove(removed);
                }
            }

            return removed;
        }

        public static DiversionDelegate<TArg> operator +(DiversionDelegate<TArg> result, Delegate toAdd)
        {
            result.Add(toAdd);
            return result;
        }

        public static DiversionDelegate<TArg> operator -(DiversionDelegate<TArg> result, Delegate toRemove)
        {
            result.Remove(toRemove);
            return result;
        }
    }
}
