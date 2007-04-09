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
// Created: Sunday, January 21, 2007 6:54:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using System.Windows.Forms;
using System.Resources;
using System.IO;
using SharpUtilities;
using SharpUtilities.Collections;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing the main GUI interface.
	/// </summary>
	public class GUI
		: IDisposable
	{
		#region Variables.
		private Control _gorgonOwner = null;				// Owner control for binding events with.
		private GUISkin _skin = null;						// GUI skin.
		private Sprite _background = null;					// Background sprite.
		private Vector2D _cursorPosition;					// Mouse cursor position.
		private bool _cursorVisible = true;					// Flag to indicate that the mouse cursor is visible.
		private GUIControlList _controls;					// GUI controls.
		private bool _previousCursorVisible;				// Previous cursor visibility state.
		private GUIWindow _focusWindow = null;				// Window with current focus.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the control that owns the current Gorgon instance.
		/// </summary>
		internal Control GorgonOwnerControl
		{
			get
			{
				return _gorgonOwner;
			}
		}

		/// <summary>
		/// Property to set or return the mouse cursor position.
		/// </summary>
		public Vector2D MousePosition
		{
			get
			{
				return _cursorPosition;
			}
			set
			{
				_cursorPosition = value;
			}
		}

		/// <summary>
		/// Property to set or return the background sprite.
		/// </summary>
		public Sprite Background
		{
			get
			{
				return _background;
			}
			set
			{
				_background = value;

				if ((value != null) && (value.Image != null))
					ScaleBackground();
			}
		}

		/// <summary>
		/// Property to return the GUI skin.
		/// </summary>
		public GUISkin Skin
		{
			get
			{
				return _skin;
			}
		}

		/// <summary>
		/// Property to set or return whether the mouse cursor is visible or not.
		/// </summary>
		public bool CursorVisible
		{
			get
			{
				return _cursorVisible;
			}
			set
			{
				_cursorVisible = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine which window has the mouse input.
		/// </summary>
		/// <param name="e">Mouse event arguments.</param>
		/// <returns>Window that received the mouse input.</returns>
		private GUIWindow HitTest(MouseEventArgs e)
		{
			// Test the focus window first.
			if (_focusWindow != null) 
			{
				if (_focusWindow.IsDragging)
					return _focusWindow;

				if (_focusWindow.Bounds.Contains(e.X, e.Y))
				{
					_focusWindow.MouseInView = true;
					return _focusWindow;
				}
				else
					_focusWindow.MouseInView = false;				
			}

			// Go through each control and test its rectangle.
			foreach (GUIWindow control in _controls)
			{
				if (control.Bounds.Contains(e.X, e.Y))
				{
					control.MouseInView = true;
					return control;
				}
				else
					control.MouseInView = false;
			}

			return null;
		}

		/// <summary>
		/// Function to build the mouse input event arguments.
		/// </summary>
		/// <param name="target">Window that is receiving the event.</param>
		/// <param name="e">Source mouse event arguments.</param>
		/// <returns>Gorgon mouse input event arguments.</returns>
		private MouseInputEventArgs BuildInputEvent(GUIWindow target, MouseEventArgs e)
		{
			MouseInputEventArgs eventArgs = null;	// Mouse input event arguments.
			MouseButton buttons;					// Mouse buttons.
			Vector2D position;						// Position of the mouse in client space.

			position = target.ConvertToClient((Vector2D)e.Location);

			buttons = MouseButton.None;

			// Get mouse buttons.
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
				buttons |= MouseButton.Left;
			if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
				buttons |= MouseButton.Right;
			if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle)
				buttons |= MouseButton.Left;
			if ((e.Button & MouseButtons.XButton1) == MouseButtons.XButton1)
				buttons |= MouseButton.Button4;
			if ((e.Button & MouseButtons.XButton2) == MouseButtons.XButton2)
				buttons |= MouseButton.Button5;

			eventArgs = new MouseInputEventArgs(buttons, MouseButton.None, position, 0, Vector2D.Zero, e.Delta, e.Clicks);

			return eventArgs;
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GUIWindow window = null;	// Window that received the event.

			window = HitTest(e);
			if (window != null)
				window.OnMouseDoubleClick(this, BuildInputEvent(window, e));
		}

		/// <summary>
		/// Handles the MouseWheel event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_MouseWheel(object sender, MouseEventArgs e)
		{
			GUIWindow window = null;	// Window that received the event.

			window = HitTest(e);
			if (window != null)
				window.OnMouseWheel(this, BuildInputEvent(window, e));
		}

		/// <summary>
		/// Handles the MouseUp event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_MouseUp(object sender, MouseEventArgs e)
		{
			GUIWindow window = null;	// Window that received the event.

			window = HitTest(e);
			if (window != null)
				window.OnMouseUp(this, BuildInputEvent(window, e));
		}

		/// <summary>
		/// Handles the MouseDown event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_MouseDown(object sender, MouseEventArgs e)
		{
			GUIWindow window = null;	// Window that received the event.

			window = HitTest(e);
			if (window != null)
				window.OnMouseDown(this, BuildInputEvent(window, e));
		}

		/// <summary>
		/// Handles the MouseMove event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_MouseMove(object sender, MouseEventArgs e)
		{
			GUIWindow window = null;	// Window that received the event.

			_cursorPosition.X = e.X;
			_cursorPosition.Y = e.Y;

			if (_cursorPosition.X < 0)
				_cursorPosition.X = 0;
			if (_cursorPosition.Y < 0)
				_cursorPosition.Y = 0;
			if (_cursorPosition.X >= Gorgon.Screen.Width)
				_cursorPosition.X = Gorgon.Screen.Width-1;
			if (_cursorPosition.Y >= Gorgon.Screen.Height)
				_cursorPosition.Y = Gorgon.Screen.Height - 1;			

			window = HitTest(e);
			if (window != null)
				window.OnMouseMove(this, BuildInputEvent(window, e));
		}

		/// <summary>
		/// Handles the KeyUp event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_KeyUp(object sender, KeyEventArgs e)
		{
		}

		/// <summary>
		/// Handles the KeyDown event of the _gorgonOwner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void _gorgonOwner_KeyDown(object sender, KeyEventArgs e)
		{
		}

		/// <summary>
		/// Function to scale the background image.
		/// </summary>
		private void ScaleBackground()
		{
			float newWidth = 0;				// New width.
			float newHeight = 0;			// New height.

			if ((_background == null) || (_background.Image != null))
				return;

			// Scale the image if necessary.
			if ((int)_background.Width != Gorgon.Screen.Width)
				newWidth = ((float)Gorgon.Screen.Width / _background.Width);
			else
				newWidth = Gorgon.Screen.Width;

			if ((int)_background.Height != Gorgon.Screen.Height)
				newHeight = ((float)Gorgon.Screen.Height / _background.Height);
			else
				newHeight = Gorgon.Screen.Height;

			_background.Smoothing = Smoothing.Smooth;
			_background.SetScale((int)newWidth, (int)newHeight);
			_background.SetPosition(0, 0);
		}

		/// <summary>
		/// Function to give a particular window focus.
		/// </summary>
		/// <param name="window">Window to give focus.</param>
		internal void GiveFocus(GUIWindow window)
		{
			if (_focusWindow != null)
				_focusWindow.LosingFocus();
			_focusWindow = window;
			_focusWindow.GainingFocus();
		}

		/// <summary>
		/// Function to return a GUI skin from a stream.
		/// </summary>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <returns>The sprite contained within the stream.</returns>
		private GUISkin FromStream(ResourceManager resources, Stream stream, bool isXML)
		{
			ISerializer skinSerializer = null;		// GUI Skin serializer.
			GUISkin newSkin = null;					// New GUI skin object.
			string skinPath = string.Empty;			// Skin path.

			try
			{
				// Get the filename if this is a file stream.
				if (stream is FileStream)
					skinPath = ((FileStream)stream).Name;
				else
					skinPath = "@MemoryResource.";

				// Create the sprite object.
				newSkin = new GUISkin(skinPath);

				// Open the file for reading.
				if (isXML)
					skinSerializer = new XMLSerializer(newSkin, stream);
				else
					skinSerializer = new XMLSerializer(newSkin, stream);

				if (resources != null)
					skinSerializer.Parameters.AddParameter<ResourceManager>("ResourceManager", resources);

				// Don't close the underlying stream.
				skinSerializer.DontCloseStream = true;

				skinSerializer.Deserialize();

				// If the name already exists, then return that sprite instead.
				if (_skin != null)
					_skin.Dispose();

				_skin = newSkin;
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(skinPath, typeof(GUISkin), ex);
			}
			finally
			{
				if (skinSerializer != null)
					skinSerializer.Dispose();
				skinSerializer = null;
			}

			return newSkin;
		}

		/// <summary>
		/// Function to load a GUI skin.
		/// </summary>
		/// <param name="skinPath">Filename/path to the skin.</param>
		/// <returns>The skin stored on the disk.</returns>
		public GUISkin SkinFromFile(string skinPath)
		{
			Stream skinStream = null;				// Stream for the skin.

			try
			{
				// Open the file for reading.
				skinStream = File.OpenRead(skinPath);

				// Open the file for reading.
				if (Path.GetExtension(skinPath).ToLower() == ".xml")
					return FromStream(null, skinStream, true);
				else
					return FromStream(null, skinStream, false);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(skinPath, typeof(GUISkin), ex);
			}
			finally
			{
				if (skinStream != null)
					skinStream.Dispose();
				skinStream = null;
			}
		}

		/// <summary>
		/// Function to create a GUI window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="caption">Caption for the window.</param>
		/// <param name="position">Position of the window.</param>
		/// <param name="size">Width and height of the window.</param>
		/// <returns>A new GUI window.</returns>
		public GUIWindow CreateWindow(string name, string caption, Vector2D position, Vector2D size)
		{
			GUIWindow window = null;		// New GUI window.

			if (_controls.Contains(name))
				throw new DuplicateObjectException(name);

			// Create the window.
			window = new GUIWindow(this, name, caption, position.X, position.Y, size.X, size.Y);
			GiveFocus(window);
			_controls.Add(window);

			return window;
		}

		/// <summary>
		/// Function to draw the GUI.
		/// </summary>
		public void Draw()
		{
			// Draw the background (desktop).
			if (_background != null)
			{
				ScaleBackground();
				_background.Draw();
			}


			// Draw the controls.
			for (int i = 0; i < _controls.Count; i++)
			{
				if ((_controls[i].Visible) && (_focusWindow != _controls[i]))
					_controls[i].DrawControl();
			}

			// Draw focus window on top of other windows.
			if (_focusWindow != null)
				_focusWindow.DrawControl();

			// Draw the cursor.
			if ((_skin.Cursor != null) && (_cursorVisible))
			{
				_skin.Cursor.Position = _cursorPosition;
				_skin.Cursor.Draw();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public GUI()
		{
			if (Gorgon.Screen == null)
				throw new NotInitializedException("Gorgon was not initialized.  Please call Gorgon.Initialize() and Gorgon.SetMode().", null);

			// Turn on the windows cursor.
			if ((Gorgon.InputDevices != null) && (Gorgon.InputDevices.Mouse != null))
			{
				_previousCursorVisible = Gorgon.InputDevices.Mouse.CursorVisible;
				Gorgon.InputDevices.Mouse.CursorVisible = false;
			}
			else
				Cursor.Hide();

			// Get the owner.
			_gorgonOwner = Gorgon.Screen.Owner;			
			_controls = new GUIControlList(null);

			// We bind to the window events for mouse/keyboard interaction because
			// it's not necessary to get the detailed info from the raw interfaces.
			// Plus by using Windows mouse events we gain some of the features 
			// windows has regarding mouse acceleration and such.
			_gorgonOwner.MouseMove += new MouseEventHandler(_gorgonOwner_MouseMove);
			_gorgonOwner.MouseDown += new MouseEventHandler(_gorgonOwner_MouseDown);
			_gorgonOwner.MouseUp += new MouseEventHandler(_gorgonOwner_MouseUp);
			_gorgonOwner.MouseWheel += new MouseEventHandler(_gorgonOwner_MouseWheel);
			_gorgonOwner.MouseDoubleClick += new MouseEventHandler(_gorgonOwner_MouseDoubleClick);
			_gorgonOwner.KeyDown += new KeyEventHandler(_gorgonOwner_KeyDown);
			_gorgonOwner.KeyUp += new KeyEventHandler(_gorgonOwner_KeyUp);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_skin.Dispose();

				// Remove events.
				_gorgonOwner.MouseMove -= new MouseEventHandler(_gorgonOwner_MouseMove);
				_gorgonOwner.MouseDown -= new MouseEventHandler(_gorgonOwner_MouseDown);
				_gorgonOwner.MouseUp -= new MouseEventHandler(_gorgonOwner_MouseUp);
				_gorgonOwner.MouseWheel -= new MouseEventHandler(_gorgonOwner_MouseWheel);
				_gorgonOwner.MouseDoubleClick -= new MouseEventHandler(_gorgonOwner_MouseDoubleClick);
				_gorgonOwner.KeyDown -= new KeyEventHandler(_gorgonOwner_KeyDown);
				_gorgonOwner.KeyUp -= new KeyEventHandler(_gorgonOwner_KeyUp);

				// Turn off the windows cursor.
				if ((Gorgon.InputDevices != null) && (Gorgon.InputDevices.Mouse != null))
					Gorgon.InputDevices.Mouse.CursorVisible = _previousCursorVisible;
				else
					Cursor.Show();
			}

			// Do unmanaged clean up.
			_skin = null;			
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
