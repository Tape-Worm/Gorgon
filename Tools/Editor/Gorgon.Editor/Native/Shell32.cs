#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: November 28, 2018 6:52:18 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Gorgon.Editor.Native;

/// <summary>
/// Possible flags for the SHFileOperation method.
/// </summary>
[Flags]
internal enum FileOperationFlags
{
    /// <summary>
    /// Do not show a dialog during the process
    /// </summary>
    FOF_SILENT = 0x0004,
    /// <summary>
    /// Do not ask the user to confirm selection
    /// </summary>
    FOF_NOCONFIRMATION = 0x0010,
    /// <summary>
    /// Do not show the names of the files or folders that are being recycled.
    /// </summary>
    FOF_SIMPLEPROGRESS = 0x0100,
    /// <summary>
    /// Surpress errors, if any occur during the process.
    /// </summary>
    FOF_NOERRORUI = 0x0400,
    /// <summary>
    /// Warn if files are too big to fit in the recycle bin and will need
    /// to be deleted completely.
    /// </summary>
    FOF_WANTNUKEWARNING = 0x4000,
}

/// <summary>
/// File Operation Function Type for SHFileOperation
/// </summary>
internal enum FileOperationType
{
    /// <summary>
    /// Move the objects
    /// </summary>
    FO_MOVE = 0x0001,
    /// <summary>
    /// Copy the objects
    /// </summary>
    FO_COPY = 0x0002,
    /// <summary>
    /// Delete (or recycle) the objects
    /// </summary>
    FO_DELETE = 0x0003,
    /// <summary>
    /// Rename the object(s)
    /// </summary>
    FO_RENAME = 0x0004,
}

/// <summary>
/// SHFILEOPSTRUCT for SHFileOperation from COM
/// </summary>
[NativeMarshalling(typeof(ShFileOpStructMarshaller))]
[StructLayout(LayoutKind.Sequential)]
internal struct SHFILEOPSTRUCT
{
    public nint hwnd;
    public FileOperationType wFunc;
    public string pFrom;
    public string pTo;
    public FileOperationFlags fFlags;
    public bool fAnyOperationsAborted;
    public nint hNameMappings;
    public string lpszProgressTitle;
}

/// <summary>
/// Shell 32 functionality.
/// </summary>
/// <remarks>
/// <para>
/// This code was lifted from https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin.
/// </para>
/// </remarks>
internal static partial class Shell32
{
    [LibraryImport("shell32.dll")]
    private static partial int SHFileOperation(ref SHFILEOPSTRUCT fileOp);

    /// <summary>
    /// Send file to recycle bin
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
    public static bool SendToRecycleBin(string path, FileOperationFlags flags)
    {
        try
        {
            var fs = new SHFILEOPSTRUCT
            {
                wFunc = FileOperationType.FO_DELETE,
                pFrom = path + '\0' + '\0',
                fFlags = (FileOperationFlags)0x40 | flags // This combines the UNDO flag with our chosen flags.
            };
            _ = SHFileOperation(ref fs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Initializes static members of the <see cref="Shell32"/> class.</summary>
    static Shell32() => Marshal.PrelinkAll(typeof(Shell32));
}
