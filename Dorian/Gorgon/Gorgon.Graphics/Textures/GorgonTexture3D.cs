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
// Created: Wednesday, February 08, 2012 3:04:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a 3D texture.
	/// </summary>
	public class GorgonTexture3DSettings
		: ITextureSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture3DSettings FromFile = new GorgonTexture3DSettings();
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3DSettings"/> class.
		/// </summary>
		public GorgonTexture3DSettings()
		{
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
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
		int ITextureSettings.ArrayCount
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
	}

	/// <summary>
	/// A 3 dimensional texture object.
	/// </summary>
	public class GorgonTexture3D
		: GorgonTexture
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D texture object.
		/// </summary>
		internal D3D.Texture3D D3DTexture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public GorgonTexture3DSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the texture settings from an existing texture.
		/// </summary>
		private void RetrieveSettings()
		{
			GorgonTexture3DSettings settings = new GorgonTexture3DSettings();

			settings.Depth = D3DTexture.Description.Depth;
			settings.Format = (BufferFormat)D3DTexture.Description.Format;
			settings.Height = D3DTexture.Description.Height;
			settings.MipCount = D3DTexture.Description.MipLevels;
			settings.Usage = (BufferUsage)D3DTexture.Description.Usage;
			settings.Width = D3DTexture.Description.Width;

			Settings = settings;
		}

		/// <summary>
		/// Function to create the resource view for the texture.
		/// </summary>
		private void CreateResourceView()
		{
			if (Settings.Usage != BufferUsage.Staging)
			{
				Gorgon.Log.Print("Gorgon 3D texture {0}: Creating D3D11 resource view...", Diagnostics.LoggingLevel.Verbose, Name);
				D3DTexture.DebugName = "Gorgon 3D Texture '" + Name + "'";
				View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
				View.DebugName = "Gorgon 3D Texture '" + Name + "' resource view";
			}

			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (D3DTexture != null)
						D3DTexture.Dispose();

					Gorgon.Log.Print("Gorgon 3D texture {0}: D3D 11 Texture destroyed", Diagnostics.LoggingLevel.Verbose, Name);
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

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
			imageInfo.Depth = Settings.Depth;
			imageInfo.Filter = (D3D.FilterFlags)filter;
			imageInfo.FirstMipLevel = 0;
			imageInfo.Format = (SharpDX.DXGI.Format)Settings.Format;
			imageInfo.Height = Settings.Height;
			imageInfo.Width = Settings.Width;
			imageInfo.MipFilter = (D3D.FilterFlags)mipFilter;
			imageInfo.MipLevels = Settings.MipCount;
			imageInfo.OptionFlags = D3D.ResourceOptionFlags.None;
			imageInfo.Usage = (D3D.ResourceUsage)Settings.Usage;
			imageInfo.Width = Settings.Width;

			Gorgon.Log.Print("Gorgon 3D texture {0}: Loading D3D11 3D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = D3D.Texture3D.FromMemory<D3D.Texture3D>(Graphics.D3DDevice, imageData, imageInfo);

			RetrieveSettings();
			CreateResourceView();
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="data">Data used to populate the texture.</param>
		protected internal void Initialize(GorgonTexture3DData? data)
		{
			D3D.Texture3DDescription desc = new D3D.Texture3DDescription();
			DX.DataBox[] dataRects = null;

			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.Height = Settings.Height;
			desc.Depth = Settings.Depth;
			desc.MipLevels = Settings.MipCount;
			if (Settings.Usage != BufferUsage.Staging)
				desc.BindFlags = D3D.BindFlags.ShaderResource;
			else
				desc.BindFlags = D3D.BindFlags.None;
			desc.Usage = (D3D.ResourceUsage)Settings.Usage;
			switch(Settings.Usage)
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

			Gorgon.Log.Print("Gorgon 3D texture {0}: Creating D3D11 3D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			if (data != null)
			{
				dataRects = new DX.DataBox[data.Value.Data.Length];
				for (int i = 0; i < dataRects.Length; i++)
				{
					dataRects[i].DataPointer = data.Value.Data[i].PositionPointer;
					dataRects[i].RowPitch = data.Value.Pitch[i];
					dataRects[i].SlicePitch = data.Value.Slice[i];
				}

				D3DTexture = new D3D.Texture3D(Graphics.D3DDevice, desc, dataRects);
			}
			else
				D3DTexture = new D3D.Texture3D(Graphics.D3DDevice, desc);

			CreateResourceView();
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			long position = stream.Position;

			if (format == ImageFileFormat.DDS)
				throw new ArgumentException("Volume textures can only be saved to DDS format.", "format");
			
			D3D.Texture3D.ToStream<D3D.Texture3D>(Graphics.Context, D3DTexture, fileFormat, stream);
			stream.Position = position;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture3D(GorgonGraphics graphics, string name, GorgonTexture3DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			Settings = settings;
		}
		#endregion
	}
}
