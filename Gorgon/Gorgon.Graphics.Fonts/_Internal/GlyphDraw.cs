#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 15, 2017 9:22:49 PM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// Provides functionality to draw and measure glyph data.
/// </summary>
internal class GlyphDraw
{
    #region Variables.
    // Data used to generate the font glyphs.
    private readonly GdiFontData _fontData;
    // Information about the font to create.
    private readonly IGorgonFontInfo _fontInfo;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to determine if a bitmap is empty.
    /// </summary>
    /// <param name="pixels">Pixels to evaluate.</param>
    /// <param name="x">Horizontal position.</param>
    /// <returns><b>true</b> if empty, <b>false</b> if not.</returns>
    private static unsafe bool IsBitmapColumnEmpty(BitmapData pixels, int x)
    {
        int* pixel = (int*)pixels.Scan0.ToPointer() + x;

        for (int y = 0; y < pixels.Height; y++)
        {
            if (((*pixel >> 24) & 0xff) != 0)
            {
                return false;
            }

            pixel += pixels.Width;
        }

        return true;
    }

    /// <summary>
    /// Function to determine if a bitmap is empty.
    /// </summary>
    /// <param name="pixels">Pixels to evaluate.</param>
    /// <param name="y">Vertical position.</param>
    /// <returns><b>true</b> if empty, <b>false</b> if not.</returns>
    private static unsafe bool IsBitmapRowEmpty(BitmapData pixels, int y)
    {
        int* pixel = (int*)pixels.Scan0.ToPointer();

        pixel += y * pixels.Width;

        for (int x = 0; x < pixels.Width; x++)
        {
            if (((*pixel >> 24) & 0xff) != 0)
            {
                return false;
            }

            pixel++;
        }

        return true;
    }

    /// <summary>
    /// Function to retrieve a character range for the specified character.
    /// </summary>
    /// <param name="character">The character to evaluate.</param>
    /// <param name="graphics">The graphics context to use.</param>
    /// <returns>The region for the character range.</returns>
    private RectangleF? GetCharacterRange(ref char character, System.Drawing.Graphics graphics)
    {
        Region[] characterRanges = null;
        Region[] defaultCharacterRanges = null;

        try
        {
            // Try to get the character size.
            characterRanges = graphics.MeasureCharacterRanges(character.ToString(CultureInfo.CurrentCulture),
                                                              _fontData.Font,
                                                              new RectangleF(0, 0, _fontInfo.TextureWidth, _fontInfo.TextureHeight),
                                                              _fontData.StringFormat);

            defaultCharacterRanges = graphics.MeasureCharacterRanges(_fontInfo.DefaultCharacter.ToString(CultureInfo.CurrentCulture),
                                                                     _fontData.Font,
                                                                     new RectangleF(0,
                                                                                            0,
                                                                                            _fontInfo.TextureWidth,
                                                                                            _fontInfo.TextureHeight),
                                                                     _fontData.StringFormat);

            // If the character doesn't exist, then return an empty value.
            switch (characterRanges.Length)
            {
                case 0 when (defaultCharacterRanges.Length == 0):
                    return null;
                case 0 when (defaultCharacterRanges.Length > 0):
                    character = _fontInfo.DefaultCharacter;
                    characterRanges = defaultCharacterRanges;
                    break;
            }

            // If we didn't get a size, but we have a default, then use that.

            RectangleF result = characterRanges[0].GetBounds(graphics);

            if ((result.Width >= 0.1f) || (result.Height >= 0.1f))
            {
                return result;
            }

            character = _fontInfo.DefaultCharacter;
            characterRanges = defaultCharacterRanges;
            result = characterRanges[0].GetBounds(graphics);
            characterRanges[0].Dispose();
            characterRanges = null;

            return ((result.Width < 0.1f) || (result.Height < 0.1f))
                ? null
                : result;
        }
        finally
        {
            if (characterRanges is not null)
            {
                foreach (Region region in characterRanges)
                {
                    region.Dispose();
                }
            }

            if (defaultCharacterRanges is not null)
            {
                foreach (Region region in defaultCharacterRanges)
                {
                    region.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Function to create a new bounding rectangle for the specified glyph character.
    /// </summary>
    /// <param name="character">The character representing the glyph.</param>
    /// <param name="glyphBounds">The current glyph boundaries.</param>
    /// <param name="glyphBitmap">The bitmap that will contain the glyph.</param>
    /// <param name="glyphGraphics">The graphics context for the glyph bitmap.</param>
    /// <param name="drawOutline"><b>true</b> to draw the outline for measurement, or <b>false</b> to draw the actual glyph for measurement.</param>
    /// <returns>The bounding rectangle for the glyph.</returns>
    private DX.Rectangle CropGlyphRegion(char character, RectangleF glyphBounds, Bitmap glyphBitmap, System.Drawing.Graphics glyphGraphics, bool drawOutline)
    {
        BitmapData pixels = null;
        var cropTopLeft = new Point(0, 0);
        var cropRightBottom = new Point(glyphBitmap.Width - 1, glyphBitmap.Height - 1);

        try
        {
            // Draw the character.
            if (!drawOutline)
            {
                DrawGlyphCharacter(character, glyphBitmap, glyphGraphics, Brushes.White);
            }
            else
            {
                DrawGlyphCharacterOutline(character, glyphBitmap, glyphGraphics);
            }

            // We use unsafe mode to scan pixels, it's much faster.
            pixels = glyphBitmap.LockBits(new Rectangle(0, 0, glyphBitmap.Width, glyphBitmap.Height),
                                          ImageLockMode.ReadWrite,
                                          PixelFormat.Format32bppArgb);

            // Crop left and right.
            while ((cropTopLeft.X < cropRightBottom.X)
                   && (IsBitmapColumnEmpty(pixels, cropTopLeft.X)))
            {
                cropTopLeft.X++;
            }

            while ((cropRightBottom.X > cropTopLeft.X)
                   && (IsBitmapColumnEmpty(pixels, cropRightBottom.X)))
            {
                cropRightBottom.X--;
            }

            // Crop top and bottom.
            while ((cropTopLeft.Y < cropRightBottom.Y)
                   && (IsBitmapRowEmpty(pixels, cropTopLeft.Y)))
            {
                cropTopLeft.Y++;
            }

            while ((cropRightBottom.Y > cropTopLeft.Y)
                   && (IsBitmapRowEmpty(pixels, cropRightBottom.Y)))
            {
                cropRightBottom.Y--;
            }

            // If we have a 0 width/height rectangle, then reset to calculated size.
            if (cropTopLeft.X == cropRightBottom.X)
            {
                cropTopLeft.X = (int)glyphBounds.X;
                cropRightBottom.X = (int)glyphBounds.Width - 1;
            }

            // ReSharper disable InvertIf
            if (cropTopLeft.Y == cropRightBottom.Y)
            {
                cropTopLeft.Y = (int)glyphBounds.Y;
                cropRightBottom.Y = (int)glyphBounds.Height - 1;
            }
            // ReSharper restore InvertIf

            return new DX.Rectangle
            {
                Left = cropTopLeft.X,
                Top = cropTopLeft.Y,
                Right = cropRightBottom.X,
                Bottom = cropRightBottom.Y
            };
        }
        finally
        {
            if (pixels is not null)
            {
                glyphBitmap.UnlockBits(pixels);
            }
        }
    }

    /// <summary>
    /// Function to draw the glyph character onto the bitmap.
    /// </summary>
    /// <param name="character">Character to write.</param>
    /// <param name="glyphBitmap">The bitmap that will receive the glyph.</param>
    /// <param name="glyphGraphics">The graphics context for the glyph bitmap.</param>
    /// <param name="brush">The brush used to draw the glyph.</param>
    private void DrawGlyphCharacter(char character, Bitmap glyphBitmap, System.Drawing.Graphics glyphGraphics, Brush brush)
    {
        string charString = character.ToString(CultureInfo.CurrentCulture);

        glyphGraphics.Clear(Color.FromArgb(0));

        // Assign a region for the glyph brush.
        using (var glyphRenderer = new GraphicsPath())
        {
            glyphRenderer.AddString(charString,
                                        _fontData.Font.FontFamily,
                                        (int)_fontInfo.FontStyle,
                                        _fontData.Font.Size,
                                        new RectangleF(0, 0, glyphBitmap.Width, glyphBitmap.Height),
                                        _fontData.DrawFormat);


            glyphGraphics.FillPath(brush, glyphRenderer);
        }

        glyphGraphics.Flush();
    }

    /// <summary>
    /// Function to draw the glyph character outline onto the bitmap.
    /// </summary>
    /// <param name="character">Character to write.</param>
    /// <param name="glyphBitmap">The bitmap that will receive the glyph.</param>
    /// <param name="glyphGraphics">The graphics context for the glyph bitmap.</param>
    private void DrawGlyphCharacterOutline(char character, Bitmap glyphBitmap, System.Drawing.Graphics glyphGraphics)
    {
        string charString = character.ToString(CultureInfo.CurrentCulture);

        glyphGraphics.Clear(Color.FromArgb(0));

        using (var outlineRenderer = new GraphicsPath())
        {
            outlineRenderer.AddString(charString,
                                        _fontData.Font.FontFamily,
                                        (int)_fontInfo.FontStyle,
                                        _fontData.Font.Size,
                                        new RectangleF(0, 0, glyphBitmap.Width, glyphBitmap.Height),
                                        _fontData.DrawFormat);

            // If we want an outline, then draw that first.
            if ((_fontInfo.OutlineColor1 == _fontInfo.OutlineColor2)
                || (_fontInfo.OutlineSize < 3))
            {
                using var outlinePen = new Pen(_fontInfo.OutlineColor1, _fontInfo.OutlineSize * 2);
                outlinePen.LineJoin = LineJoin.Round;
                glyphGraphics.DrawPath(outlinePen, outlineRenderer);
            }
            else
            {
                GorgonColor start = _fontInfo.OutlineColor1;
                GorgonColor end = _fontInfo.OutlineColor2;

                // Fade from the first to the second color via a linear function.
                for (int i = _fontInfo.OutlineSize; i > 0; --i)
                {
                    float delta = ((float)(i - 1) / (_fontInfo.OutlineSize - 1));

                    GorgonColor.Lerp(in start, in end, delta, out GorgonColor penColor);

                    using var outlinePen = new Pen(penColor, i);
                    outlinePen.LineJoin = LineJoin.Round;
                    glyphGraphics.DrawPath(outlinePen, outlineRenderer);
                }
            }
        }

        glyphGraphics.Flush();
    }

    /// <summary>
    /// Function to return a glyph bitmap, sized to fit the glyph.
    /// </summary>
    /// <param name="oldBitmap">The previous bitmap used to store the glyph.</param>
    /// <param name="oldGraphics">The previous graphics context for the bitmap.</param>
    /// <param name="glyphSize">The size of the glyph.</param>
    private void GetGlyphBitmap(ref Bitmap oldBitmap, ref System.Drawing.Graphics oldGraphics, RectangleF glyphSize)
    {
        if ((oldBitmap is not null) && (glyphSize.Width * 2 <= oldBitmap.Width) && (glyphSize.Height <= oldBitmap.Height))
        {
            return;
        }

        oldBitmap?.Dispose();
        oldGraphics?.Dispose();
        oldBitmap = new Bitmap((int)glyphSize.Width * 2, (int)glyphSize.Height, PixelFormat.Format32bppArgb);

        oldGraphics = System.Drawing.Graphics.FromImage(oldBitmap);
        oldGraphics.PageUnit = GraphicsUnit.Pixel;
        oldGraphics.CompositingMode = CompositingMode.SourceOver;
        oldGraphics.CompositingQuality = CompositingQuality.HighQuality;

        switch (_fontInfo.AntiAliasingMode)
        {
            case FontAntiAliasMode.AntiAlias:
                oldGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                oldGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                break;
            default:
                oldGraphics.SmoothingMode = SmoothingMode.None;
                oldGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                break;
        }
    }

    /// <summary>
    /// Function to return the largest rectangle size for all the glyphs.
    /// </summary>
    /// <param name="glyphBounds">The list of glyph boundaries.</param>
    /// <param name="hasOutline"><b>true</b> if the font is using an outline, <b>false</b> if not.</param>
    /// <returns>The size of the largest glyph.</returns>
    private static DX.Size2 GetMaxGlyphSize(Dictionary<char, GlyphRegions> glyphBounds, bool hasOutline)
    {
        var result = new DX.Size2(int.MinValue, int.MinValue);
        foreach (KeyValuePair<char, GlyphRegions> glyphBound in glyphBounds)
        {
            result = !hasOutline
                         ? new DX.Size2(glyphBound.Value.CharacterRegion.Right.Max(result.Width), glyphBound.Value.CharacterRegion.Bottom.Max(result.Height))
                         : new DX.Size2(glyphBound.Value.OutlineRegion.Right.Max(result.Width), glyphBound.Value.OutlineRegion.Bottom.Max(result.Height));
        }

        return new DX.Size2(result.Width, result.Height);
    }

    /// <summary>
    /// Function to rasterize a series of glyphs to a packed bitmap.
    /// </summary>
    /// <param name="packedGraphics">The graphics context for the packed bitmap.</param>
    /// <param name="glyphGraphics">The graphics context for the glyph bitmap.</param>
    /// <param name="packedBitmap">The packed bitmap.</param>
    /// <param name="glyphBitmap">The glyph bitmap.</param>
    /// <param name="characters">The list of characters to use as glyphs.</param>
    /// <param name="glyphBounds">The list of boundaries for the glyphs.</param>
    /// <param name="packedGlyphs">The resulting set of packed glyphs.</param>
    /// <param name="brush">The brush used to draw the glyphs.</param>
    /// <param name="hasOutline"><b>true</b> if the font contains an outline, <b>false</b> if not.</param>
    private void RasterizeGlyphs(System.Drawing.Graphics packedGraphics,
                                System.Drawing.Graphics glyphGraphics,
                                Bitmap packedBitmap,
                                Bitmap glyphBitmap,
                                List<char> characters,
                                Dictionary<char, GlyphRegions> glyphBounds,
                                Dictionary<char, GlyphInfo> packedGlyphs,
                                GorgonGlyphBrush brush,
                                bool hasOutline)
    {
        int index = 0;
        int packingSpace = _fontInfo.PackingSpacing > 0 ? _fontInfo.PackingSpacing * 2 : 1;

        while (index < characters.Count)
        {
            char character = characters[index];

            // If we've already put this glyph in, then skip it.
            if (packedGlyphs.ContainsKey(character))
            {
                characters.Remove(character);
                continue;
            }

            DX.Rectangle charRect = glyphBounds[character].CharacterRegion;

            // Skip whitespace characters. These won't be drawn, just measured.
            if (char.IsWhiteSpace(character))
            {
                characters.Remove(character);
                packedGlyphs[character] = new GlyphInfo(null, new DX.Rectangle(0, 0, charRect.Width + 1, charRect.Height + 1), DX.Point.Zero, DX.Rectangle.Empty, DX.Point.Zero);
                continue;
            }

            DX.Rectangle outlineRect = glyphBounds[character].OutlineRegion;

            var size = new DX.Size2(charRect.Width + 1, charRect.Height + 1);

            Rectangle? placement = GlyphPacker.Add(new Size(charRect.Width + packingSpace, charRect.Height + packingSpace));
            Rectangle? outlinePlacement = null;

            if ((hasOutline) && (!outlineRect.IsEmpty))
            {
                outlinePlacement = GlyphPacker.Add(new Size(outlineRect.Width + packingSpace, outlineRect.Height + packingSpace));
            }

            if ((placement is null)
                || ((hasOutline) && (outlinePlacement is null) && (!outlineRect.IsEmpty)))
            {
                ++index;
                continue;
            }

            characters.Remove(character);

            Point location = placement.Value.Location;
            Point outlineLocation = outlinePlacement?.Location ?? Point.Empty;

            location.X += _fontInfo.PackingSpacing;
            location.Y += _fontInfo.PackingSpacing;

            // If we're using a linear gradient, then we have to define the bounds for that gradient.
            if (brush.BrushType == GlyphBrushType.LinearGradient)
            {
                ((GorgonGlyphLinearGradientBrush)brush).GradientRegion = new DX.Rectangle(charRect.Left, charRect.Top, charRect.Width, charRect.Height);
            }

            using Brush gdiBrush = brush.ToGDIBrush();
            // Draw the main character glyph.
            DrawGlyphCharacter(character, glyphBitmap, glyphGraphics, gdiBrush);

            packedGraphics.DrawImage(glyphBitmap,
                                     new Rectangle(location.X, location.Y, size.Width, size.Height),
                                     new Rectangle(charRect.X, charRect.Y, size.Width, size.Height),
                                     GraphicsUnit.Pixel);

            // Draw the outline for the character glyph.
            if ((hasOutline) && (!outlineRect.IsEmpty))
            {
                outlineLocation.X += _fontInfo.PackingSpacing;
                outlineLocation.Y += _fontInfo.PackingSpacing;
                DrawGlyphCharacterOutline(character, glyphBitmap, glyphGraphics);

                packedGraphics.DrawImage(glyphBitmap,
                                         new Rectangle(outlineLocation.X, outlineLocation.Y, outlineRect.Width + 1, outlineRect.Height + 1),
                                         new Rectangle(outlineRect.X, outlineRect.Y, outlineRect.Width + 1, outlineRect.Height + 1),
                                         GraphicsUnit.Pixel);
            }

            packedGlyphs[character] = new GlyphInfo(packedBitmap,
                                                    new DX.Rectangle(location.X, location.Y, size.Width, size.Height),
                                                    new DX.Point(charRect.X, charRect.Y),
                                                    new DX.Rectangle(outlineLocation.X, outlineLocation.Y, outlineRect.Width + 1, outlineRect.Height + 1),
                                                    new DX.Point(outlineRect.X, outlineRect.Y));
        }
    }

    /// <summary>
    /// Function to draw all the glyphs into a packed bitmap.
    /// </summary>
    /// <param name="characters">The characters to create glyphs from.</param>
    /// <param name="glyphBounds">The rectangular boundaries for the glyphs.</param>
    /// <param name="hasOutline"><b>true</b> if the font has an outline, <b>false</b> if not.</param>
    /// <returns>A list of bitmaps assigned to the characters.</returns>
    public Dictionary<char, GlyphInfo> DrawToPackedBitmaps(List<char> characters, Dictionary<char, GlyphRegions> glyphBounds, bool hasOutline)
    {
        var result = new Dictionary<char, GlyphInfo>();
        System.Drawing.Graphics packedGraphics = null;
        DX.Size2 maxGlyphSize = GetMaxGlyphSize(glyphBounds, hasOutline);
        var glyphBitmap = new Bitmap(maxGlyphSize.Width + 10, maxGlyphSize.Height + 10, PixelFormat.Format32bppArgb);
        var glyphGraphics = System.Drawing.Graphics.FromImage(glyphBitmap);
        GorgonGlyphBrush glyphBrush = _fontInfo.Brush ?? new GorgonGlyphSolidBrush
        {
            Color = GorgonColor.White
        };

        try
        {
            glyphGraphics.PageUnit = GraphicsUnit.Pixel;
            glyphGraphics.CompositingMode = CompositingMode.SourceOver;
            glyphGraphics.CompositingQuality = CompositingQuality.HighQuality;

            switch (_fontInfo.AntiAliasingMode)
            {
                case FontAntiAliasMode.AntiAlias:
                    glyphGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    glyphGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    break;
                default:
                    glyphGraphics.SmoothingMode = SmoothingMode.None;
                    glyphGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    break;
            }

            while (characters.Count > 0)
            {
                var packedBitmap = new Bitmap(_fontInfo.TextureWidth, _fontInfo.TextureHeight, PixelFormat.Format32bppArgb);
                packedGraphics?.Dispose();
                packedGraphics = System.Drawing.Graphics.FromImage(packedBitmap);

                packedGraphics.CompositingMode = CompositingMode.SourceCopy;
                packedGraphics.CompositingQuality = CompositingQuality.HighQuality;

                packedGraphics.Clear(Color.FromArgb(0));

                // Sort by size.
                characters.Sort((left, right) =>
                                {
                                    var leftSize = new DX.Size2(glyphBounds[left].CharacterRegion.Width, glyphBounds[left].CharacterRegion.Height);
                                    var rightSize = new DX.Size2(glyphBounds[right].CharacterRegion.Width, glyphBounds[right].CharacterRegion.Height);

                                    return leftSize.Height == rightSize.Height ? left.CompareTo(right) : leftSize.Height < rightSize.Height ? 1 : -1;
                                });

                GlyphPacker.CreateRoot(_fontInfo.TextureWidth - _fontInfo.PackingSpacing, _fontInfo.TextureHeight - _fontInfo.PackingSpacing);

                RasterizeGlyphs(packedGraphics, glyphGraphics, packedBitmap, glyphBitmap, characters, glyphBounds, result, glyphBrush, hasOutline);

                packedGraphics.Flush();
            }
        }
        finally
        {
            packedGraphics?.Dispose();
            glyphGraphics.Dispose();
            glyphBitmap.Dispose();
        }

        return result;
    }

    /// <summary>
    /// Function to retrieve the rectangular region occupied by each glyph.
    /// </summary>
    /// <param name="characters">The list of characters mapped by glyphs.</param>
    /// <param name="hasOutline"><b>true</b> if the font uses an outline, <b>false</b> if not.</param>
    /// <returns>A list of glyph regions.</returns>
    public Dictionary<char, GlyphRegions> GetGlyphRegions(List<char> characters, bool hasOutline)
    {
        var result = new Dictionary<char, GlyphRegions>();
        System.Drawing.Graphics glyphBitmapGraphics = null;
        Bitmap glyphBitmap = null;
        var tempBitmap = new Bitmap(_fontInfo.TextureWidth, _fontInfo.TextureHeight, PixelFormat.Format32bppArgb);
        var tempBitmapGraphics = System.Drawing.Graphics.FromImage(tempBitmap);

        tempBitmapGraphics.PageUnit = GraphicsUnit.Pixel;

        try
        {
            foreach (char availableChar in characters)
            {
                char updatedChar = availableChar;

                RectangleF? hasCharacterRange = GetCharacterRange(ref updatedChar, tempBitmapGraphics);

                if (hasCharacterRange is null)
                {
                    continue;
                }

                RectangleF characterRange = hasCharacterRange.Value;

                // Increase size to ensure we have some padding.
                if ((!char.IsWhiteSpace(updatedChar)) && (hasOutline))
                {
                    characterRange.Width += (_fontInfo.OutlineSize * 3);
                    characterRange.Height += (_fontInfo.OutlineSize * 3);
                }
                else
                {
                    characterRange.Width += 1;
                    characterRange.Height += 1;
                }

                // Ensure that the bitmap that will receive the glyph is large enough.
                GetGlyphBitmap(ref glyphBitmap, ref glyphBitmapGraphics, characterRange);

                DX.Rectangle glyphRectangle = CropGlyphRegion(updatedChar, characterRange, glyphBitmap, glyphBitmapGraphics, false);

                if (glyphRectangle.IsEmpty)
                {
                    continue;
                }

                DX.Rectangle outlineRect = DX.Rectangle.Empty;

                // Now measure the outline as a separate image so we can draw it under the character.
                if ((hasOutline) && (!char.IsWhiteSpace(updatedChar)))
                {
                    outlineRect = CropGlyphRegion(updatedChar, characterRange, glyphBitmap, glyphBitmapGraphics, true);
                }

                result[updatedChar] = new GlyphRegions
                {
                    CharacterRegion = glyphRectangle,
                    OutlineRegion = outlineRect
                };
            }
        }
        finally
        {
            tempBitmapGraphics.Dispose();
            tempBitmap.Dispose();
            glyphBitmapGraphics?.Dispose();
            glyphBitmap?.Dispose();
        }

        return result;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GlyphDraw"/> class.
    /// </summary>
    /// <param name="fontInfo">The font information used to create the font.</param>
    /// <param name="fontData">The font data for the glyphs.</param>
    public GlyphDraw(IGorgonFontInfo fontInfo, GdiFontData fontData)
    {
        _fontInfo = fontInfo;
        _fontData = fontData;
    }
    #endregion
}
