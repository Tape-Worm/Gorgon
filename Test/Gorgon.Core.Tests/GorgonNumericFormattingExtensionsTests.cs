using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonNumericFormattingExtensionsTests
{
    [TestMethod]
    public void FormatMemory()
    {
        byte bytes = 255;
        short shorts = 32767;
        ushort ushorts = 65535;
        int megabyte = 1_048_576;
        uint umegabyte = 1_048_576U;
        int gigabyte = int.MaxValue;
        uint ugigabyte = uint.MaxValue;
        long terrabyte = 1_099_511_627_776;
        ulong uterrabyte = 1_099_511_627_776UL;
        long petabyte = 1_125_899_906_842_624;
        float floatValue = 294_322f;
        double doubleValue = 294_322;
        decimal decimalValue = 294_322M;

        Assert.AreEqual("255 bytes", bytes.FormatMemory(), true);
        Assert.AreEqual("32 KB", shorts.FormatMemory(), true);
        Assert.AreEqual("64 KB", ushorts.FormatMemory(), true);
        Assert.AreEqual("1.0 MB", megabyte.FormatMemory(), true);
        Assert.AreEqual("1.0 MB", umegabyte.FormatMemory(), true);
        Assert.AreEqual("2.0 GB", gigabyte.FormatMemory(), true);
        Assert.AreEqual("4.0 GB", ugigabyte.FormatMemory(), true);
        Assert.AreEqual("1.0 TB", terrabyte.FormatMemory(), true);
        Assert.AreEqual("1.0 TB", uterrabyte.FormatMemory(), true);
        Assert.AreEqual("1.0 PB", petabyte.FormatMemory(), true);
        Assert.AreEqual("287.4 KB", floatValue.FormatMemory(),true);
        Assert.AreEqual("287.4 KB", doubleValue.FormatMemory(), true);
        Assert.AreEqual("287.4 KB", decimalValue.FormatMemory(), true);
    }

    [TestMethod]
    public void FormatHex()
    {
        byte bytes = 255;
        short shorts = 32767;
        ushort ushorts = 65535;
        int ints = int.MaxValue;
        uint uints = uint.MaxValue;
        long longs = long.MaxValue;
        ulong ulongs = ulong.MaxValue;
        nint ptr = nint.Zero;
        nuint uptr = nuint.Zero;

        Assert.AreEqual("ff", bytes.FormatHex(), true);
        Assert.AreEqual("7fff", shorts.FormatHex(), true);
        Assert.AreEqual("ffff", ushorts.FormatHex(), true);
        Assert.AreEqual("7fffffff", ints.FormatHex(), true);
        Assert.AreEqual("ffffffff", uints.FormatHex(), true);
        Assert.AreEqual("7fffffffffffffff", longs.FormatHex(), true);
        Assert.AreEqual("ffffffffffffffff", ulongs.FormatHex(), true);
        if (Environment.Is64BitProcess)
        {
            Assert.AreEqual("0000000000000000", ptr.FormatHex(), true);
            Assert.AreEqual("0000000000000000", uptr.FormatHex(), true);
        }
        else
        {
            Assert.AreEqual("00000000", ptr.FormatHex(), true);
            Assert.AreEqual("00000000", uptr.FormatHex(), true);
        }
    }
}
