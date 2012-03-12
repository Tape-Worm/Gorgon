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
using System.Drawing;
using System.Runtime.InteropServices;
using SlimMath;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A vertex for a renderable object.
	/// </summary>		
	public struct Gorgon2DVertex
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
		/// Initializes a new instance of the <see cref="Gorgon2DVertex"/> struct.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="color">The color.</param>
		/// <param name="uv">The texture coordinate.</param>
		public Gorgon2DVertex(Vector4 position, Vector4 color, Vector2 uv)
		{
			Position = position;
			Color = color;
			UV = uv;
		}
	}

	/// <summary>
	/// The renderer for 2D graphics.
	/// </summary>
	/// <remarks>This is the interface to handle sprites, primitives like lines, circles, ellipses, etc... and text.  The 2D renderer allows for using sprites in a 3D space.  
	/// That is, it respects the depth axis so that placing a 2 sprites, one at coordinates (0.1, 0.1, 0.1) and the other at (0.1, 0.1, 1.0), will make the 2nd appear 
	/// smaller.
	/// <para>A default render target is required to use this interface, however that render target can be a render target texture or <see cref="M:GorgonLibrary.Graphics.GorgonOutputMerger.CreateSwapChain">swap chain</see>.  
	/// Note that this does not mean that this interface is limited to one target, the target can be changed at will via the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.Target">Target</see> property.</para>
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
			public GorgonRenderTarget Target;
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
			/// Depth stencil states
			/// </summary>
			public GorgonDepthStencilStates DepthStencilState;
			/// <summary>
			/// Depth stencil reference.
			/// </summary>
			public int DepthStencilReference;

			/// <summary>
			/// Function to restore the previous states.
			/// </summary>
			/// <param name="graphics">Graphics interface.</param>
			public void Restore(GorgonGraphics graphics)
			{				
				graphics.Output.RenderTargets[0] = Target;
				if (Target != null)
					graphics.Rasterizer.SetViewport(Target.Viewport);
				graphics.Input.IndexBuffer = IndexBuffer;
				graphics.Input.VertexBuffers[0] = VertexBuffer;
				graphics.Input.Layout = InputLayout;
				graphics.Input.PrimitiveType = PrimitiveType;
				graphics.Shaders.PixelShader.Current = PixelShader;
				graphics.Shaders.VertexShader.Current = VertexShader;
				graphics.Output.BlendingState.BlendSampleMask = BlendSampleMask;
				graphics.Output.BlendingState.BlendFactor = BlendFactor;
				graphics.Output.BlendingState.States = BlendStates;
				graphics.Output.DepthStencilState.States = DepthStencilState;
				graphics.Output.DepthStencilState.DepthStencilReference = DepthStencilReference;
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
				DepthStencilState = graphics.Output.DepthStencilState.States;
				DepthStencilReference = graphics.Output.DepthStencilState.DepthStencilReference;
				RasterStates.IsScissorTestingEnabled = false;
			}
		}
		#endregion

		#region Variables.
		private GorgonSwapChain _swapChain = null;													// Swap chain target.
		private int _baseVertex = 0;																// Base vertex.
		private int _vertexSize = 0;																// Size, in bytes, of a vertex.
		private Gorgon2DVertex[] _vertexCache = null;												// List of vertices to cache.
		private int _cacheStart = 0;																// Starting cache vertex buffer index.
		private int _renderIndexStart = 0;															// Starting index to render.
		private int _renderIndexCount = 0;															// Number of indices to render.
		private int _cacheEnd = 0;																	// Ending vertex buffer cache index.
		private int _cacheWritten = 0;																// Number of vertices written.
		private bool _useCache = true;																// Flag to indicate that we want to use the cache.
		private bool _disposed = false;																// Flag to indicate that the object was disposed.
		private int _cacheSize = 32768;																// Number of vertices that we can stuff into a vertex buffer.
		private GorgonRenderTarget _defaultTarget = null;											// Default render target.
		private GorgonRenderTarget _target = null;													// Current render target.	
		private GorgonInputLayout _layout = null;													// Input layout.
		private Stack<PreviousStates> _stateRecall = null;											// State recall.
		private GorgonStateManager _stateManager = null;											// State manager.
		private bool _multiSampleEnable = false;													// Flag to indicate that multi sampling is enabled.
		private GorgonViewport? _viewPort = null;													// Viewport to use.
		private Rectangle? _clip = null;															// Clipping rectangle.
		private ICamera _camera = null;																// Current camera.
		private GorgonOrthoCamera _defaultCamera = null;											// Default camera.
		private GorgonSprite _logoSprite = null;													// Logo sprite.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the icons used for specific renderable items.
		/// </summary>
		internal GorgonTexture2D Icons
		{
			get;
			private set;
		}

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
		/// Property to return the currently active camera.
		/// </summary>
		internal ICamera CurrentCamera
		{
			get
			{
				return (_camera == null ? _defaultCamera : _camera);
			}
		}

		/// <summary>
		/// Property to return the renderables interface.
		/// </summary>
		/// <remarks>This is used to create renderable objects (sprites, ellipses, lines, rectangles, etc...) or draw them directly.</remarks>
		public GorgonRenderables Renderables
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether the Gorgon logo should be shown in the lower right hand corner of the target.
		/// </summary>
		public bool IsLogoVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the immediate drawing interface.
		/// </summary>
		/// <remarks>This is used to draw items like points, rectangles, etc...  It is also used to perform quick blitting of render targets and textures to the current render target.</remarks>
		public GorgonDrawing Drawing
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to use multisampling.
		/// </summary>
		/// <remarks>This will turn multisampling on or off.
		/// <para>Please note that if using a video device that supports SM4_1 or SM5, this setting cannot be disabled.  SM4_1/5 video devices always enable multisampling 
		/// when the sample count is greater than 1 for a render target.  For SM2_a_b or SM4 devices, this setting will disable multisampling regardless of sample count.</para>
		/// </remarks>
		public bool IsMultisamplingEnabled
		{
			get
			{
				return _multiSampleEnable;
			}
			set
			{
				if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b))
					_multiSampleEnable = value;
			}
		}

		/// <summary>
		/// Property to set or return the current viewport.
		/// </summary>
		/// <remarks>Changing this will constrain the view to the area passed in.  By defining a new viewport the display area will be stretched or shrunk to accomodate 
		/// the size of the view and consequently all rendered data in the view will be scaled appropriately.
		/// <para>This will not allow for clipping to a rectangle.  Use the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.ClipRegion">ClipRegion</see> property instead.</para>
		/// </remarks>
		public GorgonViewport? Viewport
		{
			get
			{
				return _viewPort;
			}
			set
			{
				if (_viewPort != value)
				{
					// Force a render when switching viewports.
					RenderObjects();

					_viewPort = value;
					if (value != null)
						Graphics.Rasterizer.SetViewport(_viewPort.Value);
					else
						Graphics.Rasterizer.SetViewport(Target.Viewport);
				}
			}
		}

		/// <summary>
		/// Property to set or return the clipping region.
		/// </summary>
		/// <remarks>Use this to clip a rectangular region on the target.  Pixels outside of the region do not get rendered.
		/// <para>Clipping state is not restored when <see cref="M:GorgonLibrary.Renderers.Gorgon2D.End2D">End2D</see> is called, it is merely turned off and must be restored by the user.</para>
		/// </remarks>
		public Rectangle? ClipRegion
		{
			get
			{
				return _clip;
			}
			set
			{
				if (_clip != value)
				{
					RenderObjects();

					_clip = value;
					if (value != null)
						Graphics.Rasterizer.SetClip(_clip.Value);
				}
			}
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
		/// Property to set or return whether the depth buffer should be enabled.
		/// </summary>
		public bool IsDepthBufferEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the stencil buffer is enabled.
		/// </summary>
		public bool IsStencilEnabled
		{
			get;
			set;
		}
		
		/// <summary>
		/// Property to set or return whether blending is enabled or not.
		/// </summary>
		public bool IsBlendingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use alpha testing for this renderable.
		/// </summary>
		/// <remarks>The alpha testing tests to see if an alpha value is between or equal to the values in <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.AlphaTestValues">AlphaTestValues</see> and rejects the pixel if it is not.
		/// <para>Typically, performance is improved when alpha testing is turned on with a range of 0.  This will reject any pixels with an alpha of 0.</para>
		/// <para>Be aware that the default shaders implement alpha testing.  However, a custom shader will have to make use of the GorgonAlphaTest constant buffer 
		/// in order to take advantage of alpha testing.</para>
		/// </remarks>
		public bool IsAlphaTestEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current camera.
		/// </summary>
		public ICamera Camera
		{
			get
			{
				return _camera;
			}
			set
			{
				if (_camera != value)
				{
					RenderObjects();
					_camera = value;

					// Force an update.
					CurrentCamera.Update();
					Shaders.UpdateGorgonTransformation();
				}
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
				if (value < 1024)
					value = 1024;
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
		/// <remarks>Changing the current render target will reset the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.Viewport">Viewport</see> and the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.ClipRegion">ClipRegion</see>.</remarks>
		public GorgonRenderTarget Target
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
					RenderObjects();

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

			// Find out if the target is a swap chain.
			_swapChain = Target as GorgonSwapChain;

			Graphics.Output.RenderTargets[0] = Target;

			// Update our default camera.
			// User cameras will need to be updated by the user on a resize or target change.
			if (_camera == null)
			{
				_defaultCamera.UpdateFromTarget(Target);
				_defaultCamera.Anchor = new Vector2(Target.Settings.Width / 2.0f, Target.Settings.Height / 2.0f);
				_defaultCamera.Position = -_defaultCamera.Anchor;
			}

			if (ClipRegion != null)
				ClipRegion = null;

			if (_viewPort == null)
				Graphics.Rasterizer.SetViewport(Target.Viewport);
			else
				Graphics.Rasterizer.SetViewport(_viewPort.Value);

			// Re-assign the event.
			if (Target != null)
				Target.Resized += new EventHandler(target_Resized);
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
				_layout = Graphics.Input.CreateInputLayout("2D_Sprite_Vertex_Layout", typeof(Gorgon2DVertex), Shaders.DefaultVertexShader.Shader);
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
			_vertexCache = new Gorgon2DVertex[VertexCacheSize];
			_cacheStart = 0;
			_cacheEnd = 0;
			_cacheWritten = 0;
			_renderIndexStart = 0;		
			_renderIndexCount = 0;
			_baseVertex = 0;
		}

		/// <summary>
		/// Function to handle a resize of the current render target.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void target_Resized(object sender, EventArgs e)
		{
			UpdateTarget();
		}

		/// <summary>
		/// Function to render our objects with the current state.
		/// </summary>
		private void RenderObjects()
		{
			ICamera currentCamera = (_camera == null ? _defaultCamera : _camera);
			BufferLockFlags flags = BufferLockFlags.Discard | BufferLockFlags.Write;
			GorgonVertexBufferBinding vbBinding = Graphics.Input.VertexBuffers[0];

			if (_cacheWritten == 0)
				return;

			if ((currentCamera.NeedsProjectionUpdate) || (currentCamera.NeedsViewUpdate))
			{
				currentCamera.Update();

				// Send projection/view to the shader.
				Shaders.UpdateGorgonTransformation();
			}

			if (_cacheStart > 0)
				flags = BufferLockFlags.NoOverwrite | BufferLockFlags.Write;

			// If we're using the cache, then populate the default vertex buffer.
			if (_useCache)
			{
				// Ensure that we have a vertex buffer bound.
				if ((vbBinding.VertexBuffer == null) || (vbBinding.Stride == 0))
				{
					vbBinding = DefaultVertexBufferBinding;
					Graphics.Input.VertexBuffers[0] = vbBinding;
				}

				// Update buffers depending on type.
				switch (vbBinding.VertexBuffer.BufferUsage)
				{
					case BufferUsage.Dynamic:
						using (GorgonDataStream stream = vbBinding.VertexBuffer.Lock(flags))
						{
							stream.Position = _cacheStart * _vertexSize;
							stream.WriteRange<Gorgon2DVertex>(_vertexCache, _cacheStart, _cacheWritten);
							vbBinding.VertexBuffer.Unlock();
						}
						break;
					case BufferUsage.Default:
						using (GorgonDataStream stream = new GorgonDataStream(_vertexCache, _cacheStart, _cacheWritten))
							vbBinding.VertexBuffer.Update(stream, _cacheStart * _vertexSize, (int)stream.Length);
						break;
				}
			}

			switch (Graphics.Input.PrimitiveType)
			{
				case PrimitiveType.PointList:
				case PrimitiveType.LineList:
					Graphics.Output.Draw(_cacheStart, _cacheWritten);
					break;
				case PrimitiveType.TriangleList:
					if (Graphics.Input.IndexBuffer == null)
						Graphics.Output.Draw(_cacheStart, _cacheWritten);
					else
						Graphics.Output.DrawIndexed(_renderIndexStart, _baseVertex, _renderIndexCount);
					break;
			}

			_cacheStart = _cacheEnd;
			_cacheWritten = 0;
			_renderIndexStart += _renderIndexCount;
			_renderIndexCount = 0;
		}

		/// <summary>
		/// Function to add a renderable object to the vertex buffer.
		/// </summary>
		/// <param name="renderable">Renderable object to add.</param>
		internal void AddRenderable(IRenderable renderable)
		{
			int cacheIndex = 0;
			int verticesCount = renderable.VertexCount;
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

				// If we switch vertex buffers, then reset the cache.
				if ((stateChange & StateChange.VertexBuffer) == StateChange.VertexBuffer)
				{
					_baseVertex = 0;
					_cacheStart = 0;
					_cacheEnd = 0;
					_renderIndexCount = 0;
					_renderIndexStart = 0;
					cacheEnd = 0;
					_useCache = Graphics.Input.VertexBuffers[0].Equals(DefaultVertexBufferBinding);

					if (!_useCache)
					{
						// We skip the cache for objects that have their own vertex buffers.
						_cacheEnd = verticesCount;
						return;
					}
				}
			}

			// If the next set of vertices is going to overflow the buffer, then render the buffer contents.
			if (cacheEnd > _cacheSize)
			{
				// Ensure that we don't render the same scene twice.
				if ((!hasRendered) && (_cacheWritten > 0))
					RenderObjects();

				_baseVertex = 0;
				_cacheStart = 0;
				_cacheEnd = 0;
				_renderIndexStart = 0;
				cacheEnd = 0;
			}

			for (int i = 0; i < verticesCount; i++)
			{
				cacheIndex = _cacheEnd + i;
				_vertexCache[cacheIndex] = renderable.Vertices[i];
			}

			_renderIndexCount += renderable.IndexCount;
			_cacheEnd += verticesCount;
			_cacheWritten = _cacheEnd - _cacheStart;

			// We need to shift the vertices for those items that change the index buffer.
			_baseVertex += renderable.BaseVertexCount;
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depth">Depth value to clear with.</param>
		/// <param name="stencil">Stencil value to clear with.</param>
		/// <remarks>Unlike a render target <see cref="M:GorgonLibrary.Graphics.GorgonRenderTarget.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color, float depth, int stencil)
		{
			if ((_clip == null) && (_viewPort == null))
			{
				Target.Clear(color, depth, stencil);
				return;
			}

			if (Target.DepthStencil != null)
				Target.DepthStencil.Clear(depth, stencil);

			Drawing.FilledRectangle(new RectangleF(0, 0, Target.Settings.Width, Target.Settings.Height), color);
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depth">Depth value to clear with.</param>
		/// <remarks>Unlike a render target <see cref="M:GorgonLibrary.Graphics.GorgonRenderTarget.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color, float depth)
		{
			Clear(color, depth, 0);
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <remarks>Unlike a render target <see cref="M:GorgonLibrary.Graphics.GorgonRenderTarget.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color)
		{
			Clear(color, 1.0f, 0);
		}

		/// <summary>
		/// Function to create a new orthographic camera object.
		/// </summary>
		/// <param name="name">Name of the camera.</param>
		/// <param name="viewDimensions">Dimensions for the ortho camera.</param>
		/// <param name="maxDepth">Maximum depth of the camera.</param>
		/// <returns>A new orthographic camera.</returns>
		public GorgonOrthoCamera CreateCamera(string name, Vector2 viewDimensions, float maxDepth)
		{
			return new GorgonOrthoCamera(this, name, viewDimensions, maxDepth);
		}

		/// <summary>
		/// Function to start 2D rendering.
		/// </summary>
		/// <remarks>This is used to remember previous states, and set the default states for the 2D renderer.
		/// <para>The 2D renderer uses a LIFO stack to remember the last set of states, so calling this method multiple times and calling <see cref="M:GorgonLibrary.Renderers.Gorgon2D.End2D">End2D</see> will restore the last set of render states prior to the Begin2D call.</para>
		/// <para>This is implicitly called by the constructor and does not need to be called after creating an instance of the 2D interface.</para>
		/// </remarks>
		public void Begin2D()
		{
			_stateRecall.Push(new PreviousStates(Graphics));

			// Set our default shaders.
			Shaders.VertexShader = Shaders.DefaultVertexShader;
			Shaders.PixelShader = Shaders.DefaultPixelShaderDiffuse;
			Graphics.Input.IndexBuffer = DefaultIndexBuffer;
			Graphics.Input.VertexBuffers[0] = DefaultVertexBufferBinding;
			Graphics.Input.Layout = _layout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;
						
			IsMultisamplingEnabled = Graphics.Rasterizer.States.IsMultisamplingEnabled;

			// By default, turn on multi sampling over a count of 2.
			if ((!IsMultisamplingEnabled) && (Target.Settings.MultiSample.Count > 1) && ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4_1) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM5)))
			    IsMultisamplingEnabled = true;

			IsBlendingEnabled = true;
			IsAlphaTestEnabled = true;

			if (Shaders.PixelShader != null)
			{
				GorgonTextureSamplerStates sampler = GorgonTextureSamplerStates.DefaultStates;
				sampler.TextureFilter = TextureFilter.Point;
				Shaders.PixelShader.Samplers[0] = sampler;
				Shaders.PixelShader.Textures[0] = null;
			}

			Graphics.Rasterizer.SetViewport(Target.Viewport);
			Graphics.Rasterizer.States = GorgonRasterizerStates.DefaultStates;
			Graphics.Output.BlendingState.States = GorgonBlendStates.DefaultStates;			
			Graphics.Output.DepthStencilState.States = GorgonDepthStencilStates.DefaultStates;
			Graphics.Output.DepthStencilState.DepthStencilReference = 0;

			if (_stateManager == null)
				_stateManager = new GorgonStateManager(this);

			_stateManager.GetDefaults();			

			UpdateTarget();
		}

		/// <summary>
		/// Function to end 2D rendering.
		/// </summary>
		/// <remarks>This will restore the states to their original values before the 2D renderer was started, or to when the last <see cref="M:GorgonLibrary.Renderers.Gorgon2D.Begin2D">Begin2D</see> method called.
		/// <para>The 2D renderer uses a LIFO stack to remember the last set of states, so calling this method multiple times will rewind the stack for each Begin2D call.</para>
		/// <para>When restoring, the viewport may not be reset when the initial render target is NULL (Nothing in VB.Net), and consequently will need to be set when a new render target is assigned to the <see cref="GorgonLibrary.Graphics.GorgonGraphics">Graphics</see> interface.</para>
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
		/// Function to draw the Gorgon logo at the bottom-right of the screen.
		/// </summary>
		private void DrawLogo()
		{
			_logoSprite.Position = new Vector2(Target.Settings.Width, Target.Settings.Height);
			_logoSprite.Draw();
		}

		/// <summary>
		/// Function to force the renderer to render its data to the current render target.
		/// </summary>
		/// <param name="flip">TRUE to flip the back buffer to the front buffer of the render target.</param>
		/// <remarks>Call this method to draw the renderable objects to the target.  If this method is not called, then nothing will appear on screen.
		/// <para>Gorgon uses a cache of vertex data to queue up what needs to be drawn in order to maintain performance.  However, if this queue gets 
		/// full or the state (i.e. Texture, Blending mode, etc...) changes then this method is called implicitly.</para>
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) since the graphics library had control over the main loop.  Since it does not any more, the 
		/// user is now responsible for calling this method.</para>
		/// </remarks>
		public void Render(bool flip)
		{
			ICamera camera = _camera;
			GorgonViewport? previousViewport = _viewPort;
			Rectangle? previousClip = _clip;

			// Only draw the logo when we're flipping, and we're on the default target and the default target is a swap chain.
			if ((flip) && (IsLogoVisible) && (_swapChain != null) && (_swapChain == _defaultTarget))
			{
				// Reset any view/projection/clip/viewport.
				if (_camera != null)
					Camera = null;
				if (_viewPort != null)
					Viewport = null;
				if (_clip != null)
					ClipRegion = null;

				DrawLogo();
			}

			RenderObjects();

			if (flip)
			{
				if (_swapChain != null)
					_swapChain.Flip();

				if (IsLogoVisible)
				{
					if (camera != null)
						Camera = camera;
					if (previousViewport != null)
						Viewport = previousViewport;
					if (previousClip != null)
						ClipRegion = previousClip;
				}
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
			Render(true);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2D"/> class.
		/// </summary>
		/// <param name="target">The primary render target to use.</param>		
		internal Gorgon2D(GorgonRenderTarget target)
		{			
			_stateRecall = new Stack<PreviousStates>(16);
			TrackedObjects = new GorgonTrackedObjectCollection();
			Graphics = target.Graphics;
			_defaultTarget = target;
			_swapChain = target as GorgonSwapChain;

			Initialize();

			Icons = Graphics.Textures.FromGDIBitmap("Gorgon2D.Icons", Properties.Resources.Icons, GorgonTexture2DSettings.FromFile);
			_logoSprite = new GorgonSprite(this, "Gorgon2D.LogoSprite", new GorgonSpriteSettings()
			{
				Anchor = new Vector2(200, 56),
				Texture = Graphics.Textures.GorgonLogo,
				TextureRegion = new RectangleF(0, 0, 200, 56),
				Color = Color.White,
				Size = new Vector2(200, 56)
			});
			_defaultCamera = new GorgonOrthoCamera(this, "Gorgon.Camera.Default", new Vector2(target.Settings.Width, target.Settings.Height), 100.0f);			

			Begin2D();

			Renderables = new GorgonRenderables(this);
			Drawing = new GorgonDrawing(this);
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

					if (Icons != null)
						Icons.Dispose();

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

					Graphics.RemoveTrackedObject(this);
				}

				Icons = null;
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
