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
// Created: Wednesday, August 12, 2015 11:29:45 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Gorgon.Input;
using Gorgon.Input.Properties;

namespace Gorgon.Native
{
    internal enum CursorInfoFlags
    {
        CursorHidden = 0,
        CursorShowing = 1,
        Suppressed = 2
    }

    /// <summary>
    /// Win32 native keyboard input functionality.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class UserApi
    {
        #region Constants.
        // Retrieves a window procedure.
        public const int WindowLongWndProc = -4;
        #endregion

        #region Properties.
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

                switch (keyboardType)
                {
                    case 1:
                        return KeyboardType.XT;
                    case 2:
                        return KeyboardType.OlivettiICO;
                    case 3:
                        return KeyboardType.AT;
                    case 4:
                        return KeyboardType.Enhanced;
                    case 5:
                        return KeyboardType.Nokia1050;
                    case 6:
                        return KeyboardType.Nokia9140;
                    case 7:
                        return KeyboardType.Japanese;
                    case 81:
                        return KeyboardType.USB;
                    default:
                        return KeyboardType.Unknown;
                }
            }
        }

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
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(uint keyCode,
                                            uint scanCode,
                                            byte[] keyboardState,
                                            [Out] [MarshalAs(UnmanagedType.LPArray)] char[] buffer,
                                            int bufferSize,
                                            uint flags);

        /// <summary>
        /// Function to set the visibility of the pointing device cursor.
        /// </summary>
        /// <param name="bShow"><b>true</b> to show, <b>false</b> to hide.</param>
        /// <returns>-1 if no pointing device is installed, 0 or greater for the number of times this function has been called with <b>true</b>.</returns>
        [DllImport("User32.dll")]
        public static extern int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorInfo(IntPtr pci);

        public static CursorInfoFlags IsCursorVisible()
        {
            unsafe
            {
                var cursorInfo = new CURSORINFO
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

                if (GetCursorInfo((IntPtr)(&cursorInfo)))
                {
                    return cursorInfo.flags;
                }

                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(string.Format(Resources.GORINP_ERR_WIN32_CURSOR_INFO, win32Error));
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve keyboard type information.
        /// </summary>
        /// <param name="nTypeFlag">The type of info.</param>
        /// <returns>The requested information.</returns>
        [DllImport("User32.dll", CharSet = CharSet.Ansi)]
        private static extern int GetKeyboardType(int nTypeFlag);

        /// <summary>
        /// Function to get the state of a key.
        /// </summary>
        /// <param name="nVirtKey">Virtual key code to retrieve.</param>
        /// <returns>A bit mask containing the state of the virtual key.</returns>
        [DllImport("User32.dll"), SuppressUnmanagedCodeSecurity]
        private static extern short GetKeyState(Keys nVirtKey);

        /// <summary>
        /// Function to retrieve the scan code for a virtual key.
        /// </summary>
        /// <param name="uCode">Virtual key code</param>
        /// <param name="uMapType">Mapping type.</param>
        /// <returns>The scan code.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
        private static extern int MapVirtualKey(Keys uCode, int uMapType);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongx86(HandleRef hwnd, int index);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongx64(HandleRef hwnd, int index);

        /// <summary>
        /// Function to set information for the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to set information on.</param>
        /// <param name="index">Type of information.</param>
        /// <param name="info">Information to set.</param>
        /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongx86(HandleRef hwnd, int index, IntPtr info);

        /// <summary>
        /// Function to set information for the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to set information on.</param>
        /// <param name="index">Type of information.</param>
        /// <param name="info">Information to set.</param>
        /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongx64(HandleRef hwnd, int index, IntPtr info);

        /// <summary>
        /// Function to call a window procedure.
        /// </summary>
        /// <param name="wndProc">Pointer to the window procedure function to call.</param>
        /// <param name="hwnd">Window handle to use.</param>
        /// <param name="msg">Message to send.</param>
        /// <param name="wParam">Parameter for the message.</param>
        /// <param name="lParam">Parameter for the message.</param>
        /// <returns>The return value specifies the result of the message processing and depends on the message sent.</returns>
        [DllImport("user32.dll", EntryPoint = "CallWindowProc", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        public static IntPtr GetWindowLong(HandleRef hwnd, int index) => IntPtr.Size == 4 ? GetWindowLongx86(hwnd, index) : GetWindowLongx64(hwnd, index);

        /// <summary>
        /// Function to set information for the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to set information on.</param>
        /// <param name="index">Type of information.</param>
        /// <param name="info">Information to set.</param>
        /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
        public static IntPtr SetWindowLong(HandleRef hwnd, int index, IntPtr info) => IntPtr.Size == 4 ? SetWindowLongx86(hwnd, index, info) : SetWindowLongx64(hwnd, index, info);

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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes static members of the <see cref="UserApi"/> class.
        /// </summary>
        static UserApi() => Marshal.PrelinkAll(typeof(UserApi));
        #endregion
    }
}
