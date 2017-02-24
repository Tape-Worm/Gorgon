﻿#region MIT.
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A factory used to create, read, and cache font data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This factory will create new bitmap fonts for applications to use when rendering text data. Fonts generated by the factory are cached for the lifetime of the factory and will be reused if font information 
	/// (e.g. name, information, etc...) is the same.
	/// </para>
	/// <para>
	/// Applications can also use this factory to load fonts from the file system, and have them cached as well.
	/// </para>
	/// <para>
	/// The factory will also built a <see cref="DefaultFont">default font</see> for quick use in applications. 
	/// </para>
	/// <para>
	/// Applications should only create a single instance of object for the lifetime of the application, any more than that may be wasteful.
	/// </para>
	/// <para>
	/// Because this object can contain a large amount of data, and implements <see cref="IDisposable"/>, it is required for applications to call the <see cref="IDisposable.Dispose"/> method when shutting down the 
	/// factory.
	/// </para>
	/// </remarks>
	public class GorgonFontFactory
		: IDisposable
	{
		#region Variables.
		// The graphics interface used to generate the fonts.
		private readonly GorgonGraphics _graphics;
		// The default font.
		private GorgonFont _default;
		// Thread synchronization.
		private readonly object _syncLock = new object();
		// The cache used to hold previously created font data.
		private readonly Dictionary<string, GorgonFont> _fontCache = new Dictionary<string, GorgonFont>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default font.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will return a default font used in applications for quick testing.
		/// </para>
		/// <para>
		/// The font will be based on Segoe UI, have a size of 10 pixels, and will be bolded and antialiased.
		/// </para>
		/// </remarks>
		public GorgonFont DefaultFont
		{
			get
			{
			    lock(_syncLock)
			    {
			        if (_default != null)
			        {
			            return _default;
			        }

			        // Create the default font.
				    _default = new GorgonFont("Gorgon.Font.Default.SegoeUI_14px",
				                              _graphics,
				                              new GorgonFontInfo("Segoe UI", 14)
				                              {
					                              AntiAliasingMode = FontAntiAliasMode.AntiAlias,
					                              FontStyle = FontStyle.Bold,
					                              OutlineSize = 0
				                              });

			        _default.GenerateFont();
			    }

			    return _default;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read the information for a font.
		/// </summary>
		/// <param name="fontFile">The reader for the chunked font file format.</param>
		/// <returns>A new <see cref="IGorgonFontInfo"/> containing the font information.</returns>
		internal static GorgonFontInfo ReadFontInfo(GorgonChunkFileReader fontFile)
		{
			GorgonBinaryReader reader = fontFile.OpenChunk(GorgonFont.FontInfoChunk);
			GorgonFontInfo info = new GorgonFontInfo(reader.ReadString(), reader.ReadSingle(), reader.ReadValue<FontHeightMode>())
			{
				FontStyle = reader.ReadValue<FontStyle>(),
				DefaultCharacter = reader.ReadChar(),
				Characters = reader.ReadString(),
				AntiAliasingMode = reader.ReadValue<FontAntiAliasMode>(),
				OutlineColor1 = new GorgonColor(reader.ReadInt32()),
				OutlineColor2 = new GorgonColor(reader.ReadInt32()),
				OutlineSize = reader.ReadInt32(),
				PackingSpacing = reader.ReadInt32(),
				TextureWidth = reader.ReadInt32(),
				TextureHeight = reader.ReadInt32(),
				UsePremultipliedTextures = reader.ReadValue<bool>(),
				UseKerningPairs = reader.ReadBoolean()
			};
			fontFile.CloseChunk();

			return info;
		}

		/// <summary>
		/// Function to read a font from a stream.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The font in the stream.</returns>
		/// <remarks>
		/// <para>Fonts may only be created on the immediate context.</para></remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="name"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
		public GorgonFont FromStream(string name, Stream stream)
		{
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.Length == 0)
            {
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(stream));
            }

			lock (_syncLock)
			{
				GorgonChunkFileReader fontFile = null;

				try
				{
					fontFile = new GorgonChunkFileReader(stream,
					                                     new[]
					                                     {
						                                     GorgonFont.FileHeader.ChunkID()
					                                     });
					fontFile.Open();

					var font = new GorgonFont(name, _graphics);

					// Read the font information chunk data.
					GorgonFontInfo info = ReadFontInfo(fontFile);

					// Check the cache for a font with the same name and having the same information.
					GorgonFont existingFont;

					if ((_fontCache.TryGetValue(name, out existingFont))
					    && (!IsFontDifferent(existingFont.Info, info)))
					{
						font.Dispose();
						return existingFont;
					}

					font.ReadFont(fontFile, info);

					_fontCache[name] = font;

					return font;
				}
				finally
				{
					fontFile?.Close();
				}
			}
		}

		/// <summary>
		/// Function to read a font from memory.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="fontData">Byte array containing the font data.</param>
		/// <returns>The font in the array.</returns>
		/// <remarks>
		/// <para>Fonts may only be created on the immediate context.</para></remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fontData"/> or the <paramref name="name"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the fontData array is empty.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
		public GorgonFont FromMemory(string name, byte[] fontData)
        {
            using (IGorgonPointer pointer = new GorgonPointerPinned<byte>(fontData))
            {
                return FromStream(name, new GorgonDataStream(pointer));
            }
        }

		/// <summary>
		/// Function to read a font from a file.
		/// </summary>
		/// <param name="name">Name of the font object.</param>
		/// <param name="fileName">Path and filename of the font to load.</param>
		/// <returns>The font in the file.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileName"/> or the <paramref name="name"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the fileName or name parameters are empty strings.
		/// <para>-or-</para>
		/// <para>Thrown if the font uses external textures, but the stream is not a file stream.</para></exception>
		/// <exception cref="GorgonException">Thrown if the font cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown if the graphics context is deferred.</para>
		/// </exception>
		public GorgonFont FromFile(string name, string fileName)
		{
			FileStream stream = null;

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(fileName));
            }
            
            try
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				return FromStream(name, stream);
			}
			finally
            {
	            stream?.Dispose();
            }
		}

		/// <summary>
		/// Function to invalidate the cached fonts for this factory.
		/// </summary>
		public void InvalidateCache()
		{
			lock (_syncLock)
			{
				foreach (KeyValuePair<string, GorgonFont> font in _fontCache)
				{
					font.Value.Dispose();
				}

				_fontCache.Clear();
			}
		}

		/// <summary>
		/// Function to compare two <see cref="IGorgonFontInfo"/> types to determine equality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if the fonts are different, <b>false</b> if not.</returns>
		private static bool IsFontDifferent(IGorgonFontInfo left, IGorgonFontInfo right)
		{
			return ((left.UseKerningPairs != right.UseKerningPairs)
			        || (left.FontHeightMode != right.FontHeightMode)
			        || (left.TextureWidth != right.TextureWidth)
			        || (left.TextureHeight != right.TextureHeight)
			        || (left.AntiAliasingMode != right.AntiAliasingMode)
			        || (left.Brush != right.Brush)
			        || (left.DefaultCharacter != right.DefaultCharacter)
			        || (!string.Equals(left.FontFamilyName, right.FontFamilyName, StringComparison.CurrentCultureIgnoreCase))
			        || (left.FontStyle != right.FontStyle)
			        || (left.OutlineColor1 != right.OutlineColor1)
			        || (left.OutlineColor2 != right.OutlineColor2)
			        || (left.OutlineSize != right.OutlineSize)
			        || (left.PackingSpacing != right.PackingSpacing)
			        || (!left.Size.EqualsEpsilon(right.Size))
			        || (!left.Characters.SequenceEqual(right.Characters)));
		}

		/// <summary>
		/// Function to return or create a new <see cref="GorgonFont"/>.
		/// </summary>
		/// <param name="name">The name of the font.</param>
		/// <param name="fontInfo">The information used to create the font.</param>
		/// <returns>A new or existing <see cref="GorgonFont"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="fontInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="IGorgonFontInfo.TextureWidth"/> or <see cref="IGorgonFontInfo.TextureHeight"/> parameters exceed the <see cref="IGorgonVideoDevice.MaxTextureWidth"/> or 
		/// <see cref="IGorgonVideoDevice.MaxTextureHeight"/> available for the current <see cref="FeatureLevelSupport"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="IGorgonFontInfo.Characters"/> list does not contain the <see cref="IGorgonFontInfo.DefaultCharacter"/> character.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to display text (or symbols) on the screen.  
		/// Kerning information (the proper spacing for a glyph), advances, etc... are all included for the glyphs with font.
		/// </para>
		/// <para>
		/// The <paramref name="name"/> parameter is used in caching, and is user defined. It is not necessary to have it share the same name as the font family name in the <paramref name="fontInfo"/> parameter, 
		/// however it is best practice to indicate the font family name in the name for ease of use. 
		/// </para>
		/// <para>
		/// If a font with the same name was previously created by this factory, then that font will be returned if the <paramref name="fontInfo"/> is the same as the cached version. If no font with the same 
		/// name or the <paramref name="fontInfo"/> is different, then a new font is generated. If the names are the same, then the new font will replace the old.
		/// </para>
		/// </remarks>
		public GorgonFont GetFont(string name, IGorgonFontInfo fontInfo)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (fontInfo == null)
			{
				throw new ArgumentNullException(nameof(fontInfo));
			}

			lock (_syncLock)
			{
				GorgonFont result;

				// Check the cache for a font with the same name.
				if ((_fontCache.TryGetValue(name, out result))
					&& (!IsFontDifferent(fontInfo, result.Info)))
				{
					return result;
				}

				if ((fontInfo.TextureWidth > _graphics.VideoDevice.MaxTextureWidth)
				    || (fontInfo.TextureHeight > _graphics.VideoDevice.MaxTextureHeight))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_TEXTURE_SIZE_TOO_LARGE,
					                                          fontInfo.TextureWidth,
					                                          fontInfo.TextureHeight),
					                            nameof(fontInfo));
				}

				if (!fontInfo.Characters.Contains(fontInfo.DefaultCharacter))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_DOES_NOT_EXIST, fontInfo.DefaultCharacter));
				}

				if (Convert.ToInt32(fontInfo.DefaultCharacter) < 32)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Convert.ToInt32(fontInfo.DefaultCharacter)));
				}

				// Destroy the previous font if one exists with the same name.
				result?.Dispose();

				// If not found, then create a new font and cache it.
				_fontCache[name] = result = new GorgonFont(name, _graphics, fontInfo);
				result.GenerateFont();

				return result;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GorgonFont defaultFont = Interlocked.Exchange(ref _default, null);

			defaultFont?.Dispose();

			InvalidateCache();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontFactory"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create the font data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
		public GorgonFontFactory(GorgonGraphics graphics)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			_graphics = graphics;
		}
		#endregion
	}
}
