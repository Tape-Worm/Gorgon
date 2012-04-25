#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, July 20, 2006 10:26:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Drawing = System.Drawing;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image that can receive rendering.
	/// </summary>
	public class RenderImage
		: RenderTarget, IDeviceStateObject
	{
		#region Variables.
		private Image _renderTarget;						            // Image used as a render target.
		private ImageBufferFormats _format;					            // Format of the image.
		private DepthBufferFormats _depthFormat;			            // Depth buffer forma.
		private Sprite _blitter = null;						            // Sprite blitter.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this render target is valid for post pixel shader blending.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// If the driver supports post pixel shader blending of render targets (<see cref="GorgonLibrary.Driver.SupportMRTPostPixelShaderBlending">Driver.SupportMRTPostPixelShaderBlending</see> = True) 
		/// then this property needs to be queried to find out if the particular render target can support post pixel shader blending.
		/// </remarks>
		public override bool IsValidForMRTPostPixelShaderBlending
		{
			get
			{
				return Gorgon.Direct3D.CheckDeviceFormat(Gorgon.CurrentDriver.DriverIndex, Driver.DeviceType, Converter.Convert(_format), 
					D3D9.Usage.QueryPostPixelShaderBlending | D3D9.Usage.RenderTarget, D3D9.ResourceType.Surface, Converter.Convert(_format));
			}
		}

		/// <summary>
		/// Property to return the depth buffer format.
		/// </summary>
		public DepthBufferFormats DepthBufferFormat
		{
			get
			{
				return _depthFormat;
			}
		}

		/// <summary>
		/// Property to set or return whether we want to use a depth buffer or not.
		/// </summary>
		public override bool UseDepthBuffer
		{
			get
			{
				return base.UseDepthBuffer;
			}
			set
			{
				if (!value)
					base.UseStencilBuffer = false;
				base.UseDepthBuffer = value;
				CreateRenderTarget(_format, value, UseStencilBuffer, false);
			}
		}

		/// <summary>
		/// Property to set or return whether we want to use a stencil buffer or not.
		/// </summary>
		public override bool UseStencilBuffer
		{
			get
			{
				return base.UseStencilBuffer;
			}
			set
			{
				base.UseStencilBuffer = value;
				CreateRenderTarget(_format, base.UseDepthBuffer, base.UseStencilBuffer, false);
			}
		}

		/// <summary>
		/// Property to set or return the format of the render image.
		/// </summary>
		public ImageBufferFormats Format
		{
			get
			{
				return _format;
			}
			set
			{
				SetDimensions(Width, Height, value);
			}
		}

		/// <summary>
		/// Property to return the image object used for the render target.
		/// </summary>
		public Image Image
		{
			get
			{
				return _renderTarget;
			}
		}

		/// <summary>
		/// Property to set or return the width of the render target.
		/// </summary>
		public override int Width
		{
			get
			{
				return base.Width;
			}
			set
			{				
				SetDimensions(value, Height, _format);
			}
		}

		/// <summary>
		/// Property to set or return the height of the render target.
		/// </summary>
		public override int Height
		{
			get
			{
				return base.Height;
			}
			set
			{
				SetDimensions(Width, value, _format);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the render target.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="useDepthBuffer">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="preserve">When updating the image, TRUE will preserve the contents, FALSE will not.</param>
		private void CreateRenderTarget(ImageBufferFormats format, bool useDepthBuffer, bool useStencil, bool preserve)
		{
			// Turn off active render target.
			if (Gorgon.CurrentRenderTarget == this)
				Gorgon.CurrentRenderTarget = null;

			SetColorBuffer(null);
			SetDepthBuffer(null);

			if (_renderTarget == null)
			{
				// Create the image.
				_renderTarget = new Image("@RenderImage." + Name, ImageType.RenderTarget, Width, Height, format, true);
				DeviceStateList.Remove(_renderTarget);
			}
			else
				_renderTarget.SetDimensions(Width, Height, format, preserve);	// Resize the target.

			// Get the final format.
			_format = _renderTarget.Format;

			// Get a copy of the color buffer.			
			SetColorBuffer(_renderTarget.D3DTexture.GetSurfaceLevel(0));
			Gorgon.Log.Print("RenderImage", "Color buffer acquired.", LoggingLevel.Verbose);

			// Get stencil type.
			if ((useDepthBuffer) || (useStencil))
			{
				UpdateDepthStencilFormat(useStencil, useDepthBuffer);
				if (_depthFormat != DepthBufferFormats.BufferUnknown)
				{
					SetDepthBuffer(D3D9.Surface.CreateDepthStencil(Gorgon.Screen.Device, Width, Height, Converter.ConvertDepthFormat(_depthFormat), D3D9.MultisampleType.None, 0, false));
					Gorgon.Log.Print("RenderImage", "Depth buffer acquired: {0}.", LoggingLevel.Verbose, _depthFormat.ToString());
				}
				else
					Gorgon.Log.Print("RenderImage", "Could not find suitable depth/stencil format.", LoggingLevel.Verbose);
			}

			Gorgon.Log.Print("RenderImage", "Render image: {0}x{1}, Format: {2}, Depth buffer:{3}, Stencil buffer:{4}, Depth Format: {5}.", LoggingLevel.Intermediate, _renderTarget.ActualWidth, _renderTarget.ActualHeight, _format, useDepthBuffer, useStencil, _depthFormat);

			// Set default states.
			Gorgon.Renderer.RenderStates.SetStates();
			// Set default image layer states.
			for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
				Gorgon.Renderer.ImageLayerStates[i].SetStates();

			Refresh();
		}

		/// <summary>
		/// Function to return a viable stencil/depth buffer.
		/// </summary>
		/// <param name="usestencil">TRUE to use a stencil, FALSE to exclude.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		protected override void UpdateDepthStencilFormat(bool usestencil, bool usedepth)
		{
			int i;							// Loop.						

			// List of depth/stencil formats.
			D3D9.Format[][] dsFormats = new D3D9.Format[][] {new D3D9.Format[] {D3D9.Format.D24SingleS8,D3D9.Format.D24S8,D3D9.Format.D24X4S4,D3D9.Format.D15S1,D3D9.Format.L16},
																		new D3D9.Format[] {D3D9.Format.D32,D3D9.Format.D24X8,D3D9.Format.D16}};

			base.UseStencilBuffer = false;
			base.UseDepthBuffer = false;

			// Deny the stencil buffer if it's not supported.
			if ((!Gorgon.CurrentDriver.SupportStencil) && (usestencil))
			{
				usestencil = false;
				Gorgon.Log.Print("RenderWindow", "Stencil buffer was requested, but the driver doesn't support it.", LoggingLevel.Verbose);
			}

			if ((usestencil) && (usedepth))
			{
				// If we want a stencil buffer, find an appropriate format.
				for (i = 0; i < dsFormats[0].Length; i++)
				{
					if (Gorgon.CurrentDriver.DepthFormatSupported(_format, Converter.ConvertDepthFormat(dsFormats[0][i])))
					{
						_depthFormat = Converter.ConvertDepthFormat(dsFormats[0][i]);
						base.UseStencilBuffer = true;
						base.UseDepthBuffer = true;
						Gorgon.Log.Print("RenderImage", "Stencil and depth buffer requested and found.  Using stencil buffer. ({0})", LoggingLevel.Verbose, _depthFormat.ToString());
						break;
					}
				}
			}

			// If we haven't set this yet, then we haven't selected a buffer format yet.
			if ((!UseDepthBuffer) && (usedepth))
			{
				// Find depth buffers without a stencil buffer.
				for (i = 0; i < dsFormats[1].Length; i++)
				{
					if (Gorgon.CurrentDriver.DepthFormatSupported(_format, Converter.ConvertDepthFormat(dsFormats[1][i])))
					{
						_depthFormat = Converter.ConvertDepthFormat(dsFormats[1][i]);
						base.UseDepthBuffer = true;
						Gorgon.Log.Print("RenderImage", "Stencil buffer not requested or found.  Using depth buffer. ({0}).", LoggingLevel.Verbose, _depthFormat.ToString());
						break;
					}
				}
			}

			if (!UseDepthBuffer)
				Gorgon.Log.Print("RenderImage", "No acceptable depth/stencil buffer found or requested.  Driver may use alternate form of HSR.", LoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		public override void Update()
		{
			Gorgon.Renderer.Render();
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		/// <param name="x">Left position of the blit.</param>
		/// <param name="y">Top position of the blit.</param>
		/// <param name="width">Width to blit with.</param>
		/// <param name="height">Height to blit with.</param>
        /// <param name="color">Color to modulate with the image.</param>
        /// <param name="mode">Mode to define how to handle dimensions that are smaller/larger than the image.</param>
        /// <remarks>Mode is used to either crop or scale the image when the dimensions are larger or smaller than the source image.  It can be one of:
        /// <para>
        /// <list type="table">
        /// <item><term>Scale</term><description>Scale the image up or down to match the width and height parameters.</description></item>
        /// <item><term>Crop</term><description>Clip the image if it's larger than the width and height passed.  If the image is smaller, then the image will be handled according to the
        /// <see cref="GorgonLibrary.Graphics.RenderTarget.HorizontalWrapMode">HorizontalWrapMode</see> or the
        /// <see cref="GorgonLibrary.Graphics.RenderTarget.VerticalWrapMode">VerticalWrapMode</see>.</description></item></list>
        /// </para></remarks>
        public void Blit(float x, float y, float width, float height, Drawing.Color color, BlitterSizeMode mode)
		{
			// We shouldn't blit to ourselves.
			if (Gorgon.CurrentRenderTarget == this)
				return;

			// Create new sprite blitter.
			if (_blitter == null)
				_blitter = new Sprite("Blitter", this);

            if (color != _blitter.Color)
                _blitter.Color = color;

			// Perform scale.
			if (mode == BlitterSizeMode.Scale)
			{
				// Calculate the new scale.
				Vector2D newScale = new Vector2D(width / (float)Width, height / (float)Height);

				// Adjust scale if necessary.
				if (newScale != _blitter.Scale)
					_blitter.Scale = newScale;
			}
			else
			{
				if (_blitter.UniformScale != 1.0f)
					_blitter.UniformScale = 1.0f;

				// Crop.
				if (width != _blitter.Width)
					_blitter.Width = width;
				if (height != _blitter.Height)
					_blitter.Height = height;
			}

			// Set the position.
			if ((_blitter.Position.X != x) || (_blitter.Position.Y != y))
				_blitter.Position = new Vector2D(x, y);

			// Inherit the target states.
			if (_blitter.Smoothing != Smoothing)
				_blitter.Smoothing = Smoothing;
			if (_blitter.SourceBlend != SourceBlend)
				_blitter.SourceBlend = SourceBlend;
			if (_blitter.DestinationBlend != DestinationBlend)
				_blitter.DestinationBlend = DestinationBlend;
            if (_blitter.SourceBlendAlpha != SourceBlendAlpha)
                _blitter.SourceBlendAlpha = SourceBlendAlpha;
            if (_blitter.DestinationBlendAlpha != DestinationBlendAlpha)
                _blitter.DestinationBlendAlpha = DestinationBlendAlpha;
			if (_blitter.HorizontalWrapMode != HorizontalWrapMode)
				_blitter.HorizontalWrapMode = HorizontalWrapMode;
			if (_blitter.VerticalWrapMode != VerticalWrapMode)
				_blitter.VerticalWrapMode = VerticalWrapMode;
			if (_blitter.AlphaMaskFunction != AlphaMaskFunction)
				_blitter.AlphaMaskFunction = AlphaMaskFunction;
			if (_blitter.AlphaMaskValue != AlphaMaskValue)
				_blitter.AlphaMaskValue = AlphaMaskValue;
			if (_blitter.StencilCompare != StencilCompare)
				_blitter.StencilCompare = StencilCompare;
			if (_blitter.StencilEnabled != StencilEnabled)
				_blitter.StencilEnabled = StencilEnabled;
			if (_blitter.StencilFailOperation != StencilFailOperation)
				_blitter.StencilFailOperation = StencilFailOperation;
			if (_blitter.StencilMask != StencilMask)
				_blitter.StencilMask = StencilMask;
			if (_blitter.StencilPassOperation != StencilPassOperation)
				_blitter.StencilPassOperation = StencilPassOperation;
			if (_blitter.StencilReference != StencilReference)
				_blitter.StencilReference = StencilReference;
			if (_blitter.StencilZFailOperation != StencilZFailOperation)
				_blitter.StencilZFailOperation = StencilZFailOperation;

			_blitter.Draw();
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		/// <param name="x">Left position of the blit.</param>
		/// <param name="y">Top position of the blit.</param>
		/// <param name="width">Width to blit with.</param>
		/// <param name="height">Height to blit with.</param>
		public void Blit(float x, float y, float width, float height)
		{
            Blit(x, y, width, height, Drawing.Color.White, BlitterSizeMode.Scale);
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		/// <param name="x">Left position of the blit.</param>
		/// <param name="y">Top position of the blit.</param>
		public void Blit(float x, float y)
		{
            Blit(x, y, Width, Height, Drawing.Color.White, BlitterSizeMode.Crop);
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		public void Blit()
		{
            Blit(0, 0, Width, Height, Drawing.Color.White, BlitterSizeMode.Crop);
		}

		/// <summary>
		/// Function to set the dimensions for the render target.
		/// </summary>
		/// <param name="width">Width of the target.</param>
		/// <param name="height">Height of the target.</param>
		/// <param name="format">Format for the target.</param>
		/// <param name="preserve">TRUE to preserve the image contents, FALSE to destroy.</param>
		public void SetDimensions(int width, int height, ImageBufferFormats format, bool preserve)
		{
			base.Width = width;
			base.Height = height;
			CreateRenderTarget(format, UseDepthBuffer, UseStencilBuffer, preserve);
		}

		/// <summary>
		/// Function to set the dimensions for the render target.
		/// </summary>
		/// <param name="width">Width of the target.</param>
		/// <param name="height">Height of the target.</param>
		/// <param name="format">Format for the target.</param>		
		public void SetDimensions(int width, int height, ImageBufferFormats format)
		{
			SetDimensions(width, height, format, false);
		}

		/// <summary>
		/// Function to set the dimensions for the render target.
		/// </summary>
		/// <param name="width">Width of the target.</param>
		/// <param name="height">Height of the target.</param>
		public void SetDimensions(int width, int height)
		{
			SetDimensions(width, height, _format, false);
		}

		/// <summary>
		/// Function to convert the render image into a standard image.
		/// </summary>
		/// <param name="image"></param>
		private void ConvertToImageData(Image image)
		{
			if (image == null)
				throw new ArgumentNullException();

			// Resize the image.
			if ((_renderTarget.Width != image.Width) || (_renderTarget.Height != image.Height) || (_renderTarget.Format != image.Format))
				image.SetDimensions(_renderTarget.Width, _renderTarget.Height, _renderTarget.Format, true);

			// Transfer to the image via a proxy.
			image.Copy(_renderTarget);
		}

		/// <summary>
		/// Function to copy the render target into an image.
		/// </summary>
		/// <param name="image">Image that will receive the data.</param>
		public override void CopyToImage(Image image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			if ((image.Width != Width) || (image.Height != Height) || (image.Format != Format) || (image.ImageType == ImageType.Normal))
				ConvertToImageData(image);
			else
				ConvertToImageData(image, Format);
		}

		/// <summary>
		/// Function to copy the render target into a render target image.
		/// </summary>
		/// <param name="destination">The render image that will receive the data.</param>
		public override void CopyToImage(RenderImage destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			CopyToImage(destination.Image);
		}

		/// <summary>
		/// Function to copy the contents of an image into this render image.
		/// </summary>
		/// <param name="image">Image that will be copied.</param>
		public void CopyFromImage(Image image)
		{
			RenderTarget current = Gorgon.CurrentRenderTarget;		// Current render target.
			BlendingModes currentMode = BlendingMode;				// Current blending mode.

			if (image == null)
				throw new ArgumentNullException("image");

			try
			{
				// Set us as the current target.				
				Gorgon.CurrentRenderTarget = this;
				BlendingMode = BlendingModes.None;
				image.Blit();
			}
			finally
			{
				// Restore.
				BlendingMode = currentMode;
				Gorgon.CurrentRenderTarget = current;
			}
		}

		/// <summary>
		/// Function to retrieve the underlying image data for direct access.
		/// </summary>
		/// <param name="lockRectangle">The area to lock.</param>
		/// <returns>An <see cref="GorgonLibrary.Graphics.Image.ImageLockBox">Image.ImageLockBox</see> which contains the direct access to the underlying image.</returns>
		/// <remarks>This function acts as a means of direct image maniuplation.  This means while the user is given direct control over the contents of the image the system will 
		/// provide NO support for any data read/written to the lock area.  There will be no image resizing, no color conversion, the user will be on their own and must
		/// be aware of the image format.</remarks>
		public Image.ImageLockBox GetImageData(Drawing.Rectangle lockRectangle)
		{
			return _renderTarget.GetImageData(lockRectangle);
		}

		/// <summary>
		/// Function to retrieve the underlying image data for direct access.
		/// </summary>
		/// <returns>An <see cref="GorgonLibrary.Graphics.Image.ImageLockBox">Image.ImageLockBox</see> which contains the direct access to the underlying image.</returns>
		/// <remarks>This function acts as a means of direct image maniuplation.  This means while the user is given direct control over the contents of the image the system will 
		/// provide NO support for any data read/written to the lock area.  There will be no image resizing, no color conversion, the user will be on their own and must
		/// be aware of the image format.<para>This overload of the function will lock the entire image for reading/writing.</para></remarks>
		public Image.ImageLockBox GetImageData()
		{
			return _renderTarget.GetImageData();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RenderImage"/> class.
		/// </summary>
		/// <param name="name">Name of the render image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="format">Format of the image.</param>
		/// <param name="useDepthBuffer">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencilBuffer">TRUE to use a stencil buffer, FALSE to exclude.</param>
		public RenderImage(string name, int width, int height, ImageBufferFormats format, bool useDepthBuffer, bool useStencilBuffer)
			: base(name,width,height)
		{
			Gorgon.Log.Print("RenderImage", "Creating rendering image '{0}' ...", LoggingLevel.Intermediate, name);
			_depthFormat = DepthBufferFormats.BufferUnknown;
			_renderTarget = null;
			Gorgon.Log.Print("RenderImage", "Rendering image '{0}' created.", LoggingLevel.Intermediate, name);

			// Create render target.
			CreateRenderTarget(format, useDepthBuffer, useStencilBuffer, false);

			// Add to cache.
			RenderTargetCache.Targets.Add(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderImage"/> class.
		/// </summary>
		/// <param name="name">Name of the render image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="format">Format of the image.</param>
		public RenderImage(string name, int width, int height, ImageBufferFormats format)
			: this(name, width, height, format, false, false)
		{
		}

		/// <summary>
		/// Function to handle clean up.
		/// </summary>
		/// <param name="disposing">TRUE if disposing of all objects, FALSE to only affect unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				Gorgon.Log.Print("RenderImage", "Destroying render image '{0}'.", LoggingLevel.Intermediate, Name);

				SetColorBuffer(null);
				Gorgon.Log.Print("RenderImage", "Releasing color buffer.", LoggingLevel.Verbose);
				SetDepthBuffer(null);
				Gorgon.Log.Print("RenderImage", "Releasing depth buffer.", LoggingLevel.Verbose);
				if (_renderTarget != null)
					_renderTarget.Dispose();

				Gorgon.Log.Print("RenderImage", "Render Image '{0}' destroyed.", LoggingLevel.Intermediate, Name);
			}

			_renderTarget = null;
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			base.DeviceLost();
			Gorgon.CurrentRenderTarget = null;

			// Remove the image since it won't get updated automatically.
			if (_renderTarget != null)
				_renderTarget.DeviceLost();

			// Turn off depth buffer.
			SetDepthBuffer(null);
			SetColorBuffer(null);
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			// Rebuild the texture.
			if (_renderTarget != null)
				_renderTarget.DeviceReset();	
			CreateRenderTarget(_format, UseDepthBuffer, UseStencilBuffer, false);
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public override void ForceRelease()
		{
			DeviceLost();
		}
		#endregion
	}
}
