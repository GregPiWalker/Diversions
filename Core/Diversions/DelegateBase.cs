using log4net;
using System;
using System.Reflection;

namespace Diversions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    public abstract class DelegateBase<TArg>
    {
        protected static readonly ILog _Logger = LogManager.GetLogger(typeof(DelegateBase<TArg>));
        private MarshalInfo _marshalInfo;

        protected DelegateBase(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TArg>)Delegate.CreateDelegate(typeof(EventHandler<TArg>), target, method);
        }

        internal MarshalInfo MarshalInfo 
        { 
            get => _marshalInfo;
            set
            {
                _marshalInfo = value;
                OnMarshalInfoSet();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler<TArg> DirectDelegate { get; private set; }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> represented by the delegate.
        /// </summary>
        /// <exception cref="MemberAccessException">The caller does not have access to the method represented by the delegate</exception>
        public MethodInfo DirectMethod { get; private set; }

        /// <summary>
        /// Gets the object on which the current delegate invokes the instance method, if the
        /// delegate represents an instance method; null if the delegate represents a static
        /// method.
        /// </summary>
        public object DirectTarget { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        public abstract void Invoke(object sender, TArg arg);

        /// <summary>
        /// 
        /// </summary>
        protected abstract void OnMarshalInfoSet();
    }
}
