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
// Created: Tuesday, May 27, 2008 5:25:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Object representing a collection of panels.
	/// </summary>
	public class GUIWindowCollection
		: BaseCollection<GUIWindow>
	{
		#region Variables.
		private Desktop _desktop = null;				// Desktop that owns this collection.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a panel by its index.
		/// </summary>
		public GUIWindow this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a panel by its name.
		/// </summary>
		public GUIWindow this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to add a panel to the collection.
		/// </summary>
		/// <param name="panel">Panel to add.</param>
		public void Add(GUIWindow panel)
		{
			if (panel == null)
				throw new ArgumentNullException("panel");

			AddItem(panel.Name, panel);
			panel.SetDesktop(_desktop);
						
			foreach (GUIWindow panelItem in this)
				panelItem.ZOrder = int.MinValue;

			_desktop.BringToFront(panel);
		}

		/// <summary>
		/// Function to remove a GUI panel from the collection.
		/// </summary>
		/// <param name="panel">GUI panel to remove.</param>
		internal void Remove(GUIWindow panel)
		{
			if (panel == null)
				throw new ArgumentNullException("panel");
			RemoveItem(panel.Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIWindowCollection"/> class.
		/// </summary>
		internal GUIWindowCollection(Desktop desktop)
			: base(128, true)
		{
			if (desktop == null)
				throw new ArgumentNullException("desktop");
			_desktop = desktop;
		}
		#endregion
	}
}
