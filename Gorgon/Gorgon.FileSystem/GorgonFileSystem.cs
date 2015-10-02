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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonFileSystem"/>
	public class GorgonFileSystem 
		: IGorgonFileSystem
	{
		#region Variables.
		/// <summary>
		/// Directory separator character.
		/// </summary>
	    internal static readonly string PhysicalDirSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

		// Synchronization object.
		private readonly object _syncLock = new object();
		// The list of providers available to the file system.
		private readonly Dictionary<string, IGorgonFileSystemProvider> _providers;
		// The log file for the application.
		private readonly IGorgonLog _log;
		// The root directory for the file system.
		private readonly VirtualDirectory _rootDirectory;
		// A list of mount points grouped by provider.
		private readonly HashSet<GorgonFileSystemMountPoint> _mountProviders;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public IEnumerable<IGorgonFileSystemProvider> Providers => _providers.Select(item => item.Value);

		/// <inheritdoc/>
		/// <remarks>
		/// This is the default folder file system provider.
		/// </remarks>
		public IGorgonFileSystemProvider DefaultProvider
		{
			get;
		}

		/// <inheritdoc/>
		public IEnumerable<GorgonFileSystemMountPoint> MountPoints => _mountProviders;

		/// <inheritdoc cref="IGorgonFileSystem.RootDirectory"/>
		internal VirtualDirectory InternalRootDirectory => _rootDirectory;

		/// <inheritdoc/>
		public IGorgonVirtualDirectory RootDirectory => _rootDirectory;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to mount a file as a mount point in the virtual file system.
		/// </summary>
		/// <param name="physicalPath">The physical path to the file.</param>
		/// <param name="mountPath">The path to mount under.</param>
		/// <returns>A new mount point.</returns>
		private GorgonFileSystemMountPoint MountFile(string physicalPath, string mountPath)
		{
			// Rebuild the file path.
			if (!File.Exists(physicalPath))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, physicalPath));
			}

			IGorgonFileSystemProvider provider = _providers.FirstOrDefault(item => item.Value.CanReadFileSystem(physicalPath)).Value;

			if (provider == null)
			{
				throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_READ_FILESYSTEM, physicalPath));
			}

			var mountPoint = new GorgonFileSystemMountPoint(provider, physicalPath, mountPath);

			GetFileSystemObjects(mountPoint);

			if (_mountProviders.Contains(mountPoint))
			{
				_mountProviders.Remove(mountPoint);
			}

			_mountProviders.Add(mountPoint);

			return mountPoint;
		}

		/// <summary>
		/// Function to mount an operating system directory as a mount point in the virtual file system.
		/// </summary>
		/// <param name="physicalPath">The physical path to the operating system directory.</param>
		/// <param name="mountPath">The path to mount under.</param>
		/// <returns>A new mount point.</returns>
		private GorgonFileSystemMountPoint MountDirectory(string physicalPath, string mountPath)
		{
			physicalPath = physicalPath.FormatDirectory(Path.DirectorySeparatorChar);

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, physicalPath));
			}
			
			// Use the default folder provider.
			if (!Directory.Exists(physicalPath))
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, physicalPath));
			}

			var mountPoint = new GorgonFileSystemMountPoint(DefaultProvider, physicalPath, mountPath);

			GetFileSystemObjects(mountPoint);

			if (_mountProviders.Contains(mountPoint))
			{
				_mountProviders.Remove(mountPoint);
			}

			// Assign the default provider to the mount point.
			_mountProviders.Add(mountPoint);

			return mountPoint;
		}

		/// <summary>
		/// Function to mount a non physical location in the file system.
		/// </summary>
		/// <param name="location">The non physical location to mount.</param>
		/// <param name="mountDirectory">The virtual directory to mount in.</param>
		/// <returns>A <see cref="GorgonFileSystemMountPoint"/>.</returns>
		private GorgonFileSystemMountPoint MountNonPhysicalLocation(string location, string mountDirectory)
		{
			IGorgonFileSystemProvider provider = _providers.FirstOrDefault(item => item.Value.CanReadFileSystem(location)).Value;

			if (provider == null)
			{
				throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_READ_FILESYSTEM, location));
			}

			var mountPoint = new GorgonFileSystemMountPoint(provider, location, mountDirectory, true);

			GetFileSystemObjects(mountPoint);

			if (_mountProviders.Contains(mountPoint))
			{
				_mountProviders.Remove(mountPoint);
			}

			// Assign the default provider to the mount point.
			_mountProviders.Add(mountPoint);

			return mountPoint;
		}

		/// <summary>
		/// Function to retrieve the file system objects from the physical file system.
		/// </summary>
		/// <param name="mountPoint">The mount point to link the physical file system with the virtual file system.</param>
		private void GetFileSystemObjects(GorgonFileSystemMountPoint mountPoint)
        {
	        string physicalPath = mountPoint.IsFakeMount ? mountPoint.PhysicalPath : Path.GetFullPath(mountPoint.PhysicalPath);
            string fileName = mountPoint.IsFakeMount ? null : Path.GetFileName(physicalPath);

			if (!mountPoint.IsFakeMount)
			{
				physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory(Path.DirectorySeparatorChar);

				if (!string.IsNullOrWhiteSpace(fileName))
				{
					physicalPath += fileName.FormatFileName();
				}
			}

			// Find existing mount point.
			VirtualDirectory mountDirectory = InternalGetDirectory(mountPoint.MountLocation);

			if (mountDirectory == null)
			{
				mountDirectory = _rootDirectory.Directories.Add(mountPoint, mountPoint.MountLocation);
			}
			else
			{
				mountDirectory.MountPoint = mountPoint;
			}

			_log.Print("Mounting physical file system path '{0}' to virtual file system path '{1}'.", LoggingLevel.Simple, physicalPath, mountPoint.MountLocation);

	        GorgonPhysicalFileSystemData data = mountPoint.Provider.Enumerate(physicalPath, mountDirectory);

            // Process the directories.
            foreach (string directory in data.Directories)
            {
	            VirtualDirectory existingDirectory = InternalGetDirectory(directory);

				// If the directory path already exists for another provider, then override it with the 
				// provider we're currently loading. All directories and files will be overridden by the last 
				// provider loaded if they already exist.
				if (existingDirectory == null)
	            {
		            _rootDirectory.Directories.Add(mountPoint, directory);
	            }
	            else
				{
					_log.Print($"\"{existingDirectory.FullPath}\" already exists in provider: " +
							   $"\"{existingDirectory.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
							   LoggingLevel.Verbose);

		            existingDirectory.MountPoint = mountPoint;
	            }
            }

			// Process the files.
			foreach (IGorgonPhysicalFileInfo file in data.Files)
			{
				string directoryName = Path.GetDirectoryName(file.VirtualPath).FormatDirectory('/');

				if (!directoryName.StartsWith("/"))
				{
					directoryName += "/";
				}

				VirtualDirectory directory = InternalGetDirectory(directoryName);

				if (directory == null)
				{
					throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryName));
				}

				VirtualFile virtualFile;

				// Update the file information to the most recent provider.
				if (!directory.Files.TryGetValue(file.Name, out virtualFile))
				{
					directory.Files.Add(mountPoint, file);
				}
				else
				{
					_log.Print($"\"{file.FullPath}\" already exists in provider: " +
							   $"\"{virtualFile.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
							   LoggingLevel.Verbose);

					virtualFile.MountPoint = mountPoint;
					virtualFile.PhysicalFile = file;
				}
			}

            _log.Print("{0} directories parsed, and {1} files processed.", LoggingLevel.Simple, data.Directories.Count, data.Files.Count);
        }

		/// <summary>
		/// Function to determine if the name of an object matches the pattern specified.
		/// </summary>
		/// <typeparam name="T">Type of named object to compare.</typeparam>
		/// <param name="item">Item to compare.</param>
		/// <param name="searchMask">The mask used to filter.</param>
		/// <returns><b>true</b> if the name of the item matches the pattern, <b>false</b> if not.</returns>
		private static bool IsPatternMatch<T>(T item, string searchMask)
			where T : IGorgonNamedObject
		{
			
			if ((searchMask.EndsWith("*")) || (!searchMask.StartsWith("*")) || (searchMask.Length != 1))
			{
				return Regex.IsMatch(item.Name,
				                     Regex.Escape(searchMask).Replace(@"\*", ".*").Replace(@"\?", "."),
				                     RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}

			return item.Name.EndsWith(searchMask.Substring(searchMask.IndexOf('*') + 1), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Function to flatten a directory hierarchy.
		/// </summary>
		/// <param name="directory">The directory to start searching in.</param>
		/// <param name="searchMask">The pattern mask to search on.</param>
		/// <returns>An enumerable containing the flattened list.</returns>
		internal static IEnumerable<VirtualDirectory> FlattenDirectoryHierarchy(VirtualDirectory directory, string searchMask)
		{
			IEnumerable<VirtualDirectory> directories = directory.Directories.InternalEnumerable();

			if (!string.Equals(searchMask, "*", StringComparison.OrdinalIgnoreCase))
			{
				directories = directories.Where(item => IsPatternMatch(item, searchMask));
			}

			var stack = new Queue<VirtualDirectory>(directories);

			while (stack.Count > 0)
			{
				VirtualDirectory current = stack.Dequeue();

				yield return current;

				directories = current.Directories.InternalEnumerable();

				if (!string.Equals(searchMask, "*", StringComparison.OrdinalIgnoreCase))
				{
					directories = directories.Where(item => IsPatternMatch(item, searchMask));
				}

				foreach (VirtualDirectory subDirectory in directories)
				{
					stack.Enqueue(subDirectory);
				}
			}
		}

		/// <summary>
		/// Function to flatten a file hierarchy.
		/// </summary>
		/// <param name="directory">The directory to start searching in.</param>
		/// <param name="searchMask">The pattern mask to search on.</param>
		/// <returns>An enumerable containing the flattened list.</returns>
		private static IEnumerable<VirtualFile> FlattenFileHierarchy(VirtualDirectory directory, string searchMask)
		{
			IEnumerable<VirtualDirectory> directories = FlattenDirectoryHierarchy(directory, "*");

			foreach (VirtualFile file in directory.Files.InternalEnumerable().Where(item => IsPatternMatch(item, searchMask)))
			{
				yield return file;
			}

			foreach (VirtualFile file in directories.SelectMany(subDirectory => subDirectory.Files.InternalEnumerable().Where(item => IsPatternMatch(item, searchMask))))
			{
				yield return file;
			}
		}

		/// <inheritdoc cref="IGorgonFileSystem.FindDirectories"/>
		internal IEnumerable<VirtualDirectory> InternalFindDirectories(string path, string directoryMask, bool recursive)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			VirtualDirectory startDirectory = InternalGetDirectory(path);
			if (startDirectory == null)
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
			}

			return !recursive
				       ? (string.Equals(directoryMask, "*", StringComparison.OrdinalIgnoreCase)
					          ? startDirectory.Directories.InternalEnumerable()
					          : startDirectory.Directories.InternalEnumerable()
					                          .Where(item => IsPatternMatch(item, directoryMask)))
				       : FlattenDirectoryHierarchy(startDirectory, directoryMask);
		}

		/// <inheritdoc cref="IGorgonFileSystem.FindFiles"/>
		internal IEnumerable<VirtualFile> InternalFindFiles(string path, string fileMask, bool recursive)
		{
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
            }

			VirtualDirectory start = InternalGetDirectory(path);

		    if (start == null)
		    {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
		    }

			return !recursive
				       ? (string.Equals(fileMask, "*", StringComparison.OrdinalIgnoreCase)
					          ? start.Files.InternalEnumerable()
					          : start.Files.InternalEnumerable().Where(item => IsPatternMatch(item, fileMask)))
				       : FlattenFileHierarchy(start, fileMask);
		}

		/// <inheritdoc cref="IGorgonFileSystem.GetFile"/>
		internal VirtualFile InternalGetFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				path = "/" + path;
			}

			// Get path parts.
			string directory = Path.GetDirectoryName(path);

			directory = string.IsNullOrWhiteSpace(directory) ? "/" : directory.FormatDirectory('/');

			string filename = Path.GetFileName(path).FormatFileName();

			// Check for file name.
			if (string.IsNullOrWhiteSpace(filename))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path), nameof(path));
			}

			// Start search.
			VirtualDirectory search = InternalGetDirectory(directory);

			if (search == null)
			{
				return null;
			}

			return search.Files.Contains(filename) ? search.Files[filename] : null;
		}

		/// <inheritdoc cref="IGorgonFileSystem.GetDirectory"/>
		internal VirtualDirectory InternalGetDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, path);
			}

			path = path.FormatDirectory('/');

			if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				path = "/" + path;
			}

			// Optimization to deal with the root path.
			if (path == "/")
			{
				return _rootDirectory;
			}

			string[] directories = path.Split(new[]
				{
					'/'
				}, StringSplitOptions.RemoveEmptyEntries);

			if (directories.Length == 0)
			{
				return null;
			}

			VirtualDirectory directory = _rootDirectory;

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

		/// <inheritdoc/>
		public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask, bool recursive = true) => InternalFindDirectories(path, directoryMask, recursive);

		/// <summary>
		/// <inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/>
		/// </summary>
		/// <param name="directoryMask"><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></param>
		/// <param name="recursive"><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></param>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></exception>
		/// <exception cref="ArgumentException"><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></exception>
		/// <exception cref="DirectoryNotFoundException"><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></exception>
		/// <returns><inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/></returns>
		/// <remarks>
		/// <inheritdoc cref="IGorgonFileSystem.FindDirectories(string, string, bool)"/>
		/// </remarks>
		public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string directoryMask, bool recursive = true) => InternalFindDirectories("/", directoryMask, recursive);

		/// <inheritdoc/>
		public IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask, bool recursive = true) => InternalFindFiles("/", fileMask, recursive);

		/// <summary>
		/// <inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/>
		/// </summary>
		/// <param name="fileMask"><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></param>
		/// <param name="recursive"><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></param>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></exception>
		/// <exception cref="ArgumentException"><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></exception>
		/// <exception cref="DirectoryNotFoundException"><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></exception>
		/// <returns><inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/></returns>
		/// <remarks>
		/// <inheritdoc cref="IGorgonFileSystem.FindFiles(string, string, bool)"/>
		/// </remarks>
		public IEnumerable<IGorgonVirtualFile> FindFiles(string fileMask, bool recursive = true) => InternalFindFiles("/", fileMask, recursive);

		/// <inheritdoc/>
		public IGorgonVirtualFile GetFile(string path) => InternalGetFile(path);

		/// <inheritdoc/>
		public IGorgonVirtualDirectory GetDirectory(string path) => InternalGetDirectory(path);

		/// <inheritdoc/>
		public void Refresh()
		{
			// Don't allow multiple threads to refresh this file system more than once.
			lock (_syncLock)
			{
				// Unload everything at once.
				_rootDirectory.Directories.Clear();
				_rootDirectory.Files.Clear();
				_mountProviders.Clear();

				// Refresh the mount points so we can capture the most up to date data.
				foreach (GorgonFileSystemMountPoint mountPoint in MountPoints)
				{
					Mount(mountPoint.PhysicalPath, mountPoint.MountLocation);
				}
			}
		}

		/// <inheritdoc/>
		public void Unmount(GorgonFileSystemMountPoint mountPoint)
		{
			lock (_syncLock)
			{
				if (!_mountProviders.Contains(mountPoint))
				{
					throw new ArgumentException(string.Format(Resources.GORFS_ERR_MOUNTPOINT_NOT_FOUND,
					                                          mountPoint.MountLocation,
					                                          mountPoint.PhysicalPath),
					                            nameof(mountPoint));
				}

				// Find the directory for the mount point.
				VirtualDirectory mountPointDirectory = InternalGetDirectory(mountPoint.MountLocation);

				// If we don't have the directory in the file system, then we have nothing to remove.
				if (mountPointDirectory == null)
				{
					return;
				}

				IEnumerable<VirtualFile> files = InternalFindFiles(mountPoint.MountLocation, "*", true)
					.Where(item => item.MountPoint.Equals(mountPoint))
					.ToArray();

				foreach (VirtualFile file in files)
				{
					file.Directory.Files.Remove(file);
				}

				// Find all directories and files that are related to the provider.
				IEnumerable<VirtualDirectory> directories = InternalFindDirectories(mountPoint.MountLocation, "*", true)
					.Where(item => item.MountPoint.Equals(mountPoint))
					.OrderByDescending(item => item.FullPath)
					.ThenByDescending(item => item.FullPath.Length)
					.ToArray();

				GorgonFileSystemMountPoint newMountPoint;

				foreach (VirtualDirectory directory in directories)
				{
					if ((directory.Files.Count == 0) && (directory.Directories.Count == 0))
					{
						directory.Parent?.Directories.Remove(directory);
					}
					else
					{
						newMountPoint = directory.Directories.Count > 0
										 ? directory.Directories.First().MountPoint
										 : (directory.Files.Count > 0
												? directory.Files.First().MountPoint
												: new GorgonFileSystemMountPoint(DefaultProvider, directory.MountPoint.PhysicalPath, directory.MountPoint.MountLocation));
						// If there are still files or sub directories in the directory, then keep it around, and 
						// set its provider back to the first provider in the list of directories, files or the 
						// default.
						directory.MountPoint = newMountPoint;
					}
				}

				_mountProviders.Remove(mountPoint);

				// If there's nothing left under this mount point, and it's not the root, then dump the directory.
				if ((mountPointDirectory.Parent == null)
					|| (mountPointDirectory.Directories.Count != 0)
					|| (mountPointDirectory.Files.Count != 0))
				{
					newMountPoint = mountPointDirectory.Directories.Count > 0
										? mountPointDirectory.Directories.First().MountPoint
										: (mountPointDirectory.Files.Count > 0
											   ? mountPointDirectory.Files.First().MountPoint
											   : new GorgonFileSystemMountPoint(DefaultProvider,
																				mountPointDirectory.MountPoint.PhysicalPath,
																				mountPointDirectory.MountPoint.MountLocation));


					mountPointDirectory.MountPoint = newMountPoint;
					return;
				}

				mountPointDirectory.Parent.Directories.Remove(mountPointDirectory);
			}
		}

		/// <inheritdoc/>
		public void Unmount(string physicalPath, string mountLocation)
		{
			// Find all mount points with the physical and virtual paths supplied.
			var mountPoints = MountPoints.Where(item =>
			                                    string.Equals(Path.GetFullPath(physicalPath),
			                                                  Path.GetFullPath(item.PhysicalPath),
			                                                  StringComparison.OrdinalIgnoreCase)
			                                    &&
			                                    string.Equals(mountLocation.FormatDirectory('/'),
			                                                  item.MountLocation.FormatDirectory('/'),
			                                                  StringComparison.OrdinalIgnoreCase))
			                             .ToArray();

			foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
			{
				Unmount(mountPoint);
			}
		}

		/// <inheritdoc/>
		public void Unmount(string physicalPath)
		{
			var mountPoints = MountPoints.Where(item =>
			                                    string.Equals(Path.GetFullPath(physicalPath),
			                                                  Path.GetFullPath(item.PhysicalPath),
			                                                  StringComparison.OrdinalIgnoreCase))
			                             .ToArray();

			foreach (var mountPoint in mountPoints)
			{
				Unmount(mountPoint);				
			}
		}

		/// <inheritdoc/>
		public GorgonFileSystemMountPoint Mount(string physicalPath, string mountPath = null)
		{
			if (physicalPath == null)
            {
                throw new ArgumentNullException(nameof(physicalPath));
            }

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(physicalPath));
            }

			if (string.IsNullOrWhiteSpace(mountPath))
			{
				mountPath = "/";
			}

			lock (_syncLock)
			{
				if (physicalPath.StartsWith(@"::\\", StringComparison.OrdinalIgnoreCase))
				{
					return MountNonPhysicalLocation(physicalPath, mountPath);
				}

				physicalPath = Path.GetFullPath(physicalPath);
				string fileName = Path.GetFileName(physicalPath).FormatFileName();
				string directory = Path.GetDirectoryName(physicalPath)?.FormatDirectory(Path.DirectorySeparatorChar);

				// If we have a file, then mount the file using a provider.
				if (!string.IsNullOrWhiteSpace(fileName))
				{
					// If we omitted a directory, then assume we're using the current directory.
					if (string.IsNullOrWhiteSpace(directory))
					{
						directory = Directory.GetCurrentDirectory().FormatDirectory(Path.DirectorySeparatorChar);
					}

					return MountFile(directory + fileName, mountPath);
				}

				// Ensure that our directory name is not empty or null.
				// We can't mount that.
				if (string.IsNullOrWhiteSpace(directory))
				{
					throw new ArgumentException(string.Format(Resources.GORFS_ERR_ILLEGAL_PATH, physicalPath), nameof(physicalPath));
				}

				return MountDirectory(physicalPath, mountPath);
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
		/// To retrieve a <paramref name="provider"/>, use the <see cref="GorgonFileSystemProviderFactory.CreateProvider"/> method.
		/// </remarks>
		public GorgonFileSystem(IGorgonFileSystemProvider provider, IGorgonLog log = null)
			: this(log)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			_providers[provider.GetType().FullName] = provider;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
		/// </summary>
		/// <param name="providers">The providers available to this file system.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <remarks>
		/// To get a list of providers to pass in, use the <see cref="GorgonFileSystemProviderFactory"/> object to create the providers.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonFileSystem(IEnumerable<IGorgonFileSystemProvider> providers, IGorgonLog log = null)
			: this(log)
		{
			if (providers == null)
			{
				throw new ArgumentNullException(nameof(providers));
			}
			
			// Get all the providers in the parameter.
			foreach (IGorgonFileSystemProvider provider in providers.Where(item => item != null))
			{
				_providers[provider.GetType().FullName] = provider;
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
		{
			_log = log ?? new GorgonLogDummy();

			_providers = new Dictionary<string, IGorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);
			_mountProviders = new HashSet<GorgonFileSystemMountPoint>();

			DefaultProvider = new GorgonFileSystemProvider();

			_rootDirectory = new VirtualDirectory(default(GorgonFileSystemMountPoint), this, null, "/");
		}
		#endregion
	}
}
