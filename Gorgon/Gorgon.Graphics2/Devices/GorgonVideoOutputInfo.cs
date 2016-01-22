﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: Thursday, January 7, 2016 7:15:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using Gorgon.Core;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Values to indicate how the back buffers should be rotated to fit the physical rotation of the monitor.
	/// </summary>
	public enum DisplayModeRotation
	{
		/// <summary>
		/// No rotation specified.
		/// </summary>
		Unspecified = DXGI.DisplayModeRotation.Unspecified,
		/// <summary>
		/// No rotation.
		/// </summary>
		Identity = DXGI.DisplayModeRotation.Identity,
		/// <summary>
		/// Display is rotated 90 degrees.
		/// </summary>
		Rotate90 = DXGI.DisplayModeRotation.Rotate90,
		/// <summary>
		/// Display is rotated 180 degrees.
		/// </summary>
		Rotate180 = DXGI.DisplayModeRotation.Rotate180,
		/// <summary>
		/// Display is rotated 270 degrees.
		/// </summary>
		Rotate270 = DXGI.DisplayModeRotation.Rotate270
	}

	/// <summary>
	/// Provides information about an output on a <see cref="VideoDevice"/>.
	/// </summary>
	/// <remarks>
	/// An output is typically a physical connection between the video device and another device.
	/// </remarks>
	public class GorgonVideoOutputInfo 
		: IGorgonNamedObject
	{
		#region Variables.
		// Output description.
		private DXGI.OutputDescription _desc;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the index of the output.
		/// </summary>
		public int Index
		{
			get;
		}

		/// <summary>
		/// Property to return the name of this output.
		/// </summary>
		public string Name
		{
			get;
		}

		/// <summary>
		/// Property to return the handle to the monitor that is attached to the output.
		/// </summary>
		public IntPtr MonitorHandle => _desc.MonitorHandle;

		/// <summary>
		/// Property to return the bounds of the output in desktop coordinates.
		/// </summary>
		/// <remarks>
		/// The desktop coordinates depend on the dots per inch (DPI) of the desktop. For more information about writing DPI-aware Win32 applications, see <a target="_blank" href="https://msdn.microsoft.com/en-us/library/bb173068.aspx">High DPI</a>.
		/// </remarks>
		public Rectangle DesktopBounds => Rectangle.FromLTRB(_desc.DesktopBounds.Left, _desc.DesktopBounds.Top, _desc.DesktopBounds.Right, _desc.DesktopBounds.Bottom);

		/// <summary>
		/// Property to return whether the output is attached to the desktop or not.
		/// </summary>
		public bool IsAttachedToDesktop => _desc.IsAttachedToDesktop;

		/// <summary>
		/// Property to return how the display image is rotated by the output.
		/// </summary>
		public DisplayModeRotation Rotation => (DisplayModeRotation)_desc.Rotation;

		/// <summary>
		/// Property to return the list of video modes supported by this output.
		/// </summary>
		public IReadOnlyList<GorgonVideoMode> VideoModes
		{
			get;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoOutputInfo" /> class.
		/// </summary>
		/// <param name="index">The index of the output.</param>
		/// <param name="output">The output used to provide information.</param>
		/// <param name="modes">The list of full screen display modes supported by this output on the video device.</param>
		internal GorgonVideoOutputInfo(int index, DXGI.Output4 output, IReadOnlyList<GorgonVideoMode> modes)
		{
			Index = index;
			_desc = output.Description;
			Name = _desc.DeviceName.Replace("\0", string.Empty);
			VideoModes = modes;
		}
		#endregion
	}
}
