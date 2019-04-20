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
// Created: Wednesday, September 30, 2015 8:42:57 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <summary>
	/// An implementation of the <see cref="IGorgonFileSystemProvider"/> that acts similar to a ram disk.
	/// </summary>
	/// <remarks>
	/// <para>
	/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
	/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
	/// file system file, or a file inside of a zip archive.  
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// As the documentation states, providers can read data from a file system. However, no mechanism is available to write to a file system through a provider. This is by design. The <see cref="IGorgonFileSystemWriter{T}"/> 
	/// type allows writing to a file system via a predefined area in a physical file system. 
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// When this type is implemented, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). And since this type inherits from <see cref="GorgonPlugin"/>, 
	/// the file system provider can be loaded dynamically through Gorgon's plug in system.
	/// </para>
	/// <para>
	/// Some providers may not use a physical location on the operating system file system. In such cases, implementors of a <see cref="IGorgonFileSystemProvider"/> must provide a prefix for a physical location 
	/// (e.g. <c>Mount("::\\Prefix\DirectoryName", "/");</c>, <c>Mount("::\\Prefix", "/")</c>, or whatever else the provider chooses). This prefix is specific to the provider and should be made available via 
	/// the <see cref="Prefix"/> property. The prefix must <u>always</u> begin with the characters <c>::\\</c>. Otherwise, the <see cref="IGorgonFileSystem"/> will not know how to parse the physical location.
	/// </para>
	/// <para>
	/// This implementation uses a dictionary of <see cref="MemoryStream"/> objects to hold file data. When mounting using this provider, call <see cref="IGorgonFileSystem.Mount"/> with the 
	/// <c>physicalLocation</c> parameter set to: <c>::\\Memory</c>.
	/// </para>
	/// <para>
	/// The streams used to hold the file system data are local to this object. Because of this, creating multiple instances of this provider will create multiple physical file systems. 
	/// </para>
	/// <para>
	/// Because this provider stores file data in memory, it is not recommended for use with a large amount of file data.
	/// </para>
	/// </remarks>
	public class GorgonFileSystemRamDiskProvider
		: GorgonPlugin, IGorgonFileSystemProvider
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of files in the physical file system.
		/// </summary>
		internal RamDiskFileSystem FileData
		{
			get;
		}

		/// <summary>
		/// Property to return the provider specific prefix for a physical location.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some providers may not use a physical location on the operating system file system. In such cases, implementors of a <see cref="IGorgonFileSystemProvider"/> must provide a prefix for a physical 
		/// location (e.g. <c>Mount("::\\Prefix\DirectoryName", "/");</c>, <c>Mount("::\\Prefix", "/")</c>, or whatever else the provider chooses). 
		/// </para>
		/// <para>
		/// This value must <u>always</u> begin with the characters <c>::\\</c>. Otherwise, the <see cref="IGorgonFileSystem"/> will not know how to parse the physical location.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// If the provider accesses a physical file system directory or file for its information, then this value should always return <see cref="string.Empty"/> or <b>null</b>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public string Prefix => @"::\\Memory";

		/// <summary>
		/// Property to return a list of preferred file extensions (if applicable).
		/// </summary>
		/// <remarks>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> that reads from a packed file should supply a list of well known file name extensions wrapped in <see cref="GorgonFileExtension"/> objects for 
		/// that physical file system type. This list can then be then used in an application to filter the types of files to open with a <see cref="IGorgonFileSystem"/>. If the file system reads directories on 
		/// the native file system, then this collection should remain empty.
		/// </remarks>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
		}

        /// <summary>Property to return the path to the provider assembly (if applicable).</summary>
        public string ProviderPath => PlugInPath ?? string.Empty;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
        /// </summary>
        /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
        /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
        /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
        /// <remarks>
        /// Since this provider holds data in its own block of memory, there's nothing to enumerate when the provider is loaded. Thus, this will always return empty data.
        /// </remarks>
        protected virtual GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint) => new GorgonPhysicalFileSystemData(FileData.GetDirectories(),
                                                    FileData.GetFileInfos()
                                                            .Select(item =>
                                                                    new PhysicalFileInfo(Prefix + "::" + item.FullPath,
                                                                                         item.CreateDate,
                                                                                         item.Size,
                                                                                         item.FullPath,
                                                                                         0,
                                                                                         item.LastModified))
                                                            .ToArray());


        /// <summary>
        /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
        /// </summary>
        /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
        /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
        /// responsibility of the user to close the stream when finished.
        /// </para>
        /// </remarks>
        protected virtual Stream OnOpenFileStream(IGorgonVirtualFile file) => FileData.OpenReadStream(file.FullPath);

        /// <summary>
        /// Function to determine if a physical file system can be read by this provider.
        /// </summary>
        /// <param name="physicalPath">Path to the packed file containing the file system.</param>
        /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
        /// <remarks>
        /// This value will return <b>true</b> when the <paramref name="physicalPath"/> is set to <c>::\\Memory</c> on the <c>physicalLocation</c> parameter for the <see cref="IGorgonFileSystem.Mount"/> method.
        /// </remarks>
        public bool CanReadFileSystem(string physicalPath) => physicalPath.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
        /// </summary>
        /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
        /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
        /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
        /// <remarks>
        /// Since this provider holds data in its own block of memory, there's nothing to enumerate when the provider is loaded. Thus, this will always return empty data.
        /// </remarks>
        public GorgonPhysicalFileSystemData Enumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint) => OnEnumerate(physicalLocation, mountPoint);

        /// <summary>
        /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
        /// </summary>
        /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
        /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
        /// responsibility of the user to close the stream when finished.
        /// </para>
        /// </remarks>
        public Stream OpenFileStream(IGorgonVirtualFile file)
		{
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			
			return OnOpenFileStream(file);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemRamDiskProvider"/> class.
		/// </summary>
		public GorgonFileSystemRamDiskProvider()
			: base(Resources.GORFS_RAMDISK_FS_DESC)
		{
			PreferredExtensions = new GorgonNamedObjectDictionary<GorgonFileExtension>();
			FileData = new RamDiskFileSystem();
		}
		#endregion
	}
}
