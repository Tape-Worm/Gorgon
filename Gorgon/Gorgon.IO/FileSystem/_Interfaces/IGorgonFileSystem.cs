
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, September 24, 2015 10:30:17 PM
// 

using Gorgon.Core;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.IO.FileSystem;

/// <summary>
/// The status of a file when opened as a stream.
/// </summary>
public enum FileStreamStatus
{
    /// <summary>
    /// The file was opened as a read only stream.
    /// </summary>
    ReadOnly = 0,
    /// <summary>
    /// The file did not exist, and was created as a new file.
    /// </summary>
    NewFile = 1,
    /// <summary>
    /// The file exists, and was opened as a writeable stream.
    /// </summary>
    Updated = 2
}

/// <summary>
/// The virtual file System interface
/// </summary>
/// <remarks>
/// <para>
/// This will allow the user to mount directories or packed files (such as Zip files) into a unified file system.  For example, if the user has mounted MyData.zip and C:\users\Bob\Data\ into the file 
/// system then all files and/or directories from both sources would be combined into a single virtual file system. This has the advantage of being able to access disparate file systems without having 
/// to run through multiple interfaces to get at their data
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
/// The file system is read only until a writeable area (directory) is mounted. Once a writeable area is mounted, then the file system is considered read/write.  This is useful for scenarios where the file 
/// system needs to store data. For example, a game may have a zip file containing all the game data, and a directory for save data. The zip file would be mounted as read only, and the save data directory 
/// can be used store save game data on the hard drive in some arbitrary place, but still accessible from the same virtual file system.
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
/// loaded into the file system by way of a <see cref="GorgonFileSystemProvider"/>. Providers are typically plugin objects loaded via the <c>Gorgon.IO.FileSystem.Plugins</c> API.
/// Once a provider plugin is loaded, then the contents of that file system can be mounted like a standard directory. 
/// </para>
/// <para>
/// When a file system provider is added to the virtual file system object upon creation, the object will contain 2 providers, the default provider is always available with any additional providers
/// </para>
/// </remarks>
/// <seealso cref="GorgonFileSystemProvider"/>
/// <example>
/// This example shows how to create a file system with the default provider, and mount a directory to the root:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
/// 
/// fileSystem.Mount(@"C:\MyDirectory\"); 
/// ]]>
/// </code>
/// This example shows how to load a provider from the provider factory and use it with the file system:
/// <code language="csharp">
/// <![CDATA[
/// // First we need to load the assembly with the provider plugin
/// using GorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache();
/// 
///	assemblies.Load(@"C:\Plugins\GorgonFileSystem.Zip.dll"); 
///	GorgonPluginService PluginService = new GorgonPluginService(assemblies);
/// 
///	// We'll use the factory to get the zip plugin provider
///	IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(PluginService);
///		
///	IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
/// 
///	// Now create the file system with the zip provider
///	IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
///	fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/");
///   
/// ]]>
/// </code>
/// </example>
public interface IGorgonFileSystem
{
    /// <summary>
    /// Event triggered when a virtual directory has been added to the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryCreatedEventArgs>? VirtualDirectoryCreated;

    /// <summary>
    /// Event triggered when a virtual directory has been deleted from the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryDeletedEventArgs>? VirtualDirectoryDeleted;

    /// <summary>
    /// Event triggered when a virtual directory has been renamed in the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryRenamedEventArgs> VirtualDirectoryRenamed;

    /// <summary>
    /// Event triggered when a virtual directory has been copied in the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryCopied;

    /// <summary>
    /// Event triggered when a virtual directory has been moved in the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryMoved;

    /// <summary>
    /// Event triggered when a virtual file has been deleted from the file system.
    /// </summary>
    event EventHandler<VirtualFileDeletedEventArgs>? VirtualFileDeleted;

    /// <summary>
    /// Event triggered when a virtual file has been copied in the file system.
    /// </summary>
    event EventHandler<VirtualFileCopiedMovedEventArgs>? VirtualFileCopied;

    /// <summary>
    /// Event triggered when a virtual file has been moved in the file system.
    /// </summary>
    event EventHandler<VirtualFileCopiedMovedEventArgs>? VirtualFileMoved;

    /// <summary>
    /// Event triggered when a virtual file has been renamed in the file system.
    /// </summary>
    event EventHandler<VirtualFileRenamedEventArgs> VirtualFileRenamed;

    /// <summary>
    /// Event triggered when a virtual file has been opened for reading or writing in the file system.
    /// </summary>
    event EventHandler<VirtualFileOpenedEventArgs> VirtualFileOpened;

    /// <summary>
    /// Property to return whether the file system is in a read only state.
    /// </summary>
    /// <remarks>
    /// If this value returns <b>true</b>, then no call to <see cref="MountWriteArea(string)"/> has been called. Otherwise, a physical directory has been mounted as a writable filesystem area.
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    bool IsReadOnly
    {
        get;
    }

    /// <summary>
    /// Property to return the default provider for the file system.
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
    IReadOnlyList<GorgonFileSystemMountPoint> MountPoints
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

    /// <summary>
    /// Function to find all the directories with the name specified by the directory mask.
    /// </summary>
    /// <param name="path">The path to the directory to start searching from.</param>
    /// <param name="directoryMask">[Optional] The directory name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
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
    /// If the <paramref name="directoryMask"/> is omitted, then all directories will be returned. Otherwise only directories that match the following masks will be returned:
    /// <list type="bullet">
    /// <item>
    ///     <term>*test</term>
    ///     <description>Matches all directory names ending with "test".</description>
    /// </item>
    /// <item>
    ///     <term>test*</term>
    ///     <description>Matches all directory names starting with "test".</description>
    /// </item>
    /// <item>
    ///     <term>test*pattern</term>
    ///     <description>Matches all directory names starting with "test" and ending with "pattern".</description>
    /// </item>
    /// <item>
    ///     <term>*test*</term>
    ///     <description>Matches all directory names ending with test in the middle of the name.</description>
    /// </item>
    /// </list>
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
    IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask = "*", bool recursive = true);

    /// <summary>
    /// Function to find all the files with the name specified by the file mask.
    /// </summary>
    /// <param name="path">The path to the directory to start searching from.</param>
    /// <param name="fileMask">[Optional] The file name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/> objects that match the <paramref name="fileMask"/>.</returns>
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
    /// If the <paramref name="fileMask"/> is omitted, then all files will be returned. Otherwise only files that match the following masks will be returned:
    /// <list type="bullet">
    /// <item>
    ///     <term>*test</term>
    ///     <description>Matches all file names ending with "test".</description>
    /// </item>
    /// <item>
    ///     <term>test*</term>
    ///     <description>Matches all file names starting with "test".</description>
    /// </item>
    /// <item>
    ///     <term>test*pattern</term>
    ///     <description>Matches all file names starting with "test" and ending with "pattern".</description>
    /// </item>
    /// <item>
    ///     <term>*test*</term>
    ///     <description>Matches all file names ending with test in the middle of the name.</description>
    /// </item>
    /// </list>
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
    IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask = "*", bool recursive = true);

    /// <summary>
    /// Function to retrieve a <see cref="IGorgonVirtualFile"/> from the file system.
    /// </summary>
    /// <param name="path">Path to the file to retrieve.</param>
    /// <returns>The <see cref="IGorgonVirtualFile"/> requested or <b>null</b> if the file was not found.</returns>
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
    IGorgonVirtualFile? GetFile(string path);

    /// <summary>
    /// Function to retrieve a directory from the file system.
    /// </summary>
    /// <param name="path">Path to the directory to retrieve.</param>
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
    IGorgonVirtualDirectory? GetDirectory(string path);

    /// <summary>
    /// Function to reload all the files and directories in the file system.
    /// </summary> 
    /// <remarks>
    /// <para>
    /// This will unmount and re-mount all the known mount points for the file system, effectively rebuilding the file system file/directory tree.
    /// </para>
    /// <para>
    /// Any files or directories sharing the same virtual path, and if the physical location is different than those in the writeable mount point area will be restored if they were deleted. 
    /// <para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Let's assume this contains a file called "MyBudget.xls"
    /// fileSystem.Mount("MyZip.zip", "/");
    /// 
    /// // Assume the write area also contains a file called "MyBudget.xls".
    /// fileSystem.MountWriteDirectory(@"d:\my_write_area\");
    /// 
    /// // The "MyBudget.xls" in the write area overrides the file in the zip file since the write area mount 
    /// // is higher in the order list.
    /// //
    /// // This will now delete the MyBudget.xls from the virtual file system and from the physical write area.
    /// fileSystem.Delete("MyBudget.xls");
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
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the <paramref name="path"/> points to a directory that does not exist.</exception>
    /// <remarks>
    /// <para>
    /// Any files or directories sharing the same virtual path, and if the physical location is different than those in the writeable mount point area  will be restored if they were deleted.
    /// <para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Let's assume this contains a file called "MyBudget.xls"
    /// fileSystem.Mount(@"C:\MountDirectory\", "/");
    /// 
    /// // Assume the write area also contains a file called "MyBudget.xls" in the "/Files/" directory.
    /// fileSystem.MountWriteDirectory(@"d:\my_write_area\");
    /// 
    /// // The "MyBudget.xls" in the write area overrides the file in the physical folder since the write area mount 
    /// // is higher in the order list.
    /// //
    /// // This will now delete the MyBudget.xls from the virtual file system and from the physical write area.
    /// fileSystem.Delete("/Files/MyBudget.xls");
    /// 
    /// // Now refresh the file system...
    /// fileSystem.Refresh("/Files/");
    /// 
    /// // Get the file... again.
    /// IGorgonVirtualFile file = fileSystem.GetFile("/Files/MyBudget.xls");
    /// 
    /// // The file will now show up in the directory.
    /// if (file is not null)
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
    /// Function to unmount all mounted physical file systems from the virtual file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this method is called, the file system will be completely cleared of all mount points, directories and files. This will effectively remove all physical file systems from the virtual file 
    /// system, however no physical files/directories will be removed.
    /// </para>
    /// </remarks>
    void Unmount();

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
    /// Function to mount a physical file system into the virtual file system.
    /// </summary>
    /// <param name="physicalPath">Path to the physical file system directory or file that contains the files/directories to enumerate.</param>
    /// <param name="provider">[Optionl] The provider used to retrieve the physical file system data.</param>
    /// <param name="mountPath">[Optional] Virtual directory path to mount into.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.
    /// <para>-or-</para>
    /// <para>Thrown if mounting a directory and there is no directory in the path.</para>
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="physicalPath"/> was not found.</exception>
    /// <exception cref="FileNotFoundException">Thrown if a file was specified by <paramref name="physicalPath"/> was not found.</exception>    
    /// <exception cref="IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by the specified <paramref name="provider"/>.</exception>
    /// <returns>A mount point value for the currently mounted physical path, its mount point in the virtual file system, and the provider used to mount the physical location.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to mount the contents of a physical file system (such as a windows directory, or a file if the appropriate provider is installed) into a virtual directory in the file system. 
    /// All folders and files in the physical file system object will be made available under the virtual folder specified by the <paramref name="mountPath"/> parameter.
    /// </para>
    /// <para>
    /// If specified, the <paramref name="provider"/> parameter is used to retrieve the file system data from the physical source. If the provider is omitted, then the default folder file system provider is 
    /// used to mount the physical file system. If the provider cannot support the <paramref name="physicalPath"/> then an exception will be thrown. Providers can be loaded via the 
    /// <c>Gorgon.IO.FileSystem.Plugins</c> interface.
    /// </para>
    /// <para>
    /// If the <paramref name="provider"/> expects a file (e.g. a zip file), then the <paramref name="physicalPath"/> must point to a file. If the <paramref name="provider"/> expects a directory, then the 
    /// <paramref name="physicalPath"/> should point to a directory name. Relative paths are supported, and will be converted into absolute paths based on the current directory.
    /// </para>
    /// <para>
    /// The <paramref name="mountPath"/> parameter is optional, and if omitted, the contents of the physical file system object will be mounted into the root (<c>/</c>) of the virtual file system. If 
    /// the <paramref name="mountPath"/> is supplied, then a virtual directory is created (or used if it already exists) to host the contents of the physical file system.
    /// </para>
    /// <para>
    /// Every physical file system mounted into the virtual file system will be mounted as read only. If an area is required for writing, then the <see cref="MountWriteArea(string)"/> method should be 
    /// used instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to create a file system with the default provider, and mount a directory to the root:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\"); 
    /// ]]>
    /// </code>
    /// This example shows how to load a provider from the provider factory and use it with the file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// // First we need to load the assembly with the provider plugin.
    /// using (GorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache())
    /// {
    ///		assemblies.Load(@"C:\Plugins\GorgonFileSystem.Zip.dll"); 
    ///		GorgonPluginService PluginService = new GorgonPluginService(assemblies);
    /// 
    ///		// We'll use the factory to get the zip plugin provider.
    ///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(PluginService);
    ///		
    ///		IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
    /// 
    ///		// Now create the file system with the zip provider.
    ///		IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
    ///		fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/", zipProvider);
    /// }  
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="MountWriteArea(string)"/>
    GorgonFileSystemMountPoint Mount(string physicalPath, string? mountPath = null, IGorgonFileSystemProvider? provider = null);

    /// <summary>
    /// Function to mount a directory on the physical file system to be used as an area that applications can write into.
    /// </summary>
    /// <param name="physicalPath">Path to the physical file system directory the writable files/directories.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="physicalPath"/> directory could not be read, or written into.</exception>
    /// <returns>A mount point value for the mounted write area.</returns>
    /// <remarks>
    /// <para>
    /// Like the <see cref="Mount"/> method, this method will mount a directory on the physical file system into the root of the file system. But, unlike the <see cref="Mount"/> method, which mounts a file 
    /// or directory as a read only file system, this method will mount the directory as read-write. This means applications can create, delete, or update files, and directories within the file system. When 
    /// an application writes to the file system, the physical location will actually contain the written data. 
    /// </para>
    /// <para>
    /// All data written to this file system will supercede files and directories mounted by other providers. Meaning that a file with the same virtual file system path (e.g. '/Dir/File.txt') in another 
    /// physical file system will be replaced with the created/deleted/updated one within the file system structure with the file with the same path on the physical write area. If the writable mount point 
    /// is unmounted, or the filesystem is refreshed, then any files deleted that exist in other read-only file systems (those mounted with <see cref="Mount"/>) will reappear. This means that the changes 
    /// made to the file system will never modify files in other file systems (with the exception of directories mounted from the physical file system as a writable area).
    /// </para>
    /// <para>
    /// Only one writable file mount point can exist at a time. If an application mounts another directory, the previous mounted directory will be unmounted.
    /// </para>
    /// <para>
    /// If the directory specified by the <paramref name="physicalPath"/> parameter does not exist, then it will be created.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to create a file system with the default provider, and mount a directory to the root, and mount an area to write to into the root of the virtual file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
    /// // This will mount the contents of the "C:\users\Username\Temp\MyWorkspace\" 
    /// // directory into the root of the file system.
    /// fileSystem.MountWriteArea(@"C:\users\Username\Temp\MyWorkspace\");
    /// 
    /// // This will create a directory under the root called "AnotherDirectory".
    /// // This will actually be "C:\users\Username\Temp\MyWorkspace\AnotherDirectory" 
    /// // on the physical file system.
    /// fileSystem.CreateDirectory("/AnotherDirectory/");
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="Mount"/>
    GorgonFileSystemMountPoint MountWriteArea(string physicalPath);

    /// <summary>
    /// Function to open a file for reading or for writing.
    /// </summary>
    /// <param name="path">The path to the file to read/write.</param>
    /// <param name="write"><b>true</b> to open the file for writing, or <b>false</b> to open for reading.</param>
    /// <param name="closeCallback">[Optional] A method that is called when the resulting stream is closed.</param>
    /// <returns>An open <see cref="Stream"/> to the file.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown when the file system is in read only mode and the <paramref name="write"/> parameter is set to <b>true</b>.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file referenced by the <paramref name="path"/> was not found and the <paramref name="write"/> parameter is set to <b>false</b>.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="path"/> points to a directory, and the <paramref name="write"/> parameter is set to <b>false</b>.</para>
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> was not found.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="write"/> parameter is <b>true</b> and the <paramref name="path"/> points to a directory.</exception>
    /// <remarks>
    /// <para>
    /// This will open a file for reading or writing depending on the value passed to the <paramref name="write"/> parameter. 
    /// </para>
    /// <para>
    /// When a file is opened for writing, it will be opened and truncated to 0 bytes and the resulting stream cannot be read. Otherwise, when a file is opened for reading, the resulting stream cannot be 
    /// written into.
    /// </para>
    /// <para>
    /// If the <see cref="IsReadOnly"/> property returns <b>true</b> and the <paramref name="write"/> parameter is set to <b>true</b>, then an exception will be thrown.
    /// </para>
    /// <para>
    /// When the <paramref name="closeCallback"/> parameter is defined, then the method passed will be called when the resulting <see cref="Stream"/> is closed or disposed. The callback method 
    /// passes the <see cref="IGorgonVirtualFile"/> that was opened, and a flag indicating whether the file was created as a new file.
    /// </para>
    /// <para>
    /// If the path to the file is actually a path to a directory, then a <see cref="FileNotFoundException"/> will be thrown when <paramref name="write"/> is <b>false</b>, and a <see cref="IOException"/> 
    /// will be thrown when <paramref name="write"/> is <b>true</b>.
    /// </para>
    /// <para>
    /// <example>
    /// This is an example of how to open a file for reading:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\");
    /// 
    /// using Stream dataStream = fileSystem.OpenStream("/SubDir/File.txt", false);
    /// 
    /// byte[] data = new byte[dataStream.Length];
    /// dataStream.Read(data.AsSpan());
    /// 
    /// dataStream.Close();
    /// 
    /// // You can also have a callback method that is called when the stream is closed.
    /// void CloseCallback(IGorgonVirtualFile file, bool write)
    /// {
    ///     string text = Encoding.UTF8.GetString(data);
    ///     Console.WriteLine(text);
    /// }
    /// 
    /// using Stream dataStream2 = fileSystem.OpenStream("/SubDir/File.txt", false, CloseCallback);
    /// 
    /// data = new byte[dataStream2.Length];
    /// dataStream2.Read(data.AsSpan());
    /// 
    /// dataStream.Close();
    /// ]]>
    /// </code>
    /// </example>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonFileSystemStream"/>
    GorgonFileSystemStream OpenStream(string path, bool write, Action<IGorgonVirtualFile, FileStreamStatus>? closeCallback = null);

    /// <summary>
    /// Function to create a new directory in the writable area on the physical file system.
    /// </summary>
    /// <param name="path">Path to the directory (or directories) to create.</param>    
    /// <returns>The <see cref="IGorgonVirtualDirectory"/> for the created directory.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.</exception>
    /// <exception cref="IOException">Thrown when a part of the <paramref name="path"/> has the same name as a file name in the parent of the directory being created.</exception>
    /// <remarks>
    /// <para>
    /// This will create a new directory within the physical writable directory mounted with <see cref="MountWriteArea"/>. If the <paramref name="path"/> contains multiple directories that don't exist 
    /// (e.g. <c><![CDATA[/Exists/AlsoExists/DoesNotExist/DoesNotExistEither/]]></c>), then those directories will be created until the path is completely parsed. The file system will be updated to ensure 
    /// that those directories will exist and can be referenced.
    /// </para>
    /// <para>
    /// If the directory path contains a name that is the same as a file name within a directory (e.g. <c><![CDATA[/MyDirectory/SomeFile.txt/AnotherDirectory]]></c>, where <c>SomeFile.txt</c> already exists 
    /// as a file under <c>MyDirectory</c>), then an exception will be thrown.
    /// </para>
    /// <para>
    /// If the directory already exists (either in the <see cref="IGorgonFileSystem"/> or on the physical file system), then nothing will be done and the existing directory will be returned from the method.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea"/>
    IGorgonVirtualDirectory CreateDirectory(string path);

    /// <summary>
    /// Function to delete a directory from the file system.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    /// <param name="options">[Optional] The options to pass to the delete operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="path"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="path"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will delete a directory, its subdirectories, and any files under this directory and subdirectories. It will remove all references to those files and directories from the file system, and will 
    /// delete these items from the writable physical directory if they exist in that location.
    /// </para>
    /// <para>
    /// If the <paramref name="path"/> is set to the root of the file system (<c>/</c>), then the entire file system will be deleted, but the root directory will always remain.
    /// </para>
    /// <para>
    /// If the <paramref name="options"/> parameter is supplied, then users can supply functionality to report progress, and cancel the operation.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the directory specified in <paramref name="path"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist in the 
    /// write area can be deleted.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryDeleted"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonFileSystemDeleteOptions"/>
    /// <seealso cref="MountWriteArea(string)"/>
    void DeleteDirectory(string path, GorgonFileSystemDeleteOptions? options = null);

    /// <summary>
    /// Function to rename a directory on the physical file system.
    /// </summary>
    /// <param name="path">The original path to the directory to rename.</param>
    /// <param name="newName">The new name for the directory.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/>, or the <paramref name="newName"/> parameters are empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="path"/> was not found on the file system.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="path"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown if the <paramref name="path"/> points to the root directory.
    /// <para>-or-</para>
    /// <para>Thrown if the new directory is the same as the <paramref name="path"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the the new directory name already exists.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will change the name of the directory specified by <paramref name="path"/>. The name provided by <paramref name="newName"/> must be a valid directory name.
    /// </para>
    /// <para>
    /// If the <paramref name="newName"/> is the same as an already existing directory in the parent directory of the <paramref name="path"/> directory, then an exception will be thrown.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the directory specified in <paramref name="path"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist in the 
    /// write area can be renamed.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryRenamed"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    void RenameDirectory(string path, string newName);

    /// <summary>
    /// Function to copy a directory in the file system to another location.
    /// </summary>
    /// <param name="sourcePath">The path of the directory to copy.</param>
    /// <param name="destinationPath">The destination path for the copied directory.</param>
    /// <param name="options">[Optional] The options to pass to the copy operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.</exception>
    /// <exception cref="IOException">Thrown if the source and destination paths are the same.
    /// <para>-or-</para>
    /// <para>Thrown if the destination is a child of the source directory.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will copy a directory, its subdirectories, and any files under this directory and subdirectories to another directory on the file system. For example, copying <c>/MyDir/MyChildDir/</c> to 
    /// <c>/MyOtherDir/</c> will create a directory under <c>MyOtherDir</c> called <c>MyOtherDir/MyChildDir/</c>, and all child directories and files under <c>MyChildDir</c> will be copied.
    /// </para>
    /// <para>
    /// When the directory is copied, it will be copied to the writable area as specified with <see cref="MountWriteArea(string)"/>. All files and directories in the writable area will override files 
    /// with the same mount path imported from other physical file systems.
    /// </para>
    /// <para>
    /// If the root of the file system is specified as the <paramref name="sourcePath"/>, then an exception will be thrown. Directories cannot be copied into their child directories, this includes 
    /// the root.
    /// </para>
    /// <para>
    /// If the <paramref name="options"/> parameter is supplied, then users can supply functionality to report progress, and cancel the operation.
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    /// <seealso cref="GorgonFileSystemCopyOptions"/>
    void CopyDirectory(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null);

    /// <summary>
    /// Function to move a directory in the file system to another location.
    /// </summary>
    /// <param name="sourcePath">The path of the directory to move.</param>
    /// <param name="destinationPath">The destination path for the moved directory.</param>
    /// <param name="options">[Optional] The options to pass to the move operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="sourcePath"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown if the source and destination paths are the same.
    /// <para>-or-</para>
    /// <para>Thrown if the destination is a child of the source directory.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will move a directory, its subdirectories, and any files under this directory and subdirectories to another directory on the file system. For example, moving <c>/MyDir/MyChildDir/</c> to 
    /// <c>/MyOtherDir/</c> will create a directory under <c>MyOtherDir</c> called <c>MyOtherDir/MyChildDir/</c>, and all child directories and files under <c>MyChildDir</c> will be copied and the source 
    /// directory, and all of it's files and subdirectories will be removed.
    /// </para>
    /// <para>
    /// If the root of the file system is specified as the <paramref name="sourcePath"/>, then an exception will be thrown. Directories cannot be moved into their child directories, this includes 
    /// the root.
    /// </para>
    /// <para>
    /// If the <paramref name="options"/> parameter is supplied, then users can supply functionality to report progress, and cancel the operation.
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the directory specified in <paramref name="sourcePath"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist 
    /// in the write area can be moved. 
    /// </para>
    /// <para>
    /// This does not apply to the <paramref name="destinationPath"/> because this method will create the directory in the writeable area if it does not exist.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    /// <seealso cref="GorgonFileSystemCopyOptions"/>
    void MoveDirectory(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null);

    /// <summary>
    /// Function to move a file from one location to another.
    /// </summary>
    /// <param name="sourcePath">The path of the file to move.</param>
    /// <param name="destinationPath">The destination path for the moved file.</param>
    /// <param name="options">[Optional] The options to pass to the move operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file specified by the <paramref name="sourcePath"/> could not be found.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="destinationPath"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="sourcePath"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown if the source and destination paths are the same.</exception>
    /// <remarks>
    /// <para>
    /// This will move a file, to another directory on the file system.
    /// </para>
    /// <para>
    /// If the <paramref name="options"/> parameter is supplied, then users can supply functionality to report progress, and cancel the operation.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the file specified in <paramref name="sourcePath"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist in the 
    /// write area can be moved.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    /// <seealso cref="GorgonFileSystemCopyOptions"/>
    void MoveFile(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null);

    /// <summary>
    /// Function to delete an individual file from file system.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file specified by the <paramref name="path"/> could not be found.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified in the <paramref name="path"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="path"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will delete a file specified by the <paramref name="path"/>. It will remove the files from the file system, and will physically delete these file from the writable physical directory if it 
    /// exists in that location.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the file specified in <paramref name="path"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist in the 
    /// write area can be deleted.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileDeleted"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    void DeleteFile(string path);

    /// <summary>
    /// Function to copy a file from one location to another.
    /// </summary>
    /// <param name="sourcePath">The path to the file to copy.</param>
    /// <param name="destinationPath">The destination path and file name for the copied file.</param>
    /// <param name="options">[Optional] Options to pass to the file copy operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourcePath"/>, or the <paramref name="destinationPath"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.</exception>
    /// <exception cref="IOException">
    /// <para>Thrown if the source and destination paths are the same.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the file copy operation failed during the process.</para>
    /// <para>-or-</para>
    /// <para>The <paramref name="options"/> contains a <see cref="GorgonFileSystemCopyOptions.ConflictResolutionCallback"/> that returns the <see cref="FileConflictResolution.Exception"/> value.</para>
    /// </exception>
    /// <exception cref="FileNotFoundException">Thrown when the source file was not found in the file system.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the source file, or destination file directory was not found in the file system.</exception>
    /// <remarks>
    /// <para>
    /// This will copy a file in the file system from one location to another.
    /// </para>
    /// <para>
    /// If the <paramref name="destinationPath"/> does not contain a file name, then the file name from the <paramref name="sourcePath"/> will be used.
    /// </para>
    /// <para>
    /// When the file is copied, it will be copied to the writable area (specified with <see cref="MountWriteArea(string)"/>). If a file with the same path exists in another mounted file system that is 
    /// read-only (e.g. a zip file), then it will override the read-only file, and all file operations will be done on the file in the writable area. 
    /// </para>
    /// <para>
    /// Applications can specify the <paramref name="options"/> parameter to provide functionality to report progress, handle file conflicts, and cancel the operation. When handling file conflicts (i.e. 
    /// the destination file exists), the user can specify whether to overwrite, rename, skip the file or cancel the operation or throw an exception via the 
    /// <see cref="GorgonFileSystemCopyOptions.ConflictResolutionCallback"/>. 
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    /// <seealso cref="GorgonFileSystemCopyOptions"/>
    void CopyFile(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null);

    /// <summary>
    /// Function to rename a file in the file system.
    /// </summary>
    /// <param name="path">The path to the file to rename.</param>
    /// <param name="newName">The new name for the file.</param>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.
    /// <para>-or-</para>
    /// <para>The <paramref name="path"/> belongs to a read only file system provider.</para>
    /// </exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/>, or the <paramref name="newName"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file pointed at by <paramref name="path"/> was not found on the file system.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="path"/> contains a directory that does not exist.</exception>
    /// <exception cref="IOException">Thrown if the new file name already exists in the directory in the <paramref name="path"/>.</exception>
    /// <remarks>
    /// <para>
    /// This will change the filename of the file specified by <paramref name="path"/>. The name provided by <paramref name="newName"/> must be a valid file name (with or without an extension). 
    /// </para>
    /// <para>
    /// If the file being renamed is in the writable area, then the file will be renamed in the physical file system as well as the virtual. If it is not, then the original file will be copied to the 
    /// writable area with the new name, and supersede the original file. In this case, a rename will be considerably slower then renaming a file already in the writeable area.
    /// </para>
    /// <para>
    /// If the <paramref name="newName"/> is the same as an already existing file in the <paramref name="path"/> directory, then an exception will be thrown.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// If the file specified in <paramref name="path"/> is not in the writeable area specified by <see cref="MountWriteArea(string)"/>, then an exception will be thrown. Only paths that exist in the 
    /// write area can be renamed.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileRenamedEventArgs"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountWriteArea(string)"/>
    void RenameFile(string path, string newName);
}
