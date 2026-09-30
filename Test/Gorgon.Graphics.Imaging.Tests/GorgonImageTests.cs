// GorgonImage: buffer layout, copies and the image buffer list.
// Moved from the Imaging integration battery (Test/Experimental/ImagingFunctions): memory only, no WIC, no codec IO.

using System;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonImageTests
{
    [TestMethod]
    [Description("Does Fill on a buffer taken from an image throw ObjectDisposedException after the image is disposed?")]
    public void BufferAfterImageDisposeThrows() => ImagingTestHelper.AssertPass(CheckBufferAfterImageDisposeThrows());

    [TestMethod]
    [Description("Does a 2D image with arrays and mips have the right size, buffer count, buffer sizes and array-major offsets?")]
    public void Layout2D() => ImagingTestHelper.AssertPass(CheckLayout2D());

    [TestMethod]
    [Description("Does a 3D image with mips halve the depth per mip and lay the slices out mip-major?")]
    public void Layout3D() => ImagingTestHelper.AssertPass(CheckLayout3D());

    [TestMethod]
    [Description("Does a 1D image with arrays and mips have a height of 1 and the right size?")]
    public void Layout1D() => ImagingTestHelper.AssertPass(CheckLayout1D());

    [TestMethod]
    [Description("Does a cube image have 6 array indices per cube?")]
    public void LayoutCube() => ImagingTestHelper.AssertPass(CheckLayoutCube());

    [TestMethod]
    [Description("Is a mip count larger than the maximum clamped on creation?")]
    public void MipCountClamped() => ImagingTestHelper.AssertPass(CheckMipCountClamped());

    [TestMethod]
    [Description("Does the span constructor copy (not wrap) the data, and throw on a size mismatch?")]
    public void FromSpanCopiesData() => ImagingTestHelper.AssertPass(CheckFromSpanCopiesData());

    [TestMethod]
    [Description("Does Copy produce an identical image that doesn't share memory with the original?")]
    public void CopyIsDeep() => ImagingTestHelper.AssertPass(CheckCopyIsDeep());

    [TestMethod]
    [Description("Does GetDepthCount throw ArgumentOutOfRangeException for mipLevel == MipCount (the docs say 'exceeds, or equals')?")]
    public void GetDepthCountOutOfRange() => ImagingTestHelper.AssertPass(CheckGetDepthCountOutOfRange());

    [TestMethod]
    [Description("Does the Buffers[mip, array] indexer throw for an out of range mip or array index?")]
    public void BuffersIndexerBounds() => ImagingTestHelper.AssertPass(CheckBuffersIndexerBounds());

    [TestMethod]
    [Description("For a 3D image (depth 4, 3 mips) do Buffers[1, 2] and Buffers[2, 1] throw (mip 1 has 2 slices, mip 2 has 1)?")]
    public void Buffers3DDepthPerMip() => ImagingTestHelper.AssertPass(CheckBuffers3DDepthPerMip());

    [TestMethod]
    [Description("For a 3D image (depth 4, 3 mips) does Buffers.IndexOf(mip, slice) with a slice past the mip's depth return -1 or a buffer of that mip?")]
    public void IndexOf3DStaysInMip() => ImagingTestHelper.AssertPass(CheckIndexOf3DStaysInMip());

    [TestMethod]
    [Description("Does Buffers.Contains return false for a mip level or slice/array index the image doesn't have?")]
    public void ContainsOutOfRange() => ImagingTestHelper.AssertPass(CheckContainsOutOfRange());

    [TestMethod]
    [Description("Does BeginUpdate throw NotSupportedException for a block compressed image?")]
    public void BeginUpdateCompressedThrows() => ImagingTestHelper.AssertPass(CheckBeginUpdateCompressedThrows());

    private static string? CheckLayout2D()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 64, 32, 2, 3));
        // Per array index: 64x32 + 32x16 + 16x8 pixels, 4 bytes each.
        const long arraySize = ((64 * 32) + (32 * 16) + (16 * 8)) * 4;

        IGorgonImageBuffer mip1Array1 = image.Buffers[1, 1];

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image.SizeInBytes == arraySize * 2, $"SizeInBytes is {image.SizeInBytes}, expected {arraySize * 2}."),
                       () => ImagingTestHelper.Check(image.Buffers.Count == 6, $"Buffer count is {image.Buffers.Count}, expected 6."),
                       () => ImagingTestHelper.Check((mip1Array1.Width == 32) && (mip1Array1.Height == 16), $"Mip 1 is {mip1Array1.Width}x{mip1Array1.Height}, expected 32x16."),
                       () => ImagingTestHelper.Check((mip1Array1.MipLevel == 1) && (mip1Array1.ArrayIndex == 1), $"Buffers[1, 1] reports mip {mip1Array1.MipLevel}, array {mip1Array1.ArrayIndex}."),
                       () => ImagingTestHelper.Check((image.Buffers[0, 1].ImageData - image.ImageData) == arraySize, $"Array 1 starts at byte {image.Buffers[0, 1].ImageData - image.ImageData}, expected {arraySize}."),
                       () => ImagingTestHelper.Check((mip1Array1.ImageData - image.ImageData) == arraySize + (64 * 32 * 4), $"Mip 1 of array 1 starts at byte {mip1Array1.ImageData - image.ImageData}, expected {arraySize + (64 * 32 * 4)}."));
    }

    private static string? CheckLayout3D()
    {
        using GorgonImage image = new(GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 8, 3));
        // Mip 0: 8 slices of 16x16, mip 1: 4 slices of 8x8, mip 2: 2 slices of 4x4.
        const long expectedSize = ((8 * 16 * 16) + (4 * 8 * 8) + (2 * 4 * 4)) * 4;
        const long mip1Slice3Offset = (8 * 16 * 16 * 4) + (3 * 8 * 8 * 4);

        IGorgonImageBuffer mip1Slice3 = image.Buffers[1, 3];

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image.SizeInBytes == expectedSize, $"SizeInBytes is {image.SizeInBytes}, expected {expectedSize}."),
                       () => ImagingTestHelper.Check((image.GetDepthCount(0) == 8) && (image.GetDepthCount(1) == 4) && (image.GetDepthCount(2) == 2),
                                     $"Depth counts are {image.GetDepthCount(0)}, {image.GetDepthCount(1)}, {image.GetDepthCount(2)}; expected 8, 4, 2."),
                       () => ImagingTestHelper.Check(image.Buffers.Count == 14, $"Buffer count is {image.Buffers.Count}, expected 14."),
                       () => ImagingTestHelper.Check((mip1Slice3.Width == 8) && (mip1Slice3.DepthSliceIndex == 3), $"Buffers[1, 3] is {mip1Slice3.Width} wide, slice {mip1Slice3.DepthSliceIndex}."),
                       () => ImagingTestHelper.Check((mip1Slice3.ImageData - image.ImageData) == mip1Slice3Offset, $"Mip 1 slice 3 starts at byte {mip1Slice3.ImageData - image.ImageData}, expected {mip1Slice3Offset}."));
    }

    private static string? CheckLayout1D()
    {
        using GorgonImage image = new(GorgonImageInfo.Create1DImageInfo(BufferFormat.R8G8B8A8_UNorm, 64, 2, 2));
        const long expectedSize = (64 + 32) * 4 * 2;

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image.Height == 1, $"Height is {image.Height}, expected 1."),
                       () => ImagingTestHelper.Check(image.SizeInBytes == expectedSize, $"SizeInBytes is {image.SizeInBytes}, expected {expectedSize}."),
                       () => ImagingTestHelper.Check(image.Buffers[1, 1].Width == 32, $"Mip 1 width is {image.Buffers[1, 1].Width}, expected 32."));
    }

    private static string? CheckLayoutCube()
    {
        using GorgonImage image = new(GorgonImageInfo.CreateCubeImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 2, 2));

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image.ImageType == ImageDataType.ImageCube, $"ImageType is {image.ImageType}."),
                       () => ImagingTestHelper.Check(image.ArrayCount == 12, $"ArrayCount is {image.ArrayCount}, expected 12."),
                       () => ImagingTestHelper.Check(image.Buffers.Count == 24, $"Buffer count is {image.Buffers.Count}, expected 24."));
    }

    private static string? CheckMipCountClamped()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 1, 10));

        return ImagingTestHelper.Check(image.MipCount == 5, $"MipCount is {image.MipCount}, expected 5 (16, 8, 4, 2, 1).");
    }

    private static string? CheckFromSpanCopiesData()
    {
        GorgonImageInfo info = GorgonImageInfo.Create2DImageInfo(BufferFormat.R8_UNorm, 8, 8);
        byte[] data = new byte[64];

        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = (byte)i;
        }

        using GorgonImage image = new(info, data.AsSpan());

        // Changing the source afterwards must not change the image.
        data[10] = 0xff;

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image.ImageData[10] == 10, $"Image byte 10 is {image.ImageData[10]}, expected 10 (the data was wrapped, not copied)."),
                       () => ImagingTestHelper.Check(image.ImageData[63] == 63, $"Image byte 63 is {image.ImageData[63]}, expected 63."),
                       () => ImagingTestHelper.Throws<ArgumentException>(() => new GorgonImage(info, new byte[63].AsSpan()).Dispose(), "63 byte span"));
    }

    private static string? CheckCopyIsDeep()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 2, 2));
        ImagingTestHelper.FillBytes(image);

        using IGorgonImage copy = image.Copy();
        string? compare = ImagingTestHelper.CompareImages(image, copy);

        if (compare is not null)
        {
            return compare;
        }

        copy.Buffers[0, 1].ImageData[0] ^= 0xff;

        return ImagingTestHelper.Check(image.Buffers[0, 1].ImageData[0] != copy.Buffers[0, 1].ImageData[0], "Changing the copy changed the original.");
    }

    private static string? CheckGetDepthCountOutOfRange()
    {
        using GorgonImage image2D = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 1, 3));
        using GorgonImage image3D = new(GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 4, 3));

        return ImagingTestHelper.First(() => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => image2D.GetDepthCount(3), "2D GetDepthCount(MipCount)"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => image3D.GetDepthCount(3), "3D GetDepthCount(MipCount)"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => image2D.GetDepthCount(-1), "GetDepthCount(-1)"));
    }

    private static string? CheckBuffersIndexerBounds()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 2, 3));

        return ImagingTestHelper.First(() => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[3, 0], "Buffers[3, 0]"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[0, 2], "Buffers[0, 2]"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[-1, 0], "Buffers[-1, 0]"));
    }

    private static string? CheckBuffers3DDepthPerMip()
    {
        using GorgonImage image = new(GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 4, 3));

        return ImagingTestHelper.First(() => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[1, 2], "Buffers[1, 2]"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[2, 1], "Buffers[2, 1]"),
                       () => ImagingTestHelper.Throws<ArgumentOutOfRangeException>(() => _ = image.Buffers[2, 3], "Buffers[2, 3]"));
    }

    private static string? CheckIndexOf3DStaysInMip()
    {
        // Buffers: mip 0 = 0-3, mip 1 = 4-5, mip 2 = 6.
        using GorgonImage image = new(GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 4, 3));
        int mip1 = image.Buffers.IndexOf(1, 3);
        int mip2 = image.Buffers.IndexOf(2, 2);

        return ImagingTestHelper.First(() => ImagingTestHelper.Check((mip1 == -1) || (mip1 is >= 4 and <= 5), $"IndexOf(1, 3) is {mip1}, expected -1 or 4-5."),
                       () => ImagingTestHelper.Check((mip2 == -1) || (mip2 == 6), $"IndexOf(2, 2) is {mip2}, expected -1 or 6."));
    }

    private static string? CheckContainsOutOfRange()
    {
        using GorgonImage image2D = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 2, 3));
        using GorgonImage image3D = new(GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 16, 16, 4, 3));

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(image2D.Buffers.Contains(2, 1), "2D Contains(2, 1) is false, expected true (control)."),
                       () => ImagingTestHelper.Check(!image2D.Buffers.Contains(3, 0), "2D Contains(3, 0) is true, but the image has 3 mips."),
                       () => ImagingTestHelper.Check(!image2D.Buffers.Contains(0, 2), "2D Contains(0, 2) is true, but the image has 2 array indices."),
                       () => ImagingTestHelper.Check(!image3D.Buffers.Contains(1, 2), "3D Contains(1, 2) is true, but mip 1 has 2 slices."));
    }

    private static string? CheckBeginUpdateCompressedThrows()
    {
        using GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.BC1_UNorm, 16, 16));

        return ImagingTestHelper.Throws<NotSupportedException>(() => image.BeginUpdate(), "BeginUpdate on BC1");
    }

    private static string? CheckBufferAfterImageDisposeThrows()
    {
        GorgonImage image = new(GorgonImageInfo.Create2DImageInfo(BufferFormat.R8G8B8A8_UNorm, 64, 64));
        IGorgonImageBuffer buffer = image.Buffers[0];

        image.Dispose();

        return ImagingTestHelper.First(() => ImagingTestHelper.Check(buffer.ImageData == GorgonPtr<byte>.NullPtr, "The buffer still points at the image memory after the image was disposed."),
                       () => ImagingTestHelper.Throws<ObjectDisposedException>(() => buffer.Fill(0xff), "Fill after the image was disposed"));
    }
}
