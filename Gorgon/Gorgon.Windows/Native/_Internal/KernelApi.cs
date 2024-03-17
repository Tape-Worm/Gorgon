
// 
// Gorgon
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


using System.Runtime.InteropServices;
using System.Security;

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
[SuppressUnmanagedCodeSecurity]
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
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
            };

            return GlobalMemoryStatusEx(ref memory) ? memory.ullTotalPhysical : -1;
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
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
            };

            return GlobalMemoryStatusEx(ref memory) ? memory.ullAvailablePhysical : -1;
        }
    }



    /// <summary>
    /// Event delegate for closing the console window.
    /// </summary>
    /// <param name="dwControlType">The type of event.</param>
    /// <returns><b>true</b> if the event is handled, <b>false</b> if not.</returns>
    public delegate bool ConsoleCloseHandler(ConsoleCloseSignal dwControlType);



    /// <summary>
    /// Function to retrieve the standard handle.
    /// </summary>
    /// <param name="nStdHandle">The standard handle.</param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll")]
    public static partial nint GetStdHandle(int nStdHandle);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lpFileName"></param>
    /// <param name="dwDesiredAccess"></param>
    /// <param name="dwShareMode"></param>
    /// <param name="lpSecurityAttributes"></param>
    /// <param name="dwCreationDisposition"></param>
    /// <param name="dwFlagsAndAttributes"></param>
    /// <param name="hTemplateFile"></param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial nint CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        nint lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        nint hTemplateFile);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(nint handle, uint mode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hConsoleHandle"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleHandle, out uint mode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nStdHandle"></param>
    /// <param name="handle"></param>
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetStdHandle(int nStdHandle, nint handle);

    /// <summary>
    /// Function to set up a console control handler to intercept console close events.
    /// </summary>
    /// <param name="handler">The handler to assign.</param>
    /// <param name="add"><b>true</b> to add the handler, <b>false</b> to remove it.</param>
    /// <returns><b>true</b> if the function succeeds, <b>false</b> if not.</returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport("kernel32.dll")]
    public static partial bool SetConsoleCtrlHandler(ConsoleCloseHandler handler, [MarshalAs(UnmanagedType.Bool)] bool add);

    /// <summary>
    /// Function to allocate a console window.
    /// </summary>
    /// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport("kernel32.dll")]
    public static partial bool AllocConsole();

    /// <summary>
    /// Function to free an allocated console window.
    /// </summary>
    /// <returns>Non zero if successful, zero if failed.</returns>
    [LibraryImport("kernel32.dll")]
    public static partial int FreeConsole();

    /// <summary>
    /// Function to return the amount of memory available on the machine.
    /// </summary>
    /// <param name="stat">Memory status data.</param>
    /// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX stat);

    /// <summary>
    /// Function to return the frequency of the high precision timer.
    /// </summary>
    /// <remarks>See the MSDN documentation for a detailed description.</remarks>
    /// <param name="performanceFrequency">Frequency of timer.</param>
    /// <returns><b>true</b> if system supports high precision timing, <b>false</b> if not.</returns>
    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryPerformanceFrequency(out long performanceFrequency);

    /// <summary>
    /// Function to return the time from a high resolution timer.
    /// </summary>
    /// <remarks>See the MSDN documentation for a detailed description.</remarks>
    /// <param name="performanceCount">Time from the timer.</param>
    /// <returns><b>true</b> if system supports high precision timing, <b>false</b> if not.</returns>
    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryPerformanceCounter(out long performanceCount);



    /// <summary>
    /// Initializes static members of the <see cref="KernelApi"/> class.
    /// </summary>
    static KernelApi() => Marshal.PrelinkAll(typeof(KernelApi));

}
