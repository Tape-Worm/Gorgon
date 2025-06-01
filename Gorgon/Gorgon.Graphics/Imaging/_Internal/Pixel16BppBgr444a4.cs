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
internal sealed class Pixel16BppBgr444a4
    : GorgonPixel<ushort>
{
    /// <inheritdoc/>
    public override GorgonColor Decode(ushort value)
    {
        uint a = (uint)(value >> 12) & 0xf;
        uint r = (uint)(value >> 8) & 0xf;
        uint g = (uint)(value >> 4) & 0xf;
        uint b = (uint)(value & 0xf);

        return new GorgonColor(r / 15.0f, g / 15.0f, b / 15.0f, a / 15.0f);
    }

    /// <inheritdoc/>
    public override ushort Encode(GorgonColor color)
    {
        uint r = (uint)(color.Red * 15.0f) & 0xf;
        uint g = (uint)(color.Green * 15.0f) & 0xf;
        uint b = (uint)(color.Blue * 15.0f) & 0xf;
        uint a = (uint)(color.Alpha * 15.0f) & 0xf;

        return (ushort)(a << 12 | r << 8 | g << 4 | b);
    }

    /// <inheritdoc/>
    public override bool SupportsFormat(BufferFormat format) => format is BufferFormat.B4G4R4A4_UNorm;
}
