#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, March 13, 2012 1:02:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings to describe a texture structure.
	/// </summary>
	public interface ITextureSettings
		: IImageSettings
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the shader view format.
		/// </summary>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  The default value is Unknown.</remarks>
		BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the view uses unordered access.
		/// </summary>
		bool ViewIsUnordered
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <remarks>When setting this value to TRUE, ensure that the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return FALSE.  The default value is FALSE.</para></remarks>
		bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).
		/// <para>Note that multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.MipCount">MipCount</see> should be set to 1.</para>
		/// </remarks>
		GorgonMultisampling Multisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the texture.
		/// </summary>
		/// <remarks>The default value is Default.</remarks>
		BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		ImageFilters FileFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of mipmap filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		ImageFilters FileMipFilter
		{
			get;
			set;
		}
		#endregion
	}
	
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
			ArrayCount = 1;
			MipCount = 1;
			ViewFormat = BufferFormat.Unknown;
			Usage = BufferUsage.Default;
			FileFilter = ImageFilters.None;
			FileMipFilter = ImageFilters.None;
		}
		#endregion

		#region ITextureSettings Members
		#region Properties.
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		public ImageType ImageType
		{
			get
			{
				return ImageType.Image1D;
			}
		}

		/// <summary>
		/// Property to set or return the type of filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of mipmap filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileMipFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When setting this value to TRUE, ensure that the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return FALSE.  The default value is FALSE.</para></remarks>
		bool ITextureSettings.IsTextureCube
		{
			get
			{
				return false;
			}
			set
			{
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
		/// <remarks>This applies to 2D and 3D textures only.</remarks>
		int IImageSettings.Height
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This applies to 3D textures only.</remarks>
		int IImageSettings.Depth
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This sets the format of the texture data.  If you want to change the format of a texture when being sampled in a shader, then set the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ViewFormat">ViewFormat</see> property to anything other than Unknown.</remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format.
		/// </summary>
		/// <value></value>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  The default value is Unknown.</remarks>
		public BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the view uses unordered access.
		/// </summary>
		public bool ViewIsUnordered
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
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
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).</remarks>
		GorgonMultisampling ITextureSettings.Multisampling
		{
			get
			{
				return new GorgonMultisampling(1, 0);
			}
			set
			{
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
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0);
			}
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Settings for a 2D texture.
	/// </summary>
	public class GorgonTexture2DSettings
		: ITextureSettings
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the size of the texture.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size(Width, Height);
			}
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSettings"/> class.
		/// </summary>
		public GorgonTexture2DSettings()
		{
			Width = 0;
			Height = 0;
			Format = BufferFormat.Unknown;
			MipCount = 1;
			ArrayCount = 1;
			Multisampling = new GorgonMultisampling(1, 0);
			ViewFormat = BufferFormat.Unknown;
			Usage = BufferUsage.Default;
			FileFilter = ImageFilters.None;
			FileMipFilter = ImageFilters.None;
		}
		#endregion

		#region ITextureSettings Members
		#region Properties.
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
		/// Property to set or return the type of filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of mipmap filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileMipFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When setting this value to TRUE, ensure that the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return FALSE.  The default value is FALSE.</para></remarks>
		public bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When loading a file, leave as 0 to use the width from the file source.</remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the height from the file source.
		/// <para>This applies to 2D and 3D textures only.</para></remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the width from the depth source.
		/// <para>This applies to 3D textures only.</para></remarks>
		int IImageSettings.Depth
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a texture from a file, leave this as Unknown to get the file format from the source file.
		/// <para>This sets the format of the texture data.  If you want to change the format of a texture when being sampled in a shader, then set the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ViewFormat">ViewFormat</see> property to anything other than Unknown.</para></remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format.
		/// </summary>
		/// <value></value>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  The default value is Unknown.</remarks>
		public BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the view uses unordered access.
		/// </summary>
		public bool ViewIsUnordered
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip maps in a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>To have the system generate mipmaps for you, set this value to 0.  The default value for this setting is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).
		/// <para>Note that multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.MipCount">MipCount</see> should be set to 1.</para>
		/// </remarks>
		public GorgonMultisampling Multisampling
		{
			get;
			set;
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
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0) &&
						((Height == 0) || (Height & (Height - 1)) == 0);
			}
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Settings for a 3D texture.
	/// </summary>
	public class GorgonTexture3DSettings
		: ITextureSettings
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3DSettings"/> class.
		/// </summary>
		public GorgonTexture3DSettings()
		{
			Width = 0;
			Height = 0;
			Depth = 0;
			Format = BufferFormat.Unknown;
			MipCount = 1;
			ViewFormat = BufferFormat.Unknown;
			Usage = BufferUsage.Default;
			FileFilter = ImageFilters.None;
			FileMipFilter = ImageFilters.None;
		}
		#endregion

		#region ITextureSettings Members
		#region Properties.
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		public ImageType ImageType
		{
			get
			{
				return ImageType.Image3D;
			}
		}

		/// <summary>
		/// Property to set or return the type of filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of mipmap filter when loading an image from a stream or file.
		/// </summary>
		/// <remarks>This only applies to textures created when loading an image from a stream or file.  Texture creation methods do not use this.
		/// <para>This defaults to None.</para>
		/// </remarks>
		public ImageFilters FileMipFilter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When setting this value to TRUE, ensure that the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return FALSE.  The default value is FALSE.</para></remarks>
		bool ITextureSettings.IsTextureCube
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the width of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When loading a file, leave as 0 to use the width from the file source.</remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the height from the file source.
		/// <para>This applies to 2D and 3D textures only.</para></remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the width from the depth source.
		/// <para>This applies to 3D textures only.</para></remarks>
		public int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a texture from a file, leave this as Unknown to get the file format from the source file.
		/// <para>This sets the format of the texture data.  If you want to change the format of a texture when being sampled in a shader, then set the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ViewFormat">ViewFormat</see> property to anything other than Unknown.</para></remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format.
		/// </summary>
		/// <value></value>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  The default value is Unknown.</remarks>
		public BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the view uses unordered access.
		/// </summary>
		public bool ViewIsUnordered
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
		int IImageSettings.ArrayCount
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the number of mip maps in a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>To have the system generate mipmaps for you, set this value to 0.  The default value for this setting is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).
		/// <para>Note that multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.MipCount">MipCount</see> should be set to 1.</para>
		/// </remarks>
		GorgonMultisampling ITextureSettings.Multisampling
		{
			get
			{
				return new GorgonMultisampling(1, 0);
			}
			set
			{
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
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0) &&
						((Height == 0) || (Height & (Height - 1)) == 0) &&
						((Depth == 0) || (Depth & (Depth - 1)) == 0);
			}
		}
		#endregion
		#endregion
	}
}
