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
	/// The base texture object for all textures.
	/// </summary>
	public abstract class GorgonTexture
		: GorgonNamedObject, IDisposable, IShaderResource
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the texture was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the shader resource view for the texture.
		/// </summary>
		protected D3D.ShaderResourceView View
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
		/// Property to return information about the format for the texture.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
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
					Graphics.Shaders.PixelShader.Textures.Unbind(this);
					Graphics.Shaders.VertexShader.Textures.Unbind(this);

					Gorgon.Log.Print("Gorgon texture {0}: Unbound from shaders.", Diagnostics.LoggingLevel.Verbose, Name);

					if (View != null)
						View.Dispose();
					Gorgon.Log.Print("Gorgon texture {0}: Destroying D3D 11 resource view.", Diagnostics.LoggingLevel.Verbose, Name);

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
