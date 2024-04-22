using System;
using System.Collections;
using System.Collections.Generic;
using Moq;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonNamedObjectTests
{
    private sealed class EnumerableType(List<IGorgonNamedObject> source)
        : IEnumerable<IGorgonNamedObject>
    {
        public IEnumerator<IGorgonNamedObject> GetEnumerator()
        {
            foreach (IGorgonNamedObject item in source)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [TestMethod]
    public void CreateNamedObjects()
    {
        string name = Guid.NewGuid().ToString();
        IGorgonNamedObject item1 = Mock.Of<IGorgonNamedObject>(x => x.Name == name);

        Assert.AreEqual(name, item1.Name, false);
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
        Justification = "MOQ does not work with String.Equals")]
    public void LinqExtensions()
    {
        IGorgonNamedObject item1 = Mock.Of<IGorgonNamedObject>(x => x.Name == Guid.NewGuid().ToString().ToUpper());
        IGorgonNamedObject item2 = Mock.Of<IGorgonNamedObject>(x => x.Name == Guid.NewGuid().ToString());
        IGorgonNamedObject item3 = Mock.Of<IGorgonNamedObject>(x => x.Name == Guid.NewGuid().ToString().ToUpper());
        IGorgonNamedObject item4 = Mock.Of<IGorgonNamedObject>(x => x.Name == Guid.NewGuid().ToString());

        List<IGorgonNamedObject> objects = [item1, item2, item3, item4];
        EnumerableType enumObjects = new(objects);

        Assert.IsTrue(objects.ContainsName(item1.Name));
        Assert.IsFalse(objects.ContainsName(item1.Name.ToLower(), StringComparer.Ordinal));
        Assert.AreNotEqual(objects.IndexOfName(item3.Name), -1);
        Assert.AreEqual(objects.IndexOfName(item2.Name, StringComparer.Ordinal), 1);
        Assert.AreEqual(objects.IndexOfName(Guid.Empty.ToString(), StringComparer.Ordinal), -1);

        IGorgonNamedObject itemFinal = objects.GetByName(item4.Name);

        Assert.AreEqual(item4, itemFinal);

        Assert.ThrowsException<KeyNotFoundException>(() => objects.GetByName(item3.Name.ToLower(), StringComparer.InvariantCulture));
        Assert.ThrowsException<KeyNotFoundException>(() => objects.GetByName(Guid.Empty.ToString()));

        Assert.IsTrue(enumObjects.ContainsName(item1.Name));
        Assert.IsFalse(enumObjects.ContainsName(item1.Name.ToLower(), StringComparer.Ordinal));
        Assert.AreNotEqual(enumObjects.IndexOfName(item3.Name), -1);
        Assert.AreEqual(enumObjects.IndexOfName(item2.Name, StringComparer.Ordinal), 1);
        Assert.AreEqual(enumObjects.IndexOfName(Guid.Empty.ToString(), StringComparer.Ordinal), -1);

        itemFinal = enumObjects.GetByName(item4.Name);

        Assert.AreEqual(item4, itemFinal);

        Assert.ThrowsException<KeyNotFoundException>(() => enumObjects.GetByName(item3.Name.ToLower(), StringComparer.InvariantCulture));
        Assert.ThrowsException<KeyNotFoundException>(() => enumObjects.GetByName(Guid.Empty.ToString()));
    }
}