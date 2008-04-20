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
// Created: Monday, May 01, 2006 4:57:53 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Object representing a list of modules that contain plug-ins.
	/// </summary>
	internal class PlugInModuleList
		: BaseCollection<Assembly>
	{
		#region Properties.
		/// <summary>
		/// Property to return an assembly from this list.
		/// </summary>
		/// <param name="key">Name of the assembly.</param>
		/// <returns>An assembly in the list.</returns>
		public Assembly this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}

		/// <summary>
		/// Property to return an assembly from this list.
		/// </summary>
		/// <param name="index">Index of the assembly.</param>
		/// <returns>An assembly in the list.</returns>
		public Assembly this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to test the current app domain and determine if an assembly is already loaded.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to check for.</param>
		/// <returns>The assembly, or NULL if it doesn't exist in the application domain.</returns>
		private Assembly AssemblyExists(string assemblyName)
		{
			Assembly[] assemblies = null;		// List of assemblies.

			// Get the current assembly list.
			assemblies = AppDomain.CurrentDomain.GetAssemblies();

			if ((assemblies == null) || (assemblies.Length == 0))
				return null;

			// Find the assembly.
			foreach (Assembly assembly in assemblies)
			{
				if (assemblyName.IndexOf(", Version=") > -1)
				{
					if (string.Compare(assembly.FullName, assemblyName, true) == 0)
						return assembly;
				}
				else
				{
					if (string.Compare(assembly.ManifestModule.FullyQualifiedName, assemblyName, true) == 0)
						return assembly;
				}
			}

			return null;
		}

		/// <summary>
		/// Function to return whether a key exists in the collection or not.
		/// </summary>
		/// <param name="key">Key of the object in the collection.</param>
		/// <returns>TRUE if the object exists, FALSE if not.</returns>
		public override bool Contains(string key)
		{
			return base.Contains(key.ToUpper());
		}

		/// <summary>
		/// Function to load a module.
		/// </summary>
		/// <param name="moduleName">Name of the module to load.</param>
		/// <returns>Assembly that was loaded.</returns>
		public Assembly LoadModule(string moduleName)
		{
			Assembly newModule;			// Module to load.

			try
			{	
				Gorgon.Log.Print("PlugInModuleList","Attempting to load plug-in module \"{0}\".", LoggingLevel.Intermediate,moduleName);							
				newModule = AssemblyExists(moduleName);

				// If the module is not in the application domain, then load it.
				if (newModule == null)
				{
					// Load using the full name of the assembly.
					if (moduleName.IndexOf(", Version=") > -1)
						newModule = Assembly.Load(moduleName);
					else
						newModule = Assembly.LoadFile(moduleName);
				}
				else
					Gorgon.Log.Print("PlugInModuleList", "Plug-in module already exists, using version from the current app domain.", LoggingLevel.Verbose);

				// If it's not in our list, then add it, else get its reference.
				if (!Contains(newModule.FullName))
					AddItem(newModule.FullName, newModule);

				Gorgon.Log.Print("PlugInModuleList", "Plug-in module \"{0}\" loaded successfully.", LoggingLevel.Intermediate, newModule.FullName);
				return newModule;
			}
			catch (Exception ex)
			{
				throw new ModuleCannotLoadException(moduleName, ex);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal PlugInModuleList()
			: base(16, false)
		{
		}
		#endregion
	}
}
