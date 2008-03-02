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
// Created: Wednesday, May 17, 2006 10:50:26 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities.Utility;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing the various renderstates.
	/// </summary>
	public class RenderStates
	{
		#region Variables.
		private CullingMode _cullingMode;						// Culling mode.		
		private float _vertexPointSize;							// Size of vertex points.		
		private bool _normalize;								// Renormalize flag.		
		private bool _depthBufferEnabled;						// Flag to indicate that the depth buffer is enabled or not.		
		private ShadingMode _shadingMode;						// Shading mode.	
		private AlphaBlendOperation _sourceAlphaOperation;		// Source alpha operation mode.
		private AlphaBlendOperation _destinationAlphaOperation;	// Destination alpha operation mode.
		private bool _depthBufferWriteEnabled;					// Flag to indicate that depth buffer writing is enabled or not.		
		private float _depthBufferBias;							// Depth buffer biasing.		
		private bool _lightingEnabled;							// Flag to indicate that lighting is enabled.		
		private bool _specularEnabled;							// Flag to indicate that specular lighting is enabled.		
		private bool _useWBuffer;								// Flag to use the W Buffer for 16 bit depth buffers.		
		private CompareFunctions _depthTestFunction;			// Depth testing function.		
		private DrawingMode _drawingMode;						// Drawing mode for the scene.
		private bool _alphaBlendEnabled;						// Flag to indicate that alpha blending is enabled.
		private bool _alphaTestEnabled;							// Flag to indicate that alpha comparison is on or off.
		private CompareFunctions _alphaTestFunction;			// Function used for alpha testing.
		private int _alphaTestValue;							// Value used for alpha testing.
		private bool _drawLastPixel;							// Flag to indicate whether D3D should draw the last pixel or not.
		private Drawing.Rectangle _scissorRectangle;			// Scissor rectangle.
		private bool _scissorTest;								// Flag to indicate whether scissor testing is enabled.
		private bool _stencilEnabled;							// Flag to indicate that the stencil buffer should be enabled.
		private int _stencilReference;							// Reference value for stencil operations.
		private int _stencilMask;								// Mask value for stencil operations.
		private StencilOperations _stencilPassOperation;		// Stencil pass operation.
		private StencilOperations _stencilFailOperation;		// Stencil failure operation.
		private StencilOperations _stencilZFailOperation;		// Stencil z-buffer failure operation.
		private CompareFunctions _stencilCompare;				// Stencil testing function.
		private bool _ditherEnabled;                            // Flag to indicate whether dithering is enabled or not.
		private D3D9.Device _device = null;						// D3D device object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the device is ready or not.
		/// </summary>
		private bool DeviceReady
		{
			get
			{
				if (Device == null)
					return false;

				return !Gorgon.Screen.DeviceNotReset;
			}
		}

		/// <summary>
		/// Property to return the device object.
		/// </summary>
		private D3D9.Device Device
		{
			get
			{
				if (Gorgon.Screen == null)
					return null;

				if (_device == null)
					_device = Gorgon.Screen.Device;

				return _device;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the stencil buffer or not.
		/// </summary>
		public bool StencilEnable
		{
			get
			{
				return _stencilEnabled;
			}
			set
			{
				if (value == _stencilEnabled)
					return;

				SetStencilEnabled(value);
				_stencilEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		public int StencilReference
		{
			get
			{
				return _stencilReference;
			}
			set
			{
				if (value == _stencilReference)
					return;

				SetStencilReference(value);
				_stencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		public int StencilMask
		{
			get
			{
				return _stencilMask;
			}
			set
			{
				if (value == _stencilMask)
					return;

				SetStencilMask(value);
				_stencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		public StencilOperations StencilPassOperation
		{
			get
			{
				return _stencilPassOperation;
			}
			set
			{
				if (value == _stencilPassOperation)
					return;

				SetStencilPassOperation(value);
				_stencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		public StencilOperations StencilFailOperation
		{
			get
			{
				return _stencilFailOperation;
			}
			set
			{
				if (value == _stencilFailOperation)
					return;

				SetStencilFailOperation(value);
				_stencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		public StencilOperations StencilZFailOperation
		{
			get
			{
				return _stencilZFailOperation;
			}
			set
			{
				if (value == _stencilZFailOperation)
					return;

				SetStencilZFailOperation(value);
				_stencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		public CompareFunctions StencilCompare
		{
			get
			{
				return _stencilCompare;
			}
			set
			{
				if (value == _stencilCompare)
					return;

				SetStencilCompare(value);
				_stencilCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return whether scissor testing is enabled.
		/// </summary>
		public bool ScissorTesting
		{
			get
			{
				return _scissorTest;
			}
			set
			{
				if (value == _scissorTest)
					return;

				// Reset the scissor rectangle.
				_scissorRectangle = Drawing.Rectangle.Empty;

				if (Gorgon.CurrentDriver.SupportScissorTesting)
				{
					SetScissorTest(value);
					_scissorTest = value;
				}
				else
					_scissorTest = false;
			}
		}

		/// <summary>
		/// Property to set or return the scissor test rectangle.
		/// </summary>
		public Drawing.Rectangle ScissorRectangle
		{
			get
			{
				return _scissorRectangle;
			}
			set
			{
				if (value == _scissorRectangle)
					return;

				if (Gorgon.CurrentDriver.SupportScissorTesting)
				{
					SetScissorRectangle(value);
					_scissorRectangle = value;
				}
				else
					_scissorRectangle = Drawing.Rectangle.Empty;
			}
		}

		/// <summary>
		/// Property to set or return whether D3D should draw the last pixel in a primitive operation.
		/// </summary>
		public bool DrawLastPixel
		{
			get
			{
				return _drawLastPixel;
			}
			set
			{
				if (value == _drawLastPixel)
					return;

				SetDrawLastPixel(value);
				_drawLastPixel = value;
			}
		}

		/// <summary>
		/// Property to set or return the culling mode used.
		/// </summary>
		public CullingMode CullingMode
		{
			get
			{
				return _cullingMode;
			}
			set
			{
				if (value == _cullingMode)
					return;

				SetCullMode(value);
				_cullingMode = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the scene should be renormalized.
		/// </summary>
		public bool Normalize
		{
			get
			{
				return _normalize;
			}
			set
			{
				if (_normalize == value)
					return;

				SetNormalize(value);
				_normalize = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the depth buffer is enabled or disabled.
		/// </summary>
		public bool DepthBufferEnabled
		{
			get
			{
				return _depthBufferEnabled;
			}
			set
			{
				if (_depthBufferEnabled == value)
					return;

				SetDepthBufferEnabled(value);
				if (value)
				{
					if (_useWBuffer)
						SetWBufferEnabled(true);
					else
						SetWBufferEnabled(false);
				}
				_depthBufferEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we can write to the depth buffer or not.
		/// </summary>
		public bool DepthBufferWriteEnabled
		{
			get
			{
				return _depthBufferWriteEnabled;
			}
			set
			{
				if (_depthBufferWriteEnabled == value)
					return;

				if (_depthBufferEnabled)
					SetDepthBufferWriteEnabled(value);

				_depthBufferWriteEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether lighting is enabled or not.
		/// </summary>
		public bool LightingEnabled
		{
			get
			{
				return _lightingEnabled;
			}
			set
			{
				if (_lightingEnabled == value)
					return;
				SetLightingEnabled(value);
				_lightingEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return the shading mode.
		/// </summary>
		public ShadingMode ShadingMode
		{
			get
			{
				return _shadingMode;
			}
			set
			{
				if (_shadingMode == value)
					return;
				SetShadingMode(value);
				_shadingMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer test comparison function.
		/// </summary>
		public CompareFunctions DepthTestFunction
		{
			get
			{
				return _depthTestFunction;
			}
			set
			{
				if (_depthTestFunction == value)
					return;

				if (_depthBufferEnabled)
					SetDepthBufferCompareFunction(value);
				_depthTestFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer biasing.
		/// </summary>
		public float DepthBias
		{
			get
			{
				return _depthBufferBias;
			}
			set
			{
				if (_depthBufferBias == value)
					return;

				if (_depthBufferEnabled)
					SetDepthBufferBias(value);
				_depthBufferBias = value;
			}
		}

		/// <summary>
		/// Property to set or return whether specular hilights are enabled or not.
		/// </summary>
		public bool SpecularEnabled
		{
			get
			{
				return _specularEnabled;
			}
			set
			{
				if (_specularEnabled == value)
					return;
				SetSpecularEnabled(value);
				_specularEnabled = value;
			}
		}

		/// <summary>
		/// Property to return or set the drawing mode for the scene.
		/// </summary>
		public DrawingMode DrawingMode
		{
			get
			{
				return _drawingMode;
			}
			set
			{
				if (_drawingMode == value)
					return;
				SetDrawingMode(value);
				_drawingMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the size of individual vertex points.
		/// </summary>
		public float PointSize
		{
			get
			{
				return _vertexPointSize;
			}
			set
			{
				if (_vertexPointSize == value)
					return;

				if (value < 1.0f)
					value = 1.0f;

				if (value > Gorgon.CurrentDriver.MaximumPointSize)
					value = Gorgon.CurrentDriver.MaximumPointSize;
				SetVertexPointSize(value);
				_vertexPointSize = value;
			}
		}

		/// <summary>
		/// Property to return whether the W buffer can be used or not.
		/// </summary>
		public bool UsingWBuffer
		{
			get
			{
				return _useWBuffer;
			}
		}

		/// <summary>
		/// Property to set or return whether alpha blending is enabled or not.
		/// </summary>
		public bool AlphaBlendEnabled
		{
			get
			{
				return _alphaBlendEnabled;
			}
			set
			{
				if (_alphaBlendEnabled == value)
					return;
				SetAlphaBlendEnabled(value);
				_alphaBlendEnabled = value;
			}
		}

		/// <summary>
		/// Property to set the source alpha blending operation.
		/// </summary>
		public AlphaBlendOperation SourceAlphaBlendOperation
		{
			get
			{
				return _sourceAlphaOperation;
			}
			set
			{
				if (_sourceAlphaOperation == value)
					return;
				SetSourceAlphaOperation(value);
				_sourceAlphaOperation = value;
			}
		}

		/// <summary>
		/// Property to set the destination alpha blending operation.
		/// </summary>
		public AlphaBlendOperation DestinationAlphaBlendOperation
		{
			get
			{
				return _destinationAlphaOperation;
			}
			set
			{
				if (_destinationAlphaOperation == value)
					return;
				SetDestinationAlphaOperation(value);
				_destinationAlphaOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the function used for alpha testing.
		/// </summary>
		public CompareFunctions AlphaTestFunction
		{
			get
			{
				return _alphaTestFunction;
			}
			set
			{
				if (_alphaTestFunction == value)
					return;

				SetAlphaTestFunction(value);
				_alphaTestFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the reference value used for alpha testing.
		/// </summary>
		public int AlphaTestValue
		{
			get
			{
				return _alphaTestValue;
			}
			set
			{
				if (_alphaTestValue == value)
					return;

				// Mask to the last 8 bits.
				value &= 0x000000FF;

				SetAlphaTestValue(value);
				_alphaTestValue = value;
			}
		}

		/// <summary>
		/// Property to set or return whether alpha testing is enabled or not.
		/// </summary>
		public bool AlphaTestEnabled
		{
			get
			{
				return _alphaTestEnabled;
			}
			set
			{
				if (_alphaTestEnabled == value)
					return;

				SetAlphaTestEnabled(value);
				_alphaTestEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether dithering is enabled or not.
		/// </summary>
		public bool DitheringEnabled
		{
			get
			{
				return _ditherEnabled;
			}
			set
			{
				if (_ditherEnabled == value)
					return;

				SetDitherEnabled(value);
				_ditherEnabled = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enable/disable scissor testing.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetScissorTest(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.ScissorTestEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the scissor test rectangle.
		/// </summary>
		/// <param name="value">Rectangle to use for scissor testing.</param>
		private void SetScissorRectangle(Drawing.Rectangle value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.ScissorRect = value;
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the alpha testing function.
		/// </summary>
		/// <param name="value">Function used for testing.</param>
		private void SetAlphaTestFunction(CompareFunctions value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Compare>(D3D9.RenderState.AlphaFunc, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the alpha testing reference value.
		/// </summary>
		/// <param name="value">Value to use for testing.</param>
		private void SetAlphaTestValue(int value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.AlphaRef, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set whether or not the alpha testing enabled.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetAlphaTestEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.AlphaTestEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the source alpha blending operation.
		/// </summary>
		/// <param name="value">Blend operation for the source.</param>
		private void SetSourceAlphaOperation(AlphaBlendOperation value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Blend>(D3D9.RenderState.SourceBlend, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the destination alpha blending operation.
		/// </summary>
		/// <param name="value">Blend operation for the destination.</param>
		private void SetDestinationAlphaOperation(AlphaBlendOperation value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Blend>(D3D9.RenderState.DestinationBlend, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set whether alpha blending is enabled or not.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetAlphaBlendEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.AlphaBlendEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the culling mode.
		/// </summary>
		/// <param name="value">Culling mode to set.</param>
		private void SetCullMode(CullingMode value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Cull>(D3D9.RenderState.CullMode, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the vertex point size.
		/// </summary>
		/// <param name="value">Size of the vertex point.</param>
		private void SetVertexPointSize(float value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.PointSize, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the re-normalize flag.
		/// </summary>
		/// <param name="value">TRUE to renormalize the normals, FALSE to leave them alone.</param>
		private void SetNormalize(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.NormalizeNormals, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the depth buffer enabled flag.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetDepthBufferEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
			{
				if (value)
					Device.SetRenderState<D3D9.ZBufferType>(D3D9.RenderState.ZEnable, D3D9.ZBufferType.UseZBuffer);
				else
					Device.SetRenderState<D3D9.ZBufferType>(D3D9.RenderState.ZEnable, D3D9.ZBufferType.DontUseZBuffer);
			}
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the shading mode.
		/// </summary>
		/// <param name="value">Shading mode.</param>
		private void SetShadingMode(ShadingMode value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.ShadeMode>(D3D9.RenderState.ShadeMode, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the depth buffer writing enabled flag.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetDepthBufferWriteEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.ZWriteEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the lighting enabled flag.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetLightingEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.Lighting, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the specular highlighting enabled flag.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetSpecularEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.SpecularEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the WBuffer enabled flag.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetWBufferEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if ((Gorgon.Screen != null) && (Gorgon.Screen.UseDepthBuffer))
			{

				if (value)
					Gorgon.Log.Print("RenderStates", "Using WBuffer for Depth buffer.", LoggingLevel.Verbose);
				else
					Gorgon.Log.Print("RenderStates", "Using ZBuffer for Depth buffer.", LoggingLevel.Verbose);

				if (DeviceReady)
					Device.SetRenderState<D3D9.ZBufferType>(D3D9.RenderState.ZEnable, D3D9.ZBufferType.UseWBuffer);
			}
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the depth buffer bias.
		/// </summary>
		/// <param name="value">Depth buffer bias value.</param>
		private void SetDepthBufferBias(float value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.DepthBias, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the depth buffer comparison function.
		/// </summary>
		/// <param name="value">Depth buffer comparison function.</param>
		private void SetDepthBufferCompareFunction(CompareFunctions value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Compare>(D3D9.RenderState.ZFunc, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the drawing mode.
		/// </summary>
		/// <param name="value">The triangle drawing mode.</param>
		private void SetDrawingMode(DrawingMode value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.FillMode>(D3D9.RenderState.FillMode, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set whether the last pixel should be drawn or not.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetDrawLastPixel(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.LastPixel, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set whether the stencil buffer is enabled or not.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE if not.</param>
		private void SetStencilEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.StencilEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the stencil buffer reference value.
		/// </summary>
		/// <param name="value">Reference value for the stencil buffer.</param>
		private void SetStencilReference(int value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.StencilRef, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the stencil buffer mask value.
		/// </summary>
		/// <param name="value">Mask value for the stencil buffer.</param>
		private void SetStencilMask(int value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.StencilMask, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the pass stencil operation.
		/// </summary>
		/// <param name="value">Operation for pass.</param>
		private void SetStencilPassOperation(StencilOperations value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.StencilOperation>(D3D9.RenderState.StencilPass, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the fail stencil operation.
		/// </summary>
		/// <param name="value">Operation for fail.</param>
		private void SetStencilFailOperation(StencilOperations value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.StencilOperation>(D3D9.RenderState.StencilFail, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the z-fail stencil operation.
		/// </summary>
		/// <param name="value">Operation for z-fail.</param>
		private void SetStencilZFailOperation(StencilOperations value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.StencilOperation>(D3D9.RenderState.StencilZFail, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set the comparison stencil operation.
		/// </summary>
		/// <param name="value">Comparison operation.</param>
		private void SetStencilCompare(CompareFunctions value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState<D3D9.Compare>(D3D9.RenderState.StencilFunc, Converter.Convert(value));
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to set whether dithering is enabled or not.
		/// </summary>
		/// <param name="value">TRUE to enable, FALSE to disable.</param>
		private void SetDitherEnabled(bool value)
		{
			DX.Configuration.AlwaysThrowOnError = false;
			if (DeviceReady)
				Device.SetRenderState(D3D9.RenderState.DitherEnable, value);
			DX.Configuration.AlwaysThrowOnError = true;
		}

		/// <summary>
		/// Function to enable or disable W buffer support for 16 bit depth buffers.
		/// </summary>
		public void CheckForWBuffer(DepthBufferFormats targetFormat)
		{
			// Determine if we should use W Buffers or not.
			if (Gorgon.CurrentDriver.SupportWBuffer)
			{
				switch (targetFormat)
				{
					case DepthBufferFormats.BufferDepth15Stencil1:
					case DepthBufferFormats.BufferDepth16:
					case DepthBufferFormats.BufferDepth16Lockable:
					case DepthBufferFormats.BufferLuminance16:
						_useWBuffer = true;
						break;
					default:
						_useWBuffer = false;
						break;
				}
			}
			else
				_useWBuffer = false;
		}

		/// <summary>
		/// Function to set the render states.
		/// </summary>
		public void SetStates()
		{
			SetCullMode(_cullingMode);
			SetVertexPointSize(_vertexPointSize);
			SetNormalize(_normalize);
			SetWBufferEnabled(_useWBuffer);
			SetSpecularEnabled(_specularEnabled);
			SetDepthBufferEnabled(_depthBufferEnabled);
			SetDepthBufferWriteEnabled(_depthBufferWriteEnabled);
			SetLightingEnabled(_lightingEnabled);
			SetDepthBufferCompareFunction(_depthTestFunction);
			SetDepthBufferBias(_depthBufferBias);
			SetShadingMode(_shadingMode);
			SetDrawingMode(_drawingMode);
			SetAlphaBlendEnabled(_alphaBlendEnabled);
			SetSourceAlphaOperation(_sourceAlphaOperation);
			SetDestinationAlphaOperation(_destinationAlphaOperation);
			SetAlphaTestEnabled(_alphaTestEnabled);
			SetAlphaTestFunction(_alphaTestFunction);
			SetAlphaTestValue(_alphaTestValue);
			SetDrawLastPixel(_drawLastPixel);
			SetScissorTest(_scissorTest);
			SetScissorRectangle(_scissorRectangle);
			SetStencilCompare(_stencilCompare);
			SetStencilEnabled(_stencilEnabled);
			SetStencilFailOperation(_stencilFailOperation);
			SetStencilMask(_stencilMask);
			SetStencilPassOperation(_stencilPassOperation);
			SetStencilReference(_stencilReference);
			SetStencilZFailOperation(_stencilZFailOperation);
			SetDitherEnabled(_ditherEnabled);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal RenderStates()
		{
			_cullingMode = CullingMode.Clockwise;
			_vertexPointSize = 1.0f;
			_depthBufferEnabled = true;
			_depthBufferWriteEnabled = true;
			_lightingEnabled = true;
			_depthTestFunction = CompareFunctions.LessThanOrEqual;
			_sourceAlphaOperation = AlphaBlendOperation.One;
			_destinationAlphaOperation = AlphaBlendOperation.Zero;
			_shadingMode = ShadingMode.Gouraud;
			_drawingMode = DrawingMode.Solid;
			_alphaTestFunction = CompareFunctions.Always;
			_scissorRectangle = Drawing.Rectangle.Empty;
			_stencilPassOperation = StencilOperations.Keep;
			_stencilFailOperation = StencilOperations.Replace;
			_stencilZFailOperation = StencilOperations.Replace;
			_stencilCompare = CompareFunctions.Always;
			_stencilMask = -1;
		}
		#endregion
	}
}
