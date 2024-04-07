using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Memory;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonLinearPoolTests
{
    [TestMethod]
    public void ShouldThrowWhenMaxObjectCountLessThanOne()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonLinearPool<string>(0, () => "Test"));
    }

    [TestMethod]
    public void ShouldThrowWhenPoolIsFull()
    {
        const int poolSize = 3;
        GorgonLinearPool<string> pool = new(poolSize, () => "Test");
        for (int i = 0; i < poolSize; i++)
        {
            pool.Allocate();
        }
        // Now the pool is full, the next allocation should throw an exception.
        Assert.ThrowsException<GorgonException>(() => pool.Allocate());
    }

    [TestMethod]
    public void ShouldRecycleWhenFullAndReset()
    {
        const int poolSize = 3;
        List<DisposableObject> list = new();
        GorgonLinearPool<DisposableObject> pool = new(poolSize, () => new DisposableObject());
        for (int i = 0; i < poolSize; i++)
        {
            list.Add(pool.Allocate());
        }

        pool.Reset();

        DisposableObject obj = pool.Allocate();

        // Now the pool is full, the next allocation should throw an exception.
        Assert.AreEqual(list[0], obj);
    }

    [TestMethod]
    public void ShouldResetPool()
    {
        GorgonLinearPool<string> pool = new(1, () => "Test");
        string item = pool.Allocate();
        pool.Reset();
        string newItem = pool.Allocate();
        Assert.AreSame(item, newItem);
    }

    [TestMethod]
    public void ShouldCleanupOnReset()
    {
        GorgonLinearPool<DisposableObject> pool = new(1, () => new DisposableObject());
        DisposableObject item = pool.Allocate();
        pool.Reset(poolItem => poolItem.Dispose());
        Assert.IsTrue(item.IsDisposed);
    }

    [TestMethod]
    public void ShouldAllocateMultipleObjects()
    {
        const int poolSize = 5;
        GorgonLinearPool<string> pool = new(poolSize, () => Guid.NewGuid().ToString());

        List<string> allocatedItems = new();
        for (int i = 0; i < poolSize; i++)
        {
            allocatedItems.Add(pool.Allocate());
        }

        // Ensure all allocated items are unique.
        Assert.AreEqual(allocatedItems.Distinct().Count(), poolSize);
    }

    [TestMethod]
    public void ShouldAllocateMultipleItems_WithInitializer()
    {
        GorgonLinearPool<DisposableObject> pool = new(5, () => new());
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
    public void ShouldThrowWhenItemAllocatorReturnsNull()
    {
        GorgonLinearPool<string> pool = new(1, () => null!);
        Assert.ThrowsException<GorgonException>(() => pool.Allocate());
    }

    private class DisposableObject : IDisposable
    {
        public string Text
        {
            get;
            set;
        } = string.Empty;

        public bool IsDisposed
        {
            get; private set;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

}
