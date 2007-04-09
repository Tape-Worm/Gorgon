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
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Object representing a list of child objects for sprites.
    /// </summary>
    public class RenderableChildren
        : Collection<RenderableChildren.ChildObject>
    {
        #region Classes.
        /// <summary>
        /// Object representing a child object.
        /// </summary>
        public class ChildObject
            : NamedObject
        {
            #region Variables.
            private Vector2D _offset = Vector2D.Zero;           // Offset position.
            private IMoveable _child = null;					// Child object.
            private string _childObjectName = string.Empty;     // Child object name.
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the real name of the child object.
            /// </summary>
            internal string ChildObjectName
            {
                get
                {
                    return _childObjectName;
                }
            }

            /// <summary>
            /// Property to return offset of the child from its parent.
            /// </summary>
            public Vector2D Offset
            {
                get
                {
                    return _offset;
                }
            }

            /// <summary>
            /// Property to return the child object.
            /// </summary>
            public IMoveable Child
            {
                get
                {
                    return _child;
                }
                set
                {
                    _child = value;
                }
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="name">Name of the child.</param>
            /// <param name="offset">Offset of the child.</param>
            /// <param name="child">Child object.</param>
            /// <param name="childObjectName">Child object name.</param>
            internal ChildObject(string name, Vector2D offset, IMoveable child, string childObjectName)
                : base(name)
            {
                _offset = offset;
                _child = child;
                _childObjectName = childObjectName;
            }
            #endregion
        }
        #endregion

        #region Variables.
        private IMoveable _owner;      // Owner object.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return a child object by its reference.
        /// </summary>
        /// <param name="child">Child object to retrieve.</param>
        /// <returns>The child object.</returns>
        public ChildObject this[IMoveable child]
        {
            get
            {
                foreach (ChildObject childObject in this)
                {
                    if (childObject.Child == child)
                        return childObject;
                }
                
                throw new SharpUtilities.Collections.KeyNotFoundException(child.Name);
            }
        }

        /// <summary>
        /// Property to return a child object by its index.
        /// </summary>
        public override RenderableChildren.ChildObject this[int index]
        {
            get
            {
                ChildObject item = null;        // Child.

                if ((index < 0) || (index > _items.Count))
                    throw new IndexOutOfBoundsException(index);

                item = base[index];
                return item;
            }
        }

        /// <summary>
        /// Property to return a child object by its name.
        /// </summary>
        public override RenderableChildren.ChildObject this[string key]
        {
            get
            {
                ChildObject item = null;        // Child.

                if (!Contains(key))
                    throw new SharpUtilities.Collections.KeyNotFoundException(key);

                item = base[key];
                return item;
            }
        }
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
            this[index].Child.SetParent(null);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Function to remove an object from the list by key.
        /// </summary>
        /// <param name="key">Key of the object to remove.</param>
        protected override void RemoveItem(string key)
        {
            this[key].Child.SetParent(null);
            base.RemoveItem(key);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
        /// <param name="name">Name of the child.</param>
        /// <param name="offset">Offset of the child from the parent.</param>
        /// <param name="child">Child to add.</param>
        internal void AddName(string name, Vector2D offset, string child)
        {
            ChildObject newChild = null;        // New object object.

            if ((name == null) || (name == string.Empty))
                throw new ArgumentNullException();

            if (Contains(name))
                throw new DuplicateObjectException(name);

            // Add the child object.            
            newChild = new ChildObject(name, offset, null, child);
            _items.Add(name, newChild);
        }

        /// <summary>
        /// Function to remove an object from the list by reference.
        /// </summary>
        /// <param name="child">Child object to remove.</param>
        public void Remove(IMoveable child)
        {
            if (!Contains(child.Name))
                throw new SharpUtilities.Collections.KeyNotFoundException(child.Name);

            child.SetParent(null);
            _items.Remove(child.Name);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
        /// <param name="name">Name of the child.</param>
        /// <param name="offset">Offset of the child from the parent.</param>
        /// <param name="child">Child to add.</param>
        public void Add(string name, Vector2D offset, IMoveable child)
        {
            ChildObject newChild = null;        // New object object.

            if ((name == null) || (name == string.Empty))
                throw new ArgumentNullException();

            if (Contains(name))
                throw new DuplicateObjectException(name);

            // Add the child object.            
            newChild = new ChildObject(name, offset, child, child.Name);
            _items.Add(name, newChild);
            if (child != null)
            {
                child.Position = offset;
                child.SetParent(_owner);
            }
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
        /// <param name="name">Name of the child.</param>
        /// <param name="child">Child to add.</param>
        public void Add(string name, IMoveable child)
        {
            Add(name, child.Position, child);
        }

        /// <summary>
        /// Function to add a child object.
        /// </summary>
        /// <param name="child">Child to add.</param>
        public void Add(IMoveable child)
        {
            Add(child.Name, child);
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
