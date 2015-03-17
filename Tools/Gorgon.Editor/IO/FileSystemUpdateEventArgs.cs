#region MIT.
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
// Created: Monday, March 16, 2015 9:01:38 PM
// 
#endregion

using System;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// File system updated event arguments.
	/// </summary>
	class FileSystemUpdateEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return file system object that was just updated.
		/// </summary>
		public IEditorFileSystem FileSystem
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemUpdateEventArgs"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system that was updated.</param>
		public FileSystemUpdateEventArgs(IEditorFileSystem fileSystem)
		{
			if (fileSystem == null)
			{
				throw new ArgumentNullException("fileSystem");
			}

			FileSystem = fileSystem;
		}
		#endregion
	}
}
