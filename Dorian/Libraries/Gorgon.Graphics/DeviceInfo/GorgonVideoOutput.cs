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
