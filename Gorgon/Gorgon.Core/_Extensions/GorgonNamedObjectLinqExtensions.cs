// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: November 17, 2023 5:05:35 PM
//

using Gorgon.Properties;

namespace Gorgon.Core;

/// <summary>
/// Extension methods for IEnumerable&lt;T&gt; types and <see cref="IGorgonNamedObject"/> values.
/// </summary>
public static class GorgonNamedObjectLinqExtensions
{
    /// <summary>
    /// Function to return an item in the <see cref="IEnumerable{T}"/> by name.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>
    /// <returns>The item with the specified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no with the <paramref name="name"/> could be found in the <paramref name="list"/>.</exception>
    /// <remarks>
    /// <para>
    /// The default comparer is <see cref="StringComparer.OrdinalIgnoreCase"/>. This means that the <see cref="IGorgonNamedObject.Name"/> is case insensitive, and uses a binary comparison.
    /// </para>
    /// </remarks>
    public static T GetByName<T>(this IEnumerable<T> list, string name)
        where T : IGorgonNamedObject => GetByName(list, name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Function to return an item in the <see cref="IEnumerable{T}"/> by name.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>
    /// <param name="comparer">The comparer to use when comparing strings.</param>
    /// <returns>The item with the specified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no with the <paramref name="name"/> could be found in the <paramref name="list"/>.</exception>    
    public static T GetByName<T>(this IEnumerable<T> list, string name, StringComparer comparer)
        where T : IGorgonNamedObject
    {
        ArgumentNullException.ThrowIfNull(list);

        if (string.IsNullOrEmpty(name))
        {
            throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
        }

        switch (list)
        {
            case IReadOnlyList<T> readOnlyList:
                for (int i = 0; i < readOnlyList.Count; ++i)
                {
                    T item = readOnlyList[i];

                    if (comparer.Compare(item.Name, name) == 0)
                    {
                        return item;
                    }
                }
                throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
            case IList<T> actualList:
                for (int i = 0; i < actualList.Count; ++i)
                {
                    T item = actualList[i];

                    if (comparer.Compare(item.Name, name) == 0)
                    {
                        return item;
                    }
                }
                throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
            default:
                {
                    using IEnumerator<T> enumerator = list.GetEnumerator();

                    if (!enumerator.MoveNext())
                    {
                        throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
                    }

                    do
                    {
                        if (enumerator.Current is null)
                        {
                            continue;
                        }

                        if (comparer.Compare(name, enumerator.Current.Name) == 0)
                        {
                            return enumerator.Current;
                        }
                    }
                    while (enumerator.MoveNext());

                    throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
                }
        }
    }

    /// <summary>
    /// Function to return the index of a <see cref="IGorgonNamedObject"/> in an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>
    /// <returns>The index of the item, if found. Or, -1, if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// The default comparer is <see cref="StringComparer.OrdinalIgnoreCase"/>. This means that the <see cref="IGorgonNamedObject.Name"/> is case insensitive, and uses a binary comparison.
    /// </para>
    /// </remarks>
    public static int IndexOfName<T>(this IEnumerable<T> list, string name)
        where T : IGorgonNamedObject => IndexOfName(list, name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Function to return whether a <see cref="IGorgonNamedObject"/> is contained in an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>    
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// The default comparer is <see cref="StringComparer.OrdinalIgnoreCase"/>. This means that the <see cref="IGorgonNamedObject.Name"/> is case insensitive, and uses a binary comparison.
    /// </para>
    /// </remarks>
    public static bool ContainsName<T>(this IEnumerable<T> list, string name)
        where T : IGorgonNamedObject => IndexOfName(list, name) != -1;

    /// <summary>
    /// Function to return whether a <see cref="IGorgonNamedObject"/> is contained in an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>
    /// <param name="comparer">The comparer to use when comparing strings.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    public static bool ContainsName<T>(this IEnumerable<T> list, string name, StringComparer comparer)
        where T : IGorgonNamedObject => IndexOfName(list, name, comparer) != -1;

    /// <summary>
    /// Function to return the index of a <see cref="IGorgonNamedObject"/> in an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to look for, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="list">The list to evaluate.</param>
    /// <param name="name">The name of the object to look up.</param>
    /// <param name="comparer">The comparer to use when comparing strings.</param>
    /// <returns>The index of the item, if found. Or, -1, if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> parameter is <b>null</b>.</exception>
    public static int IndexOfName<T>(this IEnumerable<T> list, string name, StringComparer comparer)
        where T : IGorgonNamedObject
    {
        int count = 0;

        ArgumentNullException.ThrowIfNull(list);

        if (string.IsNullOrEmpty(name))
        {
            return -1;
        }

        switch (list)
        {
            case IReadOnlyList<T> readOnlyList:
                for (int i = 0; i < readOnlyList.Count; i++)
                {
                    if (comparer.Compare(readOnlyList[i].Name, name) == 0)
                    {
                        return i;
                    }
                }

                return -1;
            case IList<T> actualList:
                for (int i = 0; i < actualList.Count; i++)
                {
                    if (comparer.Compare(actualList[i].Name, name) == 0)
                    {
                        return i;
                    }
                }

                return -1;
            default:
                {
                    using IEnumerator<T> enumerator = list.GetEnumerator();

                    if (!enumerator.MoveNext())
                    {
                        return -1;
                    }

                    do
                    {
                        if (enumerator.Current is null)
                        {
                            continue;
                        }

                        if (comparer.Compare(name, enumerator.Current.Name) == 0)
                        {
                            return count;
                        }

                        ++count;
                    }
                    while (enumerator.MoveNext());

                    return -1;
                }
        }
    }
}