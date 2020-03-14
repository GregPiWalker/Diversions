using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Diversions
{
    internal class TaskDelegate<TArg> : DelegateBase<TArg>
    {
        public TaskDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public TaskDelegate(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        }

        public override void Invoke(object sender, TArg arg)
        {
            var current = SynchronizationContext.Current;
            // An invoker with static arguments was defined, so push the target action into the specified invoker
            // using a lambda so that closure is performed over the arguments.
            Action action = () => {
                try
                {
                    DirectDelegate(sender, arg);
                }
                catch (Exception ex)
                {
                    _Logger.Error($"{nameof(DirectDelegate)}.{nameof(DirectMethod.Invoke)}: {ex.GetType().Name} during method invocation.", ex);
                }
            };

            if (SynchronizationContext.Current != null && SynchronizationContext.Current == MarshalInfo.SynchronizationContext)
            {
                //TODO: Does this ever even happen?
                action();
            }
            else
            {
                var indirectArgs = new object[] { action };
                if (MarshalInfo.StaticArguments != null)
                {
                    indirectArgs = indirectArgs.Concat(MarshalInfo.StaticArguments).ToArray();
                }

                MarshalInfo.MarshalMethod.Invoke(MarshalInfo.Marshaller, indirectArgs);
            }
        }
    }
}
