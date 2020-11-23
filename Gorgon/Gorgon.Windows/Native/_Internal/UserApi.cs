#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, August 12, 2015 7:52:00 PM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
    /// <summary>
    /// The win 32 windowing API.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class UserApi
    {
        #region Methods.
        /// <summary>
        /// Function to retrieve a window handle from a position.
        /// </summary>
        /// <param name="pnt">The position to pass in.</param>
        /// <returns>A handle to the window.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point pnt);

        /// <summary>
        /// Function to release the captured mouse.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseCapture();

        /// <summary>
        /// Function to process window messages.
        /// </summary>
        /// <remarks>See the MSDN documentation for a detailed description.</remarks>
        /// <param name="msg">Message block to retrieve.</param>
        /// <param name="hwnd">Window to retrieve messages from, <b>false</b> for all.</param>
        /// <param name="wFilterMin">Minimum message.</param>
        /// <param name="wFilterMax">Maximum message.</param>
        /// <param name="flags">Flags for the function.</param>
        /// <returns><b>true</b> if messages are ready for processing, <b>false</b> if not.</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out MSG msg, IntPtr hwnd, uint wFilterMin, uint wFilterMax, uint flags);

        /// <summary>
        /// Function to send a message to a window.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Function to send a message to a window.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref HDITEM lParam);

        /// <summary>
        /// Function to retrieve the device context for a window.
        /// </summary>
        /// <param name="hwnd">The handle to the window.</param>
        /// <returns>The device context handle.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hwnd);

        /// <summary>
        /// Function to release the device context for a window.
        /// </summary>
        /// <param name="hwnd">The handle to the window.</param>
        /// <param name="dc">The device context to release.</param>
        /// <returns>The return value indicates whether the DC was released. If the DC was released, the return value is 1. If the DC was not released, the return value is zero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        /// <summary>
        /// Function to retrieve the size of a window.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="lpRect">The resulting window boundaries.</param>
        /// <returns><b>true</b> if failed, <b>false</b> if successful.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes static members of the <see cref="UserApi"/> class.
        /// </summary>
        static UserApi() => Marshal.PrelinkAll(typeof(UserApi));
        #endregion
    }
}
