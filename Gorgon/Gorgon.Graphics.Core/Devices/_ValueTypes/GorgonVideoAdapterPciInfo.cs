
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: November 6, 2017 5:56:30 PM
// 

namespace Gorgon.Graphics.Core;

/// <summary>
/// PCI slot information for a <see cref="IGorgonVideoAdapterInfo"/>
/// </summary>
public readonly struct GorgonVideoAdapterPciInfo
{
    /// <summary>
    /// The PCI device ID for the adapter.
    /// </summary>
    public readonly int DeviceID;

    /// <summary>
    /// The PCI ID revision number for the adapter.
    /// </summary>
    public readonly int Revision;

    /// <summary>
    /// The PCI sub system ID for the adapter.
    /// </summary>
    public readonly int SubSystemID;

    /// <summary>
    /// The PCI vendor ID for the adapter.
    /// </summary>
    public readonly int VendorID;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoAdapterPciInfo"/> struct.
    /// </summary>
    /// <param name="deviceID">The device identifier.</param>
    /// <param name="revision">The revision of the device.</param>
    /// <param name="subsystemID">The subsystem identifier.</param>
    /// <param name="vendorID">The vendor identifier.</param>
    internal GorgonVideoAdapterPciInfo(int deviceID, int revision, int subsystemID, int vendorID)
    {
        DeviceID = deviceID;
        Revision = revision;
        SubSystemID = subsystemID;
        VendorID = vendorID;
    }
}
