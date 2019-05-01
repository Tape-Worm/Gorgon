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
// Created: Thursday, June 23, 2011 11:24:37 AM
// 
#endregion

using System.Reflection;
using Gorgon.Core;

namespace Gorgon.PlugIns
{
    /// <summary>
    /// The base for all plug ins used by the <see cref="IGorgonPlugInService"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any plug ins used by the <see cref="IGorgonPlugInService"/> must be derived from this type. The plug in service will scan any plug in assemblies loaded and will enumerate only types that inherit 
    /// this type.
    /// </para>
    /// <para>
    /// <h3>Defining your own plugin</h3>
    /// While any class can be a plugin within an assembly, Gorgon uses the following strategy to define a plugin assembly.
    /// </para>
    /// <para>
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
    public abstract class GorgonPlugIn
		: IGorgonNamedObject
	{
		#region Properties.
	    /// <summary>
	    /// Property to return the name of this object.
	    /// </summary>
	    public string Name
	    {
	        get;
	    }

		/// <summary>
		/// Property to return the assembly that contains this plugin.
		/// </summary>
		public AssemblyName Assembly
		{
			get;
		}

		/// <summary>
		/// Property to return the path to the plugin assembly.
		/// </summary>
		public string PlugInPath
		{
			get;
		}

		/// <summary>
		/// Property to return the description of the plugin.
		/// </summary>
		public string Description
		{
			get;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plugin.</param>
		/// <remarks>
		/// Implementors of this base class should pass in a hard coded description to the base constructor.
		/// </remarks>
		protected GorgonPlugIn(string description)
		{
			Description = description ?? string.Empty;

			Assembly = GetType().Assembly.GetName();
			PlugInPath = GetType().Assembly.ManifestModule.FullyQualifiedName;
			Name = GetType().FullName;
		}
		#endregion
	}
}
