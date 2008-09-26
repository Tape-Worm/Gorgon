#region MIT.
// 
// Gorgon_x64.
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
// Created: Saturday, April 19, 2008 5:45:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace GorgonLibrary.FileSystems.Tools
{
    /// <summary>
    /// Shell icon size.
    /// </summary>
    public enum ShellIconSize
    {
        /// <summary>
        /// Large icon.
        /// </summary>
        Large = 0,
        /// <summary>
        /// Small icon.
        /// </summary>
        Small = 1
    }

    /// <summary>
    /// Native Win32 functions.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Flags for SHGetInfo.
        /// </summary>
        [Flags()]
        private enum ShellInfoFlags
        {
            /// <summary>
            /// Get icon.
            /// </summary>
            Icon = 0x000000100,
            /// <summary>
            /// Get display name.
            /// </summary>
            DisplayName = 0x000000200,
            /// <summary>
            /// Get type name.
            /// </summary>
            TypeName = 0x000000400,
            /// <summary>
            /// Get attributes.
            /// </summary>
            Attributes = 0x000000800,
            /// <summary>
            /// Get icon location.
            /// </summary>
            IconLocation = 0x000001000,
            /// <summary>
            /// Get EXE type.
            /// </summary>
            EXEType = 0x000002000,
            /// <summary>
            /// Get system icon index.
            /// </summary>
            SystemIconIndex = 0x000004000,
            /// <summary>
            /// Put a link overlay on the icon.
            /// </summary>
            LinkOverlay = 0x000008000,
            /// <summary>
            /// Show icon as selected.
            /// </summary>
            Selected = 0x000010000,
            /// <summary>
            /// Get specified attributes.
            /// </summary>
            AttributesSpecified = 0x000020000,
            /// <summary>
            /// Get large icon.
            /// </summary>
            LargeIcon = 0x000000000,
            /// <summary>
            /// Get small icon.
            /// </summary>
            SmallIcon = 0x000000001,
            /// <summary>
            /// Get open icon.
            /// </summary>
            OpenIcon = 0x000000002,
            /// <summary>
            /// Get shell icon size.
            /// </summary>
            ShellIconSize = 0x000000004,
            /// <summary>
            /// Path is a PIDL.
            /// </summary>
            PIDL = 0x000000008,
            /// <summary>
            /// Use dwFileAttribute.
            /// </summary>
            UseFileAttributes = 0x000000010,
            /// <summary>
            /// Apply overlays.
            /// </summary>
            AddOverlays = 0x000000020,
            /// <summary>
            /// Get the index of the overlay in the upper 8 bits of the IconIndex member.
            /// </summary>
            OverlayIndex = 0x000000040
        }

        /// <summary>
        /// Value type containing shell file information.
        /// </summary>
        private struct SHFILEINFO
        {
            /// <summary>
            /// Icon handle.
            /// </summary>
            public IntPtr IconHandle;
            /// <summary>
            /// Index of the icon image within the system image list. 
            /// </summary>
            public IntPtr IconIndex;
            /// <summary>
            /// Attributes.
            /// </summary>
            public uint Attributes;
            /// <summary>
            /// Display name.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string DisplayName;
            /// <summary>
            /// File type.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string Type;
        }

        /// <summary>
        /// Function to destroy an unmanaged icon resource.
        /// </summary>
        /// <param name="handle">Handle to the icon.</param>
        /// <returns>Non zero if successful, 0 if not.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>
        /// Function to get file information from the shell.
        /// </summary>
        /// <param name="pszPath">Path to the file.</param>
        /// <param name="dwFileAttributes">File attributes.</param>
        /// <param name="psfi">GetFileInfo data.</param>
        /// <param name="cbSizeFileInfo">Size of structure.</param>
        /// <param name="uFlags">Flags to use.</param>
        /// <returns>Returns a value whose meaning depends on the uFlags parameter. If uFlags does not contain SHGFI_EXETYPE or SHGFI_SYSICONINDEX, the return value is nonzero if successful, or zero otherwise.  If uFlags contains the SHGFI_EXETYPE flag, the return value specifies the type of the executable file</returns>
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        /// <summary>
        /// Function to build a managed icon from an unmanaged icon handle.
        /// </summary>
        /// <param name="iconHandle">Icon handle to create from.</param>
        /// <returns>A new managed icon.</returns>
        private static Icon ConvertUnmanagedIcon(IntPtr iconHandle)
        {
            Icon icon = null;		// New icon.
            Icon clone = null;		// Clone of the icon.

            // Get the icon from the unmanaged handle.
            icon = Icon.FromHandle(iconHandle);
            clone = (Icon)icon.Clone();

            // Clean up.
            icon.Dispose();
            DestroyIcon(iconHandle);

            return clone;
        }

        /// <summary>
        /// Function to get an icon based on a file extension.
        /// </summary>
        /// <param name="extension">File extension to use.</param>
        /// <param name="size">Size of the icon.</param>
        /// <returns>An icon from the shell that is related to the extension passed.</returns>
        /// <remarks>This code was created by Gil Schmidt from CodeProject.com.  http://www.codeproject.com/csharp/iconhandler.asp</remarks>
        public static Icon IconFromExtension(string extension, ShellIconSize size)
        {
            Icon newIcon = null;			// New icon.
            SHFILEINFO shellInfo;			// Shell file info data.
            uint flags = 0;					// Flags for function.

            if (extension == null)
                throw new ArgumentNullException("extension");

            try
            {
                //add '.' if nessesry
                if ((extension.Length > 0) && (extension[0] != '.'))
                    extension = '.' + extension;

                shellInfo = new SHFILEINFO();

                // Get the icon through the shell.
                flags = (uint)(ShellInfoFlags.Icon | ShellInfoFlags.UseFileAttributes) | (uint)size;
                SHGetFileInfo(extension, 0, ref shellInfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);

                newIcon = ConvertUnmanagedIcon(shellInfo.IconHandle);
                return newIcon;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Function to return a string formatted to the nearest unit of measure for byte amounts.
        /// </summary>
        /// <param name="amount">Amount in bytes.</param>
        /// <returns>A string containing the amount and proper unit for memory.</returns>
        public static string FormatByteUnits(ulong amount)
        {
            double value = amount;		// Amount to convert.

            // If we get less than 1 here, we're dealing with bytes.
            if ((value / 1024) < 1)
                return amount.ToString() + " bytes";

            // Convert to KB.
            value /= 1024;

            // If we get less than 1 here, we're dealing with kilobytes.
            if ((value / 1024) < 1)
                return value.ToString("0.00") + " KB";

            // Convert to MB.
            value /= 1024;

            // If we get less than 1 here, we're dealing with megabytes.
            if ((value / 1024) < 1)
                return value.ToString("0.00") + " MB";

            // Convert to GB.
            value /= 1024;

            // If we get less than 1 here, we're dealing with gigabytes.
            if ((value / 1024) < 1)
                return value.ToString("0.00") + " GB";

            // Convert to TB.
            value /= 1024;

            // Default to terrabytes.
            return value.ToString("0.00") + " TB";
        }
    }
}
