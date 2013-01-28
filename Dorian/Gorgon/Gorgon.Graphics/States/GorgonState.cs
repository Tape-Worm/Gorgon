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
// Created: Saturday, January 26, 2013 3:48:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A base class for state types.
	/// </summary>
	/// <typeparam name="T">Type of state.</typeparam>
	public abstract class GorgonState<T>
		: IDisposable
		where T : struct, IEquatableByRef<T>
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private GorgonStateCacheNEW<T> _cache = null;						// Cache for the states.
		private T _state = default(T);										// Immutable state for the object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of state objects cached.
		/// </summary>
		protected int StateCacheCount
		{
			get
			{
				return _cache.CacheCount;
			}
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return a state to apply.
		/// </summary>
		/// <remarks>To set the default state, set this value to NULL (Nothing in VB.Net).</remarks>
		public virtual T States
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
				D3D.DeviceChild d3dState = null;

				// If the state is the same as the current, then leave.
				if (_state.Equals(ref value))
				{
					return;
				}

				_state = value;

				// Try to get the state from the cache first.
				d3dState = _cache.GetItem(ref value);

				if (d3dState == null)
				{
					// If we couldn't find it, then put a new one in the cache.
					d3dState = GetStateObject(ref value);
					_cache.SetItem(ref value, d3dState);
				}
								
				ApplyState(d3dState);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to store the state object in the cache.
		/// </summary>
		/// <param name="stateType">Type of state object.</param>
		/// <param name="stateObject">The D3D state object.</param>
		internal void StoreInCache(ref T stateType, D3D.DeviceChild stateObject)
		{
			_cache.SetItem(ref stateType, stateObject);
		}

		/// <summary>
		/// Function to retrieve the state object from the cache.
		/// </summary>
		/// <param name="stateType">Type of state object.</param>
		/// <returns>The state object if it's in the cache, NULL if not.</returns>
		internal D3D.DeviceChild GetFromCache(ref T stateType)
		{
			return _cache.GetItem(ref stateType);
		}

		/// <summary>
		/// Function to apply the state to the current rendering context.
		/// </summary>
		/// <param name="stateObject">State to apply.</param>
		internal abstract void ApplyState(D3D.DeviceChild stateObject);

		/// <summary>
		/// Function to retrieve the D3D state object.
		/// </summary>
		/// <param name="stateType">The state type information.</param>
		/// <returns>The D3D state object.</returns>
		internal abstract D3D.DeviceChild GetStateObject(ref T stateType);

		/// <summary>
		/// Function to purge a specific state from the cached states.
		/// </summary>
		/// <param name="stateType">State to purge.</param>
		public void Purge(ref T stateType)
		{
			_cache.Purge(ref stateType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonState{T}" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected GorgonState(GorgonGraphics graphics)
		{
			Graphics = graphics;
			_cache = new GorgonStateCacheNEW<T>();
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
					if (_cache != null)
					{
						_cache.Dispose();
					}					
				}

				_cache = null;
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
