using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonFormatInfoTests
{
//#error Finish me.
    [TestMethod]
    public void Ctor()
    {
        GorgonFormatInfo info = new(BufferFormat.R8G8B8A8_UNorm);

        Assert.AreEqual(BufferFormat.R8G8B8A8_UNorm, info.Format);

        info = new(BufferFormat.Unknown);

        Assert.AreEqual(0, info.BitDepth);
    }

    [TestMethod]
    public void CompressedFormats()
    {
        BufferFormat[] formats = [
            BufferFormat.BC1_Typeless,
            BufferFormat.BC2_Typeless,
            BufferFormat.BC3_Typeless,
            BufferFormat.BC4_Typeless,
            BufferFormat.BC5_Typeless,
            BufferFormat.BC6H_Typeless,
            BufferFormat.BC7_Typeless,
            BufferFormat.BC1_UNorm,
            BufferFormat.BC2_UNorm,
            BufferFormat.BC3_UNorm,
            BufferFormat.BC4_UNorm,
            BufferFormat.BC5_UNorm,                
            BufferFormat.BC7_UNorm,
            BufferFormat.BC1_UNorm_SRgb,
            BufferFormat.BC2_UNorm_SRgb,
            BufferFormat.BC3_UNorm_SRgb,
            BufferFormat.BC7_UNorm_SRgb,
            BufferFormat.BC4_SNorm,
            BufferFormat.BC5_SNorm,
            BufferFormat.BC6H_Sf16,
            BufferFormat.BC6H_Uf16            
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);
            Assert.IsTrue(info.IsCompressed, $"{format} not compressed");
        }        
    }

    [TestMethod]
    public void Groups()
    {
        Dictionary<BufferFormat, List<BufferFormat>> groupedFormats = new()
        {
            [BufferFormat.R32G32B32A32_Typeless] = new() { BufferFormat.R32G32B32A32_Float, BufferFormat.R32G32B32A32_UInt, BufferFormat.R32G32B32A32_SInt },
            [BufferFormat.R32G32B32_Typeless] = new() { BufferFormat.R32G32B32_Float, BufferFormat.R32G32B32_UInt, BufferFormat.R32G32B32_SInt },
            [BufferFormat.R16G16B16A16_Typeless] = new() { BufferFormat.R16G16B16A16_Float, BufferFormat.R16G16B16A16_UNorm, BufferFormat.R16G16B16A16_UInt, BufferFormat.R16G16B16A16_SNorm, BufferFormat.R16G16B16A16_SInt },
            [BufferFormat.R32G32_Typeless] = new() { BufferFormat.R32G32_Float, BufferFormat.R32G32_UInt, BufferFormat.R32G32_SInt },
            [BufferFormat.R10G10B10A2_Typeless] = new() { BufferFormat.R10G10B10A2_UNorm, BufferFormat.R10G10B10A2_UInt },
            [BufferFormat.R8G8B8A8_Typeless] = new() { BufferFormat.R8G8B8A8_UNorm, BufferFormat.R8G8B8A8_UNorm_SRgb, BufferFormat.R8G8B8A8_UInt, BufferFormat.R8G8B8A8_SNorm, BufferFormat.R8G8B8A8_SInt },
            [BufferFormat.R16G16_Typeless] = new() { BufferFormat.R16G16_Float, BufferFormat.R16G16_UNorm, BufferFormat.R16G16_UInt, BufferFormat.R16G16_SNorm, BufferFormat.R16G16_SInt },
            [BufferFormat.R32_Typeless] = new() { BufferFormat.R32_Float, BufferFormat.R32_UInt, BufferFormat.R32_SInt },
            [BufferFormat.R8G8_Typeless] = new() { BufferFormat.R8G8_UNorm, BufferFormat.R8G8_UInt, BufferFormat.R8G8_SNorm, BufferFormat.R8G8_SInt },
            [BufferFormat.R16_Typeless] = new() { BufferFormat.R16_Float, BufferFormat.R16_UNorm, BufferFormat.R16_UInt, BufferFormat.R16_SNorm, BufferFormat.R16_SInt },
            [BufferFormat.R8_Typeless] = new() { BufferFormat.R8_UNorm, BufferFormat.R8_UInt, BufferFormat.R8_SNorm, BufferFormat.R8_SInt },
            [BufferFormat.BC1_Typeless] = new() { BufferFormat.BC1_UNorm, BufferFormat.BC1_UNorm_SRgb },
            [BufferFormat.BC2_Typeless] = new() { BufferFormat.BC2_UNorm, BufferFormat.BC2_UNorm_SRgb },
            [BufferFormat.BC3_Typeless] = new() { BufferFormat.BC3_UNorm, BufferFormat.BC3_UNorm_SRgb },
            [BufferFormat.BC4_Typeless] = new() { BufferFormat.BC4_UNorm, BufferFormat.BC4_SNorm },
            [BufferFormat.BC5_Typeless] = new() { BufferFormat.BC5_UNorm, BufferFormat.BC5_SNorm },
            [BufferFormat.B8G8R8A8_Typeless] = new() { BufferFormat.B8G8R8A8_UNorm, BufferFormat.B8G8R8A8_UNorm_SRgb },
            [BufferFormat.B8G8R8X8_Typeless] = new() { BufferFormat.B8G8R8X8_UNorm, BufferFormat.B8G8R8X8_UNorm_SRgb },
            [BufferFormat.BC7_Typeless] = new() { BufferFormat.BC7_UNorm, BufferFormat.BC7_UNorm_SRgb }
        };

        foreach (BufferFormat format in groupedFormats.SelectMany(item => item.Value))
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(groupedFormats.ContainsKey(info.Group), $"{info.Group} not found.");
            Assert.IsTrue(groupedFormats[info.Group].Contains(info.Format), $"{info.Format} not found in group {info.Group}");
        }
    }

    [TestMethod]
    public void HasDepth()
    {
        BufferFormat[] formats = [
            BufferFormat.D16_UNorm,
            BufferFormat.D24_UNorm_S8_UInt,
            BufferFormat.D32_Float,
            BufferFormat.D32_Float_S8X24_UInt            
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.HasDepth, $"{format} not depth");
        }
    }

    [TestMethod]
    public void HasStencil()
    {
        BufferFormat[] formats = [
            BufferFormat.D24_UNorm_S8_UInt,
            BufferFormat.D32_Float_S8X24_UInt
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.HasStencil, $"{format} not stencil");
        }
    }

    [TestMethod]
    public void IsPacked()
    {
        BufferFormat[] formats = [
            BufferFormat.R8G8_B8G8_UNorm,
            BufferFormat.G8R8_G8B8_UNorm,
            BufferFormat.Y410,
            BufferFormat.Y416,
            BufferFormat.Y210,
            BufferFormat.Y216
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.IsPacked, $"{format} not packed!");
        }
    }

    [TestMethod]
    public void IsPlanar()
    {
        BufferFormat[] formats = [
            BufferFormat.P010,
            BufferFormat.P016,
            BufferFormat.P208,
            BufferFormat.Opaque420,
            BufferFormat.NV11,
            BufferFormat.NV12
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.IsPlanar, $"{format} not planar");
        }
    }

    public void IsPalettized() 
    {
        BufferFormat[] formats =
        {
            BufferFormat.P8,
            BufferFormat.A8P8,
        };

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.IsPalettized, $"{format} should be palettized.");
        }
    }

    [TestMethod]
    public void IsTypeless()
    {
        BufferFormat[] formats = [
            BufferFormat.R32G32B32A32_Typeless,
            BufferFormat.BC1_Typeless,
            BufferFormat.BC2_Typeless,
            BufferFormat.BC3_Typeless,
            BufferFormat.BC4_Typeless,
            BufferFormat.BC5_Typeless,
            BufferFormat.BC6H_Typeless,
            BufferFormat.BC7_Typeless,
            BufferFormat.R32G32B32_Typeless,
            BufferFormat.R32G32_Typeless,
            BufferFormat.R32G8X24_Typeless,
            BufferFormat.R32_Float_X8X24_Typeless,
            BufferFormat.X32_Typeless_G8X24_UInt,
            BufferFormat.R16G16B16A16_Typeless,
            BufferFormat.R32_Typeless,
            BufferFormat.R24G8_Typeless,
            BufferFormat.R24_UNorm_X8_Typeless,
            BufferFormat.R16G16_Typeless,
            BufferFormat.R10G10B10A2_Typeless,
            BufferFormat.R8G8B8A8_Typeless,
            BufferFormat.B8G8R8A8_Typeless,
            BufferFormat.B8G8R8X8_Typeless,
            BufferFormat.R16_Typeless,
            BufferFormat.R8G8_Typeless,
            BufferFormat.R8_Typeless
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.IsTypeless, $"{format} should be typeless.");
        }
    }

    [TestMethod]
    public void HasAlpha()
    {
        BufferFormat[] formats = [
            BufferFormat.R32G32B32A32_Float,
            BufferFormat.R32G32B32A32_Typeless,
            BufferFormat.R32G32B32A32_UInt,
            BufferFormat.R32G32B32A32_SInt,
            BufferFormat.BC1_Typeless,
            BufferFormat.BC1_UNorm,
            BufferFormat.BC1_UNorm_SRgb,
            BufferFormat.BC2_Typeless,
            BufferFormat.BC2_UNorm,
            BufferFormat.BC2_UNorm_SRgb,
            BufferFormat.BC3_Typeless,
            BufferFormat.BC3_UNorm,
            BufferFormat.BC3_UNorm_SRgb,
            BufferFormat.BC7_Typeless,
            BufferFormat.BC7_UNorm,
            BufferFormat.BC7_UNorm_SRgb,
            BufferFormat.R16G16B16A16_Typeless,
            BufferFormat.R16G16B16A16_Float,
            BufferFormat.R16G16B16A16_UNorm,
            BufferFormat.R16G16B16A16_UInt,
            BufferFormat.R16G16B16A16_SNorm,
            BufferFormat.R16G16B16A16_SInt,
            BufferFormat.R10G10B10A2_Typeless,
            BufferFormat.R10G10B10A2_UNorm,
            BufferFormat.R10G10B10A2_UInt,
            BufferFormat.R10G10B10_Xr_Bias_A2_UNorm,
            BufferFormat.R8G8B8A8_Typeless,
            BufferFormat.R8G8B8A8_UNorm,
            BufferFormat.R8G8B8A8_UNorm_SRgb,
            BufferFormat.R8G8B8A8_UInt,
            BufferFormat.R8G8B8A8_SNorm,
            BufferFormat.R8G8B8A8_SInt,
            BufferFormat.B8G8R8A8_UNorm,
            BufferFormat.B8G8R8A8_Typeless,
            BufferFormat.B8G8R8A8_UNorm_SRgb,
            BufferFormat.B5G5R5A1_UNorm,
            BufferFormat.B4G4R4A4_UNorm,
            BufferFormat.A8_UNorm
        ];

        foreach (BufferFormat format in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.IsTrue(info.HasAlpha, $"{format} should have alpha channel.");            
        }
    }

    [TestMethod]
    public void BitDepth()
    {
        (BufferFormat format, int bitCount)[] formats = [
            (BufferFormat.Unknown, 0),
            (BufferFormat.R32G32B32A32_Float, 128),
            (BufferFormat.R32G32B32A32_Typeless, 128),
            (BufferFormat.R32G32B32A32_UInt, 128),
            (BufferFormat.R32G32B32A32_SInt, 128),
            (BufferFormat.BC2_Typeless, 128),
            (BufferFormat.BC2_UNorm, 128),
            (BufferFormat.BC2_UNorm_SRgb, 128),
            (BufferFormat.BC3_Typeless, 128),
            (BufferFormat.BC3_UNorm, 128),
            (BufferFormat.BC3_UNorm_SRgb, 128),
            (BufferFormat.BC5_Typeless, 128),
            (BufferFormat.BC5_UNorm, 128),
            (BufferFormat.BC5_SNorm, 128),
            (BufferFormat.BC6H_Typeless, 128),
            (BufferFormat.BC6H_Uf16, 128),
            (BufferFormat.BC6H_Sf16, 128),
            (BufferFormat.BC7_Typeless, 128),
            (BufferFormat.BC7_UNorm, 128),
            (BufferFormat.BC7_UNorm_SRgb, 128),
            (BufferFormat.R32G32B32_Typeless, 96),
            (BufferFormat.R32G32B32_Float, 96),
            (BufferFormat.R32G32B32_UInt, 96),
            (BufferFormat.R32G32B32_SInt, 96),
            (BufferFormat.BC1_Typeless, 64),
            (BufferFormat.BC1_UNorm, 64),
            (BufferFormat.BC1_UNorm_SRgb, 64),
            (BufferFormat.BC4_Typeless, 64),
            (BufferFormat.BC4_UNorm, 64),
            (BufferFormat.BC4_SNorm, 64),
            (BufferFormat.R32G32_Typeless, 64),
            (BufferFormat.R32G32_Float, 64),
            (BufferFormat.R32G32_UInt, 64),
            (BufferFormat.R32G32_SInt, 64),
            (BufferFormat.R32G8X24_Typeless, 64),
            (BufferFormat.D32_Float_S8X24_UInt, 64),
            (BufferFormat.R32_Float_X8X24_Typeless, 64),
            (BufferFormat.X32_Typeless_G8X24_UInt, 64),
            (BufferFormat.R16G16B16A16_Typeless, 64),
            (BufferFormat.R16G16B16A16_Float, 64),
            (BufferFormat.R16G16B16A16_UNorm, 64),
            (BufferFormat.R16G16B16A16_UInt, 64),
            (BufferFormat.R16G16B16A16_SNorm, 64),
            (BufferFormat.R16G16B16A16_SInt, 64),
            (BufferFormat.R32_Typeless, 32),
            (BufferFormat.R32_Float, 32),
            (BufferFormat.R32_UInt, 32),
            (BufferFormat.R32_SInt, 32),
            (BufferFormat.D32_Float, 32),
            (BufferFormat.R24G8_Typeless, 32),
            (BufferFormat.D24_UNorm_S8_UInt, 32),
            (BufferFormat.R24_UNorm_X8_Typeless, 32),
            (BufferFormat.X24_Typeless_G8_UInt, 32),
            (BufferFormat.R16G16_Typeless, 32),
            (BufferFormat.R16G16_Float, 32),
            (BufferFormat.R16G16_UNorm, 32),
            (BufferFormat.R16G16_UInt, 32),
            (BufferFormat.R16G16_SNorm, 32),
            (BufferFormat.R16G16_SInt, 32),
            (BufferFormat.R10G10B10A2_Typeless, 32),
            (BufferFormat.R10G10B10A2_UNorm, 32),
            (BufferFormat.R10G10B10A2_UInt, 32),
            (BufferFormat.R10G10B10_Xr_Bias_A2_UNorm, 32),
            (BufferFormat.R11G11B10_Float, 32 ),
            (BufferFormat.R8G8B8A8_Typeless, 32),
            (BufferFormat.R8G8B8A8_UNorm, 32),
            (BufferFormat.R8G8B8A8_UNorm_SRgb, 32),
            (BufferFormat.R8G8B8A8_UInt, 32),
            (BufferFormat.R8G8B8A8_SNorm, 32),
            (BufferFormat.R8G8B8A8_SInt, 32),
            (BufferFormat.B8G8R8A8_UNorm, 32),
            (BufferFormat.B8G8R8X8_UNorm, 32),
            (BufferFormat.B8G8R8A8_Typeless, 32),
            (BufferFormat.B8G8R8A8_UNorm_SRgb, 32),
            (BufferFormat.B8G8R8X8_Typeless, 32),
            (BufferFormat.B8G8R8X8_UNorm_SRgb, 32),
            (BufferFormat.R8G8_B8G8_UNorm, 32),
            (BufferFormat.G8R8_G8B8_UNorm, 32),
            (BufferFormat.Y410, 32),
            (BufferFormat.Y416, 32),
            (BufferFormat.AYUV, 32),
            (BufferFormat.R9G9B9E5_SharedExp, 32),
            (BufferFormat.R16_Typeless, 16),
            (BufferFormat.R16_Float, 16),
            (BufferFormat.D16_UNorm, 16),
            (BufferFormat.R16_UNorm, 16),
            (BufferFormat.R16_UInt, 16),
            (BufferFormat.R16_SNorm, 16),
            (BufferFormat.R16_SInt, 16),
            (BufferFormat.R8G8_Typeless, 16),
            (BufferFormat.R8G8_UNorm, 16),
            (BufferFormat.R8G8_UInt, 16),
            (BufferFormat.R8G8_SNorm, 16),
            (BufferFormat.R8G8_SInt, 16),
            (BufferFormat.B5G5R5A1_UNorm, 16),
            (BufferFormat.B5G6R5_UNorm, 16),
            (BufferFormat.B4G4R4A4_UNorm, 16),
            (BufferFormat.A8P8, 16),
            (BufferFormat.P010, 16),
            (BufferFormat.P016, 16),
            (BufferFormat.Y210, 16),
            (BufferFormat.Y216, 16),
            (BufferFormat.YUY2, 16),
            (BufferFormat.NV11, 16),
            (BufferFormat.NV12, 16),
            (BufferFormat.Opaque420, 16),
            (BufferFormat.P208, 16),
            (BufferFormat.V208, 16),
            (BufferFormat.V408, 16),
            (BufferFormat.R8_Typeless, 8),
            (BufferFormat.R8_UNorm, 8),
            (BufferFormat.R8_UInt, 8),
            (BufferFormat.R8_SNorm, 8),
            (BufferFormat.R8_SInt, 8),
            (BufferFormat.A8_UNorm, 8),
            (BufferFormat.AI44, 8),
            (BufferFormat.IA44, 8),
            (BufferFormat.P8, 8),            
            (BufferFormat.R1_UNorm, 8)
        ];

        foreach ((BufferFormat format, int bitSize) in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.AreEqual(bitSize, info.BitDepth, $"{format} bit size should be {bitSize}, but is {info.BitDepth}");
            Assert.AreEqual(bitSize / 8, info.SizeInBytes);
        }
    }

    [TestMethod]
    public void ComponentCount()
    {
        (BufferFormat format, int bitCount)[] formats = [
            (BufferFormat.R32G32B32A32_Float, 4),
            (BufferFormat.R32G32B32A32_Typeless, 4),
            (BufferFormat.R32G32B32A32_UInt, 4),
            (BufferFormat.R32G32B32A32_SInt, 4),
            (BufferFormat.R32G32B32_Typeless, 3),
            (BufferFormat.R32G32B32_Float, 3),
            (BufferFormat.R32G32B32_UInt, 3),
            (BufferFormat.R32G32B32_SInt, 3),
            (BufferFormat.R32G32_Typeless, 2),
            (BufferFormat.R32G32_Float, 2),
            (BufferFormat.R32G32_UInt, 2),
            (BufferFormat.R32G32_SInt, 2),
            (BufferFormat.R32G8X24_Typeless, 2),
            (BufferFormat.D32_Float_S8X24_UInt, 2),
            (BufferFormat.R32_Float_X8X24_Typeless, 1),
            (BufferFormat.X32_Typeless_G8X24_UInt, 1),
            (BufferFormat.R16G16B16A16_Typeless, 4),
            (BufferFormat.R16G16B16A16_Float, 4),
            (BufferFormat.R16G16B16A16_UNorm, 4),
            (BufferFormat.R16G16B16A16_UInt, 4),
            (BufferFormat.R16G16B16A16_SNorm, 4),
            (BufferFormat.R16G16B16A16_SInt, 4),
            (BufferFormat.R32_Typeless, 1),
            (BufferFormat.R32_Float, 1),
            (BufferFormat.R32_UInt, 1),
            (BufferFormat.R32_SInt, 1),
            (BufferFormat.D32_Float, 1),
            (BufferFormat.R24G8_Typeless, 2),
            (BufferFormat.D24_UNorm_S8_UInt, 2),
            (BufferFormat.R24_UNorm_X8_Typeless, 1),
            (BufferFormat.X24_Typeless_G8_UInt, 1),
            (BufferFormat.R16G16_Typeless, 2),
            (BufferFormat.R16G16_Float, 2),
            (BufferFormat.R16G16_UNorm, 2),
            (BufferFormat.R16G16_UInt, 2),
            (BufferFormat.R16G16_SNorm, 2),
            (BufferFormat.R16G16_SInt, 2),
            (BufferFormat.R10G10B10A2_Typeless, 4),
            (BufferFormat.R10G10B10A2_UNorm, 4),
            (BufferFormat.R10G10B10A2_UInt, 4),
            (BufferFormat.R10G10B10_Xr_Bias_A2_UNorm, 4),
            (BufferFormat.R11G11B10_Float, 3),
            (BufferFormat.R8G8B8A8_Typeless, 4),
            (BufferFormat.R8G8B8A8_UNorm, 4),
            (BufferFormat.R8G8B8A8_UNorm_SRgb, 4),
            (BufferFormat.R8G8B8A8_UInt, 4),
            (BufferFormat.R8G8B8A8_SNorm, 4),
            (BufferFormat.R8G8B8A8_SInt, 4),
            (BufferFormat.B8G8R8A8_UNorm, 4),
            (BufferFormat.B8G8R8X8_UNorm, 3),
            (BufferFormat.B8G8R8A8_Typeless, 4),
            (BufferFormat.B8G8R8A8_UNorm_SRgb, 4),
            (BufferFormat.B8G8R8X8_Typeless, 3),
            (BufferFormat.B8G8R8X8_UNorm_SRgb, 3),
            (BufferFormat.R8G8_B8G8_UNorm, 4),
            (BufferFormat.G8R8_G8B8_UNorm, 4),
            (BufferFormat.R9G9B9E5_SharedExp, 3),
            (BufferFormat.R16_Typeless, 1),
            (BufferFormat.R16_Float, 1),
            (BufferFormat.D16_UNorm, 1),
            (BufferFormat.R16_UNorm, 1),
            (BufferFormat.R16_UInt, 1),
            (BufferFormat.R16_SNorm, 1),
            (BufferFormat.R16_SInt, 1),
            (BufferFormat.R8G8_Typeless, 2),
            (BufferFormat.R8G8_UNorm, 2),
            (BufferFormat.R8G8_UInt, 2),
            (BufferFormat.R8G8_SNorm, 2),
            (BufferFormat.R8G8_SInt, 2),
            (BufferFormat.B5G5R5A1_UNorm, 4),
            (BufferFormat.B5G6R5_UNorm, 3),
            (BufferFormat.B4G4R4A4_UNorm, 4),
            (BufferFormat.A8P8, 2),
            (BufferFormat.R8_Typeless, 1),
            (BufferFormat.R8_UNorm, 1),
            (BufferFormat.R8_UInt, 1),
            (BufferFormat.R8_SNorm, 1),
            (BufferFormat.R8_SInt, 1),
            (BufferFormat.A8_UNorm, 1),
            (BufferFormat.AI44, 2),
            (BufferFormat.IA44, 2),
            (BufferFormat.P8, 1),
            (BufferFormat.R1_UNorm, 1)
        ];

        foreach ((BufferFormat format, int componentCount) in formats)
        {
            GorgonFormatInfo info = new(format);

            Assert.AreEqual(componentCount, info.ComponentCount, $"{format} bit size should be {componentCount}, but is {info.ComponentCount}");
        }
    }

    [TestMethod]
    public void CalculateScanLines()
    {
        GorgonFormatInfo info = new(BufferFormat.R8G8B8A8_UNorm);

        int actual = info.CalculateScanlines(200);

        Assert.AreEqual(200, actual, $"{BufferFormat.R8G8B8A8_UNorm} scanline count incorrect.");

        info = new(BufferFormat.NV11);

        actual = info.CalculateScanlines(200);

        Assert.AreEqual(400, actual, $"{BufferFormat.NV11} scanline count incorrect.");

        BufferFormat[] planarFormats = [
            BufferFormat.NV12,
            BufferFormat.P010,
            BufferFormat.P016,
            BufferFormat.Opaque420
        ];

        foreach (BufferFormat format in planarFormats)
        {
            info = new(format);

            actual = info.CalculateScanlines(200);

            Assert.AreEqual(300, actual, $"Planar {format} scanline count incorrect.");
        }
    }

    [TestMethod]
    public void GetPitchForFormat()
    {
        GorgonFormatInfo info = new(BufferFormat.R8G8B8A8_UNorm);
        GorgonPitchLayout layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(1280, layout.RowPitch, $"{BufferFormat.R8G8B8A8_UNorm} row pitch incorrect.");
        Assert.AreEqual(256_000, layout.SlicePitch, $"{BufferFormat.R8G8B8A8_UNorm} slice pitch incorrect.");

        info = new(BufferFormat.BC1_UNorm);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(640, layout.RowPitch, $"{BufferFormat.BC1_UNorm} row pitch incorrect.");
        Assert.AreEqual(32_000, layout.SlicePitch, $"{BufferFormat.BC1_UNorm} slice pitch incorrect.");
        Assert.AreEqual(80, layout.HorizontalBlockCount, $"{BufferFormat.BC1_UNorm} H block count incorrect.");
        Assert.AreEqual(50, layout.VerticalBlockCount, $"{BufferFormat.BC1_UNorm} V block count incorrect.");

        info = new(BufferFormat.BC7_UNorm);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(1280, layout.RowPitch, $"{BufferFormat.BC7_UNorm} row pitch incorrect.");
        Assert.AreEqual(64_000, layout.SlicePitch, $"{BufferFormat.BC7_UNorm} slice pitch incorrect.");
        Assert.AreEqual(80, layout.HorizontalBlockCount, $"{BufferFormat.BC7_UNorm} H block count incorrect.");
        Assert.AreEqual(50, layout.VerticalBlockCount, $"{BufferFormat.BC1_UNorm} V block count incorrect.");

        info = new(BufferFormat.R8G8_B8G8_UNorm);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(640, layout.RowPitch, $"{BufferFormat.R8G8B8A8_UNorm} row pitch incorrect.");
        Assert.AreEqual(128_000, layout.SlicePitch, $"{BufferFormat.R8G8B8A8_UNorm} slice pitch incorrect.");

        info = new(BufferFormat.Y210);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(2560, layout.RowPitch, $"{BufferFormat.Y210} row pitch incorrect.");
        Assert.AreEqual(512_000, layout.SlicePitch, $"{BufferFormat.R8G8B8A8_UNorm} slice pitch incorrect.");

        info = new(BufferFormat.NV11);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(320, layout.RowPitch, $"{BufferFormat.NV11} row pitch incorrect.");
        Assert.AreEqual(128_000, layout.SlicePitch, $"{BufferFormat.NV11} slice pitch incorrect.");

        info = new(BufferFormat.NV12);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(320, layout.RowPitch, $"{BufferFormat.NV12} row pitch incorrect.");
        Assert.AreEqual(96_000, layout.SlicePitch, $"{BufferFormat.NV12} slice pitch incorrect.");

        info = new(BufferFormat.P010);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(640, layout.RowPitch, $"{BufferFormat.P010} row pitch incorrect.");
        Assert.AreEqual(192_000, layout.SlicePitch, $"{BufferFormat.P010} slice pitch incorrect.");

        info = new(BufferFormat.P208);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(320, layout.RowPitch, $"{BufferFormat.P208} row pitch incorrect.");
        Assert.AreEqual(128_000, layout.SlicePitch, $"{BufferFormat.P208} slice pitch incorrect.");

        info = new(BufferFormat.V208);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(320, layout.RowPitch, $"{BufferFormat.V208} row pitch incorrect.");
        Assert.AreEqual(192_000, layout.SlicePitch, $"{BufferFormat.V208} silce pitch incorrect.");

        info = new(BufferFormat.V408);
        layout = info.GetPitchForFormat(320, 200, PitchFlags.None);

        Assert.AreEqual(320, layout.RowPitch, $"{BufferFormat.V408} row pitch incorrect.");
        Assert.AreEqual(192_000, layout.SlicePitch, $"{BufferFormat.V408} slice pitch incorrect.");
    }
}
