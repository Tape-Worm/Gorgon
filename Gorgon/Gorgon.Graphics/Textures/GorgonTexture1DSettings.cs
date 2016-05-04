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
// Created: Tuesday, June 4, 2013 7:27:52 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for a 1D texture.
	/// </summary>
	public class GorgonTexture1DSettings
		: ITextureSettings
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1DSettings"/> class.
		/// </summary>
		public GorgonTexture1DSettings()
		{
			Width = 0;
			Format = BufferFormat.Unknown;
			ShaderViewFormat = BufferFormat.Unknown;
			AllowUnorderedAccessViews = false;
			ArrayCount = 1;
			MipCount = 1;
			Usage = BufferUsage.Default;
		}
		#endregion

		#region ITextureSettings Members
		#region Properties.
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		public ImageType ImageType => ImageType.Image1D;

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 2D textures.  This value is always <b>false</b>.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this value is made.</exception>
		bool ITextureSettings.IsTextureCube
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the width of a texture.
		/// </summary>
		/// <value></value>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This applies to 2D and 3D textures only.  This value always returns 1.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this value is made.</exception>
		int IImageSettings.Height
		{
			get
			{
				return 1;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This applies to 3D textures only.  This value always returns 1.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this value is made.</exception>
		int IImageSettings.Depth
		{
			get
			{
				return 1;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a texture from a file, leave this as Unknown to get the file format from the source file.
		/// <para>This sets the format of the texture data. To reinterpret the format of the data inside of a shader, create a new <see cref="Gorgon.Graphics.GorgonTexture1D.GetShaderView(BufferFormat, int, int, int, int)">shader view</see> and assign it to the texture.</para></remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  When this value is set to Unknown, then default view uses the texture format.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ShaderViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow an unordered access view of this texture.
		/// </summary>
		/// <remarks>This allows a texture to be accessed via an unordered access view in a shader.
		/// <para>Textures using an unordered access view can only use a typed (e.g. int, uint, float) format that belongs to the same group as the format assigned to the texture, 
		/// or R32_UInt/Int/Float (but only if the texture format is 32 bit).  Any other format will raise an exception.  Note that if the format is not set to R32_UInt/Int/Float, 
		/// then write-only access will be given to the UAV.</para> 
		/// <para>To check to see if a format is supported for UAV, use the <see cref="Gorgon.Graphics.VideoDevice.SupportsUnorderedAccessViewFormat">GorgonVideoDevice.SupportsUnorderedAccessViewFormat</see> 
		/// Function to determine if the format is supported.</para>
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool AllowUnorderedAccessViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>The default value for this setting is 1.</remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip maps in a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>The default value for this setting is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 2D textures.  This will always returns a count of 1, and a quality of 0 (no multisampling).</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this value is made.</exception>
		GorgonMultiSampleInfo ITextureSettings.Multisampling
		{
			get
			{
				return GorgonMultiSampleInfo.NoMultiSampling;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the usage for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>The default value is Default.</remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		public bool IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0);

		#endregion

		#region Methods.
		/// <summary>
		/// Function to clone the current 1D texture settings.
		/// </summary>
		/// <returns>A clone of the image settings object.</returns>
		public IImageSettings Clone()
		{
			return new GorgonTexture1DSettings
				{
					Width = Width,
					Format = Format,
					ArrayCount = ArrayCount,
					MipCount = MipCount,
					ShaderViewFormat = ShaderViewFormat,
					AllowUnorderedAccessViews = AllowUnorderedAccessViews,
					Usage = Usage
				};
		}
		#endregion
		#endregion
	}
}