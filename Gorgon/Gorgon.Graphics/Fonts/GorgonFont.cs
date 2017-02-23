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
using Drawing = System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Gorgon.Graphics
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
	/// <para>
	/// Applications can also persist this font to a <see cref="Stream"/> or file for reuse.
	/// </para>
	/// </remarks>
	public sealed class GorgonFont
		: GorgonNamedObject, IDisposable
	{
		#region Constants.
		// FONTHIGH chunk.
		private const string FontHeightChunk = "FONTHIGH";
		// TEXTURES chunk.
		private const string TextureChunk = "FNTTXTRS";
		// GLYFDATA chunk.
		private const string GlyphDataChunk = "GLYFDATA";
		// KERNPAIR chunk.
		private const string KernDataChunk = "KERNPAIR";

		/// <summary>
		/// FONTINFO chunk.
		/// </summary>
		internal const string FontInfoChunk = "FONTINFO";

		/// <summary>
		/// Header for a Gorgon font file.
		/// </summary>
		public const string FileHeader = "GORFNT10";

        /// <summary>
        /// Default extension for font files.
        /// </summary>
	    public const string DefaultExtension = ".gorFont";
		#endregion

        #region Variables.
		// A list of internal textures created by the font generator.
		private readonly List<GorgonTexture> _internalTextures;
		// The information used to generate the font.
		private GorgonFontInfo _info;
		#endregion

		#region Properties.
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
		/// Function to copy bitmap data to a texture.
		/// </summary>
		/// <param name="bitmap">Bitmap to copy.</param>
		/// <param name="image">Image to receive the data.</param>
		/// <param name="arrayIndex">The index in the bitmap array to copy from.</param>
		private unsafe void CopyBitmap(Drawing.Bitmap bitmap, GorgonImage image, int arrayIndex)
		{
			BitmapData sourcePixels = bitmap.LockBits(new Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			try
			{
				var pixels = (int*)sourcePixels.Scan0.ToPointer();

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

						image.Buffers[0, arrayIndex].Data.Write(destOffset, color.ToABGR());
						offset++;
						destOffset += image.FormatInfo.SizeInBytes;
					}
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
		/// Function to retrieve the built in kerning pairs and advancement information for the font.
		/// </summary>
		/// <param name="graphics">The GDI graphics interface.</param>
		/// <param name="font">The GDI font.</param>
		/// <param name="allowedCharacters">The list of characters available to the font.</param>
		private Dictionary<char, ABC> GetKerningInformation(Drawing.Graphics graphics, Drawing.Font font, IList<char> allowedCharacters)
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

				foreach (var pair in kerningPairs.Where(item => item.KernAmount != 0))
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
			List<char> result = (from character in Info.Characters
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private void GenerateTextures(Dictionary<Drawing.Bitmap, IEnumerable<GlyphInfo>> glyphData)
		{
			var imageSettings = new GorgonImageInfo(ImageType.Image2D, Format.R8G8B8A8_UNorm)
			{
				Width = Info.TextureWidth,
				Height = Info.TextureHeight,
				Depth = 1
			};
			var textureSettings = new GorgonTextureInfo
			{
				Format = Format.R8G8B8A8_UNorm,
				Width = Info.TextureWidth,
				Height = Info.TextureHeight,
				Depth = 1,
				TextureType = TextureType.Texture2D,
				Usage = ResourceUsage.Default,
				Binding = TextureBinding.ShaderResource,
				IsCubeMap = false,
				MipLevels = 1,
				MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling
			};

			GorgonImage image = null;
			GorgonTexture texture = null;
			int arrayIndex = 0;
			int bitmapCount = glyphData.Count;

			try
			{
				// We copy each bitmap into a texture array index until we've hit the max texture array size, and then 
				// we move to a new texture.  This will keep our glyph textures inside of a single texture object until 
				// it is absolutely necessary to change and should improve performance when rendering.
				foreach (KeyValuePair<Drawing.Bitmap, IEnumerable<GlyphInfo>> glyphBitmap in glyphData)
				{
					if ((image == null) || (arrayIndex >= Graphics.VideoDevice.MaxTextureArrayCount))
					{
						textureSettings.ArrayCount = imageSettings.ArrayCount = bitmapCount.Min(Graphics.VideoDevice.MaxTextureArrayCount);
						arrayIndex = 0;

						image?.Dispose();
						image = new GorgonImage(imageSettings);

						texture = image.ToTexture($"GorgonFont_Internal_Texture_{Guid.NewGuid():N}",
												  Graphics,
												  new GorgonImageToTextureInfo
												  {
													  Binding = TextureBinding.ShaderResource,
													  Usage = ResourceUsage.Default,
													  MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling
												  });
						_internalTextures.Add(texture);
					}

					CopyBitmap(glyphBitmap.Key, image, arrayIndex);

					// Send to our texture.
					texture.UpdateSubResource(image.Buffers[0, arrayIndex], null, arrayIndex);

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
				ABC kernData;
				int advance = 0;

				if (kerningData.TryGetValue(glyph.Key, out kernData))
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
			GorgonGlyph defaultGlyph;

			if (!Glyphs.TryGetValue(Info.DefaultCharacter, out defaultGlyph))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Info.DefaultCharacter));
			}

			for (int i = 0; i < line.Length; i++)
			{
				char character = line[i];
				GorgonGlyph glyph;

				if (!Glyphs.TryGetValue(character, out glyph))
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
				int kernAmount;

				if (KerningPairs.TryGetValue(kerning, out kernAmount))
				{
					size += kernAmount;
				}
			}

			return size;
		}

		/// <summary>
		/// Function to read the textures from the font file itself.
		/// </summary>
		/// <param name="fontFile">Font file reader.</param>
		/// <param name="width">The width of the texture, in pixels.</param>
		/// <param name="height">The height of the texture, in pixels.</param>
		private void ReadInternalTextures(GorgonChunkFileReader fontFile, int width, int height)
		{
			IGorgonImage image = null;
			GorgonBinaryReader reader = fontFile.OpenChunk(TextureChunk);

			try
			{
				int textureCount = reader.ReadInt32();

				for (int i = 0; i < textureCount; i++)
				{
					string textureName = reader.ReadString();
					
					image = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, Format.R8G8B8A8_UNorm)
					                        {
						                        ArrayCount = reader.ReadInt32(),
												Depth = 1,
												MipCount = 1,
												Width = width,
												Height = height
					                        });

					// If the texture size is invalid, then the file has been corrupted.
					if (reader.BaseStream.Position >= reader.BaseStream.Length)
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_CORRUPT);
					}

					// Decompress the image data into our image buffer.
					using (var memoryStream = new GorgonDataStream(image.ImageData))
					using (var textureStream = new GZipStream(reader.BaseStream, CompressionMode.Decompress, true))
					{
						textureStream.CopyTo(memoryStream, 80000);
					}

					// Convert to a texture and add that texture to our internal list.
					_internalTextures.Add(image.ToTexture(textureName,
					                                      Graphics,
					                                      new GorgonImageToTextureInfo
					                                      {
						                                      MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling,
						                                      Usage = ResourceUsage.Default,
						                                      Binding = TextureBinding.ShaderResource
					                                      }));
					image.Dispose();
					image = null;
				}

				fontFile.CloseChunk();
			}
			finally
			{
				image?.Dispose();
			}
		}

		/// <summary>
		/// Function to read the kerning pairs for the font, if they exist.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		private void ReadKernPairs(GorgonChunkFileReader fontFile)
		{
			GorgonBinaryReader reader = fontFile.OpenChunk(KernDataChunk);
			
			// Read optional kerning information.
			int kernCount = reader.ReadInt32();
			
			for (int i = 0; i < kernCount; ++i)
			{
				var kernPair = new GorgonKerningPair(reader.ReadChar(), reader.ReadChar());
				KerningPairs[kernPair] = reader.ReadInt32();
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to read the glyphs from the texture.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		private void ReadGlyphs(GorgonChunkFileReader fontFile)
		{
			// Get glyph information.
			GorgonBinaryReader reader = fontFile.OpenChunk(GlyphDataChunk);

			// Read all information for glyphs that don't use textures (whitespace).
			int nonTextureGlyphCount = reader.ReadInt32();
			for (int i = 0; i < nonTextureGlyphCount; ++i)
			{
				Glyphs.Add(new GorgonGlyph(reader.ReadChar(), reader.ReadInt32()));
			}

			// Glyphs are grouped by associated texture.
			int groupCount = reader.ReadInt32();

			for (int i = 0; i < groupCount; i++)
			{
				// Get the name of the texture.
				string textureName = reader.ReadString();
				// Get a count of all the glyphs associated with the texture.
				int glyphCount = reader.ReadInt32();

				// Locate the texture in our texture cache.
				GorgonTexture texture = _internalTextures.FirstOrDefault(item => string.Equals(textureName, item.Name, StringComparison.OrdinalIgnoreCase));

				// The associated texture was not found, thus the file is corrupt.
				if (texture == null)
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));	
				}

				// Read the glyphs for this texture.
				for (int j = 0; j < glyphCount; j++)
				{
					char glyphChar = reader.ReadChar();
					DX.Rectangle glyphRect = new DX.Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
					DX.Point offset = new DX.Point(reader.ReadInt32(), reader.ReadInt32());
					DX.Rectangle outlineRect = new DX.Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
					DX.Point outlineOffset = new DX.Point(reader.ReadInt32(), reader.ReadInt32());
					int advance = reader.ReadInt32();
					int textureIndex = reader.ReadInt32();

					GorgonGlyph glyph;

					// For whitespace, we don't have a texture.
					if (char.IsWhiteSpace(glyphChar))
					{
						glyph = new GorgonGlyph(glyphChar, advance);
					}
					else
					{
						glyph = new GorgonGlyph(glyphChar, advance)
						        {
							        Offset = offset,
							        OutlineOffset = outlineOffset
						        };
					}

					glyph.UpdateTexture(texture, glyphRect, outlineRect, textureIndex);

					Glyphs.Add(glyph);
				}
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to read the glyphs from the texture.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		private void WriteGlyphs(GorgonChunkFileWriter fontFile)
		{
			// Write glyph data.
			GorgonBinaryWriter writer = fontFile.OpenChunk(GlyphDataChunk);

			GorgonGlyph[] nonTextureGlyphs = (from GorgonGlyph glyph in Glyphs
			                                  where glyph.TextureView[0] == null
			                                  select glyph).ToArray();

			// Write all information for glyphs that don't use textures (whitespace).
			writer.Write(nonTextureGlyphs.Length);
			foreach (GorgonGlyph glyph in nonTextureGlyphs)
			{
				writer.Write(glyph.Character);
				writer.Write(glyph.Advance);
			}

			// Write glyphs.
			var textureGlyphs = (from GorgonGlyph glyph in Glyphs
								 let textureView = (GorgonTextureShaderView)glyph.TextureView[0]
								 where textureView != null
								 group glyph by textureView.Texture);
			
			// Glyphs are grouped by associated texture.
			writer.Write(_internalTextures.Count);

			foreach (var glyphGroup in textureGlyphs)
			{
				writer.Write(glyphGroup.Key != null ? glyphGroup.Key.Name : _internalTextures[0].Name);
				writer.Write(glyphGroup.Count());

				foreach (GorgonGlyph glyph in glyphGroup)
				{
					writer.Write(glyph.Character);
					writer.Write(glyph.GlyphCoordinates.Left);
					writer.Write(glyph.GlyphCoordinates.Top);
					writer.Write(glyph.GlyphCoordinates.Width);
					writer.Write(glyph.GlyphCoordinates.Height);
					writer.Write(glyph.Offset.X);
					writer.Write(glyph.Offset.Y);
					writer.Write(glyph.OutlineCoordinates.Left);
					writer.Write(glyph.OutlineCoordinates.Top);
					writer.Write(glyph.OutlineCoordinates.Width);
					writer.Write(glyph.OutlineCoordinates.Height);
					writer.Write(glyph.OutlineOffset.X);
					writer.Write(glyph.OutlineOffset.Y);
					writer.Write(glyph.Advance);
					writer.Write(glyph.TextureIndex);
				}
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to write out textures as internal data in the font file.
		/// </summary>
		/// <param name="fontFile">The font file that is being persisted.</param>
		private void WriteInternalTextures(GorgonChunkFileWriter fontFile)
		{
			GorgonBinaryWriter writer = fontFile.OpenChunk(TextureChunk);

			// Write out how many textures to expect.
			writer.Write(_internalTextures.Count);

			foreach (GorgonTexture texture in _internalTextures)
			{
				writer.Write(texture.Name);

				// Ensure that we know how many array indices there are.
				writer.Write(texture.Info.ArrayCount);

				// Compress and store the backing textures for this font.
				using (IGorgonImage imageData = texture.ToImage())
				using (GorgonDataStream memoryStream = new GorgonDataStream(imageData.ImageData))
				using (GZipStream streamData = new GZipStream(writer.BaseStream, CompressionLevel.Optimal, true))
				{
					memoryStream.CopyTo(streamData, 80000);

					// Ensure the remainder gets pumped into the base stream.
					streamData.Flush();
				}
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to write out the kerning pair information for the font.
		/// </summary>
		/// <param name="fontFile">The font file that is being persisted.</param>
		private void WriteKerningValues(GorgonChunkFileWriter fontFile)
		{
			GorgonBinaryWriter writer = fontFile.OpenChunk(KernDataChunk);	

			writer.Write(KerningPairs.Count);

			foreach (var kerningInfo in KerningPairs)
			{
				writer.Write(kerningInfo.Key.LeftCharacter);
				writer.Write(kerningInfo.Key.RightCharacter);
				writer.Write(kerningInfo.Value);
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to read the font data in from the chunked file.
		/// </summary>
		/// <param name="fontFile">Reader for the chunked file.</param>
		/// <param name="fontInfo">The information used to create the font.</param>
		internal void ReadFont(GorgonChunkFileReader fontFile, GorgonFontInfo fontInfo)
		{
			GorgonBinaryReader reader = fontFile.OpenChunk(FontHeightChunk);

			// Read in information about the font height.
			FontHeight = reader.ReadSingle();
			LineHeight = reader.ReadSingle();
			Ascent = reader.ReadSingle();
			Descent = reader.ReadSingle();
			fontFile.CloseChunk();

			// Read font information.
			ReadInternalTextures(fontFile, fontInfo.TextureWidth, fontInfo.TextureHeight);

			ReadGlyphs(fontFile);

			if (fontFile.Chunks.Contains(KernDataChunk))
			{
				ReadKernPairs(fontFile);
			}

			// Finally, assign the font information to the actual font.
			_info = fontInfo;
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
		public GorgonGlyph GetDefaultGlyph()
		{
			GorgonGlyph glyph;

			if (!TryGetDefaultGlyph(out glyph))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Info.DefaultCharacter));
			}

			return glyph;
		}

		/// <summary>
		/// Function to retrieve the glyph used for the default character assigned in the font <see cref="Info"/>.
		/// </summary>
		/// <returns><b>true</b> if the glyph was found, or <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// The default character is assigned to the <see cref="IGorgonFontInfo.DefaultCharacter"/> property of the <see cref="IGorgonFontInfo"/> type passed to the constructor of the font.
		/// </para>
		/// </remarks>
		/// <seealso cref="IGorgonFontInfo"/>
		public bool TryGetDefaultGlyph(out GorgonGlyph glyph)
		{
			return Glyphs.TryGetValue(Info.DefaultCharacter, out glyph);
		}

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

			GorgonGlyph defaultGlyph;
			var wordText = new StringBuilder(text);

			if (!Glyphs.TryGetValue(Info.DefaultCharacter, out defaultGlyph))
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

				GorgonGlyph glyph;

				if (!Glyphs.TryGetValue(character, out glyph))
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
					int kernValue;
					
					if (KerningPairs.TryGetValue(new GorgonKerningPair(character, wordText[index + 1]), out kernValue))
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

			return new DX.Size2F(lineWidth, FontHeight * lineSpacing);
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

			string formattedText = text.FormatStringForRendering(tabSpaceCount);
			DX.Size2F result = DX.Size2F.Zero;

			if (wordWrapWidth != null)
			{
				formattedText = WordWrap(text, wordWrapWidth.Value);
			}

			string[] lines = formattedText.GetLines();

			if (lines.Length == 0)
			{
				return result;
			}

			if (lineSpacing.EqualsEpsilon(1.0f))
			{
				result.Height = lines.Length * FontHeight;
			}
			else
			{
				// For a modified line spacing, we have to adjust for the last line not being affected by the line spacing.
				result.Height = (lines.Length - 1) * (((FontHeight) * lineSpacing)) + (FontHeight);
			}

			if ((HasOutline) && (useOutline))
			{
				result.Height += Info.OutlineSize * 0.5f;
			}

			// Get width.
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
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">The stream that will receive the font data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
		/// <exception cref="IOException">Thrown when the stream parameter does not allow for writing or seeking.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORGFX_ERR_STREAM_READ_ONLY);
			}

			if (!stream.CanSeek)
			{
				throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
			}

			GorgonChunkFileWriter fontFile = new GorgonChunkFileWriter(stream, FileHeader.ChunkID());

			try
			{
				fontFile.Open();

				GorgonBinaryWriter writer = fontFile.OpenChunk(FontInfoChunk);

				writer.Write(Info.FontFamilyName);
				writer.Write(Info.Size);
				writer.WriteValue(Info.FontHeightMode);
				writer.WriteValue(Info.FontStyle);
				writer.Write(Info.DefaultCharacter);
				writer.Write(string.Join(string.Empty, Info.Characters));
				writer.WriteValue(Info.AntiAliasingMode);
				writer.Write(Info.OutlineColor1.ToARGB());
				writer.Write(Info.OutlineColor2.ToARGB());
				writer.Write(Info.OutlineSize);
				writer.Write(Info.PackingSpacing);
				writer.Write(Info.TextureWidth);
				writer.Write(Info.TextureHeight);
				writer.Write(Info.UsePremultipliedTextures);
				writer.Write(Info.UseKerningPairs);
				fontFile.CloseChunk();

				writer = fontFile.OpenChunk(FontHeightChunk);
				writer.Write(FontHeight);
				writer.Write(LineHeight);
				writer.Write(Ascent);
				writer.Write(Descent);
				fontFile.CloseChunk();

				WriteInternalTextures(fontFile);

				WriteGlyphs(fontFile);

				if (Info.UseKerningPairs)
				{
					WriteKerningValues(fontFile);
				}
			}
			finally
			{
				fontFile.Close();
			}
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">File name and path of the font to save.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		public void Save(string fileName)
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
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream);
			}
			finally			
			{
				stream?.Dispose();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			foreach (GorgonTexture texture in _internalTextures)
			{
				texture?.Dispose();
			}

			_internalTextures.Clear();
			KerningPairs.Clear();
			Glyphs.Clear();
		}

		/// <summary>
		/// Function to create or update the font.
		/// </summary>
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
		internal void GenerateFont()
		{
			Drawing.Bitmap setupBitmap = null;
			Drawing.Graphics graphics = null;
			GdiFontData fontData = default(GdiFontData);
			Dictionary<Drawing.Bitmap, IEnumerable<GlyphInfo>> groupedByBitmap = null;

			try
			{
				// Temporary bitmap used to gather a graphics context.				
				setupBitmap = new Drawing.Bitmap(2, 2, PixelFormat.Format32bppArgb);
				
				// Get a context for the rasterizing surface.
				graphics = Drawing.Graphics.FromImage(setupBitmap);
				graphics.PageUnit = Drawing.GraphicsUnit.Pixel;

				// Build up the information using a GDI+ font.
				fontData = GdiFontData.GetFontData(graphics, Info);

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
				LineHeight = fontData.LineHeight ;
			}
			finally
			{
				graphics?.Dispose();
				setupBitmap?.Dispose();

				fontData?.Dispose();

				if (groupedByBitmap != null)
				{
					foreach (Drawing.Bitmap glyphBitmap in groupedByBitmap.Keys)
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
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		internal GorgonFont(string name, GorgonGraphics graphics)
			: base(name)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			_internalTextures = new List<GorgonTexture>();

			Graphics = graphics;
			Glyphs = new GorgonGlyphCollection();
			KerningPairs = new Dictionary<GorgonKerningPair, int>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFont"/> class.
		/// </summary>
		/// <param name="name">The name of the font.</param>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="info">The information used to generate the font.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or the <paramref name="info"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		internal GorgonFont(string name, GorgonGraphics graphics, IGorgonFontInfo info)
			: this(name, graphics)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			_info = new GorgonFontInfo(info);
		}
		#endregion
	}
}