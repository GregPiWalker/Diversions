using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MarshallingDelegation
{
    //TODO probably delete this interface
    public interface IRedirect<TArg>
    {
        void Invoke(object sender, TArg arg);

        Task InvokeAsync(object sender, TArg arg);
    }

    public class MarshallingDelegate<TArg> //: IRedirect<TArg>
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(MarshallingDelegate<TArg>));
        //private readonly List<MarshallingDelegate<TArg>> _invocationList = new List<MarshallingDelegate<TArg>>();
        private readonly List<DelegateBase<TArg>> _invocationList = new List<DelegateBase<TArg>>();

        public MarshallingDelegate()
        { }

        //public MarshallingDelegate(Delegate temporary)
        //    : this(temporary.Target, temporary.Method)
        //{
        //}

        //public MarshallingDelegate(object target, MethodInfo method)
        //{
        //    DirectTarget = target;
        //    DirectMethod = method;
        //    DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        //}

        //internal MarshalInfo MarshalInfo { get; set; }

        //public EventHandler<TArg> DirectDelegate { get; private set; }
        
        ///// <summary>
        ///// Gets the <see cref="MethodInfo"/> represented by the delegate.
        ///// </summary>
        ///// <exception cref="MemberAccessException">The caller does not have access to the method represented by the delegate</exception>
        //public MethodInfo DirectMethod { get; private set; }
        
        ///// <summary>
        ///// Gets the object on which the current delegate invokes the instance method, if the
        ///// delegate represents an instance method; null if the delegate represents a static
        ///// method.
        ///// </summary>
        //public object DirectTarget { get; private set; }

        //public void Invoke(object sender, TArg arg)
        //{
        //    if (_invocationList.Count == 0)
        //    {
        //        var current = SynchronizationContext.Current;
        //        if (MarshalInfo == null || MarshalInfo.MarshalMethod == null || MarshalInfo.Marshaller == null)
        //        {
        //            // Either there was no custom marshaller defined for the delegate target,
        //            // or there is no delegate at all.

        //            if (DirectDelegate == null)
        //            {
        //                // No event handlers are connected.
        //                return;
        //            }

        //            DirectDelegate(sender, arg);
        //        }
        //        else if (MarshalInfo.MethodParameters.Length == 2)
        //        {
        //            if (SynchronizationContext.Current != null && SynchronizationContext.Current == MarshalInfo.SynchronizationContext)
        //            {
        //                if (DirectDelegate == null)
        //                {
        //                    // No event handlers are connected.
        //                    return;
        //                }

        //                DirectDelegate(sender, arg);
        //            }
        //            else
        //            {
        //                // The defined marshaller's SyncrhonizationContext did not match the current context.
        //                // A marshaller with parameters was defined, so inject the target action into the specified marshaller.
        //                MarshalInfo.MarshalMethod.Invoke(MarshalInfo.Marshaller, new object[] { DirectDelegate, new object[] { sender, arg } });
        //            }
        //        }
        //        else
        //        {
        //            // An invoker with static arguments was defined, so push the target action into the specified invoker
        //            // using a lambda so that closure is performed over the arguments.
        //            Action action = () => {
        //                try
        //                {
        //                    DirectDelegate(sender, arg);
        //                }
        //                catch (Exception ex)
        //                {
        //                    _Logger.Error($"{nameof(DirectDelegate)}.{nameof(DirectMethod.Invoke)}: {ex.GetType().Name} during method invocation.", ex);
        //                }
        //            };

        //            if (SynchronizationContext.Current != null && SynchronizationContext.Current == MarshalInfo.SynchronizationContext)
        //            {
        //                //TODO: Does this ever even happen?
        //                action();
        //            }
        //            else
        //            {
        //                var indirectArgs = new object[] { action };
        //                if (MarshalInfo.StaticArguments != null)
        //                {
        //                    indirectArgs = indirectArgs.Concat(MarshalInfo.StaticArguments).ToArray();
        //                }

        //                MarshalInfo.MarshalMethod.Invoke(MarshalInfo.Marshaller, indirectArgs);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var target in _invocationList)
        //        {
        //            target.Invoke(sender, arg);
        //        }
        //    }
        //}


        public void Invoke(object sender, TArg arg)
        {
            foreach (var target in _invocationList)
            {
                target.Invoke(sender, arg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public async Task InvokeAsync(object sender, TArg arg)
        {
            await Task.Run(() => Invoke(this, arg)).ConfigureAwait(false);
        }

        //TODO: Disable this until I confirm that the threading does what was intended
        //public async void InvokeParallel(object sender, TArg arg)
        //{
        //    if (_invocationList.Count == 0)
        //    {
        //        await InvokeAsync(sender, arg);
        //    }
        //    else
        //    {
        //        foreach (var target in _invocationList)
        //        {
        //            await target.InvokeAsync(sender, arg);
        //        }
        //    }
        //}

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
            //var newDel = new MarshallingDelegate<TArg>(toAdd);

            DelegateBase<TArg> newDel = null;
            var attrib = toAdd.Method.GetCustomAttribute(typeof(ThreadedHandlerAttribute)) as ThreadedHandlerAttribute;
            if (attrib == null)
            {
                // This applies the configured default target loader if none is found.
                attrib = new ThreadedHandlerAttribute();
            }

            switch (attrib.SelectedOption)
            {
                case MarshalOption.CurrentThread:
                    newDel = new CurrentThreadDelegate<TArg>(toAdd);
                    break;

                case MarshalOption.StartNewTask:
                case MarshalOption.RunTask:
                    newDel = new TaskDelegate<TArg>(toAdd);
                    break;

                case MarshalOption.Dispatcher:
                    newDel = new DispatcherDelegate<TArg>(toAdd);
                    break;
            }

            newDel.MarshalInfo = attrib.MarshalInfo;
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
            DelegateBase<TArg> removed = null;
            lock (_invocationList)
            {
                foreach (var redirect in _invocationList)
                {
                    if ((redirect.DirectTarget == toRemove.Target && redirect.DirectMethod == toRemove.Method)
                        || (redirect.MarshalInfo.Marshaller == toRemove.Target && redirect.MarshalInfo.MarshalMethod == toRemove.Method))
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

        public static MarshallingDelegate<TArg> operator +(MarshallingDelegate<TArg> result, Delegate toAdd)
        {
            result.Add(toAdd);
            return result;
        }

        public static MarshallingDelegate<TArg> operator -(MarshallingDelegate<TArg> result, Delegate toRemove)
        {
            result.Remove(toRemove);
            return result;
        }
    }
}
