using Gorgon.IO;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonFileExtensionTests
{
    [TestMethod]
    public void GorgonFileExtensionConstructorShouldInitializeCorrectlyWithDescription()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual("Text File", fileExtension.Description);
    }

    [TestMethod]
    public void GorgonFileExtensionConstructorShouldInitializeCorrectlyWithoutDescription()
    {
        GorgonFileExtension fileExtension = new(".txt");
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual(string.Empty, fileExtension.Description);
    }

    [TestMethod]
    public void GorgonFileExtensionConstructorShouldThrowExceptionWhenExtensionIsEmpty() => Assert.ThrowsException<ArgumentEmptyException>(() => new GorgonFileExtension(string.Empty));

    [TestMethod]
    public void GorgonFileExtensionConstructorShouldRemoveLeadingPeriodFromExtension()
    {
        GorgonFileExtension fileExtension = new(".txt");
        Assert.AreEqual("txt", fileExtension.Extension);
    }

    [TestMethod]
    public void GorgonFileExtensionConstructorShouldHandleNullDescription()
    {
        GorgonFileExtension fileExtension = new(".txt", null);
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual(string.Empty, fileExtension.Description);
    }

    [TestMethod]
    public void OperatorEqualsShouldReturnTrueWhenExtensionsAreEqual()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 == fileExtension2);
    }

    [TestMethod]
    public void OperatorNotEqualsShouldReturnFalseWhenExtensionsAreEqual()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".txt", "Text Document");
        Assert.IsFalse(fileExtension1 != fileExtension2);
    }

    [TestMethod]
    public void OperatorLessThanShouldReturnTrueWhenLeftIsLessThanRight()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".doc", "Document");
        Assert.IsFalse(fileExtension2 > fileExtension1);
    }

    [TestMethod]
    public void OperatorGreaterThanShouldReturnFalseWhenLeftIsLessThanRight()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".doc", "Document");
        Assert.IsTrue(fileExtension2 < fileExtension1);
    }

    [TestMethod]
    public void OperatorLessThanOrEqualShouldReturnTrueWhenLeftIsLessThanOrEqualRight()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 <= fileExtension2);
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqualShouldReturnTrueWhenLeftIsGreaterThanOrEqualRight()
    {
        GorgonFileExtension fileExtension1 = new(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 >= fileExtension2);
    }

    [TestMethod]
    public void OperatorEqualsShouldReturnTrueWhenExtensionEqualsString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsTrue(fileExtension == ".txt");
    }

    [TestMethod]
    public void OperatorNotEqualsShouldReturnFalseWhenExtensionEqualsString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsFalse(fileExtension != ".txt");
    }

    [TestMethod]
    public void OperatorLessThanShouldReturnTrueWhenExtensionIsLessThanString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsFalse(fileExtension < ".doc");
    }

    [TestMethod]
    public void OperatorGreaterThanShouldReturnFalseWhenExtensionIsLessThanString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsFalse(".doc" > fileExtension);
    }

    [TestMethod]
    public void OperatorLessThanOrEqualShouldReturnTrueWhenExtensionIsLessThanOrEqualString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsTrue(fileExtension <= ".txt");
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqualShouldReturnTrueWhenExtensionIsGreaterThanOrEqualString()
    {
        GorgonFileExtension fileExtension = new(".txt", "Text File");
        Assert.IsTrue(fileExtension >= ".txt");
    }

    [TestMethod]
    public void EqualsGorgonFileExtensionShouldReturnTrueWhenEqual()
    {
        GorgonFileExtension extension1 = new("txt");
        GorgonFileExtension extension2 = new("txt");

        Assert.IsTrue(extension1.Equals(extension2));
    }

    [TestMethod]
    public void EqualsGorgonFileExtensionShouldReturnFalseWhenNotEqual()
    {
        GorgonFileExtension extension1 = new("txt");
        GorgonFileExtension extension2 = new("doc");

        Assert.IsFalse(extension1.Equals(extension2));
    }

    [TestMethod]
    public void CompareToGorgonFileExtensionShouldReturnZeroWhenEqual()
    {
        GorgonFileExtension extension1 = new("txt");
        GorgonFileExtension extension2 = new("txt");

        Assert.AreEqual(0, extension1.CompareTo(extension2));
    }

    [TestMethod]
    public void CompareToGorgonFileExtensionShouldReturnNonZeroWhenNotEqual()
    {
        GorgonFileExtension extension1 = new("txt");
        GorgonFileExtension extension2 = new("doc");

        Assert.AreNotEqual(0, extension1.CompareTo(extension2));
    }

    [TestMethod]
    public void EqualsStringShouldReturnTrueWhenEqual()
    {
        GorgonFileExtension extension = new("txt");

        Assert.IsTrue(extension.Equals("txt"));
        Assert.IsTrue(extension.Equals(".txt"));
    }

    [TestMethod]
    public void EqualsStringShouldReturnFalseWhenNotEqual()
    {
        GorgonFileExtension extension = new("txt");

        Assert.IsFalse(extension.Equals("doc"));
        Assert.IsFalse(extension.Equals(".doc"));
    }

    [TestMethod]
    public void CompareToStringShouldReturnZeroWhenEqual()
    {
        GorgonFileExtension extension = new("txt");

        Assert.AreEqual(0, extension.CompareTo("txt"));
        Assert.AreEqual(0, extension.CompareTo(".txt"));
    }

    [TestMethod]
    public void CompareToStringShouldReturnNonZeroWhenNotEqual()
    {
        GorgonFileExtension extension = new("txt");

        Assert.AreNotEqual(0, extension.CompareTo("doc"));
        Assert.AreNotEqual(0, extension.CompareTo(".doc"));
    }

    [TestMethod]
    public void CompareToStringShouldReturnPositiveWhenOtherIsNull()
    {
        GorgonFileExtension extension = new("txt");

        Assert.AreEqual(1, extension.CompareTo(null));
    }
}
