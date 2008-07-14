#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
