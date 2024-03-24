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
// Created: March 23, 2024 6:42:51 PM
//
 
namespace Gorgon.Collections;

/// <summary>
/// Extension methods for the <see cref="IEnumerable{T}"/> type.
/// </summary>
public static class GorgonIEnumerableExtensions
{
    /// <summary>
    /// Function to convert an <see cref="IEnumerable{T}"/> to a <see cref="GorgonArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item in the array. Must implement <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="source">The enumerable to copy into the array.</param>
    /// <param name="markDirty">[Optional] <b>true</b> to mark the elements in the resulting array as dirty, <b>false</b> to mark the resulting array as clean.</param>
    /// <returns>A new <see cref="GorgonArray{T}"/> containing the elements in the <see cref="IEnumerable{T}"/>.</returns>
    /// <remarks>
    /// <para>
    /// The <see cref="GorgonArray{T}"/> returned will have
    /// </para>
    /// </remarks>
    public static GorgonArray<T> ToGorgonArray<T>(this IEnumerable<T> source, bool markDirty = false)
        where T : IEquatable<T>
            => new(source, markDirty);

    /// <summary>
    /// Function to flatten a tree of objects into a flat traversable list using a depth first approach.
    /// </summary>
    /// <typeparam name="T">The type of value in the tree.</typeparam>
    /// <param name="children">The list of objects to evaluate.</param>
    /// <param name="getChildren">The method to retrieve the next level of children.</param>
    /// <returns>An enumerable containing the flattened list of objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="children"/> parameter is <b>null</b>.</exception>
    public static IEnumerable<T> TraverseDepthFirst<T>(this IEnumerable<T> children, Func<T, IEnumerable<T>> getChildren)
    {
        ArgumentNullException.ThrowIfNull(children);

        Stack<T> queue = new();
        foreach (T child in children.Reverse())
        {
            queue.Push(child);
        }

        while (queue.Count > 0)
        {
            T node = queue.Pop();

            yield return node;

            IEnumerable<T> subChildren = getChildren?.Invoke(node);

            if (subChildren is null)
            {
                continue;
            }

            foreach (T child in subChildren.Reverse())
            {
                queue.Push(child);
            }
        }
    }

    /// <summary>
    /// Function to flatten a tree of objects into a flat traversable list using a breadth first approach.
    /// </summary>
    /// <typeparam name="T">The type of value in the tree.</typeparam>
    /// <param name="children">The list of objects to evaluate.</param>
    /// <param name="getChildren">The method to retrieve the next level of children.</param>
    /// <returns>An enumerable containing the flattened list of objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="children"/> parameter is <b>null</b>.</exception>
    public static IEnumerable<T> TraverseBreadthFirst<T>(this IEnumerable<T> children, Func<T, IEnumerable<T>> getChildren)
    {
        ArgumentNullException.ThrowIfNull(children);

        Queue<T> queue = new();
        foreach (T child in children)
        {
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            T node = queue.Dequeue();

            yield return node;

            IEnumerable<T> subChildren = getChildren?.Invoke(node);

            if (subChildren is null)
            {
                continue;
            }

            foreach (T child in subChildren)
            {
                queue.Enqueue(child);
            }
        }
    }
}
