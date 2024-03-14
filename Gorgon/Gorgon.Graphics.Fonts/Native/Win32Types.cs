
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Sunday, July 24, 2011 10:16:37 PM
// 


using System.Runtime.InteropServices;

namespace Gorgon.Native;

// ReSharper disable InconsistentNaming

/// <summary>
/// Map modes for SetMapMode
/// </summary>
internal enum MapModes
{
    /// <summary>
    /// Each logical unit is mapped to one device pixel. Positive x is to the right; positive y is down.
    /// </summary>
    MM_TEXT = 1,
    /// <summary>
    /// Each logical unit is mapped to 0.1 millimeter. Positive x is to the right; positive y is up.
    /// </summary>
    MM_LOMETRIC = 2,
    /// <summary>
    /// Each logical unit is mapped to 0.01 millimeter. Positive x is to the right; positive y is up.
    /// </summary>
    MM_HIMETRIC = 3,
    /// <summary>
    /// Each logical unit is mapped to 0.1 millimeter. Positive x is to the right; positive y is up.
    /// </summary>
    MM_LOENGLISH = 4,
    /// <summary>
    /// Each logical unit is mapped to 0.01 inch. Positive x is to the right; positive y is up.
    /// </summary>
    MM_HIENGLISH = 5,
    /// <summary>
    /// Each logical unit is mapped to one twentieth of a printer's point (1/1440 inch, also called a twip). Positive x is to the right; positive y is up.
    /// </summary>
    MM_TWIPS = 6,
    /// <summary>
    /// Logical units are mapped to arbitrary units with equally scaled axes; that is, one unit along the x-axis is equal to one unit along the y-axis. 
    /// </summary>
    MM_ISOTROPIC = 7,
    /// <summary>
    /// Logical units are mapped to arbitrary units with arbitrarily scaled axes.
    /// </summary>
    MM_ANISOTROPIC = 8
}

/// <summary>
/// Structure for font kerning offsets
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ABC
{
    /// <summary>
    /// The A spacing of the character. The A spacing is the distance to add to the current position before drawing the character glyph.
    /// </summary>
    public int A;
    /// <summary>
    /// The B spacing of the character. The B spacing is the width of the drawn portion of the character glyph.
    /// </summary>
    public uint B;
    /// <summary>
    /// The C spacing of the character. The C spacing is the distance to add to the current position to provide white space to the right of the character glyph.
    /// </summary>
    public int C;
}

/// <summary>
/// The KERNINGPAIR structure defines a kerning pair
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct KERNINGPAIR
{
    /// <summary>
    /// The character code for the first character in the kerning pair.
    /// </summary>
    public ushort First;
    /// <summary>
    /// The character code for the second character in the kerning pair.
    /// </summary>
    public ushort Second;
    /// <summary>
    /// The amount this pair will be kerned if they appear side by side in the same font and size. This value is typically negative, because pair kerning usually results in two characters being set more tightly than normal. The value is specified in logical units; that is, it depends on the current mapping mode.
    /// </summary>
    public int KernAmount;
}

// ReSharper restore InconsistentNaming
