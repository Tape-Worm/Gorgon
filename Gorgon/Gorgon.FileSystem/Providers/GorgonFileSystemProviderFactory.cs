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
using System.Linq;
using System.Reflection;
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
	/// using (GorgonPlugInAssemblyCache cache = new GorgonPlugInAssemblyCache())
	/// {
	///		// Load the assembly.
	///		service.Load("c:\path\to\your\assembly\7zProvider.dll");
	/// 
	///		// Get the plug in service.
	///		GorgonPlugInService service = new GorgonPlugInService(cache);
	/// 	
	///		// Create the provider factory.
	///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(service);
	/// 
	///		// Get our provider from the factory.
	///		IGorgonFileSystemProvider provider = CreateProvider("Fully.Qualified.TypeName.SevenZipProvider");
	/// 
	///		// Mount the file system.
	///		GorgonFileSystem fileSystem = new GorgonFileSystem(provider);
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
		// A plugin service where instances of the provider plugins can be found.
		private readonly IGorgonPlugInService _pluginService;
		// The application log file.
		private readonly IGorgonLog _log;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new file system provider.
		/// </summary>
		/// <param name="providerPlugInName">The fully qualified type name of the plugin that contains the file system provider.</param>
		/// <returns>The new file system provider object, or if it was previously created, the previously created instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providerPlugInName"/> is <b>null</b></exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="providerPlugInName"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the plugin specified by the <paramref name="providerPlugInName"/> parameter was not found.</exception>
		public GorgonFileSystemProvider CreateProvider(string providerPlugInName)
		{
			if (providerPlugInName == null)
			{
				throw new ArgumentNullException(nameof(providerPlugInName));
			}

			if (string.IsNullOrWhiteSpace(providerPlugInName))
			{
				throw new ArgumentEmptyException(nameof(providerPlugInName));
			}

			_log.Print("Creating file system provider '{0}'.", LoggingLevel.Simple, providerPlugInName);

			GorgonFileSystemProvider plugin =  _pluginService.GetPlugIn<GorgonFileSystemProvider>(providerPlugInName);

			if (plugin == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPlugInName));
			}

			return plugin;
		}

        /// <summary>
        /// Function to retrieve all the file system providers from the available plugins in the plugin service.
        /// </summary>
        /// <param name="pluginAssembly">[Optional] The name of the assembly to load file system providers from.</param>
        /// <returns>A list of file system providers</returns>
        /// <remarks>
        /// When the <paramref name="pluginAssembly"/> parameter is set to <b>null</b>, then only the file system providers within that assembly will 
        /// be loaded. Otherwise, all file system providers available in the <see cref="IGorgonPlugInService"/> passed to the object constructor will be created (or have 
        /// a previously created instance returned).
        /// </remarks>
        public IReadOnlyList<GorgonFileSystemProvider> CreateProviders(AssemblyName pluginAssembly = null) => pluginAssembly == null
                       ? _pluginService.GetPlugIns<GorgonFileSystemProvider>()
                                       .ToArray()
                       : _pluginService.GetPlugIns<GorgonFileSystemProvider>(pluginAssembly)
                                       .ToArray();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemProviderFactory"/> class.
        /// </summary>
        /// <param name="pluginService">The plugin service used to retrieve file system provider plugins.</param>
        /// <param name="log">[Optional] The application log file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/> parameter is <b>null</b>.</exception>
        public GorgonFileSystemProviderFactory(IGorgonPlugInService pluginService, IGorgonLog log = null)
		{
		    _log = log ?? GorgonLog.NullLog;
			_pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
		}
		#endregion
	}
}
