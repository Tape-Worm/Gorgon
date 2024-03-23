
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: January 5, 2020 10:33:13 PM
// 

namespace Gorgon.IO;

/// <summary>
/// A value indicating how to handle a file conflict in the file system
/// </summary>
public enum FileConflictResolution
{
    /// <summary>
    /// The operation should be canceled.
    /// </summary>
    Cancel = 0,
    /// <summary>
    /// The operation should overwrite the destination.
    /// </summary>
    Overwrite = 1,
    /// <summary>
    /// The operation should rename the destination.
    /// </summary>
    Rename = 2,
    /// <summary>
    /// The operation should overwrite the destination, and this should be the default for all conflicts from this point forward.
    /// </summary>
    OverwriteAll = 3,
    /// <summary>
    /// The operation should rename the destination, and this should be the default for all conflicts from this point forward.
    /// </summary>
    RenameAll = 4,
    /// <summary>
    /// An exception should be thrown.
    /// </summary>
    Exception = 5,
    /// <summary>
    /// The operation should skip this item.
    /// </summary>
    Skip = 6,
    /// <summary>
    /// The operation should skip this item and all conflicting items after.
    /// </summary>
    SkipAll = 7
}

/// <summary>
/// Callback options for copying a directory or file
/// </summary>
/// <remarks>
/// <para>
/// This is used to perform the copy with progress reporting, overwrite options, and cancelation support for long running operations. 
/// </para>
/// <para>
/// The typical use case for these callbacks/cancelation support is with a UI that reports progress, and allows the user to determine how to handle file overwriting. The cancellation support is provided 
/// for asynchronous copying, especially in cases where lots of data is being copied and a considerable amount of time is consumed during the copy
/// </para>
/// </remarks>
public class GorgonCopyCallbackOptions
{
    /// <summary>
    /// Property to set or return the callback used to report copy progress.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This callback method takes two parameters. The first is the full path to the directory or file being copied, and the second reports the total number of bytes copied for a given file as a 
    /// normalized percentage (e.g. 0 - 1.0). 
    /// </para>
    /// </remarks>
    public Action<string, double> ProgressCallback
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the callback used to handle a conflict where the source file shares the same name as the destination file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This callback method takes 2 parameters. The first is the full path to the file being copied, and the second is the destination file path. The return value of the callback method is a 
    /// <see cref="FileConflictResolution"/> enumeration type which tells the copy operation how to handle the conflict.
    /// </para>
    /// </remarks>
    /// <seealso cref="FileConflictResolution"/>
    public Func<string, string, FileConflictResolution> ConflictResolutionCallback
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the cancelation token for the copy operation.
    /// </summary>
    public CancellationToken CancelToken
    {
        get;
        set;
    } = CancellationToken.None;
}
