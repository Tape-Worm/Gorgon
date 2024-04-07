using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Memory;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonPoolTests
{
    private class DisposableObject
        : IDisposable
    {
        public string Text
        {
            get;
            set;
        } = string.Empty;

        public bool IsDisposed
        {
            get;
            private set;
        }

        public void Dispose() => IsDisposed = true;
    }

    [TestMethod]
    public void ShouldThrowWhenMaxObjectCountLessThanOne()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonPool<string>(0, () => Guid.NewGuid().ToString()));
    }

    [TestMethod]
    public void ShouldThrowWhenItemAllocatorReturnsNull()
    {
        GorgonPool<string> pool = new(1, () => null!);
        Assert.ThrowsException<GorgonException>(() => pool.Allocate());
    }

    [TestMethod]
    public void ShouldAllocateMultipleItems_WithInitializer()
    {
        GorgonPool<DisposableObject> pool = new(5, () => new());
        List<DisposableObject> items = new List<DisposableObject>();

        for (int i = 0; i < 5; i++)
        {
            items.Add(pool.Allocate(o => o.Text = $"Text {i}"));
        }

        Assert.AreEqual(5, items.Count);
        Assert.AreEqual(0, pool.AvailableSlots);

        for (int i = 0; i < 5; ++i)
        {
            DisposableObject o = items[i];

            Assert.AreEqual($"Text {i}", o.Text);
        }
    }

    [TestMethod]
    public void ShouldAllocateMultipleItems()
    {
        GorgonPool<string> pool = new(5, () => Guid.NewGuid().ToString());
        List<string> items = new List<string>();

        for (int i = 0; i < 5; i++)
        {
            items.Add(pool.Allocate());
        }

        Assert.AreEqual(5, items.Count);
        Assert.AreEqual(0, pool.AvailableSlots);
    }

    [TestMethod]
    public void ShouldThrowWhenPoolIsFull()
    {
        GorgonPool<string> pool = new(5, () => Guid.NewGuid().ToString());
        for (int i = 0; i < 5; i++)
        {
            pool.Allocate();
        }

        Assert.ThrowsException<GorgonException>(() => pool.Allocate());
    }

    [TestMethod]
    public void ShouldDeallocateMultipleItems()
    {
        GorgonPool<string> pool = new(5, () => Guid.NewGuid().ToString());
        List<string?> items = new List<string?>();

        for (int i = 0; i < 5; i++)
        {
            items.Add(pool.Allocate());
        }

        foreach (var item in items)
        {
            string? tempItem = item;
            pool.Deallocate(ref tempItem);
        }

        Assert.AreEqual(5, pool.AvailableSlots);
    }

    [TestMethod]
    public void ShouldDeallocateMultipleItems_Finalizer()
    {
        GorgonPool<DisposableObject> pool = new(5, () => new());
        List<DisposableObject> items = new List<DisposableObject>();

        for (int i = 0; i < 5; i++)
        {
            items.Add(pool.Allocate());
        }

        foreach (DisposableObject item in items)
        {
            DisposableObject? tempItem = item;
            pool.Deallocate(ref tempItem, poolItem => poolItem.Dispose());

            Assert.IsNull(tempItem);
        }

        Assert.AreEqual(5, pool.AvailableSlots);

        foreach (DisposableObject item in items)
        {       
            Assert.IsTrue(item.IsDisposed);
        }
    }
}
