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
using Gorgon.Math;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Options used when decoding an image from a stream as a GIF file.
	/// </summary>
	public class GorgonCodecGifDecodingOptions
		: GorgonCodecWicDecodingOptions
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether to read all frames from an animated GIF file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value is ignored if the GIF file contains only a single frame.
		/// </para> 
		/// <para>
		/// The default value is <b>true</b>.
		/// </para>
		/// </remarks>
		public bool ReadAllFrames
		{
			get
			{
				return Options.GetOption<bool>(nameof(ReadAllFrames));
			}
			set
			{
				Options.SetOption(nameof(ReadAllFrames), value);
			}
		}

		/// <summary>
		/// Property to set or return a custom color palette to apply to the GIF file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to define a new palette for the 8 bit indexed image in the GIF file. 
		/// </para>
		/// <para>
		/// This value is ignored when the GIF file has multiple frames of animation.
		/// </para>
		/// <para>
		/// The default value is <b>null</b>.
		/// </para>
		/// </remarks>
		public IList<GorgonColor> Palette
		{
			get
			{
				return Options.GetOption<IList<GorgonColor>>(nameof(Palette));
			}
			set
			{
				Options.SetOption(nameof(Palette), value);
			}
		}

		/// <summary>
		/// Property to set or return the alpha threshold percentage for this codec.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to determine what percentage of alpha channel values should be considered transparent for the GIF.  A value of 0.5f will mean that colors with an alpha component of 128 or less will 
		/// be considered transparent.
		/// </para>
		/// <para>
		/// This value does not apply to GIF files with multiple frames.
		/// </para>
		/// <para>
		/// The default value is 0.0f.
		/// </para>
		/// </remarks>
		public float AlphaThreshold
		{
			get
			{
				return Options.GetOption<float>(nameof(AlphaThreshold));
			}
			set
			{
				Options.SetOption(nameof(AlphaThreshold), value.Max(0).Min(1));
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWicDecodingOptions"/> class.
		/// </summary>
		public GorgonCodecGifDecodingOptions()
		{
			Options = new GorgonOptionBag(new Dictionary<string, Tuple<object, Type>>
			                              {
				                              {
					                              nameof(Flags), new Tuple<object, Type>(WICFlags.None, typeof(WICFlags))
				                              },
				                              {
					                              nameof(Dithering), new Tuple<object, Type>(ImageDithering.None, typeof(ImageDithering))
				                              },
				                              {
					                              nameof(ReadAllFrames), new Tuple<object, Type>(false, typeof(bool))
				                              },
				                              {
					                              nameof(Palette), new Tuple<object, Type>(new List<GorgonColor>(), typeof(IList<GorgonColor>))
				                              },
				                              {
					                              nameof(AlphaThreshold), new Tuple<object, Type>(0.0f, typeof(float))
				                              }
			                              });
		}
		#endregion
	}
}
