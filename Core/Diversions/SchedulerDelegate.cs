using System;
using System.Linq;
using System.Reflection;

namespace Diversions
{
    /// <summary>
    /// For now, this is identical to the <see cref="TaskDelegate{TArg}"/> class, but it could diverge if it
    /// were to handle arguments in the future.
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    internal class SchedulerDelegate<TArg> : DelegateBase<TArg>
    {
        public SchedulerDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public SchedulerDelegate(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        }

        public override void Invoke(object sender, TArg arg)
        {
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

            // Plug the Action argument into the input list.
            object[] args = new object[MarshalInfo.MethodInputs.Length];
            for (int i = 0; i < args.Length; i++)
            {                
                if (MarshalInfo.MethodInputs[i].Key == typeof(Action) && MarshalInfo.MethodInputs[i].Value == null)
                {
                    args[i] = action;
                }
                else
                {
                    args[i] = MarshalInfo.MethodInputs[i].Value;
                }
            }

            MarshalInfo.MarshalMethod.Invoke(MarshalInfo.Marshaller, args);
        }
    }
}
