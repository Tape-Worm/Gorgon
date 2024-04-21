using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonNativeExtensionsTests
{
    [TestMethod]
    public void ReadShouldThrowExceptionWhenPtrIsNull()
    {
        using MemoryStream stream = new();
        GorgonPtr<int> ptr = GorgonPtr<int>.NullPtr;

        Assert.ThrowsException<NullReferenceException>(() => stream.Read(ptr));
    }

    [TestMethod]
    public void ReadShouldThrowExceptionWhenStreamIsAtItsEnd()
    {
        using MemoryStream stream = new(10);
        // Set the position to the end of the stream
        stream.Position = stream.Length;

        unsafe
        {
            byte* p = stackalloc byte[10];
            GorgonPtr<byte> ptr = new(p, 10);

            Assert.ThrowsException<EndOfStreamException>(() => stream.Read<byte>(ptr));
        }
    }

    [TestMethod]
    public void ReadShouldReadDataWhenStreamAndPtrAreValid()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new(data);
        unsafe
        {
            byte* p = stackalloc byte[4];
            GorgonPtr<byte> ptr = new(p, 4);

            stream.Read<byte>(ptr);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], ptr[i]);
            }
        }
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenPtrIsNull()
    {
        using MemoryStream stream = new();
        GorgonPtr<int> ptr = GorgonPtr<int>.NullPtr;

        Assert.ThrowsException<NullReferenceException>(() => stream.Write(ptr));
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenStreamIsReadOnly()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new(data, false); // Create a read-only stream
        unsafe
        {
            byte* p = stackalloc byte[4];
            GorgonPtr<byte> ptr = new(p, 4);

            Assert.ThrowsException<IOException>(() => stream.Write<byte>(ptr));
        }
    }

    [TestMethod]
    public void WriteShouldWriteDataWhenStreamAndPtrAreValid()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new();
        unsafe
        {
            fixed (byte* p = data)
            {
                GorgonPtr<byte> ptr = new(p, data.Length);

                stream.Write<byte>(ptr);

                stream.Position = 0;
                byte[] result = new byte[data.Length];
                stream.Read(result, 0, data.Length);

                CollectionAssert.AreEqual(data, result);
            }
        }
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenIndexIsOutOfRange()
    {
        using MemoryStream stream = new();
        GorgonNativeBuffer<byte> buffer = new(10);

        Assert.ThrowsException<ArgumentException>(() => stream.Write(buffer, 11, 1));
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenCountIsOutOfRange()
    {
        using MemoryStream stream = new();
        GorgonNativeBuffer<byte> buffer = new(10);

        Assert.ThrowsException<ArgumentException>(() => stream.Write(buffer, 0, 11));
    }

    [TestMethod]
    public void WriteShouldWriteDataWhenStreamAndBufferAreValid()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new();
        GorgonNativeBuffer<byte> buffer = new(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            buffer[i] = data[i];
        }

        stream.Write<byte>(buffer);

        stream.Position = 0;
        byte[] result = new byte[data.Length];
        stream.Read(result, 0, data.Length);

        CollectionAssert.AreEqual(data, result);
    }

    [TestMethod]
    public void WriteShouldWritePartialDataWhenIndexAndCountAreSpecified()
    {
        byte[] data = [1, 2, 3, 4, 5, 6];
        using MemoryStream stream = new();
        GorgonNativeBuffer<byte> buffer = new(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            buffer[i] = data[i];
        }

        stream.Write(buffer, 1, 2); // Write only the 2nd and 3rd elements from the buffer into the stream

        CollectionAssert.AreEqual(new byte[] { 2, 3 }, stream.ToArray());
    }
    [TestMethod]
    public void ReadIntoBufferShouldThrowExceptionWhenStreamIsAtItsEnd()
    {
        using MemoryStream stream = new(10);
        // Set the position to the end of the stream
        stream.Position = stream.Length;

        GorgonNativeBuffer<byte> buffer = new(10);

        Assert.ThrowsException<EndOfStreamException>(() => stream.Read(buffer, 0));
    }

    [TestMethod]
    public void ReadIntoBufferShouldThrowExceptionWhenIndexIsOutOfRange()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new(data);
        GorgonNativeBuffer<byte> buffer = new(10);

        Assert.ThrowsException<ArgumentException>(() => stream.Read(buffer, 11));
    }

    [TestMethod]
    public void ReadIntoBufferShouldReadDataWhenStreamAndBufferAreValid()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new(data);
        GorgonNativeBuffer<byte> buffer = new(4);

        stream.Read(buffer, 0);

        for (int i = 0; i < data.Length; i++)
        {
            Assert.AreEqual(data[i], buffer[i]);
        }
    }

    [TestMethod]
    public void ReadIntoBufferShouldReadPartialDataWhenIndexIsSpecified()
    {
        byte[] data = [1, 2, 3, 4, 5, 6];
        using MemoryStream stream = new(data);
        GorgonNativeBuffer<byte> buffer = new(4);

        stream.Read(buffer, 2);

        Assert.AreEqual(0, buffer[0]);
        Assert.AreEqual(0, buffer[1]);
        Assert.AreEqual(1, buffer[2]);
        Assert.AreEqual(2, buffer[3]);
    }

    [TestMethod]
    public void ReadIntoBufferShouldThrowExceptionWhenCountIsOutOfRange()
    {
        byte[] data = [1, 2, 3, 4];
        using MemoryStream stream = new(data);
        GorgonNativeBuffer<byte> buffer = new(2);

        Assert.ThrowsException<ArgumentException>(() => stream.Read(buffer, 0, 3));
    }

    [TestMethod]
    public void ReadIntoBufferShouldReadPartialDataWhenIndexAndCountAreSpecified()
    {
        byte[] data = [1, 2, 3, 4, 5, 6];
        using MemoryStream stream = new(data);
        GorgonNativeBuffer<byte> buffer = new(4);

        stream.Read(buffer, 1, 2); // Start writing at the 2nd position in the buffer, read 2 elements from the stream

        Assert.AreEqual(0, buffer[0]);
        Assert.AreEqual(1, buffer[1]);
        Assert.AreEqual(2, buffer[2]);
        Assert.AreEqual(0, buffer[3]);
    }

    [TestMethod]
    public void PinAsNativeBufferShouldPinArraySuccessfullyWhenParametersAreValid()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5];

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(1, 3);

        // Assert
        Assert.AreEqual(3, buffer.Length);
        Assert.AreEqual(2, buffer[0]);
        Assert.AreEqual(3, buffer[1]);
        Assert.AreEqual(4, buffer[2]);
    }

    [TestMethod]
    public void PinAsNativeBufferShouldPinWholeArrayWhenIndexAndCountAreNotSpecified()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5];

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer();

        // Assert
        Assert.AreEqual(data.Length, buffer.Length);
        CollectionAssert.AreEqual(data, buffer.ToArray());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PinAsNativeBufferShouldThrowExceptionWhenIndexIsNegative()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5];

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(-1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PinAsNativeBufferShouldThrowExceptionWhenCountIsNegative()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5];

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(0, -1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PinAsNativeBufferShouldThrowExceptionWhenIndexPlusCountIsGreaterThanArrayLength()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5];

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(1, 5);
    }

    [TestMethod]
    public void PinAsNativeByteBufferShouldPinSuccessfullyWhenValueTypeIsValid()
    {
        // Arrange
        int value = 12345;

        // Act
        using GorgonNativeBuffer<byte> buffer = value.PinAsNativeByteBuffer();

        // Assert
        Assert.AreEqual(sizeof(int), buffer.Length);
        CollectionAssert.AreEqual(BitConverter.GetBytes(value), buffer.ToArray());
    }

    [TestMethod]
    public void PinAsNativeByteBufferShouldPinSuccessfullyWhenValueTypeIsStruct()
    {
        // Arrange
        Point value = new()
        { X = 10, Y = 20 };

        // Act
        using GorgonNativeBuffer<byte> buffer = value.PinAsNativeByteBuffer();

        // Assert
        Assert.AreEqual(Unsafe.SizeOf<Point>(), buffer.Length);
        CollectionAssert.AreEqual(BitConverter.GetBytes(value.X).Concat(BitConverter.GetBytes(value.Y)).ToArray(), buffer.ToArray());
    }

    private static readonly int[] _array = [1, 2, 3, 4, 5];

    [TestMethod]
    public unsafe void CopyToShouldCopySuccessfullyWhenSpanIsValid()
    {
        // Arrange
        Span<int> span = new(_array);
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new(ptr, span.Length);

        // Act
        span.CopyTo(gorgonPtr, span.Length);

        // Assert
        for (int i = 0; i < span.Length; i++)
        {
            Assert.AreEqual(span[i], gorgonPtr[i]);
        }
    }

    [TestMethod]
    public unsafe void CopyToShouldCopySuccessfullyWhenReadOnlySpanIsValid()
    {
        // Arrange
        ReadOnlySpan<int> span = new(_array);
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new(ptr, span.Length);

        // Act
        span.CopyTo(gorgonPtr, span.Length);

        // Assert
        for (int i = 0; i < span.Length; i++)
        {
            Assert.AreEqual(span[i], gorgonPtr[i]);
        }
    }

    [TestMethod]
    public unsafe void CopyToShouldThrowExceptionWhenCountIsGreaterThanSpanLength()
    {
        // Arrange
        Span<int> span = new(_array);
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new(ptr, span.Length);

        // Act
        try
        {
            span.CopyTo(gorgonPtr, span.Length + 1);
            Assert.Fail("No exception thrown");
        }
        catch (ArgumentException)
        {

        }
    }

    [TestMethod]
    public unsafe void CopyToShouldThrowExceptionWhenCountIsGreaterThanReadOnlySpanLength()
    {
        // Arrange
        ReadOnlySpan<int> span = new(_array);
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new(ptr, span.Length);

        // Act
        try
        {
            span.CopyTo(gorgonPtr, span.Length + 1);
            Assert.Fail("No exception thrown");
        }
        catch (ArgumentException)
        {

        }
    }
}
