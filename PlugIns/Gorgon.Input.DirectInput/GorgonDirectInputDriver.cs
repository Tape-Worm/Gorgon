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
// Created: Sunday, September 13, 2015 1:47:57 PM
// 
#endregion

using Gorgon.Diagnostics;
using Gorgon.Input.DirectInput.Properties;
using DI = SharpDX.DirectInput;

namespace Gorgon.Input.DirectInput;

/// <summary>
/// The driver for DirectInput functionality.
/// </summary>
/// <remarks>
/// This driver will enumerate all gaming devices except those covered by the XInput driver. To use those devices, use the XInput driver directly.
/// </remarks>
internal sealed class GorgonDirectInputDriver
    : GorgonGamingDeviceDriver
{
    #region Variables.
    // Primary direct input interface.
    private Lazy<DI.DirectInput> _directInput;
    // The available axis mappings for the individual gaming devices.
    private readonly Dictionary<IGorgonGamingDeviceInfo, IReadOnlyDictionary<GamingDeviceAxis, DI.DeviceObjectId>> _axisMappings =
                                [];
    #endregion

    #region Methods.
    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Lazy<DI.DirectInput> di = Interlocked.Exchange(ref _directInput, null);

        if ((di is null) || (!di.IsValueCreated))
        {
            return;
        }

        di.Value.Dispose();
    }

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
        IList<DI.DeviceInstance> devices = _directInput.Value.GetDevices(DI.DeviceClass.GameControl, DI.DeviceEnumerationFlags.AttachedOnly);

        Log.Print("Enumerating DirectInput gaming devices...", LoggingLevel.Verbose);

        bool FilterDiDevices(DI.DeviceInstance device)
        {
            bool isAttached = _directInput.Value.IsDeviceAttached(device.InstanceGuid);

            if ((connectedOnly) || (isAttached))
            {
                return true;
            }

            Log.Print($"WARNING: Found gaming device '{device.ProductName}', but it is not attached and enumeration is filtered for attached devices only.  Skipping...",
                      LoggingLevel.Verbose);

            return false;
        }

        DirectInputDeviceInfo CreateDeviceInfo(DI.DeviceInstance device)
        {
            var info = new DirectInputDeviceInfo(device);

            using (var joystick = new DI.Joystick(_directInput.Value, info.DeviceID))
            {
                _axisMappings[info] = info.GetDeviceCaps(joystick);

                Log.Print($"Found DirectInput gaming device \"{info.Description}\"", LoggingLevel.Verbose);
            }
            return info;
        }

        // Enumerate all controllers.
        IReadOnlyList<DirectInputDeviceInfo> result = devices.Where(FilterDiDevices)
                                                             .Select(CreateDeviceInfo)
                                                             .ToArray();

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
    public override IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo)
        => new DirectInputDevice(gamingDeviceInfo, _directInput.Value, _axisMappings[gamingDeviceInfo])
        {
            // Attempt to acquire the device immediately.
            IsAcquired = true
        };
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDirectInputDriver"/> class.
    /// </summary>
    public GorgonDirectInputDriver()
        : base(Resources.GORINP_DI_DESC) => _directInput = new Lazy<DI.DirectInput>(() => new DI.DirectInput(), true);
    #endregion
}
