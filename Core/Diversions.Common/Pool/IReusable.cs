using System;

namespace Diversions.Common.Pool
{
    public interface IReusable
    {
        /// <summary>
        /// Gets/sets an action that allows this reusable to return itself to its origin pool.
        /// </summary>
        Action<IReusable> Return { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether the object is in the object pool or not.
        /// </summary>
        bool IsPooled { get; set; }

        /// <summary>
        /// Reset this reusable to an initial state that makes it suitable for re-use.
        /// </summary>
        void Reset();
    }
}