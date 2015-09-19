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
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <inheritdoc/>
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

			RID_DEVICE_INFO rawDeviceInfo = RawInputApi.GetDeviceInfo(ref device);

			switch (deviceInfo.dwType)
			{
				case RawInputType.Keyboard:
					return new RawKeyboardInfo(device.Device, deviceName, className, deviceDescription, rawDeviceInfo.keyboard) as T;
				case RawInputType.Mouse:
					return new RawMouseInfo(device.Device, deviceName, className, deviceDescription, rawDeviceInfo.mouse) as T;
				case RawInputType.HID:
					return new GorgonRawHIDInfo(device.Device, deviceName, className, deviceDescription, rawDeviceInfo.hid) as T;
				default:
					return null;
			}
		}

		/// <inheritdoc/>
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
					_filter = new RawInputMessageFilter(_devices, _applicationWindow, _useNativeHook);
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
				if ((settings.Value.TargetWindow == IntPtr.Zero) && (settings.Value.AllowBackground))
				{
					settings = new GorgonRawInputSettings
					           {
						           TargetWindow = _applicationWindow,
						           AllowBackground = true
					           };
				}

				RawInputDeviceFlags flags = RawInputDeviceFlags.None;
				
				if (settings.Value.AllowBackground)
				{
					flags |= RawInputDeviceFlags.InputSink;
				}

				RawInputApi.RegisterRawInputDevice(device.DeviceUsage, targetHandle, flags);
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

				_log.Print("Found mouse: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.Class);

				result.Add(info);
			}

			return result;
		}

		/// <inheritdoc/>
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

				_log.Print("Found keyboard: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.Class);

				result.Add(info);
			}

			return result;
		}

		/// <inheritdoc/>
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

				_log.Print("Found human interface device: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HIDPath, info.Class);

				result.Add(info);
			}

			return result;
		}

		/// <inheritdoc/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_filter")]
		public void Dispose()
		{
			// Multiple threads should -not- call dispose, but just in case someone thinks they're smart...
			lock (_syncLock)
			{
				UnhookRawInput();

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
		/// <exception cref="ArgumentNullException">thrown when the <paramref name="applicationWindow"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
			if (applicationWindow == null)
			{
				throw new ArgumentNullException(nameof(applicationWindow));
			}

			_log = log ?? new GorgonLogDummy();
			_applicationWindow = applicationWindow.Handle;
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
		public GorgonRawInput(IntPtr windowHandle, IGorgonLog log = null)
		{
			if (windowHandle == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(windowHandle));
			}

			_log = log ?? new GorgonLogDummy();
			_applicationWindow = windowHandle;
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
