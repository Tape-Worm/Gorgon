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
// Created: June 2, 2025 3:09:41 PM
//

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A codec for a 32 bit RGBA pixel format, where Red, Green, and Blue channels are 10 bits, and alpha is 2 bits.
/// </summary>
internal sealed class Pixel32BppRgb10a2
    : GorgonPixel<uint>
{
    /// <inheritdoc/>
    public override GorgonColor Decode(uint value)
    {
        uint r = value & 0x3ff;
        uint g = (value >> 10) & 0x3ff;
        uint b = (value >> 20) & 0x3ff;
        uint a = (value >> 30) & 3;

        return new GorgonColor(r / 1023.0f, g / 1023.0f, b / 1023.0f, a / 3.0f);
    }

    /// <inheritdoc/>
    public override uint Encode(GorgonColor color)
    {
        uint r = (uint)(color.Red * 0x3ff) & 0x3ff;
        uint g = (uint)(color.Green * 0x3ff) & 0x3ff;
        uint b = (uint)(color.Blue * 0x3ff) & 0x3ff;
        uint a = (uint)(color.Alpha * 3) & 3;

        return r | g << 10 | b << 20 | a << 30;
    }

    /// <inheritdoc/>
    public override bool SupportsFormat(BufferFormat format) => format is BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt;
}
