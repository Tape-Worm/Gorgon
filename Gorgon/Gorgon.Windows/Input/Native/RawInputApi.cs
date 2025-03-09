
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
// Created: Tuesday, September 08, 2015 1:26:21 AM
// 

using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Windows.Input.Properties;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.Windows.Input.Native;

/// <summary>
/// Enumeration containing HID usage page flags
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Win32 expects ushort")]
internal enum HIDUsagePage
    : ushort
{
    /// <summary>Unknown usage page.</summary>
    Undefined = 0x00,
    /// <summary>Generic desktop controls.</summary>
    Generic = 0x01,
    /// <summary>Simulation controls.</summary>
    Simulation = 0x02,
    /// <summary>Virtual reality controls.</summary>
    VR = 0x03,
    /// <summary>Sports controls.</summary>
    Sport = 0x04,
    /// <summary>Games controls.</summary>
    Game = 0x05,
    /// <summary>Generic device controls.</summary>
    GenericDeviceControls = 0x06,
    /// <summary>Keyboard controls.</summary>
    Keyboard = 0x07,
    /// <summary>LED controls.</summary>
    LED = 0x08,
    /// <summary>Button.</summary>
    Button = 0x09,
    /// <summary>Ordinal.</summary>
    Ordinal = 0x0A,
    /// <summary>Telephony.</summary>
    Telephony = 0x0B,
    /// <summary>Consumer.</summary>
    Consumer = 0x0C,
    /// <summary>Digitizer.</summary>
    Digitizer = 0x0D,
    /// <summary>Physical interface device.</summary>
    PID = 0x0F,
    /// <summary>Unicode.</summary>
    Unicode = 0x10,
    /// <summary>SoC page.</summary>
    SoC = 0x11,
    /// <summary>Eye and head trackers.</summary>
    EyeHeadTracker = 0x12,
    /// <summary>Auxillary display.</summary>
    Auxillary = 0x14,
    /// <summary>Sensors.</summary>
    Sensors = 0x20,
    /// <summary>Medical instruments.</summary>
    Medical = 0x40,
    /// <summary>Braille display.</summary>
    Braille = 0x41,
    /// <summary>Light and illumination.</summary>
    LightIllumination = 0x59,
    /// <summary>Monitor.</summary>
    MonitorPage0 = 0x80,
    /// <summary>Monitor Enumerated.</summary>
    MonitorEnumerated = 0x81,
    /// <summary>VESA virtual controls.</summary>
    VesaVirtualControls = 0x82,
    /// <summary>Power page 0.</summary>
    PowerPage0 = 0x84,
    /// <summary>Battery System.</summary>
    BatterySystem = 0x85,
    /// <summary>Bar code scanner.</summary>
    BarCode = 0x8C,
    /// <summary>Scale page.</summary>
    Scale = 0x8D,
    /// <summary>Magnetic strip reading devices.</summary>
    MSR = 0x8E,
    /// <summary>Camera control.</summary>
    CameraControl = 0x90,
    /// <summary>Arcade controls.</summary>
    ArcadeControls = 0x91,
    /// <summary>Gaming device.</summary>
    GamingDevice = 0x92,
    /// <summary>FIDO alliance.</summary>
    FidoAlliance = 0xF1D0
}

/// <summary>
/// Enumeration for HID usage flags
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Win32 expects ushort")]
internal enum HIDUsage
    : ushort
{
    /// <summary></summary>
    None = 0,
    /// <summary></summary>
    Pointer = 0x01,
    /// <summary></summary>
    Mouse = 0x02,
    /// <summary></summary>
    Joystick = 0x04,
    /// <summary></summary>
    Gamepad = 0x05,
    /// <summary></summary>
    Keyboard = 0x06,
    /// <summary></summary>
    Keypad = 0x07,
    /// <summary></summary>
    SystemControl = 0x80,
    /// <summary></summary>
    X = 0x30,
    /// <summary></summary>
    Y = 0x31,
    /// <summary></summary>
    Z = 0x32,
    /// <summary></summary>
    RotationX = 0x33,
    /// <summary></summary>		
    RotationY = 0x34,
    /// <summary></summary>
    RotationZ = 0x35,
    /// <summary></summary>
    Slider = 0x36,
    /// <summary></summary>
    Dial = 0x37,
    /// <summary></summary>
    Wheel = 0x38,
    /// <summary></summary>
    HatSwitch = 0x39,
    /// <summary></summary>
    CountedBuffer = 0x3A,
    /// <summary></summary>
    ByteCount = 0x3B,
    /// <summary></summary>
    MotionWakeup = 0x3C,
    /// <summary></summary>
    StartButton = 0x3D,
    /// <summary></summary>
    SelectButton = 0x3E,
    /// <summary></summary>
    VectorX = 0x40,
    /// <summary></summary>
    VectorY = 0x41,
    /// <summary></summary>
    VectorZ = 0x42,
    /// <summary></summary>
    VBodyRelativeX = 0x43,
    /// <summary></summary>
    VBodyRelativeY = 0x44,
    /// <summary></summary>
    VBodyRelativeZ = 0x45,
    /// <summary></summary>
    VNoOrientation = 0x46,
    /// <summary></summary>
    SystemControlPower = 0x81,
    /// <summary></summary>
    SystemControlSleep = 0x82,
    /// <summary></summary>
    SystemControlWake = 0x83,
    /// <summary></summary>
    SystemControlContextMenu = 0x84,
    /// <summary></summary>
    SystemControlMainMenu = 0x85,
    /// <summary></summary>
    SystemControlApplicationMenu = 0x86,
    /// <summary></summary>
    SystemControlHelpMenu = 0x87,
    /// <summary></summary>
    SystemControlMenuExit = 0x88,
    /// <summary></summary>
    SystemControlMenuSelect = 0x89,
    /// <summary></summary>
    SystemControlMenuRight = 0x8A,
    /// <summary></summary>
    SystemControlMenuLeft = 0x8B,
    /// <summary></summary>
    SystemControlMenuUp = 0x8C,
    /// <summary></summary>
    SystemControlMenuDown = 0x8D,
    /// <summary></summary>
    DPadUp = 0x90,
    /// <summary></summary>
    DPadDown = 0x91,
    /// <summary></summary>
    DPadRight = 0x92,
    /// <summary></summary>
    DPadLeft = 0x93
}

/// <summary>
/// Native raw Input API functionality
/// </summary>
internal static partial class RawInputApi
{
    // The size of the raw input data header.
    private static readonly uint _headerSize = (uint)Unsafe.SizeOf<RAWINPUTHEADER>();

    /// <summary>
    /// Function to retrieve buffered raw input messages.
    /// </summary>
    /// <param name="inputEvents">The queue of raw input events to return.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    public unsafe static void GetRawInputBuffer(Queue<RAWINPUT> inputEvents, CancellationToken cancelToken)
    {
        uint bufferSize = 0;

        inputEvents.Clear();

        // Process the message loop for our thread, and grab any outstanding raw input messages.
        while ((PInvoke.PeekMessage(out MSG msg, HWND.Null, PInvoke.WM_INPUT_DEVICE_CHANGE, PInvoke.WM_INPUT, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE)) && (!cancelToken.IsCancellationRequested))
        {
            // We only care about raw input data, any other messages can be forwarded to our message window.
            if (msg.message != PInvoke.WM_INPUT)
            {
                PInvoke.TranslateMessage(in msg);
                PInvoke.DispatchMessage(in msg);
                continue;
            }

            int result = (int)PInvoke.GetRawInputData(new HRAWINPUT(msg.lParam), RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, null, ref bufferSize, _headerSize);

            if (result < 0)
            {
                throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
            }

            if (bufferSize == 0)
            {
                continue;
            }

            RAWINPUT inputPacket = default;

            if (PInvoke.GetRawInputData(new HRAWINPUT(msg.lParam), RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, &inputPacket, ref bufferSize, _headerSize) < 0)
            {
                throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
            }

            inputEvents.Enqueue(inputPacket);
        }
    }

    /// <summary>
    /// Function to return the size of a preparsed data block.
    /// </summary>
    /// <param name="deviceHandle">The handle to the human interface device to receive preparsed data from.</param>
    /// <returns>The size of the data block, in bytes.</returns>
    /// <exception cref="Win32Exception">Thrown if the size could not be retrieved.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static int GetPreparsedDataSize(HANDLE deviceHandle)
    {
        uint dataSize = 0;
        if (PInvoke.GetRawInputDeviceInfo(deviceHandle, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_PREPARSEDDATA, null, ref dataSize) < 0)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
        }

        return (int)dataSize;
    }

    /// <summary>
    /// Function to retrieve the preparsed HID data for a given device.
    /// </summary>
    /// <param name="deviceHandle">The handle to the device.</param>
    /// <param name="buffer">The buffer to populate.</param>
    /// <returns>A pointer to the block of memory holding the HID preparsed data.</returns>
    public unsafe static void GetPreparsedDeviceInfoData(HANDLE deviceHandle, Span<byte> buffer)
    {
        int win32Error;

        if (buffer.IsEmpty)
        {
            throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, buffer.Length));
        }

        fixed (byte* result = buffer)
        {
            uint dataSize = (uint)buffer.Length;

            if (PInvoke.GetRawInputDeviceInfo(deviceHandle, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_PREPARSEDDATA, result, ref dataSize) < 0)
            {
                win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
            }
        }
    }

    /// <summary>
    /// Function to register a raw input device.
    /// </summary>
    /// <param name="usage">The HID usage code to register.</param>
    /// <param name="targetWindow">The target window for the device.</param>
    /// <param name="flags">Flags used in device registration.</param>
    public unsafe static void RegisterRawInputDevice(HIDUsage usage, HWND targetWindow, RAWINPUTDEVICE_FLAGS flags)
    {
        RAWINPUTDEVICE* devices = stackalloc RAWINPUTDEVICE[1];

        devices[0] = new RAWINPUTDEVICE
        {
            dwFlags = flags,
            usUsage = (ushort)usage,
            usUsagePage = (ushort)HIDUsagePage.Generic,
            hwndTarget = targetWindow
        };

        if (!PInvoke.RegisterRawInputDevices(devices, 1, (uint)sizeof(RAWINPUTDEVICE)))
        {
            throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_REGISTER);
        }
    }

    /// <summary>
    /// Function to unregister a raw input device.
    /// </summary>
    /// <param name="usage">The HID usage code to unregister.</param>
    public unsafe static void UnregisterRawInputDevice(HIDUsage usage)
    {
        RAWINPUTDEVICE* devices = stackalloc RAWINPUTDEVICE[1];

        devices[0] = new RAWINPUTDEVICE
        {
            dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_REMOVE,
            usUsage = (ushort)usage,
            usUsagePage = (ushort)HIDUsagePage.Generic,
            hwndTarget = HWND.Null
        };

        if (!PInvoke.RegisterRawInputDevices(devices, 1, (uint)sizeof(RAWINPUTDEVICE)))
        {
            throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_REGISTER);
        }
    }

    /// <summary>
    /// Function to perform the enumeration and translation of raw input native structures.
    /// </summary>
    /// <returns>The raw input device list.</returns>
    public unsafe static RAWINPUTDEVICELIST[] EnumerateRawInputDevices()
    {
        uint deviceCount = 0;
        uint structSize = (uint)Unsafe.SizeOf<RAWINPUTDEVICELIST>();

        // Define how large the buffer needs to be.
        if (PInvoke.GetRawInputDeviceList(null, ref deviceCount, structSize) < 0)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_ENUMERATE_WIN32_ERR);
        }

        if (deviceCount == 0)
        {
            return [];
        }

        RAWINPUTDEVICELIST[] deviceList = ArrayPool<RAWINPUTDEVICELIST>.Shared.Rent((int)deviceCount);

        try
        {
            fixed (RAWINPUTDEVICELIST* ptr = &deviceList[0])
            {
                uint actualCount = PInvoke.GetRawInputDeviceList(ptr, ref deviceCount, structSize);

                if (actualCount < 0)
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_ENUMERATE_WIN32_ERR);
                }

                if (actualCount == 0)
                {
                    return [];
                }

                List<RAWINPUTDEVICELIST> result = [];

                for (int i = 0; i < actualCount; ++i)
                {
                    if (deviceList[i].hDevice == HANDLE.Null)
                    {
                        continue;
                    }

                    result.Add(deviceList[i]);
                }

                return [.. result];
            }
        }
        finally
        {
            ArrayPool<RAWINPUTDEVICELIST>.Shared.Return(deviceList, true);
        }
    }

    /// <summary>
    /// Function to retrieve information about a specific device.
    /// </summary>
    /// <param name="device">The device to retrieve information about.</param>
    /// <returns>The device information for the device.</returns>
    public unsafe static RID_DEVICE_INFO GetDeviceInfo(ref readonly RAWINPUTDEVICELIST device)
    {
        uint dataSize = 0;

        int errCode = (int)PInvoke.GetRawInputDeviceInfo(device.hDevice, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICEINFO, null, ref dataSize);

        if (errCode is not (-1) and not 0)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
        }

        if (errCode == -1)
        {
            throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize));
        }

        RID_DEVICE_INFO result = default;
        errCode = (int)PInvoke.GetRawInputDeviceInfo(device.hDevice, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICEINFO, &result, ref dataSize);

        if (errCode < -1)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
        }

        return errCode == -1
            ? throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize))
            : result;
    }

    /// <summary>
    /// Function to retrieve the name for the specified device.
    /// </summary>
    /// <param name="device">Device to retrieve the name for.</param>
    /// <param name="log">The logging interface.</param>
    /// <returns>A string containing the device name.</returns>
    public unsafe static string GetDeviceName(ref readonly RAWINPUTDEVICELIST device, IGorgonLog log)
    {
        uint dataSize = 0;

        if (((int)PInvoke.GetRawInputDeviceInfo(device.hDevice, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICENAME, null, ref dataSize)) < 0)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
        }

        // Do nothing if we have no data.
        if (dataSize < 4)
        {
            return string.Empty;
        }

        char* data = stackalloc char[(int)dataSize];

        // ReSharper disable once InvertIf
        if (((int)PInvoke.GetRawInputDeviceInfo(device.hDevice, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICENAME, data, ref dataSize)) < 0)
        {
            int win32Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(win32Error, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
        }

        // The strings that come back from native land will end with a null terminator, so crop that off.
        return new string(data, 0, (int)(dataSize - 1));
    }

    /// <summary>
    /// Initializes static members of the <see cref="RawInputApi"/> class.
    /// </summary>
    static RawInputApi() => Marshal.PrelinkAll(typeof(RawInputApi));
}
