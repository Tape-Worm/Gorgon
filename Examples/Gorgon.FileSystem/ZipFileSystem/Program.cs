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
// Created: Thursday, January 17, 2013 11:07:10 PM
// 
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.IO;

namespace Gorgon.Examples
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
	/// Gorgon's VFS is modeled after the PhysFS project (http://icculus.org/physfs/).
	/// 
    /// In this example, we'll mount a zip file as the root of the virtual file system.  All sub directories and files under the root 
    /// directory will become accessible from the virtual file system.  The program will enumerate the directories and files and list 
    /// them in the console window with relevant file information.
	/// </remarks>
	static class Program
    {
        #region Constants.
        private const string PlugInName = "Gorgon.IO.GorgonZipPlugIn";
        #endregion

        #region Variables.
        private static GorgonFileSystem _fileSystem;         // File system.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the path to the plug-ins.
        /// </summary>
        public static string PlugInPath
        {
            get
            {
                string path = Settings.Default.PlugInLocation;

                if (path.Contains("{0}"))
                {
#if DEBUG
                    path = string.Format(path, "Debug");
#else
					path = string.Format(path, "Release");					
#endif
                }

                if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                {
                    path += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
                }

                return Path.GetFullPath(path);
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (Nothing in VB.Net) or empty.</exception>
		public static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
                throw new ArgumentException("The resource was not specified.", "resourceItem");
			}

			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			{
				path += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			}

			path = path.RemoveIllegalPathChars();
			
			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
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
        /// Function to load the zip file provider plug-in.
        /// </summary>
        /// <returns><c>true</c> if successfully loaded, <c>false</c> if not.</returns>
        static bool LoadZipProviderPlugIn()
        {
            string zipProviderPath = PlugInPath + "Gorgon.FileSystem.Zip.dll";

            if (!File.Exists(zipProviderPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the plug-in assembly file:\n'{0}'.", zipProviderPath);
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
                return false;
            }

            // Load the plug-in assembly.
            AssemblyName assembly = AssemblyName.GetAssemblyName(zipProviderPath);
            GorgonApplication.PlugIns.LoadPlugInAssembly(assembly);

            // Add the provider.
            if (!GorgonApplication.PlugIns.Contains(PlugInName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The plug-in assembly file:\n'{0}' does not contain a file system plug in named '{1}'.", zipProviderPath, PlugInName);
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
                return false;
            }

            // Ensure this plug in is a file system provider.
            var plugIn = GorgonApplication.PlugIns[PlugInName] as GorgonFileSystemProviderPlugIn;

            if (plugIn == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The plug in: '{0}'\nis not a file system provider plug-in.", PlugInName);
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
                return false;
            }

            _fileSystem.Providers.LoadProvider(PlugInName);

            Console.WriteLine("\nThe zip file file system provider was loaded successfully.");
            return true;
        }

	    /// <summary>
	    /// The main entry point for the application.
	    /// </summary>
	    static void Main()
		{
            try
            {
                Console.WindowHeight = 28;
                Console.BufferHeight = Console.WindowHeight;                

                // Create a new file system.
                _fileSystem = new GorgonFileSystem();               

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
                if (!LoadZipProviderPlugIn())
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
                var physicalPath = GetResourcePath(@"FileSystem.zip");
                _fileSystem.Mount(physicalPath);                
                                
                Console.Write("\nMounted: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("'{0}'", physicalPath.Ellipses(Console.WindowWidth - 20, true));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" as ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("'/'\n");
                Console.ForegroundColor = ConsoleColor.White;

                // Get a count of all sub directories and files under the root directory.
                var directoryList = _fileSystem.FindDirectories("*", true).ToArray();

                // Display directories.
                Console.WriteLine("Virtual file system contents:");                

                for (int i = -1; i < directoryList.Length; i++)
                {
                    GorgonFileSystemDirectory directory = _fileSystem.RootDirectory;
                    
                    // Go into the sub directories under root.
                    if (i > -1)
                    {
                        directory = directoryList[i];
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0}", directory.FullPath);
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    foreach (var file in directory.Files)
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
                        });
            }
        }
        #endregion
    }
}
