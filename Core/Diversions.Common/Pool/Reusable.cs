using System;
using System.Collections.Generic;

namespace Diversions.Common.Pool
{
    /// <summary>
    /// Thread-safe locking is performed on the self-reference in order to avoid extraneous object creation.
    /// </summary>
    public abstract class Reusable : IReusable
    {
        /// <summary>
        /// Gets/sets a value indicating whether the object is in the object pool or not.
        /// </summary>
        public bool IsPooled { get; set; }

        Action<IReusable> IReusable.Return { get; set; }

        /// <summary>
        /// Initialize the reusable for use after it has been removed from the pool.
        /// </summary>
        /// <param name="initParameters">Optional set of parameters that will be used to initialize the object.</param>
        public virtual void Initialize(IEnumerable<KeyValuePair<string, object>> initParameters)
        {
            //intentionally blank
        }

        /// <summary>
        /// Configure this reusable back to a state that makes it suitable for return to the pool.
        /// </summary>
        public virtual void OnReturn()
        {
            lock (this)
            {
                OnReset();
            }
        }
        
        /// <summary>
        /// Configure this reusable back to a state that makes it suitable for return to the pool.
        /// </summary>
        protected virtual void OnReset()
        {
            //intentionally blank
        }
    }
}
