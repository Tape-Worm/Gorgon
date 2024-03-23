// 
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
// Created: Wednesday, August 12, 2015 11:29:45 PM
// 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using Gorgon.Input;
using Gorgon.Input.Native;
using Gorgon.Input.Properties;

namespace Gorgon.Native;

internal enum CursorInfoFlags
{
    CursorHidden = 0,
    CursorShowing = 1,
    Suppressed = 2
}

/// <summary>
/// Win32 native keyboard input functionality
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal static partial class UserApi
{
    // Retrieves a window procedure.
    public const int WindowLongWndProc = -4;

    /// <summary>
    /// Property to return the number of function keys on the keyboard.
    /// </summary>
    public static int FunctionKeyCount => GetKeyboardType(2);

    /// <summary>
    /// Property to return the keyboard type.
    /// </summary>
    public static KeyboardType KeyboardType
    {
        get
        {
            int keyboardType = GetKeyboardType(0);

            return keyboardType switch
            {
                1 => KeyboardType.XT,
                2 => KeyboardType.OlivettiICO,
                3 => KeyboardType.AT,
                4 => KeyboardType.Enhanced,
                5 => KeyboardType.Nokia1050,
                6 => KeyboardType.Nokia9140,
                7 => KeyboardType.Japanese,
                81 => KeyboardType.USB,
                _ => KeyboardType.Unknown,
            };
        }
    }

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorInfo(ref CURSORINFO pci);

    /// <summary>
    /// Function to retrieve keyboard type information.
    /// </summary>
    /// <param name="nTypeFlag">The type of info.</param>
    /// <returns>The requested information.</returns>
    [LibraryImport("User32.dll")]
    private static partial int GetKeyboardType(int nTypeFlag);

    /// <summary>
    /// Function to get the state of a key.
    /// </summary>
    /// <param name="nVirtKey">Virtual key code to retrieve.</param>
    /// <returns>A bit mask containing the state of the virtual key.</returns>
    [LibraryImport("User32.dll")]
    private static partial short GetKeyState(Keys nVirtKey);

    /// <summary>
    /// Function to retrieve the scan code for a virtual key.
    /// </summary>
    /// <param name="uCode">Virtual key code</param>
    /// <param name="uMapType">Mapping type.</param>
    /// <returns>The scan code.</returns>
    [LibraryImport("user32.dll")]
    private static partial int MapVirtualKey(Keys uCode, int uMapType);

    /// <summary>
    /// Function to retrieve information about the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to retrieve information from.</param>
    /// <param name="index">Type of information.</param>
    /// <returns>A pointer to the information.</returns>
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static partial nint GetWindowLongx86([MarshalUsing(typeof(HandleRefMarshaller))] HandleRef hwnd, int index);

    /// <summary>
    /// Function to retrieve information about the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to retrieve information from.</param>
    /// <param name="index">Type of information.</param>
    /// <returns>A pointer to the information.</returns>
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static partial nint GetWindowLongx64([MarshalUsing(typeof(HandleRefMarshaller))] HandleRef hwnd, int index);

    /// <summary>
    /// Function to set information for the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to set information on.</param>
    /// <param name="index">Type of information.</param>
    /// <param name="info">Information to set.</param>
    /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static partial nint SetWindowLongx86([MarshalUsing(typeof(HandleRefMarshaller))] HandleRef hwnd, int index, nint info);

    /// <summary>
    /// Function to set information for the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to set information on.</param>
    /// <param name="index">Type of information.</param>
    /// <param name="info">Information to set.</param>
    /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static partial nint SetWindowLongx64([MarshalUsing(typeof(HandleRefMarshaller))] HandleRef hwnd, int index, nint info);

    /// <summary>
    /// Function to set the visibility of the pointing device cursor.
    /// </summary>
    /// <param name="bShow"><b>true</b> to show, <b>false</b> to hide.</param>
    /// <returns>-1 if no pointing device is installed, 0 or greater for the number of times this function has been called with <b>true</b>.</returns>
    [LibraryImport("User32.dll")]
    public static partial int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);

    /// <summary>
    /// Converts a virtual key code into a unicode character representation.
    /// </summary>
    /// <param name="keyCode">Key code</param>
    /// <param name="scanCode">Scan code</param>
    /// <param name="keyboardState">State</param>
    /// <param name="buffer">Buffer to populate</param>
    /// <param name="bufferSize">Size of the buffer, in bytes.</param>
    /// <param name="flags">Flags to pass.</param>
    /// <returns>The return code for the method.</returns>
    /// <remarks>
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646320(v=vs.85).aspx"/> for more info.
    /// </remarks>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int ToUnicode(uint keyCode,
                                        uint scanCode,
                                        [In] byte[] keyboardState,
                                        [Out] char[] buffer,
                                        int bufferSize,
                                        uint flags);

    public static CursorInfoFlags IsCursorVisible()
    {
        CURSORINFO cursorInfo = new()
        {
            cbSize = Unsafe.SizeOf<CURSORINFO>(),
            flags = CursorInfoFlags.CursorHidden,
            hCursor = IntPtr.Zero,
            ptScreenPos = new POINT
            {
                Y = 0,
                X = 0
            }
        };

        if (GetCursorInfo(ref cursorInfo))
        {
            return cursorInfo.flags;
        }

        int win32Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(string.Format(Resources.GORINP_ERR_WIN32_CURSOR_INFO, win32Error));
    }

    /// <summary>
    /// Function to call a window procedure.
    /// </summary>
    /// <param name="wndProc">Pointer to the window procedure function to call.</param>
    /// <param name="hwnd">Window handle to use.</param>
    /// <param name="msg">Message to send.</param>
    /// <param name="wParam">Parameter for the message.</param>
    /// <param name="lParam">Parameter for the message.</param>
    /// <returns>The return value specifies the result of the message processing and depends on the message sent.</returns>
    [LibraryImport("user32.dll", EntryPoint = "CallWindowProcW")]
    public static partial nint CallWindowProc(nint wndProc, nint hwnd, int msg, nint wParam, nint lParam);

    /// <summary>
    /// Function to retrieve information about the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to retrieve information from.</param>
    /// <param name="index">Type of information.</param>
    /// <returns>A pointer to the information.</returns>
    public static nint GetWindowLong(HandleRef hwnd, int index) => !Environment.Is64BitProcess ? GetWindowLongx86(hwnd, index) : GetWindowLongx64(hwnd, index);

    /// <summary>
    /// Function to set information for the specified window.
    /// </summary>
    /// <param name="hwnd">Window handle to set information on.</param>
    /// <param name="index">Type of information.</param>
    /// <param name="info">Information to set.</param>
    /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
    public static nint SetWindowLong(HandleRef hwnd, int index, nint info) => !Environment.Is64BitProcess ? SetWindowLongx86(hwnd, index, info) : SetWindowLongx64(hwnd, index, info);

    /// <summary>
    /// Function to return the scan code for a virtual key.
    /// </summary>
    /// <param name="virtualKey">The virtual key to evaluate.</param>
    /// <returns>The scan code for the virtual key.</returns>
    public static int GetScancode(Keys virtualKey) => MapVirtualKey(virtualKey, 0);

    /// <summary>
    /// Function to determine if the specified virtual key is pressed or not.
    /// </summary>
    /// <param name="virtualKey">The virtual key to evaluate.</param>
    /// <returns><b>true</b> if down, <b>false</b> if not.</returns>
    public static bool CheckKeyDown(Keys virtualKey) => (GetKeyState(virtualKey) & 0x80) == 0x80;

    /// <summary>
    /// Initializes static members of the <see cref="UserApi"/> class.
    /// </summary>
    static UserApi() => Marshal.PrelinkAll(typeof(UserApi));
}
