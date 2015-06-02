#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, June 23, 2011 11:23:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Core.Collections.Specialized;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Plugins
{
	/// <summary>
    /// The return values for the <see cref="GorgonPlugInFactory.IsAssemblySigned(System.Reflection.AssemblyName,byte[])">IsAssemblySigned</see> method.
	/// </summary>
	[Flags]
	public enum PlugInSigningResult
	{
		/// <summary>
		/// Assembly is not signed.  This flag is mutally exclusive.
		/// </summary>
		NotSigned = 1,
		/// <summary>
		/// Assembly is signed, and if it was requested, the key matches.
		/// </summary>
		Signed = 2,
		/// <summary>
		/// This flag is combined with the Signed flag to indicate that it was signed, but the keys did not match.
		/// </summary>
		KeyMismatch = 4
	}

	/// <summary>
	/// A service to create <see cref="GorgonPlugIn"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this object to control loading and unloading of plugins.
	/// </para>
	/// <para>
	/// In some cases, a plugin assembly may have issues when loading an assembly. Such as a type not being found, or a type in the assembly refusing to instantiate. In these cases 
	/// use the <see cref="GorgonPlugInService.AssemblyResolver">AssemblyResolver</see> property to assign a method that will attempt to resolve any dependency 
	/// assemblies.
	/// </para>
	/// </remarks>
	public class GorgonPlugInService
	{
		#region Variables.
		// The list of plug-ins previously created.
		private GorgonNamedObjectDictionary<GorgonPlugIn> _plugins = new GorgonNamedObjectDictionary<GorgonPlugIn>(false);
		// The application log file.
		private IGorgonLog _log = new GorgonLogDummy();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a plugin by its name.
		/// </summary>
		/// <param name="name">The friendly name of the plugin or the fully qualified type name of the plugin.</param>
		public GorgonPlugIn this[string name]
		{
			get
			{
				return Items[name];
			}
		}
		#endregion

		#region Methods.

        

		/// <summary>
		/// Function to determine if a plugin implements <see cref="System.IDisposable">IDisposable</see> and dispose the object if it does.
		/// </summary>
		/// <param name="plugIn">Plug-in to check and dispose.</param>
		private static void CheckDisposable(GorgonPlugIn plugIn)
		{
			var disposer = plugIn as IDisposable;

			if (disposer != null)
			{
				disposer.Dispose();
			}
		}

		/// <summary>
		/// Function to retrieve the list of plugins associated with a specific assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to filter.</param>
		/// <returns>A read-only list of plugins.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>Unlike the overload of this method, this method only enumerates plugins from assemblies that are already loaded into memory.</remarks>
		public IReadOnlyList<GorgonPlugIn> EnumeratePlugIns(AssemblyName assemblyName)
		{
			GorgonDebug.AssertNull(assemblyName, "assemblyName");

			return this.Where(item => AssemblyName.ReferenceMatchesDefinition(item.Assembly, assemblyName)).ToArray();
		}

		/// <summary>
		/// Function to unload all the plugins.
		/// </summary>
		public void UnloadAll()
		{
			// Unload our discovery domain.
			PurgeCachedPlugInInfo();

			foreach (var item in Items)
			{
				CheckDisposable(item.Value);
			}
			
			Items.Clear();
		}

		/// <summary>
		/// Function to remove a plugin by index.
		/// </summary>
		/// <param name="name">Name of the plugin to remove.</param>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="name"/> was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">The name was an empty string..</exception>
		public void Unload(string name)
		{
			GorgonDebug.AssertParamString(name, "name");

			Items.Remove(name);
		}

		/// <summary>
		/// Function to remove a plugin.
		/// </summary>
		/// <param name="plugIn">Plug-in to remove.</param>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="plugIn"/> parameter was NULL (Nothing in VB.Net).</exception>
		public void Unload(GorgonPlugIn plugIn)
		{
		    if (plugIn == null)
		    {
		        throw new ArgumentNullException("plugIn");
		    }

		    RemoveItem(plugIn);
		}

		/// <summary>
		/// Function to determine if an assembly is signed, and optionally, signed with the correct public key.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to check.</param>
		/// <param name="publicKey">[Optional] Public key to compare, or NULL (Nothing in VB.Net) to bypass the key comparison.</param>
		/// <returns>One of the values in the <seealso cref="PlugInSigningResult">PlugInSigningResult</seealso> enumeration.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		public PlugInSigningResult IsAssemblySigned(AssemblyName assemblyName, byte[] publicKey = null)
		{
		    var result = PlugInSigningResult.Signed;

		    if (assemblyName == null)
		    {
		        throw new ArgumentNullException("assemblyName");
		    }

		    byte[] plugInPublicKey = assemblyName.GetPublicKey();

		    if ((plugInPublicKey == null) || (plugInPublicKey.Length < 1))
		    {
		        return PlugInSigningResult.NotSigned;
		    }

			if (publicKey == null)
			{
				return result;
			}

			if (publicKey.Length != plugInPublicKey.Length)
			{
				result |= PlugInSigningResult.KeyMismatch;
			}
			else
			{
				for (int i = 0; i < publicKey.Length - 1; i++)
				{
					if (publicKey[i] == plugInPublicKey[i])
					{
						continue;
					}

					result |= PlugInSigningResult.KeyMismatch;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to determine if an assembly is signed, and optionally, signed with the correct public key.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly to check.</param>
		/// <param name="publicKey">[Optional] Public key to compare, or NULL (Nothing in VB.Net) to bypass the key comparison.</param>
		/// <returns>One of the values in the <seealso cref="PlugInSigningResult">PlugInSigningResult</seealso> enumeration.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="assemblyPath"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the <see cref="P:GorgonLibrary.PlugIns.GorgonPlugInFactory.SearchPaths">search paths</see> (including the path provided in the parameter).</exception>
		public PlugInSigningResult IsAssemblySigned(string assemblyPath, byte[] publicKey = null)
		{
			return IsAssemblySigned(FindPlugInAssembly(assemblyPath), publicKey);
		}

		/// <summary>
		/// Function to load a plugin assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly.</param>
		/// <remarks>If the assembly file cannot be found, then the paths in the <see cref="P:GorgonLibrary.PlugIns.GorgonPlugInFactory.SearchPaths">SearchPaths</see> collection are used to find the assembly.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyPath"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="assemblyPath"/> is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the search paths (including the path provided in the parameter).</exception>
		/// <exception cref="GorgonException">The assembly contains a plugin type that was already loaded by another assembly.</exception>
		/// <returns>The fully qualified assembly name object for the assembly being loaded.</returns>
		public AssemblyName LoadPlugInAssembly(string assemblyPath)
		{
		    AssemblyName plugInAssemblyName = FindPlugInAssembly(assemblyPath);

			LoadPlugInAssembly(plugInAssemblyName);

			return plugInAssemblyName;
		}


		/// <summary>
		/// Function to load a plugin assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonException">The assembly contains a plugin type that was already loaded by another assembly.</exception>
		public void LoadPlugInAssembly(AssemblyName assemblyName)
		{
			if (assemblyName == null)
		    {
		        throw new ArgumentNullException("assemblyName");
		    }
			
			Assembly plugInAssembly = GorgonPluginAssemblyCache.Load(assemblyName);

			try
			{

				// Get all plugin types from the assembly.
				var plugInTypes = (from plugInType in plugInAssembly.GetTypes()
				                   where (plugInType.IsSubclassOf(typeof(GorgonPlugIn)) && (!plugInType.IsAbstract))
				                   select plugInType).ToArray();

				if (plugInTypes.Length == 0)
				{
					throw new ArgumentException(string.Format(Resources.GOR_PLUGIN_NOT_PLUGIN_ASSEMBLY, assemblyName.FullName),
					                            "assemblyName");
				}

				// Create an instance of each plugin object.
				foreach (Type plugInType in plugInTypes)
				{
					var plugIn =
						(GorgonPlugIn)
						plugInType.Assembly.CreateInstance(plugInType.FullName, false, BindingFlags.CreateInstance, null, null, null, null);

					if (plugIn == null)
					{
						throw new GorgonException(GorgonResult.CannotCreate,
						                          string.Format(Resources.GOR_PLUGIN_CANNOT_CREATE, plugInType.FullName,
						                                        plugInType.Assembly.FullName));
					}

					if (!Contains(plugIn.Name))
					{
						GorgonApplication.Log.Print("Plug-in '{0}' created.", LoggingLevel.Simple, plugIn.Name);
						Items.Add(plugIn.Name, plugIn);
					}
					else
					{
						if (plugInType != this[plugIn.Name].GetType())
						{
							throw new GorgonException(GorgonResult.CannotCreate,
							                          string.Format(Resources.GOR_PLUGIN_CONFLICT, plugIn.Name,
							                                        plugInType.Assembly.FullName,
							                                        this[plugIn.Name].GetType().Assembly.FullName));
						}

						GorgonApplication.Log.Print("Plug-in '{0}' already created.  Using this instance.", LoggingLevel.Simple, plugIn.Name);
					}
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				var errorMessage = new StringBuilder(512);

				foreach (Exception loadEx in ex.LoaderExceptions)
				{
					if (errorMessage.Length > 0)
					{
						errorMessage.Append("\n\r");
					}

					errorMessage.Append(loadEx.Message);
				}

				throw new GorgonException(GorgonResult.CannotRead,
				                          string.Format(Resources.GOR_PLUGIN_TYPE_LOAD_FAILURE, plugInAssembly.FullName,
				                                        errorMessage));
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPlugInFactory"/> class.
		/// </summary>
		public GorgonPlugInFactory()
			: base(false)
		{
			_paths = new GorgonPlugInPathCollection();
		}
		#endregion
	}
}
