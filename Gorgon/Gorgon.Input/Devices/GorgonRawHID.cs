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
// Created: Tuesday, September 07, 2015 2:27:10 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// Provides state for human interface data returned from Raw Input.
/// </summary>
/// <remarks>
/// <para>
/// This allows a user to read, and parse human interface device data from an aribtrary device. It is recommended that this object be wrapped by an actual object that will be used to present the data 
/// in an easy to manipulate format.
/// </para>
/// <para>
/// This object implements <see cref="IDisposable"/> because it manipulates native memory. It is necessary to call the <see cref="IDisposable.Dispose"/> method in order to ensure there is no memory leak 
/// when finished with this object.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonRawHID"/> class.
/// </remarks>
/// <param name="hidInfo">The human interface device information used to determine which keyboard to use.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="hidInfo"/> is <b>null</b>.</exception>
public class GorgonRawHID(GorgonRawHIDInfo hidInfo)
        : IGorgonRawHID
{
    #region Variables.
    // Pre parsed data for this device.
    private GorgonNativeBuffer<byte> _preParsedData;
    // Synchronization for multiple threads.
    private readonly object _syncLock = new();
    #endregion

    #region Events.
    /// <summary>
    /// Event triggered when Raw Input receives data from the device.
    /// </summary>
    public event EventHandler<GorgonHIDEventArgs> DataReceived;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the handle for the device.
    /// </summary>
    public nint Handle => Info.Handle;

    /// <summary>
    /// Property to return the type of device.
    /// </summary>
    public RawInputType DeviceType => RawInputType.HID;

    /// <summary>
    /// Property to return the HID usage code for this device.
    /// </summary>
    HIDUsage IGorgonRawInputDevice.DeviceUsage => Info.Usage;

    /// <summary>
    /// Property to return information about the Raw Input Human Interface Device.
    /// </summary>
    public IGorgonRawHIDInfo Info
    {
        get;
    } = hidInfo ?? throw new ArgumentNullException(nameof(hidInfo));

    /// <summary>
    /// Property to return a pointer to the block of memory that stores the HID data.
    /// </summary>
    public GorgonPtr<byte> Data
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the size of an individual HID input, in bytes.
    /// </summary>
    public int HIDSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the number of inputs in the <see cref="IGorgonRawHID.Data"/>.
    /// </summary>
    public int Count
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the pre-parsed data for this HID.
    /// </summary>
    public GorgonPtr<byte> PreParsedData
    {
        get
        {
            lock (_syncLock)
            {
                _preParsedData ??= RawInputApi.GetPreparsedDeviceInfoData(Handle);
            }

            return _preParsedData;
        }
    }
    #endregion

    #region Methods.

    /// <summary>
    /// Function to process the Gorgon raw input data into device state data and appropriate events.
    /// </summary>
    /// <param name="rawInputData">The data to process.</param>
    void IGorgonRawInputDeviceData<GorgonRawHIDData>.ProcessData(in GorgonRawHIDData rawInputData)
    {
        Data = rawInputData.HidData;
        HIDSize = rawInputData.HIDDataSize;
        Count = rawInputData.ItemCount;

        DataReceived?.Invoke(this, new GorgonHIDEventArgs(rawInputData.HidData, rawInputData.HIDDataSize, rawInputData.ItemCount));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _preParsedData?.Dispose();
        _preParsedData = null;

        GC.SuppressFinalize(this);
    }

    #endregion
}
