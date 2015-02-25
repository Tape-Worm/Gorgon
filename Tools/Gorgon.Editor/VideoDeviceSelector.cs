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
// Created: Tuesday, February 24, 2015 9:39:24 PM
// 
#endregion

using System.Linq;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A video device selector used to determine the best video device suited for the editor.
	/// </summary>
	/// <remarks>This object is only used at initialization.</remarks>
	class VideoDeviceSelector 
		: IVideoDeviceSelector
	{
		#region Variables.
		// The factory used to retrieve application forms.
		private readonly IFormFactory _factory;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine the best video device to use for the application.
		/// </summary>
		/// <returns>The best video device to use for the application, or NULL if no suitable device is found.</returns>
		public GorgonVideoDevice GetBestVideoDevice()
		{
			FormSplash splash = _factory.CreateForm<FormSplash>(null, false);

			// Initialize our graphics interface.
			splash.InfoText = Resources.GOREDIT_TEXT_INITIALIZE_GRAPHICS;

			// Find the best device in the system.
			GorgonVideoDeviceEnumerator.Enumerate(false, false);

			splash.InfoText = string.Format(Resources.GOREDIT_TEXT_FOUND_VIDEO_DEVICES, GorgonVideoDeviceEnumerator.VideoDevices.Count);

			if (GorgonVideoDeviceEnumerator.VideoDevices.Count == 0)
			{
				return null;
			}

			// Default to the first on the list.
			GorgonVideoDevice bestDevice = GorgonVideoDeviceEnumerator.VideoDevices[0];

			// If we have more than one device, use the best available device.
			if (GorgonVideoDeviceEnumerator.VideoDevices.Count > 1)
			{
				bestDevice = (from device in GorgonVideoDeviceEnumerator.VideoDevices
							  orderby device.SupportedFeatureLevel descending, GorgonVideoDeviceEnumerator.VideoDevices.IndexOf(device)
							  select device).FirstOrDefault();
			}

			return bestDevice;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoDeviceSelector"/> class.
		/// </summary>
		/// <param name="factory">The form factory used by the application.</param>
		public VideoDeviceSelector(IFormFactory factory)
		{
			_factory = factory;
		}
		#endregion
	}
}
