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
// Created: Monday, June 27, 2011 8:58:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A virtual directory in the <see cref="GorgonLibrary.IO.GorgonFileSystem"/>.
	/// </summary>
	public sealed class GorgonFileSystemDirectory
		: GorgonNamedObject
    {
        #region Classes.
        /// <summary>
        /// An equality comparer for file mount points and providers.
        /// </summary>
	    private class FileMountProviderComparer
            : IEqualityComparer<Tuple<string, GorgonFileSystemProvider>>
        {
            #region IEqualityComparer<Tuple<string,GorgonFileSystemProvider>> Members
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
            /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(Tuple<string, GorgonFileSystemProvider> x, Tuple<string, GorgonFileSystemProvider> y)
            {
                return ((x.Item2 == y.Item2) && (string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase)));
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(Tuple<string, GorgonFileSystemProvider> obj)
            {
                return 281.GenerateHash(obj.Item1).GenerateHash(obj.Item2);
            }
            #endregion
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file system that owns this directory.
        /// </summary>
        public GorgonFileSystem FileSystem
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the list of child directories for this directory.
		/// </summary>
		public GorgonFileSystemDirectoryCollection Directories
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of files within this directory.
		/// </summary>
		public GorgonFileSystemFileEntryCollection Files
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the parent directory for this directory.
		/// </summary>
		public GorgonFileSystemDirectory Parent
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the full path to the directory.
		/// </summary>
		public string FullPath
		{
			get
			{
				if (Parent == null)
					return "/";

				return Parent.FullPath + Name + "/";
			}
		}
		#endregion

        #region Methods.
        /// <summary>
        /// Function to refresh the file mount points for a directory and any child directories.
        /// </summary>
        /// <param name="includeChildren">[Optional] TRUE to include child directories, FALSE to just refresh this directory.</param>
	    public void RefreshFiles(bool includeChildren = true)
	    {
            if (includeChildren)
            {
                foreach (var directory in Directories)
                {
                    directory.RefreshFiles();
                }
            }

			string[] physicalDirectories;
			GorgonFileSystemProvider.PhysicalFileInfo[] physicalFiles;
            var fileMountPoints = Files.Select(item => new Tuple<string, GorgonFileSystemProvider>(item.MountPoint, item.Provider))
                                   .Distinct(new FileMountProviderComparer());

            foreach(var fileMount in fileMountPoints)
            {
                string fileMountDirectory = Path.GetDirectoryName(fileMount.Item1);

                Debug.Assert(!string.IsNullOrWhiteSpace(fileMountDirectory), "Mount directory is NULL!!");

                fileMountDirectory = fileMountDirectory.FormatDirectory(Path.DirectorySeparatorChar);

                // Find the mount point.
                var mountPoints = FileSystem.MountPoints.Where(item => string.Equals(item.PhysicalPath,
                                                                               fileMountDirectory,
                                                                               StringComparison.OrdinalIgnoreCase));

                foreach (GorgonFileSystemDirectory mountDirectory in mountPoints.Select(mountPoint => 
                                                                                        FileSystem
                                                                                            .GetDirectory(mountPoint.MountLocation))
                                                                                            .Where(mountDirectory => mountDirectory != null))
                {
                    fileMount.Item2.Enumerate(fileMountDirectory,
                                              mountDirectory,
                                              out physicalDirectories,
                                              out physicalFiles);

#error Refresh the files and directories for this guy.
                }
            }
	    }

        /// <summary>
        /// Function to delete the directory.
        /// </summary>
        public void Delete()
        {
            FileSystem.DeleteDirectory(this);
        }

        /// <summary>
        /// Funciton to retrieve the total number of directoryes in this directory including any directories under this one.
        /// </summary>
        /// <returns>The total number of directories.</returns>
        /// <remarks>The difference between the count property on the <see cref="Directories"/> collection and this method is that this method will enumerate child directories.  The count 
        /// property will only count directories in the immediate directory.</remarks>
        public int GetDirectoryCount()
        {
            int count = Directories.Count;

            if (count > 0)
            {
                count += Directories.Sum(item => item.GetDirectoryCount());
            }

            return count;
        }

        /// <summary>
        /// Funciton to retrieve the total number of files in this directory and optionally, any directories under this one.
        /// </summary>
        /// <param name="includeChildren">[Optional] TRUE to include child directories, FALSE to only use this directory.</param>
        /// <returns>The total number of files.</returns>
	    public int GetFileCount(bool includeChildren = true)
        {
            int count = Files.Count;

            if ((includeChildren) && (Directories.Count > 0))
            {
                count += Directories.Sum(item => item.GetFileCount());
            }

            return count;
        }

        /// <summary>
        /// Function to determine if the directory contains the specified file.
        /// </summary>
        /// <param name="file">File to look for.</param>
        /// <param name="searchChildren">[Optional] TRUE to search through child directories, FALSE to only search this directory.</param>
        /// <returns>TRUE if found, FALSE if not.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (Nothing in VB.Net).</exception>
	    public bool Contains(GorgonFileSystemFileEntry file, bool searchChildren = true)
	    {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            return Files.Any(item => item == file)
                   || ((searchChildren) && (Directories.Any(directory => directory.Contains(file))));
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemDirectory"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system that owns this directory.</param>
		/// <param name="name">The name of the directory.</param>
		/// <param name="parent">Parent directory.</param>
		internal GorgonFileSystemDirectory(GorgonFileSystem fileSystem, string name, GorgonFileSystemDirectory parent)
			: base(name.RemoveIllegalPathChars())
		{
			Directories = new GorgonFileSystemDirectoryCollection();
		    FileSystem = fileSystem;
			Files = new GorgonFileSystemFileEntryCollection(this);
			Parent = parent;			
		}
		#endregion
	}
}
