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
    public async Task FlushAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.FlushAsync());
    }

    [TestMethod]
    public async Task CopyToAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.CopyToAsync(new MemoryStream(), 1024));
    }

    [TestMethod]
    public async Task ReadAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.ReadAsync(new byte[50], 0, 50));
    }

    [TestMethod]
    public async Task ReadAsyncMemory_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.ReadAsync(new Memory<byte>(new byte[50])).AsTask());
    }

    [TestMethod]
    public async Task WriteAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.WriteAsync(new byte[50], 0, 50));
    }

    [TestMethod]
    public async Task WriteAsyncMemory_ShouldThrowNotSupportedException()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<NotSupportedException>(() => wrapper.WriteAsync(new ReadOnlyMemory<byte>(new byte[50])).AsTask());
    }

    [TestMethod]
    public void Position_SetWithinRange_ShouldUpdatePosition()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act
        wrapper.Position = 25;

        // Assert
        Assert.AreEqual(25, wrapper.Position);
    }

    [TestMethod]
    public void Position_SetBeyondRange_ShouldSetToEnd()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act
        wrapper.Position = 60;

        // Assert
        Assert.AreEqual(50, wrapper.Position);
    }

    [TestMethod]
    public void Position_SetNegative_ShouldSetToStart()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);

        // Act
        wrapper.Position = -10;

        // Assert
        Assert.AreEqual(0, wrapper.Position);
    }

    [TestMethod]
    public void Position_Get_ShouldReturnCurrentPosition()
    {
        // Arrange
        var parentStream = new MemoryStream(new byte[100]);
        var wrapper = new GorgonSubStream(parentStream, 0, 50, true);
        wrapper.Position = 25;

        // Act
        var position = wrapper.Position;

        // Assert
        Assert.AreEqual(25, position);
    }

    [TestMethod]
    public void WriteByte_Success()
    {
        using MemoryStream parentStream = new MemoryStream();
        GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte value = 123;
        subStream.WriteByte(value);

        parentStream.Position = 0;
        byte result = (byte)parentStream.ReadByte();

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void WriteByte_AdvancesPosition()
    {
        using MemoryStream parentStream = new MemoryStream();
        GorgonSubStream subStream = new GorgonSubStream(parentStream);

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
    public void WriteByte_StreamDoesNotSupportWriting_ThrowsException()
    {
        using MemoryStream readOnlyStream = new MemoryStream(new byte[0]);
        GorgonSubStream subStream = new GorgonSubStream(readOnlyStream, allowWrite: false);

        Assert.ThrowsException<NotSupportedException>(() => subStream.WriteByte(123));
    }

    [TestMethod]
    public void WriteByte_Subsection_Success()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[64]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 6, 24);

        byte value = 123;
        subStream.WriteByte(value);

        parentStream.Position = 6;
        byte result = (byte)parentStream.ReadByte();

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void WriteByte_Subsection_AdvancesPosition()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[64]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 6, 24);

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
    public void WriteByte_Subsection_ExceedsLength_Grows()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[64]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 6, 24);

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
    public void ReadByte_WithinBounds_ReturnsByte()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 1, 3);

        int result = subStream.ReadByte();

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void ReadByte_AtEnd_ReturnsMinusOne()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 1, 3);

        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();

        int result = subStream.ReadByte();

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void ReadByte_AfterEnd_ReturnsMinusOne()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        GorgonSubStream subStream = new GorgonSubStream(parentStream, 1, 3);

        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();
        subStream.ReadByte();

        int result = subStream.ReadByte();

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void Read_WhenOffsetIsNegative_ThrowsArgumentOutOfRangeException()
    {
        using MemoryStream parentStream = new MemoryStream();
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Read(buffer, -1, 0));
    }

    [TestMethod]
    public void Read_WhenCountIsNegative_ThrowsArgumentOutOfRangeException()
    {
        using MemoryStream parentStream = new MemoryStream();
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Read(buffer, 0, -1));
    }

    [TestMethod]
    public void Read_WhenOffsetAndCountAreLargerThanBufferLength_ThrowsArgumentException()
    {
        using MemoryStream parentStream = new MemoryStream();
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte[] buffer = new byte[10];
        Assert.ThrowsException<ArgumentException>(() => subStream.Read(buffer, 5, 6));
    }

    [TestMethod]
    public void Read_WhenActualCountIsZero_ReturnsZero()
    {
        using MemoryStream parentStream = new MemoryStream();
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte[] buffer = new byte[10];
        int bytesRead = subStream.Read(buffer, 0, 10);

        Assert.AreEqual(0, bytesRead);
    }

    [TestMethod]
    public void Read_WhenActualCountIsGreaterThanZero_ReturnsNumberOfBytesRead()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[20], 0, 20);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);

        byte[] buffer = new byte[10];
        int bytesRead = subStream.Read(buffer, 0, 10);

        Assert.AreEqual(10, bytesRead);
    }

    [TestMethod]
    public void Read_WhenBufferIsEmpty_ShouldReturnZero()
    {
        using MemoryStream parentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, 0, parentStream.Length);

        int bytesRead = subStream.Read(Span<byte>.Empty);

        Assert.AreEqual(0, bytesRead);
    }

    [TestMethod]
    public void Read_WhenBufferIsNotEmpty_ShouldReturnNumberOfBytesRead()
    {
        using MemoryStream parentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, 0, parentStream.Length);

        var buffer = new Span<byte>(new byte[5]);

        int bytesRead = subStream.Read(buffer);

        Assert.AreEqual(5, bytesRead);
        Assert.AreEqual("Hello", Encoding.UTF8.GetString(buffer.ToArray()));
    }

    [TestMethod]
    public void Read_WhenBufferIsLargerThanStream_ShouldReturnActualNumberOfBytesRead()
    {
        using MemoryStream parentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, 0, parentStream.Length);

        var buffer = new Span<byte>(new byte[50]);

        int bytesRead = subStream.Read(buffer);

        Assert.AreEqual(13, bytesRead); // "Hello, World!" is 13 bytes long
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(buffer.Slice(0, bytesRead).ToArray()));
    }

    [TestMethod]
    public void Seek_FromBegin_ShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new MemoryStream(new byte[100]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream);
        long newPosition = subStream.Seek(50, SeekOrigin.Begin);
        Assert.AreEqual(50, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void Seek_FromEnd_ShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new MemoryStream(new byte[100]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream);
        long newPosition = subStream.Seek(-50, SeekOrigin.End);
        Assert.AreEqual(50, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void Seek_FromCurrent_ShouldSetPositionCorrectly()
    {
        MemoryStream parentStream = new MemoryStream(new byte[100]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream);
        subStream.Seek(50, SeekOrigin.Begin);
        long newPosition = subStream.Seek(10, SeekOrigin.Current);
        Assert.AreEqual(60, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void Seek_BeyondBounds_ShouldSetPositionToBoundary()
    {
        MemoryStream parentStream = new MemoryStream(new byte[100]);
        GorgonSubStream subStream = new GorgonSubStream(parentStream);
        long newPosition = subStream.Seek(200, SeekOrigin.Begin);
        Assert.AreEqual(100, newPosition);
        newPosition = subStream.Seek(-200, SeekOrigin.Begin);
        Assert.AreEqual(0, newPosition);
        subStream.Dispose();
        parentStream.Dispose();
    }

    [TestMethod]
    public void SetLength_ShouldSetLengthCorrectly()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        subStream.SetLength(50);
        Assert.AreEqual(50, subStream.Length);
    }

    [TestMethod]
    public void SetLength_ShouldSetLengthToZero_WhenNegativeValueProvided()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        subStream.SetLength(-50);
        Assert.AreEqual(0, subStream.Length);
    }

    [TestMethod]
    public void SetLength_ShouldSetLengthToParentStreamLength_WhenValueExceedsParentStreamLength()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        subStream.SetLength(200);
        Assert.AreEqual(100, subStream.Length);
    }

    [TestMethod]
    public void SetLength_ShouldThrowIOException_WhenStreamIsReadOnly()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, allowWrite: false);
        Assert.ThrowsException<IOException>(() => subStream.SetLength(50));
    }

    [TestMethod]
    public void Write_ShouldWriteDataCorrectly()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        subStream.Write(data, 0, data.Length);
        parentStream.Position = 0;
        byte[] readData = new byte[data.Length];
        parentStream.Read(readData, 0, data.Length);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(readData));
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenOffsetAndCountAreNegative()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => subStream.Write(data, -1, -1));
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenSumOfOffsetAndCountExceedsBufferLength()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<ArgumentException>(() => subStream.Write(data, 0, data.Length + 1));
    }

    [TestMethod]
    public void Write_ShouldThrowException_WhenStreamIsReadOnly()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, allowWrite: false);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        Assert.ThrowsException<IOException>(() => subStream.Write(data, 0, data.Length));
    }

    [TestMethod]
    public void Constructor_ShouldInitializeCorrectly()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, 10, 50, true);
        Assert.AreEqual(50, subStream.Length);
        Assert.AreEqual(0, subStream.Position);
        Assert.IsTrue(subStream.CanWrite);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenStreamStartIsNegative()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonSubStream(parentStream, -1));
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenStreamSizeIsNegative()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonSubStream(parentStream, 0, -1));
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenStreamStartExceedsParentStreamLength()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        Assert.ThrowsException<EndOfStreamException>(() => new GorgonSubStream(parentStream, 200));
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenParentStreamCannotSeek()
    {
        using Stream dummy = Mock.Of<Stream>(x => x.CanSeek == false);
        Assert.ThrowsException<ArgumentException>(() => new GorgonSubStream(dummy));
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenParentStreamIsReadOnlyAndAllowWriteIsTrue()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100], false);
        Assert.ThrowsException<IOException>(() => new GorgonSubStream(parentStream, 0, null, true));
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenStreamSizeExceedsRemainingParentStreamLength()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        Assert.ThrowsException<EndOfStreamException>(() => new GorgonSubStream(parentStream, 50, 60));
    }

    [TestMethod]
    public void Constructor_ShouldStartAtCurrentParentStreamPosition_WhenStreamStartIsNotDefined()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        parentStream.Position = 50;
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
        subStream.Write(data, 0, data.Length);
        parentStream.Position = 50;
        byte[] readData = new byte[data.Length];
        parentStream.Read(readData, 0, data.Length);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(readData));
    }

    [TestMethod]
    public void Constructor_ShouldCoverEntireParentStream_WhenStreamSizeIsNotDefined()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream);
        Assert.AreEqual(100, subStream.Length);
    }

    [TestMethod]
    public void Constructor_ShouldMakeStreamReadOnly_WhenAllowWriteIsFalse()
    {
        using MemoryStream parentStream = new MemoryStream(new byte[100]);
        using GorgonSubStream subStream = new GorgonSubStream(parentStream, 0, null, false);
        Assert.IsFalse(subStream.CanWrite);
    }
}
