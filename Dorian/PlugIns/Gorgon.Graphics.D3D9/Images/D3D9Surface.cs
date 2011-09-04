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
// Created: Sunday, September 04, 2011 10:07:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A direct 3D 9 surface.
	/// </summary>
	class D3D9Surface
		: GorgonSurface, IUnmanagedObject
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the object was disposed.
		private D3D9DeviceWindow _deviceWindow = null;		// Device window.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current Direct 3D surface instance.
		/// </summary>
		public Surface D3DSurface
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_deviceWindow.UnmanagedObjects.Remove(this);

					if (D3DSurface != null)
						D3DSurface.Dispose();
				}

				D3DSurface = null;
				_disposed = true;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9Surface"/> class.
		/// </summary>
		/// <param name="name">The name of the surface.</param>
		/// <param name="deviceWindow">The device window that created the surface.</param>
		public D3D9Surface(string name, D3D9DeviceWindow deviceWindow)
			: base(name, deviceWindow)
		{
			_deviceWindow = deviceWindow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9Surface"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="deviceWindow">The device window.</param>
		/// <param name="currentSurface">The D3D surface to wrap.</param>
		public D3D9Surface(string name, D3D9DeviceWindow deviceWindow, Surface currentSurface)
			: this(name, deviceWindow)
		{
			D3DSurface = currentSurface;
		}
		#endregion

		#region IUnmanagedObject Members
		/// <summary>
		/// Function called when a device is placed in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			if (D3DSurface != null)
				D3DSurface.Dispose();								
		}

		/// <summary>
		/// Function called when a device is reset from a lost state.
		/// </summary>
		public void DeviceReset()
		{
			// TODO: Re-create.
		}
		#endregion
	}
}
