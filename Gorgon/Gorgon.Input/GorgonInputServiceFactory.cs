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
// Created: Tuesday, June 16, 2015 9:58:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.Plugins;

namespace Gorgon.Input
{
	/// <summary>
	/// A factory used to create input services.
	/// </summary>
	/// <remarks>
	/// The input service factory is used to create input services from the plugins available in a <see cref="GorgonPluginService"/> object.
	/// </remarks>
	public class GorgonInputServiceFactory
	{
		#region Variables.
		// A plugin service where instances of the provider plugins can be found.
		private readonly GorgonPluginService _pluginService;
		// The application log file.
		private readonly IGorgonLog _log = new GorgonLogDummy();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all the input services from the plugin service.
		/// </summary>
		/// <returns>A list of input services created by this method.</returns>
		private IEnumerable<GorgonInputService> GetAllServices()
		{
			var result = new List<GorgonInputService>();

			foreach (KeyValuePair<string, Assembly> assemblyItem in _pluginService.PluginAssemblyCache.PluginAssemblies)
			{
				AssemblyName name = assemblyItem.Value.GetName();

				IReadOnlyList<GorgonInputServicePlugin> plugins = _pluginService.GetPlugins<GorgonInputServicePlugin>(name);

				if (plugins.Count == 0)
				{
					continue;
				}

				result.AddRange(plugins.Select(item =>
				                               {
					                               GorgonInputService service = item.CreateInputService();

					                               service.Log = _log;

					                               return service;
				                               }));
			}

			return result;
		}

		/// <summary>
		/// Function to create a new <see cref="GorgonInputService"/> from a plugin.
		/// </summary>
		/// <param name="servicePluginName">The fully qualified type name of the plugin that contains the <see cref="GorgonInputService"/>.</param>
		/// <returns>The new <see cref="GorgonInputService"/>, or if it was previously created, the previously created instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="servicePluginName"/> is <b>null</b> (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="servicePluginName"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the plugin specified by the <paramref name="servicePluginName"/> parameter was not found.</exception>
		public GorgonInputService CreateService(string servicePluginName)
		{
			if (servicePluginName == null)
			{
				throw new ArgumentNullException(nameof(servicePluginName));
			}

			if (string.IsNullOrWhiteSpace(servicePluginName))
			{
				throw new ArgumentException(Resources.GORINP_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(servicePluginName));
			}

			GorgonInputServicePlugin plugin = _pluginService.GetPlugin<GorgonInputServicePlugin>(servicePluginName);

			if (plugin == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORINP_PLUGIN_NOT_FOUND, servicePluginName));
			}

			_log.Print("Creating input service '{0}'.", LoggingLevel.Simple, servicePluginName);

			GorgonInputService service = plugin.CreateInputService();

			service.Log = _log;

			return service;
		}

		/// <summary>
		/// Function to create and retrieve all the input services from the available plugins in the plugin service.
		/// </summary>
		/// <param name="pluginAssembly">[Optional] The name of the assembly to load file system providers from.</param>
		/// <returns>A list of input services created from the plugins.</returns>
		/// <remarks>
		/// When the <paramref name="pluginAssembly"/> parameter is set to <b>null</b> (<i>Nothing</i> in VB.Net), then only the <see cref="GorgonInputService"/> objects within that assembly will 
		/// be loaded. Otherwise, all services available in the <see cref="GorgonPluginService"/> that was passed to the object constructor will be created (or have a previously created instance returned).
		/// </remarks>
		public IEnumerable<GorgonInputService> CreateServices(AssemblyName pluginAssembly = null)
		{
			return pluginAssembly == null
				       ? GetAllServices()
				       : _pluginService.GetPlugins<GorgonInputServicePlugin>(pluginAssembly).Select(item =>
				                                                                                    {
																										GorgonInputService service = item.CreateInputService();

					                                                                                    service.Log = _log;

					                                                                                    return service;
				                                                                                    });
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputServiceFactory"/> class.
		/// </summary>
		/// <param name="pluginService">The plugin service used to retrieve file system provider plugins.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonInputServiceFactory(GorgonPluginService pluginService, IGorgonLog log = null)
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
