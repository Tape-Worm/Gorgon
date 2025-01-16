
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
// Created: Saturday, September 19, 2015 8:29:18 PM
// 

using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// Provides capability information about a Raw Input Human Interface Device
/// </summary>
public interface IGorgonRawHIDInfo
    : IGorgonRawInputDeviceInfo
{
    /// <summary>
    /// Property to return the product ID for the device.
    /// </summary>
    int ProductID
    {
        get;
    }

    /// <summary>
    /// Property to return the vendor ID for the device.
    /// </summary>
    int VendorID
    {
        get;
    }

    /// <summary>
    /// Property to return the version number for the device.
    /// </summary>
    int Version
    {
        get;
    }

    /// <summary>
    /// Property to return the top level collection usage value for this device.
    /// </summary>
    HIDUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to return the top level collection usage page value for this device.
    /// </summary>
    HIDUsagePage UsagePage
    {
        get;
    }
}
