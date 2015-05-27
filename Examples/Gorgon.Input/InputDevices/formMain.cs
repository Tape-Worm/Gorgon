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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Input;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Our main form for the example.
	/// </summary>
	/// <remarks>
	/// In this example we will instance and control the mouse (pointing device) and keyboard devices.  So, naturally, you'll
	/// need a mouse and keyboard for this.  If you don't have one... well... how are you doing anything?
	/// 
	/// Unlike the InputFactory example, we're just going to load the Raw Input plug-in and use the keyboard/mouse from that
	/// interface.  The Raw Input is most useful for mice (although it has a keyboard component) and gives very precise control
	/// over the mouse.  It allows the developer to lock the mouse to the window while it has focus, or even when the window
	/// isn't in focus (this of course means no other applications will get access to the mouse).  This isn't the default 
	/// behaviour, and must be turned on by setting the Exclusive property to true (and AllowBackground to true for background
	/// exclusive access).  Keyboards also have these properties as well.  Be warned though that when a keyboard goes into
	/// exclusive mode the standard windows hot keys will no longer be recognized (e.g. Alt+F4, etc..) and must be processed by 
	/// your application.
	/// 
	/// If you're curious about the "BufferContext" stuff, that's all from GDI+ (System.Drawing) and allows us to set up a 
	/// double buffer scenario so our mouse cursor can be drawn without flicker.  But that is not the scope of this example.
	/// 
	/// To create the mouse and keyboard object we need to instance it from the InputFactory that comes from our Raw Input 
	/// plug-in.  To learn more about loading the InputFactories from their plug-ins see the InputFactory example.
	/// 
	/// Once we have these objects via the InputFactory.CreatePointingDevice and InputFactory.CreateKeyboard methods, we can then
	/// set them up for exclusive access and then assign events that will be triggered by input from the device.  This is a 
	/// similar setup to the Windows Forms input events (KeyDown, MouseDown, MouseMove, etc...) and is great in an environment 
	/// where the overhead of events is not an issue.  But in performance critical sections or just for absolute control, these
	/// objects can be polled at any time (e.g. in your Idle loop method).  This way the application can retrieve data from a 
	/// device like the mouse in real time, that is, as fast as your computer can call the update loop.  
	/// 
	/// To see the difference between polling and events do the following while running the example:
	/// 1. Hold down the left mouse button while in event mode (default), and move around.  The spray effect updates.
	/// 2. Stop moving (but keep the left button pressed). The spray effect stops updating.  This is because there are no events
	///    being fired and the events are the methods that update the spray effect.
	/// 3. Now change to polling by pressing the "P" key.
	/// 4. Hold down the left mouse button and notice that the spray keeps updating regardless of whether we're moving.  
	/// 
	/// And that is the difference between polling and event driven input data.
	/// 
	/// Joysticks are exposed in the Raw Input plug-in, but use a very basic interface and don't expose any special features 
	/// (e.g. force feedback).  And unlike other devices, joysticks don't raise events and must be polled via its Poll() method.  
	/// In this example, if a joystick is detected it will be noted on the display panel and pressing the primary button will 
	/// draw a spray effect to the display panel.
    /// 
    /// Joystick axis infomation usually returns values much larger than the available display area, usually between a negative
    /// and positive value and with the y-axis flipped.  So when we gather the information, we need to flip the y-axis and 
    /// transform the coordinates into screen space via the JoystickTransformed property.
	/// </remarks>
	public partial class formMain : Form
	{
		#region Variables.
        private Spray _spray;                                   // The spray effect.
        private MouseCursor _cursor;                            // Our mouse cursor.
		private GorgonInputFactory _factory;					// Our input factory.
		private GorgonPointingDevice _mouse;					// Our mouse interface.
		private GorgonJoystick _joystick;						// A joystick interface.
		private GorgonKeyboard _keyboard;						// Our keyboard interface.
		private Point _mousePosition = Point.Empty;				// Mouse position.
		private Image _currentCursor;							// Current image for the cursor.
		private bool _usePolling;								// Flag to indicate whether to use polling or events.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the joystick primary axis coordinates transformed into screen space.
        /// </summary>
        /// <remarks>The joystick axis coordinates can be larger or smaller than screen space, so we 
        /// need to transform them to the confines of the display area.</remarks>
        private Point JoystickTransformed
        {
            get
            {
                Point screenPosition = Point.Empty;

                if (_joystick == null)
                {
                    return Point.Empty;
                }

                // We -must- call poll here or else the joystick will appear to be
                // disconnected and will not have any current data.
                _joystick.Poll();

                // Ensure that the joystick is connected and the button is pressed.
	            if (!_joystick.IsConnected)
	            {
		            return screenPosition;
	            }

	            // Get our joystick data and constrain it.
	            // First get the normalized joystick value.
	            // Do this by first shifting the coordinates to the minimum range value.
	            var stickNormalized = new PointF(_joystick.X - (float)_joystick.Capabilities.XAxisRange.Minimum,
		            _joystick.Y - (float)_joystick.Capabilities.YAxisRange.Minimum);
	            // Then normalize.
	            stickNormalized = new PointF(stickNormalized.X / (_joystick.Capabilities.XAxisRange.Range + 1), 
		            stickNormalized.Y / (_joystick.Capabilities.YAxisRange.Range + 1));

	            // Now transform the normalized point into display space.
	            screenPosition = new Point((int)(stickNormalized.X * (panelDisplay.ClientSize.Width - 1)) 
		            , (panelDisplay.ClientSize.Height - 1) - (int)(stickNormalized.Y * panelDisplay.ClientSize.Height));

					
	            if (_joystick.Button[0].IsPressed)
	            {
		            // Spray the screen.
		            _currentCursor = Resources.hand_pointer_icon;
		            _mouse.Position = _mousePosition = screenPosition;	
		            _spray.SprayPoint(_mousePosition);
	            }
	            else
	            {
		            // Turn off the cursor if the mouse button isn't held down.
		            if ((_mouse.Button & PointingDeviceButtons.Button1) != PointingDeviceButtons.Button1)
		            {
			            _currentCursor = Resources.hand_icon;
		            }
	            }

	            return screenPosition;
            }
        }
		#endregion

		#region Methods.
        /// <summary>
        /// Handles the Paint event of the panelMouse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
        private void DevicePanelsPaint(object sender, PaintEventArgs e)
        {
            var control = sender as Control;

	        if (control == null)
	        {
		        return;
	        }

	        using(var pen = new Pen(Color.Black, SystemInformation.BorderSize.Height))
	        {
		        e.Graphics.DrawLine(pen, new Point(0, 0), new Point(control.Width, 0));
	        }
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
                UpdateMouseLabel(_mouse.Position, _mouse.Button);
			}

            // Update the joystick information.
            UpdateJoystickLabel(JoystickTransformed);

			// Display the mouse cursor.			
            _cursor.DrawMouseCursor(_mousePosition, _currentCursor, _spray.Surface);

			return true;
		}

        /// <summary>
        /// Function to update the joystick label.
        /// </summary>
        /// <param name="joystickTransformed">The transformed screen point for the joystick.</param>
        private void UpdateJoystickLabel(Point joystickTransformed)
        {
            if (_joystick == null)
            {
                return;
            }

            // Display the proper joystick text.
            if (_joystick.IsConnected)
            {
                labelJoystick.Text = string.Format("{0} connected.  Position: {1}x{2} (Raw {4}x{5}).  Primary button {3}"
                                            , _joystick.Name
                                            , joystickTransformed.X
                                            , joystickTransformed.Y
                                            , (_joystick.Button[0].IsPressed ? "pressed" : "not pressed (press the button to spray).")
                                            , _joystick.X
                                            , _joystick.Y);
            }
            else
            {
                labelJoystick.Text = string.Format("{0} not connected.", _joystick.Name);
            }
        }

        /// <summary>
        /// Function to update the mouse information.
        /// </summary>
        /// <param name="position">The position of the mouse cursor.</param>
        /// <param name="button">The current button being held down.</param>
        private void UpdateMouseLabel(PointF position, PointingDeviceButtons button)
        {
            if ((button & PointingDeviceButtons.Button1) == PointingDeviceButtons.Button1)
            {
                _spray.SprayPoint(Point.Round(position));
                _currentCursor = Resources.hand_pointer_icon;
            }
            else
            {
                _currentCursor = Resources.hand_icon;
            }

            _mousePosition = new Point((int)position.X, (int)_mouse.Position.Y);

            labelMouse.Text = string.Format("{0}: {1}x{2}.  Button: {3}.  Using {4} for data retrieval.",
                                _mouse.Name,
                                position.X.ToString("0.#"),
                                position.Y.ToString("0.#"),
                                button,
                                (_usePolling ? "Polling" : "Events"));
        }

	    /// <summary>
	    /// Function to update the keyboard label.
	    /// </summary>
	    /// <param name="key">Key that's currently pressed.</param>
	    /// <param name="shift">Shifted keys.</param>
	    private void UpdateKeyboardLabel(KeyboardKeys key, KeyboardKeys shift)
		{
			var shiftKey = KeyboardKeys.None;

			if ((KeyboardKeys.Alt & shift) == KeyboardKeys.Alt)
			{
				shiftKey = (shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion ? KeyboardKeys.LMenu : KeyboardKeys.RMenu;
			}

			if ((shift & KeyboardKeys.Control) == KeyboardKeys.Control)
			{
				shiftKey = (shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion ? KeyboardKeys.LControlKey : KeyboardKeys.RControlKey;
			}

			if ((shift & KeyboardKeys.Shift) == KeyboardKeys.Shift)
			{
				shiftKey = (shift & KeyboardKeys.LeftVersion) == KeyboardKeys.LeftVersion ? KeyboardKeys.LShiftKey : KeyboardKeys.RShiftKey;
			}


			labelKeyboard.Text = string.Format("{2}. Currently pressed key: {0}{1}  (Press 'P' to switch between polling and events for the mouse. Press 'ESC' to close.)"
												, key
												, ((shiftKey != KeyboardKeys.None) && (shiftKey != key) ? " + " + shiftKey : string.Empty)
                                                , _keyboard.Name);				
		}

		/// <summary>
		/// Handles the PointingDeviceUp event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _mouse_PointingDeviceUp(object sender, PointingDeviceEventArgs e)
		{
			// Update the buttons so that only the buttons we have held down are showing.
            UpdateMouseLabel(e.Position, e.ShiftButtons & ~e.Buttons);
		}

		/// <summary>
		/// Handles the PointingDeviceDown event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
        private void _mouse_PointingDeviceDown(object sender, PointingDeviceEventArgs e)
		{
            UpdateMouseLabel(e.Position, e.Buttons | e.ShiftButtons);
        }

		/// <summary>
		/// Handles the PointingDeviceMove event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _mouse_PointingDeviceMove(object sender, PointingDeviceEventArgs e)
		{
            UpdateMouseLabel(e.Position, e.Buttons | e.ShiftButtons);
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

			// Exit the application.
			if (e.Key == KeyboardKeys.Escape)
			{
				Close();
				return;
			}

			UpdateKeyboardLabel(e.Key, e.ModifierKeys);
		}

		/// <summary>
		/// Handles the Resize event of the panelDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void panelDisplay_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				return;
			}

			// If we resize our window, update our position range.
			if (_mouse != null)
			{
				_mouse.PositionRange = new RectangleF(0, 0, panelDisplay.ClientSize.Width, panelDisplay.ClientSize.Height);
			}

            if (_spray != null)
            {
                _spray.Resize(panelDisplay.ClientSize);
            }
		}

        /// <summary>
        /// Function to create the mouse device.
        /// </summary>
        private void CreateMouse()
        {
            // Create the device from the factory.
            _mouse = _factory.CreatePointingDevice(this);

            // Set up the mouse for use.
            _mouse.Enabled = true;

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

			UpdateMouseLabel(_mouse.Position, PointingDeviceButtons.None);			
        }

        /// <summary>
        /// Function to create the keyboard device.
        /// </summary>
        private void CreateKeyboard()
        {
            // Create our device.
            _keyboard = _factory.CreateKeyboard(this);

            // Enable the devices.
            _keyboard.Enabled = true;
            // Set up an event handler for our keyboard.
            _keyboard.KeyDown += _keyboard_KeyDown;
            _keyboard.KeyUp += _keyboard_KeyUp;

			UpdateKeyboardLabel(KeyboardKeys.None, KeyboardKeys.None);
        }

        /// <summary>
        /// Function to create the joystick device.
        /// </summary>
        private void CreateJoystick()
        {
            // If we have a joystick controller, then let's activate it.
	        if (_factory.JoystickDevices.Count <= 0)
	        {
		        return;
	        }

	        // Find the first one that's active.
	        var activeDevice = (from joystick in _factory.JoystickDevices
		        where joystick.IsConnected
		        select joystick).FirstOrDefault();

	        if (activeDevice == null)
	        {
		        return;
	        }

	        // Note that joysticks from Raw Input are always exclusive access,
	        // so setting _joystick.Exclusive = true; does nothing.
	        _joystick = _factory.CreateJoystick(this, activeDevice.Name);

	        // Show our joystick information.
	        labelJoystick.Text = string.Empty;
	        panelJoystick.Visible = true;

	        UpdateJoystickLabel(JoystickTransformed);
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
				// Set our default cursor.
				_currentCursor = Resources.hand_icon;
				
				// Load our raw input plug-in assembly.
				GorgonApplication.PlugIns.LoadPlugInAssembly(Program.PlugInPath + "Gorgon.Input.Raw.DLL");

				// Create our input factory.
				_factory = GorgonInputFactory.CreateInputFactory("Gorgon.Input.GorgonRawPlugIn");

				// Get our device info.
                // This function is called when the factory is created.
                // However, I'm calling it here just to show that it exists.
				_factory.EnumerateDevices();

				// Validate, even though it's highly unlikely we'll run into these.
				if (_factory.PointingDevices.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "There were no mice detected on this computer.  The application requires a mouse.");
					GorgonApplication.Quit();
				}

				if (_factory.KeyboardDevices.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "There were no keyboards detected on this computer.  The application requires a keyboard.");
					GorgonApplication.Quit();
				}

				// Get our input devices.				
                CreateMouse();
                CreateKeyboard();
                CreateJoystick();

				// When the display area changes size, update the spray effect
				// and limit the mouse.
				panelDisplay.Resize += panelDisplay_Resize;

				// Set the initial range of the mouse cursor.
				_mouse.PositionRange = Rectangle.Round(panelDisplay.ClientRectangle);

                // Set up our spray object.
                _spray = new Spray(panelDisplay.ClientSize);
                _cursor = new MouseCursor(panelDisplay)
                    {
                        Hotspot = new Point(-16, -3)
                    };

			    // Set up our idle method.
                GorgonApplication.ApplicationIdleLoopMethod = Idle;                
			}
			catch (Exception ex)
			{
				// We do this here instead of just calling the dialog because this
				// function will send the exception to the Gorgon log file.
				GorgonException.Catch(ex, _ => GorgonDialogs.ErrorBox(this, _), true);
				GorgonApplication.Quit();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }

			if (_spray == null)
			{
				return;
			}

			_spray.Dispose();
			_spray = null;
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
