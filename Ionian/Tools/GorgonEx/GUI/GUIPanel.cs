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
// Created: Tuesday, May 27, 2008 1:55:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Object representing a panel that can hold GUI objects.
	/// </summary>
	public class GUIPanel
		: GUIObject, IGUIContainer
	{
		#region Variables.
		private GUIObjectCollection _guiObjects = null;				// List of objects contained in the panel.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the background color of the panel.
		/// </summary>
		public Drawing.Color BackgroundColor
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		/// <remarks>The frame delta is typically used with animations to help achieve a smooth appearance regardless of processor speed.</remarks>
		internal override void Update(float frameTime)
		{
			// Do nothing.
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		internal override void Draw()
		{
			if (Visible)
			{
				Gorgon.CurrentRenderTarget.BeginDrawing();
				Gorgon.CurrentRenderTarget.FilledRectangle(WindowDimensions.X, WindowDimensions.Y, WindowDimensions.Width, WindowDimensions.Height, BackgroundColor);
				Gorgon.CurrentRenderTarget.EndDrawing();
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIPanel"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <remarks>
		/// Since the name property is read-only, the name for an object must be passed to this constructor.
		/// <para>Typically it will be difficult to rename an object that resides in a collection with its name as the key, thus making the property writable would be counter-productive.</para>
		/// 	<para>However, objects that descend from this object may choose to implement their own renaming scheme if they so choose.</para>
		/// 	<para>Ensure that the name parameter has a non-null string or is not a zero-length string.  Failing to do so will raise a <see cref="System.ArgumentNullException">ArgumentNullException</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public GUIPanel(string name)
			: base(name)
		{
			_guiObjects = new GUIObjectCollection();
			Visible = true;
			BackgroundColor = Drawing.Color.Transparent;
		}
		#endregion

		#region IGUIContainer Members
		/// <summary>
		/// Property to return the list of objects contained within this container.
		/// </summary>
		/// <value></value>
		public GUIObjectCollection GUIObjects
		{
			get 
			{
				return _guiObjects;
			}
		}
		#endregion
	}
}
