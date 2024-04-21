using System.Numerics;
using System.Text.Json;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRectangleTests
{
    [TestMethod]
    public void InitializeWithLocationAndSizeSetsProperties()
    {
        // Arrange
        GorgonPoint location = new(10, 20);
        GorgonPoint size = new(30, 40);

        // Act
        GorgonRectangle rectangle = new(location, size);

        // Assert
        Assert.AreEqual(location, rectangle.Location);
        Assert.AreEqual(size, rectangle.Size);

        // Arrange
        Vector2 location1 = new(10.5f, 20.7f);
        Vector2 size1 = new(30.3f, 40.9f);

        // Act
        GorgonRectangleF rectangleF = new(location1, size1);

        // Assert
        Assert.AreEqual(location1, rectangleF.Location);
        Assert.AreEqual(size1, rectangleF.Size);
    }

    [TestMethod]
    public void ConvertRectangleFToRectangleReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangleF rectangleF = new(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        GorgonRectangle rectangle = GorgonRectangleF.ToGorgonRectangle(rectangleF);

        // Assert
        Assert.AreEqual((int)rectangleF.X, rectangle.X);
        Assert.AreEqual((int)rectangleF.Y, rectangle.Y);
        Assert.AreEqual((int)rectangleF.Width, rectangle.Width);
        Assert.AreEqual((int)rectangleF.Height, rectangle.Height);
    }

    [TestMethod]
    public void RoundConvertRectangleFToRectangleRoundsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Round(rectangleF);

        // Assert
        Assert.AreEqual(11, rectangle.X);
        Assert.AreEqual(21, rectangle.Y);
        Assert.AreEqual(30, rectangle.Width);
        Assert.AreEqual(41, rectangle.Height);
    }

    [TestMethod]
    public void CeilingConvertRectangleFToRectangleCeilsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Ceiling(rectangleF);

        // Assert
        Assert.AreEqual(11, rectangle.X);
        Assert.AreEqual(21, rectangle.Y);
        Assert.AreEqual(31, rectangle.Width);
        Assert.AreEqual(41, rectangle.Height);

        // Arrange
        GorgonRectangleF rectangleF2 = new(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangleF rectangle2 = GorgonRectangleF.Ceiling(rectangleF2);

        // Assert
        Assert.AreEqual(11, rectangle2.X);
        Assert.AreEqual(21, rectangle2.Y);
        Assert.AreEqual(31, rectangle2.Width);
        Assert.AreEqual(41, rectangle2.Height);
    }

    [TestMethod]
    public void FloorConvertRectangleFToRectangleFloorsValues()
    {
        // Arrange
        GorgonRectangleF rectangleF = new(10.9f, 20.1f, 30.7f, 40.5f);

        // Act
        GorgonRectangle rectangle = GorgonRectangle.Floor(rectangleF);

        // Assert
        Assert.AreEqual(10, rectangle.X);
        Assert.AreEqual(20, rectangle.Y);
        Assert.AreEqual(30, rectangle.Width);
        Assert.AreEqual(40, rectangle.Height);

        // Arrange
        GorgonRectangleF rectangleF2 = new(10.9f, 20.1f, 30.7f, 40.5f);

        // Act
        GorgonRectangleF rectangle2 = GorgonRectangleF.Floor(rectangleF2);

        // Assert
        Assert.AreEqual(10, rectangle2.X);
        Assert.AreEqual(20, rectangle2.Y);
        Assert.AreEqual(30, rectangle2.Width);
        Assert.AreEqual(40, rectangle2.Height);
    }

    [TestMethod]
    public void ToGorgonRectangleFConvertRectangleToRectangleFReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);

        // Act
        GorgonRectangleF rectangleF = GorgonRectangle.ToGorgonRectangleF(rectangle);

        // Assert
        Assert.AreEqual(rectangle.X, rectangleF.X);
        Assert.AreEqual(rectangle.Y, rectangleF.Y);
        Assert.AreEqual(rectangle.Width, rectangleF.Width);
        Assert.AreEqual(rectangle.Height, rectangleF.Height);
    }

    [TestMethod]
    public void ExpandWithPositiveAmountExpandsRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);
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
    public void ExpandWithNegativeAmountShrinksRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);
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
    public void ExpandWithZeroAmountReturnsSameRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);
        int amount = 0;

        // Act
        GorgonRectangle expandedRectangle = GorgonRectangle.Expand(rectangle, amount);

        // Assert
        Assert.AreEqual(rectangle, expandedRectangle);

        // Arrange
        GorgonRectangleF rectangleF = new(10, 20, 30, 40);
        float amount2 = 0.0f;

        // Act
        GorgonRectangleF expandedRectangleF = GorgonRectangleF.Expand(rectangleF, amount2);

        // Assert
        Assert.AreEqual(rectangleF, expandedRectangleF);
    }

    [TestMethod]
    public void ExplicitConversionConvertRectangleFToRectangleReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangleF rectangleF = new(10.1f, 20.9f, 30.5f, 40.7f);

        // Act
        GorgonRectangle rectangle = (GorgonRectangle)rectangleF;

        // Assert
        Assert.AreEqual((int)rectangleF.X, rectangle.X);
        Assert.AreEqual((int)rectangleF.Y, rectangle.Y);
        Assert.AreEqual((int)rectangleF.Width, rectangle.Width);
        Assert.AreEqual((int)rectangleF.Height, rectangle.Height);
    }

    [TestMethod]
    public void ImplicitConversionConvertRectangleToRectangleFReturnsConvertedRectangle()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);

        // Act
        GorgonRectangleF rectangleF = rectangle;

        // Assert
        Assert.AreEqual(rectangle.X, rectangleF.X);
        Assert.AreEqual(rectangle.Y, rectangleF.Y);
        Assert.AreEqual(rectangle.Width, rectangleF.Width);
        Assert.AreEqual(rectangle.Height, rectangleF.Height);
    }

    [TestMethod]
    public void DeconstructReturnsTupleWithValues()
    {
        // Arrange
        GorgonRectangle rectangle = new(10, 20, 30, 40);

        // Act
        (int x, int y, int width, int height) = rectangle;

        // Assert
        Assert.AreEqual(rectangle.X, x);
        Assert.AreEqual(rectangle.Y, y);
        Assert.AreEqual(rectangle.Width, width);
        Assert.AreEqual(rectangle.Height, height);

        // Arrange
        GorgonRectangleF rectangle2 = new(10.1f, 20.2f, 30.3f, 40.4f);

        // Act
        (float x2, float y2, float width2, float height2) = rectangle2;

        // Assert
        Assert.AreEqual(rectangle2.X, x2);
        Assert.AreEqual(rectangle2.Y, y2);
        Assert.AreEqual(rectangle2.Width, width2);
        Assert.AreEqual(rectangle2.Height, height2);
    }

    [TestMethod]
    public void EqualsReturnsTrueForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(10, 20, 30, 40);

        // Act
        bool result = GorgonRectangle.Equals(rectangle1, rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF rectangle3 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result2 = GorgonRectangleF.Equals(rectangle3, rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void EqualsReturnsFalseForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(20, 30, 40, 50);

        // Act
        bool result = GorgonRectangle.Equals(rectangle1, rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangleF rectangleF1 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangleF2 = new(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool resultF = GorgonRectangleF.Equals(rectangleF1, rectangleF2);

        // Assert
        Assert.IsFalse(resultF);
    }

    [TestMethod]
    public void OperatorEqualsReturnsTrueForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(10, 20, 30, 40);
        GorgonRectangleF rectangle3 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result = rectangle1 == rectangle2;
        bool result2 = rectangle3 == rectangle4;

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void OperatorEqualsReturnsFalseForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(20, 30, 40, 50);
        GorgonRectangleF rectangle3 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool result = rectangle1 == rectangle2;
        bool result2 = rectangle3 == rectangle4;

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void OperatorNotEqualsReturnsTrueForDifferentRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(20, 30, 40, 50);
        GorgonRectangleF rectangle3 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new(20.5f, 30.7f, 40.3f, 50.9f);

        // Act
        bool result = rectangle1 != rectangle2;
        bool result2 = rectangle3 != rectangle4;

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void OperatorNotEqualsReturnsFalseForEqualRectangles()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(10, 20, 30, 40);
        GorgonRectangle rectangle2 = new(10, 20, 30, 40);
        GorgonRectangleF rectangle3 = new(10.5f, 20.7f, 30.3f, 40.9f);
        GorgonRectangleF rectangle4 = new(10.5f, 20.7f, 30.3f, 40.9f);

        // Act
        bool result = rectangle1 != rectangle2;
        bool result2 = rectangle3 != rectangle4;

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void UnionShouldReturnCorrectResultWhenBothRectanglesAreSame()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(0, 0, 10, 10);

        // Act
        GorgonRectangle result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(0, 0, 10, 10);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void UnionShouldReturnCorrectResultWhenRectanglesAreDifferent()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(5, 5, 15, 15);

        // Act
        GorgonRectangle result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(new GorgonRectangle(0, 0, 20, 20), result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(5, 5, 15, 15);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(new GorgonRectangle(0, 0, 20, 20), result2);
    }

    [TestMethod]
    public void UnionShouldReturnCorrectResultWhenOneRectangleIsInsideAnother()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 20, 20);
        GorgonRectangle rectangle2 = new(5, 5, 10, 10);

        // Act
        GorgonRectangle result = GorgonRectangle.Union(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 20, 20);
        GorgonRectangleF rectangle4 = new(5, 5, 10, 10);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Union(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void IntersectShouldReturnCorrectResultWhenBothRectanglesAreSame()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(0, 0, 10, 10);

        // Act
        GorgonRectangle result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(rectangle1, result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(0, 0, 10, 10);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(rectangle3, result2);
    }

    [TestMethod]
    public void IntersectShouldReturnCorrectResultWhenRectanglesOverlap()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(5, 5, 15, 15);

        // Act
        GorgonRectangle result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(new GorgonRectangle(5, 5, 5, 5), result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(5, 5, 15, 15);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(new GorgonRectangleF(5, 5, 5, 5), result2);
    }

    [TestMethod]
    public void IntersectShouldReturnEmptyWhenRectanglesDoNotOverlap()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(20, 20, 10, 10);

        // Act
        GorgonRectangle result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(GorgonRectangle.Empty, result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(20, 20, 10, 10);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(GorgonRectangleF.Empty, result2);
    }

    [TestMethod]
    public void IntersectShouldReturnEmptyWhenOneRectanglesRightIsEqualToOthersLeft()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(10, 0, 10, 10);

        // Act
        GorgonRectangle result = GorgonRectangle.Intersect(rectangle1, rectangle2);

        // Assert
        Assert.AreEqual(GorgonRectangle.Empty, result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(10, 0, 10, 10);

        // Act
        GorgonRectangleF result2 = GorgonRectangleF.Intersect(rectangle3, rectangle4);

        // Assert
        Assert.AreEqual(GorgonRectangleF.Empty, result2);
    }

    [TestMethod]
    public void ContainsRectangleShouldReturnTrueWhenRectangleIsContained()
    {
        // Arrange
        GorgonRectangle outerRectangle = new(0, 0, 10, 10);
        GorgonRectangle innerRectangle = new(2, 2, 8, 8);

        // Act
        bool result = outerRectangle.Contains(innerRectangle);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF outerRectangle2 = new(0.5f, 0.5f, 10.2f, 10.1f);
        GorgonRectangleF innerRectangle2 = new(2.2f, 2.2f, 8.1f, 8);

        // Act
        bool result2 = outerRectangle2.Contains(innerRectangle2);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void ContainsRectangleShouldReturnFalseWhenRectangleIsNotContained()
    {
        // Arrange
        GorgonRectangle outerRectangle = new(0, 0, 10, 10);
        GorgonRectangle innerRectangle = new(5, 5, 15, 15);

        // Act
        bool result = outerRectangle.Contains(innerRectangle);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangleF outerRectangle2 = new(0.5f, 0.5f, 10.2f, 10.1f);
        GorgonRectangleF innerRectangle2 = new(5.5f, 5, 15.25f, 15.3f);

        // Act
        bool result2 = outerRectangle2.Contains(innerRectangle2);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void ContainsPointShouldReturnTrueWhenPointIsContained()
    {
        // Arrange
        GorgonRectangle rectangle = new(0, 0, 10, 10);
        GorgonPoint point = new(5, 5);

        // Act
        bool result = rectangle.Contains(point);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF rectangle2 = new(0.25f, 0.625f, 10.9f, 10.1f);
        Vector2 point2 = new(5.5f, 5.5f);

        // Act
        bool result2 = rectangle2.Contains(point2);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void ContainsPointShouldReturnFalseWhenPointIsNotContained()
    {
        // Arrange
        GorgonRectangle rectangle = new(0, 0, 10, 10);
        GorgonPoint point = new(15, 15);

        // Act
        bool result = rectangle.Contains(point);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangleF rectangle2 = new(0.25f, 0.625f, 10.9f, 10.1f);
        Vector2 point2 = new(15.9f, 15.75f);

        // Act
        bool result2 = rectangle2.Contains(point2);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void IntersectsWithShouldReturnTrueWhenRectanglesIntersect()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(5, 5, 15, 15);

        // Act
        bool result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(5.2f, 5.1f, 15.9f, 15.2f);

        // Act
        bool result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void IntersectsWithShouldReturnFalseWhenRectanglesDoNotIntersect()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(15, 15, 25, 25);

        // Act
        bool result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(15.1f, 15.2f, 25.3f, 25.4f);

        // Act
        bool result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void IntersectsWithShouldReturnTrueWhenRectanglesAreSame()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(0, 0, 10, 10);

        // Act
        bool result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsTrue(result);

        // Arrange
        GorgonRectangleF rectangle3 = new(0, 0, 10, 10);
        GorgonRectangleF rectangle4 = new(0, 0, 10, 10);

        // Act
        bool result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsTrue(result2);
    }

    [TestMethod]
    public void IntersectsWithShouldReturnFalseWhenOneRectanglesRightIsEqualToOthersLeft()
    {
        // Arrange
        GorgonRectangle rectangle1 = new(0, 0, 10, 10);
        GorgonRectangle rectangle2 = new(10, 0, 20, 10);

        // Act
        bool result = rectangle1.IntersectsWith(rectangle2);

        // Assert
        Assert.IsFalse(result);

        // Arrange
        GorgonRectangle rectangle3 = new(0, 0, 10, 10);
        GorgonRectangle rectangle4 = new(10, 0, 20, 10);

        // Act
        bool result2 = rectangle3.IntersectsWith(rectangle4);

        // Assert
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void FromTLRBShouldReturnCorrectRectangle()
    {
        // Arrange
        int top = 1;
        int left = 2;
        int right = 5;
        int bottom = 6;

        // Act
        GorgonRectangle result = GorgonRectangle.FromLTRB(left, top, right, bottom);

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
        GorgonRectangleF result2 = GorgonRectangleF.FromLTRB(left, top, right, bottom);

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
        GorgonRectangleF rectf = new(10.5f, 20.5f, 30.25f, 40.75f);

        actualString = JsonSerializer.Serialize(rectf);

        Assert.AreEqual(expectedString, actualString);

        GorgonRectangleF deserializedRectangleF = JsonSerializer.Deserialize<GorgonRectangleF>(actualString);

        Assert.AreEqual(rectf, deserializedRectangleF);
    }
}
