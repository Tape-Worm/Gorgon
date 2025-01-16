
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
// Created: Monday, June 27, 2011 9:33:12 AM
// 

using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Providers.Properties;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// The plug in to create a zip file provider.
/// </summary>
internal class ZipPlugIn
    : GorgonFileSystemProviderPlugIn
{
    /// <inheritdoc/>
    public override IGorgonFileSystemProvider CreateProvider(IGorgonLog? log = null) => new ZipProvider(log ?? GorgonLog.NullLog);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipPlugIn"/> class.
    /// </summary>
    public ZipPlugIn()
        : base(Resources.GORFS_ZIP_DESC)
    {
    }
}
