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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using GorgonLibrary.Collections;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// A collection to add, remove and keep track of plug-in interfaces.
	/// </summary>
	/// <remarks>Use this object to control loading and unloading of plug-ins.
	/// <para>This collection is not case-sensitive.</para></remarks>
	public class GorgonPlugInFactory
		: GorgonBaseNamedObjectCollection<GorgonPlugIn>
	{
		#region Variables.
		private static GorgonPlugInFactory _factory = null;			// Factory instance.
		private static GorgonPlugInPathCollection _paths = null;	// Search paths for the plug-in assemblies.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of plug-in types available for use.
		/// </summary>
		public static GorgonPlugInFactory PlugIns
		{
			get
			{
				if (_factory == null)
					_factory = new GorgonPlugInFactory();

				return _factory;
			}
		}

		/// <summary>
		/// Property to return the list of search paths to use.
		/// </summary>
		/// <remarks>The plug-in factory uses these paths to search for the plug-in when the plug-in cannot be found.
		/// <para>By default, the plug-in factory checks (in order):
		/// <list type="number">
		/// <item><description>The directory of the executable.</description></item>
		/// <item><description>The working directory of the executable.</description></item>
		/// <item><description>The system directory.</description></item>
		/// <item><description>The directories listed in the PATH environment variable.</description></item>
		/// </list>
		/// </para>
		/// </remarks>
		public static GorgonPlugInPathCollection SearchPaths
		{
			get
			{
				if (_paths == null)
					_paths = new GorgonPlugInPathCollection();

				return _paths;
			}
		}

		/// <summary>
		/// Property to return a plug-in by its index in the list.
		/// </summary>
		/// <param name="index">Index of the plug-in.</param>
		public GorgonPlugIn this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a plug-in by its name.
		/// </summary>
		/// <param name="name">The friendly name of the plug-in or the fully qualified type name of the plug-in.</param>
		public GorgonPlugIn this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if a plug-in implements <see cref="System.IDisposable">IDisposable</see> and dispose the object if it does.
		/// </summary>
		/// <param name="plugIn">Plug-in to check and dispose.</param>
		private void CheckDisposable(GorgonPlugIn plugIn)
		{
			IDisposable disposer = plugIn as IDisposable;

			if (disposer != null)
				disposer.Dispose();
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		protected override void RemoveItem(int index)
		{
			CheckDisposable(this[index]);
			base.RemoveItem(index);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected override void RemoveItem(GorgonPlugIn item)
		{
			CheckDisposable(item);
			base.RemoveItem(item);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		protected override void RemoveItem(string name)
		{
			CheckDisposable(this[name]);
			base.RemoveItem(name);
		}

		/// <summary>
		/// Function to remove all the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (GorgonPlugIn plugIn in this)
				CheckDisposable(plugIn);
			
			base.ClearItems();
		}

		/// <summary>
		/// Function to retrieve the list of plug-ins associated with a specific assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to filter.</param>
		/// <returns>A read-only list of plug-ins.</returns>
		public static GorgonNamedObjectReadOnlyCollection<GorgonPlugIn> GetPlugInsFrom(AssemblyName assemblyName)
		{
			List<GorgonPlugIn> plugIns = new List<GorgonPlugIn>();

			foreach (GorgonPlugIn plugIn in PlugIns)
			{
				Type plugInType = plugIn.GetType();

				if (AssemblyName.ReferenceMatchesDefinition(plugInType.Assembly.GetName(), assemblyName))
					plugIns.Add(plugIn);
			}

			return new GorgonNamedObjectReadOnlyCollection<GorgonPlugIn>(false, plugIns);
		}

		/// <summary>
		/// Function to unload all the plug-ins.
		/// </summary>
		public static void UnloadAll()
		{
			PlugIns.ClearItems();
		}

		/// <summary>
		/// Function to remove a plug-in by index.
		/// </summary>
		/// <param name="name">Name of the plug-in to remove.</param>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="name"/> was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">The name was an empty string..</exception>
		public static void Unload(string name)
		{
			GorgonUtility.AssertParamString(name, "name");
			PlugIns.RemoveItem(name);
		}

		/// <summary>
		/// Function to remove a plug-in by index.
		/// </summary>
		/// <param name="index">Index of the plug-in to remove.</param>
		/// <exception cref="System.IndexOutOfRangeException">The <paramRef name="index"/> parameter was less than 0 or greater than or equal to <see cref="P:Engine.PlugIns.EnginePlugInList.Count">Count</see>.</exception>
		public static void Unload(int index)
		{
			PlugIns.RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in to remove.</param>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="plugIn"/> parameter was NULL (Nothing in VB.Net).</exception>
		public static void Unload(GorgonPlugIn plugIn)
		{
			if (plugIn == null)
				throw new ArgumentNullException("plugIn");

			PlugIns.RemoveItem(plugIn);
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyPath"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="assemblyPath"/> is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the search paths (including the path provided in the parameter).</exception>
		/// <returns>The fully qualified assembly name object for the assembly being loaded.</returns>
		public static AssemblyName LoadPlugInAssembly(string assemblyPath)
		{
			return LoadPlugInAssembly(assemblyPath, false, null);
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly.</param>
		/// <param name="mustBeSigned">TRUE if the plug-in assembly requires that it be signed, FALSE to ignore.</param>
		/// <param name="publicKeyCompare">Public key to compare.</param>
		/// <remarks>When passing a public key token, <paramref name="mustBeSigned"/> should be set to TRUE.  Otherwise, the <paramref name="publicKeyCompare"/> parameter will be ignored.
		/// <para>To just check if the assembly is signed, pass NULL (Nothing in VB.NET) to publicKeyToken.</para>
		/// <para>If the assembly file cannot be found, then the paths in the <see cref="P:GorgonLibrary.PlugIns.GorgonPlugInFactory.SearchPaths">SearchPaths</see> collection are used to find the assembly.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyPath"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="assemblyPath"/> is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the search paths (including the path provided in the parameter).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">The assembly contains a plug-in type that was already loaded by another assembly or the <paramref name="mustBeSigned"/> parameter is set to TRUE and there is no key in the assembly
		/// or the assembly key does not match <paramref name="publicKeyCompare"/>.</exception>
		/// <returns>The fully qualified assembly name object for the assembly being loaded.</returns>
		public static AssemblyName LoadPlugInAssembly(string assemblyPath, bool mustBeSigned, byte[] publicKeyCompare)
		{
			AssemblyName plugInAssemblyName = null;

			GorgonUtility.AssertParamString(assemblyPath, "assemblyPath");

			assemblyPath = Path.GetFullPath(assemblyPath);

			if (!File.Exists(assemblyPath))
			{
				assemblyPath = Path.GetFileName(assemblyPath);
				foreach (var path in SearchPaths)
				{
					if (File.Exists(path + assemblyPath))
					{
						assemblyPath = path + assemblyPath;
						break;
					}
				}

				if (!File.Exists(assemblyPath))
					throw new FileNotFoundException("Could not find the plug-in '" + Path.GetFileName(assemblyPath) + "' on any of the search paths.", assemblyPath);
			}

			plugInAssemblyName = AssemblyName.GetAssemblyName(assemblyPath);

			LoadPlugInAssembly(plugInAssemblyName, mustBeSigned, publicKeyCompare);

			return plugInAssemblyName;
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyName"/> is NULL (Nothing in VB.Net).</exception>
		public static void LoadPlugInAssembly(AssemblyName assemblyName)
		{
			LoadPlugInAssembly(assemblyName);
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <param name="mustBeSigned">TRUE if the plug-in assembly requires that it be signed, FALSE to ignore.</param>
		/// <param name="publicKeyCompare">Public key to compare.</param>
		/// <remarks>When passing a public key token, <paramref name="mustBeSigned"/> should be set to TRUE.  Otherwise, the <paramref name="publicKeyCompare"/> parameter will be ignored.
		/// <para>To just check if the assembly is signed, pass NULL (Nothing in VB.NET) to publicKeyToken.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">The assembly contains a plug-in type that was already loaded by another assembly or the <paramref name="mustBeSigned"/> parameter is set to TRUE and there is no key in the assembly
		/// or the assembly key does not match <paramref name="publicKeyCompare"/> or there are no plug-ins in the assembly.</exception>
		public static void LoadPlugInAssembly(AssemblyName assemblyName, bool mustBeSigned, byte[] publicKeyCompare)
		{
			Assembly plugInAssembly = null;
			byte[] publicKey = null;

			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			plugInAssembly = AssemblyCache.LoadAssembly(assemblyName);

			if (mustBeSigned)
			{
				publicKey = plugInAssembly.GetName().GetPublicKey();

				if ((publicKey == null) || (publicKey.Length < 1))
					throw new GorgonException(GorgonResult.AccessDenied, "The assembly '" + plugInAssembly.FullName + "' must be signed.");

				if (publicKeyCompare != null)
				{
					if (publicKeyCompare.Length != publicKey.Length)
						throw new GorgonException(GorgonResult.AccessDenied, "The assembly '" + plugInAssembly.FullName + "' does not have the correct public key.");

					for(int i = 0; i < publicKeyCompare.Length; i++)
					{
						if (publicKeyCompare[i] != publicKey[i])
							throw new GorgonException(GorgonResult.AccessDenied, "The assembly '" + plugInAssembly.FullName + "' does not have the correct public key.");
					}
				}
			}

			try
			{
				var plugInTypes = from plugInType in plugInAssembly.GetTypes()
								  where (plugInType.IsSubclassOf(typeof(GorgonPlugIn)) && (!plugInType.IsAbstract))
								  select plugInType;

				if ((plugInTypes == null) || (plugInTypes.Count() == 0))
					throw new ArgumentException("Not a valid plug-in assembly.  There are no plug-ins in the assembly '" + assemblyName.FullName + "'.", "assemblyName");

				foreach (Type plugInType in plugInTypes)
				{
					GorgonPlugIn plugIn = plugInType.Assembly.CreateInstance(plugInType.FullName, false, BindingFlags.CreateInstance, null, null, null, null) as GorgonPlugIn;
					if (plugIn == null)
						throw new GorgonException(GorgonResult.CannotCreate, "Could not create the plug-in type '" + plugInType.FullName + "' in assembly '" + plugInType.Assembly.FullName + "'.  It was not of type EnginePlugIn.");
					if (!PlugIns.Contains(plugIn.Name))
						PlugIns.AddItem(plugIn);
					else
					{
						if (plugIn.GetType() != PlugIns[plugIn.Name].GetType())
						{
							throw new GorgonException(GorgonResult.CannotCreate, "The plug-in '" + plugIn.Name + "' in assembly '" + plugInType.Assembly.FullName +
									"' already exists in another plug-in assembly '" + PlugIns[plugIn.Name].GetType().Assembly.FullName + "' and is not the same type.");
						}
					}
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				string errorMessage = string.Empty;

				foreach (Exception loadEx in ex.LoaderExceptions)
				{
					if (!string.IsNullOrEmpty(errorMessage))
						errorMessage += "\n\r";
					errorMessage += loadEx.Message;
				}

				throw new GorgonException(GorgonResult.CannotRead, "Cannot read types from the assembly '" + plugInAssembly.FullName + "'\n\r" + errorMessage);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPlugInFactory"/> class.
		/// </summary>
		private GorgonPlugInFactory()
			: base(false)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonPlugInFactory"/> class.
		/// </summary>
		static GorgonPlugInFactory()
		{
			_factory = new GorgonPlugInFactory();
			_paths = new GorgonPlugInPathCollection();
		}
		#endregion
	}
}
