using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;

namespace Diversions.Common.Pool
{
    /// <summary>
    /// Implements a thread-safe <see cref="ObjectPool{T}"/> with optional reference counting.
    /// </summary>
    /// <typeparam name="TReusable"></typeparam>
    public class Pool<TReusable> : ObjectPool<TReusable> where TReusable : class, IReusable, new()
    {
        protected readonly object _syncLock = new object();
        
        private readonly List<TReusable> _reusables = new List<TReusable>();

        public Pool()
        {
        }

        /// <summary>
        /// Gets/sets the configuration for how the Pool treats reference counting.  When reference counting
        /// is enabled, the Pool will not return a referenced object.  When it is ignored, objects will
        /// be returned regardless of their reference count.
        /// </summary>
        public bool UseReferenceCounting { get; set; }

        public override TReusable Get()
        {
            lock (_syncLock)
            {
                bool fromPool = false;
                TReusable reusable;
                if (_reusables.Count > 0)
                {
                    fromPool = true;
                    int end = _reusables.Count - 1;
                    reusable = _reusables[end];
                    _reusables.RemoveAt(end);
                }
                else
                {
                    reusable = new TReusable();
                    reusable.Return = (r) => Return(r as TReusable);
                }

                if (reusable is ITrackedReusable tracked && fromPool)
                {
                    lock (tracked)
                    {
                        tracked.IsPooled = false;
                        tracked.UseOnce();
                    }
                }
                else
                {
                    // Shouldn't need to lock the Reusable here because nobody else has a reference to it or we just don't care.
                    reusable.IsPooled = false;
                }

                return reusable;
            }
        }

        public override void Return(TReusable reusable)
        {
            if (reusable == null)
            {
                return;
            }

            if (reusable is ITrackedReusable tracked)
            {
                // Don't let the reusable be modified until it is both reset AND added to the collection.
                // Locking the reusable is done to handle the use-case of reference counting. 
                lock (tracked)
                {
                    // Disallow returning to pool if object already in pool.
                    if (reusable.IsPooled)
                    {
                        return;
                    }

                    tracked.ReleaseOnce();
                    if (tracked.RefCount > 0)
                    {
                        return;
                    }

                    lock (_syncLock)
                    {
                        reusable.IsPooled = true;
                        reusable.Reset();
                        _reusables.Add(reusable);
                    }
                }
            }
            else
            {
                lock (_syncLock)
                {
                    // Disallow returning to pool if object already in pool.
                    if (reusable.IsPooled)
                    {
                        return;
                    }

                    reusable.IsPooled = true;
                    reusable.Reset();
                    _reusables.Add(reusable);
                }
            }
        }
    }
}
