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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO
{
	/// <summary>
	/// A factory used to create file system providers.
	/// </summary>
	/// <remarks>
	/// The file system provider factory is used to create file system providers available in a <see cref="GorgonPluginService"/> object.
	/// </remarks>
	public sealed class GorgonFileSystemProviderFactory
	{
		#region Variables.
		// A plugin service where instances of the provider plugins can be found.
		private readonly GorgonPluginService _pluginService;
		// The application log file.
		private readonly IGorgonLog _log = new GorgonLogDummy();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all the file system providers from the plugin service.
		/// </summary>
		/// <returns>A list of file system providers.</returns>
		private IEnumerable<GorgonFileSystemProvider> GetAllProviders()
		{
			var result = new List<GorgonFileSystemProvider>();

			foreach (KeyValuePair<string, Assembly> assemblyItem in _pluginService.PluginAssemblyCache.PluginAssemblies)
			{
				AssemblyName name = assemblyItem.Value.GetName();

				IReadOnlyList<GorgonFileSystemProviderPlugIn> plugins = _pluginService.GetPlugins<GorgonFileSystemProviderPlugIn>(name);

				if (plugins.Count == 0)
				{
					continue;
				}

				result.AddRange(plugins.Select(item => item.CreateProvider()));
			}

			return result;
		}

		/// <summary>
		/// Function to create a new file system provider.
		/// </summary>
		/// <param name="providerPluginName">The fully qualified type name of the plugin that contains the file system provider.</param>
		/// <returns>The new file system provider object, or if it was previously created, the previously created instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providerPluginName"/> is <c>null</c> (Nothing in VB.Net)</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="providerPluginName"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the plugin specified by the <paramref name="providerPluginName"/> parameter was not found.</exception>
		public GorgonFileSystemProvider CreateProvider(string providerPluginName)
		{
			if (providerPluginName == null)
			{
				throw new ArgumentNullException("providerPluginName");
			}

			if (string.IsNullOrWhiteSpace(providerPluginName))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "providerPluginName");
			}

			GorgonFileSystemProviderPlugIn plugin = _pluginService.GetPlugin<GorgonFileSystemProviderPlugIn>(providerPluginName);

			if (plugin == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_NO_PROVIDER_PLUGIN, providerPluginName));
			}

			_log.Print("Creating file system provider '{0}'.", LoggingLevel.Simple, providerPluginName);

			return plugin.CreateProvider();
		}

		/// <summary>
		/// Function to retrieve all the file system providers from the available plugins in the plugin service.
		/// </summary>
		/// <param name="pluginAssembly">[Optional] The name of the assembly to load file system providers from.</param>
		/// <returns>A list of file system providers</returns>
		/// <remarks>
		/// When the <paramref name="pluginAssembly"/> parameter is set to <c>null</c> (Nothing in VB.Net), then only the file system providers within that assembly will 
		/// be loaded. Otherwise, all file system providers available in the <see cref="GorgonPluginService"/> passed to the object constructor will be created (or have 
		/// a previously created instance returned).
		/// </remarks>
		public IEnumerable<GorgonFileSystemProvider> CreateProviders(AssemblyName pluginAssembly = null)
		{
			return pluginAssembly == null
				       ? GetAllProviders()
				       : _pluginService.GetPlugins<GorgonFileSystemProviderPlugIn>(pluginAssembly).Select(item => item.CreateProvider());
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderFactory"/> class.
		/// </summary>
		/// <param name="pluginService">The plugin service used to retrieve file system provider plugins.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		public GorgonFileSystemProviderFactory(GorgonPluginService pluginService, IGorgonLog log = null)
		{
			if (pluginService == null)
			{
				throw new ArgumentNullException("pluginService");
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
