// GorgonPixel: reading and writing pixels, and the per-format pixel layouts.
// Moved from the Imaging integration battery (Test/Experimental/ImagingFunctions): memory only, no WIC, no codec IO.


namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonPixelTests
{
    [TestMethod]
    [Description("Does GetPixel return the color written by SetPixel at an in-bounds point?")]
    public void GetPixelInBounds() => ImagingTestHelper.AssertPass(CheckGetPixelInBounds());

    [TestMethod]
    [Description("Does GetPixel return BlackTransparent for a point outside the buffer?")]
    public void GetPixelOutOfBounds() => ImagingTestHelper.AssertPass(CheckGetPixelOutOfBounds());

    [TestMethod]
    [Description("Does SetPixel write at (W-1, H-1) and ignore (W, H)?")]
    public void SetPixelEdges() => ImagingTestHelper.AssertPass(CheckSetPixelEdges());

    [TestMethod]
    [Description("Does Pixel32BppRgba encode red as 0xFF0000FF (R in the lowest byte)?")]
    public void Rgba32Layout() => ImagingTestHelper.AssertPass(CheckRgba32Layout());

    [TestMethod]
    [Description("Does Pixel32BppBgra encode red as 0xFFFF0000 (B in the lowest byte)?")]
    public void Bgra32Layout() => ImagingTestHelper.AssertPass(CheckBgra32Layout());

    [TestMethod]
    [Description("Does Pixel16BppBgr565 encode pure red, green and blue as 0xF800, 0x07E0 and 0x001F (B5G6R5_UNorm layout)?")]
    public void Rgb565Layout() => ImagingTestHelper.AssertPass(CheckRgb565Layout());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for every 16 bit B5G6R5 value?")]
    public void Rgb565RoundTrip() => ImagingTestHelper.AssertPass(CheckRgb565RoundTrip());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for every 16 bit B5G5R5A1 value?")]
    public void Rgb5551RoundTrip() => ImagingTestHelper.AssertPass(CheckRgb5551RoundTrip());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for every 16 bit B4G4R4A4 value?")]
    public void Rgb4444RoundTrip() => ImagingTestHelper.AssertPass(CheckRgb4444RoundTrip());

    [TestMethod]
    [Description("Does Pixel16BppA4Bgr444 encode red as 0xF00F and blue as 0x00FF (A4B4G4R4: R 12-15, G 8-11, B 4-7, A 0-3)?")]
    public void A4Bgr444Layout() => ImagingTestHelper.AssertPass(CheckA4Bgr444Layout());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for every 16 bit A4B4G4R4 value?")]
    public void A4Bgr444RoundTrip() => ImagingTestHelper.AssertPass(CheckA4Bgr444RoundTrip());

    [TestMethod]
    [Description("Does SetPixel accept an A4B4G4R4 buffer and write the encoded value?")]
    public void A4Bgr444SetPixel() => ImagingTestHelper.AssertPass(CheckA4Bgr444SetPixel());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for every 10 bit channel value and every 2 bit alpha value?")]
    public void Rgb10a2RoundTrip() => ImagingTestHelper.AssertPass(CheckRgb10a2RoundTrip());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for 32 bit RGBA and BGRA across every 8 bit channel value?")]
    public void Rgba32RoundTrip() => ImagingTestHelper.AssertPass(CheckRgba32RoundTrip());

    [TestMethod]
    [Description("Does Encode(Decode(v)) == v for R8 and A8 for every byte?")]
    public void R8A8RoundTrip() => ImagingTestHelper.AssertPass(CheckR8A8RoundTrip());

    private static string? RoundTrip16(GorgonPixel<ushort> pixel, string name)
    {
        for (int i = 0; i <= ushort.MaxValue; ++i)
        {
            ushort value = (ushort)i;
            ushort result = pixel.Encode(pixel.Decode(value));

            if (result != value)
            {
                return $"{name}: 0x{value:X4} round trips to 0x{result:X4}.";
            }
        }

        return null;
    }

    private static string? CheckGetPixelInBounds()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 8, 8));
        GorgonPixels.Pixel32BppRgba.SetPixel(image.Buffers[0], new GorgonPoint(3, 4), GorgonColors.Red);

        GorgonColor color = GorgonPixels.Pixel32BppRgba.GetPixel(image.Buffers[0], new GorgonPoint(3, 4));

        return ImagingTestHelper.Check(color.Equals(GorgonColors.Red), $"GetPixel returned {color}, expected red.");
    }

    private static string? CheckGetPixelOutOfBounds()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 8, 8));

        GorgonColor color = GorgonPixels.Pixel32BppRgba.GetPixel(image.Buffers[0], new GorgonPoint(8, 8));

        return ImagingTestHelper.Check(color.Equals(GorgonColors.BlackTransparent), $"GetPixel returned {color}, expected BlackTransparent.");
    }

    private static string? CheckSetPixelEdges()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 8, 8, 1, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        image.Buffers[1].Fill(0);

        GorgonPixels.Pixel32BppRgba.SetPixel(buffer, new GorgonPoint(7, 7), GorgonColors.Red);
        // Outside: must not write into the next mip level.
        GorgonPixels.Pixel32BppRgba.SetPixel(buffer, new GorgonPoint(8, 8), GorgonColors.Red);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(ImagingTestHelper.U32(buffer, 7, 7) == 0xFF0000FF, $"Pixel (7, 7) is 0x{ImagingTestHelper.U32(buffer, 7, 7):X8}, expected 0xFF0000FF."),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(image.Buffers[1], 0, 0) == 0, "SetPixel at (8, 8) wrote outside the buffer."));
    }

    private static string? CheckRgba32Layout()
    {
        uint value = GorgonPixels.Pixel32BppRgba.Encode(GorgonColors.Red);

        return ImagingTestHelper.Check(value == 0xFF0000FF, $"Encoded red is 0x{value:X8}, expected 0xFF0000FF.");
    }

    private static string? CheckBgra32Layout()
    {
        uint value = GorgonPixels.Pixel32BppBgra.Encode(GorgonColors.Red);

        return ImagingTestHelper.Check(value == 0xFFFF0000, $"Encoded red is 0x{value:X8}, expected 0xFFFF0000.");
    }

    private static string? CheckRgb565Layout()
    {
        GorgonPixel<ushort> pixel = GorgonPixels.Pixel16BppBgr565;
        ushort red = pixel.Encode(new GorgonColor(1, 0, 0));
        ushort green = pixel.Encode(new GorgonColor(0, 1, 0));
        ushort blue = pixel.Encode(new GorgonColor(0, 0, 1));
        GorgonColor decodedGreen = pixel.Decode(0x07E0);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(red == 0xF800, $"Red encodes as 0x{red:X4}, expected 0xF800."),
                       () => ImagingTestHelper.Check(green == 0x07E0, $"Green encodes as 0x{green:X4}, expected 0x07E0."),
                       () => ImagingTestHelper.Check(blue == 0x001F, $"Blue encodes as 0x{blue:X4}, expected 0x001F."),
                       () => ImagingTestHelper.Check(decodedGreen.Equals(new GorgonColor(0, 1, 0)), $"0x07E0 decodes as {decodedGreen}, expected pure green."));
    }

    private static string? CheckRgb565RoundTrip() => RoundTrip16(GorgonPixels.Pixel16BppBgr565, "B5G6R5");

    private static string? CheckRgb5551RoundTrip() => RoundTrip16(GorgonPixels.Pixel16BppBgr555a1, "B5G5R5A1");

    private static string? CheckRgb4444RoundTrip() => RoundTrip16(GorgonPixels.Pixel16BppBgr444a4, "B4G4R4A4");

    private static string? CheckA4Bgr444Layout()
    {
        ushort red = GorgonPixels.Pixel16BppA4Bgr444.Encode(new GorgonColor(1, 0, 0, 1));
        ushort blue = GorgonPixels.Pixel16BppA4Bgr444.Encode(new GorgonColor(0, 0, 1, 1));

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(red == 0xf00f, $"Red encodes as 0x{red:X4}, expected 0xF00F."),
                       () => ImagingTestHelper.Check(blue == 0x00ff, $"Blue encodes as 0x{blue:X4}, expected 0x00FF."));
    }

    private static string? CheckA4Bgr444RoundTrip() => RoundTrip16(GorgonPixels.Pixel16BppA4Bgr444, "A4B4G4R4");

    private static string? CheckA4Bgr444SetPixel()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.A4B4G4R4_UNorm, 4, 4));
        image.Buffers[0].Fill(0);

        GorgonPixels.Pixel16BppA4Bgr444.SetPixel(image.Buffers[0], new GorgonPoint(2, 3), new GorgonColor(1, 0, 0, 1));

        return ImagingTestHelper.Check(ImagingTestHelper.U16(image.Buffers[0], 2, 3) == 0xf00f, $"Pixel (2, 3) is 0x{ImagingTestHelper.U16(image.Buffers[0], 2, 3):X4}, expected 0xF00F.");
    }

    private static string? CheckRgb10a2RoundTrip()
    {
        GorgonPixel<uint> pixel = GorgonPixels.Pixel32BppRgb10a2;

        for (uint c = 0; c < 1024; ++c)
        {
            uint value = c | ((1023 - c) << 10) | (((c * 7) & 0x3ff) << 20) | ((c & 3) << 30);
            uint result = pixel.Encode(pixel.Decode(value));

            if (result != value)
            {
                return $"0x{value:X8} round trips to 0x{result:X8}.";
            }
        }

        return null;
    }

    private static string? CheckRgba32RoundTrip()
    {
        for (uint c = 0; c < 256; ++c)
        {
            uint value = c | ((255 - c) << 8) | (((c * 7) & 0xff) << 16) | (((c * 13) & 0xff) << 24);
            uint rgba = GorgonPixels.Pixel32BppRgba.Encode(GorgonPixels.Pixel32BppRgba.Decode(value));
            uint bgra = GorgonPixels.Pixel32BppBgra.Encode(GorgonPixels.Pixel32BppBgra.Decode(value));

            if (rgba != value)
            {
                return $"RGBA 0x{value:X8} round trips to 0x{rgba:X8}.";
            }

            if (bgra != value)
            {
                return $"BGRA 0x{value:X8} round trips to 0x{bgra:X8}.";
            }
        }

        return null;
    }

    private static string? CheckR8A8RoundTrip()
    {
        for (int i = 0; i < 256; ++i)
        {
            byte value = (byte)i;
            byte r8 = GorgonPixels.Pixel8BppR8.Encode(GorgonPixels.Pixel8BppR8.Decode(value));
            byte a8 = GorgonPixels.Pixel8BppA8.Encode(GorgonPixels.Pixel8BppA8.Decode(value));

            if (r8 != value)
            {
                return $"R8 {value} round trips to {r8}.";
            }

            if (a8 != value)
            {
                return $"A8 {value} round trips to {a8}.";
            }
        }

        return null;
    }
}
