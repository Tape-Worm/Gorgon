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
	/// Object to repesent a manager for plugins.
	/// </summary>
	public class PlugInManager
		: BaseHashMap<PlugInEntryPoint>, IDisposable
	{
		#region Variables.
		private PlugInModuleList _modules;		// List of modules.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove all the plug-ins.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The plug-in interface.</returns>
		public PlugInEntryPoint Load(Assembly pluginModule, string pluginName, object[] parameters)
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
                if (Contains(pluginName))
                    return _items[pluginName];

				Gorgon.Log.Print("PlugInManager","Loading plug-in \"{0}\".", LoggingLevel.Simple, pluginName);

				// Get attributes for this module.
				typeAttributes = pluginModule.GetCustomAttributes(typeof(PlugInAttribute), false);

				if ((typeAttributes == null) || (typeAttributes.Length == 0))
					throw new NotAPlugInException(pluginModule.GetName().Name, null);

				// Get the public types.				
				foreach (Type type in pluginModule.ManifestModule.FindTypes(
					delegate(Type plugInType,object filter)
					{
						return (typeof(PlugInEntryPoint).IsAssignableFrom(plugInType)) && (!plugInType.IsAbstract);
					}, null))
				{
					// Create the plug-in entry class.						
					entry = (PlugInEntryPoint)type.Assembly.CreateInstance(type.Namespace + "." + type.Name, false, BindingFlags.CreateInstance, null, new object[] { pluginModule.Location },null,null);
					Gorgon.Log.Print("PlugInManager","Plug-in type: {0}, Module: {1}.", LoggingLevel.Intermediate, entry.PlugInType.ToString(),entry.GetType().ToString());

					// Match name.                    
                    if ((pluginName == string.Empty) || (pluginName.ToUpper() == entry.Name.ToUpper()))
                    {
						if (!Contains(entry.Name))
						{
							_items.Add(entry.Name, entry);
							Gorgon.Log.Print("PlugInManager", "Plug-in \"{0}\" was loaded successfully.", LoggingLevel.Simple, pluginName);
						}
						else
							return _items[entry.Name];

						return entry;
                    }
				}

				throw new PlugInNotFoundException(pluginName, null);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotCreatePlugInException(pluginName, ex);
			}
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The plug-in interface.</returns>
		public PlugInEntryPoint Load(Assembly pluginModule, object[] parameters)
		{
			return Load(pluginModule, null, parameters);
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
		/// <returns>The plug-in interface.</returns>
        public PlugInEntryPoint Load(Assembly pluginModule, string pluginName)
        {
            return Load(pluginModule, pluginName, null);
        }

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginModule">Module containing the plug-in.</param>
		/// <returns>The plug-in interface.</returns>
		public PlugInEntryPoint Load(Assembly pluginModule)
		{
			return Load(pluginModule, null, null);
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
        /// <param name="parameters">Parameters for construction.</param>
		/// <returns>The plug-in interface.</returns>
		public PlugInEntryPoint Load(string pluginDLL, string pluginName, object[] parameters)
		{
			Assembly pluginModule = null;		// Module containing the plug-in.

			if ((pluginDLL != null) && (pluginDLL != string.Empty))
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
        public PlugInEntryPoint Load(string pluginDLL, string pluginName)
        {
            return Load(pluginDLL, pluginName, null);
        }

		/// <summary>
		/// Function to load multiple plug-ins from a plug-in DLL.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-ins.</param>
        /// <param name="parameters">Constructor parameters.</param>
		/// <returns>The loaded plug-in.</returns>
		public PlugInEntryPoint Load(string pluginDLL, object[] parameters)
		{
			Assembly pluginModule = null;		// Module containing the plug-in.

			if ((pluginDLL != null) && (pluginDLL != string.Empty))
			{
				if (pluginDLL.IndexOf(", Version=") > -1)
					pluginModule = _modules.LoadModule(pluginDLL);
				else
					pluginModule = _modules.LoadModule(Path.GetFullPath(pluginDLL));
			}

			return Load(pluginModule, null, parameters);
		}

		/// <summary>
		/// Function to load multiple plug-ins from a plug-in DLL.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-ins.</param>        
		/// <returns>The loaded plug-in.</returns>
        public PlugInEntryPoint Load(string pluginDLL)
        {
            return Load(pluginDLL, (object[])null);
        }

		/// <summary>
		/// Function to destroy a plug-in.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in to unload.</param>
		public void Unload(string plugInName)
		{
			if (!Contains(plugInName))
				throw new SharpUtilities.Collections.KeyNotFoundException(plugInName);
			
			_items.Remove(plugInName);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal PlugInManager()
            : base(31)
		{
			_modules = new PlugInModuleList();
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to clean up all objects, FALSE to only clean up unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Clear();
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
 			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
