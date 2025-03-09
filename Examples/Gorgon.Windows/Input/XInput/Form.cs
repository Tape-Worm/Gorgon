// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Friday, January 11, 2013 8:27:21 AM
// 

using System.Diagnostics;
using System.Numerics;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Timing;
using Gorgon.UI;
using Gorgon.Windows.Input;
using Gorgon.Windows.Input.Devices;

namespace Gorgon.Examples;

/// <summary>
/// Our main form for the example
/// </summary>
/// <remarks>
/// This example is exclusively for the XBox controller (although it could be made to work with a regular controller).  
/// 
/// Here we will load the XInput plug-in which will give us access to the functionality in the XBox controllers.  
/// 
/// The XBox controller interface will always return 4 controllers, even if there is only 1 controller connected.  Here
/// we make use of all 4 controllers (if connected) to give a sort of multi-player drawing example.  We do this by 
/// tracking the state of each controller in our idle loop.  
/// 
/// The set up is similar to the other input examples:  Load the driver plug-in assembly, create the xinput driver and 
/// create each controller as needed.  Then in the idle loop, we poll the controller for input.  If no XBox controller is 
/// found a prompt to plug one in is shown
/// 
/// This example also shows how to use vibration and how to make a pressure sensitive trigger on the XBox controllers
/// 
/// To activate the spray can effect press the trigger.  The harder it's pushed, the darker the color and the quicker 
/// the trigger is pushed, the thicker the spray.  Use the secondary directional stick to spray in a given direction.  
/// </remarks>
public partial class Form
    : System.Windows.Forms.Form
{
    // Our XBox controllers.
    private readonly List<Controller> _controllers = [];
    // Surface to draw on.
    private DrawingSurface? _surface;
    // The input system.
    private readonly IGorgonInput _input;
    // The input event buffer.
    private readonly GorgonInputEventBuffer _events = new();
    // The last throttle amount.
    private int _lastThrottle;
    // The string builder for updating the controller information.
    private readonly StringBuilder _labelText = new();

    /// <summary>
    /// Function to handle a key up event on the form.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Form_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    /// <summary>
    /// Function to update the controller labels.
    /// </summary>
    /// <param name="controller">The active controller.</param>
    private void UpdateControllerLabels(Controller controller)
    {
        Panel? panel = (Panel?)panelControllers.Controls["panelController" + controller.Device.Info.XInputSlot];

        if (panel is null)
        {
            return;
        }

        Label? label = (Label?)panel.Controls["labelController" + controller.Device.Info.XInputSlot];

        if (label is null)
        {
            return;
        }

        panel.Visible = true;

        GorgonPoint leftStick = controller.Device.ConstrainCircular(GamingDeviceAxis.XAxis, GamingDeviceAxis.YAxis);
        GorgonPoint rightStick = controller.Device.ConstrainCircular(GamingDeviceAxis.RotationX, GamingDeviceAxis.RotationY);

        _labelText.Length = 0;

        GamingDeviceBatteryLevel battLevel = controller.Device.GetBatteryLevel();

        _labelText.AppendFormat("{0}", controller.Device.Info.Description);

        switch (battLevel)
        {
            case GamingDeviceBatteryLevel.Unknown:
            case GamingDeviceBatteryLevel.Disconnected:
                _labelText.AppendFormat(": Left Stick Position {0}x{1}. Right Stick Position {2}x{3}. Right Trigger Position {4}. Left Trigger Position {5}",
                    leftStick.X,
                    leftStick.Y,
                    rightStick.X,
                    rightStick.Y,
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger),
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.LeftTrigger));
                break;
            case GamingDeviceBatteryLevel.Wired:
                _labelText.AppendFormat(" (Wired): Left Stick Position {0}x{1}. Right Stick Position {2}x{3}. Right Trigger Position {4}. Left Trigger Position {5}",
                    leftStick.X,
                    leftStick.Y,
                    rightStick.X,
                    rightStick.Y,
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger),
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.LeftTrigger));
                break;
            default:
                _labelText.AppendFormat(" (Battery: {0}): Left Stick Position {1}x{2}. Right Stick Position {3}x{4}. Right Trigger Position {5}. Left Trigger Position {6}",
                    battLevel,
                    leftStick.X,
                    leftStick.Y,
                    rightStick.X,
                    rightStick.Y,
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger),
                    controller.Device.ConstrainLinearScaled(GamingDeviceAxis.LeftTrigger));
                break;
        }

        // Set the label information.
        label.Text = _labelText.ToString();
    }

    /// <summary>
    /// Function to draw a spray on the screen.
    /// </summary>
    /// <param name="controller">The active controller.</param>
    private void DrawSpray(Controller controller)
    {
        SprayCan state = controller.Spray;

        // Update the origin of the spray.
        state.Origin = controller.Position;

        GorgonRange<int> throttleRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RightTrigger].Range;
        GorgonRange<int> rightXStickRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RotationX].Range;
        GorgonRange<int> rightYStickRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RotationY].Range;
        GorgonPoint rightStickPos = controller.Device.ConstrainCircular(GamingDeviceAxis.RotationX, GamingDeviceAxis.RotationY);
        int throttleValue = controller.Device.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger);

        if (throttleValue <= 0)
        {
            _lastThrottle = 0;
            controller.Device.SetVibration(1, 0);
            state.IsActive = false;
            return;
        }

        int throttleDelta = throttleValue - _lastThrottle;

        if (throttleDelta > 5)
        {
            // Convert the throttle value to a unit value.
            float throttleUnit = throttleValue / (float)throttleRange.Range;

            // Set up the spray state.
            state.Position = state.Origin;
            state.Amount = throttleRange.Range;
            state.Time = GorgonRandom.RandomSingle(0.25f, 2.0f);
            state.VibrationAmount = controller.Device.Info.VibrationMotorRanges[1].Maximum;
            state.SprayAlpha = (throttleUnit * GorgonRandom.RandomSingle(128, 239.0f)) + 16;
            state.IsActive = true;
        }

        if (!state.IsActive)
        {
            controller.Device.SetVibration(1, 0);
            _lastThrottle = 0;
            return;
        }

        // Set the high freq. motor active on the controller.
        controller.Device.SetVibration(1, (int)state.VibrationAmount);

        // Update the spray state.
        state.Update(rightStickPos, rightXStickRange, rightYStickRange, controller.Device.Info.XInputSlot);
        _surface?.DrawPoint(state);

        _lastThrottle = throttleValue;
    }

    /// <summary>
    /// Function to draw the controller cursor.
    /// </summary>
    /// <param name="controller">The active controller..</param>
    private void DrawControllerCursor(Controller controller)
    {
        Debug.Assert(_surface is not null, "Surface was not created.");

        GorgonRange<int> xRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.XAxis].Range;
        GorgonRange<int> yRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.YAxis].Range;
        GorgonPoint cursorHalfSize = new(_surface.CursorSize.X / 2, _surface.CursorSize.Y / 2);

        // Get the color based on the controller index.			
        int playerColorValue = (int)(((uint)0xFF << (controller.Device.Info.XInputSlot * 8)) | 0xFF000000);

        GorgonPoint stickPosition = controller.Device.ConstrainCircular(GamingDeviceAxis.XAxis, GamingDeviceAxis.YAxis);

        // Normalize the X/Y axes for the left stick.
        Vector2 moveVector = new(stickPosition.X / (xRange.Range * 0.5f), stickPosition.Y / (yRange.Range * 0.5f));

        // Set up our cursor speed.
        float speed = panelDisplay.ClientSize.Width * 0.5f * GorgonTiming.Delta;
        Vector2 position = new((speed * moveVector.X) + controller.Position.X, (speed * moveVector.Y) + controller.Position.Y);

        // Limit the range of the positioning.
        if (position.X < -cursorHalfSize.X)
        {
            position = new Vector2(panelDisplay.ClientRectangle.Right + cursorHalfSize.X, position.Y);
        }

        if (position.Y < -cursorHalfSize.Y)
        {
            position = new Vector2(position.X, panelDisplay.ClientRectangle.Bottom + cursorHalfSize.Y);
        }

        if (position.X > panelDisplay.ClientRectangle.Right + cursorHalfSize.X)
        {
            position = new Vector2(-cursorHalfSize.X, position.Y);
        }

        if (position.Y > panelDisplay.ClientRectangle.Bottom + cursorHalfSize.Y)
        {
            position = new Vector2(position.X, -cursorHalfSize.Y);
        }

        // Draw our cursor.
        _surface.DrawCursor((GorgonPoint)position, GorgonColor.FromARGB(playerColorValue));

        // Update our global position.
        controller.Position = position;
    }

    /// <summary>
    /// Function to process idle time in the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
    private bool Idle()
    {
        Debug.Assert(_surface is not null, "Surface was not created.");

        if (_controllers.Count == 0)
        {
            labelMessage.Visible = true;
            return true;
        }
        else
        {
            labelMessage.Visible = false;
        }

        _input.GetInput(InputDeviceType.GamingDevice, _events);

        _surface.Clear(Color.White);

        for (int i = 0; i < _controllers.Count; i++)
        {
            Controller controller = _controllers[i];

            controller.Device.ParseData(_events);

            // Clear the drawing if the primary button is clicked.
            if (controller.Device.Buttons[0])
            {
                _surface.ClearDrawing();
            }

            // Draw the controller cursor.
            DrawControllerCursor(controller);

            // Begin drawing.	
            DrawSpray(controller);

            UpdateControllerLabels(controller);
        }

        _surface.Render();

        return true;
    }

    /// <summary>
    /// Function to enumerate and update the active controllers list.
    /// </summary>
    private void UpdateActiveControllers()
    {
        // Only look for XInput devices.
        foreach (IGorgonGamingDeviceInfo info in _input.GamingDevices.Take(3))
        {
            if (((info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
                || (_controllers.Any(c => c.Device.Info.DeviceID == info.DeviceID)))
            {
                continue;
            }

            IGorgonGamingDevice controller = new GorgonGamingDevice(info);
            _controllers.Add(new Controller(controller, new SprayCan())
            {
                Position = new Vector2(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
                                       GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64))
            });
        }
    }

    /// <summary>
    /// Function called when a device has been attached to the system.
    /// </summary>
    /// <param name="info">The device info.</param>
    private void DeviceAttached(IGorgonInputDeviceInfo info)
    {
        if ((info is not IGorgonGamingDeviceInfo deviceInfo)
            || ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
            || (_controllers.Count == 3))
        {
            return;
        }

        if (_controllers.FindIndex(d => d.Device.Info.DeviceID == deviceInfo.DeviceID) != -1)
        {
            return;
        }

        void AddDevice()
        {
            GorgonGamingDevice controller = new(deviceInfo);

            _controllers.Add(new Controller(controller, new SprayCan())
            {
                Position = new Vector2(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
                                       GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64))
            });
        }

        // Because the attached/detached events are called from another thread, we need to ensure we're synchronized with the main thread.
        if (InvokeRequired)
        {
            Invoke(AddDevice);
        }
        else
        {
            AddDevice();
        }
    }

    /// <summary>
    /// Function called when a device has been attached from the system.
    /// </summary>
    /// <param name="info">The device info.</param>
    private void DeviceDetached(IGorgonInputDeviceInfo info)
    {
        if ((info is not IGorgonGamingDeviceInfo deviceInfo)
            || ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
            || (_controllers.Count == 0))
        {
            return;
        }

        int deviceIndex = _controllers.FindIndex(d => d.Device.Info.DeviceID == deviceInfo.DeviceID);

        if (deviceIndex < 0)
        {
            return;
        }

        void RemoveDevice()
        {
            Panel? panel = (Panel?)panelControllers.Controls["panelController" + deviceInfo.XInputSlot];

            if (panel is not null)
            {
                panel.Visible = false;
            }

            _controllers.Remove(_controllers[deviceIndex]);
        }

        // Because the attached/detached events are called from another thread, we need to ensure we're synchronized with the main thread.
        if (InvokeRequired)
        {
            Invoke(RemoveDevice);
        }
        else
        {
            RemoveDevice();
        }
    }

    /// <summary>
    /// Raises the <see cref="Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            // Get our list of active controllers.
            UpdateActiveControllers();

            // Get the graphics interface for our panel.
            _surface = new DrawingSurface(panelDisplay);

            // Set up our idle loop.
            GorgonApplication.IdleMethod += Idle;

            // Set up our input to handle connecting and disconnecting controllers.
            _input.RegisterDeviceChangeCallbacks(DeviceAttached, DeviceDetached);
        }
        catch (Exception ex)
        {
            // We do this here instead of just calling the dialog because this
            // function will send the exception to the Gorgon log file.
            ex.Handle(e => GorgonDialogs.ErrorBox(this, e), GorgonApplication.Log);
            GorgonApplication.Quit();
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        _input.Dispose();
        _events.Dispose();

        _surface?.Dispose();
        _surface = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Form" /> class.
    /// </summary>
    public Form()
    {
        InitializeComponent();

        _input = GorgonInput.CreateInput(InputFlags.GamingDevices, GorgonApplication.Log);
    }
}
