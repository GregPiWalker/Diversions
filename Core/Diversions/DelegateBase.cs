using log4net;
using System;
using System.Reflection;

namespace Diversions
{
    /// <summary>
    /// An abstract delegate base class to support event diversions.  .NET does not allow
    /// extension of their <see cref="Delegate"/> class, hence the lack of an ancestor here.
    /// </summary>
    /// <typeparam name="TEvArg">The type of the generic EventArgs.</typeparam>
    public abstract class DelegateBase<TEvArg>
    {
        protected static readonly ILog _Logger = LogManager.GetLogger(typeof(DelegateBase<TEvArg>));
        private MarshalInfo _marshalInfo;

        protected DelegateBase(object target, MethodInfo method)
        {
            DirectTarget = target;
            DirectMethod = method;
            DirectDelegate = (EventHandler<TEvArg>)Delegate.CreateDelegate(typeof(EventHandler<TEvArg>), target, method);
        }

        internal MarshalInfo MarshalInfo 
        { 
            get { return _marshalInfo; }
            set
            {
                _marshalInfo = value;
                OnMarshalInfoSet();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler<TEvArg> DirectDelegate { get; private set; }

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
        public abstract void Invoke(object sender, TEvArg arg);

        /// <summary>
        /// 
        /// </summary>
        protected abstract void OnMarshalInfoSet();
    }
}
