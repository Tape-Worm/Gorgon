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
// Created: May 7, 2024 3:02:06 PM
//

namespace Gorgon.IO.FileSystem;

/// <summary>
/// Options to pass a delete operation on a <see cref="GorgonFileSystem"/>.
/// </summary>
/// <param name="ProgressCallback">The method used to report progress as files and subdirectories are deleted.</param>
/// <param name="CancellationToken">A token used to send a cancellation request to the method.</param>
/// <remarks>
/// <para>
/// Applications can pass these options to the <see cref="IGorgonFileSystem.DeleteDirectory"/> or <see cref="IGorgonFileSystem.DeleteFile"/> methods to change the behaviour of the methods. For example, 
/// an application may want to monitor the progress of a delete operation, or, provide a means to allow the user to cancel the operation.
/// </para>
/// <para>
/// If the <paramref name="ProgressCallback"/> callback method is defined, then that method will be called for every sub directory and file that is being deleted. This is used to report progress back to the 
/// calling application. The progress method will take a string which contains the fully qualified virtual file system path to the sub directory or file being deleted.
/// </para>
/// <para>
/// The <paramref name="CancellationToken"/>, is used stop the delete process as quickly as possible. Please note however, that no previous deletions will be restored.
/// </para>
/// </remarks>
public record GorgonFileSystemDeleteOptions(Action<string>? ProgressCallback,
                                            CancellationToken CancellationToken)
{
    /// <summary>
    /// The default values for the file system delete options.
    /// </summary>
    public static readonly GorgonFileSystemDeleteOptions Default = new(null, CancellationToken.None);
}
