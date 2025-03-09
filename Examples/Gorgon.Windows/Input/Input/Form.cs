// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: March 4, 2025 4:32:05 PM
//

using System.Diagnostics;
using System.Numerics;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;
using Gorgon.Windows.Input;
using Gorgon.Windows.Input.Devices;
using InputMouseButtons = Gorgon.Windows.Input.Devices.MouseButtons;

namespace Gorgon.Examples;

/// <summary>
/// Our main form for the example
/// </summary>
/// <remarks>
/// This example shows how to use Gorgon's Input functionality to control the mouse, keyboard, joysticks, or gamepads. 
/// 
/// The input interface gives low level access to Human Interface Devices such as a keyboard, mouse and gaming devices. 
/// It will also allow use of individual mice and keyboards if there is more than one of these devices attached to your 
/// system. This has the advantage of enabling users to use, for example, 2 mice that perform different functions in 
/// your application.
/// 
/// The input interface waits for messages from the operating system on a background thread, and those messages are 
/// queued to be read by the application. This queue only holds the last 1/2 second worth of data since the last read. 
/// 
/// Users can then poll for this data by requesting the snapshot from the input system via the GetInput method which 
/// will populate a buffer with input event data. This event data can then passed to a IGorgonKeyboard, IGorgonMouse or 
/// IGorgonGamingDevice type to be parsed and then be used to interpret the event data retrieved from the input 
/// interface for the specific device type.
/// 
/// If you're curious about the "BufferContext" stuff, that's all from GDI+ (System.Drawing) and allows us to set up a 
/// double buffer scenario so our mouse cursor can be drawn without flicker.  But that is not the scope of this example
/// </remarks>
public partial class Form
    : System.Windows.Forms.Form
{
    // The spray effect and its settings.
    private Spray? _spray;
    private Vector2 _spraySize = new();
    private SprayAction _sprayAction = SprayAction.Random;
    private float _alpha = 1.0f;
    // Our mouse cursor.
    private MouseCursor? _cursor;
    private Vector2 _cursorPosition;
    // Our input service.
    private readonly IGorgonInput _input;
    // Our mouse interface.
    private readonly IGorgonMouse _mouse = new GorgonMouse();
    // Our keyboard interface.
    private readonly IGorgonKeyboard _keyboard = new GorgonKeyboard();
    // Our gaming device interfaces.
    private readonly List<IGorgonGamingDevice> _gameDevices = [];
    private IGorgonGamingDevice? _activeGameDevice;
    private readonly Dictionary<GamingDeviceAxis, int> _lastThrottle = [];
    // Mouse positions.
    private GorgonPoint _lastMouse;
    private int _lastVWheel;
    private int _lastHWheel;
    // Our buffer for our input events.
    private readonly GorgonInputEventBuffer _events = new();
    // Text buffer for the gaming device display label.
    private readonly StringBuilder _gamingDisplayText = new();
    private readonly StringBuilder _buttonDisplayText = new();
    // Text buffer for the mouse device display label.
    private readonly StringBuilder _mouseDisplayText = new();
    // Text buffer for the keyboard device display label.
    private readonly StringBuilder _keyboardDisplayText = new();
    // Flag used to enable or disable the help screen.
    private bool _helpKeyPressed;
    // The current synchronization context for the application.
    private SynchronizationContext? _currentSyncContext;

    /// <summary>
    /// Property to return the mid point of the display panel size, in screen space.
    /// </summary>
    private GorgonPoint DisplayHalfSize => new(PanelDisplay.ClientSize.Width / 2, PanelDisplay.ClientSize.Height / 2);

    /// <summary>
    /// Function to update the size of the spray nozzle.
    /// </summary>
    /// <param name="amount">The amount to increase or decrease the size.</param>
    private void UpdateSprayNozzleSize(float amount) => _spraySize = new Vector2((_spraySize.X + amount).Min(30).Max(1), (_spraySize.Y + amount).Min(30).Max(1));

    /// <summary>
    /// Function to update the alpha value for the brushes.
    /// </summary>
    /// <param name="amount">The amount to update the alpha value.</param>
    private void UpdateBrushAlpha(float amount, bool relative)
    {
        if (relative)
        {
            _alpha += amount;
        }
        else
        {
            _alpha = amount;
        }

        _alpha = _alpha.Max(0).Min(1);
    }

    /// <summary>
    /// Function to update the display controls for the gaming device.
    /// </summary>
    private void UpdateGamingDeviceDisplay()
    {
        TableJoysticks.Visible = FlowGamingDevices.Visible = _input.GamingDevices.Count > 0;

        if (_activeGameDevice is null)
        {
            LabelGamingDevice.Text = "No active gaming device. Hit a button, or move a stick.";
            return;
        }

        _activeGameDevice.ParseData(_events);

        _gamingDisplayText.Length = 0;

        _gamingDisplayText.AppendFormat("{0}. ", _activeGameDevice.Info.Description);

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.XAxis))
        {
            _gamingDisplayText.AppendFormat("X: {0:0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.XAxis));
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.YAxis))
        {
            _gamingDisplayText.AppendFormat(" Y: {0:0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.YAxis));
        }

        if ((_activeGameDevice.Axes.Contains(GamingDeviceAxis.ZAxis)) && ((_activeGameDevice.Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice))
        {
            _gamingDisplayText.AppendFormat(" Z: {0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.ZAxis));
        }

        if (_gamingDisplayText.Length > 0)
        {
            _gamingDisplayText.Append(", ");
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.RotationX))
        {
            _gamingDisplayText.AppendFormat("rX: {0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.RotationX));
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.RotationY))
        {
            _gamingDisplayText.AppendFormat(" rY: {0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.RotationY));
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.RotationZ))
        {
            _gamingDisplayText.AppendFormat(" rZ: {0}", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.RotationZ));
        }

        if (_gamingDisplayText.Length > 0)
        {
            _gamingDisplayText.Append(", ");
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.LeftTrigger))
        {
            _gamingDisplayText.AppendFormat("LT: {0} ", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.LeftTrigger));
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.RightTrigger))
        {
            _gamingDisplayText.AppendFormat("RT: {0} ", _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger));
        }

        if (_activeGameDevice.Axes.Contains(GamingDeviceAxis.Throttle))
        {
            _gamingDisplayText.AppendFormat("Throttle: {0} ", _activeGameDevice.Axes[GamingDeviceAxis.Throttle].Value);
        }

        for (int i = 0; i < _activeGameDevice.PovDirections.Length; ++i)
        {
            _gamingDisplayText.AppendFormat("POV #{0}: {1} ", i, _activeGameDevice.PovDirections[i]);
        }

        _gamingDisplayText.Append("Buttons: ");

        _buttonDisplayText.Length = 0;

        for (int i = 0; i < _activeGameDevice.Buttons.Length; ++i)
        {
            if (_activeGameDevice.Buttons[i])
            {
                if (_buttonDisplayText.Length > 0)
                {
                    _buttonDisplayText.Append(", ");
                }

                _buttonDisplayText.Append(i);
            }
        }

        if (_buttonDisplayText.Length > 0)
        {
            _gamingDisplayText.Append(_buttonDisplayText);
        }
        else
        {
            _gamingDisplayText.Append("None");
        }
    }

    /// <summary>
    /// Function to process during application idle time.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        _sprayAction = SprayAction.None;

        // Since we're using nullable refs, we need to assure the compiler that this is not 
        // null upon entry.
        Debug.Assert(_spray is not null, "Application has not initialized fully.");

        _input.GetInput(InputDeviceType.Mouse | InputDeviceType.Keyboard | InputDeviceType.GamingDevice, _events);

        HandleGamingDevices();
        HandleMouseInput();
        if (!HandleKeyboard())
        {
            return false;
        }

        // If we' only using the keyboard at this point, no need to move on.
        // This is because we turned off input using the End key.
        if (_input.Devices == InputFlags.Keyboard)
        {
            return true;
        }

        _cursorPosition = GorgonRectangleF.Clamp(new(0, 0, PanelDisplay.Width, PanelDisplay.Height), _cursorPosition);

        GorgonPoint sprayPosition = (GorgonPoint)_cursorPosition;
        _spray.SpraySize = (GorgonPoint)_spraySize;

        GorgonColor color = _spray.PerformAction(sprayPosition, _sprayAction, _alpha) ?? new GorgonColor(GorgonColors.Black, _alpha);

        // Display the mouse cursor.	        		
        _cursor?.DrawMouseCursor(sprayPosition, _spray.SpraySize, color, _sprayAction, _spray.Surface);

        return true;
    }

    /// <summary>
    /// Function to update the gaming input device information.
    /// </summary>
    private void HandleGamingDevices()
    {
        UpdateGamingDeviceDisplay();

        // If there are no mice available then there's nothing to handle, 
        // so we should bail now.
        // Also, we're checking the available axes. The odds of a gamepad
        // and joystick not having an X/Y axis is near zero, but whatever.
        if ((_activeGameDevice is null)
            || (!_activeGameDevice.Info.AxisInfo.TryGetValue(GamingDeviceAxis.XAxis, out GorgonGamingDeviceAxisInfo? xInfo))
            || (!_activeGameDevice.Info.AxisInfo.TryGetValue(GamingDeviceAxis.YAxis, out GorgonGamingDeviceAxisInfo? yInfo)))
        {
            return;
        }

        // When handling joystick/gamepad data, it's pretty important to constrain the axis values to a dead zone.
        // If we don't do this, the stick will give us every little slight vibration.
        Vector2 delta = _activeGameDevice.ConstrainCircular(GamingDeviceAxis.XAxis, GamingDeviceAxis.YAxis);

        // We'll scale this value to a unit (-1 .. 1) value and then use it map the cursor position.
        delta = new Vector2(delta.X / xInfo.Range.Maximum, delta.Y / yInfo.Range.Maximum);

        // Finally, we'll shift the cursor position by the display size per second.
        _cursorPosition = new Vector2(_cursorPosition.X + (delta.X * PanelDisplay.ClientSize.Width * GorgonTiming.Delta),
                                      _cursorPosition.Y + (delta.Y * PanelDisplay.ClientSize.Height * GorgonTiming.Delta));

        // When using axes, or point of view hats, etc... we should ensure those items are available 
        // on the device.
        if (_activeGameDevice.Povs.Length >= 1)
        {
            // In this case, use the POV hat to change the size of the brush by 30 units/second.
            POVDirection pov = _activeGameDevice.PovDirections[0] & (POVDirection.Up | POVDirection.Down);
            float scaleDelta = GorgonTiming.Delta * 30f;

            switch (pov)
            {
                case POVDirection.Up:
                    UpdateSprayNozzleSize(scaleDelta);
                    break;
                case POVDirection.Down:
                    UpdateSprayNozzleSize(-scaleDelta);
                    break;
            }
        }

        int maxThrottle = 1;
        int currentThrottle = 0;
        GamingDeviceAxis alphaAxis = GamingDeviceAxis.None;

        // If we have a throttle, we'll use the throttle set the opacity of the brush.
        if ((_activeGameDevice.Info.Capabilities & GamingDeviceCapabilityFlags.SupportsThrottle) == GamingDeviceCapabilityFlags.SupportsThrottle)
        {
            currentThrottle = _activeGameDevice.Axes[GamingDeviceAxis.Throttle].Value;
            maxThrottle = _activeGameDevice.Info.AxisInfo[GamingDeviceAxis.Throttle].Range.Maximum;
            alphaAxis = GamingDeviceAxis.Throttle;
        } // Otherwise, if it's an Xbox controller, we'll use the left trigger to set the opacity of the brush with the left trigger, and the
          // right trigger will produce random colours.
        else if ((_activeGameDevice.Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) == GamingDeviceCapabilityFlags.IsXInputDevice)
        {
            maxThrottle = _activeGameDevice.Info.AxisInfo[GamingDeviceAxis.LeftTrigger].Range.Maximum;
            currentThrottle = maxThrottle - _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.LeftTrigger);
            alphaAxis = GamingDeviceAxis.LeftTrigger;

            int rTrigger = _activeGameDevice.ConstrainLinearScaled(GamingDeviceAxis.RightTrigger);

            if (rTrigger > 0)
            {
                _sprayAction = SprayAction.Random;
            }
        }

        bool hasThrottle;

        // Check for a change in the throttle/trigger value. If we don't have a change, then don't bother 
        // setting an alpha value for the brush.
        if (_lastThrottle.TryGetValue(alphaAxis, out int lastThrottle))
        {
            hasThrottle = (currentThrottle - lastThrottle) != 0;
        }
        else
        {
            hasThrottle = currentThrottle > 0;
        }

        _lastThrottle[alphaAxis] = currentThrottle;

        if (hasThrottle)
        {
            UpdateBrushAlpha((float)currentThrottle / maxThrottle, false);
        }

        for (int i = 0; i < _activeGameDevice.Buttons.Length; ++i)
        {
            bool state = _activeGameDevice.Buttons[i];

            // If the button is not down, then we can leave, and ensure we don't leave the motors active.
            if (!state)
            {
                if ((_activeGameDevice.Info.Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) == GamingDeviceCapabilityFlags.SupportsVibration)
                {
                    _activeGameDevice.SetVibration(0, 0);
                    _activeGameDevice.SetVibration(1, 0);
                }
                continue;
            }
            else
            {
                // If the device is an XInput compatible device, set the vibration motors active if we've held a button.
                if ((_activeGameDevice.Info.Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) == GamingDeviceCapabilityFlags.SupportsVibration)
                {
                    _activeGameDevice.SetVibration(0, 16384);
                    _activeGameDevice.SetVibration(1, 16384);
                }
            }

            switch (i)
            {
                case 0:
                    _sprayAction = SprayAction.Brush1;
                    break;
                case 1:
                    _sprayAction = SprayAction.Brush2;
                    break;
                case 2:
                    _sprayAction = SprayAction.Brush3;
                    break;
                case 3:
                    _sprayAction = SprayAction.Brush4;
                    break;
                case 4:
                    _sprayAction = SprayAction.Brush5;
                    break;
                case 5:
                    _sprayAction = SprayAction.Brush6;
                    break;
                case 6:
                    _sprayAction = SprayAction.Brush7;
                    break;
                case 7:
                    _sprayAction = SprayAction.Brush8;
                    break;
                case 8:
                    _sprayAction = SprayAction.Brush9;
                    break;
                case 9:
                    _sprayAction = SprayAction.Brush10;
                    break;
                case 10:
                    _sprayAction = SprayAction.Brush11;
                    break;
                case 11:
                    _sprayAction = SprayAction.Brush12;
                    break;
                case 12:
                    _sprayAction = SprayAction.Brush13;
                    break;
                case 13:
                    _sprayAction = SprayAction.Brush14;
                    break;
                case 14:
                    _sprayAction = SprayAction.Brush15;
                    break;
                case 15:
                    _sprayAction = SprayAction.Erase;
                    break;
            }
        }

        LabelGamingDevice.Text = _gamingDisplayText.ToString();
    }

    /// <summary>
    /// Function to update the mouse information.
    /// </summary>
    private void HandleMouseInput()
    {
        // If there are no mice available then there's nothing to handle, 
        // so we should bail now.
        if (_input.Mice.Count == 0)
        {
            FlowMouse.Visible = false;
            return;
        }

        // When the mouse is active (i.e. the application has focus, and we didn't press END on the keyboard), we 
        // need to constrain the cursor to the display area so it doesn't fly out of the window. There are several 
        // ways to do this, but ideally we should lock the cursor position like so:
        Cursor.Position = PointToScreen(DisplayHalfSize);

        _mouse.ParseData(_events);

        // This allows us to use the vertical mouse wheel to increase/decrease the size of the button and the 
        // horizontal scroll to alter the opacity of the brush.
        // We capture the delta here so that we can detect which way the the change should go (positive or
        // negative).
        int vwheelDelta = _mouse.RelativeVerticalWheel - _lastVWheel;
        int hwheelDelta = _mouse.RelativeHorizontalWheel - _lastHWheel;

        if (vwheelDelta > 0)
        {
            UpdateSprayNozzleSize(1);
        }
        else if (vwheelDelta < 0)
        {
            UpdateSprayNozzleSize(-1);
        }

        if (hwheelDelta < 0)
        {
            UpdateBrushAlpha(-GorgonTiming.Delta * 4, true);
        }
        else if (hwheelDelta > 0)
        {
            UpdateBrushAlpha(GorgonTiming.Delta * 4, true);
        }

        // The Gorgon Input mouse works by accumulating all of the changes in the cursor position over several 
        // inputs. This ensure we don't miss an input and we have an accurate position on the mouse. However, 
        // since the mouse does not have a coordinate system, we need to track the delta between the current 
        // position and the last position so we can update our cursor and brush position.
        GorgonPoint posDelta = new(_mouse.Position.X - _lastMouse.X, _mouse.Position.Y - _lastMouse.Y);

        _cursorPosition = new Vector2(_cursorPosition.X + posDelta.X, _cursorPosition.Y + posDelta.Y);

        // Here we read modifier keys so we can provide alternative functionality 
        // when we press a button.
        ReadOnlySpan<VirtualKeys> keys = _keyboard.GetModifiers();

        bool hasControlKey = keys.Contains(VirtualKeys.LeftControl);
        bool hasShiftKey = keys.Contains(VirtualKeys.LeftShift);

        _lastMouse = _mouse.Position;
        _lastVWheel = _mouse.RelativeVerticalWheel;
        _lastHWheel = _mouse.RelativeHorizontalWheel;

        // This should be obvious.
        switch (_mouse.Buttons)
        {
            case InputMouseButtons.Button1 when hasShiftKey:
                _sprayAction = SprayAction.Brush6;
                break;
            case InputMouseButtons.Button2 when hasShiftKey:
                _sprayAction = SprayAction.Brush7;
                break;
            case InputMouseButtons.Button3 when hasShiftKey:
                _sprayAction = SprayAction.Brush8;
                break;
            case InputMouseButtons.Button4 when hasShiftKey:
                _sprayAction = SprayAction.Brush9;
                break;
            case InputMouseButtons.Button5 when hasShiftKey:
                _sprayAction = SprayAction.Brush10;
                break;
            case InputMouseButtons.Button1 when hasControlKey:
                _sprayAction = SprayAction.Brush11;
                break;
            case InputMouseButtons.Button2 when hasControlKey:
                _sprayAction = SprayAction.Brush12;
                break;
            case InputMouseButtons.Button3 when hasControlKey:
                _sprayAction = SprayAction.Brush13;
                break;
            case InputMouseButtons.Button4 when hasControlKey:
                _sprayAction = SprayAction.Brush14;
                break;
            case InputMouseButtons.Button5 when hasControlKey:
                _sprayAction = SprayAction.Brush15;
                break;
            case InputMouseButtons.Button1:
                _sprayAction = SprayAction.Brush1;
                break;
            case InputMouseButtons.Button2:
                _sprayAction = SprayAction.Brush2;
                break;
            case InputMouseButtons.Button3:
                _sprayAction = SprayAction.Brush3;
                break;
            case InputMouseButtons.Button4:
                _sprayAction = SprayAction.Brush4;
                break;
            case InputMouseButtons.Button5:
                _sprayAction = SprayAction.Brush5;
                break;
        }

        _mouseDisplayText.Length = 0;
        _mouseDisplayText.AppendFormat("X: {0} Y: {1} (Raw X: {2} Y: {3}), VWheel: {4}, HWheel: {5}, Buttons: {6}", _cursorPosition.X, _cursorPosition.Y,
            _mouse.Position.X, _mouse.Position.Y,
            _mouse.RelativeVerticalWheel, _mouse.RelativeHorizontalWheel,
            _mouse.Buttons);

        LabelMouse.Text = _mouseDisplayText.ToString();

        if (!FlowMouse.Visible)
        {
            FlowMouse.Visible = true;
        }
    }

    /// <summary>
    /// Function to update the keyboard information.
    /// </summary>
    /// <returns><b>true</b> if the user hit escape, <b>false</b> if not.</returns>
    private bool HandleKeyboard()
    {
        // If there are no keyboards available then there's nothing to handle, 
        // so we should bail now.
        if (_input.Keyboards.Count == 0)
        {
            FlowKeyboard.Visible = false;
            return true;
        }

        // This is where we get our actual information for the device state.        
        // The keyboard object will be populated with the most current event 
        // state for the device.
        _keyboard.ParseData(_events);

        // If we disable the input system by pressing End, 
        // then only the keyboard will be enabled, so we'll 
        // only process the escape key in that case.
        if (_input.Devices == InputFlags.Keyboard)
        {
            FlowKeyboard.Visible = false;
            return !_keyboard[VirtualKeys.Escape];
        }

        // We have several ways of retrieving the data from the keyboard. This
        // allows us to capture a snapshot of the current keyboard state and 
        // work with it.
        ReadOnlySpan<VirtualKeys> key = _keyboard.GetPressedKeys();
        ReadOnlySpan<VirtualKeys> modKeys = _keyboard.GetModifiers();

        VirtualKeys modifiers = VirtualKeys.None;

        // We'll use these flags for changing our input.
        // We store them here so we only have to check 1 time.
        bool hasCtrlModifier = false;
        bool hasShiftModifier = false;

        for (int i = 0; i < modKeys.Length; ++i)
        {
            switch (modKeys[i])
            {
                case VirtualKeys.ControlModifier:
                    hasCtrlModifier = true;
                    break;
                case VirtualKeys.ShiftModifier:
                    hasShiftModifier = true;
                    break;
            }

            // This is typically for display purposes only.
            // OR'ing the mod keys is not recommended for logic purposes.
            modifiers |= modKeys[i];
        }

        _keyboardDisplayText.Length = 0;
        _keyboardDisplayText.Append("Key Pressed: ");

        Vector2 offset = new(GorgonTiming.Delta * DisplayHalfSize.X * 0.125f, GorgonTiming.Delta * DisplayHalfSize.Y * 0.125f);

        // By using our mod keys, we can change the speed of the cursor movement
        // when we use the direction keys.
        float moveAccel = 1.0f;

        if (hasCtrlModifier)
        {
            moveAccel += 10.0f;
        }

        if (hasShiftModifier)
        {
            moveAccel += 5.0f;
        }

        // Since we're using polling, key state is not as deterministic as key down and up events. 
        // This will allow us to actively toggle the help view once the F1 is pressed and released. 
        // If we don't do this, the help will flicker on and off very quickly because the system 
        // will update too fast.
        if (_helpKeyPressed)
        {
            if (!key.Contains(VirtualKeys.F1))
            {
                TableHelp.Visible = !TableHelp.Visible;
                _helpKeyPressed = false;
            }
        }

        for (int i = 0; i < key.Length; ++i)
        {
            // Show the character for the key (and applicable modifiers), if one exists.
            // This function will take our modifier that we built earlier and apply it 
            // to the key character. 
            string character = _keyboard.KeyToCharacter(key[i], modifiers);

            if (string.IsNullOrWhiteSpace(character))
            {
                _keyboardDisplayText.Append(key[i].ToString()).Append(" ");
            }
            else
            {
                _keyboardDisplayText.Append(key[i].ToString()).Append(" (").Append(character).Append(") ");
            }

            // This should be obvious as to what it does. 
            switch (key[i])
            {
                case VirtualKeys.Escape:
                    return false;
                case VirtualKeys.One when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush9;
                    break;
                case VirtualKeys.One:
                    _sprayAction = SprayAction.Brush1;
                    break;
                case VirtualKeys.Two when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush10;
                    break;
                case VirtualKeys.Two:
                    _sprayAction = SprayAction.Brush2;
                    break;
                case VirtualKeys.Three when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush11;
                    break;
                case VirtualKeys.Three:
                    _sprayAction = SprayAction.Brush3;
                    break;
                case VirtualKeys.Four when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush12;
                    break;
                case VirtualKeys.Four:
                    _sprayAction = SprayAction.Brush4;
                    break;
                case VirtualKeys.Five when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush13;
                    break;
                case VirtualKeys.Five:
                    _sprayAction = SprayAction.Brush5;
                    break;
                case VirtualKeys.Six when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush14;
                    break;
                case VirtualKeys.Six:
                    _sprayAction = SprayAction.Brush6;
                    break;
                case VirtualKeys.Seven when hasCtrlModifier:
                    _sprayAction = SprayAction.Brush15;
                    break;
                case VirtualKeys.Seven:
                    _sprayAction = SprayAction.Brush7;
                    break;
                case VirtualKeys.Eight when hasCtrlModifier:
                    _sprayAction = SprayAction.Erase;
                    break;
                case VirtualKeys.Eight:
                    _sprayAction = SprayAction.Brush8;
                    break;
                case VirtualKeys.R:
                    _sprayAction = SprayAction.Random;
                    break;
                case VirtualKeys.C:
                    _sprayAction = SprayAction.Clear;
                    break;
                case VirtualKeys.E:
                    _sprayAction = SprayAction.Erase;
                    break;
                case VirtualKeys.Left:
                    _cursorPosition = new Vector2(_cursorPosition.X - offset.X * moveAccel, _cursorPosition.Y);
                    break;
                case VirtualKeys.Right:
                    _cursorPosition = new Vector2(_cursorPosition.X + offset.X * moveAccel, _cursorPosition.Y);
                    break;
                case VirtualKeys.Up:
                    _cursorPosition = new Vector2(_cursorPosition.X, _cursorPosition.Y - offset.Y * moveAccel);
                    break;
                case VirtualKeys.Down:
                    _cursorPosition = new Vector2(_cursorPosition.X, _cursorPosition.Y + offset.Y * moveAccel);
                    break;
                case VirtualKeys.Add:
                case VirtualKeys.OemEquals when (modifiers & VirtualKeys.ShiftModifier) == VirtualKeys.ShiftModifier:
                    UpdateSprayNozzleSize(GorgonTiming.Delta * 30f);
                    break;
                case VirtualKeys.Subtract:
                case VirtualKeys.OemMinus:
                    UpdateSprayNozzleSize(-GorgonTiming.Delta * 30f);
                    break;
                case VirtualKeys.PageUp:
                    UpdateBrushAlpha(GorgonTiming.Delta * 0.25f, true);
                    break;
                case VirtualKeys.PageDown:
                    UpdateBrushAlpha(-GorgonTiming.Delta * 0.25f, true);
                    break;
                case VirtualKeys.F1:
                    // As mentioned before, we need to notify that the key was pressed. This is the first 
                    // part to handling a key press and release state.
                    _helpKeyPressed = true;
                    break;
                case VirtualKeys.End:
                    // This just disables the input system. Why? Because, that's why.
                    // We can re-enable input, by switching away from the application 
                    // and switching back to it, the input system will be re-enabled.

                    // When we disable, we should reset the data accumulated in the 
                    // device objects. This way we won't get any strange behaviour 
                    // once the input system is reactivated.
                    _mouse.Reset();
                    _keyboard.Reset();

                    for (int g = 0; g < _gameDevices.Count; ++g)
                    {
                        _gameDevices[g].Reset();
                    }

                    _input.Disable();

                    // We will keep the keyboard active, just so we can handle some of the other key presses.
                    _input.Enable(InputFlags.Keyboard);

                    _mouse.CursorVisible = true;
                    return true;
            }
        }

        if (key.Length == 0)
        {
            _keyboardDisplayText.Append("None ");
        }

        if (modKeys.Length > 0)
        {
            _keyboardDisplayText.Append("Modifiers: ");
        }

        for (int i = 0; i < modKeys.Length; ++i)
        {
            _keyboardDisplayText.AppendFormat("{0} ", modKeys[i]);
        }

        _keyboardDisplayText.Append("(Press 'F1' for help)");

        LabelKeyboard.Text = _keyboardDisplayText.ToString();
        FlowKeyboard.Visible = true;

        return true;
    }

    /// <summary>
    /// Handles the Resize event of the panelDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void PanelDisplay_Resize(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            return;
        }

        // When we resize the window, we need to resize the buffer in which 
        // we spray paint into.
        _spray?.Resize(PanelDisplay.ClientSize);
    }

    /// <summary>
    /// Function to create the gaming device.
    /// </summary>
    /// <param name="input">The input system.</param>
    private void CreateGamingDevices(IGorgonInput input)
    {
        _activeGameDevice = null;

        if (input.GamingDevices.Count == 0)
        {
            UpdateGameInputDeviceList(input);
            return;
        }

        // Unlike the mouse and keyboard devices, we have to bind 
        // the gaming devices to an individual instance. This is 
        // because gaming devices can vary wildly in their
        // capabilities and the computer can't just assume which 
        // device to use.
        for (int i = 0; i < input.GamingDevices.Count; ++i)
        {
            _gameDevices.Add(new GorgonGamingDevice(input.GamingDevices[i]));
        }

        // If there's only one device, then there's no need to 
        // make the user choose which device to use.
        if (_gameDevices.Count == 1)
        {
            _activeGameDevice = _gameDevices[0];
        }

        UpdateGameInputDeviceList(input);
    }

    /// <summary>
    /// Function to update the gaming device list.
    /// </summary>
    /// <param name="input">The input system to use.</param>
    private void UpdateGameInputDeviceList(IGorgonInput input)
    {
        // Show our gaming device list on the UI if we have gaming devices.
        // Otherwise, don't bother. It'll be wasted space.
        if (input.GamingDevices.Count == 0)
        {
            FlowGamingDevices.Visible = FlowJoystick.Visible = TableJoysticks.Visible = false;
            return;
        }

        ListJoysticks.Items.Clear();

        // We'll need to determine how many XInput devices we have. Why? Because if we only have 1, or 0 XInput devices, then there's no point in showing the slot number.
        int xinputCount = input.GamingDevices.Count(x => (x.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) == GamingDeviceCapabilityFlags.IsXInputDevice);

        // We'll fill in our device list for each gaming device.
        for (int i = 0; i < input.GamingDevices.Count; i++)
        {
            IGorgonGamingDeviceInfo gameInfo = input.GamingDevices[i];
            string item = string.Empty;

            // We'll match the gaming device information to show
            // which of our gaming devices is the active one
            // we're using.
            bool isActive = _activeGameDevice?.Info == gameInfo;

            // Finally, we'll add the name/description of the gaming device to our list.
            // For XInput devices (if there are more than 1), we'll also show which slot the controller occupies. This is done by determining which part 
            // of the led is lit up (XBox 360 Controllers at least, Series X/S controllers don't seem to show this, but still work).
            if (((gameInfo.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice) || (xinputCount < 2))
            {
                item = $"{(isActive ? "*" : string.Empty)}{i + 1}. {gameInfo.Description}";
            }
            else
            {
                item = $"{(isActive ? "*" : string.Empty)}{i + 1}. {gameInfo.Description} #{gameInfo.XInputSlot + 1}";
            }

            ListJoysticks.Items.Add(item);
        }

        FlowJoystick.Visible = TableJoysticks.Visible = true;
    }

    /// <summary>
    /// Function called when an input event is received from the operating system.
    /// </summary>
    /// <param name="inputEvent">The event from the system.</param>
    /// <returns><b>true</b> if the event was processed already, <b>false</b> if not.</returns>
    private bool HandleInputEvent(GorgonInputEvent inputEvent)
    {
        // Filter out events. We only want gaming devices, all others can be handled during polling.
        if ((inputEvent.DeviceType != InputDeviceType.GamingDevice) && (_input.GamingDevices.Count > 0))
        {
            return false;
        }

        // Find the device that sent this event. Unlike our add/remove device callbacks, we're using the handle to find the 
        // device. This handle changes, so we cannot use it outside of the current application instance.
        IGorgonGamingDevice? device = _gameDevices.Find(d => d.Info.Handle == inputEvent.DeviceHandle);

        // Since this device is already active, we are done and can move on.
        if (device == _activeGameDevice)
        {
            return false;
        }

        // Our callbacks are always run on the background thread that does the message processing, and as such if we want to update the UI, we need to 
        // call on the main thread. To do this, we synchronize a call using the synchronization context we stored in OnLoad.
        _currentSyncContext?.Send(_ =>
        {
            if (device is not null)
            {
                // Parse this event and get its info.
                device.ParseData(inputEvent);

                // If we pressed a button, then make it active.
                for (int i = 0; i < device.Buttons.Length; ++i)
                {
                    if (device.Buttons[i])
                    {
                        _activeGameDevice = device;
                    }
                }

                // Otherwise, check the main axes for movement.
                // We do this because the devices are pretty sensitive. So, we deaden the values by applying 
                // a deadzone to the value. This way, only an actual purposeful movement will activate the 
                // device.                
                foreach (IGorgonGamingDeviceAxis axis in device.Axes)
                {
                    switch (axis.Axis)
                    {
                        case GamingDeviceAxis.XAxis:
                        case GamingDeviceAxis.YAxis:
                        case GamingDeviceAxis.ZAxis:
                        case GamingDeviceAxis.RotationX:
                        case GamingDeviceAxis.RotationY:
                        case GamingDeviceAxis.LeftTrigger:
                        case GamingDeviceAxis.RightTrigger:
                            if (device.ConstrainLinear(axis.Value, axis.DeadZone) != 0)
                            {
                                _activeGameDevice = device;
                            }
                            break;
                    }
                }
            }

            UpdateGameInputDeviceList(_input);
            HandleGamingDevices();
        }, null);

        // We're returning false here because we want the input system to actually process this event.
        // Had we returned true, then the event would not be processed.
        return false;
    }

    /// <summary>
    /// Function called when a device is added to the system.
    /// </summary>
    /// <param name="deviceInfo">The general device information for the device that was added.</param>
    private void DeviceAdded(IGorgonInputDeviceInfo deviceInfo)
    {
        // Since we're only interested in gaming devices. Filter for those, and cast the info to a gaming device info so we have
        // the information we need.
        if ((deviceInfo.DeviceType != InputDeviceType.GamingDevice)
            || (deviceInfo is not IGorgonGamingDeviceInfo gameDeviceInfo))
        {
            return;
        }

        // Find the gaming device object associated with the info. 
        // To do this, we use the device ID which is a GUID so the device can be uniquely identified. This GUID is also stable, meaning that it does not change 
        // across application restarts and can be stored for later use. The exception to this is if the device has been plugged into a different USB port.
        IGorgonGamingDevice? device = _gameDevices.Find(d => d.Info.DeviceID == gameDeviceInfo.DeviceID);

        // If this device is already in the system (it shouldn't be, but the asynchronous nature of devices can make things unpredictable).
        if (device is not null)
        {
            return;
        }

        // Our callbacks are always run on the background thread that does the message processing, and as such if we want to update the UI, we need to 
        // call on the main thread. To do this, we synchronize a call using the synchronization context we stored in OnLoad.
        _currentSyncContext?.Send(obj =>
        {
            // This is better than capturing the data in a closure.
            var info = (IGorgonGamingDeviceInfo?)obj;

            // This shouldn't happen, but we'll put a check in just to be safe.
            Debug.Assert(info is not null, "Device information is null");

            // This is a new device on the system, so we need to create a gaming device object for it.
            device = new GorgonGamingDevice(info);

            // If we don't already have an active device, then assign our new device as active.
            _activeGameDevice ??= device;

            // Update our local gaming device list.
            _gameDevices.Add(device);

            // Update the UI.
            UpdateGameInputDeviceList(_input);
            HandleGamingDevices();
        }, gameDeviceInfo);
    }

    /// <summary>
    /// Function called when a device is removed from the system.
    /// </summary>
    /// <param name="deviceInfo">The general device information for the device that was removed.</param>
    private void DeviceRemoved(IGorgonInputDeviceInfo deviceInfo)
    {
        // Since we're only interested in gaming devices. Filter for those, and cast the info to a gaming device info so we have
        // the information we need.
        if ((deviceInfo.DeviceType != InputDeviceType.GamingDevice)
            || (deviceInfo is not IGorgonGamingDeviceInfo gameDeviceInfo))
        {
            return;
        }

        // Find the gaming device object associated with the info. 
        // To do this, we use the device ID which is a GUID so the device can be uniquely identified. This GUID is also stable, meaning that it does not change 
        // across application restarts and can be stored for later use. The exception to this is if the device has been plugged into a different USB port.
        IGorgonGamingDevice? device = _gameDevices.Find(d => d.Info.DeviceID == gameDeviceInfo.DeviceID);

        // If there's no device in the list with the specific device ID, then there's not a lot we can do, so we can exit now.
        if (device is null)
        {
            return;
        }

        // Our callbacks are always run on the background thread that does the message processing, and as such if we want to update the UI, we need to 
        // call on the main thread. To do this, we synchronize a call using the synchronization context we stored in OnLoad.
        _currentSyncContext?.Send(obj =>
        {
            // This is better than capturing the data in a closure.
            var gameDevice = (IGorgonGamingDevice?)obj;

            // This shouldn't happen, but we'll do it anyway just to be safe.
            Debug.Assert(gameDevice is not null, "Game device is null.");

            // If the device being removed was active, which, it would be. Then we have to reset the active device reference.
            // As an alternative, we could also store the Device ID and use that to restore the active device.
            if ((_activeGameDevice is not null) && (_activeGameDevice.Info.DeviceID == gameDevice.Info.DeviceID))
            {
                _activeGameDevice = null;
            }

            // Remove it from our device list so we can update our UI.
            _gameDevices.Remove(gameDevice);

            // If we're left with only one device, we can make it the active device. 
            if (_gameDevices.Count == 1)
            {
                _activeGameDevice = _gameDevices[0];
            }

            // Update the UI.
            UpdateGameInputDeviceList(_input);
            HandleGamingDevices();
        }, device);
    }

    /// <summary>
    /// Raises the <see cref="System.Windows.Forms.Form.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    private void Form_Load(object sender, EventArgs e)
    {
        try
        {
            // Ensure that we have the necessary input devices.
            if ((_input.Mice.Count == 0) || (_input.Keyboards.Count == 0))
            {
                _input.Disable();

                GorgonDialogs.ErrorBox(this, "This application requires a mouse and keybnoard.");
                GorgonApplication.Quit();
                return;
            }

            // Set up our spray object and cursor for the mouse.
            _spray = new Spray(PanelDisplay.ClientSize);
            _cursor = new MouseCursor(PanelDisplay)
            {
                Hotspot = new Point(-16, -3)
            };

            _spraySize = _spray.SpraySize;

            // Center the mouse on the window.        
            _cursorPosition = DisplayHalfSize;

            // Initialize the UI.
            HandleMouseInput();
            HandleKeyboard();
            HandleGamingDevices();

            // When the display area changes size, update the spray effect
            // and limit the mouse.
            PanelDisplay.Resize += PanelDisplay_Resize;

            // Store the current synchronization context so we can make UI 
            // calls in our callbacks.
            _currentSyncContext = SynchronizationContext.Current;

            // Register our callback methods so that we can intercept when devices 
            // have been added to or removed from the computer.
            _input.RegisterDeviceChangeCallbacks(DeviceAdded, DeviceRemoved);

            // Register our callback method to intercept when we get an input event 
            // from the system. This will allow us to update our active gaming 
            // device upon use.
            _input.RegisterInputEventCallback(HandleInputEvent);

            // If we have any gaming devices, instance them now.
            CreateGamingDevices(_input);

            // Set up our idle method.
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
    /// Handles the <see cref="E:Activated" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        _mouse.CursorVisible = false;

        if (_input.Devices is not InputFlags.None and not InputFlags.Keyboard)
        {
            return;
        }

        // We can use this to restore or change which devices we've registered (if we've previously disabled).                        
        _input.Enable(InputFlags.ExclusiveMouse | InputFlags.Keyboard | InputFlags.GamingDevices);

        // When we re-enable, we need to recreate our game device objects because the previous gaming device 
        // objects are no longer valid.
        //
        // The keyboard and mouse are not bound to a specific device, so they do not need to be recreated
        // because they will take any keyboard/mouse data from the system and aggregate it.
        CreateGamingDevices(_input);

        FlowDevices.Visible = true;
        Capture = true;
    }

    /// <summary>
    /// Handles the <see cref="E:Deactivate" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);

        if (_input.Devices is InputFlags.None or InputFlags.Keyboard)
        {
            return;
        }

        // When we deactivate the input system, we should reset the state data for the devices, just to ensure 
        // everything is read correctly when the system is enabled again.
        _mouse.Reset();
        _keyboard.Reset();

        // This turns off input monitoring for every device we previously registered for in the constructor or 
        // the last call to Enable.
        _input.Disable();
        _mouse.CursorVisible = true;

        TableJoysticks.Visible = false;
        FlowDevices.Visible = false;
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

        _mouse.CursorVisible = true;

        // Always dispose the input system. This will disable its background thread and restore our input back to normal.
        _input.Dispose();
        // Event buffers must be disposed, otherwise resources may be leaked.
        _events.Dispose();

        _cursor?.Dispose();
        _spray?.Dispose();
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
        // You will note the "ExclusiveMouse" flag. This tells the input system that we will be the exclusive owner of all the data from 
        // the mouse. This means the application will no longer respond to Windows mouse events. Both keyboards and mice can be exclusive 
        // to the input system, but be warned that the system will not respond to some system key presses like Alt+F4 when the keyboard 
        // device is exclusive.
        // 
        // Also, regardless of whether the mouse or keyboard are exclusive, they will stop sending data if the application is not focused.
        // Gaming devices however, will always receive data.
        _input = GorgonInput.CreateInput(InputFlags.ExclusiveMouse | InputFlags.Keyboard | InputFlags.GamingDevices, GorgonApplication.Log);
    }
}