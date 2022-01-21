using System;
using System.Collections.Generic;

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
        /// Initialize the reusable for use after it has been removed from the pool.
        /// </summary>
        /// <param name="initParameters">Optional set of parameters that will be used to initialize the object.</param>
        void Initialize(IEnumerable<KeyValuePair<string, object>> initParameters);

        /// <summary>
        /// Configure this reusable back to a state that makes it suitable for return to the pool.
        /// </summary>
        void OnReturn();
    }
}