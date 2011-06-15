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
        : Collection<Renderable>
    {
        #region Variables.
        private Renderable _owner;      // Owner object.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clear items from the list.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (Renderable child in this)
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
        public void Remove(Renderable child)
        {
			if (child == null)
				throw new ArgumentNullException("child");

			RemoveItem(child.Name);
            child.SetParent(null);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
		/// <param name="child">Child to add.</param>
        /// <param name="offset">Offset of the child from the parent.</param>        
        public void Add(Renderable child, Vector2D offset)
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
        public void Add(Renderable child)
        {
			if (child == null)
				throw new ArgumentNullException("child");

            Add(child, child.Position);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owning object.</param>
        public RenderableChildren(Renderable owner)
        {
            _owner = owner;
        }
        #endregion
    }
}
