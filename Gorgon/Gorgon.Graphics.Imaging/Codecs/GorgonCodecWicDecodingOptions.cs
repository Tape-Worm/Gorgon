#region MIT
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
// Created: June 29, 2016 10:46:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Configuration;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Common options for WIC (Windows imaging component) decoding.
	/// </summary>
	public class GorgonCodecWicDecodingOptions
		: IGorgonCodecWicDecodingOptions
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the type of dithering to use if the codec needs to reduce the bit depth for a pixel format.
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
		/// Property to set or return flags used to determine how to handle bit depth conversion for specific formats.
		/// </summary>
		/// <remarks>
		/// The default value is <see cref="WICFlags.None"/>.
		/// </remarks>
		public WICFlags Flags
		{
			get
			{
				return Options.GetOption<WICFlags>(nameof(Flags));
			}

			set
			{
				Options.SetOption(nameof(Flags), value);
			}
		}

		/// <summary>
		/// Property to set or return whether to read all frames from an image.
		/// </summary>
		/// <remarks>
		/// This property always returns <b>false</b> for codecs that use these options.
		/// </remarks>
		bool IGorgonImageCodecDecodingOptions.ReadAllFrames
		{
			get
			{
				return false;
			}
			set
			{
				// Intentionally left empty.
			}
		}

		/// <summary>
		/// Property to return the list of options available to the codec.
		/// </summary>
		public IGorgonOptionBag Options
		{
			get;
			protected set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWicDecodingOptions"/> class.
		/// </summary>
		public GorgonCodecWicDecodingOptions()
		{
			Options = new GorgonOptionBag(new Dictionary<string, Tuple<object, Type>>
			                              {
				                              {
					                              nameof(Flags), new Tuple<object, Type>(WICFlags.None, typeof(WICFlags))
				                              },
				                              {
					                              nameof(Dithering), new Tuple<object, Type>(ImageDithering.None, typeof(ImageDithering))
				                              }
			                              });
		}
		#endregion
	}
}
