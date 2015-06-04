#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, January 5, 2013 1:40:39 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gorgon.Plugins
{
	/// <summary>
	/// Plug-in verification object.
	/// </summary>
	class GorgonPluginVerifier
		: MarshalByRefObject
	{
		#region Variables.
		// The assembly for this type.
	    private Assembly _assembly;
		// Type info for the Gorgon Plug-In type.
		private Type _plugInType;				
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the Gorgon plug-in type.
		/// </summary>
		/// <remarks>This loads the base Gorgon assembly as a reflection only library.  This is done to extract specific reflection only 
		/// information for type comparisons later on.</remarks>
		private void GetGorgonPlugInType()
		{
			if (_plugInType != null)
			{
				return;
			}

			AssemblyName gorgonAssemblyName = typeof(GorgonPluginVerifier).Assembly.GetName();
			Assembly gorgonReflection = Assembly.ReflectionOnlyLoad(gorgonAssemblyName.FullName);

			// Get the Gorgon reflection only plug-in type.
			_plugInType = gorgonReflection.GetTypes().Single(item => typeof(GorgonPlugin).FullName == item.FullName);

			_assembly = _plugInType.Assembly;
		}

        /// <summary>
        /// Function to retrieve the assembly from the same directory as the requesting assembly.
        /// </summary>
        /// <param name="name">Name of the assembly.</param>
        /// <param name="requesting">Requesting assembly.</param>
        /// <returns>The assembly if found, NULL if not.</returns>
        private Assembly GetFromRequestedDir(AssemblyName name, Assembly requesting)
        {
            var locations = new string[requesting == null ? 1 : 2];
            Assembly result = null;

            // Check locally first.
            locations[0] = Path.GetDirectoryName(_assembly.Location) +
                           Path.DirectorySeparatorChar +
                           name.Name;

            if (locations.Length > 1)
            {
                // Otherwise, check in the requesting assembly location.
                // ReSharper disable once PossibleNullReferenceException
                locations[1] = Path.GetDirectoryName(requesting.Location) +
                           Path.DirectorySeparatorChar +
                           name.Name;
            }

            foreach (string assemblyLocation in locations)
            {
                // Search for DLLs.
                string location = Path.ChangeExtension(assemblyLocation, "dll");

                if (File.Exists(location))
                {
                    return Assembly.ReflectionOnlyLoadFrom(location);
                }

                // Search for executables.
                location = Path.ChangeExtension(location, "exe");

                result = File.Exists(location) ? Assembly.ReflectionOnlyLoadFrom(location) : null;

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }
        
        /// <summary>
	    /// Handles the ReflectionOnlyAssemblyResolve event of the CurrentDomain control.
	    /// </summary>
	    /// <param name="sender">The source of the event.</param>
	    /// <param name="args">The <see cref="ResolveEventArgs" /> instance containing the event data.</param>
	    /// <returns></returns>
	    private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
	    {
            var name = new AssemblyName(args.Name);
            Assembly result;

            // Try from the GAC first.
            try
            {
                result = Assembly.ReflectionOnlyLoad(args.Name);

                return result;
            }
            catch (FileNotFoundException)
            {
                // Eat this exception.
            }

            // We couldn't find the assembly in the requesting assembly directory, move on the to current.
            result = GetFromRequestedDir(name, args.RequestingAssembly);
            
            return result;
		}

		/// <summary>
		/// Function to load plug-in type names from the specified assembly file.
		/// </summary>
		/// <param name="name">Name of the assembly to check.</param>
		/// <returns>A list of plug-in type names.</returns>
		public string[] GetPlugInTypes(AssemblyName name)
		{
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            try
            {
		        GetGorgonPlugInType();

	            var assembly = Assembly.ReflectionOnlyLoadFrom(name.CodeBase);
                var types = assembly.GetTypes();

                return (from type in types
                        where type.IsSubclassOf(_plugInType) && !type.IsAbstract
                        select type.FullName).ToArray();
            }
            catch (ReflectionTypeLoadException)
            {
                return new string[0];
            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentDomain_ReflectionOnlyAssemblyResolve;
            }
        }

		/// <summary>
		/// Function to determine if an assembly contains plug-ins.
		/// </summary>
		/// <param name="name">Name of the assembly to check.</param>
		/// <returns><b>true</b> if plug-ins are found, <b>false</b> if not.</returns>
		public bool IsPlugInAssembly(AssemblyName name)
		{
            return GetPlugInTypes(name).Length > 0;
        }
		#endregion
	}
}
