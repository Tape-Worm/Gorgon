using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonStringFormattingExtensionsTests
{
    [TestMethod]
    public void GetLines()
    {
        string[] expected =
        {
            "This is a line of text.",
            "And this is too",
            "",
            "And this one has a blank line above it."
        };
        StringBuilder sb = new(string.Join("\n", expected));
        string[] actual = Array.Empty<string>();

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

    /// <summary>
    /// Function to find the index of the control characters in a string builder.
    /// </summary>
    /// <param name="line">The line containing the text to evaluate.</param>
    /// <param name="value">The string value to look for.</param>
    /// <param name="startIndex">The starting index within the line to start searching from.</param>
    /// <returns>The index of the control character, or -1 if not found.</returns>
    private int IndexOfControlChars(StringBuilder line, string value, int startIndex)
    {
        if (line.Length < value.Length)
        {
            return -1;
        }

        int charCount = 0;

        for (int i = 0; i < value.Length; ++i)
        {
            char c = value[i];

            for (int j = startIndex; j < line.Length; ++j)
            {
                if (char.ToUpperInvariant(c) != char.ToUpperInvariant(line[j]))
                {                    
                    continue;
                }

                startIndex = j;
                ++charCount;
                break;
            }

            if (charCount == value.Length)
            {
                return (startIndex - charCount) + 1;
            }
        }

        return -1;
    }
}
