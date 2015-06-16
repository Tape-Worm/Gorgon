#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, June 15, 2015 11:57:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gorgon.Plugins
{
	/// <summary>
	/// A service to create, cache and return <see cref="GorgonPlugin"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service object is meant to instantiate, and cache instances of <see cref="GorgonPlugin"/> objects contained within external assemblies loaded by the <see cref="GorgonPluginAssemblyCache"/>. 
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
	///		using (IGorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache())
	///		{	
	///			// For brevity, we've omitted checking to see if the assembly is valid and such.
	///			// In the real world, you should always determine whether the assembly can be loaded 
	///			// before calling the Load method.
	///			assemblies.Load("ConcreteFunctionality.dll");
	/// 			
	///			IGorgonPluginService pluginService = new GorgonPluginService(assemblies);
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
		/// <summary>
		/// Property to return the plugin assembly cache this service is using.
		/// </summary>
		GorgonPluginAssemblyCache PluginAssemblyCache
		{
			get;
		}

		/// <summary>
		/// Property to return the number of plugins that are currently loaded in this service.
		/// </summary>
		int LoadedPluginCount
		{
			get;
		}

		/// <summary>
		/// Function to retrieve the list of plugins from a given assembly.
		/// </summary>
		/// <typeparam name="T">Type of plugin to retrieve. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="assemblyName">The name of the assembly associated with the plugins.</param>
		/// <returns>A list of plugins from the assembly.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyName"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// This will retrieve instances of all the plugins of the specified type from the given assembly. 
		/// </remarks>
		IReadOnlyList<T> GetPlugins<T>(AssemblyName assemblyName)
			where T : GorgonPlugin;

		/// <summary>
		/// Function to retrieve a plugin by its fully qualified type name.
		/// </summary>
		/// <typeparam name="T">The base type of the plugin. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="pluginName">Fully qualified type name of the plugin to find.</param>
		/// <returns>The plugin, if found, or <b>null</b> (<i>Nothing</i> in VB.Net) if not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="pluginName"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="pluginName"/> is empty.</exception>
		T GetPlugin<T>(string pluginName)
			where T : GorgonPlugin;

		/// <summary>
		/// Function to retrieve a list of names for available plugins.
		/// </summary>
		/// <param name="assemblyName">[Optional] Name of the assembly containing the plugins.</param>
		/// <returns>A list of names for the available plugins.</returns>
		/// <remarks>
		/// <para>
		/// This method will retrieve a list of fully qualified type names for plugins contained within the <see cref="GorgonPluginAssemblyCache"/> passed to this object. This list is 
		/// not indicative of whether the type has been created or not.
		/// </para>
		/// <para>
		/// The <paramref name="assemblyName"/> parameter, when not <b>null</b> (<i>Nothing</i> in VB.Net), will return only plugin names belonging to that assembly. 
		/// If the assembly is not loaded, then an exception is thrown.
		/// </para>
		/// </remarks>
		IReadOnlyList<string> GetPluginNames(AssemblyName assemblyName = null);

		/// <summary>
		/// Function to scan for plugins in the loaded plugin assemblies that are cached in the <see cref="GorgonPluginAssemblyCache"/> passed to this object.
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
		/// <exception cref="System.ArgumentNullException">The <paramref name="name"/> parameter was <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
		/// <returns><b>true</b> if the plugin was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
		bool Unload(string name);
	}
}