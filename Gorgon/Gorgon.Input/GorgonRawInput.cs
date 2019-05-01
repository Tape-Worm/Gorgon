#region MIT
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Tuesday, September 07, 2015 1:45:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <summary>
	/// Raw Input functionality for keyboards, mice and human interface devices.
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
	/// type (e.g. multiple mice) to be used individually.
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
	///    // The 'yourMainApplicationWindow' is the primary window used by your application.
	///    _rawInput = new GorgonRawInput(yourMainApplicationWindow);
	/// 
	///    _mouse = new GorgonRawMouse();
	/// 
	///    _rawInput.RegisterDevice(_mouse);
	/// 
	///	   // Configure your mouse object for events here...
	/// }
	/// 
	/// private void ApplicationShutDown()
	/// {
	///		// The device should be unregistered as soon as it's no longer needed.
	///     _rawInput.UnregisterDevice(_mouse);
	/// 
	///		// Always dispose this object, otherwise message hooks may still persist and cause issues.
	///     _rawInput.Dispose();
	/// }
	/// ]]>
	/// </code>
	/// </example>
	public class GorgonRawInput
		: IGorgonRawInput
	{
		#region Variables.
		// Our message used to route raw input messages.
		private RawInputMessageFilter _filter;
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		// Synchronization lock for threads.
		private readonly object _syncLock = new object();
		// The list of registered devices.
		private readonly Dictionary<DeviceKey, IGorgonRawInputDevice> _devices;
		// The window that is receiving raw input events.
		private readonly IntPtr _applicationWindow;
		// Flag to indicate whether the native window hook is used, or Application.AddFilter is used.
		private readonly bool _useNativeHook;
        // The list of human interface devices registered on the system.
	    private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawHIDData>> _hids = new Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawHIDData>>();
        // The list of keyboard devices registered on the system.
	    private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawKeyboardData>> _keyboardDevices = new Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawKeyboardData>>();
        // The list of pointing devices registered on the system.
	    private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawMouseData>> _mouseDevices = new Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawMouseData>>();
		#endregion

		#region Methods.
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

			switch (deviceInfo.dwType)
			{
				case RawInputType.Keyboard:
					return new RawKeyboardInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.keyboard) as T;
				case RawInputType.Mouse:
					return new RawMouseInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.mouse) as T;
				case RawInputType.HID:
					return new GorgonRawHIDInfo(device.Device, deviceName, className, deviceDescription, deviceInfo.hid) as T;
				default:
					return null;
			}
		}

        /// <summary>
        /// Function to register the device with the raw input provider.
        /// </summary>
        /// <param name="device">The device to register with the raw input provider.</param>
        /// <param name="settings">[Optional] Settings for the device type.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will register the <see cref="IGorgonRawInputDevice"/> with the application. For the very first device of a specific type (e.g. a mouse, keyboard, etc...) the Raw Input object will set up 
        /// the device type registration for the device. This enables an application to start receiving Raw Input messages from a device type.
        /// </para>
        /// <para>
        /// The optional <paramref name="settings"/> parameter allows an application change how raw input handles the device being registered. It can be used to set up background input monitoring, or a 
        /// target window for raw input messages (which must be set if the background option is turned on). By default, there is no background message processing and no target window (messages go to 
        /// whichever window has focus).
        /// </para>
        /// <para>
        /// Every call to this method should be paired with a call to <see cref="UnregisterDevice"/> when the device(s) are no longer needed.
        /// </para>
        /// </remarks>
        public void RegisterDevice(IGorgonRawInputDevice device, GorgonRawInputSettings? settings = null)
		{
			IntPtr targetHandle = IntPtr.Zero;

			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			if (settings == null)
			{
				settings = new GorgonRawInputSettings();
			}

			lock (_syncLock)
			{
				// If we've not set up the filter yet, then add it to the window now.
				if (_filter == null)
				{
				    _filter = new RawInputMessageFilter(_keyboardDevices, _mouseDevices, _hids,_applicationWindow , _useNativeHook);
				}

				var key = new DeviceKey
				          {
					          DeviceType = device.DeviceType,
					          DeviceHandle = device.Handle
				          };

				if (_devices.ContainsKey(key))
				{
					return;
				}

			    switch (device.DeviceType)
			    {
                    case RawInputType.Keyboard:
                        _keyboardDevices.Add(key, (IRawInputDeviceData<GorgonRawKeyboardData>)device);
                        break;
                    case RawInputType.Mouse:
                        _mouseDevices.Add(key, (IRawInputDeviceData<GorgonRawMouseData>)device);
                        break;
			        case RawInputType.HID:
                        _hids.Add(key, (IRawInputDeviceData<GorgonRawHIDData>)device);
			            break;
                    default:
                        throw new ArgumentException(string.Format(Resources.GORINP_RAW_ERR_UNKNOWN_DEVICE_TYPE, device.DeviceType), nameof(device));
			    }

				_devices.Add(key, device);

				// Get the current device registration properties.
				RAWINPUTDEVICE? deviceReg = RawInputApi.GetDeviceRegistration(device.DeviceUsage);
				if (deviceReg != null)
				{
					// Remove the device before updating it.
					UnregisterDevice(device);
				}
				
				// If we omit the target window, and specify background messages, we'll use the application window instead.
				// This is because Raw Input requires that background devices have a window target.
				if ((settings.Value.TargetWindow.IsNull) && (settings.Value.AllowBackground))
				{
				    unsafe
				    {
				        settings = new GorgonRawInputSettings
				                   {
				                       TargetWindow = new GorgonReadOnlyPointer((void *)_applicationWindow, IntPtr.Size),
				                       AllowBackground = true
				                   };
				    }
				}

				RawInputDeviceFlags flags = RawInputDeviceFlags.None;
				
				if (settings.Value.AllowBackground)
				{
					flags |= RawInputDeviceFlags.InputSink;
				}

				RawInputApi.RegisterRawInputDevice(device.DeviceUsage, targetHandle, flags);
			}
		}

		/// <summary>
		/// Function to unregister the device from the raw input provider.
		/// </summary>
		/// <param name="device">The device to unregister from the raw input provider.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// This will unregister a previously registered <see cref="IGorgonRawInputDevice"/>. When the last device of a specific type (e.g. a mouse, keyboard, etc...) is unregistered, then the 
		/// Raw Input messages for that device type will also be unregistered and the application will no longer receive messages from that type of device.
		/// </remarks>
		public void UnregisterDevice(IGorgonRawInputDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			lock (_syncLock)
			{
				var key = new DeviceKey
				          {
					          DeviceType = device.DeviceType,
					          DeviceHandle = device.Handle
				          };

			    switch (device.DeviceType)
			    {
			        case RawInputType.Keyboard:
			            _keyboardDevices.Remove(key);
			            break;
			        case RawInputType.Mouse:
			            _mouseDevices.Remove(key);
			            break;
			        case RawInputType.HID:
			            _hids.Remove(key);
			            break;
			        default:
			            throw new ArgumentException(string.Format(Resources.GORINP_RAW_ERR_UNKNOWN_DEVICE_TYPE, device.DeviceType), nameof(device));
			    }

				if (!_devices.ContainsKey(key))
				{
					return;
				}

				_devices.Remove(key);
                
				// If all devices of this type have been unregistered, unregister with raw input as well.
				if ((_devices.Count(item => item.Value.DeviceType == device.DeviceType) == 0)
					&& (RawInputApi.GetDeviceRegistration(device.DeviceUsage) != null))
				{
					RawInputApi.UnregisterRawInputDevice(device.DeviceUsage);
				}

				// If we have no more registered devices, then uninstall the filter.
				if (_devices.Count != 0)
				{
					return;
				}

				_filter?.Dispose();
				_filter = null;
			}
		}

		/// <summary>
		/// Function to retrieve a list of mice.
		/// </summary>
		/// <returns>A read only list containing information about each mouse.</returns>
		public IReadOnlyList<IGorgonMouseInfo> EnumerateMice()
		{
			RAWINPUTDEVICELIST[] devices = RawInputApi.EnumerateRawInputDevices(RawInputType.Mouse);
			var result = new List<RawMouseInfo>();

			for (int i = 0; i < devices.Length; i++)
			{
				RawMouseInfo info = GetDeviceInfo<RawMouseInfo>(ref devices[i]);

				if (info == null)
				{
					_log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				_log.Print("Found mouse: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.DeviceClass);

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
			var result = new List<RawKeyboardInfo>();

			for (int i = 0; i < devices.Length; i++)
			{
				RawKeyboardInfo info = GetDeviceInfo<RawKeyboardInfo>(ref devices[i]);

				if (info == null)
				{
					_log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				_log.Print("Found keyboard: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.DeviceClass);

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
			var result = new List<GorgonRawHIDInfo>();

			for (int i = 0; i < devices.Length; i++)
			{
				GorgonRawHIDInfo info = GetDeviceInfo<GorgonRawHIDInfo>(ref devices[i]);

				if (info == null)
				{
					_log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				_log.Print("Found human interface device: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.DeviceClass);

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
				_filter?.Dispose();
				_filter = null;
			}

			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Finalizer.
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
		public GorgonRawInput(Control applicationWindow, IGorgonLog log = null)
		{
		    _log = log ?? GorgonLog.NullLog;
		    _applicationWindow = applicationWindow?.Handle ?? throw new ArgumentNullException(nameof(applicationWindow));
		    _devices = new Dictionary<DeviceKey, IGorgonRawInputDevice>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawInput"/> class.
		/// </summary>
		/// <param name="windowHandle">The handle to the main application window.</param>
		/// <param name="log">[Optional] The logger used for debugging.</param>
		/// <exception cref="ArgumentNullException">thrown when the <paramref name="windowHandle"/> is set to <see cref="IntPtr.Zero"/>.</exception>
		/// <remarks>
		/// <para>
		/// This constructor will allow any window handle to use a <see cref="GorgonRawInput"/> object. This allows WPF and other windowing systems to work with raw input. 
		/// </para>
		/// <para>
		/// The <paramref name="windowHandle"/> parameter is required in order to set up the application to receive <c>WM_INPUT</c> messages. Ideally, this window should be the primary application window.
		/// </para>
		/// </remarks>
		public GorgonRawInput(GorgonReadOnlyPointer windowHandle, IGorgonLog log = null)
		{
			if (windowHandle.IsNull)
			{
				throw new ArgumentNullException(nameof(windowHandle));
			}

			_log = log ?? GorgonLog.NullLog;
		    unsafe
		    {
		        _applicationWindow = new IntPtr((void*)windowHandle);
		    }
		    _devices = new Dictionary<DeviceKey, IGorgonRawInputDevice>();
			_useNativeHook = true;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonRawInput" /> class.
		/// </summary>
		~GorgonRawInput()
		{
			UnhookRawInput();
		}
		#endregion
	}
}
