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
// Created: Saturday, August 06, 2011 1:01:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{

	/// <summary>
	/// Values for setting up a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">Gorgon Device Window</see>.
	/// </summary>
	public class GorgonDeviceWindowSettings
	{
		#region Classes.
		/// <summary>
		/// Values for advanced settings.
		/// </summary>
		public class GorgonDeviceWindowAdvancedSettings
		{
			#region Variables.
			private int _backBufferCount = 1;		// Back buffer count.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the number of back buffers in the swap chain.
			/// </summary>
			public int BackBufferCount
			{
				get
				{
					return _backBufferCount;
				}
				set
				{
					if (value < 1)
						value = 1;

					_backBufferCount = value;
				}
			}

			/// <summary>
			/// Property to set or return the vertical sync interval.
			/// </summary>
			/// <remarks>This value will GorgonVSyncInterval.None in windowed mode regardless of what's been set here.</remarks>
			public GorgonVSyncInterval VSyncInterval
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the flag to indicate that we will be using this device window for video.
			/// </summary>
			public bool WillUseVideo
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the multi sampling/Anti alias quality level.
			/// </summary>
			public GorgonMSAAQualityLevel? MSAAQualityLevel
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the display function used when displaying the contents of the swap chain.
			/// </summary>
			public GorgonDisplayFunction DisplayFunction
			{
				get;
				set;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonDeviceWindowAdvancedSettings"/> class.
			/// </summary>
			internal GorgonDeviceWindowAdvancedSettings()
			{
				BackBufferCount = 2;
				WillUseVideo = false;
				VSyncInterval = GorgonVSyncInterval.None;
				MSAAQualityLevel = null;
				DisplayFunction = GorgonDisplayFunction.Discard;
			}
			#endregion
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the window to bind to the device window.
		/// </summary>
		public Control BoundWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the width, height, format and refresh rate of the device window.
		/// </summary>
		public GorgonVideoMode? DisplayMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the depth/stencil buffer for the device window.
		/// </summary>
		/// <remarks>Set this to GorgonBufferFormat.Unknown if a depth/stencil buffer is not required.</remarks>
		public GorgonBufferFormat DepthStencilFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the device window should go to full screen or windowed mode.
		/// </summary>
		public bool Windowed
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the advanced settings for the device window.
		/// </summary>
		public GorgonDeviceWindowAdvancedSettings AdvancedSettings
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <remarks>Pass NULL to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">ApplicationWindow</see>.</remarks>
		public GorgonDeviceWindowSettings(Control boundWindow)
		{
			if (boundWindow == null)
				BoundWindow = Gorgon.ApplicationWindow;

#if DEBUG
			Windowed = true;
#else
			Windowed = false;
#endif
			DepthStencilFormat = GorgonBufferFormat.Unknown;
			DisplayMode = null;
			AdvancedSettings = new GorgonDeviceWindowAdvancedSettings();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		public GorgonDeviceWindowSettings()
			: this(null)
		{			
		}
		#endregion
	}
}
