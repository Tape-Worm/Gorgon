using System;
using System.Linq;
using Gorgon.Math;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonMathTests
{
    [TestMethod]
    public void ToRadians()
    {
        Assert.AreEqual(0.7853982f, 45.0f.ToRadians());
        Assert.AreEqual(0.7853981625, 45.0.ToRadians());
        Assert.AreEqual(0.78539816250M, 45.0M.ToRadians());
    }

    [TestMethod]
    public void ToDegrees()
    {
        Assert.AreEqual(45.0f, 0.7853982f.ToDegrees());
        Assert.AreEqual(45.0, 0.7853981625.ToDegrees());
        Assert.AreEqual(45.0M, 0.78539816250M.ToDegrees());
    }

    [TestMethod]
    public void EqualsEpsilon()
    {
        float left = 0.00322124f;
        float right = 0.00322124f;

        Assert.IsTrue(left.EqualsEpsilon(right));

        left = 0.0000000012f;
        right = 0.0000000012f;

        Assert.IsTrue(left.EqualsEpsilon(right));

        left = 0.00300002f;
        right = 0.003000092f;

        Assert.IsTrue(left.EqualsEpsilon(right));
        Assert.IsFalse(left.EqualsEpsilon(right, 1E-10f));

        left = 0.192256f;
        right = 0.384512f;

        Assert.IsFalse(left.EqualsEpsilon(right));

        double left2 = 0.00322124;
        double right2 = 0.00322124;

        Assert.IsTrue(left2.EqualsEpsilon(right2));

        left2 = 0.0000000012;
        right2 = 0.0000000012;

        Assert.IsTrue(left2.EqualsEpsilon(right2));

        left2 = 0.00300001111112;
        right2 = 0.00300001111192;

        Assert.IsTrue(left2.EqualsEpsilon(right2));
        Assert.IsFalse(left2.EqualsEpsilon(right2, 1E-16f));

        left2 = 0.192256;
        right2 = 0.384512;

        Assert.IsFalse(left2.EqualsEpsilon(right2));
    }

    [TestMethod]
    public void FastSin()
    {
        float rads = 45.0f.ToRadians();

        float sinVal = MathF.Sin(rads);
        float fastSinVal = rads.FastSin();

        Assert.IsTrue(sinVal.EqualsEpsilon(fastSinVal));

        rads = 180.0f.ToRadians();

        sinVal = MathF.Sin(rads);
        fastSinVal = rads.FastSin();

        Assert.IsTrue(sinVal.EqualsEpsilon(fastSinVal));

        rads = 192.0f.ToRadians();

        sinVal = MathF.Sin(rads);
        fastSinVal = rads.FastSin();

        Assert.IsTrue(sinVal.EqualsEpsilon(fastSinVal));
    }

    [TestMethod]
    public void FastCos()
    {
        float rads = 45.0f.ToRadians();

        float cosVal = MathF.Cos(rads);
        float fastCosVal = rads.FastCos();

        Assert.IsTrue(cosVal.EqualsEpsilon(fastCosVal));

        rads = 180.0f.ToRadians();

        cosVal = MathF.Cos(rads);
        fastCosVal = rads.FastCos();

        Assert.IsTrue(cosVal.EqualsEpsilon(fastCosVal));

        rads = 192.0f.ToRadians();

        cosVal = MathF.Cos(rads);
        fastCosVal = rads.FastCos();

        Assert.IsTrue(cosVal.EqualsEpsilon(fastCosVal));
    }

    [TestMethod]
    public void Max()
    {
        float floatVal = 192.0f.Max(256.0f);

        Assert.AreEqual(256.0f, floatVal);

        double doubleVal = 192.0.Max(256.0);

        Assert.AreEqual(256.0, doubleVal);

        decimal decimalVal = 192.0M.Max(256.0M);

        Assert.AreEqual(256.0M, decimalVal);

        long longVal = 192L.Max(256L);

        Assert.AreEqual(256L, longVal);

        ulong ulongVal = 192UL.Max(256UL);

        Assert.AreEqual(256UL, ulongVal);

        int intVal = 192.Max(256);

        Assert.AreEqual(256, intVal);

        ulong uintVal = 192U.Max(256U);

        Assert.AreEqual(256U, uintVal);

        short shortVal = ((short)192).Max(256);

        Assert.AreEqual((short)256, shortVal);

        ushort ushortVal = ((ushort)192).Max(256);

        Assert.AreEqual((ushort)256, ushortVal);

        byte byteVal = ((byte)192).Max(255);

        Assert.AreEqual((byte)255, byteVal);
    }

    [TestMethod]
    public void Min()
    {
        float floatVal = 192.0f.Min(256.0f);

        Assert.AreEqual(192.0f, floatVal);

        double doubleVal = 192.0.Min(256.0);

        Assert.AreEqual(192.0, doubleVal);

        decimal decimalVal = 192.0M.Min(256.0M);

        Assert.AreEqual(192.0M, decimalVal);

        long longVal = 192L.Min(256L);

        Assert.AreEqual(192L, longVal);

        ulong ulongVal = 192UL.Min(256UL);

        Assert.AreEqual(192UL, ulongVal);

        int intVal = 192.Min(256);

        Assert.AreEqual(192, intVal);

        ulong uintVal = 192U.Min(256U);

        Assert.AreEqual(192U, uintVal);

        short shortVal = ((short)192).Min(256);

        Assert.AreEqual((short)192, shortVal);

        ushort ushortVal = ((ushort)192).Min(256);

        Assert.AreEqual((ushort)192, ushortVal);

        byte byteVal = ((byte)192).Min(255);

        Assert.AreEqual((byte)192, byteVal);
    }

    [TestMethod]
    public void Abs()
    {
        float floatVal = (-192.0f).Abs();

        Assert.AreEqual(192.0f, floatVal);

        double doubleVal = (-192.0).Abs();

        Assert.AreEqual(192.0, doubleVal);

        decimal decimalVal = (-192.0M).Abs();

        Assert.AreEqual(192.0M, decimalVal);

        long longVal = (-192L).Abs();

        Assert.AreEqual(192L, longVal);

        int intVal = (-192).Abs();

        Assert.AreEqual(192, intVal);

        int shortVal = ((short)-192).Abs();

        Assert.AreEqual((short)192, shortVal);
    }

    [TestMethod]
    public void Round()
    {
        float floatVal = 123.654f;

        Assert.AreEqual(124.0f, floatVal.Round());
        Assert.AreEqual(123.7f, floatVal.Round(1));
        Assert.AreEqual(123.0f, floatVal.Round(rounding: MidpointRounding.ToZero));
        Assert.AreEqual(124.0f, floatVal.Round(rounding: MidpointRounding.AwayFromZero));

        double doubleVal = 123.654;

        Assert.AreEqual(124.0, doubleVal.Round());
        Assert.AreEqual(123.7, doubleVal.Round(1));
        Assert.AreEqual(123.0, doubleVal.Round(rounding: MidpointRounding.ToZero));
        Assert.AreEqual(124.0, doubleVal.Round(rounding: MidpointRounding.AwayFromZero));

        decimal decimalVal = 123.654M;

        Assert.AreEqual(124.0M, decimalVal.Round());
        Assert.AreEqual(123.7M, decimalVal.Round(1));
        Assert.AreEqual(123.0M, decimalVal.Round(rounding: MidpointRounding.ToZero));
        Assert.AreEqual(124.0M, decimalVal.Round(rounding: MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void Sqrt()
    {
        float floatVal = 512.256f;

        Assert.AreEqual(22.6330719f, floatVal.Sqrt());

        double doubleVal = 512.256;

        Assert.AreEqual(22.633073145288954, doubleVal.Sqrt());
    }

    [TestMethod]
    public void InvSqrt()
    {
        float floatVal = 512.256f;

        Assert.AreEqual(0.04418313f, floatVal.InverseSqrt());

        double doubleVal = 512.256;

        Assert.AreEqual(0.044183129422181396, doubleVal.InverseSqrt());
    }

    [TestMethod]
    public void Sin()
    {
        double expectedDouble = 0.70710678055195575;
        float expectedFloat = 0.707106769f;

        Assert.AreEqual(expectedDouble, (0.7853981625).Sin());
        Assert.AreEqual(expectedFloat, (0.7853982f).Sin());
    }

    [TestMethod]
    public void Cos()
    {
        double expectedDouble = 0.70710678182113929;
        float expectedFloat = 0.707106769f;

        Assert.AreEqual(expectedDouble, (0.7853981625).Cos());
        Assert.AreEqual(expectedFloat, (0.7853982f).Cos());
    }

    [TestMethod]
    public void ASin()
    {
        double expectedDouble = 0.7853981625;
        float expectedFloat = 0.7853981f;

        Assert.AreEqual(expectedDouble, (0.70710678055195575).ASin());
        Assert.AreEqual(expectedFloat, (0.707106769f).ASin());
    }

    [TestMethod]
    public void ACos()
    {
        double expectedDouble = 0.7853981625;
        float expectedFloat = 0.7853982f;

        Assert.AreEqual(expectedDouble, (0.70710678182113929).ACos());
        Assert.AreEqual(expectedFloat, (0.707106769f).ACos());
    }

    [TestMethod]
    public void Tan()
    {
        double expectedDouble = 0.99999999820510344;
        float expectedFloat = 1f;

        Assert.AreEqual(expectedDouble, (0.7853981625).Tan());
        Assert.AreEqual(expectedFloat, (0.7853982f).Tan());
    }

    [TestMethod]
    public void ATan()
    {
        double expectedDouble = 0.7853981625;
        float expectedFloat = 0.7853982f;

        Assert.AreEqual(expectedDouble, (0.99999999820510344).ATan());
        Assert.AreEqual(expectedFloat, (1f).ATan());
    }

    [TestMethod]
    public void ATan2()
    {
        double expectedDouble = 0.7853981633974483;
        float expectedFloat = 0.7853982f;

        Assert.AreEqual(expectedDouble, (5.0).ATan(5));
        Assert.AreEqual(expectedFloat, (5f).ATan(5f));
    }

    [TestMethod]
    public void Exp()
    {
        double expectedDouble = 1.739274941520501E+18;
        float expectedFloat = 1.739275E+18f;

        Assert.AreEqual(expectedDouble, (42.0).Exp());
        Assert.AreEqual(expectedFloat, (42.0f).Exp());
    }

    [TestMethod]
    public void Pow()
    {
        double expectedDouble = 256.0;
        float expectedFloat = 256.0f;

        Assert.AreEqual(expectedDouble, (2.0).Pow(8.0));
        Assert.AreEqual(expectedFloat, (2.0f).Pow(8.0f));
    }

    [TestMethod]
    public void Log()
    {
        double expectedDouble = 0.33333333333333337;
        float expectedFloat = 0.333333343f;

        Assert.AreEqual(expectedDouble, (2.0).Log(8.0));
        Assert.AreEqual(expectedFloat, (2.0f).Log(8.0f));
    }

    [TestMethod]
    public void FastFloor()
    {
        double expectedDouble = 3;
        double expectedFloat = 3;

        Assert.AreEqual(expectedDouble, (3.14159).FastFloor());
        Assert.AreEqual(expectedFloat, (3.14159f).FastFloor());
    }

    [TestMethod]
    public void FastCeiling()
    {
        double expectedDouble = 4;
        double expectedFloat = 4;

        Assert.AreEqual(expectedDouble, (3.14159).FastCeiling());
        Assert.AreEqual(expectedFloat, (3.14159f).FastCeiling());
    }

    [TestMethod]
    public void Sign()
    {
        short expectedShortNeg = -1;
        int expectedIntNeg = -1;
        long expectedLongNeg = -1;
        float expectedFloatNeg = -1;
        double expectedDoubleNeg = -1;
        decimal expectedDecimalNeg = -1;

        short expectedShortPos = 1;
        int expectedIntPos = 1;
        long expectedLongPos = 1;
        float expectedFloatPos = 1;
        double expectedDoublePos = 1;
        decimal expectedDecimalPos = 1;

        short expectedShortZero = 0;
        int expectedIntZero = 0;
        long expectedLongZero = 0;
        float expectedFloatZero = 0;
        double expectedDoubleZero = 0;
        decimal expectedDecimalZero = 0;

        Assert.AreEqual(expectedShortNeg, ((short)-1234).Sign());
        Assert.AreEqual(expectedIntNeg, (-123456).Sign());
        Assert.AreEqual(expectedLongNeg, (-1234567890L).Sign());
        Assert.AreEqual(expectedFloatNeg, (-1234.5f).Sign());
        Assert.AreEqual(expectedDoubleNeg, (-1234.5).Sign());
        Assert.AreEqual(expectedDecimalNeg, (-1234.5M).Sign());

        Assert.AreEqual(expectedShortPos, ((short)1234).Sign());
        Assert.AreEqual(expectedIntPos, (123456).Sign());
        Assert.AreEqual(expectedLongPos, (1234567890L).Sign());
        Assert.AreEqual(expectedFloatPos, (1234.5f).Sign());
        Assert.AreEqual(expectedDoublePos, (1234.5).Sign());
        Assert.AreEqual(expectedDecimalPos, (1234.5M).Sign());

        Assert.AreEqual(expectedShortZero, ((short)0).Sign());
        Assert.AreEqual(expectedIntZero, (0).Sign());
        Assert.AreEqual(expectedLongZero, (0).Sign());
        Assert.AreEqual(expectedFloatZero, (0.0f).Sign());
        Assert.AreEqual(expectedDoubleZero, (0.0).Sign());
        Assert.AreEqual(expectedDecimalZero, (0.0M).Sign());
    }

    [TestMethod]
    public void Clamp()
    {
        byte expectedByteMin = 5;
        short expectedShortMin = -12;
        ushort expectedUShortMin = 10;
        int expectedIntMin = -60;
        uint expectedUIntMin = 60U;
        long expectedLongMin = -1024;
        ulong expectedULongMin = 777;
        float expectedFloatMin = 3.5f;
        double expectedDoubleMin = 3.5;
        decimal expectedDecimalMin = -3.5M;

        byte expectedByteMax = 42;
        short expectedShortMax = 12;
        ushort expectedUShortMax = 42;
        int expectedIntMax = 42;
        uint expectedUIntMax = 600U;
        long expectedLongMax = 1024;
        ulong expectedULongMax = 2048;
        float expectedFloatMax = 30.5f;
        double expectedDoubleMax = 30.5;
        decimal expectedDecimalMax = 30.5M;

        Assert.AreEqual(expectedByteMin, (byte.MinValue).Clamp(expectedByteMin, expectedByteMax));
        Assert.AreEqual(expectedByteMax, (byte.MaxValue).Clamp(expectedByteMin, expectedByteMax));
        Assert.AreEqual(expectedShortMin, (short.MinValue).Clamp(expectedShortMin, expectedShortMax));
        Assert.AreEqual(expectedShortMax, (short.MaxValue).Clamp(expectedShortMin, expectedShortMax));
        Assert.AreEqual(expectedUShortMin, (ushort.MinValue).Clamp(expectedUShortMin, expectedUShortMax));
        Assert.AreEqual(expectedUShortMax, (ushort.MaxValue).Clamp(expectedUShortMin, expectedUShortMax));
        Assert.AreEqual(expectedIntMin, (int.MinValue).Clamp(expectedIntMin, expectedIntMax));
        Assert.AreEqual(expectedIntMax, (int.MaxValue).Clamp(expectedIntMin, expectedIntMax));
        Assert.AreEqual(expectedUIntMin, (uint.MinValue).Clamp(expectedUIntMin, expectedUIntMax));
        Assert.AreEqual(expectedUIntMax, (uint.MaxValue).Clamp(expectedUIntMin, expectedUIntMax));
        Assert.AreEqual(expectedLongMin, (long.MinValue).Clamp(expectedLongMin, expectedLongMax));
        Assert.AreEqual(expectedLongMax, (long.MaxValue).Clamp(expectedLongMin, expectedLongMax));
        Assert.AreEqual(expectedULongMin, (ulong.MinValue).Clamp(expectedULongMin, expectedULongMax));
        Assert.AreEqual(expectedULongMax, (ulong.MaxValue).Clamp(expectedULongMin, expectedULongMax));
        Assert.AreEqual(expectedFloatMin, (float.MinValue).Clamp(expectedFloatMin, expectedFloatMax));
        Assert.AreEqual(expectedFloatMax, (float.MaxValue).Clamp(expectedFloatMin, expectedFloatMax));
        Assert.AreEqual(expectedDoubleMin, (double.MinValue).Clamp(expectedDoubleMin, expectedDoubleMax));
        Assert.AreEqual(expectedDoubleMax, (double.MaxValue).Clamp(expectedDoubleMin, expectedDoubleMax));
        Assert.AreEqual(expectedDecimalMin, (decimal.MinValue).Clamp(expectedDecimalMin, expectedDecimalMax));
        Assert.AreEqual(expectedDecimalMax, (decimal.MaxValue).Clamp(expectedDecimalMin, expectedDecimalMax));
    }

    [TestMethod]
    public void LimitAngle()
    {
        float expectedFloat1 = 315.0f;
        float expectedFloat2 = 45.0f;
        double expectedDouble1 = 315.0;
        double expectedDouble2 = 45.0;
        decimal expectedDecimal1 = 315.0M;
        decimal expectedDecimal2 = 45.0M;

        Assert.AreEqual(expectedFloat1, (-45.0f).WrapAngle());
        Assert.AreEqual(expectedFloat2, (405.0f).WrapAngle());

        Assert.AreEqual(expectedFloat1, (-3645.0f).WrapAngle());
        Assert.AreEqual(expectedFloat2, (3645.0f).WrapAngle());

        Assert.AreEqual(expectedDouble1, (-45.0).WrapAngle());
        Assert.AreEqual(expectedDouble2, (405.0).WrapAngle());

        Assert.AreEqual(expectedDouble1, (-3645.0).WrapAngle());
        Assert.AreEqual(expectedDouble2, (3645.0).WrapAngle());

        Assert.AreEqual(expectedDecimal1, (-45.0M).WrapAngle());
        Assert.AreEqual(expectedDecimal2, (405.0M).WrapAngle());

        Assert.AreEqual(expectedDecimal1, (-3645.0M).WrapAngle());
        Assert.AreEqual(expectedDecimal2, (3645.0M).WrapAngle());
    }

    [TestMethod]
    public void Lerp()
    {
        float expectedFloat = 75.0f;
        double expectedDouble = 75.0;
        decimal expectedDecimal = 75.0M;

        Assert.AreEqual(expectedFloat, (0.0f).Lerp(100.0f, 0.75f));
        Assert.AreEqual(expectedDouble, (0.0).Lerp(100.0, 0.75));
        Assert.AreEqual(expectedDecimal, (0.0M).Lerp(100.0M, 0.75M));
    }
}
