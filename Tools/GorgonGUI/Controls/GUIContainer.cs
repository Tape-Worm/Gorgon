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
// Created: Sunday, February 25, 2007 3:02:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using GorgonLibrary.Input;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Abstract object representing a container control.
	/// </summary>
	public abstract class GUIContainer
		: GUIControl
	{
		#region Variables.
		private GUIControlList _controls;		// List of GUI controls.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the control list interface.
		/// </summary>
		public GUIControlList Controls
		{
			get
			{
				return _controls;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to test and see if any of the controls in the container are under the mouse cursor.
		/// </summary>
		/// <param name="e">Mouse event arguments.</param>
		/// <returns>The GUI control that will receive events.</returns>
		protected GUIControl HitTest(MouseInputEventArgs e)
		{
			// If we're not focused, or visible, then leave.
			if ((!Focused) || (!Visible))
				return null;

			// Test the focus window first.
			// TODO: implement focus for controls.
/*			if ((_focusWindow != null) && (_focusWindow.Bounds.Contains(e.X, e.Y)))
				return _focusWindow;*/

			// Go through each control and test its rectangle.
			foreach (GUIControl control in _controls)
			{
				control.MouseInView = false;

				if (control.Bounds.Contains((Drawing.Point)e.Position))
				{
					control.MouseInView = true;
					return control;
				}	
			}

			return null;
		}

		/// <summary>
		/// Function called when a mouse double click event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseDoubleClick(object sender, MouseInputEventArgs e)
		{
			GUIControl control = null;		// Control to receive the event.

			control = HitTest(e);
			if (control != null)
				control.OnMouseDoubleClick(this, e);

			base.OnMouseDoubleClick(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button down event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseDown(object sender, MouseInputEventArgs e)
		{
			GUIControl control = null;		// Control to receive the event.

			control = HitTest(e);
			if (control != null)
				control.OnMouseDown(this, e);

			base.OnMouseDown(sender, e);
		}

		/// <summary>
		/// Function called when a mouse move event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseMove(object sender, MouseInputEventArgs e)
		{
			GUIControl control = null;		// Control to receive the event.

			control = HitTest(e);
			if (control != null)
				control.OnMouseMove(this, e);

			base.OnMouseMove(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button up event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseUp(object sender, MouseInputEventArgs e)
		{
			GUIControl control = null;		// Control to receive the event.

			control = HitTest(e);
			if (control != null)
				control.OnMouseUp(this, e);

			base.OnMouseUp(sender, e);
		}

		/// <summary>
		/// Function called when a mouse wheel event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseWheel(object sender, MouseInputEventArgs e)
		{
			GUIControl control = null;		// Control to receive the event.

			control = HitTest(e);
			if (control != null)
				control.OnMouseWheel(this, e);

			base.OnMouseWheel(sender, e);
		}

		/// <summary>
		/// Function to draw the control.
		/// </summary>
		internal override void DrawControl()
		{
			for (int i = 0; i < _controls.Count; i++)
				_controls[i].DrawControl();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="gui">GUI that owns the control.</param>
		/// <param name="name">Name of the control.</param>
		/// <param name="position">Position of the control.</param>
		/// <param name="size">Size of the control.</param>
		protected GUIContainer(GUI gui, string name, Vector2D position, Vector2D size)
			: base(gui, name, position, size)
		{
			_controls = new GUIControlList(this);
		}
		#endregion		
	}
}
