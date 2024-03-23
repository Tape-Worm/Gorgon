using System;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonPitchLayoutTests
{
    [TestMethod]
    public void Ctor()
    {
        GorgonPitchLayout layout = new();

        Assert.AreEqual(layout.SlicePitch, 0);
        Assert.AreEqual(layout.RowPitch, 0);
        Assert.AreEqual(layout.HorizontalBlockCount, 0);
        Assert.AreEqual(layout.VerticalBlockCount, 0);

        layout = new GorgonPitchLayout(320, 64000);

        Assert.AreEqual(layout.SlicePitch, 64000);
        Assert.AreEqual(layout.RowPitch, 320);
        Assert.AreEqual(layout.HorizontalBlockCount, 0);
        Assert.AreEqual(layout.VerticalBlockCount, 0);

        layout = new GorgonPitchLayout(320, 64000, 80, 50);

        Assert.AreEqual(layout.SlicePitch, 64000);
        Assert.AreEqual(layout.RowPitch, 320);
        Assert.AreEqual(layout.HorizontalBlockCount, 80);
        Assert.AreEqual(layout.VerticalBlockCount, 50);

        Assert.ThrowsException<ArgumentException>(() => new GorgonPitchLayout(320, 200, 80, 0));
        Assert.ThrowsException<ArgumentException>(() => new GorgonPitchLayout(320, 200, 0, 50));
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
        Assert.IsTrue(layout == layout2);
        Assert.IsTrue(layout != layout3);
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
