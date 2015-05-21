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

using Gorgon.Editor.Properties;
using Gorgon.Graphics;

namespace Gorgon.Editor
{
	/// <summary>
	/// A service used to bring back the graphics interface and the device selection interface.
	/// </summary>
	/// <remarks>The calling application is responsible for disposing the graphics object after calling the <see cref="Graphics"/> method.</remarks>
	sealed class GraphicsService 
		: IGraphicsService
	{
		#region Variables.
		// The application instance of the graphics interface.
		private GorgonGraphics _graphics;
		// The splash screen proxy object to display status.
		private readonly IProxyObject<FormSplash> _splashProxy;
		// The video device selector used to determine the best video device to use.
		private readonly IVideoDeviceSelector _deviceSelector;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsService"/> class.
		/// </summary>
		/// <param name="splashProxy">The proxy object holding our splash screen to allow us to return status to the user.</param>
		/// <param name="deviceSelector">The video device selector to use when searching for an appropriate video device.</param>
		public GraphicsService(IProxyObject<FormSplash> splashProxy, IVideoDeviceSelector deviceSelector)
		{
			_splashProxy = splashProxy;
			_deviceSelector = deviceSelector;
		}
		#endregion

		#region IGraphicsService Members
		/// <summary>
		/// Function to return the graphics interface.
		/// </summary>
		/// <returns>The new or previously existing graphics interface.</returns>
		/// <exception cref="GorgonException">Thrown when the graphics device could not be created because the video device in the system is not sufficient.</exception>
		public GorgonGraphics GetGraphics()
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
		#endregion
	}
}
