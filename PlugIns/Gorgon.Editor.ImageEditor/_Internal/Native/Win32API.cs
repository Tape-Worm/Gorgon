﻿
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: February 27, 2019 2:14:19 PM
// 

using System.Buffers;
using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Editor.ImageEditor.Native;

/// <summary>
/// Result value for the find executable function
/// </summary>
enum FindExecutableResult
{
    /// <summary>
    /// Call was succesful.
    /// </summary>
    Successful = 0,
    /// <summary>
    /// The file was not found.
    /// </summary>
    FileNotFound = 2,
    /// <summary>
    /// The path to the file was not valid.
    /// </summary>
    PathNotValid = 3,
    /// <summary>
    /// Access is denied.
    /// </summary>
    AccessDenied = 4,
    /// <summary>
    /// Out of memory.
    /// </summary>
    OutOfMemory = 8,
    /// <summary>
    /// No association found for the file.
    /// </summary>
    NoAssociation = 31
}

/// <summary>
/// Flags used in AssocQueryString
/// </summary>
[Flags]
enum AssociationFlags
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0x00000000,
    /// <summary>
    /// Do not remap the CLS ID.
    /// </summary>
    DontRemapCLSID = 0x00000001,
    /// <summary>
    /// Initialize by executable name.
    /// </summary>
    InitializeByExeName = 0x00000002,
    /// <summary>
    /// Open by executable name.
    /// </summary>
    OpenByExeName = InitializeByExeName,
    /// <summary>
    /// Initialize default to the star (*) registry entry.
    /// </summary>
    DefaultToStar = 0x00000004,
    /// <summary>
    /// Initialize default to the folder sub key.
    /// </summary>
    DefaultToFolder = 0x00000008,
    /// <summary>
    /// No user settings.
    /// </summary>
    NoUserSettings = 0x00000010,
    /// <summary>
    /// Do not truncate the return string.
    /// </summary>
    DontTruncate = 0x00000020,
    /// <summary>
    /// Verify that the data is accurate.
    /// </summary>
    Verify = 0x00000040,
    /// <summary>
    /// Ignore rundll.exe and return information about the target.
    /// </summary>
    RemapRunDLL = 0x00000080,
    /// <summary>
    /// Do not fix up errors in the registry.
    /// </summary>
    DontFixRegistry = 0x00000100,
    /// <summary>
    /// Ignore the base class.
    /// </summary>
    IgnoreBaseClass = 0x00000200
};

/// <summary>
/// Type of association string
/// </summary>
enum AssociationStringType
{
    /// <summary>
    /// Command string associated with a shell verb.
    /// </summary>
    Command = 1,
    /// <summary>
    /// An executable from a Shell verb command string.
    /// </summary>
    Executable = 2,
    /// <summary>
    /// The friendly name of a document type.
    /// </summary>
    FriendlyDocumentName = 3,
    /// <summary>
    /// The friendly name of an executable file.
    /// </summary>
    FriendlyApplicationName = 4,
    /// <summary>
    /// Ignore the information associated with the open subkey.
    /// </summary>
    NoOpen = 5,
    /// <summary>
    /// Look under the ShellNew subkey.
    /// </summary>
    ShellNewValue = 6,
    /// <summary>
    /// A template for DDE commands.
    /// </summary>
    DDECommand = 7,
    /// <summary>
    /// The DDE command to use to create a process.
    /// </summary>
    DDEProcessCommand = 8,
    /// <summary>
    /// The application name in a DDE broadcast.
    /// </summary>
    DDEApplication = 9,
    /// <summary>
    /// The topic name in a DDE broadcast.
    /// </summary>
    DDETopic = 10,
    /// <summary>
    /// Corresponds to the InfoTip registry value.
    /// </summary>
    InfoTip = 11,
    /// <summary>
    /// Corresponds to the QuickTip registry value.
    /// </summary>
    QuickTip = 12,
    /// <summary>
    /// Corresponds to the TileInfo registry value.
    /// </summary>
    TileInfo = 13,
    /// <summary>
    /// Describes a general type of MIME file association, such as image and bmp, so that applications can make general assumptions about a specific file type.
    /// </summary>
    ContentType = 14,
    /// <summary>
    /// Returns the path to the icon resources to use by default for this association.
    /// </summary>
    DefaultIcon = 15,
    /// <summary>
    /// For an object that has a Shell extension associated with it, you can use this to retrieve the CLSID of that Shell extension object.
    /// </summary>
    ShellExtension = 16,
    /// <summary>
    /// For a verb invoked through COM and the IDropTarget interface, you can use this flag to retrieve the IDropTarget object's CLSID.
    /// </summary>
    DropTarget = 17,
    /// <summary>
    /// For a verb invoked through COM and the IExecuteCommand interface, you can use this flag to retrieve the IExecuteCommand object's CLSID.
    /// </summary>
    DelegateExecute = 18,
    /// <summary>
    /// Max value.
    /// </summary>
    Max = 20
};

/// <summary>
/// Contains native Win32 functions
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal static partial class Win32API
{
    /// <summary>
    /// Function to retrieve a string for a file.
    /// </summary>
    /// <param name="flags">Flags defining how to search.</param>
    /// <param name="stringType">Type of string to query.</param>
    /// <param name="filePath">Path to the file to look up.</param>
    /// <param name="extra">Additional information about the location of the string.</param>
    /// <param name="result">The value for the string being queried.</param>
    /// <param name="size">The size of the result string buffer.</param>
    /// <returns>A value to indicate success or failure.</returns>
    [LibraryImport("Shlwapi.dll", EntryPoint = "AssocQueryStringW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AssocQueryString(AssociationFlags flags,
                                                AssociationStringType stringType,
                                                string filePath,
                                                string extra,
                                                [Out] char[] result,
                                                ref int size);

    /// <summary>
    /// Function to retrieve the friendly name for the executable.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <returns>A string containing the friendly name, or an empty string if no friendly name is found.</returns>
    public static string GetFriendlyExeName(string filePath)
    {
        char[] result = null;
        int size = 0;
        int hresult = AssocQueryString(AssociationFlags.Verify | AssociationFlags.OpenByExeName,
                                       AssociationStringType.FriendlyApplicationName,
                                       filePath,
                                     null,
                                       result,
                                       ref size);

        if ((size <= 0) || (hresult != 1))
        {
            return string.Empty;
        }

        result = ArrayPool<char>.Shared.Rent(size);

        try
        {
            if (AssocQueryString(AssociationFlags.Verify | AssociationFlags.OpenByExeName,
                                    AssociationStringType.FriendlyApplicationName,
                                    filePath,
                                    null,
                                    result,
                                    ref size) != 0)
            {
                return string.Empty;
            }

            int nullCharPos = result.Length - Array.IndexOf(result, '\0');

            return new string(result, 0, result.Length - nullCharPos);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(result, true);
        }
    }

    /// <summary>
    /// Function to retrieve the path to the associated executable file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <returns>The path to the associated executable file, or an empty string if no executable is associated with the file.</returns>
    public static string GetAssociatedExecutable(string filePath)
    {
        char[] result = null;
        int size = 0;
        int hresult = AssocQueryString(AssociationFlags.DontRemapCLSID | AssociationFlags.RemapRunDLL, AssociationStringType.Executable, filePath, null, result, ref size);

        // WARNING: That shitty fucking photo UWP app steals the registration of some image files.  When this happens, we can't get the path info.  UWP is ABSOLUTE GARBAGE.
        if ((size <= 0) || (hresult != 1))
        {
            return string.Empty;
        }

        result = ArrayPool<char>.Shared.Rent(size);

        try
        {
            if (AssocQueryString(AssociationFlags.DontRemapCLSID | AssociationFlags.RemapRunDLL,
                            AssociationStringType.Executable,
                            filePath,
                            null,
                            result,
                            ref size) != 0)
            {
                return string.Empty;
            }

            int nullCharPos = result.Length - Array.IndexOf(result, '\0');

            return new(result, 0, result.Length - nullCharPos);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(result, true);
        }
    }
}
