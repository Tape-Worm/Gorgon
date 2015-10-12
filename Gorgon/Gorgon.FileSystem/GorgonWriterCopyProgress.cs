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
// Created: Friday, October 2, 2015 8:41:40 PM
// 
#endregion

namespace Gorgon.IO
{
	/// <summary>
	/// Progress information passed to a calling application via the <see cref="IGorgonFileSystemWriter{T}.CopyFromAsync"/> method.
	/// </summary>
	/// <remarks>
	/// This information only includes file copy information. The directory that the file is copied from is stored in the <see cref="File"/> value and can be obtained from there.
	/// </remarks>
	public struct GorgonWriterCopyProgress
	{
		/// <summary>
		/// The current file being copied.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Yes, it is immutable")]
		public readonly IGorgonVirtualFile File;
		/// <summary>
		/// The total number of files being copied.
		/// </summary>
		public readonly int TotalFiles;
		/// <summary>
		/// The total number of directories being copied.
		/// </summary>
		public readonly int TotalDirectories;
		/// <summary>
		/// The number of files copied at this point.
		/// </summary>
		public readonly int FilesCopied;

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWriterCopyProgress"/> struct.
		/// </summary>
		/// <param name="file">The file being copied.</param>
		/// <param name="filesCopied">The number of files copied at this point.</param>
		/// <param name="totalFiles">The total number of files.</param>
		/// <param name="totalDirectories">The total number of directories.</param>
		public GorgonWriterCopyProgress(IGorgonVirtualFile file, int filesCopied, int totalFiles, int totalDirectories)
		{
			File = file;
			FilesCopied = filesCopied;
			TotalFiles = totalFiles;
			TotalDirectories = totalDirectories;
		}
	}
}
