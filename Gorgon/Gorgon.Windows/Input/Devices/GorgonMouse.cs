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
// Created: January 19, 2025 11:55:54 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Windows.Input.Properties;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.Windows.Input.Devices;

/// <inheritdoc cref="IGorgonMouse"/>
/// <param name="mouseDevice">[Optional] The specific mouse to retrieve data for.</param>
/// <remarks>
/// <para>
/// If the <paramref name="mouseDevice"/> parameter is not specified, then this keyboard instance will accept input for any mouse. Otherwise, the data passed to the mouse from the input system will be 
/// filtered to the device specified.
/// </para>
/// <para>
/// If the <paramref name="mouseDevice"/> is not <b>null</b>, then this object will have to be recreated if the <see cref="IGorgonInput.Enable"/> method is called.
/// </para>
/// </remarks>
public class GorgonMouse(IGorgonMouseInfo? mouseDevice = null) : IGorgonMouse
{
    // The system mouse information.
    private static readonly IGorgonMouseInfo _systemMouse;
    // Lock for multiple threads.
    private static readonly object _syncLock = new();

    // The last known absolute position.
    private GorgonPoint? _lastAbsPosition;

    /// <summary>
    /// The wheel dentent value.
    /// </summary>
    /// <remarks>
    /// This is used to determine how many "clicks" the wheel has moved. This value is used to determine how many lines to scroll in a window, or how many "clicks" to move in a game.
    /// </remarks>
    public const int WheelDelta = 120;

    /// <summary>
    /// The maximum value for the wheel.
    /// </summary>
    public const int MaxWheelValue = 2_147_483_640;
    /// <summary>
    /// The minimum value for the wheel.
    /// </summary>
    public const int MinWheelValue = -2_147_483_640;

    /// <summary>
    /// Minimum horizontal value for the <see cref="Position"/>.
    /// </summary>
    public const int MinXValue = int.MinValue;

    /// <summary>
    /// Maximum horizontal value for the <see cref="Position"/>.
    /// </summary>
    public const int MaxXValue = int.MaxValue;

    /// <summary>
    /// Minimum vertical value for the <see cref="Position"/>.
    /// </summary>
    public const int MinYValue = int.MinValue;

    /// <summary>
    /// Maximum vertical value for the <see cref="Position"/>.
    /// </summary>
    public const int MaxYValue = int.MaxValue;

    /// <inheritdoc/>
    public bool CursorVisible
    {
        get => PInvoke.IsCursorVisible() == CURSORINFO_FLAGS.CURSOR_SHOWING;
        set
        {
            lock (_syncLock)
            {
                ShowMouseCursor(value);
            }
        }
    }

    /// <inheritdoc/>
    public IGorgonMouseInfo Info
    {
        get;
    } = mouseDevice ?? _systemMouse;

    /// <inheritdoc/>
    public GorgonPoint Position
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public GorgonPoint DeltaPosition
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public int DeltaVerticalWheel
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public int DeltaHorizontalWheel
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public MouseButtons Buttons
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public int RelativeVerticalWheel
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public int RelativeHorizontalWheel
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to show the mouse cursor or hide it.
    /// </summary>
    /// <param name="show"><b>true</b> to show the mouse cursor, <b>false</b> to hide it.</param>
    private static void ShowMouseCursor(bool show)
    {
        CURSORINFO_FLAGS isVisible = PInvoke.IsCursorVisible();

        // If the cursor is suppressed, then we're using a touch interface.
        // So we'll not acknowledge requests to show the cursor in that case.
        if ((isVisible == CURSORINFO_FLAGS.CURSOR_SUPPRESSED)
            || ((isVisible == CURSORINFO_FLAGS.CURSOR_SHOWING) && (show))
            || ((isVisible != CURSORINFO_FLAGS.CURSOR_SHOWING) && (!show)))
        {
            return;
        }

        SpinWait waiter = new();

        if (show)
        {
            while (PInvoke.ShowCursor(true) < 0)
            {
                waiter.SpinOnce();
            }

            return;
        }

        while (PInvoke.ShowCursor(false) > -1)
        {
            waiter.SpinOnce();
        }
    }

    /// <summary>
    /// Function to update the button states of the specified mouse.
    /// </summary>
    /// <param name="buttons">The current button state.</param>
    private void UpdateMouseButtons(uint buttons)
    {
        if (buttons is 0 or PInvoke.RI_MOUSE_WHEEL or PInvoke.RI_MOUSE_HWHEEL)
        {
            return;
        }

        MouseButtons mouseButtons = Buttons;

        if ((buttons & PInvoke.RI_MOUSE_BUTTON_1_DOWN) == PInvoke.RI_MOUSE_BUTTON_1_DOWN)
        {
            mouseButtons |= MouseButtons.Button1;
        }
        else if ((buttons & PInvoke.RI_MOUSE_BUTTON_1_UP) == PInvoke.RI_MOUSE_BUTTON_1_UP)
        {
            mouseButtons &= ~MouseButtons.Button1;
        }

        if ((buttons & PInvoke.RI_MOUSE_BUTTON_2_DOWN) == PInvoke.RI_MOUSE_BUTTON_2_DOWN)
        {
            mouseButtons |= MouseButtons.Button2;
        }
        else if ((buttons & PInvoke.RI_MOUSE_BUTTON_2_UP) == PInvoke.RI_MOUSE_BUTTON_2_UP)
        {
            mouseButtons &= ~MouseButtons.Button2;
        }

        if ((buttons & PInvoke.RI_MOUSE_BUTTON_3_DOWN) == PInvoke.RI_MOUSE_BUTTON_3_DOWN)
        {
            mouseButtons |= MouseButtons.Button3;
        }
        else if ((buttons & PInvoke.RI_MOUSE_BUTTON_3_UP) == PInvoke.RI_MOUSE_BUTTON_3_UP)
        {
            mouseButtons &= ~MouseButtons.Button3;
        }

        if ((buttons & PInvoke.RI_MOUSE_BUTTON_4_DOWN) == PInvoke.RI_MOUSE_BUTTON_4_DOWN)
        {
            mouseButtons |= MouseButtons.Button4;
        }
        else if ((buttons & PInvoke.RI_MOUSE_BUTTON_4_UP) == PInvoke.RI_MOUSE_BUTTON_4_UP)
        {
            mouseButtons &= ~MouseButtons.Button4;
        }

        if ((buttons & PInvoke.RI_MOUSE_BUTTON_5_DOWN) == PInvoke.RI_MOUSE_BUTTON_5_DOWN)
        {
            mouseButtons |= MouseButtons.Button5;
        }
        else if ((buttons & PInvoke.RI_MOUSE_BUTTON_5_UP) == PInvoke.RI_MOUSE_BUTTON_5_UP)
        {
            mouseButtons &= ~MouseButtons.Button5;
        }

        Buttons = mouseButtons;
    }

    /// <summary>
    /// Function to reset the data for the mouse.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        Buttons = MouseButtons.None;
        Position = GorgonPoint.Zero;
        DeltaPosition = GorgonPoint.Zero;
        RelativeVerticalWheel = 0;
        RelativeHorizontalWheel = 0;
        _lastAbsPosition = null;
        DeltaVerticalWheel = 0;
        DeltaHorizontalWheel = 0;
    }

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEvent inputEvent)
    {
        if ((_systemMouse != Info) && (Info.Handle != inputEvent.DeviceHandle))
        {
            return false;
        }

        if (inputEvent.DeviceType != InputDeviceType.Mouse)
        {
            Reset();
            return false;
        }

        ref readonly RawMouseData mouseData = ref inputEvent.RawInputMouseData;

        if (!mouseData.IsAbsoute)
        {
            DeltaPosition = mouseData.Position;
        }
        else
        {
            if (_lastAbsPosition == null)
            {
                _lastAbsPosition = mouseData.Position;
                DeltaPosition = GorgonPoint.Zero;
            }
            else
            {
                DeltaPosition = mouseData.Position - _lastAbsPosition.Value;
                _lastAbsPosition = mouseData.Position;
            }
        }

        // Limit the values so we don't overflow.
        int newX = (int)((long)(Position.X + DeltaPosition.X)).Clamp(MinXValue, MaxXValue);
        int newY = (int)((long)(Position.Y + DeltaPosition.Y)).Clamp(MinYValue, MaxYValue);

        Position = new GorgonPoint(newX, newY);

        UpdateMouseButtons(mouseData.Buttons);

        DeltaVerticalWheel = mouseData.WheelDelta;
        DeltaHorizontalWheel = mouseData.HWheelDelta;

        RelativeVerticalWheel = ((mouseData.WheelDelta + RelativeVerticalWheel).Clamp(MinWheelValue, MaxWheelValue));
        RelativeHorizontalWheel = ((mouseData.HWheelDelta + RelativeHorizontalWheel).Clamp(MinWheelValue, MaxWheelValue));

        return true;
    }

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEventBuffer inputEvents)
    {
        // If we're bound to a specific mouse, then we only want to parse data from that mouse.
        if (inputEvents.MouseEventCount == 0)
        {
            return false;
        }

        for (int i = 0; i < inputEvents.MouseEventCount; ++i)
        {
            ParseData(inputEvents.GetMouseEvent(i));
        }

        return true;
    }

    /// <summary>
    /// Initializes the <see cref="GorgonMouse"/> class.
    /// </summary>
    static GorgonMouse() => _systemMouse = new MouseInfo(IntPtr.Zero, "NULL", "NULL", Resources.GORINP_DESC_SYSTEM_MOUSE, 0, PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CMOUSEBUTTONS));
}
