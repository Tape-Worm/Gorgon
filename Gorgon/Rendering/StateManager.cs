#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, January 06, 2007 1:07:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a state manager for the sprites.
	/// </summary>
	public class StateManager
		: IDisposable, IDeviceStateObject
	{
		#region Classes.
		/// <summary>
		/// Object representing a manager for rendering an object.
		/// </summary>
		internal class RenderManager
			: IDisposable
		{
			#region Variables.
			private VertexCache<VertexTypes.PositionDiffuse2DTexture1> _vertices;		// Layer vertex buffer.
			private IndexCache<short> _indices;											// Layer index buffer.
			private int _vertexCount;													// Initial vertex count.
			private PrimitiveStyle _currentPrimitiveStyle;								// Current used primitive style.
			private bool _currentUseIndices;											// Current use indices flag.			
			private Renderer _renderer = null;											// Renderer to use.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the offset for the vertex buffer.
			/// </summary>
			public int VertexOffset
			{
				get
				{
					return _vertices.DataOffset;
				}
				set
				{
					_vertices.DataOffset = value;
				}
			}

			/// <summary>
			/// Property to return the offset for the index buffer.
			/// </summary>
			public int IndexOffset
			{
				get
				{
					return _indices.DataOffset;
				}
				set
				{
					_indices.DataOffset = value;
				}
			}

			/// <summary>
			/// Property to return the number of indices written to the cache.
			/// </summary>
			public int IndicesWritten
			{
				get
				{
					return (_vertices.DataWritten * 6) / 4;
				}
				set
				{
					_indices.DataWritten = value;
				}
			}

			/// <summary>
			/// Property to return the number of vertices written to the cache.
			/// </summary>
			public int VerticesWritten
			{
				get
				{
					return _vertices.DataWritten;
				}
				set
				{
					_vertices.DataWritten = value;
				}
			}

			/// <summary>
			/// Property to return whether we're using indices or not.
			/// </summary>
			public bool UseIndices
			{
				get
				{
					return _currentUseIndices;
				}
				set
				{
					_currentUseIndices = value;
				}
			}

			/// <summary>
			/// Property to return the vertex buffer.
			/// </summary>
			/// <value></value>
			public VertexBuffer VertexBuffer
			{
				get
				{
					return _vertices.VertexBuffer;
				}
			}

			/// <summary>
			/// Property to return the index buffer.
			/// </summary>
			/// <value></value>
			public IndexBuffer IndexBuffer
			{
				get
				{
					return _indices.IndexBuffer;
				}
			}

			/// <summary>
			/// Property to return vertex type.
			/// </summary>
			/// <value></value>
			public VertexType VertexType
			{
				get
				{
					return _vertices.VertexType;
				}
			}

			/// <summary>
			/// Property to set or return the style of primitive to use.
			/// </summary>
			/// <value></value>
			public PrimitiveStyle PrimitiveStyle
			{
				get
				{
					return _currentPrimitiveStyle;
				}
				set
				{
					_currentPrimitiveStyle = value;
				}
			}

			/// <summary>
			/// Property to return the allocated number of vertices in the buffer.
			/// </summary>
			public int VertexCount
			{
				get
				{
					return _vertices.Count;
				}
			}

			/// <summary>
			/// Property to return the allocated number of indices in the buffer.
			/// </summary>
			public int IndexCount
			{
				get
				{
					return _indices.Count;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to update the vertex data.
			/// </summary>
			public void UpdateVertexData()
			{
				// Create buffers.
				if (_vertices != null)
					_vertices.Dispose();
				if (_indices != null)
					_indices.Dispose();

				_vertices = new VertexCache<VertexTypes.PositionDiffuse2DTexture1>();
				_indices = new IndexCache<short>();

				// Initialize vertex type.
				_vertices.VertexType = _renderer.VertexTypes["PositionDiffuse2DTexture1"];

				// Set up access types.
				_vertices.BufferUsage = BufferUsage.Dynamic | BufferUsage.WriteOnly;
				_indices.BufferUsage = BufferUsage.Static | BufferUsage.WriteOnly;

				_vertices.Count = _vertexCount;
				_indices.Count = (_vertices.Count / 4) * 6;

				// Build the index buffer.
				short index = 0;
				int pos = 0;
				for (int i = 0; i < (_vertices.Count / 4); i++)
				{
					_indices.CachedData[pos] = (short)(index + 2);
					_indices.CachedData[pos + 1] = (short)(index + 1);
					_indices.CachedData[pos + 2] = index;
					_indices.CachedData[pos + 3] = index;
					_indices.CachedData[pos + 4] = (short)(index + 3);
					_indices.CachedData[pos + 5] = (short)(index + 2);
					pos += 6;
					index += 4;
				}
				_indices.DataWritten = _indices.Count;
			}

			/// <summary>
			/// Property to return the vertex cache for the list.
			/// </summary>
			public VertexCache<VertexTypes.PositionDiffuse2DTexture1> VertexCache
			{
				get
				{
					return _vertices;
				}
			}

			/// <summary>
			/// Property to return the index cache for the list.
			/// </summary>
			public IndexCache<short> IndexCache
			{
				get
				{
					return _indices;
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="renderer">Renderer to use.</param>
			public RenderManager(Renderer renderer)
			{
				// Default to 4000 vertices.
				_vertexCount = 4000;
				_renderer = renderer;

				UpdateVertexData();
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
				{
					if (_indices != null)
						_indices.Dispose();

					if (_vertices != null)
						_vertices.Dispose();
				}

				// Do unmanaged clean up.
				_vertices = null;
				_indices = null;
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
		#endregion

		#region Variables.
		private Smoothing _lastSmooth;												// Last magnification.
		private Blending _globalBlending;											// Global blending setting.
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
		private int _globalStencilReference;										// Stencil reference value.
		private int _globalStencilMask;												// Stencil mask value.
		private bool _globalUseStencil;												// Flag to indicate whether to use the stencil or not.
		private bool _dither;                                                       // Flag to indicate whether dithering will be used or not.
		private RenderManager _renderManager;										// Render manager.
		private Renderer _renderer = null;											// Renderer.
		private RenderStates _renderStates = null;									// Render states.
		private ImageLayerStates _imageStates = null;								// Image layer states.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the render manager object.
		/// </summary>
		internal RenderManager RenderData
		{
			get
			{
				return _renderManager;
			}
		}

		/// <summary>
		/// Property to set or return whether to dither the images or not.
		/// </summary>
		public bool Dither
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
		public Blending LayerBlending
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
		public AlphaBlendOperation SourceBlend
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
		/// Property to set or return the destination blending operation.
		/// </summary>
		public AlphaBlendOperation DestinationBlend
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
		public int LayerAlphaMaskValue
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
		public CompareFunctions LayerAlphaMaskFunction
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
		public Smoothing LayerSmoothing
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
		public bool StencilEnabled
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
		public int StencilReference
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
		public int StencilMask
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
		public StencilOperations StencilPassOperation
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
		public StencilOperations StencilFailOperation
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
		public StencilOperations StencilZFailOperation
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
		public CompareFunctions StencilCompare
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
		public ImageAddressing HorizontalWrapMode
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
		public ImageAddressing VerticalWrapMode
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
		private void SetBlendMode(Blending value)
		{
			switch (value)
			{
				case Blending.Additive:
					_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
					_globalDestBlend = AlphaBlendOperation.One;
					break;
				case Blending.Burn:
					_globalSourceBlend = AlphaBlendOperation.DestinationColor;
					_globalDestBlend = AlphaBlendOperation.SourceColor;
					break;
				case Blending.Dodge:
					_globalSourceBlend = AlphaBlendOperation.DestinationColor;
					_globalDestBlend = AlphaBlendOperation.DestinationColor;
					break;
				case Blending.None:
					_globalSourceBlend = AlphaBlendOperation.One;
					_globalDestBlend = AlphaBlendOperation.Zero;
					break;
				case Blending.Normal:
					_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
					_globalDestBlend = AlphaBlendOperation.InverseSourceAlpha;
					break;
			}
		}

		/// <summary>
		/// Function to set specific render states per sprite.
		/// </summary>
		/// <param name="renderObject">Layer to retrieve states from.</param>
		internal void SetStates(IRenderable renderObject)
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
			if ((renderObject.BlendingMode & Blending.ColorAdditive) != 0)
				_imageStates.ColorOperation = ImageOperations.Additive;
			else
				_imageStates.ColorOperation = ImageOperations.Modulate;

			_imageStates.ColorOperationArgument1 = ImageOperationArguments.Diffuse;
			_imageStates.ColorOperationArgument2 = ImageOperationArguments.Texture;
			if ((renderObject.BlendingMode == Blending.None) && (!_renderStates.AlphaTestEnabled))
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

			// Set stencil _renderStates.
			_renderStates.StencilEnable = renderObject.StencilEnabled;
			_renderStates.StencilCompare = renderObject.StencilCompare;
			_renderStates.StencilFailOperation = renderObject.StencilFailOperation;
			_renderStates.StencilMask = renderObject.StencilMask;
			_renderStates.StencilPassOperation = renderObject.StencilPassOperation;
			_renderStates.StencilZFailOperation = renderObject.StencilZFailOperation;
			_renderStates.StencilReference = renderObject.StencilReference;

			// Set the requested clipper.
			_renderer.CurrentClippingView = renderObject.ClippingViewport;

			// Set the currently active shader.
			_renderer.CurrentShader = renderObject.Shader;

			// Set dithering flag.
			_renderStates.DitheringEnabled = _dither;
			_renderManager.PrimitiveStyle = renderObject.PrimitiveStyle;
			_renderManager.UseIndices = renderObject.UseIndices;
		}

		/// <summary>
		/// Function to return whether the state has changed or not.
		/// </summary>
		/// <param name="renderObject">Layer to retrieve states from.</param>
		/// <returns>TRUE if the state has changed, FALSE if not.</returns>
		internal bool StateChanged(IRenderable renderObject)
		{
			bool result = false;			// Result.
			RenderStates states;			// Render states.
			ImageLayerStates _imageStates;	// Image layer states.
			
			states = _renderStates;
			_imageStates = _renderer.ImageLayerStates[0];

			// Check for state changes.
			if ((_renderer.CurrentClippingView != renderObject.ClippingViewport) && (_renderer.CurrentClippingView != null))
				result = true;

			if (renderObject.StencilCompare != states.StencilCompare)
				result = true;

			if (renderObject.StencilEnabled != states.StencilEnable)
				result = true;

			if (renderObject.StencilPassOperation != states.StencilPassOperation)
				result = true;

			if (renderObject.StencilFailOperation != states.StencilFailOperation)
				result = true;

			if (renderObject.StencilZFailOperation != states.StencilZFailOperation)
				result = true;

			if (renderObject.StencilReference != states.StencilReference)
				result = true;

			if (renderObject.StencilMask != states.StencilMask)
				result = true;

			if (renderObject.SourceBlend != states.SourceAlphaBlendOperation)
				result = true;

			if (renderObject.DestinationBlend != states.DestinationAlphaBlendOperation)
				result = true;			

			if (renderObject.HorizontalWrapMode != _imageStates.HorizontalAddressing)
				result = true;

			if (renderObject.VerticalWrapMode != _imageStates.VerticalAddressing)
				result = true;

			if (renderObject.AlphaMaskFunction != states.AlphaTestFunction)
				result = true;

			if (renderObject.AlphaMaskValue != states.AlphaTestValue)
				result = true;

			if (renderObject.Smoothing != _lastSmooth)
				result = true;

			if (_renderManager.PrimitiveStyle != renderObject.PrimitiveStyle)
				result = true;

			if (_renderManager.UseIndices != renderObject.UseIndices)
				result = true;

			if (renderObject.Shader != _renderer.CurrentShader)
				result = true;

			if ((renderObject.Shader != null) && (_renderer.CurrentTechnique != renderObject.Shader.ActiveTechnique))
				result = true;

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="renderer">Renderer to use for states.</param>
		internal StateManager(Renderer renderer)
		{
			// Initialize states.
			_lastSmooth = Smoothing.None;
			_dither = false;

			// Set the global settings for sprites to inherit from.
			_globalBlending = Blending.Normal;
			_globalMaskFunction = CompareFunctions.GreaterThan;
			_globalMaskValue = 1;
			_globalSmoothing = Smoothing.None;
			_globalSourceBlend = AlphaBlendOperation.SourceAlpha;
			_globalDestBlend = AlphaBlendOperation.InverseSourceAlpha;
			_globalWrapHMode = ImageAddressing.Clamp;
			_globalWrapVMode = ImageAddressing.Clamp;
			_globalStencilCompare = CompareFunctions.Always;
			_globalUseStencil = false;
			_globalStencilPassOperation = StencilOperations.Keep;
			_globalStencilFailOperation = StencilOperations.Keep;
			_globalStencilZFailOperation = StencilOperations.Keep;
			_globalStencilReference = 0;
			_globalStencilMask = -1;
			_renderer = renderer;
			_renderStates = renderer.RenderStates;
			_imageStates = renderer.ImageLayerStates[0];

			// Create the render manager.
			_renderManager = new RenderManager(_renderer);
			
			Gorgon.DeviceStateList.Add(this);
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
			{
				Gorgon.DeviceStateList.Remove(this);
				_renderManager.Dispose();
			}

			_renderManager = null;
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
			_renderManager.VerticesWritten = 0;
			_renderManager.VertexOffset = 0;
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
