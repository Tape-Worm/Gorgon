﻿
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
// Created: Monday, June 27, 2011 8:59:02 AM
// 

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;

namespace Gorgon.IO;

/// <summary>
/// A collection of file system virtual directories
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VirtualDirectoryCollection" /> class
/// </remarks>
/// <param name="parent">The parent directory that owns this collection.</param>
internal class VirtualDirectoryCollection(VirtualDirectory parent)
    : IReadOnlyDictionary<string, IGorgonVirtualDirectory>
{
    // The backing store for the directories.
    private readonly Dictionary<string, VirtualDirectory> _directories = new(StringComparer.OrdinalIgnoreCase);
    // The parent directory that owns this collection.
    private readonly VirtualDirectory _parent = parent;

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    /// <returns>
    /// The number of elements in the collection. 
    /// </returns>
    public int Count => _directories.Count;

    /// <inheritdoc/>
    IEnumerable<string> IReadOnlyDictionary<string, IGorgonVirtualDirectory>.Keys => _directories.Keys;

    /// <inheritdoc/>
    IEnumerable<IGorgonVirtualDirectory> IReadOnlyDictionary<string, IGorgonVirtualDirectory>.Values => _directories.Values;

    /// <summary>
    /// Property to return a directory by its name.
    /// </summary>
    IGorgonVirtualDirectory IReadOnlyDictionary<string, IGorgonVirtualDirectory>.this[string key]
    {
        get
        {
            // Ensure the key is formatted to remove illegal path characters.
            key = key.FormatPathPart();

            return !_directories.TryGetValue(key, out VirtualDirectory? directory)
                ? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, key))
                : directory;
        }
    }

    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}" />.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
    /// <exception cref="ArgumentException">Thrown when this collection already contains a directory with the same name as the <paramref name="item"/> parameter.</exception>
    public void Add(VirtualDirectory item)
    {
        if (_directories.ContainsKey(item.Name))
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, item.Name));
        }

        _directories.Add(item.Name, item);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
    /// <returns>
    /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
    /// </returns>
    public bool Remove(VirtualDirectory item)
    {
        if (item is null)
        {
            return false;
        }

        return _directories.Remove(item.Name);
    }

    /// <summary>
    /// Function to return a directory by its name.
    /// </summary>
    /// <param name="name">The name of the item to look up.</param>
    /// <param name="value">The directory, if found, or <b>null</b> if not.</param>
    /// <returns>
    ///   <b>true</b> if the directory was found, <b>false</b> if not.
    /// </returns>
    public bool TryGetValue(string name, [NotNullWhen(true)] out VirtualDirectory? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = null;
            return false;
        }

        return _directories.TryGetValue(name, out value);
    }

    /// <summary>
    /// Function to return the concrete virtual directories for for this collection.
    /// </summary>
    /// <returns>The <see cref="IEnumerable{T}"/> for this collection.</returns>
    public IEnumerable<VirtualDirectory> EnumerateVirtualDirectories() => _directories.Select(item => item.Value);

    /// <inhertidoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_directories.Values).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, IGorgonVirtualDirectory>> IEnumerable<KeyValuePair<string, IGorgonVirtualDirectory>>.GetEnumerator()
    {
        foreach (KeyValuePair<string, VirtualDirectory> directory in _directories)
        {
            yield return new KeyValuePair<string, IGorgonVirtualDirectory>(directory.Key, directory.Value);
        }
    }

    /// <summary>
    /// Function to add a new directory to the collection.
    /// </summary>
    /// <param name="mountPoint">The mount point that is creating this directory.</param>
    /// <param name="path">The path to the directory.</param>
    public VirtualDirectory Add(GorgonFileSystemMountPoint mountPoint, string path)
    {
        if (string.Equals(path, GorgonFileSystem.SeparatorString, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, "/"));
        }

        if (path[0] != GorgonFileSystem.DirectorySeparator)
        {
            path = GorgonFileSystem.SeparatorString + path;
        }

        ReadOnlySpan<char> dirPath = path.AsSpan();

        if (dirPath[^1] != GorgonFileSystem.DirectorySeparator)
        {
            dirPath = dirPath.FormatDirectory(GorgonFileSystem.DirectorySeparator);
        }

        GorgonSpanCharEnumerator directories = dirPath.Split(GorgonFileSystem.Separator);

        VirtualDirectory directory = _parent;

        foreach (ReadOnlySpan<char> item in directories)
        {
            string pathItem = item.FormatPathPart().ToString();

            // If there's a file with the same name as the directory, then we can't continue.
            if (directory.Files.ContainsKey(pathItem))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, directory.Files[pathItem].FullPath));
            }

            if (directory.Directories.TryGetValue(pathItem, out VirtualDirectory? childDirectory))
            {
                directory = childDirectory;
            }
            else
            {
                VirtualDirectory newDirectoryInfo = new(mountPoint, _parent.FileSystem, directory, pathItem);
                directory.Directories.Add(newDirectoryInfo);
                directory = newDirectoryInfo;
            }
        }

        if (directory == _parent)
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
        }

        return directory;
    }

    /// <summary>
    /// Removes all items from the <see cref="ICollection{T}" />.
    /// </summary>
    public void Clear() => _directories.Clear();

    /// <summary>
    /// Function to return whether an item with the specified name exists in this collection.
    /// </summary>
    /// <param name="name">Name of the item to find.</param>
    /// <returns><b>true</b>if found, <b>false</b> if not.</returns>
    public bool ContainsKey(string name)
    {
        name = name.FormatPathPart();

        return !string.IsNullOrWhiteSpace(name) && _directories.ContainsKey(name);
    }

    /// <summary>
    /// Function to return an item from the collection.
    /// </summary>
    /// <param name="name">The name of the item to look up.</param>
    /// <param name="value">The item, if found, or the default value for the type if not.</param>
    /// <returns><b>true</b> if the item was found, <b>false</b> if not.</returns>
    bool IReadOnlyDictionary<string, IGorgonVirtualDirectory>.TryGetValue(string name, [MaybeNullWhen(false)] out IGorgonVirtualDirectory value)
    {
        if (!TryGetValue(name, out VirtualDirectory? directory))
        {
            value = null;
            return false;
        }

        value = directory;
        return true;
    }
}
