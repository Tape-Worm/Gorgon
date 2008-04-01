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
// Created: Thursday, September 06, 2007 2:55:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private Input _input = null;								// Input devices interface.
		private Mouse _mouse = null;								// Mouse interface.
		private Keyboard _keyboard = null;							// Keyboard interface.
		private RenderImage _backBuffer = null;						// Back buffer.
		private float _radius = 4.0f;								// Pen radius.
		private BlendingModes _blendMode = BlendingModes.Modulated;	// Blend mode.
		private Image _backup = null;								// Screen backup.
		private Joystick _joystick = null;							// Joystick.
		private int _counter = 0;									// Joystick index counter.
		private TextSprite _messageSprite = null;					// Message sprite.
		private Font _font = null;									// Text font. 
		#endregion

		#region Methods.
		/// <summary>
		/// Handles any keyboard events.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void KeyboardEvent(object sender, KeyboardInputEventArgs e)
		{
			switch (e.Key)
			{
				case KeyboardKeys.Escape:
					Close();			// Close
					break;
				case KeyboardKeys.Down:
					_radius -= 1.0f;
					if (_radius < 2.0f)
						_radius = 2.0f;
					break;
				case KeyboardKeys.Up:
					_radius += 1.0f;
					if (_radius > 10.0f)
						_radius = 10.0f;
					break;
				case KeyboardKeys.F1:
					_blendMode = BlendingModes.Modulated;
					break;
				case KeyboardKeys.F2:
					_blendMode = BlendingModes.Additive;
					break;
				case KeyboardKeys.F3:
					_blendMode = BlendingModes.None;
					break;
				case KeyboardKeys.C:
					_backBuffer.Clear();
					break;
				case KeyboardKeys.J:
					if (_input.Joysticks.Count != 0)
					{
						// If we wrap around, then reset to no joystick.
						if (((_counter + 1) >= _input.Joysticks.Count) && (_joystick != null))
						{
							if (_joystick != null)
								_joystick.Enabled = false;

							_counter = 0;
							_messageSprite.Text = "Using mouse and keyboard.";
							_joystick = null;
						}
						else
						{
							// Move to next joystick.
							if (_joystick != null)
							{
								_joystick.Enabled = false;
								_counter++;
							}

							// Get the next stick.
							_joystick = _input.Joysticks[_counter];
							_joystick.Enabled = true;
							_messageSprite.Text = "Using joystick " + _joystick.Name;
						}
					}
					break;
			}
		}

		/// <summary>
		/// Handles any mouse input.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void MouseEvent(object sender, MouseInputEventArgs e)
		{
			Drawing.Color drawColor = Drawing.Color.Black;		// Drawing color.

			if (e.Buttons != GorgonLibrary.InputDevices.MouseButtons.None)
			{
				// Draw to the back buffer.
				_backBuffer.BeginDrawing();
				_backBuffer.BlendingMode = _blendMode;
				if ((e.Buttons & GorgonLibrary.InputDevices.MouseButtons.Button1) == GorgonLibrary.InputDevices.MouseButtons.Button1)
					drawColor = Drawing.Color.FromArgb(64, 0, 0, 192);
				if ((e.Buttons & GorgonLibrary.InputDevices.MouseButtons.Button2) == GorgonLibrary.InputDevices.MouseButtons.Button2)
					drawColor = Drawing.Color.FromArgb(64, 192, 0, 0);

				if (_radius > 3.0f)
					_backBuffer.FilledCircle(e.Position.X, e.Position.Y, _radius, drawColor);
				else
					_backBuffer.FilledRectangle(e.Position.X - (_radius / 2.0f), e.Position.Y - (_radius / 2.0f), _radius, _radius, drawColor);

				_backBuffer.EndDrawing();
			}
		}

		/// <summary>
		/// Handles the mouse wheel move event.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void MouseWheelMove(object sender, MouseInputEventArgs e)
		{
			if (e.WheelDelta > 0)
				_radius += 1.0f;
			if (e.WheelDelta < 0)
				_radius -= 1.0f;

			if (_radius < 2.0f)
				_radius = 2.0f;
			if (_radius > 10.0f)
				_radius = 10.0f;
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Vector2D cursorPosition = Vector2D.Zero;		// Cursor position.

			// Dump to the screen.
			_backBuffer.BlendingMode = BlendingModes.None;
			_backBuffer.Blit();

			if (_joystick == null)
				cursorPosition = _mouse.Position;
			else
			{
				// Adjust position to match screen coordinates.
				cursorPosition = new Vector2D(_joystick.X, _joystick.Y);
				cursorPosition.X = ((_joystick.X / (float)_joystick.AxisRanges[0].Range) * Gorgon.Screen.Width);
				cursorPosition.Y = ((_joystick.Y / (float)_joystick.AxisRanges[1].Range) * Gorgon.Screen.Height);
			}

			// Draw cursor.
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.BlendingMode = BlendingModes.Inverted;
			if (_radius > 3.0f)
				Gorgon.Screen.FilledCircle(cursorPosition.X, cursorPosition.Y, _radius, Drawing.Color.White);
			else
				Gorgon.Screen.FilledRectangle(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius, Drawing.Color.White);
			Gorgon.Screen.EndDrawing();
						
			// If we have a joystick button down, then draw a black dot.
			if ((_joystick != null) && (_joystick.Button[0]))
			{
				_backBuffer.BeginDrawing();
				if (_radius > 3.0f)
					_backBuffer.FilledCircle(cursorPosition.X, cursorPosition.Y, _radius, Drawing.Color.FromArgb(128, Drawing.Color.Black));
				else
					_backBuffer.FilledRectangle(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius, Drawing.Color.FromArgb(128, Drawing.Color.Black));
				_backBuffer.EndDrawing();
			}

			_messageSprite.BlendingMode = BlendingModes.Modulated;
			_messageSprite.Draw();
		}

		/// <summary>
		/// Handles the DeviceLost event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceLost(object sender, EventArgs e)
		{
			// Copy to the backup image.
			_backBuffer.CopyToImage(_backup);
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			// Reset the size of the back buffer.
			_backBuffer.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
			_backBuffer.Clear(Drawing.Color.White);

			// If we have backup data then restore it.
			if (_backup != null)
				_backBuffer.CopyFromImage(_backup);

			_mouse.SetPositionRange(0, 0, Gorgon.Screen.Width, Gorgon.Screen.Height);			
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Remove the standard events.
			Gorgon.Idle -= new FrameEventHandler(Gorgon_Idle);
			Gorgon.DeviceLost -= new EventHandler(Gorgon_DeviceLost);

			// Remove the input events.
			_mouse.MouseDown -= new MouseInputEvent(MouseEvent);
			_mouse.MouseMove -= new MouseInputEvent(MouseEvent);
			_mouse.MouseWheelMove -= new MouseInputEvent(MouseWheelMove);
			_keyboard.KeyDown -= new KeyboardInputEvent(KeyboardEvent);

			// Terminate.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Initialize Gorgon.
				Gorgon.Initialize();

				// Create text font.
				_font = new Font("Arial9pt", "Arial", 9.0f, true, true);

				// Load the input devices plug-in.
				_input = Input.LoadInputPlugIn(Environment.CurrentDirectory + @"\GorgonInput.DLL", "Gorgon.RawInput");

				// Bind the devices to this window.
				_input.Bind(this);

				// Enable the mouse.
				Cursor = Cursors.Cross;
				_mouse = _input.Mouse;
				_mouse.Enabled = true;
				_mouse.Exclusive = false;
				_mouse.MouseDown += new MouseInputEvent(MouseEvent);
				_mouse.MouseMove += new MouseInputEvent(MouseEvent);
				_mouse.MouseWheelMove += new MouseInputEvent(MouseWheelMove);

				// Enable the keyboard.
				_keyboard = _input.Keyboard;
				_keyboard.Enabled = true;
				_keyboard.Exclusive = false;
				_keyboard.KeyDown += new KeyboardInputEvent(KeyboardEvent);

				// Set the video mode.				
				Gorgon.SetMode(this, 640, 480, Gorgon.DesktopVideoMode.Format, true);

				// Create text sprite.
				_messageSprite = new TextSprite("Message", string.Empty, _font);
				_messageSprite.Color = Drawing.Color.Black;
				_messageSprite.Text = "Using mouse and keyboard.";

				// Create a back buffer.
				_backBuffer = new RenderImage("BackBuffer", 640, 480, ImageBufferFormats.BufferRGB888X8);
				_backBuffer.Clear(Drawing.Color.White);				

				// Set the mouse range and position.
				Cursor.Position = PointToScreen(new Drawing.Point(320, 240));
				_mouse.SetPosition(320.0f, 240.0f);
				_mouse.SetPositionRange(0, 0, 640.0f, 480.0f);

				// Set gorgon events.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
				Gorgon.DeviceLost += new EventHandler(Gorgon_DeviceLost);
				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Create backup image.
				_backup = new Image("BackupImage", _backBuffer.Width, _backBuffer.Height, _backBuffer.Format);

 				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
				Application.Exit();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion
	}
}