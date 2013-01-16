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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using GorgonLibrary;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.Examples
{
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
	/// Gorgon's VFS system is modeled after the PhysFS project (http://icculus.org/physfs/).
	/// 
	/// </remarks>
	static class Program
    {
        #region Variables.
        private static GorgonFileSystem _fileSystem = null;         // File system.
        #endregion

        #region Methods.
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (Nothing in VB.Net) or empty.</exception>
		public static string GetResourcePath(string resourceItem)
		{
			string path = Properties.Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException("The resource was not specified.", "resourceItem");
			}

			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += Path.DirectorySeparatorChar.ToString();
			}

			path = path.RemoveIllegalPathChars();
			
			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += resourceItem.RemoveIllegalPathChars();
			}
			else
			{
				// Otherwise, sanitize the file name.
				path += resourceItem.RemoveIllegalFilenameChars();
			}

			// Ensure that 
			return Path.GetFullPath(path);
		}

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
        /// <param name="args">Command line arguments.</param>
		static void Main(string[] args)
		{
            try
            {
                // Create a new file system.
                _fileSystem = new GorgonFileSystem();

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
				var physicalPath = GetResourcePath(@"FolderSystem\");
				_fileSystem.Mount(physicalPath);

				Console.WriteLine("Mounted:\n\"{0}\"\nas\n\"/\" on the Virtual File System.\n", physicalPath.Ellipses(Console.WindowWidth - 3, true));

				var directoryCount = _fileSystem.FindDirectories("*", true).Count();
				var fileCount = _fileSystem.FindFiles("*", true).Count();

				Console.WriteLine("Directory count: {0}\nFile count: {1}", directoryCount, fileCount);

				Console.ResetColor();
				Console.WriteLine("\nPress any key to close.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                // Catch all exceptions here.  If we had logging for the application enabled, then this 
                // would record the exception in the log.
                GorgonException.Catch(ex, () =>
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", ex.Message, ex.StackTrace);
                });
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
            }
        }
        #endregion
    }
}
