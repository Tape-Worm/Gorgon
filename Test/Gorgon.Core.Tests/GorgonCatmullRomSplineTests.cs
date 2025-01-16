using System;
using System.Numerics;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonCatmullRomSplineTests
{
    [TestMethod]
    public void GetInterpolatedValueStartPointIndexInRangeReturnsInterpolatedValue()
    {
        // Arrange
        GorgonCatmullRomSpline spline = new();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        Vector4 result = spline.GetInterpolatedValue(1, 0.5f);

        // Assert
        Assert.AreEqual(new Vector4(1.6687499f, -0.45624995f, 1.5f, 1.5f), result);
    }

    [TestMethod]
    public void GetInterpolatedValueStartPointIndexOutOfRangeThrowsArgumentOutOfRangeException()
    {
        // Arrange
        GorgonCatmullRomSpline spline = new();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1, 1, 1, 1));
        spline.Points.Add(new Vector4(2, 2, 2, 2));
        spline.Points.Add(new Vector4(3, 3, 3, 3));
        spline.UpdateTangents();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => spline.GetInterpolatedValue(4, 0.5f));
    }

    [TestMethod]
    public void GetInterpolatedValueDeltaEqualsZeroReturnsStartPoint()
    {
        // Arrange
        GorgonCatmullRomSpline spline = new();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1, 1, 1, 1));
        spline.Points.Add(new Vector4(2, 2, 2, 2));
        spline.Points.Add(new Vector4(3, 3, 3, 3));
        spline.UpdateTangents();

        // Act
        Vector4 result = spline.GetInterpolatedValue(1, 0.0f);

        // Assert
        Assert.AreEqual(new Vector4(1, 1, 1, 1), result);
    }

    [TestMethod]
    public void GetInterpolatedValueDeltaEqualsOneReturnsExactPoint()
    {
        // Arrange
        GorgonCatmullRomSpline spline = new();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        Vector4 result = spline.GetInterpolatedValue(1, 1.0f);

        // Assert
        Assert.AreEqual(new Vector4(1.8f, 0.5f, 2, 2), result);
    }

    [TestMethod]
    public void GetInterpolatedValueDelta()
    {
        // Arrange
        GorgonCatmullRomSpline spline = new();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        Vector4 result = spline.GetInterpolatedValue(1, -0.5f);

        // Assert
        Assert.AreEqual(new Vector4(0.5062499f, -0.1937499f, 0.5f, 0.5f), result);
    }
}
