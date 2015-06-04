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
// Created: Friday, June 24, 2011 10:04:17 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Input.Properties;
using Gorgon.Math;

namespace Gorgon.Input
{
	#region Keyboard Keys.
	/// <summary>
	/// Enumeration for keyboard keys.
	/// </summary>
	[Flags]
	public enum KeyboardKeys
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
	/// Enumeration containing key state.
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
	/// Enumeration containing the key state reset modes for the keyboard device.
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
	/// Object that will represent keyboard data.
	/// </summary>
	public abstract class GorgonKeyboard
		: GorgonInputDevice
	{
		#region Value types.
		/// <summary>
		/// Value type containing the code and text character mapping.
		/// </summary>
		public struct KeyCharMap
            : IEquatable<KeyCharMap>
		{
			#region Variables.
			/// <summary>Key that the character represents.</summary>
			public readonly KeyboardKeys Key;
			/// <summary>Character representation.</summary>
            public readonly char Character;
			/// <summary>Character representation with shift modifier.</summary>
            public readonly char Shifted;
			#endregion

            #region Methods.
            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(Key);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is KeyCharMap)
                {
                    return ((KeyCharMap)obj).Key == Key;
                }

                return base.Equals(obj);
            }

            /// <summary>
            /// Function to determine if 2 instances are equal.
            /// </summary>
            /// <param name="left">The left instance to compare.</param>
            /// <param name="right">The right instance to compare.</param>
            /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
            public static bool Equals(ref KeyCharMap left, ref KeyCharMap right)
            {
                return left.Key == right.Key;
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(Resources.GORINP_KEYCHARMAP_TOSTR, Key, Character, Shifted);
            }

            /// <summary>
            /// Operator to compare 2 instances for equality.
            /// </summary>
            /// <param name="left">Left instance to compare.</param>
            /// <param name="right">Right instance to compare.</param>
            /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
            public static bool operator ==(KeyCharMap left, KeyCharMap right)
            {
                return Equals(ref left, ref right);
            }

            /// <summary>
            /// Operator to compare 2 instances for inequality.
            /// </summary>
            /// <param name="left">Left instance to compare.</param>
            /// <param name="right">Right instance to compare.</param>
            /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
            public static bool operator !=(KeyCharMap left, KeyCharMap right)
            {
                return !Equals(ref left, ref right);
            }
            #endregion

            #region Constructor.
            /// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="key">Key that the character represents.</param>
			/// <param name="character">Character representation.</param>
			/// <param name="shifted">Character representation with shift modifier.</param>
			public KeyCharMap(KeyboardKeys key, char character, char shifted)
			{
				Key = key;
				Character = character;
				Shifted = shifted;
			}
			#endregion

            #region IEquatable<KeyCharMap> Members
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the other parameter; otherwise, false.
            /// </returns>
            public bool Equals(KeyCharMap other)
            {
                return Equals(ref this, ref other);
            }
            #endregion
        }
		#endregion

		#region Classes.
		/// <summary>
		/// Object representing the keyboard mappings.
		/// </summary>
		public class KeyMapCollection
			: ICollection<KeyCharMap>
		{
			#region Variables.
			private readonly SortedDictionary<KeyboardKeys, KeyCharMap> _keys;    // Keyboard mappings.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the key mapping assigned to the key.
			/// </summary>
			/// <param name="key">Key to retrieve mapping for.</param>
			/// <returns>Character mapping for the key.</returns>
			public KeyCharMap this[KeyboardKeys key]
			{
				get
				{
				    KeyCharMap result;

				    if (!_keys.TryGetValue(key, out result))
				    {
                        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEYBOARD_KEY_NO_MAPPING, key));
				    }

				    return result;
				}
				set
				{
				    if (!_keys.ContainsKey(key))
				    {
                        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEYBOARD_KEY_NO_MAPPING, key));
				    }

				    _keys[key] = value;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to add a key mapping to the collection.
			/// </summary>
			/// <param name="key">Key to map.</param>
			/// <param name="value">Value to map to the key.</param>
			/// <param name="shiftedValue">Value to map if the shift key is held down.</param>
			public void Add(KeyboardKeys key, char value, char shiftedValue)
			{
                Add(new KeyCharMap(key, value, shiftedValue));
			}

			/// <summary>
			/// Function to return whether a key exists in this collection or not.
			/// </summary>
			/// <param name="key">Key to check for.</param>
			/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
			public bool Contains(KeyboardKeys key)
			{
				return _keys.ContainsKey(key);
			}

			/// <summary>
			/// Function to remove the specified key from the mappings list.
			/// </summary>
			/// <param name="key">Key to remove.</param>
			public void Remove(KeyboardKeys key)
			{
			    if (!_keys.ContainsKey(key))
			    {
			        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEY_NOT_FOUND, key));
			    }

			    _keys.Remove(key);
			}

			/// <summary>
			/// Function to clear the key mappings.
			/// </summary>
			public void Clear()
			{
				_keys.Clear();
			}

            /// <summary>
            /// Function to attempt to retrieve a key character mapping from the list.
            /// </summary>
            /// <param name="key">Key to look up.</param>
            /// <param name="value">Value in the list.</param>
            /// <returns><b>true</b> if the value was found, <b>false</b> if not.</returns>
		    public bool TryGetValue(KeyboardKeys key, out KeyCharMap value)
		    {
		        return _keys.TryGetValue(key, out value);
		    }
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KeyMapCollection"/> class.
			/// </summary>
			internal KeyMapCollection()
			{
				_keys = new SortedDictionary<KeyboardKeys, KeyCharMap>();
			}
			#endregion

			#region IEnumerable<KeyCharMap> Members
			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<KeyCharMap> GetEnumerator()
			{
			    // ReSharper disable once LoopCanBeConvertedToQuery
			    foreach (KeyValuePair<KeyboardKeys, KeyCharMap> value in _keys)
			    {
			        yield return value.Value;
			    }
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion

            #region ICollection<KeyCharMap> Members
            #region Properties.
            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
            public int Count
            {
                get
                {
                    return _keys.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            public void Add(KeyCharMap item)
            {
                _keys[item.Key] = item;
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            /// </returns>
            public bool Contains(KeyCharMap item)
            {
                return _keys.ContainsValue(item);
            }

            /// <summary>
            /// Copies to.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            public void CopyTo(KeyCharMap[] array, int arrayIndex)
            {
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}

				if ((arrayIndex < 0) || (arrayIndex >= array.Length))
				{
					throw new ArgumentOutOfRangeException("arrayIndex");	
				}

	            int count = array.Length.Min(Count);

				for (int i = arrayIndex; i < count; i++)
				{
					array[i] = this.ElementAt(i);
				}
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </returns>
            /// <exception cref="System.NotSupportedException"></exception>
            bool ICollection<KeyCharMap>.Remove(KeyCharMap item)
            {
                if (!_keys.ContainsKey(item.Key))
                {
                    return false;
                }

                Remove(item.Key);

                return true;
            }
            #endregion
            #endregion
        }

		/// <summary>
		/// Object representing the keyboard state.
		/// </summary>
		public class KeyStateCollection
            : ICollection<KeyState>
		{
			#region Variables.
			private readonly Dictionary<KeyboardKeys, KeyState> _keys;      // Keyboard key state.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the state of a given key.
			/// </summary>
			/// <param name="key">Key to check.</param>
			/// <returns>The state of the key.</returns>
			public KeyState this[KeyboardKeys key]
			{
				get
				{
				    KeyState result;

				    if (!_keys.TryGetValue(key, out result))
				    {
				        _keys.Add(key, KeyState.Up);
				    }

				    return result;
				}
				set
				{
				    _keys[key] = value;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to reset the key states.
			/// </summary>
			public void Reset()
			{
				var keys = (KeyboardKeys[])Enum.GetValues(typeof(KeyboardKeys));

			    foreach (var key in keys)
			    {
			        this[key] = KeyState.Up;
			    }
			}

			/// <summary>
			/// Function to reset any modifier keys.
			/// </summary>
			public void ResetModifiers()
			{
				this[KeyboardKeys.Menu] = KeyState.Up;
				this[KeyboardKeys.RMenu] = KeyState.Up;
				this[KeyboardKeys.LMenu] = KeyState.Up;
				this[KeyboardKeys.ShiftKey] = KeyState.Up;
				this[KeyboardKeys.LShiftKey] = KeyState.Up;
				this[KeyboardKeys.RShiftKey] = KeyState.Up;
				this[KeyboardKeys.ControlKey] = KeyState.Up;
				this[KeyboardKeys.RControlKey] = KeyState.Up;
				this[KeyboardKeys.LControlKey] = KeyState.Up;
				this[KeyboardKeys.Alt] = KeyState.Up;
				this[KeyboardKeys.Control] = KeyState.Up;
				this[KeyboardKeys.Shift] = KeyState.Up;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KeyStateCollection"/> class.
			/// </summary>
			internal KeyStateCollection()
			{
				_keys = new Dictionary<KeyboardKeys, KeyState>();
			}
			#endregion

            #region ICollection<KeyState> Members
            #region Properties.
            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
            public int Count
            {
                get
                {
                    return _keys.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            void ICollection<KeyState>.Add(KeyState item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            void ICollection<KeyState>.Clear()
            {
                Reset();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            /// </returns>
            bool ICollection<KeyState>.Contains(KeyState item)
            {
                return _keys.ContainsValue(item);
            }

            /// <summary>
            /// Copies to.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            void ICollection<KeyState>.CopyTo(KeyState[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </returns>
            /// <exception cref="System.NotSupportedException"></exception>
            bool ICollection<KeyState>.Remove(KeyState item)
            {
                return false;
            }
            #endregion
            #endregion

            #region IEnumerable<KeyState> Members
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<KeyState> GetEnumerator()
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (KeyValuePair<KeyboardKeys, KeyState> state in _keys)
                {
                    yield return state.Value;
                }
            }
            #endregion

            #region IEnumerable Members
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
		#endregion
		
		#region Events.
		/// <summary>
		/// Key down event.
		/// </summary>
		public event EventHandler<KeyboardEventArgs> KeyDown;

		/// <summary>
		/// Key up event.
		/// </summary>
		public event EventHandler<KeyboardEventArgs> KeyUp;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return if the key states should be reset when focus is lost.
		/// </summary>
		/// <remarks>
		/// When the <see cref="P:GorgonLibrary.Input.GorgonInputDevice.BoundControl">BoundControl</see> loses focus, the <see cref="P:GorgonLibrary.Input.GorgonKeyboard.KeyStates">key state buffer</see> may keep the previous key state, leading to undesirable results.  
		/// This setting will allow the user to control how the key states are preserved after a loss of focus.  
		/// <para>The user may also manually reset the key states by setting the individual key state to Up, or use the <see cref="M:GorgonLibrary.Input.GorgonKeyboard.KeyStateCollection.Reset">KeyStates.Reset</see> (or the <see cref="M:GorgonLibrary.Input.GorgonKeyboard.KeyStateCollection.ResetModifiers">KeyStates.ResetModifiers</see>) method.</para>
		/// <para>The key reset settings are as follows:
		/// <list type="table">
		/// <listheader><term>Mode</term><description>Description</description></listheader>
		/// <item>
		///		<term>None</term>
		///		<description>Preserve all previous key states.</description>
		/// </item>
		/// <item>
		///		<term>ResetModifiers</term>
		///		<description>Reset only the modifier keys (Ctrl, Alt, and Shift) to the Up key state.</description>
		/// </item>
		/// <item>
		///		<term>ResetAll (Default)</term>
		///		<description>Reset all keys to the Up key state.</description>
		/// </item>
		/// </list>
		/// </para>
		/// </remarks>
		public KeyStateResetMode KeyStateResetMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the keyboard to character mappings.
		/// </summary>
		public KeyMapCollection KeyMappings
		{
			get;
            private set;
		}

		/// <summary>
		/// Property to return the key states.
		/// </summary>
		public KeyStateCollection KeyStates
		{
			get;
            private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the modifier keys.
		/// </summary>
		/// <returns>The modifier keys.</returns>
		private KeyboardKeys GetModifiers()
		{
			var result = KeyboardKeys.None;

			if (KeyStates[KeyboardKeys.ControlKey] == KeyState.Down)
			{
			    if (KeyStates[KeyboardKeys.LControlKey] == KeyState.Down)
			    {
			        result |= KeyboardKeys.Control | KeyboardKeys.LeftVersion;
			    }

			    if (KeyStates[KeyboardKeys.RControlKey] == KeyState.Down)
			    {
			        result |= KeyboardKeys.Control | KeyboardKeys.RightVersion;
			    }
			}
			if (KeyStates[KeyboardKeys.Menu] == KeyState.Down)
			{
			    if (KeyStates[KeyboardKeys.LMenu] == KeyState.Down)
			    {
			        result |= KeyboardKeys.Alt | KeyboardKeys.LeftVersion;
			    }

			    if (KeyStates[KeyboardKeys.RMenu] == KeyState.Down)
			    {
			        result |= KeyboardKeys.Alt | KeyboardKeys.RightVersion;
			    }

			}
			if (KeyStates[KeyboardKeys.ShiftKey] != KeyState.Down)
			{
				return result;
			}

			if (KeyStates[KeyboardKeys.LShiftKey] == KeyState.Down)
			{
				result |= KeyboardKeys.Shift | KeyboardKeys.LeftVersion;
			}

			if (KeyStates[KeyboardKeys.RShiftKey] == KeyState.Down)
			{
				result |= KeyboardKeys.Shift | KeyboardKeys.RightVersion;
			}

			return result;
		}

		/// <summary>
		/// Function to fire the key down event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyDown(KeyboardKeys key, int scan)
		{
		    if (KeyDown == null)
		    {
		        return;
		    }

		    KeyCharMap character;
            
            KeyMappings.TryGetValue(key, out character);
		    var e = new KeyboardEventArgs(key, GetModifiers(), character, scan);

		    KeyDown(this, e);
		}

		/// <summary>
		/// Function to fire the key up event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyUp(KeyboardKeys key, int scan)
		{
		    if (KeyUp == null)
		    {
		        return;
		    }

		    KeyCharMap character;
                
            KeyMappings.TryGetValue(key, out character);
		    var e = new KeyboardEventArgs(key, GetModifiers(), character, scan);

		    KeyUp(this, e);
		}

		/// <summary>
		/// Function called if the bound window loses focus.
		/// </summary>
		protected override void OnBoundWindowUnfocused()
		{
			base.OnBoundWindowUnfocused();

			// If we lose focus, then remove any modifiers.
			switch(KeyStateResetMode)
			{
				case KeyStateResetMode.ResetAll:
					KeyStates.Reset();
					break;
				case KeyStateResetMode.ResetModifiers:
					KeyStates.ResetModifiers();
					break;
			}
		}
	
		/// <summary>
		/// Function to retrieve character mappings for keys.
		/// </summary>
		public void GetDefaultKeyMapping()
		{
			// Add default key mapping.
			KeyMappings.Clear();
			KeyMappings.Add(KeyboardKeys.Tab, '\t', '\t');
			KeyMappings.Add(KeyboardKeys.Return, '\n', '\n');
			KeyMappings.Add(KeyboardKeys.Space, ' ', ' ');
			KeyMappings.Add(KeyboardKeys.D0, '0', ')');
			KeyMappings.Add(KeyboardKeys.D1, '1', '!');
			KeyMappings.Add(KeyboardKeys.D2, '2', '@');
			KeyMappings.Add(KeyboardKeys.D3, '3', '#');
			KeyMappings.Add(KeyboardKeys.D4, '4', '$');
			KeyMappings.Add(KeyboardKeys.D5, '5', '%');
			KeyMappings.Add(KeyboardKeys.D6, '6', '^');
			KeyMappings.Add(KeyboardKeys.D7, '7', '&');
			KeyMappings.Add(KeyboardKeys.D8, '8', '*');
			KeyMappings.Add(KeyboardKeys.D9, '9', '(');
			KeyMappings.Add(KeyboardKeys.A, 'a', 'A');
			KeyMappings.Add(KeyboardKeys.B, 'b', 'B');
			KeyMappings.Add(KeyboardKeys.C, 'c', 'C');
			KeyMappings.Add(KeyboardKeys.D, 'd', 'D');
			KeyMappings.Add(KeyboardKeys.E, 'e', 'E');
			KeyMappings.Add(KeyboardKeys.F, 'f', 'F');
			KeyMappings.Add(KeyboardKeys.G, 'g', 'G');
			KeyMappings.Add(KeyboardKeys.H, 'h', 'H');
			KeyMappings.Add(KeyboardKeys.I, 'i', 'I');
			KeyMappings.Add(KeyboardKeys.J, 'j', 'J');
			KeyMappings.Add(KeyboardKeys.K, 'k', 'K');
			KeyMappings.Add(KeyboardKeys.L, 'l', 'L');
			KeyMappings.Add(KeyboardKeys.M, 'm', 'M');
			KeyMappings.Add(KeyboardKeys.N, 'n', 'N');
			KeyMappings.Add(KeyboardKeys.O, 'o', 'O');
			KeyMappings.Add(KeyboardKeys.P, 'p', 'P');
			KeyMappings.Add(KeyboardKeys.Q, 'q', 'Q');
			KeyMappings.Add(KeyboardKeys.R, 'r', 'R');
			KeyMappings.Add(KeyboardKeys.S, 's', 'S');
			KeyMappings.Add(KeyboardKeys.T, 't', 'T');
			KeyMappings.Add(KeyboardKeys.U, 'u', 'U');
			KeyMappings.Add(KeyboardKeys.V, 'v', 'V');
			KeyMappings.Add(KeyboardKeys.W, 'w', 'W');
			KeyMappings.Add(KeyboardKeys.X, 'x', 'X');
			KeyMappings.Add(KeyboardKeys.Y, 'y', 'Y');
			KeyMappings.Add(KeyboardKeys.Z, 'z', 'Z');
			KeyMappings.Add(KeyboardKeys.NumPad0, '0', '0');
			KeyMappings.Add(KeyboardKeys.NumPad1, '1', '1');
			KeyMappings.Add(KeyboardKeys.NumPad2, '2', '2');
			KeyMappings.Add(KeyboardKeys.NumPad3, '3', '3');
			KeyMappings.Add(KeyboardKeys.NumPad4, '4', '4');
			KeyMappings.Add(KeyboardKeys.NumPad5, '5', '5');
			KeyMappings.Add(KeyboardKeys.NumPad6, '6', '6');
			KeyMappings.Add(KeyboardKeys.NumPad7, '7', '7');
			KeyMappings.Add(KeyboardKeys.NumPad8, '8', '8');
			KeyMappings.Add(KeyboardKeys.NumPad9, '9', '9');
			KeyMappings.Add(KeyboardKeys.Multiply, '*', '*');
			KeyMappings.Add(KeyboardKeys.Add, '+', '+');
			KeyMappings.Add(KeyboardKeys.Subtract, '-', '-');
			KeyMappings.Add(KeyboardKeys.Divide, '/', '/');
			KeyMappings.Add(KeyboardKeys.OemPipe, '\\', '|');
			KeyMappings.Add(KeyboardKeys.Oem1, ';', ':');
			KeyMappings.Add(KeyboardKeys.OemSemicolon, ';', ':');
			KeyMappings.Add(KeyboardKeys.Oemplus, '=', '+');
			KeyMappings.Add(KeyboardKeys.Oemcomma, ',', '<');
			KeyMappings.Add(KeyboardKeys.OemMinus, '-', '_');
			KeyMappings.Add(KeyboardKeys.OemPeriod, '.', '>');
			KeyMappings.Add(KeyboardKeys.OemQuestion, '/', '?');
			KeyMappings.Add(KeyboardKeys.Oemtilde, '`', '~');
			KeyMappings.Add(KeyboardKeys.OemOpenBrackets, '[', '{');
			KeyMappings.Add(KeyboardKeys.Oem6, ']', '}');
			KeyMappings.Add(KeyboardKeys.Oem7, '\'', '\"');
			KeyMappings.Add(KeyboardKeys.OemBackslash, '\\', '|');
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is NULL (or Nothing in VB.NET).</exception>
		protected GorgonKeyboard(GorgonInputFactory owner, string deviceName)
			: base(owner, deviceName)
		{
            KeyMappings = new KeyMapCollection();
            KeyStates = new KeyStateCollection();
			KeyStateResetMode = KeyStateResetMode.ResetAll;
		}
		#endregion
	}
}
