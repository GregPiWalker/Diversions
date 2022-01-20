namespace Diversions.Pool
{
    public interface ITrackedReusable : IReusable
    {
        /// <summary>
        /// Gets the reference count of this reusable, if counting is being used.
        /// </summary>
        uint RefCount { get; }

        /// <summary>
        /// Remove a use of this reusable; decrements the reference count.
        /// </summary>
        void ReleaseOnce();

        /// <summary>
        /// Mark the reusable as being used; increments the reference count.
        /// </summary>
        /// <returns>True if the reusable was successfully marked as used; false otherwise.</returns>
        bool UseOnce();
    }
}