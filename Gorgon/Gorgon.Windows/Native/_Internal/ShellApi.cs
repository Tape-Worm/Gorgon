// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 29, 2018 12:34:04 PM
// 

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native;

/// <summary>
/// The shell icon indexes for extraction from the standard shell library.
/// </summary>
internal enum StandardShellIcons
{
    /// <summary>
    /// Removable drives.
    /// </summary>
    Removable = 6,

    /// <summary>
    /// Fixed disks.
    /// </summary>
    Fixed = 7,

    /// <summary>
    /// Network drive.
    /// </summary>
    Network = 9,

    /// <summary>
    /// CD Rom.
    /// </summary>
    CdRom = 11,

    /// <summary>
    /// Ram disk.
    /// </summary>
    RamDisk = 12,

    /// <summary>
    /// Empty folder.
    /// </summary>
    Folder = 4,

    /// <summary>
    /// Unknown type icon.
    /// </summary>
    Unknown = 0
}

/// <summary>
/// Shell get file info flags.
/// </summary>
[Flags]
internal enum SHGFI : uint
{
    /// <summary>get icon</summary>
    Icon = 0x000000100,

    /// <summary>get display name</summary>
    DisplayName = 0x000000200,

    /// <summary>get type name</summary>
    TypeName = 0x000000400,

    /// <summary>get attributes</summary>
    Attributes = 0x000000800,

    /// <summary>get icon location</summary>
    IconLocation = 0x000001000,

    /// <summary>return exe type</summary>
    ExeType = 0x000002000,

    /// <summary>get system icon index</summary>
    SysIconIndex = 0x000004000,

    /// <summary>put a link overlay on icon</summary>
    LinkOverlay = 0x000008000,

    /// <summary>show icon in selected state</summary>
    Selected = 0x000010000,

    /// <summary>get only specified attributes</summary>
    Attr_Specified = 0x000020000,

    /// <summary>get large icon</summary>
    LargeIcon = 0x000000000,

    /// <summary>get small icon</summary>
    SmallIcon = 0x000000001,

    /// <summary>get open icon</summary>
    OpenIcon = 0x000000002,

    /// <summary>get shell size icon</summary>
    ShellIconSize = 0x000000004,

    /// <summary>pszPath is a pidl</summary>
    PIDL = 0x000000008,

    /// <summary>use passed dwFileAttribute</summary>
    UseFileAttributes = 0x000000010,

    /// <summary>apply the appropriate overlays</summary>
    AddOverlays = 0x000000020,

    /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
    OverlayIndex = 0x000000040
}

/// <summary>
/// Icon sizes for the shell icons.
/// </summary>
internal enum ShellIconSize
{
    /// <summary>
    /// Get large icons (32x32) - Affected by DPI.
    /// </summary>
    Large = 0,
    /// <summary>
    /// Small icons (16x16) - Affected by DPI.
    /// </summary>
    Small = 0x1,
    /// <summary>
    /// Extra large icons (48x48) - Affected by DPI.
    /// </summary>
    ExtraLarge = 0x2,
    /// <summary>
    /// Small icons (16x16 - defined by system).
    /// </summary>
    SysSmall = 0x3,
    /// <summary>
    /// Very large icons (256x256)
    /// </summary>
    Jumbo = 0x4
}

/// <summary>
/// Functionality from ShellApi.dll
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal partial class ShellApi
{

    /// <summary>
    /// ID for the image list COM objects.
    /// </summary>
    private const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
    /// <summary>
    /// Image should have transparency.
    /// </summary>
    private const int ILD_TRANSPARENT = 0x00000001;
    /// <summary>
    /// Mask not required.
    /// </summary>
    private const int ILD_IMAGE = 0x00000020;



    /// <summary>
    /// Function to retrieve an image list.
    /// </summary>
    /// <param name="iImageList"></param>
    /// <param name="riid"></param>
    /// <param name="ppv"></param>
    /// <returns></returns>
    [LibraryImport("shell32.dll", EntryPoint = "#727")]
    private static partial int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

    /// <summary>
    /// Function to extract a standard shell icon and return it as an Icon image.
    /// </summary>
    /// <param name="icon">The icon index to look up.</param>
    /// <returns>The Icon if found, or <b>null</b> if not.</returns>
    public static Icon ExtractShellIcon(StandardShellIcons icon)
    {
        IImageList imageList = null;

        // COM interface ID for the shell image list.
        var IID = new Guid(IID_IImageList);

        _ = SHGetImageList((int)ShellIconSize.ExtraLarge, ref IID, ref imageList);

        if (imageList is null)
        {
            return null;
        }

        _ = imageList.GetIcon((int)icon, ILD_TRANSPARENT | ILD_IMAGE, out nint hIcon);

        Icon result = null;

        if (hIcon != IntPtr.Zero)
        {
            result = Icon.FromHandle(hIcon);
        }

        return result;
    }

    /// <summary>
    /// Initializes static members of the <see cref="ShellApi"/> class.
    /// </summary>
    static ShellApi() => Marshal.PrelinkAll(typeof(ShellApi));
}
