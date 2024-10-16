﻿// 
// Gorgon
// Copyright (C) 2024 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, August 12, 2015 7:52:00 PM
// 

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native;

/// <summary>
/// The win 32 windowing API
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal static partial class UserApi
{
    /// <summary>
    /// Function to release the captured mouse.
    /// </summary>
    /// <returns></returns>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

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
    [LibraryImport("user32.dll", EntryPoint = "PeekMessageW")]
    public static partial bool PeekMessage(out MSG msg, nint hwnd, uint wFilterMin, uint wFilterMax, uint flags);

    /// <summary>
    /// Function to send a message to a window.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static partial nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    /// <summary>
    /// Function to send a message to a window.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static partial nint SendMessage(nint hWnd, uint msg, nint wParam, ref HDITEM lParam);

    /// <summary>
    /// Function to retrieve the device context for a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window.</param>
    /// <returns>The device context handle.</returns>
    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial nint GetDC(nint hwnd);

    /// <summary>
    /// Function to release the device context for a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window.</param>
    /// <param name="dc">The device context to release.</param>
    /// <returns>The return value indicates whether the DC was released. If the DC was released, the return value is 1. If the DC was not released, the return value is zero.</returns>
    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial int ReleaseDC(nint hwnd, nint dc);

    /// <summary>
    /// Function to retrieve the size of a window.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="lpRect">The resulting window boundaries.</param>
    /// <returns><b>true</b> if failed, <b>false</b> if successful.</returns>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(nint hwnd, out RECT lpRect);

    /// <summary>
    /// Initializes static members of the <see cref="UserApi"/> class.
    /// </summary>
    static UserApi() => Marshal.PrelinkAll(typeof(UserApi));
}
