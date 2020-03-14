using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MarshallingDelegation
{
    public class MarshallingDelegate<TArg>
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(MarshallingDelegate<TArg>));
        private readonly List<DelegateBase<TArg>> _invocationList = new List<DelegateBase<TArg>>();

        public MarshallingDelegate()
        { }

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
            await Task.Run(() => Invoke(this, arg));
        }

        public void InvokeParallel(object sender, TArg arg)
        {
            foreach (var target in _invocationList)
            {
                Task.Run(() => target.Invoke(this, arg));
            }
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
