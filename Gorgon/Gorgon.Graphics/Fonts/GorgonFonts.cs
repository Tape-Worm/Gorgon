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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.IO;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Font interface for Gorgon.
	/// </summary>
	public class GorgonFonts
	{
		#region Variables.
		private readonly GorgonGraphics _graphics;			        // Graphics interface.
		private GorgonFont _default;						        // Default font for debugging, etc...
	    private static readonly object _syncLock = new object();    // Thread synchronization.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default font.
		/// </summary>
		public GorgonFont DefaultFont
		{
			get
			{
			    lock(_syncLock)
			    {
                    // If we're on a deferred context, then return the default font from the immediate context.
                    if (_graphics.IsDeferred)
                    {
                        return _graphics.ImmediateContext.Fonts.DefaultFont;
                    }

			        if (_default != null)
			        {
			            return _default;
			        }

			        // Create the default font.
			        _default = new GorgonFont(_graphics,
			            "Gorgon.Default.Font",
			            new GorgonFontSettings
			            {
			                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
			                Characters = Enumerable.Range(32, 127).
			                    Select(Convert.ToChar).
			                    Where(c => !char.IsControl(c)),
			                DefaultCharacter = ' ',
			                FontFamilyName = "Tahoma",
			                FontHeightMode = FontHeightMode.Pixels,
			                FontStyle = FontStyle.Bold,
			                OutlineSize = 0,
			                Size = 14,
			                TextureSize = new Size(128, 128)
			            });

			        _default.GenerateFont(_default.Settings);
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
		/// Function to read a font from a stream.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="stream">Stream to read from.</param>
		/// <param name="missingTextureFunction">[Optional] A function that is called if a required user defined texture has not been loaded for the font.</param>
		/// <returns>The font in the stream.</returns>
		/// <remarks>
		/// The fonts will not store user defined glyph textures, in order to facilitate the loading of the textures the users must either load the texture 
		/// before loading the font or specify a callback method in the <paramref name="missingTextureFunction"/> parameter.  This function will pass a name 
		/// for the texture and will expect a texture to be returned.  If NULL is returned, then an exception will be raised.
		/// <para>Fonts may only be created on the immediate context.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="name"/> parameters are NULL.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
		public GorgonFont FromStream(string name, Stream stream, Func<string, Size, GorgonTexture2D> missingTextureFunction = null)
		{
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.Length == 0)
            {
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "stream");
            }

			var font = new GorgonFont(_graphics, name, new GorgonFontSettings());

			using (var chunk = new GorgonChunkReader(stream))
			{
				chunk.Begin(GorgonFont.FileHeader);
				font.ReadFont(chunk, missingTextureFunction);
			}

			_graphics.AddTrackedObject(font);

            return font;
		}

        /// <summary>
        /// Function to read a font from memory.
        /// </summary>
        /// <param name="name">Name of the font object.</param>
        /// <param name="fontData">Byte array containing the font data.</param>
		/// <param name="missingTextureFunction">[Optional] A function that is called if a required user defined texture has not been loaded for the font.</param>
        /// <returns>The font in the array.</returns>
		/// <remarks>
		/// The fonts will not store user defined glyph textures, in order to facilitate the loading of the textures the users must either load the texture 
		/// before loading the font or specify a callback method in the <paramref name="missingTextureFunction"/> parameter.  This function will pass a name 
		/// for the texture and will expect a texture to be returned.  If NULL is returned, then an exception will be raised.
		/// <para>Fonts may only be created on the immediate context.</para></remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fontData"/> or the <paramref name="name"/> parameters are NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown if the font uses external textures.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the fontData array is empty.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
        public GorgonFont FromMemory(string name, byte[] fontData, Func<string, Size, GorgonTexture2D> missingTextureFunction = null)
        {
            using (var memoryStream = new GorgonDataStream(fontData))
            {
                return FromStream(name, memoryStream, missingTextureFunction);
            }
        }

		/// <summary>
		/// Function to read a font from a file.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="fileName">Path and filename of the font to load.</param>
		/// <param name="missingTextureFunction">[Optional] A function that is called if a required user defined texture has not been loaded for the font.</param>
		/// <remarks>
		/// The fonts will not store user defined glyph textures, in order to facilitate the loading of the textures the users must either load the texture 
		/// before loading the font or specify a callback method in the <paramref name="missingTextureFunction"/> parameter.  This function will pass a name 
		/// for the texture and will expect a texture to be returned.  If NULL is returned, then an exception will be raised.
		/// <para>Fonts may only be created on the immediate context.</para></remarks>
		/// <returns>The font in the file.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> or the <paramref name="name"/> parameters are NULL.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the fileName or name parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
		public GorgonFont FromFile(string name, string fileName, Func<string, Size, GorgonTexture2D> missingTextureFunction = null)
		{
			FileStream stream = null;

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "fileName");
            }
            
            try
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				return FromStream(name, stream, missingTextureFunction);
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
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
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <paramref name="fontFamily"/> parameter.</para>
		/// <para>Fonts may only be created on the immediate context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or fontFamily parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName or fontFamily parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown if the graphics context is deferred.</exception>
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
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <paramref name="fontFamily"/> parameter.</para>
		/// <para>Fonts may only be created on the immediate context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or fontFamily parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName or fontFamily parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown if the graphics context is deferred.</exception>
		public GorgonFont CreateFont(string fontName, string fontFamily, float pointSize, FontStyle style, FontAntiAliasMode antiAliasMode, Size textureSize)
		{
			if (pointSize < 1e-6f)
			{
				pointSize = 1e-6f;
			}

			var settings = new GorgonFontSettings
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
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the font family name in the <paramref name="font"/> parameter.</para>
		/// <para>Fonts may only be created on the immediate context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or <paramref name="font"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.</exception>
		/// <exception cref="GorgonException">Thrown if the graphics context is deferred.</exception>
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
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the font family name in the <paramref name="font"/> parameter.</para>
		/// <para>Fonts may only be created on the immediate context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or <paramref name="font"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="textureSize"/> width or height is larger than can be handled by the current feature level.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown if the graphics context is deferred.</exception>
		public GorgonFont CreateFont(string fontName, Font font, FontAntiAliasMode antiAliasMode, Size textureSize)
		{
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }

			var settings = new GorgonFontSettings
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
		/// <para>Please note that the <paramref name="fontName"/> parameter is user defined and does not have to be the same as the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.FontFamilyName">FontFamilyName</see> in the <paramref name="settings"/> parameter.</para>
		/// <para>Fonts may only be created on the immediate context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the fontName or settings parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fontName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.TextureSize">settings.TextureSize</see> width or height is larger than can be handled by the current feature level.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.DefaultCharacter">settings.DefaultCharacter</see> cannot be located in the <see cref="P:GorgonLibrary.Graphics.GorgonFontSettings.Characters">settings.Characters</see> list.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown if the graphics context is deferred.</exception>
		public GorgonFont CreateFont(string fontName, GorgonFontSettings settings)
		{
			if (_graphics.IsDeferred)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
			}

            if (fontName == null)
            {
                throw new ArgumentNullException("fontName");
            }

            if (string.IsNullOrWhiteSpace("fontName"))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "fontName");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (string.IsNullOrWhiteSpace(settings.FontFamilyName))
            {
                throw new ArgumentException(Resources.GORGFX_FONT_FAMILY_NAME_MUST_NOT_BE_EMPTY, "settings");
            }

			var result = new GorgonFont(_graphics, fontName, settings);

			result.GenerateFont(settings);

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
