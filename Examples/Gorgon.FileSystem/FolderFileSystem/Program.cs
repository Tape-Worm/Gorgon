#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, January 5, 2013 3:29:58 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Example entry point.
/// </summary>
/// <remarks>
/// Gorgon is capable of making use of Virtual File Systems.  This is quite different from the file system type that was in the
/// first version of Gorgon which was really nothing more than a compressed file reader/writer (even though it could mount 
/// folders).
/// 
/// Virtual File Systems take a directory, or some packed data file and mount it as a root directory.  Any subsequent directories
/// and files inside of the directory (or file) are mapped to be relative to the root point on the file system.  For example,
/// mounting the D:\DataDirectory\ path as a VFS will make the path D:\DataDirectory\SubDirectory map to /SubDirectory in the
/// virtual file system.  This has to advantage of making it so your application can't go higher than the directory (or file) 
/// that was mounted as the root of the VFS.  This allows for a certain level of security to keep users from writing or reading
/// areas outside of the intended directory structure.  
/// 
/// Gorgon's VFS is modeled after the PhysFS project (http://icculus.org/physfs/).
/// 
/// In this example, we'll mount a physical file system directory as the root of the virtual file system.  All sub directories
/// and files under the root directory will become accessible from the virtual file system.  The program will enumerate the 
/// directories and files and list them in the console window with relevant file information.
/// </remarks>
internal static class Program
{
    #region Variables.
    // File system.
    private static GorgonFileSystem _fileSystem;
    // The log used logging debug messages.
    private static IGorgonLog _log;
    #endregion

    #region Methods.
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main()
    {
        DirectoryInfo resourceBaseDirectory = new(Path.Combine(ExampleConfig.Default.ResourceLocation, "FileSystems", "FolderSystem"));

        _log = new GorgonTextFileLog("FolderFileSystem", "Tape_Worm");
        _log.LogStart();

        try
        {
            Console.WindowHeight = 26.Max(Console.WindowHeight);
            Console.BufferHeight = Console.WindowHeight;

            // Create a new file system.
            _fileSystem = new GorgonFileSystem(_log);

            // Set the following directory as root on the virtual file system.
            // To mount a directory we need to put in the trailing slash.  
            //
            // If we wanted to, we could mount a directory as a sub directory of
            // the root of the virtual file system.  For example, mounting the
            // directory D:\Dir with Mount(@"D:\Dir", "/VFSDir"); would mount the
            // contents of the D:\Dir directory under /VFSDir.
            //
            // It's also important to point out that the old Gorgon "file system"
            // would load files from the system into memory when mounting a 
            // directory.  While this version only loads directory and file 
            // information when mounting.  This is considerably more efficient.
            string physicalPath = resourceBaseDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            _fileSystem.Mount(physicalPath);

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("This example will mount a physical file system directory as the root of a");
            Console.WriteLine("virtual file system.  The virtual file system is capable of mounting various");
            Console.WriteLine("types of data such as a zip file, a file system folder, etc... as the root or a");
            Console.WriteLine("sub directory.  You can even mount a zip file as the root, and a physical file");
            Console.WriteLine("system directory as a virtual sub directory in the same virtual file system and");
            Console.WriteLine("access them as a single unified file system.");

            Console.Write("\nMounted: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("'{0}'", physicalPath.Ellipses(Console.WindowWidth - 20, true));
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
            ex.Catch(_ =>
                     {
                         Console.Clear();
                         Console.ForegroundColor = ConsoleColor.Red;
                         Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", _.Message, _.StackTrace);

                         Console.ResetColor();
#if DEBUG
                         Console.ReadKey();
#endif
                     },
                     _log);
        }
        finally
        {
            _log.LogEnd();
        }
    }
    #endregion
}