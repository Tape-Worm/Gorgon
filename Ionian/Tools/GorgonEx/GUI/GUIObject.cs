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
// Created: Tuesday, May 27, 2008 1:56:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GorgonLibrary.Internal;
using GorgonLibrary.InputDevices;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Abstract object for GUI components.
	/// </summary>
	public abstract class GUIObject
		: NamedObject, IDisposable
	{
		#region Events.
		/// <summary>
		/// Event fired when the mouse is moved.
		/// </summary>
		public event MouseInputEvent MouseMove;
		/// <summary>
		/// Event fired when a mouse button is pressed.
		/// </summary>
		public event MouseInputEvent MouseDown;
		/// <summary>
		/// Event fired when a mouse button is released.
		/// </summary>
		public event MouseInputEvent MouseUp;
		/// <summary>
		/// Event fired when the mouse wheel is moved.
		/// </summary>
		public event MouseInputEvent MouseWheelMove;
		/// <summary>
		/// Event fired when a key is pressed.
		/// </summary>
		public event KeyboardInputEvent KeyDown;
		/// <summary>
		/// Event fired when a key is released.
		/// </summary>
		public event KeyboardInputEvent KeyUp;
		#endregion

		#region Variables.
		private Rectangle _clientArea = Rectangle.Empty;			// Client area.
		private Rectangle _windowArea = Rectangle.Empty;			// Window area.
		private Viewport _clippedView = null;						// Clipping view.
		private Viewport _previousView = null;						// Previous clipping view.
		private GUIObject _owner = null;							// Object that will own this object.
		private Desktop _desktop = null;							// Desktop that owns this object.
		private bool _disposed = false;								// Flag to indicate that the object is disposed.
		private bool _enabled = true;								// Flag to incidate that the object is enabled.
		private bool _visible = false;								// Flag to indicate that the object is visible.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the desktop that this object belongs to.
		/// </summary>
		public Desktop Desktop
		{
			get
			{
				return _desktop;
			}
		}

		/// <summary>
		/// Property to set or return the Z-Order for this object.
		/// </summary>
		public int ZOrder
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the owner object for this object.
		/// </summary>
		public virtual GUIObject Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				IGUIContainer container = value as IGUIContainer;				// The destination container.
				IGUIContainer currentContainer = _owner as IGUIContainer;		// Current container.

				if (currentContainer != null)
					currentContainer.GUIObjects.Remove(this);

				if (value != null)
				{
					if (container == null)
						throw new ArgumentException("The GUI object '" + value.Name + "' is not a container.");
					else
					{
						if (Desktop == null)
							SetDesktop(value.Desktop);

						if (value.Desktop != Desktop)
							throw new ArgumentException("The GUI object '" + value.Name + "' does not belong to the same desktop.");
						else
							container.GUIObjects.Add(this);
					}
				}

				_owner = value;
			}
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		public Rectangle WindowDimensions
		{
			get
			{
				return _windowArea;
			}
			set
			{
				_windowArea = value;
				SetClientArea(_windowArea);
			}				
		}

		/// <summary>
		/// Property to return the client area of the object.
		/// </summary>
		public Rectangle ClientArea
		{
			get
			{
				return _clientArea;
			}
		}

		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		public Point Position
		{
			get
			{
				return _windowArea.Location;
			}
			set
			{
				_windowArea.Location = value;
			}
		}

		/// <summary>
		/// Property to set or return the size of the object.
		/// </summary>
		public Size Size
		{
			get
			{
				return _windowArea.Size;
			}
			set
			{
				_windowArea.Size = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the GUI object is enabled or not.
		/// </summary>
		public virtual bool Enabled
		{
			get
			{
				if ((Owner != null) && (!Owner.Enabled))
					return false;

				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the GUI object is visible or not.
		/// </summary>
		public virtual bool Visible
		{
			get
			{
				if ((Owner != null) && (!Owner.Visible))
					return false;

				return _visible;
			}
			set
			{
				_visible = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the active clipping region.
		/// </summary>
		/// <param name="clippingArea">Rectangle defining the clipping area.</param>
		protected void SetClippingRegion(Rectangle clippingArea)
		{
			_previousView = Gorgon.CurrentClippingViewport;
			_clippedView.Left = clippingArea.Left;
			_clippedView.Top = clippingArea.Top;
			_clippedView.Width = clippingArea.Width;
			_clippedView.Height = clippingArea.Height;

			Gorgon.CurrentClippingViewport = _clippedView;
		}

		/// <summary>
		/// Function to restore the previous clipping region.
		/// </summary>
		protected void ResetClippingRegion()
		{
			Gorgon.CurrentClippingViewport = _previousView;
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnMouseMove(object sender, MouseInputEventArgs e)
		{		
			if (MouseMove != null)
				MouseMove(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button is pressed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnMouseDown(object sender, MouseInputEventArgs e)
		{
			if (MouseDown != null)
				MouseDown(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button is released.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnMouseUp(object sender, MouseInputEventArgs e)
		{
			if (MouseUp != null)
				MouseUp(sender, e);
		}

		/// <summary>
		/// Function called when the mouse wheel is moved.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnMouseWheelMove(object sender, MouseInputEventArgs e)
		{
			if (MouseWheelMove != null)
				MouseWheelMove(sender, e);
		}

		/// <summary>
		/// Function called when a key is pressed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnKeyDown(object sender, KeyboardInputEventArgs e)
		{
			if (KeyDown != null)
				KeyDown(sender, e);
		}

		/// <summary>
		/// Function called when a key is released.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnKeyUp(object sender, KeyboardInputEventArgs e)
		{
			if (KeyUp != null)
				KeyUp(sender, e);
		}

		/// <summary>
		/// Function to return the screen point in the coordinates of the owner object.
		/// </summary>
		/// <param name="screenPoint">Screen point to convert.</param>
		/// <returns>The point in owner space.</returns>
		protected Point GetOwnerPoint(Point screenPoint)
		{
			if (Owner != null)
				return Owner.ScreenToPoint(screenPoint);
			return screenPoint;
		}

		/// <summary>
		/// Function called for default mouse events.
		/// </summary>
		/// <param name="eventType">Mouse event type.</param>
		/// <param name="e">Event parameters.</param>
		protected void DefaultMouseEvent(MouseEventType eventType, MouseInputEventArgs e)
		{
			MouseInputEventArgs args = null;		// Arguments.

			args = new MouseInputEventArgs(e.Buttons, e.ShiftButtons, ScreenToPoint((Point)e.Position), e.WheelPosition,
															 e.RelativePosition, e.WheelDelta, e.ClickCount);

			// If the mouse event is in the client area, then pass it on.
			if (this.ClientArea.Contains((Point)args.Position))
			{
				switch (eventType)
				{
					case MouseEventType.MouseMoved:
						OnMouseMove(this, args);
						break;
					case MouseEventType.MouseButtonDown:
						OnMouseDown(this, args);
						break;
					case MouseEventType.MouseButtonUp:
						OnMouseUp(this, args);
						break;
					case MouseEventType.MouseWheelMove:
						OnMouseWheelMove(this, args);
						break;
				}
			}
		}

		/// <summary>
		/// Function called for default keyboard events.
		/// </summary>
		/// <param name="keyDown">TRUE if the key is held, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected void DefaultKeyboardEvent(bool keyDown, KeyboardInputEventArgs e)
		{
			// TODO: In here we will check to see if a control within the window is focused.
			// Then we can process the key and/or send it on to the control.
			if (keyDown)
				OnKeyDown(this, e);
			else
				OnKeyUp(this, e);
		}
		
		/// <summary>
		/// Function to set the client area for the object.
		/// </summary>
		/// <param name="windowArea">Full area of the window.</param>
		protected virtual void SetClientArea(Rectangle windowArea)
		{
			_clientArea = new Rectangle(0, 0, windowArea.Width, windowArea.Height);
		}

		/// <summary>
		/// Function called when a mouse event has taken place in this window.
		/// </summary>
		/// <param name="eventType">Type of event that should be fired.</param>
		/// <param name="e">Event parameters.</param>
		protected abstract internal void MouseEvent(MouseEventType eventType, MouseInputEventArgs e);

		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected abstract internal void KeyboardEvent(bool isDown, KeyboardInputEventArgs e);

		/// <summary>
		/// Function to convert a client rectangle to screen coordinates.
		/// </summary>
		/// <param name="clientRect">Client rectangle to convert.</param>
		/// <returns>The rectangle in screen coordinates.</returns>
		public Rectangle RectToScreen(Rectangle clientRect)
		{
			return new Rectangle(PointToScreen(clientRect.Location), clientRect.Size);
		}

		/// <summary>
		/// Function to convert a client point to screen coordinates.
		/// </summary>
		/// <param name="clientPoint">Client point to convert.</param>
		/// <returns>The screen coordinates of the point.</returns>
		public virtual Point PointToScreen(Point clientPoint)
		{
			Point currentPosition = clientPoint;		// Client area.

			if (Owner != null)
				currentPosition = Owner.PointToScreen(currentPosition);

			currentPosition.X += Position.X;
			currentPosition.Y += Position.Y;

			return currentPosition;
		}

		/// <summary>
		/// Function to convert a screen point to client coordinates.
		/// </summary>
		/// <param name="screenPoint">Screen point to convert.</param>
		/// <returns>The client coordinates of the point.</returns>
		public virtual Point ScreenToPoint(Point screenPoint)
		{
			Point currentPosition = screenPoint;		// Client area.

			if (Owner != null)
				currentPosition = Owner.ScreenToPoint(currentPosition);

			currentPosition.X -= Position.X;
			currentPosition.Y -= Position.Y;

			return currentPosition;
		}

		/// <summary>
		/// Function to set the desktop for this object.
		/// </summary>
		/// <param name="desktop">Desktop to set.</param>
		internal void SetDesktop(Desktop desktop)
		{
			IGUIContainer container = this as IGUIContainer;	// Our child controls.

			_desktop = desktop;
			SetClientArea(WindowDimensions);
			if (container != null)
			{
				// Assign all children to the same desktop.
				foreach (GUIObject guiObject in container.GUIObjects)
					guiObject.SetDesktop(_desktop);
			}
		}

		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		/// <remarks>The frame delta is typically used with animations to help achieve a smooth appearance regardless of processor speed.</remarks>
		internal abstract void Update(float frameTime);
		/// <summary>
		/// Function to draw the object.
		/// </summary>
		internal abstract void Draw();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIObject"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <remarks>
		/// Since the name property is read-only, the name for an object must be passed to this constructor.
		/// <para>Typically it will be difficult to rename an object that resides in a collection with its name as the key, thus making the property writable would be counter-productive.</para>
		/// 	<para>However, objects that descend from this object may choose to implement their own renaming scheme if they so choose.</para>
		/// 	<para>Ensure that the name parameter has a non-null string or is not a zero-length string.  Failing to do so will raise a <see cref="System.ArgumentNullException">ArgumentNullException</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		protected GUIObject(string name)
			: base(name)
		{
			ZOrder = int.MinValue;
			_clippedView = new Viewport(1, 1, 1, 1);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Destroy all child objects.
					IGUIContainer container = this as IGUIContainer;
					IGUIContainer ownerContainer = Owner as IGUIContainer;

					if (container != null)
					{
						while (container.GUIObjects.Count > 0)
							container.GUIObjects[0].Dispose();
					}

					if (ownerContainer != null)
						ownerContainer.GUIObjects.Remove(this);
				}
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
