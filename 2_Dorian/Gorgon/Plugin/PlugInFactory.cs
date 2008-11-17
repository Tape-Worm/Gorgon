#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, May 01, 2006 4:21:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Object to repesent a list of plugins.
	/// </summary>
	public static class PlugInFactory
	{
		#region Variables.
		private static PlugInModuleCollection _modules = null;	// List of modules.
		private static PlugInCollection _plugIns = null;				// List of plug-ins.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of previously loaded plug-ins.
		/// </summary>
		public static PlugInCollection PlugIns
		{
			get
			{
				return _plugIns;
			}
		}

		/// <summary>
		/// Property to set or return whether plug-ins must be signed.
		/// </summary>
		public static bool RequireSignedPlugIns
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the plug-ins from a module.
		/// </summary>
		/// <param name="pluginModule">Module to retrieve plug-ins from.</param>
		/// <param name="parameters">Parameters to pass to the plug-in.</param>
		private static void GetPlugInTypes(Assembly pluginModule, object[] parameters)
		{
			PlugIn entry = null;					// Entry point to the plug-in.
			Type[] plugIns = null;					// List of plug-ins in the module.

			// Get the public types.
			plugIns = pluginModule.ManifestModule.FindTypes(
				(plugInType, filter) =>
				{
					return (plugInType.IsSubclassOf(typeof(PlugIn)));
				}, null);

			if (plugIns.Length == 0)
				throw new GorgonException(GorgonErrors.InvalidPlugin, pluginModule.FullName + " does not contain any plug-ins.");

			// Get all the plug-in types and add them.
			foreach (Type type in plugIns)
			{
				// Create the plug-in entry class.
				entry = type.Assembly.CreateInstance(type.Namespace + "." + type.Name, false, BindingFlags.CreateInstance, null, new object[] { pluginModule.Location }, null, null) as PlugIn;

				if (entry == null)
					throw new GorgonException(GorgonErrors.InvalidPlugin, "Unable to create '" + type.FullName + "'.");

				if (!_plugIns.Contains(entry.Name))
				{
					Gorgon.Log.Print("Found plug-in type: {0}, Module: {1}.", LoggingLevel.Intermediate, entry.PlugInType.ToString(), entry.GetType().ToString());
					_plugIns.Add(entry);
				}
			}
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
		/// <param name="parameters">Parameters for construction.</param>
		/// <param name="key">Byte array containing the public key to verify.</param>
		/// <returns>The plug-in interface.</returns>
		/// <remarks>The key argument is only used when <see cref="T:HeraLibrary.PlugInCollection.RequireSignedPlugIns"/> is set to TRUE.  Otherwise it is ignored.</remarks>
		public static PlugIn Load(string pluginDLL, string pluginName, byte[] key, object[] parameters)
		{
			Assembly pluginModule = null;		// Module containing the plug-in.

			if ((RequireSignedPlugIns) && (key != null))
				throw new ArgumentNullException("key");

			if (string.IsNullOrEmpty(pluginDLL))
				throw new ArgumentNullException("pluginDLL");

			if (string.IsNullOrEmpty(pluginName))
				throw new ArgumentNullException("pluginName");

			if (pluginDLL.IndexOf(", Version=") > -1)
				pluginModule = _modules.LoadModule(pluginDLL, RequireSignedPlugIns, key);
			else
				pluginModule = _modules.LoadModule(Path.GetFullPath(pluginDLL), RequireSignedPlugIns, key);

			GetPlugInTypes(pluginModule, parameters);

			if (!_plugIns.Contains(pluginName))
				throw new GorgonException(GorgonErrors.InvalidPlugin, pluginName + " was not found.");

			// Load the module.			
			return _plugIns[pluginName];
		}

		/// <summary>
		/// Function to load a plug-in.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in.</param>
		/// <param name="pluginName">Name of the plug-in to create.</param>
		/// <param name="key">Byte array containing the public key to verify.</param>
		/// <returns>Object wrapped in the plug-in.</returns>        
		/// <remarks>The key argument is only used when <see cref="T:HeraLibrary.PlugInCollection.RequireSignedPlugIns"/> is set to TRUE.  Otherwise it is ignored.</remarks>
		public static PlugIn Load(string pluginDLL, string pluginName, byte[] key)
		{
			return Load(pluginDLL, pluginName, key, null);
		}

		/// <summary>
		/// Function to retrieve all available plug-in metadata from a specified module.
		/// </summary>
		/// <param name="pluginDLL">Path to the DLL containing the plug-in(s).</param>
		/// <param name="key">Key used to sign the plug-in.</param>
		/// <returns>An array with all the plug-in metadata.</returns>
		public static PlugInDescriptionAttribute[] GetPlugInMetaData(string pluginDLL, byte[] key)
		{
			Type[] plugIns = null;
			PlugInDescriptionAttribute[] result = null;
			Assembly pluginModule = null;

			if ((RequireSignedPlugIns) && (key != null))
				throw new ArgumentNullException("key");

			if (string.IsNullOrEmpty(pluginDLL))
				throw new ArgumentNullException("pluginDLL");

			if (pluginDLL.IndexOf(", Version=") > -1)
				pluginModule = _modules.LoadModule(pluginDLL, RequireSignedPlugIns, key);
			else
				pluginModule = _modules.LoadModule(Path.GetFullPath(pluginDLL), RequireSignedPlugIns, key);

			// Get the public types.
			plugIns = pluginModule.ManifestModule.FindTypes(
				(plugInType, filter) =>
				{
					return (plugInType.IsSubclassOf(typeof(PlugIn)));
				}, null);

			if (plugIns.Length == 0)
				return new PlugInDescriptionAttribute[0];

			result = new PlugInDescriptionAttribute[plugIns.Length];

			// Get all the plug-in types and add them.
			for (int i = 0; i < plugIns.Length; i++)
			{
				PlugInDescriptionAttribute[] attribute = null;

				attribute = plugIns[i].GetCustomAttributes(typeof(PlugInDescriptionAttribute), false) as PlugInDescriptionAttribute[];

				if ((attribute == null) || (attribute.Length == 0))
					throw new GorgonException(GorgonErrors.InvalidPlugin, "The plug-in type '" + plugIns[i].FullName + "' has no PlugInDescriptionAttribute");

				result[i] = attribute[0];
			}

			return result;
		}

		/// <summary>
		/// Function to remove a plug-in from the factory.
		/// </summary>
		/// <param name="plugIn">Plug-in to remove.</param>
		public static void Unload(PlugIn plugIn)
		{
			Gorgon.Log.Print("Removing plug-in: {0}.", LoggingLevel.Intermediate, plugIn.Name);
			_plugIns[plugIn.Name].Unload();
			_plugIns.Remove(plugIn.Name);
		}

		/// <summary>
		/// Function to remove a plug-in by its index.
		/// </summary>
		/// <param name="index">Index of the plug-in to remove.</param>
		public static void Unload(int index)
		{
			Gorgon.Log.Print("Removing plug-in: {0}.", LoggingLevel.Intermediate, _plugIns[index].Name);
			_plugIns[index].Unload();
			_plugIns.Remove(index);
		}

		/// <summary>
		/// Function to remove a plug-in by its name.
		/// </summary>
		/// <param name="name">Name of the plug-in to unload.</param>
		public static void Unload(string name)
		{
			Gorgon.Log.Print("Removing plug-in: {0}.", LoggingLevel.Intermediate, name);
			_plugIns[name].Unload();
			_plugIns.Remove(name);
		}


		/// <summary>
		/// Function to clear the plug-in list.
		/// </summary>
		public static void UnloadAll()
		{
			Gorgon.Log.Print("Removing all plug-ins.", LoggingLevel.Intermediate);
			foreach (PlugIn plugIn in _plugIns)
				plugIn.Unload();
			_plugIns.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static PlugInFactory()
		{
			_modules = new PlugInModuleCollection();
			_plugIns = new PlugInCollection();
		}
		#endregion	
	}
}
