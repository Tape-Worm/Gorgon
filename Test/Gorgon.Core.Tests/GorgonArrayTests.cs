using System;
using System.Diagnostics;
using System.Linq;
using Gorgon.Collections;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonArrayTests
{
    private readonly string[] _testData =
    [
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString()
    ];

    [TestMethod]
    public void Creation()
    {
        GorgonArray<string?> array = new(32);

        Assert.AreEqual(32, array.Length);

        array = new GorgonArray<string?>(_testData);
        Assert.AreEqual(_testData.Length, array.Length);
        Assert.IsFalse(array.IsDirty);

        array = new GorgonArray<string?>(_testData, true);
        Assert.AreEqual(_testData.Length, array.Length);
        Assert.IsTrue(array.IsDirty);
    }

    [TestMethod]
    public void IsDirty()
    {
        GorgonArray<string?> array = new(32);

        Assert.IsFalse(array.IsDirty);

        for (int i = 0; i < 10; ++i)
        {
            array[i] = Guid.NewGuid().ToString();
        }

        Assert.IsTrue(array.IsDirty);
    }

    [TestMethod]
    public void IsIndexDirty()
    {
        GorgonArray<string?> array = new(_testData);

        Assert.IsFalse(array.IsDirty);

        array[3] = "This is not a GUID!";

        Assert.IsTrue(array.IsDirty);
    }

    [TestMethod]
    public void Clear()
    {
        GorgonArray<string?> array = new(_testData);

        array.Clear();

        Assert.AreEqual(_testData.Length, array.Length);
        Assert.IsTrue(array.All(item => item is null));
    }

    [TestMethod]
    public void RemoveAt()
    {
        GorgonArray<string?> array = new(_testData);

        array.RemoveAt(3);

        Assert.IsNull(array[3]);
        Assert.IsTrue(array.IsDirty);
        Assert.IsTrue(array.IsIndexDirty(3));
    }

    [TestMethod]
    public void GetDirtyStartIndexAndCount()
    {
        GorgonArray<string?> array = new(_testData);

        array[1] = Guid.NewGuid().ToString();
        array[2] = Guid.NewGuid().ToString();
        array[6] = Guid.NewGuid().ToString();

        Assert.IsTrue(array.IsDirty);

        (int start, int count) = array.GetDirtyStartIndexAndCount(true);

        Assert.AreNotEqual(_testData[start], array[start]);
        Assert.AreNotEqual(_testData[start + 1], array[start + 1]);
        Assert.AreNotEqual(_testData[start + count - 1], array[start + count - 1]);

        (start, count) = array.GetDirtyStartIndexAndCount();

        Assert.IsFalse(array.IsDirty);
        Assert.IsFalse(array.IsIndexDirty(1));
        Assert.IsFalse(array.IsIndexDirty(2));
        Assert.IsFalse(array.IsIndexDirty(6));

        Assert.AreNotEqual(_testData[start], array[start]);
        Assert.AreNotEqual(_testData[start + 1], array[start + 1]);
        Assert.AreNotEqual(_testData[start + count - 1], array[start + count - 1]);
    }

    [TestMethod]
    public void GetDirtySpan()
    {
        GorgonArray<string?> array = new(_testData);

        array[1] = Guid.NewGuid().ToString();
        array[2] = Guid.NewGuid().ToString();
        array[6] = Guid.NewGuid().ToString();

        Assert.AreNotEqual(_testData[1], array[1]);
        Assert.AreNotEqual(_testData[2], array[2]);
        Assert.AreNotEqual(_testData[6], array[6]);

        ReadOnlySpan<string?> range = array.GetDirtySpan(true);

        for (int i = 0; i < 5; ++i)
        {
            Assert.AreEqual(array[i + 1], range[i]);
        }

        Assert.IsTrue(array.IsDirty);

        range = array.GetDirtySpan();

        for (int i = 0; i < 5; ++i)
        {
            Assert.AreEqual(array[i + 1], range[i]);
        }

        Assert.IsFalse(array.IsDirty);
        Assert.IsFalse(array.IsIndexDirty(1));
        Assert.IsFalse(array.IsIndexDirty(2));
        Assert.IsFalse(array.IsIndexDirty(6));
    }

    [TestMethod]
    public void MarkDirty()
    {
        GorgonArray<string?> array = new(_testData);
        array.MarkDirty(2);

        Assert.IsTrue(array.IsDirty);
        Assert.IsTrue(array.IsIndexDirty(2));

        ReadOnlySpan<string?> range = array.GetDirtySpan();

        Assert.AreEqual(array[2], range[0]);
        Assert.AreEqual(1, range.Length);

        Assert.IsFalse(array.IsDirty);

        array.MarkDirty(2..5);

        Assert.IsTrue(array.IsDirty);
        Assert.IsTrue(array.IsIndexDirty(2));
        Assert.IsTrue(array.IsIndexDirty(3));
        Assert.IsTrue(array.IsIndexDirty(4));
    }

    [TestMethod]
    public void MarkClean()
    {
        GorgonArray<string?> array = new(_testData, true);
        array.MarkClean(2);

        Assert.IsFalse(array.IsIndexDirty(2));

        array.MarkClean(2..5);

        Assert.IsFalse(array.IsIndexDirty(2));
        Assert.IsFalse(array.IsIndexDirty(3));
        Assert.IsFalse(array.IsIndexDirty(4));
    }

    [TestMethod]
    public void CopyTo()
    {
        GorgonArray<string?> array = new(_testData);
        GorgonArray<string?> destArray = new(4);

        array.CopyTo(destArray);

        Assert.IsTrue(destArray.IsDirty);

        destArray.Clear();

        array.CopyTo(destArray, 2);

        Assert.IsTrue(destArray.IsDirty);
        Assert.IsFalse(destArray.IsIndexDirty(0));
        Assert.IsFalse(destArray.IsIndexDirty(1));
        Assert.IsTrue(destArray.IsIndexDirty(2));
        Assert.IsTrue(destArray.IsIndexDirty(3));

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.CopyTo(destArray, -4));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.CopyTo(destArray, 6));
    }

    [TestMethod]
    public void ToGorgonArray()
    {
        int[] list = [5, 6, 10, 11, 15];
        GorgonArray<int> dirty = list.ToGorgonArray(true);
        GorgonArray<int> clean = list.ToGorgonArray();

        Assert.IsTrue(dirty.SequenceEqual(list));
        Assert.IsTrue(clean.SequenceEqual(list));

        Assert.IsTrue(dirty.IsDirty);
        Assert.IsFalse(clean.IsDirty);
    }

    [TestMethod]
    public void CopyDirty()
    {
        GorgonArray<string?> array = new(_testData);
        GorgonArray<string?> destArray = new(6);

        array.MarkDirty(3);
        array.MarkDirty(4);

        array.CopyDirty(destArray);

        Assert.IsTrue(destArray.IsDirty);
        Assert.AreNotEqual(array[0], destArray[0]);
        Assert.AreNotEqual(array[5], destArray[5]);
        Assert.AreEqual(array[3], destArray[3]);
        Assert.AreEqual(array[4], destArray[4]);
    }

    [TestMethod]
    public void SelectDirty()
    {
        GorgonArray<string?> array = new(_testData);

        array.MarkDirty(3);
        array.MarkDirty(4);
        array.MarkDirty(7);

        string?[] dirty = array.SelectDirty().ToArray();

        Assert.AreEqual(array[3], dirty[0]);
        Assert.AreEqual(array[4], dirty[1]);
        Assert.AreEqual(array[7], dirty[2]);
    }

    [TestMethod]
    public void SelectClean()
    {
        GorgonArray<string?> array = new(_testData, true);

        array.MarkClean(3);
        array.MarkClean(4);
        array.MarkClean(7);

        string?[] dirty = array.SelectClean().ToArray();

        Assert.AreEqual(array[3], dirty[0]);
        Assert.AreEqual(array[4], dirty[1]);
        Assert.AreEqual(array[7], dirty[2]);
    }

    [TestMethod]
    public void RangeTest()
    {
        GorgonArray<string?> array = new(_testData, false);

        array.MarkDirty(3..);

        for (int i = 3; i < array.Length; ++i)
        {
            Assert.IsTrue(array.IsIndexDirty(i));
        }

        array.MarkClean(..^2);

        for (int i = 0; i < array.Length - 2; ++i)
        {
            Assert.IsFalse(array.IsIndexDirty(i));
        }

        ReadOnlySpan<string?> spanRange = array[2..6];

        for (int i = 0; i < spanRange.Length; ++i)
        {
            Assert.AreEqual(array[i + 2], spanRange[i]);
        }
    }
}
