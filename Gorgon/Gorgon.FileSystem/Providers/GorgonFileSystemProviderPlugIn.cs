
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Monday, June 27, 2011 9:00:18 AM
// 

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Gorgon.Core;
using Gorgon.PlugIns;
using Microsoft.IO;

namespace Gorgon.IO.Providers;

/// <summary>
/// A file system provider that mounts Windows file system directories
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IGorgonFileSystemProvider"/> implementors must inherit from this type to create a provider plug-in. 
/// </para>
/// <para>
/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
/// file system file, or a file inside of a zip archive.  
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// As the documentation states, providers can read data from a file system. However, no mechanism is available to write to a file system through a provider. This is by design. The <see cref="IGorgonFileSystemWriter{T}"/> 
/// type allows writing to a file system via a predefined area in a physical file system. 
/// </para>
/// </note>
/// </para>
/// <para>
/// When this type is implemented, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). And since this type inherits from <see cref="GorgonPlugIn"/>, 
/// the file system provider can be loaded dynamically through Gorgon's plug-in system
/// </para>
/// <para>
/// This type allows the mounting of a directory so that data can be read from the native operating system file system. This is the default provider for any <see cref="IGorgonFileSystem"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonFileSystemProvider"/>
public abstract class GorgonFileSystemProviderPlugIn
        : GorgonFileSystemProvider, IGorgonPlugIn
{
    /// <summary>
    /// Property to return the assembly that contains this plugin.
    /// </summary>
    public AssemblyName Assembly
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the provider assembly (if applicable).
    /// </summary>
    public string ProviderPath
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the plugin assembly.
    /// </summary>
    string IGorgonPlugIn.PlugInPath => ProviderPath;

    /// <summary>
    /// Initializes new instance of the <see cref="GorgonFileSystemProviderPlugIn"/> class.
    /// </summary>
    /// <param name="providerDescription">The human readable description for the file system provider.</param>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    protected GorgonFileSystemProviderPlugIn(string providerDescription)
        : base(providerDescription)
    {
        Type type = GetType();
        Assembly = type.Assembly.GetName();
        ProviderPath = type.Assembly.ManifestModule.FullyQualifiedName;
    }
}
