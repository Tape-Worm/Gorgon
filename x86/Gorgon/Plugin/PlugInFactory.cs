#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, May 01, 2006 4:21:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Object to repesent a list of plugins.
	/// </summary>
	public static class PlugInFactory
	{
		#region Variables.
		private static PlugInModuleList _modules = null;		// List of modules.
		private static PlugInList _plugIns = null;				// List of plug-ins.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of previously loaded plug-ins.
		/// </summary>
		public static PlugInList PlugIns
		{
			get
			{
				return _plugIns;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The plug-in interface.</returns>
		public static PlugInEntryPoint Load(Assembly pluginModule, string pluginName, object[] parameters)
		{
			try
			{
				PlugInEntryPoint entry = null;					// Entry point to the plug-in.
				object[] typeAttributes = null;					// List of type attributes.

				if (pluginName == null)
					pluginName = string.Empty;

				// If we pass NULL, try to retrieve the plug-in from the entry assembly.
				if (pluginModule == null)
					pluginModule = Assembly.GetEntryAssembly();

				// Don't duplicate.
				if ((pluginName != string.Empty) && (_plugIns.Contains(pluginName)))
					return _plugIns[pluginName];

				// Get attributes for this module.
				typeAttributes = pluginModule.GetCustomAttributes(typeof(PlugInAttribute), false);

				if ((typeAttributes == null) || (typeAttributes.Length == 0))
					throw new PlugInNotValidException(pluginModule.GetName().Name);

				// Get the public types.				
				foreach (Type type in pluginModule.ManifestModule.FindTypes(
					delegate(Type plugInType, object filter)
					{
						object[] attributes;		// Get attributes for the type.

						attributes = plugInType.GetCustomAttributes(typeof(PlugInAttribute), true);

						if ((attributes == null) || (attributes.Length == 0))
							return false;

						return true;
					}, null))
				{
					// Create the plug-in entry class.						
					entry = type.Assembly.CreateInstance(type.Namespace + "." + type.Name, false, BindingFlags.CreateInstance, null, new object[] { pluginModule.Location }, null, null) as PlugInEntryPoint;

					if (entry == null)
						throw new ApplicationException("Unable to create the plug-in entry point.");

					Gorgon.Log.Print("PlugInManager", "Plug-in type: {0}, Module: {1}.", LoggingLevel.Intermediate, entry.PlugInType.ToString(), entry.GetType().ToString());

					// Match name or load all plug-ins from the assembly. 
					if ((pluginName == string.Empty) || (string.Compare(pluginName,entry.Name, true) == 0))
					{
						Gorgon.Log.Print("PlugInManager", "Loading plug-in \"{0}\".", LoggingLevel.Simple, entry.Name);

						if (!_plugIns.Contains(entry.Name))
							_plugIns.Add(entry);

						Gorgon.Log.Print("PlugInManager", "Plug-in \"{0}\" was loaded successfully.", LoggingLevel.Simple, entry.Name);

						if (pluginName != string.Empty)
							return entry;
					}
				}
				
				if (pluginName != string.Empty)
					throw new PlugInNotFoundException(pluginName);

				return null;
			}
			catch (PlugInNotValidException)
			{
				throw;
			}
			catch (PlugInNotFoundException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new PlugInCannotCreateException(pluginName, ex);
			}
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
		/// <returns>The plug-in interface.</returns>
        public static PlugInEntryPoint Load(Assembly pluginModule, string pluginName)
        {
            return Load(pluginModule, pluginName, null);
        }

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <returns>The plug-in interface.</returns>
		public static void Load(Assembly pluginModule)
		{
			Load(pluginModule, null, null);
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
        /// <param name="parameters">Parameters for construction.</param>
		/// <returns>The plug-in interface.</returns>
		public static PlugInEntryPoint Load(string pluginDLL, string pluginName, object[] parameters)
		{
			Assembly pluginModule = null;		// Module containing the plug-in.

			if (!string.IsNullOrEmpty(pluginDLL))
			{
				if (pluginDLL.IndexOf(", Version=") > -1)
					pluginModule = _modules.LoadModule(pluginDLL);
				else
					pluginModule = _modules.LoadModule(Path.GetFullPath(pluginDLL));
			}

			// Load the module.			
			return Load(pluginModule, pluginName, parameters);
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
		/// <returns>Object wrapped in the plug-in.</returns>        
        public static PlugInEntryPoint Load(string pluginDLL, string pluginName)
        {
            return Load(pluginDLL, pluginName, null);
        }

		/// <summary>
		/// Function to load multiple plug-ins from a plug-in DLL.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-ins.</param>        
		/// <returns>The loaded plug-in.</returns>
        public static void Load(string pluginDLL)
        {
            Load(pluginDLL, null, null);
        }

		/// <summary>
		/// Function to destroy a plug-in.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in to unload.</param>
		public static void Unload(string plugInName)
		{
			_plugIns.Remove(plugInName);
		}

		/// <summary>
		/// Function to remove all plug-ins.
		/// </summary>
		public static void DestroyAll()
		{
			_plugIns.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static PlugInFactory()
		{
			_modules = new PlugInModuleList();
			_plugIns = new PlugInList();
		}
		#endregion	
	}
}
