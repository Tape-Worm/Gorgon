#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Friday, April 13, 2012 6:51:08 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts
{
    /// <summary>
    /// A font used to render text data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type contains all the necessary information used to render glyphs to represent characters on the screen. Complete kerning information is provided as well (if available on the original font object), 
    /// and can be customized by the user.
    /// </para>
    /// <para>
    /// The font also contains a customizable glyph collection that users can modify to provide custom glyph to character mapping (e.g. a special texture used for a single character).
    /// </para>
    /// </remarks>
    public sealed class GorgonFont
        : GorgonNamedObject, IDisposable
    {
        #region Variables.
        // A worker buffer for formatting the string.
        private readonly StringBuilder _workBuffer = new StringBuilder(256);
        // A list of internal textures created by the font generator.
        private readonly List<GorgonTexture2D> _internalTextures;
        // The information used to generate the font.
        private readonly GorgonFontInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the factory that registered this font.
        /// </summary>
        internal GorgonFontFactory Factory
        {
            get;
        }

        /// <summary>
        /// Property to return the number of textures used by this font.
        /// </summary>
        public int TextureCount => _internalTextures.Count;

        /// <summary>
        /// Property to return whether there is an outline for this font.
        /// </summary>
        public bool HasOutline => Info.OutlineSize > 0 && (Info.OutlineColor1.Alpha > 0 || Info.OutlineColor2.Alpha > 0);

        /// <summary>
        /// Property to return the list of kerning pairs associated with the font.
        /// </summary>
        /// <remarks>
        /// Applications may use this list to define custom kerning information when rendering.
        /// </remarks>
        public IDictionary<GorgonKerningPair, int> KerningPairs
        {
            get;
        }

        /// <summary>
        /// Property to return the information used to create this font.
        /// </summary>
        public IGorgonFontInfo Info => _info;

        /// <summary>
        /// Property to return the graphics interface used to create this font.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the glyphs for this font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A glyph is a graphical representation of a character.  For Gorgon, this means a glyph for a specific character will point to a region of texels on a texture.
        /// </para>
        /// <para>
        /// Note that the glyph for a character is not required to represent the exact character (for example, the character "A" could map to the "V" character on the texture).  This will allow mapping of symbols 
        /// to a character representation.
        /// </para>
        /// </remarks>
        public GorgonGlyphCollection Glyphs
        {
            get;
        }

        /// <summary>
        /// Property to return the ascent for the font, in pixels.
        /// </summary>
        public float Ascent
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the descent for the font, in pixels.
        /// </summary>
        public float Descent
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the line height, in pixels, for the font.
        /// </summary>
        public float LineHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the font height, in pixels.
        /// </summary>
        /// <remarks>
        /// This is not the same as line height, the line height is a combination of ascent, descent and internal/external leading space.
        /// </remarks>
        public float FontHeight
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to format a string for rendering using a <see cref="GorgonFont"/>.
        /// </summary>
        /// <param name="renderText">The text to render.</param>
        /// <param name="tabSpacing">[Optional] The number of spaces used to replace a tab control character.</param>
        /// <returns>The formatted text.</returns>
        /// <remarks>
        /// <para>
        /// This method will format the string so that all control characters such as carriage return, and tabs are converted into spaces. 
        /// </para>
        /// <para>
        /// If the <paramref name="tabSpacing"/> parameter is changed from its default of 4, then that will be the number of space substituted for the tab control character.
        /// </para>
        /// </remarks>
        public string FormatStringForRendering(string renderText, int tabSpacing = 4)
        {
            if (string.IsNullOrEmpty(renderText))
            {
                return string.Empty;
            }

            tabSpacing = tabSpacing.Max(1);
            _workBuffer.Length = 0;
            _workBuffer.Append(renderText);

            // Strip all carriage returns.
            _workBuffer.Replace("\r", string.Empty);

            // Convert tabs to spaces.
            _workBuffer.Replace("\t", new string(' ', tabSpacing));

            return _workBuffer.ToString();
        }

        /// <summary>
        /// Function to copy bitmap data to a texture.
        /// </summary>
        /// <param name="bitmap">Bitmap to copy.</param>
        /// <param name="image">Image to receive the data.</param>
        /// <param name="arrayIndex">The index in the bitmap array to copy from.</param>
        private unsafe void CopyBitmap(Bitmap bitmap, GorgonImage image, int arrayIndex)
        {
            BitmapData sourcePixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int* pixels = (int*)sourcePixels.Scan0.ToPointer();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    // We only need the width here, as our pointer will handle the stride by virtue of being an int.
                    int* offset = pixels + (y * bitmap.Width);

                    int destOffset = y * image.Buffers[0, arrayIndex].PitchInformation.RowPitch;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
                        // lowest, but in fact, it is lowest to highest.
                        // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
                        // (R at byte 0) to the highest byte (A at byte 3).
                        // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
                        var color = new GorgonColor(*offset);

                        if (Info.UsePremultipliedTextures)
                        {
                            // Convert to premultiplied.
                            color = new GorgonColor(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);
                        }

                        int* destBuffer = (int*)(Unsafe.AsPointer(ref image.Buffers[0, arrayIndex].Data[destOffset]));
                        *destBuffer = color.ToABGR();
                        offset++;
                        destOffset += image.FormatInfo.SizeInBytes;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(sourcePixels);
            }
        }

        /// <summary>
        /// Function to retrieve the built in kerning pairs and advancement information for the font.
        /// </summary>
        /// <param name="graphics">The GDI graphics interface.</param>
        /// <param name="font">The GDI font.</param>
        /// <param name="allowedCharacters">The list of characters available to the font.</param>
        private Dictionary<char, ABC> GetKerningInformation(System.Drawing.Graphics graphics, Font font, IList<char> allowedCharacters)
        {
            Dictionary<char, ABC> advancementInfo;
            KerningPairs.Clear();

            IntPtr prevGdiHandle = Win32API.SetActiveFont(graphics, font);

            try
            {
                advancementInfo = Win32API.GetCharABCWidths(allowedCharacters[0], allowedCharacters[allowedCharacters.Count - 1]);

                if (!Info.UseKerningPairs)
                {
                    return advancementInfo;
                }

                IList<KERNINGPAIR> kerningPairs = Win32API.GetKerningPairs();

                foreach (KERNINGPAIR pair in kerningPairs.Where(item => item.KernAmount != 0))
                {
                    var newPair = new GorgonKerningPair(Convert.ToChar(pair.First), Convert.ToChar(pair.Second));

                    if ((!allowedCharacters.Contains(newPair.LeftCharacter)) ||
                        (!allowedCharacters.Contains(newPair.RightCharacter)))
                    {
                        continue;
                    }

                    KerningPairs[newPair] = pair.KernAmount;
                }
            }
            finally
            {
                Win32API.RestoreActiveObject(prevGdiHandle);
            }

            return advancementInfo;
        }

        /// <summary>
        /// Function to retrieve the available characters for use when generating the font.
        /// </summary>
        /// <returns>A new list of available characters, sorted by character.</returns>
        private List<char> GetAvailableCharacters()
        {
            var result = (from character in Info.Characters
                          where (!char.IsControl(character))
                                && (Convert.ToInt32(character) >= 32)
                                && (!char.IsWhiteSpace(character))
                          orderby character
                          select character).ToList();

            // Ensure the default character is there.
            if (!result.Contains(Info.DefaultCharacter))
            {
                result.Insert(0, Info.DefaultCharacter);
            }

            return result;
        }

        /// <summary>
        /// Function to generate textures based on the packed bitmaps generated for the glyphs.
        /// </summary>
        /// <param name="glyphData">The glyph data, grouped by packed bitmap.</param>
        /// <returns>A dictionary of characters associated with a texture.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void GenerateTextures(Dictionary<Bitmap, IEnumerable<GlyphInfo>> glyphData)
        {
            var imageSettings = new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
            {
                Width = Info.TextureWidth,
                Height = Info.TextureHeight,
                Depth = 1
            };
            var textureSettings = new GorgonTexture2DInfo
            {
                Format = BufferFormat.R8G8B8A8_UNorm,
                Width = Info.TextureWidth,
                Height = Info.TextureHeight,
                Usage = ResourceUsage.Default,
                Binding = TextureBinding.ShaderResource,
                IsCubeMap = false,
                MipLevels = 1,
                MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling
            };

            GorgonImage image = null;
            GorgonTexture2D texture = null;
            int arrayIndex = 0;
            int bitmapCount = glyphData.Count;

            try
            {
                // We copy each bitmap into a texture array index until we've hit the max texture array size, and then 
                // we move to a new texture.  This will keep our glyph textures inside of a single texture object until 
                // it is absolutely necessary to change and should improve performance when rendering.
                foreach (KeyValuePair<Bitmap, IEnumerable<GlyphInfo>> glyphBitmap in glyphData)
                {
                    if ((image == null) || (arrayIndex >= Graphics.VideoAdapter.MaxTextureArrayCount))
                    {
                        textureSettings.ArrayCount = imageSettings.ArrayCount = bitmapCount.Min(Graphics.VideoAdapter.MaxTextureArrayCount);
                        arrayIndex = 0;

                        image?.Dispose();
                        image = new GorgonImage(imageSettings);

                        texture = image.ToTexture2D(Graphics, new GorgonTexture2DLoadOptions
                        {
                            Name = $"GorgonFont_{Name}_Internal_Texture_{Guid.NewGuid():N}"
                        });
                        _internalTextures.Add(texture);
                    }

                    CopyBitmap(glyphBitmap.Key, image, arrayIndex);

                    // Send to our texture.
                    texture.SetData(image.Buffers[0, arrayIndex], destArrayIndex: arrayIndex);

                    foreach (GlyphInfo info in glyphBitmap.Value)
                    {
                        info.Texture = texture;
                        info.TextureArrayIndex = arrayIndex;
                    }

                    bitmapCount--;
                    arrayIndex++;
                }
            }
            finally
            {
                image?.Dispose();
            }
        }

        /// <summary>
        /// Function to build the list of glyphs for the font.
        /// </summary>
        /// <param name="glyphData">The glyph data used to create the glyphs.</param>
        /// <param name="kerningData">The kerning information used to handle spacing adjustment between glyphs.</param>
        private void GenerateGlyphs(Dictionary<char, GlyphInfo> glyphData, Dictionary<char, ABC> kerningData)
        {
            foreach (KeyValuePair<char, GlyphInfo> glyph in glyphData)
            {
                int advance = 0;

                if (kerningData.TryGetValue(glyph.Key, out ABC kernData))
                {
                    advance = kernData.A + (int)kernData.B + kernData.C;
                }

                // For whitespace, we add a dummy glyph (no texture, offset, etc...).
                if (char.IsWhiteSpace(glyph.Key))
                {
                    Glyphs.Add(new GorgonGlyph(glyph.Key, glyph.Value.Region.Width)
                    {
                        Offset = DX.Point.Zero
                    });
                    continue;
                }

                var newGlyph = new GorgonGlyph(glyph.Key, advance)
                {
                    Offset = glyph.Value.Offset,
                    OutlineOffset = HasOutline ? glyph.Value.OutlineOffset : DX.Point.Zero
                };

                // Assign the texture to each glyph (and its outline if needed).
                newGlyph.UpdateTexture(glyph.Value.Texture, glyph.Value.Region, HasOutline ? glyph.Value.OutlineRegion : DX.Rectangle.Empty, glyph.Value.TextureArrayIndex);

                Glyphs.Add(newGlyph);
            }
        }


        /// <summary>
        /// Function to measure the width of an individual line of text.
        /// </summary>
        /// <param name="line">The line to measure.</param>
        /// <param name="useOutline"><b>true</b> to use the font outline, <b>false</b> to disregard it.</param>
        /// <returns>The width of the line.</returns>
        private float GetLineWidth(string line, bool useOutline)
        {
            float size = 0;
            bool firstChar = true;

            if (!Glyphs.TryGetValue(Info.DefaultCharacter, out GorgonGlyph defaultGlyph))
            {
                throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Info.DefaultCharacter));
            }

            for (int i = 0; i < line.Length; i++)
            {
                char character = line[i];

                if (!Glyphs.TryGetValue(character, out GorgonGlyph glyph))
                {
                    glyph = defaultGlyph;
                }

                // Skip out on carriage returns and newlines.
                if ((character == '\r')
                    || (character == '\n'))
                {
                    continue;
                }

                // Whitespace will use the glyph width.
                if (char.IsWhiteSpace(character))
                {
                    size += glyph.Advance;
                    continue;
                }

                // Include the initial offset.
                if (firstChar)
                {
                    size += (useOutline && glyph.OutlineCoordinates.Width > 0) ? glyph.OutlineOffset.X : glyph.Offset.X;
                    firstChar = false;
                }

                size += glyph.Advance;

                if (!Info.UseKerningPairs)
                {
                    continue;
                }

                if ((i == line.Length - 1)
                    || (KerningPairs.Count == 0))
                {
                    continue;
                }

                var kerning = new GorgonKerningPair(character, line[i + 1]);

                if (KerningPairs.TryGetValue(kerning, out int kernAmount))
                {
                    size += kernAmount;
                }
            }

            return size;
        }

        /// <summary>
        /// Function to retrieve the glyph used for the default character assigned in the font <see cref="Info"/>.
        /// </summary>
        /// <returns>The <see cref="GorgonGlyph"/> representing the default character.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no glyph could be located that matches the default character.</exception>
        /// <remarks>
        /// <para>
        /// The default character is assigned to the <see cref="IGorgonFontInfo.DefaultCharacter"/> property of the <see cref="IGorgonFontInfo"/> type passed to the constructor of the font.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonFontInfo"/>
        public GorgonGlyph GetDefaultGlyph() => !TryGetDefaultGlyph(out GorgonGlyph glyph)
                ? throw new KeyNotFoundException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Info.DefaultCharacter))
                : glyph;

        /// <summary>
        /// Function to retrieve the glyph used for the default character assigned in the font <see cref="Info"/>.
        /// </summary>
        /// <param name="glyph">The default glyph, or <b>null</b> if not found.</param>
        /// <returns><b>true</b> if the glyph was found, or <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// The default character is assigned to the <see cref="IGorgonFontInfo.DefaultCharacter"/> property of the <see cref="IGorgonFontInfo"/> type passed to the constructor of the font.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonFontInfo"/>
        /// <seealso cref="GorgonGlyph"/>
        public bool TryGetDefaultGlyph(out GorgonGlyph glyph) => Glyphs.TryGetValue(Info.DefaultCharacter, out glyph);

        /// <summary>
        /// Function to perform word wrapping on a string based on this font.
        /// </summary>
        /// <param name="text">The text to word wrap.</param>
        /// <param name="wordWrapWidth">The maximum width, in pixels, that must be met for word wrapping to occur.</param>
        /// <returns>The string with word wrapping.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="wordWrapWidth"/> is the maximum number of pixels required for word wrapping, if an individual font glyph cell width (the <see cref="GorgonGlyph.Offset"/> + 
        /// <see cref="GorgonGlyph.Advance"/>) exceeds that of the <paramref name="wordWrapWidth"/>, then the parameter value is updated to glyph cell width.
        /// </para>
        /// </remarks>
        public string WordWrap(string text, float wordWrapWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var wordText = new StringBuilder(text);

            if (!Glyphs.TryGetValue(Info.DefaultCharacter, out GorgonGlyph defaultGlyph))
            {
                throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Info.DefaultCharacter));
            }

            int maxLength = wordText.Length;
            int index = 0;
            float position = 0.0f;
            bool firstChar = true;

            while (index < maxLength)
            {
                char character = wordText[index];

                // Don't count newline or carriage return.
                if ((character == '\n')
                    || (character == '\r'))
                {
                    firstChar = true;
                    position = 0;
                    ++index;
                    continue;
                }


                if (!Glyphs.TryGetValue(character, out GorgonGlyph glyph))
                {
                    glyph = defaultGlyph;
                }

                float glyphCellWidth = glyph.Advance;

                if (firstChar)
                {
                    glyphCellWidth += glyph.Offset.X;
                    firstChar = false;
                }

                // If we're using kerning, then adjust for the kerning value.
                if ((Info.UseKerningPairs)
                    && (index < maxLength - 1))
                {

                    if (KerningPairs.TryGetValue(new GorgonKerningPair(character, wordText[index + 1]), out int kernValue))
                    {
                        glyphCellWidth += kernValue;
                    }
                }

                position += glyphCellWidth;

                // Update the word wrap boundary if the cell size exceeds it.
                if (glyphCellWidth > wordWrapWidth)
                {
                    wordWrapWidth = glyphCellWidth;
                }

                // We're not at the break yet.
                if (position < wordWrapWidth)
                {
                    ++index;
                    continue;
                }

                int whiteSpaceIndex = index;

                // If we hit the max width, then we need to find the previous whitespace and inject a newline.
                while ((whiteSpaceIndex <= index) && (whiteSpaceIndex >= 0))
                {
                    char breakChar = wordText[whiteSpaceIndex];

                    if ((char.IsWhiteSpace(breakChar))
                        && (breakChar != '\n')
                        && (breakChar != '\r'))
                    {
                        index = whiteSpaceIndex;
                        break;
                    }

                    --whiteSpaceIndex;
                }

                // If we're at the beginning, then we cannot wrap this text, so we'll break it at the border specified.
                if (index != whiteSpaceIndex)
                {
                    if (index != 0)
                    {
                        wordText.Insert(index, '\n');
                        maxLength = wordText.Length;
                        ++index;
                    }
                    position = 0;
                    firstChar = true;
                    // Move to next character.
                    ++index;
                    continue;
                }

                // Extract the space.
                wordText[whiteSpaceIndex] = '\n';
                position = 0;
                firstChar = true;
                index = whiteSpaceIndex + 1;
            }

            return wordText.ToString();
        }

        /// <summary>
        /// Function to measure a single line of text using this font.
        /// </summary>
        /// <param name="text">The single line of text to measure.</param>
        /// <param name="useOutline"><b>true</b> to include the outline in the measurement, <b>false</b> to exclude.</param>
        /// <param name="lineSpacing">[Optional] The factor used to determine the amount of space between each line.</param>
        /// <returns>A vector containing the width and height of the text line when rendered using this font.</returns>
        /// <remarks>
        /// <para>
        /// This will measure the specified <paramref name="text"/> and return the size, in pixels, of the region containing the text. Unlike the <see cref="MeasureText"/> method, this method does not format 
        /// the text or take into account newline/carriage returns. It is meant for a single line of text only.
        /// </para>
        /// <para>
        /// If the <paramref name="useOutline"/> parameter is <b>true</b>, then the outline size is taken into account when measuring, otherwise only the standard glyph size is taken into account. If the font 
        /// <see cref="HasOutline"/> property is <b>false</b>, then this parameter is ignored.
        /// </para>
        /// <para>
        /// The <paramref name="lineSpacing"/> parameter adjusts the amount of space between each line by multiplying it with the <see cref="FontHeight"/> value (and the <see cref="IGorgonFontInfo.OutlineSize"/> * 2 
        /// if <paramref name="useOutline"/> is <b>true</b> and <see cref="HasOutline"/> is <b>true</b>). For example, to achieve a double spacing effect, change this value to 2.0f.
        /// </para>
        /// </remarks>
        /// <seealso cref="MeasureText"/>
        public DX.Size2F MeasureLine(string text, bool useOutline, float lineSpacing = 1.0f)
        {
            if (string.IsNullOrEmpty(text))
            {
                return DX.Size2F.Zero;
            }

            float lineWidth = GetLineWidth(text, useOutline && HasOutline);

            if ((HasOutline) && (useOutline))
            {
                lineWidth += Info.OutlineSize;
            }

            return new DX.Size2F(lineWidth, FontHeight.FastFloor() * lineSpacing);
        }

        /// <summary>
        /// Function to measure the specified text using this font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="useOutline"><b>true</b> to include the outline in the measurement, <b>false</b> to exclude.</param>
        /// <param name="tabSpaceCount">[Optional] The number of spaces represented by a tab control character.</param>
        /// <param name="lineSpacing">[Optional] The factor used to determine the amount of space between each line.</param>
        /// <param name="wordWrapWidth">[Optional] The maximum width to return if word wrapping is required.</param>
        /// <returns>A vector containing the width and height of the text when rendered using this font.</returns>
        /// <remarks>
        /// <para>
        /// This will measure the specified <paramref name="text"/> and return the size, in pixels, of the region containing the text.
        /// </para>
        /// <para>
        /// If the <paramref name="wordWrapWidth"/> is specified and greater than zero, then word wrapping is assumed to be on and the text will be handled using word wrapping.
        /// </para>
        /// <para>
        /// If the <paramref name="useOutline"/> parameter is <b>true</b>, then the outline size is taken into account when measuring, otherwise only the standard glyph size is taken into account. If the font 
        /// <see cref="HasOutline"/> property is <b>false</b>, then this parameter is ignored.
        /// </para>
        /// <para>
        /// The <paramref name="lineSpacing"/> parameter adjusts the amount of space between each line by multiplying it with the <see cref="FontHeight"/> value (and the <see cref="IGorgonFontInfo.OutlineSize"/> * 2 
        /// if <paramref name="useOutline"/> is <b>true</b> and <see cref="HasOutline"/> is <b>true</b>). For example, to achieve a double spacing effect, change this value to 2.0f.
        /// </para>
        /// <para>
        /// If measuring a single line of text with no breaks (i.e. newline or carriage return), and no word wrapping, then call the <see cref="MeasureLine"/> method instead for better performance.
        /// </para>
        /// </remarks>
        /// <seealso cref="MeasureLine"/>
        public DX.Size2F MeasureText(string text, bool useOutline, int tabSpaceCount = 4, float lineSpacing = 1.0f, float? wordWrapWidth = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return DX.Size2F.Zero;
            }

            string formattedText = FormatStringForRendering(text, tabSpaceCount);
            DX.Size2F result = DX.Size2F.Zero;

            if (wordWrapWidth != null)
            {
                formattedText = WordWrap(text, wordWrapWidth.Value);
            }

            string[] lines = formattedText.GetLines();
            float fontHeight = FontHeight.FastFloor();

            if (lines.Length == 0)
            {
                return result;
            }

            if (lineSpacing.EqualsEpsilon(1.0f))
            {
                result.Height = lines.Length * fontHeight;
            }
            else
            {
                // For a modified line spacing, we have to adjust for the last line not being affected by the line spacing.
                result.Height = ((lines.Length - 1) * (((fontHeight) * lineSpacing))) + (fontHeight);
            }

            if ((HasOutline) && (useOutline))
            {
                result.Height += Info.OutlineSize * 0.5f;
            }

            // Get width.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < lines.Length; ++i)
            {
                float lineWidth = GetLineWidth(lines[i], useOutline && HasOutline);

                if ((HasOutline) && (useOutline))
                {
                    lineWidth += Info.OutlineSize;
                }

                result.Width = result.Width.Max(lineWidth);
            }

            return result;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Get rid of this font from its factory cache when its disposed. Otherwise it'll live on in a half initialized state.
            Factory?.UnregisterFont(this);

            foreach (GorgonTexture2D texture in _internalTextures)
            {
                texture?.Dispose();
            }

            var brush = _info.Brush as IDisposable;
            brush?.Dispose();
            _internalTextures.Clear();
            KerningPairs.Clear();
            Glyphs.Clear();
        }

        /// <summary>
        /// Function to create or update the font.
        /// </summary>
        /// <param name="externalFontCollections">The external font collections from the application.</param>
        /// <remarks>
        /// <para>
        /// This is used to generate a new set of font textures, and essentially "create" the font object.
        /// </para>
        /// <para>
        /// This method will clear all the glyphs and textures in the font and rebuild the font with the specified parameters. This means that any custom glyphs, texture mapping, and/or kerning will be lost. 
        /// Users must find a way to remember and restore any custom font info when updating.
        /// </para>
        /// <para>
        /// Internal textures used by the glyph will be destroyed.  However, if there's a user defined texture or glyph using a user defined texture, then it will not be destroyed and clean up will be the 
        /// responsibility of the user.
        /// </para>
        /// </remarks>
        /// <exception cref="GorgonException">Thrown when the texture size in the settings exceeds that of the capabilities of the feature level.
        /// <para>-or-</para>
        /// <para>Thrown when the font family name is <b>null</b> or Empty.</para>
        /// </exception>
        internal void GenerateFont(IEnumerable<System.Drawing.Text.PrivateFontCollection> externalFontCollections)
        {
            Bitmap setupBitmap = null;
            System.Drawing.Graphics graphics = null;
            GdiFontData fontData = default;
            Dictionary<Bitmap, IEnumerable<GlyphInfo>> groupedByBitmap = null;

            try
            {
                // Temporary bitmap used to gather a graphics context.				
                setupBitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb);

                // Get a context for the rasterizing surface.
                graphics = System.Drawing.Graphics.FromImage(setupBitmap);
                graphics.PageUnit = GraphicsUnit.Pixel;

                // Build up the information using a GDI+ font.
                fontData = GdiFontData.GetFontData(graphics, Info, externalFontCollections);

                // Remove control characters and anything below a space.
                List<char> availableCharacters = GetAvailableCharacters();

                // Set up the code to draw glyphs to bitmaps.
                var glyphDraw = new GlyphDraw(Info, fontData);

                // Gather the boundaries for each glyph character.
                Dictionary<char, GlyphRegions> glyphBounds = glyphDraw.GetGlyphRegions(availableCharacters, HasOutline);

                // Because the dictionary above remaps characters (if they don't have a glyph or their rects are empty),
                // we'll need to drop these from our main list of characters.
                availableCharacters.RemoveAll(item => !glyphBounds.ContainsKey(item));

                // Get kerning and glyph advancement information.
                Dictionary<char, ABC> abcAdvances = GetKerningInformation(graphics, fontData.Font, availableCharacters);

                // Put the glyphs on packed bitmaps.
                Dictionary<char, GlyphInfo> glyphBitmaps = glyphDraw.DrawToPackedBitmaps(availableCharacters, glyphBounds, HasOutline);

                groupedByBitmap = (from glyphBitmap in glyphBitmaps
                                   where glyphBitmap.Value.GlyphBitmap != null
                                   group glyphBitmap.Value by glyphBitmap.Value.GlyphBitmap).ToDictionary(k => k.Key, v => v.Select(item => item));

                // Generate textures from the bitmaps. 
                // We will pack each bitmap into a single arrayed texture up to the maximum number of array indices allowed.
                // Once that limit is reached a new texture will be used. This should help performance a little, although it 
                // is much better to resize the texture so that it has a single array index and single texture.
                GenerateTextures(groupedByBitmap);

                // Finally, generate our glyphs.
                GenerateGlyphs(glyphBitmaps, abcAdvances);

                FontHeight = fontData.FontHeight;
                Ascent = fontData.Ascent;
                Descent = fontData.Descent;
                LineHeight = fontData.LineHeight;
            }
            finally
            {
                graphics?.Dispose();
                setupBitmap?.Dispose();

                fontData?.Dispose();

                if (groupedByBitmap != null)
                {
                    foreach (Bitmap glyphBitmap in groupedByBitmap.Keys)
                    {
                        glyphBitmap.Dispose();
                    }
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFont"/> class.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="factory">The factory that this font is registered with.</param>
        /// <param name="info">The information used to create the font.</param>
        /// <param name="fontHeight">The height of the font, in pixels.</param>
        /// <param name="lineHeight">The height of a line, in pixels.</param>
        /// <param name="ascent">The font ascent, in pixels.</param>
        /// <param name="descent">The font descent, in pixels.</param>
        /// <param name="glyphs">The glyphs for the font.</param>
        /// <param name="textures">The textures for the font.</param>
        /// <param name="kerningPairs">The kerning pairs for the font.</param>
        internal GorgonFont(string name,
                            GorgonFontFactory factory,
                            IGorgonFontInfo info,
                            float fontHeight,
                            float lineHeight,
                            float ascent,
                            float descent,
                            IReadOnlyList<GorgonGlyph> glyphs,
                            IReadOnlyList<GorgonTexture2D> textures,
                            IReadOnlyDictionary<GorgonKerningPair, int> kerningPairs)
            : base(name)
        {
            Factory = factory;
            _info = new GorgonFontInfo(info)
            {
                Brush = info.Brush.Clone()
            };
            Graphics = Factory.Graphics;
            FontHeight = fontHeight;
            LineHeight = lineHeight;
            Ascent = ascent;
            Descent = descent;
            Glyphs = new GorgonGlyphCollection(glyphs);
            KerningPairs = kerningPairs?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<GorgonKerningPair, int>();
            _internalTextures = new List<GorgonTexture2D>(textures);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFont"/> class.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="factory">The factory that created this font.</param>
        /// <param name="info">The information used to generate the font.</param>
        internal GorgonFont(string name, GorgonFontFactory factory, IGorgonFontInfo info)
            : base(name)
        {
            Factory = factory;
            Graphics = Factory.Graphics;
            _info = new GorgonFontInfo(info)
            {
                Brush = info.Brush?.Clone()
            };
            _internalTextures = new List<GorgonTexture2D>();
            Glyphs = new GorgonGlyphCollection();
            KerningPairs = new Dictionary<GorgonKerningPair, int>();
        }
        #endregion
    }
}