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
// Created: Wednesday, June 5, 2013 7:35:32 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for defining a 2D render target.
	/// </summary>
	public class GorgonRenderTarget2DSettings
		: IRenderTargetTextureSettings
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of render target.
		/// </summary>
		public RenderTargetType RenderTargetType
		{
			get
			{
				return RenderTargetType.Target2D;
			}
		}

		/// <summary>
		/// Property to set or return the format of the backing texture for the render target.
		/// </summary>
		/// <remarks>If this value is Unknown, then it will use the format from <see cref="Gorgon.Graphics.GorgonTexture2DSettings.Format">Format</see>.
		/// <para>If both the Format and this parameter is Unknown, an exception will be raised.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat TextureFormat
		{
			get;
			set;
		}

	
		/// <summary>
		/// Property to set or return the default depth and/or stencil buffer format.
		/// </summary>
		/// <remarks>Setting this value to Unknown will create the target without a depth buffer.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat DepthStencilFormat
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget2DSettings"/> class.
		/// </summary>
		public GorgonRenderTarget2DSettings()
		{
			ArrayCount = 1;
			MipCount = 1;
			Format = BufferFormat.Unknown;
			TextureFormat = BufferFormat.Unknown;
			DepthStencilFormat = BufferFormat.Unknown;
			ShaderViewFormat = BufferFormat.Unknown;
			Multisampling = GorgonMultisampling.NoMultiSampling;
			AllowUnorderedAccessViews = false;
		}
		#endregion

		#region ITextureSettings Members
		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <remarks>
		/// This changes how the render target is sampled/viewed in a shader.  When this value is set to Unknown, then default view uses the render target <see cref="Format">Format</see>.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ShaderViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow an unordered access view of this render target.
		/// </summary>
		/// <remarks>
		/// This allows the render target to be accessed via an unordered access view in a shader.
		/// <para>Render targets using an unordered access view can only use a typed (e.g. int, uint, float) format that belongs to the same group as the format assigned to the <see cref="TextureFormat">backing texture</see>,
		/// or R32_UInt/Int/Float (but only if the texture format is 32 bit).  Any other format will raise an exception.  Note that if the format is not set to R32_UInt/Int/Float,
		/// then write-only access will be given to the UAV.</para>
		/// <para>To check to see if a format is supported for UAV, use the <see cref="Gorgon.Graphics.GorgonVideoDevice.SupportsUnorderedAccessViewFormat">GorgonVideoDevice.SupportsUnorderedAccessViewFormat</see>
		/// Function to determine if the format is supported.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccessViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this render target has a cube texture for its backing store.
		/// </summary>
		/// <remarks>
		/// When setting this value to TRUE, ensure that the <see cref="Gorgon.Graphics.IImageSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the render target.
		/// </summary>
		/// <remarks>Multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="Gorgon.Graphics.IImageSettings.MipCount">MipCount</see> should be set to 1.
		/// <para>The default is a count of 1 and a quality of 0 (no multisampling).</para>
		/// </remarks>
		public GorgonMultisampling Multisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the render target.
		/// </summary>
		/// <remarks>For render targets, this value is always Default.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set the value is made.</exception>
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
		/// Property to return the type of render target data.
		/// </summary>
		public ImageType ImageType
		{
			get
			{
				return IsTextureCube ? ImageType.ImageCube : ImageType.Image2D;
			}
		}

		/// <summary>
		/// Property to set or return the width of a render target.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a render target.
		/// </summary>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a render target.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this value is made.</exception>
		/// <remarks>
		/// This applies to 3D images only.  This parameter will always return 1.
		/// </remarks>
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
		/// Property to set or return the pixel layout of the default render target view.
		/// </summary>
		/// <remarks>This value must not be set to Unknown if the <see cref="TextureFormat"/> is set to Unknown.  Having both values set to unknown will raise an exception.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip map levels in the render target backing texture.
		/// </summary>
		/// <remarks>The default value is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the size of the render target is a power of 2 or not.
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
		/// Property to set or return the number of images there are in the render target backing texture.
		/// </summary>
		/// <remarks>The default value is 1.</remarks>
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
			return new GorgonRenderTarget2DSettings
				{
					Width = Width,
					Height = Height,
					AllowUnorderedAccessViews = AllowUnorderedAccessViews,
					ArrayCount = ArrayCount,
					DepthStencilFormat = DepthStencilFormat,
					Format = Format,
					IsTextureCube = IsTextureCube,
					MipCount = MipCount,
					Multisampling = Multisampling,
					ShaderViewFormat = ShaderViewFormat,
					TextureFormat = TextureFormat
				};
		}
		#endregion
	}
}