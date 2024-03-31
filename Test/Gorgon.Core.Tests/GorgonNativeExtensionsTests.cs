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
    public void Read_ShouldThrowException_WhenPtrIsNull()
    {
        using (var stream = new MemoryStream())
        {
            GorgonPtr<int> ptr = GorgonPtr<int>.NullPtr;

            Assert.ThrowsException<NullReferenceException>(() => stream.Read(ptr));
        }
    }

    [TestMethod]
    public void Read_ShouldThrowException_WhenStreamIsAtItsEnd()
    {
        using (var stream = new MemoryStream(10))
        {
            // Set the position to the end of the stream
            stream.Position = stream.Length;

            unsafe
            {
                byte* p = stackalloc byte[10];
                var ptr = new GorgonPtr<byte>(p, 10);

                Assert.ThrowsException<EndOfStreamException>(() => stream.Read<byte>(ptr));
            }
        }
    }

    [TestMethod]
    public void Read_ShouldReadData_WhenStreamAndPtrAreValid()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream(data))
        {
            unsafe
            {
                byte* p = stackalloc byte[4];
                var ptr = new GorgonPtr<byte>(p, 4);

                stream.Read<byte>(ptr);

                for (int i = 0; i < data.Length; i++)
                {
                    Assert.AreEqual(data[i], ptr[i]);
                }
            }
        }
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenPtrIsNull()
    {
        using (var stream = new MemoryStream())
        {
            GorgonPtr<int> ptr = GorgonPtr<int>.NullPtr;

            Assert.ThrowsException<NullReferenceException>(() => stream.Write<int>(ptr));
        }
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenStreamIsReadOnly()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream(data, false)) // Create a read-only stream
        {
            unsafe
            {
                byte* p = stackalloc byte[4];
                var ptr = new GorgonPtr<byte>(p, 4);

                Assert.ThrowsException<IOException>(() => stream.Write<byte>(ptr));
            }
        }
    }

    [TestMethod]
    public void Write_ShouldWriteData_WhenStreamAndPtrAreValid()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream())
        {
            unsafe
            {
                fixed (byte* p = data)
                {
                    var ptr = new GorgonPtr<byte>(p, data.Length);

                    stream.Write<byte>(ptr);

                    stream.Position = 0;
                    var result = new byte[data.Length];
                    stream.Read(result, 0, data.Length);

                    CollectionAssert.AreEqual(data, result);
                }
            }
        }
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenIndexIsOutOfRange()
    {
        using (var stream = new MemoryStream())
        {
            var buffer = new GorgonNativeBuffer<byte>(10);

            Assert.ThrowsException<ArgumentException>(() => stream.Write<byte>(buffer, 11, 1));
        }
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenCountIsOutOfRange()
    {
        using (var stream = new MemoryStream())
        {
            var buffer = new GorgonNativeBuffer<byte>(10);

            Assert.ThrowsException<ArgumentException>(() => stream.Write<byte>(buffer, 0, 11));
        }
    }

    [TestMethod]
    public void Write_ShouldWriteData_WhenStreamAndBufferAreValid()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream())
        {
            var buffer = new GorgonNativeBuffer<byte>(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = data[i];
            }

            stream.Write<byte>(buffer);

            stream.Position = 0;
            var result = new byte[data.Length];
            stream.Read(result, 0, data.Length);

            CollectionAssert.AreEqual(data, result);
        }
    }

    [TestMethod]
    public void Write_ShouldWritePartialData_WhenIndexAndCountAreSpecified()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6 };
        using (var stream = new MemoryStream())
        {
            var buffer = new GorgonNativeBuffer<byte>(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = data[i];
            }

            stream.Write<byte>(buffer, 1, 2); // Write only the 2nd and 3rd elements from the buffer into the stream

            CollectionAssert.AreEqual(new byte[] { 2, 3 }, stream.ToArray());
        }
    }
    [TestMethod]
    public void ReadIntoBuffer_ShouldThrowException_WhenStreamIsAtItsEnd()
    {
        using (var stream = new MemoryStream(10))
        {
            // Set the position to the end of the stream
            stream.Position = stream.Length;

            var buffer = new GorgonNativeBuffer<byte>(10);

            Assert.ThrowsException<EndOfStreamException>(() => stream.Read<byte>(buffer, 0));
        }
    }

    [TestMethod]
    public void ReadIntoBuffer_ShouldThrowException_WhenIndexIsOutOfRange()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream(data))
        {
            var buffer = new GorgonNativeBuffer<byte>(10);

            Assert.ThrowsException<ArgumentException>(() => stream.Read<byte>(buffer, 11));
        }
    }

    [TestMethod]
    public void ReadIntoBuffer_ShouldReadData_WhenStreamAndBufferAreValid()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream(data))
        {
            var buffer = new GorgonNativeBuffer<byte>(4);

            stream.Read<byte>(buffer, 0);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], buffer[i]);
            }
        }
    }

    [TestMethod]
    public void ReadIntoBuffer_ShouldReadPartialData_WhenIndexIsSpecified()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6 };
        using (var stream = new MemoryStream(data))
        {
            var buffer = new GorgonNativeBuffer<byte>(4);

            stream.Read<byte>(buffer, 2);

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
            Assert.AreEqual(1, buffer[2]);
            Assert.AreEqual(2, buffer[3]);
        }
    }

    [TestMethod]
    public void ReadIntoBuffer_ShouldThrowException_WhenCountIsOutOfRange()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using (var stream = new MemoryStream(data))
        {
            var buffer = new GorgonNativeBuffer<byte>(2);

            Assert.ThrowsException<ArgumentException>(() => stream.Read<byte>(buffer, 0, 3));
        }
    }

    [TestMethod]
    public void ReadIntoBuffer_ShouldReadPartialData_WhenIndexAndCountAreSpecified()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6 };
        using (var stream = new MemoryStream(data))
        {
            var buffer = new GorgonNativeBuffer<byte>(4);

            stream.Read<byte>(buffer, 1, 2); // Start writing at the 2nd position in the buffer, read 2 elements from the stream

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(0, buffer[3]);
        }
    }

    [TestMethod]
    public void PinAsNativeBuffer_ShouldPinArraySuccessfully_WhenParametersAreValid()
    {
        // Arrange
        int[] data = { 1, 2, 3, 4, 5 };

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(1, 3);

        // Assert
        Assert.AreEqual(3, buffer.Length);
        Assert.AreEqual(2, buffer[0]);
        Assert.AreEqual(3, buffer[1]);
        Assert.AreEqual(4, buffer[2]);
    }

    [TestMethod]
    public void PinAsNativeBuffer_ShouldPinWholeArray_WhenIndexAndCountAreNotSpecified()
    {
        // Arrange
        int[] data = { 1, 2, 3, 4, 5 };

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer();

        // Assert
        Assert.AreEqual(data.Length, buffer.Length);
        CollectionAssert.AreEqual(data, buffer.ToArray());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PinAsNativeBuffer_ShouldThrowException_WhenIndexIsNegative()
    {
        // Arrange
        int[] data = { 1, 2, 3, 4, 5 };

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(-1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PinAsNativeBuffer_ShouldThrowException_WhenCountIsNegative()
    {
        // Arrange
        int[] data = { 1, 2, 3, 4, 5 };

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(0, -1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PinAsNativeBuffer_ShouldThrowException_WhenIndexPlusCountIsGreaterThanArrayLength()
    {
        // Arrange
        int[] data = { 1, 2, 3, 4, 5 };

        // Act
        using GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer(1, 5);
    }

    [TestMethod]
    public void PinAsNativeByteBuffer_ShouldPinSuccessfully_WhenValueTypeIsValid()
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
    public void PinAsNativeByteBuffer_ShouldPinSuccessfully_WhenValueTypeIsStruct()
    {
        // Arrange
        var value = new Point { X = 10, Y = 20 };

        // Act
        using GorgonNativeBuffer<byte> buffer = value.PinAsNativeByteBuffer();

        // Assert
        Assert.AreEqual(Unsafe.SizeOf<Point>(), buffer.Length);
        CollectionAssert.AreEqual(BitConverter.GetBytes(value.X).Concat(BitConverter.GetBytes(value.Y)).ToArray(), buffer.ToArray());
    }

    [TestMethod]
    public unsafe void CopyTo_ShouldCopySuccessfully_WhenSpanIsValid()
    {
        // Arrange
        Span<int> span = new Span<int>(new int[] { 1, 2, 3, 4, 5 });
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new GorgonPtr<int>(ptr, span.Length);

        // Act
        span.CopyTo(gorgonPtr, span.Length);

        // Assert
        for (int i = 0; i < span.Length; i++)
        {
            Assert.AreEqual(span[i], gorgonPtr[i]);
        }
    }

    [TestMethod]
    public unsafe void CopyTo_ShouldCopySuccessfully_WhenReadOnlySpanIsValid()
    {
        // Arrange
        ReadOnlySpan<int> span = new ReadOnlySpan<int>(new int[] { 1, 2, 3, 4, 5 });
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new GorgonPtr<int>(ptr, span.Length);

        // Act
        span.CopyTo(gorgonPtr, span.Length);

        // Assert
        for (int i = 0; i < span.Length; i++)
        {
            Assert.AreEqual(span[i], gorgonPtr[i]);
        }
    }

    [TestMethod]
    public unsafe void CopyTo_ShouldThrowException_WhenCountIsGreaterThanSpanLength()
    {
        // Arrange
        Span<int> span = new Span<int>(new int[] { 1, 2, 3, 4, 5 });
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new GorgonPtr<int>(ptr, span.Length);

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
    public unsafe void CopyTo_ShouldThrowException_WhenCountIsGreaterThanReadOnlySpanLength()
    {
        // Arrange
        ReadOnlySpan<int> span = new ReadOnlySpan<int>(new int[] { 1, 2, 3, 4, 5 });
        int* ptr = stackalloc int[span.Length];
        GorgonPtr<int> gorgonPtr = new GorgonPtr<int>(ptr, span.Length);

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
