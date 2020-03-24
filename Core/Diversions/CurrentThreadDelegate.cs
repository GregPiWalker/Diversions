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
            : base(target, method)
        {
        }

        public override void Invoke(object sender, TArg arg)
        {
            DirectDelegate(sender, arg);
        }

        protected override void OnMarshalInfoSet()
        {
            // Empty impl.
        }
    }
}
