using Gorgon.IO;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonChunkTests
{
    [TestMethod]
    public void GorgonChunk_Constructor_ShouldSetPropertiesCorrectly()
    {
        GorgonChunk chunk = new("CHNKID01".ChunkID(), 456, 789);

        Assert.AreEqual("CHNKID01".ChunkID(), chunk.ID);
        Assert.AreEqual(456, chunk.Size);
        Assert.AreEqual(789UL, chunk.FileOffset);
    }

    [TestMethod]
    public void GorgonChunk_Equals_ShouldReturnTrueForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsTrue(GorgonChunk.Equals(ref chunk1, ref chunk2));
    }

    [TestMethod]
    public void GorgonChunk_Equals_ShouldReturnFalseForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsFalse(GorgonChunk.Equals(ref chunk1, ref chunk2));
    }

    [TestMethod]
    public void GorgonChunk_EqualityOperator_ShouldReturnTrueForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsTrue(chunk1 == chunk2);
    }

    [TestMethod]
    public void GorgonChunk_EqualityOperator_ShouldReturnFalseForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsFalse(chunk1 == chunk2);
    }

    [TestMethod]
    public void GorgonChunk_InequalityOperator_ShouldReturnTrueForUnequalChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID02".ChunkID(), 654, 987);

        Assert.IsTrue(chunk1 != chunk2);
    }

    [TestMethod]
    public void GorgonChunk_InequalityOperator_ShouldReturnFalseForEqualChunks()
    {
        GorgonChunk chunk1 = new("CHNKID01".ChunkID(), 456, 789);
        GorgonChunk chunk2 = new("CHNKID01".ChunkID(), 456, 789);

        Assert.IsFalse(chunk1 != chunk2);
    }
}
