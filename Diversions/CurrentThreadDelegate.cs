using System;
using System.Reflection;

namespace Diversions
{
    internal class CurrentThreadDelegate<TArg> : DelegateBase<TArg>
    {
        public CurrentThreadDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public CurrentThreadDelegate(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        }

        public override void Invoke(object sender, TArg arg)
        {
            // Either there was no custom marshaller defined for the delegate target,
            // or there is no delegate at all.

            if (DirectDelegate == null)
            {
                // No event handlers are connected.
                return;
            }

            DirectDelegate(sender, arg);
        }
    }
}
