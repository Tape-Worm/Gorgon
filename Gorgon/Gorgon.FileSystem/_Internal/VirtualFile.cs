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
// Created: Monday, June 27, 2011 8:59:25 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonVirtualFile"/>
	class VirtualFile
		: IGorgonVirtualFile
	{
		#region Properties.
		/// <inheritdoc/>
		public IGorgonFileSystem FileSystem => Directory?.FileSystem;

		/// <inheritdoc/>
		public IGorgonPhysicalFileInfo PhysicalFile
		{
			get;
			set;
		}

		/// <inheritdoc/>
		public string FullPath
		{
			get
			{
			    if (Directory == null)
			    {
			        return Name;
			    }

			    return Directory.FullPath + Name;
			}
		}

		/// <inheritdoc/>
		public GorgonFileSystemMountPoint MountPoint
		{
			get;
			set;
		}

		/// <inheritdoc/>
		IGorgonVirtualDirectory IGorgonVirtualFile.Directory => Directory;

		/// <inheritdoc cref="IGorgonVirtualFile.Directory"/>
		public VirtualDirectory Directory
		{
			get;
		}

		/// <inheritdoc/>
		public string Extension
		{
			get;
			set;
		}

		/// <inheritdoc/>
		public string BaseFileName
		{
			get;
			set;
		}

		/// <inheritdoc/>
		public long Size => PhysicalFile.Length;

		/// <inheritdoc/>
		public DateTime CreateDate => PhysicalFile.CreateDate;

		/// <inheritdoc/>
		public DateTime LastModifiedDate => PhysicalFile.LastModifiedDate;

		/// <inheritdoc/>
		public string Name => PhysicalFile.Name;
		#endregion

		#region Methods.
        /// <inheritdoc/>
        public Stream OpenStream()
        {
	        return MountPoint.Provider.OpenFileStream(this);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualFile"/> class.
		/// </summary>
		/// <param name="mountPoint">The mount point that supplies this file.</param>
		/// <param name="fileInfo">Information about the physical file.</param>
		/// <param name="parent">The parent directory for this file..</param>
		public VirtualFile(GorgonFileSystemMountPoint mountPoint, IGorgonPhysicalFileInfo fileInfo, VirtualDirectory parent)
		{
			MountPoint = mountPoint;
			PhysicalFile = fileInfo;
			Directory = parent;
			Extension = Path.GetExtension(fileInfo.Name);
			BaseFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
			MountPoint = mountPoint;
		}
		#endregion
	}
}
