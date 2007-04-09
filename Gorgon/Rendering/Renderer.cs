#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Sunday, July 10, 2005 6:53:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using Microsoft.Win32;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Shaders;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing an abstract layer that will handle the rendering calls.
	/// </summary>
	public class Renderer 
	{
		#region Variables.
		private RenderTargetList _renderTargets;						// List of render targets.
		private D3D.Viewport _viewPort;									// D3D viewport.		
		private RenderStates _renderStates;								// Render state manager.		
		private RenderTarget _activeRenderTarget;						// Active render target.		
		private Viewport _activeView;									// Active window.		
		private Image[] _activeImage;									// Active image.
		private VertexType _currentVertexType;							// Current vertex type.
		private ImageLayerStates[] _imageLayerStates;					// Layer states.
		private VertexTypes _vertexTypes;								// List of predefined vertex types.
		private Shader _currentShader;									// Currently active shader.
		private ShaderTechnique _currentTechnique;						// Current shader technique.
		private Viewport _activeClipper;								// Active clipping view.
		#endregion

		#region Properties.
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
					UI.ErrorBox(null, "Unable to determine Direct3D runtime settings.", ex.Message);
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
		/// Property to determine if the device is ready for use.
		/// </summary>
		internal bool DeviceReady
		{
			get
			{
				if (Gorgon.Screen == null)
					return false;

				return ((Gorgon.Screen.Device != null) && (!Gorgon.Screen.DeviceNotReset));
			}
		}

		/// <summary>
		/// Property to return whether the device has been lost or not.
		/// </summary>
		internal bool DeviceIsLost
		{
			get
			{
				int coopResult;		// Cooperative level test result.

				if (!Gorgon.Screen.Device.CheckCooperativeLevel(out coopResult))
				{
					switch ((D3D.ResultCode)coopResult)
					{
						case D3D.ResultCode.DeviceNotReset:
							// HACK: Quick way to get the device to reset itself.
							Gorgon.Screen.Windowed = Gorgon.Screen.Windowed;
							break;
					}
				}
				else
					return false;

				return true;
			}
		}

		/// <summary>
		/// Property to return if the Direct 3D device has been created or not.
		/// </summary>
		internal bool DeviceCreated
		{
			get
			{
				return (Gorgon.Screen.Device != null);
			}
		}

		/// <summary>
		/// Property to return the render target list.
		/// </summary>
		internal RenderTargetList RenderTargets
		{
			get
			{
				return _renderTargets;
			}
		}

		/// <summary>
		/// Property to set or return the current clipping view port.
		/// </summary>
		public Viewport CurrentClippingView
		{
			get
			{
				return _activeClipper;
			}
			set
			{
				// If the clipper has changed, then re-apply.
				if (_activeClipper != value)
				{
					// If we're capable of scissor testing, use that instead.
					if (Gorgon.Driver.SupportScissorTesting)
					{
						// No clipper?  Then turn off the scissor testing.
						if (value == null)
							_renderStates.ScissorTesting = false;
						else
						{
							// Set the new clipping dimensions.
							_renderStates.ScissorTesting = true;
							_renderStates.ScissorRectangle = value.ClippedDimensions;
						}
					}
					else
					{
						// If we don't have scissor testing, then we need to set the view port
						// and projection matrix to force clipping.
						if (value == null)
						{
							// Perform clipping by altering the viewport & projection.
							if (_activeRenderTarget != null)
								SetActiveView(_activeRenderTarget.ProjectionMatrix, _activeRenderTarget.DefaultView);
						}
						else
							SetActiveView(value.ProjectionMatrix, value);
					}
				}

				// If we're not using scissor testing, and the actual viewport has been altered, but we
				// don't have a clipper already applied, then re-apply the default view from the active
				// render target.
				if ((value == null) && (_activeRenderTarget != null) && (_activeView != _activeRenderTarget.DefaultView) && (!Gorgon.Driver.SupportScissorTesting))
					SetActiveView(_activeRenderTarget.ProjectionMatrix, _activeRenderTarget.DefaultView);

				_activeClipper = value;
			}
		}

        /// <summary>
        /// Property to return an instance to the current renderer.
        /// </summary>
        public static Renderer Instance
        {
            get
            {                
                return Gorgon.Renderer;
            }
        }

		/// <summary>
		/// Property to return the list of predefined vertex types.
		/// </summary>
		public VertexTypes VertexTypes
		{
			get
			{
				return _vertexTypes;
			}
		}

		/// <summary>
		/// Property to return the currently active technique.
		/// </summary>
		public ShaderTechnique CurrentTechnique
		{
			get
			{
				return _currentTechnique;
			}
		}

		/// <summary>
		/// Property to set or return the currently active shader.
		/// </summary>
		public Shader CurrentShader
		{
			get
			{
				return _currentShader;
			}
			set
			{
				if (_currentShader != value)
					_currentShader = value;

				if (value != null)
					_currentTechnique = value.ActiveTechnique;
				else
					_currentTechnique = null;
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
		/// Property to return the currently active render target.
		/// </summary>
		public RenderTarget CurrentRenderTarget
		{
			get
			{
				return _activeRenderTarget;
			}
		}

		/// <summary>
		/// Property to return the currently active view port.
		/// </summary>
		public Viewport CurrentViewport
		{
			get
			{
				return _activeView;
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
		private int CalculateIndices(bool useindices,int VertexCount,int IndexCount,PrimitiveStyle style)
		{
			switch(style)
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
		/// Function to call the device lost event.
		/// </summary>
		internal void DeviceLost()
		{
			Gorgon.DeviceStateList.DeviceWasLost();
			Gorgon.Timer.Reset();

			Gorgon.Log.Print("Renderer", "Device has been put into a lost state.", LoggingLevel.Intermediate);
		}

		/// <summary>
		/// Functio to call the device reset event.
		/// </summary>
		internal void DeviceReset()
		{
			Gorgon.DeviceStateList.DeviceWasReset();

			if ((Gorgon.Screen != null) && (Gorgon.Screen.Device != null))
			{
				// The device has been reset.
				Gorgon.Screen.DeviceNotReset = false;

				// Unbind streams.
				SetStreamData(null);

				// Turn off all textures.
				for (int i = 0; i < Gorgon.Driver.MaximumTextureStages; i++)
					SetImage(i, null);
				// Turn off current vertex type (should get reset on next render pass).
				SetVertexType(null);
			}			
			Gorgon.Log.Print("Renderer", "Device has been reset from lost state.", LoggingLevel.Intermediate);
		}

		/// <summary>
		/// Function to release the resources of the renderer.
		/// </summary>
		internal void ReleaseResources()
		{
			if (_renderTargets != null)
				_renderTargets.Dispose();

			if (Gorgon.DeviceStateList != null)
			{
				Gorgon.DeviceStateList.ForceRelease();
				Gorgon.DeviceStateList.Clear();
			}

			// Recreate and reset the internal data.
			_renderTargets = null;
			_renderTargets = new RenderTargetList();
			_activeRenderTarget = null;
			_activeView = null;
			// Default to windows control colors for the default material.
			_activeImage = new Image[Gorgon.Driver.MaximumTextureStages];
			
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
			if (Gorgon.Driver.SupportScissorTesting)
				_renderStates.ScissorTesting = true;

			// Set initial layer states.
			_imageLayerStates = new ImageLayerStates[Gorgon.Driver.MaximumTextureStages];
			for (int i = 0; i < Gorgon.Driver.MaximumTextureStages;i++)
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
		}

		/// <summary>
		/// Function to create an ortho matrix.
		/// </summary>
		/// <param name="left">Horizontal position.</param>
		/// <param name="top">Vertical position.</param>
		/// <param name="width">Width to use in calculations.</param>
		/// <param name="height">Height to use in calculations.</param>
		/// <returns>An orthogonal matrix.</returns>
		public Matrix CreateOrthoMatrix(int left, int top, int width, int height)
		{
			Rectangle3DF projectionBounds;			// Projection matrix bounds.
			Matrix orthoMatrix = Matrix.Identity;	// Orthogonal matrix.

			projectionBounds = new Rectangle3DF(left, -top, 0, width, -height, 1.0f);
			//projectionBounds.Width = MathUtility.Abs(projectionBounds.Height * aspectRatio);

			orthoMatrix.m11 = 2.0f / (projectionBounds.Width);
			orthoMatrix.m22 = 2.0f / (projectionBounds.Height);
			orthoMatrix.m33 = 1.0f / (projectionBounds.Near - projectionBounds.Far);
			orthoMatrix.m14 = (projectionBounds.Left + projectionBounds.Right) / (projectionBounds.Left - projectionBounds.Right);
			orthoMatrix.m24 = (projectionBounds.Top + projectionBounds.Bottom) / projectionBounds.Height;
			orthoMatrix.m34 = projectionBounds.Near / (projectionBounds.Near - projectionBounds.Far);

			return orthoMatrix;
		}

		/// <summary>
		/// Function to set the image.
		/// </summary>
		/// <param name="stage">Image stage to use.</param>
		/// <param name="image">Image to use.</param>
		public void SetImage(int stage, Image image)
		{
			if ((_activeImage[stage] != image) && (DeviceReady))
			{
				if (image != null)
					Gorgon.Screen.Device.SetTexture(stage, image.D3DTexture);
				else
					Gorgon.Screen.Device.SetTexture(stage, null);

				_activeImage[stage] = image;
			}			
		}

		/// <summary>
		/// Function to get the currently active image.
		/// </summary>
		/// <param name="stage">Image stage to use.</param>
		/// <returns>The current set image.</returns>
		public Image GetImage(int stage)
		{
			return _activeImage[stage];
		}

		/// <summary>
		/// Function to begin rendering.
		/// </summary>
		public void BeginRendering()
		{
			if ((Gorgon.Screen == null) || (Gorgon.Screen.Device == null))
				throw new NoDeviceObjectException(null);

			if ((Gorgon.Screen.DeviceNotReset) || (DeviceIsLost))
			{
				if (!DeviceReady)
					return;
			}

			D3D.Direct3DXException.IsExceptionIgnored = true;
			Gorgon.Screen.Device.BeginScene();			
			D3D.Direct3DXException.IsExceptionIgnored = false;
		}

		/// <summary>
		/// Function to end rendering.
		/// </summary>
		public void EndRendering()
		{
			if ((Gorgon.Screen == null) || (Gorgon.Screen.Device == null))
				throw new NoDeviceObjectException(null);

			if ((Gorgon.Screen.DeviceNotReset) || (DeviceIsLost))
				return;

			Gorgon.Screen.Device.EndScene();
		}

		/// <summary>
		/// Function to force triangles to the screen.
		/// </summary>
		/// <param name="primitiveStyle">Style of primitive to use.</param>
		/// <param name="indices">Indices to render.</param>
		/// <param name="vertices">Geometry to render.</param>
		public void DrawTriangles<V, I>(PrimitiveStyle primitiveStyle, IndexCache<I> indices, VertexCache<V> vertices) 
			where V : struct
		{
		    if (!DeviceReady)
		        return;

			if (indices != null)
			{
				SetIndexBuffer(indices.IndexBuffer);
				Gorgon.Screen.Device.DrawIndexedPrimitives(Converter.Convert(primitiveStyle), vertices.DataOffset, 0, vertices.DataWritten, indices.DataOffset, CalculateIndices(true, 0, indices.DataWritten, primitiveStyle));
			}
			else
				Gorgon.Screen.Device.DrawPrimitives(Converter.Convert(primitiveStyle), vertices.DataOffset, CalculateIndices(false, vertices.DataWritten, 0, primitiveStyle));
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
			if (Gorgon.Screen.Device == null)
				throw new NoDeviceObjectException(null);

			if (_activeRenderTarget == null)
				return;

			if (!DeviceReady)
				return;

			if ((clearTargets & ClearTargets.BackBuffer) != 0)
				Gorgon.Screen.Device.Clear(D3D.ClearFlags.Target, color, 1.0f, 0);

			// If we have a depth buffer, overwrite it.
			if (((clearTargets & ClearTargets.DepthBuffer) != 0) && (_activeRenderTarget.UseDepthBuffer))
				Gorgon.Screen.Device.Clear(D3D.ClearFlags.ZBuffer, 0, depthValue, 0);

			// If we have a stencil buffer, overwrite it.
			if (((clearTargets & ClearTargets.StencilBuffer) != 0) && (_activeRenderTarget.UseStencilBuffer))
				Gorgon.Screen.Device.Clear(D3D.ClearFlags.Stencil, 0, 1.0f, stencilValue);
		}

		/// <summary>
		/// Function to return whether this format can be used for hardware accelerated rendering.
		/// </summary>
		/// <param name="driverindex">Index of the driver we're using.</param>
		/// <param name="display">Format of the display to check.</param>
		/// <param name="backbuffer">Back buffer format.</param>
		/// <param name="windowed">TRUE if we're going to be windowed, FALSE if not.</param>
		/// <returns>TRUE if this format can be used, FALSE if not.</returns>
		static internal bool ValidFormat(int driverindex, D3D.Format display, D3D.Format backbuffer, bool windowed)
		{
			return D3D.Manager.CheckDeviceType(driverindex, RenderWindow.DeviceType, display, backbuffer, windowed);
		}

		/// <summary>
		/// Function to set the vertex type.
		/// </summary>
		/// <param name="vertexType">Type of vertex declaration to set.</param>
		public void SetVertexType(VertexType vertexType)
		{
			if ((_currentVertexType != vertexType) && (DeviceReady))
			{
				// SetVertexDeclaration is expensive...
				if (vertexType != null)
					Gorgon.Screen.Device.VertexDeclaration = vertexType.D3DVertexDeclaration;
				else
					Gorgon.Screen.Device.VertexDeclaration = null;

				_currentVertexType = vertexType;
			}			
		}

		/// <summary>
		/// Function to set the active index buffer.
		/// </summary>
		/// <param name="indices">The index buffer.</param>
		public void SetIndexBuffer(IndexBuffer indices)
		{
			if (DeviceReady)
				Gorgon.Screen.Device.Indices = indices.D3DIndexBuffer;
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		/// <param name="clearTargets">What should be cleared.</param>
		public void Clear(Drawing.Color color, float depthValue, ClearTargets clearTargets)
		{
			Clear(color, depthValue, 0, clearTargets);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="clearTargets">What should be cleared.</param>
		public void Clear(Drawing.Color color, ClearTargets clearTargets)
		{
			Clear(color, 1.0f, 0, clearTargets);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="clearTargets">What should be cleared.</param>
		public void Clear(ClearTargets clearTargets)
		{
			Clear(Drawing.Color.Black, 1.0f, 0,clearTargets);
		}

		/// <summary>
		/// Function to flip the backbuffer to the screen.
		/// </summary>
		public void Flip()
		{
			if ((Gorgon.Screen == null) || (Gorgon.Screen.Device == null))
				throw new NoDeviceObjectException(null);

			try
			{
				// If we do not have a render target, then exit.
				if ((_activeRenderTarget == null) || (DeviceIsLost))
				{
					Gorgon.Screen.DeviceNotReset = true;
					return;
				}
				else
					Gorgon.Screen.DeviceNotReset = false;

				if ((_activeRenderTarget == Gorgon.Screen) && (DeviceReady))
					Gorgon.Screen.Device.Present();
				else
				{
					// Do not present until we're sure the chain has been recreated.
					if (!Gorgon.Screen.DeviceNotReset)
					{
						if (_activeRenderTarget is RenderWindow)
							((RenderWindow)_activeRenderTarget).SwapChain.Present();
					}
				}
			}
			catch (D3D.DeviceLostException)
			{
				if (!Gorgon.Screen.DeviceNotReset)
					DeviceLost();
				if (Gorgon.Screen != null)
					Gorgon.Screen.DeviceNotReset = true;
			}
		}

		/// <summary>
		/// Function to set a world matrix.
		/// </summary>
		/// <param name="index">Index of the matrix to set.</param>
		/// <param name="matrix">Matrix to set.</param>
		public void SetWorldMatrix(int index, Matrix matrix)
		{
			if (!DeviceReady)
				return;

			switch (index)
			{
				case 1:
					Gorgon.Screen.Device.Transform.World1 = Converter.Convert(matrix);
					return;
				case 2:
					Gorgon.Screen.Device.Transform.World2 = Converter.Convert(matrix);
					return;
				case 3:
					Gorgon.Screen.Device.Transform.World3 = Converter.Convert(matrix);
					return;
			}

			Gorgon.Screen.Device.Transform.World = Converter.Convert(matrix);
		}

		/// <summary>
		/// Function to set a render target.
		/// </summary>
		/// <param name="target">Render target to set.</param>
		public void SetRenderTarget(RenderTarget target)
		{
			// Set render target if it's changed.
			if ((_activeRenderTarget != target) && (target != null) && (DeviceReady))
			{				
				Gorgon.Screen.Device.SetRenderTarget(0, target.SurfaceBuffer);
				Gorgon.Screen.Device.DepthStencilSurface = target.DepthBuffer;

				target.DefaultView.Refresh();

				// Reset the active view.
				_activeView = null;
				target.ProjectionNeedsUpdate = true;
			}
			
			_activeRenderTarget = target;			
		}

		/// <summary>
		/// Function to set the view port.
		/// </summary>
		/// <remarks>This function will override any clipping parameters.</remarks>
		/// <param name="projection">Projection matrix.</param>
		/// <param name="view">Viewport to set as active.</param>
		public void SetActiveView(Matrix projection, Viewport view)
		{
			if ((view != null) && ((view != _activeView) || (view.Updated)) && (_activeRenderTarget != null))
			{
				if (DeviceReady)
				{
					// Disable clipping.
					if (_renderStates.ScissorTesting)
						_renderStates.ScissorTesting = false;

					D3D.Viewport d3dview = new D3D.Viewport();		// D3D view.

					d3dview.X = view.View.Left;
					d3dview.Y = view.View.Top;
					d3dview.Width = view.View.Width;
					d3dview.Height = view.View.Height;
					d3dview.MinZ = 0.0f;
					d3dview.MaxZ = 1.0f;

					Gorgon.Screen.Device.Viewport = d3dview;
					Gorgon.Screen.Device.Transform.Projection = Converter.Convert(projection);
					_activeView = view;
					_activeView.Updated = false;
				}
			}
		}

		/// <summary>
		/// Function to set the stream source data.
		/// </summary>
		/// <param name="vertexData">Vertex cache containing vertex buffers and stream information.</param>
		public void SetStreamData(VertexBuffer vertexData)
		{
			try
			{
				if (!DeviceReady)
					return;

				if (vertexData != null)
					Gorgon.Screen.Device.SetStreamSource(0, vertexData.D3DVertexBuffer, 0, vertexData.VertexSize);
				else
					Gorgon.Screen.Device.SetStreamSource(0, null, 0, 0);
			}
			catch(Exception ex)
			{
				throw new CannotSetStreamException(ex);
			}
		}

		/// <summary>
		/// Function to perform rendering.
		/// </summary>
		public void Render()
		{
			int passCount = 1;							// How many passes?
			D3D.Effect shaderEffect = null;				// Shader effect.
			StateManager.RenderManager manager = null;	// Render manager.

			if (!DeviceReady)
			{
				// Reset offsets for buffers.
				Gorgon.StateManager.RenderData.VerticesWritten = 0;
				Gorgon.StateManager.RenderData.VertexOffset = 0;
				Gorgon.StateManager.DeviceLost();
				return;
			}

			// Do nothing if the active render target is to be drawn by the user.
			if (_activeRenderTarget == null)
				return;

			manager = Gorgon.StateManager.RenderData;

			// Begin the rendering process.
			BeginRendering();

			if (manager.VerticesWritten > 0)
			{
				SetVertexType(manager.VertexType);
				SetStreamData(manager.VertexBuffer);
				if ((manager.UseIndices) && (manager.IndicesWritten > 0))
				{
					SetIndexBuffer(manager.IndexBuffer);

					// Begin shader.
					if ((_currentShader != null) && (_currentTechnique != null))
					{
						// Commit any parameters.
						_currentShader.CommitParameters();
						if (_currentTechnique.Parameters.Count > 0)
							_currentTechnique.CommitParameters();

						shaderEffect = _currentShader.D3DEffect;
						shaderEffect.Technique = _currentTechnique.D3DEffectHandle;
						shaderEffect.Begin(D3D.FX.None);

						passCount = _currentTechnique.Passes.Count;

						// If we don't have any passes, then exit.
						if (passCount < 0)
							passCount = 1;
					}
					else
						_currentTechnique = null;

					for (int pass = 0; pass < passCount; pass++)
					{
						if (shaderEffect != null)
							shaderEffect.BeginPass(pass);

						DX.DirectXException.IsExceptionIgnored = true;
						Gorgon.Screen.Device.DrawIndexedPrimitives(Converter.Convert(manager.PrimitiveStyle), manager.VertexOffset, 0, manager.VerticesWritten, manager.IndexOffset, CalculateIndices(true, 0, manager.IndicesWritten, manager.PrimitiveStyle));
						DX.DirectXException.IsExceptionIgnored = false;

						if (shaderEffect != null)
							shaderEffect.EndPass();
					}

					// End shader.
					if (shaderEffect != null)
						shaderEffect.End();
				}
				else
				{
					// TODO: Apply shader code here?
					DX.DirectXException.IsExceptionIgnored = true;
					Gorgon.Screen.Device.DrawPrimitives(Converter.Convert(manager.PrimitiveStyle), manager.VertexOffset, CalculateIndices(false, manager.VerticesWritten, 0, manager.PrimitiveStyle));
					DX.DirectXException.IsExceptionIgnored = false;
				}

				// Flush.
				manager.VertexOffset = 0;
				manager.VerticesWritten = 0;
			}

			EndRendering();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public Renderer()
		{
			Gorgon.Log.Print("Renderer", "Starting renderer...",LoggingLevel.Simple);

			D3D.Device.IsUsingEventHandlers = false;			
			if (IsD3DDebug)
				Gorgon.Log.Print("Renderer", "[*WARNING*] The Direct 3D runtime is currently set to DEBUG mode.  Performance will be hindered. [*WARNING*]", LoggingLevel.Verbose);
#if INCLUDE_D3DREF
			if (Gorgon.UseReferenceDevice)
				Gorgon.Log.Print("Renderer", "[*WARNING*] The D3D device will be a REFERENCE device.  Performance will be greatly hindered. [*WARNING*]", LoggingLevel.All);
#endif

			_currentVertexType = null;			
			_viewPort = new D3D.Viewport();
			_vertexTypes = new VertexTypes();

			Gorgon.Log.Print("Renderer", "Renderer successfully initialized.", LoggingLevel.Intermediate);

			// Reset states.			
			ReleaseResources();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~Renderer()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to remove all objects, FALSE to remove only unmanaged.</param>
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_renderTargets != null)
					_renderTargets.Dispose();

				if (_vertexTypes != null)
					_vertexTypes.Dispose();

				Gorgon.Log.Print("Renderer","Renderer destroyed.",LoggingLevel.Simple);
			}

			_renderTargets = null;
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
