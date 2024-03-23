
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 17, 2013 11:07:10 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;

namespace Gorgon.Examples;

/// <summary>
/// Example entry point
/// </summary>
/// <remarks>
/// Gorgon is capable of making use of Virtual File Systems.  This is quite different from the file system type that was in the
/// first version of Gorgon which was really nothing more than a compressed file reader/writer (even though it could mount 
/// folders)
/// 
/// Virtual File Systems take a directory, or some packed data file and mount it as a root directory.  Any subsequent directories
/// and files inside of the directory (or file) are mapped to be relative to the root point on the file system.  For example,
/// mounting the D:\DataDirectory\ path as a VFS will make the path D:\DataDirectory\SubDirectory map to /SubDirectory in the
/// virtual file system.  This has to advantage of making it so your application can't go higher than the directory (or file) 
/// that was mounted as the root of the VFS.  This allows for a certain level of security to keep users from writing or reading
/// areas outside of the intended directory structure.  
/// 
/// Gorgon's VFS is modeled after the PhysFS project (http://icculus.org/physfs/)
/// 
/// In this example, we'll mount a zip file as the root of the virtual file system.  All sub directories and files under the root 
/// directory will become accessible from the virtual file system.  The program will enumerate the directories and files and list 
/// them in the console window with relevant file information
/// </remarks>
internal static class Program
{

    private const string PlugInName = "Gorgon.IO.Zip.ZipProvider";

    // The plugin assemblies.
    private static GorgonMefPlugInCache _pluginAssemblies;
    // File system.
    private static GorgonFileSystem _fileSystem;
    // The log file used for debug logging.
    private static IGorgonLog _log;

    /// <summary>
    /// Function to retrieve the directory that contains the plugins for an application.
    /// </summary>
    /// <param name="pluginDirectory">The directory containing the plug ins.</param>
    /// <returns>A directory information object for the plugin path.</returns>
    private static DirectoryInfo GetPlugInPath(DirectoryInfo pluginDirectory)
    {
        string path = pluginDirectory.FullName;

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new IOException("No plug in path has been assigned.");
        }

        if (path.Contains("{0}"))
        {
#if DEBUG
            path = string.Format(path, "Debug");
#else
            path = string.Format(path, "Release");					
#endif
        }

        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            path += Path.DirectorySeparatorChar.ToString();
        }

        return new DirectoryInfo(Path.GetFullPath(path));
    }

    /// <summary>
    /// Function to load the zip file provider plugin.
    /// </summary>
    /// <param name="pluginDirectory">The directory containing the plug ins.</param>
    /// <returns><b>true</b> if successfully loaded, <b>false</b> if not.</returns>
    private static bool LoadZipProviderPlugIn(DirectoryInfo pluginDirectory)
    {
        FileInfo zipProviderFile = new(Path.Combine(GetPlugInPath(pluginDirectory).FullName.FormatDirectory(Path.DirectorySeparatorChar), "Gorgon.FileSystem.Zip.dll"));

        // Check to see if the file exists.
        if (!zipProviderFile.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not find the plugin assembly file:\n'{0}'.", zipProviderFile.FullName);
            Console.ResetColor();
#if DEBUG
            Console.ReadKey();
#endif
            return false;
        }

        // Create our file system provider factory so we can retrieve the zip file provider.
        GorgonFileSystemProviderFactory providerFactory = new(_pluginAssemblies, _log);

        // Get our zip file provider.
        GorgonFileSystemProvider provider;

        try
        {
            provider = providerFactory.CreateProvider(zipProviderFile.FullName, PlugInName);
        }
        catch (GorgonException gEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(gEx.Message);
            Console.ResetColor();
#if DEBUG
            Console.ReadKey();
#endif
            return false;
        }

        _fileSystem = new GorgonFileSystem(provider, _log);

        Console.WriteLine("\nThe zip file file system provider was loaded successfully.");
        return true;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main()
    {
        DirectoryInfo resourceBaseDirectory = new(Path.Combine(ExampleConfig.Default.ResourceLocation, "FileSystems", "FileSystem.zip"));
        DirectoryInfo plugInLocationDirectory = new(ExampleConfig.Default.PlugInLocation);

        _log = new GorgonTextFileLog("ZipFileSystem", "Tape_Worm");
        _log.LogStart();

        // Create the plugin assembly cache.
        _pluginAssemblies = new GorgonMefPlugInCache(_log);

        try
        {
            Console.WindowHeight = 28;
            Console.BufferHeight = Console.WindowHeight;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("This example will mount a zip file as the root of a virtual file system.  The");
            Console.WriteLine("virtual file system is capable of mounting various types of data such as a zip,");
            Console.WriteLine("a file system folder, etc... as the root or a sub directory.  You can even");
            Console.WriteLine("mount a zip file as the root, and a physical file system directory as a virtual");
            Console.WriteLine("sub directory in the same virtual file system and access them as a single");
            Console.WriteLine("unified file system.");

            // Unlike the folder file system example, we need to load
            // a provider to handle zip files before trying to mount
            // one.
            if (!LoadZipProviderPlugIn(plugInLocationDirectory))
            {
                return;
            }

            // Set the following zip file as root on the virtual file system.
            //
            // If we wanted to, we could mount a zip file as a sub directory of
            // the root of the virtual file system.  For example, mounting the
            // directory D:\Dir\zipFile.zip with Mount(@"D:\Dir", "/VFSDir"); would mount 
            // the contents of the D:\Dir\zipFile.zip directory under /VFSDir.
            //
            // It's also important to point out that the old Gorgon "file system"
            // would load files from the system into memory when mounting a 
            // directory.  While this version only loads directory and file 
            // information when mounting.  This is considerably more efficient.
            _fileSystem.Mount(resourceBaseDirectory.FullName);

            Console.Write("\nMounted: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("'{0}'", resourceBaseDirectory.FullName.Ellipses(Console.WindowWidth - 20, true));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" as ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("'/'\n");
            Console.ForegroundColor = ConsoleColor.White;

            // Get a count of all sub directories and files under the root directory.
            IGorgonVirtualDirectory[] directoryList = _fileSystem.FindDirectories("*").ToArray();

            // Display directories.
            Console.WriteLine("Virtual file system contents:");

            for (int i = -1; i < directoryList.Length; i++)
            {
                IGorgonVirtualDirectory directory = _fileSystem.RootDirectory;

                // Go into the sub directories under root.
                if (i > -1)
                {
                    directory = directoryList[i];
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("{0}", directory.FullPath);

                Console.ForegroundColor = ConsoleColor.Yellow;

                foreach (IGorgonVirtualFile file in directory.Files)
                {
                    Console.Write("   {0}", file.Name);
                    // Align the size to the same place.
                    Console.CursorLeft = 65;
                    Console.WriteLine("{0}", file.Size.FormatMemory());
                }
            }

            Console.ResetColor();
            Console.WriteLine("\nPress any key to close.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            ex.Handle(e =>
                     {
                         Console.Clear();
                         Console.ForegroundColor = ConsoleColor.Red;
                         Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", e.Message, e.StackTrace);

                         Console.ResetColor();
#if DEBUG
                         Console.ReadKey();
#endif
                     });
        }
        finally
        {
            // Always dispose the cache to clean up the temporary app domain it creates.
            _pluginAssemblies.Dispose();
            _log.LogEnd();
        }
    }

}