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
// Created: Sunday, September 20, 2015 12:06:21 AM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.IO.Providers
{
	/// <inheritdoc cref="IGorgonPhysicalFileSystemData"/>
	public class GorgonPhysicalFileSystemData
		: IGorgonPhysicalFileSystemData
	{
		#region Properties.
		/// <inheritdoc/>
		public IReadOnlyList<string> Directories
		{
			get;
		}

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonPhysicalFileInfo> Files
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPhysicalFileSystemData"/> class.
		/// </summary>
		/// <param name="directories">The directories.</param>
		/// <param name="files">The files.</param>
		public GorgonPhysicalFileSystemData(IReadOnlyList<string> directories, IReadOnlyList<IGorgonPhysicalFileInfo> files)
		{
			Directories = directories;
			Files = files;
		}
		#endregion
	}
}
