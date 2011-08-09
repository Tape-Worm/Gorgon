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
// Created: Sunday, July 24, 2011 9:40:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A video output on a video device.
	/// </summary>
	/// <remarks>A video output is a head on the video device.</remarks>
	public abstract class GorgonVideoOutput
	{
		#region Properties.
		/// <summary>
		/// Property to return the device name of the output.
		/// </summary>
		public string Name
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the display dimensions of the desktop.
		/// </summary>
		public Rectangle DesktopDimensions
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the output device is attached to the desktop.
		/// </summary>
		public bool IsAttachedToDesktop
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the rotation in degrees for the monitor.
		/// </summary>
		public int Rotation
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the video modes for this output.
		/// </summary>
		public GorgonVideoModeList VideoModes
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default video mode for this output.
		/// </summary>
		public GorgonVideoMode DefaultVideoMode
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the video modes for this output.
		/// </summary>
		/// <returns>An enumerable list of video modes.</returns>
		protected abstract IEnumerable<GorgonVideoMode> GetVideoModes();

		/// <summary>
		/// Function to retrieve the video modes for the output.
		/// </summary>
		internal void GetOutputModes()
		{
			VideoModes = new GorgonVideoModeList(GetVideoModes());
		}

		/// <summary>
		/// Function to determine if a specified display format is supported by the hardware or not.
		/// </summary>
		/// <param name="format">Backbuffer format to check.</param>
		/// <param name="isWindowed">TRUE if in windowed mode, FALSE if not.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		/// <remarks>Some devices can handle having a
		/// display mode with one format type, and a backbuffer of another format type.  This method will determine if the specific output is capable of this.
		/// <para>Implementors may choose to override this to determine if a specific device format is supported in hardware.  But it is not required.</para>
		/// </remarks>
		public virtual bool SupportsBackBufferFormat(GorgonDisplayFormat format, bool isWindowed)
		{
			return true;
		}

		/// <summary>
		/// Function to determine if a depth/stencil format can be used with a specific display format.
		/// </summary>
		/// <param name="displayFormat">Display format to use.</param>
		/// <param name="targetFormat">Format of the render target.</param>
		/// <param name="depthStencilFormat">Depth/stencil format to check.</param>
		/// <param name="isWindowed">TRUE if using windowed mode, FALSE if not.</param>
		/// <returns>TRUE if the depth stencil type is supported, FALSE if not.</returns>
		/// <remarks>Implementors may choose to override this to determine if a specific device format is supported in hardware.  But it is not required.</remarks>
		public virtual bool SupportsDepthFormat(GorgonDisplayFormat displayFormat, GorgonBufferFormat targetFormat, GorgonDepthBufferFormat depthStencilFormat, bool isWindowed)
		{
			return true;
		}

		/// <summary>
		/// Function to determine if a depth/stencil format can be used with a specific display format.
		/// </summary>
		/// <param name="displayFormat">Display format to use.</param>
		/// <param name="targetFormat">Format of the render target.</param>
		/// <param name="depthStencilFormat">Depth/stencil format to check.</param>
		/// <param name="isWindowed">TRUE if using windowed mode, FALSE if not.</param>
		/// <returns>TRUE if the depth stencil type is supported, FALSE if not.</returns>
		public bool SupportsDepthFormat(GorgonDisplayFormat displayFormat, GorgonDepthBufferFormat depthStencilFormat, bool isWindowed)
		{
			GorgonBufferFormat bufferFormat = GorgonBufferFormat.Unknown;

			switch (displayFormat)
			{
				case GorgonDisplayFormat.A8R8G8B8:
					bufferFormat = GorgonBufferFormat.R8G8B8A8_UIntNorm;
					break;
				case GorgonDisplayFormat.X8R8G8B8:
					bufferFormat = GorgonBufferFormat.R8G8B8X8_UInt;
					break;
				case GorgonDisplayFormat.A2R10G10B10:
					bufferFormat = GorgonBufferFormat.R10G10B10A2_UIntNorm;
					break;
			}

			return SupportsDepthFormat(displayFormat, bufferFormat, depthStencilFormat, isWindowed);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoOutput"/> class.
		/// </summary>
		protected GorgonVideoOutput()
		{
			IsAttachedToDesktop = false;
			Name = "NULL Display";
			DesktopDimensions = new Rectangle(0, 0, 1, 1);
		}
		#endregion
	}
}
