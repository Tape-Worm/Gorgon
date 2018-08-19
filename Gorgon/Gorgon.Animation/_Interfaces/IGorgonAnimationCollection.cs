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
// Created: August 16, 2018 11:43:57 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Animation
{
    /// <summary>
    /// A collection of animations for a <see cref="GorgonAnimationController{T}"/>
    /// </summary>
    /// <seealso cref="GorgonAnimationController{T}"/>
    public interface IGorgonAnimationCollection
        : ICollection<IGorgonAnimation>
    {
        /// <summary>
        /// Property to return an animation by name.
        /// </summary>
        IGorgonAnimation this[string name]
        {
            get;
            set;
        }

        /// <summary>
        /// Function to retrieve an item from the collection by name.
        /// </summary>
        /// <param name="name">The name of the animation to retrieve.</param>
        /// <param name="animation">The animation in the collection, or <b>null</b> if no animation exists with the specified name.</param>
        /// <returns><b>true</b> if the object was found, <b>false</b> if not.</returns>
        bool TryGetValue(string name, out IGorgonAnimation animation);

        /// <summary>
        /// Function to determine if an animation with the specified name exists in the collection.
        /// </summary>
        /// <param name="name">The name of the collection.</param>
        /// <returns><b>true</b> if the item exists, or <b>false</b> if not.</returns>
        bool Contains(string name);

        /// <summary>
        /// Function to remove an animation from the collection.
        /// </summary>
        /// <param name="animationName">Name of the animation to remove.</param>
        void Remove(string animationName);
    }
}