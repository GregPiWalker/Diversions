using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;

namespace Diversions
{
    /// <summary>
    /// For now, this is identical to the <see cref="TaskDelegate{TArg}"/> class, but it could diverge if it
    /// were to handle arguments in the future.
    /// </summary>
    /// <typeparam name="TArg">Type of EventHandler generic parameter</typeparam>
    internal class SchedulerDelegate<TArg> : DelegateBase<TArg>
    {
        internal delegate IDisposable SchedulerSchedule(IScheduler scheduler, Action action);

        public SchedulerDelegate(Delegate temporary)
            : this(temporary.Target, temporary.Method)
        {
        }

        public SchedulerDelegate(object target, MethodInfo method)
            : base(target, method)
        {
        }

        public SchedulerSchedule IndirectDelegate { get; private set; }

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

            IndirectDelegate((IScheduler)MarshalInfo.MethodInputs[0].Value, action);
        }

        protected override void OnMarshalInfoSet()
        {
            if (MarshalInfo == null)
            {
                IndirectDelegate = null;
            }
            else
            {
                // Create a delegate to the static Schedule method.
                IndirectDelegate = (SchedulerSchedule)Delegate.CreateDelegate(typeof(SchedulerSchedule), MarshalInfo.MarshalMethod, true);
            }
        }
    }
}
