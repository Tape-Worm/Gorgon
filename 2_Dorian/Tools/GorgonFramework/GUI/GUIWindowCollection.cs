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
