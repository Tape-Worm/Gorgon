using Gorgon.IO;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonChunkTests
{
    [TestMethod]
    public void GorgonChunkConstructorShouldSetPropertiesCorrectly()
    {
        GorgonChunk chunk = new("CHNKID01".ChunkID(), 456, 789);

        Assert.AreEqual("CHNKID01".ChunkID(), chunk.ID);
        Assert.AreEqual(456, chunk.Size);
        Assert.AreEqual(789UL, chunk.FileOffset);
    }

    [TestMethod]
    public void GorgonChunkEqualsShouldReturnTrueForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsTrue(GorgonChunk.Equals(ref chunk1, ref chunk2));
    }

    [TestMethod]
    public void GorgonChunkEqualsShouldReturnFalseForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsFalse(GorgonChunk.Equals(ref chunk1, ref chunk2));
    }

    [TestMethod]
    public void GorgonChunkEqualityOperatorShouldReturnTrueForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsTrue(chunk1 == chunk2);
    }

    [TestMethod]
    public void GorgonChunkEqualityOperatorShouldReturnFalseForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsFalse(chunk1 == chunk2);
    }

    [TestMethod]
    public void GorgonChunkInequalityOperatorShouldReturnTrueForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsTrue(chunk1 != chunk2);
    }

    [TestMethod]
    public void GorgonChunkInequalityOperatorShouldReturnFalseForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsFalse(chunk1 != chunk2);
    }
}
