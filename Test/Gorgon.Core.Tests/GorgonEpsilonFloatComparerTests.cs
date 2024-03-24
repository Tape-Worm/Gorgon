using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonEpsilonFloatComparerTests
{
    [TestMethod]
    public void Compare()
    {
        float left = 0.456787f;
        float right = 0.456786f;

        GorgonEpsilonFloatComparer comparer = new();
        Assert.AreEqual(0, comparer.Compare(left, right));

        comparer = new(1e-7f);
        Assert.AreNotEqual(0, comparer.Compare(left, right));
        Assert.AreEqual(-1, comparer.Compare(left, right));
        Assert.AreEqual(1, comparer.Compare(right, left));

        left = 0.45655f;
        right = 0.45016f;

        comparer = new(1e-3f);
        Assert.AreNotEqual(0, comparer.Compare(left, right));

        left = 0.45655f;
        right = 0.45616f;

        comparer = new(1e-3f);
        Assert.AreEqual(0, comparer.Compare(left, right));
    }
}
