#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, May 27, 2008 2:04:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Object representing a list of GUI objects.
	/// </summary>
	public class GUIObjectCollection
		: BaseCollection<GUIObject>
	{
		#region Properties.
		/// <summary>
		/// Property to return a GUI object from the collection by its index.
		/// </summary>
		public GUIObject this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a GUI object from the collection by its name.
		/// </summary>
		public GUIObject this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the ZOrder of the objects.
		/// </summary>
		internal void UpdateZOrder()
		{
			int zOrder = 0;
			var guiObjects = from obj in this
							 orderby obj.ZOrder ascending
							 select obj;

			foreach (GUIObject obj in guiObjects)
			{
				obj.SuspendOrdering = true;
				obj.ZOrder = zOrder;
				zOrder++;
				obj.SuspendOrdering = false;
			}
		}

		/// <summary>
		/// Function to add a GUI object to the collection.
		/// </summary>
		/// <param name="guiObject"></param>
		internal void Add(GUIObject guiObject)
		{
			if (guiObject == null)
				throw new ArgumentNullException("guiObject");
			if (guiObject.Owner != null)
				throw new ArithmeticException("The GUI object '" + guiObject.Name + "'already has an owner.");
			AddItem(guiObject.Name, guiObject);
			guiObject.ZOrder = (from obj in this where obj != null orderby obj.ZOrder ascending select obj.ZOrder).Max() + 1;
		}

		/// <summary>
		/// Function to remove a GUI object from the collection.
		/// </summary>
		/// <param name="guiObject">GUI object to remove.</param>
		internal void Remove(GUIObject guiObject)
		{
			if (guiObject == null)
				throw new ArgumentNullException("guiObject");
			RemoveItem(guiObject.Name);
			UpdateZOrder();
		}

		/// <summary>
		/// Function to find the index of a specific GUI object instance.
		/// </summary>
		/// <param name="guiObject">Instance to find.</param>
		/// <returns>The index of the object in the collection, or -1 if not found.</returns>
		public int IndexOf(GUIObject guiObject)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i] == guiObject)
					return i;
			}
			return -1;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIObjectCollection"/> class.
		/// </summary>
		internal GUIObjectCollection()
			: base(128, false)
		{
		}
		#endregion
	}
}
