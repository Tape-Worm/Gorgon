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
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Internal.Native
{
    #region Value types.
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
        public WindowMessages Message;
        /// <summary>Window message parameter 1.</summary>
        public uint wParam;
        /// <summary>Window message parameter 2.</summary>
        public uint lParam;
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
        #region Methods.
        /// <summary>
        /// Function to set the visibility of the mouse cursor.
        /// </summary>
        /// <param name="bShow">TRUE to show, FALSE to hide.</param>
        /// <returns>-1 if no mouse is installed, 0 or greater for the number of times this function has been called with TRUE.</returns>
        [DllImport("User32.dll")]		
        public static extern int ShowCursor(bool bShow);

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
        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage(out MSG msg, IntPtr hwnd, int wFilterMin, int wFilterMax, PeekMessageFlags flags);

        /// <summary>
        /// Function to translate windows messages.
        /// </summary>
        /// <param name="msg">Message to translate.</param>
        /// <returns>TRUE if successful, FALSE if not.</returns>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG msg);

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
        public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

        /// <summary>
        /// Function to return the time from a high resolution timer.
        /// </summary>
        /// <remarks>See the MSDN documentation for a detailed description.</remarks>
        /// <param name="PerformanceCount">Time from the timer.</param>
        /// <returns>TRUE if system supports high precision timing, FALSE if not.</returns>
        [DllImport("kernel32",CharSet=CharSet.Auto)]
        public static extern bool QueryPerformanceCounter(ref long PerformanceCount);

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
	}
}

