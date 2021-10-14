using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;

namespace Diversions.Pool
{
    public class Pool<TReusable> : ObjectPool<TReusable> where TReusable : Reusable, new()
    {
        protected readonly object _syncLock = new object();
        //TODO: change to non concurrent data type
        private readonly ConcurrentBag<TReusable> _reusables = new ConcurrentBag<TReusable>();

        public Pool()
        {
        }

        public override TReusable Get()
        {
            lock (_syncLock)
            {
                TReusable reusable = _reusables.TryTake(out TReusable item) ? item : new TReusable();
                lock (reusable.ReuseSync)
                {
                    reusable.IsFree = false;
                    reusable.Use();
                }
                return reusable;
            }
        }

        public override void Return(TReusable reusable)
        {
            // Don't let the item be modified until it is both reset AND added to the collection.
            lock (reusable.ReuseSync)
            {
                reusable.Release();
                if (reusable.RefCount > 0)
                {
                    return;
                }

                lock (_syncLock)
                {
                    reusable.IsFree = true;
                    reusable.Reset();
                    _reusables.Add(reusable);
                }
            }
        }
    }
}
