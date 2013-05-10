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
// Created: Sunday, May 20, 2012 10:06:05 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.IO;
using SlimMath;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Font interface for Gorgon.
	/// </summary>
	public class GorgonFonts
	{
		#region Variables.
		private readonly GorgonGraphics _graphics;			// Graphics interface.
		private GorgonFont _default;						// Default font for debugging, etc...
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default font.
		/// </summary>
		public GorgonFont DefaultFont
		{
			get
			{
				if (_default == null)
				{
					// Create the default font.
					_default = new GorgonFont(_graphics, "Gorgon.Default.Font", new GorgonFontSettings()
					{
						AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
						TextContrast = 2,
						Characters = Enumerable.Range(32, 127).
							Select(i => Convert.ToChar(i)).
							Where(c => !char.IsControl(c)),
						DefaultCharacter = ' ',
						FontFamilyName = "Tahoma",
						FontHeightMode = FontHeightMode.Pixels,
						FontStyle = FontStyle.Bold,
						OutlineSize = 0,
						Size = 14,
						TextureSize = new Size(128, 128)
					});

					_default.Update(_default.Settings);
				}

				return _default;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up any resources.
		/// </summary>
		internal void CleanUp()
		{
		    if (_default != null)
		    {
		        _default.Dispose();
		    }

		    _default = null;
		}

		/// <summary>
		/// Function to retrieve an existing font (if it was not generated interally) texture for a texture.
		/// </summary>
		/// <param name="textureName">Name of the texture to look up.</param>
		/// <returns>The texture if found, NULL if not.</returns>
		private GorgonTexture2D GetFontTexture(string textureName)
		{
			GorgonTexture2D result = null;

			// Only look at textures that weren't created by Gorgon internally and aren't font textures.
			if ((!textureName.StartsWith("GorgonFont.", StringComparison.CurrentCultureIgnoreCase))
				|| (textureName.IndexOf(".InternalTexture_", StringComparison.CurrentCultureIgnoreCase) == -1))
			{
				result = (from texture in _graphics.GetGraphicsObjectOfType<GorgonTexture2D>()
						  where (string.Compare(texture.Name, textureName, true) == 0)
						  select texture).FirstOrDefault();
			}
						
			return result;
		}

		/// <summary>
		/// Function to load the font from a stream.
		/// </summary>
		/// <param name="fontName">Name of the font object.</param>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The font loaded from the stream.</returns>
		private GorgonFont LoadFont(string fontName, Stream stream)
		{
			var settings = new GorgonFontSettings();
			GorgonFont font = null;
			FileStream fileStream = null;
            float fontHeight = 0.0f;
            float fontAscent = 0.0f;
            float fontDescent = 0.0f;
            float fontLineHeight = 0.0f;

            // Output the font in chunked format.
            using (var chunk = new GorgonChunkReader(stream))
            {
                chunk.Begin(GorgonFont.FileHeader);

                // Write font information.
                chunk.Begin("FONTDATA");
                settings.FontFamilyName = chunk.ReadString();
                settings.Size = chunk.ReadFloat();
                settings.FontHeightMode = chunk.Read<FontHeightMode>();
                settings.FontStyle = chunk.Read<FontStyle>();
                settings.DefaultCharacter = chunk.ReadChar();
                settings.Characters = chunk.ReadString();
                fontHeight = chunk.ReadFloat();
                fontLineHeight = chunk.ReadFloat();
                fontAscent = chunk.ReadFloat();
                fontDescent = chunk.ReadFloat();
                chunk.End();

                // Write rendering information.
                chunk.Begin("RNDRDATA");
                settings.AntiAliasingMode = chunk.Read<FontAntiAliasMode>();
				settings.BaseColors.Clear();
                var colorCount = chunk.ReadInt32();

                for (int i = 0; i < colorCount; i++)
                {
                    settings.BaseColors.Add(chunk.Read<GorgonColor>());
                }

				if (settings.BaseColors.Count < 1)
				{
					settings.BaseColors.Add(GorgonColor.White);
				}
                settings.OutlineColor = chunk.Read<GorgonColor>();
                settings.OutlineSize = chunk.ReadInt32();
                settings.TextContrast = chunk.ReadInt32();
                chunk.End();

                // Create the font object.
                font = new GorgonFont(_graphics, fontName, settings);                

                // Write texture information.
                chunk.Begin("TXTRDATA");
                settings.PackingSpacing = chunk.ReadInt32();
                settings.TextureSize = chunk.ReadSize();
                int textureCount = chunk.ReadInt32();
                bool isExternal = false;

                if (chunk.HasChunk("TXTREXTL"))
                {
                    isExternal = true;
                    chunk.Begin("TXTREXTL");

					fileStream = stream as FileStream;
					if (fileStream == null)
					{
						throw new ArgumentException("Cannot load external textures for the font because the stream is not a file stream.", "stream");
					}
                }
                else
                {
                    chunk.Begin("TXTRINTL");
                }

                // Load in the textures.
                for (int i = 0; i < textureCount; i++)
                {
                    GorgonTexture2D texture = null;
                    string textureName = chunk.ReadString();

                    texture = GetFontTexture(textureName);

                    // Add to the font.
                    if (texture != null)
                    {
                        font.Textures.Add(texture);
                    }

                    if (!isExternal)
                    {
                        int textureSize = chunk.ReadInt32();

                        if (texture == null)
                        {
                            texture = _graphics.Textures.FromStream<GorgonTexture2D>(textureName, stream, textureSize, new GorgonCodecPNG());
                            // Don't track these textures.
                            _graphics.RemoveTrackedObject(texture);
                            font.Textures.AddBind(texture);
                        }
                        else
                        {
                            chunk.SkipBytes(textureSize);
                        }
                    }
                    else
                    {
                        // Get the path to the texture (must be local to the font file).
                        string texturePath = Path.GetDirectoryName(fileStream.Name).FormatDirectory(Path.DirectorySeparatorChar) + chunk.ReadString();

                        // If the texture exists, then don't bother loading it.
                        // Otherwise load it in.
                        if (texture == null)
                        {
                            texture = _graphics.Textures.FromFile<GorgonTexture2D>(textureName, texturePath, new GorgonCodecPNG());
                            _graphics.RemoveTrackedObject(texture);
                            font.Textures.AddBind(texture);
                        }
                    }
                }
                chunk.End();

                // Get glyph information.
                chunk.Begin("GLYFDATA");
                int groupCount = chunk.ReadInt32();

                for (int i = 0; i < groupCount; i++)
                {
                    string textureName = chunk.ReadString();
                    int glyphCount = chunk.ReadInt32();

                    for (int j = 0; j < glyphCount; j++)
                    {
                        var glyph = new GorgonGlyph(chunk.ReadChar(), font.Textures[textureName],
                            chunk.ReadRectangle(),
                            chunk.Read<Vector2>(),
                            chunk.Read<Vector3>());

                        font.Glyphs.Add(glyph);
                    }
                }
                chunk.End();

                // Read optional kerning information.
                if (chunk.HasChunk("KERNDATA"))
                {
                    chunk.Begin("KERNDATA");
                    int kernCount = chunk.ReadInt32();
                    for (int i = 0; i < kernCount; i++)
                    {
                        var pair = new GorgonKerningPair(chunk.ReadChar(), chunk.ReadChar());
                        font.KerningPairs.Add(pair, chunk.ReadInt32());
                    }
                    chunk.End();
                }
            }
            
			font.FontHeight = fontHeight;
			font.LineHeight = fontLineHeight;
			font.Ascent = fontAscent;
			font.Descent = fontDescent;

			font.HasChanged = false;

			_graphics.AddTrackedObject(font);

			return font;
		}

		/// <summary>
		/// Function to read a font from a stream.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The font in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="name"/> parameters are NULL.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the font cannot be read.</exception>
		public GorgonFont FromStream(string name, Stream stream)
		{
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.Length == 0)
            {
                throw new ArgumentException("The parameter must not be empty.", "stream");
            }
            
            return LoadFont(name, stream);
		}

        /// <summary>
        /// Function to read a font from memory.
        /// </summary>
        /// <param name="name">Name of the font object.</param>
        /// <param name="fontData">Byte array containing the font data.</param>
        /// <returns>The font in the array.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fontData"/> or the <paramref name="name"/> parameters are NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown if the font uses external textures.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the fontData array is empty.</para>
        /// </exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown if the font cannot be read.</exception>
        public GorgonFont FromMemory(string name, byte[] fontData)
        {
            using (var memoryStream = new GorgonDataStream(fontData))
            {
                return FromStream(name, memoryStream);
            }
        }

		/// <summary>
		/// Function to read a font from a file.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="fileName">Path and filename of the font to load.</param>
		/// <returns>The font in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> or the <paramref name="name"/> parameters are NULL.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the fileName or name parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the font cannot be read.</exception>
		public GorgonFont FromFile(string name, string fileName)
		{
			FileStream stream = null;

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The parameter must not be empty.", "fileName");
            }
            
            try
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				return LoadFont(name, stream);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to create a new font texture object from a GDI+ font.
		/// </summary>
		/// <param name="fontName">Name of the font texture object.</param>
		/// <param name="fontFamily">Font family to use.</param>
		/// <param name="pointSize">Point size for the font.</param>
		/// <param name="antiAliasMode">Anti-aliasing mode.</param>
		/// <param name="textureSize">Size of the textures to generate.</param>
		/// <returns>The new font texture object.</returns>
		/// <remarks>This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to 
		/// display text (or symbols) on the screen.  Kerning information (the proper spacing for a glyph) is included in the glyphs and font.
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <paramref name="fontFamily"/> parameter.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or fontFamily parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName or fontFamily parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		public GorgonFont CreateFont(string fontName, string fontFamily, float pointSize, FontAntiAliasMode antiAliasMode, Size textureSize)
		{
			return CreateFont(fontName, fontFamily, pointSize, FontStyle.Regular, antiAliasMode, textureSize);
		}

		/// <summary>
		/// Function to create a new font texture object from a GDI+ font.
		/// </summary>
		/// <param name="fontName">Name of the font texture object.</param>
		/// <param name="fontFamily">Font family to use.</param>
		/// <param name="pointSize">Point size for the font.</param>
		/// <param name="style">Style to apply to the font.</param>
		/// <param name="antiAliasMode">Anti-aliasing mode.</param>
		/// <param name="textureSize">Size of the textures to generate.</param>
		/// <returns>The new font texture object.</returns>
		/// <remarks>This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to 
		/// display text (or symbols) on the screen.  Kerning information (the proper spacing for a glyph) is included in the glyphs and font.
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <paramref name="fontFamily"/> parameter.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or fontFamily parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName or fontFamily parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		public GorgonFont CreateFont(string fontName, string fontFamily, float pointSize, FontStyle style, FontAntiAliasMode antiAliasMode, Size textureSize)
		{
			if (pointSize < 1e-6f)
				pointSize = 1e-6f;

			var settings = new GorgonFontSettings()
			{
				AntiAliasingMode = antiAliasMode,
				Brush = null,
				FontFamilyName = fontFamily,
				FontStyle = style,
				Size = pointSize,
				TextureSize = textureSize
			};

			return CreateFont(fontName, settings);
		}

		/// <summary>
		/// Function to create a new font texture object from a GDI+ font.
		/// </summary>
		/// <param name="fontName">Name of the font texture object.</param>
		/// <param name="font">GDI+ font to use.</param>
		/// <param name="antiAliasMode">Anti-aliasing mode.</param>
		/// <returns>The new font texture object.</returns>
		/// <remarks>This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to 
		/// display text (or symbols) on the screen.  Kerning information (the proper spacing for a glyph) is included in the glyphs and font.
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the font family name in the <paramref name="font"/> parameter.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or <paramref name="font"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.
		/// </exception>
		public GorgonFont CreateFont(string fontName, Font font, FontAntiAliasMode antiAliasMode)
		{
			return CreateFont(fontName, font, antiAliasMode, new Size(256, 256));
		}

		/// <summary>
		/// Function to create a new font texture object from a GDI+ font.
		/// </summary>
		/// <param name="fontName">Name of the font texture object.</param>
		/// <param name="font">GDI+ font to use.</param>
		/// <param name="antiAliasMode">Anti-aliasing mode.</param>
		/// <param name="textureSize">Size of the textures to generate.</param>
		/// <returns>The new font texture object.</returns>
		/// <remarks>This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to 
		/// display text (or symbols) on the screen.  Kerning information (the proper spacing for a glyph) is included in the glyphs and font.
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the font family name in the <paramref name="font"/> parameter.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or <paramref name="font"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		public GorgonFont CreateFont(string fontName, Font font, FontAntiAliasMode antiAliasMode, Size textureSize)
		{
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }

			var settings = new GorgonFontSettings()
			{
				AntiAliasingMode = antiAliasMode,
				Brush = null,
				FontFamilyName = font.FontFamily.Name,
				FontStyle = font.Style,
				Size = font.SizeInPoints,
				TextureSize = textureSize
			};

			return CreateFont(fontName, settings);
		}

		/// <summary>
		/// Function to create a new font texture object.
		/// </summary>
		/// <param name="fontName">Name of the font texture object.</param>
		/// <param name="settings">Settings for the font.</param>
		/// <returns>The new font texture object.</returns>
		/// <remarks>This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to 
		/// display text (or symbols) on the screen.  Kerning information (the proper spacing for a glyph) is included in the glyphs and font.
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.FontFamilyName">FontFamilyName</see> in the <paramref name="settings"/> parameter.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or settings parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.TextureSize">settings.TextureSize</see> width or height is larger than can be handled by the current feature level.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.DefaultCharacter">settings.DefaultCharacter</see> cannot be located in the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.Characters">settings.Characters</see> list.</para>
		/// </exception>
		public GorgonFont CreateFont(string fontName, GorgonFontSettings settings)
		{
            if (fontName == null)
            {
                throw new ArgumentNullException("fontName");
            }

            if (string.IsNullOrWhiteSpace("fontName"))
            {
                throw new ArgumentException("The parameter must not be empty.", "fontName");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (string.IsNullOrWhiteSpace(settings.FontFamilyName))
            {
                throw new ArgumentNullException("The font family name must not be NULL or empty.", "settings");
            }

			var result = new GorgonFont(_graphics, fontName, settings);

			result.Update(settings);

			_graphics.AddTrackedObject(result);

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFonts"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface.</param>
		internal GorgonFonts(GorgonGraphics graphics)
		{
			_graphics = graphics;
		}
		#endregion
	}
}
