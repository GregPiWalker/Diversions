using System;
using System.Reflection;
using System.Threading;

namespace Diversions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    internal class DispatcherDelegate<TArg> : DelegateBase<TArg>
    {
        internal delegate object DispatcherInvoke(Delegate d, params object[] a);
        private readonly object[] _innerArgs;

        public DispatcherDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public DispatcherDelegate(object target, MethodInfo method)
            : base(target, method)
        {
            _innerArgs = new object[2];
        }

        public DispatcherInvoke IndirectDelegate { get; private set; }

        public override void Invoke(object sender, TArg arg)
        {
            // The Dispatcher internally chooses whether to invoke directly or queue the work up,
            // so no need to bother doing that here.
            _innerArgs[0] = sender;
            _innerArgs[1] = arg;

            IndirectDelegate(DirectDelegate, _innerArgs);
        }

        protected override void OnMarshalInfoSet()
        {
            if (MarshalInfo == null)
            {
                IndirectDelegate = null;
            }
            else
            {
                // Create a delegate to the Invoke instance method.
                IndirectDelegate = (DispatcherInvoke)Delegate.CreateDelegate(typeof(DispatcherInvoke), MarshalInfo.Marshaller, MarshalInfo.MarshalMethod);
            }
        }
    }
}
