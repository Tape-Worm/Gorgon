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
// Created: June 2, 2025 3:20:02 PM
//

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A predefined list of pixel encoding and decoding interfaces used to manipulate image data.
/// </summary>
/// <seealso cref="GorgonPixel{T}"/>
public static class GorgonPixels
{
    /// <summary>
    /// Encodes/decodes 32 bits per pixel, with an R, G, B, A layout.
    /// </summary>
    public static readonly GorgonPixel<uint> Pixel32BppRgba = new Pixel32BppRgba();

    /// <summary>
    /// Encodes/decodes 32 bits per pixel, with an B, G, R, A layout.
    /// </summary>
    public static readonly GorgonPixel<uint> Pixel32BppBgra = new Pixel32BppBgra();

    /// <summary>
    /// Encodes/decodes 32 bits per pixel, with an R, G, B, A layout. 
    /// </summary>
    /// <remarks>
    /// Red, Green, and Blue channels are 10 bits, and Alpha is 2 bits.
    /// </remarks>
    public static readonly GorgonPixel<uint> Pixel32BppRgb10a2 = new Pixel32BppRgb10a2();

    /// <summary>
    /// Encodes/decodes 16 bits per pixel, with a B, G, R layout.
    /// </summary>
    /// <remarks>
    /// Red channel is 5 bits, Green channel is 6 bits, and Blue channel is 5 bits.
    /// </remarks>
    public static readonly GorgonPixel<ushort> Pixel16BppRgb565 = new Pixel16BppBgr565();

    /// <summary>
    /// Encodes/decodes 16 bits per pixel, with a B, G, R layout.
    /// </summary>
    /// <remarks>
    /// Red channel is 5 bits, Green channel is 5 bits, Blue channel is 5 bits, and Alpha is 1 bit.
    /// </remarks>
    public static readonly GorgonPixel<ushort> Pixel16BppRgb555a1 = new Pixel16BppBgr555a1();

    /// <summary>
    /// Encodes/decodes 16 bits per pixel, with a B, G, R layout.
    /// </summary>
    /// <remarks>
    /// Red channel is 4 bits, Green channel is 4 bits, Blue channel is 4 bits, and Alpha is 4 bits.
    /// </remarks>
    public static readonly GorgonPixel<ushort> Pixel16BppRgb444a4 = new Pixel16BppBgr444a4();

    /// <summary>
    /// Encodes/decodes 8 bits per pixel, utilizing the R channel of a <see cref="GorgonColor"/>.
    /// </summary>
    public static readonly GorgonPixel<byte> Pixel8BppR8 = new Pixel8BppR8();

    /// <summary>
    /// Encodes/decodes 8 bits per pixel, utilizing the A channel of a <see cref="GorgonColor"/>.
    /// </summary>
    public static readonly GorgonPixel<byte> Pixel8BppA8 = new Pixel8BppA8();
}

