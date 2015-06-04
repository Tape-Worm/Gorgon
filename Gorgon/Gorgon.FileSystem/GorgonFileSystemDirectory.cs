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
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;

namespace Gorgon.IO
{
	/// <summary>
	/// A virtual directory in the <see cref="Gorgon.IO.GorgonFileSystem"/>.
	/// </summary>
	public sealed class GorgonFileSystemDirectory
		: GorgonNamedObject
	{
		#region Variables.
		// The list of sub directories.
		private readonly GorgonFileSystemDirectoryCollection _directories = new GorgonFileSystemDirectoryCollection();
		// The list of files in this directory.
		private readonly GorgonFileSystemFileEntryCollection _files;
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
		public IGorgonNamedObjectReadOnlyDictionary<GorgonFileSystemDirectory> Directories
		{
			get
			{
				return _directories;
			}
		}

		/// <summary>
		/// Property to return the list of files within this directory.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonFileSystemFileEntry> Files
		{
			get
			{
				return _files;
			}
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
				{
					return "/";
				}

				return Parent.FullPath + Name + "/";
			}
		}
		#endregion

        #region Methods.
		/// <summary>
		/// Function to return all the parents up to the root of this directory.
		/// </summary>
		/// <returns>A list of all the parents, up to and including the root. Or NULL (<i>Nothing</i> in VB.Net) if there is no parent directory.</returns>
		/// <remarks>A NULL (<i>Nothing</i> in VB.Net) return value indicates that this directory is the root directory.</remarks>
		public IEnumerable<GorgonFileSystemDirectory> GetParents()
		{
			if (FileSystem.RootDirectory == this)
			{
				return null;
			}

			var result = new List<GorgonFileSystemDirectory>
			             {
				             Parent
			             };

			IEnumerable<GorgonFileSystemDirectory> parents = Parent.GetParents();

			if (parents != null)
			{
				result.AddRange(parents);
			}

			return result;
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
        /// <param name="includeChildren">[Optional] <b>true</b> to include child directories, <b>false</b> to only use this directory.</param>
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
        /// <param name="searchChildren">[Optional] <b>true</b> to search through child directories, <b>false</b> to only search this directory.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
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
		    FileSystem = fileSystem;
			_files = new GorgonFileSystemFileEntryCollection(this);
			Parent = parent;			
		}
		#endregion
	}
}
