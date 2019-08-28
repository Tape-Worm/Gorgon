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
// Created: Wednesday, September 9, 2015 8:36:25 PM
// 
#endregion

using System.Drawing;
using System.Windows.Forms;
using Gorgon.Native;

namespace Gorgon.Input
{
    /// <summary>
    /// Dispatches Raw Input data to the appropriate device.
    /// </summary>
    internal static class RawInputDispatcher
    {
        /// <summary>
        /// Function to dispatch the raw input data to a HID.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        public static void Dispatch(IRawInputDeviceData<GorgonRawHIDData> device, ref RAWINPUTHID rawData)
        {
            unsafe
            {
                var data = new GorgonRawHIDData
                {
                    HidData = new GorgonReadOnlyPointer(rawData.Data.ToPointer(), rawData.Size * rawData.Count),
                    ItemCount = rawData.Count,
                    HIDDataSize = rawData.Size
                };

                device.ProcessData(ref data);
            }
        }

        /// <summary>
        /// Function to dispatch the raw input to a mouse device.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        public static void Dispatch(IRawInputDeviceData<GorgonRawMouseData> device, ref RAWINPUTMOUSE rawData)
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

            var processedData = new GorgonRawMouseData
            {
                ButtonState = state,
                MouseWheelDelta = wheelDelta,
                Position = new Point(rawData.LastX, rawData.LastY),
                IsRelative = ((rawData.Flags & RawMouseFlags.MoveAbsolute) != RawMouseFlags.MoveAbsolute)
            };

            device.ProcessData(ref processedData);
        }

        /// <summary>
        /// Function to dispatch the raw input to a keyboard device.
        /// </summary>
        /// <param name="device">The device that will receive the data.</param>
        /// <param name="rawData">Raw input data to translate.</param>
        public static void Dispatch(IRawInputDeviceData<GorgonRawKeyboardData> device, ref RAWINPUTKEYBOARD rawData)
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

            var data = new GorgonRawKeyboardData
            {
                ScanCode = rawData.MakeCode,
                Key = (Keys)rawData.VirtualKey,
                Flags = flags
            };

            device.ProcessData(ref data);
        }
    }
}
