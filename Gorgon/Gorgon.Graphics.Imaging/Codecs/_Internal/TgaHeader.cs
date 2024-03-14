
// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
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
// Created: August 8, 2016 12:38:49 AM
// 


using System.Runtime.InteropServices;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Image types
/// </summary>
internal enum TgaImageType
    : byte
{
    /// <summary>
    /// No image data.
    /// </summary>
    NoImage = 0,
    /// <summary>
    /// Color mapped image data.
    /// </summary>
    ColorMapped = 1,
    /// <summary>
    /// True color image data.
    /// </summary>
    TrueColor = 2,
    /// <summary>
    /// Black and white image data.
    /// </summary>
    BlackAndWhite = 3,
    /// <summary>
    /// Compressed color mapped image data.
    /// </summary>
    ColorMappedRLE = 9,
    /// <summary>
    /// Compressed true color image data.
    /// </summary>
    TrueColorRLE = 10,
    /// <summary>
    /// Compressed black and white data.
    /// </summary>
    BlackAndWhiteRLE = 11
}

/// <summary>
/// TGA pixel format descriptor flags
/// </summary>
[Flags]
internal enum TgaDescriptor
    : byte
{
    /// <summary>
    /// 16 bit.
    /// </summary>
    RGB555A1 = 0x1,
    /// <summary>
    /// 32 bit
    /// </summary>
    RGB888A8 = 0x8,
    /// <summary>
    /// Invert on the x-axis.
    /// </summary>
    InvertX = 0x10,
    /// <summary>
    /// Invert on the y-axis.
    /// </summary>
    InvertY = 0x20,
    /// <summary>
    /// 2 way interleaved (depreciated).
    /// </summary>
    Interleaved2Way = 0x40,
    /// <summary>
    /// 4 way interleaved (depreciated).
    /// </summary>
    Interleaved4Way = 0x80
}

/// <summary>
/// Header information for TGA file
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TgaHeader
{
    /// <summary>
    /// Length of the ID.
    /// </summary>
    public readonly byte IDLength;
    /// <summary>
    /// Color map type.
    /// </summary>
    public readonly byte ColorMapType;
    /// <summary>
    /// Image type.
    /// </summary>
    public TgaImageType ImageType;
    /// <summary>
    /// First color map index.
    /// </summary>
    private readonly ushort _colorMapFirst;
    /// <summary>
    /// Length of the color map indices.
    /// </summary>
    public readonly ushort ColorMapLength;
    /// <summary>
    /// Size of the color map.
    /// </summary>
    private readonly byte _colorMapSize;
    /// <summary>
    /// Starting horizontal position.
    /// </summary>
    private readonly ushort _xOrigin;
    /// <summary>
    /// Starting vertical position.
    /// </summary>
    private readonly ushort _yOrigin;
    /// <summary>
    /// Width of the image.
    /// </summary>
    public ushort Width;
    /// <summary>
    /// Height of the image.
    /// </summary>
    public ushort Height;
    /// <summary>
    /// Bits per pixel.
    /// </summary>
    public byte BPP;
    /// <summary>
    /// Descriptor flag.
    /// </summary>
    public TgaDescriptor Descriptor;
}
