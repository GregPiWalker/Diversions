using System;
using System.Reflection;
using System.Threading;

namespace MarshallingDelegation
{
    internal class DispatcherDelegate<TArg> : DelegateBase<TArg>
    {
        public DispatcherDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public DispatcherDelegate(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        }

        public override void Invoke(object sender, TArg arg)
        {
            var current = SynchronizationContext.Current;
            if (SynchronizationContext.Current != null && SynchronizationContext.Current == MarshalInfo.SynchronizationContext)
            {
                if (DirectDelegate == null)
                {
                    // No event handlers are connected.
                    return;
                }

                DirectDelegate(sender, arg);
            }
            else
            {
                // The defined marshaller's SyncrhonizationContext did not match the current context.
                // A marshaller with parameters was defined, so inject the target action into the specified marshaller.
                MarshalInfo.MarshalMethod.Invoke(MarshalInfo.Marshaller, new object[] { DirectDelegate, new object[] { sender, arg } });
            }
        }
    }
}
