using System;

namespace Diversions.Pool
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
        /// Reset this reusable to an initial state that makes it suitable for re-use.
        /// </summary>
        public virtual void Reset()
        {
            lock (this)
            {
                OnReset();
            }
        }

        protected virtual void OnReset()
        {
            //intentionally blank
        }
    }
}
