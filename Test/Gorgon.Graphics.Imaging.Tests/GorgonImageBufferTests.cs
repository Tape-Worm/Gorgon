// GorgonImageBuffer: CopyTo, GetRegion, Fill and SetAlpha.
// Moved from the Imaging integration battery (Test/Experimental/ImagingFunctions): memory only, no WIC, no codec IO.

using System;
using Gorgon.Core;

namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonImageBufferTests
{
    [TestMethod]
    [Description("Does CopyTo copy a whole buffer into one of the same size?")]
    public void CopyToSameSize() => ImagingTestHelper.AssertPass(CheckCopyToSameSize());

    [TestMethod]
    [Description("Does CopyTo copy a source region to a destination offset when both fit?")]
    public void CopyToRegionAndOffset() => ImagingTestHelper.AssertPass(CheckCopyToRegionAndOffset());

    [TestMethod]
    [Description("Does CopyTo place a 16x16 buffer at (40, 40) in a 64x64 buffer?")]
    public void CopyToLargerWithOffset() => ImagingTestHelper.AssertPass(CheckCopyToLargerWithOffset());

    [TestMethod]
    [Description("Does CopyTo clip a 64x64 buffer to a 16x16 destination (docs: 'will provide clipping')?")]
    public void CopyToSmallerClips() => ImagingTestHelper.AssertPass(CheckCopyToSmallerClips());

    [TestMethod]
    [Description("Does CopyTo at (-4, -2) put source (4, 2) at destination (0, 0) and clip the rest?")]
    public void CopyToNegativeOffset() => ImagingTestHelper.AssertPass(CheckCopyToNegativeOffset());

    [TestMethod]
    [Description("With two buffers of the same size, does CopyTo of region (4, 4, 8, 8) to (4, 4) leave the rest of the destination alone?")]
    public void CopyToSameSizeSubRegion() => ImagingTestHelper.AssertPass(CheckCopyToSameSizeSubRegion());

    [TestMethod]
    [Description("Does CopyTo into GorgonImageBuffer.Empty throw ArgumentEmptyException (passing an empty destination is the caller's mistake)?")]
    public void CopyToEmptyThrows() => ImagingTestHelper.AssertPass(CheckCopyToEmptyThrows());

    [TestMethod]
    [Description("Does CopyTo throw ArgumentException for a destination with a different format?")]
    public void CopyToFormatMismatchThrows() => ImagingTestHelper.AssertPass(CheckCopyToFormatMismatchThrows());

    [TestMethod]
    [Description("Does GetRegion return a buffer holding the requested sub rectangle?")]
    public void GetRegionCopiesSubRect() => ImagingTestHelper.AssertPass(CheckGetRegionCopiesSubRect());

    [TestMethod]
    [Description("Does GetRegion return GorgonImageBuffer.Empty (the null sentinel replacement) for an empty clipped region?")]
    public void GetRegionEmptyReturnsEmpty() => ImagingTestHelper.AssertPass(CheckGetRegionEmptyReturnsEmpty());

    [TestMethod]
    [Description("Does Fill set every byte of the buffer?")]
    public void FillSetsEveryByte() => ImagingTestHelper.AssertPass(CheckFillSetsEveryByte());

    [TestMethod]
    [Description("Does SetAlpha(1) on R8G8B8A8 set every alpha to 255 and leave RGB alone?")]
    public void SetAlphaWholeBuffer() => ImagingTestHelper.AssertPass(CheckSetAlphaWholeBuffer());

    [TestMethod]
    [Description("Does SetAlpha with region (4, 8, 4, 4) only change rows 8-11, columns 4-7?")]
    public void SetAlphaRegion() => ImagingTestHelper.AssertPass(CheckSetAlphaRegion());

    [TestMethod]
    [Description("Does SetAlpha leave alpha values below the range minimum alone (range 0.5-1.0, alpha 10 untouched)?")]
    public void SetAlphaRangeMinimum() => ImagingTestHelper.AssertPass(CheckSetAlphaRangeMinimum());

    [TestMethod]
    [Description("Does SetAlpha(1) on R32G32B32A32_Float write an alpha of 1.0?")]
    public void SetAlphaFloat() => ImagingTestHelper.AssertPass(CheckSetAlphaFloat());

    [TestMethod]
    [Description("Does SetAlpha(1) on R16G16B16A16_Float write an alpha of 1.0?")]
    public void SetAlphaHalf() => ImagingTestHelper.AssertPass(CheckSetAlphaHalf());

    [TestMethod]
    [Description("Does SetAlpha(1) on R16G16B16A16_UNorm write 65535?")]
    public void SetAlphaR16UNorm() => ImagingTestHelper.AssertPass(CheckSetAlphaR16UNorm());

    [TestMethod]
    [Description("Does SetAlpha(1) on R8G8B8A8_SNorm (default range -1 to 1) write 127?")]
    public void SetAlphaSNorm8() => ImagingTestHelper.AssertPass(CheckSetAlphaSNorm8());

    [TestMethod]
    [Description("Does SetAlpha(0.5) on R10G10B10A2_UNorm write a 2-bit alpha of 1 or 2?")]
    public void SetAlphaR10G10B10A2Half() => ImagingTestHelper.AssertPass(CheckSetAlphaR10G10B10A2Half());

    [TestMethod]
    [Description("Does SetAlpha(0.7) on B5G5R5A1_UNorm write a 1-bit alpha of 1?")]
    public void SetAlphaB5G5R5A1() => ImagingTestHelper.AssertPass(CheckSetAlphaB5G5R5A1());

    [TestMethod]
    [Description("Does SetAlpha(1, range 0.5-1) on B5G5R5A1_UNorm leave an alpha of 0 alone?")]
    public void SetAlphaB5G5R5A1Range() => ImagingTestHelper.AssertPass(CheckSetAlphaB5G5R5A1Range());

    [TestMethod]
    [Description("Does SetAlpha(1) on A8_UNorm write 255?")]
    public void SetAlphaA8() => ImagingTestHelper.AssertPass(CheckSetAlphaA8());

    private static GorgonImage NewRgba(int width, int height) => new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, width, height));

    // Fills a 16 bit per pixel buffer with one value.
    private static void Fill16(IGorgonImageBuffer buffer, ushort value)
    {
        for (int y = 0; y < buffer.Height; ++y)
        {
            for (int x = 0; x < buffer.Width; ++x)
            {
                ImagingTestHelper.SetU16(buffer, x, y, value);
            }
        }
    }

    // Fills an 8 bit per pixel buffer with one value.
    private static void Fill8(IGorgonImageBuffer buffer, byte value)
    {
        for (int y = 0; y < buffer.Height; ++y)
        {
            for (int x = 0; x < buffer.Width; ++x)
            {
                ImagingTestHelper.SetU8(buffer, x, y, value);
            }
        }
    }

    private static string? CheckCopyToSameSize()
    {
        using GorgonImage source = NewRgba(32, 32);
        using GorgonImage destination = NewRgba(32, 32);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);

        source.Buffers[0].CopyTo(destination.Buffers[0]);

        return ImagingTestHelper.CompareBuffers(source.Buffers[0], destination.Buffers[0], "copy");
    }

    private static string? CheckCopyToRegionAndOffset()
    {
        using GorgonImage source = NewRgba(64, 64);
        using GorgonImage destination = NewRgba(64, 64);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);
        ImagingTestHelper.FillSolid(destination.Buffers[0], 0xdeadbeef);

        source.Buffers[0].CopyTo(destination.Buffers[0], new GorgonRectangle(8, 8, 16, 16), new GorgonPoint(4, 4));

        return ImagingTestHelper.First(() => ImagingTestHelper.CompareRegion(source.Buffers[0], 8, 8, destination.Buffers[0], 4, 4, 16, 16, "copied region"),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 3, 3) == 0xdeadbeef, "Pixel (3, 3) outside the copied region was written."),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 20, 20) == 0xdeadbeef, "Pixel (20, 20) outside the copied region was written."));
    }

    private static string? CheckCopyToLargerWithOffset()
    {
        using GorgonImage source = NewRgba(16, 16);
        using GorgonImage destination = NewRgba(64, 64);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);

        source.Buffers[0].CopyTo(destination.Buffers[0], null, new GorgonPoint(40, 40));

        return ImagingTestHelper.CompareRegion(source.Buffers[0], 0, 0, destination.Buffers[0], 40, 40, 16, 16, "16x16 at (40, 40)");
    }

    private static string? CheckCopyToSmallerClips()
    {
        using GorgonImage source = NewRgba(64, 64);
        using GorgonImage destination = NewRgba(16, 16);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);

        source.Buffers[0].CopyTo(destination.Buffers[0]);

        return ImagingTestHelper.CompareRegion(source.Buffers[0], 0, 0, destination.Buffers[0], 0, 0, 16, 16, "top left 16x16");
    }

    private static string? CheckCopyToNegativeOffset()
    {
        using GorgonImage source = NewRgba(16, 16);
        using GorgonImage destination = NewRgba(16, 16);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);
        ImagingTestHelper.FillSolid(destination.Buffers[0], 0xdeadbeef);

        source.Buffers[0].CopyTo(destination.Buffers[0], null, new GorgonPoint(-4, -2));

        return ImagingTestHelper.First(() => ImagingTestHelper.CompareRegion(source.Buffers[0], 4, 2, destination.Buffers[0], 0, 0, 12, 14, "source (4, 2)-(15, 15) at (0, 0)"),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 12, 0) == 0xdeadbeef, "Pixel (12, 0) past the clipped copy was written."),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 0, 14) == 0xdeadbeef, "Pixel (0, 14) past the clipped copy was written."));
    }

    private static string? CheckCopyToSameSizeSubRegion()
    {
        using GorgonImage source = NewRgba(32, 32);
        using GorgonImage destination = NewRgba(32, 32);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);
        ImagingTestHelper.FillSolid(destination.Buffers[0], 0xdeadbeef);

        source.Buffers[0].CopyTo(destination.Buffers[0], new GorgonRectangle(4, 4, 8, 8), new GorgonPoint(4, 4));

        return ImagingTestHelper.First(() => ImagingTestHelper.CompareRegion(source.Buffers[0], 4, 4, destination.Buffers[0], 4, 4, 8, 8, "copied region"),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 0, 0) == 0xdeadbeef, "Pixel (0, 0) outside the copied region was written."),
                       () => ImagingTestHelper.Check(ImagingTestHelper.U32(destination.Buffers[0], 20, 20) == 0xdeadbeef, "Pixel (20, 20) outside the copied region was written."));
    }

    private static string? CheckCopyToEmptyThrows()
    {
        using GorgonImage source = NewRgba(16, 16);

        return ImagingTestHelper.Throws<ArgumentEmptyException>(() => source.Buffers[0].CopyTo(GorgonImageBuffer.Empty), "CopyTo(Empty)");
    }

    private static string? CheckCopyToFormatMismatchThrows()
    {
        using GorgonImage source = NewRgba(16, 16);
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.B8G8R8A8_UNorm, 16, 16));

        return ImagingTestHelper.Throws<ArgumentException>(() => source.Buffers[0].CopyTo(destination.Buffers[0]), "RGBA into BGRA");
    }

    private static string? CheckGetRegionCopiesSubRect()
    {
        using GorgonImage source = NewRgba(64, 64);
        ImagingTestHelper.FillCoords(source.Buffers[0], 1);

        using IGorgonImageBuffer region = source.Buffers[0].GetRegion(new GorgonRectangle(10, 20, 16, 8));

        return ImagingTestHelper.First(() => ImagingTestHelper.Check((region.Width == 16) && (region.Height == 8), $"Region is {region.Width}x{region.Height}, expected 16x8."),
                       () => ImagingTestHelper.CompareRegion(source.Buffers[0], 10, 20, region, 0, 0, 16, 8, "region"));
    }

    private static string? CheckGetRegionEmptyReturnsEmpty()
    {
        using GorgonImage source = NewRgba(16, 16);

        IGorgonImageBuffer region = source.Buffers[0].GetRegion(new GorgonRectangle(100, 100, 8, 8));

        return ImagingTestHelper.Check(ReferenceEquals(region, GorgonImageBuffer.Empty), "GetRegion did not return GorgonImageBuffer.Empty.");
    }

    private static string? CheckFillSetsEveryByte()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 1, 2));
        IGorgonImageBuffer buffer = image.Buffers[1, 0];
        image.Buffers[0, 0].Fill(0x11);

        buffer.Fill(0x5a);

        for (long i = 0; i < buffer.SizeInBytes; ++i)
        {
            if (buffer.ImageData[i] != 0x5a)
            {
                return $"Byte {i} is 0x{buffer.ImageData[i]:X2}.";
            }
        }

        // The buffer before (mip 0) must be untouched.
        return ImagingTestHelper.Check(image.Buffers[0, 0].ImageData[image.Buffers[0, 0].SizeInBytes - 1] == 0x11, "Fill wrote into the previous mip level.");
    }

    private static string? CheckSetAlphaWholeBuffer()
    {
        using GorgonImage image = NewRgba(16, 16);
        ImagingTestHelper.FillSolid(image.Buffers[0], ImagingTestHelper.Rgba(10, 20, 30, 0));

        image.Buffers[0].SetAlpha(1.0f);

        return ImagingTestHelper.CheckSolid(image.Buffers[0], ImagingTestHelper.Rgba(10, 20, 30, 255), "after SetAlpha(1)");
    }

    private static string? CheckSetAlphaRegion()
    {
        using GorgonImage image = NewRgba(16, 16);
        IGorgonImageBuffer buffer = image.Buffers[0];
        ImagingTestHelper.FillSolid(buffer, ImagingTestHelper.Rgba(10, 20, 30, 0));

        buffer.SetAlpha(1.0f, null, new GorgonRectangle(4, 8, 4, 4));

        for (int y = 0; y < 16; ++y)
        {
            for (int x = 0; x < 16; ++x)
            {
                bool inside = (x is >= 4 and < 8) && (y is >= 8 and < 12);
                byte alpha = (byte)(ImagingTestHelper.U32(buffer, x, y) >> 24);

                if (alpha != (inside ? 255 : 0))
                {
                    return $"Pixel ({x}, {y}) has alpha {alpha}, expected {(inside ? 255 : 0)}.";
                }
            }
        }

        return null;
    }

    private static string? CheckSetAlphaRangeMinimum()
    {
        using GorgonImage image = NewRgba(2, 1);
        IGorgonImageBuffer buffer = image.Buffers[0];
        ImagingTestHelper.SetU32(buffer, 0, 0, ImagingTestHelper.Rgba(1, 2, 3, 10));
        ImagingTestHelper.SetU32(buffer, 1, 0, ImagingTestHelper.Rgba(1, 2, 3, 200));

        buffer.SetAlpha(1.0f, new GorgonRange<float>(0.5f, 1.0f));

        byte alpha0 = (byte)(ImagingTestHelper.U32(buffer, 0, 0) >> 24);
        byte alpha1 = (byte)(ImagingTestHelper.U32(buffer, 1, 0) >> 24);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(alpha0 == 10, $"Alpha 10 (below the 0.5 minimum) became {alpha0}."),
                       () => ImagingTestHelper.Check(alpha1 == 255, $"Alpha 200 (inside the range) became {alpha1}, expected 255."));
    }

    private static string? CheckSetAlphaFloat()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R32G32B32A32_Float, 4, 4));
        IGorgonImageBuffer buffer = image.Buffers[0];

        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 4; ++x)
            {
                ImagingTestHelper.SetF32Channel(buffer, x, y, 0, 0.5f);
                ImagingTestHelper.SetF32Channel(buffer, x, y, 3, 0.25f);
            }
        }

        buffer.SetAlpha(1.0f);

        float alpha = ImagingTestHelper.F32Channel(buffer, 2, 2, 3);
        float red = ImagingTestHelper.F32Channel(buffer, 2, 2, 0);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(alpha == 1.0f, $"Alpha is {alpha}, expected 1.0."),
                       () => ImagingTestHelper.Check(red == 0.5f, $"Red changed to {red}."));
    }

    private static string? CheckSetAlphaHalf()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R16G16B16A16_Float, 4, 4));
        IGorgonImageBuffer buffer = image.Buffers[0];

        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 4; ++x)
            {
                ImagingTestHelper.SetF16Channel(buffer, x, y, 3, (Half)0.25f);
            }
        }

        buffer.SetAlpha(1.0f);

        float alpha = (float)ImagingTestHelper.F16Channel(buffer, 1, 1, 3);

        return ImagingTestHelper.Check(alpha == 1.0f, $"Alpha is {alpha}, expected 1.0.");
    }

    private static string? CheckSetAlphaR16UNorm()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R16G16B16A16_UNorm, 4, 4));
        IGorgonImageBuffer buffer = image.Buffers[0];

        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 4; ++x)
            {
                ImagingTestHelper.SetU16Channel(buffer, x, y, 0, 1234);
                ImagingTestHelper.SetU16Channel(buffer, x, y, 3, 100);
            }
        }

        buffer.SetAlpha(1.0f);

        ushort alpha = ImagingTestHelper.U16Channel(buffer, 3, 3, 3);
        ushort red = ImagingTestHelper.U16Channel(buffer, 3, 3, 0);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(alpha == 65535, $"Alpha is {alpha}, expected 65535."),
                       () => ImagingTestHelper.Check(red == 1234, $"Red changed to {red}."));
    }

    private static string? CheckSetAlphaSNorm8()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_SNorm, 2, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        // Red 10, alpha -64 (0xC0).
        ImagingTestHelper.FillSolid(buffer, 0xC000000A);

        buffer.SetAlpha(1.0f);

        sbyte alpha = (sbyte)(ImagingTestHelper.U32(buffer, 1, 1) >> 24);
        byte red = (byte)ImagingTestHelper.U32(buffer, 1, 1);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(alpha == 127, $"Alpha is {alpha}, expected 127."),
                       () => ImagingTestHelper.Check(red == 10, $"Red changed to {red}."));
    }

    private static string? CheckSetAlphaR10G10B10A2Half()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R10G10B10A2_UNorm, 2, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        // Red 1000, alpha 0.
        ImagingTestHelper.FillSolid(buffer, 1000u);

        buffer.SetAlpha(0.5f);

        uint pixel = ImagingTestHelper.U32(buffer, 1, 1);
        uint alpha = pixel >> 30;

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(alpha is 1 or 2, $"Alpha is {alpha}, expected 1 or 2 (0.5 of 3)."),
                       () => ImagingTestHelper.Check((pixel & 0x3ff) == 1000, $"Red changed to {pixel & 0x3ff}."));
    }

    private static string? CheckSetAlphaB5G5R5A1()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.B5G5R5A1_UNorm, 2, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        Fill16(buffer, 0x001f);

        buffer.SetAlpha(0.7f);

        ushort pixel = ImagingTestHelper.U16(buffer, 1, 1);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check((pixel >> 15) == 1, $"Alpha is {pixel >> 15}, expected 1 (0.7 rounds to 1)."),
                       () => ImagingTestHelper.Check((pixel & 0x7fff) == 0x001f, $"Color changed to 0x{pixel & 0x7fff:X4}."));
    }

    private static string? CheckSetAlphaB5G5R5A1Range()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.B5G5R5A1_UNorm, 2, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        Fill16(buffer, 0x001f);

        buffer.SetAlpha(1.0f, new GorgonRange<float>(0.5f, 1.0f));

        ushort pixel = ImagingTestHelper.U16(buffer, 1, 1);

        return ImagingTestHelper.Check((pixel >> 15) == 0, $"Alpha 0 (outside 0.5-1) became {pixel >> 15}.");
    }

    private static string? CheckSetAlphaA8()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.A8_UNorm, 2, 2));
        IGorgonImageBuffer buffer = image.Buffers[0];
        Fill8(buffer, 0x10);

        buffer.SetAlpha(1.0f);

        byte alpha = ImagingTestHelper.U8(buffer, 1, 1);

        return ImagingTestHelper.Check(alpha == 255, $"Alpha is {alpha}, expected 255.");
    }
}
