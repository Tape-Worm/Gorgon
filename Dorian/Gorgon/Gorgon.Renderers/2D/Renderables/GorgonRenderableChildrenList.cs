#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, February 28, 2013 7:36:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// A list of renderables that are children to a parent renderable.
    /// </summary>
    public class GorgonRenderableChildrenList
        : GorgonBaseNamedObjectList<IRenderableChild>
    {
        #region Variables.
        private IRenderableChild _parent = null;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return a child renderable at the specified index.
        /// </summary>
        public IRenderableChild this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                if (value == null)
                {
                    RemoveItem(index);
                    return;
                }

                SetItem(index, value);
            }
        }

        /// <summary>
        /// Property to set or return a child renderable with the specified name.
        /// </summary>
        public IRenderableChild this[string name]
        {
            get
            {
                return GetItem(name);
            }
            set
            {
                int index = IndexOf(name);

                if (value == null)
                {
                    if (index != -1)
                    {
                        RemoveItem(index);
                    }
                    return;
                }

                if (index != -1)
                {
                    SetItem(index, value);
                }
                else
                {
                    AddItem(value);
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.OnParentChanged(null);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            GorgonDebug.AssertParamRange(index, 0, Count, "index");
            this[index].OnParentChanged(null);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void RemoveItem(IRenderableChild item)
        {
            GorgonDebug.AssertNull<IRenderableChild>(item, "item");
            item.OnParentChanged(null);
            base.RemoveItem(item);
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void AddItem(IRenderableChild value)
        {
            GorgonDebug.AssertNull<IRenderableChild>(value, "value");

#if DEBUG
            if (IndexOf(value.Name) != -1)
            {
                throw new ArgumentException("The renderable '" + value.Name + "' already exists in this collection.", "value");
            }

            if (value.Parent != null)
            {
                throw new ArgumentException("The renderable '" + value.Name + "' already has a parent.", "value");
            }
#endif
            value.OnParentChanged(_parent);
            base.AddItem(value);
        }

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentException">The renderable ' + value.Name + ' already has a parent.;value</exception>
        protected override void SetItem(int index, IRenderableChild value)
        {
            GorgonDebug.AssertNull<IRenderableChild>(value, "value");
#if DEBUG
            if (value.Parent != null)
            {
                throw new ArgumentException("The renderable '" + value.Name + "' already has a parent.", "value");
            }
#endif
            value.OnParentChanged(_parent);
            base.SetItem(index, value);
        }

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void SetItem(IRenderableChild value)
        {
            GorgonDebug.AssertNull<IRenderableChild>(value, "value");
#if DEBUG
            if (value.Parent != null)
            {
                throw new ArgumentException("The renderable '" + value.Name + "' already has a parent.", "value");
            }
#endif
            value.OnParentChanged(_parent);
            base.SetItem(value);
        }

        /// <summary>
        /// Adds the items.
        /// </summary>
        /// <param name="items">The items.</param>
        protected override void AddItems(IEnumerable<IRenderableChild> items)
        {
            GorgonDebug.AssertNull<IEnumerable<IRenderableChild>>(items, "items");

            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        /// Function to perform any updates on child renderables.
        /// </summary>
        internal void UpdateChildren()
        {
            if (Count == 0)
            {
                return;
            }

            foreach (var child in this)
            {
                child.UpdateFromParent();
            }
        }

        /// <summary>
        /// Function to add a child renderable to the list.
        /// </summary>
        /// <param name="child">Child renderable to add.</param>
        /// <param name="offset">Initial offset of the child from the parent.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="child"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the child parameter already has a parent.
        /// <para>-or-</para>
        /// <para>The the collection already contains a child with the same name.</para>
        /// </exception>
        public void Add(IRenderableChild child, Vector2 offset)
        {
            AddItem(child);
            child.Position = offset;
        }

        /// <summary>
        /// Function to remove a child from the collection.
        /// </summary>
        /// <param name="child">Child to remove from the collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="child"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown if the child does not exist in the collection.</exception>
        public void Remove(IRenderableChild child)
        {
            RemoveItem(child);
        }

        /// <summary>
        /// Function to remove a child from the collection by name.
        /// </summary>
        /// <param name="name">Name of the child to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="child"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the child does not exist in this collection.</exception>
        public void Remove(string name)
        {
            RemoveItem(this[name]);
        }

        /// <summary>
        /// Function to remove a child from the collection by index.
        /// </summary>
        /// <param name="index">Index of the child to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the index is less than 0 or greater than or equal to the number of items in the collection.</exception>
        public void Remove(int index)
        {
            RemoveItem(index);
        }

        /// <summary>
        /// Function to clear the list of renderable items.
        /// </summary>
        public void Clear()
        {
            ClearItems();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderableChildrenList{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent that owns the renderable children.</param>
        internal GorgonRenderableChildrenList(IRenderableChild parent)
        {
            _parent = parent;            
        }
        #endregion
    }
}
