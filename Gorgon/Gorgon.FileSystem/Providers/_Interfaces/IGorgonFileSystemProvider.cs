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
// Created: Saturday, September 19, 2015 11:40:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Collections;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <summary>
	/// A file system provider that mounts Windows file system directories.
	/// </summary>
	/// <remarks>
	/// <para>
	/// File system providers provide access to a physical file system, and allows the communications necessary to read the data from that physical file system. When used in conjunction with the <see cref="GorgonFileSystem"/> 
	/// object, a provider enables access to multiple types of physical file systems so that seamlessly appear to be a single file system. The underlying system has no idea if the file is a standard file system file, 
	/// or a file inside of a zip archive.  
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// As the documentation states, providers can read data from a file system. However, no mechanism is available to write to a file system through a provider. This is by design. The <see cref="GorgonFileSystem"/> 
	/// type allows writing via its <see cref="GorgonFileSystem.WriteLocation"/> property, and only through there can files be written.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// This unmodified type allows for the mounting of a directory on a physical disk so that data can be read from the native Windows file system. This is the default provider for any <see cref="GorgonFileSystem"/>.
	/// </para>
	/// <para>
	/// When this type is overridden, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). And since this type inherits from <see cref="GorgonPlugin"/>, 
	/// the file system provider can be loaded dynamically through Gorgon's plug in system.
	/// </para>
	/// </remarks>
	public interface IGorgonFileSystemProvider
	{
		/// <summary>
		/// Property to return a list of preferred file extensions (if applicable).
		/// </summary>
		/// <remarks>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> that reads from a packed file should supply a list of well known file name extensions wrapped in <see cref="GorgonFileExtension"/> objects for 
		/// that physical file system type. This list can then be then used in an application to filter the types of files to open with a <see cref="GorgonFileSystem"/>. If the file system reads directories on 
		/// the native file system, then this collection should remain empty.
		/// </remarks>
		IGorgonNamedObjectDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
		}

		/// <summary>
		/// Property to return the human readable description of the provider.
		/// </summary>
		string Description
		{
			get;
		}

		/// <summary>
		/// Function to enumerate the files and directories from a physical location and map it to a virtual location.
		/// </summary>
		/// <param name="physicalLocation">Mount point being enumerated.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>		
		/// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
		/// <remarks>
		/// <para>
		/// This will return a <see cref="IGorgonPhysicalFileSystemData"/> representing the paths to directories and files under the virtual file system. Each file system file and directory is mapped from its 
		/// <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the mount point is set to <c>/MyMount/</c>, and the 
		/// physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value would be <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
		/// </para>
		/// </remarks>
		IGorgonPhysicalFileSystemData Enumerate(string physicalLocation, GorgonFileSystemDirectory mountPoint);

		/// <summary>
		/// Function to open a stream to a file on the physical file system from the virtual file passed in.
		/// </summary>
		/// <param name="file">The virtual file that will be used to locate the file that will be opened on the physical file system.</param>
		/// <returns>A <see cref="Stream"/> to the file, or <b>null</b> (<i>Nothing</i> in VB.Net) if the file does not exist.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This will take the virtual <paramref name="file"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
		/// responsibility of the user to close the stream when finished.
		/// </para>
		/// </remarks>
		Stream OpenFileStream(GorgonFileSystemFileEntry file);

		/// <summary>
		/// Function to determine if a packed physical file system can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the packed file containing the file system.</param>
		/// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This will test a packed file (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method will always 
		/// return <b>false</b>.
		/// </para>
		/// </remarks>
		bool CanReadFile(string physicalPath);
	}
}