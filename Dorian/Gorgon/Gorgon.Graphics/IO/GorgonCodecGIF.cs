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
// Created: Thursday, February 14, 2013 9:24:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A codec to handle reading/writing GIF files.
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// <para>The limitations of this codec are as follows:
	/// <list type="bullet">
	///		<item>
	///			<description>Only supports saving as 8 bit indexed only.  Fidelity loss may be dramatic.</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public sealed unsafe class GorgonCodecGIF
		: GorgonCodecWIC
	{
		#region Variables.
		private double _alphaPercent = 0.0;			// Alpha threshold.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether all frames in a multi-frame image should be encoded/decoded or not.
		/// </summary>
		/// <remarks>This property will encode or decode multiple frames from or into an array.  Note that this is only supported on codecs that support multiple frames (e.g. animated Gif).  
		/// Images that do not support multiple frames will ignore this flag.
		/// <para>This property applies to both encoding and decoding of image data.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool UseAllFrames
		{
			get
			{
				return base.CodecUseAllFrames;
			}
			set
			{
				base.CodecUseAllFrames = value;
			}
		}

		/// <summary>
		/// Property to set or return the palette to assign to the 8 bit indexed data for this image.
		/// </summary>		
		/// <remarks>
		/// Use this to alter the color palette for the GIF as it's decoded or encoded.  This array will only support up to 256 indices.  More than 256 indices will 
		/// be ignored.  Set this to NULL (Nothing in VB.Net) to use the palette contained within in the image.
		/// <para>This value does not apply to GIF files with multiple frames.</para>
		/// <para>This property affects both encoding and decoding.</para>
		/// <para>The default value is NULL.</para>
		/// </remarks>
		public IList<GorgonColor> Palette
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the alpha threshold percentage for this codec.
		/// </summary>
		/// <remarks>Use this to determine what percentage of alpha values should be considered transparent for the GIF.  A value of 50 will mean that alpha with values 
		/// of 128 or less will be considered transparent.
		/// <para>This value does not apply to GIF files with multiple frames.</para>
		/// <para>This property affects both encoding and decoding.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public double AlphaThresholdPercent
		{
			get
			{
				return _alphaPercent;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 100)
				{
					value = 100;
				}

				_alphaPercent = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the offset for the frame being decoded.
		/// </summary>
		/// <param name="frame">Frame to decode.</param>
		/// <returns>
		/// The position of the offset.
		/// </returns>
		internal override System.Drawing.Point GetFrameOffset(WIC.BitmapFrameDecode frame)
		{
			System.Drawing.Point offset = System.Drawing.Point.Empty;

			if (frame == null)
			{
				return offset;
			}

			// Get frame offsets.
			var offsetX = frame.MetadataQueryReader.GetMetadataByName("/imgdesc/left");
			var offsetY = frame.MetadataQueryReader.GetMetadataByName("/imgdesc/top");

			if (offsetX != null)
			{
				offset.X = (ushort)offsetX;
			}
			if (offsetY != null)
			{
				offset.Y = (ushort)offsetY;
			}

			return offset;
		}

		/// <summary>
		/// Function to retrieve palette information for indexed images.
		/// </summary>
		/// <param name="wic">The WIC interface.</param>
		/// <param name="decoding">TRUE if decoding, FALSE if not.</param>
		/// <returns>
		/// A tuple containing the palette data, alpha percentage and the type of palette.
		/// </returns>
		internal override Tuple<WIC.Palette, double, WIC.BitmapPaletteType> GetPaletteInfo(GorgonWICImage wic, bool decoding)
		{			
			WIC.Palette palette = null;

			if (Palette == null)
			{
				// If decoding, just return the default, otherwise we'll need to generate from the frame.
				if (decoding)
				{
					return base.GetPaletteInfo(wic, decoding);
				}
				else
				{
					return null;
				}
			}

			// Generate from our custom palette.
			DX.Color4[] paletteColors = new DX.Color4[256];
			int size = paletteColors.Length.Min(Palette.Count);

			for (int i = 0; i < size; i++)
			{
				paletteColors[i] = Palette[i].SharpDXColor4;
			}

			palette = new WIC.Palette(wic.Factory);
			palette.Initialize(paletteColors);

			return new Tuple<WIC.Palette, double, WIC.BitmapPaletteType>(palette, AlphaThresholdPercent, WIC.BitmapPaletteType.Custom);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWIC" /> class.
		/// </summary>
		internal GorgonCodecGIF()
			: base("GIF", "Graphics Interchange Format", new string[] { "gif" })
		{
			SupportedFormats = new[]
			{
				WIC.ContainerFormatGuids.Gif
			};
		}
		#endregion
	}
}
