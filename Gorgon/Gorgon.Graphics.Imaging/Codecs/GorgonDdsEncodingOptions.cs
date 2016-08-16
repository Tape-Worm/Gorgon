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
// Created: August 16, 2016 3:08:12 PM
// 
#endregion

using Gorgon.Configuration;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Options used when decoding an image from a stream as a DDS file.
	/// </summary>
	public class GorgonDdsEncodingOptions
		: IGorgonImageCodecEncodingOptions
	{
		#region Properties.

		/// <summary>
		/// Property to set or return whether all frames in an image array should be persisted.
		/// </summary>
		/// <remarks>
		/// This flag only applies to codecs that support multiple image frames.  For all other codec types, this flag will be ignored.
		/// </remarks>
		bool IGorgonImageCodecEncodingOptions.SaveAllFrames
		{
			get
			{
				return true;
			}
			set
			{
				// Intentionally left blank.
			}
		}

		/// <summary>
		/// Property to return the list of options available to the codec.
		/// </summary>
		public IGorgonOptionBag Options
		{
			get;
		}

		/// <summary>
		/// Property to set or return flags used when encoding a file as a legacy DDS format specification.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Older versions of the DDS format specification (or just bugs in general) have made files written with those specifications unreadable by a standardized DDS reader without extra processing. 
		/// These flags define how to handle a legacy encoded file.
		/// </para>
		/// <para>
		/// The default value is <see cref="DdsLegacyFlags.None"/>.
		/// </para>
		/// </remarks>
		public DdsLegacyFlags LegacyFormatConversionFlags
		{
			get
			{
				return Options.GetOptionValue<DdsLegacyFlags>(nameof(LegacyFormatConversionFlags));
			}
			set
			{
				Options.SetOptionValue(nameof(LegacyFormatConversionFlags), value);
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDdsEncodingOptions"/> class.
		/// </summary>
		public GorgonDdsEncodingOptions()
		{
			Options = new GorgonOptionBag(new []
			                              {
				                              GorgonOption.CreateOption(nameof(LegacyFormatConversionFlags), DdsLegacyFlags.None)
			                              });
		}
		#endregion
	}
}
