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
/// A codec for a 16 bit RGBA pixel format.
/// </summary>
internal sealed class Pixel16BppBgr555a1
    : GorgonPixel<ushort>
{
    /// <inheritdoc/>
    public override GorgonColor Decode(ushort value)
    {
        uint a = (uint)(value >> 15) & 0x1f;
        uint r = (uint)(value >> 10) & 0x1f;
        uint g = (uint)(value >> 5) & 0x1f;
        uint b = (uint)(value & 0x1f);

        return new GorgonColor(r / 31.0f, g / 31.0f, b / 31.0f, a != 0 ? 1.0f : 0.0f);
    }

    /// <inheritdoc/>
    public override ushort Encode(GorgonColor color)
    {
        uint r = (uint)(color.Red * 31.0f) & 0x1f;
        uint g = (uint)(color.Green * 31.0f) & 0x1f;
        uint b = (uint)(color.Blue * 31.0f) & 0x1f;
        uint a = (color.Alpha > 0.0f) ? 1U : 0U;

        return (ushort)(a << 15 | r << 10 | g << 5 | b);
    }

    /// <inheritdoc/>
    public override bool SupportsFormat(BufferFormat format) => format is BufferFormat.B5G5R5A1_UNorm;
}
