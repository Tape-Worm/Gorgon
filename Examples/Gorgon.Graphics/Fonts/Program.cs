﻿
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 14, 2018 7:28:55 PM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// The example entry point
/// </summary>
static class Program
{

    // The main graphics interface.
    private static GorgonGraphics _graphics;
    // The swap chain representing our "screen".
    private static GorgonSwapChain _screen;
    // Our 2D renderer, used to draw text.
    private static Gorgon2D _renderer;
    // The list of built-in Windows font family names to use.
    private static readonly List<string> _fontFamilies =
    [
        "Usuzi",
        "Algerian",
        "Bauhaus 93",
        "Blackadder ITC",
        "Viner Hand ITC",
        "Broadway",
        "Rage Italic",
        "Chiller",
        "Old English Text MT",
        "Times New Roman",
        "Ravie",
        "Playbill"
    ];
    // The fonts for the application. 
    private static readonly List<GorgonFont> _font = [];
    // The starting time.
    private static float _startTime = float.MinValue;
    // The current font index.
    private static int _fontIndex;
    // The index of the font that uses glow (so we can actually see it).
    private static int _glowIndex;
    // The text to draw with our font.
    private static GorgonTextSprite _textSprite;
    // The alpha value for the glow effect.
    private static float _glowAlpha = 1.0f;
    // The velocity at which to animate the glow effect.
    private static float _glowVelocity = -0.5f;
    // Angle used to calculate the bounce animation.
    private static float _bounceAngle = -90.0f;
    // The max height of the last line on the bounce.
    private static float _max;
    // The velocity of the rotation for calculating the bounce.
    private static float _angleSpeed = 360.0f;
    // The text to display.
    private readonly static string _text = Resources.Lorem_Ipsum;

    /// <summary>
    /// Function to generate the Gorgon bitmap fonts.
    /// </summary>
    /// <param name="fontFamilies">The list of TrueType font families to use.</param>
    /// <param name="window">The window that contains the loading message.</param>
    private static async Task GenerateGorgonFontsAsync(IReadOnlyList<FontFamily> fontFamilies, FormMain window)
    {
        // Pick a font to use with outlines.
        int fontWithOutlineIndex = GorgonRandom.RandomInt32(1, 5);
        _glowIndex = GorgonRandom.RandomInt32(fontWithOutlineIndex + 1, fontWithOutlineIndex + 5);
        int fontWithGradient = GorgonRandom.RandomInt32(_glowIndex + 1, _glowIndex + 5);
        int fontWithTexture = GorgonRandom.RandomInt32(fontWithGradient + 1, fontWithGradient + 5).Min(_fontFamilies.Count - 1);

        GorgonCodecPng pngCodec = new();
        using IGorgonImage texture = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\Fonts\").FullName, "Gradient.png"));
        for (int i = 0; i < _fontFamilies.Count; ++i)
        {
            string fontFamily = _fontFamilies[i];

            // Use this to determine if the font is avaiable.
            if (fontFamilies.All(item => !string.Equals(item.Name, fontFamily, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Can't locate this one, move on...
                continue;
            }

            bool isExternal = FontFamily.Families.All(item => !string.Equals(item.Name, fontFamily, StringComparison.InvariantCultureIgnoreCase));
            string fontName;
            int outlineSize = 0;
            GorgonColor outlineColor1 = GorgonColors.BlackTransparent;
            GorgonColor outlineColor2 = GorgonColors.BlackTransparent;
            GorgonGlyphBrush brush = null;

            if (i == fontWithOutlineIndex)
            {
                fontName = $"{fontFamily} 32px Outlined{(isExternal ? " External TTF" : string.Empty)}";
                outlineColor1 = GorgonColors.Black;
                outlineColor2 = GorgonColors.Black;
                outlineSize = 3;
            }
            else if (i == _glowIndex)
            {
                fontName = $"{fontFamily} 32px Outline as Glow{(isExternal ? " External TTF" : string.Empty)}";
                outlineColor1 = new GorgonColor(GorgonColors.Yellow, 1.0f);
                outlineColor2 = new GorgonColor(GorgonColors.DarkRed, 0.0f);
                outlineSize = 16;
            }
            else if (i == fontWithGradient)
            {
                fontName = $"{fontFamily} 32px Gradient{(isExternal ? " External TTF" : string.Empty)}";
                brush = new GorgonGlyphLinearGradientBrush
                {
                    StartColor = GorgonColors.White,
                    EndColor = GorgonColors.Black,
                    Angle = 45.0f
                };
            }
            else if (i == fontWithTexture)
            {
                fontName = $"{fontFamily} 32px Textured{(isExternal ? " External TTF" : string.Empty)}";
                brush = new GorgonGlyphTextureBrush(texture);
            }
            else
            {
                fontName = $"{fontFamily} 32px{(isExternal ? " External TTF" : string.Empty)}";
            }

            window.UpdateStatus($"Generating Font: {fontFamily}".Ellipses(50));

            GorgonFontInfo fontInfo = new(fontFamily,
                                              30.25f,
                                              GorgonFontHeightMode.Pixels)
            {
                Name = fontName,
                AntiAliasingMode = GorgonFontAntiAliasMode.AntiAlias,
                OutlineSize = outlineSize,
                OutlineColor1 = outlineColor1,
                OutlineColor2 = outlineColor2,
                UsePremultipliedTextures = false,
                Brush = brush
            };

            // Because fonts can take a bit of time to generate (especially if using compression), we can retrieve the font asynchronously from the 
            // font factory. In addition to an async method, a GetFont method is also available to grab/generate the font synchronously.
            _font.Add(await GorgonExample.Fonts.GetFontAsync(fontInfo));

            // Texture brushes have to be disposed when we're done with them.
            IDisposable disposableBrush = brush as IDisposable;
            disposableBrush?.Dispose();
        }
    }

    /// <summary>
    /// Function to load true type fonts to generate from.
    /// </summary>
    /// <param name="window">The window containing the loading message.</param>
    /// <returns>The font families to use when building the bitmap fonts.</returns>
    private static Task<IReadOnlyList<FontFamily>> LoadTrueTypeFontsAsync(FormMain window)
    {
        // Load in a bunch of true type fonts.
        DirectoryInfo dirInfo = GorgonExample.GetResourcePath("Fonts");
        FileInfo[] files = dirInfo.GetFiles("*.ttf", SearchOption.TopDirectoryOnly);

        List<FontFamily> fontFamilies = [];

        return Task.Run(() =>
        {
            // Load all external true type fonts for this example.
            // This takes a while...
            foreach (FileInfo file in files)
            {
                window.UpdateStatus($"Loading Font: {file.FullName}".Ellipses(50));
                FontFamily externFont = GorgonExample.Fonts.LoadTrueTypeFontFamily(file.FullName);
                _fontFamilies.Insert(0, externFont.Name);
                fontFamilies.Add(externFont);
            }

            // Load this font from our resources section.
            window.UpdateStatus($"Loading Resource Font...");
            using (MemoryStream stream = new(Resources.Achafexp))
            {
                FontFamily resFont = GorgonExample.Fonts.LoadTrueTypeFontFamily(stream);
                _fontFamilies.Insert(0, resFont.Name);
                fontFamilies.Add(resFont);
            }

            window.UpdateStatus(null);

            fontFamilies.AddRange(FontFamily.Families);

            return (IReadOnlyList<FontFamily>)fontFamilies;
        });
    }

    /// <summary>
    /// Function called during idle time the application.
    /// </summary>
    /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        GorgonFont currentFont = _font[_fontIndex];

        if (_startTime < 0)
        {
            _startTime = GorgonTiming.SecondsSinceStart;
        }

        _screen.RenderTargetView.Clear(_glowIndex != _fontIndex ? GorgonColors.CornFlowerBlue : new GorgonColor(0, 0, 0.2f));

        Vector2 textSize = _text.MeasureText(currentFont, false);
        Vector2 position = new((int)((_screen.Width / 2.0f) - (textSize.X / 2.0f)).Max(4.0f), (int)((_screen.Height / 2.0f) - (textSize.Y / 2.0f)).Max(100));
        _textSprite.Font = currentFont;
        _textSprite.Position = position;

        // If we have glow on, then draw the glow outline in a separate pass.
        if (_glowIndex == _fontIndex)
        {
            _textSprite.OutlineTint = new GorgonColor(1, 1, 1, _glowAlpha);
            _textSprite.DrawMode = TextDrawMode.OutlineOnly;
            _renderer.Begin(Gorgon2DBatchState.AdditiveBlend);
            _renderer.DrawTextSprite(_textSprite);
            _renderer.End();
        }

        _textSprite.OutlineTint = GorgonColors.White;
        _textSprite.Color = _glowIndex != _fontIndex ? GorgonColors.White : GorgonColors.Black;
        _textSprite.DrawMode = ((_glowIndex == _fontIndex) || (!currentFont.HasOutline)) ? TextDrawMode.GlyphsOnly : TextDrawMode.OutlinedGlyphs;

        // Draw the font identification.
        _renderer.Begin();
        _renderer.DrawString($"Now displaying [c #FFFFE03F]'{currentFont.Name}'[/c]...", new Vector2(4.0f, 64.0f));
        _renderer.DrawTextSprite(_textSprite);
        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        // Animate our glow alpha.
        _glowAlpha += _glowVelocity * GorgonTiming.Delta;

        if (_glowAlpha > 1.0f)
        {
            _glowAlpha = 1.0f;
            _glowVelocity = -0.5f;
        }
        else if (_glowAlpha < 0.25f)
        {
            _glowAlpha = 0.25f;
            _glowVelocity = 0.5f;
        }

        // Animate the line height so we can drop the lines and make them bounce... just because we can.
        float normalSin = (_bounceAngle.ToRadians().Sin() + 1.0f) / 2.0f;
        float scaledSin = normalSin * (1.0f - _max);
        _textSprite.LineSpace = scaledSin + _max;

        _bounceAngle += _angleSpeed * GorgonTiming.Delta;

        if (_bounceAngle > 90.0f)
        {
            _bounceAngle = 90.0f;
            if (_max.EqualsEpsilon(0))
            {
                _max = 0.5f;
            }
            else
            {
                _max += 0.125f;
            }

            if (_max >= 1.0f)
            {
                _max = 1.0f;
            }
            _angleSpeed = -_angleSpeed * (_max + 1.0f);
        }

        if (_bounceAngle < -90.0f)
        {
            _angleSpeed = -_angleSpeed;
            _bounceAngle = -90.0f;
        }

        int timeDiff = (int)(GorgonTiming.SecondsSinceStart - _startTime).FastCeiling();

        // Switch to a new font every 4 seconds.
        if (timeDiff > 4)
        {
            _startTime = GorgonTiming.SecondsSinceStart;
            ++_fontIndex;
            _textSprite.LineSpace = -0.015f;

            // Reset glow animation.
            if (_fontIndex == _glowIndex)
            {
                _glowAlpha = 1.0f;
                _glowVelocity = -0.5f;
            }

            // Reset bounce.
            _bounceAngle = -90.0f;
            _max = 0.0f;
            _angleSpeed = 360.0f;
        }

        if (_fontIndex >= _font.Count)
        {
            _fontIndex = 0;
        }

        _screen.Present(1);

        return true;
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    /// <returns>The main window for the application.</returns>
    private static FormMain Initialize()
    {
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

        // Use a callback so we can use async.
        static async void OnLoad(object sender, EventArgs e)
        {
            FormMain form = (FormMain)sender;

            // Create our fonts.
            try
            {
                IReadOnlyList<IGorgonVideoAdapterInfo> videoDevices = await Task.Run(() => GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log));

                if (videoDevices.Count == 0)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                                "Gorgon requires at least a Direct3D 11.2 capable video device.\nThere is no suitable device installed on the system.");
                }

                // Find the best video device.
                _graphics = new GorgonGraphics(videoDevices.OrderByDescending(item => item.FeatureSet).First());

                _screen = new GorgonSwapChain(_graphics,
                                                form,
                                                new GorgonSwapChainInfo(ExampleConfig.Default.Resolution.X,
                                                                             ExampleConfig.Default.Resolution.Y,
                                                                             BufferFormat.R8G8B8A8_UNorm)
                                                {
                                                    Name = "Gorgon2D Effects Example Swap Chain"
                                                });

                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                // Load our logo.
                GorgonExample.LoadResources(_graphics);

                IReadOnlyList<FontFamily> fonts = await LoadTrueTypeFontsAsync(form);
                await GenerateGorgonFontsAsync(fonts, form);

                // Build our text sprite.
                _textSprite = new GorgonTextSprite(GorgonExample.Fonts.DefaultFont, _text)
                {
                    LineSpace = 0
                };

                GorgonApplication.IdleMethod = Idle;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(form, ex);
                GorgonApplication.Quit();
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        return GorgonExample.Initialize(new GorgonPoint(ExampleConfig.Default.Resolution.X, ExampleConfig.Default.Resolution.Y), "Fonts", OnLoad);
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GorgonApplication.Run(Initialize());
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            GorgonExample.UnloadResources();

            _renderer?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
        }
    }
}
