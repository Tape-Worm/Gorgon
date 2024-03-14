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

using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Input;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using GorgonMouseButtons = Gorgon.Input.MouseButtons;
// ReSharper disable All

namespace Gorgon.Examples;

/// <summary>
/// Our main form for the example.
/// </summary>
/// <remarks>
/// This example shows how to use Gorgon's Raw Input functionality to control the mouse and keyboard. 
/// 
/// The Raw Input interface gives low level access to Human Interface Devices such as a keyboard and mouse. It will also 
/// allow use of individual mice and keyboards if there is more than one of these devices attached to your system. This 
/// has the advantage of enabling users to use, for example, 2 mice that perform different functions in your application.
/// 
/// Raw Input is by its very nature, raw data. The devices will send their data to the application in a pretty much 
/// unprocessed form. Raw Keyboard data is the same as you'd get in standard windows events, and as such doesn't offer 
/// much over regular windows keyboard events. One advantage it has is that the keyboard can be set up to retrieve 
/// information while the application does not have focus. The mouse however, is a different story. It sends raw mouse 
/// data to the application in the form of "Mickeys". Thus, when you read the raw input data from a mouse, you will find 
/// that it does not correspond to your screen or application window size/position. This is by design. The raw mouse data 
/// is meant to be processed and transformed by an application into something useful (e.g. a 3rd person camera in a 3D 
/// game). The other thing to note is that a raw mouse does not use Windows mouse ballistics, so if a cursor is mapped 
/// to the raw input data, that cursor movement may seem off. The best practices would recommend that Raw Input be only 
/// used for things where the ballistics would offer a negative experience, and use Windows mouse events for things like 
/// GUIs. This example will help illustrate the subtle things that need to be done in order to use Raw Input as a GUI 
/// system (again, this is not recommended).
/// 
/// The Raw Input data is passed to the GorgonRawKeyboard and GorgonRawMouse objects. When a WM_INPUT (raw input) event 
/// comes in to the application message queue, the data is parsed and sent to the appropriate device. This allows us to 
/// check the state of the mouse and/or keyboard at any point in the application. These objects also implement events 
/// similar to those of Windows Forms keyboard/mouse events (in fact, this is the preferred usage).
/// 
/// If you're curious about the "BufferContext" stuff, that's all from GDI+ (System.Drawing) and allows us to set up a 
/// double buffer scenario so our mouse cursor can be drawn without flicker.  But that is not the scope of this example.
/// 
/// To create the raw mouse and keyboard object we only need to do the following:
/// 1. Create the GorgonRawInput object. This will coordinate how the devices talk with their objects.
/// 2. Create a GorgonRawMouse and GorgonRawKeyboard object. 
/// 3. Register those devices with the GorgonRawInput.RegisterDevice(...) method.
/// 
/// This example will use both polling and events for the raw input devices. To see the difference between the two, do 
/// the following while running the example:
/// 1. Hold down the left mouse button while in event mode (default), and move around.  The spray effect updates.
/// 2. Stop moving (but keep the left button pressed). The spray effect stops updating.  This is because there are no events
///    being fired and the events are the methods that update the spray effect.
/// 3. Now change to polling by pressing the "P" key.
/// 4. Hold down the left mouse button and notice that the spray keeps updating regardless of whether we're moving.  
/// 
/// You will also notice that the cursor may move slower while polling. This is because it is being updated on every iteration 
/// of the window message loop. This has the effect if retrieving the relative mouse position every time (and resetting it). 
/// And because of this, the amount of relative movement will appear smaller.
/// </remarks>
public partial class Form : System.Windows.Forms.Form
{
    #region Variables.
    // The spray effect.
    private Spray _spray;
    // Our mouse cursor.
    private MouseCursor _cursor;
    // Our input service.
    private GorgonRawInput _rawInput;
    // Our mouse interface.
    private GorgonRawMouse _mouse;
    // Our keyboard interface.
    private GorgonRawKeyboard _keyboard;
    // Mouse position.
    private Point _mousePosition = Point.Empty;
    // Current image for the cursor.
    private Image _currentCursor;
    // Pointing hand image for the cursor.
    private Image _pointerCursor;
    // Open hand image for the cursor.
    private Image _openHandCursor;
    // Flag to indicate whether to use polling or events.
    private bool _usePolling;
    // Timer used to refresh the labels.
    private readonly GorgonTimerQpc _labelTimer = new();
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the mid point of the display panel size, in screen space.
    /// </summary>
    private Point DisplayHalfSize => new(panelDisplay.ClientSize.Width / 2, panelDisplay.ClientSize.Height / 2);
    #endregion

    #region Methods.
    /// <summary>
    /// Handles the Paint event of the panelMouse control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
    private void DevicePanelsPaint(object sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using var pen = new Pen(Color.Black, SystemInformation.BorderSize.Height);
        e.Graphics.DrawLine(pen, new Point(0, 0), new Point(control.Width, 0));
    }

    /// <summary>
    /// Function to process during application idle time.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        // If we're using a polling method, then we can retrieve all the mouse
        // info in real time during our idle time.
        if (_usePolling)
        {
            UpdateMousePointerDisplay(_mouse.RelativePositionOffset, _mouse.Buttons);

            // Reset the relative positioning here. Otherwise, this value will not be updated until the next 
            // mouse event, which would cause our cursor to drift.
            _mouse.RelativePositionOffset = DX.Point.Zero;
        }

        // Display the mouse cursor.			
        _cursor.DrawMouseCursor(_mousePosition, _currentCursor, _spray.Surface);

        return true;
    }

    /// <summary>
    /// Function to update the mouse information.
    /// </summary>
    /// <param name="relativePosition">The raw input relative position of the mouse since it was last moved.</param>
    /// <param name="button">The current button being held down.</param>
    private void UpdateMousePointerDisplay(DX.Point relativePosition, GorgonMouseButtons button)
    {
        Cursor.Position = PointToScreen(DisplayHalfSize);

        int mousePositionX = _mousePosition.X + relativePosition.X.Sign();
        int mousePositionY = _mousePosition.Y + relativePosition.Y.Sign();

        // Limit the cursor to the client area of the drawing display.
        if ((mousePositionX >= panelDisplay.ClientRectangle.Left) && (mousePositionX <= panelDisplay.ClientRectangle.Right))
        {
            _mousePosition.X = mousePositionX;
        }

        // We split the axis to ensure that the position that -is- updated put into the final cursor positon.
        if ((mousePositionY >= panelDisplay.ClientRectangle.Top) && (mousePositionY <= panelDisplay.ClientRectangle.Bottom))
        {
            _mousePosition.Y = mousePositionY;
        }

        switch (button)
        {
            case GorgonMouseButtons.Button1:
                _spray.SprayPoint(_mousePosition, false);
                _currentCursor = _pointerCursor;
                break;
            case GorgonMouseButtons.Button2:
                _spray.SprayPoint(_mousePosition, true);
                _currentCursor = _pointerCursor;
                break;
            default:
                _currentCursor = _openHandCursor;
                break;
        }

        if (_labelTimer.Milliseconds < 15)
        {
            return;
        }

        string newText = $"{_mouse.Info.Description}: {_mousePosition.X}x{_mousePosition.Y} (Raw: {_mouse.Position.X}x{_mouse.Position.Y}) " +
                          $"Button: {button}.  Using {(_usePolling ? "Polling" : "Events")} for data retrieval.";

        if (!string.Equals(newText, labelMouse.Text, StringComparison.CurrentCulture))
        {
            labelMouse.Text = newText;
        }

        _labelTimer.Reset();
    }

    /// <summary>
    /// Function to update the keyboard information.
    /// </summary>
    /// <param name="key">Key that's currently pressed.</param>
    /// <param name="shift">Shifted keys.</param>
    private void UpdateKeyboardDisplay(Keys key, Keys shift)
    {
        Keys shiftKey = Keys.None;

        if ((Keys.Alt & shift) == Keys.Alt)
        {
            shiftKey = (shift & Keys.LMenu) == Keys.LMenu ? Keys.LMenu : Keys.RMenu;
        }

        if ((shift & Keys.Control) == Keys.Control)
        {
            shiftKey = (shift & Keys.LControlKey) == Keys.LControlKey ? Keys.LControlKey : Keys.RControlKey;
        }

        if ((shift & Keys.Shift) == Keys.Shift)
        {
            shiftKey = (shift & Keys.LShiftKey) == Keys.LShiftKey ? Keys.LShiftKey : Keys.RShiftKey;
        }

        string newText = $"{_keyboard.Info.Description}. Currently pressed key: {key}{((shiftKey != Keys.None) && (shiftKey != key) ? " + " + shiftKey : string.Empty)}  (Press 'P' to switch between polling and events for the mouse. Press 'ESC' to close.)";

        if (!string.Equals(newText, labelKeyboard.Text, StringComparison.CurrentCulture))
        {
            labelKeyboard.Text = newText;
        }
    }

    /// <summary>
    /// Handles the PointingDeviceUp event of the _mouse control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonMouseEventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void Mouse_ButtonUp(object sender, GorgonMouseEventArgs e) =>
        // Update the buttons so that only the buttons we have held down are showing.
        UpdateMousePointerDisplay(e.RelativePosition, e.ShiftButtons & ~e.Buttons);

    /// <summary>
    /// Handles the PointingDeviceDown event of the _mouse control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonMouseEventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void Mouse_ButtonDown(object sender, GorgonMouseEventArgs e) => UpdateMousePointerDisplay(e.RelativePosition, e.Buttons | e.ShiftButtons);

    /// <summary>
    /// Handles the PointingDeviceMove event of the _mouse control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonMouseEventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void Mouse_Move(object sender, GorgonMouseEventArgs e) =>
        UpdateMousePointerDisplay(e.RelativePosition, e.Buttons | e.ShiftButtons);

    /// <summary>
    /// Handles the KeyUp event of the _keyboard control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonKeyboardEventArgs" /> instance containing the event data.</param>
    private void Keyboard_KeyUp(object sender, GorgonKeyboardEventArgs e)
    {
        // If we press "P", then switch between polling and events.
        if (e.Key == Keys.P)
        {
            _mouse.RelativePositionOffset = DX.Point.Zero;
            _usePolling = !_usePolling;
            if (_usePolling)
            {
                // Turn off mouse events when polling.
                _mouse.MouseMove -= Mouse_Move;
                _mouse.MouseButtonDown -= Mouse_ButtonDown;
                _mouse.MouseButtonUp -= Mouse_ButtonUp;
            }
            else
            {
                // Turn on mouse events when not polling.
                _mouse.MouseMove += Mouse_Move;
                _mouse.MouseButtonDown += Mouse_ButtonDown;
                _mouse.MouseButtonUp += Mouse_ButtonUp;
            }
        }

        UpdateKeyboardDisplay(Keys.None, e.ModifierKeys);
    }

    /// <summary>
    /// Handles the KeyDown event of the _keyboard control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonKeyboardEventArgs" /> instance containing the event data.</param>		
    private void Keyboard_KeyDown(object sender, GorgonKeyboardEventArgs e)
    {
        // Exit the application.
        if (e.Key == Keys.Escape)
        {
            Close();
            return;
        }

        UpdateKeyboardDisplay(e.Key, e.ModifierKeys);
    }

    /// <summary>
    /// Handles the Resize event of the panelDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void PanelDisplay_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            return;
        }

        _spray?.Resize(panelDisplay.ClientSize);
    }

    /// <summary>
    /// Function to create the mouse device.
    /// </summary>
    private void CreateMouse()
    {
        // Create the raw input mouse.
        // Note, this constructor could be changed to:
        // _mouse = new GorgonRawMouse(mouseInfo) 
        //
        // to retrieve a raw input interface for only a specific device.
        // When left empty like this, it will use the combined input from all mice.
        _mouse = new GorgonRawMouse();

        // Assign an event to notify us when the mouse is moving.
        _mouse.MouseMove += Mouse_Move;

        // Assign another event to notify us when a mouse button was clicked.
        _mouse.MouseButtonDown += Mouse_ButtonDown;
        _mouse.MouseButtonUp += Mouse_ButtonUp;

        // Center the mouse on the window.
        _mousePosition = DisplayHalfSize;
        Cursor.Position = PointToScreen(DisplayHalfSize);

        // Finally, register our mouse device with the raw input system so it can receive 
        // raw input data.
        _rawInput.RegisterDevice(_mouse);

        UpdateMousePointerDisplay(_mouse.Position, GorgonMouseButtons.None);
    }

    /// <summary>
    /// Function to create the keyboard device.
    /// </summary>
    private void CreateKeyboard()
    {
        // Create the raw input keyboard.
        // Note, this constructor could be changed to:
        // _mouse = new GorgonRawKeyboard(keyboardInfo) 
        //
        // to retrieve a raw input interface for only a specific device.
        // When left empty like this, it will use the combined input from all mice.
        _keyboard = new GorgonRawKeyboard();

        // Set up an event handler for our keyboard.
        _keyboard.KeyDown += Keyboard_KeyDown;
        _keyboard.KeyUp += Keyboard_KeyUp;

        // Finally, register our keyboard device with the raw input system so it can receive 
        // raw input data.
        _rawInput.RegisterDevice(_keyboard);

        UpdateKeyboardDisplay(Keys.None, Keys.None);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            // Set our default cursor.
            _currentCursor = _openHandCursor = Resources.hand_icon;
            _pointerCursor = Resources.hand_pointer_icon;

            // Create our input factory.
            _rawInput = new GorgonRawInput(this);

            // Ensure that we have the necessary input devices.
            // This enumeration method could be used to retrieve only a single specific mouse out of many.
            if (_rawInput.EnumerateMice().Count == 0)
            {
                GorgonDialogs.ErrorBox(this, "There were no mice detected on this computer.  The application requires a mouse.");
                GorgonApplication.Quit();
            }

            // This enumeration method could be used to retrieve only a single specific keyboard out of many.
            if (_rawInput.EnumerateKeyboards().Count == 0)
            {
                GorgonDialogs.ErrorBox(this, "There were no keyboards detected on this computer.  The application requires a keyboard.");
                GorgonApplication.Quit();
            }

            // Get our input devices.				
            CreateMouse();
            CreateKeyboard();

            // When the display area changes size, update the spray effect
            // and limit the mouse.
            panelDisplay.Resize += PanelDisplay_Resize;

            // Set up our spray object.
            _spray = new Spray(panelDisplay.ClientSize);
            _cursor = new MouseCursor(panelDisplay)
            {
                Hotspot = new Point(-16, -3)
            };

            // Set up our idle method.
            GorgonApplication.IdleMethod = Idle;
        }
        catch (Exception ex)
        {
            // We do this here instead of just calling the dialog because this
            // function will send the exception to the Gorgon log file.
            ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
            GorgonApplication.Quit();
        }
    }

    /// <summary>
    /// Handles the <see cref="E:Activated" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        if (_mouse is null)
        {
            return;
        }

        GorgonRawMouse.CursorVisible = false;
    }

    /// <summary>
    /// Handles the <see cref="E:Deactivate" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);

        if (_mouse is null)
        {
            return;
        }

        GorgonRawMouse.CursorVisible = true;
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // Unlock the mouse cursor and make it visible again.
        Capture = false;
        GorgonRawMouse.CursorVisible = true;

        // Always dispose the raw input interface. 
        if (_mouse is not null)
        {
            _rawInput?.UnregisterDevice(_mouse);
            _mouse = null;
        }

        if (_keyboard is not null)
        {
            _rawInput?.UnregisterDevice(_keyboard);
        }

        _rawInput?.Dispose();

        _cursor?.Dispose();

        _spray?.Dispose();
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="Form" /> class.
    /// </summary>
    public Form() => InitializeComponent();
    #endregion
}
