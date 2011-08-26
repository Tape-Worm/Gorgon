#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, June 27, 2011 9:36:13 AM
// 
#endregion

using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// Plug-in entry point for the zip file file system provider plug-in.
	/// </summary>
	public class GorgonZipPlugIn
		: GorgonFileSystemProviderPlugIn 
	{
		/// <summary>
		/// Function to create a new file system provider plug-in instance.
		/// </summary>
		/// <param name="fileSystem">File system that owns this provider.</param>
		/// <returns>The file system provider plug-in.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		public override GorgonFileSystemProvider CreateProvider(GorgonFileSystem fileSystem)
		{
			return new ZipProvider.GorgonZipFileSystemProvider(fileSystem);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonZipPlugIn"/> class.
		/// </summary>
		public GorgonZipPlugIn()
			: base("A zip file provider for the Gorgon virtual file system interface.")
		{
		}
	}
}
