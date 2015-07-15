#region MIT
// 
// Gorgon.
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
// Created: Monday, July 13, 2015 8:11:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Input
{
	#region Keyboard Keys.
	/// <summary>
	/// Enumeration for keyboard keys.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags", Justification = "Because these are VK codes, which you bastards wrote."), Flags]
	public enum KeyboardKey
		: uint
	{
		/// <summary>Key: None</summary>
		None = 0x00000000,
		/// <summary>Key: LButton</summary>
		LButton = 0x00000001,
		/// <summary>Key: RButton</summary>
		RButton = 0x00000002,
		/// <summary>Key: Cancel</summary>
		Cancel = 0x00000003,
		/// <summary>Key: MButton</summary>
		MButton = 0x00000004,
		/// <summary>Key: XButton1</summary>
		XButton1 = 0x00000005,
		/// <summary>Key: XButton2</summary>
		XButton2 = 0x00000006,
		/// <summary>Key: Back</summary>
		Back = 0x00000008,
		/// <summary>Key: Tab</summary>
		Tab = 0x00000009,
		/// <summary>Key: LineFeed</summary>
		LineFeed = 0x0000000A,
		/// <summary>Key: Clear</summary>
		Clear = 0x0000000C,
		/// <summary>Key: Enter</summary>
		Enter = 0x0000000D,
		/// <summary>Key: Return</summary>
		Return = 0x0000000D,
		/// <summary>Key: ShiftKey</summary>
		ShiftKey = 0x00000010,
		/// <summary>Key: ControlKey</summary>
		ControlKey = 0x00000011,
		/// <summary>Key: Menu</summary>
		Menu = 0x00000012,
		/// <summary>Key: Pause</summary>
		Pause = 0x00000013,
		/// <summary>Key: CapsLock</summary>
		CapsLock = 0x00000014,
		/// <summary>Key: Capital</summary>
		Capital = 0x00000014,
		/// <summary>Key: HangulMode</summary>
		HangulMode = 0x00000015,
		/// <summary>Key: HanguelMode</summary>
		HanguelMode = 0x00000015,
		/// <summary>Key: KanaMode</summary>
		KanaMode = 0x00000015,
		/// <summary>Key: JunjaMode</summary>
		JunjaMode = 0x00000017,
		/// <summary>Key: FinalMode</summary>
		FinalMode = 0x00000018,
		/// <summary>Key: KanjiMode</summary>
		KanjiMode = 0x00000019,
		/// <summary>Key: HanjaMode</summary>
		HanjaMode = 0x00000019,
		/// <summary>Key: Escape</summary>
		Escape = 0x0000001B,
		/// <summary>Key: IMEConvert</summary>
		IMEConvert = 0x0000001C,
		/// <summary>Key: IMENonconvert</summary>
		IMENonconvert = 0x0000001D,
		/// <summary>Key: IMEAccept</summary>
		IMEAccept = 0x0000001E,
		/// <summary>Key: IMEAceept</summary>
		IMEAceept = 0x0000001E,
		/// <summary>Key: IMEModeChange</summary>
		IMEModeChange = 0x0000001F,
		/// <summary>Key: Space</summary>
		Space = 0x00000020,
		/// <summary>Key: Prior</summary>
		Prior = 0x00000021,
		/// <summary>Key: PageUp</summary>
		PageUp = 0x00000021,
		/// <summary>Key: PageDown</summary>
		PageDown = 0x00000022,
		/// <summary>Key: Next</summary>
		Next = 0x00000022,
		/// <summary>Key: End</summary>
		End = 0x00000023,
		/// <summary>Key: Home</summary>
		Home = 0x00000024,
		/// <summary>Key: Left</summary>
		Left = 0x00000025,
		/// <summary>Key: Up</summary>
		Up = 0x00000026,
		/// <summary>Key: Right</summary>
		Right = 0x00000027,
		/// <summary>Key: Down</summary>
		Down = 0x00000028,
		/// <summary>Key: Select</summary>
		Select = 0x00000029,
		/// <summary>Key: Print</summary>
		Print = 0x0000002A,
		/// <summary>Key: Execute</summary>
		Execute = 0x0000002B,
		/// <summary>Key: Snapshot</summary>
		Snapshot = 0x0000002C,
		/// <summary>Key: PrintScreen</summary>
		PrintScreen = 0x0000002C,
		/// <summary>Key: Insert</summary>
		Insert = 0x0000002D,
		/// <summary>Key: Delete</summary>
		Delete = 0x0000002E,
		/// <summary>Key: Help</summary>
		Help = 0x0000002F,
		/// <summary>Key: D0</summary>
		D0 = 0x00000030,
		/// <summary>Key: D1</summary>
		D1 = 0x00000031,
		/// <summary>Key: D2</summary>
		D2 = 0x00000032,
		/// <summary>Key: D3</summary>
		D3 = 0x00000033,
		/// <summary>Key: D4</summary>
		D4 = 0x00000034,
		/// <summary>Key: D5</summary>
		D5 = 0x00000035,
		/// <summary>Key: D6</summary>
		D6 = 0x00000036,
		/// <summary>Key: D7</summary>
		D7 = 0x00000037,
		/// <summary>Key: D8</summary>
		D8 = 0x00000038,
		/// <summary>Key: D9</summary>
		D9 = 0x00000039,
		/// <summary>Key: A</summary>
		A = 0x00000041,
		/// <summary>Key: B</summary>
		B = 0x00000042,
		/// <summary>Key: C</summary>
		C = 0x00000043,
		/// <summary>Key: D</summary>
		D = 0x00000044,
		/// <summary>Key: E</summary>
		E = 0x00000045,
		/// <summary>Key: F</summary>
		F = 0x00000046,
		/// <summary>Key: G</summary>
		G = 0x00000047,
		/// <summary>Key: H</summary>
		H = 0x00000048,
		/// <summary>Key: I</summary>
		I = 0x00000049,
		/// <summary>Key: J</summary>
		J = 0x0000004A,
		/// <summary>Key: K</summary>
		K = 0x0000004B,
		/// <summary>Key: L</summary>
		L = 0x0000004C,
		/// <summary>Key: M</summary>
		M = 0x0000004D,
		/// <summary>Key: N</summary>
		N = 0x0000004E,
		/// <summary>Key: O</summary>
		O = 0x0000004F,
		/// <summary>Key: P</summary>
		P = 0x00000050,
		/// <summary>Key: Q</summary>
		Q = 0x00000051,
		/// <summary>Key: R</summary>
		R = 0x00000052,
		/// <summary>Key: S</summary>
		S = 0x00000053,
		/// <summary>Key: T</summary>
		T = 0x00000054,
		/// <summary>Key: U</summary>
		U = 0x00000055,
		/// <summary>Key: V</summary>
		V = 0x00000056,
		/// <summary>Key: W</summary>
		W = 0x00000057,
		/// <summary>Key: X</summary>
		X = 0x00000058,
		/// <summary>Key: Y</summary>
		Y = 0x00000059,
		/// <summary>Key: Z</summary>
		Z = 0x0000005A,
		/// <summary>Key: LWin</summary>
		LWin = 0x0000005B,
		/// <summary>Key: RWin</summary>
		RWin = 0x0000005C,
		/// <summary>Key: Apps</summary>
		Apps = 0x0000005D,
		/// <summary>Key: Sleep</summary>
		Sleep = 0x0000005F,
		/// <summary>Key: NumPad0</summary>
		NumPad0 = 0x00000060,
		/// <summary>Key: NumPad1</summary>
		NumPad1 = 0x00000061,
		/// <summary>Key: NumPad2</summary>
		NumPad2 = 0x00000062,
		/// <summary>Key: NumPad3</summary>
		NumPad3 = 0x00000063,
		/// <summary>Key: NumPad4</summary>
		NumPad4 = 0x00000064,
		/// <summary>Key: NumPad5</summary>
		NumPad5 = 0x00000065,
		/// <summary>Key: NumPad6</summary>
		NumPad6 = 0x00000066,
		/// <summary>Key: NumPad7</summary>
		NumPad7 = 0x00000067,
		/// <summary>Key: NumPad8</summary>
		NumPad8 = 0x00000068,
		/// <summary>Key: NumPad9</summary>
		NumPad9 = 0x00000069,
		/// <summary>Key: Multiply</summary>
		Multiply = 0x0000006A,
		/// <summary>Key: Add</summary>
		Add = 0x0000006B,
		/// <summary>Key: Separator</summary>
		Separator = 0x0000006C,
		/// <summary>Key: Subtract</summary>
		Subtract = 0x0000006D,
		/// <summary>Key: Decimal</summary>
		Decimal = 0x0000006E,
		/// <summary>Key: Divide</summary>
		Divide = 0x0000006F,
		/// <summary>Key: F1</summary>
		F1 = 0x00000070,
		/// <summary>Key: F2</summary>
		F2 = 0x00000071,
		/// <summary>Key: F3</summary>
		F3 = 0x00000072,
		/// <summary>Key: F4</summary>
		F4 = 0x00000073,
		/// <summary>Key: F5</summary>
		F5 = 0x00000074,
		/// <summary>Key: F6</summary>
		F6 = 0x00000075,
		/// <summary>Key: F7</summary>
		F7 = 0x00000076,
		/// <summary>Key: F8</summary>
		F8 = 0x00000077,
		/// <summary>Key: F9</summary>
		F9 = 0x00000078,
		/// <summary>Key: F10</summary>
		F10 = 0x00000079,
		/// <summary>Key: F11</summary>
		F11 = 0x0000007A,
		/// <summary>Key: F12</summary>
		F12 = 0x0000007B,
		/// <summary>Key: F13</summary>
		F13 = 0x0000007C,
		/// <summary>Key: F14</summary>
		F14 = 0x0000007D,
		/// <summary>Key: F15</summary>
		F15 = 0x0000007E,
		/// <summary>Key: F16</summary>
		F16 = 0x0000007F,
		/// <summary>Key: F17</summary>
		F17 = 0x00000080,
		/// <summary>Key: F18</summary>
		F18 = 0x00000081,
		/// <summary>Key: F19</summary>
		F19 = 0x00000082,
		/// <summary>Key: F20</summary>
		F20 = 0x00000083,
		/// <summary>Key: F21</summary>
		F21 = 0x00000084,
		/// <summary>Key: F22</summary>
		F22 = 0x00000085,
		/// <summary>Key: F23</summary>
		F23 = 0x00000086,
		/// <summary>Key: F24</summary>
		F24 = 0x00000087,
		/// <summary>Key: NumLock</summary>
		NumLock = 0x00000090,
		/// <summary>Key: Scroll</summary>
		Scroll = 0x00000091,
		/// <summary>Key: LShiftKey</summary>
		LShiftKey = 0x000000A0,
		/// <summary>Key: RShiftKey</summary>
		RShiftKey = 0x000000A1,
		/// <summary>Key: LControlKey</summary>
		LControlKey = 0x000000A2,
		/// <summary>Key: RControlKey</summary>
		RControlKey = 0x000000A3,
		/// <summary>Key: LMenu</summary>
		LMenu = 0x000000A4,
		/// <summary>Key: RMenu</summary>
		RMenu = 0x000000A5,
		/// <summary>Key: BrowserBack</summary>
		BrowserBack = 0x000000A6,
		/// <summary>Key: BrowserForward</summary>
		BrowserForward = 0x000000A7,
		/// <summary>Key: BrowserRefresh</summary>
		BrowserRefresh = 0x000000A8,
		/// <summary>Key: BrowserStop</summary>
		BrowserStop = 0x000000A9,
		/// <summary>Key: BrowserSearch</summary>
		BrowserSearch = 0x000000AA,
		/// <summary>Key: BrowserFavorites</summary>
		BrowserFavorites = 0x000000AB,
		/// <summary>Key: BrowserHome</summary>
		BrowserHome = 0x000000AC,
		/// <summary>Key: VolumeMute</summary>
		VolumeMute = 0x000000AD,
		/// <summary>Key: VolumeDown</summary>
		VolumeDown = 0x000000AE,
		/// <summary>Key: VolumeUp</summary>
		VolumeUp = 0x000000AF,
		/// <summary>Key: MediaNextTrack</summary>
		MediaNextTrack = 0x000000B0,
		/// <summary>Key: MediaPreviousTrack</summary>
		MediaPreviousTrack = 0x000000B1,
		/// <summary>Key: MediaStop</summary>
		MediaStop = 0x000000B2,
		/// <summary>Key: MediaPlayPause</summary>
		MediaPlayPause = 0x000000B3,
		/// <summary>Key: LaunchMail</summary>
		LaunchMail = 0x000000B4,
		/// <summary>Key: SelectMedia</summary>
		SelectMedia = 0x000000B5,
		/// <summary>Key: LaunchApplication1</summary>
		LaunchApplication1 = 0x000000B6,
		/// <summary>Key: LaunchApplication2</summary>
		LaunchApplication2 = 0x000000B7,
		/// <summary>Key: OemSemicolon</summary>
		OemSemicolon = 0x000000BA,
		/// <summary>Key: Oem1</summary>
		Oem1 = 0x000000BA,
		/// <summary>Key: Oemplus</summary>
		Oemplus = 0x000000BB,
		/// <summary>Key: Oemcomma</summary>
		Oemcomma = 0x000000BC,
		/// <summary>Key: OemMinus</summary>
		OemMinus = 0x000000BD,
		/// <summary>Key: OemPeriod</summary>
		OemPeriod = 0x000000BE,
		/// <summary>Key: Oem2</summary>
		Oem2 = 0x000000BF,
		/// <summary>Key: OemQuestion</summary>
		OemQuestion = 0x000000BF,
		/// <summary>Key: Oem3</summary>
		Oem3 = 0x000000C0,
		/// <summary>Key: Oemtilde</summary>
		Oemtilde = 0x000000C0,
		/// <summary>Key: Oem4</summary>
		Oem4 = 0x000000DB,
		/// <summary>Key: OemOpenBrackets</summary>
		OemOpenBrackets = 0x000000DB,
		/// <summary>Key: OemPipe</summary>
		OemPipe = 0x000000DC,
		/// <summary>Key: Oem5</summary>
		Oem5 = 0x000000DC,
		/// <summary>Key: OemCloseBrackets</summary>
		OemCloseBrackets = 0x000000DD,
		/// <summary>Key: Oem6</summary>
		Oem6 = 0x000000DD,
		/// <summary>Key: OemQuotes</summary>
		OemQuotes = 0x000000DE,
		/// <summary>Key: Oem7</summary>
		Oem7 = 0x000000DE,
		/// <summary>Key: Oem8</summary>
		Oem8 = 0x000000DF,
		/// <summary>Key: Oem102</summary>
		Oem102 = 0x000000E2,
		/// <summary>Key: OemBackslash</summary>
		OemBackslash = 0x000000E2,
		/// <summary>Key: ProcessKey</summary>
		ProcessKey = 0x000000E5,
		/// <summary>Key: Packet</summary>
		Packet = 0x000000E7,
		/// <summary>Key: Attn</summary>
		Attn = 0x000000F6,
		/// <summary>Key: Crsel</summary>
		Crsel = 0x000000F7,
		/// <summary>Key: Exsel</summary>
		Exsel = 0x000000F8,
		/// <summary>Key: EraseEof</summary>
		EraseEof = 0x000000F9,
		/// <summary>Key: Play</summary>
		Play = 0x000000FA,
		/// <summary>Key: Zoom</summary>
		Zoom = 0x000000FB,
		/// <summary>Key: NoName</summary>
		NoName = 0x000000FC,
		/// <summary>Key: Pa1</summary>
		Pa1 = 0x000000FD,
		/// <summary>Key: OemClear</summary>
		OemClear = 0x000000FE,
		/// <summary>Key: KeyCode</summary>
		KeyCode = 0x0000FFFF,
		/// <summary>Key: Shift</summary>
		Shift = 0x00010000,
		/// <summary>Key: Control</summary>
		Control = 0x00020000,
		/// <summary>Key: Alt</summary>
		Alt = 0x00040000,
		/// <summary>Key: Left version of key.</summary>
		LeftVersion = 0x0080000,
		/// <summary>Key:  Right version of key.</summary>
		RightVersion = 0x0100000,
		/// <summary>Key: Modifiers</summary>
		Modifiers = 0xFFFF0000
	}
	#endregion

	/// <summary>
	/// Key states.
	/// </summary>
	public enum KeyState
	{
		/// <summary>
		/// Key is not pressed.
		/// </summary>
		Up = 0,
		/// <summary>
		/// Key is pressed.
		/// </summary>
		Down = 1
	}

	/// <summary>
	/// Actions to take when resetting the key state reset modes for the keyboard device after its bound control loses focus.
	/// </summary>
	public enum KeyStateResetMode
	{
		/// <summary>
		/// Don't reset after losing focus.
		/// </summary>
		None = 0,
		/// <summary>
		/// Reset only the modifier (Ctrl, Alt and Shift) keys after losing focus.
		/// </summary>
		ResetModifiers = 1,
		/// <summary>
		/// Reset all keys after losing focus.
		/// </summary>
		ResetAll = 2
	}

	/// <summary>
	/// Provides an interface to access a keyboard device.
	/// </summary>
	public interface IGorgonKeyboard
		: IGorgonInputDevice
	{
		#region Properties.
		/// <summary>
		/// Property to return the input service that controls this device.
		/// </summary>
		IGorgonInputService InputService
		{
			get;
		}

		/// <summary>
		/// Property to return information about this keyboard device.
		/// </summary>
		IGorgonKeyboardInfo2 Info
		{
			get;
		}
		#endregion

		#region Methods.
		#endregion
	}
}
