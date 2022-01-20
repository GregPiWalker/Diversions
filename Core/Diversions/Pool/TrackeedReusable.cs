using System;

namespace Diversions.Pool
{
    /// <summary>
    /// Thread-safe locking is performed on the self-reference in order to avoid extraneous object creation.
    /// </summary>
    public abstract class TrackedReusable : Reusable, ITrackedReusable
    {
        private uint _refCount;

        public uint RefCount => _refCount;

        public bool IsInUse
        {
            get { lock (this) return _refCount > 0; }
        }

        /// <summary>
        /// For reference counting use-cases, this adds another use of
        /// this Reusable.
        /// </summary>
        /// <returns></returns>
        public bool UseOnce()
        {
            lock (this)
            {
                if (IsPooled)
                {
                    return false;
                }

                _refCount++;
                return true;
            }
        }

        /// <summary>
        /// For reference counting use-cases, this removes a use of
        /// this Reusable.
        /// </summary>
        public void ReleaseOnce()
        {
            lock (this)
            {
                if (IsPooled || _refCount == 0)
                {
                    return;
                }

                _refCount--;
            }
        }

        /// <summary>
        /// Reset this reusable to an initial state that makes it suitable for re-use.
        /// </summary>
        public override void Reset()
        {
            lock (this)
            {
                if (_refCount != 0)
                {
                    return;
                }

                OnReset();
            }
        }
    }
}
