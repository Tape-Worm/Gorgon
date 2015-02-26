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
// Created: Thursday, June 23, 2011 11:22:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// A cache to hold recently loaded assemblies so we don't load them over and over.
	/// </summary>
	static class AssemblyCache
	{
		#region Variables.
		private static readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();		// List of loaded assemblies.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the currently loaded assemblies in the current app domain.
		/// </summary>
		private static void GetAssemblies()
		{
		    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

		    if (assemblies.Length == 0)
		    {
		        return;
		    }

		    Gorgon.Log.Print("Assembly list is empty.  Retrieving assembly list...", LoggingLevel.Verbose);

			// Weed out already cached assemblies and those that are involved in the creation of dynamic assemblies (throws exception).
			foreach (Assembly assembly in assemblies.Where(assembly => !_assemblies.ContainsKey(assembly.FullName)
				&& (!(assembly is System.Reflection.Emit.AssemblyBuilder)
				&& (!string.Equals(assembly.GetType().FullName, "System.Reflection.Emit.InternalAssemblyBuilder", StringComparison.OrdinalIgnoreCase)))))
			{
			    _assemblies.Add(assembly.FullName, assembly);
			    Gorgon.Log.Print("Added Assembly '{0}' from {1}", LoggingLevel.Verbose, assembly.FullName, assembly.EscapedCodeBase);
			}

			Gorgon.Log.Print("{0} assemblies available.", LoggingLevel.Verbose, _assemblies.Count);
		}

		/// <summary>
		/// Function to load an assembly holding a plug-in.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <returns>The loaded assembly.</returns>
		public static Assembly LoadAssembly(AssemblyName assemblyName)
		{
		    if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			if (_assemblies.ContainsKey(assemblyName.FullName))
			{
				Gorgon.Log.Print("Plug-in assembly '{0}' from {1} is already loaded.  Using this assembly.", LoggingLevel.Simple, assemblyName.FullName, assemblyName.EscapedCodeBase);
				return _assemblies[assemblyName.FullName];
			}

			Gorgon.Log.Print("Loading plug-in assembly '{0}' from {1}", LoggingLevel.Simple, assemblyName.FullName, assemblyName.EscapedCodeBase);

			Assembly assembly = Assembly.Load(assemblyName);

			if (!_assemblies.ContainsKey(assembly.FullName))
			{
				_assemblies.Add(assembly.FullName, assembly);
			}

			Gorgon.Log.Print("Plug-in assembly '{0}' loaded successfully.", LoggingLevel.Simple,
				                assemblyName.FullName);

			return assembly;
		}

		/// <summary>
		/// Initializes the <see cref="AssemblyCache"/> class.
		/// </summary>
		static AssemblyCache()
		{
			GetAssemblies();
		}
		#endregion
	}
}
