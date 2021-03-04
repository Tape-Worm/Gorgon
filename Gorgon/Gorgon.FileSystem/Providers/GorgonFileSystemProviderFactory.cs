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
// Created: Tuesday, June 2, 2015 9:56:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;
using Gorgon.PlugIns;

namespace Gorgon.IO.Providers
{
    /// <summary>
    /// A factory object used to create file system provider plug ins.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will generate providers that will allow access to different types of file systems. For example, a user may create a file system provider that will open 7-zip files, but the <see cref="IGorgonFileSystem"/> 
    /// will not know how to read those files without the appropriate provider. This object would be used to load that 7-zip provider, and add it to the file system object so that it will know how to mount those 
    /// file types.
    /// </para>
    /// <para>
    /// File system providers are plug ins, and should have their assemblies loaded by the <see cref="GorgonMefPlugInCache"/> before using this method and a <see cref="IGorgonPlugInService"/> should be 
    /// created in order to pass it to this factory.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows how to use the provider factory:
    /// <code language="csharp"> 
    /// <![CDATA[
    /// // In a real world application, you would keep your cache for as long as you need your plug ins.
    /// // Premature disposal can cause errors.
    /// using (GorgonMefPlugInCache cache = new GorgonMefPlugInCache())
    /// {
    ///		// Create the provider factory.
    ///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(cache);
    /// 
    ///		// Get our provider from the factory.
    ///		IGorgonFileSystemProvider provider = CreateProvider(@"C:\FileSystemProviders\Gorgon.FileSystem.7zip.dll", "Gorgon.FileSystem.SevenZipProvider");
    /// 
    ///		// Mount the file system.
    ///		IGorgonFileSystem fileSystem = new GorgonFileSystem(provider);
    /// 
    ///		fileSystem.Mount("c:\path\to\your\archive\file.7z");
    ///		
    ///		// Do stuff...
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public sealed class GorgonFileSystemProviderFactory
        : IGorgonFileSystemProviderFactory
    {
        #region Variables.
        // The service for locating plug ins.
        private readonly GorgonMefPlugInCache _plugInCache;
        // The application log file.
        private readonly IGorgonLog _log;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new file system provider.
        /// </summary>
        /// <param name="path">The path to the file system plug in assemblies.</param>
        /// <param name="providerPlugInName">The fully qualified type name of the plugin that contains the file system provider.</param>
        /// <returns>The new file system provider object, or if it was previously created, the previously created instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/>, or the <paramref name="providerPlugInName"/> is <b>null</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/>, or the <paramref name="providerPlugInName"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the driver type name specified by <paramref name="providerPlugInName"/> was not found in any of the loaded plug in assemblies.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="path"/> was invalid.</para>
        /// </exception>
        public GorgonFileSystemProvider CreateProvider(string path, string providerPlugInName)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (providerPlugInName is null)
            {
                throw new ArgumentNullException(nameof(providerPlugInName));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(providerPlugInName))
            {
                throw new ArgumentEmptyException(nameof(providerPlugInName));
            }

            string dirName = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dirName))
            {
                dirName = Directory.GetCurrentDirectory();
            }

            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
            }

            _plugInCache.LoadPlugInAssemblies(dirName, fileName);

            _log.Print("Creating file system provider '{0}'.", LoggingLevel.Simple, providerPlugInName);

            IGorgonPlugInService plugInService = new GorgonMefPlugInService(_plugInCache);

            GorgonFileSystemProvider plugin = plugInService.GetPlugIn<GorgonFileSystemProvider>(providerPlugInName);

            return plugin ?? throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPlugInName));
        }

        /// <summary>
        /// Function to retrieve all the file system providers from the available plugins in the plugin service.
        /// </summary>
        /// <param name="path">The path to the file system plug in assemblies.</param>
        /// <returns>A list of file system providers</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> was invalid.</exception>
        public IReadOnlyList<GorgonFileSystemProvider> CreateProviders(string path)
        {
            IGorgonPlugInService plugInService = new GorgonMefPlugInService(_plugInCache);

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            string dirName = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dirName))
            {
                dirName = Directory.GetCurrentDirectory();
            }

            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
            }

            _plugInCache.LoadPlugInAssemblies(dirName, fileName);

            return plugInService.GetPlugIns<GorgonFileSystemProvider>().ToArray();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemProviderFactory"/> class.
        /// </summary>
        /// <param name="plugInCache">The cache used to load and store plugin assemblies.</param>
        /// <param name="log">[Optional] The application log file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugInCache"/> parameter is <b>null</b>.</exception>
        public GorgonFileSystemProviderFactory(GorgonMefPlugInCache plugInCache, IGorgonLog log = null)
        {
            _log = log ?? GorgonLog.NullLog;
            _plugInCache = plugInCache ?? throw new ArgumentNullException(nameof(plugInCache));            
        }
        #endregion
    }
}
