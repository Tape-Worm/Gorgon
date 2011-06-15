#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, November 20, 2006 1:19:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Object representing a list of animations.
    /// </summary>
    public class AnimationList
        : Collection<Animation>
    {
        #region Variables.
        private IAnimated _owner = null;          // Owner for the animation list.
        #endregion

        #region Methods.
		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public override void Clear()
		{
			foreach (Animation anim in this)
				anim.SetOwner(null);
			base.Clear();
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		public override void Remove(int index)
		{
			this[index].SetOwner(null);
			base.Remove(index);
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		public override void Remove(string key)
		{
			this[key].SetOwner(null);
			base.Remove(key);
		}

        /// <summary>
        /// Function to add an animation.
        /// </summary>
        /// <param name="animation">Animation to add.</param>
        public void Add(Animation animation)
        {
			if (animation == null)
				throw new ArgumentNullException("animation");

			AddItem(animation.Name, animation);
			if (animation.Owner != _owner)
				animation.SetOwner(_owner);
        }

		/// <summary>
		/// Function to rename an animation.
		/// </summary>
		/// <param name="oldName">Old name of the animation.</param>
		/// <param name="newName">New name of the animation.</param>
		public void Rename(string oldName, string newName)
		{
			Animation newAnimation = null;			// Clone of the animation.
			if (string.IsNullOrEmpty(oldName))
				throw new ArgumentNullException("oldName");
			if (string.IsNullOrEmpty(newName))
				throw new ArgumentNullException("newName");

            if (!Contains(oldName))
                throw new KeyNotFoundException("The animation '" + oldName + "' does not exist.");
            if (Contains(newName))
                throw new ArgumentException("The animation '" + newName + "' already exists.");

			// Create a copy.
			newAnimation = this[oldName].Clone(_owner);
			newAnimation.Name = newName;

			// Replace in the collection.
			Remove(oldName);
			Add(newAnimation);
		}

		/// <summary>
		/// Function to copy the animations in this animation list to another.
		/// </summary>
		/// <param name="destination">Animated object that will receive a copy of the animations in this list.</param>
		public void CopyTo(IAnimated destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			// Clone the animations.
			foreach (Animation animation in this)
			{
				if (destination.Animations.Contains(animation.Name))
					throw new ArgumentException("The destination object already contains an animation with the name '" + animation.Name + "'.");

				destination.Animations.Add(animation.Clone(destination));
			}
		}
        #endregion

        #region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationList"/> class.
		/// </summary>
		/// <param name="owner">Owner of this animation list.</param>
        public AnimationList(IAnimated owner)
			: base(false)
        {
			if (owner == null)
				throw new ArgumentNullException("owner");

            _owner = owner;
        }
        #endregion
    }
}
