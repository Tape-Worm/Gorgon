
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
// Created: Tuesday, September 07, 2015 1:51:39 PM
// 

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// Provides information about a Raw Input keyboard device
/// </summary>
/// <param name="Handle">The device handle.</param>
/// <param name="DeviceName">The human interface device path.</param>
/// <param name="DeviceClass">The class of device.</param>
/// <param name="Description">The human readable description of this device.</param>
/// <param name="FunctionKeyCount">The number of function keys on the keyboard.</param>
/// <param name="KeyboardType">The type of keyboard.</param>
internal record class KeyboardInfo(nint Handle, string DeviceName, string DeviceClass, string Description, int FunctionKeyCount, KeyboardType KeyboardType)
        : IGorgonKeyboardInfo
{
    /// <inheritdoc/>
    public InputDeviceType DeviceType => InputDeviceType.Keyboard;
}