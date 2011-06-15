#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, January 06, 2007 1:07:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a cache for sprite/text sprite render states.
	/// </summary>
	public class SpriteStateCache
		: IDisposable, IDeviceStateObject
	{
		#region Variables.
		private Smoothing _lastSmooth;												// Last magnification.
		private BlendingModes _globalBlending;										// Global blending setting.
		private int _globalMaskValue;												// Global mask value.
		private CompareFunctions _globalMaskFunction;								// Global mask function.
		private Smoothing _globalSmoothing;											// Global smoothing function.
		private AlphaBlendOperation _globalSourceBlend;								// Global source blending mode for sprites.		
		private AlphaBlendOperation _globalDestBlend;								// Global destination blending mode for sprites.		
		private ImageAddressing _globalWrapHMode;									// Horizontal image wrapping mode.
		private ImageAddressing _globalWrapVMode;									// Horizontal image wrapping mode.
		private StencilOperations _globalStencilPassOperation;						// Stencil pass operation.
		private StencilOperations _globalStencilFailOperation;						// Stencil fail operation.
		private StencilOperations _globalStencilZFailOperation;						// Stencil Z fail operation.
		private CompareFunctions _globalStencilCompare;								// Stencil compare operation.
		private bool _globalDepthWriteEnabled;										// Depth buffer writing enabled.
		private float _globalDepthBias;												// Depth buffer bias.
		private CompareFunctions _globalDepthTestFunction;							// Depth buffer testing function.
		private int _globalStencilReference;										// Stencil reference value.
		private int _globalStencilMask;												// Stencil mask value.
		private bool _globalUseStencil;												// Flag to indicate whether to use the stencil or not.
		private bool _dither;                                                       // Flag to indicate whether dithering will be used or not.
		private RenderStates _renderStates = null;									// Render states.
		private ImageLayerStates _imageStates = null;								// Image layer states.
		private int _maxSprites = 1000;												// Maximum number of sprites per render batch.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the depth buffer (if applicable) is enabled or not.
		/// </summary>
		/// <remarks>If the current render target does not have a depth buffer attached (i.e. <see cref="GorgonLibrary.Graphics.RenderTarget.UseDepthBuffer">RenderTarget.Graphics.RenderTarget</see>=false) then this property will always return FALSE.</remarks>
		public bool DepthBufferEnabled
		{
			get
			{
				if ((Gorgon.CurrentRenderTarget == null) || (!Gorgon.CurrentRenderTarget.UseDepthBuffer))
					return false;

				return Gorgon.Renderer.RenderStates.DepthBufferEnabled;
			}
			set
			{
				if ((Gorgon.CurrentRenderTarget == null) || (!Gorgon.CurrentRenderTarget.UseDepthBuffer))
					return;

				Gorgon.Renderer.RenderStates.DepthBufferEnabled = true;
			}
		}
		
		/// <summary>
		/// Property to set or return the maximum number of sprites per batch.
		/// </summary>
		public int MaxSpritesPerBatch
		{
			get
			{
				return _maxSprites;
			}
			set
			{
				if (value < 256)
					throw new InvalidOperationException("The batch cannot be less than 256 sprites.");

				_maxSprites = value;
				Geometry.UpdateVertexData(_maxSprites * 4);
			}
		}

		/// <summary>
		/// Property to set or return whether to dither the images or not.
		/// </summary>
		public bool GlobalDither
		{
			get
			{
				return _dither;
			}
			set
			{
				_dither = value;
			}
		}

		/// <summary>
		/// Property to set or return the global sprite blending function.
		/// </summary>
		public BlendingModes GlobalBlending
		{
			get
			{
				return _globalBlending;
			}
			set
			{
				_globalBlending = value;
				SetBlendMode(value);
			}
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		public AlphaBlendOperation GlobalSourceBlend
		{
			get
			{
				return _globalSourceBlend;
			}
			set
			{
				_globalSourceBlend = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the depth buffer writing (if applicable) is enabled or not.
		/// </summary>
		public bool GlobalDepthWriteEnabled
		{
			get
			{
				return _globalDepthWriteEnabled;
			}
			set
			{
				_globalDepthWriteEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the depth buffer bias (if applicable).
		/// </summary>
		public float GlobalDepthBufferBias
		{
			get
			{
				return _globalDepthBias;
			}
			set
			{
				_globalDepthBias = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the depth buffer test function (if applicable) is enabled or not.
		/// </summary>
		public CompareFunctions GlobalDepthBufferTestFunction
		{
			get
			{
				return _globalDepthTestFunction;
			}
			set
			{
				_globalDepthTestFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		public AlphaBlendOperation GlobalDestinationBlend
		{
			get
			{
				return _globalDestBlend;
			}
			set
			{
				_globalDestBlend = value;
			}
		}

		/// <summary>
		/// Property to set or return the global sprite alpha mask value.
		/// </summary>
		public int GlobalAlphaMaskValue
		{
			get
			{
				return _globalMaskValue;
			}
			set
			{
				_globalMaskValue = value;
			}
		}

		/// <summary>
		/// Property to set or return the global alpha masking function.
		/// </summary>
		public CompareFunctions GlobalAlphaMaskFunction
		{
			get
			{
				return _globalMaskFunction;
			}
			set
			{
				_globalMaskFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the global smoothing function for sprites.
		/// </summary>
		public Smoothing GlobalSmoothing
		{
			get
			{
				return _globalSmoothing;
			}
			set
			{
				_globalSmoothing = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		public bool GlobalStencilEnabled
		{
			get
			{
				return _globalUseStencil;
			}
			set
			{
				_globalUseStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		public int GlobalStencilReference
		{
			get
			{
				return _globalStencilReference;
			}
			set
			{
				_globalStencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		public int GlobalStencilMask
		{
			get
			{
				return _globalStencilMask;
			}
			set
			{
				_globalStencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		public StencilOperations GlobalStencilPassOperation
		{
			get
			{
				return _globalStencilPassOperation;
			}
			set
			{
				_globalStencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		public StencilOperations GlobalStencilFailOperation
		{
			get
			{
				return _globalStencilFailOperation;
			}
			set
			{
				_globalStencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		public StencilOperations GlobalStencilZFailOperation
		{
			get
			{
				return _globalStencilZFailOperation;
			}
			set
			{
				_globalStencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		public CompareFunctions GlobalStencilCompare
		{
			get
			{
				return _globalStencilCompare;
			}
			set
			{
				_globalStencilCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		public ImageAddressing GlobalHorizontalWrapMode
		{
			get
			{
				return _globalWrapHMode;
			}
			set
			{
				_globalWrapHMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		public ImageAddressing GlobalVerticalWrapMode
		{
			get
			{
				return _globalWrapVMode;
			}
			set
			{
				_globalWrapVMode = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the blending modes.
		/// </summary>
		/// <param name="value">Blending value.</param>
		protected void SetBlendMode(BlendingModes value)
		{
			switch (value)
			{
				case BlendingModes.Additive:
					_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
					_globalDestBlend = AlphaBlendOperation.One;
					break;
				case BlendingModes.Color:
					_globalSourceBlend = AlphaBlendOperation.SourceColor;
					_globalDestBlend = AlphaBlendOperation.DestinationColor;
					break;
				case BlendingModes.ModulatedInverse:
					_globalSourceBlend = AlphaBlendOperation.InverseSourceAlpha;
					_globalDestBlend = AlphaBlendOperation.SourceAlpha;
					break;
				case BlendingModes.None:
					_globalSourceBlend = AlphaBlendOperation.One;
					_globalDestBlend = AlphaBlendOperation.Zero;
					break;
				case BlendingModes.Modulated:
					_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
					_globalDestBlend = AlphaBlendOperation.InverseSourceAlpha;
					break;
				case BlendingModes.Inverted:
					_globalSourceBlend = AlphaBlendOperation.InverseDestinationColor;
					_globalDestBlend = AlphaBlendOperation.InverseSourceColor;
					break;
				case BlendingModes.PreMultiplied:
					_globalSourceBlend = AlphaBlendOperation.One;
					_globalDestBlend = AlphaBlendOperation.InverseSourceAlpha;
					break;
			}
		}

		/// <summary>
		/// Function to set specific render states per sprite.
		/// </summary>
		/// <param name="renderObject">Cache to retrieve states from.</param>
		protected internal virtual void SetStates(IRenderableStates renderObject)
		{
			// Set the wrapping mode.
			_imageStates.HorizontalAddressing = renderObject.HorizontalWrapMode;
			_imageStates.VerticalAddressing = renderObject.VerticalWrapMode;

			// Turn off alpha testing if necessary.
			if (renderObject.AlphaMaskFunction != CompareFunctions.Always)
				_renderStates.AlphaTestEnabled = true;
			else
				_renderStates.AlphaTestEnabled = false;

			// Set values for alpha testing.
			_renderStates.AlphaTestFunction = renderObject.AlphaMaskFunction;
			_renderStates.AlphaTestValue = renderObject.AlphaMaskValue;

			// Turn off the alpha blending if not needed.
			if ((renderObject.SourceBlend == AlphaBlendOperation.One) && (renderObject.DestinationBlend == AlphaBlendOperation.Zero))
				_renderStates.AlphaBlendEnabled = false;
			else
				_renderStates.AlphaBlendEnabled = true;

			_renderStates.SourceAlphaBlendOperation = renderObject.SourceBlend;
			_renderStates.DestinationAlphaBlendOperation = renderObject.DestinationBlend;

			// We can combine color additive to the other values.
			if ((renderObject.BlendingMode & BlendingModes.ColorAdditive) != 0)
				_imageStates.ColorOperation = ImageOperations.Additive;
			else
				_imageStates.ColorOperation = ImageOperations.Modulate;

			_imageStates.ColorOperationArgument1 = ImageOperationArguments.Diffuse;
			_imageStates.ColorOperationArgument2 = ImageOperationArguments.Texture;
			if ((renderObject.BlendingMode == BlendingModes.None) && (!_renderStates.AlphaTestEnabled))
			{
				_imageStates.AlphaOperation = ImageOperations.Disable;
				_imageStates.AlphaOperationArgument1 = ImageOperationArguments.Diffuse;
				_imageStates.AlphaOperationArgument2 = ImageOperationArguments.Current;
			}
			else
			{
				_imageStates.AlphaOperation = ImageOperations.Modulate;
				_imageStates.AlphaOperationArgument1 = ImageOperationArguments.Diffuse;
				_imageStates.AlphaOperationArgument2 = ImageOperationArguments.Texture;
			}

			_imageStates.BorderColor = renderObject.BorderColor;

			// Set smoothing operation.
			switch (renderObject.Smoothing)
			{
				case Smoothing.None:
					_imageStates.MagnificationFilter = ImageFilters.Point;
					_imageStates.MinificationFilter = ImageFilters.Point;
					break;
				case Smoothing.Smooth:
					_imageStates.MagnificationFilter = ImageFilters.Bilinear;
					_imageStates.MinificationFilter = ImageFilters.Bilinear;
					break;
				case Smoothing.MagnificationSmooth:
					_imageStates.MagnificationFilter = ImageFilters.Bilinear;
					_imageStates.MinificationFilter = ImageFilters.Point;
					break;
				case Smoothing.MinificationSmooth:
					_imageStates.MagnificationFilter = ImageFilters.Point;
					_imageStates.MinificationFilter = ImageFilters.Bilinear;
					break;
			}
			_lastSmooth = renderObject.Smoothing;

			// Set stencil render states.
			_renderStates.StencilEnable = renderObject.StencilEnabled;
			_renderStates.StencilCompare = renderObject.StencilCompare;
			_renderStates.StencilFailOperation = renderObject.StencilFailOperation;
			_renderStates.StencilMask = renderObject.StencilMask;
			_renderStates.StencilPassOperation = renderObject.StencilPassOperation;
			_renderStates.StencilZFailOperation = renderObject.StencilZFailOperation;
			_renderStates.StencilReference = renderObject.StencilReference;

			// Set depth render states.
			_renderStates.DepthBias = renderObject.DepthBufferBias;
			_renderStates.DepthBufferWriteEnabled = renderObject.DepthWriteEnabled;
			_renderStates.DepthTestFunction = renderObject.DepthTestFunction;

			// Set culling.
			_renderStates.CullingMode = renderObject.CullingMode;

			// Set dithering flag.
			_renderStates.DitheringEnabled = _dither;
			Geometry.PrimitiveStyle = renderObject.PrimitiveStyle;
			Geometry.UseIndices = renderObject.UseIndices;
		}

		/// <summary>
		/// Function to return whether the state has changed or not.
		/// </summary>
		/// <param name="renderObject">Cache to retrieve states from.</param>
		/// <param name="image">Image used by the renderable.</param>
		/// <returns>TRUE if the state has changed, FALSE if not.</returns>
		protected internal virtual bool StateChanged(IRenderableStates renderObject, Image image)
		{
			RenderStates states;			// Render states.
			ImageLayerStates _imageStates;	// Image layer states.

			states = _renderStates;
			_imageStates = Gorgon.Renderer.ImageLayerStates[0];

			if (image != Gorgon.Renderer.GetImage(0))
				return true;

			if (renderObject.BorderColor != _imageStates.BorderColor)
				return true;

			if (((renderObject.BlendingMode & BlendingModes.ColorAdditive) == BlendingModes.ColorAdditive) && (_imageStates.ColorOperation != ImageOperations.Additive))
				return true;

			if (((renderObject.BlendingMode & BlendingModes.ColorAdditive) != BlendingModes.ColorAdditive) && (_imageStates.ColorOperation == ImageOperations.Additive))
				return true;

			if (renderObject.StencilCompare != states.StencilCompare)
				return true;

			if (renderObject.StencilEnabled != states.StencilEnable)
				return true;

			if (renderObject.StencilPassOperation != states.StencilPassOperation)
				return true;

			if (renderObject.StencilFailOperation != states.StencilFailOperation)
				return true;

			if (renderObject.StencilZFailOperation != states.StencilZFailOperation)
				return true;

			if (renderObject.StencilReference != states.StencilReference)
				return true;

			if (renderObject.StencilMask != states.StencilMask)
				return true;

			if (renderObject.SourceBlend != states.SourceAlphaBlendOperation)
				return true;

			if (renderObject.DestinationBlend != states.DestinationAlphaBlendOperation)
				return true;

			if (renderObject.HorizontalWrapMode != _imageStates.HorizontalAddressing)
				return true;

			if (renderObject.VerticalWrapMode != _imageStates.VerticalAddressing)
				return true;

			if (renderObject.AlphaMaskFunction != states.AlphaTestFunction)
				return true;

			if (renderObject.AlphaMaskValue != states.AlphaTestValue)
				return true;

			if (renderObject.Smoothing != _lastSmooth)
				return true;

			if (renderObject.DepthTestFunction != states.DepthTestFunction)
				return true;

			if (renderObject.DepthWriteEnabled != states.DepthBufferWriteEnabled)
				return true;

			if (renderObject.DepthBufferBias != states.DepthBias)
				return true;

			if (renderObject.CullingMode != states.CullingMode)
				return true;

			if (Geometry.PrimitiveStyle != renderObject.PrimitiveStyle)
				return true;

			if (Geometry.UseIndices != renderObject.UseIndices)
				return true;			

			return false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal SpriteStateCache()
		{
			// Initialize states.
			_lastSmooth = Smoothing.None;

			// Set the global settings for sprites to inherit from.
			_globalBlending = BlendingModes.Modulated;
			_globalMaskFunction = CompareFunctions.GreaterThan;
			_globalMaskValue = 1;
			_globalSmoothing = Smoothing.None;
			_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
			_globalDestBlend = AlphaBlendOperation.InverseSourceAlpha;
			_globalWrapHMode = ImageAddressing.Clamp;
			_globalWrapVMode = ImageAddressing.Clamp;
			_globalStencilCompare = CompareFunctions.Always;
			_globalStencilPassOperation = StencilOperations.Keep;
			_globalStencilFailOperation = StencilOperations.Keep;
			_globalStencilZFailOperation = StencilOperations.Keep;
			_globalStencilMask = -1;
			_globalDepthBias = 0.0f;
			_globalDepthTestFunction = CompareFunctions.LessThanOrEqual;
			_globalDepthWriteEnabled = true;
			_renderStates = Gorgon.Renderer.RenderStates;
			_imageStates = Gorgon.Renderer.ImageLayerStates[0];

			// Create the render manager.
			DeviceStateList.Add(this);
		}
		#endregion

		#region IDisposable
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		private void Dispose(bool disposing)
		{
			if (disposing)
				DeviceStateList.Remove(this);

			Geometry.DestroyBuffers();
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

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is put into a lost state.
		/// </summary>
		public void DeviceLost()
		{
			Geometry.VerticesWritten = 0;
			Geometry.VertexOffset = 0;
		}

		/// <summary>
		/// Function called when the device is restored from a lost state.
		/// </summary>
		public void DeviceReset()
		{
		}

		/// <summary>
		/// Function called when the device is forcably destroying resources.
		/// </summary>
		public void ForceRelease()
		{
			DeviceLost();
		}
		#endregion
	}
}
