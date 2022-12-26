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
// Created: Tuesday, September 8, 2015 4:30:52 PM
// 
#endregion

using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// The type device that is transmitting raw input data.
/// </summary>
public enum RawInputType
{
    /// <summary>
    /// Mouse input.
    /// </summary>
    Mouse = 0,
    /// <summary>
    /// Keyboard input.
    /// </summary>
    Keyboard = 1,
    /// <summary>
    /// Human Interface Device.
    /// </summary>
    HID = 2
}

/// <summary>
/// A common interface for all raw input devices.
/// </summary>
/// <remarks>
/// This interface is not meant to be used within an application and should only be used internally by Gorgon.
/// </remarks>
public interface IGorgonRawInputDevice
{
    /// <summary>
    /// Property to return the handle for the device.
    /// </summary>
    nint Handle
    {
        get;
    }

    /// <summary>
    /// Property to return the type of device.
    /// </summary>
    RawInputType DeviceType
    {
        get;
    }

    /// <summary>
    /// Property to return the HID usage code for this device.
    /// </summary>
    HIDUsage DeviceUsage
    {
        get;
    }
}
