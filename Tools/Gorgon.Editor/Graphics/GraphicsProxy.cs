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
// Created: Tuesday, February 24, 2015 9:27:01 PM
// 
#endregion

using System;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The proxy used to create a new instance of the graphics object and return an existing object upon subsequent calls.
	/// </summary>
	sealed class GraphicsProxy
		: IProxyObject<GorgonGraphics>
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The application instance of the graphics interface.
		private GorgonGraphics _graphics;
		// The splash screen proxy object to display status.
		private readonly IProxyObject<FormSplash> _splashProxy;
		// The video device selector used to determine the best video device to use.
		private readonly IVideoDeviceSelector _deviceSelector;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsProxy"/> class.
		/// </summary>
		/// <param name="splashProxy">The proxy object holding our splash screen to allow us to return status to the user.</param>
		/// <param name="deviceSelector">The video device selector to use when searching for an appropriate video device.</param>
		public GraphicsProxy(IProxyObject<FormSplash> splashProxy, IVideoDeviceSelector deviceSelector)
		{
			_splashProxy = splashProxy;
			_deviceSelector = deviceSelector;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GraphicsProxy"/> class.
		/// </summary>
		~GraphicsProxy()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Implementation.
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
				if (_graphics != null)
				{
					_graphics.Dispose();
				}
			}

			_graphics = null;
			_disposed = true;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IProxyObject<GorgonGraphics> Members
		/// <summary>
		/// Property to return the proxied item.
		/// </summary>
		public GorgonGraphics Item
		{
			get
			{
				if (_graphics != null)
				{
					return _graphics;
				}

				FormSplash splash = _splashProxy.Item;
				GorgonVideoDevice device = _deviceSelector.GetBestVideoDevice();

				if (device == null)
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_CANNOT_CREATE_GFX);
				}

				// Create our graphics interface.
				_graphics = new GorgonGraphics(device);

				splash.InfoText = string.Format(Resources.GOREDIT_TEXT_USING_VIDEO_DEVICE, device.Name, device.SupportedFeatureLevel);

				return _graphics;
			}
		}
		#endregion
	}
}
