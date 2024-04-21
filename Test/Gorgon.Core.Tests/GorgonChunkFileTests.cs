using System.IO;
using Gorgon.IO;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonChunkFileTests
{
    [TestMethod]
    public void ShouldThrowExceptionWhenStreamIsAtEnd()
    {
        using MemoryStream stream = new();

        using GorgonChunkFileWriter writer = new(stream, 0x12345678);
        writer.Open();

        using IGorgonChunkWriter chunk = writer.OpenChunk(0x666);
        chunk.WriteString("Test 1 2 3");
        chunk.Close();
        writer.Close();

        Assert.ThrowsException<EndOfStreamException>(() => new GorgonChunkFileReader(stream, new ulong[] { 0x12345678 }));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenAppSpecificIdsIsEmpty()
    {
        using MemoryStream stream = new(new byte[100]);
        Assert.ThrowsException<ArgumentEmptyException>(() => new GorgonChunkFileReader(stream, []));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenOpeningNonExistentChunk()
    {
        using MemoryStream stream = new();

        using GorgonChunkFileWriter writer = new(stream, 0x12345678);
        writer.Open();

        using IGorgonChunkWriter chunk = writer.OpenChunk(0x666);

        chunk.WriteString("Test 1 2 3");
        chunk.Close();
        writer.Close();

        stream.Position = 0;

        using GorgonChunkFileReader reader = new(stream, new ulong[] { 0x12345679 });
        Assert.ThrowsException<GorgonException>(() => reader.Open());
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenOpeningChunkFileWithInvalidHeader()
    {
        using MemoryStream stream = new();

        using GorgonChunkFileWriter writer = new(stream, 0x12348765);

        writer.Open();
        using IGorgonChunkWriter chunk = writer.OpenChunk(0x666);
        chunk.WriteString("Test 1 2 3");
        chunk.Close();
        writer.Close();

        stream.Position = 0;

        Assert.ThrowsException<GorgonException>(() =>
        {
            using GorgonChunkFileReader reader = new(stream, new ulong[] { 0x12345679 });
            reader.Open();
        });
    }

    [TestMethod]
    public void OpenChunkShouldThrowExceptionWhenFileNotOpen()
    {
        using MemoryStream stream = new();
        using GorgonChunkFileWriter writer = new(stream, 0xBAADBEEFBAADF00D);
        using GorgonChunkFileWriter reader = new(stream, 0xBAADBEEFBAADF00D);

        Assert.ThrowsException<IOException>(() => writer.OpenChunk("TESTCHNK"));
        Assert.ThrowsException<IOException>(() => reader.OpenChunk("TESTCHNK"));
    }

    [TestMethod]
    public void OpenChunkMultipleChunksInOrder()
    {
        using MemoryStream stream = new();
        using GorgonChunkFileWriter writer = new(stream, 0xBAADBEEFBAADF00D);

        writer.Open();

        using IGorgonChunkWriter chunk1 = writer.OpenChunk(1111);
        chunk1.WriteInt32(42);
        chunk1.Close();

        using IGorgonChunkWriter chunk2 = writer.OpenChunk(2222);
        chunk2.WriteString("Test 1 2 3");
        chunk2.Close();

        writer.Close();

        stream.Position = 0;

        using GorgonChunkFileReader reader = new(stream, [0xBAADBEEFBAADF00D]);
        reader.Open();

        using IGorgonChunkReader chunkReader1 = reader.OpenChunk(1111);
        Assert.AreEqual(42, chunkReader1.ReadInt32());
        chunkReader1.Close();

        using IGorgonChunkReader chunkReader2 = reader.OpenChunk(2222);
        Assert.AreEqual("Test 1 2 3", chunkReader2.ReadString());
        chunkReader2.Close();
    }

    [TestMethod]
    public void OpenChunkMultipleChunksOutOfOrder()
    {
        using MemoryStream stream = new();
        using GorgonChunkFileWriter writer = new(stream, 0xBAADBEEFBAADF00D);

        writer.Open();

        using IGorgonChunkWriter chunk1 = writer.OpenChunk(1111);
        chunk1.WriteInt32(42);
        chunk1.Close();

        using IGorgonChunkWriter chunk2 = writer.OpenChunk(2222);
        chunk2.WriteString("Test 1 2 3");
        chunk2.Close();

        writer.Close();

        stream.Position = 0;

        using GorgonChunkFileReader reader = new(stream, [0xBAADBEEFBAADF00D]);
        reader.Open();

        using IGorgonChunkReader chunkReader2 = reader.OpenChunk(2222);
        Assert.AreEqual("Test 1 2 3", chunkReader2.ReadString());
        chunkReader2.Close();

        using IGorgonChunkReader chunkReader1 = reader.OpenChunk(1111);
        Assert.AreEqual(42, chunkReader1.ReadInt32());
        chunkReader1.Close();
    }

    [TestMethod]
    public void OpenChunkShouldThrowExceptionWhenAnotherChunkIsOpen()
    {
        using MemoryStream stream = new();
        using GorgonChunkFileWriter writer = new(stream, 0xBAADBEEFBAADF00D);

        writer.Open();

        using IGorgonChunkWriter chunk1 = writer.OpenChunk("TESTCHNK");
        Assert.ThrowsException<IOException>(() => writer.OpenChunk(9999));

        chunk1.WriteInt32(42);
        chunk1.Close();

        writer.Close();

        stream.Position = 0;

        using GorgonChunkFileReader reader = new(stream, [0xBAADBEEFBAADF00D]);
        reader.Open();

        using IGorgonChunkReader chunkReader1 = reader.OpenChunk("TESTCHNK");
        Assert.ThrowsException<IOException>(() => reader.OpenChunk(9999));
    }
}
