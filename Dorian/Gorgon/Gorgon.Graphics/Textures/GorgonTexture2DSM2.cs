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
// Created: Sunday, March 11, 2012 9:35:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 2D texture shim for SM2 cards.
	/// </summary>
	class GorgonTexture2DSM2
		: GorgonTexture2D
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private D3D.Texture2D _shimTexture = null;				// Shim texture to get around D3DX issues when saving textures.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to destroy the shim texture.
		/// </summary>
		private void DestroyShim()
		{
			if ((_shimTexture != null) && (_shimTexture != D3DTexture))
			{
				_shimTexture.Dispose();
				_shimTexture = null;
			}
		}

		/// <summary>
		/// Function to work around a bug when saving on a SM2_a_b device.
		/// </summary>
		/// <remarks>As per the 10level9 documentation:
		/// A resource that has the shader binding flag set (which we do for our textures) and no other flags cannot use CopyResource to copy data into CPU accessible memory.  Therefore, the D3DX functions will fail every time the 
		/// user tries to save a texture (the CPU needs to be able to read the data from a staging texture, which is only in CPU memory).</remarks>
		private void CreateShim()
		{
			try
			{
				DestroyShim();

				// Since we have another flag specified, we can copy this texture without removing the shader binding.
				if (D3DTexture.Description.BindFlags != D3D.BindFlags.ShaderResource)
				{
					_shimTexture = D3DTexture;
					return;
				}

				if (!Graphics.VideoDevice.SupportsRenderTargetFormat(Settings.Format, Settings.Multisampling.Count > 1 || Settings.Multisampling.Quality > 0))
					throw new GorgonException(GorgonResult.CannotWrite, "Cannot save this texture.  There are restrictions in place for SM2_a_b video devices that limit the formats that can be saved.  The format '" + Settings.Format.ToString() + "' cannot be persisted to a stream or file.");

				// Create our temporary texture.
				D3D.Texture2DDescription tempDesc = new D3D.Texture2DDescription()
				{
					ArraySize = Settings.ArrayCount,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					BindFlags = D3D.BindFlags.RenderTarget,
					Format = (SharpDX.DXGI.Format)Settings.Format,
					Height = Settings.Height,
					MipLevels = Settings.MipCount,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SampleDescription = GorgonMultiSampling.Convert(Settings.Multisampling),
					Usage = D3D.ResourceUsage.Default,
					Width = Settings.Width
				};

				_shimTexture = new D3D.Texture2D(Graphics.D3DDevice, tempDesc);
				_shimTexture.DebugName = "HACK: Fix for broken D3DX. Texture: \"" + Name + "\"";

				// Copy the texture to the temporary texture.
				Graphics.Context.CopyResource(D3DTexture, _shimTexture);
			}
			catch
			{
				if (_shimTexture != null)
					_shimTexture.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Function to convert this format to RGBA 32 bit normalized unsigned int.
		/// </summary>
		/// <returns>
		/// The converted texture.
		/// </returns>
		protected override GorgonTexture2D ConvertToNormalized32Bit()
		{
			GorgonTexture2DSM2 tempTexture = null;
			GorgonTexture2DSettings settings = Settings;

			try
			{
				// Convert to RGBA 32bit format.
				if ((Settings.Format != BufferFormat.R8G8B8A8_UIntNormal) && (Settings.Format != BufferFormat.R8G8B8A8_UIntNormal_sRGB))
				{
					settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
					tempTexture = new GorgonTexture2DSM2(Graphics, Name + ".TempTexture", settings);
					tempTexture.Initialize(null);
					tempTexture.Copy(this);
					tempTexture.CreateShim();

					// Link to our shim texture.
					DestroyShim();
					_shimTexture = tempTexture._shimTexture;
				}
				else
				{
					tempTexture = this;
					CreateShim();
				}
			}
			catch
			{
				if ((tempTexture != null) && (tempTexture != this))
					tempTexture.Dispose();
				throw;
			}

			return tempTexture;
		}

		/// <summary>
		/// Function to copy a texture from another texture.
		/// </summary>
		/// <param name="texture">Texture to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.InvalidOperationException">Thrown when the this texture is an immutable texture.
		///   <para>-or-</para>
		///   <para>Thrown when the video device is a SM2_a_b device and an there is an attempt to convert between two different formats.</para>
		///   <para>-or-</para>
		///   <para>Thrown when this texture or the source texture have differing multisample values.</para>
		///   <para>-or-</para>
		///   <para>Thrown when the size of the source texture and this texture are different and multisampling is enabled or either the source texture or this texture are depth/stencil buffers.</para>
		///   </exception>
		public override void Copy(GorgonTexture2D texture)
		{			
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			GorgonTexture2DSM2 sourceTexture = texture as GorgonTexture2DSM2;

			if (sourceTexture == null)
				throw new ArgumentException("The texture was created with a different device.", "texture");

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new InvalidOperationException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if (texture.Settings.Format != Settings.Format)
			{
				SlowCopy(texture);
				return;
			}

			try
			{
				// Check to see if we need a shim.
				if (sourceTexture._shimTexture != sourceTexture.D3DTexture)
					sourceTexture.CreateShim();

				// If the width and height differ, then use a sub resource copy.
				if ((texture.Settings.Width != Settings.Width) || (texture.Settings.Height != Settings.Height))
				{
					if ((texture.Settings.Multisampling.Count > 1) || (texture.Settings.Multisampling.Quality > 0) ||
						(Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0))
						throw new InvalidOperationException("A multisampled texture must be the same size as its destination.");

					if ((texture.IsDepthStencil) || (this.IsDepthStencil))
						throw new InvalidOperationException("A depth/stencil must be the same size as its destination.");

					Graphics.Context.CopySubresourceRegion(sourceTexture._shimTexture, 0, new D3D.ResourceRegion()
					{
						Back = 1,
						Front = 0,
						Left = 0,
						Top = 0,
						Right = texture.Settings.Width > Settings.Width ? Settings.Width : texture.Settings.Width,
						Bottom = texture.Settings.Height > Settings.Height ? Settings.Height : texture.Settings.Height
					}, D3DTexture, 0, 0, 0, 0);
					return;
				}

				Graphics.Context.CopyResource(sourceTexture._shimTexture, D3DTexture);
			}
			finally
			{
				if (sourceTexture != null)
					sourceTexture.DestroyShim();
			}
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when attempting to save a <see cref="GorgonLibrary.Graphics.GorgonTexture2D.IsDepthStencil">depth/stencil texture</see>.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			GorgonTexture2DSM2 tempTexture = null;

			if (IsDepthStencil)
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot save a depth/stencil buffer texture.");

			try
			{
				// We can only save to 32 bit RGBA uint normalized formats if we're not using DDS, so we have to convert.
				if (format != ImageFileFormat.DDS)
					tempTexture = (GorgonTexture2DSM2)ConvertToNormalized32Bit();
				else
				{
					tempTexture = this;
					CreateShim();
				}

				D3D.Texture2D.ToStream<D3D.Texture2D>(Graphics.Context, tempTexture._shimTexture, fileFormat, stream);
			}
			finally
			{
				DestroyShim();

				if ((tempTexture != null) && (tempTexture != this))
					tempTexture.Dispose();
			}
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
					if ((_shimTexture != null) && (_shimTexture != D3DTexture))
						_shimTexture.Dispose();
				}

				_shimTexture = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSM2"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">The settings for the texture.</param>
		internal GorgonTexture2DSM2(GorgonGraphics graphics, string name, GorgonTexture2DSettings settings)
			: base(graphics, name, settings)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSM2"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain to get texture information from.</param>
		internal GorgonTexture2DSM2(GorgonSwapChain swapChain)
			: base(swapChain)
		{
		}
		#endregion
	}
}
