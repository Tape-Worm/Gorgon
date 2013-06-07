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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Native;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A vertex for a renderable object.
	/// </summary>		
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Gorgon2DVertex
	{
        /// <summary>
        /// The size of the vertex, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = DirectAccess.SizeOf<Gorgon2DVertex>();

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
	/// <remarks>This is the interface that renders 2D graphics such as sprites, lines, circles, etc...  This object is also a factory for various types of 2D renderable objects such as a <see cref="GorgonLibrary.Renderers.GorgonSprite">Sprite</see>.
	/// <para>This renderer also handles state management for the various 2D objects through the exposed properties on the renderer object and automatically through the states from the properties on each object being rendered.</para>
	/// <para>A developer can initialize this object with any render target as the default render target, or one will be created automatically when this object is initialized.  
	/// Note that this does not mean that this interface is limited to one target, the target can be changed at will via the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.Target">Target</see> property.  
	/// It is important to use the 2D interface Target property as it will perform state checking to keep rendering consistent.
	/// </para>
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

		#region Variables.
		private int _baseVertex;														// Base vertex.		
		private Gorgon2DVertex[] _vertexCache;											// List of vertices to cache.
		private int _cacheStart;														// Starting cache vertex buffer index.
		private int _renderIndexStart;													// Starting index to render.
		private int _renderIndexCount;													// Number of indices to render.
		private int _cacheEnd;															// Ending vertex buffer cache index.
		private int _cacheWritten;														// Number of vertices written.
		private bool _useCache = true;													// Flag to indicate that we want to use the cache.
		private bool _disposed;															// Flag to indicate that the object was disposed.
		private int _cacheSize = 32768;													// Number of vertices that we can stuff into a vertex buffer.
		private GorgonInputLayout _layout;												// Input layout.
		private bool _multiSampleEnable;												// Flag to indicate that multi sampling is enabled.
		private GorgonViewport? _viewPort;												// Viewport to use.
		private Rectangle? _clip;														// Clipping rectangle.
		private ICamera _camera;														// Current camera.
		private readonly GorgonOrthoCamera _defaultCamera;								// Default camera.
		private readonly GorgonSprite _logoSprite;										// Logo sprite.
		private GorgonDataStream _projectionViewStream;									// Stream used to write to the projection/view matrix buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the swap chain created was created by the system or by the user.
		/// </summary>
		internal bool SystemCreatedTarget
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the global state manager.
		/// </summary>
		internal GorgonStateManager StateManager
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the buffer for the projection/view matrix.
		/// </summary>
		internal GorgonConstantBuffer ProjectionViewBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the alpha test constant buffer.
		/// </summary>
		internal GorgonConstantBuffer AlphaTestBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the stream to the alpha test buffer.
		/// </summary>
		internal GorgonDataStream AlphaTestStream
		{
			get;
			private set;
		}

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
		internal GorgonDisposableObjectCollection TrackedObjects
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
				return (_camera ?? _defaultCamera);
			}
		}

		/// <summary>
		/// Property to return the default render target.
		/// </summary>
		/// <remarks>This is the inital target that the Gorgon2D interface was created with, or the one internally generated depending on the constructor being used.</remarks>
		public GorgonRenderTarget2D DefaultTarget
		{
			get;
			private set;
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
						Graphics.Rasterizer.SetScissorRectangle(_clip.Value);
				}
			}
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

					// Refresh our camera information if we're jumping back to the default camera.
					if (value == null)
					{
						_defaultCamera.UpdateFromTarget(Target.Settings.Width, Target.Settings.Height);
						_defaultCamera.Anchor = new Vector2(Target.Settings.Width / 2.0f, Target.Settings.Height / 2.0f);
						_defaultCamera.Position = -_defaultCamera.Anchor;
					}

					// Force an update.
					CurrentCamera.Update();
					UpdateProjectionViewMatrix(CurrentCamera);
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
		/// Property to set or return the active render target view.
		/// </summary>
		/// <remarks>Changing the current render target will reset the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.Viewport">Viewport</see> and the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.ClipRegion">ClipRegion</see>.</remarks>
		public GorgonRenderTarget2D Target
		{
			get
			{
				var targetView = Graphics.Output.GetRenderTarget(0);
				
				// TODO: Clean this shit up.
				return targetView == null ? DefaultTarget : (targetView.Resource as GorgonRenderTarget2D) ?? DefaultTarget;
			}
			set
			{
                // TODO: Clean this shit up.
				if (Graphics.Output.GetRenderTarget(0) == GorgonRenderTarget2D.ToRenderTargetView(value))
				{
					return;
				}

				RenderObjects();
				UpdateTarget(value ?? DefaultTarget);
			}
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public Gorgon2DVertexShaderState VertexShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the pixel shader state(s).
		/// </summary>
		/// <remarks>Use this to set constant buffers, textures, and the current pixel shader.</remarks>
		public Gorgon2DPixelShaderState PixelShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a group of pre-defined effects.
		/// </summary>
		public Gorgon2DEffects Effects
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the projection view matrix.
		/// </summary>
		/// <param name="currentCamera">The currently active camera.</param>
		private void UpdateProjectionViewMatrix(ICamera currentCamera)
		{
			_projectionViewStream.Position = 0;
			_projectionViewStream.Write(currentCamera.ViewProjection);
			_projectionViewStream.Position = 0;
			ProjectionViewBuffer.Update(_projectionViewStream);
		}

		/// <summary>
		/// Function to update the current render target.
		/// </summary>
		/// <param name="target">The target being bound.</param>
		private void UpdateTarget(GorgonRenderTarget2D target)
		{
			// Remove any previous handler.
			if (Target.SwapChain != null)
				Target.SwapChain.Resized -= new EventHandler<GorgonSwapChainResizedEventArgs>(target_Resized);

			Graphics.Output.SetRenderTarget(target, target.DepthStencilBuffer);

			// Update our default camera.
			// User cameras will need to be updated by the user on a resize or target change.
			if (_camera == null)
			{
                _defaultCamera.UpdateFromTarget(target.Settings.Width, target.Settings.Height);
				_defaultCamera.Anchor = new Vector2(target.Settings.Width / 2.0f, target.Settings.Height / 2.0f);
				_defaultCamera.Position = -_defaultCamera.Anchor;
			}

			ClipRegion = null;

			if (_viewPort == null)
				Graphics.Rasterizer.SetViewport(target.Viewport);
			else
				Graphics.Rasterizer.SetViewport(_viewPort.Value);

			// Re-assign the event.
			if (Target.SwapChain != null)
				Target.SwapChain.Resized += new EventHandler<GorgonSwapChainResizedEventArgs>(target_Resized);
		}

		/// <summary>
		/// Function to handle a resize of the current render target.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void target_Resized(object sender, GorgonSwapChainResizedEventArgs e)
		{
			UpdateTarget(Target);
		}

        /// <summary>
        /// Function to clear up the vertex cache.
        /// </summary>
        internal void ClearCache()
        {
            // Clear up the cache.
            _baseVertex = 0;
            _cacheStart = 0;
            _cacheEnd = 0;
            _renderIndexCount = 0;
            _renderIndexStart = 0;
            _cacheWritten = 0;
        }

		/// <summary>
		/// Function to initialize the 2D renderer.
		/// </summary>
		internal void Initialize()
		{
			// Create the default projection matrix.
			if ((Target != null) && (Target.SwapChain != null))
			{
				Target.SwapChain.Resized -= target_Resized;
			}

			// Create constant buffers.
			if (ProjectionViewBuffer == null)
			{
				ProjectionViewBuffer = Graphics.Buffers.CreateConstantBuffer("Gorgon2D Projection/View Matrix Constant Buffer",
				                                                             new GorgonConstantBufferSettings
					                                                             {
						                                                             SizeInBytes = Matrix.SizeInBytes,
					                                                             });
				_projectionViewStream = new GorgonDataStream(ProjectionViewBuffer.SizeInBytes);
			}

			if (AlphaTestBuffer == null)
			{
				AlphaTestBuffer = Graphics.Buffers.CreateConstantBuffer("Gorgon2D Alpha Test Constant Buffer",
				                                                        new GorgonConstantBufferSettings
					                                                        {
						                                                        SizeInBytes = 32
					                                                        });
				AlphaTestStream = new GorgonDataStream(AlphaTestBuffer.SizeInBytes);
				AlphaTestStream.Write(new byte[AlphaTestBuffer.SizeInBytes], 0, AlphaTestBuffer.SizeInBytes);
				AlphaTestStream.Position = 0;
				AlphaTestBuffer.Update(AlphaTestStream);
			}

			// Add shader includes.
			if (!Graphics.Shaders.IncludeFiles.Contains("Gorgon2DShaders"))
				Graphics.Shaders.IncludeFiles.Add("Gorgon2DShaders", Encoding.UTF8.GetString(Properties.Resources.BasicSprite));

			// Create shader states.
			if (PixelShader == null)
				PixelShader = new Gorgon2DPixelShaderState(this);

			if (VertexShader == null)
			{
				VertexShader = new Gorgon2DVertexShaderState(this);

				if (_layout != null)
					_layout.Dispose();
				_layout = null;
			}

			// Create pre-defined effects objects.
			if (Effects == null)
			    Effects = new Gorgon2DEffects(this);

			// Create default shaders.
			// Create layout information so we can bind our vertices to the shader.
			if (_layout == null)
			{
				_layout = Graphics.Input.CreateInputLayout("Gorgon2D Input Layout", typeof(Gorgon2DVertex), VertexShader.DefaultVertexShader);
			}

			if (DefaultIndexBuffer != null)
				DefaultIndexBuffer.Dispose();
			if (DefaultVertexBufferBinding.VertexBuffer != null)
				DefaultVertexBufferBinding.VertexBuffer.Dispose();
						
			int spriteVBSize = Gorgon2DVertex.SizeInBytes * _cacheSize;
			int spriteIBSize = sizeof(int) * _cacheSize * 6;

			// Set up our index buffer.
			using (var ibData = new GorgonDataStream(spriteIBSize))
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
				DefaultIndexBuffer = Graphics.Buffers.CreateIndexBuffer("Gorgon2D Default Index Buffer", new GorgonIndexBufferSettings()
					{
						IsOutput = false,
						SizeInBytes = (int)ibData.Length,
						Usage = BufferUsage.Immutable,
						Use32BitIndices = true
					}, ibData);
			}
			
			// Create our empty vertex buffer.
			DefaultVertexBufferBinding =
				new GorgonVertexBufferBinding(
					Graphics.Buffers.CreateVertexBuffer("Gorgon 2D Default Vertex Buffer", new GorgonBufferSettings
						{
							SizeInBytes = spriteVBSize,
							Usage = BufferUsage.Dynamic
						}), Gorgon2DVertex.SizeInBytes);

			// Create the vertex cache.
			_vertexCache = new Gorgon2DVertex[VertexCacheSize];
            ClearCache();
		}

		/// <summary>
		/// Function to render our objects with the current state.
		/// </summary>
		internal void RenderObjects()
		{
			ICamera currentCamera = (_camera ?? _defaultCamera);
			BufferLockFlags flags = BufferLockFlags.Discard | BufferLockFlags.Write;
			GorgonVertexBufferBinding vbBinding = Graphics.Input.VertexBuffers[0];

			if (_cacheWritten == 0)
				return;

			if ((currentCamera.NeedsProjectionUpdate) || (currentCamera.NeedsViewUpdate))
			{
				currentCamera.Update();

				// Send projection/view to the shader.
				UpdateProjectionViewMatrix(currentCamera);
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

#if DEBUG
				if (Graphics.Shaders.PixelShader.Current == null)
				{
					throw new NullReferenceException("No pixel shader was bound to the pipeline.  Cannot render.");
				}

				if (Graphics.Shaders.VertexShader.Current == null)
				{
					throw new NullReferenceException("No vertex shader was bound to the pipeline.  Cannot render.");
				}

				if ((Graphics.Input.VertexBuffers[0].Stride == 0) || (Graphics.Input.VertexBuffers[0].VertexBuffer == null))
				{
					throw new NullReferenceException("No vertex buffer was bound to the pipeline.  Cannot render.");
				}
#endif

				// Update buffers depending on type.
				switch (vbBinding.VertexBuffer.Settings.Usage)
				{
					case BufferUsage.Dynamic:
						using (GorgonDataStream stream = vbBinding.VertexBuffer.Lock(flags))
						{
							stream.Position = _cacheStart * Gorgon2DVertex.SizeInBytes;
							stream.WriteRange<Gorgon2DVertex>(_vertexCache, _cacheStart, _cacheWritten);
							vbBinding.VertexBuffer.Unlock();
						}
						break;
					case BufferUsage.Default:
						using (var stream = new GorgonDataStream(_vertexCache, _cacheStart, _cacheWritten))
							vbBinding.VertexBuffer.Update(stream, _cacheStart * Gorgon2DVertex.SizeInBytes, (int)stream.Length);
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
		/// Function to add vertices to the vertex cache without any state checking.
		/// </summary>
		/// <param name="vertices">Vertices to add.</param>
		/// <param name="baseVertex">Base vertex to start rendering with.</param>
		/// <param name="indicesPerPrimitive">Number of indices per primitive object.</param>
		/// <param name="vertexStart">Starting vertex to add.</param>
		/// <param name="vertexCount">Number of vertices to add.</param>
		/// <returns>The number of remaining vertices in the cache.</returns>
		internal void AddVertices(Gorgon2DVertex[] vertices, int baseVertex, int indicesPerPrimitive, int vertexStart, int vertexCount)
		{
			int cacheIndex = 0;
			int vertexIndex = 0;

			// Do nothing if there aren't any vertices.
			if (vertices.Length == 0)
				return;

			// If the next set of vertices is going to overflow the buffer, then render the buffer contents.
			if (vertexCount + _cacheEnd > _cacheSize)
			{
				// Ensure that we don't render the same scene twice.
				if (_cacheWritten > 0)
					RenderObjects();

				_baseVertex = 0;
				_cacheStart = 0;
				_cacheEnd = 0;
				_renderIndexStart = 0;
			}

			for (int i = 0; i < vertexCount; i++)
			{
				cacheIndex = _cacheEnd + i;
				vertexIndex = i + vertexStart;
				_vertexCache[cacheIndex] = vertices[vertexIndex];
			}

			_renderIndexCount += indicesPerPrimitive;
			_cacheEnd += vertexCount;
			_cacheWritten = _cacheEnd - _cacheStart;

			// We need to shift the vertices for those items that change the index buffer.
			_baseVertex += baseVertex;
		}

		/// <summary>
		/// Function to add a renderable object to the vertex buffer.
		/// </summary>
		/// <param name="renderable">Renderable object to add.</param>
		internal void AddRenderable(IRenderable renderable)
		{
			var stateChange = StateChange.None;

			// Check for state changes.
			stateChange = StateManager.CheckState(renderable);

			if (stateChange != StateChange.None)
			{
				if (_cacheWritten > 0)
					RenderObjects();

				StateManager.ApplyState(renderable, stateChange);

				// If we switch vertex buffers, then reset the cache.
				if ((stateChange & StateChange.VertexBuffer) == StateChange.VertexBuffer)
				{
                    ClearCache();
					_useCache = Graphics.Input.VertexBuffers[0].Equals(DefaultVertexBufferBinding);

					// We skip the cache for objects that have their own vertex buffers.
					if (!_useCache)
					{
						_cacheEnd = renderable.VertexCount;
						return;
					}
				}
			}

			AddVertices(renderable.Vertices, renderable.BaseVertexCount, renderable.IndexCount, 0, renderable.VertexCount);
		}

		/// <summary>
		/// Function to set up the renderers initial state.
		/// </summary>
		internal void Setup()
		{
			// Reset the cache values.
            ClearCache();

			// Set our default shaders.
			VertexShader.Current = VertexShader.DefaultVertexShader;
			PixelShader.Current = PixelShader.DefaultPixelShaderDiffuse;
			Graphics.Input.IndexBuffer = DefaultIndexBuffer;
			Graphics.Input.VertexBuffers[0] = DefaultVertexBufferBinding;
			Graphics.Input.Layout = _layout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

			IsMultisamplingEnabled = Graphics.Rasterizer.States.IsMultisamplingEnabled;

			// By default, turn on multi sampling over a count of 2.
			if ((!IsMultisamplingEnabled)
				&& (Target.Settings.Multisampling.Count > 1)
				&& ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4_1)
					|| (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM5)))
			{
				IsMultisamplingEnabled = true;
			}

			IsBlendingEnabled = true;
			IsAlphaTestEnabled = true;

			// Add shader includes if they're gone.
			if (!Graphics.Shaders.IncludeFiles.Contains("Gorgon2DShaders"))
				Graphics.Shaders.IncludeFiles.Add("Gorgon2DShaders", Encoding.UTF8.GetString(Properties.Resources.BasicSprite));

			if (PixelShader != null)
			{
				GorgonTextureSamplerStates sampler = GorgonTextureSamplerStates.DefaultStates;
				sampler.TextureFilter = TextureFilter.Point;
				Graphics.Shaders.PixelShader.TextureSamplers[0] = sampler;
				Graphics.Shaders.PixelShader.Resources[0] = null;
			}

			Graphics.Rasterizer.SetViewport(Target.Viewport);
			Graphics.Rasterizer.States = GorgonRasterizerStates.DefaultStates;
			Graphics.Output.BlendingState.States = GorgonBlendStates.DefaultStates;
			Graphics.Output.DepthStencilState.States = GorgonDepthStencilStates.DefaultStates;
			Graphics.Output.DepthStencilState.DepthStencilReference = 0;

			if (StateManager == null)
				StateManager = new GorgonStateManager(this);

			StateManager.GetDefaults();

			UpdateTarget(DefaultTarget);
		}

		/// <summary>
		/// Function to create an 2D specific effect object.
		/// </summary>
		/// <typeparam name="T">Type of effect to create.</typeparam>
		/// <param name="name">Name of the effect.</param>
		/// <param name="passCount">Number of passes in the effect.</param>
		/// <returns>The new effect object.</returns>
		/// <remarks>Effects are used to simplify rendering with multiple passes when using a shader.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="passCount"/> parameter is less than 0.</exception>
		public T Create2DEffect<T>(string name, int passCount)
			where T : Gorgon2DEffect
		{
			var effect = (T)Activator.CreateInstance(typeof(T), new object[] { this, name, passCount });

			TrackedObjects.Add(effect);

			return effect;
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

			if (Target.DepthStencilBuffer != null)
				Target.DepthStencilBuffer.Clear(depth, stencil);

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
		/// <param name="interval">Presentation interval.</param>
		/// <remarks>Call this method to draw the renderable objects to the target.  If this method is not called, then nothing will appear on screen.
		/// <para>Gorgon uses a cache of vertex data to queue up what needs to be drawn in order to maintain performance.  However, if this queue gets 
		/// full or the state (i.e. Texture, Blending mode, etc...) changes then this method is called implicitly.</para>
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) because the graphics library had control over the main loop.  
		/// Since it does not any more, the user is now responsible for calling this method.</para>
		/// <para>The <paramref name="interval"/> parameter is the number of vertical retraces to wait.  If the <paramref name="flip"/> parameter is FALSE, then the interval parameter has no effect.</para>
		/// </remarks>
		public void Render(bool flip, int interval)
		{
			ICamera camera = _camera;
			GorgonViewport? previousViewport = _viewPort;
			Rectangle? previousClip = _clip;

			// Only draw the logo when we're flipping, and we're on the default target and the default target is a swap chain.
			if ((flip) && (IsLogoVisible) && (Target.SwapChain != null) && (Target == DefaultTarget))
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
			    if (Target.SwapChain != null)
			    {
			        Target.SwapChain.Flip(interval);
			    }

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
		/// <param name="flip">TRUE to flip the back buffer to the front buffer of the render target.</param>
		/// <remarks>Call this method to draw the renderable objects to the target.  If this method is not called, then nothing will appear on screen.
		/// <para>Gorgon uses a cache of vertex data to queue up what needs to be drawn in order to maintain performance.  However, if this queue gets 
		/// full or the state (i.e. Texture, Blending mode, etc...) changes then this method is called implicitly.</para>
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) since the graphics library had control over the main loop.  Since it does not any more, the 
		/// user is now responsible for calling this method.</para>
		/// </remarks>
		public void Render(bool flip)
		{
			Render(flip, 0);
		}

		/// <summary>
		/// Function to force the renderer to render its data to the current render target.
		/// </summary>
		/// <param name="interval">Presentation interval.</param>
		/// <remarks>Call this method to draw the renderable objects to the target.  If this method is not called, then nothing will appear on screen.
		/// <para>Gorgon uses a cache of vertex data to queue up what needs to be drawn in order to maintain performance.  However, if this queue gets 
		/// full or the state (i.e. Texture, Blending mode, etc...) changes then this method is called implicitly.</para>
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) since the graphics library had control over the main loop.  Since it does not any more, the 
		/// user is now responsible for calling this method.</para>
		/// <para>The <paramref name="interval"/> parameter is the number of vertical retraces to wait.</para>
		/// </remarks>
		public void Render(int interval)
		{
			Render(true, interval);
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
			Render(true, 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2D"/> class.
		/// </summary>
		/// <param name="target">The primary render target to use.</param>		
		internal Gorgon2D(GorgonRenderTarget2D target)
		{						
			TrackedObjects = new GorgonDisposableObjectCollection();
			Graphics = target.Graphics;
			DefaultTarget = target;

			Icons = Graphics.Textures.Create2DTextureFromGDIImage("Gorgon2D.Icons", Properties.Resources.Icons);
			_logoSprite = new GorgonSprite(this, "Gorgon2D.LogoSprite", new GorgonSpriteSettings()
			{
				Anchor = new Vector2(Graphics.Textures.GorgonLogo.Settings.Size),
				Texture = Graphics.Textures.GorgonLogo,
				TextureRegion = new RectangleF(Vector2.Zero, new Vector2(1)),
				Color = Color.White,
				Size = Graphics.Textures.GorgonLogo.Settings.Size
			});
			_defaultCamera = new GorgonOrthoCamera(this, "Gorgon.Camera.Default", new Vector2(target.Settings.Width, target.Settings.Height), 100.0f);			

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
					this.End2D();

					if (Icons != null)
						Icons.Dispose();

                    if (Effects != null)
                    {
                        Effects.FreeShaders();
                        Effects = null;
                    }

					TrackedObjects.ReleaseAll();

				    if ((Target != null) && (Target.SwapChain != null))
				    {
				        Target.SwapChain.Resized -= new EventHandler<GorgonSwapChainResizedEventArgs>(target_Resized);
				    }

				    if (_layout != null)
						_layout.Dispose();

					if (VertexShader != null)
						VertexShader.CleanUp();
					if (PixelShader != null)
						PixelShader.CleanUp();

					VertexShader = null;
					PixelShader = null;

					if (DefaultVertexBufferBinding != null)
						DefaultVertexBufferBinding.VertexBuffer.Dispose();
					if (DefaultIndexBuffer != null)
						DefaultIndexBuffer.Dispose();

					if (ProjectionViewBuffer != null)
						ProjectionViewBuffer.Dispose();
					if (_projectionViewStream != null)
						_projectionViewStream.Dispose();
					if (AlphaTestBuffer != null)
						AlphaTestBuffer.Dispose();
					if (AlphaTestStream != null)
						AlphaTestStream.Dispose();

					if ((SystemCreatedTarget) && (DefaultTarget != null))
					{
						DefaultTarget.Dispose();
						DefaultTarget = null;
					}

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
