
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
// Created: Wednesday, August 12, 2015 11:45:26 PM
// 

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Enumeration for virtual keys
/// </summary>
internal enum VirtualKeys
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
    Return = Enter,
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
    Capital = CapsLock,
    /// <summary>Key: HangulMode</summary>
    HangulMode = 0x0015,
    /// <summary>Key: HanguelMode</summary>
    HanguelMode = HangulMode,
    /// <summary>Key: KanaMode</summary>
    KanaMode = HangulMode,
    /// <summary>Key: JunjaMode</summary>
    JunjaMode = 0x0017,
    /// <summary>Key: FinalMode</summary>
    FinalMode = 0x0018,
    /// <summary>Key: KanjiMode</summary>
    KanjiMode = 0x0019,
    /// <summary>Key: HanjaMode</summary>
    HanjaMode = KanjiMode,
    /// <summary>Key: Escape</summary>
    Escape = 0x001B,
    /// <summary>Key: IMEConvert</summary>
    IMEConvert = 0x001C,
    /// <summary>Key: IMENonconvert</summary>
    IMENonconvert = 0x001D,
    /// <summary>Key: IMEAccept</summary>
    IMEAccept = 0x001E,
    /// <summary>Key: IMEAceept</summary>
    IMEAceept = IMEAccept,
    /// <summary>Key: IMEModeChange</summary>
    IMEModeChange = 0x001F,
    /// <summary>Key: Space</summary>
    Space = 0x0020,
    /// <summary>Key: Prior</summary>
    Prior = 0x0021,
    /// <summary>Key: PageUp</summary>
    PageUp = Prior,
    /// <summary>Key: PageDown</summary>
    PageDown = 0x0022,
    /// <summary>Key: Next</summary>
    Next = PageDown,
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
    PrintScreen = Snapshot,
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
    Oem1 = OemSemicolon,
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
    OemQuestion = Oem2,
    /// <summary>Key: Oem3</summary>
    Oem3 = 0x00C0,
    /// <summary>Key: Oemtilde</summary>
    Oemtilde = Oem3,
    /// <summary>Key: Oem4</summary>
    Oem4 = 0x00DB,
    /// <summary>Key: OemOpenBrackets</summary>
    OemOpenBrackets = Oem4,
    /// <summary>Key: OemPipe</summary>
    OemPipe = 0x00DC,
    /// <summary>Key: Oem5</summary>
    Oem5 = OemPipe,
    /// <summary>Key: OemCloseBrackets</summary>
    OemCloseBrackets = 0x00DD,
    /// <summary>Key: Oem6</summary>
    Oem6 = OemCloseBrackets,
    /// <summary>Key: OemQuotes</summary>
    OemQuotes = 0x00DE,
    /// <summary>Key: Oem7</summary>
    Oem7 = OemQuotes,
    /// <summary>Key: Oem8</summary>
    Oem8 = 0x00DF,
    /// <summary>Key: Oem102</summary>
    Oem102 = 0x00E2,
    /// <summary>Key: OemBackslash</summary>
    OemBackslash = Oem102,
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
    Shift = None,
    /// <summary>Key: Control</summary>
    Control = None,
    /// <summary>Key: Alt</summary>
    Alt = None,
    /// <summary>Key: Modifiers</summary>
    Modifiers = None
}

/// <summary>
/// Enumeration containing flags for raw keyboard input
/// </summary>
[Flags]
internal enum RawKeyboardFlags
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
    TerminalServerSetLed = 8,
    /// <summary></summary>
    TerminalServerShadow = 0x10
}

/// <summary>
/// Value type for raw input from a keyboard
/// </summary>	
[StructLayout(LayoutKind.Explicit)]
internal struct RAWINPUTKEYBOARD
{
    /// <summary>Scan code for key depression.</summary>
    [FieldOffset(0)]
    public short MakeCode;
    /// <summary>Scan code information.</summary>
    [FieldOffset(2)]
    public RawKeyboardFlags Flags;
    /// <summary>Reserved.</summary>
    [FieldOffset(4)]
    public short Reserved;
    /// <summary>Virtual key code.</summary>
    [FieldOffset(6)]
    public VirtualKeys VirtualKey;
    /// <summary>Corresponding window message.</summary>
    [FieldOffset(8)]
    public uint Message;
    /// <summary>Extra information.</summary>
    [FieldOffset(12)]
    public int ExtraInformation;
}
