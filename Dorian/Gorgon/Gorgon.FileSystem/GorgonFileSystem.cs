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
// Created: Monday, June 27, 2011 8:54:59 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// The File System interface.
	/// </summary>
	/// <remarks>This will allow the user to mount folders or packed files (such as Zip files) into a unified file system.  For example, if the user has mount MyData.zip and C:\users\Bob\Data\ into the file system then
	/// all files and/or directories from both sources would be combined into a single virtual file system.
	/// <para>Accessing a file is handled like this:  GorgonFileSystem.ReadFile("/MyFile.txt");  It won't matter where MyFile.txt is stored on the physical file system, the system will know where to find it.</para>
	/// <para>Writing in the file system reroutes the data to a location under a physical file system directory.  This directory is specified by the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocaton</see> property.  
	/// For example, if the user sets the WriteLocation to C:\MyWriteDirectory, and proceeds to create a new file called "SomeText.txt" in the root of the virtual file system, then the file will be sent to 
	/// "C:\MyWriteDirectory\SomeText.txt".  Likewise, if a file, /SubDir1/SomeText.txt is in a sub directory on the virtual file system, the file will be rerouted to "C:\MyWriteDirectory\SubDir1\SomeText.txt".</para>
	/// <para>The order in which file systems are mounted into the virtual file system is important.  If a zip file contains SomeText.txt, and a directory to be mounted as root contains the same file and the
	/// directory is mounted first, followed by the zip, then the zip file version of the SomeText.txt file will take precedence and will be used.  The only exception to this rule is the WriteLocation directory
	/// which has the highest precedence over all files.</para>
	/// </remarks>
	public class GorgonFileSystem
	{
		#region Variables.
		private bool _mountListChanged = false;									// Flag to indicate that the mount point list was changed.
		private static readonly object _syncLock = new object();				// Synchronization object.
		private ReadOnlyCollection<GorgonFileSystemMountPoint> _mountPointList = null;			// The list of mount points to expose to the user.
		private IList<GorgonFileSystemMountPoint> _mountPoints = null;							// Mount points.
		private string _writeLocation = string.Empty;							// The area on the physical file system that we can write into.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default file system provider.
		/// </summary>
		internal Type DefaultProviderType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of current mount points.
		/// </summary>
		public IList<GorgonFileSystemMountPoint> MountPoints
		{
			get
			{
				if (_mountListChanged)
				{
					lock (_syncLock)
					{
						if (_mountListChanged)
						{
							_mountListChanged = false;
							_mountPointList = new ReadOnlyCollection<GorgonFileSystemMountPoint>(_mountPoints);							
						}
					}
				}

				return _mountPointList;
			}
		}

		/// <summary>
		/// Property to set or return the location on the physical file system that can be written to.
		/// </summary>
		/// <remarks>Files written to this location will be created using the directories of the virtual file system.  So, if the user sets "C:\MyWriteArea\" as the writeable location and then
		/// proceeds to create a directory:  "/LetsSayThisIsaZipFileDirectory/ZipDirectory/MyNewDirectory", the directory will be created in "C:\MyWriteArea\LetsSayThisIsaZipFileDirectory\ZipDirectory\".
		/// This also applies to files being written into a directory.  If the user creates a file: "/MountedFolder/MyFile.txt", then the file will be created as "C:\MyWriteArea\MountedFolder\MyFile.txt".
		/// <para>The write directory is automatically mounted last and will have precedence over all files in the file system (i.e. files retrieved will come from the write location first if they exist).</para>
		/// <para>If this value is set to NULL (Nothing in VB.Net) or an empty string, then no writeable area is assigned.  This will make the file system read-only until a valid write location is applied 
		/// to the property.  This is the default setting.</para>
		/// <para>The path for the write location must be an area that the user can write into, otherwise any write operations will fail.</para>
		/// </remarks>
		public string WriteLocation
		{
			get
			{
				return _writeLocation;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_writeLocation = value;

				if (!string.IsNullOrEmpty(_writeLocation))
				{
					_writeLocation = _writeLocation.FormatDirectory(Path.DirectorySeparatorChar);

					if (!Directory.Exists(_writeLocation))
						Directory.CreateDirectory(_writeLocation);
				}

				// Query the files/sub directories in the write location.
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the root directory for the file system.
		/// </summary>
		public GorgonFileSystemDirectory RootDirectory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of loaded providers.
		/// </summary>
		public GorgonFileSystemProviderCollection Providers
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to query the write location.
		/// </summary>
		private void QueryWriteLocation()
		{
			var folderProvider = Providers[DefaultProviderType.FullName];

			// If we've not included a write location, then leave.
			if ((string.IsNullOrEmpty(WriteLocation)) || (folderProvider == null))
			{
				return;
			}

			// Mount the writable location into the root.			
			folderProvider.Mount(WriteLocation, "/");
		}
		
		/// <summary>
		/// Function to recursively search for files.
		/// </summary>
		/// <param name="parent">Parent directory for the files.</param>
		/// <param name="entries">List of entries to populate.</param>
		/// <param name="recurse">TRUE to search all children, FALSE to use only the immediate directory.</param>
		/// <param name="fileName">Filename/mask to search for.</param>
		private void SearchDirectories(GorgonFileSystemDirectory parent, IList<GorgonFileSystemDirectory> entries, bool recurse, string fileName)
		{
			foreach (GorgonFileSystemDirectory entry in parent.Directories)
			{
				if (Regex.IsMatch(entry.Name, Regex.Escape(fileName).Replace(@"\*", ".*").Replace(@"\?", "."), RegexOptions.Singleline | RegexOptions.IgnoreCase))
					entries.Add(entry);

				if ((recurse) && (entry.Directories.Count > 0))
					SearchDirectories(entry, entries, true, fileName);
			}
		}

		/// <summary>
		/// Function to recursively search for files.
		/// </summary>
		/// <param name="parent">Parent directory for the files.</param>
		/// <param name="entries">List of entries to populate.</param>
		/// <param name="recurse">TRUE to search all children, FALSE to use only the immediate directory.</param>
		/// <param name="fileName">Filename/mask to search for.</param>
		private void SearchFiles(GorgonFileSystemDirectory parent, IList<GorgonFileSystemFileEntry> entries, bool recurse, string fileName)
		{
			if (recurse)
			{
				foreach (GorgonFileSystemDirectory directory in parent.Directories)
					SearchFiles(directory, entries, true, fileName);
			}

			foreach (GorgonFileSystemFileEntry entry in parent.Files)
			{
				if ((fileName.StartsWith("*")) && (fileName.Length > 1))
				{
					string endString = fileName.Substring(fileName.IndexOf('*') + 1);
					if (entry.Name.EndsWith(endString, StringComparison.CurrentCultureIgnoreCase))
						entries.Add(entry);
				}
				else
				{
					if (Regex.IsMatch(entry.Name, Regex.Escape(fileName).Replace(@"\*", ".*").Replace(@"\?", "."), RegexOptions.Singleline | RegexOptions.IgnoreCase))
						entries.Add(entry);
				}
			}
		}

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a mount point.
		/// </summary>
		/// <param name="mountPoint">Mount point to unmount.</param>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		/// <returns>TRUE if unmounted, FALSE if not.</returns>
		private bool UnmountMountPoint(GorgonFileSystemMountPoint mountPoint)
		{
			if (string.IsNullOrWhiteSpace(mountPoint.PhysicalPath))
			{
				return false;
			}

			string physicalPath = Path.GetFullPath(mountPoint.PhysicalPath);

			if (!_mountPoints.Contains(mountPoint))
			{
				throw new IOException("The mount point '" + mountPoint.MountLocation + "' with physical location '" + mountPoint.PhysicalPath + "' was not found.");
			}

			_mountPoints.Remove(mountPoint);
			_mountListChanged = true;

			return true;
		}
		
		/// <summary>
		/// Function to add a directory entry to the list of directories.
		/// </summary>
		/// <param name="path">Path to the directory entry.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The directory already exists.</para>
		/// <para>-or-</para><para>The driectory path is not valid.</para></exception>
		/// <returns>A new virtual directory entry.</returns>
		internal GorgonFileSystemDirectory AddDirectoryEntry(string path)
		{
			string parentPath = string.Empty;
			GorgonFileSystemDirectory directory = null;
			string[] directories = null;

			GorgonDebug.AssertParamString(path, "path");

			path = path.FormatDirectory('/');

			if (!path.StartsWith("/"))
				path = "/" + path;

			if (path == "/")
				throw new ArgumentException("The directory '/' already exists.", "directoryPath");

			directories = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (directories.Length == 0)
				throw new ArgumentException("The path '" + path + "' is not valid.", "directoryPath");

			directory = RootDirectory;
			foreach (string item in directories)
			{
				if (directory.Directories.Contains(item))
					directory = directory.Directories[item];
				else
				{
					GorgonFileSystemDirectory newDirectory = new GorgonFileSystemDirectory(item, directory);
					directory.Directories.Add(newDirectory);
					directory = newDirectory;
				}
			}

			return directory;
		}

		/// <summary>
		/// Function to add a file entry to the list of files.
		/// </summary>
		/// <param name="provider">Provider to use.</param>
		/// <param name="path">Path to the file entry.</param>
		/// <param name="mountPoint">The mount point that holds the file.</param>
		/// <param name="physicalLocation">The location of the file entry on the physical file system.</param>
		/// <param name="size">Size of the file in bytes.</param>
		/// <param name="offset">Offset of the file within a packed file.</param>
		/// <param name="createDate">Date the file was created.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The file already exists.</para>
		/// <para>-or-</para><para>The file name is missing from the path.</para>
		/// <para>-or-</para><para>The path was not found in the file system.</para>
		/// </exception>
		/// <returns>A new file system entry.</returns>
		internal GorgonFileSystemFileEntry AddFileEntry(GorgonFileSystemProvider provider, string path, string mountPoint, string physicalLocation, long size, long offset, DateTime createDate)
		{
			string fileName = string.Empty;						// Filename to create.
			string directoryName = string.Empty;				// Directory for the path.
			GorgonFileSystemDirectory directory = null;		// Directory to create the file in.
			GorgonFileSystemFileEntry result = null;			// The new file.

			GorgonDebug.AssertParamString(path, "path");
			GorgonDebug.AssertParamString(mountPoint, "mountPoint");
			GorgonDebug.AssertParamString(physicalLocation, "physicalLocation");

			directoryName = Path.GetDirectoryName(path).FormatDirectory('/');

			if (!directoryName.StartsWith("/"))
				directoryName += "/";

			fileName = Path.GetFileName(path).FormatFileName();
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentException("The path '" + path + "' does not contain a file name.", "path");

			directory = GetDirectory(directoryName);

			if (directory == null)
				throw new ArgumentException("The path '" + directoryName + "' could not be found.", "path");

			// If the file exists, then override it.
			result = new GorgonFileSystemFileEntry(provider, directory, fileName, mountPoint, physicalLocation, size, offset, createDate);
			directory.Files[fileName] = result;

			return result;
		}

		/// <summary>
		/// Function to return the writeable path including the virtual path passed in.
		/// </summary>
		/// <param name="path">Virtual path.</param>
		/// <returns>The physical writeable path.</returns>
		internal string GetWritePath(string path)
		{
			string fileName = string.Empty;
			string directory = string.Empty;

			if ((string.IsNullOrEmpty(path)) || (path == "/") || (string.IsNullOrEmpty(WriteLocation)))
				return WriteLocation;

			// The file name just gets tacked on, we don't use it for processing.
			fileName = Path.GetFileName(path).FormatFileName();

			directory = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

			if (directory.StartsWith(Path.DirectorySeparatorChar.ToString()))
				directory = directory.Substring(1);

			return (WriteLocation + directory).FormatDirectory(Path.DirectorySeparatorChar) + fileName;
		}

		/// <summary>
		/// Function to add a file system provider.
		/// </summary>
		/// <param name="providerTypeName">Fully qualified type name of the file system provider.</param>
		/// <remarks>Use this method to arbitrarily add custom file system provider add-ins to the file system.  If a file system provider is not added and an attempt to mount the target target file system, then the Gorgon file system will not know how to load the data within the external file system.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerTypeName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="providerTypeName"/> parameter is an empty string.</exception>
		public void AddProvider(string providerTypeName)
		{
            GorgonDebug.AssertParamString(providerTypeName, "providerTypeName");

			Providers.Add(providerTypeName);
		}

        /// <summary>
        /// Function to add a file system provider.
        /// </summary>
        /// <param name="providerType">The type of the file system provider.</param>
        /// <remarks>Use this method to arbitrarily add custom file system provider add-ins to the file system.  If a file system provider is not added and an attempt to mount the target target file system, then the Gorgon file system will not know how to load the data within the external file system.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerType"/> parameter is NULL (Nothing in VB.Net).</exception>
        public void AddProvider(Type providerType)
        {
            GorgonDebug.AssertNull<Type>(providerType, "providerType");
            AddProvider(providerType.FullName);
        }

		/// <summary>
		/// Function to search through the plug-in list and add any providers that haven't already been loaded.
		/// </summary>
		/// <remarks>This is a convenience method to allow mass loading of file system providers.</remarks>
		public void AddAllProviders()
		{
			var plugIns = from plugInList in Gorgon.PlugIns
							 where plugInList is GorgonFileSystemProviderPlugIn
							 select plugInList;

			foreach (var plugIn in plugIns)
				AddProvider(plugIn.Name);
		}
		
		/// <summary>
		/// Function to find all the directories specified in the directory mask.
		/// </summary>
		/// <param name="path">Path to the directory to start searching in.</param>
		/// <param name="directoryMask">The directory name or mask to search for.</param>
		/// <param name="recursive">TRUE to search all child directories, FALSE to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="GorgonLibrary.FileSystem.GorgonFileSystemDirectory">GorgonFileSystemDirectory</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="directoryMask"/> or the path parameter is a zero length string.</exception>
		/// <remarks>This method will accept file name masks like directory*, directory??1 and directory*a* when searching.
		/// <para>Please note that the <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Specifying something like /MyDir/ThisDir/C*w/ will fail.</para>
		/// </remarks>
		public IEnumerable<GorgonFileSystemDirectory> FindDirectories(string path, string directoryMask, bool recursive)
		{
			List<GorgonFileSystemDirectory> entries = null;			// List of file system entries.
			string directoryName = string.Empty;				// Directory for the path.
			GorgonFileSystemDirectory startDirectory = null;			// Starting directory.

			GorgonDebug.AssertParamString(path, "path");
			GorgonDebug.AssertParamString(directoryMask, "directoryMask");

			startDirectory = GetDirectory(path);
			if (startDirectory == null)
				throw new ArgumentException("The path '" + path + "' could not be found.", "path");

			entries = new List<GorgonFileSystemDirectory>();
			SearchDirectories(startDirectory, entries, recursive, directoryMask);

			return entries;
		}

		/// <summary>
		/// Function to find all the directories specified in the directory mask.
		/// </summary>
		/// <param name="directoryMask">The directory name or mask to search for.</param>
		/// <param name="recursive">TRUE to search all child directories, FALSE to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="GorgonLibrary.FileSystem.GorgonFileSystemDirectory">GorgonFileSystemDirectory</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="directoryMask"/> parameter is a zero length string.</exception>
		/// <remarks>This method will accept file name masks like directory*, directory??1 and directory*a* when searching.
		/// <para>Please note that the <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Specifying something like /MyDir/ThisDir/C*w/ will fail.</para>
		/// </remarks>
		public IEnumerable<GorgonFileSystemDirectory> FindDirectories(string directoryMask, bool recursive)
		{
			return FindDirectories("/", directoryMask, recursive);
		}

		/// <summary>
		/// Function to find all the files specified in the file mask.
		/// </summary>
		/// <param name="path">Path to start searching in.</param>
		/// <param name="fileMask">The file name or mask to search for.</param>
		/// <param name="recursive">TRUE to search all directories, FALSE to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="GorgonLibrary.FileSystem.GorgonFileSystemFileEntry">GorgonFileSystemFileEntry</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> is a zero length string.
		/// <para>-or-</para><para>Thrown when path specified in the <paramref name="path"/> parameter was not found.</para>
		/// </exception>
		/// <remarks>This method will accept file name masks like file*, file??1 and file*a* when searching.
		/// <para>Please note that the <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Specifying something like /MyDir/ThisDir/C*w.ext will fail.</para>
		/// </remarks>
		public IEnumerable<GorgonFileSystemFileEntry> FindFiles(string path, string fileMask, bool recursive)
		{
			List<GorgonFileSystemFileEntry> entries = null;		// List of file system entries.
			GorgonFileSystemDirectory start = null;				// Directory to start searching in.

			GorgonDebug.AssertParamString(path, "path");
			GorgonDebug.AssertParamString(fileMask, "fileMask");

			start = GetDirectory(path);

			if (start == null)
				throw new ArgumentException("The path '" + path + "' was not found.");

			entries = new List<GorgonFileSystemFileEntry>();
			SearchFiles(start, entries, recursive, fileMask);

			return entries;
		}

		/// <summary>
		/// Function to find all the files specified in the file mask.
		/// </summary>
		/// <param name="fileMask">The file name or mask to search for.</param>
		/// <param name="recursive">TRUE to search all directories, FALSE to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="GorgonLibrary.FileSystem.GorgonFileSystemFileEntry">GorgonFileSystemFileEntry</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileMask"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="fileMask"/> is a zero length string.</exception>
		/// <remarks>This method will accept file name masks like file*, file??1 and file*a* when searching.
		/// <para>Please note that the <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Specifying something like /MyDir/ThisDir/C*w.ext will fail.</para>
		/// </remarks>
		public IEnumerable<GorgonFileSystemFileEntry> FindFiles(string fileMask, bool recursive)
		{
			return FindFiles("/", fileMask, recursive);
		}

		/// <summary>
		/// Function to retrieve a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The file requested or NULL (Nothing in VB.Net) if the file was not found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when there is no file name in the path.</para>
		/// </exception>
		public GorgonFileSystemFileEntry GetFile(string path)
		{
			string directory = string.Empty;
			string filename = string.Empty;
			GorgonFileSystemDirectory search = RootDirectory;

			GorgonDebug.AssertParamString(path, "path");

			if (!path.StartsWith("/"))
				path = "/" + path;

			directory = Path.GetDirectoryName(path).FormatDirectory('/');
			filename = Path.GetFileName(path).FormatFileName();

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentException("There was no file name in the path '" + path + "'", "path");

			search = GetDirectory(directory);

			if (search == null)
				return null;

			if (search.Files.Contains(filename))
				return search.Files[filename];

			return null;
		}

		/// <summary>
		/// Function to retrieve a directory from the provider.
		/// </summary>
		/// <param name="path">Path to the directory to retrieve.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <returns>The file system directory if found, NULL (Nothing in VB.Net) if not.</returns>
		public GorgonFileSystemDirectory GetDirectory(string path)
		{
			string[] directories = null;
			GorgonFileSystemDirectory directory = null;

			GorgonDebug.AssertParamString(path, "path");

			path = path.FormatDirectory('/');

			if (!path.StartsWith("/"))
				path = "/" + path;

			if (path == "/")
				return RootDirectory;

			directories = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (directories.Length == 0)
				return null;

			directory = RootDirectory;

			for (int i = 0; i < directories.Length; i++)
			{
				if (directory.Directories.Contains(directories[i]))
					directory = directory.Directories[directories[i]];
				else
					return null;
			}

			return directory;
		}

		/// <summary>
		/// Function to read a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file to read.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in <paramref name="path"/> was not found.</exception>
		/// <returns>An array of bytes containing the data in the file.</returns>
		public byte[] ReadFile(string path)
		{
			GorgonFileSystemFileEntry file = null;

			file = GetFile(path);

			if (file == null)
				throw new FileNotFoundException("Could not find the file '" + path + "'.", path);

			return file.Provider.ReadFile(file);
		}

		/// <summary>
		/// Function to read a write a file to the file system.
		/// </summary>
		/// <param name="path">Path to the file to write.</param>
		/// <param name="data">Data to write.</param>
        /// <returns>The file system file entry that was updated or created.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>The file system provider that holds the file is read-only.</para></exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in <paramref name="path"/> was not found.</exception>
        /// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter will only add a file entry (if it does not already exist) to the virtual directory, but will not create 
        /// it on the actual physical file system.  To create a 0 byte file, pass an empty array into the data parameter.</remarks>
		public GorgonFileSystemFileEntry WriteFile(string path, byte[] data)
		{
			GorgonFileSystemFileEntry file = null;

			if (string.IsNullOrEmpty(WriteLocation))
				throw new InvalidOperationException("Cannot write the file without a writeable location set.");

			file = GetFile(path);

			if (file == null)
				file = AddFileEntry(Providers[DefaultProviderType.FullName], path, Path.GetDirectoryName(GetWritePath(path)) + Path.DirectorySeparatorChar.ToString(), GetWritePath(path), 0, 0, DateTime.Now);

            // If we're not passing any data, then do nothing.
            if (data != null)
            {
                file.Provider.WriteFile(file, data);
            }

            return file;
		}

		/// <summary>
		/// Function to open a file stream for reading/writing.
		/// </summary>
		/// <param name="path">Path to the file to read or write.</param>
		/// <param name="writeable">TRUE to write to this file, FALSE to open read-only.</param>
		/// <remarks>Some file system providers cannot write, and will throw an exception if this is the case.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>The file system provider that holds the file is read-only.</para>
		/// <para>-or-</para><para>Thrown when the <paramref name="writeable"/> parameter is TRUE and the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</para>
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in <paramref name="path"/> was not found and <paramref name="writeable"/> is FALSE.</exception>
		/// <returns>The open <see cref="GorgonLibrary.FileSystem.GorgonFileSystemStream"/> file stream object.</returns>
		public GorgonFileSystemStream OpenStream(string path, bool writeable)
		{
			GorgonFileSystemFileEntry file = null;

			if ((writeable) && (string.IsNullOrEmpty(WriteLocation)))
				throw new ArgumentException("Cannot write the file without a writeable location set.", "writeable");

			file = GetFile(path);

			if (file == null)
			{
				if (!writeable)
					throw new FileNotFoundException("Could not find the file '" + path + "'.", path);
				else
					file = AddFileEntry(Providers[DefaultProviderType.FullName], path, Path.GetDirectoryName(GetWritePath(path)) + Path.DirectorySeparatorChar.ToString(), GetWritePath(path), 0, 0, DateTime.Now);
			}
			
			return file.Provider.OpenStream(file, writeable);
		}

		/// <summary>
		/// Function to create a directory.
		/// </summary>
		/// <param name="path">Path to the new directory.</param>
		/// <returns>The new directory.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <remarks>This method will create a directory in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public GorgonFileSystemDirectory CreateDirectory(string path)
		{
			GorgonFileSystemDirectory directory = null;
			string newPath = path;

			GorgonDebug.AssertParamString(path, "path");

			directory = GetDirectory(path);

			if (directory == null)
				directory = AddDirectoryEntry(path);

			if (!string.IsNullOrEmpty(WriteLocation))
			{
				newPath = GetWritePath(directory.FullPath);

				if (!Directory.Exists(newPath))
					Directory.CreateDirectory(newPath);
			}

			return directory;
		}

		/// <summary>
		/// Function to delete a file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the file was not found.</para>
		/// </exception>
		/// <remarks>This method will remove a file in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public void DeleteFile(string path)
		{
			GorgonFileSystemFileEntry file = null;
			string newPath = path;

			GorgonDebug.AssertParamString(path, "path");

			file = GetFile(path);

			// Remove from the parent directory.
			file.Directory.Files.Remove(file);

			if (!string.IsNullOrEmpty(WriteLocation))
			{
				newPath = GetWritePath(path);
				if (File.Exists(newPath))
					File.Delete(newPath);
			}
		}

		/// <summary>
		/// Function to delete a directory.
		/// </summary>
		/// <param name="path">Path to the directory.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the directory was not found.</para>
		/// </exception>
		/// <remarks>This method will remove a directory in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public void DeleteDirectory(string path)
		{
			GorgonFileSystemDirectory directory = null;
			string newPath = path;

			GorgonDebug.AssertParamString(path, "path");

			directory = GetDirectory(path);

			if (directory == null)
				throw new DirectoryNotFoundException("The directory in '" + path + "' was not found.");

			// If we specify the root, then remove everything.
			if (directory.Name == "/")
			{
				directory.Directories.Clear();
				directory.Files.Clear();

				if (!string.IsNullOrEmpty(WriteLocation))
					newPath = WriteLocation;
			}
			else
			{
				if (!string.IsNullOrEmpty(WriteLocation))
					newPath = GetWritePath(directory.FullPath);
				
				directory.Parent.Directories.Remove(directory);
			}

			if (!string.IsNullOrEmpty(newPath))
			{
				if (Directory.Exists(newPath))
				{
					// Remove from the write area if the directory exists.
					if (directory.Name != "/")
					{
						DirectoryInfo dirInfo = new DirectoryInfo(newPath);
						dirInfo.Delete(true);
					}
					else
					{
						var directories = Directory.GetDirectories(newPath, "*", SearchOption.TopDirectoryOnly);
						var files = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

						foreach (var directoryPath in directories)
						{
							DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
							dirInfo.Delete(true);
						}

						foreach (var file in files)
						{
							FileInfo fileInfo = new FileInfo(file);
							fileInfo.Delete();
						}
					}
				}
			}
		}

		/// <summary>
		/// Function to clear all the mounted directories and files.
		/// </summary>
		public void Clear()
		{
			_mountListChanged = true;
			_mountPoints.Clear();
			RootDirectory.Directories.Clear();
			RootDirectory.Files.Clear();
		}

		/// <summary>
		/// Function to refresh all the files and directories in the file system.
		/// </summary>
		public void Refresh()
		{
			string writeLocation = this.WriteLocation;

			try
			{
				RootDirectory.Directories.Clear();
				RootDirectory.Files.Clear();

				_writeLocation = string.Empty;

				// This isn't the most efficient way, but it works.
				// Since the directories don't track which physical location they're in
				// and files only track the last mounted physical location, we have no way
				// to selectively exclude files.  The best approach at this point is to 
				// just clear it all out, and start over.
				foreach (GorgonFileSystemMountPoint mountPoint in _mountPoints)
				{
					Mount(mountPoint.PhysicalPath, mountPoint.MountLocation);
				}
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(writeLocation))
				{
					_writeLocation = writeLocation;
					QueryWriteLocation();
				}
			}
		}

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a mount point.
		/// </summary>
		/// <param name="mountPoint">The mount point to unmount.</param>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		public void Unmount(GorgonFileSystemMountPoint mountPoint)
		{
			if (UnmountMountPoint(mountPoint))
			{
				Refresh();
			}
		}

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at the by the physical path specified and mounted into the mount location specified.
		/// </summary>
		/// <param name="physicalPath">Physical file system path.</param>
		/// <param name="mountLocation">Virtual sub directory that the physical location is mounted under.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.</exception>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		public void Unmount(string physicalPath, string mountLocation)
		{
			GorgonDebug.AssertParamString(physicalPath, "physicalPath");
			GorgonDebug.AssertParamString(mountLocation, "mountLocation");

			Unmount(new GorgonFileSystemMountPoint(physicalPath, mountLocation));
		}

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a physical path.
		/// </summary>
		/// <param name="physicalPath">The physical path to unmount.</param>
		/// <remarks>This overload will unmount all the mounted virtual files/directories for every mount point with the specified <paramref name="physicalPath"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
        /// <exception cref="System.IO.IOException">The path was not found.</exception>
        public void Unmount(string physicalPath)
		{
			var mountPoints = _mountPoints.Where(item => string.Compare(Path.GetFullPath(physicalPath), Path.GetFullPath(item.PhysicalPath), true) == 0);

			foreach (var mountPoint in mountPoints)
			{
				UnmountMountPoint(mountPoint);				
			}

			Refresh();
		}		

		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="source">Path to the directory or file that contains the files/directories to enumerate.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="source"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the path in the <paramref name="source"/> parameter is not valid.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		public void Mount(string source)
		{
			Mount(source, "/");
		}

		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="mountPoint">The mount point containing the physical path and the virtual file system location.</param>
		/// <exception cref="System.ArgumentException">Thrown when the physical path in the <paramref name="mountPoint"/> parameter is not valid.
		/// <para>-or-</para><para>Thrown when the file in the mountPoint parameter cannot be read by any of the file system providers.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		public void Mount(GorgonFileSystemMountPoint mountPoint)
		{
			Mount(mountPoint.PhysicalPath, mountPoint.MountLocation);
		}

		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="physicalPath">Path to the directory or file that contains the files/directories to enumerate.</param>
		/// <param name="mountPath">Folder path to mount into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountPath"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the path in the <paramref name="physicalPath"/> parameter is not valid.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		public void Mount(string physicalPath, string mountPath)
		{
			string fileName = string.Empty;
			string directory = string.Empty;
			GorgonFileSystemProvider provider = null;

			GorgonDebug.AssertParamString(physicalPath, "physicalPath");
			GorgonDebug.AssertParamString(mountPath, "mountPath");

			// Don't mount the write location.  It will be automatically queried every time we mount a physical location.
			if ((!string.IsNullOrEmpty(WriteLocation)) && (string.Compare(physicalPath, WriteLocation, true) == 0))
			{
				return;
			}

			physicalPath = Path.GetFullPath(physicalPath);
			fileName = Path.GetFileName(physicalPath);
			directory = Path.GetDirectoryName(physicalPath);

			// If we have no file name, assume we're mounting a folder from the OS filesystem.
			if (string.IsNullOrEmpty(fileName))
			{
				if (string.IsNullOrEmpty(directory))
					throw new ArgumentException("The path in '" + physicalPath + "' is not a valid path.", "path");

				directory = directory.FormatDirectory(Path.DirectorySeparatorChar);

				// Use the default folder provider.
				provider = Providers[DefaultProviderType.FullName];
				if (!Directory.Exists(directory))
					throw new ArgumentException("The path '" + directory + "' does not exist.", "physicalPath");
				
				provider.Mount(physicalPath, mountPath);

				GorgonFileSystemMountPoint mountPoint = new GorgonFileSystemMountPoint(physicalPath, mountPath);
				if (!_mountPoints.Contains(mountPoint))
				{
					_mountPoints.Add(mountPoint);
					_mountListChanged = true;
				}
				return;
			}

			fileName = fileName.FormatFileName();
			if (!string.IsNullOrEmpty(directory))
				directory = directory.FormatDirectory(Path.DirectorySeparatorChar);

			if (!File.Exists(directory + fileName))
				throw new ArgumentException("The path '" + directory + fileName + "' does not exist.", "physicalPath");

			// Find the file system provider that can read this file type.
			foreach (GorgonFileSystemProvider fileSystemProvider in Providers)
			{
				if (fileSystemProvider.CanReadFile(directory + fileName))
				{					
					fileSystemProvider.Mount(physicalPath, mountPath);

					GorgonFileSystemMountPoint mountPoint = new GorgonFileSystemMountPoint(physicalPath, mountPath);
					if (!_mountPoints.Contains(mountPoint))
					{
						_mountPoints.Add(mountPoint);
						_mountListChanged = true;
					}
					return;
				}
			}

			// Requery the write location if it exists.
			QueryWriteLocation();

			throw new IOException("The file '" + directory + fileName + "' could not be read by any of the loaded file system providers.");
		}

		/// <summary>
		/// Function to remove a file system provider from the Gorgon file system.
		/// </summary>
		/// <param name="providerTypeName">Fully qualified type name of the file system provider to unload.</param>
		/// <remarks>This will remove all the file entries that belong to the provider as well.  To remove an individual file, use the <see cref="M:GorgonLibrary.FileSystem.GorgonFileSystem.DeleteFile">DeleteFile</see> method.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerTypeName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="providerTypeName"/> parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the provider specified by <paramref name="providerTypeName"/> parameter could not be found.</exception>
		public void RemoveProvider(string providerTypeName)
		{
			Providers.Remove(providerTypeName);
		}

		/// <summary>
		/// Function to unmount all the file system providers and their related files.
		/// </summary>
		/// <remarks>This will remove all providers and files and reset to the default folder file system provider.</remarks>
		public void RemoveAll()
		{
			Providers.Clear();
			Providers = new GorgonFileSystemProviderCollection(this);
			Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonFileSystem"/> class.
		/// </summary>
		public GorgonFileSystem()
		{
			DefaultProviderType = typeof(GorgonFolderFileSystemProvider);
			_mountPoints = new List<GorgonFileSystemMountPoint>();
			_mountPointList = new ReadOnlyCollection<GorgonFileSystemMountPoint>(_mountPoints);

			Providers = new GorgonFileSystemProviderCollection(this);
			RootDirectory = new GorgonFileSystemDirectory("/", null);
			Clear();
			WriteLocation = string.Empty;
		}
		#endregion
	}
}
