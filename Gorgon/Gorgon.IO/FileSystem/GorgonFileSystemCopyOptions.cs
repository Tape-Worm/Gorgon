// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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

namespace Gorgon.IO.FileSystem;

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
/// <param name="ProgressCallback">The method used to report progress as files and subdirectories are deleted.</param>
/// <param name="ConflictResolutionCallback">A method that is called when a file with the same name already exists at the destination.</param>
/// <param name="CancellationToken">A token used to send a cancellation request to the method.</param>
/// <remarks>
/// <para>
/// This is used to perform the copy with progress reporting, overwrite options, and cancelation support for long running operations. 
/// </para>
/// <para>
/// The typical use case for these callbacks/cancelation support is with a UI that reports progress, and allows the user to determine how to handle file overwriting. The cancellation support is provided 
/// for asynchronous copying, especially in cases where lots of data is being copied and a considerable amount of time is consumed during the copy
/// </para>
/// <para>
/// If the <paramref name="ProgressCallback"/> callback method is defined, then that method will be called for every file that is being copied. This is used to report progress back to the 
/// calling application. The progress method will take a string which contains the fully qualified virtual file system path to the file being copied, and a double value that reports the total number of 
/// bytes copied for a given file as a normalized percentage (e.g. 0 - 1.0).
/// </para>
/// <para>
/// If the <paramref name="ConflictResolutionCallback"/> callback method is defined, then that method will be called if the file being copied has the same name in the destination. The method will take 
/// a string as a parameter containing full path to the file being copied, and another string parameter that contains the destination file path. The method will return a <see cref="FileConflictResolution"/> 
/// value to indicate how to proceed when a conflict occurs.
/// </para>
/// <para>
/// The <paramref name="CancellationToken"/>, is used stop the copy process as quickly as possible. When the copy operation is canceled, and a file is in the middle of being copied, then the current partially 
/// copied file will be deleted.
/// </para>
/// </remarks>
public record GorgonFileSystemCopyOptions(Action<string, double> ProgressCallback, Func<string, string, FileConflictResolution> ConflictResolutionCallback, CancellationToken CancellationToken)
{
    /// <summary>
    /// Default method for progress.
    /// </summary>
    /// <param name="name">Not used.</param>
    /// <param name="percent">Not used.</param>
    private static void DefaultProgress(string name, double percent)
    {
    }

    /// <summary>
    /// Default method for file conflict resolution.
    /// </summary>
    /// <param name="src">Not used.</param>
    /// <param name="dest">Not used.</param>
    /// <returns><see cref="FileConflictResolution.Overwrite"/></returns>
    private static FileConflictResolution DefaultResolution(string src, string dest) => FileConflictResolution.Overwrite;

    /// <summary>
    /// The default copy operation options.
    /// </summary>
    public static readonly GorgonFileSystemCopyOptions Default = new(DefaultProgress, DefaultResolution, CancellationToken.None);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemCopyOptions"/> class.
    /// </summary>
    /// <param name="conflictResolutionCallback">A method that is called when a file with the same name already exists at the destination.</param>
    /// <param name="cancelToken">A token used to send a cancellation request to the method.</param>
    public GorgonFileSystemCopyOptions(Func<string, string, FileConflictResolution> conflictResolutionCallback, CancellationToken cancelToken)
        : this(DefaultProgress, conflictResolutionCallback, cancelToken)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemCopyOptions"/> class.
    /// </summary>
    /// <param name="progressCallback">The method used to report progress as files and subdirectories are deleted.</param>
    /// <param name="cancelToken">A token used to send a cancellation request to the method.</param>
    public GorgonFileSystemCopyOptions(Action<string, double> progressCallback, CancellationToken cancelToken)
        : this(progressCallback, DefaultResolution, cancelToken)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemCopyOptions"/> class.
    /// </summary>
    /// <param name="cancelToken">A token used to send a cancellation request to the method.</param>
    public GorgonFileSystemCopyOptions(CancellationToken cancelToken)
        : this(DefaultProgress, DefaultResolution, cancelToken)
    {
    }
}
