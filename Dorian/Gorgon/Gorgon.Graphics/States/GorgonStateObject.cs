#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Tuesday, November 22, 2011 7:37:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Base state object class.
	/// </summary>
	/// <typeparam name="T">Type of state description.</typeparam>
	public abstract class GorgonStateObject<T> 
		: IDisposable
		where T : struct, IEquatable<T>
	{
		#region Classes.
		/// <summary>
		/// A cached object.
		/// </summary>
		private class CachedObject
		{
			private DateTime _lastUsed;				// Time that the object was last used.
			private DateTime _lifeTime;				// Lifetime of the object.
			private int _lifeTimeMilliseconds = 0;	// The lifetime of the object, in milliseconds.

			/// <summary>
			/// Property to set or return the time that the object was last used.
			/// </summary>
			public DateTime LastUsed
			{
				get
				{
					return _lastUsed;
				}
			}

			/// <summary>
			/// Property to return whether this object is expired or not.
			/// </summary>
			public bool IsExpired
			{
				get
				{
					return DateTime.Now > _lifeTime;
				}
			}

			
			/// <summary>
			/// Object being cached.
			/// </summary>
			public IDisposable CacheObject
			{
				get;
				set;
			}

			/// <summary>
			/// Function to update the last used time for this object.
			/// </summary>
			public void Touch()
			{
				_lastUsed = DateTime.Now;
				_lifeTime = _lastUsed.AddMilliseconds(_lifeTimeMilliseconds);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonStateObject&lt;T&gt;.CachedObject"/> class.
			/// </summary>
			/// <param name="cacheObject">The cache object.</param>
			/// <param name="timeOutMilliseconds">The number of milliseconds the object has to live after the last use.</param>
			public CachedObject(IDisposable cacheObject, int timeOutMilliseconds)
			{
				Touch();
				CacheObject = cacheObject;
				_lifeTimeMilliseconds = timeOutMilliseconds;
			}
		}
		#endregion

		#region Variables.
		private bool _disposed = false;									// Flag to indicate that the object was disposed.
		private T _state = default(T);									// The immutable state for the object.		
		private IDictionary<T, CachedObject> _stateCache = null;		// State cache.
		private int _cacheLimit = 4096;									// Cache limit.
		private int _cacheObjectLifetime = 0;							// The lifetime of an object in the cache, in milliseconds.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of items in the state cache.
		/// </summary>
		protected int StateCacheCount
		{
			get
			{
				return _stateCache.Count;
			}
		}		

		/// <summary>
		/// Property to return the graphics interface that created this state object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the immutable states for this state object.
		/// </summary>
		public T States
		{
			get
			{
				return _state;
			}
			set
			{
#if DEBUG
				if (Graphics.Context == null)
					throw new InvalidOperationException("No usable context was found.");
#endif

				if (!_state.Equals(value))
				{
					_state = value;
					ApplyState(GetState());
				}
				else
					_stateCache[value].Touch();

				// Drop expired items if we are at or over our limit.
				if (_stateCache.Count >= _cacheLimit)
					EvictCache();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the cache.
		/// </summary>
		private void EvictCache()
		{
			// Evict the oldest items.
			var expired = (from cache in _stateCache
							where cache.Value.IsExpired
							select cache).ToList();

			// Clean the cache.
			while (expired.Count > 0)
			{
				expired[0].Value.CacheObject.Dispose();
				_stateCache.Remove(expired[0].Key);
				expired.Remove(expired[0]);
			}
		}

		/// <summary>
		/// Function to free any backing resources for the state.
		/// </summary>
		private void FreeResources()
		{
			if (_stateCache != null)
			{
				foreach (var item in _stateCache)
				{
					if (item.Value != null)
						item.Value.CacheObject.Dispose();
				}

				_stateCache.Clear();
			}
		}

		/// <summary>
		/// Function to get the state from the state cache.
		/// </summary>
		/// <returns>The Direct3D state object to apply.</returns>
		private IDisposable GetState()
		{
			CachedObject cacheObj = null;

			if (_stateCache.ContainsKey(States))
			{
				cacheObj = _stateCache[States];
				return cacheObj.CacheObject;
			}

			// Add to the cache.
			_stateCache[States] = cacheObj = new CachedObject(Convert(), _cacheObjectLifetime);
			return cacheObj.CacheObject;
		}

		/// <summary>
		/// Function to apply the state to the appropriate state object.
		/// </summary>
		/// <param name="state">The Direct3D state object to apply.</param>
		protected abstract void ApplyState(IDisposable state);

		/// <summary>
		/// Function to convert this state object to the native state object type.
		/// </summary>
		/// <returns>The Direct 3D state object.</returns>
		protected abstract IDisposable Convert();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStateObject&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		/// <param name="cacheLimit">Size limit for the cache.</param>
		/// <param name="cacheObjectLifetime">The lifetime of an object in the cache, in milliseconds.</param>
		protected GorgonStateObject(GorgonGraphics graphics, int cacheLimit, int cacheObjectLifetime)
		{
			Graphics = graphics;
			_stateCache = new Dictionary<T, CachedObject>(cacheLimit);
			_cacheObjectLifetime = cacheObjectLifetime;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					FreeResources();

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
