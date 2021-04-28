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
// Created: Sunday, July 24, 2011 10:15:39 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Win 32 API function calls.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class Win32API
    {
        #region Constants.
        private const uint VER_MINORVERSION = 0x0000001;
        private const uint VER_MAJORVERSION = 0x0000002;
        private const uint VER_BUILDNUMBER = 0x0000004;
        private const uint VER_SERVICEPACKMAJOR = 0x0000020;
        private const byte VER_GREATER_EQUAL = 3;
        #endregion

        #region Win32 Methods.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwlConditionMask"></param>
        /// <param name="dwTypeBitMask"></param>
        /// <param name="dwConditionMask"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern ulong VerSetConditionMask(ulong dwlConditionMask, uint dwTypeBitMask, byte dwConditionMask);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpVersionInfo"></param>
        /// <param name="dwTypeMask"></param>
        /// <param name="dwlConditionMask"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern int VerifyVersionInfo([In] ref OSVERSIONINFOEX lpVersionInfo, uint dwTypeMask, ulong dwlConditionMask);

        /// <summary>
        /// Function to determine if the version of Windows that is running is what we want.
        /// </summary>
        /// <param name="majorVersion">Major version number to look up.</param>
        /// <param name="minorVersion">Minor version number to look up.</param>
        /// <param name="buildNumber">The build number to look up.</param>
        /// <param name="servicePackMajorVersion">Major service pack version number to look up.</param>
        /// <returns><b>true</b> if the version is the same or better, <b>false</b> if not.</returns>
        private static bool IsWindowsVersionOrGreater(uint majorVersion, uint minorVersion, uint? buildNumber, ushort? servicePackMajorVersion)
        {
            var osInfoEx = new OSVERSIONINFOEX
            {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf<OSVERSIONINFOEX>(),
                dwMajorVersion = majorVersion,
                dwMinorVersion = minorVersion,
                dwBuildNumber = buildNumber ?? 0,
                wServicePackMajor = servicePackMajorVersion ?? 0
            };

            uint typeMask = VER_MAJORVERSION | VER_MINORVERSION;
            ulong versionMask = VerSetConditionMask(VerSetConditionMask(0, VER_MAJORVERSION, VER_GREATER_EQUAL),
                                                                              VER_MINORVERSION,
                                                                              VER_GREATER_EQUAL);

            if (buildNumber is not null)
            {
                versionMask = VerSetConditionMask(versionMask, VER_BUILDNUMBER, VER_GREATER_EQUAL);
                typeMask |= VER_BUILDNUMBER;
            }

            if (servicePackMajorVersion is null)
            {
                return VerifyVersionInfo(ref osInfoEx, typeMask, versionMask) != 0;
            }

            versionMask = VerSetConditionMask(versionMask, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
            typeMask |= VER_SERVICEPACKMAJOR;

            return VerifyVersionInfo(ref osInfoEx, typeMask, versionMask) != 0;
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than, Windows 10 with the specified build number.
        /// </summary>
        /// <returns><b>true</b> if the current OS version matches, or is greater than Windows 10 with the specified build number; otherwise <b>false</b>.</returns>
	    public static bool IsWindows10OrGreater(int build) => IsWindowsVersionOrGreater(10, 0, (uint)build, null);

        /// <summary>
        /// Function to retrieve the nearest monitor to the window.
        /// </summary>
        /// <param name="hwnd">Handle to the window.</param>
        /// <param name="flags">Flags to pass in.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint MonitorFromWindow(nint hwnd, MonitorFlags flags);
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes static members of the <see cref="Win32API"/> class.
        /// </summary>
        static Win32API() => Marshal.PrelinkAll(typeof(Win32API));
        #endregion
    }
    // ReSharper restore InconsistentNaming
}
