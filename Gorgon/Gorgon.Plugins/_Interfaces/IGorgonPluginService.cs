using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins;

/// <summary>
/// The return values for the an assembly signing test
/// </summary>
[Flags]
public enum AssemblySigningResults
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
/// A service to create, cache and return <see cref="IGorgonPlugin"/> instances
/// </summary>
/// <remarks>
/// <para>
/// This service object is meant to instantiate, and cache instances of <see cref="IGorgonPlugin"/> objects contained within external assemblies loaded by an assembly cache of some kind. 
/// It also allows the user to unload Plugin instances when necessary
/// </para>
/// <para>
/// A Plugin can be any class within an assembly that inherits from the <see cref="IGorgonPlugin"/> base object. When the service is created, it will retrieve a list of all known Plugins types that exist 
/// in previously loaded Plugin assemblies (this list can also be updated with the <see cref="ScanPlugins"/> method). Plugins are not created until they are requested from the service via the 
/// <see cref="GetPlugin{T}"/> or <see cref="GetPlugins{T}"/> methods. When these methods are called, they will instantiate the Plugin type, and cache it for quick retrieval on subsequent calls to the 
/// methods
/// </para>
/// <note type="tip">
/// A Plugin assembly may contain many or one Plugin type, otherwise it is not considered when enumerating Plugin types
/// </note>
/// <para>
/// <para>
/// <h3>Defining your own Plugin</h3>
/// While any class can be a Plugin within an assembly, Gorgon uses the following strategy to define a Plugin assembly
/// </para>
/// <h3>In your host assembly (an application, DLL, etc...):</h3>
/// <code language="csharp">
/// <![CDATA[
/// // This will go into your host assembly (e.g. an application, another DLL, etc...)
/// // This defines the functionality that you wish to override in your Plugin assembly
/// public abstract class FunctionalityBase
/// {
///		public abstract int DoSomething();
/// }
/// 
/// // This too will go into the host assembly and be overridden in your Plugin assembly
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
/// <h3>In your Plugin assembly:</h3>
/// <note type="tip">
/// Be sure to reference your host assembly in the Plugin assembly project
/// </note>
/// <code language="csharp">
/// <![CDATA[
/// // We put the namespace here because when loading the Plugin in our example below, we need to give a fully qualified name for the type that we're loading
/// namespace Fully.Qualified.Name
/// {
///		// Typically Gorgon makes the extension classes internal, but they can have a public accessor if you wish
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
/// This example shows how to load a Plugin and get its Plugin instance. It will use the <c>ConcreteFunctionalityPlugin</c> above:
/// <code language="csharp"> 
/// <![CDATA[
/// // Our base functionality
/// private FunctionalityBase _functionality;
/// 
/// void LoadFunctionality()
/// {
///		using (GorgonMefPluginCache assemblies = new GorgonMefPluginCache())
///		{	
///			// For brevity, we've omitted checking to see if the assembly is valid and such
///			// In the real world, you should always determine whether the assembly can be loaded 
///			// before calling the Load method
///			assemblies.LoadPluginAssemblies("Your\Directory\Here");  // You can also pass a wild card like (e.g. *.dll, *.exe, etc...)
/// 			
///			IGorgonPluginService PluginService = new GorgonMefPluginService(assemblies);
/// 
///			_functionality = PluginService.GetPlugin<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugin"); 
///		}
/// }
/// 
/// void Main()
/// {
///		LoadFunctionality();
///		
///		Console.WriteLine($"The ultimate answer and stuff: {_functionality.DoSomething()}");
/// }
/// ]]>
/// </code>
/// </example>
public interface IGorgonPluginService
{
    /// <summary>
    /// Property to return the number of Plugins that are currently loaded in this service.
    /// </summary>
    int LoadedPluginCount
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the list of Plugins from a given assembly.
    /// </summary>
    /// <typeparam name="T">Type of Plugin to retrieve. Must implement <see cref="IGorgonPlugin"/>.</typeparam>
    /// <param name="assemblyName">[Optional] The name of the assembly associated with the Plugins.</param>
    /// <returns>A list of Plugins from the assembly.</returns>
    /// <remarks>
    /// <para>
    /// This will retrieve all the Plugins from the Plugin service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
    /// the only the assembly with that name will be scanned for the Plugin type.
    /// </para>
    /// </remarks>
    IReadOnlyList<T> GetPlugins<T>(AssemblyName? assemblyName = null)
        where T : class, IGorgonPlugin;

    /// <summary>
    /// Function to retrieve a Plugin by its fully qualified type name.
    /// </summary>
    /// <typeparam name="T">The base type of the Plugin. Must implement <see cref="IGorgonPlugin"/>.</typeparam>
    /// <param name="pluginName">Fully qualified type name of the Plugin to find.</param>
    /// <returns>The Plugin, if found, or <b>null</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="pluginName"/> is empty.</exception>
    T? GetPlugin<T>(string pluginName)
        where T : class, IGorgonPlugin;

    /// <summary>
    /// Function to retrieve a list of names for available Plugins.
    /// </summary>
    /// <param name="assemblyName">[Optional] Name of the assembly containing the Plugins.</param>
    /// <returns>A list of names for the available Plugins.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only Plugin names belonging to that assembly. 
    /// If the assembly is not loaded, then an exception is thrown.
    /// </para>
    /// </remarks>
    IReadOnlyList<string> GetPluginNames(AssemblyName? assemblyName = null);

    /// <summary>
    /// Function to scan for Plugins in the loaded Plugin assemblies.
    /// </summary>
    /// <remarks>
    /// This method will unload any active Plugins, and, if implemented, call the dispose method for any Plugin.
    /// </remarks>
    void ScanPlugins();

    /// <summary>
    /// Function to unload all the Plugins.
    /// </summary>
    void UnloadAll();

    /// <summary>
    /// Function to unload a Plugin by its name.
    /// </summary>
    /// <param name="name">Fully qualified type name of the Plugin to remove.</param>
    /// <exception cref="ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
    /// <returns><b>true</b> if the Plugin was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
    bool Unload(string name);

}
