using System;
using System.Collections.Generic;
using Gorgon.Configuration;

namespace Gorgon.Core.Tests;

// GorgonOption<T> and GorgonRangedOption<T>: typed access, clamping, and the IGorgonOption conversions on top of them.
[TestClass]
public class GorgonTypedOptionTests
{
    [TestMethod]
    public void ValueRoundTripsWithoutConversion()
    {
        GorgonOption<int> option = new("TestOption", 5);

        option.Value = 7;

        Assert.AreEqual(7, option.Value);
        Assert.AreEqual(7, option.GetValue<int>());
        Assert.AreEqual(5, option.DefaultValue);
        Assert.AreEqual(typeof(int), option.Type);
    }

    [TestMethod]
    public void RangedOptionClampsAssignedValues()
    {
        GorgonRangedOption<int> option = new("TestOption", 5, null, 0, 10);

        option.Value = 20;
        int high = option.Value;
        option.Value = -5;
        int low = option.Value;

        Assert.AreEqual(10, high);
        Assert.AreEqual(0, low);
    }

    [TestMethod]
    public void RangedOptionClampsDefaultValue()
    {
        GorgonRangedOption<int> option = new("TestOption", 50, null, 0, 10);

        Assert.AreEqual(10, option.DefaultValue);
        Assert.AreEqual(10, option.Value);
    }

    [TestMethod]
    public void RangedOptionWithOneLimitOnlyClampsThatSide()
    {
        GorgonRangedOption<int> option = new("TestOption", 5, null, minValue: 0);

        option.Value = 1000;

        Assert.AreEqual(1000, option.Value);
        Assert.IsNull(option.MaxValue);
        Assert.AreEqual(0, option.GetMaxValue<int>());
    }

    [TestMethod]
    public void RangedOptionReportsLimits()
    {
        GorgonRangedOption<float> option = GorgonOption.CreateSingleOption("TestOption", 0.5f, null, 0.0f, 1.0f);

        Assert.AreEqual(0.0f, option.MinValue);
        Assert.AreEqual(1.0f, option.MaxValue);
        Assert.AreEqual(1.0, option.GetMaxValue<double>());
    }

    [TestMethod]
    public void UnrangedOptionLimitsAreDefault()
    {
        GorgonOption<int> valueOption = new("ValueOption", 5);
        GorgonOption<string> referenceOption = new("ReferenceOption", "Default");

        Assert.AreEqual(0, valueOption.GetMinValue<int>());
        Assert.AreEqual(0, valueOption.GetMaxValue<int>());
        Assert.IsNull(referenceOption.GetMinValue<string>());
        Assert.IsNull(referenceOption.GetMaxValue<string>());
    }

    [TestMethod]
    public void SetValueConvertsThenClamps()
    {
        GorgonRangedOption<byte> option = GorgonOption.CreateByteOption("TestOption", 10, null, 0, 100);

        option.SetValue(90L);
        byte inRange = option.Value;
        option.SetValue(150L);
        byte clamped = option.Value;

        Assert.AreEqual((byte)90, inRange);
        Assert.AreEqual((byte)100, clamped);
    }

    [TestMethod]
    public void SetValueOutsideTheTargetTypeThrowsOverflow()
    {
        GorgonRangedOption<byte> option = GorgonOption.CreateByteOption("TestOption", 10, null, 0, 100);

        Assert.ThrowsExactly<OverflowException>(() => option.SetValue(500));
    }

    [TestMethod]
    public void RangedDateTimeClamps()
    {
        DateTime min = new(2020, 1, 1);
        DateTime max = new(2020, 12, 31);
        GorgonRangedOption<DateTime> option = GorgonOption.CreateDateTimeOption("TestOption", min, null, min, max);

        option.Value = new DateTime(2025, 6, 1);

        Assert.AreEqual(max, option.Value);
    }

    [TestMethod]
    public void EnumOptionReadsAsUnderlyingNumber()
    {
        GorgonOption<DayOfWeek> option = new("TestOption", DayOfWeek.Friday);

        Assert.AreEqual((int)DayOfWeek.Friday, option.GetValue<int>());
    }

    [TestMethod]
    public void InterfaceReadOfDerivedTypeNeedsNoConversion()
    {
        GorgonOption<List<int>> option = new("TestOption", [1, 2]);

        IReadOnlyList<int>? value = option.GetValue<IReadOnlyList<int>>();

        Assert.IsNotNull(value);
        Assert.AreSame(option.Value, value);
    }

    [TestMethod]
    public void CreateOptionWithValueKeepsDefault()
    {
        GorgonOption<int> option = GorgonOption.CreateOption("TestOption", 7, 5);

        Assert.AreEqual(7, option.Value);
        Assert.AreEqual(5, option.DefaultValue);
    }
}
