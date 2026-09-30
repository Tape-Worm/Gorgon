// Codec option classes (option bag reads of list options).
// Moved from the Imaging integration battery (Test/Experimental/ImagingFunctions): memory only, no WIC, no codec IO.

using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Graphics.Imaging.Tests;

[TestClass]
public class CodecOptionTests
{
    [TestMethod]
    [Description("Can GorgonGifEncodingOptions.FrameDelays be read on a fresh instance?")]
    public void GifFrameDelaysDefault() => ImagingTestHelper.AssertPass(CheckGifFrameDelaysDefault());

    [TestMethod]
    [Description("Can GorgonGifEncodingOptions.FrameDelays be read back after setting it?")]
    public void GifFrameDelaysSet() => ImagingTestHelper.AssertPass(CheckGifFrameDelaysSet());

    [TestMethod]
    [Description("Can GorgonDdsDecodingOptions.Palette be read on a fresh instance?")]
    public void DdsPaletteDefault() => ImagingTestHelper.AssertPass(CheckDdsPaletteDefault());

    [TestMethod]
    [Description("Can GorgonGifEncodingOptions.Palette be read on a fresh instance (created as IList, control)?")]
    public void GifEncodingPaletteDefault() => ImagingTestHelper.AssertPass(CheckGifEncodingPaletteDefault());

    [TestMethod]
    [Description("Can GorgonGifDecodingOptions.Palette be read on a fresh instance (created as IList, control)?")]
    public void GifDecodingPaletteDefault() => ImagingTestHelper.AssertPass(CheckGifDecodingPaletteDefault());

    private static string? CheckGifFrameDelaysDefault() => ImagingTestHelper.Check(new GorgonGifEncodingOptions().FrameDelays.Count == 0, "FrameDelays is not empty.");

    private static string? CheckGifFrameDelaysSet()
    {
        GorgonGifEncodingOptions options = new()
        {
            FrameDelays = [10, 20]
        };

        return ImagingTestHelper.Check((options.FrameDelays.Count == 2) && (options.FrameDelays[1] == 20), $"FrameDelays is [{string.Join(", ", options.FrameDelays)}].");
    }

    private static string? CheckDdsPaletteDefault() => ImagingTestHelper.Check(new GorgonDdsDecodingOptions().Palette.Count == 0, "Palette is not empty.");

    private static string? CheckGifEncodingPaletteDefault() => ImagingTestHelper.Check(new GorgonGifEncodingOptions().Palette.Count == 0, "Palette is not empty.");

    private static string? CheckGifDecodingPaletteDefault() => ImagingTestHelper.Check(new GorgonGifDecodingOptions().Palette.Count == 0, "Palette is not empty.");

    [TestMethod]
    [Description("Does GorgonPngEncodingOptions.DpiX keep a fractional value (the option was created as an int)?")]
    public void PngDpiKeepsFraction()
    {
        GorgonPngEncodingOptions options = new()
        {
            DpiX = 96.5,
            DpiY = 72.25
        };

        Assert.AreEqual(96.5, options.DpiX);
        Assert.AreEqual(72.25, options.DpiY);
        Assert.AreEqual(96.5, options.Options.GetOptionValue<double>(nameof(GorgonPngEncodingOptions.DpiX)));
    }

    [TestMethod]
    [Description("Does GorgonJpegEncodingOptions.ImageQuality clamp to 0 - 1?")]
    public void JpegQualityClamps()
    {
        GorgonJpegEncodingOptions options = new()
        {
            ImageQuality = 2.0f
        };
        float high = options.ImageQuality;

        options.ImageQuality = -1.0f;

        Assert.AreEqual(1.0f, high);
        Assert.AreEqual(0.0f, options.ImageQuality);
    }

    [TestMethod]
    [Description("Does GorgonGifEncodingOptions.AlphaThreshold clamp to 0 - 1?")]
    public void GifAlphaThresholdClamps()
    {
        GorgonGifEncodingOptions options = new()
        {
            AlphaThreshold = 5.0f
        };

        Assert.AreEqual(1.0f, options.AlphaThreshold);
    }

    [TestMethod]
    [Description("Does a value set through the option bag show up in the property, and the reverse?")]
    public void PropertyAndOptionBagAgree()
    {
        GorgonPngEncodingOptions png = new();
        GorgonTgaDecodingOptions tga = new()
        {
            SetZeroAlphaAsOpaque = false
        };

        png.Options.SetOptionValue(nameof(GorgonPngEncodingOptions.Interlacing), true);
        png.Options.SetOptionValue(nameof(GorgonPngEncodingOptions.Filter), PngFilter.Paeth);

        Assert.IsTrue(png.Interlacing);
        Assert.AreEqual(PngFilter.Paeth, png.Filter);
        Assert.IsFalse(tga.Options.GetOptionValue<bool>(nameof(GorgonTgaDecodingOptions.SetZeroAlphaAsOpaque)));
    }
}
