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
	/// The graphics interface used to create a new instance of the graphics object and return an existing object upon subsequent calls.
	/// </summary>
	sealed class GraphicsFactory
		: IGraphicsFactory
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The application instance of the graphics interface.
		private GorgonGraphics _graphics;
		// Factory used to retrieve forms for the application.
		private readonly IFormFactory _factory;
		// The video device selector used to determine the best video device to use.
		private readonly IVideoDeviceSelector _deviceSelector;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the graphics interface instance.
		/// </summary>
		/// <returns>A new graphics interface if called for the first time, or an existing graphics interface if called again.</returns>
		/// <exception cref="GorgonException">Thrown when a suitable video could not be found for the application.</exception>
		public GorgonGraphics GetGraphics()
		{
			if (_graphics != null)
			{
				return _graphics;
			}

			FormSplash splash = _factory.CreateForm<FormSplash>(null, false);
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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsFactory"/> class.
		/// </summary>
		/// <param name="factory">The application form factory used to retrieve form instances.</param>
		/// <param name="deviceSelector">The video device selector to use when searching for an appropriate video device.</param>
		public GraphicsFactory(IFormFactory factory, IVideoDeviceSelector deviceSelector)
		{
			_factory = factory;
			_deviceSelector = deviceSelector;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GraphicsFactory"/> class.
		/// </summary>
		~GraphicsFactory()
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
	}
}
