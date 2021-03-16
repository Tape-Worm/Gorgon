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
// Created: Tuesday, September 08, 2015 12:31:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Input;
using DX = SharpDX;


namespace Gorgon.Native
{
    /// <summary>
    /// Provides a window hook in order to filter raw input messages.
    /// </summary>
    internal class RawInputMessageFilter
        : IDisposable
    {
        #region Variables.
        // Flag to indicate that the message hook has been installed.
        private static int _hookInstalled;
        // The window handle to hook into.
        private static IntPtr _hwnd = IntPtr.Zero;
        // The devices that are registered with the raw input provider.
        private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawKeyboardData>> _keyboardDevices;
        private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawMouseData>> _mouseDevices;
        private readonly Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawHIDData>> _hidDevices;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a raw input HID.
        /// </summary>
        /// <param name="key">The key for the raw input device.</param>
        /// <param name="deviceHandle">The device handle.</param>
        /// <returns></returns>
        private IGorgonRawInputDeviceData<GorgonRawHIDData> GetRawInputHid(DeviceKey key, IntPtr deviceHandle)
        {
            if (_hidDevices.TryGetValue(key, out IGorgonRawInputDeviceData<GorgonRawHIDData> result))
            {
                return result;
            }

            key.DeviceHandle = deviceHandle;

            return !_hidDevices.TryGetValue(key, out result) ? null : result;
        }

        /// <summary>
        /// Function to retrieve a raw input mouse.
        /// </summary>
        /// <param name="key">The key for the raw input device.</param>
        /// <param name="deviceHandle">The device handle.</param>
        /// <returns></returns>
        private IGorgonRawInputDeviceData<GorgonRawMouseData> GetRawInputMouseDevice(DeviceKey key, IntPtr deviceHandle)
        {
            if (_mouseDevices.TryGetValue(key, out IGorgonRawInputDeviceData<GorgonRawMouseData> result))
            {
                return result;
            }

            key.DeviceHandle = deviceHandle;

            return !_mouseDevices.TryGetValue(key, out result) ? null : result;
        }

        /// <summary>
        /// Function to retrieve a raw input keyboard.
        /// </summary>
        /// <param name="key">The key for the raw input device.</param>
        /// <param name="deviceHandle">The device handle.</param>
        /// <returns></returns>
        private IGorgonRawInputDeviceData<GorgonRawKeyboardData> GetRawInputKeyboardDevice(DeviceKey key, IntPtr deviceHandle)
        {
            if (_keyboardDevices.TryGetValue(key, out IGorgonRawInputDeviceData<GorgonRawKeyboardData> result))
            {
                return result;
            }

            key.DeviceHandle = deviceHandle;

            return !_keyboardDevices.TryGetValue(key, out result) ? null : result;
        }

        /// <summary>
        /// Function to dispatch the raw input data to a HID.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        private static void Dispatch(IGorgonRawInputDeviceData<GorgonRawHIDData> device, ref RAWINPUTHID rawData)
        {
            var data = new GorgonRawHIDData(new GorgonPtr<byte>(rawData.Data, rawData.Size * rawData.Count), rawData.Size);
            device.ProcessData(in data);
        }

        /// <summary>
        /// Function to dispatch the raw input to a mouse device.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        private static void Dispatch(IGorgonRawInputDeviceData<GorgonRawMouseData> device, ref RAWINPUTMOUSE rawData)
        {
            short wheelDelta = 0;
            MouseButtonState state = MouseButtonState.None;

            if ((rawData.ButtonFlags & RawMouseButtons.MouseWheel) == RawMouseButtons.MouseWheel)
            {
                wheelDelta = (short)rawData.ButtonData;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.LeftDown) == RawMouseButtons.LeftDown)
            {
                state = MouseButtonState.ButtonLeftDown;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.RightDown) == RawMouseButtons.RightDown)
            {
                state = MouseButtonState.ButtonRightDown;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.MiddleDown) == RawMouseButtons.MiddleDown)
            {
                state = MouseButtonState.ButtonMiddleDown;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.Button4Down) == RawMouseButtons.Button4Down)
            {
                state = MouseButtonState.Button4Down;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.Button5Down) == RawMouseButtons.Button5Down)
            {
                state = MouseButtonState.Button5Down;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.LeftUp) == RawMouseButtons.LeftUp)
            {
                state = MouseButtonState.ButtonLeftUp;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.RightUp) == RawMouseButtons.RightUp)
            {
                state = MouseButtonState.ButtonRightUp;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.MiddleUp) == RawMouseButtons.MiddleUp)
            {
                state = MouseButtonState.ButtonMiddleUp;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.Button4Up) == RawMouseButtons.Button4Up)
            {
                state = MouseButtonState.Button4Up;
            }

            if ((rawData.ButtonFlags & RawMouseButtons.Button5Up) == RawMouseButtons.Button5Up)
            {
                state = MouseButtonState.Button5Up;
            }

            var data = new GorgonRawMouseData(new DX.Point(rawData.LastX, rawData.LastY),
                                              wheelDelta,
                                              state,
                                              ((rawData.Flags & RawMouseFlags.MoveAbsolute) != RawMouseFlags.MoveAbsolute));

            device.ProcessData(in data);
        }

        /// <summary>
        /// Function to dispatch the raw input to a keyboard device.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        private static void Dispatch(IGorgonRawInputDeviceData<GorgonRawKeyboardData> device, ref RAWINPUTKEYBOARD rawData)
        {
            KeyboardDataFlags flags = KeyboardDataFlags.KeyDown;

            if ((rawData.Flags & RawKeyboardFlags.KeyBreak) == RawKeyboardFlags.KeyBreak)
            {
                flags = KeyboardDataFlags.KeyUp;
            }

            if ((rawData.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0)
            {
                flags |= KeyboardDataFlags.LeftKey;
            }

            if ((rawData.Flags & RawKeyboardFlags.KeyE1) == RawKeyboardFlags.KeyE1)
            {
                flags |= KeyboardDataFlags.RightKey;
            }

            // Shift has to be handled in a special case since it doesn't actually detect left/right from raw input.
            if (rawData.VirtualKey == VirtualKeys.Shift)
            {
                flags |= rawData.MakeCode == 0x36 ? KeyboardDataFlags.RightKey : KeyboardDataFlags.LeftKey;
            }

            var data = new GorgonRawKeyboardData(rawData.MakeCode, flags, (Keys)rawData.VirtualKey);

            device.ProcessData(in data);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _hookInstalled, 0) == 1)
            {
                _hwnd = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Function to provide filtering of window messages for the application.
        /// </summary>
        /// <param name="m">Window message to filter.</param>
        /// <returns><b>true</b> if the message is processed by this method, or <b>false</b> if not.</returns>
        public bool PreFilterMessage(ref Message m)
        {
            if ((m.Msg != RawInputApi.WmRawInput) || (m.HWnd != _hwnd))
            {
                return false;
            }

            RAWINPUT _rawInput = default;
            RawInputApi.GetRawInputData(m.LParam, ref _rawInput);
            var key = new DeviceKey
            {
                DeviceType = _rawInput.Header.Type,
                DeviceHandle = IntPtr.Zero
            };

            switch (_rawInput.Header.Type)
            {
                case RawInputType.Keyboard:
                    IGorgonRawInputDeviceData<GorgonRawKeyboardData> keyboard = GetRawInputKeyboardDevice(key, _rawInput.Header.Device);

                    if (keyboard is null)
                    {
                        return false;
                    }

                    Dispatch(keyboard, ref _rawInput.Union.Keyboard);
                    break;
                case RawInputType.Mouse:
                    IGorgonRawInputDeviceData<GorgonRawMouseData> mouse = GetRawInputMouseDevice(key, _rawInput.Header.Device);

                    if (mouse is null)
                    {
                        return false;
                    }

                    Dispatch(mouse, ref _rawInput.Union.Mouse);
                    break;
                case RawInputType.HID:
                    IGorgonRawInputDeviceData<GorgonRawHIDData> hid = GetRawInputHid(key, _rawInput.Header.Device);

                    if (hid is null)
                    {
                        return false;
                    }

                    Dispatch(hid, ref _rawInput.Union.HID);
                    break;
            }

            return true;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="RawInputMessageFilter"/> class.
        /// </summary>
        /// <param name="keyboardDevices">The keyboard devices that are registered with the raw input provider.</param>
        /// <param name="pointingDevices">The pointing devices that are registered with the raw input provider.</param>
        /// <param name="hidDevices">The HID devices that are registered with the raw input provider.</param>
        /// <param name="hwnd">The window handle to the main application window.</param>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when the message hook could be applied to the window.</exception>
        public RawInputMessageFilter(
            Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawKeyboardData>> keyboardDevices,
            Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawMouseData>> pointingDevices,
            Dictionary<DeviceKey, IGorgonRawInputDeviceData<GorgonRawHIDData>> hidDevices, IntPtr hwnd)
        {
            _keyboardDevices = keyboardDevices;
            _mouseDevices = pointingDevices;
            _hidDevices = hidDevices;

            if ((Interlocked.Exchange(ref _hookInstalled, 1) == 0) && (_hwnd == IntPtr.Zero))
            {
                _hwnd = hwnd;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RawInputMessageFilter"/> class.
        /// </summary>
        ~RawInputMessageFilter()
        {
            Dispose();
        }
        #endregion
    }
}
