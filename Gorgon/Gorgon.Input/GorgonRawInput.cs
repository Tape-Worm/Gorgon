
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, September 07, 2015 1:45:21 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// Raw Input functionality for keyboards, mice and human interface devices
/// </summary>
/// <remarks>
/// <para>
/// This enables use of the <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645536(v=vs.85).aspx">Raw Input</a> functionality provided by windows. 
/// </para>
/// <para>
/// This object will allow for enumeration of multiple keyboard, mouse and human interface devices attached to the system and will allow an application to register these types of devices for use with the 
/// application.  
/// </para>
/// <para>
/// The <see cref="GorgonRawInput"/> object will also coordinate <c>WM_INPUT</c> messages and forward Raw Input data to an appropriate raw input device. This is done to allow multiple devices of the same 
/// type (e.g. multiple mice) to be used individually
/// </para>
/// </remarks>
/// <example>
/// The following code shows how to create a mouse device and register it with the <see cref="GorgonRawInput"/> object for use in an application:
/// <code language="csharp">
/// <![CDATA[
/// private GorgonRawMouse _mouse;
/// private GorgonRawInput _rawInput;
/// 
/// private void CreateRawMouse(Control yourMainApplicationWindow)
/// {
///    // The 'yourMainApplicationWindow' is the primary window used by your application
///    _rawInput = new GorgonRawInput(yourMainApplicationWindow);
/// 
///    _mouse = new GorgonRawMouse();
/// 
///    _rawInput.RegisterDevice(_mouse);
/// 
///	   // Configure your mouse object for events here..
/// }
/// 
/// private void ApplicationShutDown()
/// {
///		// The device should be unregistered as soon as it's no longer needed
///     _rawInput.UnregisterDevice(_mouse);
/// 
///		// Always dispose this object, otherwise message hooks may still persist and cause issues
///     _rawInput.Dispose();
/// }
/// ]]>
/// </code>
/// </example>
public class GorgonRawInput
    : IGorgonRawInput
{

    // Our message used to route raw input messages.
    private Lazy<RawInputMessageFilter> _filter;
    // The logger used for debugging.
    private readonly IGorgonLog _log;
    // Synchronization lock for threads.
    private readonly object _syncLock = new();
    // The list of registered devices.
    private readonly Dictionary<DeviceKey, IGorgonRawInputDevice> _devices;
    // The handle to the window that is receiving raw input events.
    private readonly nint _applicationWindow;
    // The list of human interface devices registered on the system.
    private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawHIDData>> _hids = [];
    // The list of keyboard devices registered on the system.
    private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawKeyboardData>> _keyboardDevices = [];
    // The list of pointing devices registered on the system.
    private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawMouseData>> _mouseDevices = [];

    /// <summary>
    /// Function to unhook raw input from the application.
    /// </summary>
    private void UnhookRawInput()
    {
        foreach (HIDUsage type in _devices.Values.Select(item => item.DeviceUsage).Distinct())
        {
            RawInputApi.UnregisterRawInputDevice(type);
        }
    }

    /// <summary>
    /// Function to retrieve the device name.
    /// </summary>
    /// <param name="device">Raw input device to gather information from.</param>
    /// <returns>A device name structure.</returns>
    private T GetDeviceInfo<T>(ref RAWINPUTDEVICELIST device)
        where T : class, IGorgonRawInputDeviceInfo
    {
        RID_DEVICE_INFO deviceInfo = RawInputApi.GetDeviceInfo(ref device);

        string deviceName = RawInputApi.GetDeviceName(ref device);

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return null;
        }

        string className = RawInputDeviceRegistryInfo.GetDeviceClass(deviceName, _log);
        string deviceDescription = RawInputDeviceRegistryInfo.GetDeviceDescription(deviceName, _log);

        return deviceInfo.dwType switch
        {
            RawInputType.Keyboard => new RawKeyboardInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.keyboard) as T,
            RawInputType.Mouse => new RawMouseInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.mouse) as T,
            RawInputType.HID => new GorgonRawHIDInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.hid) as T,
            _ => null,
        };
    }

    /// <summary>
    /// Function to initialize registration for a raw input device.
    /// </summary>
    /// <param name="device">The device to be registered.</param>
    /// <param name="key">The unique key for the device.</param>
    /// <returns><b>true</b> if the device can be registered, <b>false</b> if the device was previously registered.</returns>
    private bool InitRegistration(IGorgonRawInputDevice device, out DeviceKey key)
    {
        key = new DeviceKey
        {
            DeviceType = device.DeviceType,
            DeviceHandle = device.Handle
        };

        return !_devices.ContainsKey(key);
    }

    /// <summary>
    /// Function to unregister the device from the raw input system and shut down the raw input filter when no devices are available.
    /// </summary>
    /// <param name="key">The key for the device.</param>
    /// <param name="device">The device to remove.</param>
    private void Unregister(DeviceKey key, IGorgonRawInputDevice device)
    {
        if (!_devices.Remove(key))
        {
            return;
        }

        // If all devices of this type have been unregistered, unregister with raw input as well.
        if ((!_devices.Any(item => item.Value.DeviceType == device.DeviceType))
            && (RawInputApi.GetDeviceRegistration(device.DeviceUsage) is not null))
        {
            RawInputApi.UnregisterRawInputDevice(device.DeviceUsage);
        }

        // If we have no more registered devices, then uninstall the filter.
        if (_devices.Count != 0)
        {
            return;
        }

        if (_filter.IsValueCreated)
        {
            MessageFilterHook.RemoveFilter(_applicationWindow, _filter.Value);
        }
    }

    /// <summary>
    /// Function to register a raw input device with the raw input system.
    /// </summary>
    /// <param name="device">The device to register.</param>
    /// <param name="key">The unique key for the device.</param>
    /// <param name="settings">The settings for the device.</param>
    private void Register(IGorgonRawInputDevice device, DeviceKey key, GorgonRawInputSettings settings)
    {
        MessageFilterHook.AddFilter(_applicationWindow, _filter.Value);

        nint targetHandle = IntPtr.Zero;
        _devices.Add(key, device);

        // If we omit the target window, and specify background messages, we'll use the application window instead.
        // This is because Raw Input requires that background devices have a window target.
        if ((settings.TargetWindow == IntPtr.Zero) && (settings.AllowBackground))
        {
            settings = new GorgonRawInputSettings
            {
                TargetWindow = _applicationWindow,
                AllowBackground = true
            };
        }

        RawInputDeviceFlags flags = RawInputDeviceFlags.None;

        if (settings.AllowBackground)
        {
            flags |= RawInputDeviceFlags.InputSink;
        }

        RawInputApi.RegisterRawInputDevice(device.DeviceUsage, targetHandle, flags);
    }

    /// <summary>
    /// Function to register a mouse device with the raw input provider.
    /// </summary>
    /// <param name="device">The mouse device to register with the raw input provider.</param>
    /// <param name="settings">[Optional] Settings for the device type.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will register the <see cref="IGorgonMouse"/> device with the application. For the very first device the Raw Input object will set up the registration for the mouse. This enables an 
    /// application to start receiving Raw Input messages from a mouse.
    /// </para>
    /// <para>
    /// The optional <paramref name="settings"/> parameter allows an application change how raw input handles the device being registered. It can be used to set up background input monitoring, or a 
    /// target window for raw input messages (which must be set if the background option is turned on). By default, there is no background message processing and no target window (messages go to 
    /// whichever window has focus).
    /// </para>
    /// <para>
    /// Every call to this method should be paired with a call to <see cref="UnregisterDevice(IGorgonMouse)"/> when the mouse is no longer needed.
    /// </para>
    /// </remarks>
    public void RegisterDevice(IGorgonMouse device, GorgonRawInputSettings? settings = null)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        settings ??= default;

        lock (_syncLock)
        {
            if (!InitRegistration(device, out DeviceKey key))
            {
                return;
            }

            // Get the current device registration properties.
            RAWINPUTDEVICE? deviceReg = RawInputApi.GetDeviceRegistration(device.DeviceUsage);
            if (deviceReg is not null)
            {
                // Remove the device before updating it.
                UnregisterDevice(device);
            }

            _mouseDevices.Add(key, device);

            Register(device, key, settings.Value);
        }
    }

    /// <summary>
    /// Function to register a keyboard device with the raw input provider.
    /// </summary>
    /// <param name="device">The keyboard device to register with the raw input provider.</param>
    /// <param name="settings">[Optional] Settings for the device type.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will register the <see cref="IGorgonKeyboard"/> device with the application. For the very first device the Raw Input object will set up the registration for the keyboard. This enables an 
    /// application to start receiving Raw Input messages from a keyboard.
    /// </para>
    /// <para>
    /// The optional <paramref name="settings"/> parameter allows an application change how raw input handles the device being registered. It can be used to set up background input monitoring, or a 
    /// target window for raw input messages (which must be set if the background option is turned on). By default, there is no background message processing and no target window (messages go to 
    /// whichever window has focus).
    /// </para>
    /// <para>
    /// Every call to this method should be paired with a call to <see cref="UnregisterDevice(IGorgonKeyboard)"/> when the keyboard is no longer needed.
    /// </para>
    /// </remarks>
    public void RegisterDevice(IGorgonKeyboard device, GorgonRawInputSettings? settings = null)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        settings ??= default;

        lock (_syncLock)
        {
            if (!InitRegistration(device, out DeviceKey key))
            {
                return;
            }

            // Get the current device registration properties.
            RAWINPUTDEVICE? deviceReg = RawInputApi.GetDeviceRegistration(device.DeviceUsage);
            if (deviceReg is not null)
            {
                // Remove the device before updating it.
                UnregisterDevice(device);
            }

            _keyboardDevices.Add(key, device);

            Register(device, key, settings.Value);
        }
    }

    /// <summary>
    /// Function to register a HID with the raw input provider.
    /// </summary>
    /// <param name="device">The HID to register with the raw input provider.</param>
    /// <param name="settings">[Optional] Settings for the device type.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will register the <see cref="IGorgonRawHID"/> device with the application. For the very first device the Raw Input object will set up the registration for the HID. This enables an 
    /// application to start receiving Raw Input messages from a HID.
    /// </para>
    /// <para>
    /// The optional <paramref name="settings"/> parameter allows an application change how raw input handles the device being registered. It can be used to set up background input monitoring, or a 
    /// target window for raw input messages (which must be set if the background option is turned on). By default, there is no background message processing and no target window (messages go to 
    /// whichever window has focus).
    /// </para>
    /// <para>
    /// Every call to this method should be paired with a call to <see cref="UnregisterDevice(IGorgonRawHID)"/> when the HID is no longer needed.
    /// </para>
    /// </remarks>
    public void RegisterDevice(IGorgonRawHID device, GorgonRawInputSettings? settings = null)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        settings ??= default;

        lock (_syncLock)
        {
            if (!InitRegistration(device, out DeviceKey key))
            {
                return;
            }

            // Get the current device registration properties.
            RAWINPUTDEVICE? deviceReg = RawInputApi.GetDeviceRegistration(device.DeviceUsage);
            if (deviceReg is not null)
            {
                // Remove the device before updating it.
                UnregisterDevice(device);
            }

            _hids.Add(key, device);

            Register(device, key, settings.Value);
        }
    }

    /// <summary>
    /// Function to unregister a mouse from the raw input provider.
    /// </summary>
    /// <param name="device">The mouse to unregister from the raw input provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will unregister a previously registered <see cref="IGorgonMouse"/>. When the last mouse is unregistered, then the Raw Input messages for that device type will also be unregistered 
    /// and the application will no longer receive messages from any mouse.
    /// </para>
    /// </remarks>
    public void UnregisterDevice(IGorgonMouse device)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        lock (_syncLock)
        {
            DeviceKey key = new()
            {
                DeviceType = device.DeviceType,
                DeviceHandle = device.Handle
            };

            _mouseDevices.Remove(key);

            Unregister(key, device);
        }
    }

    /// <summary>
    /// Function to unregister a keyboard from the raw input provider.
    /// </summary>
    /// <param name="device">The keyboard to unregister from the raw input provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will unregister a previously registered <see cref="IGorgonKeyboard"/>. When the last keyboard is unregistered, then the Raw Input messages for that device type will also be unregistered 
    /// and the application will no longer receive messages from any keyboard.
    /// </para>
    /// </remarks>
    public void UnregisterDevice(IGorgonKeyboard device)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        lock (_syncLock)
        {
            DeviceKey key = new()
            {
                DeviceType = device.DeviceType,
                DeviceHandle = device.Handle
            };

            _keyboardDevices.Remove(key);

            Unregister(key, device);
        }
    }

    /// <summary>
    /// Function to unregister a HID from the raw input provider.
    /// </summary>
    /// <param name="device">The HID to unregister from the raw input provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will unregister a previously registered <see cref="IGorgonRawHID"/>. When the last HID is unregistered, then the Raw Input messages for that device type will also be unregistered 
    /// and the application will no longer receive messages from any HID.
    /// </para>
    /// </remarks>
    public void UnregisterDevice(IGorgonRawHID device)
    {
        if (device is null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        lock (_syncLock)
        {
            DeviceKey key = new()
            {
                DeviceType = device.DeviceType,
                DeviceHandle = device.Handle
            };

            _hids.Remove(key);

            Unregister(key, device);
        }
    }

    /// <summary>
    /// Function to retrieve a list of mice.
    /// </summary>
    /// <returns>A read only list containing information about each mouse.</returns>
    public IReadOnlyList<IGorgonMouseInfo> EnumerateMice()
    {
        RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices(RawInputType.Mouse);
        List<RawMouseInfo> result = [];

        for (int i = 0; i < devices.Length; i++)
        {
            RawMouseInfo info = GetDeviceInfo<RawMouseInfo>(ref devices[i]);

            if (info is null)
            {
                _log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
                continue;
            }

            _log.Print($"Found mouse: '{info.Description}' on HID path {info.HIDPath}, class {info.DeviceClass}.", LoggingLevel.Verbose);

            result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Function to retrieve a list of keyboards.
    /// </summary>
    /// <returns>A read only list containing information about each keyboard.</returns>
    public IReadOnlyList<IGorgonKeyboardInfo> EnumerateKeyboards()
    {
        RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices(RawInputType.Keyboard);
        List<RawKeyboardInfo> result = [];

        for (int i = 0; i < devices.Length; i++)
        {
            RawKeyboardInfo info = GetDeviceInfo<RawKeyboardInfo>(ref devices[i]);

            if (info is null)
            {
                _log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
                continue;
            }

            _log.Print($"Found keyboard: '{info.Description}' on HID path {info.HIDPath}, class {info.DeviceClass}.", LoggingLevel.Verbose);

            result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Function to retrieve a list of human interface devices (HID).
    /// </summary>
    /// <returns>A read only list containing information about each human interface device.</returns>
    public IReadOnlyList<GorgonRawHIDInfo> EnumerateHumanInterfaceDevices()
    {
        RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices(RawInputType.HID);
        List<GorgonRawHIDInfo> result = [];

        for (int i = 0; i < devices.Length; i++)
        {
            GorgonRawHIDInfo info = GetDeviceInfo<GorgonRawHIDInfo>(ref devices[i]);

            if (info is null)
            {
                _log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
                continue;
            }

            _log.Print($"Found human interface device: '{info.Description}' on HID path {info.HIDPath}, class {info.DeviceClass}.", LoggingLevel.Verbose);

            result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Multiple threads should -not- call dispose.
        lock (_syncLock)
        {
            UnhookRawInput();

            _mouseDevices.Clear();
            _keyboardDevices.Clear();
            _hids.Clear();

            _devices.Clear();
            if ((_filter is not null) && (_filter.IsValueCreated))
            {
                if (_applicationWindow != IntPtr.Zero)
                {
                    MessageFilterHook.RemoveFilter(_applicationWindow, _filter.Value);
                }

                _filter.Value.Dispose();
            }
            _filter = null;
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRawInput"/> class.
    /// </summary>
    /// <param name="applicationWindow">The main application window.</param>
    /// <param name="log">[Optional] The logger used for debugging.</param>
    /// <exception cref="ArgumentNullException">thrown when the <paramref name="applicationWindow"/> is set to <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This constructor will only allow Windows Forms controls as the main application window. For other window types, use the overloaded constructor.
    /// </para>
    /// <para>
    /// The <paramref name="applicationWindow"/> parameter is required in order to set up the application to receive <c>WM_INPUT</c> messages. Ideally, this window should be the primary application window.
    /// </para>
    /// </remarks>
    public GorgonRawInput(IWin32Window applicationWindow, IGorgonLog log = null)
    {
        RawInputMessageFilter CreateFilter() => new(_keyboardDevices, _mouseDevices, _hids, _applicationWindow);

        _log = log ?? GorgonLog.NullLog;
        _applicationWindow = applicationWindow?.Handle ?? throw new ArgumentNullException(nameof(applicationWindow));
        _devices = [];
        _filter = new Lazy<RawInputMessageFilter>(CreateFilter, true);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GorgonRawInput" /> class.
    /// </summary>
    ~GorgonRawInput()
    {
        UnhookRawInput();
    }
}
