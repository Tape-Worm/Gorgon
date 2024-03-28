using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.IO;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonFileExtensionTests
{
    [TestMethod]
    public void GorgonFileExtension_Constructor_ShouldInitializeCorrectly_WithDescription()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual("Text File", fileExtension.Description);
    }

    [TestMethod]
    public void GorgonFileExtension_Constructor_ShouldInitializeCorrectly_WithoutDescription()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt");
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual(string.Empty, fileExtension.Description);
    }

    [TestMethod]
    public void GorgonFileExtension_Constructor_ShouldThrowException_WhenExtensionIsEmpty()
    {
        Assert.ThrowsException<ArgumentEmptyException>(() => new GorgonFileExtension(string.Empty));
    }

    [TestMethod]
    public void GorgonFileExtension_Constructor_ShouldRemoveLeadingPeriodFromExtension()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt");
        Assert.AreEqual("txt", fileExtension.Extension);
    }

    [TestMethod]
    public void GorgonFileExtension_Constructor_ShouldHandleNullDescription()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", null);
        Assert.AreEqual("txt", fileExtension.Extension);
        Assert.AreEqual(".txt", fileExtension.FullExtension);
        Assert.AreEqual(string.Empty, fileExtension.Description);
    }

    [TestMethod]
    public void OperatorEquals_ShouldReturnTrue_WhenExtensionsAreEqual()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 == fileExtension2);
    }

    [TestMethod]
    public void OperatorNotEquals_ShouldReturnFalse_WhenExtensionsAreEqual()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".txt", "Text Document");
        Assert.IsFalse(fileExtension1 != fileExtension2);
    }

    [TestMethod]
    public void OperatorLessThan_ShouldReturnTrue_WhenLeftIsLessThanRight()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".doc", "Document");
        Assert.IsFalse(fileExtension2 > fileExtension1);
    }

    [TestMethod]
    public void OperatorGreaterThan_ShouldReturnFalse_WhenLeftIsLessThanRight()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".doc", "Document");
        Assert.IsTrue(fileExtension2 < fileExtension1);        
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_ShouldReturnTrue_WhenLeftIsLessThanOrEqualRight()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 <= fileExtension2);
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_ShouldReturnTrue_WhenLeftIsGreaterThanOrEqualRight()
    {
        GorgonFileExtension fileExtension1 = new GorgonFileExtension(".txt", "Text File");
        GorgonFileExtension fileExtension2 = new GorgonFileExtension(".txt", "Text Document");
        Assert.IsTrue(fileExtension1 >= fileExtension2);
    }

    [TestMethod]
    public void OperatorEquals_ShouldReturnTrue_WhenExtensionEqualsString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsTrue(fileExtension == ".txt");
    }

    [TestMethod]
    public void OperatorNotEquals_ShouldReturnFalse_WhenExtensionEqualsString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsFalse(fileExtension != ".txt");
    }

    [TestMethod]
    public void OperatorLessThan_ShouldReturnTrue_WhenExtensionIsLessThanString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsFalse(fileExtension < ".doc");
    }

    [TestMethod]
    public void OperatorGreaterThan_ShouldReturnFalse_WhenExtensionIsLessThanString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsFalse(".doc" > fileExtension);
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_ShouldReturnTrue_WhenExtensionIsLessThanOrEqualString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsTrue(fileExtension <= ".txt");
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_ShouldReturnTrue_WhenExtensionIsGreaterThanOrEqualString()
    {
        GorgonFileExtension fileExtension = new GorgonFileExtension(".txt", "Text File");
        Assert.IsTrue(fileExtension >= ".txt");
    }

    [TestMethod]
    public void Equals_GorgonFileExtension_ShouldReturnTrueWhenEqual()
    {
        GorgonFileExtension extension1 = new GorgonFileExtension("txt");
        GorgonFileExtension extension2 = new GorgonFileExtension("txt");

        Assert.IsTrue(extension1.Equals(extension2));
    }

    [TestMethod]
    public void Equals_GorgonFileExtension_ShouldReturnFalseWhenNotEqual()
    {
        GorgonFileExtension extension1 = new GorgonFileExtension("txt");
        GorgonFileExtension extension2 = new GorgonFileExtension("doc");

        Assert.IsFalse(extension1.Equals(extension2));
    }

    [TestMethod]
    public void CompareTo_GorgonFileExtension_ShouldReturnZeroWhenEqual()
    {
        GorgonFileExtension extension1 = new GorgonFileExtension("txt");
        GorgonFileExtension extension2 = new GorgonFileExtension("txt");

        Assert.AreEqual(0, extension1.CompareTo(extension2));
    }

    [TestMethod]
    public void CompareTo_GorgonFileExtension_ShouldReturnNonZeroWhenNotEqual()
    {
        GorgonFileExtension extension1 = new GorgonFileExtension("txt");
        GorgonFileExtension extension2 = new GorgonFileExtension("doc");

        Assert.AreNotEqual(0, extension1.CompareTo(extension2));
    }

    [TestMethod]
    public void Equals_String_ShouldReturnTrueWhenEqual()
    {
        GorgonFileExtension extension = new GorgonFileExtension("txt");

        Assert.IsTrue(extension.Equals("txt"));
        Assert.IsTrue(extension.Equals(".txt"));
    }

    [TestMethod]
    public void Equals_String_ShouldReturnFalseWhenNotEqual()
    {
        GorgonFileExtension extension = new GorgonFileExtension("txt");

        Assert.IsFalse(extension.Equals("doc"));
        Assert.IsFalse(extension.Equals(".doc"));
    }

    [TestMethod]
    public void CompareTo_String_ShouldReturnZeroWhenEqual()
    {
        GorgonFileExtension extension = new GorgonFileExtension("txt");

        Assert.AreEqual(0, extension.CompareTo("txt"));
        Assert.AreEqual(0, extension.CompareTo(".txt"));
    }

    [TestMethod]
    public void CompareTo_String_ShouldReturnNonZeroWhenNotEqual()
    {
        GorgonFileExtension extension = new GorgonFileExtension("txt");

        Assert.AreNotEqual(0, extension.CompareTo("doc"));
        Assert.AreNotEqual(0, extension.CompareTo(".doc"));
    }

    [TestMethod]
    public void CompareTo_String_ShouldReturnPositiveWhenOtherIsNull()
    {
        GorgonFileExtension extension = new GorgonFileExtension("txt");

        Assert.AreEqual(1, extension.CompareTo(null));
    }
}
