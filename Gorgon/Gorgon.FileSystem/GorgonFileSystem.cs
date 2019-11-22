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
using System.Diagnostics;
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
    /// <summary>
    /// The virtual file System interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will allow the user to mount directories or packed files (such as Zip files) into a unified file system.  For example, if the user has mounted MyData.zip and C:\users\Bob\Data\ into the file 
    /// system then all files and/or directories from both sources would be combined into a single virtual file system. This has the advantage of being able to access disparate file systems without having 
    /// to run through multiple interfaces to get at their data.
    /// </para>
    /// <para>
    /// The virtual file system is a read only file system. This is done by design so that the integrity of the original physical file systems can be preserved. If your application needs to write data into 
    /// the file system, then the <see cref="IGorgonFileSystemWriter{T}"/> has been provided to give access to a writable area that will integrate with this object.
    /// </para>
    /// <para>
    /// Physical file systems (such as a windows directory or a Zip file) are "mounted" into this object. When a physical file system is mounted, all of the file names (and other info) and directory names 
    /// will be stored in hierarchal structure similar to a Unix directory structure. Because of this, there will be some differences from the typical Windows directory setup:
    /// <list type="bullet">
    /// <item>
    ///		<description>The root of the file system is not "C:\" or "\\Computer\". In this object, the root is '/' (e.g. <c>/MyFile.txt</c> is a file located in the root).</description> 
    /// </item>
    /// <item>
    ///		<description>The directory separators are forward slashes: / and not back slashes: \. (e.g. <c>c:\MyDirectory\MySubDirectory\</c> is now <c><![CDATA[/MyDirectory/MySubDirectory/]]></c>)</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The order in which file systems are mounted into the virtual file system is important.  If a zip file contains SomeText.txt, and a directory contains the same file path, then if the zip file is 
    /// mounted, followed by the directory, the file in the directory will override the file in the zip file. This is true for directories as well. 
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// By default, a new file system instance will only have access to the directories and files of the hard drive via the default provider. File systems that are in packed files (e.g. Zip files) can be 
    /// loaded into the file system by way of a <see cref="GorgonFileSystemProvider"/>. Providers are typically plug in objects that are loaded into the file system via the <see cref="GorgonFileSystemProviderFactory"/>.  
    /// Once a provider plug in is loaded, then the contents of that file system can be mounted like a standard directory. 
    /// </para>
    /// <para>
    /// When a file system provider is added to the virtual file system object upon creation, the object will contain 2 providers, the default provider is always available with any additional providers.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonFileSystemWriter{T}"/>
    /// <seealso cref="GorgonFileSystemProvider"/>
    /// <example>
    /// This example shows how to create a file system with the default provider, and mount a directory to the root:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
    /// ]]>
    /// </code>
    /// This example shows how to load a provider from the provider factory and use it with the file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// // First we need to load the assembly with the provider plug in.
    /// using (GorgonPlugInAssemblyCache assemblies = new GorgonPlugInAssemblyCache())
    /// {
    ///		assemblies.Load(@"C:\PlugIns\GorgonFileSystem.Zip.dll"); 
    ///		GorgonPlugInService plugInService = new GorgonPlugInService(assemblies);
    /// 
    ///		// We'll use the factory to get the zip plug in provider.
    ///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(plugInService);
    ///		
    ///		IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
    /// 
    ///		// Now create the file system with the zip provider.
    ///		IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
    ///		fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/");
    /// }  
    /// ]]>
    /// </code>
    /// </example>
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
        // A list of mount points grouped by provider.
        private readonly HashSet<GorgonFileSystemMountPoint> _mountProviders;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the <see cref="IGorgonFileSystemProvider"/> installed in this file system.
        /// </summary>
        public IEnumerable<IGorgonFileSystemProvider> Providers => _providers.Select(item => item.Value);

        /// <summary>
        /// Property to return the default file system provider for this file system.
        /// </summary>
        /// <remarks>
        /// This is the default folder file system provider.
        /// </remarks>
        public IGorgonFileSystemProvider DefaultProvider
        {
            get;
        }

        /// <summary>
        /// Property to return a list of mount points that are currently assigned to this file system.
        /// </summary>
        /// <remarks>
        /// This is a list of <see cref="GorgonFileSystemMountPoint"/> values. These values contain location of the mount point in the virtual file system, the physical location of the physical file system and 
        /// the provider that mounted the physical file system.
        /// </remarks>
        public IEnumerable<GorgonFileSystemMountPoint> MountPoints
        {
            get
            {
                lock (_syncLock)
                {
                    return _mountProviders;
                }
            }
        }

        /// <summary>
        /// A read/write version of the RootDirectory property.
        /// </summary>
        internal VirtualDirectory InternalRootDirectory
        {
            get;
        }

        /// <summary>
        /// Property to return the root directory for the file system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the beginning of the directory/file structure for the file system. Users can walk through the properties on this object to get the sub directories and files for a virtual file system as a 
        /// hierarchical view.
        /// </para>
        /// <para>
        /// <note type="tip">
        /// <para>
        /// When populating a tree view, this property is useful for helping to lay out the nodes in the tree.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public IGorgonVirtualDirectory RootDirectory => InternalRootDirectory;
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
                mountDirectory = InternalRootDirectory.Directories.Add(mountPoint, mountPoint.MountLocation);
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
                    InternalRootDirectory.Directories.Add(mountPoint, directory);
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


                // Update the file information to the most recent provider.
                if (!directory.Files.TryGetValue(file.Name, out VirtualFile virtualFile))
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

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((searchMask.EndsWith("*")) || (!searchMask.StartsWith("*")) || (searchMask.Length != 1))
            {
                return Regex.IsMatch(item.Name,
                                     Regex.Escape(searchMask).Replace(@"\*", ".*").Replace(@"\?", "."),
                                     RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
#pragma warning restore IDE0046 // Convert to conditional expression
            return item.Name.EndsWith(searchMask.Substring(searchMask.IndexOf('*') + 1), StringComparison.OrdinalIgnoreCase);
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
        /// Function to return the list of <see cref="VirtualDirectory"/> objects for this file system.
        /// </summary>
        /// <param name="path">Path to iterate.</param>
        /// <param name="directoryMask">The mask to use when filtering entries.</param>
        /// <param name="recursive">true to recursively evaluate directories, false to just evaluate the top directory.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing <see cref="VirtualDirectory"/> objects.</returns>
        internal IEnumerable<VirtualDirectory> InternalFindDirectories(string path, string directoryMask, bool recursive)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
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

        /// <summary>
        /// Function to return the list of <see cref="VirtualFile"/> objects for this file system.
        /// </summary>
        /// <param name="path">Path to iterate.</param>
        /// <param name="fileMask">The mask to use when filtering entries.</param>
        /// <param name="recursive">true to recursively evaluate directories, false to just evaluate the top directory.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing <see cref="VirtualFile"/> objects.</returns>
        internal IEnumerable<VirtualFile> InternalFindFiles(string path, string fileMask, bool recursive)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
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

        /// <summary>
        /// Function to retrieve a writable <see cref="VirtualFile"/> entry from the file system.
        /// </summary>
        /// <param name="path">The path to the file entry.</param>
        /// <returns>The file entry if found, null if not.</returns>
        internal VirtualFile InternalGetFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
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

            return search == null ? null : search.Files.Contains(filename) ? search.Files[filename] : null;
        }

        /// <summary>
        /// Function to retrieve a writable <see cref="VirtualDirectory"/> entry from the file system.
        /// </summary>
        /// <param name="path">The path to the directory entry.</param>
        /// <returns>The directory entry if found, null if not.</returns>
        internal VirtualDirectory InternalGetDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(path);
            }

            path = path.FormatDirectory('/');

            if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                path = "/" + path;
            }

            // Optimization to deal with the root path.
            if (path == "/")
            {
                return InternalRootDirectory;
            }

            string[] directories = path.Split(new[]
                {
                    '/'
                }, StringSplitOptions.RemoveEmptyEntries);

            if (directories.Length == 0)
            {
                return null;
            }

            VirtualDirectory directory = InternalRootDirectory;

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
        /// Function to find all the directories with the name specified by the directory mask.
        /// </summary>
        /// <param name="path">The path to the directory to start searching from.</param>
        /// <param name="directoryMask">The directory name or mask to search for.</param>
        /// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
        /// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter are empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory in specified by the <paramref name="path"/> parameter was not found.</exception>
        /// <remarks>
        /// <para>
        /// This will look for all the directories specified by the <paramref name="directoryMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching 
        /// which can be used to find a series of directories. If the wild card is omitted, then only that name is sought out.
        /// </para> 
        /// <para>
        /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
        /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically.
        /// </para> 
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Calling <c>FindDirectories("/", "<![CDATA[/MyDir/ThisDir/C*w/]]>");</c> 
        /// will not return any results. Use the <paramref name="path"/> parameter to specify the origin for the search.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask, bool recursive = true) => InternalFindDirectories(path, directoryMask, recursive);

        /// <summary>
        /// Function to find all the directories with the name specified by the directory mask.
        /// </summary>
        /// <param name="directoryMask">The directory name or mask to search for.</param>
        /// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
        /// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryMask"/> parameter is empty.</exception>
        /// <remarks> 
        /// <para>
        /// This will look for all the directories specified by the <paramref name="directoryMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching 
        /// which can be used to find a series of directories. If the wild card is omitted, then only that name is sought out.
        /// </para> 
        /// <para>
        /// Unlike the <see cref="FindDirectories(string,string,bool)"/> overload, this method starts searching from the root of the virtual file system.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Calling <c>FindDirectories("/", "<![CDATA[/MyDir/ThisDir/C*w/]]>");</c> 
        /// will not return any results. 
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="FindDirectories(string,string,bool)"/>
        public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string directoryMask, bool recursive = true) => InternalFindDirectories("/", directoryMask, recursive);

        /// <summary>
        /// Function to find all the files with the name specified by the file mask.
        /// </summary>
        /// <param name="path">The path to the directory to start searching from.</param>
        /// <param name="fileMask">The file name or mask to search for.</param>
        /// <param name="recursive">[Optional] <b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
        /// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/> objects that match the <paramref name="fileMask"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> are empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory in specified by the <paramref name="path"/> parameter was not found.</exception>
        /// <remarks>
        /// <para>
        /// This will look for all the files specified by the <paramref name="fileMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching which can be 
        /// used to find a series of files. If the wild card is omitted, then only that name is sought out.
        /// </para> 
        /// <para>
        /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
        /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically.
        /// </para> 
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Calling <c>FindFiles("/", "<![CDATA[/MyDir/C*w.txt]]>");</c> will not return 
        /// any results. Use the <paramref name="path"/> parameter to specify the origin for the search.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask, bool recursive = true) => InternalFindFiles(path, fileMask, recursive);

        /// <summary>
        /// Function to find all the files with the name specified by the file mask.
        /// </summary>
        /// <param name="fileMask">The file name or mask to search for.</param>
        /// <param name="recursive">[Optional] <b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
        /// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/> objects that match the <paramref name="fileMask"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileMask"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileMask"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will look for all the files specified by the <paramref name="fileMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching which can be 
        /// used to find a series of files. If the wild card is omitted, then only that name is sought out.
        /// </para> 
        /// <para>
        /// Unlike the <see cref="FindFiles(string,string,bool)"/> overload, this method will start searching from the root of the file system.
        /// </para> 
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Calling <c>FindFiles("/", "<![CDATA[/MyDir/C*w.txt]]>");</c> will not return 
        /// any results. 
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public IEnumerable<IGorgonVirtualFile> FindFiles(string fileMask, bool recursive = true) => InternalFindFiles("/", fileMask, recursive);

        /// <summary>
        /// Function to retrieve a <see cref="IGorgonVirtualFile"/> from the file system.
        /// </summary>
        /// <param name="path">Path to the file to retrieve.</param>
        /// <returns>The <see cref="IGorgonVirtualFile"/> requested or <b>null</b> if the file was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when there is no file name in the <paramref name="path"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This is the primary method of accessing files from the file system. It will return a <see cref="IGorgonVirtualFile"/> object that will allow users to open a stream to the file and read its 
        /// contents. 
        /// </para>
        /// <para>
        /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
        /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically. This is also true for filenames without a directory in the path.
        /// </para> 
        /// </remarks>
        /// <example>
        /// This example will show how to read a file from the file system:
        /// <code language="csharp">
        /// <![CDATA[
        /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
        /// 
        /// fileSystem.Mount(@"C:\MyDirectory\", "/");
        /// 
        /// IGorgonVirtualFile file = fileSystem.GetFile("/ASubDirectory/MyFile.txt");
        /// 
        /// using (Stream stream = file.OpenStream())
        /// {
        ///    // Read the file from the stream...
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IGorgonVirtualFile GetFile(string path) => InternalGetFile(path);

        /// <summary>
        /// Function to retrieve a directory from the file system.
        /// </summary>
        /// <param name="path">Path to the directory to retrieve.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
        /// <returns>A <see cref="IGorgonVirtualDirectory"/> if found, <b>null</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This is the primary method of accessing directories from the file system. It will return a <see cref="IGorgonVirtualDirectory"/> object that will allow users retrieve the files or any sub 
        /// directories under that directory.
        /// </para>
        /// <para>
        /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
        /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically. 
        /// </para> 
        /// </remarks>
        /// <example>
        /// This example will show how to retrieve a directory from the file system:
        /// <code language="csharp">
        /// <![CDATA[
        /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
        /// 
        /// fileSystem.Mount(@"C:\MyDirectory\", "/");
        /// 
        /// IGorgonVirtualDirectory directory = fileSystem.GetDirectory("/ASubDirectory");
        /// 
        /// foreach(IGorgonVirtualDirectory subDirectory in directory.Directories)
        /// {
        ///		Console.WriteLine("Sub directory: {0}", subDirectory.FullPath);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IGorgonVirtualDirectory GetDirectory(string path) => InternalGetDirectory(path);

        /// <summary>
        /// Function to reload all the files and directories within, and optionally, under the specified directory.
        /// </summary> 
        /// <param name="path">The path to the directory to refresh.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the <paramref name="path"/> points to a directory that does not exist.</exception>
        /// <remarks>
        /// <para>
        /// Any files or directories sharing the same path as those in the <see cref="IGorgonFileSystemWriter{T}"/> will be restored if they were deleted. This is because the physical file systems (other 
        /// than the write area) are never changed. For example:
        /// <para>
        /// <code language="csharp">
        /// <![CDATA[
        /// // Let's assume this contains a file called "MyBudget.xls"
        /// fileSystem.Mount(@"C:\MountDirectory\", "/");
        /// 
        /// // Copy an external file into out mounted directory.
        /// File.Copy(@"C:\OtherData\MyBudget.xls", "C:\MountDirectory\Files\");
        /// 
        /// // Get the file...
        /// IGorgonVirtualFile file = fileSystem.GetFile("/Files/MyBudget.xls");
        /// 
        /// // The file does not exist yet because the file system has no idea that it's been added.
        /// if (file == null)
        /// {
        ///    Console.WriteLine("File does not exist.");
        /// }
        /// 
        /// // Now refresh the file system...
        /// fileSystem.Refresh("/Files/");
        /// 
        /// // Get the file... again.
        /// IGorgonVirtualFile file = fileSystem.GetFile("/Files/MyBudget.xls");
        /// 
        /// // The file will now show up in the directory.
        /// if (file != null)
        /// {
        ///    Console.WriteLine("File exists.");
        /// }
        /// ]]>
        /// </code>
        /// </para>
        /// </para>
        /// </remarks>
        public void Refresh(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            VirtualDirectory directory = InternalGetDirectory(path);

            if (directory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
            }

            if (path == "/")
            {
                Refresh();
                return;
            }

            directory.Directories.Clear();
            directory.Files.Clear();

            string physicalPath = directory.MountPoint.Provider.MapToPhysicalPath(path, directory.MountPoint).FormatDirectory(PhysicalDirSeparator[0]);

            if (string.Equals(physicalPath, directory.MountPoint.PhysicalPath, StringComparison.OrdinalIgnoreCase))
            {
                Refresh();
                return;
            }

            GorgonPhysicalFileSystemData fsData = directory.MountPoint.Provider.Enumerate(physicalPath, directory);

            if ((fsData.Directories.Count == 0) && (fsData.Files.Count == 0))
            {
                return;
            }

            foreach (string dirPath in fsData.Directories)
            {
                InternalRootDirectory.Directories.Add(directory.MountPoint, dirPath);
            }

            foreach (IGorgonPhysicalFileInfo file in fsData.Files)
            {
                string directoryName = Path.GetDirectoryName(file.VirtualPath).FormatDirectory('/');

                if (!directoryName.StartsWith("/"))
                {
                    directoryName += "/";
                }

                VirtualDirectory subDirectory = InternalGetDirectory(directoryName);

                if (subDirectory == null)
                {
                    throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryName));
                }

                subDirectory.Files.Add(directory.MountPoint, file);
            }
        }

        /// <summary>
        /// Function to reload all the files and directories in the file system.
        /// </summary> 
        /// <remarks>
        /// <para>
        /// This will unmount and re-mount all the known mount points for the file system, effectively rebuilding the file system file/directory tree.
        /// </para>
        /// <para>
        /// Any files or directories sharing the same path as those in the <see cref="IGorgonFileSystemWriter{T}"/> will be restored if they were deleted. This is because the physical file systems (other 
        /// than the write area) are never changed. For example:
        /// <para>
        /// <code language="csharp">
        /// <![CDATA[
        /// // Let's assume this contains a file called "MyBudget.xls"
        /// fileSystem.Mount("MyZip.zip", "/");
        /// 
        /// // And the write area also contains a file called "MyBudget.xls".
        /// writeArea.Mount();
        /// 
        /// // The "MyBudget.xls" in the write area overrides the file in the zip file since the write area mount 
        /// // is higher in the order list.
        /// //
        /// // This will now delete the MyBudget.xls from the virtual file system and from the physical write area.
        /// writeArea.Delete("MyBudget.xls");
        /// 
        /// // Now refresh the file system...
        /// fileSystem.Refresh();
        /// 
        /// // Now the file will be back, only this time it will belong to the zip file because 
        /// // the write area had its file deleted.
        /// var file = fileSystem.GetFile("MyBudget.xls");
        /// ]]>
        /// </code>
        /// </para>
        /// </para>
        /// </remarks>
        public void Refresh()
        {
            // Don't allow multiple threads to refresh this file system at the same time.
            lock (_syncLock)
            {
                // We need to copy the current mount point locations before refreshing.
                (string physicalPath, string mountLocation)[] mountPoints = _mountProviders.Select(item => (item.PhysicalPath, item.MountLocation)).ToArray();

                // Unload everything at once.
                InternalRootDirectory.Directories.Clear();
                InternalRootDirectory.Files.Clear();
                _mountProviders.Clear();

                // Refresh the mount points so we can capture the most up to date data.
                foreach ((string physicalPath, string mountLocation) in mountPoints)
                {
                    Mount(physicalPath, mountLocation);
                }
            }
        }

        /// <summary>
        /// Function to unmount the mounted virtual file system directories and files specified by the mount point.
        /// </summary>
        /// <param name="mountPoint">The mount point to unmount.</param>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="mountPoint"/> was not found in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that <paramref name="mountPoint"/>. 
        /// </para>
        /// <para>
        /// When passing the <paramref name="mountPoint"/> parameter, users should pass the return value from the <see cref="IGorgonFileSystem.Mount"/> method.
        /// </para>
        /// <para>
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Function to unmount the mounted virtual file system directories and files pointed at the by the physical path specified and mounted into the mount location specified.
        /// </summary>
        /// <param name="physicalPath">The physical file system path.</param>
        /// <param name="mountLocation">The virtual sub directory that the physical location is mounted under.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.
        /// <para>-or-</para>
        /// <para>Thrown when the mount point with the <paramref name="physicalPath"/> and <paramref name="mountLocation"/> was not found in the file system.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that mount point <paramref name="physicalPath"/> and <paramref name="mountLocation"/>. 
        /// </para>
        /// <para>
        /// Unlike the <see cref="O:Gorgon.IO.IGorgonFileSystem.Unmount">Unmount</see> overloads, this method will unmount all mount points with the specified <paramref name="physicalPath"/> and <paramref name="mountLocation"/>.
        /// </para>
        /// <para>
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
        public void Unmount(string physicalPath, string mountLocation)
        {
            // Find all mount points with the physical and virtual paths supplied.
            GorgonFileSystemMountPoint[] mountPoints = MountPoints.Where(item =>
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

        /// <summary>
        /// Function to unmount the mounted virtual file system directories and files pointed at by a physical path.
        /// </summary>
        /// <param name="physicalPath">The physical path to unmount.</param>
        /// <remarks>This overload will unmount all the mounted virtual files/directories for every mount point with the specified <paramref name="physicalPath"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the mount point with the <paramref name="physicalPath"/> was not found in the file system.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that mount point <paramref name="physicalPath"/>. 
        /// </para>
        /// <para>
        /// Unlike the <see cref="O:Gorgon.IO.IGorgonFileSystem.Unmount">Unmount</see> overloads, this method will unmount all mount points containing the path specified by the <paramref name="physicalPath"/>.
        /// </para>
        /// <para>
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
        public void Unmount(string physicalPath)
        {
            GorgonFileSystemMountPoint[] mountPoints = MountPoints.Where(item =>
                                                string.Equals(Path.GetFullPath(physicalPath),
                                                              Path.GetFullPath(item.PhysicalPath),
                                                              StringComparison.OrdinalIgnoreCase))
                                         .ToArray();

            foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
            {
                Unmount(mountPoint);
            }
        }

        /// <summary>
        /// Function to mount a physical file system into the virtual file system.
        /// </summary>
        /// <param name="physicalPath">Path to the physical file system directory or file that contains the files/directories to enumerate.</param>
        /// <param name="mountPath">[Optional] Virtual directory path to mount into.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown if mounting a directory and there is no directory in the path.</para>
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="physicalPath"/> was not found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if a file was specified by <paramref name="physicalPath"/> was not found.</exception>
        /// <exception cref="IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
        /// <returns>A mount point value for the currently mounted physical path, its mount point in the virtual file system, and the provider used to mount the physical location.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to mount the contents of a physical file system (such as a windows directory, or a file if the appropriate provider is installed) into a virtual directory in the file system. 
        /// All folders and files in the physical file system object will be made available under the virtual folder specified by the <paramref name="mountPath"/> parameter.
        /// </para>
        /// <para>
        /// The method will determine if the user is attempting to mount a physical directory or file. For directories, the <paramref name="physicalPath"/> must end with a trailing backward slash (or 
        /// forward slash depending on your native physical file system). For files, the directory should contain a file name. If mounting a file, and only the file name is supplied, then the current 
        /// directory is used to locate the file. Relative paths are supported, and will be converted into absolute paths based on the current directory.
        /// </para>
        /// <para>
        /// When files (e.g. zip files) are mounted, the appropriate provider must be loaded and installed via the constructor of this object. The providers can be loaded through a 
        /// <see cref="IGorgonFileSystemProviderFactory"/> instance.
        /// </para>
        /// <para>
        /// The <paramref name="physicalPath"/> is usually a file or a directory on the operating system file system. But in some cases, this physical location may point to somewhere completely virtual 
        /// (e.g. <see cref="GorgonFileSystemRamDiskProvider"/>). In order to mount data from those file systems, a provider-specific prefix must be prefixed to the parameter (see provider documentation 
        /// for the correct prefix). This prefix must always begin with <c>::\\</c>.
        /// </para>
        /// <para>
        /// The <paramref name="mountPath"/> parameter is optional, and if omitted, the contents of the physical file system object will be mounted into the root (<c>/</c>) of the virtual file system. If 
        /// the <paramref name="mountPath"/> is supplied, then a virtual directory is created (or used if it already exists) to host the contents of the physical file system.
        /// </para>
        /// </remarks>
        /// <example>
        /// This example shows how to create a file system with the default provider, and mount a directory to the root:
        /// <code language="csharp">
        /// <![CDATA[
        /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
        /// 
        /// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
        /// ]]>
        /// </code>
        /// This example shows how to load a provider from the provider factory and use it with the file system:
        /// <code language="csharp">
        /// <![CDATA[
        /// // First we need to load the assembly with the provider plug in.
        /// using (GorgonPlugInAssemblyCache assemblies = new GorgonPlugInAssemblyCache())
        /// {
        ///		assemblies.Load(@"C:\PlugIns\GorgonFileSystem.Zip.dll"); 
        ///		GorgonPlugInService plugInService = new GorgonPlugInService(assemblies);
        /// 
        ///		// We'll use the factory to get the zip plug in provider.
        ///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(plugInService);
        ///		
        ///		IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
        /// 
        ///		// Now create the file system with the zip provider.
        ///		IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
        ///		fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/");
        /// }  
        /// ]]>
        /// </code>
        /// </example>
        public GorgonFileSystemMountPoint Mount(string physicalPath, string mountPath = null)
        {
            if (physicalPath == null)
            {
                throw new ArgumentNullException(nameof(physicalPath));
            }

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                throw new ArgumentEmptyException(nameof(physicalPath));
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
                string directory = Path.GetDirectoryName(physicalPath).FormatDirectory(Path.DirectorySeparatorChar);

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
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="provider" /> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// To retrieve a <paramref name="provider"/>, use the <see cref="GorgonFileSystemProviderFactory.CreateProvider"/> method.
        /// </remarks>
        public GorgonFileSystem(IGorgonFileSystemProvider provider, IGorgonLog log = null)
            : this(log) => _providers[provider.GetType().FullName ?? throw new ArgumentNullException()] = provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
        /// </summary>
        /// <param name="providers">The providers available to this file system.</param>
        /// <param name="log">[Optional] The application log file.</param>
        /// <remarks>
        /// To get a list of providers to pass in, use the <see cref="GorgonFileSystemProviderFactory"/> object to create the providers.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b>.</exception>
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
                string providerTypeName = provider.GetType().FullName;

                Debug.Assert(providerTypeName != null, nameof(providerTypeName) + " != null");

                _providers[providerTypeName] = provider;
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
            _log = log ?? GorgonLog.NullLog;

            _providers = new Dictionary<string, IGorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);
            _mountProviders = new HashSet<GorgonFileSystemMountPoint>();

            DefaultProvider = new GorgonFileSystemProvider();

            InternalRootDirectory = new VirtualDirectory(default, this, null, "/");
        }
        #endregion
    }
}
