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
	/// Values for advanced settings.
	/// </summary>
	public struct GorgonDeviceWindowAdvancedSettings
	{
		#region Variables.
		/// <summary>
		/// Number of back buffers in the swap chain.
		/// </summary>
		public int BackBufferCount;
		/// <summary>
		/// Vertical sync interval.
		/// </summary>
		public GorgonVSyncInterval VSyncInterval;
		/// <summary>
		/// Flag to indicate that we will be using this device window for video.
		/// </summary>
		public bool WillUseVideo;
		/// <summary>
		/// Multi sampling/Anti alias quality level.
		/// </summary>
		public GorgonMSAAQualityLevel MSAAQualityLevel;
		/// <summary>
		/// The display function used when displaying the contents of the swap chain.
		/// </summary>
		public GorgonDisplayFunction DisplayFunction;
		#endregion
	}


	/// <summary>
	/// Values for setting up a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">Gorgon Device Window</see>.
	/// </summary>
	public struct GorgonDeviceWindowSettings
	{
		#region Variables.
		/// <summary>
		/// Control to bind to the device window.
		/// </summary>
		public Control BoundWindow;
		/// <summary>
		/// The width, height, format and refresh rate of the device window.
		/// </summary>
		public GorgonVideoMode? DisplayMode;
		/// <summary>
		/// The format of the depth/stencil buffer.  Use the Unknown format to skip the creation of the depth/stencil buffer.
		/// </summary>
		public GorgonBufferFormat DepthStencilFormat;
		/// <summary>
		/// TRUE to use windowed mode, FALSE to use full screen mode.
		/// </summary>
		public bool Windowed;
		#endregion
	}
}
