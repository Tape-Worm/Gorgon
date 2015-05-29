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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.UI;

namespace Gorgon.Editor
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
		// List of disabled plug-ins.
		private readonly List<DisabledPlugIn> _disabled;
		// List of file system provider plug-ins.
		private readonly Dictionary<string, GorgonFileSystemProviderPlugIn> _fileSystemPlugIns;
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
					if (!GorgonApplication.PlugIns.IsPlugInAssembly(assemblyFile.FullName))
					{
						_log.Print("PlugInRegistry: The file '{0}' is not a plug-in assembly. It will be skipped.", LoggingLevel.Verbose, currentAssembly);
						continue;
					}

					splash.InfoText = string.Format(Resources.GOREDIT_TEXT_PLUG_IN, Path.GetFileNameWithoutExtension(assemblyFile.FullName).Ellipses(40, true));

					AssemblyName name = GorgonApplication.PlugIns.LoadPlugInAssembly(assemblyFile.FullName);
					IEnumerable<GorgonPlugIn> plugIns = GorgonApplication.PlugIns.EnumeratePlugIns(name);

					_log.Print("PlugInRegistry: Plug-in assembly '{0}' loaded from {1}.", LoggingLevel.Verbose, name.FullName, assemblyFile.FullName);

					// TODO: In time, add other plug-in types here.
					result.AddRange(plugIns.Where(plugIn => plugIn is GorgonFileSystemProviderPlugIn));
				}
			}
			catch (Exception ex)
			{
				// Don't let an errant plug-in break our application, so just log the exception and tell the user 
				// that we can't load the plug-in assembly for whatever reason.
				_log.Print("PlugInRegistry: The file '{0}' was not loaded due to an exception.", LoggingLevel.Simple, currentAssembly);
				GorgonExceptionExtensions.Catch(ex, () => GorgonDialogs.ErrorBox(splash, string.Format(Resources.GOREDIT_ERR_ERROR_LOADING_PLUGIN, currentAssembly), null, ex));
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
				GorgonPlugIn plugIn = GorgonApplication.PlugIns.FirstOrDefault(item => string.Equals(disabledPlugIn.Name, item.Name, StringComparison.OrdinalIgnoreCase)
																			&& string.Equals(disabledPlugIn.Path, item.PlugInPath, StringComparison.OrdinalIgnoreCase));

				if (plugIn == null)
				{
					continue;
				}

				GorgonApplication.PlugIns.Unload(plugIn);
			}
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
			_disabled = new List<DisabledPlugIn>();
			_fileSystemPlugIns = new Dictionary<string, GorgonFileSystemProviderPlugIn>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region IPlugInRegistry Implementation.
		#region Events.
		/// <summary>
		/// Event fired when an already loaded plug-in is disabled.
		/// </summary>
		public event EventHandler<PlugInDisabledEventArgs> PlugInDisabled;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of the disabled plug-ins.
		/// </summary>
		public IReadOnlyList<DisabledPlugIn> DisabledPlugIns
		{
			get
			{
				return _disabled;
			}
		}

		/// <summary>
		/// Property to return the collection of file system provider plug-ins.
		/// </summary>
		public IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> FileSystemPlugIns
		{
			get
			{
				return _fileSystemPlugIns;
			}
		}
		#endregion
		
		#region Methods.
		/// <summary>
		/// Function to determine if a plug-in is disabled.
		/// </summary>
		/// <param name="plugIn">The plug-in to query.</param>
		/// <returns><c>true</c> if disabled, <c>false</c> if not.</returns>
		public bool IsDisabled(GorgonPlugIn plugIn)
		{
			if (plugIn == null)
			{
				return false;
			}

			var disabled = new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, null);

			return DisabledPlugIns.Any(item => item.Equals(disabled));
		}

		/// <summary>
		/// Function to disable a plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in to disable.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugIn"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void DisablePlugIn(GorgonPlugIn plugIn)
		{
			if (plugIn == null)
			{
				throw new ArgumentNullException("plugIn");
			}

			// Plug-in is already disabled.
			if (IsDisabled(plugIn))
			{
				return;
			}

			_log.Print("PlugInRegistry: Plug-in '{0}' disabled by the user.", LoggingLevel.Verbose, plugIn.Name);

			var disabled = new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, Resources.GOREDIT_TEXT_DISABLED_BY_USER);
			_disabled.Add(disabled);

			// Remove from the known plug-in lists.
			if (_fileSystemPlugIns.ContainsKey(plugIn.Name))
			{
				_fileSystemPlugIns.Remove(plugIn.Name);
			}

			// Inform the application that we've disabled a plug-in.
			if (PlugInDisabled != null)
			{
				PlugInDisabled(this, new PlugInDisabledEventArgs(plugIn));
			}

			// Finally, unload the plug-in.
			GorgonApplication.PlugIns.Unload(plugIn);
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
				_log.Print("PlugInRegistry: No plug-ins found in '{0}'.", LoggingLevel.Verbose, _settings.PlugInDirectory);
				return;
			}

			foreach (GorgonPlugIn plugIn in plugIns)
			{
				_log.Print("PlugInRegistry: Found plug-in '{0}' of type [{1}] in assembly '{2}'.", LoggingLevel.Verbose, plugIn.Description, plugIn.Name, plugIn.PlugInPath);

				// Weed out the disabled plug-ins.
				if (_settings.DisabledPlugIns.Contains(plugIn.Name, StringComparer.OrdinalIgnoreCase))
				{
					_log.Print("PlugInRegistry: Skipping plug-in '{0}'.  Reason:  Disabled by user.", LoggingLevel.Verbose, plugIn.Description);
					_disabled.Add(new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, Resources.GOREDIT_TEXT_DISABLED_BY_USER));
					continue;
				}

				var fileSysPlugIn = plugIn as GorgonFileSystemProviderPlugIn;

				// We found a file system provider plug-in.
				if (fileSysPlugIn != null)
				{
					_fileSystemPlugIns[fileSysPlugIn.Name] = fileSysPlugIn;
					continue;
				}

				// TODO: Editor plug-ins will have a validation method that will allow us to disable the plug-in if the validation
				// TODO: does not pass.

				// We cannot determine the type for this plug-in, so disable it and move on.
				_log.Print("PlugInRegistry: Failed to load '{0}'. Reason: Plug-in type [{1}] is unknown.", LoggingLevel.Verbose,
									plugIn.Description,
									plugIn.Name);

				_disabled.Add(new DisabledPlugIn(plugIn.Name, plugIn.PlugInPath, Resources.GOREDIT_TEXT_UNKNOWN_PLUG_IN_TYPE));
			}

			PurgeDisabledPlugIns();
		}
		#endregion
		#endregion
	}
}
