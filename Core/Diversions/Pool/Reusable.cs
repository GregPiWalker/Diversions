using System;

namespace Diversions.Pool
{
    public abstract class Reusable
    {
        private readonly object _reuseSync = new object();
        private bool _isFree;
        private uint _refCount;

        protected internal object ReuseSync => _reuseSync;

        internal bool IsFree { get; set; }

        internal uint RefCount => _refCount;

        public bool IsInUse
        {
            get { lock (_reuseSync) return _refCount > 0; }
        }

        public bool Use()
        {
            lock (_reuseSync)
            {
                if (_isFree)
                {
                    return false;
                }

                _refCount++;
                return true;
            }
        }

        public void Release()
        {
            lock (_reuseSync)
            {
                if (_isFree || _refCount == 0)
                {
                    return;
                }

                _refCount--;
            }
        }

        internal void Reset()
        {
            lock (_reuseSync)
            {
                if (_refCount != 0)
                {
                    return;
                }

                OnReset();
            }
        }

        protected virtual void OnReset()
        {
            //intentionally blank
        }
    }
}
