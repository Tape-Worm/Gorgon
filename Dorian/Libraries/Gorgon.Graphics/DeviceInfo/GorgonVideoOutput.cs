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
using GI = SlimDX.DXGI;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A video output on a video device.
	/// </summary>
	/// <remarks>A video output is a head on the video device.</remarks>
	public class GorgonVideoOutput
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;		// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the GI output.
		/// </summary>
		internal GI.Output GIOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the device name of the output.
		/// </summary>
		public string Name
		{
			get
			{
				return GIOutput.Description.Name;
			}
		}

		/// <summary>
		/// Property to return the display location and dimensions of the output.
		/// </summary>
		public Rectangle OutputBounds
		{
			get
			{
				return GIOutput.Description.DesktopBounds;
			}
		}

		/// <summary>
		/// Property to return whether the output device is attached to the desktop.
		/// </summary>
		public bool IsAttachedToDesktop
		{
			get
			{
				return GIOutput.Description.IsAttachedToDesktop;
			}
		}

		/// <summary>
		/// Property to return the rotation in degrees for the monitor.
		/// </summary>
		public int Rotation
		{
			get
			{
				switch (GIOutput.Description.Rotation)
				{
					case GI.DisplayModeRotation.Rotate90Degrees:
						return 90;
					case GI.DisplayModeRotation.Rotate270Degrees:
						return 270;
					case GI.DisplayModeRotation.Rotate180Degrees:
						return 180;
					default:
						return 0;
				}				
			}
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

		/// <summary>
		/// Property to return the handle of the output device.
		/// </summary>
		public IntPtr Handle
		{
			get
			{
				return GIOutput.Description.MonitorHandle;
			}
		}

		/// <summary>
		/// Property to return the video device for this output.
		/// </summary>
		public GorgonVideoDevice VideoDevice
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoOutput"/> class.
		/// </summary>
		/// <param name="videoDevice">Video device that owns this output.</param>
		/// <param name="output">Output to evaluate.</param>
		internal GorgonVideoOutput(GorgonVideoDevice videoDevice, GI.Output output)
		{
			if (videoDevice == null)
				throw new ArgumentNullException("videoDevice");

			if (output == null)
				throw new ArgumentNullException("output");

			VideoModes = new GorgonVideoModeList(this);
			VideoDevice = videoDevice;
			GIOutput = output;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (GIOutput != null)
				{
					Gorgon.Log.Print("Removing DXGI output...", Diagnostics.GorgonLoggingLevel.Verbose);
					GIOutput.Dispose();
				}

				GIOutput = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
