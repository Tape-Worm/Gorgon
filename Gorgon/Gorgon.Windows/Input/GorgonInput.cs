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
// Created: January 8, 2025 9:37:51 PM
//

// Much of the code present was adapted from the Simple Direct Media layer (SDL v3.xx) game controller code found at
// https://github.com/libsdl-org/SDL

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Timing;
using Gorgon.Windows.Input.Devices;
using Gorgon.Windows.Input.Native;
using Gorgon.Windows.Input.Properties;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;
using Windows.Win32.UI.Input.XboxController;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.Windows.Input;

/// <summary>
/// <inheritdoc cref="IGorgonInput"/>
/// </summary>
/// <remarks>
/// <inheritdoc cref="IGorgonInput"/>
/// <para>
/// This implementation of the input system is a singleton system, and only one instance of the input system can be active for a process. This is because the underlying system (Raw Input) is a singleton 
/// system, and only one raw input system can be active for a process. Thus, it cannot be instanced via a public constructor, and instead uses a factory method: <see cref="CreateInput"/>. When the system is 
/// created it will create a background thread that will monitor and process input data from the operating system. This thread is long lived and will continue to run until the application is shut down or 
/// the input system is disposed.
/// </para>
/// </remarks>
/// <seealso cref="CreateInput"/>
public sealed class GorgonInput
    : IGorgonInput
{
    // The number of microseconds that an event should be kept in the queue.
    private const long MaxTimeInQueue = 500_000;

    // The instance.
    private static volatile GorgonInput? _instance = null;

    // The log for debug messages.
    private static IGorgonLog _log = GorgonLog.NullLog;
    // The flags used to register input devices.
    private static InputFlags _flags;
    // Flag to indicate that the devices are registered for raw input.
    private static int _devicesRegistered;
    // The flag to indicate that the input thread is running.
    private static CancellationTokenSource? _cancelSource;
    // The notifier used to notify that the system is shutting down.
    private static ManualResetEventSlim? _shutdownNotification;
    // The window that will receive raw input data.
    private InputWindow? _window;
    // Queue of inputs.
    private readonly GorgonRingPool<GorgonInputEvent> _inputEventPool = new(16_384, () => new GorgonInputEvent());
    // Our ongoing list of input readings.
    private readonly ConcurrentQueue<GorgonInputEvent> _rawReadings = new();
    // The input thread.
    private Thread? _inputThread;
    // The lists of devices attached to the system.
    private readonly List<KeyboardInfo> _keyboardDevices = [];
    private readonly List<MouseInfo> _mouseDevices = [];
    private readonly List<GamingDeviceInfo> _gameDevices = [];
    // The callbacks used to monitor input.
    private Action<IGorgonInputDeviceInfo>? _deviceAttachedCallback;
    private Action<IGorgonInputDeviceInfo>? _deviceDetachedCallback;
    private Func<GorgonInputEvent, bool>? _inputEventCallback;
    // Device update lock.
    private readonly object _deviceUpdateLock = new();
    private static readonly object _inputCreateLock = new();

    /// <inheritdoc/>
    public IReadOnlyList<IGorgonMouseInfo> Mice => _mouseDevices;

    /// <inheritdoc/>
    public IReadOnlyList<IGorgonKeyboardInfo> Keyboards => _keyboardDevices;

    /// <inheritdoc/>
    public IReadOnlyList<IGorgonGamingDeviceInfo> GamingDevices => _gameDevices;

    /// <inheritdoc/>
    public InputFlags Devices
    {
        get
        {
            InputFlags result = InputFlags.None;

            if ((Keyboards.Count == 0) && (Mice.Count == 0) && (GamingDevices.Count == 0))
            {
                return result;
            }

            result |= _flags & (InputFlags.Mouse | InputFlags.ExclusiveMouse);
            result |= _flags & (InputFlags.Keyboard | InputFlags.ExclusiveKeyboard);
            result |= _flags & InputFlags.GamingDevices;

            return result;
        }
    }

    /// <summary>
    /// Function to enable raw input hooks.
    /// </summary>
    /// <param name="flags">The flags for device registration.</param>
    private void EnableRawInput(InputFlags flags)
    {
        Debug.Assert(_window is not null, "Message window is missing.");

        HWND handle = _window.Handle;

        RAWINPUTDEVICE_FLAGS keyboardFlags = RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY;
        RAWINPUTDEVICE_FLAGS mouseFlags = RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY;

        if ((flags & InputFlags.ExclusiveKeyboard) == InputFlags.ExclusiveKeyboard)
        {
            keyboardFlags |= RAWINPUTDEVICE_FLAGS.RIDEV_NOLEGACY;
        }

        if ((flags & InputFlags.ExclusiveMouse) == InputFlags.ExclusiveMouse)
        {
            mouseFlags |= RAWINPUTDEVICE_FLAGS.RIDEV_NOLEGACY;
        }

        if ((flags & InputFlags.Keyboard) == InputFlags.Keyboard)
        {
            RawInputApi.RegisterRawInputDevice(HIDUsage.Keyboard, handle, keyboardFlags);
        }

        if ((flags & InputFlags.Mouse) == InputFlags.Mouse)
        {
            RawInputApi.RegisterRawInputDevice(HIDUsage.Mouse, handle, mouseFlags);
        }

        if ((flags & InputFlags.GamingDevices) == InputFlags.GamingDevices)
        {
            RawInputApi.RegisterRawInputDevice(HIDUsage.Gamepad, handle, RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY);
            RawInputApi.RegisterRawInputDevice(HIDUsage.Joystick, handle, RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY);
        }

        // Reset the flag so that we can wait on new raw input messages.
        _ = PInvoke.GetQueueStatus(QUEUE_STATUS_FLAGS.QS_RAWINPUT);

        _flags = flags;
    }

    /// <summary>
    /// Function to disable the raw input hooks.
    /// </summary>
    private void DisableRawInput()
    {
        if ((_flags & InputFlags.Mouse) == InputFlags.Mouse)
        {
            RawInputApi.UnregisterRawInputDevice(HIDUsage.Mouse);
        }

        if ((_flags & InputFlags.Keyboard) == InputFlags.Keyboard)
        {
            RawInputApi.UnregisterRawInputDevice(HIDUsage.Keyboard);
        }

        if ((_flags & InputFlags.GamingDevices) != InputFlags.GamingDevices)
        {
            return;
        }

        for (int i = 0; i < _gameDevices.Count; ++i)
        {
            if (_gameDevices[i].XInputSlot == -1)
            {
                continue;
            }

            // For XInput devices, force vibration off.
            if ((_gameDevices[i].Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) == GamingDeviceCapabilityFlags.SupportsVibration)
            {
                XINPUT_VIBRATION vib = new();
                PInvoke.XInputSetState((uint)_gameDevices[i].XInputSlot, in vib);
            }

            _gameDevices[i].XInputSlot = -1;
        }

        RawInputApi.UnregisterRawInputDevice(HIDUsage.Gamepad);
        RawInputApi.UnregisterRawInputDevice(HIDUsage.Joystick);
    }

    /// <summary>
    /// Function to release managed or unmanged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> if the object is being disposed by a call to <see cref="Dispose()"/>, or <b>false</b> if called by the finalizer.</param>
    private void Dispose(bool disposing)
    {
        Interlocked.Exchange(ref _instance, null);

        if (disposing)
        {
            Interlocked.Exchange(ref _deviceAttachedCallback, null);
            Interlocked.Exchange(ref _deviceDetachedCallback, null);
            Interlocked.Exchange(ref _inputEventCallback, null);
        }

        // Tell the thread we're done and cancel the input gathering notifier.
        _cancelSource?.Cancel();

        _log.Print("Shutting down Gorgon Input...", LoggingLevel.Simple);

        if (Interlocked.Exchange(ref _devicesRegistered, 0) != 0)
        {
            if ((_flags & InputFlags.Mouse) == InputFlags.Mouse)
            {
                RawInputApi.UnregisterRawInputDevice(HIDUsage.Mouse);
            }

            if ((_flags & InputFlags.Keyboard) == InputFlags.Keyboard)
            {
                RawInputApi.UnregisterRawInputDevice(HIDUsage.Keyboard);
            }

            if ((_flags & InputFlags.GamingDevices) == InputFlags.GamingDevices)
            {
                for (int i = 0; i < 4; ++i)
                {
                    XInputApi.SetVibrationMotor(i, 0, 0);
                }

                RawInputApi.UnregisterRawInputDevice(HIDUsage.Gamepad);
                RawInputApi.UnregisterRawInputDevice(HIDUsage.Joystick);
            }

            _log.Print($"{_flags} input devices unregistered.", LoggingLevel.Verbose);
        }

        // Ensure the thread is dead, but only for a few seconds.
        _shutdownNotification?.Wait(3000);

        ManualResetEventSlim? shutdown = Interlocked.Exchange(ref _shutdownNotification, null);

        shutdown?.Dispose();
    }

    /// <summary>
    /// Function to retrieve data about the device.
    /// </summary>
    /// <param name="device">The device handle used to locate the device data.</param>
    /// <returns>A tuple containing the device information data, the device system name, the device class, and a friendly description of the device. If this method fails, then the strings will be empty.</returns>
    private static (RID_DEVICE_INFO DeviceInfo, string DeviceName, string ClassName, string Description) GetDeviceData(ref readonly RAWINPUTDEVICELIST device)
    {
        RID_DEVICE_INFO deviceInfo = RawInputApi.GetDeviceInfo(in device);
        string deviceName = RawInputApi.GetDeviceName(in device, _log);

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            if (deviceInfo.dwType == RID_DEVICE_INFO_TYPE.RIM_TYPEHID)
            {
                HIDUsage usage = (HIDUsage)deviceInfo.Anonymous.hid.usUsage;

                if (usage is HIDUsage.Gamepad or HIDUsage.Joystick)
                {
                    _log.PrintWarning($"{deviceInfo.dwType} {usage} 0x{((nint)device.hDevice).FormatHex()} skipped due to lack of name.", LoggingLevel.Simple);
                }
            }
            else
            {
                _log.PrintWarning($"{deviceInfo.dwType} 0x{((nint)device.hDevice).FormatHex()} skipped due to lack of name.", LoggingLevel.Simple);
            }

            return (default, string.Empty, string.Empty, string.Empty);
        }

        string className = RawInputDeviceRegistryInfo.GetDeviceClass(deviceName, _log);
        string deviceDescription = RawInputDeviceRegistryInfo.GetDeviceDescription(deviceName, _log);

        return (deviceInfo, deviceName, className, deviceDescription);
    }

    /// <summary>
    /// Function to enumerate raw inputs devices attached to the system.
    /// </summary>
    /// <param name="flags">The flags for registering devices.</param>
    private void EnumerateRawInputDevices(InputFlags flags)
    {
        if (flags == InputFlags.None)
        {
            lock (_deviceUpdateLock)
            {
                _keyboardDevices.Clear();
                _mouseDevices.Clear();
                _gameDevices.Clear();
            }
            return;
        }

        _log.Print("Enumerating Input devices...", LoggingLevel.Simple);

        // Capture all keyboards/mice/HID devices.
        RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices();

        _log.Print($"{devices.Length} devices found.", LoggingLevel.Intermediate);

        List<MouseInfo> mice = [];
        List<KeyboardInfo> keyboards = [];
        List<GamingDeviceInfo> gameDevices = [];

        for (int i = 0; i < devices.Length; ++i)
        {
            ref RAWINPUTDEVICELIST device = ref devices[i];
            (RID_DEVICE_INFO deviceInfo, string deviceName, string className, string deviceDescription) = GetDeviceData(in device);

            if (string.IsNullOrWhiteSpace(deviceName))
            {
                continue;
            }

            switch (deviceInfo.dwType)
            {
                case RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE:
                    if ((flags & InputFlags.Mouse) != InputFlags.Mouse)
                    {
                        break;
                    }

                    _log.Print($"Found mouse input device '{deviceDescription}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);

                    mice.Add(new MouseInfo(device.hDevice, deviceName, className, deviceDescription, (int)deviceInfo.Anonymous.mouse.dwSampleRate, (int)deviceInfo.Anonymous.mouse.dwNumberOfButtons));
                    break;
                case RID_DEVICE_INFO_TYPE.RIM_TYPEKEYBOARD:
                    if ((flags & InputFlags.Keyboard) != InputFlags.Keyboard)
                    {
                        break;
                    }

                    _log.Print($"Found keyboard input device '{deviceDescription}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);

                    keyboards.Add(new KeyboardInfo(device.hDevice, deviceName, className, deviceDescription, (int)deviceInfo.Anonymous.keyboard.dwNumberOfFunctionKeys, (KeyboardType)deviceInfo.Anonymous.keyboard.dwType));
                    break;
                case RID_DEVICE_INFO_TYPE.RIM_TYPEHID:
                    if ((flags & InputFlags.GamingDevices) != InputFlags.GamingDevices)
                    {
                        break;
                    }

                    GamingDeviceInfo gamingDeviceInfo = GamingDeviceInfo.GetGamingDeviceInfo(deviceName, className, deviceDescription, ref device, ref deviceInfo, _log);

                    if (gamingDeviceInfo == GamingDeviceInfo.Empty)
                    {
                        break;
                    }

                    _log.Print($"Found {(HIDUsage)deviceInfo.Anonymous.hid.usUsage} input device '{gamingDeviceInfo.Description}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);

                    gameDevices.Add(gamingDeviceInfo);
                    break;
                default:
                    _log.PrintWarning($"The device type {deviceInfo.dwType} is not known and will be skipped.", LoggingLevel.Verbose);
                    break;
            }
        }

        lock (_deviceUpdateLock)
        {
            _keyboardDevices.Clear();
            _mouseDevices.Clear();
            _gameDevices.Clear();

            _mouseDevices.AddRange(mice);
            _keyboardDevices.AddRange(keyboards);
            _gameDevices.AddRange(gameDevices.OrderBy(d => d.Description));
        }
    }

    /// <summary>
    /// Function to retrieve the index of the mouse with the specified handle.
    /// </summary>
    /// <param name="deviceHandle">The handle to find.</param>
    /// <returns>The index of the mouse, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetMouseIndex(HANDLE deviceHandle)
    {
        lock (_deviceUpdateLock)
        {
            if (Mice.Count == 0)
            {
                return -1;
            }

            if ((deviceHandle == HANDLE.Null) || (Mice.Count == 1))
            {
                return 0;
            }

            for (int i = 0; i < Mice.Count; ++i)
            {
                if (Mice[i].Handle == deviceHandle)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// Function to retrieve the index of the keyboard with the specified handle.
    /// </summary>
    /// <param name="deviceHandle">The handle to find.</param>
    /// <returns>The index of the keyboard, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetKeyboardIndex(HANDLE deviceHandle)
    {
        lock (_deviceUpdateLock)
        {
            if (Keyboards.Count == 0)
            {
                return -1;
            }

            if ((deviceHandle == HANDLE.Null) || (Keyboards.Count == 1))
            {
                return 0;
            }

            for (int i = 0; i < Keyboards.Count; ++i)
            {
                if (Keyboards[i].Handle == deviceHandle)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// Function to retrieve the index of the gaming device with the specified handle.
    /// </summary>
    /// <param name="deviceHandle">The handle to find.</param>
    /// <returns>The index of the gaming device, or -1 if not found.</returns>
    private int GetGamingDeviceIndex(HANDLE deviceHandle)
    {
        lock (_deviceUpdateLock)
        {
            if ((_gameDevices.Count == 0) || (deviceHandle == HANDLE.Null))
            {
                return -1;
            }

            if (_gameDevices.Count == 1)
            {
                return (_gameDevices[0].Handle == deviceHandle) ? 0 : -1;
            }

            for (int i = 0; i < _gameDevices.Count; ++i)
            {
                if (_gameDevices[i].Handle == deviceHandle)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// Function to update the mouse wheel state data for the specified mouse.
    /// </summary>
    /// <param name="mouseData">The data sent from the raw input system.</param>
    /// <returns>The mouse information to store with the event.</returns>
    private static RawMouseData UpdateMouseState(ref readonly RAWMOUSE mouseData)
    {
        GorgonPoint position;
        short vWheelDelta = 0, hWheelDelta = 0;
        bool absolute = (mouseData.usFlags & MOUSE_STATE.MOUSE_MOVE_ABSOLUTE) == MOUSE_STATE.MOUSE_MOVE_ABSOLUTE;

        // For mouse relative movement, we accumulate the movement and pass accumulated X and Y values back to the caller.
        // This makes it so callers do not have to calculate movement themselves.
        // These values are not in any kind of "space", meaning that it's amount of movement, not screen coordinates.
        if (!absolute)
        {
            position = new GorgonPoint(mouseData.lLastX, mouseData.lLastY);
        }
        else
        {
            // For absolute, we need to normalize the number into the correct screen space and then return that.
            int l, t, r, b;

            absolute = true;

            if ((mouseData.usFlags & MOUSE_STATE.MOUSE_VIRTUAL_DESKTOP) == MOUSE_STATE.MOUSE_VIRTUAL_DESKTOP)
            {
                l = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
                t = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);
                r = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
                b = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);
            }
            else
            {
                l = 0;
                t = 0;
                r = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
                b = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
            }

            position = new GorgonPoint(mouseData.lLastX.MulDiv(r, ushort.MaxValue) + l, mouseData.lLastY.MulDiv(b, ushort.MaxValue) + t);
        }

        if ((mouseData.Anonymous.Anonymous.usButtonFlags & PInvoke.RI_MOUSE_WHEEL) == PInvoke.RI_MOUSE_WHEEL)
        {
            vWheelDelta = (short)mouseData.Anonymous.Anonymous.usButtonData;
        }

        if ((mouseData.Anonymous.Anonymous.usButtonFlags & PInvoke.RI_MOUSE_HWHEEL) == PInvoke.RI_MOUSE_HWHEEL)
        {
            hWheelDelta = (short)mouseData.Anonymous.Anonymous.usButtonData;
        }

        return new(position, mouseData.Anonymous.ulButtons, vWheelDelta, hWheelDelta, absolute);
    }

    /// <summary>
    /// Function to update the state of the keyboard.
    /// </summary>
    /// <param name="keyboardData">The raw input keyboard data.</param>    
    private static RawKeyboardData UpdateKeyboardState(ref readonly RAWKEYBOARD keyboardData)
    {
        KeyboardDataFlags flags;
        VirtualKeys key;
        int scanCode;

        key = (VirtualKeys)keyboardData.VKey;

        flags = ((keyboardData.Flags & PInvoke.RI_KEY_BREAK) == PInvoke.RI_KEY_BREAK) ? KeyboardDataFlags.KeyUp : KeyboardDataFlags.KeyDown;

        if (key is VirtualKeys.Control or VirtualKeys.Alt)
        {
            flags |= (keyboardData.Flags & PInvoke.RI_KEY_E0) == PInvoke.RI_KEY_E0 ? KeyboardDataFlags.RightKey : KeyboardDataFlags.LeftKey;
        }

        // Shift has to be handled in a special case since it doesn't actually detect left/right from raw input.
        if (key == VirtualKeys.Shift)
        {
            flags |= keyboardData.MakeCode == 0x36 ? KeyboardDataFlags.RightKey : KeyboardDataFlags.LeftKey;
        }

        scanCode = keyboardData.MakeCode;

        return new(scanCode, flags, key);
    }

    /// <summary>
    /// Function to purge any old, unprocessed messages from the queue.
    /// </summary>
    /// <param name="timeStamp">The current timestamp.</param>
    private void PurgeOldEvents(long timeStamp)
    {
        if (_rawReadings.IsEmpty)
        {
            return;
        }

        long maxTime = timeStamp - MaxTimeInQueue;

        while ((!_rawReadings.IsEmpty) && (_rawReadings.TryPeek(out GorgonInputEvent? firstEvent)) && (firstEvent.TimeStamp <= maxTime))
        {
            if (!_rawReadings.TryDequeue(out firstEvent))
            {
                break;
            }

            firstEvent.Reset();
        }
    }

    /// <summary>
    /// Function to take the raw input events and transform them into Gorgon input events.
    /// </summary>
    /// <param name="timeStamp">The current timestamp.</param>
    /// <param name="increment">The increment for the timestamp.</param>
    /// <param name="rawInputEvents">The raw input events to process.</param>
    /// <param name="cancelToken">The cancellation token used to cancel the operation.</param>    
    private void GatherRawInputEvents(long timeStamp, long increment, Queue<RAWINPUT> rawInputEvents, CancellationToken cancelToken)
    {
        GorgonInputEvent inputEvent;

        while (rawInputEvents.Count != 0)
        {
            // If we cancelled, exit the loop.
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }

            // Drop anything that exceeds our pool size. We're reusing objects, so 
            // anything placed at the beginning of the queue is lost anyway.
            while (_rawReadings.Count > _inputEventPool.TotalSize)
            {
                if (!_rawReadings.TryDequeue(out _))
                {
                    break;
                }
            }

            rawInputEvents.TryDequeue(out RAWINPUT inputPacket);
            timeStamp += increment;

            var deviceType = (RID_DEVICE_INFO_TYPE)inputPacket.header.dwType;

            switch (deviceType)
            {
                case RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE:
                    GorgonPoint position = GorgonPoint.Zero;
                    ref readonly RAWMOUSE mouseData = ref inputPacket.data.mouse;
                    int mouseIndex = GetMouseIndex(inputPacket.header.hDevice);

                    if (mouseIndex == -1)
                    {
                        break;
                    }

                    RawMouseData mouseState = UpdateMouseState(in mouseData);

                    inputEvent = _inputEventPool.Allocate();
                    inputEvent.FromRawInput(inputPacket.header.hDevice, in mouseState, timeStamp);

                    if ((_inputEventCallback is null) || (!_inputEventCallback(inputEvent)))
                    {
                        _rawReadings.Enqueue(inputEvent);
                    }
                    break;
                case RID_DEVICE_INFO_TYPE.RIM_TYPEKEYBOARD:
                    ref readonly RAWKEYBOARD keyboardData = ref inputPacket.data.keyboard;
                    int keyboardIndex = GetKeyboardIndex(inputPacket.header.hDevice);

                    if (keyboardIndex == -1)
                    {
                        break;
                    }

                    RawKeyboardData keyboardState = UpdateKeyboardState(in keyboardData);

                    inputEvent = _inputEventPool.Allocate();
                    inputEvent.FromRawInput(inputPacket.header.hDevice, in keyboardState, timeStamp);

                    if ((_inputEventCallback is null) || (!_inputEventCallback(inputEvent)))
                    {
                        _rawReadings.Enqueue(inputEvent);
                    }
                    break;
                case RID_DEVICE_INFO_TYPE.RIM_TYPEHID:
                    ref readonly RAWHID hidData = ref inputPacket.data.hid;
                    int gamingDeviceIndex = GetGamingDeviceIndex(inputPacket.header.hDevice);

                    if ((gamingDeviceIndex == -1)
                        || (_gameDevices[gamingDeviceIndex].DeviceType != InputDeviceType.GamingDevice))
                    {
                        break;
                    }

                    GamingDeviceInfo info = _gameDevices[gamingDeviceIndex];

                    inputEvent = _inputEventPool.Allocate();
                    inputEvent.FromRawInput(inputPacket.header.hDevice, in hidData, info, timeStamp);

                    if ((_inputEventCallback is null) || (!_inputEventCallback(inputEvent)))
                    {
                        _rawReadings.Enqueue(inputEvent);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Function called when a device has been detached from the system.
    /// </summary>
    /// <param name="deviceHandle">The handle for the device.</param>
    private void HandleDeviceDetached(HANDLE deviceHandle)
    {
        RID_DEVICE_INFO_TYPE deviceType = RID_DEVICE_INFO_TYPE.RIM_TYPEHID;

        int deviceIndex = GetGamingDeviceIndex(deviceHandle);

        // Find out what type of detached.
        if (deviceIndex == -1)
        {
            deviceIndex = GetKeyboardIndex(deviceHandle);

            if (deviceIndex == -1)
            {
                deviceIndex = GetMouseIndex(deviceHandle);

                if (deviceIndex == -1)
                {
                    return;
                }
                else
                {
                    deviceType = RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE;
                }
            }
            else
            {
                deviceType = RID_DEVICE_INFO_TYPE.RIM_TYPEKEYBOARD;
            }
        }

        _log.Print($"{deviceType} device with handle 0x{((nint)deviceHandle).FormatHex()} has been removed from the system.", LoggingLevel.Intermediate);

        switch (deviceType)
        {
            case RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE:
                MouseInfo mouseInfo = _mouseDevices[deviceIndex];
                lock (_deviceUpdateLock)
                {
                    if (_mouseDevices.Remove(mouseInfo))
                    {
                        _deviceDetachedCallback?.Invoke(mouseInfo);
                    }
                }

                _log.Print($"Mouse input device '{mouseInfo.Description}' on HID path {mouseInfo.DeviceName}, class {mouseInfo.DeviceClass} was removed.", LoggingLevel.Verbose);
                break;
            case RID_DEVICE_INFO_TYPE.RIM_TYPEKEYBOARD:
                KeyboardInfo keyInfo = _keyboardDevices[deviceIndex];
                lock (_deviceUpdateLock)
                {
                    if (_keyboardDevices.Remove(keyInfo))
                    {
                        _deviceDetachedCallback?.Invoke(keyInfo);
                    }
                }
                _log.Print($"Keyboard input device '{keyInfo.Description}' on HID path {keyInfo.DeviceName}, class {keyInfo.DeviceClass} was removed.", LoggingLevel.Verbose);
                break;
            case RID_DEVICE_INFO_TYPE.RIM_TYPEHID:
                GamingDeviceInfo gamingDeviceInfo = _gameDevices[deviceIndex];
                lock (_deviceUpdateLock)
                {
                    if (_gameDevices.Remove(gamingDeviceInfo))
                    {
                        _deviceDetachedCallback?.Invoke(gamingDeviceInfo);
                    }

                    gamingDeviceInfo.XInputSlot = -1;
                }
                _log.Print($"The gaming input device '{gamingDeviceInfo.Description}' on HID path {gamingDeviceInfo.DeviceName}, class {gamingDeviceInfo.DeviceClass} has been removed.", LoggingLevel.Verbose);
                break;
        }
    }

    /// <summary>
    /// Function called when a device has been added to the system.
    /// </summary>
    /// <param name="deviceHandle">The handle for the device.</param>
    private void HandleDeviceAttached(HANDLE deviceHandle)
    {
        int gameDeviceIndex = GetGamingDeviceIndex(deviceHandle);
        int keyboardDeviceIndex = GetKeyboardIndex(deviceHandle);
        int mouseDeviceIndex = GetMouseIndex(deviceHandle);

        // If we've already registered this device in one of the collections (we can't know which one was registered at this point), then move on.
        if ((gameDeviceIndex != -1) || (keyboardDeviceIndex != -1) || (mouseDeviceIndex != -1))
        {
            return;
        }

        RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices();

        // Found our device.
        RAWINPUTDEVICELIST addedDevice = devices.FirstOrDefault(item => item.hDevice == deviceHandle);

        if (addedDevice.hDevice.IsNull)
        {
            _log.PrintError($"Failed to locate the newly added device with handle 0x{((nint)deviceHandle).FormatHex()}.", LoggingLevel.Simple);
            return;
        }

        (RID_DEVICE_INFO deviceInfo, string deviceName, string className, string deviceDescription) = GetDeviceData(ref addedDevice);

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return;
        }

        _log.Print($"New {addedDevice.dwType} device {deviceName} has been attached to the system.", LoggingLevel.Intermediate);

        switch (deviceInfo.dwType)
        {
            case RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE:
                MouseInfo mouseInfo = new(deviceHandle, deviceName, className, deviceDescription, (int)deviceInfo.Anonymous.mouse.dwSampleRate, (int)deviceInfo.Anonymous.mouse.dwNumberOfButtons);

                lock (_deviceUpdateLock)
                {
                    _mouseDevices.Add(mouseInfo);
                    _log.Print($"Found mouse input device '{deviceDescription}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);
                    _deviceAttachedCallback?.Invoke(mouseInfo);
                }
                break;
            case RID_DEVICE_INFO_TYPE.RIM_TYPEKEYBOARD:
                KeyboardInfo keyboardInfo = new(deviceHandle, deviceName, className, deviceDescription, (int)deviceInfo.Anonymous.keyboard.dwNumberOfFunctionKeys, (KeyboardType)deviceInfo.Anonymous.keyboard.dwType);

                lock (_deviceUpdateLock)
                {
                    _keyboardDevices.Add(keyboardInfo);
                    _log.Print($"Found keyboard input device '{deviceDescription}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);
                    _deviceAttachedCallback?.Invoke(keyboardInfo);
                }
                break;
            case RID_DEVICE_INFO_TYPE.RIM_TYPEHID:
                GamingDeviceInfo gamingDeviceInfo = GamingDeviceInfo.GetGamingDeviceInfo(deviceName, className, deviceDescription, ref addedDevice, ref deviceInfo, _log);

                if (gamingDeviceInfo == GamingDeviceInfo.Empty)
                {
                    _log.PrintError("Failed to add the newly attached gaming device to the system.", LoggingLevel.Simple);
                    return;
                }

                lock (_deviceUpdateLock)
                {
                    if (_gameDevices.Count > 0)
                    {
                        // Force the list to be ordered by the device name.
                        IEnumerable<GamingDeviceInfo> orderedList = _gameDevices.ToArray().Append(gamingDeviceInfo).OrderBy(d => d.DeviceName);
                        _gameDevices.Clear();
                        _gameDevices.AddRange(orderedList);
                    }
                    else
                    {
                        _gameDevices.Add(gamingDeviceInfo);
                    }

                    _log.Print($"Found {(HIDUsage)deviceInfo.Anonymous.hid.usUsage} input device '{gamingDeviceInfo.Description}' on HID path {deviceName}, class {className}.", LoggingLevel.Intermediate);
                    _deviceAttachedCallback?.Invoke(gamingDeviceInfo);
                }
                break;
        }
    }

    /// <summary>
    /// Function used to loop on the input thread and provide updates.
    /// </summary>
    /// <param name="flags">The flags used to initialize the raw input system.</param>
    private void InputThread(InputFlags flags)
    {
        _log.Print("Starting input thread...", LoggingLevel.Simple);

        _cancelSource = new CancellationTokenSource();
        _window = new InputWindow(HandleDeviceAttached, HandleDeviceDetached);
        _shutdownNotification = new ManualResetEventSlim(false);

        Enable(flags);

        Queue<RAWINPUT> rawInputBuffer = new();
        GorgonTimerQpc timer = new();

        long lastTime = 0;

        try
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                // Notify the caller that is waiting on the events.
                PInvoke.MessageWaitForMultipleObjects([], QUEUE_STATUS_FLAGS.QS_RAWINPUT, _cancelSource.Token);

                long startTime = (long)timer.Microseconds;
                long delta = startTime - lastTime;
                long increment = 0;
                long currentTime = startTime;

                // If we get wrap around, then readjust.
                if (delta < 0)
                {
                    delta = (startTime - delta).Min(MaxTimeInQueue - 1);
                }

                // Reset the flag so that the wait for multiple objects function will wait on the next go around.
                _ = PInvoke.GetQueueStatus(QUEUE_STATUS_FLAGS.QS_RAWINPUT);

                // If we cancelled, exit the loop.
                if (_cancelSource.IsCancellationRequested)
                {
                    break;
                }

                // Check for device registrations, if we have none at the moment, then continue and wait until we do.
                if (_devicesRegistered == 0)
                {
                    lastTime = startTime;
                    continue;
                }

                RawInputApi.GetRawInputBuffer(rawInputBuffer, _cancelSource.Token);

                if ((rawInputBuffer.Count > 1) && (delta <= 100_000_000))
                {
                    increment = delta / rawInputBuffer.Count;
                    currentTime = lastTime;
                }

                // If there's nothing to write, then we are done.
                if (rawInputBuffer.Count == 0)
                {
                    lastTime = startTime;
                    continue;
                }

                PurgeOldEvents(currentTime);

                GatherRawInputEvents(currentTime, increment, rawInputBuffer, _cancelSource.Token);

                // If we cancelled, exit the thread.
                if (_cancelSource.IsCancellationRequested)
                {
                    break;
                }

                lastTime = startTime;
            }

            _log.Print("Input thread shut down.", LoggingLevel.Simple);
        }
        catch (OperationCanceledException)
        {
            _log.Print("Input thread shut down.", LoggingLevel.Simple);
        }
        finally
        {
            // Get rid of the window we use for receiving raw input messages.
            InputWindow? window = Interlocked.Exchange(ref _window, null);
            window?.Dispose();

            // Notify the dispose method that it's safe to dispose of the notifiers now.
            _shutdownNotification?.Set();
        }
    }

    /// <summary>
    /// Function to start up the thread used to listen for input data.
    /// </summary>
    private void StartInputThread(InputFlags flags)
    {
        void ThreadStartMethod() => InputThread(flags);

        _inputThread = new Thread(ThreadStartMethod)
        {
            Name = $"GorgonInputThread-{Guid.NewGuid()}",
            Priority = ThreadPriority.Highest,
            IsBackground = true
        };
        _inputThread.Start();
    }

    /// <inheritdoc/>
    public void Disable()
    {
        if ((_cancelSource is null) || (_cancelSource.IsCancellationRequested))
        {
            return;
        }

        if (Interlocked.Exchange(ref _devicesRegistered, 0) == 0)
        {
            return;
        }

        DisableRawInput();

        _inputEventPool.Reset();
        _rawReadings.Clear();
        _gameDevices.Clear();
        _mouseDevices.Clear();
        _keyboardDevices.Clear();

        _log.Print("Input system disabled.", LoggingLevel.Intermediate);
    }

    /// <inheritdoc/>
    public void Enable(InputFlags flags)
    {
        if ((_window is null)
            || (_cancelSource is null)
            || (_cancelSource.IsCancellationRequested))
        {
            return;
        }

        // If we change the flags, disable so we can reset the bindings.
        if (flags != _flags)
        {
            Disable();
        }

        if (Interlocked.Exchange(ref _devicesRegistered, 1) == 1)
        {
            return;
        }

        if ((_keyboardDevices.Count == 0) && (_mouseDevices.Count == 0) && (_gameDevices.Count == 0))
        {
            EnumerateRawInputDevices(flags);
        }

        EnableRawInput(flags);

        _log.Print($"Input system enabled {flags}.", LoggingLevel.Intermediate);
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonInput"/> object.
    /// </summary>
    /// <param name="flags">The flags used to define how devices should be registered with the input system.</param>
    /// <param name="log">The log used for debug messaging.</param>
    /// <returns>A new <see cref="GorgonInput"/> object.</returns>
    /// <remarks>
    /// <para>
    /// This method will create a new single instance of the <see cref="GorgonInput"/> type. Any subsequent calls to this method will always return the same instance. This is because the underlying system 
    /// (Raw Input) is a singleton system, and can only be used once within a process until it's disabled again.
    /// </para>
    /// <para>
    /// The <paramref name="flags"/> can be set to indicate which devices are read by the input system, and whether or not these devices are exclusive (keyboard/mouse only). To initialize the input system 
    /// without binding the input devices up front, pass in the <see cref="InputFlags.None"/> flag to the method and call the <see cref="Enable(InputFlags)"/> method when ready. 
    /// </para>
    /// <para>
    /// When a keyboard or mouse is set as exclusive, then the application will no longer respond to windows messages from these devices. This will cause the mouse to appear to freeze (the cursor at least), 
    /// and the keyboard will stop responding to the various system keypresses for a window (e.g. Alt+F4 will be disabled). Regardless of whether these devices are exclusive or not, the application will 
    /// no longer receive messages from them when the application is not in focus. Note that joysticks and gamepads cannot be set as exclusive and messages from these devices will always be received 
    /// regardless of whether the application is in the foreground or not.
    /// </para>
    /// </remarks>
    /// <seealso cref="Enable(InputFlags)"/>
    /// <seealso cref="Disable"/>
    public static IGorgonInput CreateInput(InputFlags flags, IGorgonLog? log = null)
    {
        lock (_inputCreateLock)
        {
            if (_instance is null)
            {
                _instance = new GorgonInput(log ?? GorgonLog.NullLog, flags);
                _instance.EnumerateRawInputDevices(flags);
                _instance.StartInputThread(flags);
            }

            return _instance;
        }
    }

    /// <inheritdoc/>
    public int GetInput(InputDeviceType deviceType, GorgonInputEventBuffer events)
    {
        if ((_cancelSource is null)
            || (_cancelSource.IsCancellationRequested)
            || (_devicesRegistered == 0)
            || (events.BufferLength == 0)
            || (deviceType == InputDeviceType.None))
        {
            return 0;
        }

        try
        {
            events.AddEvents(_rawReadings, deviceType);

            return events.MouseEventCount + events.KeyboardEventCount + events.GamingDeviceEventCount;
        }
        catch
        {
            _cancelSource?.Cancel();
            throw;
        }
    }

    /// <inheritdoc/>
    public void RegisterInputEventCallback(Func<GorgonInputEvent, bool> inputEvent) => _inputEventCallback = inputEvent;

    /// <inheritdoc/>
    public void RegisterDeviceChangeCallbacks(Action<IGorgonInputDeviceInfo>? deviceAttached, Action<IGorgonInputDeviceInfo>? deviceDetached)
    {
        _deviceAttachedCallback = deviceAttached;
        _deviceDetachedCallback = deviceDetached;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for the object.
    /// </summary>
    ~GorgonInput()
    {
        Dispose(false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonInput"/> class.
    /// </summary>
    /// <param name="log">The log for debug messaging.</param>
    /// <param name="flags">The flags for device registration.</param>
    private GorgonInput(IGorgonLog log, InputFlags flags)
    {
        _log = log;
        _flags = flags;

        _log.Print($"Initializing Gorgon Input with flags {_flags} ...", LoggingLevel.Simple);
        _log.Print("Initializing RawInput...", LoggingLevel.Verbose);
    }

    /// <summary>
    /// Initializes the static instance of the class.
    /// </summary>
    static GorgonInput()
    {
        if (!OperatingSystem.IsOSPlatformVersionAtLeast("Windows", 10, 0, 0))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORINP_ERR_INVALID_OS));
        }
    }
}