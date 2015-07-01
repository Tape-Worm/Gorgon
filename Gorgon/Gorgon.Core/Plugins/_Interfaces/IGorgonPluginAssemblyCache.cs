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
// Created: Monday, June 15, 2015 11:57:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins
{
	/// <summary>
	/// The return values for the <see cref="IGorgonPluginAssemblyCache.VerifyAssemblyStrongName"/> method.
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
	/// A cache to hold plugin assemblies.
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
	/// <para>
	/// In some cases, a plugin assembly may have issues when loading an assembly. Such as a type not being found, or a type in the assembly refusing to instantiate. 
	/// In these cases use the <see cref="AssemblyResolver"/> property to assign a method that will attempt to resolve any dependency assemblies.
	/// </para>
	/// <para>
	/// <note type="Important">
	/// <para>
	/// This object may use a separate app domain to interrogate a potential plugin assembly, and because of this, it is essential to call the <see cref="IDisposable.Dispose"/> 
	/// method when shutting down this object. Failure to do so will leave the application domain resident until the application shuts down, and multiple <see cref="IGorgonPluginAssemblyCache"/> instances 
	/// may create more and more application domains for type interrogation. This could lead to a memory leak.
	/// </para> 
	/// <para>
	/// This happens because we cannot unload an application domain from a finalizer since the application domain unload functionality runs on a separate thread. This conflicts with the finalizer thread 
	/// and may cause a deadlock. Thus it is essential that <see cref="IDisposable.Dispose"/> be called on this object as soon as its usefulness is at an end.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	public interface IGorgonPluginAssemblyCache : IDisposable
	{
		/// <summary>
		/// Property to return the list of search paths to use.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If an assembly cannot be found, then these paths are used to search for the assembly.
		/// </para>
		/// <para>By default, the plugin factory checks (in order):
		/// <list type="number">
		/// <item><description>The directory of the executable.</description></item>
		/// <item><description>The working directory of the executable.</description></item>
		/// <item><description>The system directory.</description></item>
		/// <item><description>The directories listed in the PATH environment variable.</description></item>
		/// </list>
		/// </para>
		/// <para>
		/// While the assembly cache attempts to be as thread-safe as possible, this collection is not.
		/// </para>
		/// </remarks>
		GorgonPluginPathCollection SearchPaths
		{
			get;
		}

		/// <summary>
		/// Property to set or return a function that will be used to resolve plugin assembly dependencies.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property will intercept an event on the current application domain to resolve assembly dependencies as they are loaded.  This is necessary to handle issues where 
		/// types won't load or instantiate in an assembly at run time.  
		/// </para>
		/// <para>
		/// For example, if a custom type converter attribute is specified in a plugin assembly, it may not instantiate unless some assemblies are resolved at load time.  Setting 
		/// this property with a method that will look up assemblies in the current application domain will correct the issue.
		/// </para>
		/// </remarks>
		Func<AppDomain, ResolveEventArgs, Assembly> AssemblyResolver
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of cached plugin assemblies.
		/// </summary>
		IReadOnlyDictionary<string, Assembly> PluginAssemblies
		{
			get;
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
		AssemblySigningResult VerifyAssemblyStrongName(string assemblyPath, byte[] publicKey = null);

		/// <summary>
		/// Function to enumerate all the plugin names from an assembly.
		/// </summary>
		/// <param name="assemblyName">The name of the assembly to enumerate from.</param>
		/// <returns>A read-only list of plugin names.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// If the file pointed at by <paramref name="assemblyName"/> contains <see cref="GorgonPlugin"/> types, then this method will retrieve a list of plugin names from that assembly. This method 
		/// can and should be used to determine if the plugin assembly actually contains any plugins, or to retrieve a catalog of available plugins.
		/// </para>
		/// <para>
		/// The assembly being enumerated is not loaded into the current application domain, and as such, is not cached. 
		/// </para>
		/// <para>
		/// Users should call <see cref="IGorgonPluginAssemblyCache.IsPluginAssembly(System.Reflection.AssemblyName)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		IReadOnlyList<string> EnumeratePlugins(AssemblyName assemblyName);

		/// <summary>
		/// Function to enumerate all the plugin names from an assembly.
		/// </summary>
		/// <param name="assemblyFile">Path to the file containing the plugins.</param>
		/// <returns>A read-only list of plugin names.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyFile"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyFile parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <exception cref="BadImageFormatException">Thrown when the assembly pointed to by <paramref name="assemblyFile"/> is not a valid .NET assembly.</exception>
		/// <remarks>
		/// <para>
		/// If the file pointed at by <paramref name="assemblyFile"/> contains <see cref="GorgonPlugin"/> types, then this method will retrieve a list of plugin names from that assembly. This method 
		/// can and should be used to determine if the plugin assembly actually contains any plugins, or to retrieve a catalog of available plugins.
		/// </para>
		/// <para>
		/// The assembly being enumerated is not loaded into the current application domain, and as such, is not cached. 
		/// </para>
		/// <para>
		/// If the file pointed by the <paramref name="assemblyFile"/> parameter is not a .NET assembly, then an exception will be raised.
		/// </para>
		/// <para>
		/// Since this method takes a path to an assembly, the <see cref="IGorgonPluginAssemblyCache.SearchPaths"/> property will be used if the assembly could not be found 
		/// on the path specified.
		/// </para>
		/// <para>
		/// Users should call <see cref="IGorgonPluginAssemblyCache.IsPluginAssembly(string)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		IReadOnlyList<string> EnumeratePlugins(string assemblyFile);

		/// <summary>
		/// Function to determine if an assembly is a plugin assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns><b>true</b> if this is a plugin assembly, <b>false</b> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This method will load the assembly into a separate <see cref="AppDomain"/> and will determine if it contains any types that inherit from 
		/// <see cref="GorgonPlugin"/>. If the assembly does not contain any plugin types, then this method returns <b>false</b>, otherwise it will 
		/// return <b>true</b>.
		/// </para>
		/// <para>
		/// Users should call this method before calling <see cref="IGorgonPluginAssemblyCache.Load(System.Reflection.AssemblyName)"/> to determine whether or not a plugin assembly should be loaded into the 
		/// application.
		/// </para>
		/// <para>
		/// Because this method loads the assembly into a separate application domain, the assembly will not be cached.
		/// </para>
		/// </remarks>
		bool IsPluginAssembly(AssemblyName assemblyName);

		/// <summary>
		/// Function to determine if an assembly is a plugin assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly file.</param>
		/// <returns><b>true</b> if this is a plugin assembly, <b>false</b> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyPath parameter is an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This method will load the assembly into a separate <see cref="AppDomain"/> and will determine if it contains any types that inherit from 
		/// <see cref="GorgonPlugin"/>. If the assembly does not contain any plugin types, then this method returns <b>false</b>, otherwise it will 
		/// return <b>true</b>. 
		/// </para>
		/// <para>
		/// This method will also check to determine if the assembly is a valid .NET assembly. If it is not, then the method will return <b>false</b>, 
		/// otherwise it will return <b>true</b>.
		/// </para>
		/// <para>
		/// Users should call this method before calling <see cref="IGorgonPluginAssemblyCache.Load(string)"/> to determine whether or not a plugin assembly should be loaded into the 
		/// application.
		/// </para>
		/// <para>
		/// Because this method loads the assembly into a separate application domain, the assembly will not be cached.
		/// </para>
		/// <para>
		/// Since this method takes a path to an assembly, the <see cref="IGorgonPluginAssemblyCache.SearchPaths"/> property will be used if the assembly could not be found 
		/// on the path specified.
		/// </para>
		/// </remarks>
		bool IsPluginAssembly(string assemblyPath);

		/// <summary>
		/// Function to load an assembly that contains <see cref="GorgonPlugin"/> types.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the assembly does not contain any types that inherit from <see cref="GorgonPlugin"/>.</exception>
		/// <remarks>
		/// <para>
		/// This method will load an assembly that contains <see cref="GorgonPlugin"/> types. If the assembly does not contain any types that inherit from 
		/// <see cref="GorgonPlugin"/>, then an exception will be raised.
		/// </para>
		/// <para>
		/// Users should call <see cref="IGorgonPluginAssemblyCache.IsPluginAssembly(System.Reflection.AssemblyName)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		void Load(AssemblyName assemblyName);

		/// <summary>
		/// Function to load an assembly that contains <see cref="GorgonPlugin"/> types.
		/// </summary>
		/// <param name="assemblyPath">Name of the assembly to load.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="assemblyPath"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the assembly does not contain any types that inherit from <see cref="GorgonPlugin"/>.</exception>
		/// <exception cref="BadImageFormatException">Thrown when the assembly pointed to by <paramref name="assemblyPath"/> is not a valid .NET assembly.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <returns>A <see cref="System.Reflection.AssemblyName"/> type containing the qualified name of the assembly that was loaded.</returns>
		/// <remarks>
		/// <para>
		/// This method will load an assembly that contains <see cref="GorgonPlugin"/> types. If the assembly does not contain any types that inherit from 
		/// <see cref="GorgonPlugin"/>, then an exception will be raised.
		/// </para>
		/// <para>
		/// Users should call <see cref="IGorgonPluginAssemblyCache.IsPluginAssembly(string)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		AssemblyName Load(string assemblyPath);
	}
}