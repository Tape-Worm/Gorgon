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
// Created: Friday, June 24, 2011 10:04:58 AM
// 
#endregion

using System;

namespace Gorgon.Native
{
    // ReSharper disable InconsistentNaming
	#region Enumerations
    /// <summary>
    /// Window long functionality flags.
    /// </summary>
    enum WindowLongType
    {
        /// <summary>
        /// Retrieves extended window styles.
        /// </summary>
        ExStyle = -20,
        /// <summary>
        /// Retrieves a handle to the application instance.
        /// </summary>
        HInstance = -6,
        /// <summary>
        /// Retrieves the handle to the parent window if one exists.
        /// </summary>
        HwndParent = -8,
        /// <summary>
        /// Retrieves the identifier of the window.
        /// </summary>
        ID = -12,
        /// <summary>
        /// Retrieves the window styles.
        /// </summary>
        Style = -16,
        /// <summary>
        /// Retrieves user defined data associated with the window.
        /// </summary>
        UserData = -21,
        /// <summary>
        /// Retrieves the window procedure.
        /// </summary>
        WndProc = -4
    }

	/// <summary>
	/// Enumeration containing the joystick information flags.
	/// </summary>
	[Flags]
	enum JoystickInfoFlags
	{
		/// <summary></summary>
		All = (ReturnX | ReturnY | ReturnZ | ReturnRudder | ReturnAxis5 | ReturnAxis6 | ReturnPOV | ReturnButtons),
		/// <summary></summary>
		ReturnX = 0x00000001,
		/// <summary></summary>
		ReturnY = 0x00000002,
		/// <summary></summary>
		ReturnZ = 0x00000004,
		/// <summary></summary>
		ReturnRudder = 0x00000008,
		/// <summary></summary>
		ReturnAxis5 = 0x00000010,
		/// <summary></summary>
		ReturnAxis6 = 0x00000020,
		/// <summary></summary>
		ReturnPOV = 0x00000040,
		/// <summary></summary>
		ReturnButtons = 0x00000080,
		/// <summary></summary>
		ReturnRawData = 0x00000100,
		/// <summary></summary>
		ReturnPOVContinuousDegreeBearings = 0x00000200,
		/// <summary></summary>
		ReturnCentered = 0x00000400,
		/// <summary></summary>
		UseDeadzone = 0x00000800,
		/// <summary></summary>
		CalibrationReadAlways = 0x00010000,
		/// <summary></summary>
		CalibrationReadXYOnly = 0x00020000,
		/// <summary></summary>
		CalibrationRead3 = 0x00040000,
		/// <summary></summary>
		CalibrationRead4 = 0x00080000,
		/// <summary></summary>
		CalibrationReadXOnly = 0x00100000,
		/// <summary></summary>
		CalibrationReadYOnly = 0x00200000,
		/// <summary></summary>
		CalibrationRead5 = 0x00400000,
		/// <summary></summary>
		CalibrationRead6 = 0x00800000,
		/// <summary></summary>
		CalibrationReadZOnly = 0x01000000,
		/// <summary></summary>
		CalibrationReadRudderOnly = 0x02000000,
		/// <summary></summary>
		CalibrationReadAxis5Only = 0x04000000,
		/// <summary></summary>
		CalibrationReadaxis6Only = 0x08000000
	}

	/// <summary>
	/// Enumeration for joystick buttons.
	/// </summary>
	[Flags]
	enum JoystickButton
		: uint
	{
		/// <summary></summary>
		Button1 = 0x0001,
		/// <summary></summary>
		Button2 = 0x0002,
		/// <summary></summary>
		Button3 = 0x0003,
		/// <summary></summary>
		Button4 = 0x0004,
		/// <summary></summary>
		Button1Changed = 0x0100,
		/// <summary></summary>
		Button2Changed = 0x0200,
		/// <summary></summary>
		Button3Changed = 0x0400,
		/// <summary></summary>
		Button4Changed = 0x0800,
		/// <summary></summary>
		Button5 = 0x00000010,
		/// <summary></summary>
		Button6 = 0x00000020,
		/// <summary></summary>
		Button7 = 0x00000040,
		/// <summary></summary>
		Button8 = 0x00000080,
		/// <summary></summary>
		Button9 = 0x00000100,
		/// <summary></summary>
		Button10 = 0x00000200,
		/// <summary></summary>
		Button11 = 0x00000400,
		/// <summary></summary>
		Button12 = 0x00000800,
		/// <summary></summary>
		Button13 = 0x00001000,
		/// <summary></summary>
		Button14 = 0x00002000,
		/// <summary></summary>
		Button15 = 0x00004000,
		/// <summary></summary>
		Button16 = 0x00008000,
		/// <summary></summary>
		Button17 = 0x00010000,
		/// <summary></summary>
		Button18 = 0x00020000,
		/// <summary></summary>
		Button19 = 0x00040000,
		/// <summary></summary>
		Button20 = 0x00080000,
		/// <summary></summary>
		Button21 = 0x00100000,
		/// <summary></summary>
		Button22 = 0x00200000,
		/// <summary></summary>
		Button23 = 0x00400000,
		/// <summary></summary>
		Button24 = 0x00800000,
		/// <summary></summary>
		Button25 = 0x01000000,
		/// <summary></summary>
		Button26 = 0x02000000,
		/// <summary></summary>
		Button27 = 0x04000000,
		/// <summary></summary>
		Button28 = 0x08000000,
		/// <summary></summary>
		Button29 = 0x10000000,
		/// <summary></summary>
		Button30 = 0x20000000,
		/// <summary></summary>
		Button31 = 0x40000000,
		/// <summary></summary>
		Button32 = 0x80000000
	}

	/// <summary>
	/// Enumeration for joystick capabilities.
	/// </summary>
	[Flags]
	enum JoystickCaps
	{
		/// <summary>Has a Z axis.</summary>
		HasZ = 0x0001,
		/// <summary>Has a rudder axis.</summary>
		HasRudder = 0x0002,
		/// <summary>Has a 5th axis.</summary>
		HasU = 0x0004,
		/// <summary>Has a 6th axis.</summary>
		HasV = 0x0008,
		/// <summary>Has a POV hat.</summary>
		HasPOV = 0x0010,
		/// <summary>Has a 4 direction POV hat.</summary>
		POV4Directions = 0x0020,
		/// <summary>Has a continuous degree bearing POV hat.</summary>
		POVContinuousDegreeBearings = 0x0040
	}

	/// <summary>
	/// Enumeration containing HID usage page flags.
	/// </summary>
	enum HIDUsagePage
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
		/// <summary>Alphanumeric display.</summary>
		AlphaNumeric = 0x14,
		/// <summary>Medical instruments.</summary>
		Medical = 0x40,
		/// <summary>Monitor page 0.</summary>
		MonitorPage0 = 0x80,
		/// <summary>Monitor page 1.</summary>
		MonitorPage1 = 0x81,
		/// <summary>Monitor page 2.</summary>
		MonitorPage2 = 0x82,
		/// <summary>Monitor page 3.</summary>
		MonitorPage3 = 0x83,
		/// <summary>Power page 0.</summary>
		PowerPage0 = 0x84,
		/// <summary>Power page 1.</summary>
		PowerPage1 = 0x85,
		/// <summary>Power page 2.</summary>
		PowerPage2 = 0x86,
		/// <summary>Power page 3.</summary>
		PowerPage3 = 0x87,
		/// <summary>Bar code scanner.</summary>
		BarCode = 0x8C,
		/// <summary>Scale page.</summary>
		Scale = 0x8D,
		/// <summary>Magnetic strip reading devices.</summary>
		MSR = 0x8E
	}

	/// <summary>
	/// Constants for HID usage flags.
	/// </summary>
	enum HIDUsage
		: ushort
	{
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
		 RelativeX = 0x33,
		/// <summary></summary>		
		 RelativeY = 0x34,
		/// <summary></summary>
		 RelativeZ = 0x35,
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
		 VX = 0x40,
		/// <summary></summary>
		 VY = 0x41,
		/// <summary></summary>
		 VZ = 0x42,
		/// <summary></summary>
		 VBRX = 0x43,
		/// <summary></summary>
		 VBRY = 0x44,
		/// <summary></summary>
		 VBRZ = 0x45,
		/// <summary></summary>
		 VNO = 0x46,
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
		 KeyboardNoEvent = 0x00,
		/// <summary></summary>
		 KeyboardRollover = 0x01,
		/// <summary></summary>
		 KeyboardPostFail = 0x02,
		/// <summary></summary>
		 KeyboardUndefined = 0x03,
		/// <summary></summary>
		 KeyboardaA = 0x04,
		/// <summary></summary>
		 KeyboardzZ = 0x1D,
		/// <summary></summary>
		 Keyboard1 = 0x1E,
		/// <summary></summary>
		 Keyboard0 = 0x27,
		/// <summary></summary>
		 KeyboardLeftControl = 0xE0,
		/// <summary></summary>
		 KeyboardLeftShift = 0xE1,
		/// <summary></summary>
		 KeyboardLeftALT = 0xE2,
		/// <summary></summary>
		 KeyboardLeftGUI = 0xE3,
		/// <summary></summary>
		 KeyboardRightControl = 0xE4,
		/// <summary></summary>
		 KeyboardRightShift = 0xE5,
		/// <summary></summary>
		 KeyboardRightALT = 0xE6,
		/// <summary></summary>
		 KeyboardRightGUI = 0xE7,
		/// <summary></summary>
		 KeyboardScrollLock = 0x47,
		/// <summary></summary>
		 KeyboardNumLock = 0x53,
		/// <summary></summary>
		 KeyboardCapsLock = 0x39,
		/// <summary></summary>
		 KeyboardF1 = 0x3A,
		/// <summary></summary>
		 KeyboardF12 = 0x45,
		/// <summary></summary>
		 KeyboardReturn = 0x28,
		/// <summary></summary>
		 KeyboardEscape = 0x29,
		/// <summary></summary>
		 KeyboardDelete = 0x2A,
		/// <summary></summary>
		 KeyboardPrintScreen = 0x46,
		/// <summary></summary>
		 LEDNumLock = 0x01,
		/// <summary></summary>
		 LEDCapsLock = 0x02,
		/// <summary></summary>
		 LEDScrollLock = 0x03,
		/// <summary></summary>
		 LEDCompose = 0x04,
		/// <summary></summary>
		 LEDKana = 0x05,
		/// <summary></summary>
		 LEDPower = 0x06,
		/// <summary></summary>
		 LEDShift = 0x07,
		/// <summary></summary>
		 LEDDoNotDisturb = 0x08,
		/// <summary></summary>
		 LEDMute = 0x09,
		/// <summary></summary>
		 LEDToneEnable = 0x0A,
		/// <summary></summary>
		 LEDHighCutFilter = 0x0B,
		/// <summary></summary>
		 LEDLowCutFilter = 0x0C,
		/// <summary></summary>
		 LEDEqualizerEnable = 0x0D,
		/// <summary></summary>
		 LEDSoundFieldOn = 0x0E,
		/// <summary></summary>
		 LEDSurroundFieldOn = 0x0F,
		/// <summary></summary>
		 LEDRepeat = 0x10,
		/// <summary></summary>
		 LEDStereo = 0x11,
		/// <summary></summary>
		 LEDSamplingRateDirect = 0x12,
		/// <summary></summary>
		 LEDSpinning = 0x13,
		/// <summary></summary>
		 LEDCAV = 0x14,
		/// <summary></summary>
		 LEDCLV = 0x15,
		/// <summary></summary>
		 LEDRecordingFormatDet = 0x16,
		/// <summary></summary>
		 LEDOffHook = 0x17,
		/// <summary></summary>
		 LEDRing = 0x18,
		/// <summary></summary>
		 LEDMessageWaiting = 0x19,
		/// <summary></summary>
		 LEDDataMode = 0x1A,
		/// <summary></summary>
		 LEDBatteryOperation = 0x1B,
		/// <summary></summary>
		 LEDBatteryOK = 0x1C,
		/// <summary></summary>
		 LEDBatteryLow = 0x1D,
		/// <summary></summary>
		 LEDSpeaker = 0x1E,
		/// <summary></summary>
		 LEDHeadset = 0x1F,
		/// <summary></summary>
		 LEDHold = 0x20,
		/// <summary></summary>
		 LEDMicrophone = 0x21,
		/// <summary></summary>
		 LEDCoverage = 0x22,
		/// <summary></summary>
		 LEDNightMode = 0x23,
		/// <summary></summary>
		 LEDSendCalls = 0x24,
		/// <summary></summary>
		 LEDCallPickup = 0x25,
		/// <summary></summary>
		 LEDConference = 0x26,
		/// <summary></summary>
		 LEDStandBy = 0x27,
		/// <summary></summary>
		 LEDCameraOn = 0x28,
		/// <summary></summary>
		 LEDCameraOff = 0x29,
		/// <summary></summary>		
		 LEDOnLine = 0x2A,
		/// <summary></summary>
		 LEDOffLine = 0x2B,
		/// <summary></summary>
		 LEDBusy = 0x2C,
		/// <summary></summary>
		 LEDReady = 0x2D,
		/// <summary></summary>
		 LEDPaperOut = 0x2E,
		/// <summary></summary>
		 LEDPaperJam = 0x2F,
		/// <summary></summary>
		 LEDRemote = 0x30,
		/// <summary></summary>
		 LEDForward = 0x31,
		/// <summary></summary>
		 LEDReverse = 0x32,
		/// <summary></summary>
		 LEDStop = 0x33,
		/// <summary></summary>
		 LEDRewind = 0x34,
		/// <summary></summary>
		 LEDFastForward = 0x35,
		/// <summary></summary>
		 LEDPlay = 0x36,
		/// <summary></summary>
		 LEDPause = 0x37,
		/// <summary></summary>
		 LEDRecord = 0x38,
		/// <summary></summary>
		 LEDError = 0x39,
		/// <summary></summary>
		 LEDSelectedIndicator = 0x3A,
		/// <summary></summary>
		 LEDInUseIndicator = 0x3B,
		/// <summary></summary>
		 LEDMultiModeIndicator = 0x3C,
		/// <summary></summary>
		 LEDIndicatorOn = 0x3D,
		/// <summary></summary>
		 LEDIndicatorFlash = 0x3E,
		/// <summary></summary>
		 LEDIndicatorSlowBlink = 0x3F,
		/// <summary></summary>
		 LEDIndicatorFastBlink = 0x40,
		/// <summary></summary>
		 LEDIndicatorOff = 0x41,
		/// <summary></summary>
		 LEDFlashOnTime = 0x42,
		/// <summary></summary>
		 LEDSlowBlinkOnTime = 0x43,
		/// <summary></summary>
		 LEDSlowBlinkOffTime = 0x44,
		/// <summary></summary>
		 LEDFastBlinkOnTime = 0x45,
		/// <summary></summary>
		 LEDFastBlinkOffTime = 0x46,
		/// <summary></summary>
		 LEDIndicatorColor = 0x47,
		/// <summary></summary>
		 LEDRed = 0x48,
		/// <summary></summary>
		 LEDGreen = 0x49,
		/// <summary></summary>
		 LEDAmber = 0x4A,
		/// <summary></summary>
		 LEDGenericIndicator = 0x3B,
		/// <summary></summary>
		 TelephonyPhone = 0x01,
		/// <summary></summary>
		 TelephonyAnsweringMachine = 0x02,
		/// <summary></summary>
		 TelephonyMessageControls = 0x03,
		/// <summary></summary>
		 TelephonyHandset = 0x04,
		/// <summary></summary>
		 TelephonyHeadset = 0x05,
		/// <summary></summary>
		 TelephonyKeypad = 0x06,
		/// <summary></summary>
		 TelephonyProgrammableButton = 0x07,
		/// <summary></summary>
		 SimulationRudder = 0xBA,
		/// <summary></summary>
		 SimulationThrottle = 0xBB
	}

	/// <summary>
	/// Enumeration containing flags for a raw input device.
	/// </summary>
	[Flags]
	enum RawInputDeviceFlags
	{
		/// <summary>No flags.</summary>
		None = 0,
		/// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
		Remove = 0x00000001,
		/// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
		Exclude = 0x00000010,
		/// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
		PageOnly = 0x00000020,
		/// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
		NoLegacy = 0x00000030,
		/// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
		InputSink = 0x00000100,
		/// <summary>If set, the mouse button click does not activate the other window.</summary>
		CaptureMouse = 0x00000200,
		/// <summary>If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
		NoHotKeys = 0x00000200,
		/// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
		AppKeys = 0x00000400
	}

	/// <summary>
	/// Enumeration containing the type device the raw input is coming from.
	/// </summary>
	enum RawInputType
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
		/// Another device that is not the keyboard or the mouse.
		/// </summary>
		HID = 2
	}

	/// <summary>
	/// Enumeration containing the flags for raw mouse data.
	/// </summary>
	[Flags]
	enum RawMouseFlags
		: uint
	{
		/// <summary>Relative to the last position.</summary>
		MoveRelative = 0,
		/// <summary>Absolute positioning.</summary>
		MoveAbsolute = 1,
		/// <summary>Coordinate data is mapped to a virtual desktop.</summary>
		VirtualDesktop = 2,
		/// <summary>Attributes for the mouse have changed.</summary>
		AttributesChanged = 4
	}

	/// <summary>
	/// Enumeration containing the button data for raw mouse input.
	/// </summary>
	[Flags]
	enum RawMouseButtons
		: ushort
	{
		/// <summary>No button.</summary>
		None = 0,
		/// <summary>Left (button 1) down.</summary>
		LeftDown = 0x0001,
		/// <summary>Left (button 1) up.</summary>
		LeftUp = 0x0002,
		/// <summary>Right (button 2) down.</summary>
		RightDown = 0x0004,
		/// <summary>Right (button 2) up.</summary>
		RightUp = 0x0008,
		/// <summary>Middle (button 3) down.</summary>
		MiddleDown = 0x0010,
		/// <summary>Middle (button 3) up.</summary>
		MiddleUp = 0x0020,
		/// <summary>Button 4 down.</summary>
		Button4Down = 0x0040,
		/// <summary>Button 4 up.</summary>
		Button4Up = 0x0080,
		/// <summary>Button 5 down.</summary>
		Button5Down = 0x0100,
		/// <summary>Button 5 up.</summary>
		Button5Up = 0x0200,
		/// <summary>Mouse wheel moved.</summary>
		MouseWheel = 0x0400
	}

	/// <summary>
	/// Enumeration containing flags for raw keyboard input.
	/// </summary>
	[Flags]
	enum RawKeyboardFlags
		: ushort
	{
		/// <summary></summary>
		KeyMake = 0,
		/// <summary></summary>
		KeyBreak = 1,
		/// <summary></summary>
		KeyE0 = 2,
		/// <summary></summary>
		KeyE1 = 4,
		/// <summary></summary>
		TerminalServerSetLED = 8,
		/// <summary></summary>
		TerminalServerShadow = 0x10
	}

	/// <summary>
	/// Enumeration contanining the command types to issue.
	/// </summary>
	enum RawInputCommand
	{
		/// <summary>
		/// Get input data.
		/// </summary>
		Input = 0x10000003,
		/// <summary>
		/// Get header data.
		/// </summary>
		Header = 0x10000005,
		/// <summary>
		/// Previously parsed data.
		/// </summary>
		PreparsedData = 0x20000005,
		/// <summary>
		/// Only return the device name, return value means number of characters, not bytes.
		/// </summary>
		DeviceName = 0x20000007,
		/// <summary>
		/// Return RAWINPUTDEVICEINFO data.
		/// </summary>
		DeviceInfo = 0x2000000B
	}

	/// <summary>
	/// Enumeration for virtual keys.
	/// </summary>
	enum VirtualKeys
		: ushort
	{
		/// <summary>Key: None</summary>
		None = 0x0000,
		/// <summary>Key: LButton</summary>
		LButton = 0x0001,
		/// <summary>Key: RButton</summary>
		RButton = 0x0002,
		/// <summary>Key: Cancel</summary>
		Cancel = 0x0003,
		/// <summary>Key: MButton</summary>
		MButton = 0x0004,
		/// <summary>Key: XButton1</summary>
		XButton1 = 0x0005,
		/// <summary>Key: XButton2</summary>
		XButton2 = 0x0006,
		/// <summary>Key: Back</summary>
		Back = 0x0008,
		/// <summary>Key: Tab</summary>
		Tab = 0x0009,
		/// <summary>Key: LineFeed</summary>
		LineFeed = 0x000A,
		/// <summary>Key: Clear</summary>
		Clear = 0x000C,
		/// <summary>Key: Enter</summary>
		Enter = 0x000D,
		/// <summary>Key: Return</summary>
		Return = 0x000D,
		/// <summary>Key: ShiftKey</summary>
		ShiftKey = 0x0010,
		/// <summary>Key: ControlKey</summary>
		ControlKey = 0x0011,
		/// <summary>Key: Menu</summary>
		Menu = 0x0012,
		/// <summary>Key: Pause</summary>
		Pause = 0x0013,
		/// <summary>Key: CapsLock</summary>
		CapsLock = 0x0014,
		/// <summary>Key: Capital</summary>
		Capital = 0x0014,
		/// <summary>Key: HangulMode</summary>
		HangulMode = 0x0015,
		/// <summary>Key: HanguelMode</summary>
		HanguelMode = 0x0015,
		/// <summary>Key: KanaMode</summary>
		KanaMode = 0x0015,
		/// <summary>Key: JunjaMode</summary>
		JunjaMode = 0x0017,
		/// <summary>Key: FinalMode</summary>
		FinalMode = 0x0018,
		/// <summary>Key: KanjiMode</summary>
		KanjiMode = 0x0019,
		/// <summary>Key: HanjaMode</summary>
		HanjaMode = 0x0019,
		/// <summary>Key: Escape</summary>
		Escape = 0x001B,
		/// <summary>Key: IMEConvert</summary>
		IMEConvert = 0x001C,
		/// <summary>Key: IMENonconvert</summary>
		IMENonconvert = 0x001D,
		/// <summary>Key: IMEAccept</summary>
		IMEAccept = 0x001E,
		/// <summary>Key: IMEAceept</summary>
		IMEAceept = 0x001E,
		/// <summary>Key: IMEModeChange</summary>
		IMEModeChange = 0x001F,
		/// <summary>Key: Space</summary>
		Space = 0x0020,
		/// <summary>Key: Prior</summary>
		Prior = 0x0021,
		/// <summary>Key: PageUp</summary>
		PageUp = 0x0021,
		/// <summary>Key: PageDown</summary>
		PageDown = 0x0022,
		/// <summary>Key: Next</summary>
		Next = 0x0022,
		/// <summary>Key: End</summary>
		End = 0x0023,
		/// <summary>Key: Home</summary>
		Home = 0x0024,
		/// <summary>Key: Left</summary>
		Left = 0x0025,
		/// <summary>Key: Up</summary>
		Up = 0x0026,
		/// <summary>Key: Right</summary>
		Right = 0x0027,
		/// <summary>Key: Down</summary>
		Down = 0x0028,
		/// <summary>Key: Select</summary>
		Select = 0x0029,
		/// <summary>Key: Print</summary>
		Print = 0x002A,
		/// <summary>Key: Execute</summary>
		Execute = 0x002B,
		/// <summary>Key: Snapshot</summary>
		Snapshot = 0x002C,
		/// <summary>Key: PrintScreen</summary>
		PrintScreen = 0x002C,
		/// <summary>Key: Insert</summary>
		Insert = 0x002D,
		/// <summary>Key: Delete</summary>
		Delete = 0x002E,
		/// <summary>Key: Help</summary>
		Help = 0x002F,
		/// <summary>Key: D0</summary>
		D0 = 0x0030,
		/// <summary>Key: D1</summary>
		D1 = 0x0031,
		/// <summary>Key: D2</summary>
		D2 = 0x0032,
		/// <summary>Key: D3</summary>
		D3 = 0x0033,
		/// <summary>Key: D4</summary>
		D4 = 0x0034,
		/// <summary>Key: D5</summary>
		D5 = 0x0035,
		/// <summary>Key: D6</summary>
		D6 = 0x0036,
		/// <summary>Key: D7</summary>
		D7 = 0x0037,
		/// <summary>Key: D8</summary>
		D8 = 0x0038,
		/// <summary>Key: D9</summary>
		D9 = 0x0039,
		/// <summary>Key: A</summary>
		A = 0x0041,
		/// <summary>Key: B</summary>
		B = 0x0042,
		/// <summary>Key: C</summary>
		C = 0x0043,
		/// <summary>Key: D</summary>
		D = 0x0044,
		/// <summary>Key: E</summary>
		E = 0x0045,
		/// <summary>Key: F</summary>
		F = 0x0046,
		/// <summary>Key: G</summary>
		G = 0x0047,
		/// <summary>Key: H</summary>
		H = 0x0048,
		/// <summary>Key: I</summary>
		I = 0x0049,
		/// <summary>Key: J</summary>
		J = 0x004A,
		/// <summary>Key: K</summary>
		K = 0x004B,
		/// <summary>Key: L</summary>
		L = 0x004C,
		/// <summary>Key: M</summary>
		M = 0x004D,
		/// <summary>Key: N</summary>
		N = 0x004E,
		/// <summary>Key: O</summary>
		O = 0x004F,
		/// <summary>Key: P</summary>
		P = 0x0050,
		/// <summary>Key: Q</summary>
		Q = 0x0051,
		/// <summary>Key: R</summary>
		R = 0x0052,
		/// <summary>Key: S</summary>
		S = 0x0053,
		/// <summary>Key: T</summary>
		T = 0x0054,
		/// <summary>Key: U</summary>
		U = 0x0055,
		/// <summary>Key: V</summary>
		V = 0x0056,
		/// <summary>Key: W</summary>
		W = 0x0057,
		/// <summary>Key: X</summary>
		X = 0x0058,
		/// <summary>Key: Y</summary>
		Y = 0x0059,
		/// <summary>Key: Z</summary>
		Z = 0x005A,
		/// <summary>Key: LWin</summary>
		LWin = 0x005B,
		/// <summary>Key: RWin</summary>
		RWin = 0x005C,
		/// <summary>Key: Apps</summary>
		Apps = 0x005D,
		/// <summary>Key: Sleep</summary>
		Sleep = 0x005F,
		/// <summary>Key: NumPad0</summary>
		NumPad0 = 0x0060,
		/// <summary>Key: NumPad1</summary>
		NumPad1 = 0x0061,
		/// <summary>Key: NumPad2</summary>
		NumPad2 = 0x0062,
		/// <summary>Key: NumPad3</summary>
		NumPad3 = 0x0063,
		/// <summary>Key: NumPad4</summary>
		NumPad4 = 0x0064,
		/// <summary>Key: NumPad5</summary>
		NumPad5 = 0x0065,
		/// <summary>Key: NumPad6</summary>
		NumPad6 = 0x0066,
		/// <summary>Key: NumPad7</summary>
		NumPad7 = 0x0067,
		/// <summary>Key: NumPad8</summary>
		NumPad8 = 0x0068,
		/// <summary>Key: NumPad9</summary>
		NumPad9 = 0x0069,
		/// <summary>Key: Multiply</summary>
		Multiply = 0x006A,
		/// <summary>Key: Add</summary>
		Add = 0x006B,
		/// <summary>Key: Separator</summary>
		Separator = 0x006C,
		/// <summary>Key: Subtract</summary>
		Subtract = 0x006D,
		/// <summary>Key: Decimal</summary>
		Decimal = 0x006E,
		/// <summary>Key: Divide</summary>
		Divide = 0x006F,
		/// <summary>Key: F1</summary>
		F1 = 0x0070,
		/// <summary>Key: F2</summary>
		F2 = 0x0071,
		/// <summary>Key: F3</summary>
		F3 = 0x0072,
		/// <summary>Key: F4</summary>
		F4 = 0x0073,
		/// <summary>Key: F5</summary>
		F5 = 0x0074,
		/// <summary>Key: F6</summary>
		F6 = 0x0075,
		/// <summary>Key: F7</summary>
		F7 = 0x0076,
		/// <summary>Key: F8</summary>
		F8 = 0x0077,
		/// <summary>Key: F9</summary>
		F9 = 0x0078,
		/// <summary>Key: F10</summary>
		F10 = 0x0079,
		/// <summary>Key: F11</summary>
		F11 = 0x007A,
		/// <summary>Key: F12</summary>
		F12 = 0x007B,
		/// <summary>Key: F13</summary>
		F13 = 0x007C,
		/// <summary>Key: F14</summary>
		F14 = 0x007D,
		/// <summary>Key: F15</summary>
		F15 = 0x007E,
		/// <summary>Key: F16</summary>
		F16 = 0x007F,
		/// <summary>Key: F17</summary>
		F17 = 0x0080,
		/// <summary>Key: F18</summary>
		F18 = 0x0081,
		/// <summary>Key: F19</summary>
		F19 = 0x0082,
		/// <summary>Key: F20</summary>
		F20 = 0x0083,
		/// <summary>Key: F21</summary>
		F21 = 0x0084,
		/// <summary>Key: F22</summary>
		F22 = 0x0085,
		/// <summary>Key: F23</summary>
		F23 = 0x0086,
		/// <summary>Key: F24</summary>
		F24 = 0x0087,
		/// <summary>Key: NumLock</summary>
		NumLock = 0x0090,
		/// <summary>Key: Scroll</summary>
		Scroll = 0x0091,
		/// <summary>Key: LShiftKey</summary>
		LShiftKey = 0x00A0,
		/// <summary>Key: RShiftKey</summary>
		RShiftKey = 0x00A1,
		/// <summary>Key: LControlKey</summary>
		LControlKey = 0x00A2,
		/// <summary>Key: RControlKey</summary>
		RControlKey = 0x00A3,
		/// <summary>Key: LMenu</summary>
		LMenu = 0x00A4,
		/// <summary>Key: RMenu</summary>
		RMenu = 0x00A5,
		/// <summary>Key: BrowserBack</summary>
		BrowserBack = 0x00A6,
		/// <summary>Key: BrowserForward</summary>
		BrowserForward = 0x00A7,
		/// <summary>Key: BrowserRefresh</summary>
		BrowserRefresh = 0x00A8,
		/// <summary>Key: BrowserStop</summary>
		BrowserStop = 0x00A9,
		/// <summary>Key: BrowserSearch</summary>
		BrowserSearch = 0x00AA,
		/// <summary>Key: BrowserFavorites</summary>
		BrowserFavorites = 0x00AB,
		/// <summary>Key: BrowserHome</summary>
		BrowserHome = 0x00AC,
		/// <summary>Key: VolumeMute</summary>
		VolumeMute = 0x00AD,
		/// <summary>Key: VolumeDown</summary>
		VolumeDown = 0x00AE,
		/// <summary>Key: VolumeUp</summary>
		VolumeUp = 0x00AF,
		/// <summary>Key: MediaNextTrack</summary>
		MediaNextTrack = 0x00B0,
		/// <summary>Key: MediaPreviousTrack</summary>
		MediaPreviousTrack = 0x00B1,
		/// <summary>Key: MediaStop</summary>
		MediaStop = 0x00B2,
		/// <summary>Key: MediaPlayPause</summary>
		MediaPlayPause = 0x00B3,
		/// <summary>Key: LaunchMail</summary>
		LaunchMail = 0x00B4,
		/// <summary>Key: SelectMedia</summary>
		SelectMedia = 0x00B5,
		/// <summary>Key: LaunchApplication1</summary>
		LaunchApplication1 = 0x00B6,
		/// <summary>Key: LaunchApplication2</summary>
		LaunchApplication2 = 0x00B7,
		/// <summary>Key: OemSemicolon</summary>
		OemSemicolon = 0x00BA,
		/// <summary>Key: Oem1</summary>
		Oem1 = 0x00BA,
		/// <summary>Key: Oemplus</summary>
		Oemplus = 0x00BB,
		/// <summary>Key: Oemcomma</summary>
		Oemcomma = 0x00BC,
		/// <summary>Key: OemMinus</summary>
		OemMinus = 0x00BD,
		/// <summary>Key: OemPeriod</summary>
		OemPeriod = 0x00BE,
		/// <summary>Key: Oem2</summary>
		Oem2 = 0x00BF,
		/// <summary>Key: OemQuestion</summary>
		OemQuestion = 0x00BF,
		/// <summary>Key: Oem3</summary>
		Oem3 = 0x00C0,
		/// <summary>Key: Oemtilde</summary>
		Oemtilde = 0x00C0,
		/// <summary>Key: Oem4</summary>
		Oem4 = 0x00DB,
		/// <summary>Key: OemOpenBrackets</summary>
		OemOpenBrackets = 0x00DB,
		/// <summary>Key: OemPipe</summary>
		OemPipe = 0x00DC,
		/// <summary>Key: Oem5</summary>
		Oem5 = 0x00DC,
		/// <summary>Key: OemCloseBrackets</summary>
		OemCloseBrackets = 0x00DD,
		/// <summary>Key: Oem6</summary>
		Oem6 = 0x00DD,
		/// <summary>Key: OemQuotes</summary>
		OemQuotes = 0x00DE,
		/// <summary>Key: Oem7</summary>
		Oem7 = 0x00DE,
		/// <summary>Key: Oem8</summary>
		Oem8 = 0x00DF,
		/// <summary>Key: Oem102</summary>
		Oem102 = 0x00E2,
		/// <summary>Key: OemBackslash</summary>
		OemBackslash = 0x00E2,
		/// <summary>Key: ProcessKey</summary>
		ProcessKey = 0x00E5,
		/// <summary>Key: Packet</summary>
		Packet = 0x00E7,
		/// <summary>Key: Attn</summary>
		Attn = 0x00F6,
		/// <summary>Key: Crsel</summary>
		Crsel = 0x00F7,
		/// <summary>Key: Exsel</summary>
		Exsel = 0x00F8,
		/// <summary>Key: EraseEof</summary>
		EraseEof = 0x00F9,
		/// <summary>Key: Play</summary>
		Play = 0x00FA,
		/// <summary>Key: Zoom</summary>
		Zoom = 0x00FB,
		/// <summary>Key: NoName</summary>
		NoName = 0x00FC,
		/// <summary>Key: Pa1</summary>
		Pa1 = 0x00FD,
		/// <summary>Key: OemClear</summary>
		OemClear = 0x00FE,
		/// <summary>Key: KeyCode</summary>
		KeyCode = 0xFFFF,
		/// <summary>Key: Shift</summary>
		Shift = 0x0000,
		/// <summary>Key: Control</summary>
		Control = 0x0000,
		/// <summary>Key: Alt</summary>
		Alt = 0x0000,
		/// <summary>Key: Modifiers</summary>
		Modifiers = 0x0000
	}

	/// <summary>
	/// Types of messages that passed to a window.
	/// </summary>
	/// <remarks>See the MSDN documentation for more detail.</remarks>
	enum WindowMessages
	{
		/// <summary>System command (WM_SYSCOMMAND)</summary>
		SysCommand = 0x0112,
		/// <summary>Quit command (WM_QUIT)</summary>
		Quit = 0x0012,
		/// <summary>Window has been resized. (WM_SIZE)</summary>
		Size = 0x0005,
		/// <summary>Query the drag icon. (WM_QUERYDRAGICON)</summary>
		QueryDragIcon = 0x0037,
		/// <summary>Get the window icon. (WM_GETICON)</summary>
		GetIcon = 0x007F,
		/// <summary>Set the window icon. (WM_SETICON)</summary>
		SetIcon = 0x0080,
		/// <summary></summary>
		NULL = 0,
		/// <summary></summary>
		Create = 0x0001,
		/// <summary></summary>
		Destroy = 0x0002,
		/// <summary></summary>
		Move = 0x0003,
		/// <summary></summary>
		Activate = 0x0006,
		/// <summary></summary>
		SetFocus = 0x0007,
		/// <summary></summary>
		KillFocus = 0x0008,
		/// <summary></summary>
		Enable = 0x000A,
		/// <summary></summary>
		SetRedraw = 0x000B,
		/// <summary></summary>
		SetText = 0x000C,
		/// <summary></summary>
		GetText = 0x000D,
		/// <summary></summary>
		GetTextLength = 0x000E,
		/// <summary></summary>
		Paint = 0x000F,
		/// <summary></summary>
		Close = 0x0010,
		/// <summary></summary>
		QueryEndSession = 0x0011,
		/// <summary></summary>
		QueryOpen = 0x0013,
		/// <summary></summary>
		EraseBackground = 0x0014,
		/// <summary></summary>
		SystemColorChange = 0x0015,
		/// <summary></summary>
		EndSession = 0x0016,
		/// <summary></summary>
		ShowWindow = 0x0018,
		/// <summary></summary>
		ControlColor = 0x0019,
		/// <summary></summary>
		WinINIChange = 0x001A,
		/// <summary></summary>
		SettingChange = 0x001A,
		/// <summary></summary>
		DeviceModeChange = 0x001B,
		/// <summary></summary>
		ActivateApplication = 0x001C,
		/// <summary></summary>
		FontChange = 0x001D,
		/// <summary></summary>
		TimeChange = 0x001E,
		/// <summary></summary>
		CancelMode = 0x001F,
		/// <summary></summary>
		SetCursor = 0x0020,
		/// <summary></summary>
		MouseActivate = 0x0021,
		/// <summary></summary>
		ChildActivate = 0x0022,
		/// <summary></summary>
		QueueSync = 0x0023,
		/// <summary></summary>
		GetMinMaxInformation = 0x0024,
		/// <summary></summary>
		PaintIcon = 0x0026,
		/// <summary></summary>
		IconEraseBackground = 0x0027,
		/// <summary></summary>
		NextDialogControl = 0x0028,
		/// <summary></summary>
		SpoolerStatus = 0x002A,
		/// <summary></summary>
		DrawItem = 0x002B,
		/// <summary></summary>
		MeasureItem = 0x002C,
		/// <summary></summary>
		DeleteItem = 0x002D,
		/// <summary></summary>
		VKeyToItem = 0x002E,
		/// <summary></summary>
		CharToItem = 0x002F,
		/// <summary></summary>
		SetFont = 0x0030,
		/// <summary></summary>
		GetFont = 0x0031,
		/// <summary></summary>
		SetHotKey = 0x0032,
		/// <summary></summary>
		GetHotKey = 0x0033,
		/// <summary></summary>
		CompareItem = 0x0039,
		/// <summary></summary>
		GetObject = 0x003D,
		/// <summary></summary>
		Compacting = 0x0041,
		/// <summary></summary>
		COMMNotify = 0x0044,
		/// <summary></summary>
		WindowPositionChanging = 0x0046,
		/// <summary></summary>
		WindowPositionChanged = 0x0047,
		/// <summary></summary>
		Power = 0x0048,
		/// <summary></summary>
		CopyData = 0x004A,
		/// <summary></summary>
		CancelJournal = 0x004B,
		/// <summary></summary>
		Notify = 0x004E,
		/// <summary></summary>
		InputLanguageChangeRequest = 0x0050,
		/// <summary></summary>
		InputLanguageChange = 0x0051,
		/// <summary></summary>
		TCard = 0x0052,
		/// <summary></summary>
		Help = 0x0053,
		/// <summary></summary>
		UserChanged = 0x0054,
		/// <summary></summary>
		NotifyFormat = 0x0055,
		/// <summary></summary>
		ContextMenu = 0x007B,
		/// <summary></summary>
		StyleChanging = 0x007C,
		/// <summary></summary>
		StyleChanged = 0x007D,
		/// <summary></summary>
		DisplayChange = 0x007E,
		/// <summary></summary>
		NCCreate = 0x0081,
		/// <summary></summary>
		NCDestroy = 0x0082,
		/// <summary></summary>
		NCCalcSize = 0x0083,
		/// <summary></summary>
		NCHitTest = 0x0084,
		/// <summary></summary>
		NCPaint = 0x0085,
		/// <summary></summary>
		NCActivate = 0x0086,
		/// <summary></summary>
		GetDialogCode = 0x0087,
		/// <summary></summary>
		SynchronizePaint = 0x0088,
		/// <summary></summary>
		NCMouseMove = 0x00A0,
		/// <summary></summary>
		NCLeftButtonDown = 0x00A1,
		/// <summary></summary>
		NCLeftButtonUp = 0x00A2,
		/// <summary></summary>
		NCLeftButtonDoubleClick = 0x00A3,
		/// <summary></summary>
		NCRightButtonDown = 0x00A4,
		/// <summary></summary>
		NCRightButtonUp = 0x00A5,
		/// <summary></summary>
		NCRightButtonDoubleClick = 0x00A6,
		/// <summary></summary>
		NCMiddleButtonDown = 0x00A7,
		/// <summary></summary>
		NCMiddleButtonUp = 0x00A8,
		/// <summary></summary>
		NCMiddleButtonDoubleClick = 0x00A9,
		/// <summary></summary>
		KeyDown = 0x0100,
		/// <summary></summary>
		KeyUp = 0x0101,
		/// <summary></summary>
		Char = 0x0102,
		/// <summary></summary>
		DeadChar = 0x0103,
		/// <summary></summary>
		SysKeyDown = 0x0104,
		/// <summary></summary>
		SysKeyUp = 0x0105,
		/// <summary></summary>
		SysChar = 0x0106,
		/// <summary></summary>
		SysDeadChar = 0x0107,
		/// <summary>
		/// 
		/// </summary>
		UniChar = 0x0109,
		/// <summary></summary>
		KeyLast = 0x0180,
		/// <summary></summary>
		IMEStartComposition = 0x010D,
		/// <summary></summary>
		IMEEndComposition = 0x010E,
		/// <summary></summary>
		IMEComposition = 0x010F,
		/// <summary></summary>
		IMEKeyLast = 0x010F,
		/// <summary></summary>
		InitializeDialog = 0x0110,
		/// <summary></summary>
		Command = 0x0111,
		/// <summary></summary>
		Timer = 0x0113,
		/// <summary></summary>
		HorizontalScroll = 0x0114,
		/// <summary></summary>
		VerticalScroll = 0x0115,
		/// <summary></summary>
		InitializeMenu = 0x0116,
		/// <summary></summary>
		InitializeMenuPopup = 0x0117,
		/// <summary></summary>
		MenuSelect = 0x011F,
		/// <summary></summary>
		MenuChar = 0x0120,
		/// <summary></summary>
		EnterIdle = 0x0121,
		/// <summary></summary>
		MenuRightButtonUp = 0x0122,
		/// <summary></summary>
		MenuDrag = 0x0123,
		/// <summary></summary>
		MenuGetObject = 0x0124,
		/// <summary></summary>
		UnInitializeMenuPopup = 0x0125,
		/// <summary></summary>
		MenuCommand = 0x0126,
		/// <summary></summary>
		ControlColorMessageBox = 0x0132,
		/// <summary></summary>
		ControlColorEdit = 0x0133,
		/// <summary></summary>
		ControlColorListBox = 0x0134,
		/// <summary></summary>
		ControlColorButton = 0x0135,
		/// <summary></summary>
		ControlColorDialog = 0x0136,
		/// <summary></summary>
		ControlColorScrollbar = 0x0137,
		/// <summary></summary>
		ControlColorStatic = 0x0138,
		/// <summary></summary>
		MouseMove = 0x0200,
		/// <summary></summary>
		LeftButtonDown = 0x0201,
		/// <summary></summary>
		LeftButtonUp = 0x0202,
		/// <summary></summary>
		LeftButtonDoubleClick = 0x0203,
		/// <summary></summary>
		RightButtonDown = 0x0204,
		/// <summary></summary>
		RightButtonUp = 0x0205,
		/// <summary></summary>
		RightButtonDoubleClick = 0x0206,
		/// <summary></summary>
		MiddleButtonDown = 0x0207,
		/// <summary></summary>
		MiddleButtonUp = 0x0208,
		/// <summary></summary>
		MiddleButtonDoubleClick = 0x0209,
		/// <summary></summary>
		MouseWheel = 0x020A,
		/// <summary>
		/// 
		/// </summary>
		XButtonDown = 0x20B,
		/// <summary>
		/// 
		/// </summary>
		XButtonUp = 0x20C,
		/// <summary>
		/// 
		/// </summary>
		XButtonDoubleClick = 0x20D,
		/// <summary></summary>
		ParentNotify = 0x0210,
		/// <summary></summary>
		EnterMenuLoop = 0x0211,
		/// <summary></summary>
		ExitMenuLoop = 0x0212,
		/// <summary></summary>
		NextMenu = 0x0213,
		/// <summary></summary>
		Sizing = 0x0214,
		/// <summary></summary>
		CaptureChanged = 0x0215,
		/// <summary></summary>
		Moving = 0x0216,
		/// <summary></summary>
		DeviceChange = 0x0219,
		/// <summary></summary>
		MDICreate = 0x0220,
		/// <summary></summary>
		MDIDestroy = 0x0221,
		/// <summary></summary>
		MDIActivate = 0x0222,
		/// <summary></summary>
		MDIRestore = 0x0223,
		/// <summary></summary>
		MDINext = 0x0224,
		/// <summary></summary>
		MDIMaximize = 0x0225,
		/// <summary></summary>
		MDITile = 0x0226,
		/// <summary></summary>
		MDICacade = 0x0227,
		/// <summary></summary>
		MDIIconArrange = 0x0228,
		/// <summary></summary>
		MDIGetActive = 0x0229,
		/// <summary></summary>
		MDISetMenu = 0x0230,
		/// <summary></summary>
		EnterSizeMove = 0x0231,
		/// <summary></summary>
		ExitSizeMove = 0x0232,
		/// <summary></summary>
		DropFiles = 0x0233,
		/// <summary></summary>
		MDIRefreshMenu = 0x0234,
		/// <summary></summary>
		IMESetContext = 0x0281,
		/// <summary></summary>
		IMENotify = 0x0282,
		/// <summary></summary>
		IMEControl = 0x0283,
		/// <summary></summary>
		IMECompositionFull = 0x0284,
		/// <summary></summary>
		IMESelect = 0x0285,
		/// <summary></summary>
		IMEChar = 0x0286,
		/// <summary></summary>
		IMERequest = 0x0288,
		/// <summary></summary>
		IMEKeyDown = 0x0290,
		/// <summary></summary>
		IMEKeyUp = 0x0291,
		/// <summary></summary>
		MouseHover = 0x02A1,
		/// <summary></summary>
		MouseLeave = 0x02A3,
		/// <summary></summary>
		Cut = 0x0300,
		/// <summary></summary>
		Copy = 0x0301,
		/// <summary></summary>
		Paste = 0x0302,
		/// <summary></summary>
		Clear = 0x0303,
		/// <summary></summary>
		Undo = 0x0304,
		/// <summary></summary>
		RenderFormat = 0x0305,
		/// <summary></summary>
		RenderAllFormats = 0x0306,
		/// <summary></summary>
		DestroyClipboard = 0x0307,
		/// <summary></summary>
		DrawClipboard = 0x0308,
		/// <summary></summary>
		Paintclipboard = 0x0309,
		/// <summary></summary>
		VerticalScrollClipboard = 0x030A,
		/// <summary></summary>
		SizeClipboard = 0x030B,
		/// <summary></summary>
		AskClipboardFormatName = 0x030C,
		/// <summary></summary>
		ChangeClipboardChain = 0x030D,
		/// <summary></summary>
		HorizontalScrollClipboard = 0x030E,
		/// <summary></summary>
		QueryNewPalette = 0x030F,
		/// <summary></summary>
		PaletteIsChanging = 0x0310,
		/// <summary></summary>
		PaletteChanged = 0x0311,
		/// <summary></summary>
		HotKey = 0x0312,
		/// <summary></summary>
		Print = 0x0317,
		/// <summary></summary>
		PrintClient = 0x0318,
		/// <summary>
		/// 
		/// </summary>
		AppCommand = 0x0319,
		/// <summary></summary>
		HandheldFirst = 0x0358,
		/// <summary></summary>
		HandheldLast = 0x035F,
		/// <summary></summary>
		AFXFirst = 0x0360,
		/// <summary></summary>
		AFXLast = 0x037F,
		/// <summary></summary>
		PenWindowFirst = 0x0380,
		/// <summary></summary>
		PenWindowLast = 0x038F,
		/// <summary></summary>
		Application = 0x8000,
		/// <summary></summary>
		User = 0x0400,
		/// <summary></summary>
		RawInput = 0x00FF
	}
	#endregion
    // ReSharper restore InconsistentNaming
}
