#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, July 15, 2011 6:22:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Input.XInput.Properties;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput;

/// <summary>
/// The driver for XInput functionality.
/// </summary>
public class GorgonXInputDriver
    : GorgonGamingDeviceDriver
{
    #region Methods.
    /// <summary>
    /// Function to enumerate the gaming devices supported by this driver.
    /// </summary>
    /// <param name="connectedOnly">[Optional] <b>true</b> to only enumerate devices that are connected, or <b>false</b> to enumerate all devices.</param>
    /// <returns>A read only list of gaming device info values.</returns>
    /// <remarks>
    /// This will return only the devices supported by the driver. In some cases, the driver may not return a complete listing of all gaming devices attached to the system because the underlying provider 
    /// may not support those device types.
    /// </remarks>
    public override IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false)
    {
        Log.Print("Enumerating XInput controllers...", LoggingLevel.Verbose);

        // Enumerate all controllers.
        IReadOnlyList<XInputDeviceInfo> result =
            (from deviceIndex in (XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))
             where deviceIndex != XI.UserIndex.Any
             let controller = new XI.Controller(deviceIndex)
             where !connectedOnly || controller.IsConnected
             orderby deviceIndex
             select
                 new XInputDeviceInfo(string.Format(Resources.GORINP_XINP_DEVICE_NAME, (int)deviceIndex + 1), deviceIndex))
                .ToArray();

        foreach (XInputDeviceInfo info in result)
        {
            Log.Print($"Found XInput controller {info.Description}", LoggingLevel.Verbose);
            info.GetCaps(new XI.Controller(info.DeviceID.ToUserIndex()));
        }

        return result;
    }

    /// <summary>
    /// Function to create a <see cref="GorgonGamingDevice"/> object.
    /// </summary>
    /// <param name="gamingDeviceInfo">The <see cref="IGorgonGamingDeviceInfo"/> used to determine which device to associate with the resulting object.</param>
    /// <returns>A <see cref="GorgonGamingDevice"/> representing the data from the physical device.</returns>
    /// <remarks>
    /// <para>
    /// This will create a new instance of a <see cref="IGorgonGamingDevice"/> which will relay data from the physical device using the native provider. 
    /// </para>
    /// <para>
    /// Some devices may allocate native resources in order to communicate with the underlying native providers, and because of this, it is important to call the <see cref="IDisposable.Dispose"/> method 
    /// on the object when you are done with the object so that those resources may be freed.
    /// </para>
    /// </remarks>
    public override IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo) => gamingDeviceInfo is not XInputDeviceInfo xInputInfo ? throw new ArgumentException(Resources.GORINP_ERR_XINP_NOT_AN_XINPUT_DEVICE_INFO, nameof(gamingDeviceInfo))
            : new XInputDevice(xInputInfo);

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        // Nothing to do dispose.
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonXInputDriver"/> class.
    /// </summary>
    public GorgonXInputDriver()
        : base(Resources.GORINP_XINP_SERVICEDESC)
    {
    }
    #endregion
}
