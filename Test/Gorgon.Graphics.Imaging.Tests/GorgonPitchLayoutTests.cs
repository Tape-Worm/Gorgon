using System;

namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class GorgonPitchLayoutTests
{
    [TestMethod]
    public void Ctor()
    {
        GorgonPitchLayout layout = new();

        Assert.AreEqual(0, layout.SlicePitch);
        Assert.AreEqual(0, layout.RowPitch);
        Assert.AreEqual(0, layout.HorizontalBlockCount);
        Assert.AreEqual(0, layout.VerticalBlockCount);

        layout = new GorgonPitchLayout(320, 64000);

        Assert.AreEqual(64000, layout.SlicePitch);
        Assert.AreEqual(320, layout.RowPitch);
        Assert.AreEqual(0, layout.HorizontalBlockCount);
        Assert.AreEqual(0, layout.VerticalBlockCount);

        layout = new GorgonPitchLayout(320, 64000, 80, 50);

        Assert.AreEqual(64000, layout.SlicePitch);
        Assert.AreEqual(320, layout.RowPitch);
        Assert.AreEqual(80, layout.HorizontalBlockCount);
        Assert.AreEqual(50, layout.VerticalBlockCount);

        Assert.ThrowsExactly<ArgumentException>(() => _ = new GorgonPitchLayout(320, 200, 80, 0));
        Assert.ThrowsExactly<ArgumentException>(() => _ = new GorgonPitchLayout(320, 200, 0, 50));
    }

    [TestMethod]
    public void Operators()
    {
        GorgonPitchLayout layout = new(320, 64000);
        GorgonPitchLayout layout2 = new(320, 64000);
        GorgonPitchLayout layout3 = new(640, 128000);

        Assert.IsTrue(GorgonPitchLayout.Equals(layout, layout2));
        Assert.IsFalse(GorgonPitchLayout.Equals(layout, layout3));
        Assert.IsTrue(layout.Equals(layout2));
        Assert.IsFalse(layout.Equals(layout3));
        Assert.AreEqual(layout2, layout);
        Assert.AreNotEqual(layout3, layout);
    }

    [TestMethod]
    public void Properties()
    {
        GorgonPitchLayout layout = new(320, 64000, 80, 50);

        Assert.AreEqual(320, layout.RowPitch);
        Assert.AreEqual(64000, layout.SlicePitch);
        Assert.AreEqual(80, layout.HorizontalBlockCount);
        Assert.AreEqual(50, layout.VerticalBlockCount);

        Assert.AreEqual(16, GorgonPitchLayout.SizeInBytes);
        Assert.AreEqual(GorgonPitchLayout.Empty, new GorgonPitchLayout());
    }
}
