// Shared helpers for the image tests. A check returns null when the behaviour is correct, or a message describing what was wrong.

using System;
using System.Collections.Generic;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.Tests;

internal static class ImagingTestHelper
{
    // Fails the test with the message returned by a check (a check returns null when the behaviour is correct).
    public static void AssertPass(string? failure) => Assert.IsNull(failure, failure);

    // Returns a message when the condition is false.
    public static string? Check(bool condition, string message) => condition ? null : message;

    // Returns the first non-null message.
    public static string? First(params Func<string?>[] checks)
    {
        foreach (Func<string?> check in checks)
        {
            string? result = check();

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    // Expects an exception of type TException. Returns a message if nothing (or the wrong thing) was thrown.
    public static string? Throws<TException>(Action action, string label)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return null;
        }
        catch (Exception ex)
        {
            return $"{label}: expected {typeof(TException).Name}, got {ex.GetType().Name}: {ex.Message}";
        }

        return $"{label}: expected {typeof(TException).Name}, nothing was thrown.";
    }

    public static GorgonPtr<byte> Row(IGorgonImageBuffer buffer, int y) => buffer.ImageData + (y * buffer.PitchInformation.RowPitch);

    public static byte U8(IGorgonImageBuffer buffer, int x, int y) => Row(buffer, y)[x];
    public static ushort U16(IGorgonImageBuffer buffer, int x, int y) => ((GorgonPtr<ushort>)Row(buffer, y))[x];
    public static uint U32(IGorgonImageBuffer buffer, int x, int y) => ((GorgonPtr<uint>)Row(buffer, y))[x];

    public static void SetU8(IGorgonImageBuffer buffer, int x, int y, byte value) => Row(buffer, y)[x] = value;
    public static void SetU16(IGorgonImageBuffer buffer, int x, int y, ushort value) => ((GorgonPtr<ushort>)Row(buffer, y))[x] = value;
    public static void SetU32(IGorgonImageBuffer buffer, int x, int y, uint value) => ((GorgonPtr<uint>)Row(buffer, y))[x] = value;

    // Channel access for 4 channel formats with 16 bit or 32 bit channels.
    public static ushort U16Channel(IGorgonImageBuffer buffer, int x, int y, int channel) => ((GorgonPtr<ushort>)Row(buffer, y))[x * 4 + channel];
    public static void SetU16Channel(IGorgonImageBuffer buffer, int x, int y, int channel, ushort value) => ((GorgonPtr<ushort>)Row(buffer, y))[x * 4 + channel] = value;
    public static Half F16Channel(IGorgonImageBuffer buffer, int x, int y, int channel) => ((GorgonPtr<Half>)Row(buffer, y))[x * 4 + channel];
    public static void SetF16Channel(IGorgonImageBuffer buffer, int x, int y, int channel, Half value) => ((GorgonPtr<Half>)Row(buffer, y))[x * 4 + channel] = value;
    public static float F32Channel(IGorgonImageBuffer buffer, int x, int y, int channel) => ((GorgonPtr<float>)Row(buffer, y))[x * 4 + channel];
    public static void SetF32Channel(IGorgonImageBuffer buffer, int x, int y, int channel, float value) => ((GorgonPtr<float>)Row(buffer, y))[x * 4 + channel] = value;

    // Packs an RGBA color into the memory layout of an R8G8B8A8 pixel (R in the lowest byte).
    public static uint Rgba(byte r, byte g, byte b, byte a) => (uint)(r | (g << 8) | (b << 16) | (a << 24));

    // Packs a color into the memory layout of a B8G8R8A8 pixel (B in the lowest byte).
    public static uint Bgra(byte r, byte g, byte b, byte a) => (uint)(b | (g << 8) | (r << 16) | (a << 24));

    // Reads a pixel from an 8 bit per channel RGBA, BGRA or BGRX buffer as (R, G, B, A).
    public static (byte R, byte G, byte B, byte A) ReadColor(IGorgonImageBuffer buffer, int x, int y)
    {
        uint value = U32(buffer, x, y);
        byte c0 = (byte)(value & 0xff);
        byte c1 = (byte)((value >> 8) & 0xff);
        byte c2 = (byte)((value >> 16) & 0xff);
        byte c3 = (byte)(value >> 24);

        return buffer.Format switch
        {
            BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb => (c0, c1, c2, c3),
            BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8A8_UNorm_SRgb => (c2, c1, c0, c3),
            BufferFormat.B8G8R8X8_UNorm or BufferFormat.B8G8R8X8_UNorm_SRgb => (c2, c1, c0, 255),
            _ => throw new NotSupportedException($"ReadColor does not support {buffer.Format}.")
        };
    }

    // Fills every byte of the buffer with a position dependent pattern. Works for any format.
    public static void FillBytes(IGorgonImageBuffer buffer, int seed)
    {
        GorgonPtr<byte> data = buffer.ImageData;

        for (long i = 0; i < buffer.SizeInBytes; ++i)
        {
            data[i] = (byte)(((i * 7) + (seed * 31) + ((i >> 8) * 13)) & 0xff);
        }
    }

    // Fills every buffer in the image with a byte pattern (each buffer gets its own seed).
    public static void FillBytes(IGorgonImage image)
    {
        int seed = 1;

        foreach (IGorgonImageBuffer buffer in image.Buffers)
        {
            FillBytes(buffer, seed++);
        }
    }

    // Fills an R8G8B8A8 buffer so each pixel encodes its own coordinates: R = x, G = y, B = seed, A = 255.
    public static void FillCoords(IGorgonImageBuffer buffer, byte seed)
    {
        for (int y = 0; y < buffer.Height; ++y)
        {
            for (int x = 0; x < buffer.Width; ++x)
            {
                SetU32(buffer, x, y, Rgba((byte)x, (byte)y, seed, 255));
            }
        }
    }

    public static void FillSolid(IGorgonImageBuffer buffer, uint value)
    {
        for (int y = 0; y < buffer.Height; ++y)
        {
            for (int x = 0; x < buffer.Width; ++x)
            {
                SetU32(buffer, x, y, value);
            }
        }
    }

    // Checks that every pixel of a 32 bit buffer equals the given value.
    public static string? CheckSolid(IGorgonImageBuffer buffer, uint expected, string label)
    {
        for (int y = 0; y < buffer.Height; ++y)
        {
            for (int x = 0; x < buffer.Width; ++x)
            {
                uint actual = U32(buffer, x, y);

                if (actual != expected)
                {
                    return $"{label}: pixel ({x}, {y}) is 0x{actual:X8}, expected 0x{expected:X8}.";
                }
            }
        }

        return null;
    }

    // Compares a w x h pixel region of two buffers with the same format, byte for byte.
    public static string? CompareRegion(IGorgonImageBuffer expected, int ex, int ey, IGorgonImageBuffer actual, int ax, int ay, int width, int height, string label)
    {
        int pixelSize = expected.FormatInformation.SizeInBytes;

        for (int y = 0; y < height; ++y)
        {
            GorgonPtr<byte> expectedRow = Row(expected, ey + y) + (ex * pixelSize);
            GorgonPtr<byte> actualRow = Row(actual, ay + y) + (ax * pixelSize);

            for (int i = 0; i < width * pixelSize; ++i)
            {
                if (expectedRow[i] != actualRow[i])
                {
                    return $"{label}: mismatch at pixel ({(i / pixelSize) + ax}, {y + ay}) byte {i % pixelSize}: expected 0x{expectedRow[i]:X2}, got 0x{actualRow[i]:X2}.";
                }
            }
        }

        return null;
    }

    // Compares two buffers of the same format and size.
    public static string? CompareBuffers(IGorgonImageBuffer expected, IGorgonImageBuffer actual, string label)
    {
        if ((expected.Width != actual.Width) || (expected.Height != actual.Height))
        {
            return $"{label}: size {actual.Width}x{actual.Height}, expected {expected.Width}x{expected.Height}.";
        }

        if (expected.FormatInformation.IsCompressed)
        {
            GorgonPtr<byte> e = expected.ImageData;
            GorgonPtr<byte> a = actual.ImageData;

            for (long i = 0; i < expected.SizeInBytes; ++i)
            {
                if (e[i] != a[i])
                {
                    return $"{label}: mismatch at byte {i}: expected 0x{e[i]:X2}, got 0x{a[i]:X2}.";
                }
            }

            return null;
        }

        return CompareRegion(expected, 0, 0, actual, 0, 0, expected.Width, expected.Height, label);
    }

    // Compares every buffer of two images with the same layout.
    public static string? CompareImages(IGorgonImage expected, IGorgonImage actual)
    {
        if ((expected.ImageType != actual.ImageType) || (expected.Format != actual.Format) || (expected.Width != actual.Width) || (expected.Height != actual.Height)
            || (expected.Depth != actual.Depth) || (expected.ArrayCount != actual.ArrayCount) || (expected.MipCount != actual.MipCount))
        {
            return $"Image info differs. Expected {Describe(expected)}, got {Describe(actual)}.";
        }

        if (expected.Buffers.Count != actual.Buffers.Count)
        {
            return $"Buffer count {actual.Buffers.Count}, expected {expected.Buffers.Count}.";
        }

        // Buffers[i] would pick the [mipLevel, arrayIndex = 0] indexer, so go through the list interface for a flat index.
        IReadOnlyList<IGorgonImageBuffer> expectedBuffers = expected.Buffers;
        IReadOnlyList<IGorgonImageBuffer> actualBuffers = actual.Buffers;

        for (int i = 0; i < expectedBuffers.Count; ++i)
        {
            IGorgonImageBuffer e = expectedBuffers[i];
            string? result = CompareBuffers(e, actualBuffers[i], $"buffer {i} (mip {e.MipLevel}, array {e.ArrayIndex}, slice {e.DepthSliceIndex})");

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    public static string Describe(IGorgonImageInfo info) => $"{info.ImageType} {info.Format} {info.Width}x{info.Height}x{info.Depth} array {info.ArrayCount} mips {info.MipCount}";

    public static string Describe(GorgonImageInfo info) => $"{info.ImageType} {info.Format} {info.Width}x{info.Height}x{info.Depth} array {info.ArrayCount} mips {info.MipCount}";

    // Peak signal to noise ratio over the RGB channels of two 8 bit per channel buffers.
    public static double Psnr(IGorgonImageBuffer expected, IGorgonImageBuffer actual)
    {
        double sum = 0;
        long count = 0;

        for (int y = 0; y < expected.Height; ++y)
        {
            for (int x = 0; x < expected.Width; ++x)
            {
                (byte R, byte G, byte B, byte _) = ReadColor(expected, x, y);
                (byte R, byte G, byte B, byte A) a = ReadColor(actual, x, y);

                sum += ((R - a.R) * (R - a.R)) + ((G - a.G) * (G - a.G)) + ((B - a.B) * (B - a.B));
                count += 3;
            }
        }

        double mse = sum / count;

        return mse == 0 ? double.PositiveInfinity : 10.0 * System.Math.Log10(255.0 * 255.0 / mse);
    }

    public static bool Near(int actual, int expected, int tolerance) => System.Math.Abs(actual - expected) <= tolerance;
}
