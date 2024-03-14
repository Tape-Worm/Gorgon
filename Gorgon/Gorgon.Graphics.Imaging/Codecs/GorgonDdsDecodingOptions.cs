
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
// Created: August 16, 2016 3:08:12 PM
// 


using Gorgon.Configuration;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Flags used to decode an existing DDS image encoded with a legacy version of the format specification
/// </summary>
[Flags]
public enum DdsLegacyFlags
    : uint
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,
    /// <summary>
    /// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
    /// </summary>
    LegacyDWORD = 0x1,
    /// <summary>
    /// Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8) 
    /// </summary>
    NoLegacyExpansion = 0x2,
    /// <summary>
    /// Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
    /// </summary>
    NoR10B10G10A2Fix = 0x4,
    /// <summary>
    /// Convert DXGI 1.1 BGR formats to BufferFormat.R8G8B8A8_UNorm to avoid use of optional WDDM 1.1 formats
    /// </summary>
    ForceRGB = 0x8,
    /// <summary>
    /// Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
    /// </summary>
    No16BPP = 0x10,
    /// <summary>
    /// Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
    /// </summary>
    ForceDX10 = 0x10000
}

/// <summary>
/// Options used when decoding an image from a stream as a DDS file
/// </summary>
public class GorgonDdsDecodingOptions
    : IGorgonImageCodecDecodingOptions
{

    /// <summary>
    /// Property to return the list of options available to the codec.
    /// </summary>
    public IGorgonOptionBag Options
    {
        get;
    }

    /// <summary>
    /// Property to set or return flags used when decoding a file encoded with a legacy DDS format specification.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Older versions of the DDS format specification (or just bugs in general) have made files written with those specifications unreadable by a standardized DDS reader without extra processing. 
    /// These flags define how to handle a legacy encoded file.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Gorgon will make no attempt to identify if a file is "legacy" or not. It is up to the end user to identify which files are legacy, and in what way and provide the appropriate flags to the 
    /// decoder. The best course of action is to use the decoder to decode the file, and then save it again with the proper DDS standard. The codec will write out DDS files in a standardized, modern 
    /// format.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The default value is <see cref="DdsLegacyFlags.None"/>.
    /// </para>
    /// </remarks>
    public DdsLegacyFlags LegacyFormatConversionFlags
    {
        get => Options.GetOptionValue<DdsLegacyFlags>(nameof(LegacyFormatConversionFlags));
        set => Options.SetOptionValue(nameof(LegacyFormatConversionFlags), value);
    }

    /// <summary>
    /// Property to set or return a custom color palette to apply to the DDS file that uses an indexed 8 bit per pixel format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this to define a new palette for the 8 bit indexed image in the DDS file during decoding.
    /// </para>
    /// <para>
    /// This value does not remap the pixel values to the corresponding palette color, therefore, palettes assigned to the image may not give the desired results without careful palette selection.
    /// </para>
    /// <para>
    /// The default value is an empty list.
    /// </para>
    /// </remarks>
    public IList<GorgonColor> Palette
    {
        get => Options.GetOptionValue<IList<GorgonColor>>(nameof(Palette));
        set => Options.SetOptionValue(nameof(Palette), value);
    }

    /// <summary>
    /// Property to set or return whether to read all frames from an image.
    /// </summary>
    /// <remarks>
    /// This flag always returns <b>true</b> for this implementation.
    /// </remarks>
    bool IGorgonImageCodecDecodingOptions.ReadAllFrames
    {
        get => true;
        set
        {
            // Intentionally left blank.
        }
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDdsDecodingOptions"/> class.
    /// </summary>
    public GorgonDdsDecodingOptions() => Options = new GorgonOptionBag(
                                      [
                                            GorgonOption.CreateOption(nameof(LegacyFormatConversionFlags), DdsLegacyFlags.None),
                                            GorgonOption.CreateOption(nameof(Palette), new List<GorgonColor>())
                                      ]);

}
