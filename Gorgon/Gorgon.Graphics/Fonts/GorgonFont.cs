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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides functionality for creating, reading, and saving bitmap fonts.
	/// </summary>
	public sealed class GorgonFont
		: GorgonNamedObject, IDisposable
	{
		#region Constants.
		/// <summary>
		/// Header for a Gorgon font file.
		/// </summary>
		public const string FileHeader = "GORFNT10";

        /// <summary>
        /// Default extension for font files.
        /// </summary>
	    public const string DefaultExtension = ".gorFont";
		#endregion

        #region Events
        /// <summary>
        /// Event fired when the font was changed.
        /// </summary>
	    public event EventHandler FontChanged;
        #endregion

        #region Variables.
		private readonly GorgonTexture2DSettings _textureSettings;		// Settings for the texture.
		private Bitmap _charBitmap;										// Bitmap used for character cropping.
		private readonly List<IDisposable> _internalTextures;			// A list of internal textures created by the font generator.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether there is an outline for this font.
		/// </summary>
		public bool HasOutline
		{
			get
			{
				return Settings.OutlineSize > 0 && (Settings.OutlineColor1.Alpha > 0 || Settings.OutlineColor2.Alpha > 0);
			}
		}

        /// <summary>
        /// Property to return whether the object was disposed or not.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
		/// Property to return a list of kerning pairs.
		/// </summary>
		public IReadOnlyDictionary<GorgonKerningPair, int> KerningPairs
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the font settings.
		/// </summary>
		public GorgonFontSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the glyphs for this font.
		/// </summary>
		/// <remarks>A glyph is a graphical representation of a character.  For Gorgon, this means a glyph for a specific character will point to a region of texels on a texture.
		/// <para>Note that the glyph for a character is not required to represent the exact character (for example, the character "A" could map to the "V" character on the texture).  This 
		/// will allow mapping of symbols to a character representation.</para>
		/// </remarks>
		public GorgonGlyphCollection Glyphs
		{
			get;
			private set;
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
		/// <remarks>This is not the same as line height, the line height is a combination of ascent, descent and internal/external leading space.</remarks>
		public float FontHeight
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to draw the glyph character onto the bitmap.
	    /// </summary>
	    /// <param name="graphics">Graphics interface.</param>
	    /// <param name="font">Font to use.</param>
	    /// <param name="format">Formatter for the string.</param>
	    /// <param name="character">Character to write.</param>
	    /// <param name="position">Position on the bitmap.</param>
	    /// <param name="useDefaultBrush"><b>true</b> to use the default brush, <b>false</b> to use the font glyph brush.</param>
	    private void DrawGlyphCharacter(System.Drawing.Graphics graphics, Font font, StringFormat format, char character, Rectangle position, bool useDefaultBrush)
	    {
		    string charString = character.ToString(CultureInfo.CurrentCulture);

			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.Clear(Color.FromArgb(0));

			switch (Settings.AntiAliasingMode)
			{
				case FontAntiAliasMode.AntiAlias:
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					break;
				default:
					graphics.SmoothingMode = SmoothingMode.None;
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					break;
			}

			if ((Settings.Brush.BrushType == GlyphBrushType.LinearGradient)
				&& (!useDefaultBrush))
			{
				((GorgonGlyphLinearGradientBrush)Settings.Brush).GradientRegion = position;
			}

		    using (Brush brush = useDefaultBrush ? new SolidBrush(Color.White) : Settings.Brush.ToGDIBrush())
		    {
				using (var outlineRenderer = new GraphicsPath())
				{
					outlineRenderer.AddString(charString,
					                            font.FontFamily,
					                            (int)Settings.FontStyle,
					                            font.Size,
					                            new RectangleF(0, 0, _charBitmap.Width, _charBitmap.Height), 
					                            format);

					// If we want an outline, then draw that first.
					if (HasOutline)
					{
						if ((Settings.OutlineColor1 == Settings.OutlineColor2)
						    || (Settings.OutlineSize < 3))
						{
							using (var outlinePen = new Pen(Settings.OutlineColor1, Settings.OutlineSize * 2))
							{
								outlinePen.LineJoin = LineJoin.Round;
								graphics.DrawPath(outlinePen, outlineRenderer);
							}
						}
						else
						{
							GorgonColor start = Settings.OutlineColor1;
							GorgonColor end = Settings.OutlineColor2;

							// Fade from the first to the second color via a linear function.
							for (int i = Settings.OutlineSize; i > 0; --i)
							{
								float delta = ((float)(i - 1) / (Settings.OutlineSize - 1));
								GorgonColor penColor;

								GorgonColor.Lerp(ref start, ref end, delta, out penColor);

								using (var outlinePen = new Pen(penColor, i))
								{
									outlinePen.LineJoin = LineJoin.Round;
									graphics.DrawPath(outlinePen, outlineRenderer);
								}
							}
						}
					}

					graphics.FillPath(brush, outlineRenderer);
				}
		    }

		    graphics.Flush();
		}

		/// <summary>
		/// Function to determine if a bitmap is empty.
		/// </summary>
		/// <param name="pixels">Pixels to evaluate.</param>
		/// <param name="x">Horizontal position.</param>
		/// <returns><b>true</b> if empty, <b>false</b> if not.</returns>
		private static unsafe bool IsBitmapColumnEmpty(BitmapData pixels, int x)
		{
			int* pixel = (int *)pixels.Scan0.ToPointer() + x;
			
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
			var pixel = (int*)pixels.Scan0.ToPointer();

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
		/// Function to return the character bounding rectangles.
		/// </summary>
		/// <param name="g">Graphics interface to use.</param>
		/// <param name="font">Font to apply.</param>
		/// <param name="format">Format for the font.</param>
		/// <param name="drawFormat">The string format used to draw the glyph.</param>
		/// <param name="c">Character to evaluate.</param>
		/// <returns>A rectangle for the bounding box.</returns>
		private Rectangle GetCharRect(System.Drawing.Graphics g, Font font, StringFormat format, StringFormat drawFormat, ref char c)
		{
			Region[] size = null;
			Region[] defaultSize = null;
		    char currentCharacter = c;
			BitmapData pixels = null;

			try
			{
				// Try to get the character size.
                size = g.MeasureCharacterRanges(currentCharacter.ToString(CultureInfo.CurrentCulture),
			                                    font,
			                                    new RectangleF(0, 0, Settings.TextureSize.Width, Settings.TextureSize.Height),
			                                    format);
			    defaultSize = g.MeasureCharacterRanges(Settings.DefaultCharacter.ToString(CultureInfo.CurrentCulture),
			                                           font,
			                                           new RectangleF(0,
			                                                          0,
			                                                          Settings.TextureSize.Width,
			                                                          Settings.TextureSize.Height),
			                                           format);

				// If the character doesn't exist, then return an empty value.
			    if ((size.Length == 0)
			        && (defaultSize.Length == 0))
			    {
			        return Rectangle.Empty;
			    }

			    // If we didn't get a size, but we have a default, then use that.
				if ((size.Length == 0) && (defaultSize.Length > 0))
				{
					currentCharacter = Settings.DefaultCharacter;
					size = defaultSize;
				}

				RectangleF result = size[0].GetBounds(g);

				if ((result.Width < 0.1f) || (result.Height < 0.1f))
				{
					currentCharacter = Settings.DefaultCharacter;
					size = defaultSize;
					result = size[0].GetBounds(g);
					size[0].Dispose();
					size = null;

				    if ((result.Width < 0.1f)
				        || (result.Height < 0.1f))
				    {
				        return Rectangle.Empty;
				    }
				}

				// Don't apply outline padding to whitespace.
				if ((HasOutline) && (!char.IsWhiteSpace(c)))
				{
					result.Width += Settings.OutlineSize * 3;
					result.Height += Settings.OutlineSize * 3;
				}
				else
				{
					result.Width += 1;
					result.Height += 1;
				}

				result.Width = (float)System.Math.Ceiling(result.Width);
				result.Height = (float)System.Math.Ceiling(result.Height);

				// Update the cached character bitmap size.
				if ((result.Width * 2 > _charBitmap.Width) || (result.Height > _charBitmap.Height))
				{
					_charBitmap.Dispose();
					_charBitmap = new Bitmap((int)result.Width * 2, (int)result.Height, PixelFormat.Format32bppArgb);					
				}

				var cropTopLeft = new Point(0, 0);
				var cropRightBottom = new Point(_charBitmap.Width - 1, _charBitmap.Height - 1);

				// Perform cropping.
				using (System.Drawing.Graphics charGraphics = System.Drawing.Graphics.FromImage(_charBitmap))
				{
					charGraphics.PageUnit = g.PageUnit;

					// Draw the character.
					DrawGlyphCharacter(charGraphics,
							            font,
							            drawFormat,
							            currentCharacter,
							            new Rectangle(0, 0, _charBitmap.Width, _charBitmap.Height),
										true);

					// We use unsafe mode to scan pixels, it's much faster.
				    pixels = _charBitmap.LockBits(new Rectangle(0, 0, _charBitmap.Width, _charBitmap.Height),
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
						cropTopLeft.X = (int)result.X;
						cropRightBottom.X = (int)result.Width - 1;
					}

				    // ReSharper disable InvertIf
					if (cropTopLeft.Y == cropRightBottom.Y)
					{
						cropTopLeft.Y = (int)result.Y;
						cropRightBottom.Y = (int)result.Height - 1;
					}
                    // ReSharper restore InvertIf
				}

				c = currentCharacter;

				return Rectangle.FromLTRB(cropTopLeft.X, cropTopLeft.Y, cropRightBottom.X, cropRightBottom.Y);
			}
			finally
			{
			    if (pixels != null)
			    {
			        _charBitmap.UnlockBits(pixels);
			    }

			    if (defaultSize != null)
				{
				    foreach (var item in defaultSize)
				    {
				        item.Dispose();
				    }
				}

                // ReSharper disable InvertIf
				if (size != null)
				{
				    foreach (var item in size)
				    {
				        item.Dispose();
				    }
				}
                // ReSharper restore InvertIf
            }
		}

		/// <summary>
		/// Function to copy bitmap data to a texture.
		/// </summary>
		/// <param name="bitmap">Bitmap to copy.</param>
		/// <param name="texture">Texture to receive the data.</param>
		private static unsafe void CopyBitmap(Bitmap bitmap, GorgonTexture2D texture)
		{
			BitmapData sourcePixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				var pixels = (int*)sourcePixels.Scan0.ToPointer();

				using (var data = new GorgonImageData(texture.Settings, pixels, texture.SizeInBytes))
				{
					for (int y = 0; y < bitmap.Height; y++)
					{
						int* offset = pixels + (y * bitmap.Width);
						for (int x = 0; x < bitmap.Width; x++)
						{
							data.Buffers[0].Data.Write(GorgonColor.FromABGR(*offset).ToARGB());
							offset++;
						}
					}

                    data.Buffers[0].Data.Position = 0;
                    texture.UpdateSubResource(data.Buffers[0], new Rectangle(0, 0, bitmap.Width, bitmap.Height));
				}
			}
			finally
			{
			    if (sourcePixels != null)
			    {
			        bitmap.UnlockBits(sourcePixels);
			    }
			}
		}

		/// <summary>
		/// Function to read the font data in from the chunked file.
		/// </summary>
		/// <param name="chunk">Reader for the chunked file.</param>
		/// <param name="missingFontTextureFunction">The method to call if a user defined glyph texture is missing.</param>
		internal void ReadFont(GorgonChunkReader chunk, Func<string, Size, GorgonTexture2D> missingFontTextureFunction)
		{
			FileStream fileStream = null;

			// Write font information.
			chunk.Begin("FONTDATA");
			Settings.FontFamilyName = chunk.ReadString();
			Settings.Size = chunk.ReadFloat();
			Settings.FontHeightMode = chunk.Read<FontHeightMode>();
			Settings.FontStyle = chunk.Read<FontStyle>();
			Settings.DefaultCharacter = chunk.ReadChar();
			Settings.Characters = chunk.ReadString();
			FontHeight = chunk.ReadFloat();
			LineHeight = chunk.ReadFloat();
			Ascent = chunk.ReadFloat();
			Descent = chunk.ReadFloat();
			chunk.End();

			// Write rendering information.
			chunk.Begin("RNDRDATA");
			Settings.AntiAliasingMode = chunk.Read<FontAntiAliasMode>();
			Settings.OutlineColor1 = chunk.Read<GorgonColor>();
			Settings.OutlineColor2 = chunk.Read<GorgonColor>();
			Settings.OutlineSize = chunk.ReadInt32();
			chunk.End();

			// Read in the glyph brush.
			if (chunk.HasChunk("BRSHDATA"))
			{
				chunk.Begin("BRSHDATA");
				var brushType = chunk.Read<GlyphBrushType>();

				Settings.Brush = GorgonGlyphBrush.CreateBrush(brushType, Graphics);

				if (Settings.Brush != null)
				{
					Settings.Brush.Read(chunk);
				}

				chunk.End();
			}
			
			// If we didn't get a brush, then create a default brush.
			if (Settings.Brush == null)
			{
				Settings.Brush = new GorgonGlyphSolidBrush();
			}

			// Write texture information.
			chunk.Begin("TXTRDATA");
			Settings.PackingSpacing = chunk.ReadInt32();
			Settings.TextureSize = chunk.ReadSize();
			int textureCount = chunk.ReadInt32();
			bool isExternal = false;

			if (chunk.HasChunk("TXTREXTL"))
			{
				isExternal = true;
				chunk.Begin("TXTREXTL");

				fileStream = chunk.BaseStream as FileStream;
				if (fileStream == null)
				{
					throw new IOException(Resources.GORGFX_FONT_MUST_BE_FILE_STREAM);
				}
			}
			else
			{
				chunk.Begin("TXTRINTL");
			}

			_internalTextures.Clear();
			var textures = new Dictionary<string, GorgonTexture2D>();

			// Load in the textures.
			for (int i = 0; i < textureCount; i++)
			{
				string textureName = chunk.ReadString();
				bool userTexture = chunk.ReadBoolean();

				GorgonTexture2D texture = null;

				// Only look at textures that weren't created by Gorgon internally and aren't font textures.
				if ((!textureName.StartsWith("GorgonFont.", StringComparison.OrdinalIgnoreCase))
					|| (textureName.IndexOf(".InternalTexture_", StringComparison.OrdinalIgnoreCase) == -1))
				{
					var textureFormat = BufferFormat.R8G8B8A8_UIntNormal;
					int mipCount = 1;
					int arrayCount = 1;

					if (userTexture)
					{
						textureFormat = chunk.Read<BufferFormat>();
						arrayCount = chunk.ReadInt32();
						mipCount = chunk.ReadInt32();
					}

					texture = (from internalTexture in Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
							  where (string.Equals(internalTexture.Name, textureName, StringComparison.OrdinalIgnoreCase))
								&& (internalTexture.Settings.Width == Settings.TextureSize.Width)
								&& (internalTexture.Settings.Height == Settings.TextureSize.Height)
								&& (internalTexture.Settings.Format == textureFormat)
								&& (internalTexture.Settings.MipCount == mipCount)
								&& (internalTexture.Settings.ArrayCount == arrayCount)
							  select internalTexture).FirstOrDefault();
				}

				if (texture != null)
				{
					textures[texture.Name] = texture;

					// If this is a user defined texture, then continue on.
					if (userTexture)
					{
						continue;
					}
				}
				
				// If this is a user texture, and we didn't get one from the cache, then
				// ask the user load the texture via a callback.  Otherwise, throw an exception.
				if (userTexture)
				{
					if (missingFontTextureFunction == null)
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));
					}

					texture = missingFontTextureFunction(textureName, Settings.TextureSize);
				
					// Returning NULL will mean we can't load the texture.
					if (texture == null)
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));
					}

					textures[textureName] = texture;
					continue;
				}

				if (!isExternal)
				{
					int textureSize = chunk.ReadInt32();

					if (textureSize == 0)
					{
						continue;
					}

					// If we've already got the texture, then move on.
					if (texture != null)
					{
						// Skip this data if we have to.
						chunk.SkipBytes(textureSize);
						continue;
					}

					texture = Graphics.Textures.FromStream<GorgonTexture2D>(textureName,
																				chunk.BaseStream,
																				textureSize,
																				new GorgonCodecPNG());
				}
				else
				{
					// Get the path to the texture (must be local to the font file).
					string texturePath = Path.GetDirectoryName(fileStream.Name).FormatDirectory(Path.DirectorySeparatorChar) + chunk.ReadString();

					if ((texture != null) || (string.IsNullOrWhiteSpace(texturePath)) || (!File.Exists(texturePath)))
					{
						continue;
					}

					texture = Graphics.Textures.FromFile<GorgonTexture2D>(textureName,
																			texturePath,
																			new GorgonCodecPNG());
				}

				// Don't track these textures.
				Graphics.RemoveTrackedObject(texture);
				textures[texture.Name] = texture;
				_internalTextures.Add(texture);
			}
			chunk.End();

			// Get glyph information.
			chunk.Begin("GLYFDATA");
			int groupCount = chunk.ReadInt32();

			for (int i = 0; i < groupCount; i++)
			{
				string textureName = chunk.ReadString();
				int glyphCount = chunk.ReadInt32();

				GorgonTexture2D texture;
				textures.TryGetValue(textureName, out texture);

				for (int j = 0; j < glyphCount; j++)
				{
					char glyphChar = chunk.ReadChar();

					// If the texture does not exist (hasn't been loaded or something), then do not allow the glyph to be rendered.
					if (texture == null)
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));
					}

				    var glyph = new GorgonGlyph(glyphChar,
				                                texture,
				                                chunk.ReadRectangle(),
				                                new Point(chunk.ReadInt32(), chunk.ReadInt32()),
                                                chunk.ReadInt32())
				                {
				                    IsExternalTexture = !_internalTextures.Contains(texture)
				                };
					Glyphs.Add(glyph);
				}
			}
			chunk.End();

			if (chunk.HasChunk("CUSTGLYF"))
			{
				chunk.Begin("CUSTGLYF");

				groupCount = chunk.ReadInt32();

				Settings.Glyphs.Clear();

				for (int i = 0; i < groupCount; ++i)
				{
					string textureName = chunk.ReadString();
					int glyphCount = chunk.ReadInt32();

					GorgonTexture2D texture;
					textures.TryGetValue(textureName, out texture);

					for (int j = 0; j < glyphCount; j++)
					{
						char glyphChar = chunk.ReadChar();

						// If the texture does not exist (hasn't been loaded or something), then do not allow the glyph to be rendered.
						if (texture == null)
						{
							throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));
						}

						var glyph = new GorgonGlyph(glyphChar,
													texture,
													chunk.ReadRectangle(),
													new Point(chunk.ReadInt32(), chunk.ReadInt32()), 
													chunk.ReadInt32());
						Settings.Glyphs.Add(glyph);
					}
				}
				chunk.End();
			}

			// Read optional kerning information.
			int kernCount;

			Dictionary<GorgonKerningPair, int> kerningPairs = null;

			if (chunk.HasChunk("KERNDATA"))
			{
				kerningPairs = new Dictionary<GorgonKerningPair, int>();
				chunk.Begin("KERNDATA");
				kernCount = chunk.ReadInt32();
				for (int i = 0; i < kernCount; i++)
				{
					kerningPairs.Add(new GorgonKerningPair(chunk.ReadChar(), chunk.ReadChar()), chunk.ReadInt32());
				}
				chunk.End();
			}

			// Check for custom kerning information metadata.
			if (chunk.HasChunk("CUSTKERN"))
			{
				if (kerningPairs == null)
				{
					kerningPairs = new Dictionary<GorgonKerningPair, int>();	
				}

				Settings.KerningPairs.Clear();

				// Restore the custom meta data.
				chunk.Begin("CUSTKERN");
				kernCount = chunk.ReadInt32();
				for (int i = 0; i < kernCount; ++i)
				{
					var kerningPair = new GorgonKerningPair(chunk.ReadChar(), chunk.ReadChar());

					kerningPairs[kerningPair] = Settings.KerningPairs[kerningPair] = chunk.ReadInt32();
				}
				chunk.End();
			}

			if ((kerningPairs != null) && (Settings.UseKerningPairs))
			{
				KerningPairs = new ReadOnlyDictionary<GorgonKerningPair, int>(kerningPairs);
			}

			// Check for custom advancements.
			if (chunk.HasChunk("CUSTABCS"))
			{
				Settings.Advances.Clear();

				chunk.Begin("CUSTABCS");
				int abcCount = chunk.ReadInt32();

				for (int i = 0; i < abcCount; ++i)
				{
                    Settings.Advances.Add(chunk.ReadChar(), chunk.ReadInt32());
				}
				chunk.End();
			}

			// Check for custom vertical offsets.
			if (!chunk.HasChunk("CUSTVOFF"))
			{
				return;
			}

			Settings.Offsets.Clear();

			chunk.Begin("CUSTVOFF");
			int offsetCount = chunk.ReadInt32();

			for (int i = 0; i < offsetCount; ++i)
			{
				Settings.Offsets.Add(chunk.ReadChar(), new Point(chunk.ReadInt32(), chunk.ReadInt32()));
			}
			chunk.End();
		}

		/// <summary>
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		/// <param name="externalTextures">[Optional] <b>true</b> to save the textures as external files, <b>false</b> to bundle them with the font.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter does not allow for writing.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the externalTextures parameter is <b>true</b> and the stream is not a file stream.</exception>
		/// <remarks>The <paramref name="externalTextures"/> parameter will only work on file streams, if the stream is not a file stream, then an exception will be thrown.</remarks>
		public void Save(Stream stream, bool externalTextures = false)
		{
			var fileStream = stream as FileStream;

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORGFX_STREAM_READ_ONLY);
			}

			if ((externalTextures) && (fileStream == null))
			{
				throw new ArgumentException(Resources.GORGFX_FONT_MUST_BE_FILE_STREAM, "externalTextures");
			}

			// Output the font in chunked format.
			using (var chunk = new GorgonChunkWriter(stream))
			{
				string characterList = string.Join(string.Empty, Settings.Characters);

				chunk.Begin(FileHeader);

				// Write font information.
				chunk.Begin("FONTDATA");
				chunk.WriteString(Settings.FontFamilyName);
				chunk.WriteFloat(Settings.Size);
				chunk.Write(Settings.FontHeightMode);
				chunk.Write(Settings.FontStyle);
				chunk.WriteChar(Settings.DefaultCharacter);
				chunk.WriteString(characterList);
				chunk.WriteFloat(FontHeight);
				chunk.WriteFloat(LineHeight);
				chunk.WriteFloat(Ascent);
				chunk.WriteFloat(Descent);
				chunk.End();

				// Write rendering information.
				chunk.Begin("RNDRDATA");
				chunk.Write(Settings.AntiAliasingMode);
				chunk.Write(Settings.OutlineColor1);
				chunk.Write(Settings.OutlineColor2);
				chunk.WriteInt32(Settings.OutlineSize);
				chunk.End();

				// If we assigned a brush to render the glyph, then serialize it.
				if (Settings.Brush == null)
				{
					Settings.Brush = new GorgonGlyphSolidBrush();
				}
				Settings.Brush.Write(chunk);

				// Write out glyph data.
				var textureGlyphs = (from GorgonGlyph glyph in Glyphs
									 where glyph.Texture != null
									 group glyph by glyph.Texture).ToArray();

				// Write texture information.
				chunk.Begin("TXTRDATA");
				chunk.WriteInt32(Settings.PackingSpacing);
				chunk.WriteSize(Settings.TextureSize);
				chunk.WriteInt32(textureGlyphs.Length);
				chunk.End();

				// Write out actual textures.
				chunk.Begin(!externalTextures ? "TXTRINTL" : "TXTREXTL");

				int textureCounter = 0;

				foreach (GorgonTexture2D texture in textureGlyphs.Select(textureGroup => textureGroup.Key))
				{
					chunk.WriteString(texture.Name);

					// If we didn't create this texture, then record its name for deferred loading.
					if (!_internalTextures.Contains(texture))
					{
						GorgonTexture2DSettings settings = texture.Settings;

						chunk.WriteBoolean(true);
						chunk.Write(settings.Format);
						chunk.Write(settings.ArrayCount);
						chunk.Write(settings.MipCount);
						textureCounter++;
						continue;
					}
					
					// We created this texture.
					chunk.WriteBoolean(false);

					if (!externalTextures)
					{
						long startPosition = stream.Position;		// Save the stream position to avoid having to save the PNG data to an array.

						// Write placeholder.
						chunk.WriteInt32(0);

						// Save our texture and record where we left off.
						texture.Save(stream, new GorgonCodecPNG());
						long endPosition = stream.Position;

						// Put the image size in the placeholder.
						stream.Position = startPosition;
						chunk.WriteInt32((int)(endPosition - startPosition - sizeof(int)));
						stream.Position = endPosition;
					}
					else
					{
						string path = Path.GetDirectoryName(fileStream.Name);
						string textureFileName = Path.GetFileNameWithoutExtension(fileStream.Name) + "Texture_" + textureCounter.ToString("0000") + ".png";

						textureFileName = textureFileName.FormatFileName().Replace(' ', '_');

						// Write out the file in the same directory as the font info.
						texture.Save(path.FormatDirectory(Path.DirectorySeparatorChar) + textureFileName, new GorgonCodecPNG());

						chunk.WriteString(textureFileName);
					}

					textureCounter++;
				}
				chunk.End();

				chunk.Begin("GLYFDATA");
				chunk.WriteInt32(textureGlyphs.Length);
				foreach (var glyphGroup in textureGlyphs)
				{
					chunk.WriteString(glyphGroup.Key.Name);
					chunk.WriteInt32(glyphGroup.Count());

					foreach (var glyph in glyphGroup)
					{
						chunk.WriteChar(glyph.Character);
						chunk.WriteRectangle(glyph.GlyphCoordinates);
						chunk.Write(glyph.Offset);
						chunk.Write(glyph.Advance);
					}
				}
				chunk.End();

				// Write out custom glyph information if it exists.
				if ((Settings.Glyphs != null) && (Settings.Glyphs.Count > 0))
				{
					chunk.Begin("CUSTGLYF");

					var customGlyphs = (from glyph in Settings.Glyphs
					                   group glyph by glyph.Texture).ToArray();

					chunk.WriteInt32(customGlyphs.Length);
					foreach (var glyphTexture in customGlyphs)
					{
						chunk.WriteString(glyphTexture.Key.Name);
						chunk.WriteInt32(glyphTexture.Count());
						foreach (var glyph in glyphTexture)
						{
							chunk.WriteChar(glyph.Character);
							chunk.WriteRectangle(glyph.GlyphCoordinates);
							chunk.WriteInt32(glyph.Offset.X);
                            chunk.WriteInt32(glyph.Offset.Y);
							chunk.WriteInt32(glyph.Advance);
						}
					}
					chunk.End();
				}

				// Write out optional kerning information.
				if (Settings.UseKerningPairs)
				{
					if (KerningPairs.Count > 0)
					{
						chunk.Begin("KERNDATA");
						chunk.WriteInt32(KerningPairs.Count);
						foreach (var kernInfo in KerningPairs)
						{
							chunk.WriteChar(kernInfo.Key.LeftCharacter);
							chunk.WriteChar(kernInfo.Key.RightCharacter);
							chunk.WriteInt32(kernInfo.Value);
						}
						chunk.End();
					}

					// Write custom kerning information if it exists.
					if (Settings.KerningPairs.Count > 0)
					{
						chunk.Begin("CUSTKERN");
						chunk.WriteInt32(Settings.KerningPairs.Count);
						foreach (var kernPair in Settings.KerningPairs)
						{
							chunk.WriteChar(kernPair.Key.LeftCharacter);
							chunk.WriteChar(kernPair.Key.RightCharacter);
							chunk.WriteInt32(kernPair.Value);
						}
						chunk.End();
					}
				}

				if (Settings.Advances.Count > 0)
				{
					chunk.Begin("CUSTABCS");
					chunk.WriteInt32(Settings.Advances.Count);
					foreach (var abc in Settings.Advances)
					{
						chunk.WriteChar(abc.Key);
						chunk.WriteInt32(abc.Value);
					}
					chunk.End();
				}

				if (Settings.Offsets.Count == 0)
				{
					return;
				}

				chunk.Begin("CUSTVOFF");
				chunk.WriteInt32(Settings.Offsets.Count);
				foreach (var offset in Settings.Offsets)
				{
					chunk.WriteChar(offset.Key);
					chunk.WriteInt32(offset.Value.X);
					chunk.WriteInt32(offset.Value.Y);
				}
				chunk.End();
			}
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">File name and path of the font to save.</param>
		/// <param name="externalTextures">[Optional] <b>true</b> to save the textures external to the font file, <b>false</b> to bundle together with the font file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		/// <remarks>Saving the textures externally with the <paramref name="externalTextures"/> parameter set to <b>true</b> is good for altering the textures in an image 
		/// editing application.  Ultimately, it is recommended that the textures be bundled with the font by setting externalTextures to <b>false</b>.</remarks>
		public void Save(string fileName, bool externalTextures = false)
		{
			FileStream stream = null;

			GorgonDebug.AssertParamString(fileName, "fileName");

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, externalTextures);
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
		/// Function to retrieve the built in kerning pairs and advancement information for the font.
		/// </summary>
		/// <param name="graphics">The GDI graphics interface.</param>
		/// <param name="font">The GDI font.</param>
		/// <param name="allowedCharacters">The list of characters available to the font.</param>
		private IDictionary<char, ABC> GetKerningInformation(System.Drawing.Graphics graphics, Font font, IList<char> allowedCharacters)
		{
			IDictionary<char, ABC> advancementInfo;
			var newKernData = new Dictionary<GorgonKerningPair, int>();

			Win32API.SetActiveFont(graphics, font);

			try
			{
				advancementInfo = Win32API.GetCharABCWidths(allowedCharacters[0], allowedCharacters[allowedCharacters.Count - 1]);

				if (!Settings.UseKerningPairs)
				{
					return advancementInfo;
				}

				IList<KERNINGPAIR> kerningPairs = Win32API.GetKerningPairs();

				foreach (var pair in kerningPairs.Where(item => item.KernAmount != 0))
				{
					var newPair = new GorgonKerningPair(Convert.ToChar(pair.First), Convert.ToChar(pair.Second));

					if ((!allowedCharacters.Contains(newPair.LeftCharacter)) || 
						(!allowedCharacters.Contains(newPair.RightCharacter)))
					{
						continue;
					}

					newKernData[newPair] = pair.KernAmount;
				}

				// Copy the custom kern settings.
				foreach (var customKern in Settings.KerningPairs)
				{
					if (((!allowedCharacters.Contains(customKern.Key.LeftCharacter))
					     && (Settings.Glyphs.All(item => item.Character != customKern.Key.LeftCharacter)))
					    || ((!allowedCharacters.Contains(customKern.Key.RightCharacter))
					        && (Settings.Glyphs.All(item => item.Character != customKern.Key.RightCharacter))))
					{
						continue;
					}

					newKernData[customKern.Key] = customKern.Value;
				}
			}
			finally
			{
				Win32API.RestoreActiveObject();
				KerningPairs = new ReadOnlyDictionary<GorgonKerningPair, int>(newKernData);
			}

			return advancementInfo;
		}

		/// <summary>
		/// Function to create or update the font.
		/// </summary>
		/// <param name="settings">Font settings to use.</param>
		/// <remarks>
		/// This is used to generate a new set of font textures, and essentially "create" the font object.
		/// <para>This method will clear all the glyphs and textures in the font and rebuild the font with the specified parameters. This means that any 
		/// custom glyphs, texture mapping, and/or kerning will be lost. Users must find a way to remember and restore any custom font info when updating.</para>
		/// <para>Internal textures used by the glyph will be destroyed.  However, if there's a user defined texture or glyph using a user defined texture, then it will not be destroyed 
		/// and clean up will be the responsibility of the user.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the texture size in the settings exceeds that of the capabilities of the feature level.
		/// <para>-or-</para>
		/// <para>Thrown when the font family name is NULL or Empty.</para>
		/// </exception>
		internal void GenerateFont(GorgonFontSettings settings)
		{
			Font newFont = null;
		    Bitmap tempBitmap = null;
			StringFormat stringFormat = null;
			StringFormat drawFormat = null;
			System.Drawing.Graphics graphics = null;
		    var range = new[] { new CharacterRange(0, 1) };

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			if (string.IsNullOrWhiteSpace(settings.FontFamilyName))
			{
				throw new ArgumentException(Resources.GORGFX_FONT_FAMILY_NAME_MUST_NOT_BE_EMPTY, "settings");
			}

			if ((settings.TextureSize.Width >= Graphics.Textures.MaxWidth) 
                || (settings.TextureSize.Height >= Graphics.Textures.MaxHeight))
			{
			    throw new GorgonException(GorgonResult.CannotCreate,
			        string.Format(Resources.GORGFX_FONT_TEXTURE_SIZE_TOO_LARGE,
			                      Settings.TextureSize.Width,
			                      Settings.TextureSize.Height));
			}

			if (!settings.Characters.Contains(settings.DefaultCharacter))
			{
			    throw new GorgonException(GorgonResult.CannotCreate,
			        string.Format(Resources.GORGFX_FONT_DEFAULT_CHAR_DOES_NOT_EXIST, settings.DefaultCharacter));
			}

			try
			{
				// Remove all previous glyphs.
				Settings = settings;

				if (Settings.Brush == null)
				{
					Settings.Brush = new GorgonGlyphSolidBrush();
				}

				// Create the font and the rasterizing surface.				
				tempBitmap = new Bitmap(Settings.TextureSize.Width, Settings.TextureSize.Height, PixelFormat.Format32bppArgb);
				
				graphics = System.Drawing.Graphics.FromImage(tempBitmap);
				graphics.PageUnit = GraphicsUnit.Pixel;
				
				// Scale the font appropriately.
			    if (Settings.FontHeightMode == FontHeightMode.Points)
			    {
			        newFont = new Font(Settings.FontFamilyName,
			                           (Settings.Size * graphics.DpiY) / 72.0f,
			                           Settings.FontStyle,
			                           GraphicsUnit.Pixel);
			    }
			    else
			    {
			        newFont = new Font(Settings.FontFamilyName, Settings.Size, Settings.FontStyle, GraphicsUnit.Pixel);
			    }

			    FontHeight = newFont.GetHeight(graphics);
				FontFamily family = newFont.FontFamily;

				// Get font metrics.
				LineHeight = (FontHeight * family.GetLineSpacing(newFont.Style)) / family.GetEmHeight(newFont.Style);
				Ascent = (FontHeight * family.GetCellAscent(newFont.Style)) / family.GetEmHeight(newFont.Style);
				Descent = (FontHeight * family.GetCellDescent(newFont.Style)) / family.GetEmHeight(newFont.Style);

				stringFormat = new StringFormat(StringFormat.GenericTypographic)
				    {
				        FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces,
				        Alignment = StringAlignment.Near,
				        LineAlignment = StringAlignment.Near
				    };
			    stringFormat.SetMeasurableCharacterRanges(range);

				// Create a separate drawing format because some glyphs are being clipped when they have overhang
				// on the left boundary.
				drawFormat = new StringFormat(StringFormat.GenericDefault)
				    {
				        FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces,
				        Alignment = StringAlignment.Near,
				        LineAlignment = StringAlignment.Near
				    };
			    drawFormat.SetMeasurableCharacterRanges(range);

				// Add custom glyphs.
				Glyphs = new GorgonGlyphCollection(Settings.Glyphs);

				// Remove control characters and anything below a space.
				List<char> availableCharacters = (from chars in Settings.Characters
				                                  where (!Char.IsControl(chars)) && (!Glyphs.Contains(chars) && (Convert.ToInt32(chars) >= 32) && (!char.IsWhiteSpace(chars)))
				                                  select chars).ToList();

				// Ensure the default character is there.
			    if (!availableCharacters.Contains(settings.DefaultCharacter))
			    {
			        availableCharacters.Insert(0, settings.DefaultCharacter);
			    }

				// Default to the line height size.
				_charBitmap = new Bitmap((int)(System.Math.Ceiling(LineHeight)), (int)(System.Math.Ceiling(LineHeight)));

				// Get kerning and glyph advancement information.
				IDictionary<char, ABC> charABC = GetKerningInformation(graphics, newFont, availableCharacters);
				var charBounds = new Dictionary<char, Rectangle>();

				var gradRegion = new Rectangle(Int32.MaxValue, Int32.MaxValue, Int32.MinValue, Int32.MinValue);

				// Get all the character rectangles.
				foreach (char availableChar in availableCharacters)
				{
					char updatedChar = availableChar;

					Rectangle charRect = GetCharRect(graphics, newFont, stringFormat, drawFormat, ref updatedChar);

					if (charRect == Rectangle.Empty)
					{
						continue;
					}

					charBounds[updatedChar] = charRect;

					// Find the largest bounding area for the gradient fill (if we have one).
					gradRegion = Rectangle.FromLTRB(gradRegion.Left.Min(charRect.Left),
					                           gradRegion.Top.Min(charRect.Top),
					                           gradRegion.Right.Max(charRect.Right),
					                           gradRegion.Bottom.Max(charRect.Bottom));
				}

				// Because the dictionary above remaps characters (if they don't have a glyph or their rects are empty),
				// we'll need to drop these from our main list of characters.
				availableCharacters.RemoveAll(item => !charBounds.ContainsKey(item));

				while (availableCharacters.Count > 0)
				{
					// Sort by size.
					availableCharacters.Sort((left, right) =>
					                         {
						                         Size leftSize = charBounds[left].Size;
						                         Size rightSize = charBounds[right].Size;

						                         if (leftSize.Height == rightSize.Height)
						                         {
							                         return left.CompareTo(right);
						                         }

						                         return leftSize.Height < rightSize.Height ? 1 : -1; //leftArea < rightArea ? 1 : -1;
					                         });

					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.Clear(Color.FromArgb(0, 0, 0, 0));

					// Create a texture.
					_textureSettings.Width = Settings.TextureSize.Width;
					_textureSettings.Height = Settings.TextureSize.Height;
					var currentTexture = new GorgonTexture2D(Graphics, "GorgonFont." + Name + ".InternalTexture_" + Guid.NewGuid(), _textureSettings);
					currentTexture.Initialize(null);

					GorgonGlyphPacker.CreateRoot(Settings.TextureSize);

					// Begin rasterization.
					int charIndex = 0;
					int packingSpace = Settings.PackingSpacing > 0 ? Settings.PackingSpacing * 2 : 1;

					while (charIndex < availableCharacters.Count)
					{
						char c = availableCharacters[charIndex];

						// Skip whitespace characters.
						if (char.IsWhiteSpace(c))
						{
							availableCharacters.Remove(c);
							c = Settings.DefaultCharacter;
						}

						// If we've already put this glyph in, then skip it.
						if (Glyphs.Contains(c))
						{
							availableCharacters.Remove(c);
							continue;
						}

						Rectangle charRect = charBounds[c];
					    
						Size size = charRect.Size;
						size.Width += 1;
						size.Height += 1;

						// Don't add whitespace, we can auto calculate that.
						if (!Char.IsWhiteSpace(c))
						{
							Rectangle? placement = GorgonGlyphPacker.Add(new Size(charRect.Width + packingSpace, charRect.Height + packingSpace));
						    
                            if (placement == null)
                            {
	                            ++charIndex;
						        continue;
						    }

							availableCharacters.Remove(c);

						    Point location = placement.Value.Location;

						    location.X += Settings.PackingSpacing;
						    location.Y += Settings.PackingSpacing;

							using (var charGraphics = System.Drawing.Graphics.FromImage(_charBitmap))
							{
								charGraphics.PageUnit = GraphicsUnit.Pixel;
								DrawGlyphCharacter(charGraphics,
								                   newFont,
								                   drawFormat,
								                   c,
								                   Rectangle.Inflate(new Rectangle(charRect.Left,
								                                                   gradRegion.Top,
								                                                   charRect.Width,
								                                                   gradRegion.Height),
								                                     2,
								                                     2),
								                   false);
							}
							
						    graphics.DrawImage(_charBitmap, new Rectangle(location, size), new Rectangle(charRect.Location, size), GraphicsUnit.Pixel);

							// If we've defined a vertical offset override, then use it.
						    Point offset;

                            if (Settings.Offsets.TryGetValue(c, out offset))
							{
								charRect.Location = offset;
							}

							int advance;

							// Get the ABC override if one exists.
							if (!Settings.Advances.TryGetValue(c, out advance))
							{
                                ABC advanceData;
                                charABC.TryGetValue(c, out advanceData);
                                advance = advanceData.A + (int)advanceData.B + advanceData.C;
                            }

							Glyphs.Add(new GorgonGlyph(c,
						                               currentTexture,
						                               new Rectangle(location, size),
						                               charRect.Location,
						                               advance)
						               {
						                   IsExternalTexture = false
						               });
						}
						else
						{
							// Add whitespace glyph, this will never be rendered, but we need the size in order to determine how much space is required.
						    Glyphs.Add(new GorgonGlyph(settings.DefaultCharacter,
						                                currentTexture,
						                                new Rectangle(0, 0, size.Width, size.Height),
						                                Point.Empty,
						                                size.Width)
						                {
						                    IsExternalTexture = false
						                });
						}
					}

					// Copy the data to the texture.
					CopyBitmap(tempBitmap, currentTexture);
					_internalTextures.Add(currentTexture);
				}

				if (FontChanged != null)
				{
				    FontChanged(this, EventArgs.Empty);
				}
			}
			finally
			{
				Win32API.RestoreActiveObject();

				if (_charBitmap != null)
				{
					_charBitmap.Dispose();
					_charBitmap = null;
				}

				if (stringFormat != null)
				{
					stringFormat.Dispose();
				}

				if (drawFormat != null)
				{
					drawFormat.Dispose();
				}

				if (tempBitmap != null)
				{
					tempBitmap.Dispose();
				}

				if (newFont != null)
				{
					newFont.Dispose();
				}

				if (graphics != null)
				{
					graphics.Dispose();
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFont"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the font.</param>
		/// <param name="settings">Settings to apply to the font.</param>
		internal GorgonFont(GorgonGraphics graphics, string name, GorgonFontSettings settings)
			: base(name)
		{
			_internalTextures = new List<IDisposable>();

			Graphics = graphics;
			Settings = settings;
			Glyphs = new GorgonGlyphCollection(null);
			_textureSettings = new GorgonTexture2DSettings
				{
				Width = Settings.TextureSize.Width,
				Height = Settings.TextureSize.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				ArrayCount = 1,
				IsTextureCube = false,
				MipCount = 1,
				Multisampling = new GorgonMultisampling(1, 0),
				Usage = BufferUsage.Default,
				ShaderViewFormat = BufferFormat.Unknown
			};
			KerningPairs = new Dictionary<GorgonKerningPair, int>();
		}

        /// <summary>
        /// Finalizes an instance of the <see cref="GorgonFont"/> class.
        /// </summary>
        ~GorgonFont()
        {
            Dispose(false);
        }
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				Graphics.RemoveTrackedObject(this);

				if (_charBitmap != null)
				{
					_charBitmap.Dispose();
				}

				Glyphs = null;

				if (_internalTextures != null)
				{
					foreach (var texture in _internalTextures)
					{
						texture.Dispose();
					}
					_internalTextures.Clear();
				}
			}

			IsDisposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}