using System.Numerics;
using System.Text.Json;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonBoxTests
{
    [TestMethod]
    public void CTor()
    {
        GorgonBox box = new(new GorgonRectangle(-10, -10, 20, 20), -10, 20);

        Assert.AreEqual(-10, box.Left);
        Assert.AreEqual(-10, box.Top);
        Assert.AreEqual(-10, box.Front);
        Assert.AreEqual(10, box.Right);
        Assert.AreEqual(10, box.Bottom);
        Assert.AreEqual(10, box.Back);

        box = new(-10, -10, -10, 20, 20, 20);

        Assert.AreEqual(-10, box.Left);
        Assert.AreEqual(-10, box.Top);
        Assert.AreEqual(-10, box.Front);
        Assert.AreEqual(10, box.Right);
        Assert.AreEqual(10, box.Bottom);
        Assert.AreEqual(10, box.Back);

        box = GorgonBox.FromLTFRBB(-10, -10, -10, 10, 10, 10);

        Assert.AreEqual(-10, box.X);
        Assert.AreEqual(-10, box.Y);
        Assert.AreEqual(-10, box.Z);
        Assert.AreEqual(20, box.Width);
        Assert.AreEqual(20, box.Height);
        Assert.AreEqual(20, box.Depth);

        GorgonBoxF boxf = new(box);
        Assert.AreEqual(-10, boxf.X);
        Assert.AreEqual(-10, boxf.Y);
        Assert.AreEqual(-10, boxf.Z);
        Assert.AreEqual(20, boxf.Width);
        Assert.AreEqual(20, boxf.Height);
        Assert.AreEqual(20, boxf.Depth);

        boxf = new(new Vector3(-10, -10, -10), new Vector3(20, 20, 20));
        Assert.AreEqual(-10, boxf.X);
        Assert.AreEqual(-10, boxf.Y);
        Assert.AreEqual(-10, boxf.Z);
        Assert.AreEqual(20, boxf.Width);
        Assert.AreEqual(20, boxf.Height);
        Assert.AreEqual(20, boxf.Depth);

        boxf = new(new GorgonRectangleF(-10, -10, 20, 20), -10, 20);
        Assert.AreEqual(-10, boxf.X);
        Assert.AreEqual(-10, boxf.Y);
        Assert.AreEqual(-10, boxf.Z);
        Assert.AreEqual(20, boxf.Width);
        Assert.AreEqual(20, boxf.Height);
        Assert.AreEqual(20, boxf.Depth);

        boxf = new(new GorgonRectangle(-10, -10, 20, 20), -10, 20);
        Assert.AreEqual(-10, boxf.X);
        Assert.AreEqual(-10, boxf.Y);
        Assert.AreEqual(-10, boxf.Z);
        Assert.AreEqual(20, boxf.Width);
        Assert.AreEqual(20, boxf.Height);
        Assert.AreEqual(20, boxf.Depth);

        boxf = new(-10, -10, -10, 20, 20, 20);
        Assert.AreEqual(-10, boxf.X);
        Assert.AreEqual(-10, boxf.Y);
        Assert.AreEqual(-10, boxf.Z);
        Assert.AreEqual(20, boxf.Width);
        Assert.AreEqual(20, boxf.Height);
        Assert.AreEqual(20, boxf.Depth);
    }

    [TestMethod]
    public void Floor()
    {
        GorgonBoxF floatBox = new(10.8f, 9.2f, 5.5f, 11.2f, 15.6f, 19.0f);
        GorgonBox.Floor(in floatBox, out GorgonBox floor);

        Assert.AreEqual(10.0f, floor.X);
        Assert.AreEqual(9.0f, floor.Y);
        Assert.AreEqual(5.0f, floor.Z);
        Assert.AreEqual(11.0f, floor.Width);
        Assert.AreEqual(15.0f, floor.Height);
        Assert.AreEqual(19.0f, floor.Depth);

        GorgonBoxF.Floor(in floatBox, out GorgonBoxF floorF);

        Assert.AreEqual(10.0f, floorF.X);
        Assert.AreEqual(9.0f, floorF.Y);
        Assert.AreEqual(5.0f, floorF.Z);
        Assert.AreEqual(11.0f, floorF.Width);
        Assert.AreEqual(15.0f, floorF.Height);
        Assert.AreEqual(19.0f, floorF.Depth);
    }

    [TestMethod]
    public void Ceiling()
    {
        GorgonBoxF floatBox = new(10.8f, 9.2f, 5.5f, 11.2f, 15.6f, 19.0f);
        GorgonBox.Ceiling(in floatBox, out GorgonBox ceiling);

        Assert.AreEqual(11.0f, ceiling.X);
        Assert.AreEqual(10.0f, ceiling.Y);
        Assert.AreEqual(6.0f, ceiling.Z);
        Assert.AreEqual(12.0f, ceiling.Width);
        Assert.AreEqual(16.0f, ceiling.Height);
        Assert.AreEqual(19.0f, ceiling.Depth);

        GorgonBoxF.Ceiling(in floatBox, out GorgonBoxF ceilingF);

        Assert.AreEqual(11.0f, ceilingF.X);
        Assert.AreEqual(10.0f, ceilingF.Y);
        Assert.AreEqual(6.0f, ceilingF.Z);
        Assert.AreEqual(12.0f, ceilingF.Width);
        Assert.AreEqual(16.0f, ceilingF.Height);
        Assert.AreEqual(19.0f, ceilingF.Depth);
    }

    [TestMethod]
    public void Round()
    {
        GorgonBoxF floatBox = new(10.8f, 9.2f, 5.5f, 11.2f, 15.6f, 19.0f);
        GorgonBox.Round(in floatBox, out GorgonBox rounded);

        Assert.AreEqual(11.0f, rounded.X);
        Assert.AreEqual(9.0f, rounded.Y);
        Assert.AreEqual(6.0f, rounded.Z);
        Assert.AreEqual(11.0f, rounded.Width);
        Assert.AreEqual(16.0f, rounded.Height);
        Assert.AreEqual(19.0f, rounded.Depth);
    }

    [TestMethod]
    public void Conversions()
    {
        GorgonBoxF floatBox = new(10.8f, 9.2f, 5.5f, 11.2f, 15.6f, 19.0f);
        GorgonRectangleF floatRect = (GorgonRectangleF)floatBox;
        GorgonBox intBox = (GorgonBox)floatBox;
        GorgonRectangle intRect = (GorgonRectangle)intBox;

        Assert.AreEqual(10, intBox.X);
        Assert.AreEqual(9, intBox.Y);
        Assert.AreEqual(5, intBox.Z);
        Assert.AreEqual(11, intBox.Width);
        Assert.AreEqual(15, intBox.Height);
        Assert.AreEqual(19, intBox.Depth);

        GorgonBoxF newBox = intBox;

        Assert.AreEqual(10, newBox.X);
        Assert.AreEqual(9, newBox.Y);
        Assert.AreEqual(5, newBox.Z);
        Assert.AreEqual(11, newBox.Width);
        Assert.AreEqual(15, newBox.Height);
        Assert.AreEqual(19, newBox.Depth);

        Assert.AreEqual(floatRect.X, floatBox.X);
        Assert.AreEqual(floatRect.Y, floatBox.Y);
        Assert.AreEqual(floatRect.Width, floatBox.Width);
        Assert.AreEqual(floatRect.Height, floatBox.Height);

        Assert.AreEqual(intRect.X, intBox.X);
        Assert.AreEqual(intRect.Y, intBox.Y);
        Assert.AreEqual(intRect.Width, intBox.Width);
        Assert.AreEqual(intRect.Height, intBox.Height);
    }

    [TestMethod]
    public void Truncate()
    {
        GorgonBoxF floatBox = new(10.8f, 9.2f, 5.5f, 11.2f, 15.6f, 19.0f);
        GorgonBoxF.Truncate(in floatBox, out GorgonBoxF trunc);

        Assert.AreEqual(10.0f, trunc.X);
        Assert.AreEqual(9.0f, trunc.Y);
        Assert.AreEqual(5.0f, trunc.Z);
        Assert.AreEqual(11.0f, trunc.Width);
        Assert.AreEqual(15.0f, trunc.Height);
        Assert.AreEqual(19.0f, trunc.Depth);
    }

    [TestMethod]
    public void Expand()
    {
        GorgonBox box = new(new GorgonRectangle(-10, -10, 20, 20), -10, 20);
        GorgonBoxF boxf = box;

        box = GorgonBox.Expand(box, 5);

        Assert.AreEqual(-15, box.Left);
        Assert.AreEqual(-15, box.Top);
        Assert.AreEqual(-15, box.Front);
        Assert.AreEqual(15, box.Right);
        Assert.AreEqual(15, box.Bottom);
        Assert.AreEqual(15, box.Back);

        boxf = GorgonBoxF.Expand(boxf, 0.5f);
        Assert.AreEqual(-10.5f, boxf.Left);
        Assert.AreEqual(-10.5f, boxf.Top);
        Assert.AreEqual(-10.5f, boxf.Front);
        Assert.AreEqual(10.5f, boxf.Right);
        Assert.AreEqual(10.5f, boxf.Bottom);
        Assert.AreEqual(10.5f, boxf.Back);
    }

    [TestMethod]
    public void Operators()
    {
        GorgonBox box1 = new(-10, -10, -10, 20, 20, 20);
        GorgonBox box2 = new(-10, -10, -10, 20, 20, 20);
        GorgonBox box3 = new(-5, -5, -5, 10, 10, 10);

        GorgonBoxF boxf1 = new(-10.5f, -10.25f, -10.0f, 20.25f, 20.5f, 20.125f);
        GorgonBoxF boxf2 = new(-10.5f, -10.25f, -10.0f, 20.25f, 20.5f, 20.125f);
        GorgonBoxF boxf3 = new(-5.5f, -5.25f, -5.0f, 10.25f, 10.5f, 10.125f);

        Assert.AreEqual(box2, box1);
        Assert.AreEqual(boxf2, boxf1);

        // Inequality operator
        Assert.AreNotEqual(box3, box1);
        Assert.AreNotEqual(boxf3, boxf1);

        // Implicit operator
        GorgonBoxF implicitBoxf = box1;
        Assert.AreEqual(implicitBoxf.X, box1.X);
        Assert.AreEqual(implicitBoxf.Y, box1.Y);
        Assert.AreEqual(implicitBoxf.Z, box1.Z);
        Assert.AreEqual(implicitBoxf.Width, box1.Width);
        Assert.AreEqual(implicitBoxf.Height, box1.Height);
        Assert.AreEqual(implicitBoxf.Depth, box1.Depth);

        // Explicit operator
        GorgonBox explicitBox = (GorgonBox)boxf1;
        Assert.AreEqual(explicitBox.X, (int)boxf1.X);
        Assert.AreEqual(explicitBox.Y, (int)boxf1.Y);
        Assert.AreEqual(explicitBox.Z, (int)boxf1.Z);
        Assert.AreEqual(explicitBox.Width, (int)boxf1.Width);
        Assert.AreEqual(explicitBox.Height, (int)boxf1.Height);
        Assert.AreEqual(explicitBox.Depth, (int)boxf1.Depth);
    }

    [TestMethod]
    public void IntersectsWithFloat()
    {
        GorgonBoxF main = new(-20, -20, -20, 40, 40, 40);
        GorgonBoxF inside = new(-5, -5, -5, 10, 10, 10);
        GorgonBoxF outsideLeft = new(-100, 0, 0, 50, 50, 50);
        GorgonBoxF outsideRight = new(100, 0, 0, 50, 50, 50);
        GorgonBoxF outsideTop = new(0, -100, 0, 50, 50, 50);
        GorgonBoxF outsideBottom = new(0, 100, 0, 50, 50, 50);
        GorgonBoxF outsideFront = new(0, 0, -100, 50, 50, 50);
        GorgonBoxF outsideBehind = new(0, 0, 100, 50, 50, 50);
        GorgonBoxF tlf = new(-30, -30, -30, 20, 20, 20);
        GorgonBoxF trf = new(10, -30, -30, 20, 20, 20);
        GorgonBoxF blf = new(-30, 10, -30, 20, 20, 20);
        GorgonBoxF brf = new(10, 10, -30, 20, 20, 20);
        GorgonBoxF tlb = new(-30, -30, 10, 20, 20, 20);
        GorgonBoxF trb = new(10, -30, 10, 20, 20, 20);
        GorgonBoxF blb = new(-30, 10, 10, 20, 20, 20);
        GorgonBoxF brb = new(10, 10, 10, 20, 20, 20);
        GorgonBoxF leftBorderTouch = new(-30, -20, -20, 10, 40, 40);

        Assert.IsTrue(main.IntersectsWith(in inside));
        Assert.IsTrue(inside.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideLeft));
        Assert.IsFalse(outsideLeft.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideRight));
        Assert.IsFalse(outsideRight.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideTop));
        Assert.IsFalse(outsideTop.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideBottom));
        Assert.IsFalse(outsideBottom.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideFront));
        Assert.IsFalse(outsideFront.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideBehind));
        Assert.IsFalse(outsideBehind.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in tlf));
        Assert.IsTrue(tlf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in trf));
        Assert.IsTrue(trf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in blf));
        Assert.IsTrue(blf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in brf));
        Assert.IsTrue(brf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in tlb));
        Assert.IsTrue(tlb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in trb));
        Assert.IsTrue(trb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in blb));
        Assert.IsTrue(blb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in brb));
        Assert.IsTrue(brb.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in leftBorderTouch));
        Assert.IsFalse(leftBorderTouch.IntersectsWith(in main));
    }

    [TestMethod]
    public void IntersectsWith()
    {
        GorgonBox main = new(-20, -20, -20, 40, 40, 40);
        GorgonBox inside = new(-5, -5, -5, 10, 10, 10);
        GorgonBox outsideLeft = new(-100, 0, 0, 50, 50, 50);
        GorgonBox outsideRight = new(100, 0, 0, 50, 50, 50);
        GorgonBox outsideTop = new(0, -100, 0, 50, 50, 50);
        GorgonBox outsideBottom = new(0, 100, 0, 50, 50, 50);
        GorgonBox outsideFront = new(0, 0, -100, 50, 50, 50);
        GorgonBox outsideBehind = new(0, 0, 100, 50, 50, 50);
        GorgonBox tlf = new(-30, -30, -30, 20, 20, 20);
        GorgonBox trf = new(10, -30, -30, 20, 20, 20);
        GorgonBox blf = new(-30, 10, -30, 20, 20, 20);
        GorgonBox brf = new(10, 10, -30, 20, 20, 20);
        GorgonBox tlb = new(-30, -30, 10, 20, 20, 20);
        GorgonBox trb = new(10, -30, 10, 20, 20, 20);
        GorgonBox blb = new(-30, 10, 10, 20, 20, 20);
        GorgonBox brb = new(10, 10, 10, 20, 20, 20);
        GorgonBox leftBorderTouch = new(-30, -20, -20, 10, 40, 40);

        Assert.IsTrue(main.IntersectsWith(in inside));
        Assert.IsTrue(inside.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideLeft));
        Assert.IsFalse(outsideLeft.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideRight));
        Assert.IsFalse(outsideRight.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideTop));
        Assert.IsFalse(outsideTop.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideBottom));
        Assert.IsFalse(outsideBottom.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideFront));
        Assert.IsFalse(outsideFront.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in outsideBehind));
        Assert.IsFalse(outsideBehind.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in tlf));
        Assert.IsTrue(tlf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in trf));
        Assert.IsTrue(trf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in blf));
        Assert.IsTrue(blf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in brf));
        Assert.IsTrue(brf.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in tlb));
        Assert.IsTrue(tlb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in trb));
        Assert.IsTrue(trb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in blb));
        Assert.IsTrue(blb.IntersectsWith(in main));
        Assert.IsTrue(main.IntersectsWith(in brb));
        Assert.IsTrue(brb.IntersectsWith(in main));
        Assert.IsFalse(main.IntersectsWith(in leftBorderTouch));
        Assert.IsFalse(leftBorderTouch.IntersectsWith(in main));
    }

    [TestMethod]
    public void Union()
    {
        // Test case 1: Two boxes with no intersection
        GorgonBox box1 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box2 = new(20, 20, 20, 10, 10, 10);
        GorgonBox expected1 = new(0, 0, 0, 30, 30, 30);
        GorgonBox result1 = GorgonBox.Union(box1, box2);
        Assert.AreEqual(expected1, result1);

        // Test case 2: Two boxes with partial intersection
        GorgonBox box3 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box4 = new(5, 5, 5, 10, 10, 10);
        GorgonBox expected2 = new(0, 0, 0, 15, 15, 15);
        GorgonBox result2 = GorgonBox.Union(box3, box4);
        Assert.AreEqual(expected2, result2);

        // Test case 3: Two boxes with complete intersection
        GorgonBox box5 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box6 = new(5, 5, 5, 5, 5, 5);
        GorgonBox expected3 = new(0, 0, 0, 10, 10, 10);
        GorgonBox result3 = GorgonBox.Union(box5, box6);
        Assert.AreEqual(expected3, result3);

        // Test case 4: Two boxes with one box completely inside the other
        GorgonBox box7 = new(0, 0, 0, 20, 20, 20);
        GorgonBox box8 = new(5, 5, 5, 10, 10, 10);
        GorgonBox expected4 = new(0, 0, 0, 20, 20, 20);
        GorgonBox result4 = GorgonBox.Union(box7, box8);
        Assert.AreEqual(expected4, result4);
    }

    [TestMethod]
    public void Intersect()
    {
        // Test case 1: Two boxes with no intersection
        GorgonBox box1 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box2 = new(20, 20, 20, 10, 10, 10);
        GorgonBox expected1 = GorgonBox.Empty;
        GorgonBox result1 = GorgonBox.Intersect(box1, box2);
        Assert.AreEqual(expected1, result1);

        // Test case 2: Two boxes with partial intersection
        GorgonBox box3 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box4 = new(5, 5, 5, 10, 10, 10);
        GorgonBox expected2 = new(5, 5, 5, 5, 5, 5);
        GorgonBox result2 = GorgonBox.Intersect(box3, box4);
        Assert.AreEqual(expected2, result2);

        // Test case 3: Two boxes with complete intersection
        GorgonBox box5 = new(0, 0, 0, 10, 10, 10);
        GorgonBox box6 = new(5, 5, 5, 5, 5, 5);
        GorgonBox expected3 = new(5, 5, 5, 5, 5, 5);
        GorgonBox result3 = GorgonBox.Intersect(box5, box6);
        Assert.AreEqual(expected3, result3);

        // Test case 4: Two boxes with one box completely inside the other
        GorgonBox box7 = new(0, 0, 0, 20, 20, 20);
        GorgonBox box8 = new(5, 5, 5, 10, 10, 10);
        GorgonBox expected4 = new(5, 5, 5, 10, 10, 10);
        GorgonBox result4 = GorgonBox.Intersect(box7, box8);
        Assert.AreEqual(expected4, result4);
    }

    [TestMethod]
    public void TupleDeconstruction()
    {
        GorgonBoxF box = new(0, 0, 0, 10, 10, 10);
        (float x, float y, float z, float width, float height, float depth) = box;

        Assert.AreEqual(0, x);
        Assert.AreEqual(0, y);
        Assert.AreEqual(0, z);
        Assert.AreEqual(10, width);
        Assert.AreEqual(10, height);
        Assert.AreEqual(10, depth);
    }

    [TestMethod]
    public void FromLTFRBB()
    {
        // Test case 1: Valid input
        GorgonBoxF expected1 = new(-30, -30, -30, 40, 40, 40);
        GorgonBoxF result1 = GorgonBoxF.FromLTFRBB(-30, -30, -30, 10, 10, 10);
        Assert.AreEqual(expected1, result1);

        // Test case 2: Zero width, height, and depth
        GorgonBoxF expected2 = new(-30, -30, -30, 0, 0, 0);
        GorgonBoxF result2 = GorgonBoxF.FromLTFRBB(-30, -30, -30, 0, 0, 0);
        Assert.AreNotEqual(expected2, result2);

        // Test case 3: Negative width, height, and depth
        GorgonBoxF expected3 = new(-30, -30, -30, -10, -10, -10);
        GorgonBoxF result3 = GorgonBoxF.FromLTFRBB(-30, -30, -30, -10, -10, -10);
        Assert.AreNotEqual(expected3, result3);
    }

    [TestMethod]
    public void Contains()
    {
        // Test case 1: Box contains a point
        GorgonBox box1 = new(0, 0, 0, 10, 10, 10);
        Assert.IsTrue(box1.Contains(5, 5, 5));

        // Test case 2: Box does not contain a point
        GorgonBox box2 = new(0, 0, 0, 10, 10, 10);
        Assert.IsFalse(box2.Contains(15, 15, 15));

        // Test case 3: Box contains another box
        GorgonBox box3 = new(0, 0, 0, 10, 10, 10);
        GorgonBox containedBox = new(2, 2, 2, 6, 6, 6);
        Assert.IsTrue(box3.Contains(in containedBox));

        // Test case 4: Box does not contain another box
        GorgonBox box4 = new(0, 0, 0, 10, 10, 10);
        GorgonBox notContainedBox = new(12, 12, 12, 6, 6, 6);
        Assert.IsFalse(box4.Contains(in notContainedBox));

        // Test case 5: Box contains a point with float coordinates
        GorgonBoxF box5 = new(0, 0, 0, 10, 10, 10);
        Assert.IsTrue(box5.Contains(5.5f, 5.5f, 5.5f));

        // Test case 6: Box does not contain a point with float coordinates
        GorgonBoxF box6 = new(0, 0, 0, 10, 10, 10);
        Assert.IsFalse(box6.Contains(15.5f, 15.5f, 15.5f));

        // Test case 7: Box contains another box with float coordinates
        GorgonBoxF box7 = new(0, 0, 0, 10, 10, 10);
        GorgonBoxF containedBoxF = new(2, 2, 2, 6, 6, 6);
        Assert.IsTrue(box7.Contains(in containedBoxF));

        // Test case 8: Box does not contain another box with float coordinates
        GorgonBoxF box8 = new(0, 0, 0, 10, 10, 10);
        GorgonBoxF notContainedBoxF = new(12, 12, 12, 6, 6, 6);
        Assert.IsFalse(box8.Contains(in notContainedBoxF));

        // Test case 9: Box contains a point with Vector3 parameter
        GorgonBoxF box9 = new(0, 0, 0, 10, 10, 10);
        Vector3 point = new(5, 5, 5);
        Assert.IsTrue(box9.Contains(point));

        // Test case 10: Box does not contain a point with Vector3 parameter
        GorgonBoxF box10 = new(0, 0, 0, 10, 10, 10);
        Vector3 point1 = new(15, 15, 15);
        Assert.IsFalse(box10.Contains(point1));

        // Test case 11: Box contains a point with GorgonPoint parameter
        GorgonBox box11 = new(0, 0, 0, 10, 10, 10);
        GorgonPoint point2 = new(5, 5);
        Assert.IsTrue(box11.Contains(point2, 5));

        // Test case 12: Box contains a point with GorgonPoint parameter
        GorgonBox box12 = new(0, 0, 0, 10, 10, 10);
        GorgonPoint point3 = new(15, 15);
        Assert.IsFalse(box12.Contains(point3, 15));
    }

    [TestMethod]
    public void Serialization()
    {
        string expectedString = "{\"x\":10,\"y\":20,\"z\":5,\"width\":30,\"height\":40,\"depth\":50}";
        GorgonBox box = new(10, 20, 5, 30, 40, 50);

        string actualString = JsonSerializer.Serialize(box);

        Assert.AreEqual(expectedString, actualString);

        GorgonBox deserializedRectangle = JsonSerializer.Deserialize<GorgonBox>(actualString);

        Assert.AreEqual(box, deserializedRectangle);

        expectedString = "{\"x\":10.5,\"y\":20.5,\"z\":5.2,\"width\":30.25,\"height\":40.75,\"depth\":50.22}";
        GorgonBoxF boxf = new(10.5f, 20.5f, 5.2f, 30.25f, 40.75f, 50.22f);

        actualString = JsonSerializer.Serialize(boxf);

        Assert.AreEqual(expectedString, actualString);

        GorgonBoxF deserializedRectangleF = JsonSerializer.Deserialize<GorgonBoxF>(actualString);

        Assert.AreEqual(boxf, deserializedRectangleF);
    }
}
