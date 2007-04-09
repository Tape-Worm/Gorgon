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
using SharpUtilities.Collections;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Animations
{
    /// <summary>
    /// Object representing a list of animations.
    /// </summary>
    public class AnimationList
        : Collection<Animation>
    {
        #region Variables.
        private IAnimatable _owner = null;          // Owner for the animation list.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add an animation.
        /// </summary>
        /// <param name="animation">Animation to add.</param>
        internal void Add(Animation animation)
        {
            if (Contains(animation.Name))
                throw new DuplicateObjectException(animation.Name);

            _items.Add(animation.Name, animation);
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
                throw new DuplicateObjectException(name);

            newAnimation = new Animation(name, _owner, length);
            _items.Add(name, newAnimation);
            newAnimation.Length = length;

            return newAnimation;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owner of this animation list.</param>
        internal AnimationList(IAnimatable owner)
        {
            _owner = owner;
        }
        #endregion
    }
}
