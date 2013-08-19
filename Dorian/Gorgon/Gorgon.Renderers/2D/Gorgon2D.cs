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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Native;
using GorgonLibrary.Renderers.Properties;
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
		[Flags]
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
		private readonly bool _systemCreatedTarget;										// Flag to indicate whether Gorgon created the default target or not.
		private Gorgon2DTarget _currentTarget;											// Current render target.
		private Gorgon2DTarget _defaultTarget;											// Default render target.
		private int _baseVertex;														// Base vertex.		
		private Gorgon2DVertex[] _vertexCache;											// List of vertices to cache.
		private int _cacheStart;														// Starting cache vertex buffer index.
		private int _renderIndexStart;													// Starting index to render.
		private int _renderIndexCount;													// Number of indices to render.
		private int _cacheEnd;															// Ending vertex buffer cache index.
		private int _cacheWritten;														// Number of vertices written.
		private bool _useCache = true;													// Flag to indicate that we want to use the cache.
		private bool _disposed;															// Flag to indicate that the object was disposed.
		private readonly int _cacheSize;												// Number of vertices that we can stuff into a vertex buffer.
		private bool _multiSampleEnable;												// Flag to indicate that multi sampling is enabled.
		private GorgonViewport? _viewPort;												// Viewport to use.
		private Rectangle? _clip;														// Clipping rectangle.
		private ICamera _camera;														// Current camera.
		private readonly GorgonOrthoCamera _defaultCamera;								// Default camera.
		private readonly GorgonSprite _logoSprite;										// Logo sprite.
		private GorgonVertexBufferBinding _defaultVertexBuffer;							// Default vertex buffer binding.
		private Gorgon2DStateRecall _initialState;										// The initial state of the graphics API before the renderer was created.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the tracked objects interface.
		/// </summary>
		internal GorgonDisposableObjectCollection TrackedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default layout for our 2D vertex type.
		/// </summary>
		public GorgonInputLayout DefaultLayout
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return our default vertex buffer binding.
		/// </summary>
		public GorgonVertexBufferBinding DefaultVertexBufferBinding
		{
			get
			{
				return _defaultVertexBuffer;
			}
		}

		/// <summary>
		/// Property to return our default index buffer.
		/// </summary>
		public GorgonIndexBuffer DefaultIndexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default state for the 2D renderer.
		/// </summary>
		public Gorgon2DStateRecall DefaultState
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to set or return the currently active depth/stencil view.
        /// </summary>
	    public GorgonDepthStencilView DepthStencil
	    {
	        get
	        {
	            return _currentTarget.DepthStencil;
	        }
	        set
	        {
		        if (_currentTarget.DepthStencil == value)
		        {
			        return;
		        }

		        var target = new Gorgon2DTarget(_currentTarget.Target, value);

				UpdateTarget(ref target);
	        }
	    }

		/// <summary>
		/// Property to return the default view port.
		/// </summary>
		/// <remarks>This returns the default viewport that is bound to the currently active render target.</remarks>
		public GorgonViewport DefaultViewport
		{
			get
			{
				return _currentTarget.Target == null ? _defaultTarget.Viewport : _currentTarget.Viewport;
			}
		}
        
		/// <summary>
		/// Property to return the default render target view.
		/// </summary>
		/// <remarks>This is the inital target view that the Gorgon2D interface was created with, or the one internally generated depending on how the 2D renderer was created.</remarks>
		public GorgonRenderTargetView DefaultTarget
		{
			get
			{
				return _defaultTarget.Target;
			}
			private set
			{
				_defaultTarget = new Gorgon2DTarget(value, null);
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
				if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4) ||
				    (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b))
				{
					_multiSampleEnable = value;
				}
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
			    if (_viewPort == value)
			    {
			        return;
			    }

			    // Force a render when switching viewports.
			    Flush();

			    _viewPort = value;

			    Graphics.Rasterizer.SetViewport(value != null ? _viewPort.Value : _currentTarget.Viewport);
			}
		}

		/// <summary>
		/// Property to set or return the clipping region.
		/// </summary>
		/// <remarks>Use this to clip a rectangular region on the target.  Pixels outside of the region do not get rendered.</remarks>
		public Rectangle? ClipRegion
		{
			get
			{
				return _clip;
			}
			set
			{
			    if (_clip == value)
			    {
			        return;
			    }

			    Flush();

			    _clip = value;

			    if (value != null)
			    {
			        Graphics.Rasterizer.SetScissorRectangle(_clip.Value);
			    }
			    else
			    {
			        Graphics.Rasterizer.SetScissorRectangles(null);
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
				return _camera ?? _defaultCamera;
			}
			set
			{
			    if (_camera == value)
			    {
			        return;
			    }

			    Flush();
			    _camera = value;

				if (value == null)
				{
					value = _defaultCamera;
				}

			    // Refresh our camera information if we're jumping back to the default camera.
			    if (value.AutoUpdate)
			    {
					value.UpdateRegion(Vector2.Zero, new Vector2(_currentTarget.Width, _currentTarget.Height));
			    }

			    // Force an update.
			    value.Update();
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
		/// <remarks>
		/// Changing the current render target will reset the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.Viewport">Viewport</see> 
		/// and the <see cref="P:GorgonLibrary.Renderers.Gorgon2D.ClipRegion">ClipRegion</see>.
		/// </remarks>
		public GorgonRenderTargetView Target
		{
			get
			{
				return _currentTarget.Target;
			}
			set
			{
				if ((_currentTarget.Target == value)
					|| ((value == null) && (Gorgon2DTarget.Equals(ref _currentTarget, ref _defaultTarget))))
				{
					return;
				}

				Gorgon2DTarget newTarget = value == null ? _defaultTarget : new Gorgon2DTarget(value, DepthStencil);

				Flush();
				UpdateTarget(ref newTarget);
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
		/// Function to update the current render target.
		/// </summary>
		/// <param name="target">The new target.</param>
		private void UpdateTarget(ref Gorgon2DTarget target)
		{
            // If we currently have a swap chain bound, then we need to unbind its resize event.
			if (_currentTarget.SwapChain != null)
			{
				_currentTarget.SwapChain.AfterSwapChainResized -= target_Resized;
			}

			if (!Gorgon2DTarget.Equals(ref target, ref _currentTarget))
			{
				if (target.Target != _currentTarget.Target)
				{
					Graphics.Output.SetRenderTarget(target.Target, target.DepthStencil);
				}

				_currentTarget = target;
			}

			// Update camera.
			var camera = _camera ?? _defaultCamera;

			if (camera.AutoUpdate)
			{
				camera.UpdateRegion(Vector2.Zero, new Vector2(_currentTarget.Width, _currentTarget.Height));
			}

			var clipRegion = ClipRegion;

			// Restore the clipping region.
			if (clipRegion != null)
			{
				ClipRegion = null;
				ClipRegion = clipRegion;
			}

			Graphics.Rasterizer.SetViewport(_viewPort == null ? _currentTarget.Viewport : _viewPort.Value);

			if (_currentTarget.SwapChain == null)
			{
				return;
			}

			_currentTarget.SwapChain.AfterSwapChainResized += target_Resized;
		}

		/// <summary>
		/// Function to handle a resize of the current render target.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void target_Resized(object sender, GorgonAfterSwapChainResizedEventArgs e)
		{
			GorgonDepthStencilView depthStencil = _currentTarget.SwapChain.DepthStencilBuffer;

			if (DepthStencil != null)
			{
				depthStencil = DepthStencil;
			}

			var target = new Gorgon2DTarget(_currentTarget.SwapChain, depthStencil);

			UpdateTarget(ref target);
		}

		/// <summary>
		/// Function to set up the renderers initial state.
		/// </summary>
		private void SetDefaultStates()
		{
			// Record the initial state before set up.
			if (_initialState == null)
			{
				_initialState = new Gorgon2DStateRecall(this);
			}

			// Reset the cache values.
			ClearCache();

			// Set our default shaders.
			VertexShader.Current = VertexShader.DefaultVertexShader;
			PixelShader.Current = PixelShader.DefaultPixelShaderDiffuse;
			Graphics.Input.IndexBuffer = DefaultIndexBuffer;
			Graphics.Input.VertexBuffers[0] = DefaultVertexBufferBinding;
			Graphics.Input.Layout = DefaultLayout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

			IsMultisamplingEnabled = Graphics.Rasterizer.States.IsMultisamplingEnabled;

			// Add shader includes if they're gone.
			if (!Graphics.Shaders.IncludeFiles.Contains("Gorgon2DShaders"))
			{
				Graphics.Shaders.IncludeFiles.Add("Gorgon2DShaders", Encoding.UTF8.GetString(Resources.BasicSprite));
			}

			if (PixelShader != null)
			{
				GorgonTextureSamplerStates sampler = GorgonTextureSamplerStates.LinearFilter;
				sampler.TextureFilter = TextureFilter.Point;
				Graphics.Shaders.PixelShader.TextureSamplers[0] = sampler;
				Graphics.Shaders.PixelShader.Resources[0] = null;
			}

			Graphics.Rasterizer.States = GorgonRasterizerStates.CullBackFace;
			Graphics.Output.BlendingState.States = GorgonBlendStates.DefaultStates;
			Graphics.Output.DepthStencilState.States = GorgonDepthStencilStates.NoDepthStencil;
			Graphics.Output.DepthStencilState.DepthStencilReference = 0;

			UpdateTarget(ref _defaultTarget);

			// Get the current state.
			DefaultState = new Gorgon2DStateRecall(this);

			// By default, turn on multi sampling over a count of 2.
			if (Target.Resource.ResourceType != ResourceType.Texture2D)
			{
				return;
			}

			var target2D = (GorgonRenderTarget2D)Target.Resource;

			if ((!IsMultisamplingEnabled)
			    && (target2D.Settings.Multisampling.Count > 1)
			    && ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4_1)
			        || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM5)))
			{
				_multiSampleEnable = true;
			}
		}

		/// <summary>
		/// Function to clear up the vertex cache.
		/// </summary>
		private void ClearCache()
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
			// Add shader includes.
			Graphics.ImmediateContext.Shaders.IncludeFiles.Add("Gorgon2DShaders", Encoding.UTF8.GetString(Resources.BasicSprite));

			// Create shader states.
			PixelShader = new Gorgon2DPixelShaderState(this);

			VertexShader = new Gorgon2DVertexShaderState(this);

			// Create layout information so we can bind our vertices to the shader.
			DefaultLayout = Graphics.ImmediateContext.Input.CreateInputLayout("Gorgon2D Input Layout", typeof(Gorgon2DVertex), VertexShader.DefaultVertexShader);

			// Create pre-defined effects objects.
			Effects = new Gorgon2DEffects(this);

			int spriteVertexBufferSize = Gorgon2DVertex.SizeInBytes * _cacheSize;
			int spriteIndexBufferSize = sizeof(int) * _cacheSize * 6;

			// Set up our index buffer.
			using (var ibData = new GorgonDataStream(spriteIndexBufferSize))
			{
				int index = 0;
				for (int i = 0; i < _cacheSize; i++)
				{
					ibData.Write(index);
					ibData.Write(index + 1);
					ibData.Write(index + 2);
					ibData.Write(index + 1);
					ibData.Write(index + 3);
					ibData.Write(index + 2);
					index += 4;
				}

				ibData.Position = 0;
				DefaultIndexBuffer = Graphics.ImmediateContext.Buffers.CreateIndexBuffer("Gorgon2D Default Index Buffer", new GorgonIndexBufferSettings
					{
						IsOutput = false,
						SizeInBytes = (int)ibData.Length,
						Usage = BufferUsage.Immutable,
						Use32BitIndices = true
					}, ibData);
			}
			
			// Create our empty vertex buffer.
			_defaultVertexBuffer =
				new GorgonVertexBufferBinding(
					Graphics.ImmediateContext.Buffers.CreateVertexBuffer("Gorgon 2D Default Vertex Buffer", new GorgonBufferSettings
						{
							SizeInBytes = spriteVertexBufferSize,
							Usage = BufferUsage.Dynamic
						}), Gorgon2DVertex.SizeInBytes);

			// Create the vertex cache.
			_vertexCache = new Gorgon2DVertex[_cacheSize];

			// Set up the default render states.
			SetDefaultStates();
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
			// Do nothing if there aren't any vertices.
			if (vertices.Length == 0)
			{
				return;
			}

			// If the next set of vertices is going to overflow the buffer, then render the buffer contents.
			if (vertexCount + _cacheEnd > _cacheSize)
			{
				// Ensure that we don't render the same scene twice.
				if (_cacheWritten > 0)
					Flush();

				_baseVertex = 0;
				_cacheStart = 0;
				_cacheEnd = 0;
				_renderIndexStart = 0;
			}

			for (int i = 0; i < vertexCount; i++)
			{
				int cacheIndex = _cacheEnd + i;
				int vertexIndex = i + vertexStart;
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
			// Check for state changes.
			StateChange stateChange = DefaultState.Compare(renderable);

			if (stateChange != StateChange.None)
			{
				if (_cacheWritten > 0)
				{
					Flush();
				}

				DefaultState.UpdateState(renderable, stateChange);

				// If we switch vertex buffers, then reset the cache.
				if ((stateChange & StateChange.VertexBuffer) == StateChange.VertexBuffer)
				{
					ClearCache();

					_useCache = Graphics.Input.VertexBuffers[0].Equals(ref _defaultVertexBuffer);

					// We skip the cache for objects that have their own vertex buffers.
					if (!_useCache)
					{
						return;
					}
				}
			}

			AddVertices(renderable.Vertices, renderable.BaseVertexCount, renderable.IndexCount, 0, renderable.VertexCount);
		}

		/// <summary>
		/// Function to flush the rendering cache by rendering any outstanding objects.
		/// </summary>
		/// <remarks>This method is provided as a means to force the renderer to flush its cache in specific circumstances.  Calling this method manually 
		/// is not recommended and may cause severe performance issues.</remarks>
		public void Flush()
		{
			ICamera currentCamera = (_camera ?? _defaultCamera);
			BufferLockFlags flags = BufferLockFlags.Discard | BufferLockFlags.Write;
			GorgonVertexBufferBinding vbBinding = Graphics.Input.VertexBuffers[0];

			if (_cacheWritten == 0)
			{
				return;
			}

			if (currentCamera.NeedsUpdate)
			{
				currentCamera.Update();
			}

			if (_cacheStart > 0)
			{
				flags = BufferLockFlags.NoOverwrite | BufferLockFlags.Write;
			}

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
					throw new NullReferenceException(string.Format(Resources.GOR2D_SHADER_NOT_BOUND, ShaderType.Pixel));
				}

				if (Graphics.Shaders.VertexShader.Current == null)
				{
					throw new NullReferenceException(string.Format(Resources.GOR2D_SHADER_NOT_BOUND, ShaderType.Vertex));
				}

				if ((Graphics.Input.VertexBuffers[0].Stride == 0) || (Graphics.Input.VertexBuffers[0].VertexBuffer == null))
				{
					throw new NullReferenceException(string.Format(Resources.GOR2D_VERTEX_BUFFER_NOT_BOUND));
				}
#endif

				// Update buffers depending on type.
				switch (vbBinding.VertexBuffer.Settings.Usage)
				{
					case BufferUsage.Dynamic:
						using (GorgonDataStream stream = vbBinding.VertexBuffer.Lock(flags))
						{
							stream.Position = _cacheStart * Gorgon2DVertex.SizeInBytes;
							stream.WriteRange(_vertexCache, _cacheStart, _cacheWritten);
							vbBinding.VertexBuffer.Unlock();
						}
						break;
					case BufferUsage.Default:
						using (var stream = new GorgonDataStream(_vertexCache, _cacheStart, _cacheWritten))
						{
							vbBinding.VertexBuffer.Update(stream, _cacheStart * Gorgon2DVertex.SizeInBytes, (int)stream.Length);
						}
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
        /// Function to prepare the renderer for 2D rendering.
        /// </summary>
        /// <returns>A state recall object that is used to recall the previous state before this method was called.</returns>
        /// <remarks>This method should be used to initialize a scene for 2D rendering.  When this method is called, it will return an object that contains the current state before this method was called. 
        /// Use this state object with the <see cref="End2D"/> method to return the state back to the original settings.
        /// </remarks>
	    public Gorgon2DStateRecall Begin2D()
	    {
			var currentState = new Gorgon2DStateRecall(this);

			SetDefaultStates();

	        return currentState;
	    }

        /// <summary>
        /// Function to end 2D rendering and restore the state of the graphics interface.
        /// </summary>
        /// <param name="state">[Optional] The state object that contains the states returned by the <see cref="Begin2D"/> method.</param>
        /// <remarks>This method is used to end 2D rendering and restore the state of the graphics API to the state recorded by the <paramref name="state"/> parameter.  If NULL (Nothing in VB.Net) 
        /// is passed for the <paramref name="state"/> parameter, then the state will be reset to the state of the graphics interface before this renderer was created.
        /// </remarks>
	    public void End2D(Gorgon2DStateRecall state = null)
	    {
			ClearCache();

            if (state == null)
            {
	            if (_initialState != null)
	            {
		            _initialState.Restore(false);
	            }

	            return;
            }

			// Reset the cache before updating the states.
            state.Restore(false);
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
		/// <remarks>Unlike a render target <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color, float depth, byte stencil)
		{
			if ((_clip == null) && (_viewPort == null))
			{
                if (DepthStencil != null)
                {
                    DepthStencil.ClearDepth(depth);
                    DepthStencil.ClearStencil(stencil);
                }

                Target.Clear(color);

                return;
			}

			if (DepthStencil != null)
			{
				DepthStencil.ClearDepth(depth);
				DepthStencil.ClearStencil(stencil);
			}

			var currentBlend = Drawing.BlendingMode;

			Drawing.BlendingMode = BlendingMode.None;
			Drawing.FilledRectangle(new RectangleF(0, 0, _currentTarget.Width, _currentTarget.Height), color);
			Drawing.BlendingMode = currentBlend;
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depth">Depth value to clear with.</param>
		/// <remarks>Unlike a render target <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color, float depth)
		{
			Clear(color, depth, 0);
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <remarks>Unlike a render target <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
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
			_logoSprite.Position = new Vector2(_currentTarget.Width, _currentTarget.Height);
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
			if ((flip) && (IsLogoVisible) && (_currentTarget.SwapChain != null) && (_currentTarget.Target == _defaultTarget.Target))
			{
				// Reset any view/projection/clip/viewport.
				if (_camera != null)
				{
					Camera = null;
				}

				if (_viewPort != null)
				{
					Viewport = null;
				}

				if (_clip != null)
				{
					ClipRegion = null;
				}

				DrawLogo();
			}

			Flush();

			if (!flip)
			{
				return;
			}

			if ((_currentTarget.SwapChain != null) && (!Graphics.IsDeferred))
			{
				_currentTarget.SwapChain.Flip(interval);
			}

			if (!IsLogoVisible)
			{
				return;
			}

			if (camera != null)
			{
				Camera = camera;
			}

			if (previousViewport != null)
			{
				Viewport = previousViewport;
			}

			if (previousClip != null)
			{
				ClipRegion = previousClip;
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
		/// <param name="vertexCacheSize">The number of vertices that can be placed in vertex cache.</param>
		/// <param name="autoCreatedTarget">TRUE if Gorgon created the target, FALSE if the user created the target.</param>
		internal Gorgon2D(GorgonRenderTargetView target, int vertexCacheSize, bool autoCreatedTarget)
		{
			_systemCreatedTarget = autoCreatedTarget;

			IsBlendingEnabled = true;
			IsAlphaTestEnabled = true;

			TrackedObjects = new GorgonDisposableObjectCollection();
			Graphics = target.Resource.Graphics;
			DefaultTarget = target;
		    _cacheSize = vertexCacheSize.Max(1024);
			
			_logoSprite = new GorgonSprite(this, "Gorgon2D.LogoSprite", new GorgonSpriteSettings
			{
				Anchor = new Vector2(Graphics.Textures.GorgonLogo.Settings.Size),
				Texture = Graphics.Textures.GorgonLogo,
				TextureRegion = new RectangleF(Vector2.Zero, new Vector2(1)),
				Color = Color.White,
				Size = Graphics.Textures.GorgonLogo.Settings.Size
			});
			_defaultCamera = new GorgonOrthoCamera(this, "Gorgon.Camera.Default", new Vector2(_defaultTarget.Width, _defaultTarget.Height), 100.0f)
			{
				AutoUpdate = true,
				Anchor = new Vector2(_defaultTarget.Width * 0.5f, _defaultTarget.Height * 0.5f),
				Position = new Vector2(-_defaultTarget.Width * 0.5f, -_defaultTarget.Height * 0.5f)
			};

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
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_initialState != null)
				{
					_initialState.Restore(true);
					_initialState = null;
				}

				if (Effects != null)
				{
					Effects.FreeShaders();
					Effects = null;
				}

				TrackedObjects.ReleaseAll();

				if (_currentTarget.SwapChain != null)
				{
					_currentTarget.SwapChain.AfterSwapChainResized -= target_Resized;
				}

				if (DefaultLayout != null)
				{
					DefaultLayout.Dispose();
				}

				if (VertexShader != null)
				{
					VertexShader.CleanUp();
				}
				if (PixelShader != null)
				{
					PixelShader.CleanUp();
				}

				VertexShader = null;
				PixelShader = null;

				DefaultVertexBufferBinding.VertexBuffer.Dispose();

				if (DefaultIndexBuffer != null)
				{
					DefaultIndexBuffer.Dispose();
				}

				if ((_systemCreatedTarget) && (_defaultTarget.Target != null))
				{
					_defaultTarget.Target.Resource.Dispose();
					_defaultTarget = default(Gorgon2DTarget);
				}

				Graphics.RemoveTrackedObject(this);
			}
			
			_disposed = true;
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
