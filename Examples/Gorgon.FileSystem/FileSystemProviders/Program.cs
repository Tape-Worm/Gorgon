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
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;

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
/// Gorgon's VFS is modelled after the PhysFS project (http://icculus.org/physfs/).
/// 
/// The VFS object in Gorgon comes with the ability to mount a directory as a root of a VFS.  However, it's possible to mount a
/// zip file, or the old Gorgon BZip2 Pack file format as a VFS.  This is done through file system providers.  Similar to the
/// input factories, these providers are plug ins and can be loaded into a file system object to give access to these types of 
/// files.  A provider plug in can be written to pull data from a SQL server, or a network stream or any access point that can
/// stream data.
/// 
/// In this example, we'll show how to load some of these providers.
/// </remarks>
internal static class Program
{
    #region Variables.
    // The providers that were loaded.
    private static IReadOnlyList<GorgonFileSystemProvider> _providers;
    // The cache that will hold the assemblies where our plugins will live.
    private static GorgonMefPlugInCache _pluginAssemblies;
    // The log used for debug logging.
    private static IGorgonLog _log;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the path to the plug ins.
    /// </summary>
    public static string PlugInPath
    {
        get
        {
            string path = ExampleConfig.Default.PlugInLocation;

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
    /// Function to load the file system providers.
    /// </summary>
    /// <param name="pluginDirectory">The directory containing the plug ins.</param>
    /// <returns>The number of file system provider plug ins.</returns>
    private static int LoadFileSystemProviders(DirectoryInfo pluginDirectory)
    {
        // Get the file system provider factory so we can retrieve our newly loaded providers.
        IGorgonFileSystemProviderFactory providerFactory = new GorgonFileSystemProviderFactory(_pluginAssemblies, _log);

        // Get all the providers.
        // We could limit this to a single provider, or to a single plugin assembly if we choose.  But for 
        // this example, we'll get everything we've got.
        _providers = providerFactory.CreateProviders(Path.Combine(GetPlugInPath(pluginDirectory).FullName, "Gorgon.FileSystem.*.dll"));

        return _providers.Count;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main()
    {
        DirectoryInfo plugInLocationDirectory = new(ExampleConfig.Default.PlugInLocation);

        _log = new GorgonTextFileLog("FileSystemProviders", "Tape_Worm");
        _log.LogStart();

        // Create a plugin assembly cache to hold our plugin assemblies.
        _pluginAssemblies = new GorgonMefPlugInCache(_log);

        try
        {
            Console.WriteLine("Gorgon is capable of mounting virtual file systems for file access.  A virtual");
            Console.WriteLine("file system root can be a folder on a hard drive, a zip file, or any data store");
            Console.WriteLine("(assuming there's a provider for it).\n");
            Console.WriteLine("In Gorgon, the types of data that can be mounted as a virtual file system are");
            Console.WriteLine("managed by objects called providers. By default, the file system has a folder");
            Console.WriteLine("provider.  This allows a folder to be mounted as the root of a virtual file\nsystem.\n");
            Console.WriteLine("This example will show how to load extra providers that can be used in a file\nsystem.\n");

            Console.ForegroundColor = ConsoleColor.White;

            // Get our file system providers.                
            Console.WriteLine("Found {0} external file system plug ins.\n", LoadFileSystemProviders(plugInLocationDirectory));

            // Loop through each provider and print some info.
            for (int i = 0; i < _providers.Count; ++i)
            {
                // Print some info about the file system provider.
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("{0}. {1}", i + 1, _providers[i].Name);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("    Description: {0}", _providers[i].Description);

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
                string[] extensionList = (from preferred in _providers[i].PreferredExtensions
                                          select $"*.{preferred.Extension}").ToArray();

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
            ex.Catch(_ =>
                     {
                         Console.Clear();
                         Console.ForegroundColor = ConsoleColor.Red;
                         Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", _.Message, _.StackTrace);
                     },
                     _log);
            Console.ResetColor();
#if DEBUG
            Console.ReadKey();
#endif
        }
        finally
        {
            // Always call dispose so we can unload our temporary application domain.
            _pluginAssemblies.Dispose();

            _log.LogEnd();
        }
    }
    #endregion
}