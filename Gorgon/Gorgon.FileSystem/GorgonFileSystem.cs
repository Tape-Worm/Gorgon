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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// The virtual file System interface.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will allow the user to mount folders or packed files (such as Zip files) into a unified file system.  For example, if the user has mount MyData.zip and C:\users\Bob\Data\ into the file system then
	/// all files and/or directories from both sources would be combined into a single virtual file system.
	/// </para>
	/// <para>
	/// Accessing a file is handled like this:  GorgonFileSystem.ReadFile("/MyFile.txt");  It won't matter where MyFile.txt is stored on the physical file system, the system will know where to find it.
	/// </para>
	/// <para>
	/// Writing in the file system reroutes the data to a location under a physical file system directory.  This directory is specified by the <see cref="WriteLocation">WriteLocaton</see> property.  
	/// For example, if the user sets the WriteLocation to C:\MyWriteDirectory, and proceeds to create a new file called "SomeText.txt" in the root of the virtual file system, then the file will be sent to 
	/// "C:\MyWriteDirectory\SomeText.txt".  Likewise, if a file, /SubDir1/SomeText.txt is in a sub directory on the virtual file system, the file will be rerouted to "C:\MyWriteDirectory\SubDir1\SomeText.txt".
	/// </para>
	/// <para>
	/// The order in which file systems are mounted into the virtual file system is important.  If a zip file contains SomeText.txt, and a directory to be mounted as root contains the same file and the
	/// directory is mounted first, followed by the zip, then the zip file version of the SomeText.txt file will take precedence and will be used.  The only exception to this rule is the WriteLocation directory
	/// which has the highest precedence over all files.
	/// </para>
    /// <para>
    /// By default, a new file system instance will only have access to the folders and files of the hard drive via a folder file system.  File systems that are in packed files (e.g. WinZip files) can be loaded into the 
    /// file system by way of a <see cref="GorgonFileSystemProvider"/>.  Providers are plug-in objects that are loaded into the file system via the <see cref="GorgonFileSystemProviderFactory"/>.  Once a provider plug-in 
    /// is loaded, then the contents of that file system can be mounted like a standard directory.  For example, if the zip file provider plug-in is loaded, then the file system may be mounted into the root by: 
    /// <c>fileSystem.Mount("d:\zipFiles\myZipFile.zip", "/");</c>.
    /// </para>
	/// </remarks>
	public class GorgonFileSystem
	{
		#region Variables.
		/// <summary>
		/// Directory separator character.
		/// </summary>
	    internal static readonly string PhysicalDirSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

		// Flag to indicate that the mount point list was changed.
		private bool _mountListChanged;
		// Synchronization object.
		private static readonly object _syncLock = new object();
		// The list of mount points to expose to the user.
		private ReadOnlyCollection<GorgonFileSystemMountPoint> _mountPointList;
		// Mount points.
		private readonly List<GorgonFileSystemMountPoint> _mountPoints;
		// The area on the physical file system that we can write into.
		private string _writeLocation = string.Empty;
		// The default file system provider.
		private readonly GorgonFileSystemProvider _defaultProvider = new GorgonFileSystemProvider();
		// Interlock increment variable.
	    private int _refreshSemaphore;
		// The list of providers available to the file system.
		private readonly Dictionary<string, GorgonFileSystemProvider> _providers;
		// The log file for the application.
		private IGorgonLog _log = new GorgonLogDummy();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of current mount points.
		/// </summary>
		public IList<GorgonFileSystemMountPoint> MountPoints
		{
			get
			{
				if (!_mountListChanged)
				{
					return _mountPointList;
				}

				lock (_syncLock)
				{
					_mountListChanged = false;
					_mountPointList = new ReadOnlyCollection<GorgonFileSystemMountPoint>(_mountPoints);
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
		/// <para>If this value is set to NULL (<i>Nothing</i> in VB.Net) or an empty string, then no writeable area is assigned.  This will make the file system read-only until a valid write location is applied 
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
				{
					value = string.Empty;
				}

				_writeLocation = value;

				if (!string.IsNullOrWhiteSpace(_writeLocation))
				{
					_writeLocation = _writeLocation.FormatDirectory(Path.DirectorySeparatorChar);

					var info = new DirectoryInfo(_writeLocation);
					if (!info.Exists)
					{
						info.Create();
					}
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
		}

		/// <summary>
		/// Property to return the list of providers available to this file system.
		/// </summary>
		public IReadOnlyDictionary<string, GorgonFileSystemProvider> Providers => _providers;

		#endregion

		#region Methods.
        /// <summary>
        /// Function to retrieve the file system objects from the physical file system.
        /// </summary>
        /// <param name="provider">The provider to the physical file system objects.</param>
        /// <param name="physicalPath">Physical path that is being mounted.</param>
        /// <param name="mountPath">Mounting point in the virtual file system.</param>
        private void GetFileSystemObjects(GorgonFileSystemProvider provider, string physicalPath, string mountPath)
        {
            string[] physicalDirectories;
            GorgonFileSystemProvider.PhysicalFileInfo[] physicalFiles;

            physicalPath = Path.GetFullPath(physicalPath);

            string fileName = Path.GetFileName(physicalPath);

            physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory(Path.DirectorySeparatorChar);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                physicalPath += fileName.FormatFileName();
            }

            // Find existing mount point.
            GorgonFileSystemDirectory mountDirectory = GetDirectory(mountPath) ?? AddDirectoryEntry(mountPath);

            _log.Print("Mounting physical file system path '{0}' to virtual file system path '{1}'.", LoggingLevel.Simple, physicalPath, mountPath);

            provider.Enumerate(physicalPath, mountDirectory, out physicalDirectories, out physicalFiles);

            // Process the directories.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < physicalDirectories.Length; i++)
            {
                string directory = physicalDirectories[i];

                if (GetDirectory(directory) == null)
                {
                    AddDirectoryEntry(directory);
                }
            }

            foreach (var file in physicalFiles)
            {
                AddFileEntry(provider, file.VirtualPath, physicalPath, file.FullPath, file.Length, file.Offset, file.CreateDate);
            }

            _log.Print("{0} directories parsed, and {1} files processed.", LoggingLevel.Simple, physicalDirectories.Length, physicalFiles.Length);
        }

        /// <summary>
		/// Function to query the write location.
		/// </summary>
		private void QueryWriteLocation()
		{
			// If we've not included a write location, then leave.
			if (string.IsNullOrWhiteSpace(WriteLocation))
			{
				return;
			}

			// Mount the writable location into the root.			
			GetFileSystemObjects(_defaultProvider, WriteLocation, "/");
		}
		
		/// <summary>
		/// Function to recursively search for files.
		/// </summary>
		/// <param name="parent">Parent directory for the files.</param>
		/// <param name="entries">List of entries to populate.</param>
		/// <param name="recurse"><b>true</b> to search all children, <b>false</b> to use only the immediate directory.</param>
		/// <param name="fileName">Filename/mask to search for.</param>
		private static void SearchDirectories(GorgonFileSystemDirectory parent, ICollection<GorgonFileSystemDirectory> entries, bool recurse, string fileName)
		{
			foreach (GorgonFileSystemDirectory entry in parent.Directories)
			{
				if (Regex.IsMatch(entry.Name, Regex.Escape(fileName).Replace(@"\*", ".*").Replace(@"\?", "."),
				                  RegexOptions.Singleline | RegexOptions.IgnoreCase))
				{
					entries.Add(entry);
				}

				if ((recurse) && (entry.Directories.Count > 0))
				{
					SearchDirectories(entry, entries, true, fileName);
				}
			}
		}

		/// <summary>
		/// Function to recursively search for files.
		/// </summary>
		/// <param name="parent">Parent directory for the files.</param>
		/// <param name="entries">List of entries to populate.</param>
		/// <param name="recurse"><b>true</b> to search all children, <b>false</b> to use only the immediate directory.</param>
		/// <param name="fileName">Filename/mask to search for.</param>
		private static void SearchFiles(GorgonFileSystemDirectory parent, ICollection<GorgonFileSystemFileEntry> entries, bool recurse, string fileName)
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
					if (entry.Name.EndsWith(endString, StringComparison.OrdinalIgnoreCase))
					{
						entries.Add(entry);
					}
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
		/// <returns><b>true</b> if unmounted, <b>false</b> if not.</returns>
		private bool UnmountMountPoint(GorgonFileSystemMountPoint mountPoint)
		{
			if (string.IsNullOrWhiteSpace(mountPoint.PhysicalPath))
			{
				return false;
			}

			if (!_mountPoints.Contains(mountPoint))
			{
			    throw new IOException(string.Format(Resources.GORGFS_MOUNTPOINT_NOT_FOUND,
			        mountPoint.MountLocation,
			        mountPoint.PhysicalPath));
			}

			_mountPoints.Remove(mountPoint);
			_mountListChanged = true;

			return true;
		}

        /// <summary>
        /// Function to return the writeable path including the virtual path passed in.
        /// </summary>
        /// <param name="path">Virtual path.</param>
        /// <returns>The physical writeable path.</returns>
        private string GetWritePath(string path)
        {
            var writePath = new StringBuilder(1024);

            if (string.IsNullOrWhiteSpace(WriteLocation))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_NO_WRITE_LOCATION, path));
            }

            if (path == "/")
            {
                return WriteLocation;
            }

            // The file name just gets tacked on, we don't use it for processing.
            string fileName = Path.GetFileName(path).FormatFileName();

            writePath.Append(Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar));

            if ((writePath.Length > 0) && (writePath[0] == Path.DirectorySeparatorChar))
            {
                writePath.Remove(0, 1);
            }

            // Prepend the write location.
            writePath.Insert(0, WriteLocation);

            string result = writePath.ToString();

            // Create the directory if necessary.
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

	        if (string.IsNullOrWhiteSpace(fileName))
	        {
		        return result;
	        }

	        writePath.Append(fileName);
	        result = writePath.ToString();

	        return result;
        }

		/// <summary>
		/// Function to add a directory entry to the list of directories.
		/// </summary>
		/// <param name="path">Path to the directory entry.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The directory already exists.</para>
		/// <para>-or-</para><para>The directory path is not valid.</para></exception>
		/// <returns>A new virtual directory entry.</returns>
		private GorgonFileSystemDirectory AddDirectoryEntry(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
			}

			path = path.FormatDirectory('/');

			if (!path.StartsWith("/"))
			{
				path = "/" + path;
			}

			if (path == "/")
			{
				throw new ArgumentException(string.Format(Resources.GORFS_DIRECTORY_EXISTS, "/"), nameof(path));
			}

			string[] directories = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (directories.Length == 0)
			{
				throw new ArgumentException(string.Format(Resources.GORFS_PATH_INVALID, path), nameof(path));
			}

			GorgonFileSystemDirectory directory = RootDirectory;
			foreach (string item in directories)
			{
                if (directory.Files.Contains(item))
                {
					throw new ArgumentException(string.Format(Resources.GORFS_FILE_EXISTS, item), nameof(path));
                }

				if (directory.Directories.Contains(item))
				{
					directory = directory.Directories[item];
				}
				else
				{
					var newDirectory = new GorgonFileSystemDirectory(this, item, directory);
					((IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>)directory.Directories).Add(newDirectory);
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The file already exists.</para>
		/// <para>-or-</para><para>The file name is missing from the path.</para>
		/// <para>-or-</para><para>The path was not found in the file system.</para>
		/// </exception>
		/// <returns>A new file system entry.</returns>
		private GorgonFileSystemFileEntry AddFileEntry(GorgonFileSystemProvider provider, string path, string mountPoint, string physicalLocation, long size, long offset, DateTime createDate)
		{
		    string directoryName = Path.GetDirectoryName(path).FormatDirectory('/');
            string fileName = Path.GetFileName(path).FormatFileName();

		    if (!directoryName.StartsWith("/"))
		    {
		        directoryName += "/";
		    }

		    if (string.IsNullOrWhiteSpace(fileName))
		    {
                throw new ArgumentException(string.Format(Resources.GORFS_NO_FILENAME, path));
		    }

		    GorgonFileSystemDirectory directory = GetDirectory(directoryName);

		    if (directory == null)
		    {
		        throw new ArgumentException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, directoryName), nameof(path));
		    }

		    if (directory.Directories.Contains(fileName))
            {
                throw new ArgumentException(string.Format(Resources.GORFS_DIRECTORY_EXISTS, fileName));
            }

			// Create the entry.
			var result = new GorgonFileSystemFileEntry(provider, directory, fileName, mountPoint, physicalLocation, size, offset, createDate);

            // If the file exists, then override it, otherwise it'll just be added.
			((IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>)directory.Files)[fileName] = result;

			return result;
		}

		/// <summary>
		/// Function to find all the directories specified in the directory mask.
		/// </summary>
		/// <param name="path">Path to the directory to start searching in.</param>
		/// <param name="directoryMask">The directory name or mask to search for.</param>
		/// <param name="recursive"><b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="Gorgon.IO.GorgonFileSystemDirectory">GorgonFileSystemDirectory</see> objects.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="directoryMask"/> or the path parameter is a zero length string.</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> was not found.</exception>
		/// <remarks>This method will accept file name masks like directory*, directory??1 and directory*a* when searching.
		/// <para>Please note that the <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Specifying something like /MyDir/ThisDir/C*w/ will fail.</para>
		/// </remarks>
		public IReadOnlyList<GorgonFileSystemDirectory> FindDirectories(string path, string directoryMask, bool recursive)
		{
		    if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

			GorgonFileSystemDirectory startDirectory = GetDirectory(path);
		    if (startDirectory == null)
		    {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, path));
		    }

		    var entries = new List<GorgonFileSystemDirectory>();
			SearchDirectories(startDirectory, entries, recursive, directoryMask);

			return entries;
		}

		/// <summary>
		/// Function to find all the directories specified in the directory mask.
		/// </summary>
		/// <param name="directoryMask">The directory name or mask to search for.</param>
		/// <param name="recursive"><b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="Gorgon.IO.GorgonFileSystemDirectory">GorgonFileSystemDirectory</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="directoryMask"/> parameter is a zero length string.</exception>
		/// <remarks>This method will accept file name masks like directory*, directory??1 and directory*a* when searching.
		/// <para>Please note that the <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Specifying something like /MyDir/ThisDir/C*w/ will fail.</para>
		/// </remarks>
		public IReadOnlyList<GorgonFileSystemDirectory> FindDirectories(string directoryMask, bool recursive)
		{
			return FindDirectories("/", directoryMask, recursive);
		}

		/// <summary>
		/// Function to find all the files specified in the file mask.
		/// </summary>
		/// <param name="path">Path to start searching in.</param>
		/// <param name="fileMask">The file name or mask to search for.</param>
		/// <param name="recursive"><b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="Gorgon.IO.GorgonFileSystemFileEntry">GorgonFileSystemFileEntry</see> objects.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> is a zero length string.</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> parameter was not found.</exception>
		/// <remarks>This method will accept file name masks like file*, file??1 and file*a* when searching.
		/// <para>Please note that the <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Specifying something like /MyDir/ThisDir/C*w.ext will fail.</para>
		/// </remarks>
		public IReadOnlyList<GorgonFileSystemFileEntry> FindFiles(string path, string fileMask, bool recursive)
		{
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

			GorgonFileSystemDirectory start = GetDirectory(path);

		    if (start == null)
		    {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, path));
		    }

		    var entries = new List<GorgonFileSystemFileEntry>();
			SearchFiles(start, entries, recursive, fileMask);

			return entries;
		}

		/// <summary>
		/// Function to find all the files specified in the file mask.
		/// </summary>
		/// <param name="fileMask">The file name or mask to search for.</param>
		/// <param name="recursive"><b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="Gorgon.IO.GorgonFileSystemFileEntry">GorgonFileSystemFileEntry</see> objects.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileMask"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="fileMask"/> is a zero length string.</exception>
		/// <remarks>This method will accept file name masks like file*, file??1 and file*a* when searching.
		/// <para>Please note that the <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Specifying something like /MyDir/ThisDir/C*w.ext will fail.</para>
		/// </remarks>
		public IReadOnlyList<GorgonFileSystemFileEntry> FindFiles(string fileMask, bool recursive)
		{
			return FindFiles("/", fileMask, recursive);
		}

		/// <summary>
		/// Function to retrieve a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The file requested or NULL (<i>Nothing</i> in VB.Net) if the file was not found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when there is no file name in the path.</para>
		/// </exception>
		public GorgonFileSystemFileEntry GetFile(string path)
		{
		    if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

		    if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
		    {
		        path = "/" + path;
		    }

            // Get path parts.
		    string directory = Path.GetDirectoryName(path).FormatDirectory('/');
			string filename = Path.GetFileName(path).FormatFileName();

            // Check for file name.
		    if (string.IsNullOrWhiteSpace(filename))
		    {
		        throw new ArgumentException(string.Format(Resources.GORFS_NO_FILENAME, path), nameof(path));
		    }

            // Start search.
		    GorgonFileSystemDirectory search = GetDirectory(directory);

			if (search == null)
			{
				return null;
			}

			return search.Files.Contains(filename) ? search.Files[filename] : null;
		}

		/// <summary>
		/// Function to retrieve a directory from the provider.
		/// </summary>
		/// <param name="path">Path to the directory to retrieve.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <returns>The file system directory if found, NULL (<i>Nothing</i> in VB.Net) if not.</returns>
		public GorgonFileSystemDirectory GetDirectory(string path)
		{
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, path);
            }

			path = path.FormatDirectory('/');

		    if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
		    {
		        path = "/" + path;
		    }

            // Optimization to deal with the root path.
		    if (path == "/")
		    {
		        return RootDirectory;
		    }

		    string[] directories = path.Split(new[]
		        {
		            '/'
		        }, StringSplitOptions.RemoveEmptyEntries);

		    if (directories.Length == 0)
		    {
		        return null;
		    }

		    GorgonFileSystemDirectory directory = RootDirectory;

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < directories.Length; i++)
			{
                // Search our child directories.
			    if (directory.Directories.Contains(directories[i]))
			    {
			        directory = directory.Directories[directories[i]];
			    }
			    else
			    {
                    // If we couldn't find this path part, then abort.
			        return null;
			    }
			}

			return directory;
		}

		/// <summary>
		/// Function to read a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file to read.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in <paramref name="path"/> was not found.</exception>
		/// <returns>An array of bytes containing the data in the file.</returns>
		public byte[] ReadFile(string path)
		{
			GorgonFileSystemFileEntry file = GetFile(path);
			
		    if (file == null)
		    {
			    throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, path));
		    }

			return file.Size == 0 ? new byte[] {} : ReadFile(file);
		}

		/// <summary>
		/// Function to read a file from the file system.
		/// </summary>
		/// <param name="file">The file to read.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <returns>An array of bytes containing the data in the file.</returns>
		public byte[] ReadFile(GorgonFileSystemFileEntry file)
		{
			byte[] data;

			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			using (GorgonFileSystemStream stream = OpenStream(file, false))
			{
				data = new byte[stream.Length];

				if (data.Length > 0)
				{
					stream.Read(data, 0, data.Length);
				}
			}

			return data;
		}

		/// <summary>
		/// Function to read a write a file to the file system.
		/// </summary>
		/// <param name="file">The file to write.</param>
		/// <param name="data">Data to write.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="file"/> belongs to another file system.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</exception>
		/// <remarks>To create a 0 byte file, pass an empty array into the data parameter.</remarks>
		public void WriteFile(GorgonFileSystemFileEntry file, byte[] data)
		{
            if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			lock(_syncLock)
			{
				// Write the file out to the write location.
				using(GorgonFileSystemStream stream = OpenStream(file, true))
				{
					stream.Write(data, 0, data.Length);
				}

				var fileInfo = new FileInfo(file.PhysicalFileSystemPath);

				file.Update(fileInfo.Length, null, fileInfo.CreationTime, WriteLocation, file.PhysicalFileSystemPath,
				            _defaultProvider);
			}
		}

		/// <summary>
		/// Function to read a write a file to the file system.
		/// </summary>
		/// <param name="path">Path to the file to write.</param>
		/// <param name="data">Data to write.</param>
        /// <returns>The file system file entry that was updated or created.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL and the file exists.  See remarks.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>The file system provider that holds the file is read-only.</para></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</exception>
        /// <remarks>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="data"/> parameter will only add a file entry (if it does not already exist) to the virtual directory, but will not create 
        /// it on the actual physical file system.  To create a 0 byte file, pass an empty array into the data parameter.</remarks>
		public GorgonFileSystemFileEntry WriteFile(string path, byte[] data)
		{
			lock(_syncLock)
			{
				GorgonFileSystemFileEntry file = GetFile(path) ??
												 AddFileEntry(_defaultProvider, path, WriteLocation, GetWritePath(path), 0, 0,
																			   DateTime.Now);

				if ((data != null) && (file != null))
				{
					WriteFile(file, data);
				}

				return file;
			}
		}

		/// <summary>
		/// Function to open a file stream for reading/writing.
		/// </summary>
		/// <param name="file">The file to read or write.</param>
		/// <param name="writeable"><b>true</b> to write to this file, <b>false</b> to open read-only.</param>
		/// <remarks>Some file system providers cannot write, and will throw an exception if this is the case.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="file"/> belongs to another file system.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the <paramref name="writeable"/> parameter is <b>true</b> and <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</exception>
		/// <returns>The open <see cref="Gorgon.IO.GorgonFileSystemStream"/> file stream object.</returns>
		public GorgonFileSystemStream OpenStream(GorgonFileSystemFileEntry file, bool writeable)
		{
			GorgonFileSystemStream stream;

			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

            if (file.FileSystem != this)
            {
                throw new ArgumentException(Resources.GORFS_FILE_FILESYSTEM_MISMATCH);
            }

			// If the file doesn't exist, then we need to create it if we're writing.
			if (writeable)
			{
				lock(_syncLock)
				{
					string writePath = GetWritePath(file.FullPath);
					
					var info = new FileInfo(writePath);
					stream = new GorgonFileSystemStream(file, File.Open(writePath, FileMode.Create, FileAccess.Write, FileShare.None));
					file.Update(info.Length, 0, DateTime.Now, WriteLocation, writePath, _defaultProvider);
				}
			}
			else
			{
				stream = file.Provider.OnOpenFileStream(file);
			}

			return stream;
		}

		/// <summary>
		/// Function to open a file stream for reading/writing.
		/// </summary>
		/// <param name="path">Path to the file to read or write.</param>
		/// <param name="writeable"><b>true</b> to write to this file, <b>false</b> to open read-only.</param>
		/// <remarks>Some file system providers cannot write, and will throw an exception if this is the case.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.
		/// <para>-or-</para><para>The file system provider that holds the file is read-only.</para>
		/// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the <paramref name="writeable"/> parameter is <b>true</b> and <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> is empty.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in <paramref name="path"/> was not found and <paramref name="writeable"/> is <b>false</b>.</exception>
		/// <returns>The open <see cref="Gorgon.IO.GorgonFileSystemStream"/> file stream object.</returns>
		public GorgonFileSystemStream OpenStream(string path, bool writeable)
		{
			GorgonFileSystemFileEntry file = GetFile(path);
            
            // If the file doesn't exist, then we need to create it if we're writing.
            if (writeable)
		    {
			    lock(_syncLock)
			    {
				    if (file != null)
				    {
					    return OpenStream(file, true);
				    }

				    string writePath = GetWritePath(path);

				    file = AddFileEntry(_defaultProvider, path,
					    WriteLocation,
					    writePath, 0,
					    0, DateTime.Now);

				    return OpenStream(file, true);
			    }
		    }
			
			if (file == null)
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, path));
			}

			return OpenStream(file, false);
		}

		/// <summary>
		/// Function to create a directory.
		/// </summary>
		/// <param name="path">Path to the new directory.</param>
		/// <returns>The new directory.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <remarks>This method will create a directory in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public GorgonFileSystemDirectory CreateDirectory(string path)
		{
		    if (path == null)
		    {
		        throw new ArgumentNullException(nameof(path));
		    }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

		    lock(_syncLock)
		    {
		        GorgonFileSystemDirectory directory = GetDirectory(path) ?? AddDirectoryEntry(path);

			    if (string.IsNullOrWhiteSpace(WriteLocation))
			    {
				    return directory;
			    }

			    string newPath = GetWritePath(directory.FullPath);

			    if (!Directory.Exists(newPath))
			    {
				    Directory.CreateDirectory(newPath);
			    }

			    return directory;
		    }
		}


		/// <summary>
		/// Function to delete a file.
		/// </summary>
		/// <param name="file">The file to delete.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="file"/> belongs to another file system.</exception>
		/// <remarks>This method will remove a file in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public void DeleteFile(GorgonFileSystemFileEntry file)
		{
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.FileSystem != this)
            {
                throw new ArgumentException(Resources.GORFS_FILE_FILESYSTEM_MISMATCH);
            }

            ((IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>)file.Directory.Files).Remove(file);

            if (string.IsNullOrWhiteSpace(WriteLocation))
            {
                return;
            }

            string newPath = GetWritePath(file.FullPath);

            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
        }


		/// <summary>
		/// Function to delete a file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in the <paramref name="path"/> parameter was not found.</exception>
		/// <remarks>This method will remove a file in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public void DeleteFile(string path)
		{
		    if (path == null)
		    {
		        throw new ArgumentNullException(nameof(path));
		    }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

			GorgonFileSystemFileEntry file = GetFile(path);

            if (file == null)
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, path));
            }

            DeleteFile(file);
		}

	    /// <summary>
	    /// Function to delete a directory.
	    /// </summary>
	    /// <param name="directory">The directory to delete.</param>
	    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="directory"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
	    /// <exception cref="System.ArgumentException">Thrown if the <paramref name="directory"/> belongs to another file system.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the <paramref name="directory"/> was not found.</exception>
        /// <remarks>This method will remove a directory in the virtual file system and in the writable area of the physical file system.
	    /// <para>The writeable area is specified by the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
	    /// </remarks>
	    public void DeleteDirectory(GorgonFileSystemDirectory directory)
	    {
	        string newPath = null;

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (directory.FileSystem != this)
            {
                throw new ArgumentException(Resources.GORFS_DIR_FILESYSTEM_MISMATCH);
            }

	        lock(_syncLock)
	        {
	            // If we specify the root, then remove everything.
	            if (directory.Name == "/")
	            {
					((IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>)directory.Directories).Clear();
	                ((IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>)directory.Files).Clear();

	                if (!string.IsNullOrWhiteSpace(WriteLocation))
	                {
	                    newPath = WriteLocation;
	                }
	            }
	            else
	            {
	                if (!string.IsNullOrWhiteSpace(WriteLocation))
	                {
	                    newPath = GetWritePath(directory.FullPath);
	                }

	                ((IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>)directory.Parent.Directories).Remove(directory);
	            }

	            if ((string.IsNullOrWhiteSpace(newPath)) || (!Directory.Exists(newPath)))
	            {
	                return;
	            }

	            var dirInfo = new DirectoryInfo(newPath);

	            // Remove from the write area if the directory exists.
	            if (directory.Name == "/")
	            {
	                var directories = dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
	                var files = dirInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

	                foreach (var directoryPath in directories)
	                {
	                    directoryPath.Delete(true);
	                }

	                foreach (var file in files)
	                {
	                    file.Delete();
	                }
	            }
	            else
	            {
	                dirInfo.Delete(true);
	            }
	        }
	    }

	    /// <summary>
		/// Function to delete a directory.
		/// </summary>
		/// <param name="path">Path to the directory.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory specified in <paramref name="path"/> was not found.</exception>
		/// <remarks>This method will remove a directory in the virtual file system and in the writable area of the physical file system.
		/// <para>The writeable area is specified by the <see cref="P:Gorgon.IO.GorgonFileSystem.WriteLocation">WriteLocation</see> property.</para>
		/// </remarks>
		public void DeleteDirectory(string path)
		{
	        if (path == null)
		    {
                throw new ArgumentNullException(nameof(path));
		    }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(path));
            }

	        lock(_syncLock)
	        {
	            GorgonFileSystemDirectory directory = GetDirectory(path);

	            if (directory == null)
	            {
	                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, path));
	            }

	            DeleteDirectory(directory);
	        }
		}

		/// <summary>
		/// Function to clear all the mounted directories and files.
		/// </summary>
		/// <remarks>
		/// This will remove all the files and directories from the virtual file system and will clear the write directory.
		/// <para>Note that this method will not delete any files or directories from the physical file system.</para></remarks>
		public void Clear()
		{
			_mountListChanged = true;
			_mountPoints.Clear();
		    _writeLocation = string.Empty;
			((IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>)RootDirectory.Directories).Clear();
			((IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>)RootDirectory.Files).Clear();
		}

		/// <summary>
		/// Function to refresh all the files and directories in the file system.
		/// </summary>
		public void Refresh()
		{
            // Don't allow multiple threads to refresh this file system more than once.
		    try
		    {
		        if (Interlocked.Increment(ref _refreshSemaphore) > 1)
		        {
		            return;
		        }

		        string writeLocation = WriteLocation;

				((IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>)RootDirectory.Directories).Clear();
				((IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>)RootDirectory.Files).Clear();

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

			    if (string.IsNullOrWhiteSpace(writeLocation))
			    {
				    return;
			    }

			    _writeLocation = writeLocation;
			    QueryWriteLocation();
		    }
		    finally
		    {
		        Interlocked.Decrement(ref _refreshSemaphore);
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.</exception>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		public void Unmount(string physicalPath, string mountLocation)
		{
			Unmount(new GorgonFileSystemMountPoint(physicalPath, mountLocation));
		}

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a physical path.
		/// </summary>
		/// <param name="physicalPath">The physical path to unmount.</param>
		/// <remarks>This overload will unmount all the mounted virtual files/directories for every mount point with the specified <paramref name="physicalPath"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
        /// <exception cref="System.IO.IOException">The path was not found.</exception>
        public void Unmount(string physicalPath)
		{
			var mountPoints =
				_mountPoints.Where(
					item =>
					String.Equals(Path.GetFullPath(physicalPath), Path.GetFullPath(item.PhysicalPath),
					               StringComparison.OrdinalIgnoreCase));

			foreach (var mountPoint in mountPoints)
			{
				UnmountMountPoint(mountPoint);				
			}

			Refresh();
		}		

		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="mountPoint">The mount point containing the physical path and the virtual file system location.</param>
		/// <exception cref="System.ArgumentException">Thrown when the physical path in the <paramref name="mountPoint"/> parameter is not valid.
		/// <para>-or-</para><para>Thrown when the file in the mountPoint parameter cannot be read by any of the file system providers.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		/// <remarks>
		/// <para>
		/// This method is used to mount the contents of a physical file system object (such as a folder, or a zip file if the appropriate provider is installed) into a virtual folder in the 
		/// file system. All folders and files in the physical file system object will be made available under the virtual folder specified by the <see cref="GorgonFileSystemMountPoint.MountLocation"/> 
		/// property of the <paramref name="mountPoint"/> parameter..
		/// </para>
		/// </remarks>
		public void Mount(GorgonFileSystemMountPoint mountPoint)
		{
			Mount(mountPoint.PhysicalPath, mountPoint.MountLocation);
		}

		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="physicalPath">Path to the directory or file that contains the files/directories to enumerate.</param>
		/// <param name="mountPath">[Optional] Folder path to mount into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory specified by <paramref name="physicalPath"/> was not found.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if a file was specified by <paramref name="physicalPath"/> and was not found.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		/// <returns>A mount point value for the currently mounted physical path and its mount point in the virtual file system.</returns>
		/// <remarks>
		/// <para>
		/// This method is used to mount the contents of a physical file system object (such as a folder, or a zip file if the appropriate provider is installed) into a virtual folder in the 
		/// file system. All folders and files in the physical file system object will be made available under the virtual folder specified by the <paramref name="mountPath"/> parameter.
		/// </para>
		/// <para>
		/// The <paramref name="mountPath"/> parameter is optional, and if omitted, the contents of the physical file system object will be mounted into the root of the virtual file system. If the 
		/// <paramref name="mountPath"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net) or empty, then the mount point will be at the root.
		/// </para>
		/// </remarks>
		public GorgonFileSystemMountPoint Mount(string physicalPath, string mountPath = null)
		{
			if (physicalPath == null)
            {
                throw new ArgumentNullException(nameof(physicalPath));
            }

            if (string.IsNullOrWhiteSpace(mountPath))
            {
	            mountPath = "/";
            }

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, nameof(physicalPath));
            }

			lock(_syncLock)
			{
				// Don't mount the write location.  It will be automatically queried every time we mount a physical location.
				if ((!string.IsNullOrWhiteSpace(WriteLocation)) &&
				    (String.Equals(physicalPath, WriteLocation, StringComparison.OrdinalIgnoreCase)))
				{
					// Requery the write location if it exists.
					QueryWriteLocation();

					return new GorgonFileSystemMountPoint(physicalPath, "/");
				}

				physicalPath = Path.GetFullPath(physicalPath);
				string fileName = Path.GetFileName(physicalPath);
				string directory = Path.GetDirectoryName(physicalPath);

				// If we have no file name, assume we're mounting a folder from the OS filesystem.
				GorgonFileSystemMountPoint mountPoint;

				if (string.IsNullOrWhiteSpace(fileName))
				{
					if (string.IsNullOrWhiteSpace(directory))
					{
						throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, physicalPath));
					}

					directory = directory.FormatDirectory(Path.DirectorySeparatorChar);

					// Use the default folder provider.
					if (!Directory.Exists(directory))
					{
						throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, directory));
					}

					GetFileSystemObjects(_defaultProvider, physicalPath, mountPath);

					mountPoint = new GorgonFileSystemMountPoint(physicalPath, mountPath);
					if (!_mountPoints.Contains(mountPoint))
					{
						_mountPoints.Add(mountPoint);
						_mountListChanged = true;
					}

					QueryWriteLocation();
					return mountPoint;
				}

				fileName = fileName.FormatFileName();
				if (!string.IsNullOrWhiteSpace(directory))
				{
					directory = directory.FormatDirectory(Path.DirectorySeparatorChar);
				}

				// Rebuild the file path.
				fileName = directory + fileName;

				if (!File.Exists(fileName))
				{
					throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, fileName));
				}

				GorgonFileSystemProvider provider = _providers.FirstOrDefault(item => item.Value.CanReadFile(fileName)).Value;

				if (provider == null)
				{
					throw new IOException(string.Format(Resources.GORFS_CANNOT_READ_FILESYSTEM, fileName));
				}

				GetFileSystemObjects(provider, physicalPath, mountPath);

				mountPoint = new GorgonFileSystemMountPoint(physicalPath, mountPath);
				if (!_mountPoints.Contains(mountPoint))
				{
					_mountPoints.Add(mountPoint);
					_mountListChanged = true;
				}

				QueryWriteLocation();
				return mountPoint;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
		/// </summary>
		/// <param name="provider">A single file system provider to assign to this file system.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="provider" /> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// To mount other file system object types (e.g. zip files), a <see cref="GorgonFileSystemProvider"/> is necessary and must be passed into this constructor. 
		/// </para> 
		/// <para>
		/// To retrieve a provider, use the <see cref="GorgonFileSystemProviderFactory.CreateProvider"/> method.
		/// </para>
		/// </remarks>
		public GorgonFileSystem(GorgonFileSystemProvider provider, IGorgonLog log = null)
			: this(new GorgonFileSystemProvider[0], log)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			_providers[provider.Name] = provider;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
		/// </summary>
		/// <param name="providers">[Optional] The providers available to this file system.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <remarks>
		/// <para>
		/// To mount other file system object types (e.g. zip files), a <see cref="GorgonFileSystemProvider"/> is necessary and must be passed into this constructor. 
		/// </para>
		/// <para>
		/// To get a list of providers to pass in, use the <see cref="GorgonFileSystemProviderFactory"/> object to create the providers.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonFileSystem(IEnumerable<GorgonFileSystemProvider> providers, IGorgonLog log = null)
		{
			if (providers == null)
			{
				throw new ArgumentNullException(nameof(providers));
			}

			if (log != null)
			{
				_log = log;
			}

			_mountListChanged = true;
			_mountPoints = new List<GorgonFileSystemMountPoint>();
			_mountPointList = new ReadOnlyCollection<GorgonFileSystemMountPoint>(_mountPoints);
			_providers = new Dictionary<string, GorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);

			RootDirectory = new GorgonFileSystemDirectory(this, "/", null);

			WriteLocation = string.Empty;

			// Get all the providers in the parameter.
			foreach (GorgonFileSystemProvider provider in providers)
			{
				_providers[provider.Name] = provider;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
		/// </summary>
		/// <param name="log">[Optional] The application log file.</param>
		/// <remarks>
		/// This will create a file system without any providers. Because of this, the only physical file system objects that can be mounted are folders.
		/// </remarks>
		public GorgonFileSystem(IGorgonLog log = null)
			: this(new GorgonFileSystemProvider[0], log)
		{
		}
		#endregion
	}
}
