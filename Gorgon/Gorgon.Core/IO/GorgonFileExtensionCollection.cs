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
// Created: Sunday, September 22, 2013 8:43:37 PM
// 

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// A collection of file extensions
/// </summary>
public class GorgonFileExtensionCollection
    : IDictionary<string, GorgonFileExtension>, IReadOnlyDictionary<string, GorgonFileExtension>
{
    // The list of extensions.
    private readonly Dictionary<string, GorgonFileExtension> _extensions = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    ICollection<string> IDictionary<string, GorgonFileExtension>.Keys => _extensions.Keys;

    /// <inheritdoc/>
    ICollection<GorgonFileExtension> IDictionary<string, GorgonFileExtension>.Values => _extensions.Values;

    /// <inheritdoc/>
    public int Count => throw new NotImplementedException();

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, GorgonFileExtension>>.IsReadOnly => false;

    /// <inheritdoc/>
    IEnumerable<string> IReadOnlyDictionary<string, GorgonFileExtension>.Keys => _extensions.Keys;

    /// <inheritdoc/>
    IEnumerable<GorgonFileExtension> IReadOnlyDictionary<string, GorgonFileExtension>.Values => _extensions.Values;

    /// <summary>
    /// Property to set or return an extension in the collection.
    /// </summary>
    public GorgonFileExtension this[string extension]
    {
        get
        {
            if (extension.StartsWith('.'))
            {
                extension = extension[1..];
            }

            return _extensions[extension];
        }
        set
        {
            if (extension.StartsWith('.'))
            {
                extension = extension[1..];
            }

            _extensions[extension] = value;
        }
    }

    /// <summary>
    /// Function to add a file extension to the collection.
    /// </summary>
    /// <param name="extension">Extension to add to the collection.</param>
    public void Add(GorgonFileExtension extension)
    {
        if (extension.Extension is null)
        {
            extension = new GorgonFileExtension(string.Empty, extension.Description);
        }

        if (ContainsKey(extension.Extension))
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_EXISTS, extension.Extension));
        }

        _extensions[extension.Extension] = extension;
    }

    /// <summary>
    /// Function to remove a file extension from the collection.
    /// </summary>
    /// <param name="extension">The file extension to remove from the collection.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="extension"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="extension"/> could not be found in the collection.</exception>
    public void Remove(string extension)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(extension);

        if (extension.StartsWith('.'))
        {
            extension = extension[1..];
        }

        if (!ContainsKey(extension))
        {
            throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_NOT_FOUND, extension));
        }

        _extensions.Remove(extension);
    }

    /// <summary>
    /// Function to remove a file extension from the collection.
    /// </summary>
    /// <param name="extension">The file extension to remove from the collection.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="extension"/> could not be found in the collection.</exception>
    public void Remove(GorgonFileExtension extension)
    {
        if (!ContainsKey(extension.Extension))
        {
            throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_NOT_FOUND, extension));
        }

        _extensions.Remove(extension.Extension);
    }

    /// <summary>
    /// Function to clear all items from the collection.
    /// </summary>
    public void Clear() => _extensions.Clear();

    /// <inheritdoc/>
    void IDictionary<string, GorgonFileExtension>.Add(string key, GorgonFileExtension value) => Add(value);

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _extensions.ContainsKey(key);

    /// <inheritdoc/>
    bool IDictionary<string, GorgonFileExtension>.Remove(string key)
    {
        if (!ContainsKey(key))
        {
            return false;
        }

        Remove(key);
        return true;
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out GorgonFileExtension value) => _extensions.TryGetValue(key, out value);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, GorgonFileExtension>>.Add(KeyValuePair<string, GorgonFileExtension> item) => Add(item.Value);

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, GorgonFileExtension>>.Contains(KeyValuePair<string, GorgonFileExtension> item) => ContainsKey(item.Value.Extension);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, GorgonFileExtension>>.CopyTo(KeyValuePair<string, GorgonFileExtension>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, GorgonFileExtension>>)_extensions).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, GorgonFileExtension>>.Remove(KeyValuePair<string, GorgonFileExtension> item)
    {
        if (!ContainsKey(item.Value.Extension))
        {
            return false;
        }

        Remove(item.Value.Extension);
        return true;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, GorgonFileExtension>> GetEnumerator() => _extensions.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_extensions).GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileExtensionCollection"/> class.
    /// </summary>
    public GorgonFileExtensionCollection()
    {
    }
}
