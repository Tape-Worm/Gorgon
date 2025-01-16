
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
using Gorgon.Diagnostics;
using Gorgon.PlugIns;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A plug-in factory for creating file system providers.
/// </summary>
/// <param name="providerDescription">The human readable description for the file system provider.</param>
/// <remarks>
/// <para>
/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the 
/// <see cref="IGorgonFileSystem"/> object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea 
/// if the file is a standard file system file, or a file inside of a zip archive.  
/// </para>
/// <para>
/// This serves as a base class for a file system provider plug-in. Plug-in developers must inherit this class and implement the <see cref="CreateProvider"/> method to return an instance of the file system 
/// provider.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonFileSystemProvider"/>
[method: RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
public abstract class GorgonFileSystemProviderPlugIn(string providerDescription)
                : GorgonPlugIn(providerDescription)
{
    /// <summary>
    /// Function to create the file system provider associated with the plug-in.
    /// </summary>
    /// <param name="log">[Optional] The logging interface used for debug messaging.</param>
    /// <returns>A new instance of a <see cref="IGorgonFileSystemProvider"/>.</returns>
    public abstract IGorgonFileSystemProvider CreateProvider(IGorgonLog? log = null);
}
