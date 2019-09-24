using System;
using System.Linq;
using System.Threading;
using Gorgon.Properties;

namespace Gorgon.Memory
{
    /// <summary>
    /// A memory allocation strategy that uses a circular buffer to perform memory allocations.
    /// </summary>
    /// <typeparam name="T">The type of objects to allocate.  These must be a reference type.</typeparam>
    /// <remarks>
    /// <para>
    /// While the .NET memory manager is quite fast (e.g. <c>new</c>), and is useful for most cases, it does have the problem of creating garbage. When these items are created and discarded, 
    /// the garbage collector may kick in at any given time, causing performance issues during time critical code (e.g. a rendering loop). By allocating a large pool of objects and then drawing 
    /// directly from this pool, we can reuse existing, but expired objects to ensure that the garbage collector does not collect these items until we are truly done with them.
    /// </para>
    /// <para>
    /// This allocator will allocate objects up until its total size, and then start over from the beginning if this value is exceeded. Objects returned will be reused.
    /// </para>
    /// <para>
    /// This allocator will never grow beyond its initial size. So care must be taken ahead of time to ensure the pool is large enough.
    /// </para>
    /// </remarks>
    public class GorgonRingPool<T>
        : IGorgonAllocator<T>
        where T : class
    {
        #region Variables.
        // The most current item in the heap.
        private int _currentItem = -1;
        // The items in the pool.
        private readonly T[] _items;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the allocator to use when creating new instances of an object.
        /// </summary>
        protected Func<T> ItemAllocator
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of items available to the allocator.
        /// </summary>
        public int TotalSize
        {
            get;
        }

        /// <summary>
        /// Property to return the number of available slots in the pool.
        /// </summary>
        /// <remarks>
        /// When this value is 0, then the pool is full and should be reset using the <see cref="Reset"/> method.
        /// </remarks>
        /// <seealso cref="Reset"/>
	    public int AvailableSlots => TotalSize - (_currentItem + 1);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to allocate a new object from the pool.
        /// </summary>
        /// <param name="initializer">[Optional] A function used to initialize the object returned by the allocator.</param>
        /// <returns>A reference to the object in the pool.</returns>
        /// <remarks>
        /// <para>
        /// This method returns the object from the pool.
        /// </para>
        /// <para>
        /// Applications can check to ensure that there is enough free space in the pool to allocate another object by checking the <see cref="AvailableSlots"/> property prior to calling this method. 
        /// If there is no more room, then the allocator will wrap around to the beginning of the pool and return objects from there.
        /// </para>
        /// <para>
        /// If the <paramref name="initializer"/> parameter is supplied, then this callback method can be used to initialize the new object before returning it from the allocator. If the object returned 
        /// is <b>null</b> (because an allocator was not supplied to the constructor), then this parameter will be ignored.
        /// </para>
        /// </remarks>
        /// <seealso cref="AvailableSlots"/>
        /// <seealso cref="Reset"/>
        public T Allocate(Action<T> initializer = null)
        {
            int nextIndex = Interlocked.Increment(ref _currentItem);

            // We're at the end, so wrap around.
            if (nextIndex >= _items.Length)
            {
                nextIndex = _currentItem = 0;
            }

            T item = _items[nextIndex];

            if ((ItemAllocator != null) && (item == null))
            {
                item = _items[nextIndex] = ItemAllocator();
            }

            if (item != null)
            {
                initializer?.Invoke(item);
            }

            return _items[nextIndex];
        }

        /// <summary>
        /// Function to reset the allocator heap and "free" all previous instances.
        /// </summary>
        /// <param name="dispose">[Optional] <b>true</b> to force objects that implement <see cref="IDisposable"/> to call their Dispose methods and reset the pool, or <b>false</b> to just reset the pool.</param>
        /// <remarks>
        /// <para>
        /// This method does not actually free any memory in the traditional sense, but merely resets the allocation pointer back to the beginning of the heap 
        /// to allow re-use of objects.
        /// </para>
        /// <para>
        /// If the <paramref name="dispose"/> parameter is <b>true</b>, and the objects in the pool implement <see cref="IDisposable"/>, then all objects will be traversed and their dispose methods will be 
        /// called. This may cause performance issues and should be used sparingly and the ideal is to not store <see cref="IDisposable"/> objects in the pool.
        /// </para>
        /// </remarks>
        public void Reset(bool dispose = false)
        {
            if (dispose)
            {
                foreach (IDisposable item in _items.OfType<IDisposable>())
                {
                    item.Dispose();
                }
            }

            Interlocked.Exchange(ref _currentItem, -1);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRingPool{T}"/> class.
        /// </summary>
        /// <param name="objectCount">The number of total objects available to the allocator.</param>
        /// <param name="allocator">[Optional] The allocator used to create an object in the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="objectCount"/> parameter is less than 1.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="allocator"/> parameter is not <b>null</b>, then the callback method passed will be executed for each object that has not been initialized in the pool (i.e. the object 
        /// would be returned as <b>null</b> when allocated). This ensures that the allocator can correctly initialize the object prior to allocation in the pool. If this parameter is omitted, then it is 
        /// up to the user to create the object if the allocator returns <b>null</b>.
        /// </para>
        /// </remarks>
        public GorgonRingPool(int objectCount, Func<T> allocator = null)
        {
            if (objectCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(objectCount), Resources.GOR_ERR_ALLOCATOR_SIZE_TOO_SMALL);
            }

            TotalSize = objectCount;
            _items = new T[objectCount];
            ItemAllocator = allocator;
        }
        #endregion
    }
}
