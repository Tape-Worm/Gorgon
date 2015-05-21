#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, January 20, 2015 12:39:46 AM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Provides an interface to automatically clip a sprite region from a texture.
	/// </summary>
	sealed class AutoClipper
	{
		#region Value Types.
		/// <summary>
		/// Stores a span of lines that form the boundaries of where to clip.
		/// </summary>
		private struct ClipSpan
		{
			/// <summary>
			/// Starting point for the clip span.
			/// </summary>
			public int Start;
			/// <summary>
			/// End point for the clip span.
			/// </summary>
			public int End;
			/// <summary>
			/// Vertical position for the clip span.
			/// </summary>
			public int Y;
		}
		#endregion

		#region Variables.
		// The starting point for discovery.
		private Point _startPoint;
		// Row stride for the texture.
		private int _stride;
		// Format of the texture.
		private BufferFormat _format;
		// The number of bytes per pixel.
		private int _bytesPerPixel;
		// The width of the texture.
		private int _textureWidth;
		// A list of pixels that have been checked by the clipper.
		private bool[] _pixels;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use the alpha component of the mask value.
		/// </summary>
		public bool UseAlpha
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use the red, green, and blue components of the mask value.
		/// </summary>
		public bool UseRGB
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the mask color to use when searching out a sprite.
		/// </summary>
		public Color MaskColor
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the pixel value is greater the same as the mask value at the requested coordinates.
		/// </summary>
		/// <param name="data">Point to the buffer holding the texture data.</param>
		/// <param name="x">Horizontal position to sample.</param>
		/// <param name="y">Vertical position to sample.</param>
		/// <returns>TRUE if the mask value is the same, FALSE if not.</returns>
		private unsafe bool IsMaskValue(byte* data, int x, int y)
		{
			byte* pixelPtr = (data + (y * _stride) + (x * _bytesPerPixel));

			if ((UseAlpha) && (!UseRGB))
			{
				switch (_format)
				{
					case BufferFormat.B8G8R8A8_UIntNormal:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_UIntNormal:
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_IntNormal:
					case BufferFormat.R8G8B8A8_Int:
						return *(pixelPtr + 3) == MaskColor.A;
					default:
						throw new GorgonException(GorgonResult.CannotRead, Resources.GORSPR_ERR_CANNOT_AUTO_CLIP);
				}
			}

			uint encodedValue;
			uint pixelValue;

			switch (_format)
			{
				case BufferFormat.B8G8R8A8_UIntNormal:
				case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					encodedValue = ((UseAlpha ? ((uint)MaskColor.A << 24) : 0xff000000) | ((uint)MaskColor.R << 16) | ((uint)MaskColor.G << 8) | (MaskColor.B));
					pixelValue = *((uint*)pixelPtr);

					if (!UseAlpha)
					{
						pixelValue = pixelValue & 0xFFFFFF | 0xFF000000;
					}

					return pixelValue == encodedValue;
				case BufferFormat.R8G8B8A8_UIntNormal:
				case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
				case BufferFormat.R8G8B8A8_IntNormal:
				case BufferFormat.R8G8B8A8_Int:
					encodedValue = ((UseAlpha ? ((uint)MaskColor.A << 24) : 0xff000000) | ((uint)MaskColor.B << 16) | ((uint)MaskColor.G << 8) | (MaskColor.R));
					pixelValue = *((uint*)pixelPtr);

					if (!UseAlpha)
					{
						pixelValue = pixelValue & 0xFFFFFF | 0xFF000000;
					}

					return pixelValue == encodedValue;
				default:
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORSPR_ERR_CANNOT_AUTO_CLIP);
			}
		}

		/// <summary>
		/// Function to record a span used to determine the size of the sprite extents.
		/// </summary>
		/// <param name="data">Point to the image data.</param>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical postion.</param>
		/// <returns>A new span value.</returns>
		private unsafe ClipSpan GetSpan(byte* data, int x, int y)
		{
			int left = x;
			int right = x;
			int index = (y * _textureWidth) + x;

			// Get starting point of the span by seeking the left most edge.
			while (true)
			{
				left--;
				_pixels[index] = true;

				index--;

				if ((left < 0) || (IsMaskValue(data, left, y) || (_pixels[index])))
				{
					break;
				}
			}
			left++;

			// Reset the index into the pixel buffer.
			index = (y * _textureWidth) + x;

			// Get the ending point of the span by seeking the right most edge.
			while (true)
			{
				right++;
				_pixels[index] = true;

				index++;

				if ((right >= _textureWidth) || (IsMaskValue(data, right, y)) || (_pixels[index]))
				{
					break;
				}
			}

			right--;

			return new ClipSpan
			       {
				       Start = left,
					   End = right,
					   Y = y
			       };
		}

		/// <summary>
		/// Function to clip the sprite region from the texture.
		/// </summary>
		/// <param name="texture">Texture containing the image data to clip.</param>
		/// <returns>A rectangle for the sprite.  Or an empty rectangle if no sprite was selected.</returns>
		public unsafe Rectangle Clip(GorgonTexture2D texture)
		{
			GorgonImageData imageData = null;

			try
			{
				// Constrain our mouse to the texture.
				_startPoint.X = _startPoint.X.Max(0).Min(texture.Settings.Width - 1);
				_startPoint.Y = _startPoint.Y.Max(0).Min(texture.Settings.Height - 1);

				imageData = GorgonImageData.CreateFromTexture(texture);

				_stride = imageData.Buffers[0].PitchInformation.RowPitch;
				_format = imageData.Settings.Format;
				_bytesPerPixel = GorgonBufferFormatInfo.GetInfo(_format).SizeInBytes;
				_textureWidth = imageData.Settings.Width;

				// Get a pointer to the buffer.
				byte* bytePtr = (byte*)imageData.UnsafePointer;

				if (IsMaskValue(bytePtr, _startPoint.X, _startPoint.Y))
				{
					return Rectangle.Empty;
				}

				Queue<ClipSpan> clipSpans = new Queue<ClipSpan>();

				_pixels = new bool[imageData.Settings.Width * imageData.Settings.Height];

				int left = _startPoint.X;
				int top = _startPoint.Y;
				int right = left;
				int bottom = top;

				// Get the initial span from our starting point and add it to our queue.
				ClipSpan span = GetSpan(bytePtr, left, top);
				clipSpans.Enqueue(span);

				while (clipSpans.Count > 0)
				{
					// Take the span off the queue.
					span = clipSpans.Dequeue();

					// Find the next vertical span above and below the current span.
					int west = span.Start;
					int east = span.End;
					int north = span.Y - 1;
					int south = span.Y + 1;

					// Check each pixel between the start and end of the upper and lower spans.
					for (int x = west; x <= east; ++x)
					{
						int pixelindex = _textureWidth * north + x;

						if ((span.Y > 0) && (!IsMaskValue(bytePtr, x, north)) && (!_pixels[pixelindex]))
						{
							clipSpans.Enqueue(GetSpan(bytePtr, x, north));
						}

						pixelindex = _textureWidth * south + x;

						if ((span.Y >= imageData.Settings.Height - 1) || (IsMaskValue(bytePtr, x, south)) || (_pixels[pixelindex]))
						{
							continue;
						}

						clipSpans.Enqueue(GetSpan(bytePtr, x, south));
					}

					// Update the boundaries.
					left = west.Min(left);
					right = (east + 1).Max(right);
					top = (north + 1).Min(top);
					bottom = south.Max(bottom);
				}

				return Rectangle.FromLTRB(left, top, right, bottom);
			}
			finally
			{
				if (imageData != null)
				{
					imageData.Dispose();
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoClipper"/> class.
		/// </summary>
		/// <param name="startPoint">The starting point for searching.</param>
		public AutoClipper(Point startPoint)
		{
			_startPoint = startPoint;
			UseAlpha = true;
			MaskColor = GorgonColor.Transparent;
		}
		#endregion
	}
}
