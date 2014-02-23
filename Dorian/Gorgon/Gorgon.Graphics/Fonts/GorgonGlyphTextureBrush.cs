#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, October 12, 2013 10:28:27 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A brush used to draw glyphs using a texture.
	/// </summary>
	/// <remarks>The texture used by this brush is a System.Drawing.Image object and not a Gorgon texture.</remarks>
	public class GorgonGlyphTextureBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType
		{
			get
			{
				return GlyphBrushType.Texture;
			}
		}

		/// <summary>
		/// Property to set or return the wrapping mode for the gradient fill.
		/// </summary>
		public WrapMode WrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the region for a single gradient run.
		/// </summary>
		/// <remarks>This value is in texture coordinates.  To use image coordinates, use one of the <see cref="GorgonTexture2D.ToTexel(SlimMath.Vector2)"/> functions to convert.</remarks>
		public RectangleF? TextureRegion
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the texture to apply to the brush.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this brush to the equivalent GDI+ brush type.
		/// </summary>
		/// <returns>
		/// The GDI+ brush type for this object.
		/// </returns>
		internal override Brush ToGDIBrush()
		{
			if (Texture == null)
			{
				return null;
			}

			Image[] gdiImage = GorgonGDIImageConverter.CreateGDIImagesFromTexture(Texture);

			if (gdiImage.Length == 0)
			{
				return null;
			}

			// Only use the first level.
			for (int i = 1; i < gdiImage.Length; ++i)
			{
				gdiImage[i].Dispose();
			}

			var textureRect = new RectangleF(0, 0, Texture.Settings.Width, Texture.Settings.Height);
			var imageRect = TextureRegion != null ? Texture.ToPixel(TextureRegion.Value) : textureRect;

			imageRect = RectangleF.Intersect(textureRect, imageRect);
			
			if (imageRect == RectangleF.Empty)
			{
				imageRect = textureRect;
			}

			return new TextureBrush(gdiImage[0], imageRect)
			       {
				       WrapMode = WrapMode
			       };
		}

		/// <summary>
		/// Function to write the brush elements out to a chunked file.
		/// </summary>
		/// <param name="chunk">Chunk writer used to persist the data.</param>
		internal override void Write(GorgonChunkWriter chunk)
		{
			// We have no texture.
			if (Texture == null)
			{
				return;
			}

			chunk.Begin("BRSHDATA");
			chunk.Write(BrushType);
			chunk.Write(WrapMode);

			chunk.WriteRectangle(TextureRegion != null ? TextureRegion.Value : new RectangleF(0, 0, 1, 1));
			chunk.WriteString(Texture.Name);

			long streamPosition = chunk.BaseStream.Position;

			chunk.WriteInt32(0);		

			Texture.Save(chunk.BaseStream, new GorgonCodecPNG());

			long size = chunk.BaseStream.Position - streamPosition;

			chunk.BaseStream.Position = streamPosition;
			chunk.WriteInt32((int)size);
			chunk.BaseStream.Position += size;

			chunk.End();
		}

		/// <summary>
		/// Function to read the brush elements in from a chunked file.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		/// <param name="chunk">Chunk reader used to read the data.</param>
		internal override void Read(GorgonGraphics graphics, GorgonChunkReader chunk)
		{
			WrapMode = chunk.Read<WrapMode>();

			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}

			TextureRegion = chunk.ReadRectangleF();
			string textureName = chunk.ReadString();
			int size = chunk.ReadInt32();

			// Attempt to load the image from the textures that are already loaded.
			Texture = graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
				        .FirstOrDefault(item => string.Equals(item.Name, textureName, StringComparison.OrdinalIgnoreCase));

			if (Texture == null)
			{
				Texture = graphics.Textures.FromStream<GorgonTexture2D>(textureName, chunk.BaseStream, size, new GorgonCodecPNG());
			}
			else
			{
				chunk.SkipBytes(size);
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphTextureBrush"/> class.
		/// </summary>
		internal GorgonGlyphTextureBrush()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
		/// </summary>
		/// <param name="textureImage">The texture to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="textureImage"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonGlyphTextureBrush(GorgonTexture2D textureImage)
		{
			if (textureImage == null)
			{
				throw new ArgumentNullException("textureImage");	
			}

			Texture = textureImage;
		}
		#endregion
	}
}
