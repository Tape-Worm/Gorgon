// GorgonImageBuffer.CopyTo with block compressed formats: copies whole 4x4 blocks, flooring the left/top edges and rounding up the right/bottom edges.

using System;

namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonImageBufferCompressedTests
{
    [TestMethod]
    [Description("Does CopyTo copy a whole BC1 buffer into one of the same size?")]
    public void CopyToCompressedSameSize() => ImagingTestHelper.AssertPass(CheckCopyToCompressedSameSize());

    [TestMethod]
    [Description("Does CopyTo of BC1 region (4, 4, 8, 8) to (8, 0) move source blocks (1-2, 1-2) to destination blocks (2-3, 0-1) and leave the rest alone?")]
    public void CopyToCompressedAlignedRegion() => ImagingTestHelper.AssertPass(CheckCopyToCompressedAlignedRegion(BufferFormat.BC1_UNorm));

    [TestMethod]
    [Description("Same as CopyToCompressedAlignedRegion, with BC7 (16 byte blocks).")]
    public void CopyToCompressedAlignedRegionBc7() => ImagingTestHelper.AssertPass(CheckCopyToCompressedAlignedRegion(BufferFormat.BC7_UNorm));

    [TestMethod]
    [Description("Does CopyTo of BC1 region (2, 2, 4, 4) to (5, 5) snap outward to source blocks (0-1, 0-1) and land them at destination block (1, 1)?")]
    public void CopyToCompressedUnalignedSnaps() => ImagingTestHelper.AssertPass(CheckCopyToCompressedUnalignedSnaps());

    [TestMethod]
    [Description("Does CopyTo of a 6x6 BC1 buffer into an 8x8 one copy all 2x2 blocks, including the partial edge blocks?")]
    public void CopyToCompressedPartialEdgeBlocks() => ImagingTestHelper.AssertPass(CheckCopyToCompressedPartialEdgeBlocks());

    [TestMethod]
    [Description("Does CopyTo of BC1 region (2, 0, 4, 4), which snaps to 2 blocks, into a 4x4 (1 block) destination copy only the block that fits?")]
    public void CopyToCompressedClampsToDestination() => ImagingTestHelper.AssertPass(CheckCopyToCompressedClampsToDestination());

    [TestMethod]
    [Description("Does GetRegion of BC1 region (4, 4, 8, 8) return an 8x8 buffer holding source blocks (1-2, 1-2)?")]
    public void GetRegionCompressedAligned() => ImagingTestHelper.AssertPass(CheckGetRegionCompressed(16, 16, new GorgonRectangle(4, 4, 8, 8), 8, 8, 1, 1, 2, 2));

    [TestMethod]
    [Description("Does GetRegion of BC1 region (2, 2, 4, 4) snap outward to an 8x8 buffer holding source blocks (0-1, 0-1)?")]
    public void GetRegionCompressedSnaps() => ImagingTestHelper.AssertPass(CheckGetRegionCompressed(16, 16, new GorgonRectangle(2, 2, 4, 4), 8, 8, 0, 0, 2, 2));

    [TestMethod]
    [Description("Does GetRegion of region (4, 4, 2, 2) in a 6x6 BC1 buffer return a 2x2 buffer (not 4x4) holding the partial edge block (1, 1)?")]
    public void GetRegionCompressedPartialEdgeBlock() => ImagingTestHelper.AssertPass(CheckGetRegionCompressed(6, 6, new GorgonRectangle(4, 4, 2, 2), 2, 2, 1, 1, 1, 1));

    // Fills every byte of the buffer with a pattern that differs per byte and per seed.
    private static void Fill(IGorgonImageBuffer buffer, int seed)
    {
        Span<byte> data = buffer.ImageData.ToSpan();

        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = (byte)((i * 7) + seed);
        }
    }

    // Copies the expected blocks into a snapshot of the destination, the way CopyTo should.
    private static void CopyBlocks(IGorgonImageBuffer source, int srcBlockX, int srcBlockY, byte[] expected, IGorgonImageBuffer destination, int dstBlockX, int dstBlockY, int blocksWide, int blocksHigh)
    {
        int blockSize = source.FormatInformation.SizeInBytes;
        ReadOnlySpan<byte> src = source.ImageData.ToSpan();

        for (int y = 0; y < blocksHigh; ++y)
        {
            int srcOffset = ((srcBlockY + y) * source.PitchInformation.RowPitch) + (srcBlockX * blockSize);
            int dstOffset = ((dstBlockY + y) * destination.PitchInformation.RowPitch) + (dstBlockX * blockSize);

            src.Slice(srcOffset, blocksWide * blockSize).CopyTo(expected.AsSpan(dstOffset));
        }
    }

    private static string? Compare(byte[] expected, IGorgonImageBuffer actual)
    {
        ReadOnlySpan<byte> data = actual.ImageData.ToSpan();

        if (data.Length != expected.Length)
        {
            return $"Destination is {data.Length} bytes, expected {expected.Length}.";
        }

        int blockSize = actual.FormatInformation.SizeInBytes;

        for (int i = 0; i < expected.Length; ++i)
        {
            if (data[i] != expected[i])
            {
                int row = i / actual.PitchInformation.RowPitch;
                int column = (i % actual.PitchInformation.RowPitch) / blockSize;
                return $"Byte {i} (block {column}, {row}) is {data[i]}, expected {expected[i]}.";
            }
        }

        return null;
    }

    private static string? CheckCopyToCompressedSameSize()
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 16, 16));
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 16, 16));
        Fill(source.Buffers[0], 1);
        Fill(destination.Buffers[0], 100);

        source.Buffers[0].CopyTo(destination.Buffers[0]);

        return Compare(source.Buffers[0].ImageData.ToSpan().ToArray(), destination.Buffers[0]);
    }

    private static string? CheckCopyToCompressedAlignedRegion(BufferFormat format)
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(format, 16, 16));
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(format, 16, 16));
        Fill(source.Buffers[0], 1);
        Fill(destination.Buffers[0], 100);
        byte[] expected = destination.Buffers[0].ImageData.ToSpan().ToArray();
        CopyBlocks(source.Buffers[0], 1, 1, expected, destination.Buffers[0], 2, 0, 2, 2);

        source.Buffers[0].CopyTo(destination.Buffers[0], new GorgonRectangle(4, 4, 8, 8), new GorgonPoint(8, 0));

        return Compare(expected, destination.Buffers[0]);
    }

    private static string? CheckCopyToCompressedUnalignedSnaps()
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 16, 16));
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 16, 16));
        Fill(source.Buffers[0], 1);
        Fill(destination.Buffers[0], 100);
        byte[] expected = destination.Buffers[0].ImageData.ToSpan().ToArray();
        CopyBlocks(source.Buffers[0], 0, 0, expected, destination.Buffers[0], 1, 1, 2, 2);

        source.Buffers[0].CopyTo(destination.Buffers[0], new GorgonRectangle(2, 2, 4, 4), new GorgonPoint(5, 5));

        return Compare(expected, destination.Buffers[0]);
    }

    private static string? CheckCopyToCompressedPartialEdgeBlocks()
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 6, 6));
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 8, 8));
        Fill(source.Buffers[0], 1);
        Fill(destination.Buffers[0], 100);
        byte[] expected = destination.Buffers[0].ImageData.ToSpan().ToArray();
        CopyBlocks(source.Buffers[0], 0, 0, expected, destination.Buffers[0], 0, 0, 2, 2);

        source.Buffers[0].CopyTo(destination.Buffers[0]);

        return Compare(expected, destination.Buffers[0]);
    }

    private static string? CheckCopyToCompressedClampsToDestination()
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 8, 4));
        using GorgonImage destination = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 4, 4));
        Fill(source.Buffers[0], 1);
        Fill(destination.Buffers[0], 100);
        byte[] expected = destination.Buffers[0].ImageData.ToSpan().ToArray();
        CopyBlocks(source.Buffers[0], 0, 0, expected, destination.Buffers[0], 0, 0, 1, 1);

        source.Buffers[0].CopyTo(destination.Buffers[0], new GorgonRectangle(2, 0, 4, 4));

        return Compare(expected, destination.Buffers[0]);
    }

    private static string? CheckGetRegionCompressed(int width, int height, GorgonRectangle clipRegion, int expectedWidth, int expectedHeight, int srcBlockX, int srcBlockY, int blocksWide, int blocksHigh)
    {
        using GorgonImage source = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, width, height));
        Fill(source.Buffers[0], 1);

        using IGorgonImageBuffer region = source.Buffers[0].GetRegion(clipRegion);

        if ((region.Width != expectedWidth) || (region.Height != expectedHeight))
        {
            return $"Region is {region.Width}x{region.Height}, expected {expectedWidth}x{expectedHeight}.";
        }

        byte[] expected = new byte[region.SizeInBytes];
        CopyBlocks(source.Buffers[0], srcBlockX, srcBlockY, expected, region, 0, 0, blocksWide, blocksHigh);

        return Compare(expected, region);
    }
}
