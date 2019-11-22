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
// Created: Thursday, September 24, 2015 10:30:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;
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
    public interface IGorgonFileSystem
    {
        #region Properties.

        /// <summary>
        /// Property to return the <see cref="IGorgonFileSystemProvider"/> installed in this file system.
        /// </summary>
        IEnumerable<IGorgonFileSystemProvider> Providers
        {
            get;
        }

        /// <summary>
        /// Property to return the default file system provider for this file system.
        /// </summary>
        IGorgonFileSystemProvider DefaultProvider
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
        IEnumerable<GorgonFileSystemMountPoint> MountPoints
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
        IGorgonVirtualDirectory RootDirectory
        {
            get;
        }
        #endregion

        #region Methods.
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
        IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask, bool recursive = true);

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
        IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask, bool recursive = true);

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
        IGorgonVirtualFile GetFile(string path);

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
        IGorgonVirtualDirectory GetDirectory(string path);

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
        void Refresh();

        /// <summary>
        /// Function to reload all the files and directories within, and optionally, under the specified directory.
        /// </summary> 
        /// <param name="path">The path to the directory to refresh.</param>
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
        void Refresh(string path);

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
        /// When passing the <paramref name="mountPoint"/> parameter, users should pass the return value from the <see cref="Mount"/> method.
        /// </para>
        /// <para>
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
        void Unmount(GorgonFileSystemMountPoint mountPoint);

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
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
        void Unmount(string physicalPath, string mountLocation);

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
        /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="Refresh()"/> method if they 
        /// wish to restore any file/directory entries.
        /// </para>
        /// </remarks>
        void Unmount(string physicalPath);

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
        GorgonFileSystemMountPoint Mount(string physicalPath, string mountPath = null);
        #endregion
    }
}