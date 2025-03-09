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
// Created: January 21, 2025 6:13:04 PM
//

using System.Runtime.CompilerServices;

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// A list of virtual keys for keyboard input.
/// </summary>
public enum VirtualKeys
{
    /// <summary>
    /// No key.
    /// </summary>
    None = 0x0,
    /// <summary>
    /// Left mouse button.
    /// </summary>
    LeftButton = 0x01,
    /// <summary>
    /// Right mouse button.
    /// </summary>
    RightButton = 0x02,
    /// <summary>
    /// Control break processing.
    /// </summary>
    Cancel = 0x03,
    /// <summary>
    /// Middle mouse button
    /// </summary>
    MiddleButton = 0x04,
    /// <summary>
    /// X1 mouse button
    /// </summary>
    XButton1 = 0x05,
    /// <summary>
    /// X2 mouse button
    /// </summary>
    XButton2 = 0x06,
    /// <summary>
    /// BACKSPACE key
    /// </summary>
    Backspace = 0x08,
    /// <summary>
    /// TAB key
    /// </summary>
    Tab = 0x09,
    /// <summary>
    /// CLEAR key
    /// </summary>
    Clear = 0x0C,
    /// <summary>
    /// ENTER key
    /// </summary>
    Enter = 0x0D,
    /// <summary>
    /// SHIFT key
    /// </summary>
    Shift = 0x10,
    /// <summary>
    /// CTRL key
    /// </summary>
    Control = 0x11,
    /// <summary>
    /// ALT key
    /// </summary>
    Alt = 0x12,
    /// <summary>
    /// PAUSE key
    /// </summary>
    Pause = 0x13,
    /// <summary>
    /// CAPS LOCK key
    /// </summary>
    CapsLock = 0x14,
    /// <summary>
    /// IME Kana mode
    /// </summary>
    IMEKana = 0x15,
    /// <summary>
    /// IME Hangul mode
    /// </summary>
    IMEHangul = IMEKana,
    /// <summary>
    /// IME On
    /// </summary>
    IMEOn = 0x16,
    /// <summary>
    /// IME Junja mode
    /// </summary>
    IMEJunja = 0x17,
    /// <summary>
    /// IME final mode
    /// </summary>
    IMEFinal = 0x18,
    /// <summary>
    /// IME Hanja mode
    /// </summary>
    IMEHanja = 0x19,
    /// <summary>
    /// IME Kanji mode
    /// </summary>
    IMEKanji = IMEHanja,
    /// <summary>
    /// IME Off
    /// </summary>
    IMEOff = 0x1A,
    /// <summary>
    /// ESC key
    /// </summary>
    Escape = 0x1B,
    /// <summary>
    /// IME convert
    /// </summary>
    IMEConvert = 0x1C,
    /// <summary>
    /// IME nonconvert
    /// </summary>
    IMENonConvert = 0x1D,
    /// <summary>
    /// IME accept
    /// </summary>
    IMEAccept = 0x1E,
    /// <summary>
    /// IME mode change request
    /// </summary>
    IMEModeChange = 0x1F,
    /// <summary>
    /// SPACEBAR
    /// </summary>
    Space = 0x20,
    /// <summary>
    /// PAGE UP key
    /// </summary>
    PageUp = 0x21,
    /// <summary>
    /// PAGE DOWN key
    /// </summary>
    PageDown = 0x22,
    /// <summary>
    /// END key
    /// </summary>
    End = 0x23,
    /// <summary>
    /// HOME key
    /// </summary>
    Home = 0x24,
    /// <summary>
    /// LEFT ARROW key
    /// </summary>
    Left = 0x25,
    /// <summary>
    /// UP ARROW key
    /// </summary>
    Up = 0x26,
    /// <summary>
    /// RIGHT ARROW key
    /// </summary>
    Right = 0x27,
    /// <summary>
    /// DOWN ARROW key
    /// </summary>
    Down = 0x28,
    /// <summary>
    /// SELECT key
    /// </summary>
    Select = 0x29,
    /// <summary>
    /// PRINT key
    /// </summary>
    Print = 0x2A,
    /// <summary>
    /// EXECUTE key
    /// </summary>
    Execute = 0x2B,
    /// <summary>
    /// PRINT SCREEN key
    /// </summary>
    PrintScreen = 0x2C,
    /// <summary>
    /// INS key
    /// </summary>
    Insert = 0x2D,
    /// <summary>
    /// DEL key
    /// </summary>
    Delete = 0x2E,
    /// <summary>
    /// HELP key
    /// </summary>
    Help = 0x2F,
    /// <summary>
    /// 0 key
    /// </summary>
    Zero = 0x30,
    /// <summary>
    /// 1 key
    /// </summary>
    One = 0x31,
    /// <summary>
    /// 2 key
    /// </summary>
    Two = 0x32,
    /// <summary>
    /// 3 key
    /// </summary>
    Three = 0x33,
    /// <summary>
    /// 4 key
    /// </summary>
    Four = 0x34,
    /// <summary>
    /// 5 key
    /// </summary>
    Five = 0x35,
    /// <summary>
    /// 6 key
    /// </summary>
    Six = 0x36,
    /// <summary>
    /// 7 key
    /// </summary>
    Seven = 0x37,
    /// <summary>
    /// 8 key
    /// </summary>
    Eight = 0x38,
    /// <summary>
    /// 9 key
    /// </summary>
    Nine = 0x39,
    /// <summary>
    /// A key
    /// </summary>
    A = 0x41,
    /// <summary>
    /// B key
    /// </summary>
    B = 0x42,
    /// <summary>
    /// C key
    /// </summary>
    C = 0x43,
    /// <summary>
    /// D key
    /// </summary>
    D = 0x44,
    /// <summary>
    /// E key
    /// </summary>
    E = 0x45,
    /// <summary>
    /// F key
    /// </summary>
    F = 0x46,
    /// <summary>
    /// G key
    /// </summary>
    G = 0x47,
    /// <summary>
    /// H key
    /// </summary>
    H = 0x48,
    /// <summary>
    /// I key
    /// </summary>
    I = 0x49,
    /// <summary>
    /// J key
    /// </summary>
    J = 0x4A,
    /// <summary>
    /// K key
    /// </summary>
    K = 0x4B,
    /// <summary>
    /// L key
    /// </summary>
    L = 0x4C,
    /// <summary>
    /// M key
    /// </summary>
    M = 0x4D,
    /// <summary>
    /// N key
    /// </summary>
    N = 0x4E,
    /// <summary>
    /// O key
    /// </summary>
    O = 0x4F,
    /// <summary>
    /// P key
    /// </summary>
    P = 0x50,
    /// <summary>
    /// Q key
    /// </summary>
    Q = 0x51,
    /// <summary>
    /// R key
    /// </summary>
    R = 0x52,
    /// <summary>
    /// S key
    /// </summary>
    S = 0x53,
    /// <summary>
    /// T key
    /// </summary>
    T = 0x54,
    /// <summary>
    /// U key
    /// </summary>
    U = 0x55,
    /// <summary>
    /// V key
    /// </summary>
    V = 0x56,
    /// <summary>
    /// W key
    /// </summary>
    W = 0x57,
    /// <summary>
    /// X key
    /// </summary>
    X = 0x58,
    /// <summary>
    /// Y key
    /// </summary>
    Y = 0x59,
    /// <summary>
    /// Z key
    /// </summary>
    Z = 0x5A,
    /// <summary>
    /// Left Windows key
    /// </summary>
    LeftWin = 0x5B,
    /// <summary>
    /// Right Windows key
    /// </summary>
    RightWin = 0x5C,
    /// <summary>
    /// Applications key
    /// </summary>
    Applications = 0x5D,
    /// <summary>
    /// The context menu key.
    /// </summary>
    ContextMenu = Applications,
    /// <summary>
    /// Computer Sleep key
    /// </summary>
    Sleep = 0x5F,
    /// <summary>
    /// Numeric keypad 0 key
    /// </summary>
    NumZero = 0x60,
    /// <summary>
    /// Numeric keypad 1 key
    /// </summary>,
    NumOne = 0x61,
    /// <summary>
    /// Numeric keypad 2 key
    /// </summary>
    NumTwo = 0x62,
    /// <summary>
    /// Numeric keypad 3 key
    /// </summary>
    NumThree = 0x63,
    /// <summary>
    /// Numeric keypad 4 key
    /// </summary>
    NumFour = 0x64,
    /// <summary>
    /// Numeric keypad 5 key
    /// </summary>
    NumFive = 0x65,
    /// <summary>
    /// Numeric keypad 6 key
    /// </summary>
    NumSix = 0x66,
    /// <summary>
    /// Numeric keypad 7 key
    /// </summary>
    NumSeven = 0x67,
    /// <summary>
    /// Numeric keypad 8 key
    /// </summary>
    NumEight = 0x68,
    /// <summary>
    /// Numeric keypad 9 key
    /// </summary>
    NumNine = 0x69,
    /// <summary>
    /// Multiply key
    /// </summary>
    Multiply = 0x6A,
    /// <summary>
    /// Add key
    /// </summary>
    Add = 0x6B,
    /// <summary>
    /// Separator key
    /// </summary>
    Separator = 0x6C,
    /// <summary>
    /// Subtract key
    /// </summary>
    Subtract = 0x6D,
    /// <summary>
    /// Decimal key
    /// </summary>
    Decimal = 0x6E,
    /// <summary>
    /// Period key
    /// </summary>
    Period = Decimal,
    /// <summary>
    /// Divide key
    /// </summary>
    Divide = 0x6F,
    /// <summary>
    /// F1 key
    /// </summary>
    F1 = 0x70,
    /// <summary>
    /// F2 key
    /// </summary>
    F2 = 0x71,
    /// <summary>
    /// F3 key
    /// </summary>
    F3 = 0x72,
    /// <summary>
    /// F4 key
    /// </summary>
    F4 = 0x73,
    /// <summary>
    /// F5 key
    /// </summary>
    F5 = 0x74,
    /// <summary>
    /// F6 key
    /// </summary>
    F6 = 0x75,
    /// <summary>
    /// F7 key
    /// </summary>
    F7 = 0x76,
    /// <summary>
    /// F8 key
    /// </summary>
    F8 = 0x77,
    /// <summary>
    /// F9 key
    /// </summary>
    F9 = 0x78,
    /// <summary>
    /// F10 key
    /// </summary>
    F10 = 0x79,
    /// <summary>
    /// F11 key
    /// </summary>
    F11 = 0x7A,
    /// <summary>
    /// F12 key
    /// </summary>
    F12 = 0x7B,
    /// <summary>
    /// F13 key
    /// </summary>
    F13 = 0x7C,
    /// <summary>
    /// F14 key
    /// </summary>
    F14 = 0x7D,
    /// <summary>
    /// F15 key
    /// </summary>
    F15 = 0x7E,
    /// <summary>
    /// F16 key
    /// </summary>
    F16 = 0x7F,
    /// <summary>
    /// F17 key
    /// </summary>
    F17 = 0x80,
    /// <summary>
    /// F18 key
    /// </summary>
    F18 = 0x81,
    /// <summary>
    /// F19 key
    /// </summary>
    F19 = 0x82,
    /// <summary>
    /// F20 key
    /// </summary>
    F20 = 0x83,
    /// <summary>
    /// F21 key
    /// </summary>
    F21 = 0x84,
    /// <summary>
    /// F22 key
    /// </summary>
    F22 = 0x85,
    /// <summary>
    /// F23 key
    /// </summary>
    F23 = 0x86,
    /// <summary>
    /// F24 key
    /// </summary>
    F24 = 0x87,
    /// <summary>
    /// NUM LOCK key
    /// </summary>
    NumLock = 0x90,
    /// <summary>
    /// SCROLL LOCK key
    /// </summary>
    ScrollLock = 0x91,
    /// <summary>
    /// OEM specific key 1.
    /// </summary>
    OemKey1 = 0x92,
    /// <summary>
    /// OEM specific key 2.
    /// </summary>
    OemKey2 = 0x93,
    /// <summary>
    /// OEM specific key 3.
    /// </summary>
    OemKey3 = 0x94,
    /// <summary>
    /// OEM specific key 4.
    /// </summary>
    OemKey4 = 0x95,
    /// <summary>
    /// OEM specific key 5.
    /// </summary>
    OemKey5 = 0x96,
    /// <summary>
    /// Left SHIFT key
    /// </summary>
    LeftShift = 0xA0,
    /// <summary>
    /// Right SHIFT key
    /// </summary>
    RightShift = 0xA1,
    /// <summary>
    /// Left CONTROL key
    /// </summary>
    LeftControl = 0xA2,
    /// <summary>
    /// Right CONTROL key
    /// </summary>
    RightControl = 0xA3,
    /// <summary>
    /// Left ALT key
    /// </summary>
    LeftAlt = 0xA4,
    /// <summary>
    /// Right ALT key
    /// </summary>
    RightAlt = 0xA5,
    /// <summary>
    /// Browser Back key
    /// </summary>
    BrowserBack = 0xA6,
    /// <summary>
    /// Browser Forward key
    /// </summary>
    BrowserForward = 0xA7,
    /// <summary>
    /// Browser Refresh key
    /// </summary>
    BrowserRefresh = 0xA8,
    /// <summary>
    /// Browser Stop key
    /// </summary>
    BrowserStop = 0xA9,
    /// <summary>
    /// Browser Search key
    /// </summary>
    BrowserSearch = 0xAA,
    /// <summary>
    /// Browser Favorites key
    /// </summary>
    BrowserFavorites = 0xAB,
    /// <summary>
    /// Browser Start and Home key
    /// </summary>
    BrowserHome = 0xAC,
    /// <summary>
    /// Volume Mute key
    /// </summary>
    VolumeMute = 0xAD,
    /// <summary>
    /// Volume Down key
    /// </summary>
    VolumeDown = 0xAE,
    /// <summary>
    /// Volume Up key
    /// </summary>
    VolumeUp = 0xAF,
    /// <summary>
    /// Next Track key
    /// </summary>
    MediaNextTrack = 0xB0,
    /// <summary>
    /// Previous Track key
    /// </summary>
    MediaPreviousTrack = 0xB1,
    /// <summary>
    /// Stop Media key
    /// </summary>
    MediaStop = 0xB2,
    /// <summary>
    /// Play/Pause Media key
    /// </summary>
    MediaPause = 0xB3,
    /// <summary>
    /// Start Mail key
    /// </summary>
    LaunchMail = 0xB4,
    /// <summary>
    /// Select Media key
    /// </summary>
    LaunchMediaSelect = 0xB5,
    /// <summary>
    /// Start Application 1 key
    /// </summary>
    LaunchApp1 = 0xB6,
    /// <summary>
    /// Start Application 2 key
    /// </summary>
    LaunchApp2 = 0xB7,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the;: key
    /// </summary>
    OemSemicolon = 0xBA,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the;: key
    /// </summary>
    Oem1 = OemSemicolon,
    /// <summary>
    /// For any country/region, the += key
    /// </summary>
    OemEquals = 0xBB,
    /// <summary>
    /// For any country/region, the &lt;, key
    /// </summary>
    OemComma = 0xBC,
    /// <summary>
    /// For any country/region, the _- key
    /// </summary>
    OemMinus = 0xBD,
    /// <summary>
    /// For any country/region, the &gt;. key
    /// </summary>
    OemPeriod = 0xBE,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the /? key
    /// </summary>
    OemForwardSlash = 0xBF,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the `~ key
    /// </summary>
    OemTilde = 0xC0,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the [{ key
    /// </summary>
    OemLeftSquareBracket = 0xDB,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the \\| key
    /// </summary>
    OemBackSlash = 0xDC,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the ]} key
    /// </summary>
    OemRightSquareBracket = 0xDD,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '" key
    /// </summary>
    OemSingleQuote = 0xDE,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the /? key
    /// </summary>
    Oem2 = OemForwardSlash,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the `~ key
    /// </summary>
    Oem3 = OemTilde,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the [{ key
    /// </summary>
    Oem4 = OemLeftSquareBracket,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the \\| key
    /// </summary>
    Oem5 = OemBackSlash,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the ]} key
    /// </summary>
    Oem6 = OemRightSquareBracket,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '" key
    /// </summary>
    Oem7 = OemSingleQuote,
    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    Oem8 = 0xDF,
    /// <summary>
    /// OEM specific key
    /// </summary>
    OemKey6 = 0xE1,
    /// <summary>
    /// The &lt;&gt; keys on the US standard keyboard, or the \\| key on the non-US 102-key keyboard
    /// </summary>
    Oem102Backslash = 0xE2,
    /// <summary>
    /// The &lt;&gt; keys on the US standard keyboard, or the \\| key on the non-US 102-key keyboard
    /// </summary>
    Oem102 = Oem102Backslash,
    /// <summary>
    /// OEM specific key.
    /// </summary>
    OemKey7 = 0xE3,
    /// <summary>
    /// OEM specific key.
    /// </summary>
    OemKey8 = 0xE4,
    /// <summary>
    /// IME PROCESS key
    /// </summary>
    IMEProcess = 0xE5,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey9 = 0xE6,
    /// <summary>
    /// Used to pass Unicode characters as if they were keystrokes.The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
    /// </summary>
    Packet = 0xE7,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey10 = 0xE9,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey11 = 0xEA,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey12 = 0xEB,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey13 = 0xEC,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey14 = 0xED,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey15 = 0xEE,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey16 = 0xEF,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey17 = 0xF0,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey18 = 0xF1,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey19 = 0xF2,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey20 = 0xF3,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey21 = 0xF4,
    /// <summary>
    /// OEM specific
    /// </summary>
    OemKey22 = 0xF5,
    /// <summary>
    /// Attn key
    /// </summary>
    Attention = 0xF6,
    /// <summary>
    /// CrSel key
    /// </summary>
    CrSel = 0xF7,
    /// <summary>
    /// ExSel key
    /// </summary>
    ExSel = 0xF8,
    /// <summary>
    /// Erase EOF key
    /// </summary>
    EraseEOF = 0xF9,
    /// <summary>
    /// Play key
    /// </summary>
    Play = 0xFA,
    /// <summary>
    /// Zoom key
    /// </summary>
    Zoom = 0xFB,
    /// <summary>
    /// Reserved
    /// </summary>
    NoName = 0xFC,
    /// <summary>
    /// PA1 key
    /// </summary>
    PA1 = 0xFD,
    /// <summary>
    /// Clear key
    /// </summary>
    OemClear = 0xFE,
    /// <summary>
    /// The bitmask to extract a key code from a key value.
    /// </summary>
    KeyCode = 65535,
    /// <summary>
    /// The SHIFT modifier key.
    /// </summary>
    ShiftModifier = 65536,
    /// <summary>
    /// The CTRL modifier key.
    /// </summary>
    ControlModifier = 131072,
    /// <summary>
    /// The ALT modifier key.
    /// </summary>
    AltModifier = 262144
}

/// <summary>
/// Extension methods for working with virtual key values.
/// </summary>
public static class GorgonVirtualKeysExtensions
{
    /// <summary>
    /// Function to find the index of a <see cref="VirtualKeys"/> value in a read only span.
    /// </summary>
    /// <param name="span">The span to evaluate.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The index of the key within the span, or -1 if not found.</returns>
    public static int IndexOf(this ReadOnlySpan<VirtualKeys> span, VirtualKeys key)
    {
        for (int i = 0; i < span.Length; ++i)
        {
            if (span[i] == key)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to determine if a <see cref="VirtualKeys"/> value is contained within a read only span.
    /// </summary>
    /// <param name="span">The span to evaluate.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains(this ReadOnlySpan<VirtualKeys> span, VirtualKeys key) => IndexOf(span, key) != -1;

    /// <summary>
    /// Function to find the index of a <see cref="VirtualKeys"/> value in a read write span.
    /// </summary>
    /// <param name="span">The span to evaluate.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The index of the key within the span, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(this Span<VirtualKeys> span, VirtualKeys key) => IndexOf(span, key);

    /// <summary>
    /// Function to determine if a <see cref="VirtualKeys"/> value is contained within a read write span.
    /// </summary>
    /// <param name="span">The span to evaluate.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains(this Span<VirtualKeys> span, VirtualKeys key) => IndexOf(span, key) != -1;
}