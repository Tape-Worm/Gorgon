
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, January 19, 2013 4:13:55 PM
// 

using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO;

/// <summary>
/// A mount point for the virtual file system
/// </summary>
public record GorgonFileSystemMountPoint
{
    /// <summary>
    /// An empty mount point.
    /// </summary>
    public static readonly GorgonFileSystemMountPoint Empty = new(null!, string.Empty, GorgonFileSystem.SeparatorString, true);

    /// <summary>
    /// Property to return the flag to indicate whether the mount point is a fake mount point or not (i.e. it has no real physical location).
    /// </summary>
    internal bool IsFakeMount
    {
        get;
    }

    /// <summary>
    /// Property to return the provider for this mount point.
    /// </summary>
    public IGorgonFileSystemProvider Provider
    {
        get;
    }

    /// <summary>
    /// Property to return the physical location of the mount point.
    /// </summary>
    public string PhysicalPath
    {
        get;
    }

    /// <summary>
    /// Property to return the virtual location of the mount point.
    /// </summary>
    public string MountLocation
    {
        get;
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORFS_TOSTR_MOUNTPOINT, MountLocation, PhysicalPath);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Provider, PhysicalPath, MountLocation);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemMountPoint" /> struct.
    /// </summary>
    /// <param name="provider">The provider for this mount point.</param>
    /// <param name="physicalPath">The physical path.</param>
    /// <param name="mountLocation">[Optional] The mount location.</param>
    /// <param name="isFakeMountPoint">[Optional] <b>true</b> if the mount point doesn't use a real physical location, or <b>false</b> if it does.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
    internal GorgonFileSystemMountPoint(IGorgonFileSystemProvider provider, string physicalPath, string? mountLocation = null, bool isFakeMountPoint = false)
    {
        Provider = provider;
        PhysicalPath = physicalPath;
        MountLocation = (mountLocation ?? string.Empty).FormatDirectory(GorgonFileSystem.DirectorySeparator);
        IsFakeMount = isFakeMountPoint;
    }
}
