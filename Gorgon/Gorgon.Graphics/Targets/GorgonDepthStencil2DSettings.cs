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
// Created: Saturday, June 8, 2013 4:14:19 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for a 2D depth/stencil buffer.
	/// </summary>
	public class GorgonDepthStencil2DSettings
		: IDepthStencilSettings
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil2DSettings"/> class.
		/// </summary>
		public GorgonDepthStencil2DSettings()
		{
			Format = BufferFormat.Unknown;
			TextureFormat = BufferFormat.Unknown;
			ArrayCount = 1;
			MipCount = 1;
			AllowShaderView = false;
			Multisampling = GorgonMultisampling.NoMultiSampling;
		}
		#endregion

		#region IDepthStencilSettings Members
		/// <summary>
		/// Property to set or return the texture format for the depth/stencil buffer texture.
		/// </summary>
		/// <remarks>
		/// If the <see cref="AllowShaderView" /> setting is set to <b>true</b>, then this value needs to be set to a typeless format.  This is because
		/// a depth/stencil buffer is not capable of having a format that is not one of the depth formats.
		/// <para>If this value is set to Unknown, then an exception will be thrown when trying to create the depth/stencil buffer.</para>
		/// <para>This value is only applicable on video devices that have a feature level of SM4 or better if no multisampling is present on the depth/stencil buffer.
		/// If the depth/stencil buffer is multisampled then a video device with a feature level of SM4_1 or better is required.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat TextureFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow shader views on the depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// Set this value to <b>true</b> if depth buffer needs to be bound to the shader.
		/// <para>This value is only applicable on video devices that have a feature level of SM4 or better.</para>
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool AllowShaderView
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the flags used for the default depth/stencil view.
        /// </summary>
        /// <remarks>
        /// Use this to determine how the default depth/stencil view is bound to the pipeline.
        /// <para>A value other than None requires a video device with a feature level of SM5 or better.</para>
        /// <para>The default value is None.</para>
        /// </remarks>
        public DepthStencilViewFlags DefaultDepthStencilViewFlags
        {
            get;
            set;
        }
        #endregion

		#region ITextureSettings Members
		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <remarks>This value is not used with depth/stencil buffers.  It will always return Unknown.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property is made.</exception>
		BufferFormat ITextureSettings.ShaderViewFormat
		{
			get
			{
				return BufferFormat.Unknown; 
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow an unordered access view of the texture for the depth/stencil buffer.
		/// </summary>
		/// <remarks>This value is not supported with depth/stencil buffers.  This value will always return <b>false</b>.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property is made.</exception>
		bool ITextureSettings.AllowUnorderedAccessViews
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
		/// Property to set or return whether this depth/stencil buffer uses a cube texture.
		/// </summary>
		/// <remarks>
		/// When setting this value to <b>true</b>, ensure that the <see cref="Gorgon.Graphics.IImageSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// Note that multisampled resources cannot have sub resources (e.g. mipmaps), so the <see cref="Gorgon.Graphics.IImageSettings.MipCount">MipCount</see> should be set to 1.
		/// <para>The default value is a count of 1, and a quality of 0 (no multisampling).</para>
		/// </remarks>
		public GorgonMultisampling Multisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the texture.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property is made.</exception>
		/// <remarks>This value is not supported on depth/stencil buffers. This value will always return Default.</remarks>
		BufferUsage ITextureSettings.Usage
		{
			get
			{
				return BufferUsage.Default;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region IImageSettings Members
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		public ImageType ImageType
		{
			get
			{
				return ImageType.Image2D;
			}
		}

		/// <summary>
		/// Property to set or return the width of the depth/stencil buffer.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the depth/stencil buffer.
		/// </summary>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property is made.</exception>
		/// <remarks>This value does not apply to depth/stencil buffers.  This value will always return 1.</remarks>
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
		/// Property to set or return the format of the depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// This value must be set to one of the depth formats (D32_Float_S8X24_UInt, D32_Float, D24_UIntNormal_S8_UInt, D16_UIntNormal).  Any other format type will raise an exception.
		/// <para>The defaut value is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip map levels in the texture used by the depth/stencil buffer.
		/// </summary>
		/// <remarks>The default value is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the size of the depth/stencil is a power of 2 or not.
		/// </summary>
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0) &&
				       ((Height == 0) || (Height & (Height - 1)) == 0);
			}
		}
		
		/// <summary>
		/// Property to set or return the number of images there are in the texture array used by the depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// The default value is 1.
		/// </remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Function to clone these image settings.
		/// </summary>
		/// <returns>
		/// A clone of the image settings.
		/// </returns>
		public IImageSettings Clone()
		{
			return new GorgonDepthStencil2DSettings
				{
					AllowShaderView = AllowShaderView,
					ArrayCount = ArrayCount,
					Format = Format,
					Width = Width,
					Height = Height,
					IsTextureCube = IsTextureCube,
					MipCount = MipCount,
					Multisampling = Multisampling,
					TextureFormat = TextureFormat
				};
		}
		#endregion
    }
}