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
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <summary>
	/// An implementation of the <see cref="IGorgonFileSystemProvider"/> that acts similar to a ram disk.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="IGorgonFileSystemProvider"/>
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
	[ExcludeAsPlugin]
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

		/// <inheritdoc/>
		public string Prefix => @"::\\Memory";

		/// <inheritdoc/>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
		}
		#endregion

		#region Methods.
		/// <inheritdoc cref="Enumerate"/>
		protected virtual GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			return new GorgonPhysicalFileSystemData(FileData.GetDirectories(),
			                                        FileData.GetFileInfos()
			                                                .Select(item =>
			                                                        new PhysicalFileInfo(Prefix + "::" + item.FullPath,
			                                                                             item.CreateDate,
			                                                                             item.Size,
			                                                                             item.FullPath,
			                                                                             0,
			                                                                             item.LastModified))
			                                                .ToArray());
		}

		/// <inheritdoc cref="OpenFileStream"/>
		protected virtual Stream OnOpenFileStream(IGorgonVirtualFile file)
		{
			return FileData.OpenReadStream(file.FullPath);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// This value will return <b>true</b> when the <paramref name="physicalPath"/> is set to <c>::\\Memory</c> on the <c>physicalLocation</c> parameter for the <see cref="IGorgonFileSystem.Mount"/> method.
		/// </remarks>
		public bool CanReadFileSystem(string physicalPath)
		{
			return physicalPath.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Since this provider holds data in its own block of memory, there's nothing to enumerate when the provider is loaded. Thus, this will always return empty data.
		/// </remarks>
		public GorgonPhysicalFileSystemData Enumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			return OnEnumerate(physicalLocation, mountPoint);
		}

		/// <inheritdoc/>
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
