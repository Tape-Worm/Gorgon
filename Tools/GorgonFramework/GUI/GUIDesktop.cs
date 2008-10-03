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
// Created: Tuesday, May 27, 2008 12:27:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Root object for the GUI system.
	/// </summary>
	public class Desktop
		: IDisposable
	{
		#region Value types.
		/// <summary>
		/// Value type to record the state of the input system before use by the GUI.
		/// </summary>
		private struct InputState
		{
			/// <summary>
			/// Indicates whether the cursor is visible or not.
			/// </summary>
			public bool CursorVisible;
			/// <summary>
			/// Indicates whether the mouse was enabled or not.
			/// </summary>
			public bool MouseEnabled;
			/// <summary>
			/// Indicates whether the keyboard was enabled or not.
			/// </summary>
			public bool KeyboardEnabled;
			/// <summary>
			/// The valid range of the mouse position.
			/// </summary>
			public Drawing.RectangleF MouseRange;
			/// <summary>
			/// The valid range for the mouse wheel.
			/// </summary>
			public Drawing.Point MouseWheelRange;
		}
		#endregion

		#region Variables.
		private bool _disposed = false;				// Flag to indicate that the object is disposed.
		private Input _input = null;				// Input system to use.
		private InputState _state;					// Input state data.
		private GUIWindowCollection _windows;		// List of GUI windows.
		private GUIWindow _focused = null;			// Currently focused object.
		private GUISkin _skin = null;				// Skin for the objects.
		private static Font _defaultFont = null;	// Default GUI font.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default font.
		/// </summary>
		public static Font DefaultFont
		{
			get
			{
				return _defaultFont;
			}
			set
			{
				if (_defaultFont != null)
					_defaultFont.Dispose();

				if (value == null)
					_defaultFont = new Font("DefaultGUIFont", "Arial", 9.0f, true, false, false, false);
				else
					_defaultFont = value;
			}
		}

		/// <summary>
		/// Property to set or return the color of the focus rectangle.
		/// </summary>
		public Drawing.Color FocusRectangleColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the focus rectangle blend mode.
		/// </summary>
		public BlendingModes FocusRectangleBlend
		{
			get;
			set;
		}
		
		/// <summary>
		/// Property to set or return whether the focus rectangle is an outline or solid rectangle.
		/// </summary>
		public bool FocusRectangleOutline
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the skin for the objects.
		/// </summary>
		public GUISkin Skin
		{
			get
			{
				return _skin;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				_skin = value;
			}
		}

		/// <summary>
		/// Property to return the list of GUI windows.
		/// </summary>
		public GUIWindowCollection Windows
		{
			get
			{
				return _windows;
			}
		}

		/// <summary>
		/// Property to return the current object with focus.
		/// </summary>
		public GUIWindow Focused
		{
			get
			{
				return _focused;
			}
		}

		/// <summary>
		/// Property to return the mouse position.
		/// </summary>
		public Vector2D MousePosition
		{
			get
			{
				if ((_input == null) || (_input.Mouse == null))
					return Vector2D.Zero;
				return _input.Mouse.Position;
			}
		}

		/// <summary>
		/// Property to set or return the sprite to use as the desktop background image.
		/// </summary>
		public Sprite DesktopBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the show the desktop background.
		/// </summary>
		public bool ShowDesktopBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the desktop background color.
		/// </summary>
		public Drawing.Color BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the primary mouse cursor.
		/// </summary>
		public Sprite Cursor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the mouse cursor is visible or not.
		/// </summary>
		public bool CursorVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the input system for the GUI.
		/// </summary>
		public Input Input
		{
			get
			{
				return _input;
			}
			set
			{
				UnbindEvents();
				_input = value;
				BindEvents();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to bind the input events from the GUI.
		/// </summary>
		private void BindEvents()
		{
			if (_input == null)
				return;

			if (Input.Window == null)
			{
				if ((Gorgon.Screen != null) && (Gorgon.Screen.Owner != null))
					Input.Bind(Gorgon.Screen.Owner);
				else
					throw new GorgonException(GorgonErrors.CannotBindInputDevice, "Cannot bind the input interface to a window handle.");
			}
			if (Input.Mouse == null)
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "The mouse was not bound to the input interface.  A mouse is required for the GUI system.");
			if (Input.Keyboard == null)
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "The keyboard was not bound to the input interface.  A keyboard is required for the GUI system.");
						
			_state.CursorVisible = Input.Mouse.CursorVisible;
			_state.MouseEnabled = Input.Mouse.Enabled;
			_state.MouseRange = Input.Mouse.PositionRange;
			_state.MouseWheelRange = Input.Mouse.WheelRange;
			_state.KeyboardEnabled = Input.Keyboard.Enabled;

			Input.Mouse.MouseDown += new MouseInputEvent(Mouse_MouseDown);
			Input.Mouse.MouseMove += new MouseInputEvent(Mouse_MouseMove);
			Input.Mouse.MouseUp += new MouseInputEvent(Mouse_MouseUp);
			Input.Mouse.MouseWheelMove += new MouseInputEvent(Mouse_MouseWheelMove);

			Input.Keyboard.KeyDown += new KeyboardInputEvent(Keyboard_KeyDown);
			Input.Keyboard.KeyUp += new KeyboardInputEvent(Keyboard_KeyUp);

			Input.Window.MouseEnter += new EventHandler(Window_Enter);
			Input.Window.MouseLeave += new EventHandler(Window_Leave);

			Input.Window.SizeChanged += new EventHandler(Window_SizeChanged);

			if (!Input.Mouse.Enabled)
				Input.Mouse.Enabled = true;
			if (!Input.Keyboard.Enabled)
				Input.Keyboard.Enabled = true;
			if (Input.Mouse.CursorVisible)
				Input.Mouse.CursorVisible = false;

			Input.Mouse.PositionRange = Input.Window.ClientRectangle;
			Input.Mouse.WheelRange = Drawing.Point.Empty;
		}

		/// <summary>
		/// Handles the SizeChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_SizeChanged(object sender, EventArgs e)
		{
			Input.Mouse.PositionRange = new Drawing.RectangleF(0, 0, Gorgon.CurrentRenderTarget.Width, Gorgon.CurrentRenderTarget.Height);
		}

		/// <summary>
		/// Function to unbind the input events from the GUI.
		/// </summary>
		private void UnbindEvents()
		{
			if (_input == null)
				return;

			Input.Mouse.CursorVisible = _state.CursorVisible;
			Input.Mouse.Enabled = _state.MouseEnabled;
			Input.Mouse.PositionRange = _state.MouseRange;
			Input.Mouse.WheelRange = _state.MouseWheelRange;

			Input.Keyboard.Enabled = _state.KeyboardEnabled;

			Input.Mouse.MouseDown -= new MouseInputEvent(Mouse_MouseDown);
			Input.Mouse.MouseMove -= new MouseInputEvent(Mouse_MouseMove);
			Input.Mouse.MouseUp -= new MouseInputEvent(Mouse_MouseUp);
			Input.Mouse.MouseWheelMove -= new MouseInputEvent(Mouse_MouseWheelMove);

			Input.Keyboard.KeyDown -= new KeyboardInputEvent(Keyboard_KeyDown);
			Input.Keyboard.KeyUp -= new KeyboardInputEvent(Keyboard_KeyUp);

			Input.Window.Enter -= new EventHandler(Window_Enter);
			Input.Window.Leave -= new EventHandler(Window_Leave);
			Input.Window.SizeChanged -= new EventHandler(Window_SizeChanged);
		}

		/// <summary>
		/// Handles the Leave event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Leave(object sender, EventArgs e)
		{
			if (!Input.Mouse.CursorVisible)
				Input.Mouse.CursorVisible = true;
		}

		/// <summary>
		/// Handles the Enter event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Enter(object sender, EventArgs e)
		{
			if (Input.Mouse.CursorVisible)
				Input.Mouse.CursorVisible = false;
		}

		/// <summary>
		/// Function to send the mouse event to the focused window.
		/// </summary>
		/// <param name="eventType">Type of event to send.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void SendMouseEvent(MouseEventType eventType, MouseInputEventArgs e)
		{
			if ((_focused != null) && (_focused.WindowDimensions.Contains((Drawing.Point)e.Position)))
				_focused.MouseEvent(eventType, e);
		}

		/// <summary>
		/// Handles the KeyUp event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void Keyboard_KeyUp(object sender, KeyboardInputEventArgs e)
		{
			if (_focused != null)
				_focused.KeyboardEvent(false, e);
		}

		/// <summary>
		/// Handles the KeyDown event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void Keyboard_KeyDown(object sender, KeyboardInputEventArgs e)
		{
			if (_focused != null)
				_focused.KeyboardEvent(true, e);
		}

		/// <summary>
		/// Handles the MouseWheelMove event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseWheelMove(object sender, MouseInputEventArgs e)
		{
			SendMouseEvent(MouseEventType.MouseWheelMove, e);
		}

		/// <summary>
		/// Handles the MouseUp event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseUp(object sender, MouseInputEventArgs e)
		{
			SendMouseEvent(MouseEventType.MouseButtonUp, e);
		}

		/// <summary>
		/// Handles the MouseMove event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseMove(object sender, MouseInputEventArgs e)
		{
			SendMouseEvent(MouseEventType.MouseMoved, e);
		}

		/// <summary>
		/// Function to determine if the mouse is over a GUI object.
		/// </summary>
		/// <param name="guiObject">Object to test.</param>
		/// <param name="position">Mouse position.</param>
		/// <returns>TRUE if the object is under the mouse, FALSE if it is not.</returns>
		private bool MouseOverObject(GUIObject guiObject, Drawing.Point position)
		{
			Drawing.Point clientPosition;		// Client position of the cursor.

			if (guiObject == null)
				throw new ArgumentNullException("guiObject");

			clientPosition = guiObject.ScreenToPoint(position);

			return guiObject.ClientArea.Contains(clientPosition);
		}

		/// <summary>
		/// Handles the MouseDown event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseDown(object sender, MouseInputEventArgs e)
		{
			GUIWindow currentObject = HitTest((Drawing.Point)e.Position);
			if (currentObject != null)
			{
				if (_focused != currentObject)
					_focused = currentObject;
								
				if (_focused.ZOrder != 0)
					BringToFront(_focused);				
			}

			SendMouseEvent(MouseEventType.MouseButtonDown, e);
		}

		/// <summary>
		/// Function to retrieve the control in the focused window under the cursor.
		/// </summary>
		/// <returns>The control under the cursor, or NULL if no control under the cursor.</returns>
		public GUIObject ControlUnderCursor()
		{
			if (_focused == null)
				return null;
			return ControlUnderCursor(_focused);
		}

		/// <summary>
		/// Function to retrieve the control in the focused window under the cursor.
		/// </summary>
		/// <param name="container">Container to search.</param>
		/// <returns>The control under the cursor, or NULL if no control under the cursor.</returns>
		public GUIObject ControlUnderCursor(IGUIContainer container)
		{
			GUIObject result = null;

			if (container == null)
				return null;

			var controls = from control in container.GUIObjects
						   where control.Visible && control.Enabled && control.Owner.RectToScreen(control.WindowDimensions).Contains((Drawing.Point)MousePosition)
						   orderby control.ZOrder
						   select control;

			foreach(var control in controls)
			{
				IGUIContainer internalContainer = control as IGUIContainer;

				if (internalContainer != null)
				{
					result = ControlUnderCursor(internalContainer);

					if (result == null)
						result = control;
				}
				else
					result = control;
			}

			return result;
		}

		/// <summary>
		/// Function to return the object under the set of coordinates passed.
		/// </summary>
		/// <param name="location">Coordinates to test.</param>
		/// <returns>The object under the coordinates.</returns>
		/// <remarks>This will only return the object at the top of the Z-Order, if there are overlapping objects then the first in the z-order will be returned.</remarks>
		public GUIWindow HitTest(Drawing.Point location)
		{
			return (from guiWindow in _windows
				   where ((guiWindow.WindowDimensions.Contains(location)) && (guiWindow.Visible) && (guiWindow.Enabled))
				   orderby guiWindow.ZOrder
				   select guiWindow).FirstOrDefault();
		}

		/// <summary>
		/// Function to bring an object to the foreground.
		/// </summary>
		/// <param name="focusedObject">Object to bring to the front.</param>
		public void BringToFront(GUIWindow focusedObject)
		{
			int zCount = 1;			// ZOrder counter.

			if (_focused != focusedObject)
				_focused = focusedObject;

			if (_focused.ZOrder != 0)
			{
				_focused.ZOrder = 0;
				var objects = from guiWindow in _windows
							  where _focused != guiWindow
							  select guiWindow;

				foreach (GUIWindow guiObject in objects)
				{
					guiObject.ZOrder = zCount;
					zCount++;
				}
			}
		}
		
		/// <summary>
		/// Function to update the GUI system.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		public void Update(float frameTime)
		{
			var objects = from guiWindow in _windows
						  where guiWindow.Enabled && guiWindow.Visible
						  select guiWindow;

			foreach (GUIWindow window in objects)
				window.Update(frameTime);

			_skin.Update(frameTime);
		}

		/// <summary>
		/// Function to draw the GUI.
		/// </summary>
		public void Draw()
		{
			var objects = from guiWindow in _windows
						  where guiWindow.Enabled && guiWindow.Visible
						  orderby guiWindow.ZOrder descending
						  select guiWindow;

			if (Input == null)
				throw new GorgonException(GorgonErrors.NotInitialized, "No input system was bound to the GUI.");

			if (ShowDesktopBackground)
			{
				if (DesktopBackground != null)
				{
					if (DesktopBackground.Color != BackgroundColor)
						DesktopBackground.Color = BackgroundColor;
					if (DesktopBackground.Axis != Vector2D.Zero)
						DesktopBackground.Axis = Vector2D.Zero;
					if (DesktopBackground.Position != Vector2D.Zero)
						DesktopBackground.Position = Vector2D.Zero;
					if (DesktopBackground.ScaledWidth != Gorgon.CurrentRenderTarget.Width)
						DesktopBackground.ScaledWidth = Gorgon.CurrentRenderTarget.Width;
					if (DesktopBackground.ScaledHeight != Gorgon.CurrentRenderTarget.Height)
						DesktopBackground.ScaledHeight = Gorgon.CurrentRenderTarget.Height;
					if (DesktopBackground.Rotation != 0.0f)
						DesktopBackground.Rotation = 0.0f;

					DesktopBackground.Draw();
				}
				else
				{
					if (BackgroundColor.A != 0)
					{
						Gorgon.CurrentRenderTarget.BeginDrawing();
						Gorgon.CurrentRenderTarget.FilledRectangle(0, 0, Gorgon.CurrentRenderTarget.Width, Gorgon.CurrentRenderTarget.Height, BackgroundColor);
						Gorgon.CurrentRenderTarget.EndDrawing();
					}
					else
						Gorgon.CurrentRenderTarget.Clear(BackgroundColor);
				}
			}

			foreach (GUIWindow panel in objects)
				panel.Draw();

			if (Cursor != null)
			{
				Cursor.Position = Input.Mouse.Position;
				if ((CursorVisible) && (!Input.Mouse.CursorVisible))
					Cursor.Draw();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Desktop"/> class.
		/// </summary>
		/// <param name="input">Input interface to use with the GUI system.</param>
		/// <param name="skin">Skin to use when drawing.</param>
		public Desktop(Input input, GUISkin skin)
		{
			if (skin == null)
				throw new ArgumentNullException("skin");
			if (input == null)
				throw new ArgumentNullException("input");

			Skin = skin;
			_windows = new GUIWindowCollection(this);
			_state = new InputState();
			Input = input;
			CursorVisible = true;			
			BackgroundColor = Drawing.Color.White;
			FocusRectangleBlend = BlendingModes.None;
			FocusRectangleColor = Drawing.Color.FromArgb(0, 0, 64);
			FocusRectangleOutline = true;
			if (!skin.Elements.Contains("Cursor.Default"))
				Cursor = new Sprite("DefaultCursor", Image.FromResource("DefaultCursorImage", GorgonLibrary.Framework.Properties.Resources.ResourceManager));
			else
				Cursor = _skin.Elements["Cursor.Default"].GetSprite();
			_defaultFont = new Font("DefaultGUIFont", "Arial", 9.0f, true, false, false, false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					UnbindEvents();

					foreach (GUIWindow window in _windows)
						window.Dispose();

					_windows.Clear();

					if (_defaultFont != null)
						_defaultFont.Dispose();
					if (_skin != null)
						_skin.Dispose();
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
