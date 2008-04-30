#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
        private Renderable _owner = null;          // Owner for the animation list.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add an animation.
        /// </summary>
        /// <param name="animation">Animation to add.</param>
        internal void Add(Animation animation)
        {
			animation.SetOwner(_owner);
			AddItem(animation.Name, animation);
        }

        /// <summary>
        /// Function to create an animation.
        /// </summary>
        /// <param name="name">Name of the animation.</param>
        /// <param name="length">Length of the animation in milliseconds.</param>
        /// <returns>A new animation.</returns>
        public Animation Create(string name, float length)
        {
            Animation newAnimation = null;          // Animation.

            if (Contains(name))
                throw new ArgumentException("The animation '" + name + "' already exists.");

            newAnimation = new Animation(name, _owner, length);
			AddItem(name, newAnimation);
            newAnimation.Length = length;

            return newAnimation;
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
			newAnimation = (Animation)this[oldName].Clone();
			newAnimation.Name = newName;

			// Replace in the collection.
			Remove(oldName);
			Add(newAnimation);
		}

		/// <summary>
		/// Function to copy the animations in this animation list to another.
		/// </summary>
		/// <param name="destination">Animation list that will receive the animations.</param>
		public void CopyTo(AnimationList destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			// Clone the animations.
			foreach (Animation animation in this)
				destination.Add((Animation)animation.Clone());
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owner of this animation list.</param>
        internal AnimationList(Renderable owner)
        {
            _owner = owner;
        }
        #endregion
    }
}
