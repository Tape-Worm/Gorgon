using System;
using Gorgon.Configuration;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonOptionTests
{
    [TestMethod]
    public void CreateByteOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        string? description = "Test description";
        byte? minValue = 5;
        byte? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<byte>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<byte>());
        Assert.AreEqual(maxValue, option.GetMaxValue<byte>());
    }

    [TestMethod]
    public void SetValueByteValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        byte newValue = 15;
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<byte>());
    }

    [TestMethod]
    public void CreateByteOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        byte defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateByteOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueByteReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue);

        // Act
        byte? result = option.GetDefaultValue<byte>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueByteReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        byte minValue = 5;
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue, null, minValue);

        // Act
        byte? result = option.GetMinValue<byte>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueByteReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        byte maxValue = 15;
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue, null, null, maxValue);

        // Act
        byte? result = option.GetMaxValue<byte>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueByteValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        byte minValue = 5;
        byte newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<byte>());
    }

    [TestMethod]
    public void SetValueByteValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        byte defaultValue = 10;
        byte maxValue = 15;
        byte newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateByteOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<byte>());
    }

    [TestMethod]
    public void CreateSByteOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        string? description = "Test description";
        sbyte? minValue = 5;
        sbyte? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<sbyte>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<sbyte>());
        Assert.AreEqual(maxValue, option.GetMaxValue<sbyte>());
    }

    [TestMethod]
    public void SetValueSByteValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        sbyte newValue = 15;
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<sbyte>());
    }

    [TestMethod]
    public void CreateSByteOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        sbyte defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateSByteOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueSByteReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue);

        // Act
        sbyte? result = option.GetDefaultValue<sbyte>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueSByteReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        sbyte minValue = 5;
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue, null, minValue);

        // Act
        sbyte? result = option.GetMinValue<sbyte>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueSByteReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        sbyte maxValue = 15;
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue, null, null, maxValue);

        // Act
        sbyte? result = option.GetMaxValue<sbyte>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueSByteValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        sbyte minValue = 5;
        sbyte newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<sbyte>());
    }

    [TestMethod]
    public void SetValueSByteValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        sbyte defaultValue = 10;
        sbyte maxValue = 15;
        sbyte newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateSByteOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<sbyte>());
    }

    [TestMethod]
    public void CreateInt16OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        string? description = "Test description";
        short? minValue = 5;
        short? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<short>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<short>());
        Assert.AreEqual(maxValue, option.GetMaxValue<short>());
    }

    [TestMethod]
    public void SetValueInt16ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        short newValue = 15;
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<short>());
    }

    [TestMethod]
    public void CreateInt16OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        short defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateInt16Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueInt16ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue);

        // Act
        short? result = option.GetDefaultValue<short>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueInt16ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        short minValue = 5;
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue, null, minValue);

        // Act
        short? result = option.GetMinValue<short>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueInt16ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        short maxValue = 15;
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue, null, null, maxValue);

        // Act
        short? result = option.GetMaxValue<short>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueInt16ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        short minValue = 5;
        short newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<short>());
    }

    [TestMethod]
    public void SetValueInt16ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        short defaultValue = 10;
        short maxValue = 15;
        short newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateInt16Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<short>());
    }

    [TestMethod]
    public void CreateUInt16OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        string? description = "Test description";
        ushort? minValue = 5;
        ushort? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<ushort>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<ushort>());
        Assert.AreEqual(maxValue, option.GetMaxValue<ushort>());
    }

    [TestMethod]
    public void SetValueUInt16ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        ushort newValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<ushort>());
    }

    [TestMethod]
    public void CreateUInt16OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        ushort defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateUInt16Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueUInt16ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue);

        // Act
        ushort? result = option.GetDefaultValue<ushort>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueUInt16ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        ushort minValue = 5;
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue, null, minValue);

        // Act
        ushort? result = option.GetMinValue<ushort>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueUInt16ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        ushort maxValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue, null, null, maxValue);

        // Act
        ushort? result = option.GetMaxValue<ushort>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueUInt16ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        ushort minValue = 5;
        ushort newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<ushort>());
    }

    [TestMethod]
    public void CreateInt32OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        string? description = "Test description";
        int? minValue = 5;
        int? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<int>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<int>());
        Assert.AreEqual(maxValue, option.GetMaxValue<int>());
    }

    [TestMethod]
    public void SetValueInt32ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        int newValue = 15;
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<int>());
    }

    [TestMethod]
    public void CreateInt32OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        int defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateInt32Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueInt32ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue);

        // Act
        int? result = option.GetDefaultValue<int>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueInt32ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        int minValue = 5;
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue, null, minValue);

        // Act
        int? result = option.GetMinValue<int>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueInt32ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        int maxValue = 15;
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue, null, null, maxValue);

        // Act
        int? result = option.GetMaxValue<int>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueInt32ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        int minValue = 5;
        int newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<int>());
    }

    [TestMethod]
    public void SetValueInt32ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        int defaultValue = 10;
        int maxValue = 15;
        int newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateInt32Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<int>());
    }

    [TestMethod]
    public void SetValueUInt16ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        ushort defaultValue = 10;
        ushort maxValue = 15;
        ushort newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateUInt16Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<ushort>());
    }

    [TestMethod]
    public void CreateUInt32OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        string? description = "Test description";
        uint? minValue = 5;
        uint? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<uint>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<uint>());
        Assert.AreEqual(maxValue, option.GetMaxValue<uint>());
    }

    [TestMethod]
    public void SetValueUInt32ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        uint newValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<uint>());
    }

    [TestMethod]
    public void CreateUInt32OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        uint defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateUInt32Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueUInt32ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue);

        // Act
        uint? result = option.GetDefaultValue<uint>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueUInt32ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        uint minValue = 5;
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue, null, minValue);

        // Act
        uint? result = option.GetMinValue<uint>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueUInt32ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        uint maxValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue, null, null, maxValue);

        // Act
        uint? result = option.GetMaxValue<uint>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueUInt32ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        uint minValue = 5;
        uint newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<uint>());
    }

    [TestMethod]
    public void CreateInt64OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        string? description = "Test description";
        long? minValue = 5;
        long? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<long>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<long>());
        Assert.AreEqual(maxValue, option.GetMaxValue<long>());
    }

    [TestMethod]
    public void SetValueInt64ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        long newValue = 15;
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<long>());
    }

    [TestMethod]
    public void CreateInt64OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        long defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateInt64Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueInt64ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue);

        // Act
        long? result = option.GetDefaultValue<long>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueInt64ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        long minValue = 5;
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue, null, minValue);

        // Act
        long? result = option.GetMinValue<long>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueInt64ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        long maxValue = 15;
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue, null, null, maxValue);

        // Act
        long? result = option.GetMaxValue<long>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueInt64ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        long minValue = 5;
        long newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<long>());
    }

    [TestMethod]
    public void SetValueInt64ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        long maxValue = 15;
        long newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateInt64Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<long>());
    }

    [TestMethod]
    public void SetValueUInt32ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        uint defaultValue = 10;
        uint maxValue = 15;
        uint newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateUInt32Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<uint>());
    }

    [TestMethod]
    public void CreateUInt64OptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        string? description = "Test description";
        ulong? minValue = 5;
        ulong? maxValue = 15;

        // Act
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<ulong>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<ulong>());
        Assert.AreEqual(maxValue, option.GetMaxValue<ulong>());
    }

    [TestMethod]
    public void SetValueUInt64ValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        ulong newValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<ulong>());
    }

    [TestMethod]
    public void CreateUInt64OptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        ulong defaultValue = 10;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateUInt64Option(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueUInt64ReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue);

        // Act
        ulong? result = option.GetDefaultValue<ulong>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueUInt64ReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        ulong minValue = 5;
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue, null, minValue);

        // Act
        ulong? result = option.GetMinValue<ulong>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueUInt64ReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        ulong maxValue = 15;
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue, null, null, maxValue);

        // Act
        ulong? result = option.GetMaxValue<ulong>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueUInt64ValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        ulong minValue = 5;
        ulong newValue = 3; // Less than minValue
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<ulong>());
    }

    [TestMethod]
    public void SetValueUInt64ValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        ulong defaultValue = 10;
        ulong maxValue = 15;
        ulong newValue = 20; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateUInt64Option(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<ulong>());
    }

    [TestMethod]
    public void CreateSingleOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        string? description = "Test description";
        float? minValue = 5.0f;
        float? maxValue = 15.0f;

        // Act
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<float>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<float>());
        Assert.AreEqual(maxValue, option.GetMaxValue<float>());
    }

    [TestMethod]
    public void SetValueSingleValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        float newValue = 15.0f;
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<float>());
    }

    [TestMethod]
    public void CreateSingleOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        float defaultValue = 10.0f;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateSingleOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueSingleReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue);

        // Act
        float? result = option.GetDefaultValue<float>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueSingleReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        float minValue = 5.0f;
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue, null, minValue);

        // Act
        float? result = option.GetMinValue<float>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueSingleReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        float maxValue = 15.0f;
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue, null, null, maxValue);

        // Act
        float? result = option.GetMaxValue<float>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueSingleValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        float minValue = 5.0f;
        float newValue = 3.0f; // Less than minValue
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<float>());
    }

    [TestMethod]
    public void SetValueSingleValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        float defaultValue = 10.0f;
        float maxValue = 15.0f;
        float newValue = 20.0f; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateSingleOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<float>());
    }

    [TestMethod]
    public void CreateDoubleOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        string? description = "Test description";
        double? minValue = 5.0;
        double? maxValue = 15.0;

        // Act
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<double>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<double>());
        Assert.AreEqual(maxValue, option.GetMaxValue<double>());
    }

    [TestMethod]
    public void SetValueDoubleValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        double newValue = 15.0;
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<double>());
    }

    [TestMethod]
    public void CreateDoubleOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        double defaultValue = 10.0;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateDoubleOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueDoubleReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue);

        // Act
        double? result = option.GetDefaultValue<double>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueDoubleReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        double minValue = 5.0;
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue, null, minValue);

        // Act
        double? result = option.GetMinValue<double>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueDoubleReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        double maxValue = 15.0;
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue, null, null, maxValue);

        // Act
        double? result = option.GetMaxValue<double>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueDoubleValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        double minValue = 5.0;
        double newValue = 3.0; // Less than minValue
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<double>());
    }

    [TestMethod]
    public void SetValueDoubleValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        double defaultValue = 10.0;
        double maxValue = 15.0;
        double newValue = 20.0; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateDoubleOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<double>());
    }

    [TestMethod]
    public void CreateDecimalOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        string? description = "Test description";
        decimal? minValue = 5.0m;
        decimal? maxValue = 15.0m;

        // Act
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<decimal>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<decimal>());
        Assert.AreEqual(maxValue, option.GetMaxValue<decimal>());
    }

    [TestMethod]
    public void SetValueDecimalValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        decimal newValue = 15.0m;
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<decimal>());
    }

    [TestMethod]
    public void CreateDecimalOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        decimal defaultValue = 10.0m;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateDecimalOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueDecimalReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue);

        // Act
        decimal? result = option.GetDefaultValue<decimal>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueDecimalReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        decimal minValue = 5.0m;
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue, null, minValue);

        // Act
        decimal? result = option.GetMinValue<decimal>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueDecimalReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        decimal maxValue = 15.0m;
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue, null, null, maxValue);

        // Act
        decimal? result = option.GetMaxValue<decimal>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueDecimalValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        decimal minValue = 5.0m;
        decimal newValue = 3.0m; // Less than minValue
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<decimal>());
    }

    [TestMethod]
    public void SetValueDecimalValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        decimal defaultValue = 10.0m;
        decimal maxValue = 15.0m;
        decimal newValue = 20.0m; // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateDecimalOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<decimal>());
    }

    [TestMethod]
    public void CreateDateTimeOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        string? description = "Test description";
        DateTime? minValue = DateTime.Now.AddDays(-1);
        DateTime? maxValue = DateTime.Now.AddDays(1);

        // Act
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue, description, minValue, maxValue);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<DateTime>());
        Assert.AreEqual(description, option.Description);
        Assert.AreEqual(minValue, option.GetMinValue<DateTime>());
        Assert.AreEqual(maxValue, option.GetMaxValue<DateTime>());
    }

    [TestMethod]
    public void SetValueDateTimeValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        DateTime newValue = DateTime.Now.AddDays(1);
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<DateTime>());
    }

    [TestMethod]
    public void CreateDateTimeOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        DateTime defaultValue = DateTime.Now;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateDateTimeOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueDateTimeReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue);

        // Act
        DateTime? result = option.GetDefaultValue<DateTime>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GetMinValueDateTimeReturnsMinValue()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        DateTime minValue = DateTime.Now.AddDays(-1);
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue, null, minValue);

        // Act
        DateTime? result = option.GetMinValue<DateTime>();

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void GetMaxValueDateTimeReturnsMaxValue()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        DateTime maxValue = DateTime.Now.AddDays(1);
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue, null, null, maxValue);

        // Act
        DateTime? result = option.GetMaxValue<DateTime>();

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void SetValueDateTimeValueLessThanMinSetsToMin()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        DateTime minValue = DateTime.Now.AddDays(-1);
        DateTime newValue = DateTime.Now.AddDays(-2); // Less than minValue
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue, null, minValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(minValue, option.GetValue<DateTime>());
    }

    [TestMethod]
    public void SetValueDateTimeValueGreaterThanMaxSetsToMax()
    {
        // Arrange
        string name = "TestOption";
        DateTime defaultValue = DateTime.Now;
        DateTime maxValue = DateTime.Now.AddDays(1);
        DateTime newValue = DateTime.Now.AddDays(2); // Greater than maxValue
        IGorgonOption option = GorgonOption.CreateDateTimeOption(name, defaultValue, null, null, maxValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(maxValue, option.GetValue<DateTime>());
    }

    [TestMethod]
    public void CreateOptionBoolValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        bool defaultValue = true;
        string? description = "Test description";

        // Act
        IGorgonOption option = GorgonOption.CreateOption(name, defaultValue, description);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<bool>());
        Assert.AreEqual(description, option.Description);
    }

    [TestMethod]
    public void SetValueBoolValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        bool defaultValue = true;
        bool newValue = false;
        IGorgonOption option = GorgonOption.CreateOption(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<bool>());
    }

    [TestMethod]
    public void CreateOptionBoolEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        bool defaultValue = true;

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateOption(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueBoolReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        bool defaultValue = true;
        IGorgonOption option = GorgonOption.CreateOption(name, defaultValue);

        // Act
        bool? result = option.GetDefaultValue<bool>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void CreateStringOptionValidParametersReturnsOption()
    {
        // Arrange
        string name = "TestOption";
        string? defaultValue = "Default";
        string? description = "Test description";

        // Act
        IGorgonOption option = GorgonOption.CreateOption<string?>(name, defaultValue, description);

        // Assert
        Assert.AreEqual(name, option.Name);
        Assert.AreEqual(defaultValue, option.GetValue<string?>());
        Assert.AreEqual(description, option.Description);
    }

    [TestMethod]
    public void SetValueStringValidParametersSetsValue()
    {
        // Arrange
        string name = "TestOption";
        string? defaultValue = "Default";
        string? newValue = "New Value";
        IGorgonOption option = GorgonOption.CreateOption<string?>(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<string?>());
    }

    [TestMethod]
    public void SetValueStringNullValueSetsValue()
    {
        // Arrange
        string name = "TestOption";
        string? defaultValue = "Default";
        string? newValue = null;
        IGorgonOption option = GorgonOption.CreateOption<string?>(name, defaultValue);

        // Act
        option.SetValue(newValue);

        // Assert
        Assert.AreEqual(newValue, option.GetValue<string?>());
    }

    [TestMethod]
    public void CreateStringOptionEmptyNameThrowsException()
    {
        // Arrange
        string name = string.Empty;
        string? defaultValue = "Default";

        // Act and Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => GorgonOption.CreateOption<string?>(name, defaultValue));
    }

    [TestMethod]
    public void GetDefaultValueStringReturnsDefaultValue()
    {
        // Arrange
        string name = "TestOption";
        string? defaultValue = "Default";
        IGorgonOption option = GorgonOption.CreateOption<string?>(name, defaultValue);

        // Act
        string? result = option.GetDefaultValue<string?>();

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void CreateInt64OptionSetLongValueGetAsInt32()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateOption(name, defaultValue);

        long newValue = 15;
        option.SetValue(newValue);

        // Act
        int retrievedValue = option.GetValue<int>();

        // Assert
        Assert.AreEqual((int)newValue, retrievedValue);
    }

    [TestMethod]
    public void CreateInt64OptionSetLongValueGetAsDouble()
    {
        // Arrange
        string name = "TestOption";
        long defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateOption(name, defaultValue);

        long newValue = 15;
        option.SetValue(newValue);

        // Act
        double retrievedValue = option.GetValue<double>();

        // Assert
        Assert.AreEqual((double)newValue, retrievedValue);
    }
}
