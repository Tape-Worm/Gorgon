using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonIOExtensionsTests
{
    [TestMethod]
    public void CopyToStream_ShouldCopyDataCorrectly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual("Hello, World!", result);

        // Act
        sourceStream.Position = 0;
        destinationStream.Position = 0;
        bytesCopied = sourceStream.CopyToStream(destinationStream, 5);

        // Assert
        Assert.AreEqual(5, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader2 = new(destinationStream);
        result = reader2.ReadToEnd();
        Assert.AreEqual("Hello, World!", result);
    }

    [TestMethod]
    public void CopyToStream_ShouldReturnZero_WhenSourceStreamIsEmpty()
    {
        // Arrange
        string testData = "";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(0, bytesCopied);
    }

    [TestMethod]
    public void CopyToStream_ShouldThrowArgumentException_WhenDestinationStreamIsReadOnly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new(new byte[0], false); // Read-only stream

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => sourceStream.CopyToStream(destinationStream, testData.Length));
    }

    [TestMethod]
    public void CopyToStream_ShouldCopyDataInChunks()
    {
        // Arrange
        string testData = "Exercitationem fugiat voluptatum est adipisci. Quia doloribus inventore explicabo quaerat. Et esse facilis esse et qui in non aut.\n\nEos qui repudiandae aut nesciunt et voluptatem deleniti debitis. At magnam ea dolorem ea veritatis. Minus ab eos id laudantium cumque dolorum.";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();
        byte[] buffer = new byte[75]; // Buffer size is 1/4 the length of the text

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length, buffer);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual(testData, result);
    }

    [TestMethod]
    public async Task CopyToStreamAsync_ShouldCopyDataCorrectly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = await sourceStream.CopyToStreamAsync(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual(testData, result);
    }

    [TestMethod]
    public async Task CopyToStreamAsync_ShouldReturnZero_WhenSourceStreamIsEmpty()
    {
        // Arrange
        string testData = "";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = await sourceStream.CopyToStreamAsync(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(0, bytesCopied);
    }

    [TestMethod]
    public async Task CopyToStreamAsync_ShouldThrowArgumentException_WhenDestinationStreamIsReadOnly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new(new byte[0], false); // Read-only stream

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => sourceStream.CopyToStreamAsync(destinationStream, testData.Length));
    }

    [TestMethod]
    public async Task CopyToStreamAsync_ShouldThrowArgumentException_WhenBufferSizeIsLessThanOne()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => sourceStream.CopyToStreamAsync(destinationStream, testData.Length, 0));
    }

    /* This test is disabled because we can't reliably test cancellation in a unit test.
    [TestMethod]
    public async Task CopyToStreamAsync_ShouldCancelOperation_WhenCancellationRequested()
    {
        // Arrange
        string testData = new string('a', 100_000_000); // Large data to ensure operation takes some time
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();
        CancellationTokenSource cancellationTokenSource = new();

        // Act
        cancellationTokenSource.CancelAfter(20); // Cancel after 10 milliseconds
        try
        {
            await Task.Delay(18);
            await sourceStream.CopyToStreamAsync(destinationStream, testData.Length, 8, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Assert
            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
            return;
        }

        Assert.Fail("Expected OperationCanceledException was not thrown.");
    }
    */

    [TestMethod]
    public unsafe void WriteFromPointer_WritesCorrectData()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can write to a stream.");
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);
        fixed (byte* pData = data)
        {
            GorgonPtr<byte> srcBuffer = new GorgonPtr<byte>(pData, data.Length);

            // Act
            writer.WriteFromPointer(srcBuffer);

            // Assert
            byte[] writtenData = stream.ToArray();
            Assert.IsTrue(data.SequenceEqual(writtenData));
        }
    }

    [TestMethod]
    public void WriteFromPointer_DoesNothing_WhenPointerIsNull()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);
        GorgonPtr<byte> nullPointer = GorgonPtr<byte>.NullPtr;

        // Act
        writer.WriteFromPointer(nullPointer);

        // Assert
        Assert.AreEqual(0, stream.Length);
    }

    [TestMethod]
    public unsafe void ReadToPointer_ReadsCorrectData()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);
        byte[] buffer = new byte[data.Length];
        fixed (byte* pBuffer = buffer)
        {
            GorgonPtr<byte> destBuffer = new GorgonPtr<byte>(pBuffer, buffer.Length);

            // Act
            reader.ReadToPointer(destBuffer);

            // Assert
            CollectionAssert.AreEqual(data, buffer);
        }
    }

    [TestMethod]
    public unsafe void ReadToPointer_ThrowsEndOfStreamException_WhenPointerIsTooLarge()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);
        byte[] buffer = new byte[data.Length + 1]; // Buffer is larger than the stream.
        fixed (byte* pBuffer = buffer)
        {
            GorgonPtr<byte> destBuffer = new GorgonPtr<byte>(pBuffer, buffer.Length);

            // Act & Assert
            Assert.ThrowsException<EndOfStreamException>(() => reader.ReadToPointer(destBuffer));
        }
    }

    [TestMethod]
    public void ReadToPointer_ThrowsNullReferenceException_WhenPointerIsNull()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);
        GorgonPtr<byte> nullPointer = GorgonPtr<byte>.NullPtr;

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => reader.ReadToPointer(nullPointer));
    }

    [TestMethod]
    public void ReadValue_ReadsCorrectData()
    {
        // Arrange
        GorgonPoint data = new GorgonPoint(123, 456);
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true); // leaveOpen is set to true.
        writer.Write(data.X);
        writer.Write(data.Y);
        stream.Position = 0;
        using BinaryReader reader = new BinaryReader(stream);

        // Act
        reader.ReadValue<GorgonPoint>(out GorgonPoint result);
        stream.Position = 0; // Reset the stream position.
        GorgonPoint result2 = reader.ReadValue<GorgonPoint>();

        // Assert
        Assert.AreEqual(data, result);
        Assert.AreEqual(data, result2);
    }

    [TestMethod]
    public void ReadValue_ThrowsEndOfStreamException_WhenStreamIsTooSmall()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(new byte[1]); // Stream is too small to hold a GorgonPoint.
        using BinaryReader reader = new BinaryReader(stream);

        // Act & Assert
        Assert.ThrowsException<EndOfStreamException>(() => reader.ReadValue<GorgonPoint>(out _));
    }

    [TestMethod]
    public void WriteValue_WritesCorrectData()
    {
        // Arrange
        GorgonPoint data = new GorgonPoint(123, 456);
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true); // leaveOpen is set to true.

        // Act
        writer.WriteValue(data);
        writer.WriteValue(in data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new BinaryReader(stream);
        GorgonPoint result = new GorgonPoint(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data, result);

        GorgonPoint result2 = new GorgonPoint(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data, result2);
    }

    [TestMethod]
    public void WriteRange_WritesCorrectData()
    {
        // Arrange
        GorgonPoint[] data = { new GorgonPoint(123, 456), new GorgonPoint(789, 012) };
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true); // leaveOpen is set to true.

        // Act
        writer.WriteRange<GorgonPoint>(data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new BinaryReader(stream);
        GorgonPoint result1 = new GorgonPoint(reader.ReadInt32(), reader.ReadInt32());
        GorgonPoint result2 = new GorgonPoint(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data[0], result1);
        Assert.AreEqual(data[1], result2);
    }

    [TestMethod]
    public void ReadRange_ReadsCorrectData()
    {
        // Arrange
        GorgonPoint[] data = new GorgonPoint[] { new GorgonPoint(123, 456), new GorgonPoint(789, 012) };
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true); // leaveOpen is set to true.
        foreach (GorgonPoint point in data)
        {
            writer.Write(point.X);
            writer.Write(point.Y);
        }
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new BinaryReader(stream);
        GorgonPoint[] result = new GorgonPoint[2];

        // Act
        reader.ReadRange<GorgonPoint>(result);

        // Assert
        Assert.AreEqual(data[0], result[0]);
        Assert.AreEqual(data[1], result[1]);
    }

    [TestMethod]
    public void ReadRange_ThrowsArgumentEmptyException_WhenValuesIsEmpty()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream();
        using BinaryReader reader = new BinaryReader(stream);
        GorgonPoint[] result = new GorgonPoint[0];

        // Act & Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => reader.ReadRange<GorgonPoint>(result));
    }

    [TestMethod]
    public void WriteString_WritesCorrectData()
    {
        // Arrange
        string data = "Test string";
        using MemoryStream stream = new MemoryStream();

        // Act
        int bytesWritten = stream.WriteString(data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
        int length = reader.Read7BitEncodedInt(); // Read the length prefix.
        string result = new string(reader.ReadChars(length));
        Assert.AreEqual(data, result);
        Assert.AreEqual(Encoding.UTF8.GetByteCount(data) + 1, bytesWritten);
    }

    [TestMethod]
    public void WriteString_ThrowsIOException_WhenStreamIsReadOnly()
    {
        // Arrange
        string data = "Test string";
        using MemoryStream stream = new MemoryStream(new byte[100], false); // Write-only stream.

        // Act & Assert
        Assert.ThrowsException<IOException>(() => stream.WriteString(data));
    }

    [TestMethod]
    public void ReadString_ReadsCorrectData()
    {
        // Arrange
        string data = "Test string";
        Encoding encoding = Encoding.UTF8;
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, encoding, true); // leaveOpen is set to true.
        writer.Write(data); // Write the string data.
        stream.Position = 0; // Reset the stream position.

        // Act
        string result = stream.ReadString(encoding);

        // Assert
        Assert.AreEqual(data, result);
    }

    [TestMethod]
    public void ReadString_ThrowsIOException_WhenStreamIsAtEnd()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream();

        // Act & Assert
        Assert.ThrowsException<IOException>(() => stream.ReadString());
    }

    [TestMethod]
    public void WriteAndReadString_WithDifferentEncodings()
    {
        // Arrange
        string data = "Test string";
        Encoding encoding1 = Encoding.UTF8;
        Encoding encoding2 = Encoding.ASCII;
        using MemoryStream stream = new MemoryStream();

        // Act
        int bytesWritten1 = stream.WriteString(data, encoding1);
        stream.Position = 0; // Reset the stream position.
        string readData1 = stream.ReadString(encoding1);

        stream.Position = 0; // Reset the stream position.
        int bytesWritten2 = stream.WriteString(data, encoding2);
        stream.Position = 0; // Reset the stream position.
        string readData2 = stream.ReadString(encoding2);

        // Assert
        Assert.AreEqual(data, readData1);
        Assert.AreEqual(encoding1.GetByteCount(data) + 1, bytesWritten1);
        Assert.AreEqual(data, readData2);
        Assert.AreEqual(encoding2.GetByteCount(data) + 1, bytesWritten2);
    }

    [TestMethod]
    public void FormatFileName_NullInput_ReturnsEmptyString()
    {
        string? path = null;
        string result = path.FormatFileName();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatFileName_EmptyInput_ReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatFileName();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatFileName_PathWithoutFilename_ReturnsEmptyString()
    {
        string path = "C:\\Some\\Path\\";
        string result = path.FormatFileName();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatFileName_PathWithIllegalCharacters_ReturnsSanitizedFilename()
    {
        string path = "C:\\Some\\Path\\file*name.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("file_name.txt", result);
    }

    [TestMethod]
    public void FormatFileName_PathWithLegalCharacters_ReturnsSameFilename()
    {
        string path = "C:\\Some\\Path\\filename.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("filename.txt", result);
    }

    [TestMethod]
    public void FormatFileName_PathWithAlternateDivider_ReturnsCorrectFilename()
    {
        string path = "C:/Some/Path/filename.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("filename.txt", result);
    }

    [TestMethod]
    public void FormatDirectory_NullInput_ReturnsEmptyString()
    {
        string? path = null;
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectory_EmptyInput_ReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithIllegalCharacters_ReturnsSanitizedPath()
    {
        string path = @"C:\Some\Invalid*Path\";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Invalid_Path\", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithAlternateSeparator_ReturnsPathWithStandardSeparator()
    {
        string path = @"C:/Some/Path/";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithDoubledSeparators_ReturnsPathWithSingleSeparators()
    {
        string path = @"C:\Some\\Path\";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithMultipleConsecutiveSeparatorsInMiddle_ReturnsPathWithSingleSeparators()
    {
        string path = @"C:\Some\\\\\Path\";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectory_NullInput_AltSeparator_ReturnsEmptyString()
    {
        string? path = null;
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectory_EmptyInput_AltSeparator_ReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithIllegalCharacters_AltSeparator_ReturnsSanitizedPath()
    {
        string path = @"C:/Some/Invalid*Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Invalid_Path/", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithAlternateSeparator_AltSeparator_ReturnsPathWithStandardSeparator()
    {
        string path = @"C:\Some\Path\";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithDoubledSeparators_AltSeparator_ReturnsPathWithSingleSeparators()
    {
        string path = @"C:/Some//Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatDirectory_PathWithMultipleConsecutiveSeparatorsInMiddle_AltSeparator_ReturnsPathWithSingleSeparators()
    {
        string path = @"C:/Some/////Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatPathPart_NullInput_ReturnsEmptyString()
    {
        string? path = null;
        string result = path.FormatPathPart();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPathPart_EmptyInput_ReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatPathPart();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPathPart_PathWithIllegalCharacters_ReturnsSanitizedPath()
    {
        string path = "Invalid*Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Invalid_Path", result);
    }

    [TestMethod]
    public void FormatPathPart_PathWithDirectorySeparator_ReturnsPathWithUnderscores()
    {
        string path = "Some\\Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Some_Path", result);
    }

    [TestMethod]
    public void FormatPathPart_PathWithAltDirectorySeparator_ReturnsPathWithUnderscores()
    {
        string path = "Some/Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Some_Path", result);
    }

    [TestMethod]
    public void GetPathParts_NullInput_ReturnsEmptyArray()
    {
        string? path = null;
        string[] result = path.GetPathParts(Path.DirectorySeparatorChar);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void GetPathParts_EmptyInput_ReturnsEmptyArray()
    {
        string path = string.Empty;
        string[] result = path.GetPathParts(Path.DirectorySeparatorChar);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void GetPathParts_PathWithDirectorySeparator_ReturnsPathParts()
    {
        string path = @"C:\Some\Path";
        string[] result = path.GetPathParts(Path.DirectorySeparatorChar);
        CollectionAssert.AreEqual(new[] { "C:", "Some", "Path" }, result);
    }

    [TestMethod]
    public void GetPathParts_PathWithAltDirectorySeparator_ReturnsPathParts()
    {
        string path = @"C:/Some/Path";
        string[] result = path.GetPathParts(Path.AltDirectorySeparatorChar);
        CollectionAssert.AreEqual(new[] { "C:", "Some", "Path" }, result);
    }

    [TestMethod]
    public void FormatPath_NullInput_ReturnsEmptyString()
    {
        string? path = null;
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPath_EmptyInput_ReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPath_PathWithIllegalCharacters_ReturnsSanitizedPath()
    {
        string path = @"C:\Some\Invalid*Path\file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Invalid_Path\file.txt", result);
    }

    [TestMethod]
    public void FormatPath_PathWithAltDirectorySeparator_ReturnsSanitizedPath()
    {
        string path = @"C:/Some/Invalid*Path/file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Invalid_Path/file.txt", result);
    }

    [TestMethod]
    public void FormatPath_PathWithIllegalCharactersInFileName_ReturnsSanitizedPath()
    {
        string path = @"C:\Some\Path\Invalid*file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\Invalid_file.txt", result);
    }

    [TestMethod]
    public void FormatPath_PathWithAltDirectorySeparatorAndIllegalCharactersInFileName_ReturnsSanitizedPath()
    {
        string path = @"C:/Some/Path/Invalid*file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/Invalid_file.txt", result);
    }

    [TestMethod]
    public void FormatPath_PathWithDoubledSeparators_ReturnsSanitizedPath()
    {
        string path = @"C:\\Some\\Path\\file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\file.txt", result);
    }

    [TestMethod]
    public void FormatPath_PathWithAltDoubledSeparators_ReturnsSanitizedPath()
    {
        string path = @"C://Some//Path//file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/file.txt", result);
    }

    [TestMethod]
    public void ChunkID_EmptyInput_ThrowsArgumentEmptyException()
    {
        string chunkName = string.Empty;
        Assert.ThrowsException<ArgumentEmptyException>(() => chunkName.ChunkID());
    }

    [TestMethod]
    public void ChunkID_ShortInput_ReturnsCorrectChunkID()
    {
        string chunkName = "Short";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x0000000074726F6853UL, result);
    }

    [TestMethod]
    public void ChunkID_ExactLengthInput_ReturnsCorrectChunkID()
    {
        string chunkName = "ExactLen";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x6E654C7463617845UL, result);
    }

    [TestMethod]
    public void ChunkID_LongInput_ReturnsCorrectChunkID()
    {
        string chunkName = "TooLongInput";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x49676e6f4c6f6f54UL, result);
    }
}
