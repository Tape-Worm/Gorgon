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
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A brush used to draw glyphs using a texture.
	/// </summary>
	/// <remarks>The texture used by this brush is a System.Drawing.Image object and not a Gorgon texture.</remarks>
	public class GorgonGlyphTextureBrush
		: GorgonGlyphBrush, IDisposable
	{
		#region Variables.
		private bool _disposed;			// Flag to indicate that the object was disposed.
		#endregion

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
		public Rectangle? TextureRegion
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the texture to apply to the brush.
		/// </summary>
		public Image Texture
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
			return Texture == null
				       ? null
				       : new TextureBrush(Texture, TextureRegion ?? new RectangleF(0, 0, Texture.Width, Texture.Height))
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
			chunk.Begin("BRSHDATA");
			chunk.Write(BrushType);
			chunk.Write(WrapMode);

			chunk.Write(TextureRegion != null ? TextureRegion.Value : new Rectangle(0, 0, Texture.Width, Texture.Height));

			// Write the dummy size.
			using (var stream = new MemoryStream())
			{
				Texture.Save(stream, ImageFormat.Png);

				stream.Position = 0;
				chunk.Write((int)stream.Length);
				stream.CopyTo(chunk.BaseStream);
			}

			chunk.End();
		}

		/// <summary>
		/// Function to read the brush elements in from a chunked file.
		/// </summary>
		/// <param name="chunk">Chunk reader used to read the data.</param>
		internal override void Read(GorgonChunkReader chunk)
		{
			WrapMode = chunk.Read<WrapMode>();

			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}

			TextureRegion = chunk.ReadRectangle();

			int size = chunk.ReadInt32();
			var buffer = new byte[80000];

			using (var stream = new MemoryStream(size))
			{
				// Copy into the memory stream.
				// GDI+ has some weird issues with streams that 
				// are embedded within other streams.
				while (size > 0)
				{
					if (size >= 80000)
					{
						chunk.ReadRange(buffer);
						stream.Write(buffer, 0, buffer.Length);
					}
					else
					{
						chunk.ReadRange(buffer, 0, size);
						stream.Write(buffer, 0, size);
					}
					size -= 80000;
				}
				stream.Position = 0;
				Texture = Image.FromStream(stream);
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
		/// <param name="textureImage">The image to use as the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="textureImage"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonGlyphTextureBrush(Image textureImage)
		{
			Texture = (Image)textureImage.Clone();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Texture != null)
				{
					Texture.Dispose();
				}
				Texture = null;
			}

			_disposed = true;
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
