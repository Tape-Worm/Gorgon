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
	/// The VFS object in Gorgon comes with the ability to mount a directory as a root of a VFS.  However, it's possible to mount a
	/// zip file, or the old Gorgon BZip2 Pack file format as a VFS.  This is done through file system providers.  Similar to the
	/// input factories, these providers are plug-ins and can be loaded into a file system object to give access to these types of 
	/// files.  A provider plug-in can be written to pull data from a SQL server, or a network stream or any access point that can
	/// stream data.
	/// 
	/// In this example, we'll show how to load some of these providers into a file system object.
	/// </remarks>
	static class Program
    {
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

				if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					path += Path.DirectorySeparatorChar.ToString();
				}

				return Path.GetFullPath(path);
			}
		}
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load the file system providers.
        /// </summary>
        /// <returns>The number of file system provider plug-ins.</returns>
        private static int LoadFileSystemProviders()
        {
            int result = 0;
            var files = Directory.GetFiles(PlugInPath, "*.dll", SearchOption.TopDirectoryOnly);
            
            // Load each assembly.
            foreach (var file in files)
            {
                // Get the assembly name.  
                // This is the preferred method of loading a plug-in assembly.
                // It keeps us from going into DLL hell because it'll contain
                // version information, public key info, etc...  
                // We wrap this in this exception handler because if a DLL is
                // a native DLL, then it'll throw an exception.  And since
                // we can't load native DLLs as our plug-in, then we should
                // skip it.
                AssemblyName name;
                try
                {
                    name = AssemblyName.GetAssemblyName(file);
                }
                catch (BadImageFormatException)
                {
                    // This happens if we try and load a DLL that's not a .NET assembly.
                    continue;
                }

                // Skip assemblies that aren't a plug-in.
                if (!GorgonApplication.PlugIns.IsPlugInAssembly(name))
                {
                    continue;
                }

                // Load the assembly DLL.
                // This will not only load the assembly DLL into the application
                // domain (if it's not already loaded), but will also enumerate
                // the plug-in types.  If there are none, an exception will be
                // thrown.  This is why we do a check with IsPlugInAssembly before
                // we load the assembly.
                GorgonApplication.PlugIns.LoadPlugInAssembly(name);

                // Now try to retrieve our file system provider plug-ins.
                // Retrieve the list of plug-ins from the assembly.  Once we have
                // the list we look for any plug-ins that are GorgonFileSystemProviderPlugIn
                // types and retrieve their type information.
                var providerPlugIns = GorgonApplication.PlugIns.EnumeratePlugIns(name)
                                    .Where(item => item is GorgonFileSystemProviderPlugIn)
                                    .Select(item => item.GetType()).ToArray();

                // Add each file system provider and plug-in to our list.                
                foreach (var providerPlugIn in providerPlugIns)
                {
                    // Here we actually create the file system provider from the plug-in
                    // by passing the file system provider plug-in type.  
                    _fileSystem.Providers.LoadProvider(providerPlugIn.FullName);
                }

                result += providerPlugIns.Length;
            }

            return result;
        }

	    /// <summary>
	    /// The main entry point for the application.
	    /// </summary>
	    static void Main()
		{
            try
            {
                // Create a new file system.
				// The file system must be created first and given access to the various
				// data sources via the provider plug-ins.
				// For example, this will allow us to create a file system that can read
				// a RAR file, while another file system would only cater to Zip files.
				// By default, every file system comes with a folder file system provider
				// that can mount a directory from the hard drive as a VFS root.
                _fileSystem = new GorgonFileSystem();

				Console.WriteLine("Gorgon is capable of mounting virtual file systems for file access.  A virtual");
				Console.WriteLine("filesystem root can be a folder on a harddrive, a zip file, or any data store");
				Console.WriteLine("(assuming there's a provider for it).\n");
				Console.WriteLine("In Gorgon, the types of data that can be mounted as a virtual file system is");
				Console.WriteLine("managed by plug-ins called providers. By default, the file system has a folder");
				Console.WriteLine("provider.  This allows a folder to be mounted as the root of a virtual file\nsystem.\n");
				Console.WriteLine("This example will show how to load extra providers into a file system.\n");
				
				Console.ForegroundColor = ConsoleColor.White;

                // Get our file system providers.                
                Console.WriteLine("Found {0} external file system plug-ins.\n", LoadFileSystemProviders());

                // Loop through each provider and print some info.
                foreach(var provider in _fileSystem.Providers.Select((item, index) => new {Provider = item, Index = index}))
                {
                    // Print some info about the file system provider.
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0}. {1}", (provider.Index + 1), provider.Provider.Name);

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("    Description: {0}", provider.Provider.Description);
                    
                    // Gather the preferred extensions.
					// File system providers that use a file (like a Zip file) as its root
					// have a list of file extensions that are preferred.  For example, the
					// Zip provider, expects to find *.zip files.  These are merely here 
					// for the convenience of the developer and are formatted like a common
					// dialog file mask so they can be easily dropped into that control.
					// In this case, we're going to just strip out the relevant part and 
					// concatenate each preferred extension description into a single string.  
					//
					// Note that a provider may have multiple preferred extensions.
	                var extensionList = (from preferred in provider.Provider.PreferredExtensions
	                                     select string.Format("*.{0}", preferred.Extension)).ToArray();

                    if (extensionList.Length > 0)
                    {
                        Console.WriteLine("    Preferred Extensions: {0}", string.Join(", ", extensionList));
                    }
                }

				Console.ResetColor();
				Console.WriteLine("\nPress any key to close.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                GorgonException.Catch(ex, _ =>
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", _.Message, _.StackTrace);
                }, true);
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
            }
        }
        #endregion
    }
}
