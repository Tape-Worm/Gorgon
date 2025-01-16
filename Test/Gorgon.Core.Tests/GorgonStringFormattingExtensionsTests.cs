using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonStringFormattingExtensionsTests
{
    private static readonly char[] _separator = ['\\'];
    private static readonly char[] _separator2 = ['\\', '/'];

    [TestMethod]
    public void SplitEnumerator()
    {
        string dir = @"d:\my path\is quite\long\you\see\look\at_this.file";

        List<string> parts = [];
        List<string> actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"d:/my path\is quite/long\you/see\look/at_this.file";
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator2))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"\d:\my path\\is quite\\\long\you\see\look\at_this.file";
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"\d:\my path\\is quite\\\long\you\see\look\at_this.file";
        actual = [string.Empty, "d:", "my path", string.Empty, "is quite", string.Empty, string.Empty, "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator, true))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"d:\my path\is quite\long\you\see\look\at_this_directory\";
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this_directory"];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"This has nothing to split.";
        actual = ["This has nothing to split."];

        foreach (ReadOnlySpan<char> part in dir.GetSplitEnumerator(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);
    }

    [TestMethod]
    public void GetLines()
    {
        string[] expected =
        [
            "This is a line of text.",
            "And this is too",
            "",
            "And this one has a blank line above it."
        ];
        StringBuilder sb = new(string.Join("\n", expected));
        string[] actual = [];

        sb.GetLines(ref actual);

        Assert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; ++i)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }

        actual = sb.ToString().GetLines();

        Assert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; ++i)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }

    [TestMethod]
    public void ToStringWithDeclaration()
    {
        const string expected = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<Root>
  <Value>Value</Value>
  <Value>Value2</Value>
</Root>";

        XDocument document = new(new XDeclaration("1.0", "utf-8", "yes"),
                                                   new XElement("Root",
                                                        new XElement("Value", "Value"),
                                                        new XElement("Value", "Value2")));

        Assert.AreEqual(expected, document.ToStringWithDeclaration());

        document = new(new XElement("Root", new XElement("Value", "Value"), new XElement("Value", "Value2")));

        Assert.AreEqual(expected, document.ToStringWithDeclaration());
    }

    [TestMethod]
    public void Ellipses()
    {
        string theString = "This is a piece of text. That will be ellipsed.";
        string actual = theString.Ellipses(10);

        Assert.AreEqual("This is a ...", actual);

        actual = theString.Ellipses(10, true);

        Assert.AreEqual("... ellipsed.", actual);
    }

    [TestMethod]
    public void GetByteCount()
    {
        string theString = "This is a piece of text.";

        int actual = theString.GetByteCount(false);

        Assert.AreEqual(24, actual);

        actual = theString.GetByteCount(true);

        Assert.AreEqual(25, actual);

        actual = theString.GetByteCount(false, Encoding.Unicode);

        Assert.AreEqual(48, actual);

        actual = theString.GetByteCount(false, Encoding.UTF32);

        Assert.AreEqual(96, actual);

        actual = theString.GetByteCount(false, Encoding.ASCII);

        Assert.AreEqual(24, actual);
    }
}
