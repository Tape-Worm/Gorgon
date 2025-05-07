// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: April 20, 2024 9:01:55 PM
//

using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins;
/// <summary>
/// The base for all plugins used by the <see cref="IGorgonPluginService"/>
/// </summary>
/// <remarks>
/// <para>
/// Any plugins used by the <see cref="IGorgonPluginService"/> must implement this type. The plugin service will scan any plugin assemblies loaded and will enumerate only types that have this interface 
/// implemented. This interface is used when a plugin needs manage all of its metadata directly, or in cases where <see cref="GorgonPlugin"/> can't be used.
/// </para>
/// <para>
/// If a plugin doesn't need this level of control, then users can opt to use the <see cref="GorgonPlugin"/> type, which will handle the setup of the interface on their behalf.
/// </para>
/// <para>
/// <h3>Defining your own Plugin</h3>
/// While any class can be a Plugin within an assembly, Gorgon uses the following strategy to define a Plugin assembly
/// </para>
/// <para>
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
///		: IGorgonPlugin
/// {
///     // These properties are from IGorgonPlugin
///     public string Name
///     {
///         get;
///     }
///     
///     public string Description
///     {
///         get;
///     }
///     
///     public string PluginPath
///     {
///         get;
///     }
///     
///     public AssemblyName Assembly
///     {
///         get;
///     }
/// 
///		public abstract FunctionalityBase GetNewFunctionality();
/// 
///		protected FunctionalityPlugin(string description)
///		{
///		    // Typically the fully qualified type name.
///		    Name = ... 
///		    // From the constructor.
///		    Description = description;
///		    // The AssemblyName for the assembly that contains this type.
///		    Assembly = ...
///		    // The path to the assembly holding the plugin type.
///		    PluginPath = ...
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
/// <seealso cref="GorgonPlugin"/>
public interface IGorgonPlugin
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the assembly that contains this Plugin.
    /// </summary>
    AssemblyName Assembly
    {
        get;
    }

    /// <summary>
    /// Property to return the description of the Plugin.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the Plugin assembly.
    /// </summary>
    string PluginPath
    {
        get;
    }
}