﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 28, 2016 10:38:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Configuration;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Options used when encoding an image to a stream as a PNG file..
	/// </summary>
	public sealed class GorgonCodecPngEncodingOptions
		: IGorgonPngEncodingOptions
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether all frames in an image array should be persisted.
		/// </summary>
		/// <remarks>
		/// This flag is not supported by this codec and will always return <b>false</b>.
		/// </remarks>
		bool IGorgonImageCodecEncodingOptions.SaveAllFrames
		{
			get
			{
				return false;
			}
			set
			{
				// Intentionally left blank.
			}
		}

		/// <summary>
		/// Property to set or return the horizontal dots-per-inch for the encoded image.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This information is metadata only, no action is taken with this value.
		/// </para>
		/// <para>
		/// The default value is 72.
		/// </para>
		/// </remarks>
		public double DpiX
		{
			get
			{
				return Options.GetOption<double>(nameof(DpiX));
			}
			set
			{
				Options.SetOption(nameof(DpiX), value);
			}
		}

		/// <summary>
		/// Property to set or return the vertical dots-per-index for the encoded image.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This information is metadata only, no action is taken with this value.
		/// </para>
		/// <para>
		/// The default value is 72.
		/// </para>
		/// </remarks>
		public double DpiY
		{
			get
			{
				return Options.GetOption<double>(nameof(DpiY));
			}
			set
			{
				Options.SetOption(nameof(DpiY), value);
			}
		}

		/// <summary>
		/// Property to set or return whether to use interlacing when encoding an image as a PNG file.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		public bool Interlacing
		{
			get
			{
				return Options.GetOption<bool>(nameof(Interlacing));
			}
			set
			{
				Options.SetOption(nameof(Interlacing), value);
			}
		}

		/// <summary>
		/// Property to set or return the type of filter to use when when compressing the PNG file.
		/// </summary>
		/// <remarks>
		/// The default value is <see cref="PNGFilter.DontCare"/>.
		/// </remarks>
		public PNGFilter Filter
		{
			get
			{
				return Options.GetOption<PNGFilter>(nameof(Filter));
			}
			set
			{
				Options.SetOption(nameof(Filter), value);
			}
		}

		/// <summary>
		/// Property to set or return the type of <see cref="ImageDithering"/> to use.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This flag is used to determine which type of dithering algorithm should be used when converting the bit depth for a pixel format to a lower bit depth. If the pixel format of the image is supported 
		/// natively by the codec, then this value will be ignored.
		/// </para> 
		/// <para> 
		/// With dithering applied, the image will visually appear closer to the original by using patterns to simulate a greater number of colors.
		/// </para>
		/// <para>
		/// The default value is <see cref="ImageDithering.None"/>.
		/// </para>
		/// </remarks>
		public ImageDithering Dithering
		{
			get
			{
				return Options.GetOption<ImageDithering>(nameof(Dithering));
			}
			set
			{
				Options.SetOption(nameof(Dithering), value);
			}
		}

		/// <summary>
		/// Property to return the list of options available to the codec.
		/// </summary>
		public IGorgonOptionBag Options
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecPngEncodingOptions"/> class.
		/// </summary>
		public GorgonCodecPngEncodingOptions()
		{
			Options = new GorgonOptionBag(new Dictionary<string, Tuple<object, Type>>
			                              {
				                              {
					                              nameof(Dithering), new Tuple<object, Type>(ImageDithering.None, typeof(ImageDithering))
				                              },
				                              {
					                              nameof(Filter), new Tuple<object, Type>(PNGFilter.DontCare, typeof(PNGFilter))
				                              },
				                              {
					                              nameof(Interlacing), new Tuple<object, Type>(false, typeof(bool))
				                              },
				                              {
					                              nameof(DpiX), new Tuple<object, Type>(72.0, typeof(double))
				                              },
				                              {
					                              nameof(DpiY), new Tuple<object, Type>(72.0, typeof(double))
				                              }
			                              });
		}
		#endregion
	}
}