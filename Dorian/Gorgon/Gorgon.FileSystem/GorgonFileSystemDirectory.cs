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

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A virtual directory in the <see cref="GorgonLibrary.IO.GorgonFileSystem"/>.
	/// </summary>
	public sealed class GorgonFileSystemDirectory
		: GorgonNamedObject
	{
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
        /// Function to delete the directory.
        /// </summary>
        public void Delete()
        {
            FileSystem.DeleteDirectory(this);
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
