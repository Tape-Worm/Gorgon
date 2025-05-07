
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: Wednesday, August 12, 2015 8:07:36 PM
// 

using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.System.SystemInformation;

namespace Gorgon.Native;

/// <summary>
/// The type of control signal used to close the console window
/// </summary>
internal enum ConsoleCloseSignal
    : uint
{
    /// <summary>
    /// The CTRL+C key event.
    /// </summary>
    CtrlC = 0,
    /// <summary>
    /// The CTRL+BREAK key event.
    /// </summary>
    Break = 1,
    /// <summary>
    /// The application close event (close on the console window or the process shut downs down).
    /// </summary>
    Close = 2,
    /// <summary>
    /// System log off close event.
    /// </summary>
    LogOff = 5,
    /// <summary>
    /// System shut down close event.
    /// </summary>
    ShutDown = 6
}

/// <summary>
/// Native windows kernal API functionality
/// </summary>
internal static partial class KernelApi
{
    /// <summary>
    /// Standard output handle.
    /// </summary>
    public const int StdOutputHandle = -11;

    /// <summary>
    /// Property to return the number of bytes of installed physical RAM.
    /// </summary>
    public static long TotalPhysicalRAM
    {
        get
        {
            MEMORYSTATUSEX memory = new()
            {
                dwLength = (uint)Unsafe.SizeOf<MEMORYSTATUSEX>()
            };

            return PInvoke.GlobalMemoryStatusEx(ref memory) ? (long)memory.ullTotalPhys : -1;
        }
    }

    /// <summary>
    /// Property to return the number of bytes of free available RAM.
    /// </summary>
    public static long AvailablePhysicalRAM
    {
        get
        {
            MEMORYSTATUSEX memory = new()
            {
                dwLength = (uint)Unsafe.SizeOf<MEMORYSTATUSEX>()
            };

            return PInvoke.GlobalMemoryStatusEx(ref memory) ? (long)memory.ullAvailPhys : -1;
        }
    }

    /// <summary>
    /// Event delegate for closing the console window.
    /// </summary>
    /// <param name="dwControlType">The type of event.</param>
    /// <returns><b>true</b> if the event is handled, <b>false</b> if not.</returns>
    public delegate bool ConsoleCloseHandler(ConsoleCloseSignal dwControlType);
}
