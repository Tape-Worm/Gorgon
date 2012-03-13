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
// Created: Monday, February 13, 2012 7:22:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a 1D texture.
	/// </summary>
	public class GorgonTexture1DSettings
		: ITextureSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture1DSettings FromFile = new GorgonTexture1DSettings();
		#endregion

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
		}
		#endregion

		#region ITextureSettings Members
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
		int ITextureSettings.Height
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
		int ITextureSettings.Depth
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
	}

	/// <summary>
	/// A 1 dimension texture object.
	/// </summary>
	/// <remarks>A 1 dimensional texture only has a width.  This is useful as a buffer of linear data in texture format.
	/// <para>This texture type cannot be created by SM2_a_b video devices.</para></remarks>
	public class GorgonTexture1D
		: GorgonTexture
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public new GorgonTexture1DSettings Settings
		{
			get
			{
				return (GorgonTexture1DSettings)base.Settings;
			}
			private set
			{
				base.Settings = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read image data from a stream.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="filter">Filter to apply to the image.</param>
		/// <param name="mipFilter">Mip map filter to apply to the mip levels of the image.</param>
		protected internal void Initialize(byte[] imageData, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageLoadInformation imageInfo = new D3D.ImageLoadInformation();

			if (Settings.Usage != BufferUsage.Staging)
				imageInfo.BindFlags = D3D.BindFlags.ShaderResource;
			else
				imageInfo.BindFlags = D3D.BindFlags.None;

			switch (Settings.Usage)
			{
				case BufferUsage.Staging:
					imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
					break;
				case BufferUsage.Dynamic:
					imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.Write;
					break;
				default:
					imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.None;
					break;
			}
			imageInfo.Depth = 0;
			imageInfo.Filter = (D3D.FilterFlags)filter;
			imageInfo.FirstMipLevel = 0;
			imageInfo.Format = (SharpDX.DXGI.Format)Settings.Format;
			imageInfo.Width = Settings.Width;
			imageInfo.MipFilter = (D3D.FilterFlags)mipFilter;
			imageInfo.MipLevels = Settings.MipCount;
			imageInfo.OptionFlags = D3D.ResourceOptionFlags.None;
			imageInfo.Usage = (D3D.ResourceUsage)Settings.Usage;
			imageInfo.Width = Settings.Width;

			Gorgon.Log.Print("Gorgon 2D texture {0}: Loading D3D11 1D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = D3D.Texture1D.FromMemory<D3D.Texture1D>(Graphics.D3DDevice, imageData, imageInfo);
			D3DTexture.DebugName = "Gorgon 1D Texture '" + Name + "'";

			Settings = GetTextureInformation() as GorgonTexture1DSettings;
			CreateResourceView();
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="data">Data used to populate the texture.</param>
		protected internal void Initialize(GorgonDataStream data)
		{
			D3D.Texture1DDescription desc = new D3D.Texture1DDescription();

			desc.ArraySize = Settings.ArrayCount;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.MipLevels = Settings.MipCount;
			
			if (Settings.Usage != BufferUsage.Staging)
				desc.BindFlags = D3D.BindFlags.ShaderResource;
			else
				desc.BindFlags = D3D.BindFlags.None;

			desc.Usage = (D3D.ResourceUsage)Settings.Usage;
			switch (Settings.Usage)
			{
				case BufferUsage.Staging:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
					break;
				case BufferUsage.Dynamic:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Write;
					break;
				default:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
					break;
			}			
			desc.OptionFlags = D3D.ResourceOptionFlags.None;

			Gorgon.Log.Print("Gorgon 1D texture {0}: Creating D3D11 1D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			if (data != null)
			{
				using (DX.DataStream stream = new DX.DataStream(data.PositionPointer, data.Length - data.Position, true, true))
					D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc, stream);
			}
			else
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc);
			D3DTexture.DebugName = "Gorgon 1D Texture '" + Name + "'";

			CreateResourceView();			
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>A 1D dimensional texture can only be saved to DDS format.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is not DDS.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			if (format != ImageFileFormat.DDS)
				throw new ArgumentException("The file format for a 1D texture can only be DDS.", "format");

			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			long position = stream.Position;
			D3D.Resource.ToStream<D3D.Resource>(Graphics.Context, D3DTexture, fileFormat, stream);
			stream.Position = position;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture1D(GorgonGraphics graphics, string name, GorgonTexture1DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			Settings = settings;
		}
		#endregion
	}
}
