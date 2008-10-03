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
// Created: Tuesday, May 27, 2008 1:55:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Object representing a panel that can hold GUI objects.
	/// </summary>
	public class GUIPanel
		: GUIObject, IGUIContainer
	{
		#region Variables.
		private GUIObjectCollection _guiObjects = null;				// List of objects contained in the panel.		
		private PanelBorderStyle _border = PanelBorderStyle.None;	// Border style.
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

		/// <summary>
		/// Property to set or return the border style of the panel.
		/// </summary>
		public PanelBorderStyle Border
		{
			get
			{
				return _border;
			}
			set
			{
				_border = value;
				SetClientArea(WindowDimensions);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void KeyboardEvent(bool isDown, GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
		{
			DefaultKeyboardEvent(isDown, e);
		}

		/// <summary>
		/// Function called when a mouse event has taken place in this window.
		/// </summary>
		/// <param name="eventType">Type of event that should be fired.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void MouseEvent(MouseEventType eventType, GorgonLibrary.InputDevices.MouseInputEventArgs e)
		{
			var children = from guiObject in GUIObjects
						   where guiObject.Visible
						   orderby guiObject.ZOrder
						   select guiObject;

			// Forward any events on to our child objects and terminate event handling if the child has received the event.
			foreach (GUIObject child in children)
			{
				if (child.WindowDimensions.Contains(ScreenToPoint((Drawing.Point)e.Position)))
				{
					GUIWindow parent = GetOwnerWindow();

					if ((parent != null) && (parent.FocusedControl != child) && (eventType == MouseEventType.MouseButtonDown))
						child.Focus();
					child.MouseEvent(eventType, e);
					return;
				}
			}

			DefaultMouseEvent(eventType, e);
		}

		/// <summary>
		/// Function to draw the non-client area.
		/// </summary>
		protected virtual void DrawNonClientArea()
		{
			if (Border != PanelBorderStyle.Single)
				return;

			Drawing.Rectangle border = RectToScreen(ClientArea);		// Convert to screen coordinates.
			border = Drawing.Rectangle.Inflate(border, 1, 1);
			Gorgon.CurrentRenderTarget.Rectangle(border.X, border.Y, border.Width, border.Height, Drawing.Color.Black);
		}

		/// <summary>
		/// Function to set the client area for the object.
		/// </summary>
		/// <param name="windowArea">Full area of the window.</param>
		protected override void SetClientArea(System.Drawing.Rectangle windowArea)
		{
			if (Border == PanelBorderStyle.Single)
			{
				windowArea.Width -= 2;
				windowArea.Height -= 2;
			}

			base.SetClientArea(windowArea);			
		}

		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		/// <remarks>The frame delta is typically used with animations to help achieve a smooth appearance regardless of processor speed.</remarks>
		protected internal override void Update(float frameTime)
		{
			// Draw each child.
			var children = from guiObject in GUIObjects
						   where guiObject.Enabled
						   orderby guiObject.ZOrder
						   select guiObject;

			foreach (GUIObject guiObject in children)
				guiObject.Update(frameTime);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		protected internal override void Draw()
		{
			IGUIContainer container;			// Parent container.
			Drawing.Rectangle screenPoints;		// Screen coordinates.

			if (Visible)
			{
				container = Owner as IGUIContainer;
				screenPoints = RectToScreen(ClientArea);

				Gorgon.CurrentRenderTarget.BeginDrawing();
				if ((container != null) && (container.ClipChildren))
					SetClippingRegion(GetClippingArea());

				DrawNonClientArea();
				
				Gorgon.CurrentRenderTarget.FilledRectangle(screenPoints.X, screenPoints.Y, screenPoints.Width, screenPoints.Height, BackgroundColor);
				Gorgon.CurrentRenderTarget.EndDrawing();

				// Draw each child.
				var children = from guiObject in GUIObjects
								where guiObject.Visible 
								orderby guiObject.ZOrder descending
								select guiObject;

				foreach (GUIObject guiObject in children)
					guiObject.Draw();

				if ((container != null) && (container.ClipChildren))
					ResetClippingRegion();
			}
		}

		/// <summary>
		/// Function to convert a client point to screen coordinates.
		/// </summary>
		/// <param name="clientPoint">Client point to convert.</param>
		/// <returns>The screen coordinates of the point.</returns>
		public override System.Drawing.Point PointToScreen(System.Drawing.Point clientPoint)
		{
			Drawing.Point newPoint;		// New point.

			newPoint = base.PointToScreen(clientPoint);
			if (Border == PanelBorderStyle.Single)
			{
				newPoint.X += 1;
				newPoint.Y += 1;				
			}			

			return newPoint;
		}

		/// <summary>
		/// Function to convert a screen point to client coordinates.
		/// </summary>
		/// <param name="screenPoint">Screen point to convert.</param>
		/// <returns>The client coordinates of the point.</returns>
		public override System.Drawing.Point ScreenToPoint(System.Drawing.Point screenPoint)
		{
			if (Border == PanelBorderStyle.Single)
			{
				screenPoint.X -= 1;
				screenPoint.Y -= 1;
			}
			return base.ScreenToPoint(screenPoint);
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
			ClipChildren = true;
			BackgroundColor = Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);
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

		/// <summary>
		/// Property to set or return whether this control will clip its children.
		/// </summary>
		/// <value></value>
		public bool ClipChildren
		{
			get;
			set;
		}
		#endregion
	}
}
