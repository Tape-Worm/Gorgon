#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: September 21, 2018 5:15:47 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Collections
{
    /// <summary>
    /// LINQ extension methods for tree structures.
    /// </summary>
    public static class TreeLinqExtensions
    {
        /// <summary>
        /// Function to flatten a tree of objects into a flat traversable list.
        /// </summary>
        /// <typeparam name="T">The type of value in the tree.</typeparam>
        /// <param name="children">The list of objects to evaluate.</param>
        /// <param name="getChildren">The method to retrieve the next level of children.</param>
        /// <returns>An enumerable containing the flattened list of objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="children"/> parameter is <b>null</b>.</exception>
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> children, Func<T, IEnumerable<T>> getChildren)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            var queue = new Queue<T>();
            foreach (T child in children)
            {
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                T node = queue.Dequeue();

                yield return node;

                IEnumerable<T> subChildren = getChildren?.Invoke(node);

                if (subChildren == null)
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
}
