using System;
using System.Collections.Generic;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonSpanExtensionsTests
{
    private static readonly char[] _separator = ['\\'];
    private static readonly char[] _separator2 = ['\\', '/'];

    [TestMethod]
    public void SplitEnumerator()
    {
        ReadOnlySpan<char> dir = @"d:\my path\is quite\long\you\see\look\at_this.file".AsSpan();

        List<string> parts = [];
        List<string> actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"d:/my path\is quite/long\you/see\look/at_this.file".AsSpan();
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator2))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"\d:\my path\\is quite\\\long\you\see\look\at_this.file".AsSpan();
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"\d:\my path\\is quite\\\long\you\see\look\at_this.file".AsSpan();
        actual = [string.Empty, "d:", "my path", string.Empty, "is quite", string.Empty, string.Empty, "long", "you", "see", "look", "at_this.file"];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator, true))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"d:\my path\is quite\long\you\see\look\at_this_directory\".AsSpan();
        actual = ["d:", "my path", "is quite", "long", "you", "see", "look", "at_this_directory"];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);

        parts.Clear();
        dir = @"This has nothing to split.".AsSpan();
        actual = ["This has nothing to split."];

        foreach (ReadOnlySpan<char> part in dir.Split(_separator))
        {
            parts.Add(part.ToString());
        }

        CollectionAssert.AreEqual(actual, parts);
    }
}
