#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 30, 2012 8:25:32 AM
// 
// This code was originally written by Paul Ingles on CodeProject at http://www.codeproject.com/Articles/2532/Obtaining-and-managing-file-and-folder-icons-using
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// Shell file info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	struct SHFILEINFO
	{
		/// <summary>
		/// 
		/// </summary>
		public IntPtr hIcon;
		/// <summary>
		/// 
		/// </summary>
		public int iIcon;
		/// <summary>
		/// 
		/// </summary>
		public uint dwAttributes;
		/// <summary>
		/// 
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;
		/// <summary>
		/// 
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	};

	/// <summary>
	/// File attributes flags.
	/// </summary>
	[Flags()]
	enum ShellFileAttributes
	{
		/// <summary>
		/// 
		/// </summary>
		FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
		/// <summary>
		/// 
		/// </summary>
		FILE_ATTRIBUTE_NORMAL = 0x00000080
	}


	/// <summary>
	/// Shell flags.
	/// </summary>
	[Flags()]
	enum ShellFlags
	{
		/// <summary>
		/// 
		/// </summary>
		SHGFI_ICON = 0x000000100,     // get icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_DISPLAYNAME = 0x000000200,     // get display name
		/// <summary>
		/// 
		/// </summary>
		SHGFI_TYPENAME = 0x000000400,     // get type name
		/// <summary>
		/// 
		/// </summary>
		SHGFI_ATTRIBUTES = 0x000000800,     // get attributes
		/// <summary>
		/// 
		/// </summary>
		SHGFI_ICONLOCATION = 0x000001000,     // get icon location
		/// <summary>
		/// 
		/// </summary>
		SHGFI_EXETYPE = 0x000002000,     // return exe type
		/// <summary>
		/// 
		/// </summary>
		SHGFI_SYSICONINDEX = 0x000004000,     // get system icon index
		/// <summary>
		/// 
		/// </summary>
		SHGFI_LINKOVERLAY = 0x000008000,     // put a link overlay on icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_SELECTED = 0x000010000,     // show icon in selected state
		/// <summary>
		/// 
		/// </summary>
		SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
		/// <summary>
		/// 
		/// </summary>
		SHGFI_LARGEICON = 0x000000000,     // get large icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_SMALLICON = 0x000000001,     // get small icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_OPENICON = 0x000000002,     // get open icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_SHELLICONSIZE = 0x000000004,     // get shell size icon
		/// <summary>
		/// 
		/// </summary>
		SHGFI_PIDL = 0x000000008,     // pszPath is a pidl
		/// <summary>
		/// 
		/// </summary>
		SHGFI_USEFILEATTRIBUTES = 0x000000010,     // use passed dwFileAttribute
		/// <summary>
		/// 
		/// </summary>
		SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
		/// <summary>
		/// 
		/// </summary>
		SHGFI_OVERLAYINDEX = 0x000000040     // Get the index of the overlay
	}

	/// <summary>
	/// Browsing flags.
	/// </summary>
	[Flags()]
	enum ShellBrowseFlags
	{
		/// <summary>
		/// Only return directories.
		/// </summary>
		BIF_RETURNONLYFSDIRS = 0x0001,
		/// <summary>
		/// 
		/// </summary>
		BIF_DONTGOBELOWDOMAIN = 0x0002,
		/// <summary>
		/// 
		/// </summary>
		BIF_STATUSTEXT = 0x0004,
		/// <summary>
		/// 
		/// </summary>
		BIF_RETURNFSANCESTORS = 0x0008,
		/// <summary>
		/// 
		/// </summary>
		BIF_EDITBOX = 0x0010,
		/// <summary>
		/// 
		/// </summary>
		BIF_VALIDATE = 0x0020,
		/// <summary>
		/// 
		/// </summary>
		BIF_NEWDIALOGSTYLE = 0x0040,
		/// <summary>
		/// 
		/// </summary>
		BIF_USENEWUI = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX),
		/// <summary>
		/// 
		/// </summary>
		BIF_BROWSEINCLUDEURLS = 0x0080,
		/// <summary>
		/// 
		/// </summary>
		BIF_BROWSEFORCOMPUTER = 0x1000,
		/// <summary>
		/// 
		/// </summary>
		BIF_BROWSEFORPRINTER = 0x2000,
		/// <summary>
		/// 
		/// </summary>
		BIF_BROWSEINCLUDEFILES = 0x4000,
		/// <summary>
		/// 
		/// </summary>
		BIF_SHAREABLE = 0x8000,
	}

	/// <summary>
	/// Win 32 API functionality.
	/// </summary>
	[SuppressUnmanagedCodeSecurity()]
	static class Win32API
	{



		/// <summary>
		/// 
		/// </summary>
		/// <param name="pszPath"></param>
		/// <param name="dwFileAttributes"></param>
		/// <param name="psfi"></param>
		/// <param name="cbFileInfo"></param>
		/// <param name="uFlags"></param>
		/// <returns></returns>
		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(
			string pszPath,
			ShellFileAttributes dwFileAttributes,
			ref SHFILEINFO psfi,
			uint cbFileInfo,
			ShellFlags uFlags
			);

		/// <summary>
		/// Provides access to function required to delete handle. This method is used internally
		/// and is not required to be called separately.
		/// </summary>
		/// <param name="hIcon">Pointer to icon handle.</param>
		/// <returns>N/A</returns>
		[DllImport("User32.dll")]
		private static extern int DestroyIcon(IntPtr hIcon);

		/// <summary>
		/// Function to retrieve the type of the file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The file type.</returns>
		public static string GetFileType(string path)
		{
			// Need to add size check, although errors generated at present!
			ShellFlags flags = ShellFlags.SHGFI_USEFILEATTRIBUTES | ShellFlags.SHGFI_TYPENAME;

			// Get the folder icon
			SHFILEINFO shfi = new SHFILEINFO();
			SHGetFileInfo(path, ShellFileAttributes.FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), flags);

			return shfi.szTypeName;
		}

		/// <summary>
		/// Function to retrieve an icon from the file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="large">TRUE to get large icons, FALSE to get small.</param>
		/// <returns>The icon in the file.</returns>
		public static System.Drawing.Icon GetFileIcon(string path, bool large)
		{
			// Need to add size check, although errors generated at present!
			ShellFlags flags = ShellFlags.SHGFI_ICON | ShellFlags.SHGFI_USEFILEATTRIBUTES;

			if (!large)
				flags |= ShellFlags.SHGFI_SMALLICON;
			else
				flags |= ShellFlags.SHGFI_LARGEICON;

			// Get the folder icon
			SHFILEINFO shfi = new SHFILEINFO();
			SHGetFileInfo(path, ShellFileAttributes.FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), flags);

			System.Drawing.Icon.FromHandle(shfi.hIcon);	// Load the icon from an HICON handle

			// Now clone the icon, so that it can be successfully stored in an ImageList
			System.Drawing.Icon icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();

			DestroyIcon(shfi.hIcon);		// Cleanup
			return icon;
		}

		/// <summary>
		/// Function to retrieve the shell icon for a folder.
		/// </summary>
		/// <param name="getOpen">TRUE to get the open folder icon, FALSE to get the closed.</param>
		/// <param name="large">TRUE to get large icons, FALSE to get small.</param>
		/// <returns>The icon for the folder.</returns>
		public static System.Drawing.Icon GetFolderIcon(bool getOpen, bool large)
		{
			// Need to add size check, although errors generated at present!
			ShellFlags flags = ShellFlags.SHGFI_ICON | ShellFlags.SHGFI_USEFILEATTRIBUTES;

			if (getOpen)
				flags |= ShellFlags.SHGFI_OPENICON;

			if (!large)
				flags |= ShellFlags.SHGFI_SMALLICON;
			else
				flags |= ShellFlags.SHGFI_LARGEICON;

			// Get the folder icon
			SHFILEINFO shfi = new SHFILEINFO();
			SHGetFileInfo(" ", ShellFileAttributes.FILE_ATTRIBUTE_DIRECTORY, ref shfi, (uint) System.Runtime.InteropServices.Marshal.SizeOf(shfi), flags );

			System.Drawing.Icon.FromHandle(shfi.hIcon);	// Load the icon from an HICON handle

			// Now clone the icon, so that it can be successfully stored in an ImageList
			System.Drawing.Icon icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();

			DestroyIcon( shfi.hIcon );		// Cleanup
			return icon;
		}	
	}
}
