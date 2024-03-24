using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;

namespace Gorgon.Core.Tests;

public class Node
{
    public string Name
    {
        get;
        set;
    } = string.Empty;        

    public List<Node> Children
    {
        get; 
    } = new List<Node>();
}

[TestClass]
public class GorgonIEnumerableExtensionsTests
{
    private Node _root = new()
    {
        Name = "Root",
        Children =
        {
            new Node
            {
                Name = "Child 1",
                Children =
                {
                    new Node
                    {
                        Name = "GrandChild 1"
                    },
                    new Node
                    {
                        Name = "GrandChild 2",
                        Children =
                        {
                            new Node
                            {
                                Name = "Great GrandChild 1"
                            },
                            new Node
                            {
                                Name = "Great GrandChild 2"
                            },
                        }
                    },
                    new Node
                    {
                        Name = "GrandChild 3"
                    }
                }
            },
            new Node
            {
                Name = "Child 2"
            }
        }
    };

    [TestMethod]
    public void TraverseDepthFirst_IEnumerable()
    {
        List<string> expected =
        [
            "Child 1",
                "GrandChild 1",
                "GrandChild 2",
                    "Great GrandChild 1",
                    "Great GrandChild 2",
                "GrandChild 3",
            "Child 2"
        ];
        List<string> actual = new();

        actual.AddRange(_root.Children.TraverseDepthFirst(x => x.Children).Select(x => x.Name));

        Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [TestMethod]
    public void TraverseBreadthFirst_IEnumerable()
    {
        List<string> expected =
        [
            "Child 1",
            "Child 2",
            "GrandChild 1",
            "GrandChild 2",
            "GrandChild 3",
            "Great GrandChild 1",
            "Great GrandChild 2",
        ];
        List<string> actual = new();

        actual.AddRange(_root.Children.TraverseBreadthFirst(x => x.Children).Select(x => x.Name));

        Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [TestMethod]
    public void ToGorgonArray()
    {
        List<int> expected = new()
        {
        1, 2, 3, 4, 5 };
        GorgonArray<int> actual = expected.ToGorgonArray();

        Assert.AreEqual(expected.Count, actual.Length);

        for (int i = 0; i < expected.Count; ++i)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }
}
