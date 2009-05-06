#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Sunday, July 10, 2005 6:53:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using Microsoft.Win32;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an abstract layer that will handle the rendering calls.
	/// </summary>
	internal class Renderer
		: IDisposable
	{
		#region Variables.
		private RenderStates _renderStates;								// Render states.		
		private Viewport _currentView;									// Current viewport.		
		private Image[] _currentImages;									// Current images.
		private VertexType _currentVertexType;							// Current vertex type.
		private ImageLayerStates[] _imageLayerStates;					// Layer states.
		private VertexTypeList _vertexTypes;							// List of predefined vertex types.
		private bool _isDisposed;										// Flag to indicate that the render has been disposed.
		private D3D9.Device _device = null;								// Device object.
		private Matrix _currentProjection = Matrix.Identity;			// Current projection matrix.
		private IndexBuffer _currentIndexBuffer = null;					// Current index buffer.
		private VertexBuffer _currentVertexBuffer = null;				// Current vertex buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether we can render yet.
		/// </summary>
		private bool CanRender
		{
			get
			{
				if (Gorgon.CurrentRenderTarget == null)
					return false;

				if (Gorgon.Screen.DeviceNotReset)
				{
					// Reset offsets for global buffers.
					Geometry.VerticesWritten = 0;
					Geometry.VertexOffset = 0;
					Gorgon.GlobalStateSettings.DeviceLost();
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Property to set or return the currently active Direct 3D device.
		/// </summary>
		private D3D9.Device D3DDevice
		{
			get
			{
				if ((Gorgon.Screen == null) || (Gorgon.Screen.DeviceNotReset))
					return null;

				if (_device == null)
					_device = Gorgon.Screen.Device;

				return _device;
			}
		}

		/// <summary>
		/// Property to return whether the D3D runtime is in debug mode or not.
		/// </summary>
		/// <returns>TRUE if in debug, FALSE if in retail.</returns>
		private bool IsD3DDebug
		{
			get
			{
				RegistryKey regKey = null;		// Registry key.
				int keyValue = 0;				// Registry key value.

				try
				{
					// Get the registry setting.
					regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Direct3D");

					if (regKey == null)
						return false;

					// Get value.
					keyValue = Convert.ToInt32(regKey.GetValue("LoadDebugRuntime", (int)0));
					if (keyValue != 0)
						return true;
				}
#if DEBUG
				catch (Exception ex)
#else
				catch
#endif
				{
#if DEBUG
                    MessageBox.Show("Unable to determine Direct 3D runtime settings.", ex.Message);
#endif
				}
				finally
				{
					if (regKey != null)
						regKey.Close();
					regKey = null;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return the render state list.
		/// </summary>
		public RenderStates RenderStates
		{
			get
			{
				return _renderStates;
			}
		}

		/// <summary>
		/// Property to return the states for the image layers.
		/// </summary>
		public ImageLayerStates[] ImageLayerStates
		{
			get
			{
				return _imageLayerStates;
			}
		}

		/// <summary>
		/// Property to return whether or not this object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get
			{
				return _isDisposed;
			}
		}

		/// <summary>
		/// Property to set or return the current projection matrix.
		/// </summary>
		public Matrix CurrentProjectionMatrix
		{
			get
			{
				return _currentProjection;
			}
			set
			{
				if (D3DDevice == null)
					return;

				_currentProjection = value;

				if (D3DDevice != null)
					D3DDevice.SetTransform(D3D9.TransformState.Projection, Converter.Convert(_currentProjection));
			}
		}

		/// <summary>
		/// Property to return the list of predefined vertex types.
		/// </summary>
		public VertexTypeList VertexTypes
		{
			get
			{
				return _vertexTypes;
			}
		}

		/// <summary>
		/// Property to return the currently active view port.
		/// </summary>
		public Viewport CurrentViewport
		{
			get
			{
				return _currentView;
			}
			set
			{
				if (D3DDevice != null)
				{
					// If null, then use the main default view port.
					if (value == null)
						value = Gorgon.CurrentRenderTarget.DefaultView;

					// Set the viewport.
					if ((_currentView != value) || (value.Updated))
					{
						D3DDevice.Viewport = Converter.Convert(value);
						value.Updated = false;
					}

					_currentView = value;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the number of indices depending on the primitive style being used.
		/// </summary>
		/// <param name="useindices">TRUE to if using indices, FALSE if not.</param>
		/// <param name="VertexCount">Number of vertices.</param>
		/// <param name="IndexCount">Number of indices.</param>
		/// <param name="style">Style of primitive to use.</param>
		/// <returns>Index count for the current vertex/index set.</returns>
		private int CalculateIndices(bool useindices, int VertexCount, int IndexCount, PrimitiveStyle style)
		{
			switch (style)
			{
				case PrimitiveStyle.PointList:
					return (useindices ? IndexCount : VertexCount);
				case PrimitiveStyle.LineList:
					return (useindices ? IndexCount : VertexCount) / 2;
				case PrimitiveStyle.LineStrip:
					return (useindices ? IndexCount : VertexCount) - 1;
				case PrimitiveStyle.TriangleList:
					return (useindices ? IndexCount : VertexCount) / 3;
				case PrimitiveStyle.TriangleStrip:
				case PrimitiveStyle.TriangleFan:
					return (useindices ? IndexCount : VertexCount) - 2;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Function to draw cached triangles.
		/// </summary>
		/// <param name="primitiveStyle">The type of triangle to draw.</param>
		/// <param name="vertexOffset">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexOffset">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		internal void DrawCachedTriangles(PrimitiveStyle primitiveStyle, int vertexOffset, int vertexCount, int indexOffset, int indexCount)
		{
			DX.Configuration.ThrowOnError = false;
			if ((indexCount > 0) && (this._currentIndexBuffer != null))
				D3DDevice.DrawIndexedPrimitives(Converter.Convert(primitiveStyle), vertexOffset, 0, vertexCount, indexOffset, CalculateIndices(true, 0, indexCount, primitiveStyle));
			else
				D3DDevice.DrawPrimitives(Converter.Convert(primitiveStyle), vertexOffset, CalculateIndices(false, vertexCount, 0, primitiveStyle));
			DX.Configuration.ThrowOnError = true;
		}

		/// <summary>
		/// Function to set the world transform.
		/// </summary>
		/// <param name="index">Index of the world transform to set.</param>
		/// <param name="matrix">Matrix to set as the transform.</param>
		public void SetWorldTransform(int index, Matrix matrix)
		{
			if (D3DDevice == null)
				return;

			D3DDevice.SetTransform(index, Converter.Convert(matrix));
		}

		/// <summary>
		/// Function to set the vertex type.
		/// </summary>
		/// <param name="vertexType">Type of vertex declaration to set.</param>
		public void SetVertexType(VertexType vertexType)
		{
			if (D3DDevice == null)
				return;

			if (_currentVertexType != vertexType)
			{
				// SetVertexDeclaration is expensive...
				if (vertexType != null)
					D3DDevice.VertexDeclaration = vertexType.D3DVertexDeclaration;
				else
					D3DDevice.VertexDeclaration = null;

				_currentVertexType = vertexType;
			}
		}

		/// <summary>
		/// Function to set the active index buffer.
		/// </summary>
		/// <param name="indices">The index buffer.</param>
		public void SetIndexBuffer(IndexBuffer indices)
		{
			if (D3DDevice == null)
				return;

			if (_currentIndexBuffer != indices)
			{
				_currentIndexBuffer = indices;

				if (_currentIndexBuffer != null)
					D3DDevice.Indices = _currentIndexBuffer.D3DIndexBuffer;
				else
					D3DDevice.Indices = null;
			}
		}

		/// <summary>
		/// Function to set the stream source data.
		/// </summary>
		/// <param name="vertexData">Vertex cache containing vertex buffers and stream information.</param>
		public void SetStreamData(VertexBuffer vertexData)
		{
			if (D3DDevice == null)
				return;

			if ((_currentVertexBuffer != vertexData) || (
				(Gorgon.CurrentDriver.Vendor == 0x8086) && (Gorgon.CurrentDriver.ID < 0x29A2)))		// This is a fix to bypass this check if you're running an older Intel (pre G?965) card.
			{
				_currentVertexBuffer = vertexData;

				if (_currentVertexBuffer != null)
					D3DDevice.SetStreamSource(0, _currentVertexBuffer.D3DVertexBuffer, 0, _currentVertexBuffer.VertexSize);
				else
					D3DDevice.SetStreamSource(0, null, 0, 0);
			}
		}

		/// <summary>
		/// Function to begin rendering.
		/// </summary>
		public void BeginRendering()
		{
			if (D3DDevice == null)
				return;

			DX.Configuration.ThrowOnError = false;
			D3DDevice.BeginScene();
			DX.Configuration.ThrowOnError = true;
		}

		/// <summary>
		/// Function to end rendering.
		/// </summary>
		public void EndRendering()
		{
			if (D3DDevice == null)
				return;

			D3DDevice.EndScene();
		}

		/// <summary>
		/// Function to initialize data after a device reset.
		/// </summary>
		public void Reset()
		{
			DeviceStateList.DeviceWasReset();

			if ((Gorgon.Screen != null) && (D3DDevice != null))
			{
				// Unbind streams.
				SetStreamData(null);

				// Turn off all textures.
				for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
					SetImage(i, null);

				// Turn off current vertex type (should get reset on next render pass).
				SetVertexType(null);
				SetIndexBuffer(null);

				// Reset the current render target.
				Gorgon.CurrentRenderTarget = null;
			}
		}

		/// <summary>
		/// Function to create an ortho projection matrix.
		/// </summary>
		/// <param name="dimensions">Dimensions to pass into the matrix.</param>
		/// <returns>An orthogonal matrix.</returns>
		public Matrix CreateOrthoProjectionMatrix(Drawing.RectangleF dimensions)
		{
			Matrix result = Matrix.Identity;	// Orthogonal matrix.

			// Invert vertical.
			dimensions.Y = -dimensions.Y;
			dimensions.Height = -dimensions.Height;

			result.m11 = 2.0f / (dimensions.Width);
			result.m22 = 2.0f / (dimensions.Height);
			result.m33 = -1.0f;
			result.m14 = (dimensions.Left + dimensions.Right) / (dimensions.Left - dimensions.Right);
			result.m24 = (dimensions.Top + dimensions.Bottom) / dimensions.Height;
			result.m34 = 0.0f;

			return result;
		}

		/// <summary>
		/// Function to create an ortho projection matrix.
		/// </summary>
		/// <param name="left">Horizontal position.</param>
		/// <param name="top">Vertical position.</param>
		/// <param name="width">Width of the view.</param>
		/// <param name="height">Height of the view.</param>
		/// <returns>An orthogonal matrix.</returns>
		public Matrix CreateOrthoProjectionMatrix(float left, float top, float width, float height)
		{
			return CreateOrthoProjectionMatrix(new Drawing.RectangleF(left, top, width, height));
		}

		/// <summary>
		/// Function to set the image.
		/// </summary>
		/// <param name="stage">Image stage to use.</param>
		/// <param name="image">Image to use.</param>
		public void SetImage(int stage, Image image)
		{
			if (D3DDevice == null)
				return;

			if (_currentImages[stage] != image)
			{
				if (image != null)
					D3DDevice.SetTexture(stage, image.D3DTexture);
				else
					D3DDevice.SetTexture(stage, null);

				_currentImages[stage] = image;
			}
		}

		/// <summary>
		/// Function to get the currently active image.
		/// </summary>
		/// <param name="stage">Image stage to use.</param>
		/// <returns>The current set image.</returns>
		public Image GetImage(int stage)
		{
			return _currentImages[stage];
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		/// <param name="stencilValue">Value to overwrite the stencil buffer with.</param>
		/// <param name="clearTargets">What to clear.</param>
		public void Clear(Drawing.Color color, float depthValue, int stencilValue, ClearTargets clearTargets)
		{
			D3D9.ClearFlags flags = SlimDX.Direct3D9.ClearFlags.None;		// Clear flags.

			if (D3DDevice == null)
				return;

			if (Gorgon.CurrentRenderTarget == null)
				return;

			// Do nothing.
			if (((clearTargets & ClearTargets.None) == ClearTargets.None) && (Gorgon.CurrentRenderTarget != Gorgon.Screen))
				return;

			// The primary window will -always- clear its backbuffer.
			if (((clearTargets & ClearTargets.BackBuffer) != 0) || (Gorgon.CurrentRenderTarget == Gorgon.Screen))
				flags |= D3D9.ClearFlags.Target;

			// If we have a depth buffer, overwrite it.
			if (((clearTargets & ClearTargets.DepthBuffer) != 0) && (Gorgon.CurrentRenderTarget.UseDepthBuffer))
				flags |= D3D9.ClearFlags.ZBuffer;

			// If we have a stencil buffer, overwrite it.
			if (((clearTargets & ClearTargets.StencilBuffer) != 0) && (Gorgon.CurrentRenderTarget.UseStencilBuffer))
				flags |= D3D9.ClearFlags.Stencil;

			D3DDevice.Clear(flags, color, depthValue, stencilValue);
		}

		/// <summary>
		/// Function to flip the backbuffer to the screen.
		/// </summary>
		/// <param name="target">Target to page flip.</param>
		public void Flip(RenderWindow target)
		{
			if (Gorgon.Screen == null)
				throw new GorgonException(GorgonErrors.NoDevice);

			if (target == null)
				target = Gorgon.Screen;

			// Attempt to reset the device if it's in a lost state.
			if (Gorgon.Screen.DeviceNotReset)
			{
				Gorgon.Screen.ResetLostDevice();
				return;
			}

			if ((target.D3DFlip() == D3D9.ResultCode.DeviceLost) && (!Gorgon.Screen.DeviceNotReset))
				Gorgon.Screen.ResetLostDevice();
		}

		/// <summary>
		/// Function to perform rendering using the default render geometry.
		/// </summary>
		public void Render()
		{
			// Do nothing if the active render target is to be drawn by the user.
			if ((Gorgon.CurrentRenderTarget == null) || (Geometry.VerticesWritten < 1))
				return;

			Render(Geometry.VertexType, Geometry.VertexBuffer, (Geometry.UseIndices ? Geometry.IndexBuffer : null), Geometry.PrimitiveStyle, Geometry.VertexOffset, Geometry.VerticesWritten, (Geometry.UseIndices ? Geometry.IndexOffset : 0), (Geometry.UseIndices ? Geometry.IndicesWritten : 0));

			// Flush.
			Geometry.VertexOffset = 0;
			Geometry.VerticesWritten = 0;
		}

		/// <summary>
		/// Function to perform rendering.
		/// </summary>
		/// <param name="vertexType">Type description of the vertex to render.</param>
		/// <param name="vertexBuffer">Vertex buffer to render.</param>
		/// <param name="indexBuffer">Optional index buffer.</param>
		/// <param name="primitiveStyle">Style of primitive to use.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		public void Render(VertexType vertexType, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			if (!CanRender)
				return;

			if (vertexType == null)
				throw new ArgumentNullException("vertexType");
			if (vertexBuffer == null)
				throw new ArgumentNullException("vertexBuffer");

			// Begin the rendering process.
			BeginRendering();
			
			SetVertexType(vertexType);
			SetStreamData(vertexBuffer);

			if ((indexBuffer != null) && (indexCount > 0))
				SetIndexBuffer(indexBuffer);

			// Begin shader.
			if (Gorgon.CurrentShader != null)
			{
				IShaderRenderer shader = Gorgon.CurrentShader as IShaderRenderer;
				if (shader != null)
				{
					shader.Begin();
					shader.Render(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
					shader.End();
				}
			}
			else
				DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);

			EndRendering();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal Renderer()
		{
			Gorgon.Log.Print("Renderer", "Starting renderer...", LoggingLevel.Simple);

			if (IsD3DDebug)
				Gorgon.Log.Print("Renderer", "[*WARNING*] The Direct 3D runtime is currently set to DEBUG mode.  Performance will be hindered. [*WARNING*]", LoggingLevel.Verbose);
#if INCLUDE_D3DREF
			if (Gorgon.UseReferenceDevice)
				Gorgon.Log.Print("Renderer", "[*WARNING*] The D3D device will be a REFERENCE device.  Performance will be greatly hindered. [*WARNING*]", LoggingLevel.All);
#endif
			_vertexTypes = new VertexTypeList();

			// Remove any left over state objects.
			DeviceStateList.Clear();

			// Default to common vertex type.
			_currentVertexType = null;

			// Recreate and reset the internal data.
			_currentView = null;

			// Default to windows control colors for the default material.
			_currentImages = new Image[Gorgon.CurrentDriver.MaximumTextureStages];

			// Create render state tracker.
			_renderStates = new RenderStates();

			// Set initial render states.
			_renderStates.AlphaBlendEnabled = true;
			_renderStates.SourceAlphaBlendOperation = AlphaBlendOperation.SourceAlpha;
			_renderStates.DestinationAlphaBlendOperation = AlphaBlendOperation.InverseSourceAlpha;
			_renderStates.DepthBufferEnabled = false;
			_renderStates.LightingEnabled = false;
			_renderStates.AlphaTestEnabled = true;
			_renderStates.AlphaTestFunction = CompareFunctions.GreaterThan;
			_renderStates.AlphaTestValue = 1;
			_renderStates.DrawLastPixel = true;
			_renderStates.ScissorTesting = false;

			// Set initial layer states.
			_imageLayerStates = new ImageLayerStates[Gorgon.CurrentDriver.MaximumTextureStages];
			for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
				_imageLayerStates[i] = new ImageLayerStates(i);

			// Set initial state.
			_imageLayerStates[0].ColorOperation = ImageOperations.Modulate;
			_imageLayerStates[0].ColorOperationArgument1 = ImageOperationArguments.Diffuse;
			_imageLayerStates[0].ColorOperationArgument2 = ImageOperationArguments.Texture;
			_imageLayerStates[0].AlphaOperation = ImageOperations.Modulate;
			_imageLayerStates[0].AlphaOperationArgument1 = ImageOperationArguments.Diffuse;
			_imageLayerStates[0].AlphaOperationArgument2 = ImageOperationArguments.Texture;
			_imageLayerStates[0].HorizontalAddressing = ImageAddressing.Clamp;
			_imageLayerStates[0].VerticalAddressing = ImageAddressing.Clamp;

			Gorgon.Log.Print("Renderer", "Renderer successfully initialized.", LoggingLevel.Intermediate);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to remove all objects, FALSE to remove only unmanaged.</param>
		private void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_isDisposed = true;

				if (_vertexTypes != null)
					_vertexTypes.Dispose();

				Gorgon.Log.Print("Renderer", "Renderer destroyed.", LoggingLevel.Simple);
			}

			DeviceStateList.Clear();
			_vertexTypes = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
