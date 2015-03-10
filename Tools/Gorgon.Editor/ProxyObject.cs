#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, February 26, 2015 9:25:10 PM
// 
#endregion

using System;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A proxy object used to create objects with a short lifetime.
	/// </summary>
	/// <typeparam name="T">Type of object.</typeparam>
	class ProxyObject<T>
		: IProxyObject<T>
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The item being proxied.
		private Lazy<T> _item;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ProxyObject{T}"/> class.
		/// </summary>
		/// <param name="sourceObject">The source object to proxy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="sourceObject"/> is NULL (Nothing in VB.Net).</exception>
		public ProxyObject(Lazy<T> sourceObject)
		{
			if (sourceObject == null)
			{
				throw new ArgumentNullException("sourceObject");
			}

			_item = sourceObject;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ProxyObject{T}"/> class.
		/// </summary>
		~ProxyObject()
		{
			Dispose(false);	
		}
		#endregion

		#region IProxyObject<T> Members
		/// <summary>
		/// Property to return the proxied item.
		/// </summary>
		public T Item
		{
			get
			{
				if (_disposed)
				{
					throw new ObjectDisposedException(Resources.GOREDIT_ERR_CANNOT_USE_DISPOSED_OBJECT);
				}
				
				return _item == null ? default(T) : _item.Value;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_item.IsValueCreated)
				{
					var disposer = _item.Value as IDisposable;

					if (disposer != null)
					{
						disposer.Dispose();
					}
				}
			}

			_item = null;
			_disposed = true;
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
