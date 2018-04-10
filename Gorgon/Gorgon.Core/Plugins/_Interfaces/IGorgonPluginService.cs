using System;
using System.Collections.Generic;
using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins
{
    /// <summary>
    /// The return values for the an assembly signing test.
    /// </summary>
    [Flags]
    public enum AssemblySigningResult
    {
        /// <summary>
        /// Assembly is not signed.  This flag is mutually exclusive.
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
	/// A service to create, cache and return <see cref="GorgonPlugin"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service object is meant to instantiate, and cache instances of <see cref="GorgonPlugin"/> objects contained within external assemblies loaded by an assembly cache of some kind. 
	/// It also allows the user to unload plugin instances when necessary.
	/// </para>
	/// <para>
	/// A plugin can be any class within an assembly that inherits from the <see cref="GorgonPlugin"/> base object. When the service is created, it will retrieve a list of all known plugins types that exist 
	/// in previously loaded plugin assemblies (this list can also be updated with the <see cref="ScanPlugins"/> method). Plugins are not created until they are requested from the service via the 
	/// <see cref="GetPlugin{T}"/> or <see cref="GetPlugins{T}"/> methods. When these methods are called, they will instantiate the plugin type, and cache it for quick retrieval on subsequent calls to the 
	/// methods.
	/// </para>
    /// <note type="tip">
    /// A plugin assembly may contain many or one plugin type, otherwise it is not considered when enumerating plugin types.
    /// </note>
    /// <para>
    /// <para>
    /// <h3>Defining your own plugin</h3>
    /// While any class can be a plugin within an assembly, Gorgon uses the following strategy to define a plugin assembly.
    /// </para>
    /// <h3>In your host assembly (an application, DLL, etc...):</h3>
    /// <code language="csharp">
    /// <![CDATA[
    /// // This will go into your host assembly (e.g. an application, another DLL, etc...)
    /// // This defines the functionality that you wish to override in your plugin assembly.
    /// public abstract class FunctionalityBase
    /// {
    ///		public abstract int DoSomething();
    /// }
    /// 
    /// // This too will go into the host assembly and be overridden in your plugin assembly.
    /// public abstract class FunctionalityPlugin
    ///		: GorgonPlugin
    /// {
    ///		public abstract FunctionalityBase GetNewFunctionality();
    /// 
    ///		protected FunctionalityPlugin(string description)
    ///		{
    ///		}
    /// }
    ///	]]>
    /// </code>
    /// <h3>In your plugin assembly:</h3>
    /// <note type="tip">
    /// Be sure to reference your host assembly in the plugin assembly project.
    /// </note>
    /// <code language="csharp">
    /// <![CDATA[
    /// // We put the namespace here because when loading the plugin in our example below, we need to give a fully qualified name for the type that we're loading.
    /// namespace Fully.Qualified.Name
    /// {
    ///		// Typically Gorgon makes the extension classes internal, but they can have a public accessor if you wish.
    ///		class ConcreteFunctionality
    ///			: FunctionalityBase
    ///		{
    ///			public override int DoSomething()
    ///			{
    ///				return 42;
    ///			}
    ///		}
    /// 
    ///		public class ConcreteFunctionalityPlugin
    ///			: FunctionalityPlugin
    ///		{
    ///			public override FunctionalityBase GetNewFunctionality()
    ///			{
    ///				return new ConcreteFunctionality();
    ///			}
    /// 
    ///			public ConcreteFunctionalityPlugin()
    ///				: base("What is the answer to life, the universe, and blah blah blah?")
    ///			{
    ///			}
    ///		}
    /// }
    /// ]]>
    /// </code>  
    /// </para>
    /// </remarks>
	/// <example>
	/// This example shows how to load a plugin and get its plugin instance. It will use the <c>ConcreteFunctionalityPlugin</c> above:
	/// <code language="csharp"> 
	/// <![CDATA[
	/// // Our base functionality.
	/// private FunctionalityBase _functionality;
	/// 
	/// void LoadFunctionality()
	/// {
	///		using (GorgonMefPluginCache assemblies = new GorgonMefPluginCache())
	///		{	
	///			// For brevity, we've omitted checking to see if the assembly is valid and such.
	///			// In the real world, you should always determine whether the assembly can be loaded 
	///			// before calling the Load method.
	///			assemblies.LoadPluginAssemblies("Your\Directory\Here");  // You can also pass a wild card like (e.g. *.dll, *.exe, etc...)
	/// 			
	///			IGorgonPluginService pluginService = new GorgonMefPluginService(assemblies);
	/// 
	///			_functionality = pluginService.GetPlugin<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugin"); 
	///		}
	/// }
	/// 
	/// void Main()
	/// {
	///		LoadFunctionality();
	///		
	///		Console.WriteLine("The ultimate answer and stuff: {0}", _functionality.DoSomething());
	/// }
	/// ]]>
	/// </code>
	/// </example>
	public interface IGorgonPluginService
	{
        #region Properties.
		/// <summary>
		/// Property to return the number of plugins that are currently loaded in this service.
		/// </summary>
		int LoadedPluginCount
		{
			get;
		}
        #endregion

        #region Methods.
		/// <summary>
		/// Function to retrieve the list of plugins from a given assembly.
		/// </summary>
		/// <typeparam name="T">Type of plugin to retrieve. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="assemblyName">[Optional] The name of the assembly associated with the plugins.</param>
		/// <returns>A list of plugins from the assembly.</returns>
		/// <remarks>
		/// This will retrieve all the plugins from the plugin service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
		/// the only the assembly with that name will be scanned for the plugin type.
		/// </remarks>
		IReadOnlyList<T> GetPlugins<T>(AssemblyName assemblyName = null)
			where T : GorgonPlugin;

		/// <summary>
		/// Function to retrieve a plugin by its fully qualified type name.
		/// </summary>
		/// <typeparam name="T">The base type of the plugin. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="pluginName">Fully qualified type name of the plugin to find.</param>
		/// <returns>The plugin, if found, or <b>null</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginName"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="pluginName"/> is empty.</exception>
		T GetPlugin<T>(string pluginName)
			where T : GorgonPlugin;

		/// <summary>
		/// Function to retrieve a list of names for available plugins.
		/// </summary>
		/// <param name="assemblyName">[Optional] Name of the assembly containing the plugins.</param>
		/// <returns>A list of names for the available plugins.</returns>
		/// <remarks>
		/// <para>
		/// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only plugin names belonging to that assembly. 
		/// If the assembly is not loaded, then an exception is thrown.
		/// </para>
		/// </remarks>
		IReadOnlyList<string> GetPluginNames(AssemblyName assemblyName = null);

		/// <summary>
		/// Function to scan for plugins in the loaded plugin assemblies.
		/// </summary>
		/// <remarks>
		/// This method will unload any active plugins, and, if implemented, call the dispose method for any plugin.
		/// </remarks>
		void ScanPlugins();

		/// <summary>
		/// Function to unload all the plugins.
		/// </summary>
		void UnloadAll();

        /// <summary>
        /// Function to unload a plugin by its name.
        /// </summary>
        /// <param name="name">Fully qualified type name of the plugin to remove.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> parameter was <b>null</b>.</exception>
        /// <exception cref="System.ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
        /// <returns><b>true</b> if the plugin was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
        bool Unload(string name);
        #endregion
	}
}