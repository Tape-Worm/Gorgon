using System.Reflection;
using Gorgon.Core;

namespace Gorgon.PlugIns;

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
/// A service to create, cache and return <see cref="IGorgonPlugIn"/> instances
/// </summary>
/// <remarks>
/// <para>
/// This service object is meant to instantiate, and cache instances of <see cref="IGorgonPlugIn"/> objects contained within external assemblies loaded by an assembly cache of some kind. 
/// It also allows the user to unload plug-in instances when necessary
/// </para>
/// <para>
/// A plug-in can be any class within an assembly that inherits from the <see cref="IGorgonPlugIn"/> base object. When the service is created, it will retrieve a list of all known plug-ins types that exist 
/// in previously loaded plug-in assemblies (this list can also be updated with the <see cref="ScanPlugIns"/> method). Plug-ins are not created until they are requested from the service via the 
/// <see cref="GetPlugIn{T}"/> or <see cref="GetPlugIns{T}"/> methods. When these methods are called, they will instantiate the plug-in type, and cache it for quick retrieval on subsequent calls to the 
/// methods
/// </para>
/// <note type="tip">
/// A plug-in assembly may contain many or one plug-in type, otherwise it is not considered when enumerating plug-in types
/// </note>
/// <para>
/// <para>
/// <h3>Defining your own plug-in</h3>
/// While any class can be a plug-in within an assembly, Gorgon uses the following strategy to define a plug-in assembly
/// </para>
/// <h3>In your host assembly (an application, DLL, etc...):</h3>
/// <code language="csharp">
/// <![CDATA[
/// // This will go into your host assembly (e.g. an application, another DLL, etc...)
/// // This defines the functionality that you wish to override in your PlugIn assembly
/// public abstract class FunctionalityBase
/// {
///		public abstract int DoSomething();
/// }
/// 
/// // This too will go into the host assembly and be overridden in your PlugIn assembly
/// public abstract class FunctionalityPlugIn
///		: GorgonPlugIn
/// {
///		public abstract FunctionalityBase GetNewFunctionality();
/// 
///		protected FunctionalityPlugIn(string description)
///		{
///		}
/// }
///	]]>
/// </code>
/// <h3>In your plug-in assembly:</h3>
/// <note type="tip">
/// Be sure to reference your host assembly in the plug-in assembly project
/// </note>
/// <code language="csharp">
/// <![CDATA[
/// // We put the namespace here because when loading the PlugIn in our example below, we need to give a fully qualified name for the type that we're loading
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
///		public class ConcreteFunctionalityPlugIn
///			: FunctionalityPlugIn
///		{
///			public override FunctionalityBase GetNewFunctionality()
///			{
///				return new ConcreteFunctionality();
///			}
/// 
///			public ConcreteFunctionalityPlugIn()
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
/// This example shows how to load a plug-in and get its plug-in instance. It will use the <c>ConcreteFunctionalityPlugIn</c> above:
/// <code language="csharp"> 
/// <![CDATA[
/// // Our base functionality
/// private FunctionalityBase _functionality;
/// 
/// void LoadFunctionality()
/// {
///		using (GorgonMefPlugInCache assemblies = new GorgonMefPlugInCache())
///		{	
///			// For brevity, we've omitted checking to see if the assembly is valid and such
///			// In the real world, you should always determine whether the assembly can be loaded 
///			// before calling the Load method
///			assemblies.LoadPlugInAssemblies("Your\Directory\Here");  // You can also pass a wild card like (e.g. *.dll, *.exe, etc...)
/// 			
///			IGorgonPlugInService PlugInService = new GorgonMefPlugInService(assemblies);
/// 
///			_functionality = PlugInService.GetPlugIn<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugIn"); 
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
public interface IGorgonPlugInService
{
    /// <summary>
    /// Property to return the number of plug-ins that are currently loaded in this service.
    /// </summary>
    int LoadedPlugInCount
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the list of plug-ins from a given assembly.
    /// </summary>
    /// <typeparam name="T">Type of plug-in to retrieve. Must implement <see cref="IGorgonPlugIn"/>.</typeparam>
    /// <param name="assemblyName">[Optional] The name of the assembly associated with the plug-ins.</param>
    /// <returns>A list of plug-ins from the assembly.</returns>
    /// <remarks>
    /// <para>
    /// This will retrieve all the plug-ins from the plug-in service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
    /// the only the assembly with that name will be scanned for the plug-in type.
    /// </para>
    /// </remarks>
    IReadOnlyList<T> GetPlugIns<T>(AssemblyName? assemblyName = null)
        where T : class, IGorgonPlugIn;

    /// <summary>
    /// Function to retrieve a plug-in by its fully qualified type name.
    /// </summary>
    /// <typeparam name="T">The base type of the plug-in. Must implement <see cref="IGorgonPlugIn"/>.</typeparam>
    /// <param name="plugInName">Fully qualified type name of the plug-in to find.</param>
    /// <returns>The plug-in, if found, or <b>null</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="plugInName"/> is empty.</exception>
    T? GetPlugIn<T>(string plugInName)
        where T : class, IGorgonPlugIn;

    /// <summary>
    /// Function to retrieve a list of names for available plug-ins.
    /// </summary>
    /// <param name="assemblyName">[Optional] Name of the assembly containing the plug-ins.</param>
    /// <returns>A list of names for the available plug-ins.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only plug-in names belonging to that assembly. 
    /// If the assembly is not loaded, then an exception is thrown.
    /// </para>
    /// </remarks>
    IReadOnlyList<string> GetPlugInNames(AssemblyName? assemblyName = null);

    /// <summary>
    /// Function to scan for plug-ins in the loaded plug-in assemblies.
    /// </summary>
    /// <remarks>
    /// This method will unload any active plug-ins, and, if implemented, call the dispose method for any plug-in.
    /// </remarks>
    void ScanPlugIns();

    /// <summary>
    /// Function to unload all the plug-ins.
    /// </summary>
    void UnloadAll();

    /// <summary>
    /// Function to unload a plug-in by its name.
    /// </summary>
    /// <param name="name">Fully qualified type name of the plug-in to remove.</param>
    /// <exception cref="ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
    /// <returns><b>true</b> if the plug-in was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
    bool Unload(string name);

}
