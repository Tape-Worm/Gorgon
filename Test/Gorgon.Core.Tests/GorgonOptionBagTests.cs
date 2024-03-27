using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Configuration;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonOptionBagTests
{
    [TestMethod]
    public void GorgonOptionBag_Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");

        // Act
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Assert
        Assert.AreEqual(1, optionBag.Count);
        Assert.AreEqual(option, optionBag[0]);
    }

    [TestMethod]
    public void GorgonOptionBag_Constructor_DuplicateOptions_ThrowsException()
    {
        // Arrange
        IGorgonOption option1 = GorgonOption.CreateOption<string>("TestOption", "Default");
        IGorgonOption option2 = GorgonOption.CreateOption<string>("TestOption", "Default");

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => new GorgonOptionBag(new[] { option1, option2 }));
    }

    [TestMethod]
    public void GorgonOptionBag_GetOptionValue_ValidParameters_ReturnsValue()
    {
        // Arrange
        string optionName = "TestOption";
        string defaultValue = "Default";
        IGorgonOption option = GorgonOption.CreateOption<string>(optionName, defaultValue);
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        string? result = optionBag.GetOptionValue<string>(optionName);

        // Assert
        Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void GorgonOptionBag_GetOptionValue_InvalidOptionName_ThrowsException()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act and Assert
        Assert.ThrowsException<KeyNotFoundException>(() => optionBag.GetOptionValue<string>("InvalidOptionName"));
    }

    [TestMethod]
    public void GorgonOptionBag_SetOptionValue_ValidParameters_SetsValue()
    {
        // Arrange
        string optionName = "TestOption";
        string defaultValue = "Default";
        string newValue = "New Value";
        IGorgonOption option = GorgonOption.CreateOption<string>(optionName, defaultValue);
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        optionBag.SetOptionValue(optionName, newValue);

        // Assert
        Assert.AreEqual(newValue, optionBag.GetOptionValue<string>(optionName));
    }

    [TestMethod]
    public void GorgonOptionBag_SetOptionValue_InvalidOptionName_ThrowsException()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act and Assert
        Assert.ThrowsException<KeyNotFoundException>(() => optionBag.SetOptionValue("InvalidOptionName", "New Value"));
    }

    [TestMethod]
    public void GorgonOptionBag_IndexOf_ValidOption_ReturnsIndex()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        int index = optionBag.IndexOf(option);

        // Assert
        Assert.AreEqual(0, index);
    }

    [TestMethod]
    public void GorgonOptionBag_IndexOf_InvalidOption_ReturnsMinusOne()
    {
        // Arrange
        IGorgonOption option1 = GorgonOption.CreateOption<string>("TestOption", "Default");
        IGorgonOption option2 = GorgonOption.CreateOption<string>("InvalidOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option1 });

        // Act
        int index = optionBag.IndexOf(option2);

        // Assert
        Assert.AreEqual(-1, index);
    }

    [TestMethod]
    public void GorgonOptionBag_Contains_ValidOption_ReturnsTrue()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        bool contains = optionBag.Contains(option);

        // Assert
        Assert.IsTrue(contains);
    }

    [TestMethod]
    public void GorgonOptionBag_Contains_InvalidOption_ReturnsFalse()
    {
        // Arrange
        IGorgonOption option1 = GorgonOption.CreateOption<string>("TestOption", "Default");
        IGorgonOption option2 = GorgonOption.CreateOption<string>("InvalidOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option1 });

        // Act
        bool contains = optionBag.Contains(option2);

        // Assert
        Assert.IsFalse(contains);
    }

    [TestMethod]
    public void GetByName_ValidOptionName_ReturnsOption()
    {
        // Arrange
        string optionName = "TestOption";
        IGorgonOption option = GorgonOption.CreateOption<string>(optionName, "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        IGorgonOption result = optionBag.GetByName(optionName);

        // Assert
        Assert.AreEqual(option, result);
    }

    [TestMethod]
    public void GetByName_InvalidOptionName_ThrowsException()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act and Assert
        Assert.ThrowsException<KeyNotFoundException>(() => optionBag.GetByName("InvalidOptionName"));
    }

    [TestMethod]
    public void IndexOfName_ValidOptionName_ReturnsIndex()
    {
        // Arrange
        string optionName = "TestOption";
        IGorgonOption option = GorgonOption.CreateOption<string>(optionName, "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        int index = optionBag.IndexOfName(optionName);

        // Assert
        Assert.AreEqual(0, index);
    }

    [TestMethod]
    public void IndexOfName_InvalidOptionName_ReturnsMinusOne()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        int index = optionBag.IndexOfName("InvalidOptionName");

        // Assert
        Assert.AreEqual(-1, index);
    }

    [TestMethod]
    public void ContainsName_ValidOptionName_ReturnsTrue()
    {
        // Arrange
        string optionName = "TestOption";
        IGorgonOption option = GorgonOption.CreateOption<string>(optionName, "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        bool contains = optionBag.ContainsName(optionName);

        // Assert
        Assert.IsTrue(contains);
    }

    [TestMethod]
    public void ContainsName_InvalidOptionName_ReturnsFalse()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<string>("TestOption", "Default");
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        // Act
        bool contains = optionBag.ContainsName("InvalidOptionName");

        // Assert
        Assert.IsFalse(contains);
    }

    [TestMethod]
    public void CreateInt64Option_SetLongValue_GetAsInt32()
    {
        // Arrange
        string optionName = "TestOption";
        long defaultValue = 10;
        IGorgonOption option = GorgonOption.CreateOption<long>(optionName, defaultValue);
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        long newValue = 15;
        optionBag.SetOptionValue(optionName, newValue);

        // Act
        int retrievedValue = optionBag.GetOptionValue<int>(optionName);

        // Assert
        Assert.AreEqual((int)newValue, retrievedValue);
    }

    [TestMethod]
    public void GorgonOptionBag_SetAndGet_NumericPrimitiveTypesAndDateTime()
    {
        // Arrange
        IGorgonOption optionInt16 = GorgonOption.CreateOption<short>("OptionInt16", 1);
        IGorgonOption optionInt32 = GorgonOption.CreateOption<int>("OptionInt32", 2);
        IGorgonOption optionInt64 = GorgonOption.CreateOption<long>("OptionInt64", 3);
        IGorgonOption optionUInt16 = GorgonOption.CreateOption<ushort>("OptionUInt16", 4);
        IGorgonOption optionUInt32 = GorgonOption.CreateOption<uint>("OptionUInt32", 5);
        IGorgonOption optionUInt64 = GorgonOption.CreateOption<ulong>("OptionUInt64", 6);
        IGorgonOption optionFloat = GorgonOption.CreateOption<float>("OptionFloat", 7.0f);
        IGorgonOption optionDouble = GorgonOption.CreateOption<double>("OptionDouble", 8.0);
        IGorgonOption optionDecimal = GorgonOption.CreateOption<decimal>("OptionDecimal", 9.0m);
        IGorgonOption optionDateTime = GorgonOption.CreateOption<DateTime>("OptionDateTime", new DateTime(2022, 1, 1));
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { optionInt16, optionInt32, optionInt64, optionUInt16, optionUInt32, optionUInt64, optionFloat, optionDouble, optionDecimal, optionDateTime });

        // Act
        short retrievedInt16 = optionBag.GetOptionValue<short>("OptionInt16");
        int retrievedInt32 = optionBag.GetOptionValue<int>("OptionInt32");
        long retrievedInt64 = optionBag.GetOptionValue<long>("OptionInt64");
        ushort retrievedUInt16 = optionBag.GetOptionValue<ushort>("OptionUInt16");
        uint retrievedUInt32 = optionBag.GetOptionValue<uint>("OptionUInt32");
        ulong retrievedUInt64 = optionBag.GetOptionValue<ulong>("OptionUInt64");
        float retrievedFloat = optionBag.GetOptionValue<float>("OptionFloat");
        double retrievedDouble = optionBag.GetOptionValue<double>("OptionDouble");
        decimal retrievedDecimal = optionBag.GetOptionValue<decimal>("OptionDecimal");
        DateTime retrievedDateTime = optionBag.GetOptionValue<DateTime>("OptionDateTime");

        // Assert
        Assert.AreEqual(1, retrievedInt16);
        Assert.AreEqual(2, retrievedInt32);
        Assert.AreEqual(3, retrievedInt64);
        Assert.AreEqual(4, retrievedUInt16);
        Assert.AreEqual(5u, retrievedUInt32);
        Assert.AreEqual(6ul, retrievedUInt64);
        Assert.AreEqual(7.0f, retrievedFloat);
        Assert.AreEqual(8.0, retrievedDouble);
        Assert.AreEqual(9.0m, retrievedDecimal);
        Assert.AreEqual(new DateTime(2022, 1, 1), retrievedDateTime);
    }

    [TestMethod]
    public void CreateByteOption_SetValue_ValueBelowMin()
    {
        // Arrange
        string optionName = "TestOption";
        byte minValue = 64;
        byte maxValue = 127;
        byte defaultValue = 64;
        IGorgonOption option = GorgonOption.CreateByteOption(optionName, defaultValue, null, minValue, maxValue);
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        byte newValue = 60;

        // Act
        optionBag.SetOptionValue(optionName, newValue);

        // Assert
        byte retrievedValue = optionBag.GetOptionValue<byte>(optionName);
        Assert.AreEqual(minValue, retrievedValue);
    }

    [TestMethod]
    public void CreateByteOption_SetValue_ValueOutOfRange()
    {
        // Arrange
        string optionName = "TestOption";
        byte minValue = 64;
        byte maxValue = 127;
        byte defaultValue = 64;
        IGorgonOption option = GorgonOption.CreateByteOption(optionName, defaultValue, null, minValue, maxValue);
        GorgonOptionBag optionBag = new GorgonOptionBag(new[] { option });

        byte newValue = 130;

        // Act
        optionBag.SetOptionValue(optionName, newValue);

        // Assert
        byte retrievedValue = optionBag.GetOptionValue<byte>(optionName);
        Assert.AreEqual(maxValue, retrievedValue);
    }
}
