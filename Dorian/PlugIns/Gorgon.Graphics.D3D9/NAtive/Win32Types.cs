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
// Created: Sunday, July 24, 2011 10:16:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Native
{
	/// <summary>
	/// A win 32 rectangle.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct RECT
	{
		/// <summary>
		/// 
		/// </summary>
		public int Left;
		/// <summary>
		/// 
		/// </summary>
		public int Top;
		/// <summary>
		/// 
		/// </summary>
		public int Right;
		/// <summary>
		/// 
		/// </summary>
		public int Bottom;
	}

    /// <summary>
    /// The MONITORINFOEX structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
    /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name 
    /// for the display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    struct MONITORINFOEX
    {
		/// <summary>
		/// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function. 
		/// Doing so lets the function determine the type of structure you are passing to it.
		/// </summary>
		public int Size;
		/// <summary>
		/// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
		/// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
		/// </summary>
		public RECT MonitorDimensions;
		/// <summary>
		/// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
		/// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
		/// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
		/// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
		/// </summary>
		public RECT WorkArea;
		/// <summary>
		/// The attributes of the display monitor.
		/// 
		/// This member can be the following value:
		///   1 : MONITORINFOF_PRIMARY
		/// </summary>
		public int Flags;
		/// <summary>
		/// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name, 
		/// and so can save some bytes by using a MONITORINFO structure.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DeviceName;
    }
}
