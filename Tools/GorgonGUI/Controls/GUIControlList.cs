#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, January 22, 2007 1:00:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities.Collections;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing a GUI control list.
	/// </summary>
	public class GUIControlList
		: BaseCollection<GUIControl>
	{
		#region Variables.
		private GUIControl _owner;		// Control that owns this list.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a GUI control.
		/// </summary>
		/// <param name="control">GUI control.</param>
		public void Add(GUIControl control)
		{
			if (control == null)
				throw new ArgumentNullException("control");
			if (Contains(control.Name))
				throw new DuplicateObjectException(control.Name);

			control.Parent = _owner;
			_items.Add(control.Name, control);			
		}

		/// <summary>
		/// Function to clear the list.
		/// </summary>
		public void Clear()
		{
			foreach (GUIControl control in this)
				control.Dispose();

			ClearItems();
		}

		/// <summary>
		/// Function to remove a control by index.
		/// </summary>
		/// <param name="index">Index of the control to remove.</param>
		public void Remove(int index)
		{
			this[index].Dispose();
			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a control by name.
		/// </summary>
		/// <param name="controlName">Name of the control to remove.</param>
		public void Remove(string controlName)
		{
			this[controlName].Dispose();
			RemoveItem(controlName);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owner of this control list.</param>
		internal GUIControlList(GUIControl owner)
			: base(16)
		{
			_owner = owner;
		}
		#endregion
	}
}
