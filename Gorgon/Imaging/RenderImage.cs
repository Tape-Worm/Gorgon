#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, July 20, 2006 10:26:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics.Shaders;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image that can receive rendering.
	/// </summary>
	public class RenderImage
		: RenderTarget, IDeviceStateObject
	{
		#region Variables.
		private Image _renderTarget;						// Image used as a render target.
		private ImageBufferFormats _format;					// Format of the image.
		private DepthBufferFormats _depthFormat;			// Depth buffer format.
		#endregion

		#region Properties.
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
				return _useDepthBuffer;
			}
			set
			{
				if (!value)
					_useStencilBuffer = false;
				CreateRenderTarget(_format, value, _useStencilBuffer, false);
			}
		}

		/// <summary>
		/// Property to set or return whether we want to use a stencil buffer or not.
		/// </summary>
		public override bool UseStencilBuffer
		{
			get
			{
				return _useStencilBuffer;
			}
			set
			{
				if (!_useDepthBuffer)
				{
					_useStencilBuffer = false;
					return;
				}

				CreateRenderTarget(_format, _useDepthBuffer, value, false);
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
				SetDimensions(_width, _height, value);
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
				SetDimensions(value, _height, _format);
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
				SetDimensions(_width, value, _format);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the dimensions for the render target.
		/// </summary>
		/// <param name="width">Width of the target.</param>
		/// <param name="height">Height of the target.</param>
		/// <param name="format">Format for the target.</param>
		/// <param name="preserve">TRUE to preserve the image contents, FALSE to destroy.</param>
		public void SetDimensions(int width, int height, ImageBufferFormats format, bool preserve)
		{
			_width = width;
			_height = height;
			CreateRenderTarget(format, _useDepthBuffer, _useStencilBuffer, preserve);
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
		/// Function to create the render target.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="useDepthBuffer">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="preserve">When updating the image, TRUE will preserve the contents, FALSE will not.</param>
		private void CreateRenderTarget(ImageBufferFormats format, bool useDepthBuffer, bool useStencil, bool preserve)
		{
			try
			{
				// Turn off active render target.
				if (Gorgon.Renderer.CurrentRenderTarget == this)
					Gorgon.Renderer.SetRenderTarget(null);

				if (_colorBuffer != null)
					_colorBuffer.Dispose();
				_colorBuffer = null;

				// Turn off depth buffer.
				if (_depthBuffer != null)
					_depthBuffer.Dispose();
				_depthBuffer = null;

				if (_renderTarget == null)
				{
					// Create the image.
					_renderTarget = new Image("@RenderImage." + _objectName, ImageType.RenderTarget, _width, _height, format, false, true);
					Gorgon.DeviceStateList.Remove(_renderTarget);
				}
				else
					_renderTarget.SetDimensions(_width, _height, format, preserve);	// Resize the target.

				// Get the final format.
				_format = _renderTarget.Format;				

				// Get a copy of the color buffer.			
				_colorBuffer = _renderTarget.D3DTexture.GetSurfaceLevel(0);
				Gorgon.Log.Print("RenderImage", "Color buffer acquired.", LoggingLevel.Verbose);

				// Get stencil type.
				if ((useDepthBuffer) || (useStencil))
				{
					UpdateDepthStencilFormat(useStencil, useDepthBuffer);
					if (_depthFormat != DepthBufferFormats.BufferUnknown)
					{
						_depthBuffer = Gorgon.Screen.Device.CreateDepthStencilSurface(_width, _height, Converter.Convert(_depthFormat), D3D.MultiSampleType.None, 0, false);
						Gorgon.Log.Print("RenderImage", "Depth buffer acquired: {0}.", LoggingLevel.Verbose, _depthFormat.ToString());
					}
					else
						Gorgon.Log.Print("RenderImage", "Could not find suitable depth/stencil format.", LoggingLevel.Verbose);
				}

				Gorgon.Log.Print("RenderImage", "Render image: {0}x{1}, Format: {2}, Depth buffer:{3}, Stencil buffer:{4}, Depth Format: {5}.", LoggingLevel.Intermediate, _renderTarget.ActualWidth, _renderTarget.ActualHeight, _format, useDepthBuffer, useStencil, _depthFormat);

				// Confirm if a W-buffer exists or not.
				Gorgon.Renderer.RenderStates.CheckForWBuffer(_depthFormat);
				// Set default states.
				Gorgon.Renderer.RenderStates.SetStates();
				// Set default image layer states.
				for (int i = 0; i < Gorgon.Driver.MaximumTextureStages; i++)
					Gorgon.Renderer.ImageLayerStates[i].SetStates();

				_defaultView.Refresh();
				Refresh();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotCreateException(Name, GetType(), ex);
			}
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
			D3D.DepthFormat[][] dsFormats = new D3D.DepthFormat[][] {new D3D.DepthFormat[] {D3D.DepthFormat.D24SingleS8,D3D.DepthFormat.D24S8,D3D.DepthFormat.D24X4S4,D3D.DepthFormat.D15S1,D3D.DepthFormat.L16},
																		new D3D.DepthFormat[] {D3D.DepthFormat.D32,D3D.DepthFormat.D24X8,D3D.DepthFormat.D16}};

			_useStencilBuffer = false;
			_useDepthBuffer = false;

			// Deny the stencil buffer if it's not supported.
			if ((!Gorgon.Driver.SupportStencil) && (usestencil))
			{
				usestencil = false;
				Gorgon.Log.Print("RenderWindow", "Stencil buffer was requested, but the driver doesn't support it.", LoggingLevel.Verbose);
			}

			if ((usestencil) && (usedepth))
			{
				// If we want a stencil buffer, find an appropriate format.
				for (i = 0; i < dsFormats[0].Length; i++)
				{
					if (CheckBackBuffer(Gorgon.Driver.DriverIndex, Converter.Convert(_format), dsFormats[0][i]))
					{
						_depthFormat = Converter.Convert(dsFormats[0][i]);
						_useStencilBuffer = true;
						_useDepthBuffer = true;
						Gorgon.Log.Print("RenderImage", "Stencil and depth buffer requested and found.  Using stencil buffer. ({0})", LoggingLevel.Verbose, Converter.Convert(dsFormats[0][i]).ToString());
						break;
					}
				}
			}

			// If we haven't set this yet, then we haven't selected a buffer format yet.
			if ((!_useDepthBuffer) && (usedepth))
			{
				// Find depth buffers without a stencil buffer.
				for (i = 0; i < dsFormats[1].Length; i++)
				{
					if (CheckBackBuffer(Gorgon.Driver.DriverIndex, Converter.Convert(_format), dsFormats[1][i]))
					{
						_depthFormat = Converter.Convert(dsFormats[1][i]);
						_useDepthBuffer = true;
						Gorgon.Log.Print("RenderImage", "Stencil buffer not requested or found.  Using depth buffer. ({0}).", LoggingLevel.Verbose, Converter.Convert(dsFormats[1][i]).ToString());
						break;
					}
				}
			}

			if (!_useDepthBuffer)
				Gorgon.Log.Print("RenderImage", "No acceptable depth/stencil buffer found or requested.  Driver may use alternate form of HSR.", LoggingLevel.Verbose);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the render image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="useDepthBuffer">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencilBuffer">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="priority">Priority of the render target.</param>
		internal RenderImage(string name,int width,int height, ImageBufferFormats format, bool useDepthBuffer, bool useStencilBuffer, int priority)
			: base(name,width,height,priority)
		{
			Gorgon.Log.Print("RenderImage", "Creating rendering image '{0}' ...", LoggingLevel.Intermediate, name);
			_depthFormat = DepthBufferFormats.BufferUnknown;
			_renderTarget = null;
			CreateRenderTarget(format, useDepthBuffer, useStencilBuffer, false);
			Gorgon.Log.Print("RenderImage", "Rendering image '{0}' created.", LoggingLevel.Intermediate, name);
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
				Gorgon.Log.Print("RenderImage", "Destroying render image '{0}'.", LoggingLevel.Intermediate, _objectName);

				if (_colorBuffer != null)
				{
					_colorBuffer.Dispose();
					Gorgon.Log.Print("RenderImage", "Releasing color buffer.", LoggingLevel.Verbose);
				}
				if (_depthBuffer != null)
				{
					_depthBuffer.Dispose();
					Gorgon.Log.Print("RenderImage", "Releasing depth buffer.", LoggingLevel.Verbose);
				}
				if (_renderTarget != null)
					_renderTarget.Dispose();

				Gorgon.Log.Print("RenderImage", "Render Image '{0}' destroyed.", LoggingLevel.Intermediate, _objectName);
			}

			_colorBuffer = null;
			_depthBuffer = null;
			_renderTarget = null;
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			Gorgon.Renderer.SetRenderTarget(null);

			// Remove the image since it won't get updated automatically.
			if (_renderTarget != null)
				_renderTarget.DeviceLost();

			// Turn off depth buffer.
			if (_depthBuffer != null)
				_depthBuffer.Dispose();
			_depthBuffer = null;

			if (_colorBuffer != null)
				_colorBuffer.Dispose();
			_colorBuffer = null;
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			// Rebuild the texture.
			if (_renderTarget != null)
				_renderTarget.DeviceReset();	
			CreateRenderTarget(_format, _useDepthBuffer, _useStencilBuffer, false);
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
