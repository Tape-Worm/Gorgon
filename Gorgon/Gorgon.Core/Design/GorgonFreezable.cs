using System.Threading;

namespace Gorgon.Design
{
    /// <summary>
    /// A base class used to define an object as frozen when accessing its members for writing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This base class will allow developers to mark an object as immutable. This is useful in cases where an object can be accessed for reading by multiple threads, but written to by a single thread.
    /// </para>
    /// </remarks>
    public abstract class GorgonFreezable
    {
        #region Variables.
        // Flag to indicate that the object is in a frozen state.
        private int _isFrozen;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the object is frozen or not.
        /// </summary>
        public bool IsFrozen => _isFrozen != 0;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to freeze the object.
        /// </summary>
        protected void Freeze() => Interlocked.Exchange(ref _isFrozen, -1);

        /// <summary>
        /// Function to thaw the object.
        /// </summary>
        protected void Thaw() => Interlocked.Exchange(ref _isFrozen, 0);
        #endregion
    }
}
