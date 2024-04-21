// Gorgon.
// Copyright (C) 2024 Michael Winsor
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

namespace Gorgon.PlugIns;
/// <summary>
/// The base for all plug-ins used by the <see cref="IGorgonPlugInService"/>
/// </summary>
/// <remarks>
/// <para>
/// Any plug-ins used by the <see cref="IGorgonPlugInService"/> must implement this type. The plug-in service will scan any plug-in assemblies loaded and will enumerate only types that have this interface 
/// implemented. This interface is used when a plug-in needs manage all of its metadata directly, or in cases where <see cref="GorgonPlugIn"/> can't be used.
/// </para>
/// <para>
/// If a plug-in doesn't need this level of control, then users can opt to use the <see cref="GorgonPlugIn"/> type, which will handle the setup of the interface on their behalf.
/// </para>
/// <para>
/// <h3>Defining your own plugin</h3>
/// While any class can be a plugin within an assembly, Gorgon uses the following strategy to define a plugin assembly
/// </para>
/// <para>
/// <h3>In your host assembly (an application, DLL, etc...):</h3>
/// <code language="csharp">
/// <![CDATA[
/// // This will go into your host assembly (e.g. an application, another DLL, etc...)
/// // This defines the functionality that you wish to override in your plugin assembly
/// public abstract class FunctionalityBase
/// {
///		public abstract int DoSomething();
/// }
/// 
/// // This too will go into the host assembly and be overridden in your plugin assembly
/// public abstract class FunctionalityPlugIn
///		: IGorgonPlugIn
/// {
///     // These properties are from IGorgonPlugIn
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
///     public string PlugInPath
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
///		protected FunctionalityPlugIn(string description)
///		{
///		    // Typically the fully qualified type name.
///		    Name = ... 
///		    // From the constructor.
///		    Description = description;
///		    // The AssemblyName for the assembly that contains this type.
///		    Assembly = ...
///		    // The path to the assembly holding the plug-in type.
///		    PlugInPath = ...
///		}
/// }
///	]]>
/// </code>
/// <h3>In your plugin assembly:</h3>
/// <note type="tip">
/// Be sure to reference your host assembly in the plugin assembly project
/// </note>
/// <code language="csharp">
/// <![CDATA[
/// // We put the namespace here because when loading the plugin in our example below, we need to give a fully qualified name for the type that we're loading
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
/// <seealso cref="GorgonPlugIn"/>
public interface IGorgonPlugIn
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the assembly that contains this plugin.
    /// </summary>
    AssemblyName Assembly
    {
        get;
    }

    /// <summary>
    /// Property to return the description of the plugin.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the plugin assembly.
    /// </summary>
    string PlugInPath
    {
        get;
    }
}