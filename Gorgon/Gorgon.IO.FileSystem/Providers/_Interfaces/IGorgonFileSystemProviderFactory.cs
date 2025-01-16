
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Saturday, September 19, 2015 11:40:20 PM
// 

using System.Diagnostics.CodeAnalysis;
using Gorgon.Core;
using Gorgon.PlugIns;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A factory object used to create file system provider plug-ins
/// </summary>
/// <remarks>
/// <para>
/// This will generate providers that will allow access to different types of file systems. For example, a user may create a file system provider that will open 7-zip files, but the <see cref="IGorgonFileSystem"/> 
/// will not know how to read those files without the appropriate provider. This object would be used to load that 7-zip provider, and add it to the file system object so that it will know how to mount those 
/// file types
/// </para>
/// <para>
/// File system providers are plug-ins, and should have their assemblies loaded by the <see cref="GorgonMefPlugInCache"/> before using this method and a <see cref="IGorgonPlugInService"/> should be 
/// created in order to pass it to this factory
/// </para>
/// </remarks>
/// <example>
/// The following example shows how to use the provider factory:
/// <code language="csharp"> 
/// <![CDATA[
/// // In a real world application, you would keep your cache for as long as you need your plug-ins
/// // Premature disposal can cause errors
/// using GorgonMefPlugInCache cache = new GorgonMefPlugInCache();
/// 
///	// Create the provider factory
///	IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(cache);
/// 
///	// Get our provider from the factory (no, we do not have a 7-zip provider, this is just an example)
///	IGorgonFileSystemProvider provider = factory.CreateProvider(@"C:\FileSystemProviders\Gorgon.FileSystem.7zip.dll", "Gorgon.FileSystem.SevenZipProvider");
/// 
///	// Mount the file system
///	IGorgonFileSystem fileSystem = new GorgonFileSystem(provider);
/// 
///	fileSystem.Mount("c:\path\to\your\archive\file.7z");
///		
/// ]]>
/// </code>
/// </example>
public interface IGorgonFileSystemProviderFactory
{
    /// <summary>
    /// Property to return the list of plug-ins already loaded by the factory.
    /// </summary>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> PlugIns
    {
        get;
    }

    /// <summary>
    /// Function to create a new <see cref="IGorgonFileSystemProvider"/> from the specified plug-in.
    /// </summary>
    /// <param name="path">The path to the file system plug-in assemblies.</param>
    /// <param name="providerPlugInName">The fully qualified type name of the plugin that contains the file system provider.</param>
    /// <returns>The new <see cref="IGorgonFileSystemProvider"/> object.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/>, or the <paramref name="providerPlugInName"/> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the driver type name specified by <paramref name="providerPlugInName"/> was not found in any of the loaded plug-in assemblies.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="path"/> was invalid.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will create a new instance of a <see cref="IGorgonFileSystemProvider"/> from the plug-in assembly specified by the <paramref name="path"/>. This object can then be used to 
    /// <see cref="IGorgonFileSystem.Mount(string, string?, IGorgonFileSystemProvider?)"/> a physical file system to the <see cref="IGorgonFileSystem"/> object.
    /// </para>
    /// <para>
    /// The plug-in associated with the provider is cached internally, so subsequent calls will not reload the plug-in assembly unnecessarily. All plug-ins can be accessed through the 
    /// <see cref="PlugIns"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonFileSystem"/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    IGorgonFileSystemProvider CreateProvider(string path, string providerPlugInName);

    /// <summary>
    /// Function to retrieve all the <see cref="IGorgonFileSystemProvider"/> objects from the available plugins in a plug-in assembly.
    /// </summary>
    /// <param name="path">The path to the file system plug-in assemblies.</param>
    /// <returns>A list of <see cref="IGorgonFileSystemProvider"/> objects from the plug-ins contained within the assembly specified on the path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b></exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> was invalid.</exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method that will load all the <see cref="IGorgonFileSystemProvider"/> objects from the plug-ins contained within a plug-in assembly at once.
    /// </para>
    /// </remarks>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    IReadOnlyList<IGorgonFileSystemProvider> CreateProviders(string path);
}
