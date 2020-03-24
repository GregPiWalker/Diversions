using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Diversions
{
    internal class TaskDelegate<TArg> : DelegateBase<TArg>
    {
        internal delegate Task StartNew(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler);

        public TaskDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public TaskDelegate(object target, MethodInfo method)
            : base(target, method)
        {
        }

        public StartNew IndirectDelegate { get; private set; }

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

            IndirectDelegate(action, (CancellationToken)MarshalInfo.MethodInputs[1].Value, (TaskCreationOptions)MarshalInfo.MethodInputs[2].Value, (TaskScheduler)MarshalInfo.MethodInputs[3].Value);
        }

        protected override void OnMarshalInfoSet()
        {
            if (MarshalInfo == null)
            {
                IndirectDelegate = null;
            }
            else
            {
                // Create a delegate to the StartNew instance method.
                IndirectDelegate = (StartNew)Delegate.CreateDelegate(typeof(StartNew), MarshalInfo.Marshaller, MarshalInfo.MarshalMethod);
            }
        }
    }
}
