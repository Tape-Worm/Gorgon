#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, April 18, 2008 11:24:46 PM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Native
{
	#region Value types.
	/// <summary>
	/// Header item structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct HDITEM
	{
		/// <summary>
		/// 
		/// </summary>
		public HeaderMask mask;
		/// <summary>
		/// 
		/// </summary>
		public int cxy;
		/// <summary>
		/// 
		/// </summary>
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszText;
		/// <summary>
		/// 
		/// </summary>
		public IntPtr hbm;
		/// <summary>
		/// 
		/// </summary>
		public int cchTextMax;
		/// <summary>
		/// 
		/// </summary>
		public HeaderFormat fmt;
		/// <summary>
		/// 
		/// </summary>
		public IntPtr lParam;
		/// <summary>
		/// 
		/// </summary>
		public int iImage;
		/// <summary>
		/// 
		/// </summary>
		public int iOrder;
		/// <summary>
		/// 
		/// </summary>
		public uint type;
		/// <summary>
		/// 
		/// </summary>
		public IntPtr pvFilter;
		/// <summary>
		/// 
		/// </summary>
		public uint state;
	};

	/// <summary>
	/// Used with GlobalMemoryStatusEx.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MemoryStatusEx
	{
		/// <summary/>
		public uint dwLength;
		/// <summary/>
		public uint dwMemoryLoad;
		/// <summary/>
		public long ullTotalPhysical;
		/// <summary/>
		public long ullAvailablePhysical;
		/// <summary/>
		public long ullTotalPageFile;
		/// <summary/>
		public long ullAvailablePageFile;
		/// <summary/>
		public long ullTotalVirtual;
		/// <summary/>
		public long ullAvailableVirtual;
		/// <summary/>
		public long ullAvailableExtendedVirtual;
	}
	
	/// <summary>
	/// Value type representing a Window message.
	/// </summary>
	/// <remarks>
	/// See the MSDN documentation for more detail.
	/// <para>
	/// Used to pass various messages back and forth between the OS and the app.
	/// </para>
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MSG
	{
		/// <summary>Window handle.</summary>
		public IntPtr hwnd;
		/// <summary>Message to process.</summary>
		public uint Message;
		/// <summary>Window message parameter 1.</summary>
		public IntPtr wParam;
		/// <summary>Window message parameter 2.</summary>
		public IntPtr lParam;
		/// <summary>Time message was sent?</summary>
		public uint time;
		/// <summary>Mouse pointer position.</summary>
		public Point pt;
	}
	#endregion
	
	/// <summary>
	/// Static class for Native Win32 methods and corresponding structures.
	/// </summary>
	/// <remarks>
	/// This is a grouping of any Windows API calls used by Gorgon.
	/// <para>
	/// This list is by no means complete.  The Win32 API is just massive and would probably take a lifetime to map.
	/// </para>
	/// 	<para>
	/// These calls are considered "unsafe" and thus should be used with care.  If you don't know how to use a function, or why you want it, you probably don't need to use them.
	/// </para>
	/// 	<para>
	/// Please note that a lot of the enumerators/structures have slightly different names than their Win32 counterparts.  This was done for the sake of readability.  This does NOT affect their results or their effect on the results of their related functionality.
	/// </para>
	/// </remarks>	
	[System.Security.SuppressUnmanagedCodeSecurity]
	internal static class Win32API
	{
		#region Properties.
		/// <summary>
		/// Property to return the number of bytes of installed physical RAM.
		/// </summary>
		public static long TotalPhysicalRAM
		{
			get
			{
				MemoryStatusEx memory = new MemoryStatusEx();

				memory.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
				if (GlobalMemoryStatusEx(ref memory))
					return memory.ullTotalPhysical;

				return -1;
			}
		}

		/// <summary>
		/// Property to return the number of bytes of free available RAM.
		/// </summary>
		public static long AvailablePhysicalRAM
		{
			get
			{
				MemoryStatusEx memory = new MemoryStatusEx();

				memory.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
				if (GlobalMemoryStatusEx(ref memory))
					return memory.ullAvailablePhysical;

				return -1;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the foreground window.
		/// </summary>
		/// <returns>The handle to the window in the foreground.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetForegroundWindow();

		/// <summary>
		/// Function to send a message to a window.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Function to send a message to a window.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM lParam);

		/// <summary>
		/// Function to send a message to a header.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref IntPtr lParam);

		/// <summary>
		/// Function to retrieve the process ID of a window.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="lpdwProcessId"></param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		/// <summary/>
		[DllImport("kernel32.dll")]
		private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx stat);
		
		/// <summary>
		/// Function to process window messages.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="msg">Message block to retrieve.</param>
		/// <param name="hwnd">Window to retrieve messages from, FALSE for all.</param>
		/// <param name="wFilterMin">Minimum message.</param>
		/// <param name="wFilterMax">Maximum message.</param>
		/// <param name="flags">Flags for the function.</param>
		/// <returns>TRUE if messages are ready for processing, FALSE if not.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool PeekMessage(out MSG msg, IntPtr hwnd, uint wFilterMin, uint wFilterMax, PeekMessageFlags flags);

		/// <summary>
		/// Function to translate keyboard accelerator keys.
		/// </summary>
		/// <param name="hWnd">Window handle</param>
		/// <param name="hAccTable">Accelerator table.</param>
		/// <param name="lpMsg">Window message.</param>
		/// <returns>Non 0 for success, 0 for failure.</returns>
		[DllImport("user32.dll")]
		public static extern int TranslateAccelerator(IntPtr hWnd, IntPtr hAccTable, [In] ref MSG lpMsg);

		/// <summary>
		/// Function to translate windows messages.
		/// </summary>
		/// <param name="msg">Message to translate.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		[DllImport("user32.dll")]
		public static extern bool TranslateMessage([In] ref MSG msg);

		/// <summary>
		/// Function to post a quit message to the message queue.
		/// </summary>
		/// <param name="nExitCode">Exit code.</param>
		[DllImport("user32.dll")]
		public static extern void PostQuitMessage(int nExitCode);

		/// <summary>
		/// Function to dispatch windows messages.
		/// </summary>
		/// <param name="msg">Message to dispatch.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		[DllImport("user32.dll")]
		public static extern IntPtr DispatchMessage([In] ref MSG msg);

		/// <summary>
		/// Function to return the frequency of the high precision timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceFrequency">Frequency of timer.</param>
		/// <returns>TRUE if system supports high precision timing, FALSE if not.</returns>
		[DllImport("kernel32", CharSet=CharSet.Auto)]
		public static extern bool QueryPerformanceFrequency(out long PerformanceFrequency);

		/// <summary>
		/// Function to return the time from a high resolution timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceCount">Time from the timer.</param>
		/// <returns>TRUE if system supports high precision timing, FALSE if not.</returns>
		[DllImport("kernel32",CharSet=CharSet.Auto)]
		public static extern bool QueryPerformanceCounter(out long PerformanceCount);

		/// <summary>
		/// Function to return time from a medium precision timer.
		/// </summary>
		/// <remarks>
		/// See the MSDN documentation for a detailed description.
		/// <para>
		/// This timer is of lower precision than the high precision timers, do not use unless
		/// your system does not support high resolution timers.
		/// </para>
		/// </remarks>
		/// <returns>Time in milliseconds.</returns>
		[DllImport("winmm.dll",CharSet=CharSet.Auto)]
		public static extern int timeGetTime();

		/// <summary>
		/// Function to start a timing session.
		/// </summary>
		/// <param name="uPeriod">Minimum resolution in milliseconds.</param>
		/// <returns>0 if successful, non 0 if not.</returns>
		[DllImport("winmm.dll",CharSet=CharSet.Auto)]
		public static extern int timeBeginPeriod(uint uPeriod);

		/// <summary>
		/// Function to end a timing session.
		/// </summary>
		/// <param name="uPeriod">Minimum resolution in milliseconds.</param>
		/// <returns>0 if successful, non 0 if not.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int timeEndPeriod(uint uPeriod);
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Win32API"/> class.
		/// </summary>
		static Win32API()
		{
			Marshal.PrelinkAll(typeof(Win32API));
		}
		#endregion
	}
}

