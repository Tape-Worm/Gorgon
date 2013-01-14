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
	/// <remarks></remarks>
	static class Program
    {
        #region Variables.
        private static GorgonFileSystem _fileSystem = null;         // File system.
        #endregion

        #region Properties.
        /// <summary>
		/// Property to return the path to the plug-ins.
		/// </summary>
		public static string PlugInPath
		{
			get
			{
				string path = Properties.Settings.Default.PlugInLocation;

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
            IList<Tuple<GorgonFileSystemProviderPlugIn, GorgonFileSystemProvider>> providers = new Tuple<GorgonFileSystemProviderPlugIn, GorgonFileSystemProvider>[] { };
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
                AssemblyName name = null;
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
                if (!Gorgon.PlugIns.IsPlugInAssembly(name))
                {
                    continue;
                }

                // Load the assembly DLL.
                // This will not only load the assembly DLL into the application
                // domain (if it's not already loaded), but will also enumerate
                // the plug-in types.  If there are none, an exception will be
                // thrown.  This is why we do a check with IsPlugInAssembly before
                // we load the assembly.
                Gorgon.PlugIns.LoadPlugInAssembly(name);

                // Now try to retrieve our file system provider plug-ins.
                // Retrieve the list of plug-ins from the assembly.  Once we have
                // the list we look for any plug-ins that are GorgonFileSystemProviderPlugIn
                // types and retrieve their type information.
                var providerPlugIns = Gorgon.PlugIns.EnumeratePlugIns(name)
                                    .Where(item => item is GorgonFileSystemProviderPlugIn)
                                    .Select(item => item.GetType());                

                // Add each file system provider and plug-in to our list.                
                foreach (var providerPlugIn in providerPlugIns)
                {
                    // Here we actually create the file system provider from the plug-in
                    // by passing the file system provider plug-in type.  
                    _fileSystem.AddProvider(providerPlugIn);
                }

                result += providerPlugIns.Count();
            }

            return result;
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

                // Get our file system providers.
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Found {0} external file system plug-ins.\n", LoadFileSystemProviders());

                // Loop through each provider.
                for (int i = 0; i < _fileSystem.Providers.Count; i++)
                {
                    var provider = _fileSystem.Providers[i];

                    // Print some info about the file system provider.
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0}. {1}", (i + 1), provider.Name);

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("    Description: {0}", provider.Description);
                    
                    IList<string> extensions = new List<string>();

                    // Gather the preferred extensions.
                    foreach(var preferred in provider.PreferredExtensions)
                    {
                        var splitString = preferred.Split(new char[] {'|'});

                        if (splitString.Length > 0)
                        {
                            extensions.Add(splitString[0]);
                        }
                    }

                    Console.WriteLine("    Preferred Extensions: {0}", string.Join(", ", extensions));
                }

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
