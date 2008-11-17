#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, November 14, 2008 10:31:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// A collection of plug-in modules.
	/// </summary>
	/// <remarks>Plug-in modules cannot be unloaded.  This is due to the fact that assemblies cannot be
	/// removed from an application until the application is closed.</remarks>
	internal class PlugInModuleCollection
		: IEnumerable<Assembly>
	{
		#region Variables.
		private Dictionary<string, Assembly> _modules = null;			// Module list.
		#endregion

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
				return _modules[key.ToLower()];
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
		/// Function to load a module.
		/// </summary>
		/// <param name="moduleName">Name of the module to load.</param>
		/// <param name="mustBeSigned">TRUE if the assembly should be signed, FALSE if not.</param>
		/// <param name="key">Key used to sign the assembly.</param>
		/// <returns>Assembly that was loaded.</returns>
		public Assembly LoadModule(string moduleName, bool mustBeSigned, byte[] key)
		{
			Assembly newModule;			// Module to load.

			Gorgon.Log.Print("Attempting to load plug-in module \"{0}\".", LoggingLevel.Intermediate,moduleName);							
			newModule = AssemblyExists(moduleName);

			// If the module is not in the application domain, then load it.
			if (newModule == null)
			{
				// Load using the full name of the assembly.
				if (moduleName.IndexOf(", Version=") > -1)
					newModule = Assembly.Load(moduleName);
				else
					newModule = Assembly.LoadFrom(moduleName);
			}
			else
				Gorgon.Log.Print("Plug-in module already exists, using version from the current app domain.", LoggingLevel.Verbose);

			if (mustBeSigned)
			{
				byte[] assemblyKey = null;			// Key signed to the assembly.
				assemblyKey = newModule.GetName().GetPublicKey();

				if (assemblyKey == null)
					throw new GorgonException(GorgonErrors.ModuleNotSigned, moduleName + " is missing its key.");

				if (key != null)
				{
					if (assemblyKey.Length != key.Length)
						throw new GorgonException(GorgonErrors.ModuleKeyMismatch, moduleName + " does not have the same key signature.");

					for (int i = 0; i < assemblyKey.Length; i++)
					{
						if (assemblyKey[i] != key[i])
							throw new GorgonException(GorgonErrors.ModuleKeyMismatch, moduleName + " does not have the same key signature.");
					}
				}
			}

			// If it's not in our list, then add it, else get its reference.
			if (!Contains(newModule.FullName))
				_modules.Add(newModule.FullName.ToLower(), newModule);

			Gorgon.Log.Print("Plug-in module \"{0}\" loaded successfully.", LoggingLevel.Intermediate, newModule.FullName);
			return newModule;
		}

		/// <summary>
		/// Function to determine if this collection contains the assembly.
		/// </summary>
		/// <param name="name">Name of the assembly.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(string name)
		{
			return _modules.ContainsKey(name.ToLower());
		}		
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInModuleCollection"/> class.
		/// </summary>
		internal PlugInModuleCollection()
		{
			_modules = new Dictionary<string, Assembly>();
		}
		#endregion

		#region IEnumerable<Assembly> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<Assembly> GetEnumerator()
		{
			foreach (KeyValuePair<string, Assembly> item in _modules)
				yield return item.Value;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
