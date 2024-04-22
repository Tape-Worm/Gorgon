using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Gorgon.IO;
using Moq;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonSubStreamTests
{
    [TestMethod]
    public void DoNotMoveParentPosition()
    {
        using MemoryStream parent = new();

        long parentPosition = parent.Position;

        using GorgonSubStream subStream = new(parent);

        byte[] bytes = Encoding.UTF8.GetBytes("Hello, World!");

        subStream.Write(bytes);

        Assert.AreEqual(parentPosition, parent.Position);

        parent.Position += subStream.Length;

        Assert.AreNotEqual(parentPosition, parent.Position);
        Assert.AreEqual(parent.Length, parent.Position);
    }

    [TestMethod]
    public async Task FlushAsyncShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.FlushAsync());
    }

    [TestMethod]
    public async Task CopyToAsyncShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.CopyToAsync(new MemoryStream(), 1024));
    }

    [TestMethod]
    public async Task ReadAsyncShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.ReadAsync(new byte[50], 0, 50));
    }

    [TestMethod]
    public async Task ReadAsyncMemoryShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.ReadAsync(new Memory<byte>(new byte[50])).AsTask());
    }

    [TestMethod]
    public async Task WriteAsyncShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.WriteAsync(new byte[50], 0, 50));
    }

    [TestMethod]
    public async Task WriteAsyncMemoryShouldThrowNotSupportedException()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.WriteAsync(new ReadOnlyMemory<byte>(new byte[50])).AsTask());
    }

    [TestMethod]
    public void PositionSetWithinRangeShouldUpdatePosition()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true)
        {
            // Act
            Position = 25
        };

        // Assert
        Assert.AreEqual(25, wrapper.Position);
    }

    [TestMethod]
    public void PositionSetBeyondRangeShouldSetToEnd()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true)
        {
            // Act
            Position = 60
        };

        // Assert
        Assert.AreEqual(50, wrapper.Position);
    }

    [TestMethod]
    public void PositionSetNegativeShouldSetToStart()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true)
        {
            // Act
            Position = -10
        };

        // Assert
        Assert.AreEqual(0, wrapper.Position);
    }

    [TestMethod]
    public void PositionGetShouldReturnCurrentPosition()
    {
        // Arrange
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream wrapper = new(parentStream, 0, 50, true)
        {
            Position = 25
        };

        // Act
        long position = wrapper.Position;

        // Assert
        Assert.AreEqual(25, position);
    }

    [TestMethod]
    public void WriteByteSuccess()
    {
        using MemoryStream parentStream = new();
        GorgonSubStream subStream = new(parentStream);

        byte value = 123;
        subStream.WriteByte(value);

        parentStream.Position = 0;
        byte result = (byte)parentStream.ReadByte();

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void WriteByteAdvancesPosition()
    {
        using MemoryStream parentStream = new();
        GorgonSubStream subStream = new(parentStream);

        byte value1 = 123;
        byte value2 = 124;

        subStream.WriteByte(value1);
        subStream.WriteByte(value2);

        parentStream.Position = 0;
        byte result1 = (byte)parentStream.ReadByte();
        byte result2 = (byte)parentStream.ReadByte();

        Assert.AreEqual(value1, result1);
        Assert.AreEqual(value2, result2);
    }

    [TestMethod]
    public void WriteByteStreamDoesNotSupportWritingThrowsException()
    {
        using MemoryStream readOnlyStream = new([]);
        GorgonSubStream subStream = new(readOnlyStream, allowWrite: false);

        Assert.ThrowsException<NotSupportedException>(() => subStream.WriteByte(123));
    }

    [TestMethod]
    public void WriteByteSubsectionSuccess()
    {
        using MemoryStream parentStream = new(new byte[64]);
        GorgonSubStream subStream = new(parentStream, 6, 24);

        byte value = 123;
        subStream.WriteByte(value);

        parentStream.Position = 6;
        byte result = (byte)parentStream.ReadByte();

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void WriteByteSubsectionAdvancesPosition()
    {
        using MemoryStream parentStream = new(new byte[64]);
        GorgonSubStream subStream = new(parentStream, 6, 24);

        byte value1 = 123;
        byte value2 = 124;

        subStream.WriteByte(value1);
        subStream.WriteByte(value2);

        parentStream.Position = 6;
        byte result1 = (byte)parentStream.ReadByte();
        byte result2 = (byte)parentStream.ReadByte();

        Assert.AreEqual(value1, result1);
        Assert.AreEqual(value2, result2);
    }

    [TestMethod]
    public void WriteByteSubsectionExceedsLengthGrows()
    {
        using MemoryStream parentStream = new(new byte[64]);
        GorgonSubStream subStream = new(parentStream, 6, 24);

        for (int i = 0; i < 24; i++)
        {
            subStream.WriteByte(123);
        }

        // Write one more byte, which should cause the substream to grow
        subStream.WriteByte(124);

        // Check that the substream's length has increased
        Assert.AreEqual(25, subStream.Length);

        // Check that the byte was written correctly
        parentStream.Position = 6 + 24;
        byte result = (byte)parentStream.ReadByte();
        Assert.AreEqual(124, result);
    }

    [TestMethod]
    public void ReadByteWithinBoundsReturnsByte()
    {
        using MemoryStream parentStream = new([1, 2, 3, 4, 5]);
        GorgonSubStream subStream = new(parentStream, 1, 3);

        int result = subStream.ReadByte();

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void ReadByteAtEndReturnsMinusOne()
    {
        using MemoryStream parentStream = new([1, 2, 3, 4, 5]);
        GorgonSubStream subStream = new(parentStream, 1, 3);

        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();

        int result = subStream.ReadByte();

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void ReadByteAfterEndReturnsMinusOne()
    {
        using MemoryStream parentStream = new([1, 2, 3, 4, 5]);
        GorgonSubStream subStream = new(parentStream, 1, 3);

        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();

        int result = subStream.ReadByte();

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void ReadWhenOffsetIsNegativeThrowsArgumentOutOfRangeException()
    {
        using MemoryStream parentStream = new();
        using GorgonSubStream subStream = new(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Read(buffer, -1, 0));
    }

    [TestMethod]
    public void ReadWhenCountIsNegativeThrowsArgumentOutOfRangeException()
    {
        using MemoryStream parentStream = new();
        using GorgonSubStream subStream = new(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Read(buffer, 0, -1));
    }

    [TestMethod]
    public void ReadWhenOffsetAndCountAreLargerThanBufferLengthThrowsArgumentException()
    {
        using MemoryStream parentStream = new();
        using GorgonSubStream subStream = new(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentException>(() => subStream.Read(buffer, 5, 6));
    }

    [TestMethod]
    public void ReadWhenActualCountIsZeroReturnsZero()
    {
        using MemoryStream parentStream = new();
        using GorgonSubStream subStream = new(parentStream);

        byte[] buffer = new byte[10];
        int bytesRead = subStream.Read(buffer, 0, 10);

        Assert.AreEqual(0, bytesRead);
    }

    [TestMethod]
    public void ReadWhenActualCountIsGreaterThanZeroReturnsNumberOfBytesRead()
    {
        using MemoryStream parentStream = new(new byte[20], 0, 20);
        using GorgonSubStream subStream = new(parentStream);

        byte[] buffer = new byte[10];
        int bytesRead = subStream.Read(buffer, 0, 10);

        Assert.AreEqual(10, bytesRead);
    }

    [TestMethod]
    public void ReadWhenBufferIsEmptyShouldReturnZero()
    {
        using MemoryStream parentStream = new(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new(parentStream, 0, parentStream.Length);

        int bytesRead = subStream.Read([]);

        Assert.AreEqual(0, bytesRead);
    }

    [TestMethod]
    public void ReadWhenBufferIsNotEmptyShouldReturnNumberOfBytesRead()
    {
        using MemoryStream parentStream = new(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new(parentStream, 0, parentStream.Length);

        Span<byte> buffer = new(new byte[5]);

        int bytesRead = subStream.Read(buffer);

        Assert.AreEqual(5, bytesRead);
        Assert.AreEqual("Hello", Encoding.UTF8.GetString(buffer.ToArray()));
    }

    [TestMethod]
    public void ReadWhenBufferIsLargerThanStreamShouldReturnActualNumberOfBytesRead()
    {
        using MemoryStream parentStream = new(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new(parentStream, 0, parentStream.Length);

        Span<byte> buffer = new(new byte[50]);

        int bytesRead = subStream.Read(buffer);

        Assert.AreEqual(13, bytesRead); // "Hello, World!" is 13 bytes long
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(buffer[..bytesRead].ToArray()));
    }

    [TestMethod]
    public void SeekFromBeginShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream subStream = new(parentStream);
        long newPosition = subStream.Seek(50, SeekOrigin.Begin);
        Assert.AreEqual(50, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void SeekFromEndShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream subStream = new(parentStream);
        long newPosition = subStream.Seek(-50, SeekOrigin.End);
        Assert.AreEqual(50, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void SeekFromCurrentShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream subStream = new(parentStream);
        subStream.Seek(50, SeekOrigin.Begin);
        long newPosition = subStream.Seek(10, SeekOrigin.Current);
        Assert.AreEqual(60, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void SeekBeyondBoundsShouldSetPositionToBoundary()
    {
        MemoryStream parentStream = new(new byte[100]);
        GorgonSubStream subStream = new(parentStream);
        long newPosition = subStream.Seek(200, SeekOrigin.Begin);
        Assert.AreEqual(100, newPosition);
        newPosition = subStream.Seek(-200, SeekOrigin.Begin);
        Assert.AreEqual(0, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void SetLengthShouldSetLengthCorrectly()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        subStream.SetLength(50);
        Assert.AreEqual(50, subStream.Length);
    }

    [TestMethod]
    public void SetLengthShouldSetLengthToZeroWhenNegativeValueProvided()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        subStream.SetLength(-50);
        Assert.AreEqual(0, subStream.Length);
    }

    [TestMethod]
    public void SetLengthShouldSetLengthToParentStreamLengthWhenValueExceedsParentStreamLength()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        subStream.SetLength(200);
        Assert.AreEqual(100, subStream.Length);
    }

    [TestMethod]
    public void SetLengthShouldThrowIOExceptionWhenStreamIsReadOnly()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream, allowWrite: false);
        Assert.ThrowsException<IOException>(() => subStream.SetLength(50));
    }

    [TestMethod]
    public void WriteShouldWriteDataCorrectly()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        subStream.Write(data, 0, data.Length);
        parentStream.Position = 0;
        byte[] readData = new byte[data.Length];
        parentStream.Read(readData, 0, data.Length);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(readData));
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenOffsetAndCountAreNegative()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Write(data, -1, -1));
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenSumOfOffsetAndCountExceedsBufferLength()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<ArgumentException>(() => subStream.Write(data, 0, data.Length + 1));
    }

    [TestMethod]
    public void WriteShouldThrowExceptionWhenStreamIsReadOnly()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream, allowWrite: false);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<IOException>(() => subStream.Write(data, 0, data.Length));
    }

    [TestMethod]
    public void ConstructorShouldInitializeCorrectly()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream, 10, 50, true);
        Assert.AreEqual(50, subStream.Length);
        Assert.AreEqual(0, subStream.Position);
        Assert.IsTrue(subStream.CanWrite);
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenStreamStartIsNegative()
    {
        using MemoryStream parentStream = new(new byte[100]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonSubStream(parentStream, -1));
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenStreamSizeIsNegative()
    {
        using MemoryStream parentStream = new(new byte[100]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonSubStream(parentStream, 0, -1));
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenStreamStartExceedsParentStreamLength()
    {
        using MemoryStream parentStream = new(new byte[100]);
        Assert.ThrowsException<EndOfStreamException>(() => new GorgonSubStream(parentStream, 200));
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenParentStreamCannotSeek()
    {
        using Stream dummy = Mock.Of<Stream>(x => x.CanSeek == false);
        Assert.ThrowsException<ArgumentException>(() => new GorgonSubStream(dummy));
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenParentStreamIsReadOnlyAndAllowWriteIsTrue()
    {
        using MemoryStream parentStream = new(new byte[100], false);
        Assert.ThrowsException<IOException>(() => new GorgonSubStream(parentStream, 0, null, true));
    }

    [TestMethod]
    public void ConstructorShouldThrowExceptionWhenStreamSizeExceedsRemainingParentStreamLength()
    {
        using MemoryStream parentStream = new(new byte[100]);
        Assert.ThrowsException<EndOfStreamException>(() => new GorgonSubStream(parentStream, 50, 60));
    }

    [TestMethod]
    public void ConstructorShouldStartAtCurrentParentStreamPositionWhenStreamStartIsNotDefined()
    {
        using MemoryStream parentStream = new(new byte[100]);
        parentStream.Position = 50;
        using GorgonSubStream subStream = new(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        subStream.Write(data, 0, data.Length);
        parentStream.Position = 50;
        byte[] readData = new byte[data.Length];
        parentStream.Read(readData, 0, data.Length);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(readData));
    }

    [TestMethod]
    public void ConstructorShouldCoverEntireParentStreamWhenStreamSizeIsNotDefined()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream);
        Assert.AreEqual(100, subStream.Length);
    }

    [TestMethod]
    public void ConstructorShouldMakeStreamReadOnlyWhenAllowWriteIsFalse()
    {
        using MemoryStream parentStream = new(new byte[100]);
        using GorgonSubStream subStream = new(parentStream, 0, null, false);
        Assert.IsFalse(subStream.CanWrite);
    }
}
