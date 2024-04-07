using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Memory;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRingPoolTests
{
    [TestMethod]
    public void ShouldThrowWhenMaxObjectCountLessThanOne()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GorgonRingPool<string>(0, () => "Test"));
    }

    [TestMethod]
    public void ShouldWrapWhenFull()
    {
        const int poolSize = 3;
        GorgonRingPool<DisposableObject> pool = new(poolSize, () => new DisposableObject());
        List<DisposableObject> items = new();

        for (int i = 0; i < poolSize; i++)
        {
            items.Add(pool.Allocate());
        }

        DisposableObject obj = pool.Allocate(i => i.Text = "Test Text.");

        Assert.AreEqual(items[0], obj);
        Assert.AreEqual(items[0].Text, obj.Text);
    }

    [TestMethod]
    public void ShouldWrapAndNotifyWhenFull()
    {
        const int poolSize = 3;
        List<DisposableObject> items = new();
        GorgonRingPool<DisposableObject> pool = new(poolSize, () => new DisposableObject(), () => items.Add(new DisposableObject()));        

        for (int i = 0; i < poolSize; i++)
        {
            items.Add(pool.Allocate());
        }

        DisposableObject obj = pool.Allocate(i => i.Text = "Test Text.");

        Assert.AreEqual(items[0], obj);
        Assert.AreEqual(items[0].Text, obj.Text);
        Assert.AreEqual(4, items.Count);
    }

    [TestMethod]
    public void ShouldResetPool()
    {
        GorgonRingPool<string> pool = new(1, () => "Test");
        string item = pool.Allocate();
        pool.Reset();
        string newItem = pool.Allocate();
        Assert.AreSame(item, newItem);
    }

    [TestMethod]
    public void ShouldCleanupOnReset()
    {
        GorgonRingPool<DisposableObject> pool = new(1, () => new DisposableObject());
        DisposableObject item = pool.Allocate();
        pool.Reset(poolItem => poolItem.Dispose());
        Assert.IsTrue(item.IsDisposed);
    }

    [TestMethod]
    public void ShouldAllocateMultipleObjects()
    {
        const int poolSize = 5;
        GorgonRingPool<string> pool = new(poolSize, () => Guid.NewGuid().ToString());

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
        GorgonRingPool<DisposableObject> pool = new(5, () => new());
        List<DisposableObject> items = new List<DisposableObject>();

        for (int i = 0; i < 5; i++)
        {
            items.Add(pool.Allocate(o => o.Text = $"Text {i}"));
        }

        for (int i = 0; i < 5; ++i)
        {
            DisposableObject o = items[i];

            Assert.AreEqual($"Text {i}", o.Text);
        }
    }

    [TestMethod]
    public void ShouldThrowWhenItemAllocatorReturnsNull()
    {
        GorgonRingPool<string> pool = new(1, () => null!);
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
