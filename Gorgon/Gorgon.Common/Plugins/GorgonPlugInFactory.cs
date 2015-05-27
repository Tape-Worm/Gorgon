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
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core.Collections;
using Gorgon.Diagnostics;

namespace Gorgon.PlugIns
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
	/// A factory to load, unload and keep track of plug-in interfaces.
	/// </summary>
	/// <remarks>Use this object to control loading and unloading of plug-ins.  It is exposed as the <see cref="P:GorgonLibrary.GorgonApplication.PlugIns">PlugIns</see> parameter on the primary 
	/// <seealso cref="Gorgon">Gorgon</seealso> object and cannot be created by the user.
	/// <para>In some cases, a plug-in assembly may have issues when loading an assembly. Such as a type not being found, or a type in the assembly refusing to instantiate. In these cases 
	/// use the <see cref="GorgonApplication.PlugIns.GorgonPlugInFactory.AssemblyResolver">AssemblyResolver</see> property to assign a method that will attempt to resolve any dependency 
	/// assemblies.</para></remarks>
	public class GorgonPlugInFactory
		: GorgonBaseNamedObjectDictionary<GorgonPlugIn>
	{
		#region Variables.
		private GorgonPlugInPathCollection _paths;	// Search paths for the plug-in assemblies.
		private AppDomain _discoveryDomain;			// An application domain used for plug-in information discovery.
		private GorgonPlugInVerifier _verifier;		// Plug-in verifier.
		#endregion

		#region Properties.
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
		public GorgonPlugInPathCollection SearchPaths
		{
			get
			{
			    return _paths ?? (_paths = new GorgonPlugInPathCollection());
			}
		}

        /// <summary>
        /// Property to set or return a fucntion that will be used to resolve plug-in assembly dependencies.
        /// </summary>
        /// <remarks>This property will intercept an event on the current application domain to resolve assembly dependencies as 
        /// they are loaded.  This is necessary to handle issues where types won't load or instantiate in an assembly at run time.  
        /// <para>This property must be set before any of the <c>Run</c> methods are called.</para>
        /// <para>For example, if a custom type converter attribute is specified in a plug-in assembly, it may not instantiate unless  
        /// some assemblies are resolved at load time.  Setting this property with a method that will look up assemblies in the 
        /// current application domain will correct the issue.</para></remarks>
        public Func<AppDomain, ResolveEventArgs, Assembly> AssemblyResolver
        {
            get;
            set;
        }

		/// <summary>
		/// Property to return a plug-in by its name.
		/// </summary>
		/// <param name="name">The friendly name of the plug-in or the fully qualified type name of the plug-in.</param>
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
        /// Function to find a plug-in assembly on a given path.
        /// </summary>
        /// <param name="plugInPath">Initial path to the plug-in</param>
        /// <returns>The assembly name for the plug-in assembly.</returns>
        private AssemblyName FindPlugInAssembly(string plugInPath)
        {
            if (plugInPath == null)
            {
                throw new ArgumentNullException("plugInPath");
            }

            if (string.IsNullOrWhiteSpace(plugInPath))
            {
                throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "plugInPath");
            }

            plugInPath = Path.GetFullPath(plugInPath);

            if (string.IsNullOrWhiteSpace(plugInPath))
            {
                throw new FileNotFoundException();
            }

            // We can't find the plug-in assembly on the initial path, so check the path list.
	        if (File.Exists(plugInPath))
	        {
		        return AssemblyName.GetAssemblyName(plugInPath);
	        }

	        var assemblyFile = Path.GetFileName(plugInPath);

	        plugInPath = SearchPaths.FirstOrDefault(path => File.Exists(path + assemblyFile));

	        if (string.IsNullOrWhiteSpace(plugInPath))
	        {
		        throw new FileNotFoundException(string.Format(Resources.GOR_PLUGIN_CANNOT_FIND_FILE,
			        assemblyFile));
	        }

	        plugInPath += assemblyFile;

	        return AssemblyName.GetAssemblyName(plugInPath);
        }

        
        /// <summary>
		/// Function to create any additional application domains we may need.
		/// </summary>
		private void CreateAppDomains()
		{
		    if (_discoveryDomain != null)
			{
				return;
			}

			Evidence evidence = AppDomain.CurrentDomain.Evidence;
			AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;

			// Create our domain.
			_discoveryDomain = AppDomain.CreateDomain("GorgonLibrary.PlugIns.Discovery", evidence, setup);
			Type verifierType = typeof(GorgonPlugInVerifier);
			_verifier = (GorgonPlugInVerifier)(_discoveryDomain.CreateInstanceFrom(verifierType.Assembly.Location, verifierType.FullName).Unwrap());
		}

		/// <summary>
		/// Function to determine if a plug-in implements <see cref="System.IDisposable">IDisposable</see> and dispose the object if it does.
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
		/// Function to enumerate all the plug-in names from an assembly.
		/// </summary>
		/// <param name="assemblyFile">File containing the plug-ins.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyFile"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyFile parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <remarks>Unlike the overload of this method, this method will check only the file pointed at by <c>assemblyFile</c>.</remarks>
		public IList<string> EnumeratePlugIns(string assemblyFile)
		{
			GorgonDebug.AssertParamString(assemblyFile, assemblyFile);

			CreateAppDomains();

			// Function to load a list of type names from an assembly.
			return _verifier.GetPlugInTypes(FindPlugInAssembly(assemblyFile));
		}

		/// <summary>
		/// Function to retrieve the list of plug-ins associated with a specific assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to filter.</param>
		/// <returns>A read-only list of plug-ins.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>Unlike the overload of this method, this method only enumerates plug-ins from assemblies that are already loaded into memory.</remarks>
		public IReadOnlyList<GorgonPlugIn> EnumeratePlugIns(AssemblyName assemblyName)
		{
			GorgonDebug.AssertNull(assemblyName, "assemblyName");

			return this.Where(item => AssemblyName.ReferenceMatchesDefinition(item.Assembly, assemblyName)).ToArray();
		}

		/// <summary>
		/// Function to purge any information gathered about plug-ins and plug-in assemblies before they were loaded.
		/// </summary>
		/// <remarks>Gorgon uses a separate application domain to load information about a plug-in assembly.  This can consume 
		/// quite a bit of memory over time, so this method will purge that application domain.</remarks>
		public void PurgeCachedPlugInInfo()
		{
			if (_discoveryDomain == null)
			{
				return;
			}

			AppDomain.Unload(_discoveryDomain);
			_discoveryDomain = null;
		}

		/// <summary>
		/// Function to unload all the plug-ins.
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
		/// Function to remove a plug-in by index.
		/// </summary>
		/// <param name="name">Name of the plug-in to remove.</param>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="name"/> was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">The name was an empty string..</exception>
		public void Unload(string name)
		{
			GorgonDebug.AssertParamString(name, "name");

			Items.Remove(name);
		}

		/// <summary>
		/// Function to remove a plug-in.
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
		/// <returns>One of the values in the <seealso cref="GorgonApplication.PlugIns.PlugInSigningResult">PlugInSigningResult</seealso> enumeration.</returns>
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
		/// <returns>One of the values in the <seealso cref="GorgonApplication.PlugIns.PlugInSigningResult">PlugInSigningResult</seealso> enumeration.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="assemblyPath"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the <see cref="P:GorgonLibrary.PlugIns.GorgonPlugInFactory.SearchPaths">search paths</see> (including the path provided in the parameter).</exception>
		public PlugInSigningResult IsAssemblySigned(string assemblyPath, byte[] publicKey = null)
		{
			return IsAssemblySigned(FindPlugInAssembly(assemblyPath), publicKey);
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly.</param>
		/// <remarks>If the assembly file cannot be found, then the paths in the <see cref="P:GorgonLibrary.PlugIns.GorgonPlugInFactory.SearchPaths">SearchPaths</see> collection are used to find the assembly.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyPath"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="assemblyPath"/> is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file could not be located on any of the search paths (including the path provided in the parameter).</exception>
		/// <exception cref="GorgonException">The assembly contains a plug-in type that was already loaded by another assembly.</exception>
		/// <returns>The fully qualified assembly name object for the assembly being loaded.</returns>
		public AssemblyName LoadPlugInAssembly(string assemblyPath)
		{
		    AssemblyName plugInAssemblyName = FindPlugInAssembly(assemblyPath);

			LoadPlugInAssembly(plugInAssemblyName);

			return plugInAssemblyName;
		}

		/// <summary>
		/// Function to determine if an assembly is a plug-in assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly file.</param>
		/// <returns><c>true</c> if this is a plug-in assembly, <c>false</c> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyPath parameter is an empty string.</exception>
		public bool IsPlugInAssembly(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException("assemblyPath");
			}

			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "assemblyPath");
			}

			AssemblyName assemblyName;

			try
			{
				assemblyName = FindPlugInAssembly(assemblyPath);
			}
			catch (BadImageFormatException)
			{
                // If a DLL/EXE is not a .NET assembly, then it will throw BadImageFormatException.
                // Catch it here and assume it's not going to load.
				return false;
			}

			return IsPlugInAssembly(assemblyName);
		}

		/// <summary>
		/// Function to determine if an assembly is a plug-in assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns><c>true</c> if this is a plug-in assembly, <c>false</c> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		public bool IsPlugInAssembly(AssemblyName assemblyName)
		{
			bool result = false;

			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}

			try
			{
				CreateAppDomains();

				result = _verifier.IsPlugInAssembly(assemblyName);
			}
			catch (ReflectionTypeLoadException rex)
			{
				// In this case, we'll just return false and log the message.				
				GorgonApplication.Log.Print("Exception while determining if assembly is a plug-in assembly:", LoggingLevel.Verbose);
				foreach (Exception loaderEx in rex.LoaderExceptions)
				{
					GorgonApplication.Log.Print("{0}", LoggingLevel.Verbose, loaderEx.Message);
				}
			}

			return result;
		}

		/// <summary>
		/// Function to load a plug-in assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="assemblyName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonException">The assembly contains a plug-in type that was already loaded by another assembly.</exception>
		public void LoadPlugInAssembly(AssemblyName assemblyName)
		{
			if (assemblyName == null)
		    {
		        throw new ArgumentNullException("assemblyName");
		    }
			
			Assembly plugInAssembly = AssemblyCache.LoadAssembly(assemblyName);

			try
			{

				// Get all plug-in types from the assembly.
				var plugInTypes = (from plugInType in plugInAssembly.GetTypes()
				                   where (plugInType.IsSubclassOf(typeof(GorgonPlugIn)) && (!plugInType.IsAbstract))
				                   select plugInType).ToArray();

				if (plugInTypes.Length == 0)
				{
					throw new ArgumentException(string.Format(Resources.GOR_PLUGIN_NOT_PLUGIN_ASSEMBLY, assemblyName.FullName),
					                            "assemblyName");
				}

				// Create an instance of each plug-in object.
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
