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
// Created: Sunday, January 01, 2012 9:04:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimMath;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections.Specialized;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// The renderer for 2D graphics.
	/// </summary>
	/// <remarks>This is the interface to handle sprites, primitives like lines, circles, ellipses, etc... and text.  The 2D renderer allows for using sprites in a 3D space.  
	/// That is, it respects the depth axis so that placing a 2 sprites, one at coordinates (0.1, 0.1, 0.1) and the other at (0.1, 0.1, 1.0), will make the 2nd appear 
	/// smaller.
	/// <para>A default render target is required to use this interface, however that render target can be a render target texture or <see cref="M:GorgonLibrary.Graphics.Output.CreateSwapChain">swap chain</see>.  
	/// Note that this does not mean that this interface is limited to one target, the target can be changed at will via the <see cref="P:GorgonLibrary.Graphics.Renderers.Gorgon2D.Target">Target</see> property.</para>
	/// </remarks>
	public class Gorgon2D
		: IDisposable
	{
		#region Enumerations.
		/// <summary>
		/// A list of state changes.
		/// </summary>
		[Flags()]
		internal enum StateChanges
		{
			/// <summary>
			/// No state change.
			/// </summary>
			None = 0,
			/// <summary>
			/// Texture changed.
			/// </summary>
			Texture = 1,
			/// <summary>
			/// Shader changed.
			/// </summary>
			Shader = 2,
			/// <summary>
			/// Blending state changed.
			/// </summary>
			BlendState = 4,
			/// <summary>
			/// Primitive type changed.
			/// </summary>
			PrimitiveType = 8,
			/// <summary>
			/// Vertex buffer changed.
			/// </summary>
			VertexBuffer = 16,
			/// <summary>
			/// Index buffer changed.
			/// </summary>
			IndexBuffer = 32,
			/// <summary>
			/// Alpha testing value changed.
			/// </summary>
			AlphaTestValue = 64,
			/// <summary>
			/// Sampler state changed.
			/// </summary>
			Sampler = 128,
			/// <summary>
			/// Blending factor.
			/// </summary>
			BlendFactor = 256,
			/// <summary>
			/// Rasterizer state changed.
			/// </summary>
			Raster = 512,
			/// <summary>
			/// Depth/stencil state changed.
			/// </summary>
			DepthStencil = 1024
		}
		#endregion

		#region Value Types.
		/// <summary>
		/// The current set of states when the renderer was started.
		/// </summary>
		private class PreviousStates
		{
			/// <summary>
			/// Pixel shader.
			/// </summary>
			public GorgonPixelShader PixelShader;
			/// <summary>
			/// Vertex shader.
			/// </summary>
			public GorgonVertexShader VertexShader;
			/// <summary>
			/// Blending states.
			/// </summary>
			public GorgonBlendStates BlendStates;
			/// <summary>
			/// Blending factor.
			/// </summary>
			public GorgonColor BlendFactor;
			/// <summary>
			/// Blending sample mask.
			/// </summary>
			public uint BlendSampleMask;
			/// <summary>
			/// Rasterizer states.
			/// </summary>
			public GorgonRasterizerStates RasterStates;
			/// <summary>
			/// Sampler states.
			/// </summary>
			public GorgonTextureSamplerStates SamplerState;
			/// <summary>
			/// Textures.
			/// </summary>
			public GorgonTexture Texture;
			/// <summary>
			/// Default target.
			/// </summary>
			public GorgonSwapChain Target;
			/// <summary>
			/// Index buffer.
			/// </summary>
			public GorgonIndexBuffer IndexBuffer;
			/// <summary>
			/// Vertex buffer.
			/// </summary>
			public GorgonVertexBufferBinding VertexBuffer;
			/// <summary>
			/// Input layout.
			/// </summary>
			public GorgonInputLayout InputLayout;
			/// <summary>
			/// Primitive type.
			/// </summary>
			public PrimitiveType PrimitiveType;

			/// <summary>
			/// Function to restore the previous states.
			/// </summary>
			/// <param name="graphics">Graphics interface.</param>
			public void Restore(GorgonGraphics graphics)
			{
				graphics.Output.RenderTargets[0] = Target;
				graphics.Input.IndexBuffer = IndexBuffer;
				graphics.Input.VertexBuffers[0] = VertexBuffer;
				graphics.Input.Layout = InputLayout;
				graphics.Input.PrimitiveType = PrimitiveType;
				graphics.Shaders.PixelShader.Current = PixelShader;
				graphics.Shaders.VertexShader.Current = VertexShader;
				graphics.Output.BlendingState.BlendSampleMask = BlendSampleMask;
				graphics.Output.BlendingState.BlendFactor = BlendFactor;
				graphics.Output.BlendingState.States = BlendStates;
				graphics.Rasterizer.States = RasterStates;
				graphics.Shaders.PixelShader.Textures[0] = Texture;
				graphics.Shaders.PixelShader.TextureSamplers[0] = SamplerState;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PreviousStates"/> struct.
			/// </summary>
			/// <param name="graphics">The graphics.</param>
			public PreviousStates(GorgonGraphics graphics)
			{
				Target = graphics.Output.RenderTargets[0];
				IndexBuffer = graphics.Input.IndexBuffer;
				VertexBuffer = graphics.Input.VertexBuffers[0];
				InputLayout = graphics.Input.Layout;
				PrimitiveType = graphics.Input.PrimitiveType;
				PixelShader = graphics.Shaders.PixelShader.Current;
				VertexShader = graphics.Shaders.VertexShader.Current;
				BlendStates = graphics.Output.BlendingState.States;
				BlendFactor = graphics.Output.BlendingState.BlendFactor;
				BlendSampleMask = graphics.Output.BlendingState.BlendSampleMask;
				RasterStates = graphics.Rasterizer.States;
				SamplerState = graphics.Shaders.PixelShader.TextureSamplers[0];
				Texture = graphics.Shaders.PixelShader.Textures[0];
			}
		}

		/// <summary>
		/// A vertex for a sprite.
		/// </summary>		
		public struct Vertex
		{
			/// <summary>
			/// Position of the vertex.
			/// </summary>
			[InputElement(0, "SV_POSITION")]
			public Vector4 Position;
			/// <summary>
			/// Color of the vertex.
			/// </summary>
			[InputElement(1, "COLOR")]
			public GorgonColor Color;
			/// <summary>
			/// Texture coordinates.
			/// </summary>
			[InputElement(2, "TEXCOORD")]
			public Vector2 UV;

			/// <summary>
			/// Initializes a new instance of the <see cref="Vertex"/> struct.
			/// </summary>
			/// <param name="position">The position.</param>
			/// <param name="color">The color.</param>
			/// <param name="uv">The texture coordinate.</param>
			public Vertex(Vector4 position, Vector4 color, Vector2 uv)
			{
				Position = position;
				Color = color;
				UV = uv;
			}
		}
		#endregion

		#region Variables.
		private int _vertexSize = 0;																// Size, in bytes, of a vertex.
		private Vertex[] _vertexCache = null;														// List of vertices to cache.
		private int _cacheStart = 0;																// Starting cache vertex buffer index.
		private int _renderIndexStart = 0;															// Starting index to render.
		private int _renderIndexCount = 0;															// Number of indices to render.
		private int _cacheEnd = 0;																	// Ending vertex buffer cache index.
		private int _cacheWritten = 0;																// Number of vertices written.
		private bool _disposed = false;																// Flag to indicate that the object was disposed.
		private int _cacheSize = 32768;																// Number of vertices that we can stuff into a vertex buffer.
		private Matrix _defaultProjection = Matrix.Identity;										// Default projection matrix.
		private Matrix _defaultView = Matrix.Identity;												// Default view matrix.
		private Matrix? _projection = null;															// Current projection matrix.
		private Matrix? _view = null;																// Current view matrix.
		private GorgonSwapChain _defaultTarget = null;												// Default render target.
		private GorgonSwapChain _target = null;														// Current render target.	
		private GorgonInputLayout _layout = null;													// Input layout.
		private Stack<PreviousStates> _stateRecall = null;											// State recall.
		private GorgonStateManager _stateManager = null;											// State manager.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return our default vertex buffer binding.
		/// </summary>
		internal GorgonVertexBufferBinding DefaultVertexBufferBinding
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return our default index buffer.
		/// </summary>
		internal GorgonIndexBuffer DefaultIndexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the tracked objects interface.
		/// </summary>
		internal GorgonTrackedObjectCollection TrackedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to use multisampling.
		/// </summary>
		public bool UseMultisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the shader interface.
		/// </summary>
		public Gorgon2DShaders Shaders
		{
			get;
			private set;
		}


		/// <summary>
		/// Property to set or return the projection matrix.
		/// </summary>
		/// <remarks>The default matrix is an orthographic matrix, which allows to pixel accurate drawing (i.e. a sprite located at 100, 100 is located at the pixel coordinate of 100x100).
		/// <para>When using a perspective matrix, such as 3D, the coordinate system changes to a relative unit system (-1.0f,1.0f).  This allows for depth to be introduced to the sprites.</para>
		/// </remarks>
		public Matrix? ProjectionMatrix
		{
			get
			{
				if (_projection == null)
					return _defaultProjection;
				else
					return _projection;
			}
			set
			{
				_projection = value;
				Shaders.UpdateGorgonTransformation();
			}
		}

		/// <summary>
		/// Property to set or return the view matrix.
		/// </summary>
		/// <remarks>The view matrix is used to relocate the objects in a scene around a camera's position.</remarks>
		public Matrix? ViewMatrix
		{
			get
			{
				if (_view == null)
					return Matrix.Identity;
				else
					return _view;
			}
			set
			{
				_view = value;
				Shaders.UpdateGorgonTransformation();
			}
		}

		/// <summary>
		/// Property to set or return the number of vertices that can be cached into a vertex buffer.
		/// </summary>
		public int VertexCacheSize
		{
			get
			{
				return _cacheSize;
			}
			set
			{
				if (_cacheSize < 1024)
					_cacheSize = 1024;
				_cacheSize = value;

				Initialize();
			}
		}

		/// <summary>
		/// Property to return the graphics interface for the 2D renderer.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the active render target.
		/// </summary>
		public GorgonSwapChain Target
		{
			get
			{
				if (_target == null)
					return _defaultTarget;

				return _target;
			}
			set
			{
				if (_target != value)
				{
					_target = value;
					UpdateTarget();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the current render target.
		/// </summary>
		private void UpdateTarget()
		{
			// Remove any previous handler.
			if (Target != null)
				Target.Resized -= new EventHandler(target_Resized);

			Graphics.Output.RenderTargets[0] = Target;
			UpdateProjection();

			// Re-assign the event.
			if (Target != null)
				Target.Resized += new EventHandler(target_Resized);
		}

		/// <summary>
		/// Function to update the default projection matrix.
		/// </summary>
		private void UpdateProjection()
		{
			// Update based on the current render target.
			Matrix.OrthoOffCenterLH(0, Target.Settings.Width, Target.Settings.Height, 0.0f, 0.0f, 100.0f, out _defaultProjection);
			Graphics.Rasterizer.SetViewport(Target.Viewport);

			Shaders.UpdateGorgonTransformation();
		}

		/// <summary>
		/// Function to initialize the 2D renderer.
		/// </summary>
		private void Initialize()
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);
			
			// Create the default projection matrix.
			Target.Resized -= new EventHandler(target_Resized);

			// Create shaders.
			if (Shaders == null)
				Shaders = new Gorgon2DShaders(this);

			// Create layout information so we can bind our vertices to the shader.
			if (_layout == null)
			{
				_layout = Graphics.Input.CreateInputLayout("2D_Sprite_Vertex_Layout", typeof(Vertex), Shaders.DefaultVertexShader.Shader);
				_vertexSize = _layout.GetSlotSize(0);
			}

			if (DefaultIndexBuffer != null)
				DefaultIndexBuffer.Dispose();
			if (DefaultVertexBufferBinding.VertexBuffer != null)
				DefaultVertexBufferBinding.VertexBuffer.Dispose();
						
			int spriteVBSize = _vertexSize * _cacheSize;
			int spriteIBSize = sizeof(int) * _cacheSize * 6;

			// Set up our index buffer.
			using (GorgonDataStream ibData = new GorgonDataStream(spriteIBSize))
			{
				int index = 0;
				for (int i = 0; i < _cacheSize; i++)
				{
					ibData.Write<int>(index);
					ibData.Write<int>(index + 1);
					ibData.Write<int>(index + 2);
					ibData.Write<int>(index + 1);
					ibData.Write<int>(index + 3);
					ibData.Write<int>(index + 2);
					index += 4;
				}

				ibData.Position = 0;
				DefaultIndexBuffer = Graphics.Input.CreateIndexBuffer((int)ibData.Length, BufferUsage.Immutable, true, ibData);
			}
			
			// Create our empty vertex buffer.
			DefaultVertexBufferBinding = new GorgonVertexBufferBinding(Graphics.Input.CreateVertexBuffer(spriteVBSize, BufferUsage.Dynamic), _vertexSize);

			// Create the vertex cache.
			_vertexCache = new Vertex[VertexCacheSize];
			_cacheStart = 0;
			_cacheEnd = 0;
			_cacheWritten = 0;
			_renderIndexStart = 0;
			_renderIndexCount = 0;
		}

		/// <summary>
		/// Function to handle a resize of the current render target.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void target_Resized(object sender, EventArgs e)
		{
			UpdateProjection();
			UpdateTarget();
		}

		/// <summary>
		/// Function to determine if state has changed, and if it has, will flush the renderer.
		/// </summary>
		/// <param name="renderable">Renderable object to check for state change.</param>
		/// <returns>The states that have changed.</returns>
		private StateChanges CheckStateChange(GorgonRenderable renderable)
		{
			StateChanges result = StateChanges.None;

			if (renderable.Texture != Shaders.PixelShader.Textures[0])
			{
				result |= StateChanges.Texture;

				if ((((Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuseAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuse)) && (renderable.Texture != null)) || 
					(((Shaders.PixelShader == Shaders.DefaultPixelShaderTexturedAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTextured)) && (renderable.Texture == null)))
					result |= StateChanges.Shader;
			}

			if (!renderable.BlendFactor.Equals(Graphics.Output.BlendingState.BlendFactor))
				result |= StateChanges.BlendFactor;

			if (!renderable.BlendingState.RenderTarget0.Equals(Graphics.Output.BlendingState.States.RenderTarget0))
			    result |= StateChanges.BlendState;

			if (renderable.PrimitiveType != Graphics.Input.PrimitiveType)
				result |= StateChanges.PrimitiveType;

			if (!renderable.VertexBufferBinding.Equals(Graphics.Input.VertexBuffers[0]))
				result |= StateChanges.VertexBuffer;

			if (renderable.IndexBuffer != Graphics.Input.IndexBuffer)
				result |= StateChanges.IndexBuffer;

			if (!renderable.SamplerState.Equals(Shaders.PixelShader.Samplers[0]))
			    result |= StateChanges.Sampler;

			if (!renderable.RasterizerStates.Equals(Graphics.Rasterizer.States))
			    result |= StateChanges.Raster;

			if (!renderable.DepthStencilState.Equals(Graphics.Output.DepthStencilState))
				result |= StateChanges.DepthStencil;

			if (renderable.AlphaTestValues != Shaders.AlphaTestValue)
			{
				result |= StateChanges.AlphaTestValue;

				if (renderable.AlphaTestValues.HasValue)
				{
					if ((Shaders.PixelShader == Shaders.DefaultPixelShaderTextured) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuse))
						result |= StateChanges.Shader;
				}
				else
				{
					if ((Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuseAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTexturedAlphaTest))
						result |= StateChanges.Shader;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to apply the states for a renderable.
		/// </summary>
		/// <param name="renderable">Renderable with states to apply.</param>
		/// <param name="stateChange">Which states have changed.</param>
		private void ApplyStates(GorgonRenderable renderable, StateChanges stateChange)
		{
			if ((stateChange & StateChanges.Texture) == StateChanges.Texture)
				Shaders.PixelShader.Textures[0] = renderable.Texture;

			if ((stateChange & StateChanges.Shader) == StateChanges.Shader)
			{
				// If we're using the default shader, switch between the default no texture or textured pixel shader depending on our state.
				if (renderable.Texture != null) 
				{
					if (renderable.AlphaTestValues.HasValue)
					{
						if ((Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuse) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTextured) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuseAlphaTest))
							Shaders.PixelShader = Shaders.DefaultPixelShaderTexturedAlphaTest;
					}
					else
					{
						if ((Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuse) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuseAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTexturedAlphaTest))
							Shaders.PixelShader = Shaders.DefaultPixelShaderTextured;
					}
				}
				else
				{
					// If we're using the default shader, switch between the default no texture or textured pixel shader depending on our state.
					if (renderable.AlphaTestValues.HasValue)
					{
						if ((Shaders.PixelShader == Shaders.DefaultPixelShaderTexturedAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTextured) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuse))
							Shaders.PixelShader = Shaders.DefaultPixelShaderDiffuseAlphaTest;
					}
					else
					{
						if ((Shaders.PixelShader == Shaders.DefaultPixelShaderTexturedAlphaTest) || (Shaders.PixelShader == Shaders.DefaultPixelShaderTextured) || (Shaders.PixelShader == Shaders.DefaultPixelShaderDiffuseAlphaTest))
							Shaders.PixelShader = Shaders.DefaultPixelShaderDiffuse;
					}
				}
			}

			if ((stateChange & StateChanges.Raster) == StateChanges.Raster)
				Graphics.Rasterizer.States = renderable.RasterizerStates;

			if ((stateChange & StateChanges.BlendFactor) == StateChanges.BlendFactor)
				Graphics.Output.BlendingState.BlendFactor = renderable.BlendFactor;

			if ((stateChange & StateChanges.BlendState) == StateChanges.BlendState)
				Graphics.Output.BlendingState.States = renderable.BlendingState;

			if ((stateChange & StateChanges.PrimitiveType) == StateChanges.PrimitiveType)
				Graphics.Input.PrimitiveType = renderable.PrimitiveType;

			if ((stateChange & StateChanges.IndexBuffer) == StateChanges.IndexBuffer)
				Graphics.Input.IndexBuffer = renderable.IndexBuffer;

			if ((stateChange & StateChanges.VertexBuffer) == StateChanges.VertexBuffer)
				Graphics.Input.VertexBuffers[0] = renderable.VertexBufferBinding;

			if ((stateChange & StateChanges.AlphaTestValue) == StateChanges.AlphaTestValue)
				Shaders.AlphaTestValue = renderable.AlphaTestValues;

			if ((stateChange & StateChanges.Sampler) == StateChanges.Sampler)
				Shaders.PixelShader.Samplers[0] = renderable.SamplerState;
		}

		/// <summary>
		/// Function to add a renderable object to the vertex buffer.
		/// </summary>
		/// <param name="renderable">Renderable object to add.</param>
		internal void AddRenderable(GorgonRenderable renderable)
		{
			int cacheIndex = 0;
			int verticesCount = renderable.Vertices.Length;
			int cacheEnd = verticesCount + _cacheEnd;
			StateChange stateChange = StateChange.None;
			bool hasRendered = false;

			// Check for state changes.
			stateChange = _stateManager.CheckState(renderable);

			if (stateChange != StateChange.None)
			{
				if (_cacheWritten > 0)
				{
					RenderObjects();
					hasRendered = true;
				}
				_stateManager.ApplyState(renderable, stateChange);
			}

			// If the next set of vertices is going to overflow the buffer, then render the buffer contents.
			if (cacheEnd > _vertexCache.Length)
			{
				// Ensure that we don't render the same scene twice.
				if ((!hasRendered) && (_cacheWritten > 0))
					RenderObjects();

				_cacheStart = 0;
				_cacheEnd = 0;
				_renderIndexStart = 0;
			}

			for (int i = 0; i < verticesCount; i++)
			{
				cacheIndex = _cacheEnd + i;
				_vertexCache[cacheIndex] = renderable.Vertices[i];
			}

			_renderIndexCount += renderable.IndexCount;
			_cacheEnd = cacheEnd;
			_cacheWritten = _cacheEnd - _cacheStart;

			// If we've filled the cache, then empty it.
			if (_cacheEnd == _vertexCache.Length)
			{
				RenderObjects();
				_cacheStart = 0;
				_cacheEnd = 0;
				_renderIndexStart = 0;
			}
		}

		/// <summary>
		/// Function to render our objects with the current state.
		/// </summary>
		internal void RenderObjects()
		{
			BufferLockFlags flags = BufferLockFlags.Discard | BufferLockFlags.Write;

			if (_cacheWritten == 0)
				return;

			if (_cacheStart > 0)
				flags = BufferLockFlags.NoOverwrite | BufferLockFlags.Write;

			using (GorgonDataStream stream = DefaultVertexBufferBinding.VertexBuffer.Lock(flags))
			{
				stream.Position = _cacheStart * _vertexSize;
				stream.WriteRange<Vertex>(_vertexCache, _cacheStart, _cacheWritten);
				DefaultVertexBufferBinding.VertexBuffer.Unlock();
			}

			switch (Graphics.Input.PrimitiveType)
			{
				case PrimitiveType.PointList:
				case PrimitiveType.LineList:
					Graphics.Draw(_cacheStart, _cacheWritten);
					break;
				case PrimitiveType.TriangleList:
					if (Graphics.Input.IndexBuffer == null)
						Graphics.Draw(_cacheStart, _cacheWritten);
					else
						Graphics.DrawIndexed(_renderIndexStart, 0, _renderIndexCount);
					break;
			}

			_cacheStart = _cacheEnd;
			_cacheWritten = 0;
			_renderIndexStart += _renderIndexCount;
			_renderIndexCount = 0;
		}

		/// <summary>
		/// Function to start 2D rendering.
		/// </summary>
		/// <remarks>This is used to remember previous states, and set the default states for the 2D renderer.
		/// <para>The 2D renderer uses a LIFO stack to remember the last set of states, so calling this method multiple times and calling <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.End2D">End2D</see> will restore the last set of render states prior to the Begin2D call.</para>
		/// <para>This is implicitly called by the constructor and does not need to be called after creating an instance of the 2D interface.</para>
		/// </remarks>
		public void Begin2D()
		{
			GorgonBlendStates state = GorgonBlendStates.DefaultStates;
			GorgonRasterizerStates rasterState = GorgonRasterizerStates.DefaultStates;

			_stateRecall.Push(new PreviousStates(Graphics));

			rasterState.IsMultisamplingEnabled = UseMultisampling = Graphics.Rasterizer.States.IsMultisamplingEnabled;

			// Set our default shaders.
			Shaders.VertexShader = Shaders.DefaultVertexShader;
			Shaders.PixelShader = Shaders.DefaultPixelShaderDiffuse;
			Graphics.Input.IndexBuffer = DefaultIndexBuffer;
			Graphics.Input.VertexBuffers[0] = DefaultVertexBufferBinding;
			Graphics.Input.Layout = _layout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

			state.RenderTarget0.SourceBlend = BlendType.SourceAlpha;
			state.RenderTarget0.DestinationBlend = BlendType.InverseSourceAlpha;
			state.RenderTarget0.IsBlendingEnabled = true;

			if (Shaders.PixelShader != null)
			{
				GorgonTextureSamplerStates sampler = GorgonTextureSamplerStates.DefaultStates;
				sampler.TextureFilter = TextureFilter.Point;
				Shaders.PixelShader.Samplers[0] = sampler;
				Shaders.PixelShader.Textures[0] = null;
			}

			Graphics.Rasterizer.SetViewport(Target.Viewport);
			Graphics.Rasterizer.SetClip(new System.Drawing.Rectangle(0, 0, Target.Settings.Width, Target.Settings.Height));

			Graphics.Rasterizer.States = rasterState;
			Graphics.Output.BlendingState.States = state;

			if (_stateManager == null)
				_stateManager = new GorgonStateManager(this);

			_stateManager.GetDefaults();

			UpdateTarget();
		}

		/// <summary>
		/// Function to end 2D rendering.
		/// </summary>
		/// <remarks>This will restore the states to their original values before the 2D renderer was started, or to when the last <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.Begin2D">Begin2D</see> method called.
		/// <para>The 2D renderer uses a LIFO stack to remember the last set of states, so calling this method multiple times will rewind the stack for each Begin2D call.</para>
		/// </remarks>		
		public void End2D()
		{
			if (_stateRecall.Count > 0)
			{
				PreviousStates states = _stateRecall.Pop();
				states.Restore(Graphics);
			}
		}

		/// <summary>
		/// Function to force the renderer to render its data to the current render target.
		/// </summary>
		/// <remarks>Call this method to draw the renderable objects to the target.  If this method is not called, then nothing will appear on screen.
		/// <para>Gorgon uses a cache of vertex data to queue up what needs to be drawn in order to maintain performance.  However, if this queue gets 
		/// full or the state (i.e. Texture, Blending mode, etc...) changes then this method is called implicitly.</para>
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) since the graphics library had control over the main loop.  Since it does not any more, the 
		/// user is now responsible for calling this method.</para>
		/// </remarks>
		public void Render()
		{
			RenderObjects();
			Target.Flip();			
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <returns>A new sprite.</returns>
		public GorgonSprite CreateSprite(string name, float width, float height)
		{
			return new GorgonSprite(this, name, width, height);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2D"/> class.
		/// </summary>
		/// <param name="target">The primary render target to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		public Gorgon2D(GorgonSwapChain target)
		{
			GorgonDebug.AssertNull<GorgonSwapChain>(target, "target");

			_stateRecall = new Stack<PreviousStates>(16);
			TrackedObjects = new GorgonTrackedObjectCollection();
			Graphics = target.Graphics;
			_defaultTarget = target;

			Initialize();
			Begin2D();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					End2D();

					TrackedObjects.ReleaseAll();

					if (Shaders != null)
						Shaders.CleanUp();

					if (Target != null)
						Target.Resized -= new EventHandler(target_Resized);

					if (_layout != null)
						_layout.Dispose();
					if (DefaultVertexBufferBinding != null)
						DefaultVertexBufferBinding.VertexBuffer.Dispose();
					if (DefaultIndexBuffer != null)
						DefaultIndexBuffer.Dispose();
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
