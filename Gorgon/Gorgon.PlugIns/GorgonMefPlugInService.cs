
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 25, 2018 3:01:41 PM
// 

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Properties;

namespace Gorgon.PlugIns;

/// <summary>
/// A service to create, cache and return <see cref="IGorgonPlugIn"/> instances by using the built in Microsoft Extensibility Framework as its provider
/// </summary>
/// <remarks>
/// <para>
/// This service object is meant to instantiate, and cache instances of <see cref="IGorgonPlugIn"/> objects contained within external assemblies loaded by the <see cref="GorgonMefPlugInService"/>. 
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
/// private GorgonMefPlugInCache _assemblies;
/// 
/// void LoadFunctionality()
/// {
///		assemblies = new GorgonMefPlugInCache();
///		
///		// For brevity, we've omitted checking to see if the assembly is valid and such
///		// In the real world, you should always determine whether the assembly can be loaded 
///		// before calling the Load method
///		_assemblies.LoadPlugInAssemblies("Your\Directory\Here", "file search pattern");  // You can pass a wild card like *.dll, *.exe, etc..., or an absolute file name like "MyPlugIn.dll"
/// 			
///		IGorgonPlugInService PlugInService = new GorgonMefPlugInService(_assemblies);
/// 
///		_functionality = PlugInService.GetPlugIn<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugIn"); 
/// }
/// 
/// void Main()
/// {
///		LoadFunctionality();
///		
///		Console.WriteLine($"The ultimate answer and stuff: {_functionality.DoSomething()}");
///		
///     _assemblies?.Dispose();
/// }
/// ]]>
/// </code>
/// </example>
/// <param name="mefCache">The cache of MEF plug-in assemblies.</param>
[method: RequiresAssemblyFiles("Plug-ins will not work with trimming and Native AOT.")]
public sealed class GorgonMefPlugInService(GorgonMefPlugInCache mefCache)
        : IGorgonPlugInService
{
    // The MEF plug-in assembly cache.
    private readonly GorgonMefPlugInCache _cache = mefCache;
    // The application log file.
    private readonly IGorgonLog _log = mefCache.Log ?? GorgonLog.NullLog;
    // List of previously loaded plug-ins.
    private readonly ConcurrentDictionary<string, Lazy<IGorgonPlugIn, IDictionary<string, object>>> _loadedPlugIns = new(StringComparer.OrdinalIgnoreCase);
    // Flag to indicate whether or not the plug-ins have been scanned.
    private int _scanned;

    /// <summary>
    /// Property to return the number of plug-ins that are currently loaded in this service.
    /// </summary>
    public int LoadedPlugInCount => _loadedPlugIns.Count;

    /// <summary>
    /// Function to unload a plug-in by its name.
    /// </summary>
    /// <param name="plugIn">The plug-in to remove.</param>
    private void DisposePlugIn(IGorgonPlugIn plugIn)
    {
        if (plugIn is not IDisposable disposable)
        {
            return;
        }

        disposable.Dispose();
        _log.Print($"PlugIn '{plugIn.Name}' disposed.", LoggingLevel.Verbose);
    }

    /// <summary>
    /// Function to retrieve a plug-in by its fully qualified type name.
    /// </summary>
    /// <typeparam name="T">The base type of the plug-in. Must implement <see cref="IGorgonPlugIn"/>.</typeparam>
    /// <param name="plugInName">Fully qualified type name of the plug-in to find.</param>
    /// <returns>The plug-in, if found, or <b>null</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="plugInName"/> is empty.</exception>
    public T? GetPlugIn<T>(string plugInName) where T : class, IGorgonPlugIn
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(plugInName);

        if (_scanned == 0)
        {
            ScanPlugIns();
        }

        return !_loadedPlugIns.TryGetValue(plugInName, out Lazy<IGorgonPlugIn, IDictionary<string, object>>? PlugIn) ? null : PlugIn.Value as T;
    }

    /// <summary>
    /// Function to retrieve a list of names for available plug-ins.
    /// </summary>
    /// <param name="assemblyName">[Optional] Name of the assembly containing the plug-ins.</param>
    /// <returns>A list of names for the available plug-ins.</returns>
    /// <remarks>
    /// <para>
    /// This method will retrieve a list of fully qualified type names for plug-ins contained within the <see cref="GorgonMefPlugInCache"/> passed to this object. This list is 
    /// not indicative of whether the type has been created or not.
    /// </para>
    /// <para>
    /// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only plug-in names belonging to that assembly. 
    /// If the assembly is not loaded, then an exception is thrown.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> GetPlugInNames(AssemblyName? assemblyName = null)
    {
        if (_scanned == 0)
        {
            ScanPlugIns();
        }

        return assemblyName is null
            ? [.. _loadedPlugIns.Keys]
            : _loadedPlugIns.Where(item =>
                                    {
                                        Debug.Assert(item.Value.Metadata.ContainsKey("Assembly"), "Assembly info not found.");

                                        AssemblyName? name = item.Value.Metadata["Assembly"] as AssemblyName;

                                        Debug.Assert(name is not null, "Assembly name is null.");

                                        return AssemblyName.ReferenceMatchesDefinition(name, assemblyName);
                                    })
                             .Select(item => item.Key)
                             .ToArray();
    }

    /// <summary>
    /// Function to retrieve the list of plug-ins from a given assembly.
    /// </summary>
    /// <typeparam name="T">Type of plug-in to retrieve. Must implement <see cref="IGorgonPlugIn"/>.</typeparam>
    /// <param name="assemblyName">[Optional] The name of the assembly associated with the plug-ins.</param>
    /// <returns>A list of plug-ins from the assembly.</returns>
    /// <remarks>
    /// This will retrieve all the plug-ins from the plug-in service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
    /// the only the assembly with that name will be scanned for the plug-in type.
    /// </remarks>
    public IReadOnlyList<T> GetPlugIns<T>(AssemblyName? assemblyName = null)
        where T : class, IGorgonPlugIn
    {
        if (_scanned == 0)
        {
            ScanPlugIns();
        }

        return assemblyName is null
            ? _loadedPlugIns.Values.Select(item => item.Value).OfType<T>().ToArray()
            : [.. _loadedPlugIns.Where(item =>
                                    {
                                        Debug.Assert(item.Value.Metadata.ContainsKey("Assembly"), "Assembly info not found.");

                                        AssemblyName? name = item.Value.Metadata["Assembly"] as AssemblyName;

                                        Debug.Assert(name is not null, "Assembly name is null.");

                                        return AssemblyName.ReferenceMatchesDefinition(name, assemblyName);
                                    })
                             .Select(item => item.Value.Value)
                             .OfType<T>()];
    }

    /// <summary>
    /// Function to scan for plug-ins in the loaded plug-in assemblies that are cached in the <see cref="GorgonMefPlugInCache"/> passed to this object.
    /// </summary>
    /// <remarks>
    /// This method will unload any active plug-ins, and, if implemented, call the dispose method for any plug-in.
    /// </remarks>
    public void ScanPlugIns()
    {
        try
        {
            while (true)
            {
                if (Interlocked.Exchange(ref _scanned, 1) == 1)
                {
                    SpinWait wait = new();
                    wait.SpinOnce();
                    continue;
                }

                UnloadAll();
                _log.Print("Scanning cached assemblies for available plug-ins...", LoggingLevel.Intermediate);

                _cache.Refresh();
                IEnumerable<Lazy<IGorgonPlugIn, IDictionary<string, object>>> PlugIns = _cache.EnumeratePlugIns();

                foreach (Lazy<IGorgonPlugIn, IDictionary<string, object>> PlugIn in PlugIns)
                {
                    Debug.Assert(PlugIn.Metadata.ContainsKey("Name"), "Name metadata not found.");

                    string? name = PlugIn.Metadata["Name"]?.ToString();

                    Debug.Assert(!string.IsNullOrWhiteSpace(name), "Name is null or whitespace.");

                    if (!_loadedPlugIns.TryAdd(name, PlugIn))
                    {
                        if (!_loadedPlugIns.TryUpdate(name, PlugIn, PlugIn))
                        {
                            continue;
                        }
                    }

                    _log.Print($"Created Plug-in '{name}'.", LoggingLevel.Verbose);
                }

                break;
            }
        }
        catch (ReflectionTypeLoadException rex)
        {
            StringBuilder errorMessage = new(512);

            foreach (Exception? loadEx in rex.LoaderExceptions)
            {
                if (loadEx is null)
                {
                    _log.PrintError("There were reflection type load exceptions, but no exception was found.", LoggingLevel.Verbose);
                    continue;
                }

                if (errorMessage.Length > 0)
                {
                    errorMessage.Append("\n\r");
                }

                errorMessage.Append(loadEx.Message);
            }

            throw new GorgonException(GorgonResult.CannotEnumerate,
                                      string.Format(Resources.GOR_ERR_PLUGIN_TYPE_LOAD_FAILURE, errorMessage));
        }
        finally
        {
            Interlocked.Exchange(ref _scanned, 2);
        }

        _log.Print($"{_loadedPlugIns.Count} Plug-ins found in the assembly cache.", LoggingLevel.Simple);
    }

    /// <summary>
    /// Function to unload a plug-in by its name.
    /// </summary>
    /// <param name="name">Fully qualified type name of the plug-in to remove.</param>
    /// <exception cref="ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
    /// <returns><b>true</b> if the plug-in was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
    public bool Unload(string name)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(name);

        if (!_loadedPlugIns.TryRemove(name, out Lazy<IGorgonPlugIn, IDictionary<string, object>>? PlugIn))
        {
            _log.Print($"PlugIn '{name}' was not found, it may not have been created yet.", LoggingLevel.Simple);
            return false;
        }

        if (!PlugIn.IsValueCreated)
        {
            return true;
        }

        DisposePlugIn(PlugIn.Value);
        return true;
    }

    /// <summary>
    /// Function to unload all the plug-ins.
    /// </summary>
    public void UnloadAll()
    {
        _log.Print("Unloading all plug-ins.", LoggingLevel.Simple);

        IGorgonPlugIn[] PlugIns = [.. _loadedPlugIns.Where(item => item.Value.IsValueCreated).Select(item => item.Value.Value)];
        _loadedPlugIns.Clear();

        foreach (IGorgonPlugIn PlugIn in PlugIns)
        {
            DisposePlugIn(PlugIn);
        }
    }
}
