﻿
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

using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// Provides information about a Raw Input mouse device
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RawMouseInfo"/> class
/// </remarks>
/// <param name="deviceHandle">The device handle.</param>
/// <param name="hidPath">The human interface device path.</param>
/// <param name="className">The class of device.</param>
/// <param name="description">The human readable description of this device.</param>
/// <param name="deviceInfo">The data about the device.</param>
internal class RawMouseInfo(nint deviceHandle, string hidPath, string className, string description, RID_DEVICE_INFO_MOUSE deviceInfo)
        : IGorgonMouseInfo
{
    /// <summary>
    /// Property to return a human friendly description of the device.
    /// </summary>
    public string Description
    {
        get;
    } = description;

    /// <summary>
    /// Property to return human interface device path for the device.
    /// </summary>
    public string HIDPath
    {
        get;
    } = hidPath;

    /// <summary>
    /// Property to return the device class name.
    /// </summary>
    public string DeviceClass
    {
        get;
    } = className;

    /// <summary>
    /// Property to return the device handle.
    /// </summary>
    public nint Handle
    {
        get;
    } = deviceHandle;

    /// <summary>
    /// Property to return the sampling rate for the mouse.
    /// </summary>
    public int SampleRate
    {
        get;
    } = deviceInfo.dwSampleRate;

    /// <summary>
    /// Property to return the number of buttons on the mouse.
    /// </summary>
    public int ButtonCount
    {
        get;
    } = deviceInfo.dwNumberOfButtons;

    /// <summary>
    /// Property to return whether the mouse supports a horizontal wheel or not.
    /// </summary>
    public bool HasHorizontalWheel
    {
        get;
    } = deviceInfo.fHasHorizontalWheel;

    /// <summary>
    /// Property to return the mouse ID.
    /// </summary>
    public int MouseID
    {
        get;
    } = deviceInfo.dwId;

}
