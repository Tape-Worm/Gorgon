﻿using System;
using System.Numerics;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonCatmullRomSplineTests
{
    [TestMethod]
    public void GetInterpolatedValue_StartPointIndexInRange_ReturnsInterpolatedValue()
    {
        // Arrange
        var spline = new GorgonCatmullRomSpline();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        var result = spline.GetInterpolatedValue(1, 0.5f);

        // Assert
        Assert.AreEqual(new Vector4(1.6687499f, -0.45624995f, 1.5f, 1.5f), result);
    }

    [TestMethod]
    public void GetInterpolatedValue_StartPointIndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var spline = new GorgonCatmullRomSpline();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1, 1, 1, 1));
        spline.Points.Add(new Vector4(2, 2, 2, 2));
        spline.Points.Add(new Vector4(3, 3, 3, 3));
        spline.UpdateTangents();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => spline.GetInterpolatedValue(4, 0.5f));
    }

    [TestMethod]
    public void GetInterpolatedValue_DeltaEqualsZero_ReturnsStartPoint()
    {
        // Arrange
        var spline = new GorgonCatmullRomSpline();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1, 1, 1, 1));
        spline.Points.Add(new Vector4(2, 2, 2, 2));
        spline.Points.Add(new Vector4(3, 3, 3, 3));
        spline.UpdateTangents();

        // Act
        var result = spline.GetInterpolatedValue(1, 0.0f);

        // Assert
        Assert.AreEqual(new Vector4(1, 1, 1, 1), result);
    }

    [TestMethod]
    public void GetInterpolatedValue_DeltaEqualsOne_ReturnsExactPoint()
    {
        // Arrange
        var spline = new GorgonCatmullRomSpline();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        var result = spline.GetInterpolatedValue(1, 1.0f);

        // Assert
        Assert.AreEqual(new Vector4(1.8f, 0.5f, 2, 2), result);
    }

    [TestMethod]
    public void GetInterpolatedValue_Delta()
    {
        // Arrange
        var spline = new GorgonCatmullRomSpline();
        spline.Points.Add(new Vector4(0, 0, 0, 0));
        spline.Points.Add(new Vector4(1.5f, -1.2f, 1, 1));
        spline.Points.Add(new Vector4(1.8f, 0.5f, 2, 2));
        spline.Points.Add(new Vector4(3.0f, 1.0f, 3, 3));
        spline.UpdateTangents();

        // Act
        var result = spline.GetInterpolatedValue(1, -0.5f);

        // Assert
        Assert.AreEqual(new Vector4(0.5062499f, -0.1937499f, 0.5f, 0.5f), result);
    }
}
