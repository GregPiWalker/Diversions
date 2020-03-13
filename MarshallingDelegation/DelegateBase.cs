using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MarshallingDelegation
{
    public abstract class DelegateBase<TArg>
    {
        protected static readonly ILog _Logger = LogManager.GetLogger(typeof(DelegateBase<TArg>));

        internal MarshalInfo MarshalInfo { get; set; }

        public EventHandler<TArg> DirectDelegate { get; protected set; }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> represented by the delegate.
        /// </summary>
        /// <exception cref="MemberAccessException">The caller does not have access to the method represented by the delegate</exception>
        public MethodInfo DirectMethod { get; protected set; }

        /// <summary>
        /// Gets the object on which the current delegate invokes the instance method, if the
        /// delegate represents an instance method; null if the delegate represents a static
        /// method.
        /// </summary>
        public object DirectTarget { get; protected set; }

        public abstract void Invoke(object sender, TArg arg);
    }
}
