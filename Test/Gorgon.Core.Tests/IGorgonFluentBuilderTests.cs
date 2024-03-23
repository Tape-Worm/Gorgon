using System;
using System.Collections.Generic;
using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Core.Tests;

public class TestClass
{
    public int Value
    {
        get;
        set;
    }

    public TestClass(int value) => Value = value;
}

public class DumbAllocator
    : IGorgonAllocator<TestClass>
{
    private readonly List<TestClass> _objects = new();

    public int Count => _objects.Count;

    public TestClass Allocate(Action<TestClass>? initializer = null)
    {
        TestClass result = new(_objects.Count);
        initializer?.Invoke(result);
        _objects.Add(result);

        return result;
    }
}

public class TestFluentBuilder
    : IGorgonFluentBuilder<TestFluentBuilder, TestClass>
{
    private int _value;

    public TestFluentBuilder SetValue(int value)
    {
        _value = value;
        return this;
    }

    public TestClass Build()
    {
        TestClass result = new(_value);

        return result;
    }

    public TestFluentBuilder Clear()
    {
        _value = 0;
        return this;
    }

    public TestFluentBuilder ResetTo(TestClass builderObject)
    {
        _value = builderObject.Value;
        return this;
    }

    public TestClass Build<TBa>(TBa allocator)
        where TBa : class, IGorgonAllocator<TestClass>
    {
        TestClass result = allocator.Allocate();
        result.Value = _value;
        return result;
    }
}

[TestClass]
public class IGorgonFluentBuilderTests
{
    [TestMethod]
    public void Build()
    {
        DumbAllocator allocator = new();
        TestFluentBuilder builder = new();

        TestClass obj = builder.SetValue(10)
                               .Build();

        Assert.AreEqual(10, obj.Value);

        obj = builder.SetValue(1024)
                     .Build<DumbAllocator>(allocator);

        obj = builder.SetValue(2048)
                    .Build<DumbAllocator>(allocator);

        obj = builder.SetValue(3072)
                     .Build<DumbAllocator>(allocator);

        Assert.AreEqual(3, allocator.Count);
        Assert.AreEqual(3072, obj.Value);
    }

    [TestMethod]
    public void Clear()
    {
        TestFluentBuilder builder = new();

        TestClass obj = builder.SetValue(10)
                               .Clear()
                               .Build();

        Assert.AreEqual(0, obj.Value);
    }

    [TestMethod]
    public void ResetTo()
    {
        TestFluentBuilder builder = new();
        TestClass expected = new(25);

        TestClass obj = builder.SetValue(10)
                               .ResetTo(expected)
                               .Build();

        Assert.AreEqual(expected.Value, obj.Value);
    }
}
