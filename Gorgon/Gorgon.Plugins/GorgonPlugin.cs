
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, June 23, 2011 11:24:37 AM
// 

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins;

/// <summary>
/// A base class for <see cref="IGorgonPlugin"/> types used by the <see cref="IGorgonPluginService"/>
/// </summary>
/// <remarks>
/// <para>
/// This is an abstract convenience class that implements and automatically populates the members of the <see cref="IGorgonPlugin"/> interface. Use this to quickly get up and running with a plugin.
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
///		: GorgonPlugin
/// {
///		public abstract FunctionalityBase GetNewFunctionality();
/// 
///		protected FunctionalityPlugin(string description)
///		    : base(description)
///		{
///		    // All plugin properties will be set by the base class.
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
/// <seealso cref="IGorgonPlugin"/>.
public abstract class GorgonPlugin
    : IGorgonPlugin
{
    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the assembly that contains this Plugin.
    /// </summary>
    public AssemblyName Assembly
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the Plugin assembly.
    /// </summary>
    public string PluginPath
    {
        get;
    }

    /// <summary>
    /// Property to return the description of the Plugin.
    /// </summary>
    public string Description
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPlugin"/> class.
    /// </summary>
    /// <param name="description">Optional description of the Plugin.</param>
    /// <remarks>
    /// Implementors of this base class should pass in a hard coded description to the base constructor.
    /// </remarks>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    protected GorgonPlugin(string description)
    {
        Description = description ?? string.Empty;

        Assembly = GetType().Assembly.GetName();
        PluginPath = GetType().Assembly.ManifestModule.FullyQualifiedName;
        // This should never happen, but I guess it's possible that the type name is null.
        Name = GetType().FullName ?? throw new GorgonException(GorgonResult.CannotCreate);
    }
}
