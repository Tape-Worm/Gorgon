using System;
using System.IO;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonMemoryStreamTests
{
    [TestMethod]
    public void TestRead()
    {
        using GorgonNativeBuffer<uint> buffer = new(4);

        for (int i = 0; i < buffer.Length; ++i)
        {
            buffer[i] = (uint)(i + 1);
        }

        using GorgonMemoryStream<uint> stream = new(buffer);

        byte[] info = new byte[16];

        stream.ReadExactly(info.AsSpan());

        Assert.AreEqual(16, stream.Position);
        Assert.AreEqual(1, info[0]);
        Assert.AreEqual(2, info[4]);
        Assert.AreEqual(3, info[8]);
        Assert.AreEqual(4, info[12]);
    }

    [TestMethod]
    public void TestWrite()
    {
        using GorgonNativeBuffer<uint> buffer = new(4);
        using GorgonMemoryStream<uint> stream = new(buffer);

        byte[] info = new byte[16];
        info[0] = 1;
        info[4] = 2;
        info[8] = 3;
        info[12] = 4;

        stream.Write(info.AsSpan());

        Assert.AreEqual(16, stream.Position);
        Assert.AreEqual(1U, buffer[0]);
        Assert.AreEqual(2U, buffer[1]);
        Assert.AreEqual(3U, buffer[2]);
        Assert.AreEqual(4U, buffer[3]);
    }

    [TestMethod]
    public void TestSeekBegin()
    {
        using GorgonNativeBuffer<uint> buffer = new(64);
        using GorgonMemoryStream<uint> stream = new(buffer);

        stream.Seek(32, SeekOrigin.Begin);

        Assert.AreEqual(32, stream.Position);
    }

    [TestMethod]
    public void TestSeekEnd()
    {
        using GorgonNativeBuffer<uint> buffer = new(16);
        using GorgonMemoryStream<uint> stream = new(buffer);

        stream.Seek(-32, SeekOrigin.End);

        Assert.AreEqual(32, stream.Position);
    }

    [TestMethod]
    public void TestSeekCurrent()
    {
        using GorgonNativeBuffer<uint> buffer = new(16);
        using GorgonMemoryStream<uint> stream = new(buffer);

        stream.Seek(32, SeekOrigin.Current);

        Assert.AreEqual(32, stream.Position);

        stream.Seek(-16, SeekOrigin.Current);

        Assert.AreEqual(16, stream.Position);
    }
}
