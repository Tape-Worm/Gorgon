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
// Created: August 15, 2018 11:50:44 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Animation.Properties;
using Gorgon.Core;

namespace Gorgon.Animation
{
    /// <summary>
    /// A collection of animations for a <see cref="GorgonAnimationController{T}"/>
    /// </summary>
    /// <seealso cref="GorgonAnimationController{T}"/>
    internal class AnimationCollection
        : IGorgonAnimationCollection
    {
        #region Variables.
        // List of read/write animations.
        private readonly Dictionary<string, IGorgonAnimation> _rwAnimations = new Dictionary<string, IGorgonAnimation>(StringComparer.OrdinalIgnoreCase);
        // Function called when an animation is removed from the collection, or the collection is cleared.
        private readonly Action<IGorgonAnimation> _stopAnimation;
        #endregion

        #region Properties.
        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count => _rwAnimations.Count;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool ICollection<IGorgonAnimation>.IsReadOnly => false;

        /// <summary>
        /// Property to return an animation by name.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the value being assigned to the collection is <b>null</b>.</exception>
        public IGorgonAnimation this[string name]

        {
            get => _rwAnimations[name];
            set => _rwAnimations[name] = value ?? throw new ArgumentNullException();
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add an animation to the collection.
        /// </summary>
        /// <param name="animation">The animation to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the animation already exists in this collection.
        /// <para>-or-</para>
        /// <para>Thrown if the animation was created by another controller.</para>
        /// </exception>
        public void Add(IGorgonAnimation animation)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            if (_rwAnimations.ContainsKey(animation.Name))
            {
                throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, animation.Name), nameof(animation));
            }

            _rwAnimations.Add(animation.Name, animation);
        }
        
		/// <summary>
		/// Function to clear the animation collection.
		/// </summary>
		public void Clear()
		{
			// Stop the current animation.
			_stopAnimation?.Invoke(null);
            _rwAnimations.Clear();
		}
        
		/// <summary>
		/// Function to remove an animation from the collection.
		/// </summary>
		/// <param name="animationName">Name of the animation to remove.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="animationName"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the animation parameter is an empty string.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
		public void Remove(string animationName)
		{
		    if (animationName == null)
			{
				throw new ArgumentNullException(nameof(animationName));
			}

			if (string.IsNullOrWhiteSpace(animationName))
			{
				throw new ArgumentEmptyException(nameof(animationName));
			}

			if (!_rwAnimations.TryGetValue(animationName, out IGorgonAnimation animation))
			{
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animationName));
		    }

            _stopAnimation.Invoke(animation);
		    _rwAnimations.Remove(animationName);
		}

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
        public bool Contains(IGorgonAnimation item)
        {
            return (item != null) && (_rwAnimations.ContainsKey(item.Name));
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(IGorgonAnimation[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            foreach (KeyValuePair<string, IGorgonAnimation> animation in _rwAnimations)
            {
                array[arrayIndex++] = animation.Value;
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(IGorgonAnimation item)
        {
            return (item != null) && (_rwAnimations.Remove(item.Name));
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGorgonAnimation> GetEnumerator()
        {
            foreach (KeyValuePair<string, IGorgonAnimation> animation in _rwAnimations)
            {
                yield return animation.Value;
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rwAnimations.Values.GetEnumerator();
        }

        /// <summary>
        /// Function to retrieve an item from the collection by name.
        /// </summary>
        /// <param name="name">The name of the animation to retrieve.</param>
        /// <param name="animation">The animation in the collection, or <b>null</b> if no animation exists with the specified name.</param>
        /// <returns><b>true</b> if the object was found, <b>false</b> if not.</returns>
        public bool TryGetValue(string name, out IGorgonAnimation animation)
        {
            if (!_rwAnimations.TryGetValue(name, out IGorgonAnimation result))
            {
                animation = null;
                return false;
            }

            animation = result;

            return true;
        }

        /// <summary>
        /// Function to determine if an animation with the specified name exists in the collection.
        /// </summary>
        /// <param name="name">The name of the collection.</param>
        /// <returns><b>true</b> if the item exists, or <b>false</b> if not.</returns>
        public bool Contains(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && _rwAnimations.ContainsKey(name);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationCollection"/> class.
        /// </summary>
        /// <param name="stopAnimationCallback">The callback method used to stop an animation if it is removed from the collection, or the collection is cleared.</param>
        internal AnimationCollection(Action<IGorgonAnimation> stopAnimationCallback)
        {
            _stopAnimation = stopAnimationCallback;
        }
        #endregion
    }
}
