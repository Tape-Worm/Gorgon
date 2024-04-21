using System.Text.Json;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonPointTests
{
    [TestMethod]
    public void EqualsReturnsTrueForEqualPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(10, 20);

        // Act
        bool result = point1.Equals(point2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EqualsReturnsFalseForDifferentPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(20, 10);

        // Act
        bool result = point1.Equals(point2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void EqualsReturnsFalseForDifferentType()
    {
        // Arrange
        GorgonPoint point = new(10, 20);
        object otherObject = new();

        // Act
        bool result = point.Equals(otherObject);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetHashCodeReturnsSameHashCodeForEqualPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(10, 20);

        // Act
        int hashCode1 = point1.GetHashCode();
        int hashCode2 = point2.GetHashCode();

        // Assert
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void GetHashCodeReturnsDifferentHashCodeForDifferentPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(20, 10);

        // Act
        int hashCode1 = point1.GetHashCode();
        int hashCode2 = point2.GetHashCode();

        // Assert
        Assert.AreNotEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void RoundReturnsRoundedPoint()
    {
        // Arrange
        System.Numerics.Vector2 vector = new(10.5f, 20.5f);

        // Act
        GorgonPoint result = GorgonPoint.Round(vector);

        // Assert
        Assert.AreEqual(11, result.X);
        Assert.AreEqual(21, result.Y);
    }

    [TestMethod]
    public void CeilingReturnsCeilingPoint()
    {
        // Arrange
        System.Numerics.Vector2 vector = new(10.1f, 20.9f);

        // Act
        GorgonPoint result = GorgonPoint.Ceiling(vector);

        // Assert
        Assert.AreEqual(11, result.X);
        Assert.AreEqual(21, result.Y);
    }

    [TestMethod]
    public void FloorReturnsFloorPoint()
    {
        // Arrange
        System.Numerics.Vector2 vector = new(10.9f, 20.1f);

        // Act
        GorgonPoint result = GorgonPoint.Floor(vector);

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void AddReturnsSumOfPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(5, 10);

        // Act
        GorgonPoint result = GorgonPoint.Add(point1, point2);

        // Assert
        Assert.AreEqual(15, result.X);
        Assert.AreEqual(30, result.Y);
    }

    [TestMethod]
    public void SubtractReturnsDifferenceOfPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(5, 10);

        // Act
        GorgonPoint result = GorgonPoint.Subtract(point1, point2);

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void MultiplyReturnsProductOfPointAndScalar()
    {
        // Arrange
        GorgonPoint point = new(10, 20);
        int scalar = 2;

        // Act
        GorgonPoint result = GorgonPoint.Multiply(point, scalar);

        // Assert
        Assert.AreEqual(20, result.X);
        Assert.AreEqual(40, result.Y);
    }

    [TestMethod]
    public void DivideReturnsQuotientOfPointAndScalar()
    {
        // Arrange
        GorgonPoint point = new(10, 20);
        int scalar = 2;

        // Act
        GorgonPoint result = GorgonPoint.Divide(point, scalar);

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToVector2ReturnsVector2()
    {
        // Arrange
        GorgonPoint point = new(10, 20);

        // Act
        System.Numerics.Vector2 result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromVector2ReturnsGorgonPoint()
    {
        // Arrange
        System.Numerics.Vector2 vector = new(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)vector;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToPointReturnsPoint()
    {
        // Arrange
        GorgonPoint point = new(10, 20);

        // Act
        System.Drawing.Point result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToPointFReturnsPointF()
    {
        // Arrange
        GorgonPoint point = new(10, 20);

        // Act
        System.Drawing.PointF result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromPointReturnsGorgonPoint()
    {
        // Arrange
        System.Drawing.Point point = new(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromPointFReturnsGorgonPoint()
    {
        // Arrange
        System.Drawing.PointF point = new(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToSizeReturnsSize()
    {
        // Arrange
        GorgonPoint point = new(10, 20);

        // Act
        System.Drawing.Size result = point;

        // Assert
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(20, result.Height);
    }

    [TestMethod]
    public void ImplicitConversionToSizeFReturnsSizeF()
    {
        // Arrange
        GorgonPoint point = new(10, 20);

        // Act
        System.Drawing.SizeF result = point;

        // Assert
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(20, result.Height);
    }

    [TestMethod]
    public void ExplicitConversionFromSizeReturnsGorgonPoint()
    {
        // Arrange
        System.Drawing.Size size = new(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)size;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromSizeFReturnsGorgonPoint()
    {
        // Arrange
        System.Drawing.SizeF size = new(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)size;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void AdditionOperatorReturnsSumOfPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(5, 10);

        // Act
        GorgonPoint result = point1 + point2;

        // Assert
        Assert.AreEqual(15, result.X);
        Assert.AreEqual(30, result.Y);
    }

    [TestMethod]
    public void SubtractionOperatorReturnsDifferenceOfPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(5, 10);

        // Act
        GorgonPoint result = point1 - point2;

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void MultiplicationOperatorReturnsProductOfPointAndScalar()
    {
        // Arrange
        GorgonPoint point = new(10, 20);
        int scalar = 2;

        // Act
        GorgonPoint result = point * scalar;

        // Assert
        Assert.AreEqual(20, result.X);
        Assert.AreEqual(40, result.Y);
    }

    [TestMethod]
    public void DivisionOperatorReturnsQuotientOfPointAndScalar()
    {
        // Arrange
        GorgonPoint point = new(10, 20);
        int scalar = 2;

        // Act
        GorgonPoint result = point / scalar;

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void EqualityOperatorReturnsTrueForEqualPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(10, 20);

        // Act
        bool result = point1 == point2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EqualityOperatorReturnsFalseForDifferentPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(20, 10);

        // Act
        bool result = point1 == point2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void InequalityOperatorReturnsTrueForDifferentPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(20, 10);

        // Act
        bool result = point1 != point2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void InequalityOperatorReturnsFalseForEqualPoints()
    {
        // Arrange
        GorgonPoint point1 = new(10, 20);
        GorgonPoint point2 = new(10, 20);

        // Act
        bool result = point1 != point2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Serialization()
    {
        string expectedString = "{\"x\":10,\"y\":20}";
        GorgonPoint point = new(10, 20);

        string actualString = JsonSerializer.Serialize(point);

        Assert.AreEqual(expectedString, actualString);

        GorgonPoint deserializedPoint = JsonSerializer.Deserialize<GorgonPoint>(actualString);

        Assert.AreEqual(point, deserializedPoint);
    }
}
