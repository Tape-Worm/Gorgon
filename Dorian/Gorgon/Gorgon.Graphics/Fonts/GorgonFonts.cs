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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using SlimMath;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Font interface for Gorgon.
	/// </summary>
	public class GorgonFonts
	{
		#region Variables.
		private byte[] _fileHeader = null;					// File header.
		private GorgonGraphics _graphics = null;			// Graphics interface.
		private GorgonFont _default = null;					// Default font for debugging, etc...
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
				_default.Dispose();
			_default = null;
		}

		/// <summary>
		/// Function to load the font from a stream.
		/// </summary>
		/// <param name="fontName">Name of the font object.</param>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The font loaded from the stream.</returns>
		private GorgonFont LoadFont(string fontName, Stream stream)
		{
			GorgonFontSettings settings = null;
			GorgonFont font = null;

			using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
			{
				// Read in our header.
				reader.Read(_fileHeader, 0, _fileHeader.Length);
				if (string.Compare(Encoding.UTF8.GetString(_fileHeader, 0, _fileHeader.Length), GorgonFont.FileHeader) != 0)
				{
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read this font.  It is not a Gorgon font or is a newer version.");
				}
			}

			using (GorgonChunkedFormat chunk = new GorgonChunkedFormat())
			{
				GorgonDataStream data = null;
				float fontHeight = 0.0f;
				float fontAscent = 0.0f;
				float fontDescent = 0.0f;
				float fontLineHeight = 0.0f;

				// Load in our chunked font data.
				chunk.Load(stream);

				settings = new GorgonFontSettings();

				// Get the font information.
				if (chunk.HasChunk("FontInfo"))
				{
					data = chunk["FontInfo"];
					byte[] familyName = data.ReadRange<byte>(data.ReadInt32());
					settings.FontFamilyName = Encoding.UTF8.GetString(familyName, 0, familyName.Length);
					settings.Size = data.ReadFloat();
					settings.FontHeightMode = data.Read<FontHeightMode>();
					settings.FontStyle = data.Read<FontStyle>();
					settings.DefaultCharacter = Convert.ToChar(data.ReadInt32());
					settings.Characters = data.ReadRange<int>(data.ReadInt32()).Select(item => Convert.ToChar(item));
					fontHeight = data.ReadFloat();
					fontLineHeight = data.ReadFloat();
					fontAscent = data.ReadFloat();
					fontDescent = data.ReadFloat();
				}
				else
				{
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read this font.  It is either corrupted or not a Gorgon font.");
				}

				// Get rendering information.
				if (chunk.HasChunk("RenderInfo"))
				{
					data = chunk["RenderInfo"];
					settings.AntiAliasingMode = data.Read<FontAntiAliasMode>();
					settings.BaseColors = data.ReadRange<GorgonColor>(data.ReadInt32());
					settings.OutlineColor = data.Read<GorgonColor>();
					settings.OutlineSize = data.ReadInt32();
					settings.TextContrast = data.ReadInt32();
				}

				// Get texture data.
				int textureCount = 0;
				if (chunk.HasChunk("Textures"))
				{
					data = chunk["Textures"];
					settings.PackingSpacing = data.ReadInt32();
					settings.TextureSize = new Size(data.ReadInt32(), data.ReadInt32());
					textureCount = data.ReadInt32();

					// Create our font before loading textures.
					font = new GorgonFont(_graphics, fontName, settings);

					// Load the textures.
					for (int i = 0; i < textureCount; i++)
					{
						GorgonTexture2D texture = _graphics.Textures.FromStream<GorgonTexture2D>(data.ReadString(), data, data.ReadInt32());
						font.Textures.Add(texture);
						_graphics.RemoveTrackedObject(texture);
					}

				}
				else if (chunk.HasChunk("ExternalTextures"))
				{
					FileStream fileStream = stream as FileStream;
					if (fileStream == null)
					{
						throw new ArgumentException("The stream is not a file stream.  External textures cannot be read.", "stream");
					}

					data = chunk["ExternalTextures"];
					settings.PackingSpacing = data.ReadInt32();
					settings.TextureSize = new Size(data.ReadInt32(), data.ReadInt32());
					textureCount = data.ReadInt32();

					// Create our font before loading textures.
					font = new GorgonFont(_graphics, fontName, settings);

					for (int i = 0; i < textureCount; i++)
					{
						string textureName = data.ReadString();
						string texturePath = Path.GetDirectoryName(fileStream.Name).FormatDirectory(Path.DirectorySeparatorChar) + data.ReadString();
						GorgonTexture2D texture = _graphics.Textures.FromFile<GorgonTexture2D>(textureName, texturePath);
						font.Textures.Add(texture);
						_graphics.RemoveTrackedObject(texture);
					}
				}
				else
				{
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read this font.  It is either corrupted or not a Gorgon font.");
				}

				// Get our glyph data.
				if (chunk.HasChunk("GlyphData"))
				{
					data = chunk["GlyphData"];

					int groupCount = data.ReadInt32();

					for (int i = 0; i < groupCount; i++)
					{
						string textureName = data.ReadString();
						int glyphCount = data.ReadInt32();

						for (int j = 0; j < glyphCount; j++)
						{
							GorgonGlyph glyph = new GorgonGlyph(Convert.ToChar(data.ReadInt32()), font.Textures[textureName],
								new Rectangle(data.ReadInt32(), data.ReadInt32(), data.ReadInt32(), data.ReadInt32()),
								data.Read<Vector2>(),
								data.Read<Vector3>());

							font.Glyphs.Add(glyph);
						}
					}
				}
				else
				{
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read this font.  It is either corrupted or not a Gorgon font.");
				}

				// Read kerning information.
				if (chunk.HasChunk("KerningPairs"))
				{
					data = chunk["KerningPairs"];

					int kernCount = data.ReadInt32();
					for (int i = 0; i < kernCount; i++)
					{
						GorgonKerningPair pair = new GorgonKerningPair(Convert.ToChar(data.ReadInt32()), Convert.ToChar(data.ReadInt32()));
						font.KerningPairs.Add(pair, data.ReadInt32());
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
			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull<Stream>(stream, "stream");
			return LoadFont(name, stream);
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

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertParamString(fileName, "fileName");

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
			GorgonDebug.AssertParamString(fontName, "fontName");
			GorgonDebug.AssertParamString(fontFamily, "fontFamily");

			if (pointSize < 1e-6f)
				pointSize = 1e-6f;

			GorgonFontSettings settings = new GorgonFontSettings()
			{
				AntiAliasingMode = antiAliasMode,
				BaseColors = new GorgonColor[] { Color.White },
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
			GorgonDebug.AssertParamString(fontName, "fontName");
			GorgonDebug.AssertNull<Font>(font, "font");

			GorgonFontSettings settings = new GorgonFontSettings()
			{
				AntiAliasingMode = antiAliasMode,
				BaseColors = null,
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
			GorgonDebug.AssertParamString(fontName, "fontName");
			GorgonDebug.AssertNull<GorgonFontSettings>(settings, "settings");
			GorgonFont result = new GorgonFont(_graphics, fontName, settings);

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
			_fileHeader = Encoding.UTF8.GetBytes(GorgonFont.FileHeader);
		}
		#endregion
	}
}
