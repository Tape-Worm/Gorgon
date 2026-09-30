using System.Collections.Generic;
using Gorgon.Configuration;

namespace Gorgon.Core.Tests;

// Found by the Imaging integration battery: DDS Palette and GIF FrameDelays are created with a List<T> default and read back as IList<T>.
[TestClass]
public class GorgonOptionReferenceTypeTests
{
    [TestMethod]
    public void GetValueListOptionAsInterfaceReturnsList()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption("TestOption", new List<int> { 1, 2 });

        // Act
        IList<int>? value = option.GetValue<IList<int>>();

        // Assert
        Assert.IsNotNull(value);
        CollectionAssert.AreEqual(new[] { 1, 2 }, (System.Collections.ICollection)value);
    }

    [TestMethod]
    public void GetDefaultValueListOptionAsInterfaceReturnsList()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption("TestOption", new List<int> { 1, 2 });

        // Act
        IList<int>? value = option.GetDefaultValue<IList<int>>();

        // Assert
        Assert.IsNotNull(value);
        CollectionAssert.AreEqual(new[] { 1, 2 }, (System.Collections.ICollection)value);
    }

    [TestMethod]
    public void SetValueListOnInterfaceOptionReadsBack()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<IList<int>>("TestOption", []);

        // Act
        option.SetValue(new List<int> { 3, 4 });
        IList<int>? value = option.GetValue<IList<int>>();

        // Assert
        Assert.IsNotNull(value);
        CollectionAssert.AreEqual(new[] { 3, 4 }, (System.Collections.ICollection)value);
    }

    [TestMethod]
    public void NullDefaultReferenceOptionReadsBackNull()
    {
        // Arrange
        IGorgonOption option = GorgonOption.CreateOption<IReadOnlyList<int>>("TestOption");

        // Act
        IReadOnlyList<int>? value = option.GetValue<IReadOnlyList<int>>();
        IReadOnlyList<int>? defaultValue = option.GetDefaultValue<IReadOnlyList<int>>();

        // Assert
        Assert.IsNull(value);
        Assert.IsNull(defaultValue);
    }

    [TestMethod]
    public void TextIsFirstLineOfDescription()
    {
        // Arrange
        string description = "First line\nSecond line";

        // Act
        IGorgonOption option = GorgonOption.CreateInt32Option("TestOption", 0, description);

        // Assert
        Assert.AreEqual("First line", option.Text);
    }

    [TestMethod]
    public void TextIsFirstLineOfCrLfDescription()
    {
        // Arrange
        string description = "First line\r\nSecond line";

        // Act
        IGorgonOption option = GorgonOption.CreateInt32Option("TestOption", 0, description);

        // Assert
        Assert.AreEqual("First line", option.Text);
    }
}
