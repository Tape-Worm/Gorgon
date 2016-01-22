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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A video output on a video device.
	/// </summary>
	/// <remarks>A video output is a head on the video device.</remarks>
	public class GorgonVideoOutput
		: IGorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the index of this output.
		/// </summary>
		internal int Index
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the device name of the output.
		/// </summary>
		public string Name
		{
			get;
		}

		/// <summary>
		/// Property to return the display location and dimensions of the output.
		/// </summary>
		public Rectangle OutputBounds
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the output device is attached to the desktop.
		/// </summary>
		public bool IsAttachedToDesktop
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the rotation in degrees for the monitor.
		/// </summary>
		public int Rotation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video modes for this output.
		/// </summary>
		public IList<GorgonVideoMode> VideoModes
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the default video mode for this output.
		/// </summary>
		public GorgonVideoMode DefaultVideoMode
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the handle of the output device.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video device for this output.
		/// </summary>
		public VideoDevice VideoDevice
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find the nearest video mode to the one specified.
		/// </summary>
		/// <param name="output">Output for the video mode.</param>
		/// <param name="mode">Mode to find.</param>
		/// <returns>The closest matching video mode to the <paramref name="mode"/> parameter.</returns>
		internal GorgonVideoMode FindMode(GI.Output output, GorgonVideoMode mode)
		{
			GI.ModeDescription findMode = mode.ToModeDesc();
			GI.ModeDescription result;
			
			output.GetClosestMatchingMode(VideoDevice.Graphics.D3DDevice, findMode, out result);

			return new GorgonVideoMode(result);
		}
		
#pragma warning disable 0618
		/// <summary>
		/// Function to find the nearest video mode to the one specified.
		/// </summary>
		/// <param name="mode">Mode to find.</param>
		/// <returns>The closest matching video mode to the <paramref name="mode"/> parameter.</returns>
		public GorgonVideoMode FindMode(GorgonVideoMode mode)
		{
		    if (VideoDevice.Graphics?.D3DDevice == null)
		    {
		        return mode;
		    }

		    using (var output = VideoDevice.Graphics.Adapter.GetOutput(Index))
		    {
		        return FindMode(output, mode);
		    }
		}
#pragma warning restore 0618
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoOutput"/> class.
		/// </summary>
		/// <param name="output">Output to evaluate.</param>
		/// <param name="device">Video device that owns the output.</param>
		/// <param name="index">The index of the output.</param>
		internal GorgonVideoOutput(GI.Output output, VideoDevice device, int index)
		{
			VideoDevice = device;
			
			Index = index;
			Handle = output.Description.MonitorHandle;
			IsAttachedToDesktop = output.Description.IsAttachedToDesktop;
			Name = output.Description.DeviceName;
			OutputBounds = Rectangle.FromLTRB(output.Description.DesktopBounds.Left,
			                                  output.Description.DesktopBounds.Top,
			                                  output.Description.DesktopBounds.Right,
			                                  output.Description.DesktopBounds.Bottom);
			switch (output.Description.Rotation)
			{
				case GI.DisplayModeRotation.Rotate90:
					Rotation = 90;
					break;
				case GI.DisplayModeRotation.Rotate270:
					Rotation = 270;
					break;
				case GI.DisplayModeRotation.Rotate180:
					Rotation = 180;
					break;
				default:
					Rotation = 0;
					break;
			}				
		}

	    /// <summary>
	    /// Initializes a new instance of the <see cref="GorgonVideoOutput"/> class.
	    /// </summary>
	    internal GorgonVideoOutput()
	    {
	        var area = Screen.AllScreens.Aggregate(Rectangle.Empty, (current, t) => Rectangle.Union(current, t.Bounds));

			Index = 0;
			Handle = Win32API.GetMonitor(null);
			IsAttachedToDesktop = true;
			Name = Resources.GORGFX_OUTPUT_SOFTWARE_DEV_NAME;
			OutputBounds = Screen.PrimaryScreen.Bounds;
			Rotation = 0;
			DefaultVideoMode = new GorgonVideoMode(area.Width, area.Height, BufferFormat.R8G8B8A8_UNorm);
		}
		#endregion
	}
}
