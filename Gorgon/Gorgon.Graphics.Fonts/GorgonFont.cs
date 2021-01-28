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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
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
        : GorgonNamedObject, IGorgonFontInfo, IDisposable
    {
        #region Variables.
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
        /// Property to return whether there is an outline for this font.
        /// </summary>
        public bool HasOutline => _info.OutlineSize > 0 && (_info.OutlineColor1.Alpha > 0 || _info.OutlineColor2.Alpha > 0);

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

        /// <summary>Property to return whether the font height is in pixels or in points.</summary>
        /// <remarks>
        ///   <para>
        /// When the font uses points for its height, the user must be aware of DPI scaling issues that may arise.
        /// </para>
        ///   <para>
        /// This will affect the <see cref="Size" /> value in that it will alter the meaning of the units.
        /// </para>
        ///   <para>
        /// The default value is <see cref="FontHeightMode.Pixels" />.
        /// </para>
        /// </remarks>
        public FontHeightMode FontHeightMode => _info.FontHeightMode;

        /// <summary>Property to return the font family name to generate the font from.</summary>
        public string FontFamilyName => _info.FontFamilyName;

        /// <summary>Property to return the font size.</summary>
        /// <remarks>
        ///   <para>
        /// This sets the height of the font.
        /// </para>
        ///   <para>
        /// This is affected by the <see cref="Fonts.FontHeightMode" />. If the <see cref="FontHeightMode" /> is set to <see cref="FontHeightMode.Points" />, then this unit is the height
        /// size height for the font. Otherwise, this represents the font height in <see cref="FontHeightMode.Pixels" />.
        /// </para>
        /// </remarks>
        public float Size => _info.Size;

        /// <summary>Property to return the width of the texture(s) used as the backing store for the bitmap font data.</summary>
        /// <remarks>
        ///   <para>
        /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of
        /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
        /// </para>
        ///   <para>
        /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="FeatureSet" /> for the <see cref="IGorgonVideoAdapterInfo" />.
        /// </para>
        ///   <para>
        /// The default width is 256.
        /// </para>
        /// </remarks>
        public int TextureWidth => _info.TextureWidth;

        /// <summary>Property to return the height of the texture(s) used as the backing store for the bitmap font data.</summary>
        /// <remarks>
        ///   <para>
        /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of
        /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
        /// </para>
        ///   <para>
        /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="FeatureSet" /> for the <see cref="IGorgonVideoAdapterInfo" />.
        /// </para>
        ///   <para>
        /// The default height is 256.
        /// </para>
        /// </remarks>
        public int TextureHeight => _info.TextureHeight;

        /// <summary>Property to return the list of available characters to use as glyphs within the font.</summary>
        /// <remarks>
        ///   <para>
        /// This will define what characters the font can display when rendering.  Any character not defined in this property will use the default character (typically a space).
        /// </para>
        ///   <para>
        /// The default encompasses characters from ASCII character code 32 to 255.
        /// </para>
        /// </remarks>
        public IEnumerable<char> Characters => _info.Characters;

        /// <summary>Property to return the anti-aliasing mode for the font.</summary>
        /// <remarks>
        ///   <para>
        /// This defines the smoothness of font pixel edges.
        /// </para>
        ///   <para>
        ///     <note type="important">
        ///       <para>
        /// Gorgon does not support clear type at this time.
        /// </para>
        ///     </note>
        ///   </para>
        ///   <para>
        /// The default value is <see cref="FontAntiAliasMode.AntiAlias" />.
        /// </para>
        /// </remarks>
        public FontAntiAliasMode AntiAliasingMode => _info.AntiAliasingMode;

        /// <summary>Property to return the size of an outline.</summary>
        /// <remarks>
        ///   <para>
        /// This defines the thickness of an outline (in pixels) for each glyph in the font.  A value greater than 0 will draw an outline, 0 or less will turn outlining off.
        /// </para>
        ///   <para>
        /// The default value is 0.
        /// </para>
        /// </remarks>
        public int OutlineSize => _info.OutlineSize;

        /// <summary>Property to return the starting color of the outline.</summary>
        /// <remarks>
        ///   <para>
        /// This defines the starting color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
        /// </para>
        ///   <para>
        /// If the alpha channel is set to 0.0f and the <see cref="OutlineColor2" /> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored
        /// if the <see cref="OutlineSize" /> value is not greater than 0.
        /// </para>
        ///   <para>
        /// The default value is <see cref="GorgonColor.Transparent" /> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
        /// </para>
        /// </remarks>
        public GorgonColor OutlineColor1 => _info.OutlineColor1;

        /// <summary>Property to return the ending color of the outline.</summary>
        /// <remarks>
        ///   <para>
        /// This defines the ending color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
        /// </para>
        ///   <para>
        /// If the alpha channel is set to 0.0f and the <see cref="OutlineColor1" /> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored
        /// if the <see cref="OutlineSize" /> value is not greater than 3.
        /// </para>
        ///   <para>
        /// The default value is <see cref="GorgonColor.Transparent" /> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
        /// </para>
        /// </remarks>
        public GorgonColor OutlineColor2 => _info.OutlineColor2;

        /// <summary>Property to return whether premultiplied textures are used when generating the glyphs for the font.</summary>
        /// <remarks>
        ///   <para>
        /// This defines whether the textures used to store the glyphs of the font will use premultiplied alpha. Premultiplied alpha is used to give a more accurate representation of blending between
        /// colors, and is accomplished by multiplying the RGB values by the Alpha component of a pixel.
        /// </para>
        ///   <para>
        /// If this value is <b>true</b>, then applications should use the <see cref="GorgonBlendState.Premultiplied" /> blending state when rendering text.
        /// </para>
        ///   <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool UsePremultipliedTextures => _info.UsePremultipliedTextures;

        /// <summary>Property to return a <see cref="GorgonGlyphBrush" /> to use for special effects on the glyphs for the font.</summary>
        /// <remarks>
        ///   <para>
        /// This value can be used to define how a glyph is rendered when the <see cref="T:Gorgon.Graphics.Fonts.GorgonFont" /> is generated. Applications can use brushes for gradients, textures, etc... to render the glyphs to give
        /// them a unique look.
        /// </para>
        ///   <para>
        /// This default value is <b>null</b>.
        /// </para>
        /// </remarks>
        public GorgonGlyphBrush Brush => _info.Brush;

        /// <summary>Property to return the style for the font.</summary>
        /// <remarks>The default value is <see cref="System.Drawing.FontStyle.Regular" />.</remarks>
        public FontStyle FontStyle => _info.FontStyle;

        /// <summary>Property to return a default character to use in place of a character that cannot be found in the font.</summary>
        /// <remarks>
        ///   <para>
        /// If some characters are unprintable (e.g. have no width/height), or they were not defined in the <see cref="Characters" /> property.  This character will be substituted in those cases.
        /// </para>
        ///   <para>
        /// The default value is a space (' ').
        /// </para>
        /// </remarks>
        public char DefaultCharacter => _info.DefaultCharacter;

        /// <summary>Property to return the spacing (in pixels) used between font glyphs on the backing texture.</summary>
        /// <remarks>
        ///   <para>
        /// This defines how much space to put between glyphs. Higher values will waste more space (and thus lead to more textures being created), but may resolve rendering issues.
        /// </para>
        ///   <para>
        /// The valid values are between 0 and 8.
        /// </para>
        ///   <para>
        /// The default value is 1 pixel.
        /// </para>
        /// </remarks>
        public int PackingSpacing => _info.PackingSpacing;

        /// <summary>Property to return whether to include kerning pair information in the font.</summary>
        /// <remarks>
        ///   <para>
        /// Kerning pairs are used to define spacing between 2 characters in a font.  When this value is set to <b>true</b> the kerning information of the font is retrieved and added to the <see cref="T:Gorgon.Graphics.Fonts.GorgonFont" />
        /// for use when rendering. This can be used to resolve rendering issues regarding horizontal font spacing.
        /// </para>
        ///   <para>
        ///     <note type="note">
        ///       <para>
        /// Some fonts do not employ the use of kerning pairs, and consequently, this setting will be ignored if that is the case.
        /// </para>
        ///     </note>
        ///   </para>
        ///   <para>
        /// The default value is <b>true</b>.
        /// </para>
        /// </remarks>
        public bool UseKerningPairs => _info.UseKerningPairs;

        /// <summary>Property to return the type of compression to apply to the font textures.</summary>
        /// <remarks>
        ///   <para>
        /// Use compression to lower the amount of video RAM consumed by the font textures. It is recommended that <see cref="UsePremultipliedTextures" /> be set to <b>true</b> when using
        /// compression.
        /// </para>
        ///   <para>
        /// The default value is <see cref="FontTextureCompression.None" />.
        /// </para>
        /// </remarks>
        public FontTextureCompression Compression => _info.Compression;
        #endregion

        #region Methods.
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
                var pixels = new GorgonPtr<int>(sourcePixels.Scan0, bitmap.Width * bitmap.Height);
                GorgonPtr<int> destBuffer = image.Buffers[0, arrayIndex].Data.To<int>();

                // Both buffers should be the same size. If they are not, bad things happen.
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        ref readonly int srcColor = ref pixels[(y * bitmap.Width) + x];
                        ref int destColor = ref destBuffer[(y * bitmap.Width) + x];

                        // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
                        // lowest, but in fact, it is lowest to highest.
                        // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
                        // (R at byte 0) to the highest byte (A at byte 3).
                        // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
                        var color = new GorgonColor(srcColor);

                        if (_info.UsePremultipliedTextures)
                        {
                            // Convert to premultiplied.
                            color = new GorgonColor(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);
                        }

                        destColor = color.ToABGR();
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

                if (!_info.UseKerningPairs)
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
            var result = (from character in _info.Characters
                          where (!char.IsControl(character))
                                && (Convert.ToInt32(character) >= 32)
                                && (!char.IsWhiteSpace(character))
                          orderby character
                          select character).ToList();

            // Ensure the default character is there.
            if (!result.Contains(_info.DefaultCharacter))
            {
                result.Insert(0, _info.DefaultCharacter);
            }

            return result;
        }

        /// <summary>
        /// Function to generate textures based on the packed bitmaps generated for the glyphs.
        /// </summary>
        /// <param name="glyphData">The glyph data, grouped by packed bitmap.</param>
        /// <returns>A list of images to upload to the GPU.</returns>
        private IReadOnlyList<(IGorgonImage image, IEnumerable<GlyphInfo> glyphs)> GenerateImages(Dictionary<Bitmap, IEnumerable<GlyphInfo>> glyphData)
        {
            var result = new List<(IGorgonImage, IEnumerable<GlyphInfo>)>();

            var imageSettings = new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
            {
                Width = _info.TextureWidth,
                Height = _info.TextureHeight,
                Depth = 1
            };

            GorgonImage image = null;
            int arrayIndex = 0;
            int bitmapCount = glyphData.Count;
            var glyphs = new List<GlyphInfo>();

            // We copy each bitmap into a texture array index until we've hit the max texture array size, and then 
            // we move to a new texture.  This will keep our glyph textures inside of a single texture object until 
            // it is absolutely necessary to change and should improve performance when rendering.
            foreach (KeyValuePair<Bitmap, IEnumerable<GlyphInfo>> glyphBitmap in glyphData)
            {
                if ((image == null) || (arrayIndex >= Graphics.VideoAdapter.MaxTextureArrayCount))
                {
                    imageSettings.ArrayCount = bitmapCount.Min(Graphics.VideoAdapter.MaxTextureArrayCount);
                    arrayIndex = 0;

                    glyphs = new List<GlyphInfo>();
                    image = new GorgonImage(imageSettings);
                    result.Add((image, glyphs));
                }

                CopyBitmap(glyphBitmap.Key, image, arrayIndex);

                foreach (GlyphInfo info in glyphBitmap.Value)
                {
                    glyphs.Add(info);
                    info.TextureArrayIndex = arrayIndex;
                }

                bitmapCount--;
                arrayIndex++;
            }

            return result;
        }

        /// <summary>
        /// Function to generate textures based on the packed bitmaps generated for the glyphs.
        /// </summary>
        /// <param name="images">The images containing the data to upload to the GPU.</param>
        private void GenerateTextures(IReadOnlyList<(IGorgonImage, IEnumerable<GlyphInfo>)> images)
        {
            if ((images == null) || (images.Count == 0))
            {
                return;
            }

            var textureSettings = new GorgonTexture2DInfo
            {
                Format = BufferFormat.R8G8B8A8_UNorm,
                Width = _info.TextureWidth,
                Height = _info.TextureHeight,
                Usage = ResourceUsage.Default,
                Binding = TextureBinding.ShaderResource,
                IsCubeMap = false,
                MipLevels = 1,
                MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling
            };

            foreach ((IGorgonImage image, IEnumerable<GlyphInfo> glyphs) in images)
            {
                textureSettings.ArrayCount = image.ArrayCount;

                switch (Compression)
                {
                    case FontTextureCompression.Fast:
                        image.BeginUpdate()
                             .Compress(BufferFormat.BC3_UNorm, quality: BcCompressionQuality.Quality)
                             .EndUpdate();
                        break;
                    case FontTextureCompression.Quality:
                        image.BeginUpdate()
                             .Compress(BufferFormat.BC7_UNorm, quality: BcCompressionQuality.Quality)
                             .EndUpdate();
                        break;
                }

                GorgonTexture2D texture = image.ToTexture2D(Graphics, new GorgonTexture2DLoadOptions
                {
                    Name = $"GorgonFont_{Name.Replace(" ", "_")}_Internal_Texture_{Guid.NewGuid():N}"
                });

                foreach (GlyphInfo glyph in glyphs)
                {
                    glyph.Texture = texture;
                }
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
        /// Function to generate font data.
        /// </summary>
        /// <param name="externalFontCollection">External fonts loaded in by the user.</param>
        /// <returns>A tuple containing the font information, advance information, and the bitmaps for the glyphs.</returns>
        private (GdiFontData fontData, Dictionary<char, ABC> abcAdvances, Dictionary<char, GlyphInfo> glyphBitmaps) GenerateFontData(PrivateFontCollection externalFontCollection)
        {
            Bitmap setupBitmap = null;
            System.Drawing.Graphics graphics = null;
            GdiFontData fontData = default;

            try
            {
                // Temporary bitmap used to gather a graphics context.
                setupBitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb);

                // Get a context for the rasterizing surface.
                graphics = System.Drawing.Graphics.FromImage(setupBitmap);
                graphics.PageUnit = GraphicsUnit.Pixel;

                // Build up the information using a GDI+ font.
                fontData = GdiFontData.GetFontData(graphics, _info, externalFontCollection);

                // Remove control characters and anything below a space.
                List<char> availableCharacters = GetAvailableCharacters();

                // Set up the code to draw glyphs to bitmaps.
                var glyphDraw = new GlyphDraw(_info, fontData);

                // Gather the boundaries for each glyph character.
                Dictionary<char, GlyphRegions> glyphBounds = glyphDraw.GetGlyphRegions(availableCharacters, HasOutline);

                // Because the dictionary above remaps characters (if they don't have a glyph or their rects are empty),
                // we'll need to drop these from our main list of characters.
                availableCharacters.RemoveAll(item => !glyphBounds.ContainsKey(item));

                // Get kerning and glyph advancement information.
                Dictionary<char, ABC> abc = GetKerningInformation(graphics, fontData.Font, availableCharacters);

                // Put the glyphs on packed bitmaps.
                Dictionary<char, GlyphInfo> glyphs = glyphDraw.DrawToPackedBitmaps(availableCharacters, glyphBounds, HasOutline);

                return (fontData, abc, glyphs);
            }
            finally
            {
                graphics?.Dispose();
                setupBitmap?.Dispose();
            }
        }

        /// <summary>
        /// Function to retrieve the glyph used for the default character assigned to the font <see cref="DefaultCharacter"/>.
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
                ? throw new KeyNotFoundException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, _info.DefaultCharacter))
                : glyph;

        /// <summary>
        /// Function to retrieve the glyph used for the default character assigned to the font <see cref="DefaultCharacter"/>.
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
        public bool TryGetDefaultGlyph(out GorgonGlyph glyph) => Glyphs.TryGetValue(_info.DefaultCharacter, out glyph);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Get rid of this font from its factory cache when its disposed. Otherwise it'll live on in an uninitialized state.
            Factory?.UnregisterFont(this);

            foreach (GorgonTexture2D glyphTexture in Glyphs.Select(item => item?.TextureView?.Texture)
                                                           .Where(item => item != null))
            {
                glyphTexture.Dispose();
            }

            var brush = _info.Brush as IDisposable;
            brush?.Dispose();

            KerningPairs.Clear();
            Glyphs.Clear();
        }

        /// <summary>
        /// Function to asynchronously create or update the font.
        /// </summary>
        /// <param name="externalFontCollection">The external font collections from the application.</param>
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
        internal async Task GenerateFontAsync(PrivateFontCollection externalFontCollection)
        {
            GdiFontData fontData = null;
            Dictionary<char, ABC> abcAdvances;
            Dictionary<char, GlyphInfo> glyphBitmaps;
            Dictionary<Bitmap, IEnumerable<GlyphInfo>> groupedByBitmap = null;
            IReadOnlyList<(IGorgonImage, IEnumerable<GlyphInfo>)> images = null;

            try
            {
                (GdiFontData, Dictionary<char, ABC>, Dictionary<char, GlyphInfo>) GetFontData() => GenerateFontData(externalFontCollection);
                IReadOnlyList<(IGorgonImage, IEnumerable<GlyphInfo>)> GetImages() => GenerateImages(groupedByBitmap);

                (fontData, abcAdvances, glyphBitmaps) = await Task.Run(GetFontData);

                groupedByBitmap = (from glyphBitmap in glyphBitmaps
                                  where glyphBitmap.Value.GlyphBitmap != null
                                  group glyphBitmap.Value by glyphBitmap.Value.GlyphBitmap).ToDictionary(k => k.Key, v => v.Select(item => item));

                // Generate textures from the bitmaps. 
                // We will pack each bitmap into a single arrayed texture up to the maximum number of array indices allowed.
                // Once that limit is reached a new texture will be used. This should help performance a little, although it 
                // is much better to resize the texture so that it has a single array index and single texture.
                images = await Task.Run(GetImages);
                // We do this on the calling thread because we can run into issues updating the textures from a separate thread.
                GenerateTextures(images);

                // Finally, generate our glyphs.
                GenerateGlyphs(glyphBitmaps, abcAdvances);

                FontHeight = fontData.FontHeight;
                Ascent = fontData.Ascent;
                Descent = fontData.Descent;
                LineHeight = fontData.LineHeight;
            }
            finally
            {
                fontData?.Dispose();

                if (images != null)
                {
                    foreach ((IGorgonImage image, _) in images)
                    {
                        image?.Dispose();
                    }
                }

                if (groupedByBitmap != null)
                {
                    foreach (Bitmap glyphBitmap in groupedByBitmap.Keys)
                    {
                        glyphBitmap.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Function to create or update the font.
        /// </summary>
        /// <param name="externalFontCollection">The external font collections from the application.</param>
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
        internal void GenerateFont(PrivateFontCollection externalFontCollection)
        {
            GdiFontData fontData = default;
            Dictionary<char, GlyphInfo> glyphBitmaps;
            Dictionary<char, ABC> abcAdvances;
            Dictionary<Bitmap, IEnumerable<GlyphInfo>> groupedByBitmap = null;
            IReadOnlyList<(IGorgonImage, IEnumerable<GlyphInfo>)> images = null;

            try
            {
                (fontData, abcAdvances, glyphBitmaps) = GenerateFontData(externalFontCollection);

                groupedByBitmap = (from glyphBitmap in glyphBitmaps
                                   where glyphBitmap.Value.GlyphBitmap != null
                                   group glyphBitmap.Value by glyphBitmap.Value.GlyphBitmap).ToDictionary(k => k.Key, v => v.Select(item => item));

                // Generate textures from the bitmaps. 
                // We will pack each bitmap into a single arrayed texture up to the maximum number of array indices allowed.
                // Once that limit is reached a new texture will be used. This should help performance a little, although it 
                // is much better to resize the texture so that it has a single array index and single texture.
                images = GenerateImages(groupedByBitmap);
                GenerateTextures(images);

                // Finally, generate our glyphs.
                GenerateGlyphs(glyphBitmaps, abcAdvances);

                FontHeight = fontData.FontHeight;
                Ascent = fontData.Ascent;
                Descent = fontData.Descent;
                LineHeight = fontData.LineHeight;
            }
            finally
            {
                fontData?.Dispose();

                if (images != null)
                {
                    foreach ((IGorgonImage image, _) in images)
                    {
                        image?.Dispose();
                    }
                }

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
            
            Glyphs = new GorgonGlyphCollection();
            KerningPairs = new Dictionary<GorgonKerningPair, int>();
        }
        #endregion
    }
}