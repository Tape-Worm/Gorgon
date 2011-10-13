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
// Created: Wednesday, October 12, 2011 6:57:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for defining a swap chain.
	/// </summary>
	public class GorgonSwapChainSettings
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the video device for the settings.
		/// </summary>
		public GorgonVideoDevice VideoDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the output to use.
		/// </summary>
		/// <remarks>Leave this value as NULL (Nothing in VB.Net) to choose the default output.</remarks>
		public GorgonVideoOutput Output
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the video mode to use.
		/// </summary>
		/// <remarks>If this value is NULL (Nothing in VB.Net), then Gorgon will attempt to find the best mode possible to use.</remarks>
		public GorgonVideoMode? Mode
		{
			get;
			set;
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainSettings"/> class.
		/// </summary>
		/// <param name="videoDevice">The video device for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="videoDevice"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonSwapChainSettings(GorgonVideoDevice videoDevice)
		{
			if (videoDevice == null)
				throw new ArgumentNullException("videoDevice");

			VideoDevice = videoDevice;
		}
		#endregion
	}
}
