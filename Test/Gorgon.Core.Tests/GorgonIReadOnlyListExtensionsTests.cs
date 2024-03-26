﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Collections;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonIReadOnlyListExtensionsTests
{
    [TestMethod]
    public void TestFindLastIndex()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };
        Predicate<int> predicate = x => x == 2;

        int result = list.FindLastIndex(predicate);

        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void TestFindIndex()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };
        Predicate<int> predicate = x => x == 2;

        int result = list.FindIndex(predicate);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void TestContains()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };

        bool result = list.Contains(3);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestIndexOf()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };

        int result = list.IndexOf(3);

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void TestCopyTo()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };
        int[] array = new int[5];

        list.CopyTo(array);

        CollectionAssert.AreEqual(list.ToArray(), array);
    }

    [TestMethod]
    public void TestCopyTo_ArrayTooSmall()
    {
        IReadOnlyList<int> list = new List<int> { 1, 2, 3, 2, 1 };
        int[] array = new int[4];

        Assert.ThrowsException<ArgumentException>(() => list.CopyTo(array));
    }
}