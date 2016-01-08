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
using Gorgon.Plugins;

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
	/// File system providers are plug ins, and should have their assemblies loaded by the <see cref="GorgonPluginAssemblyCache"/> before using this method and a <see cref="GorgonPluginService"/> should be 
	/// created in order to pass it to this factory.
	/// </para>
	/// </remarks>
	/// <example>
	/// The following example shows how to use the provider factory:
	/// <code language="csharp"> 
	/// <![CDATA[
	/// using (GorgonPluginAssemblyCache cache = new GorgonPluginAssemblyCache())
	/// {
	///		// Load the assembly.
	///		service.Load("c:\path\to\your\assembly\7zProvider.dll");
	/// 
	///		// Get the plug in service.
	///		GorgonPluginService service = new GorgonPluginService(cache);
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
		private readonly IGorgonPluginService _pluginService;
		// The application log file.
		private readonly IGorgonLog _log = GorgonLogDummy.DefaultInstance;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all the file system providers from the plugin service.
		/// </summary>
		/// <returns>A list of file system providers.</returns>
		private IReadOnlyList<GorgonFileSystemProvider> GetAllProviders()
		{
			return _pluginService.GetPlugins<GorgonFileSystemProvider>()
			                     .ToArray();
		}

		/// <summary>
		/// Function to create a new file system provider.
		/// </summary>
		/// <param name="providerPluginName">The fully qualified type name of the plugin that contains the file system provider.</param>
		/// <returns>The new file system provider object, or if it was previously created, the previously created instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providerPluginName"/> is <b>null</b> (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="providerPluginName"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the plugin specified by the <paramref name="providerPluginName"/> parameter was not found.</exception>
		public GorgonFileSystemProvider CreateProvider(string providerPluginName)
		{
			if (providerPluginName == null)
			{
				throw new ArgumentNullException(nameof(providerPluginName));
			}

			if (string.IsNullOrWhiteSpace(providerPluginName))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(providerPluginName));
			}

			_log.Print("Creating file system provider '{0}'.", LoggingLevel.Simple, providerPluginName);

			GorgonFileSystemProvider plugin =  _pluginService.GetPlugin<GorgonFileSystemProvider>(providerPluginName);

			if (plugin == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPluginName));
			}

			return plugin;
		}

		/// <summary>
		/// Function to retrieve all the file system providers from the available plugins in the plugin service.
		/// </summary>
		/// <param name="pluginAssembly">[Optional] The name of the assembly to load file system providers from.</param>
		/// <returns>A list of file system providers</returns>
		/// <remarks>
		/// When the <paramref name="pluginAssembly"/> parameter is set to <b>null</b> (<i>Nothing</i> in VB.Net), then only the file system providers within that assembly will 
		/// be loaded. Otherwise, all file system providers available in the <see cref="GorgonPluginService"/> passed to the object constructor will be created (or have 
		/// a previously created instance returned).
		/// </remarks>
		public IReadOnlyList<GorgonFileSystemProvider> CreateProviders(AssemblyName pluginAssembly = null)
		{
			return pluginAssembly == null
				       ? GetAllProviders()
				       : _pluginService.GetPlugins<GorgonFileSystemProvider>(pluginAssembly)
									   .ToArray();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderFactory"/> class.
		/// </summary>
		/// <param name="pluginService">The plugin service used to retrieve file system provider plugins.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonFileSystemProviderFactory(IGorgonPluginService pluginService, IGorgonLog log = null)
		{
			if (pluginService == null)
			{
				throw new ArgumentNullException(nameof(pluginService));
			}

			if (log != null)
			{
				_log = log;
			}

			_pluginService = pluginService;
		}
		#endregion
	}
}
