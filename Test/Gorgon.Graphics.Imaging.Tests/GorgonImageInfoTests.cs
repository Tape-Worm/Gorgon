namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonImageInfoTests
{
    [TestMethod]
    public void Create3DImageInfo()
    {
        GorgonImageInfo info = GorgonImageInfo.Create3DImageInfo(BufferFormat.R8G8B8A8_UNorm, 64, 64, 4);

        Assert.AreEqual(ImageDataType.Image3D, info.ImageType);
        Assert.AreEqual(4, info.Depth);
        Assert.AreEqual(1, info.ArrayCount);
    }
}
