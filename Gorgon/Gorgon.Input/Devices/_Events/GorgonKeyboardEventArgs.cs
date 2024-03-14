
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
// Created: Wednesday, July 15, 2015 7:00:06 PM
// 


namespace Gorgon.Input;

/// <summary>
/// Event arguments for the various events triggered on the <see cref="GorgonRawKeyboard"/> interface
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonKeyboardEventArgs"/> class
/// </remarks>
/// <param name="key">Key that is pressed.</param>
/// <param name="modifierKey">Keys that are held down during the event.</param>
/// <param name="scanData">Scan code data.</param>
public class GorgonKeyboardEventArgs(Keys key, Keys modifierKey, int scanData)
        : EventArgs
{

    /// <summary>
    /// Property to return key that is pressed.
    /// </summary>
    public Keys Key
    {
        get;
    } = key;

    /// <summary>
    /// Property to return the keys that are being held down during the event.
    /// </summary>
    public Keys ModifierKeys
    {
        get;
    } = modifierKey;

    /// <summary>
    /// Property to return if ALT is pressed or not.
    /// </summary>
    public bool Alt => (ModifierKeys & Keys.Alt) == Keys.Alt;

    /// <summary>
    /// Property to return if Ctrl is pressed or not.
    /// </summary>
    public bool Ctrl => (ModifierKeys & Keys.Control) == Keys.Control;

    /// <summary>
    /// Property to return if Shift is pressed or not.
    /// </summary>
    public bool Shift => (ModifierKeys & Keys.Shift) == Keys.Shift;

    /// <summary>
    /// Property to return the scan code data.
    /// </summary>
    public int ScanCodeData
    {
        get;
    } = scanData;



}
