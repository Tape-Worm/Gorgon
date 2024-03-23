using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Newtonsoft.Json;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonPointTests
{
    [TestMethod]
    public void Equals_ReturnsTrueForEqualPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(10, 20);

        // Act
        var result = point1.Equals(point2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equals_ReturnsFalseForDifferentPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(20, 10);

        // Act
        var result = point1.Equals(point2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_ReturnsFalseForDifferentType()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);
        var otherObject = new object();

        // Act
        var result = point.Equals(otherObject);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetHashCode_ReturnsSameHashCodeForEqualPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(10, 20);

        // Act
        var hashCode1 = point1.GetHashCode();
        var hashCode2 = point2.GetHashCode();

        // Assert
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void GetHashCode_ReturnsDifferentHashCodeForDifferentPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(20, 10);

        // Act
        var hashCode1 = point1.GetHashCode();
        var hashCode2 = point2.GetHashCode();

        // Assert
        Assert.AreNotEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void Round_ReturnsRoundedPoint()
    {
        // Arrange
        var vector = new System.Numerics.Vector2(10.5f, 20.5f);

        // Act
        var result = GorgonPoint.Round(vector);

        // Assert
        Assert.AreEqual(11, result.X);
        Assert.AreEqual(21, result.Y);
    }

    [TestMethod]
    public void Ceiling_ReturnsCeilingPoint()
    {
        // Arrange
        var vector = new System.Numerics.Vector2(10.1f, 20.9f);

        // Act
        var result = GorgonPoint.Ceiling(vector);

        // Assert
        Assert.AreEqual(11, result.X);
        Assert.AreEqual(21, result.Y);
    }

    [TestMethod]
    public void Floor_ReturnsFloorPoint()
    {
        // Arrange
        var vector = new System.Numerics.Vector2(10.9f, 20.1f);

        // Act
        var result = GorgonPoint.Floor(vector);

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void Add_ReturnsSumOfPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(5, 10);

        // Act
        var result = GorgonPoint.Add(point1, point2);

        // Assert
        Assert.AreEqual(15, result.X);
        Assert.AreEqual(30, result.Y);
    }

    [TestMethod]
    public void Subtract_ReturnsDifferenceOfPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(5, 10);

        // Act
        var result = GorgonPoint.Subtract(point1, point2);

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void Multiply_ReturnsProductOfPointAndScalar()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);
        var scalar = 2;

        // Act
        var result = GorgonPoint.Multiply(point, scalar);

        // Assert
        Assert.AreEqual(20, result.X);
        Assert.AreEqual(40, result.Y);
    }

    [TestMethod]
    public void Divide_ReturnsQuotientOfPointAndScalar()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);
        var scalar = 2;

        // Act
        var result = GorgonPoint.Divide(point, scalar);

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToVector2_ReturnsVector2()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);

        // Act
        System.Numerics.Vector2 result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromVector2_ReturnsGorgonPoint()
    {
        // Arrange
        var vector = new System.Numerics.Vector2(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)vector;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToPoint_ReturnsPoint()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);

        // Act
        System.Drawing.Point result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToPointF_ReturnsPointF()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);

        // Act
        System.Drawing.PointF result = point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromPoint_ReturnsGorgonPoint()
    {
        // Arrange
        var point = new System.Drawing.Point(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromPointF_ReturnsGorgonPoint()
    {
        // Arrange
        var point = new System.Drawing.PointF(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)point;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ImplicitConversionToSize_ReturnsSize()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);

        // Act
        System.Drawing.Size result = point;

        // Assert
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(20, result.Height);
    }

    [TestMethod]
    public void ImplicitConversionToSizeF_ReturnsSizeF()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);

        // Act
        System.Drawing.SizeF result = point;

        // Assert
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(20, result.Height);
    }

    [TestMethod]
    public void ExplicitConversionFromSize_ReturnsGorgonPoint()
    {
        // Arrange
        var size = new System.Drawing.Size(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)size;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void ExplicitConversionFromSizeF_ReturnsGorgonPoint()
    {
        // Arrange
        var size = new System.Drawing.SizeF(10, 20);

        // Act
        GorgonPoint result = (GorgonPoint)size;

        // Assert
        Assert.AreEqual(10, result.X);
        Assert.AreEqual(20, result.Y);
    }

    [TestMethod]
    public void AdditionOperator_ReturnsSumOfPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(5, 10);

        // Act
        var result = point1 + point2;

        // Assert
        Assert.AreEqual(15, result.X);
        Assert.AreEqual(30, result.Y);
    }

    [TestMethod]
    public void SubtractionOperator_ReturnsDifferenceOfPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(5, 10);

        // Act
        var result = point1 - point2;

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void MultiplicationOperator_ReturnsProductOfPointAndScalar()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);
        var scalar = 2;

        // Act
        var result = point * scalar;

        // Assert
        Assert.AreEqual(20, result.X);
        Assert.AreEqual(40, result.Y);
    }

    [TestMethod]
    public void DivisionOperator_ReturnsQuotientOfPointAndScalar()
    {
        // Arrange
        var point = new GorgonPoint(10, 20);
        var scalar = 2;

        // Act
        var result = point / scalar;

        // Assert
        Assert.AreEqual(5, result.X);
        Assert.AreEqual(10, result.Y);
    }

    [TestMethod]
    public void EqualityOperator_ReturnsTrueForEqualPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(10, 20);

        // Act
        var result = point1 == point2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void EqualityOperator_ReturnsFalseForDifferentPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(20, 10);

        // Act
        var result = point1 == point2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void InequalityOperator_ReturnsTrueForDifferentPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(20, 10);

        // Act
        var result = point1 != point2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void InequalityOperator_ReturnsFalseForEqualPoints()
    {
        // Arrange
        var point1 = new GorgonPoint(10, 20);
        var point2 = new GorgonPoint(10, 20);

        // Act
        var result = point1 != point2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Serialization()
    {
        string expectedString = "{\"x\":10,\"y\":20}";
        GorgonPoint point = new(10, 20);

        string actualString = JsonConvert.SerializeObject(point);

        Assert.AreEqual(expectedString, actualString);

        GorgonPoint deserializedPoint = JsonConvert.DeserializeObject<GorgonPoint>(actualString);

        Assert.AreEqual(point, deserializedPoint);
    }
}
