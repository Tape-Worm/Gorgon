#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, January 26, 2013 3:08:43 PM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A cache for state Direct 3D state objects.
	/// </summary>
	/// <typeparam name="T">Type of state to store.</typeparam>
	sealed class GorgonStateCacheNEW<T>
		: IDisposable
		where T : struct, IEquatableByRef<T>
	{
		#region Constants.
		private const int _cacheLimit = 4096;									// Number of objects that can initially live in the cache.
		#endregion

		#region Variables.
		private bool _disposed = false;																				// Flag to indicate that the object was disposed.
		private int _currentCachePosition = 0;																		// Current position in the cache.
		private int _cacheCount = 0;																				// Number of items in the cache.
		private Tuple<T, D3D.DeviceChild>[] _cache = new Tuple<T,D3D.DeviceChild>[_cacheLimit];						// Cache objects.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of objects cached.
		/// </summary>
		public int CacheCount
		{
			get
			{
				return _cacheCount;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the index of an existing state in the cache.
		/// </summary>
		/// <param name="state">State to check.</param>
		/// <returns>The index of the state, or -1 if not found.</returns>
		private int IndexOf(ref T state)
		{
			for (int i = 0; i < _cacheCount; i++)
			{
				if (_cache[i].Item1.Equals(ref state))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Function to evict the items from the cache.
		/// </summary>
		public void EvictCache()
		{
			for (int i = 0; i < _cacheCount; i++)
			{
				if (_cache[i].Item2 != null)
				{
					_cache[i].Item2.Dispose();
				}
				_cache[i] = null;
			}

			_cacheCount = 0;
			_currentCachePosition = 0;
		}

		/// <summary>
		/// Function to retrieve the state object to use.
		/// </summary>
		/// <param name="state">Type of state to get.</param>
		/// <returns>The D3D state object if found, or NULL if not.</returns>
		public D3D.DeviceChild GetItem(ref T state)
		{
			int index = IndexOf(ref state);

			return index == -1 ? null : _cache[index].Item2;
		}

		/// <summary>
		/// Function to put a state into the cache.
		/// </summary>
		/// <param name="state">Type of state to add.</param>
		/// <param name="d3dState">The D3D state object.</param>
		public void SetItem(ref T state, D3D.DeviceChild d3dState)
		{
			int index = IndexOf(ref state);

			if (index != -1)
			{
				_cache[index] = new Tuple<T, D3D.DeviceChild>(state, d3dState);
				return;
			}

			_cache[_currentCachePosition] = new Tuple<T, D3D.DeviceChild>(state, d3dState);

			_cacheCount++;
			_currentCachePosition++;

			if (_currentCachePosition >= _cache.Length)
			{
				Array.Resize<Tuple<T, D3D.DeviceChild>>(ref _cache, _cache.Length * 2);
			}
		}

		/// <summary>
		/// Function to purge a state from the cache.
		/// </summary>
		/// <param name="state">State to purge.</param>
		public void Purge(ref T state)
		{
			int index = IndexOf(ref state);
			int lastPosition = _currentCachePosition - 1;

			if (index != -1)
			{
				_cache[index].Item2.Dispose();

				if (index != lastPosition)
				{
					_cache[index] = _cache[lastPosition];
				}
				else
				{
					_cache[index] = null;
				}

				_cacheCount--;
				_currentCachePosition--;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStateCacheNEW{T}" /> class.
		/// </summary>
		public GorgonStateCacheNEW()
		{
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					EvictCache();
				}
				
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
