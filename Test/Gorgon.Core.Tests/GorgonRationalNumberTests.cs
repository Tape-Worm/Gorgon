using Gorgon.Math;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRationalNumberTests
{
    [TestMethod]
    public void TestEquals()
    {
        // Arrange
        GorgonRationalNumber rational1 = new(1, 2);
        GorgonRationalNumber rational2 = new(1, 2);
        GorgonRationalNumber rational3 = new(2, 3);

        // Act
        bool result1 = rational1.Equals(rational2);
        bool result2 = rational1.Equals(rational3);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void TestCompareTo()
    {
        // Arrange
        GorgonRationalNumber rational1 = new(1, 2);
        GorgonRationalNumber rational2 = new(2, 3);
        GorgonRationalNumber rational3 = new(3, 4);

        // Act
        int result1 = rational1.CompareTo(rational2);
        int result2 = rational2.CompareTo(rational3);
        int result3 = rational3.CompareTo(rational1);

        // Assert
        Assert.AreEqual(-1, result1);
        Assert.AreEqual(-1, result2);
        Assert.AreEqual(1, result3);
    }

    [TestMethod]
    public void TestOperators()
    {
        // Arrange
        GorgonRationalNumber rational1 = new(1, 2);
        GorgonRationalNumber rational2 = new(2, 3);
        GorgonRationalNumber rational3 = new(3, 4);

        // Act
        bool result1 = rational1 == rational2;
        bool result2 = rational1 != rational2;
        bool result3 = rational2 < rational3;
        bool result4 = rational2 <= rational3;
        bool result5 = rational3 > rational1;
        bool result6 = rational3 >= rational1;

        // Assert
        Assert.IsFalse(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
        Assert.IsTrue(result4);
        Assert.IsTrue(result5);
        Assert.IsTrue(result6);
    }

    [TestMethod]
    public void TestConversions()
    {
        // Arrange
        GorgonRationalNumber rational = new(3, 2);

        // Act
        decimal decimalValue = rational;
        double doubleValue = (double)rational;
        float floatValue = (float)rational;
        int intValue = (int)rational;

        // Assert
        Assert.AreEqual(1.5m, decimalValue);
        Assert.AreEqual(1.5, doubleValue);
        Assert.AreEqual(1.5f, floatValue);
        Assert.AreEqual(1, intValue);
    }
}
