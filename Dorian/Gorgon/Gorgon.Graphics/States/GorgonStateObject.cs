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
		#region Variables.
		private bool _disposed = false;									// Flag to indicate that the object was disposed.
		private T _state = default(T);									// The immutable state for the object.		
		private IList<Tuple<T, IDisposable>> _stateCache = null;		// State cache.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the cache pointer.
		/// </summary>
		protected int CachePosition
		{
			get;
			private set;
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
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to free any backing resources for the state.
		/// </summary>
		private void FreeResources()
		{
			if (_stateCache != null)
			{
				foreach (var item in _stateCache)
				{
					if (item.Item2 != null)
						item.Item2.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to get the state from the state cache.
		/// </summary>
		/// <returns>The Direct3D state object to apply.</returns>
		private IDisposable GetState()
		{
			var state = (from stateItem in _stateCache
						where stateItem.Item1.Equals(States) && stateItem.Item2 != null
						select stateItem.Item2).FirstOrDefault();

			if (state != null)
				return state;

			// If we have an entry here already, overwrite it.
			if (_stateCache[CachePosition].Item2 != null)
				_stateCache[CachePosition].Item2.Dispose();

			// Add to the cache.
			state = Convert();
			_stateCache[CachePosition] = new Tuple<T, IDisposable>(States, state);

			CachePosition++;
			if (CachePosition >= _stateCache.Count)
				CachePosition = 0;

			return state;
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
		protected GorgonStateObject(GorgonGraphics graphics)
		{
			Graphics = graphics;
			_stateCache = new Tuple<T, IDisposable>[4096];
			for(int i = 0; i < _stateCache.Count; i++)
				_stateCache[i] = new Tuple<T, IDisposable>(new T(), null);
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
				{
					FreeResources();

					if (Graphics != null)
						Graphics.RemoveTrackedObject(this);
				}

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
