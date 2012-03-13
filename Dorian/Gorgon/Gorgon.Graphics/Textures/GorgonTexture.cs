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
// Created: Monday, February 13, 2012 7:48:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using D3D = SharpDX.Direct3D11;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Formats for image files.
	/// </summary>
	public enum ImageFileFormat
	{
		/// <summary>
		/// Portable network graphics.
		/// </summary>
		PNG = 3,
		/// <summary>
		/// Joint Photographic Experts Group.
		/// </summary>
		JPG = 1,
		/// <summary>
		/// Windows bitmap.
		/// </summary>
		BMP = 0,
		/// <summary>
		/// Direct Draw Surface.
		/// </summary>
		DDS = 4
	}

	/// <summary>
	/// Settings for a texture.
	/// </summary>
	public interface ITextureSettings
	{
		/// <summary>
		/// Property to set or return the width of a texture.
		/// </summary>
		/// <remarks>When loading a file, leave as 0 to use the width from the file source.</remarks>
		int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a texture.
		/// </summary>
		/// <remarks>
		/// When loading a file, leave as 0 to use the height from the file source.
		/// <para>This applies to 2D and 3D textures only.</para></remarks>
		int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <remarks>
		/// When loading a file, leave as 0 to use the width from the depth source.
		/// <para>This applies to 3D textures only.</para></remarks>
		int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <remarks>
		/// When loading a texture from a file, leave this as Unknown to get the file format from the source file.
		/// <para>This sets the format of the texture data.  If you want to change the format of a texture when being sampled in a shader, then set the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ViewFormat">ViewFormat</see> property to anything other than Unknown.</para></remarks>
		BufferFormat Format
		{
			get;
			set;
		}

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
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
		int ArrayCount
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
		/// Property to set or return the number of mip maps in a texture.
		/// </summary>
		/// <remarks>To have the system generate mipmaps for you, set this value to 0.  The default value for this setting is 1.</remarks>
		int MipCount
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
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		bool IsPowerOfTwo
		{
			get;
		}
	}

	/// <summary>
	/// The base texture object for all textures.
	/// </summary>
	public abstract class GorgonTexture
		: GorgonNamedObject, IDisposable, IShaderResource
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the texture was disposed.
		private int _size = 0;								// Size of the texture, in bytes.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the shader resource view for the texture.
		/// </summary>
		internal D3D.ShaderResourceView View
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the D3D texture.
		/// </summary>
		internal D3D.Resource D3DTexture
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about format for the shader resource view.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData ViewFormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about the format for the texture.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the texture, in bytes.
		/// </summary>
		/// <remarks>This will take into account whether the texture is a packed format, or compressed.</remarks>
		public int SizeInBytes
		{
			get
			{
				if (_size == 0)
					_size = GetTextureSize();

				return _size;
			}
			protected set
			{
				if (value < 0)
					value = 0;
				_size = value;
			}
		}

		/// <summary>
		/// Property to set or return the settings for the texture.
		/// </summary>
		public ITextureSettings Settings
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the size of the texture, in bytes.
		/// </summary>
		/// <returns>The size of the texture in bytes.</returns>
		private int GetTextureSize()
		{
			GorgonTexture1D texture1D = this as GorgonTexture1D;
			GorgonTexture2D texture2D = this as GorgonTexture2D;
			GorgonTexture3D texture3D = this as GorgonTexture3D;
			int width = 1;
			int height = 1;
			int depth = 1;
			int mipCount = 1;
			int arrayCount = 1;		// This is always 1 for a 3D texture.
			int result = 0;
			int bytes = FormatInformation.SizeInBytes;
			bool isCompressed = FormatInformation.IsCompressed;

			if (bytes == 0)
				return 0;

			if (texture1D != null)
			{
				width = texture1D.Settings.Width;
				mipCount = texture1D.Settings.MipCount;
				arrayCount = texture1D.Settings.ArrayCount;
			}

			if (texture2D != null)
			{
				width = texture2D.Settings.Width;
				height = texture2D.Settings.Height;
				mipCount = texture2D.Settings.MipCount;
				arrayCount = texture2D.Settings.ArrayCount;
			}

			if (texture3D != null)
			{
				width = texture3D.Settings.Width;
				height = texture3D.Settings.Height;
				depth = texture3D.Settings.Depth;
				mipCount = texture3D.Settings.MipCount;
			}

			for (int arrayIndex = 0; arrayIndex < arrayCount; arrayIndex++)
			{
				int mipWidth = width;
				int mipHeight = height;

				for (int mipIndex = 0; mipIndex < mipCount; mipIndex++)
				{
					int slicePitch = 0;

					if (isCompressed)
						slicePitch = GorgonMathUtility.Max(1, ((mipHeight + 3) / 4) * bytes) * GorgonMathUtility.Max(1, ((mipWidth + 3) / 4) * bytes);
					else
						slicePitch = ((mipWidth + bytes - 1) / bytes) * mipHeight;
						
					for (int slice = 0; slice < depth; slice++)
						result += slicePitch;

					mipWidth >>= mipIndex;
					mipHeight >>= mipIndex;
					depth >>= mipIndex;

					if (mipWidth < 1)
						mipWidth = 1;
					if (mipHeight < 1)
						mipHeight = 1;
					if (depth < 1)
						depth = 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to create the shader resource view.
		/// </summary>
		private void CreateShaderResourceView()
		{
			string textureType = GetType().Name;
			bool isMultisampled = Settings.Multisampling.Count > 1 || Settings.Multisampling.Quality > 0;
			D3D.ShaderResourceViewDescription desc = default(D3D.ShaderResourceViewDescription);

			if (View != null)
			{
				View.Dispose();
				View = null;
			}

			// We cannot bind staging resources to the pipeline.
			if (Settings.Usage != BufferUsage.Staging)
			{
				if ((Settings.ViewFormat == BufferFormat.Unknown) || (Settings.ViewFormat == Settings.Format))
					View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
				else
				{
					// Determine view type.
					switch (textureType.ToLower())
					{
						case "gorgontexture1d":
							if (Settings.ArrayCount <= 1)
								desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1D;
							else
								desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1DArray;

							desc.Texture1DArray = new D3D.ShaderResourceViewDescription.Texture1DArrayResource()
							{
								MipLevels = Settings.MipCount,
								MostDetailedMip = 0,
								ArraySize = Settings.ArrayCount,
								FirstArraySlice = 0
							};
							break;
						case "gorgontexture2d":
							if (isMultisampled)
							{
								if (Settings.ArrayCount <= 1)
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled;
								else
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampledArray;

								desc.Texture2DMSArray = new D3D.ShaderResourceViewDescription.Texture2DMultisampledArrayResource()
								{
									ArraySize = Settings.ArrayCount,
									FirstArraySlice = 0
								};
							}
							else
							{
								if (Settings.ArrayCount <= 1)
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
								else
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;

								desc.Texture2DArray = new D3D.ShaderResourceViewDescription.Texture2DArrayResource()
								{
									MipLevels = Settings.MipCount,
									MostDetailedMip = 0,
									ArraySize = Settings.ArrayCount,
									FirstArraySlice = 0
								};
							}
							break;
						case "gorgontexture3d":
							desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
							desc.Texture3D = new D3D.ShaderResourceViewDescription.Texture3DResource()
							{
								MipLevels = Settings.MipCount,
								MostDetailedMip = 0
							};
							break;
						default:
							throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a resource view, the texture type '" + textureType + "' is unknown.");
					}

					View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture, desc);
				}

				View.DebugName = textureType + " '" + Name + "' Shader Resource View";

				// Unbind and rebind us to the pipeline.
				Graphics.Shaders.Reseat(this);
			}

			// If we're using the same format for the view as the texture, then use the texture format, otherwise
			// we'll get the information for the view format.
			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
			if (Settings.ViewFormat == BufferFormat.Unknown)
				ViewFormatInformation = FormatInformation;
			else
				ViewFormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.ViewFormat);
		}

		/// <summary>
		/// Function to retrieve information about an existing texture.
		/// </summary>
		/// <returns>New settings for the texture.</returns>
		protected ITextureSettings GetTextureInformation()
		{
			ITextureSettings newSettings = null;

			GorgonDebug.AssertNull<ITextureSettings>(newSettings, "newSettings");

			D3D.Texture1D texture1D = D3DTexture as D3D.Texture1D;
			D3D.Texture2D texture2D = D3DTexture as D3D.Texture2D;
			D3D.Texture3D texture3D = D3DTexture as D3D.Texture3D;

			if (texture1D != null)
			{
				D3D.Texture1DDescription desc = texture1D.Description;
				newSettings = new GorgonTexture1DSettings();
				newSettings.Width = desc.Width;
				newSettings.Height = 1;
				newSettings.ArrayCount = desc.ArraySize;
				newSettings.Depth = 1;
				newSettings.Format = (BufferFormat)desc.Format;
				newSettings.MipCount = desc.MipLevels;
				newSettings.Usage = (BufferUsage)desc.Usage;
				newSettings.ViewFormat = BufferFormat.Unknown;
				newSettings.Multisampling = new GorgonMultisampling(1, 0);
			}

			if (texture2D != null)
			{
				D3D.Texture2DDescription desc = texture2D.Description;
				newSettings = new GorgonTexture2DSettings();
				newSettings.Width = desc.Width;
				newSettings.Height = desc.Height;
				newSettings.ArrayCount = desc.ArraySize;
				newSettings.Depth = 1;
				newSettings.Format = (BufferFormat)desc.Format;
				newSettings.MipCount = desc.MipLevels;
				newSettings.Usage = (BufferUsage)desc.Usage;
				newSettings.ViewFormat = BufferFormat.Unknown;
				newSettings.Multisampling = new GorgonMultisampling(desc.SampleDescription.Count, desc.SampleDescription.Quality);
			}

			if (texture3D != null)
			{
				D3D.Texture3DDescription desc = texture3D.Description;
				newSettings = new GorgonTexture3DSettings();
				newSettings.Width = desc.Width;
				newSettings.Height = desc.Height;
				newSettings.ArrayCount = 1;
				newSettings.Depth = desc.Depth;
				newSettings.Format = (BufferFormat)desc.Format;
				newSettings.MipCount = desc.MipLevels;
				newSettings.Usage = (BufferUsage)desc.Usage;
				newSettings.ViewFormat = BufferFormat.Unknown;
				newSettings.Multisampling = new GorgonMultisampling(1, 0);
			}

			return newSettings;
		}

		/// <summary>
		/// Function to create a shader resource and optionally, an unordered access view
		/// </summary>
		protected void CreateResourceView()
		{
			CreateShaderResourceView();
		}

		/// <summary>
		/// Function to save the texture data to an array of bytes.
		/// </summary>
		/// <param name="format">Image format to use.</param>
		/// <returns>An array of bytes containing the image data.</returns>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS for a volume (3D) texture.
		/// <para>-or-</para>
		/// <para>Thrown when the file cannot be saved with the requested file <paramref name="format"/>.</para>
		/// </exception>
		public byte[] Save(ImageFileFormat format)
		{
			MemoryStream stream = new MemoryStream();
			Save(stream, format);
			stream.Position = 0;
			return stream.ToArray();
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">
		/// Thrown when the format is anything other than DDS for a volume (3D) texture.
		/// <para>-or-</para>
		/// <para>Thrown when the format is anything other than DDS.</para>
		/// </exception>
		public abstract void Save(System.IO.Stream stream, ImageFileFormat format);

		/// <summary>
		/// Function to save the texture data to a file.
		/// </summary>
		/// <param name="fileName">Name of the file to save into.</param>
		/// <param name="format">Image format to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the format is anything other than DDS for a volume (3D) texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the file cannot be saved with the requested file <paramref name="format"/>.</para>
		/// </exception>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		public void Save(string fileName, ImageFileFormat format)
		{
			GorgonDebug.AssertParamString(fileName, "fileName");

			FileStream stream = null;

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, format);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns the texture.</param>
		/// <param name="name">The name of the texture.</param>
		protected GorgonTexture(GorgonGraphics graphics, string name)
			: base(name)
		{
			Graphics = graphics;
		}
		#endregion

		#region IShaderResource Members
		/// <summary>
		/// Property to return the shader resource view for an object.
		/// </summary>
		D3D.ShaderResourceView IShaderResource.D3DResourceView
		{
			get
			{
				return View;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Gorgon.Log.Print("Gorgon texture {0}: Unbound from shaders.", Diagnostics.LoggingLevel.Verbose, Name);
					Graphics.Shaders.Unbind(this);

					if (D3DTexture != null)
						D3DTexture.Dispose();

					Gorgon.Log.Print("Gorgon texture {0}: Destroying D3D 11 shader resource view.", Diagnostics.LoggingLevel.Verbose, Name);
					if (View != null)
						View.Dispose();

					Graphics.RemoveTrackedObject(this);
				}				

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
