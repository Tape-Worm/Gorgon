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
// Created: Sunday, July 24, 2011 10:02:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A direct 3D9 video output.
	/// </summary>
	internal class D3D9VideoOutput
		: GorgonVideoOutput
	{
		#region Variables.
		private AdapterInformation _info = null;		// Adapter information.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the handle for the monitor attached to this output.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the adapter group index for this output.
		/// </summary>
		public int HeadIndex
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the video modes for this output.
		/// </summary>
		/// <returns>
		/// An enumerable list of video modes.
		/// </returns>
		protected override IEnumerable<GorgonVideoMode> GetVideoModes()
		{			
			Format[] displayModeFormats = {Format.A2R10G10B10, Format.A8R8G8B8, Format.X8R8G8B8};
			List<GorgonVideoMode> modes = new List<GorgonVideoMode>();

			foreach (var d3dformat in displayModeFormats)
			{
				DisplayModeCollection d3dModes = _info.GetDisplayModes(d3dformat);

				foreach (var d3dMode in d3dModes)
					modes.Add(D3DConvert.Convert(d3dMode));
			}

			return modes;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoOutput"/> class.
		/// </summary>
		/// <param name="adapter">Adapter information.</param>
		/// <param name="headIndex">Index of the adapter head.</param>
		/// <param name="monitorInfo">Monitor information.</param>
		public D3D9VideoOutput(AdapterInformation adapter, int headIndex, MONITORINFOEX monitorInfo)
		{
			if (adapter == null)
				throw new ArgumentNullException("adapter");
			
			_info = adapter;
			Handle = adapter.Monitor;
			Rotation = 0;
			IsAttachedToDesktop = ((monitorInfo.Flags & 1) == 1);
			DesktopDimensions = System.Drawing.Rectangle.FromLTRB(monitorInfo.WorkArea.Left, monitorInfo.WorkArea.Top, monitorInfo.WorkArea.Right, monitorInfo.WorkArea.Bottom);
			Name = monitorInfo.DeviceName;
			HeadIndex = headIndex;
			DefaultVideoMode = D3DConvert.Convert(adapter.CurrentDisplayMode);
		}
		#endregion
	}
}
