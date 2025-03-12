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
using Gorgon.UI.OLDE;
using Gorgon.Input;
using Gorgon.Input.Devices;

namespace Gorgon.Examples;

/// <summary>
/// Our main form for the example
/// </summary>
/// <remarks>
/// This example is exclusively for the XInput controller (although it could be made to work with a regular controller).  
/// 
/// Like the Input sample, we create the interface to the input system, but instead of using keyboard, mouse and any 
/// available gaming device, we filter only for XInput compatible controllers.
/// 
/// This example shows how to use vibration and how to make a pressure sensitive trigger on the XInput controllers.
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
    // The synchronization context used to synchronize other threads with the UI thread.
    private SynchronizationContext? _syncContext;

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
        SprayCan spray = controller.Spray;

        // Update the origin of the spray.
        spray.Origin = controller.Position;

        // We'll need the ranges to determine how far and for how long we should spray.
        GorgonRange<int> throttleRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RightTrigger].Range;
        GorgonRange<int> rightXStickRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RotationX].Range;
        GorgonRange<int> rightYStickRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.RotationY].Range;

        // Again, get the position of the stick. Except here, we're grabbing from the right stick axes. 
        // We still apply a deadzone so that we don't get every little wobble.
        GorgonPoint rightStickPos = controller.Device.ConstrainCircular(GamingDeviceAxis.RotationX, GamingDeviceAxis.RotationY);
        // Here we're using a linear scale because the trigger axis is the only axis we're using. The scaled 
        // function just maps the values from 0 to Max Axis Value after the trigger leaves the dead zone.
        int triggerValue = controller.Device.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger);

        // If we've released the throttle, then we should end the effect.
        if (triggerValue <= 0)
        {
            _lastThrottle = 0;
            controller.Device.SetVibration(1, 0);
            spray.IsActive = false;
            return;
        }

        // We will only update the spray information if we've changed trigger position by a predefined amount.
        int triggerDelta = triggerValue - _lastThrottle;

        if (triggerDelta > 5)
        {
            // Convert the throttle value to a unit value.
            float throttleUnit = triggerValue / (float)throttleRange.Maximum;

            // Set up the spray state.
            spray.Position = spray.Origin;
            spray.Amount = throttleRange.Maximum;
            spray.Time = GorgonRandom.RandomSingle(0.25f, 2.0f);
            spray.VibrationAmount = controller.Device.Info.VibrationMotorRanges[1].Maximum;
            spray.SprayAlpha = (throttleUnit * GorgonRandom.RandomSingle(128, 239.0f)) + 16;
            spray.IsActive = true;
        }

        if (!spray.IsActive)
        {
            controller.Device.SetVibration(1, 0);
            return;
        }

        // Set the high freq. motor active on the controller.
        controller.Device.SetVibration(1, (int)spray.VibrationAmount);

        // Now we can calculate the spray direction.
        Vector2 direction = new(rightStickPos.X / (float)rightXStickRange.Maximum, rightStickPos.Y / (float)rightYStickRange.Maximum);

        // Update the spray state.
        spray.Update(direction);
        _surface?.DrawPoint(spray);

        _lastThrottle = triggerValue;
    }

    /// <summary>
    /// Function to draw the controller cursor.
    /// </summary>
    /// <param name="controller">The active controller..</param>
    private void DrawControllerCursor(Controller controller)
    {
        Debug.Assert(_surface is not null, "Surface was not created.");

        // This is the extent of motion for the X and Y axis on the controller.
        // This maps to the left stick on an XBox controller.
        GorgonRange<int> xRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.XAxis].Range;
        GorgonRange<int> yRange = controller.Device.Info.AxisInfo[GamingDeviceAxis.YAxis].Range;

        // When handling joystick/gamepad data, it's pretty important to constrain the axis values to a dead zone.
        // If we don't do this, the stick will give us every little slight vibration.
        GorgonPoint stickPosition = controller.Device.ConstrainCircular(GamingDeviceAxis.XAxis, GamingDeviceAxis.YAxis);

        // We'll now get the vector in which we should move the cursor. We do this by normalizing the current 
        // left stick value with the maximum value that it can have. 
        Vector2 moveVector = new(stickPosition.X / (float)xRange.Maximum, stickPosition.Y / (float)yRange.Maximum);

        // Set up our cursor speed.
        float speed = panelDisplay.ClientSize.Width * 0.5f * GorgonTiming.Delta;        
        GorgonRectangleF rect = panelDisplay.DisplayRectangle;

        // Lock the cursor position to the display area.
        Vector2 position = rect.Clamp(new Vector2(((speed * moveVector.X) + controller.Position.X), 
                                                        ((speed * moveVector.Y) + controller.Position.Y)));

        // Draw our cursor.
        _surface.DrawCursor((GorgonPoint)position, controller.Spray.SprayColor);

        // Update our global position.
        controller.Position = position;
    }

    /// <summary>
    /// Function to process idle time in the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
    private bool Idle()
    {
        // Since we're using nullable refs, we need to assure the compiler that this is not 
        // null upon entry.
        Debug.Assert(_surface is not null, "Surface was not created.");

        // If we don't have any XInput controllers attached to the system, then 
        // we'll notify the user by displaying the label that tells them to 
        // connect an XInput controller.
        if (_controllers.Count == 0)
        {
            labelMessage.Visible = true;
            return true;
        }


        labelMessage.Visible = false;

        // This is where we'll pull in the data gathered by the input system for our gaming devices that
        // we're monitoring and store it in the event buffer.
        _input.GetInput(InputDeviceType.GamingDevice, _events);

        _surface.Clear(Color.White);

        for (int i = 0; i < _controllers.Count; i++)
        {
            Controller controller = _controllers[i];

            // Take the input event buffer and parse its data to the controller we're using.
            // This allows us to know the current state of the controller at the time of 
            // the data capture.
            controller.Device.ParseData(_events);

            // This will clear the drawing if the primary button is clicked.
            // This will typically map to the "A" button.
            if (controller.Device.Buttons[0])
            {
                _surface.ClearDrawing();
            }

            DrawControllerCursor(controller);
            DrawSpray(controller);
            UpdateControllerLabels(controller);
        }

        _surface.Render();

        return true;
    }

    /// <summary>
    /// Function to retrieve the colour value based on an XInput slot index.
    /// </summary>
    /// <param name="index">The index of the controller.</param>
    /// <returns>The color to assign.</returns>
    private GorgonColor GetControllerColor(int index)
    {
        // Here we get the color based on the controller index by shifting it into either the Red, Green or Blue
        // (now you see why we limit to 3 controllers) values.
        if (index is >= 0 and < 3)
        {
            return GorgonColor.FromARGB((int)(((uint)0xFF << (index << 3)) | 0xFF000000));
        }

        return GorgonColors.BlackTransparent;
    }

    /// <summary>
    /// Function to enumerate and update the active controllers list.
    /// </summary>
    private void UpdateActiveControllers()
    {
        // From here we'll enumerate the list of known gaming devices. While XInput controllers can support up to 4 controllers
        // at the same time, we only take up to 3 because we're only supporting 3 controllers in this example. 
        //
        // We also apply a filter to the list to only give us XInput compatible controllers.
        foreach (IGorgonGamingDeviceInfo info in _input.GamingDevices
                                                       .Where(i => (i.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) == GamingDeviceCapabilityFlags.IsXInputDevice))
        {
            // Skip any potential duplicates (this is extremely unlikely, but paranoia rules the day).            
            if (_controllers.Any(c => c.Device.Info.DeviceID == info.DeviceID))
            {
                continue;
            }

            // Create a new controller record so that we can link a spray can object and position to it.
            IGorgonGamingDevice controller = new GorgonGamingDevice(info);
            _controllers.Add(new Controller(controller, new SprayCan(GetControllerColor(info.XInputSlot)))
            {
                Position = new Vector2(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
                                       GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64))
            });

            // Ensure we're only using 3 XInput controllers for this example (for this example only).
            if (_controllers.Count == 3)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Function called when a device has been attached to the system.
    /// </summary>
    /// <param name="info">The device info.</param>
    private void DeviceAttached(IGorgonInputDeviceInfo info)
    {
        // Since we're only interested in gaming devices. Filter for those, and cast the info to a gaming device info so we have
        // the information we need. We're specifically interested in XInput devices, so filter for that as well.
        // While you can have 4 XInput controllers available at a time, we've limited this sample to 3 because we can.
        if ((info is not IGorgonGamingDeviceInfo deviceInfo)
            || ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
            || (_controllers.Count == 3))
        {
            return;
        }

        // Find the controller object associated with the info. 
        // To do this, we use the device ID which is a GUID so the device can be uniquely identified. This GUID is also stable, meaning that it does not change 
        // across application restarts and can be stored for later use. The exception to this is if the device has been plugged into a different USB port.
        if (_controllers.FindIndex(d => d.Device.Info.DeviceID == deviceInfo.DeviceID) != -1)
        {
            return;
        }

        // This is a local function to execute on the main thread. It'll add the new controller 
        // instance to the controller list.
        void AddDevice(object? _)
        {
            GorgonGamingDevice controller = new(deviceInfo);

            _controllers.Add(new Controller(controller, new SprayCan(GetControllerColor(deviceInfo.XInputSlot)))
            {
                Position = new Vector2(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
                                       GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64))
            });
        }

        // Our callbacks are always run on the background thread that does the message processing, and as such if we want to update the UI, we need to 
        // call on the main thread. To do this, we synchronize a call using the synchronization context we stored in OnLoad.
        _syncContext?.Send(AddDevice, null);
    }

    /// <summary>
    /// Function called when a device has been attached from the system.
    /// </summary>
    /// <param name="info">The device info.</param>
    private void DeviceDetached(IGorgonInputDeviceInfo info)
    {
        // Since we're only interested in gaming devices. Filter for those, and cast the info to a gaming device info so we have
        // the information we need. We're specifically interested in XInput devices, so filter for that as well.
        if ((info is not IGorgonGamingDeviceInfo deviceInfo)
            || ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
            || (_controllers.Count == 0))
        {
            return;
        }

        // Find the controller object associated with the info. 
        // To do this, we use the device ID which is a GUID so the device can be uniquely identified. This GUID is also stable, meaning that it does not change 
        // across application restarts and can be stored for later use. The exception to this is if the device has been plugged into a different USB port.
        int deviceIndex = _controllers.FindIndex(d => d.Device.Info.DeviceID == deviceInfo.DeviceID);

        if (deviceIndex < 0)
        {
            return;
        }

        // This is a local function to execute on the main thread. It'll remove the controller instance
        // from the controller list, and update the display.
        void RemoveDevice(object? _)
        {
            Panel? panel = (Panel?)panelControllers.Controls["panelController" + deviceInfo.XInputSlot];

            if (panel is not null)
            {
                panel.Visible = false;
            }

            _controllers.Remove(_controllers[deviceIndex]);
        }

        // Because the attached/detached events are called from another thread, we need to ensure we're synchronized with the main thread.
        _syncContext?.Send(RemoveDevice, null);
    }

    /// <summary>
    /// Raises the <see cref="System.Windows.Forms.Form.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    private void Form_Load(object sender, EventArgs e)
    {
        try
        {
            // Store the current synchronization context so we can make UI 
            // calls in our callbacks.
            _syncContext = SynchronizationContext.Current;

            // First, let's grab our XInput controllers from the input system.
            UpdateActiveControllers();

            // Get the graphics interface for our panel.
            _surface = new DrawingSurface(panelDisplay);

            // Register our callback methods so that we can intercept when XInput
            // devices have been added to or removed from the computer.
            _input.RegisterDeviceChangeCallbacks(DeviceAttached, DeviceDetached);

            // Set up our idle loop.
            GorgonApplication.IdleMethod = Idle;
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

        // Always dispose the input system. This will disable its background thread and restore our input back to normal.
        _input.Dispose();
        // Event buffers must be disposed, otherwise resources may be leaked.
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

        // This is our input system. By passing in the device types in the flags, we can tell the system to immediately start working with 
        // the devices specified. 
        //
        // In this example, we're only interested in XInput gaming devices, so we only bind with gaming devices (we filter for XInput later).
        _input = GorgonInput.CreateInput(InputFlags.GamingDevices, GorgonApplication.Log);
    }
}
