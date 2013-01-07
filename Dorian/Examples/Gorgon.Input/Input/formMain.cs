#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, January 5, 2013 3:33:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Input;
using GorgonLibrary.UI;

namespace GorgonLibrary.Examples
{
	/// <summary>
	/// Our main form for the example.
	/// </summary>
	public partial class formMain : Form
	{
		#region Variables.
		private BufferedGraphicsContext _graphicsContext = null;		// Buffered graphics context.
		private BufferedGraphics _buffer = null;						// Buffered graphics page.
		private Image _mouseImage = null;								// Image to use for double buffering our mouse.
		private Graphics _imageGraphics = null;							// Graphics interface for the mouse double buffer image.
		private Graphics _graphics = null;								// GDI+ graphics interface.
		private GorgonInputFactory _factory = null;						// Our input factory.
		private GorgonPointingDevice _mouse = null;						// Our mouse interface.
		private GorgonKeyboard _keyboard = null;						// Our keyboard interface.
		private Point _mousePosition = Point.Empty;						// Mouse position.
		private Image _currentCursor = null;							// Current image for the cursor.
		private bool _usePolling = false;								// Flag to indicate whether to use polling or events.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the double buffering data.
		/// </summary>
		private void CleanUpDoubleBuffer()
		{
			if (_buffer != null)
			{
				_buffer.Dispose();
				_buffer = null;
			}

			if (_graphicsContext != null)
			{
				_graphicsContext.Dispose();
				_graphicsContext = null;
			}

			if (_imageGraphics != null)
			{
				_imageGraphics.Dispose();
				_imageGraphics = null;
			}

			if (_mouseImage != null)
			{
				_mouseImage.Dispose();
				_mouseImage = null;
			}

			if (_graphics != null)
			{
				_graphics.Dispose();
				_graphics = null;
			}
		}

		/// <summary>
		/// Function to set up double buffering for our mouse cursor.
		/// </summary>
		private void SetupDoubleBuffer()
		{
			CleanUpDoubleBuffer();

			_graphics = Graphics.FromHwnd(panelDisplay.Handle);

			_mouseImage = new Bitmap(panelDisplay.ClientSize.Width, panelDisplay.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			_imageGraphics = Graphics.FromImage(_mouseImage);

			_graphicsContext = BufferedGraphicsManager.Current;
			_buffer = _graphicsContext.Allocate(_imageGraphics, new Rectangle(0, 0, _mouseImage.Width, _mouseImage.Height));
		}

		/// <summary>
		/// Function to process during application idle time.
		/// </summary>
		/// <returns>TRUE to continue processing, FALSE to stop.</returns>
		private bool Idle()
		{
			// If we're using a polling method, then we can retrieve all the mouse
			// info in real time during our idle time.
			if (_usePolling)
			{
				if ((_mouse.Button & PointingDeviceButtons.Button1) == PointingDeviceButtons.Button1)
				{
					_currentCursor = Properties.Resources.hand_pointer_icon;
				}
				else
				{
					_currentCursor = Properties.Resources.hand_icon;
				}

				_mousePosition = new Point((int)_mouse.Position.X - 16, (int)_mouse.Position.Y - 3);

				UpdateMouseLabel(_mouse.Position, _mouse.Button);				
			}

			// Render our mouse cursor.
			_buffer.Graphics.Clear(panelDisplay.BackColor);
			_buffer.Graphics.DrawImage(_currentCursor, _mousePosition);
			_buffer.Render();
			_buffer.Render(_graphics);

			return true;
		}

		/// <summary>
		/// Function to update the mouse label.
		/// </summary>
		/// <param name="position">Position of the mouse.</param>
		/// <param name="button">The buttons that are currently pressed.</param>
		private void UpdateMouseLabel(PointF position, PointingDeviceButtons button)
		{
			labelMouse.Text = string.Format("{0}: {1}x{2} ({3})\nUsing {4}.",
								_factory.PointingDevices[0].Name,
								position.X.ToString("0.#"),
								position.Y.ToString("0.#"),
								button.ToString(),
								(_usePolling ? "Polling" : "Events"));
		}

		/// <summary>
		/// Function to update the keyboard label.
		/// </summary>
		/// <param name="key">Key that's currently pressed.</param>
		private void UpdateKeyboardLabel(KeyboardKeys key, KeyboardKeys shift)
		{
			KeyboardKeys shiftKey = KeyboardKeys.None;

			if ((shift & KeyboardKeys.Alt) == KeyboardKeys.Alt)
			{
				if ((shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion)
				{
					shiftKey = KeyboardKeys.LMenu;
				}
				else
				{
					shiftKey = KeyboardKeys.RMenu;
				}
			}

			if ((shift & KeyboardKeys.Control) == KeyboardKeys.Control)
			{
				if ((shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion)
				{
					shiftKey = KeyboardKeys.LControlKey;
				}
				else
				{
					shiftKey = KeyboardKeys.RControlKey;
				}
			}

			if ((shift & KeyboardKeys.Shift) == KeyboardKeys.Shift)
			{
				if ((shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion)
				{
					shiftKey = KeyboardKeys.LShiftKey;
				}
				else
				{
					shiftKey = KeyboardKeys.RShiftKey;
				}
			}


			labelKeyboard.Text = string.Format("Currently pressed key: {0}{1}  (Press 'P' to switch between polling and events for the mouse)"
												, key.ToString()
												, ((shiftKey != KeyboardKeys.None) && (shiftKey != key) ? " + " + shiftKey.ToString() : string.Empty));				
		}

		/// <summary>
		/// Handles the PointingDeviceUp event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void _mouse_PointingDeviceUp(object sender, PointingDeviceEventArgs e)
		{
			_currentCursor = Properties.Resources.hand_icon;

			// Update the buttons so that only the buttons we have held down are showing.
			UpdateMouseLabel(e.Position, e.ShiftButtons & ~e.Buttons);
		}

		/// <summary>
		/// Handles the PointingDeviceDown event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void _mouse_PointingDeviceDown(object sender, PointingDeviceEventArgs e)
		{
			if ((e.Buttons & PointingDeviceButtons.Left) == PointingDeviceButtons.Left)
			{
				_currentCursor = Properties.Resources.hand_pointer_icon;
			}

			UpdateMouseLabel(e.Position, e.Buttons | e.ShiftButtons);
		}

		/// <summary>
		/// Handles the PointingDeviceMove event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void _mouse_PointingDeviceMove(object sender, PointingDeviceEventArgs e)
		{
			// The hot spot for the cursors we're using is at 16x3.
			Point position = new Point((int)e.Position.X - 16, (int)e.Position.Y - 3);

			_mousePosition = position;

			UpdateMouseLabel(e.Position, e.Buttons);			
		}

		/// <summary>
		/// Handles the KeyUp event of the _keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyboardEventArgs" /> instance containing the event data.</param>
		private void _keyboard_KeyUp(object sender, KeyboardEventArgs e)
		{
			UpdateKeyboardLabel(KeyboardKeys.None, e.ModifierKeys);
		}

		/// <summary>
		/// Handles the KeyDown event of the _keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyboardEventArgs" /> instance containing the event data.</param>		
		private void _keyboard_KeyDown(object sender, KeyboardEventArgs e)
		{
			// If we press "P", then switch between polling and events.
			if (e.Key == KeyboardKeys.P)
			{
				_usePolling = !_usePolling;
				if (_usePolling)
				{
					// Turn off mouse events when polling.
					_mouse.PointingDeviceMove -= _mouse_PointingDeviceMove;
					_mouse.PointingDeviceDown -= _mouse_PointingDeviceDown;
					_mouse.PointingDeviceUp -= _mouse_PointingDeviceUp;
				}
				else
				{
					// Turn on mouse events when not polling.
					_mouse.PointingDeviceMove += _mouse_PointingDeviceMove;
					_mouse.PointingDeviceDown += _mouse_PointingDeviceDown;
					_mouse.PointingDeviceUp += _mouse_PointingDeviceUp;
				}
			}

			UpdateMouseLabel(_mousePosition, _mouse.Button);
			UpdateKeyboardLabel(e.Key, e.ModifierKeys);
		}

		/// <summary>
		/// Function called when the window is resized.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (WindowState == FormWindowState.Minimized)
			{
				return;
			}

			// If we resize our window, update our position range.
			if (_mouse != null)
			{
				_mouse.PositionRange = new RectangleF(0, 0, panelDisplay.ClientSize.Width, panelDisplay.ClientSize.Height);
			}

			if (_graphics != null)
			{
				SetupDoubleBuffer();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Get the graphics interface for this window.
				SetupDoubleBuffer();

				// Set our default cursor.
				_currentCursor = Properties.Resources.hand_icon;
				
				// Set up our idle method.
				Gorgon.ApplicationIdleLoopMethod = Idle;

				// Load our raw input plug-in assembly.
				Gorgon.PlugIns.LoadPlugInAssembly(Program.PlugInPath + "Gorgon.Input.Raw.DLL");

				// Create our input factory.
				_factory = GorgonInputFactory.CreateInputFactory("GorgonLibrary.Input.GorgonRawPlugIn");

				// Get our device info.
				_factory.EnumerateDevices();

				// Validate, even though it's highly unlikely we'll run into these.
				if (_factory.PointingDevices.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "There were no mice detected on this computer.  The application requires a mouse.");
					Gorgon.Quit();
				}

				if (_factory.KeyboardDevices.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "There were no keyboards detected on this computer.  The application requires a keyboard.");
					Gorgon.Quit();
				}

				// Get our input devices.				
				_mouse = _factory.CreatePointingDevice(this);
				_keyboard = _factory.CreateKeyboard(this);

				// Enable the devices.
				_mouse.Enabled = true;
				_keyboard.Enabled = true;

				// Set the mouse as exclusively owned by this window.
				// This way all the mouse input will go to this window when it's got focus.
				_mouse.Exclusive = true;

				// Assign an event to notify us when the mouse is moving.
				_mouse.PointingDeviceMove += _mouse_PointingDeviceMove;

				// Assign another event to notify us when a mouse button was clicked.
				_mouse.PointingDeviceDown += _mouse_PointingDeviceDown;
				_mouse.PointingDeviceUp += _mouse_PointingDeviceUp;

				// Limit the mouse position to the client area of the window.
				_mouse.PositionRange = new RectangleF(0, 0, panelDisplay.ClientSize.Width, panelDisplay.ClientSize.Height);

				// Set up an event handler for our keyboard.
				_keyboard.KeyDown += _keyboard_KeyDown;
				_keyboard.KeyUp += _keyboard_KeyUp;

				UpdateKeyboardLabel(KeyboardKeys.None, KeyboardKeys.None);
				UpdateMouseLabel(_mouse.Position, _mouse.Button);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Gorgon.Quit();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			CleanUpDoubleBuffer();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain" /> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}
