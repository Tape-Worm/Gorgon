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
// Created: Friday, November 24, 2006 7:48:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Object representing a list of child objects for sprites.
    /// </summary>
    public class RenderableChildren
        : Collection<IMoveable>
    {
        #region Variables.
        private IMoveable _owner;      // Owner object.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clear items from the list.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (IMoveable child in this)
                child.SetParent(null);

            base.ClearItems();
        }
        
        /// <summary>
        /// Function to remove an object from the list by index.
        /// </summary>
        /// <param name="index">Index to remove at.</param>
        protected override void RemoveItem(int index)
        {
            this[index].SetParent(null);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Function to remove an object from the list by key.
        /// </summary>
        /// <param name="key">Key of the object to remove.</param>
        protected override void RemoveItem(string key)
        {
            this[key].SetParent(null);
            base.RemoveItem(key);
        }

        /// <summary>
        /// Function to remove an object from the list by reference.
        /// </summary>
        /// <param name="child">Child object to remove.</param>
        public void Remove(IMoveable child)
        {
			RemoveItem(child.Name);
            child.SetParent(null);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
		/// <param name="child">Child to add.</param>
        /// <param name="offset">Offset of the child from the parent.</param>        
        public void Add(IMoveable child, Vector2D offset)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (Contains(child.Name))
                throw new ArgumentException("The child object '" + child.Name + "' is already attached");

            // Add the child object.            
			AddItem(child.Name, child);

            child.Position = offset;
            child.SetParent(_owner);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
        /// <param name="child">Child to add.</param>
        public void Add(IMoveable child)
        {
            Add(child, child.Position);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owning object.</param>
        public RenderableChildren(IMoveable owner)
        {
            _owner = owner;
        }
        #endregion
    }
}
