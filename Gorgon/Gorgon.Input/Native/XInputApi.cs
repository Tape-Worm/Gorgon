// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: March 4, 2025 1:43:29 PM
//

using Gorgon.Diagnostics;
using Gorgon.Input.Devices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.XboxController;

namespace Gorgon.Input.Native;

/// <summary>
/// Provides functionality to locate XInput device slots.
/// </summary>
internal static partial class XInputApi
{
    // The maximum number of XInput controllers supported.
    private const int MaxInputControllers = 4;
    // The amount of milliseconds to wait between retries when retrieving the XInput slot.
    private const int RetryDelay = 1_000;

    /// <summary>
    /// Function to retrieve the battery status for the controller.
    /// </summary>
    /// <param name="slot">The slot index for the device.</param>
    /// <returns>A <see cref="GamingDeviceBatteryLevel"/> value indicating the power status.</returns>
    public static GamingDeviceBatteryLevel GetBatteryLevel(int slot)
    {
        WIN32_ERROR err = (WIN32_ERROR)PInvoke.XInputGetBatteryInformation((uint)slot, BATTERY_DEVTYPE.BATTERY_DEVTYPE_GAMEPAD, out XINPUT_BATTERY_INFORMATION batteryInfo);

        if (err == WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
        {
            return GamingDeviceBatteryLevel.Disconnected;
        }

        return err switch
        {
            WIN32_ERROR.ERROR_SUCCESS => batteryInfo.BatteryType switch
            {
                BATTERY_TYPE.BATTERY_TYPE_DISCONNECTED => GamingDeviceBatteryLevel.Disconnected,
                BATTERY_TYPE.BATTERY_TYPE_WIRED => GamingDeviceBatteryLevel.Wired,
                _ => batteryInfo.BatteryLevel switch
                {
                    BATTERY_LEVEL.BATTERY_LEVEL_EMPTY => GamingDeviceBatteryLevel.Empty,
                    BATTERY_LEVEL.BATTERY_LEVEL_LOW => GamingDeviceBatteryLevel.Low,
                    BATTERY_LEVEL.BATTERY_LEVEL_MEDIUM => GamingDeviceBatteryLevel.Medium,
                    BATTERY_LEVEL.BATTERY_LEVEL_FULL => GamingDeviceBatteryLevel.Full,
                    _ => GamingDeviceBatteryLevel.Unknown
                },
            },
            _ => GamingDeviceBatteryLevel.Error
        };

    }

    /// <summary>
    /// Function to set the vibration motor for the controller.
    /// </summary>
    /// <param name="slot">The slot index for the device.</param>
    /// <param name="leftMotorPower">The amount of power to apply to the left motor (0 disables).</param>
    /// <param name="rightMotorPower">The amount of power to apply to the right motor (0 disables).</param>
    /// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
    public static bool SetVibrationMotor(int slot, int leftMotorPower, int rightMotorPower)
    {
        XINPUT_VIBRATION vib = new()
        {
            wLeftMotorSpeed = (ushort)leftMotorPower,
            wRightMotorSpeed = (ushort)rightMotorPower
        };

        WIN32_ERROR err = (WIN32_ERROR)PInvoke.XInputSetState((uint)slot, in vib);

        return err == WIN32_ERROR.ERROR_SUCCESS;
    }

    /// <summary>
    /// Function to get the values for the triggers on the XInput device.
    /// </summary>
    /// <param name="slot">The slot index for the device.</param>
    /// <returns>A tuple containing the current left and right trigger values.</returns>
    public static (int Left, int Right) GetTriggerValues(int slot)
    {
        if (slot is < 0 or >= MaxInputControllers)
        {
            return (0, 0);
        }

        WIN32_ERROR err = (WIN32_ERROR)PInvoke.XInputGetState((uint)slot, out XINPUT_STATE xinputState);

        int left = err == WIN32_ERROR.ERROR_SUCCESS ? xinputState.Gamepad.bLeftTrigger : 0;
        int right = err == WIN32_ERROR.ERROR_SUCCESS ? xinputState.Gamepad.bRightTrigger : 0;

        return (left, right);
    }

    /// <summary>
    /// Function to locate an XInput device slot.
    /// </summary>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="log">The logging interface for debug messaging.</param>
    /// <returns>The index of the XInput device slot (0 to 3), or -1 if not connected or the slot could not be determined.</returns>
    /// <remarks>
    /// <para>
    /// This method will retry for 10 seconds if the application is unable to retrieve the slot. This is required to give wireless controllers time enough to connect to the system.
    /// </para>
    /// </remarks>
    public static int LocateXInputSlot(string deviceName, IGorgonLog log)
    {
        byte xinputSlot = ConfigManager.GetLEDState(deviceName, log);

        if (xinputSlot < MaxInputControllers)
        {
            return xinputSlot;
        }

        int counter = 0;

        log.PrintWarning("Unable to get the XInput slot.  Device may be still connecting... Retrying for 10 seconds...", LoggingLevel.Verbose);

        // The controller is probably waiting for a connection from the system if we get code 0x7f, so at this point, we should wait it out for a bit until it's fully connected.
        while ((counter < 10) && (xinputSlot == 0x7f))
        {
            // Normally sleep is not my first choice in implementing a delay, but we want to 
            // block here, and it's the easiest solution while we wait for wireless devices 
            // to make up their minds and finally connect.
            //
            // If you're really bent out of shape about it, then use the async version.
            Thread.Sleep(RetryDelay);

            xinputSlot = ConfigManager.GetLEDState(deviceName, log);

            if (xinputSlot >= MaxInputControllers)
            {
                log.PrintWarning($"Attempt #{(counter + 1)}. Still unable to get the XInput slot.  Device may be still connecting... ...", LoggingLevel.Verbose);
            }

            ++counter;
        }

        if (xinputSlot >= MaxInputControllers)
        {
            log.PrintError($"Unable to get the XInput slot for device {deviceName} (code: {xinputSlot}).", LoggingLevel.Simple);
            return -1;
        }

        // Test the device to ensure that it's actually connected.
        WIN32_ERROR err = (WIN32_ERROR)PInvoke.XInputGetCapabilities(xinputSlot, XINPUT_FLAG.XINPUT_FLAG_GAMEPAD, out _);

        if (err != WIN32_ERROR.ERROR_SUCCESS)
        {
            log.Print($"There was an error retrieving the XInput device capabilities for slot {xinputSlot}. Ensure the device is still connected.", LoggingLevel.Verbose);
            return -1;
        }

        return xinputSlot;
    }

    /// <summary>
    /// Function to locate an XInput device slot.
    /// </summary>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="log">The logging interface for debug messaging.</param>
    /// <param name="cancelToken">The cancellation token for the operation.</param>
    /// <returns>The index of the XInput device slot (0 to 3), or -1 if not connected or the slot could not be determined.</returns>
    /// <remarks>
    /// <para>
    /// This method will retry for 10 seconds if the application is unable to retrieve the slot. This is required to give wireless controllers time enough to connect to the system.
    /// </para>
    /// </remarks>
    public static async Task<int> LocateXInputSlotAsync(string deviceName, IGorgonLog log, CancellationToken cancelToken)
    {
        try
        {
            byte xinputSlot = ConfigManager.GetLEDState(deviceName, log);

            if (xinputSlot < MaxInputControllers)
            {
                return xinputSlot;
            }

            int counter = 0;

            log.PrintWarning("Unable to get the XInput slot.  Device may be still connecting... Retrying for 10 seconds...", LoggingLevel.Verbose);

            // The controller is probably waiting for a connection from the system if we get code 0x7f, so at this point, we should wait it out for a bit until it's fully connected.
            while ((counter < 10) && (xinputSlot == 0x7f) && (!cancelToken.IsCancellationRequested))
            {
                await Task.Delay(RetryDelay, cancelToken).ConfigureAwait(false);

                xinputSlot = ConfigManager.GetLEDState(deviceName, log);

                if (xinputSlot >= MaxInputControllers)
                {
                    log.PrintWarning($"Attempt #{(counter + 1)}. Still unable to get the XInput slot.  Device may be still connecting... ...", LoggingLevel.Verbose);
                }

                ++counter;
            }

            if ((cancelToken.IsCancellationRequested) || (xinputSlot >= MaxInputControllers))
            {
                log.PrintError($"Unable to get the XInput slot for device {deviceName} (code: {xinputSlot}).", LoggingLevel.Simple);
                return -1;
            }

            // Test the device to ensure that it's actually connected.
            WIN32_ERROR err = (WIN32_ERROR)PInvoke.XInputGetCapabilities(xinputSlot, XINPUT_FLAG.XINPUT_FLAG_GAMEPAD, out _);

            if ((cancelToken.IsCancellationRequested) || (err != WIN32_ERROR.ERROR_SUCCESS))
            {
                log.Print($"There was an error retrieving the XInput device capabilities for slot {xinputSlot}. Ensure the device is still connected.", LoggingLevel.Verbose);
                return -1;
            }

            return xinputSlot;
        }
        catch (OperationCanceledException)
        {
            return -1;
        }
    }
}
