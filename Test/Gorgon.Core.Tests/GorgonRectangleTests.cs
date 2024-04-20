using System.Numerics;
using Gorgon.Graphics;
using System.Text.Json;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRectangleTests
{
    [TestMethod]
    public void InitializeWithLocationAndSize_SetsProperties()
    {
        // Arrange
        GorgonPoint location = new GorgonPoint(10, 20);
        GorgonPoint size = new GorgonPoint(30, 40);

        // Act
        GorgonRectangle rectangle = new GorgonRectangle(location, size);

        // Assert
        Assert.AreEqual(location, rectangle.Location);
        Assert.AreEqual(size, rectangle.Size);

        // Arrange
        Vector2 location1 = new Vector2(10.5f, 20.7f);
        Vector2 size1 = new Vector2(30.3f, 40.9f);

        // Act
        GorgonRectangleF rectangleF = new GorgonRectangleF(location1, size1);

        // Assert
        Assert.AreEqual(location1, rectangleF.Location);
        Assert.AreEqual(size1, rectangleF.Size);
    }

    [TestMethod]
    public void ConvertRectangleFToRectangle_ReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        GorgonRectangle rectangle = GorgonRectangleF.ToGorgonRectangle(rectangleF);

        // Assert
        Assert.AreEqual((int)rectangleF.X, rectangle.X);
        Assert.AreEqual((int)rectangleF.Y, rectangle.Y);
        Assert.AreEqual((int)rectangleF.Width, rectangle.Width);
        Assert.AreEqual((int)rectangleF.Height, rectangle.Height);
    }

    [TestMethod]
    public void Round_ConvertRectangleFToRectangle_RoundsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Round(rectangleF);

        // Assert
        Assert.AreEqual(11, rectangle.X);
        Assert.AreEqual(21, rectangle.Y);
        Assert.AreEqual(30, rectangle.Width);
        Assert.AreEqual(41, rectangle.Height);
    }

    [TestMethod]
    public void Ceiling_ConvertRectangleFToRectangle_CeilsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Ceiling(rectangleF);

        // Assert
        Assert.AreEqual(11, rectangle.X);
        Assert.AreEqual(21, rectangle.Y);
        Assert.AreEqual(31, rectangle.Width);
        Assert.AreEqual(41, rectangle.Height);

        // Arrange
        GorgonRectangleF rectangleF2 = new GorgonRectangleF(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangleF rectangle2 = GorgonRectangleF.Ceiling(rectangleF2);

        // Assert
        Assert.AreEqual(11, rectangle2.X);
        Assert.AreEqual(21, rectangle2.Y);
        Assert.AreEqual(31, rectangle2.Width);
        Assert.AreEqual(41, rectangle2.Height);
    }

    [TestMethod]
    public void Floor_ConvertRectangleFToRectangle_FloorsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10.9f, 20.1f, 30.7f, 40.5f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Floor(rectangleF);

        // Assert
        Assert.AreEqual(10, rectangle.X);
        Assert.AreEqual(20, rectangle.Y);
        Assert.AreEqual(30, rectangle.Width);
        Assert.AreEqual(40, rectangle.Height);

        // Arrange
        GorgonRectangleF rectangleF2 = new GorgonRectangleF(10.9f, 20.1f, 30.7f, 40.5f);

        // Act
        GorgonRectangleF rectangle2 = GorgonRectangleF.Floor(rectangleF2);

        // Assert
        Assert.AreEqual(10, rectangle2.X);
        Assert.AreEqual(20, rectangle2.Y);
        Assert.AreEqual(30, rectangle2.Width);
        Assert.AreEqual(40, rectangle2.Height);
    }

    [TestMethod]
    public void ToGorgonRectangleF_ConvertRectangleToRectangleF_ReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);

        // Act
        GorgonRectangleF rectangleF = GorgonRectangle.ToGorgonRectangleF(rectangle);

        // Assert
        Assert.AreEqual(rectangle.X, rectangleF.X);
        Assert.AreEqual(rectangle.Y, rectangleF.Y);
        Assert.AreEqual(rectangle.Width, rectangleF.Width);
        Assert.AreEqual(rectangle.Height, rectangleF.Height);
    }

    [TestMethod]
    public void Expand_WithPositiveAmount_ExpandsRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);
        int amount = 5;

        // Act
        GorgonRectangle expandedRectangle = GorgonRectangle.Expand(rectangle, amount);

        // Assert
        Assert.AreEqual(rectangle.X - amount, expandedRectangle.X);
        Assert.AreEqual(rectangle.Y - amount, expandedRectangle.Y);
        Assert.AreEqual(rectangle.Width + amount * 2, expandedRectangle.Width);
        Assert.AreEqual(rectangle.Height + amount * 2, expandedRectangle.Height);
    }

    [TestMethod]
    public void Expand_WithNegativeAmount_ShrinksRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);
        int amount = -5;

        // Act
        GorgonRectangle expandedRectangle = GorgonRectangle.Expand(rectangle, amount);

        // Assert
        Assert.AreEqual(rectangle.X - amount, expandedRectangle.X);
        Assert.AreEqual(rectangle.Y - amount, expandedRectangle.Y);
        Assert.AreEqual(rectangle.Width + amount * 2, expandedRectangle.Width);
        Assert.AreEqual(rectangle.Height + amount * 2, expandedRectangle.Height);
    }

    [TestMethod]
    public void Expand_WithZeroAmount_ReturnsSameRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);
        int amount = 0;

        // Act
        GorgonRectangle expandedRectangle = GorgonRectangle.Expand(rectangle, amount);

        // Assert
        Assert.AreEqual(rectangle, expandedRectangle);

        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10, 20, 30, 40);
        float amount2 = 0.0f;

        // Act
        GorgonRectangleF expandedRectangleF = GorgonRectangleF.Expand(rectangleF, amount2);

        // Assert
        Assert.AreEqual(rectangleF, expandedRectangleF);
    }

    [TestMethod]
    public void ExplicitConversion_ConvertRectangleFToRectangle_ReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangleF rectangleF = new GorgonRectangleF(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangle rectangle = (GorgonRectangle)rectangleF;

        // Assert
        Assert.AreEqual((int)rectangleF.X, rectangle.X);
        Assert.AreEqual((int)rectangleF.Y, rectangle.Y);
        Assert.AreEqual((int)rectangleF.Width, rectangle.Width);
        Assert.AreEqual((int)rectangleF.Height, rectangle.Height);
    }

    [TestMethod]
    public void ImplicitConversion_ConvertRectangleToRectangleF_ReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);

        // Act
        GorgonRectangleF rectangleF = rectangle;

        // Assert
        Assert.AreEqual(rectangle.X, rectangleF.X);
        Assert.AreEqual(rectangle.Y, rectangleF.Y);
        Assert.AreEqual(rectangle.Width, rectangleF.Width);
        Assert.AreEqual(rectangle.Height, rectangleF.Height);
    }

    [TestMethod]
    public void Deconstruct_ReturnsTupleWithValues()
    {
        // Arrange
        GorgonRectangle rectangle = new GorgonRectangle(10, 20, 30, 40);

        // Act
        var (x, y, width, height) = rectangle;

        // Assert
        Assert.AreEqual(rectangle.X, x);
        Assert.AreEqual(rectangle.Y, y);
        Assert.AreEqual(rectangle.Width, width);
        Assert.AreEqual(rectangle.Height, height);

        // Arrange
        GorgonRectangleF rectangle2 = new GorgonRectangleF(10.1f, 20.2f, 30.3f, 40.4f);

        // Act
        var (x2, y2, width2, height2) = rectangle2;

        // Assert
        Assert.AreEqual(rectangle2.X, x2);
        Assert.AreEqual(rectangle2.Y, y2);
        Assert.AreEqual(rectangle2.Width, width2);
        Assert.AreEqual(rectangle2.Height, height2);
    }

    [TestMethod]
    public void Equals_ReturnsTrueForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(10, 20, 30, 40);

        // Act
        bool result = GorgonRectangle.Equals(rectangle1, rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF rectangle3 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result2 = GorgonRectangleF.Equals(rectangle3, rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void Equals_ReturnsFalseForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(20, 30, 40, 50);

        // Act
        bool result = GorgonRectangle.Equals(rectangle1, rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangleF rectangleF1 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangleF2 = new GorgonRectangleF(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool resultF = GorgonRectangleF.Equals(rectangleF1, rectangleF2);

        // Assert
        Assert.IsFalse(resultF);
    }

    [TestMethod]
    public void OperatorEquals_ReturnsTrueForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangleF rectangle3 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result = rectangle1 == rectangle2;
        bool result2 = rectangle3 == rectangle4;

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void OperatorEquals_ReturnsFalseForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(20, 30, 40, 50);
        GorgonRectangleF rectangle3 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new GorgonRectangleF(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool result = rectangle1 == rectangle2;
        bool result2 = rectangle3 == rectangle4;

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void OperatorNotEquals_ReturnsTrueForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(20, 30, 40, 50);
        GorgonRectangleF rectangle3 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new GorgonRectangleF(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool result = rectangle1 != rectangle2;
        bool result2 = rectangle3 != rectangle4;

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void OperatorNotEquals_ReturnsFalseForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new GorgonRectangle(10, 20, 30, 40);
        GorgonRectangleF rectangle3 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new GorgonRectangleF(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result = rectangle1 != rectangle2;
        bool result2 = rectangle3 != rectangle4;

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void Union_ShouldReturnCorrectResult_WhenBothRectanglesAreSame()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(0, 0, 10, 10);

        // Act
        var result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(0, 0, 10, 10);

        // Act
        var result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void Union_ShouldReturnCorrectResult_WhenRectanglesAreDifferent()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(5, 5, 15, 15);

        // Act
        var result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(new GorgonRectangle(0, 0, 20, 20), result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(5, 5, 15, 15);

        // Act
        var result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(new GorgonRectangle(0, 0, 20, 20), result2);
    }

    [TestMethod]
    public void Union_ShouldReturnCorrectResult_WhenOneRectangleIsInsideAnother()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 20, 20);
        var rectangle2 = new GorgonRectangle(5, 5, 10, 10);

        // Act
        var result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 20, 20);
        var rectangle4 = new GorgonRectangleF(5, 5, 10, 10);

        // Act
        var result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void Intersect_ShouldReturnCorrectResult_WhenBothRectanglesAreSame()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(0, 0, 10, 10);

        // Act
        var result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(0, 0, 10, 10);

        // Act
        var result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void Intersect_ShouldReturnCorrectResult_WhenRectanglesOverlap()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(5, 5, 15, 15);

        // Act
        var result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(new GorgonRectangle(5, 5, 5, 5), result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(5, 5, 15, 15);

        // Act
        var result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(new GorgonRectangleF(5, 5, 5, 5), result2);
    }

    [TestMethod]
    public void Intersect_ShouldReturnEmpty_WhenRectanglesDoNotOverlap()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(20, 20, 10, 10);

        // Act
        var result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(GorgonRectangle.Empty, result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(20, 20, 10, 10);

        // Act
        var result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(GorgonRectangleF.Empty, result2);
    }

    [TestMethod]
    public void Intersect_ShouldReturnEmpty_WhenOneRectanglesRightIsEqualToOthersLeft()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(10, 0, 10, 10);

        // Act
        var result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(GorgonRectangle.Empty, result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(10, 0, 10, 10);

        // Act
        var result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(GorgonRectangleF.Empty, result2);
    }

    [TestMethod]
    public void ContainsRectangle_ShouldReturnTrue_WhenRectangleIsContained()
    {
        // Arrange
        var outerRectangle = new GorgonRectangle(0, 0, 10, 10);
        var innerRectangle = new GorgonRectangle(2, 2, 8, 8);

        // Act
        var result = outerRectangle.Contains(innerRectangle);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        var outerRectangle2 = new GorgonRectangleF(0.5f, 0.5f, 10.2f, 10.1f);
        var innerRectangle2 = new GorgonRectangleF(2.2f, 2.2f, 8.1f, 8);

        // Act
        var result2 = outerRectangle2.Contains(innerRectangle2);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void ContainsRectangle_ShouldReturnFalse_WhenRectangleIsNotContained()
    {
        // Arrange
        var outerRectangle = new GorgonRectangle(0, 0, 10, 10);
        var innerRectangle = new GorgonRectangle(5, 5, 15, 15);

        // Act
        var result = outerRectangle.Contains(innerRectangle);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        var outerRectangle2 = new GorgonRectangleF(0.5f, 0.5f, 10.2f, 10.1f);
        var innerRectangle2 = new GorgonRectangleF(5.5f, 5, 15.25f, 15.3f);

        // Act
        var result2 = outerRectangle2.Contains(innerRectangle2);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void ContainsPoint_ShouldReturnTrue_WhenPointIsContained()
    {
        // Arrange
        var rectangle = new GorgonRectangle(0, 0, 10, 10);
        var point = new GorgonPoint(5, 5);

        // Act
        var result = rectangle.Contains(point);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        var rectangle2 = new GorgonRectangleF(0.25f, 0.625f, 10.9f, 10.1f);
        var point2 = new Vector2(5.5f, 5.5f);

        // Act
        var result2 = rectangle2.Contains(point2);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void ContainsPoint_ShouldReturnFalse_WhenPointIsNotContained()
    {
        // Arrange
        var rectangle = new GorgonRectangle(0, 0, 10, 10);
        var point = new GorgonPoint(15, 15);

        // Act
        var result = rectangle.Contains(point);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        var rectangle2 = new GorgonRectangleF(0.25f, 0.625f, 10.9f, 10.1f);
        var point2 = new Vector2(15.9f, 15.75f);

        // Act
        var result2 = rectangle2.Contains(point2);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void IntersectsWith_ShouldReturnTrue_WhenRectanglesIntersect()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(5, 5, 15, 15);

        // Act
        var result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(5.2f, 5.1f, 15.9f, 15.2f);

        // Act
        var result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void IntersectsWith_ShouldReturnFalse_WhenRectanglesDoNotIntersect()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(15, 15, 25, 25);

        // Act
        var result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(15.1f, 15.2f, 25.3f, 25.4f);

        // Act
        var result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void IntersectsWith_ShouldReturnTrue_WhenRectanglesAreSame()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(0, 0, 10, 10);

        // Act
        var result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        var rectangle3 = new GorgonRectangleF(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangleF(0, 0, 10, 10);

        // Act
        var result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void IntersectsWith_ShouldReturnFalse_WhenOneRectanglesRightIsEqualToOthersLeft()
    {
        // Arrange
        var rectangle1 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle2 = new GorgonRectangle(10, 0, 20, 10);

        // Act
        var result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        var rectangle3 = new GorgonRectangle(0, 0, 10, 10);
        var rectangle4 = new GorgonRectangle(10, 0, 20, 10);

        // Act
        var result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void FromTLRB_ShouldReturnCorrectRectangle()
    {
        // Arrange
        int top = 1;
        int left = 2;
        int right = 5;
        int bottom = 6;

        // Act
        var result = GorgonRectangle.FromLTRB(left, top, right, bottom);

        // Assert
        Assert.AreEqual(left, result.X);
        Assert.AreEqual(top, result.Y);
        Assert.AreEqual(right - left, result.Width);
        Assert.AreEqual(bottom - top, result.Height);
        Assert.AreEqual(left, result.Left);
        Assert.AreEqual(top, result.Top);
        Assert.AreEqual(right, result.Right);
        Assert.AreEqual(bottom, result.Bottom);

        // Arrange
        float top2 = 1.0f;
        float left2 = 2.0f;
        float right2 = 5.0f;
        float bottom2 = 6.0f;

        // Act
        var result2 = GorgonRectangleF.FromLTRB(left, top, right, bottom);

        // Assert
        Assert.AreEqual(left2, result2.X);
        Assert.AreEqual(top2, result2.Y);
        Assert.AreEqual(right2 - left2, result2.Width);
        Assert.AreEqual(bottom2 - top2, result2.Height);
        Assert.AreEqual(left2, result2.Left);
        Assert.AreEqual(top2, result2.Top);
        Assert.AreEqual(right2, result2.Right);
        Assert.AreEqual(bottom2, result2.Bottom);
    }

    [TestMethod]
    public void Serialization()
    {
        string expectedString = "{\"x\":10,\"y\":20,\"width\":30,\"height\":40}";
        GorgonRectangle rectangle = new(10, 20, 30, 40);

        string actualString = JsonSerializer.Serialize(rectangle);

        Assert.AreEqual(expectedString, actualString);

        GorgonRectangle deserializedRectangle = JsonSerializer.Deserialize<GorgonRectangle>(actualString);

        Assert.AreEqual(rectangle, deserializedRectangle);

        expectedString = "{\"x\":10.5,\"y\":20.5,\"width\":30.25,\"height\":40.75}";
        GorgonRectangleF rectf = new GorgonRectangleF(10.5f, 20.5f, 30.25f, 40.75f);

        actualString = JsonSerializer.Serialize(rectf);

        Assert.AreEqual(expectedString, actualString);

        GorgonRectangleF deserializedRectangleF = JsonSerializer.Deserialize<GorgonRectangleF>(actualString);

        Assert.AreEqual(rectf, deserializedRectangleF);
    }
}
