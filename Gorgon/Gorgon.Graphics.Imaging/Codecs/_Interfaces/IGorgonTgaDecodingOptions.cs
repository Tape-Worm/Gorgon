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
// Created: August 15, 2016 11:30:06 PM
// 
#endregion

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Provides options used when decoding a <see cref="IGorgonImage"/> using the <see cref="GorgonCodecTga"/> object.
	/// </summary>
	public interface IGorgonTgaDecodingOptions
		: IGorgonImageCodecDecodingOptions
	{
		/// <summary>
		/// Property to set or return whether the to force alpha values of 0 in the image to be fully opaque.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some TGA encoded images write out 32 bit images with an alpha value of 0 even though the image has fully opaque color data. This causes an image to appear to be completely transparent when it 
		/// shouldn't be. Use this flag to force the image to set the alpha channel to fully opaque in such cases.
		/// </para>
		/// <para>
		/// Note that this only works on images where all the alpha values are set to 0. If there is a mix of alpha values in the image, then this option will have no effect.
		/// </para>
		/// <para>
		/// The default value is <b>true</b>.
		/// </para>
		/// </remarks>
		bool SetZeroAlphaAsOpaque
		{
			get;
			set;
		}
	}
}
