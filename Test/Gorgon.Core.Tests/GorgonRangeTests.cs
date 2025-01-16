using System;
using System.Text.Json;
using Gorgon.Json;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRangeTests
{
    [TestMethod]
    public void Constructors()
    {
        GorgonRange<float> range = new(100, 200);

        Assert.AreEqual(100, range.Minimum);
        Assert.AreEqual(200, range.Maximum);
        Assert.AreEqual(100, range.Range);

        range = new(200, 100);

        Assert.AreEqual(100, range.Minimum);
        Assert.AreEqual(200, range.Maximum);
        Assert.AreEqual(100, range.Range);

        range = GorgonRange<float>.FromRange(10..50);
        Assert.AreEqual(10, range.Minimum);
        Assert.AreEqual(50, range.Maximum);
        Assert.AreEqual(40, range.Range);

        range = GorgonRange<float>.FromRange(50..10);
        Assert.AreEqual(10, range.Minimum);
        Assert.AreEqual(50, range.Maximum);
        Assert.AreEqual(40, range.Range);

        Assert.AreEqual(2, GorgonRange<byte>.SizeInBytes);
        Assert.AreEqual(4, GorgonRange<short>.SizeInBytes);
        Assert.AreEqual(8, GorgonRange<float>.SizeInBytes);
        Assert.AreEqual(16, GorgonRange<double>.SizeInBytes);
    }

    [TestMethod]
    public void ExplicitConvert()
    {
        Range r = 50..100;

        GorgonRange<int> range = (GorgonRange<int>)r;
        Assert.AreEqual(50, range.Minimum);
        Assert.AreEqual(100, range.Maximum);
        Assert.AreEqual(50, range.Range);

        GorgonRange<int> range2 = new(1, 25);
        r = (Range)range2;
        Assert.AreEqual(1, r.Start);
        Assert.AreEqual(25, r.End);

        range2 = (GorgonRange<int>)(100..50);
        r = (Range)range2;
        Assert.AreEqual(50, r.Start);
        Assert.AreEqual(100, r.End);
    }

    [TestMethod]
    public void IsEmpty()
    {
        GorgonRange<double> empty = GorgonRange<double>.Empty;
        GorgonRange<float> empty2 = new(0, 0);

        Assert.IsTrue(empty.IsEmpty);
        Assert.IsTrue(empty2.IsEmpty);
    }

    [TestMethod]
    public void Operators()
    {
        GorgonRange<int> left = new(10, 50);
        GorgonRange<int> right = new(5, 60);
        GorgonRange<int> same = left;

        Assert.AreNotEqual(left, right);
        Assert.AreEqual(left, same);

        Assert.IsTrue(left < right);
        Assert.IsTrue(right > left);
        Assert.IsTrue(left <= right);
        Assert.IsTrue(right >= left);
        Assert.IsTrue(left <= same);
        Assert.IsTrue(left >= same);
    }

    [TestMethod]
    public void CompareTo()
    {
        GorgonRange<int> left = new(10, 50);
        GorgonRange<int> right = new(5, 60);
        GorgonRange<int> same = left;

        Assert.AreEqual(-1, left.CompareTo(right));
        Assert.AreEqual(1, right.CompareTo(left));
        Assert.AreEqual(0, left.CompareTo(same));
    }

    [TestMethod]
    public void Range()
    {
        GorgonRange<int> range = new(10, 50);
        Assert.AreEqual(40, range.Range);
    }

    [TestMethod]
    public void Union()
    {
        GorgonRange<int> left = new(10, 15);
        GorgonRange<int> right = new(25, 60);

        GorgonRange<int> union = GorgonRange<int>.Union(left, right);

        Assert.AreEqual(10, union.Minimum);
        Assert.AreEqual(60, union.Maximum);

        left = new GorgonRange<int>(10, 70);
        union = GorgonRange<int>.Union(left, right);

        Assert.AreEqual(10, union.Minimum);
        Assert.AreEqual(70, union.Maximum);

        left = new GorgonRange<int>(30, 70);
        union = GorgonRange<int>.Union(left, right);

        Assert.AreEqual(25, union.Minimum);
        Assert.AreEqual(70, union.Maximum);

        left = new GorgonRange<int>(30, 40);
        union = GorgonRange<int>.Union(left, right);

        Assert.AreEqual(25, union.Minimum);
        Assert.AreEqual(60, union.Maximum);

        left = new GorgonRange<int>(65, 70);
        union = GorgonRange<int>.Union(left, right);

        Assert.AreEqual(25, union.Minimum);
        Assert.AreEqual(70, union.Maximum);
    }

    [TestMethod]
    public void Intersects()
    {
        GorgonRange<int> left = new(10, 45);
        GorgonRange<int> right = new(25, 60);

        Assert.IsTrue(left.Intersects(right));
        Assert.IsTrue(right.Intersects(left));

        left = new GorgonRange<int>(30, 40);
        Assert.IsTrue(left.Intersects(right));
        Assert.IsTrue(right.Intersects(left));

        left = new GorgonRange<int>(40, 100);
        Assert.IsTrue(left.Intersects(right));
        Assert.IsTrue(right.Intersects(left));

        left = new GorgonRange<int>(0, 5);
        Assert.IsFalse(left.Intersects(right));
        Assert.IsFalse(right.Intersects(left));

        left = new GorgonRange<int>(70, 200);
        Assert.IsFalse(left.Intersects(right));
        Assert.IsFalse(right.Intersects(left));
    }

    [TestMethod]
    public void Intersect()
    {
        GorgonRange<int> left = new(10, 45);
        GorgonRange<int> right = new(25, 60);

        GorgonRange<int> intersection = GorgonRange<int>.Intersect(left, right);

        Assert.AreEqual(25, intersection.Minimum);
        Assert.AreEqual(45, intersection.Maximum);

        left = new GorgonRange<int>(30, 45);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.AreEqual(30, intersection.Minimum);
        Assert.AreEqual(45, intersection.Maximum);

        left = new GorgonRange<int>(50, 80);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.AreEqual(50, intersection.Minimum);
        Assert.AreEqual(60, intersection.Maximum);

        left = new GorgonRange<int>(60, 80);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.AreEqual(60, intersection.Minimum);
        Assert.AreEqual(60, intersection.Maximum);

        left = new GorgonRange<int>(0, 25);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.AreEqual(25, intersection.Minimum);
        Assert.AreEqual(25, intersection.Maximum);

        left = new GorgonRange<int>(70, 80);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.IsTrue(intersection.IsEmpty);

        left = new GorgonRange<int>(0, 20);
        intersection = GorgonRange<int>.Intersect(left, right);

        Assert.IsTrue(intersection.IsEmpty);
    }

    [TestMethod]
    public void Contains()
    {
        GorgonRange<int> range = new(10, 45);

        Assert.IsTrue(range.Contains(10));
        Assert.IsTrue(range.Contains(45));
        Assert.IsTrue(range.Contains(20));

        Assert.IsFalse(range.Contains(2));
        Assert.IsFalse(range.Contains(60));
    }

    [TestMethod]
    public void Expand()
    {
        GorgonRange<int> range = new(10, 45);
        GorgonRange<int> actual = GorgonRange<int>.Expand(range, 30);

        Assert.AreEqual(-20, actual.Minimum);
        Assert.AreEqual(75, actual.Maximum);
        Assert.AreEqual(95, actual.Range);

        actual = GorgonRange<int>.Expand(actual, -30);
        Assert.AreEqual(10, actual.Minimum);
        Assert.AreEqual(45, actual.Maximum);
        Assert.AreEqual(35, actual.Range);
    }

    [TestMethod]
    public void Shift()
    {
        GorgonRange<int> range = new(10, 45);
        int expectedRange = range.Range;
        GorgonRange<int> actual = GorgonRange<int>.Shift(range, 30);

        Assert.AreEqual(40, actual.Minimum);
        Assert.AreEqual(75, actual.Maximum);
        Assert.AreEqual(expectedRange, actual.Range);

        actual = GorgonRange<int>.Shift(actual, -30);
        Assert.AreEqual(10, actual.Minimum);
        Assert.AreEqual(45, actual.Maximum);
        Assert.AreEqual(expectedRange, actual.Range);
    }

    [TestMethod]
    public void Clamp()
    {
        GorgonRange<int> range = new(10, 45);

        Assert.AreEqual(10, range.Clamp(5));
        Assert.AreEqual(45, range.Clamp(80));
        Assert.AreEqual(20, range.Clamp(20));
    }

    [TestMethod]
    public void Serialize()
    {
        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new GorgonRangeFloatJsonConverter(),
                new GorgonRangeDoubleJsonConverter(),
                new GorgonRangeDecimalJsonConverter(),
                new GorgonRangeByteJsonConverter(),
                new GorgonRangeInt16JsonConverter(),
                new GorgonRangeUInt16JsonConverter(),
                new GorgonRangeInt32JsonConverter(),
                new GorgonRangeUInt32JsonConverter(),
                new GorgonRangeInt64JsonConverter(),
                new GorgonRangeUInt64JsonConverter()
            }
        };

        GorgonRange<float> rfloatExpected = new(10, 50);

        string json = JsonSerializer.Serialize(rfloatExpected, options);

        GorgonRange<float> rfloatActual = JsonSerializer.Deserialize<GorgonRange<float>>(json, options);

        Assert.AreEqual(rfloatExpected, rfloatActual);

        GorgonRange<double> rdoubleExpected = new(10, 50);

        json = JsonSerializer.Serialize(rdoubleExpected, options);

        GorgonRange<double> rdoubleActual = JsonSerializer.Deserialize<GorgonRange<double>>(json, options);

        Assert.AreEqual(rdoubleExpected, rdoubleActual);

        GorgonRange<decimal> rdecimalExpected = new(10, 50);

        json = JsonSerializer.Serialize(rdecimalExpected, options);

        GorgonRange<decimal> rdecimalActual = JsonSerializer.Deserialize<GorgonRange<decimal>>(json, options);

        Assert.AreEqual(rdecimalExpected, rdecimalActual);

        GorgonRange<byte> rbyteExpected = new(10, 50);

        json = JsonSerializer.Serialize(rbyteExpected, options);

        GorgonRange<byte> rbyteActual = JsonSerializer.Deserialize<GorgonRange<byte>>(json, options);

        Assert.AreEqual(rbyteExpected, rbyteActual);

        GorgonRange<short> rshortExpected = new(10, 50);

        json = JsonSerializer.Serialize(rshortExpected, options);

        GorgonRange<short> rshortActual = JsonSerializer.Deserialize<GorgonRange<short>>(json, options);

        Assert.AreEqual(rshortExpected, rshortActual);

        GorgonRange<ushort> rushortExpected = new(10, 50);

        json = JsonSerializer.Serialize(rushortExpected, options);

        GorgonRange<ushort> rushortActual = JsonSerializer.Deserialize<GorgonRange<ushort>>(json, options);

        Assert.AreEqual(rushortExpected, rushortActual);

        GorgonRange<int> rintExpected = new(10, 50);

        json = JsonSerializer.Serialize(rintExpected, options);

        GorgonRange<int> rintActual = JsonSerializer.Deserialize<GorgonRange<int>>(json, options);

        Assert.AreEqual(rintExpected, rintActual);

        GorgonRange<uint> ruintExpected = new(10, 50);

        json = JsonSerializer.Serialize(ruintExpected, options);

        GorgonRange<uint> ruintActual = JsonSerializer.Deserialize<GorgonRange<uint>>(json, options);

        Assert.AreEqual(ruintExpected, ruintActual);

        GorgonRange<long> rlongExpected = new(10, 50);

        json = JsonSerializer.Serialize(rlongExpected, options);

        GorgonRange<long> rlongActual = JsonSerializer.Deserialize<GorgonRange<long>>(json, options);

        Assert.AreEqual(rlongExpected, rlongActual);

        GorgonRange<ulong> rulongExpected = new(10, 50);

        json = JsonSerializer.Serialize(rulongExpected, options);

        GorgonRange<ulong> rulongActual = JsonSerializer.Deserialize<GorgonRange<ulong>>(json, options);

        Assert.AreEqual(rulongExpected, rulongActual);
    }
}
