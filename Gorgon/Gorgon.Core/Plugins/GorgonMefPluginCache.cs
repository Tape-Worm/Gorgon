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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Properties;

namespace Gorgon.Plugins
{
	/// <summary>
	/// A cache to hold MEF plugin assemblies.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This assembly cache is meant to load/hold a list of plugin assemblies that contain types that implement the <see cref="GorgonPlugin"/> type and is 
	/// meant to be used in conjunction with the <see cref="IGorgonPluginService"/> type.
	/// </para>
	/// <para>
	/// The cache attempts to ensure that the application only loads an assembly once during the lifetime of the application in order to cut down on 
	/// overhead and potential errors that can come up when multiple assemblies with the same qualified name are loaded into the same context.
	/// </para>
	/// </remarks>
	public sealed class GorgonMefPluginCache
		: IDisposable
	{
		#region Variables.
        // The contract name for the plug in.
	    private readonly string _contractName = typeof(GorgonPlugin).FullName;
		// Application log file.
		private readonly IGorgonLog _log;
        // The root catalog for the plugins.
	    private AggregateCatalog _rootCatalog = new AggregateCatalog();
        // The container for the plugin definitions.
        private CompositionContainer _container;
        // The synchronization lock for multiple threads..
	    private static readonly object _syncLock = new object();
        // The builder used for type registration.
	    private readonly RegistrationBuilder _builder = new RegistrationBuilder();
		#endregion

		#region Properties.
	    /// <summary>
	    /// Property to return the list of cached plugin assemblies.
	    /// </summary>
	    public IReadOnlyList<string> PluginAssemblies
	    {
            get;
	        private set;
	    }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to update the list of assemblies.
        /// </summary>
        /// <param name="catalog">The catalog for the assembly.</param>
        /// <param name="assemblyList">The list of assemblies.</param>
	    private void UpdateAssemblyList(DirectoryCatalog catalog, HashSet<string> assemblyList)
        {
            foreach (string file in catalog.LoadedFiles)
            {
                if (assemblyList.Contains(file))
                {
                    continue;
                }

                if (catalog.Parts
                           .SelectMany(item => item.ExportDefinitions)
                           .Any(item => string.Equals(item.ContractName, _contractName, StringComparison.Ordinal)))
                {
                    assemblyList.Add(file);
                }
            }
        }

		/// <summary>
		/// Function to determine if an assembly is signed with a strong name key pair.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly to check.</param>
		/// <param name="publicKey">[Optional] The full public key to verify against.</param>
		/// <returns>A value from the <see cref="AssemblySigningResult"/>.</returns>
		/// <remarks>
		/// <para>
		/// This method can be used to determine if an assembly has a strong name key pair (i.e. signed with a strong name) before loading it. If the assembly is not found, then 
		/// the result of this method is <see cref="AssemblySigningResult.NotSigned"/>.
		/// </para>
		/// <para>
		/// The <paramref name="publicKey"/> parameter is used to compare a known full public key (note: NOT the token) against that of the assembly being queried. If the bytes in 
		/// the public key do not match that of the public key in the assembly being queried, then the return result will have a <see cref="AssemblySigningResult.KeyMismatch"/> 
		/// value OR'd with the result. To check for a mismatch do the following:
		/// <code language="csharp">
		/// // Compare the key for the current assembly to that of another assembly.
		/// byte[] expected = this.GetType().Assembly.GetName().GetPublicKey();
		/// 
		/// AssemblySigningResult result = assemblyCache.VerifyAssemblyStrongName("Path to your assembly", expected);
		/// 
		/// if ((result &amp; AssemblySigningResult.KeyMismatch) == AssemblySigningResult.KeyMismatch)
		/// {
		///    Console.Writeline("Public token mismatch.");
		/// }
		/// </code>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <h3>Disclaimer time!!!</h3>
		/// <para>
		/// If the security of your assemblies is not critical, then this method should serve the purpose of verification of an assembly. However:
		/// </para>
		/// <para>
		/// <i>
		/// This method is intended to verify that an assembly is signed, optionally contains the provide public key, and that, to the best of its knowledge, it has not been tampered 
		/// with. This is not meant to protect a system against malicious code, or provide a means of checking an identify for an assembly. This method also makes no guarantees that 
		/// the information is 100% accurate, so if security is of the utmost importance, do not rely on this method alone and use other functionality to secure your assemblies. 
		/// </i>
		/// </para>
		/// <para>
		/// For more information about signing an assembly, follow this link <a href="https://msdn.microsoft.com/en-us/library/xwb8f617(v=vs.110).aspx" target="_blank">Creating and 
		/// Using Strong-Named Assemblies</a>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static AssemblySigningResult VerifyAssemblyStrongName(string assemblyPath, byte[] publicKey = null)
		{
			if ((string.IsNullOrWhiteSpace(assemblyPath)) || (!File.Exists(assemblyPath)))
			{
				return AssemblySigningResult.NotSigned;
			}

			var clrStrongNameClsId = new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92");
			var clrStrongNameriid = new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D");

			var strongName = (IClrStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(clrStrongNameClsId, clrStrongNameriid);

			int result = strongName.StrongNameSignatureVerificationEx(assemblyPath, true, out bool wasVerified);

			if ((result != 0) || (!wasVerified))
			{
				return AssemblySigningResult.NotSigned;
			}

			if (publicKey == null)
			{
				return AssemblySigningResult.Signed;
			}

			var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
			byte[] compareToken = assemblyName.GetPublicKey();

            return (compareToken == null) || (publicKey.Length != compareToken.Length) || (!publicKey.SequenceEqual(compareToken))
                ? AssemblySigningResult.Signed | AssemblySigningResult.KeyMismatch
                : AssemblySigningResult.Signed;
        }

        /// <summary>
        /// Function to enumerate all the plugin names from the assemblies loaded into the cache.
        /// </summary>
        /// <returns>A composition container containing the plugins from the assemblies.</returns>
        public IEnumerable<Lazy<GorgonPlugin, IDictionary<string, object>>> EnumeratePlugins()
		{
		    lock (_syncLock)
		    {
                return _container == null
                    ? (Array.Empty<Lazy<GorgonPlugin, IDictionary<string, object>>>())
                    : _container.GetExports<GorgonPlugin, IDictionary<string, object>>(_contractName);
            }
        }

	    /// <summary>
	    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// <note type="important">
	    /// <para>
	    /// This method must be called, or the application domain that is created to interrogate assembly types will live until the end of the application. This could lead to memory bloat or worse. 
	    /// </para>
	    /// <para>
	    /// Because the application domain is unloaded on a separate thread, it may deadlock with the finalizer thread and thus we cannot count on the finalizer to clean evict the stale app domain on our 
	    /// behalf.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public void Dispose()
	    {
	        _log.Print("Unloading MEF plugin container and catalogs.", LoggingLevel.Intermediate);

            _container?.Dispose();
	        _container = null;
            _rootCatalog?.Dispose();
	        _rootCatalog = null;
	    }

        /// <summary>
        /// Function to load any DLL assemblies in the specified directory path.
        /// </summary>
        /// <param name="directoryPath">The path containing the plug in DLLs to load.</param>
        /// <param name="filePattern">[Optional] The file pattern to search for.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory specified by <paramref name="directoryPath"/> does not exist.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="filePattern"/> is specified, then a standard file search wildcard can be used to located specific files to load (e.g. MyPlugIn-*.dll, OtherTypes*.so, etc...).
        /// </para>
        /// </remarks>
	    public void LoadPluginAssemblies(string directoryPath, string filePattern = "*.dll")
	    {
	        if (directoryPath == null)
	        {
                throw new ArgumentNullException(nameof(directoryPath));
	        }

	        if (string.IsNullOrWhiteSpace(directoryPath))
	        {
                throw new ArgumentEmptyException(nameof(directoryPath));
	        }

            // Ensure we send in a file search pattern.
	        filePattern = Path.GetFileName(filePattern);

	        if (string.IsNullOrWhiteSpace(filePattern))
	        {
	            filePattern = "*.dll";
	        }

            var directory = new DirectoryInfo(directoryPath);

            _log.Print($"Searching '{directory.FullName}\\{filePattern}' for plug in assemblies.", LoggingLevel.Simple);

	        if (!directory.Exists)
	        {
                throw new DirectoryNotFoundException(string.Format(Resources.GOR_ERR_DIRECTORY_NOT_FOUND, directory.FullName));
	        }

	        lock (_syncLock)
	        {
	            // Check to see if we have this directory and search pattern already.
	            DirectoryCatalog catalog = _rootCatalog.Catalogs.OfType<DirectoryCatalog>()
	                                                   .FirstOrDefault(item => string.Equals(item.FullPath, directoryPath, StringComparison.OrdinalIgnoreCase)
	                                                                           && string.Equals(item.SearchPattern,
	                                                                                            filePattern,
	                                                                                            StringComparison.OrdinalIgnoreCase));

	            // This catalog was already loaded, so just refresh it by recomposing.
	            if (catalog != null)
	            {
	                _log.Print("Path is already registered in cache, no need to re-add.", LoggingLevel.Verbose);
	                catalog.Refresh();
	            }
	            else
	            {
	                catalog = new DirectoryCatalog(directory.FullName, filePattern, _builder);
	                _rootCatalog.Catalogs.Add(catalog);

	                _log.Print($"Added {catalog.LoadedFiles.Count} plug in assemblies to cache.", LoggingLevel.Verbose);
	            }
	        }

	        Refresh();
	    }

        /// <summary>
        /// Function to refresh the loaded plugin assembly list, and import any other assemblies that match the previously watched paths.
        /// </summary>
	    public void Refresh()
	    {
	        lock (_syncLock)
	        {
	            var assemblyList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	            foreach (DirectoryCatalog catalog in _rootCatalog.Catalogs.OfType<DirectoryCatalog>())
	            {
	                UpdateAssemblyList(catalog, assemblyList);
	            }

	            PluginAssemblies = assemblyList.ToArray();

	            _log.Print($"{PluginAssemblies.Count} cached with valid plug in types.", LoggingLevel.Simple);
	        }
	    }
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMefPluginCache"/> class.
		/// </summary>
		/// <param name="log">[Optional] The application log file to use.</param>
		public GorgonMefPluginCache(IGorgonLog log = null)
		{
		    _builder.ForTypesDerivedFrom<GorgonPlugin>().Export<GorgonPlugin>(b =>
		                                                                      {
		                                                                          b.AddMetadata("Name", t => t.FullName);
		                                                                          b.AddMetadata("Assembly", t => t.Assembly.GetName());
		                                                                          b.Inherited();
		                                                                      });
		    _log = log ?? GorgonLog.NullLog;
            _container = new CompositionContainer(_rootCatalog, CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe);
		}
		#endregion
	}
}
