#region MIT.
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
// Created: Wednesday, February 25, 2015 10:56:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.PlugIns;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The registry used by the application to load and register plug-ins.
	/// </summary>
	class PlugInRegistry : IPlugInRegistry
	{
		#region Variables.
		// Application settings.
		private readonly IEditorSettings _settings;
		// The proxy for the splash screen to notify the user of state.
		private readonly IProxyObject<FormSplash> _splashProxy;
		// The log file for the application.
		private readonly GorgonLogFile _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of the disabled plug-ins.
		/// </summary>
		public IReadOnlyList<DisabledPlugIn> DisabledPlugIns
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the collection of file system provider plug-ins.
		/// </summary>
		public IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> FileSystemPlugIns
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to check for the existence of the plug-in path and create it if necessary.
		/// </summary>
		/// <returns>A directory info object containing information about the path.</returns>
		private DirectoryInfo EnsurePlugInPathExists()
		{
			var result = new DirectoryInfo(_settings.PlugInDirectory.FormatDirectory(Path.DirectorySeparatorChar));

			if (!result.Exists)
			{
				result.Create();
			}

			return result;
		}

		/// <summary>
		/// Function to load the assemblies in the plug-in path and register the plug-in types.
		/// </summary>
		/// <param name="splash">The splash form used to display status to the user.</param>
		/// <returns>A list of plug-ins that were successfully loaded.</returns>
		private IList<GorgonPlugIn> LoadPlugInAssemblies(FormSplash splash)
		{
			var result = new List<GorgonPlugIn>();
			var currentAssembly = string.Empty;

			// We keep this outside of the try-catch because if we have a file system error (security, etc...)
			// then we should not proceed and the application should die gracefully.
			DirectoryInfo plugInDirectory = EnsurePlugInPathExists();

			try
			{
				IEnumerable<FileInfo> plugInAssemblies = plugInDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

				foreach (FileInfo assemblyFile in plugInAssemblies)
				{
					currentAssembly = assemblyFile.FullName;
					// Ensure that any other DLL types are not loaded (native images, non-Gorgon plug-in dlls, etc...)
					if (!Gorgon.PlugIns.IsPlugInAssembly(assemblyFile.FullName))
					{
						_log.Print("The file '{0}' is not a plug-in assembly. It will be skipped.", LoggingLevel.Verbose, currentAssembly);
						continue;
					}

					splash.InfoText = string.Format(Resources.GOREDIT_TEXT_PLUG_IN, Path.GetFileNameWithoutExtension(assemblyFile.FullName).Ellipses(40, true));

					AssemblyName name = Gorgon.PlugIns.LoadPlugInAssembly(assemblyFile.FullName);
					IEnumerable<GorgonPlugIn> plugIns = Gorgon.PlugIns.EnumeratePlugIns(name);

					// TODO: In time, add other plug-in types here.
					result.AddRange(plugIns.Where(plugIn => plugIn is GorgonFileSystemProviderPlugIn));
				}
			}
			catch (Exception ex)
			{
				// Don't let an errant plug-in break our application, so just log the exception and tell the user 
				// that we can't load the plug-in assembly for whatever reason.
				_log.Print("The file '{0}' was not loaded due to an exception.", LoggingLevel.Simple, currentAssembly);
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(splash, string.Format(Resources.GOREDIT_ERR_ERROR_LOADING_PLUGIN, currentAssembly), null, ex));
			}
			return result;
		}

		/// <summary>
		/// Function to unload disabled plug-ins from the plug-in registry.
		/// </summary>
		private void PurgeDisabledPlugIns()
		{
			if (DisabledPlugIns.Count == 0)
			{
				return;
			}

			// Unload the plug-in if it's disabled, just so there's no conflicts or attempts to use it.
			foreach (DisabledPlugIn disabledPlugIn in DisabledPlugIns)
			{
				GorgonPlugIn plugIn = Gorgon.PlugIns.FirstOrDefault(item => string.Equals(disabledPlugIn.Name, item.Name, StringComparison.OrdinalIgnoreCase)
																			&& string.Equals(disabledPlugIn.Path, item.PlugInPath, StringComparison.OrdinalIgnoreCase));

				if (plugIn == null)
				{
					continue;
				}

				Gorgon.PlugIns.Unload(plugIn);
			}
		}

		/// <summary>
		/// Function to scan for and load any plug-ins located in the plug-ins folder.
		/// </summary>
		public void ScanAndLoadPlugIns()
		{
			FormSplash splash = _splashProxy.Item;

			splash.InfoText = Resources.GOREDIT_TEXT_LOADING_PLUGINS;

			// Load and register.
			IList<GorgonPlugIn> plugIns = LoadPlugInAssemblies(splash);

			if (plugIns.Count == 0)
			{
				_log.Print("No plug-ins found in '{0}'.", LoggingLevel.Verbose, _settings.PlugInDirectory);
				return;
			}

			var disabled = new List<DisabledPlugIn>();
			var fileSystemPlugIns = new Dictionary<string, GorgonFileSystemProviderPlugIn>(StringComparer.OrdinalIgnoreCase);
			
			foreach (GorgonPlugIn plugIn in plugIns)
			{
				_log.Print("Found plug-in '{0}' of type [{1}] in assembly '{2}'.", LoggingLevel.Verbose, plugIn.Description, plugIn.Name, plugIn.PlugInPath);

				// Weed out the disabled plug-ins.
				if (_settings.DisabledPlugIns.Contains(plugIn.Name, StringComparer.OrdinalIgnoreCase))
				{
					_log.Print("Failed to load '{0}'.  Reason:  Disabled by user.", LoggingLevel.Verbose, plugIn.Description);
					disabled.Add(new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, Resources.GOREDIT_TEXT_DISABLED_BY_USER));
					continue;
				}

				var fileSysPlugIn = plugIn as GorgonFileSystemProviderPlugIn;

				// We found a file system provider plug-in.
				if (fileSysPlugIn != null)
				{
					fileSystemPlugIns[fileSysPlugIn.Name] = fileSysPlugIn;
					continue;
				}

				// We cannot determine the type for this plug-in, so disable it and move on.
				_log.Print("Failed to load '{0}'. Reason: Plug-in type [{1}] is unknown.", LoggingLevel.Verbose,
									plugIn.Description,
									plugIn.Name);

				disabled.Add(new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, Resources.GOREDIT_TEXT_UNKNOWN_PLUG_IN_TYPE));
			}

			FileSystemPlugIns = fileSystemPlugIns;
			DisabledPlugIns = disabled;

			PurgeDisabledPlugIns();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInRegistry"/> class.
		/// </summary>
		/// <param name="log">The log file for the application.</param>
		/// <param name="settings">The application settings.</param>
		/// <param name="splashProxy">The proxy object containing the splash screen to notify state for the user.</param>
		public PlugInRegistry(GorgonLogFile log, IEditorSettings settings, IProxyObject<FormSplash> splashProxy)
		{
			_log = log;
			_settings = settings;
			_splashProxy = splashProxy;
			DisabledPlugIns = new DisabledPlugIn[0];
			FileSystemPlugIns = new Dictionary<string, GorgonFileSystemProviderPlugIn>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion
	}
}
