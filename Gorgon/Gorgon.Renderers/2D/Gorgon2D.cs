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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Collections.Specialized;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
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
		/// Angle of rotation (in radians).
		/// </summary>
		[InputElement(3, "ANGLE")]
		public float Angle;
	}

	/// <summary>
	/// The renderer for 2D graphics.
	/// </summary>
	/// <remarks>This is the interface that renders 2D graphics such as sprites, lines, circles, etc...  This object is also a factory for various types of 2D renderable objects such as a <see cref="Gorgon.Renderers.GorgonSprite">Sprite</see>.
	/// <para>This renderer also handles state management for the various 2D objects through the exposed properties on the renderer object and automatically through the states from the properties on each object being rendered.</para>
	/// <para>A developer can initialize this object with any render target as the default render target, or one will be created automatically when this object is initialized.  
	/// Note that this does not mean that this interface is limited to one target, the target can be changed at will via the <see cref="P:Gorgon.Renderers.Gorgon2D.Target">Target</see> property.  
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
		private readonly Gorgon2DVertexCache _cache;                                    // Our vertex cache for the renderer.
		private readonly bool _systemCreatedTarget;										// Flag to indicate whether Gorgon created the default target or not.
		private Gorgon2DTarget _currentTarget;											// Current render target.
		private Gorgon2DTarget _defaultTarget;											// Default render target.
		private bool _disposed;															// Flag to indicate that the object was disposed.
		private bool _multiSampleEnable;												// Flag to indicate that multi sampling is enabled.
		private GorgonViewport? _viewPort;												// Viewport to use.
		private Rectangle? _clip;														// Clipping rectangle.
		private I2DCamera _camera;														// Current camera.
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
        /// Property to return the default camera for the 2D renderer.
        /// </summary>
	    public Gorgon2DOrthoCamera DefaultCamera
	    {
	        get;
	    }

		/// <summary>
		/// Property to return our default vertex buffer binding.
		/// </summary>
		public GorgonVertexBufferBinding DefaultVertexBufferBinding => _defaultVertexBuffer;

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
		public GorgonViewport DefaultViewport => _currentTarget.Target == null ? _defaultTarget.Viewport : _currentTarget.Viewport;

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
		}

		/// <summary>
		/// Property to set or return whether to use multisampling.
		/// </summary>
		/// <remarks>This will turn multisampling on or off.
		/// <para>Please note that if using a video device that supports SM4_1 or SM5, this setting cannot be disabled.  SM4_1/5 video devices always enable multisampling 
		/// when the sample count is greater than 1 for a render target.  For SM4 devices, this setting will disable multisampling regardless of sample count.</para>
		/// </remarks>
		public bool IsMultisamplingEnabled
		{
			get
			{
				return _multiSampleEnable;
			}
			set
			{
				if (Graphics.VideoDevice.RequestedFeatureLevel == DeviceFeatureLevel.Sm4)
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
		/// <para>This will not allow for clipping to a rectangle.  Use the <see cref="P:Gorgon.Renderers.Gorgon2D.ClipRegion">ClipRegion</see> property instead.</para>
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
		/// <remarks>The alpha testing tests to see if an alpha value is between or equal to the values in <see cref="P:Gorgon.Renderers.GorgonRenderable.AlphaTestValues">AlphaTestValues</see> and rejects the pixel if it is not.
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
		/// <remarks>Set this value to NULL (<i>Nothing</i> in VB.Net) to use the default camera.</remarks>
		public I2DCamera Camera
		{
			get
			{
				return _camera ?? DefaultCamera;
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
					value = DefaultCamera;
				}

			    // Refresh our camera information if we're jumping back to the default camera.
			    if (value.AutoUpdate)
			    {
				    value.ViewDimensions = new RectangleF(Vector2.Zero, new Vector2(_currentTarget.Width, _currentTarget.Height));
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
		}

		/// <summary>
		/// Property to set or return the active render target view.
		/// </summary>
		/// <remarks>
		/// Changing the current render target will reset the <see cref="P:Gorgon.Renderers.Gorgon2D.Viewport">Viewport</see> 
		/// and the <see cref="P:Gorgon.Renderers.Gorgon2D.ClipRegion">ClipRegion</see>.
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
					Graphics.Output.SetRenderTarget(target.Target, DepthStencil ?? target.DepthStencil);
				}

				_currentTarget = target;

				// If the current render target is the default render target, then copy the current target settings
				// into the default.  This is just in case we've resized the current target and the default is out
				// of sync.
				if (_defaultTarget.Target == _currentTarget.Target)
				{
					_defaultTarget = _currentTarget;
				}
			}

			// Update camera.
			var camera = _camera ?? DefaultCamera;
			
			if (camera.AutoUpdate)
			{
				camera.ViewDimensions = new RectangleF(Vector2.Zero, new Vector2(_currentTarget.Width, _currentTarget.Height));
			}

			var clipRegion = ClipRegion;

			// Restore the clipping region.
			if (clipRegion != null)
			{
				ClipRegion = null;
				ClipRegion = clipRegion;
			}

			Graphics.Rasterizer.SetViewport(_viewPort ?? _currentTarget.Viewport);

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
            // Add shader includes if they're gone.
            if (!Graphics.Shaders.IncludeFiles.Contains("Gorgon2DShaders"))
            {
                Graphics.Shaders.IncludeFiles.Add(new GorgonShaderInclude("Gorgon2DShaders", Encoding.UTF8.GetString(Resources.BasicSprite)));
            }
            
            // Record the initial state before set up.
			if (_initialState == null)
			{
				_initialState = new Gorgon2DStateRecall(this);
			}

			// Reset the cache values.
			_cache.Reset();

			// Set our default shaders.
			VertexShader.Current = VertexShader.DefaultVertexShader;
			PixelShader.Current = PixelShader.DefaultPixelShaderDiffuse;
			Graphics.Input.IndexBuffer = DefaultIndexBuffer;
			Graphics.Input.VertexBuffers[0] = DefaultVertexBufferBinding;
			Graphics.Input.Layout = DefaultLayout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

			IsMultisamplingEnabled = Graphics.Rasterizer.States.IsMultisamplingEnabled;

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
			Graphics.Output.DepthStencilState.StencilReference = 0;
            Graphics.Output.SetRenderTarget(_defaultTarget.Target, _defaultTarget.DepthStencil);

		    _currentTarget = _defaultTarget;

			UpdateTarget(ref _currentTarget);

            DefaultCamera.Update();

			// Get the current state.
			DefaultState = new Gorgon2DStateRecall(this);

			// By default, turn on multi sampling over a count of 1.
			if (Target.Resource.ResourceType != ResourceType.Texture2D)
			{
				return;
			}

			var target2D = (GorgonRenderTarget2D)Target.Resource;

			if ((!IsMultisamplingEnabled)
			    && ((target2D.Settings.Multisampling.Count > 1) || (target2D.Settings.Multisampling.Quality > 0))
			    && ((Graphics.VideoDevice.RequestedFeatureLevel == DeviceFeatureLevel.Sm41)
			        || (Graphics.VideoDevice.RequestedFeatureLevel >= DeviceFeatureLevel.Sm5)))
			{
				_multiSampleEnable = true;
			}
		}

        /// <summary>
        /// Function to render the scene and draw the Gorgon logo at the bottom-right of the screen.
        /// </summary>
        private void RenderWithLogo()
        {
            I2DCamera camera = _camera;
            GorgonViewport? previousViewport = _viewPort;
            Rectangle? previousClip = _clip;

            // Reset any view/projection/clip/viewport.
            // This will force a flush of the pipeline before drawing the logo.
            // If none of these values have changed, the the flush will be 
            // peformed when we draw the logo (due to a texture switch).
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

            _logoSprite.Position = new Vector2(_currentTarget.Width, _currentTarget.Height);
            _logoSprite.Draw();

            // This flush will force the logo to appear.
            Flush();

            // Restore the active camera, viewport and clipping region.
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
		/// Function to initialize the 2D renderer.
		/// </summary>
		private void Initialize()
		{
			// Add shader includes.
			if (!Graphics.Shaders.IncludeFiles.Contains("Gorgon2DShaders"))
			{
				Graphics.ImmediateContext.Shaders.IncludeFiles.Add(new GorgonShaderInclude("Gorgon2DShaders", Encoding.UTF8.GetString(Resources.BasicSprite)));
			}

			// Create shader states.
			PixelShader = new Gorgon2DPixelShaderState(this);

			VertexShader = new Gorgon2DVertexShaderState(this);

			// Create layout information so we can bind our vertices to the shader.
			DefaultLayout = Graphics.ImmediateContext.Input.CreateInputLayout("Gorgon2D Input Layout", typeof(Gorgon2DVertex), VertexShader.DefaultVertexShader);

			// Create pre-defined effects objects.
			Effects = new Gorgon2DEffects(this);

			int spriteVertexBufferSize = Gorgon2DVertex.SizeInBytes * _cache.CacheSize;
			int spriteIndexBufferSize = sizeof(int) * _cache.CacheSize * 6;

			// Set up our index buffer.
			using (IGorgonPointer ibData = new GorgonPointer(spriteIndexBufferSize))
			{
				unsafe
				{
					ushort index = 0;
					var buffer = (int*)ibData.Address;
					for (int i = 0; i < _cache.CacheSize; i++)
					{
						*(buffer++) = index;
						*(buffer++) = (index + 1);
						*(buffer++) = (index + 2);
						*(buffer++) = (index + 1);
						*(buffer++) = (index + 3);
						*(buffer++) = (index + 2);

						index += 4;
					}
				}

				DefaultIndexBuffer = Graphics.ImmediateContext.Buffers.CreateIndexBuffer("Gorgon2D Default Index Buffer", new GorgonIndexBufferSettings
					{
						IsOutput = false,
						SizeInBytes = (int)ibData.Size,
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

			// Set up the default render states.
			SetDefaultStates();
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
				if (_cache.NeedsFlush)
				{
					Flush();
				}

				DefaultState.UpdateState(renderable, stateChange);

				// If we switch vertex buffers, then reset the cache.
				if ((stateChange & StateChange.VertexBuffer) == StateChange.VertexBuffer)
				{
					_cache.Enabled = Graphics.Input.VertexBuffers[0].Equals(ref _defaultVertexBuffer);
				}
			}

            // We skip the cache for objects that have their own vertex buffers.
            if (!_cache.Enabled)
            {
                return;
            }

            _cache.AddVertices(renderable.Vertices, renderable.BaseVertexCount, renderable.IndexCount, 0, renderable.VertexCount);
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
        /// <remarks>This method is used to end 2D rendering and restore the state of the graphics API to the state recorded by the <paramref name="state"/> parameter.  If NULL (<i>Nothing</i> in VB.Net) 
        /// is passed for the <paramref name="state"/> parameter, then the state will be reset to the state of the graphics interface before this renderer was created.
        /// </remarks>
	    public void End2D(Gorgon2DStateRecall state = null)
	    {
            _cache.Reset();

            if (state == null)
            {
                if (_initialState == null)
                {
                    return;
                }

                _initialState.Restore(false);
                _initialState = null;

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
		/// <param name="parameters">Parameters used to initialize the effect.</param>
		/// <returns>The new effect object.</returns>
		/// <remarks>Effects are used to simplify rendering with multiple passes when using a shader.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        public T Create2DEffect<T>(string name, params GorgonEffectParameter[] parameters)
			where T : Gorgon2DEffect
		{
            var effectParameters = new GorgonEffectParameter[parameters?.Length + 1 ?? 1];

            effectParameters[0] = new GorgonEffectParameter("Gorgon2D", this);

		    if (parameters == null)
		    {
		        return Graphics.ImmediateContext.Shaders.CreateEffect<T>(name, effectParameters);
		    }

		    for (int i = 0; i < parameters.Length; i++)
		    {
		        effectParameters[i + 1] = parameters[i];
		    }

		    var result = Graphics.ImmediateContext.Shaders.CreateEffect<T>(name, effectParameters);

            TrackedObjects.Add(result);

		    return result;
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depth">Depth value to clear with.</param>
		/// <param name="stencil">Stencil value to clear with.</param>
		/// <remarks>Unlike a render target <see cref="Gorgon.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
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
		/// <remarks>Unlike a render target <see cref="Gorgon.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color, float depth)
		{
			Clear(color, depth, 0);
		}

		/// <summary>
		/// Function to clear the current target and its depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <remarks>Unlike a render target <see cref="Gorgon.Graphics.GorgonRenderTargetView.Clear">Clear</see> method, this will respect any clipping and/or viewport.  
		/// However, this only affects the color buffer, the depth/stencil will be cleared in their entirety.</remarks>
		public void Clear(GorgonColor color)
		{
			Clear(color, 1.0f, 0);
		}

		/// <summary>
		/// Function to create a new camera object.
		/// </summary>
		/// <typeparam name="T">Type of camera object.  Must implement <see cref="I2DCamera"/>.</typeparam>
		/// <param name="name">Name of the camera.</param>
		/// <param name="viewDimensions">The dimensions for the camera view.</param>
		/// <param name="minDepth">Minimum depth value of the camera.</param>
		/// <param name="maxDepth">Maximum depth value of the camera.</param>
		/// <returns>A new camera.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <remarks>This will create a camera object which allows the user to pan, rotate and scale the entire scene. 
		/// <para>The <paramref name="viewDimensions"/> allows the camera to change the coordinate system used by the various objects in Gorgon.  For example, passing a value of (-1, -1) - (1, 1) 
		/// will change the coordinate system to use -1, -1 as the upper left most corner and 1,1 as the right most corner.  Please note that changing the coordinate system affects the size of 
		/// objects created by Gorgon.  For example, if using the aforementioned coordinate system and you have a sprite sized at 32, 32, that sprite will now be much larger because the view space 
		/// is confined to -1, -1 x 1, 1.  Pass NULL (<i>Nothing</i> in VB.Net) to the <paramref name="viewDimensions"/> parameter to use the current render target dimensions.</para> 
		/// </remarks>
		public T CreateCamera<T>(string name, RectangleF? viewDimensions, float minDepth, float maxDepth)
            where T : I2DCamera
		{
		    if (name == null)
		    {
		        throw new ArgumentNullException(nameof(name));
		    }

		    if (string.IsNullOrWhiteSpace(name))
		    {
		        throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
		    }

			if (viewDimensions == null)
			{
				viewDimensions = new RectangleF(0, 0, _currentTarget.Width, _currentTarget.Height);
			}

		    return (T)Activator.CreateInstance(typeof(T),
		                BindingFlags.Instance | BindingFlags.NonPublic,
		                null,
		                new object[]
		                {
                            this,
                            name,
                            viewDimensions,
							minDepth,
                            maxDepth
		                },
		                null);
		}

        /// <summary>
        /// Function to flush the rendering cache by rendering any outstanding objects.
        /// </summary>
        /// <remarks>
        /// This will flush the rendering cache and persist any graphics data onto the currently active render target.  This method is useful when there is a need to render data but not display it.
        /// <para>Care should be taken when calling this method, as calling it too often in a loop may cause performance issues.  Also note that this method is called internally when there are 
        /// significant state changes and/or when the <see cref="Render"/> method is invoked.</para>
        /// </remarks>
        public void Flush()
        {
            if ((!_cache.Enabled)
                || !(_cache.NeedsFlush))
            {
                return;
            }

            _cache.Flush();
        }

        /// <summary>
		/// Function to force the renderer to render its data to the current render target.
		/// </summary>
		/// <param name="interval">[Optional] The presentation interval used synchronize with the vertical blank of the monitor.</param>
		/// <remarks>Call this method to draw the renderable objects to the target and perform a flip to display the objects (if the current target is a swap chain).  
		/// If this method is not called, then nothing will appear on screen.
		/// <para>In previous versions of Gorgon, this was automatic (on the primary screen) because the graphics library had control over the main loop.  
		/// Since it does not any more, the user is now responsible for calling this method at the end of a frame.</para>
		/// <para>The <paramref name="interval"/> parameter is the number of vertical retraces to wait.  If this value is set to 0, then the frame is immediately displayed.  This value 
		/// is only applicable if the current render target is a swap chain.
		/// </para>
		/// </remarks>
		public void Render(int interval = 0)
		{
			// Only draw the logo when we're flipping, and we're on the default target and the default target is a swap chain.
            if ((IsLogoVisible)
                && (_currentTarget.SwapChain != null)
                && (_currentTarget.Target == _defaultTarget.Target))
            {
                RenderWithLogo();
            }
            else
            {
                Flush();
            }

            if ((_currentTarget.SwapChain != null) && (!Graphics.IsDeferred))
			{
				_currentTarget.SwapChain.Flip(interval);
			}
		}

		/// <summary>
		/// Function to set up multiple rendertargets for MRT output from a shader.
		/// </summary>
		/// <param name="targets">An array of render targets to assign.</param>
		/// <param name="depthStencil">[Optional] A depth/stencil buffer to assign with the targets.</param>
		/// <remarks>
		/// Use this method to set up MRT (Multiple Render Targets) rendering for a shader.  This allows for efficient multiple pass rendering from the shader 
		/// by allowing the shader to output to each render target simultaneously instead of having to set up a new pass for each target output.
		/// <para>
		/// Ensure that all the render targets in the array are the same width/height, and format.  The array count and mip count must also match.  If they do 
		/// not, an exception will be thrown.  The width, height, array count, and mip count for the depth/stencil buffer must match the render targets being set.
		/// </para>
		/// <para>
		/// Setting only a single target in the <paramref name="targets"/> parameter is equivalent to setting the <see cref="Target"/> property.  Also, note that 
		/// setting the target property will unbind the rest of the targets if multiple render targets have been previously bound.
		/// </para>
		/// <para>
		/// The maximum number of render targets that can be bound simultaneously is defined by the <see cref="P:GorgonOutputMerger.MaxRenderTargetViewSlots"/> 
		/// property. If the number of <paramref name="targets"/> passed to this method exceeds this value, then an exception will be thrown.
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="targets"/> parameter is set to NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="targets"/> parameter is empty.</exception>
		public void SetMultipleRenderTargets(GorgonRenderTargetView[] targets, GorgonDepthStencilView depthStencil = null)
		{
#if DEBUG
			if (targets == null)
			{
				throw new ArgumentNullException(nameof(targets));
			}

			if (targets.Length == 0)
			{
				throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, nameof(targets));	
			}
#endif
			// Bind the first target as our primary target so that clipping states and other 
			// settings are updated.
			Target = targets[0];

			if (targets.Length == 1)
			{
				return;
			}

			// Bind the rest of the targets.
			Graphics.Output.SetRenderTargets(targets, depthStencil);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2D"/> class.
		/// </summary>
		/// <param name="target">The primary render target to use.</param>		
		/// <param name="vertexCacheSize">The number of vertices that can be placed in vertex cache.</param>
		/// <param name="autoCreatedTarget"><b>true</b> if Gorgon created the target, <b>false</b> if the user created the target.</param>
		internal Gorgon2D(GorgonRenderTargetView target, int vertexCacheSize, bool autoCreatedTarget)
		{
			_systemCreatedTarget = autoCreatedTarget;

		    _cache = new Gorgon2DVertexCache(this, vertexCacheSize.Max(1024));

			IsBlendingEnabled = true;
			IsAlphaTestEnabled = true;

			TrackedObjects = new GorgonDisposableObjectCollection();
			Graphics = target.Resource.Graphics;
			DefaultTarget = target;

			_logoSprite = new GorgonSprite(this, "Gorgon2D.LogoSprite")
			              {
				              Anchor = new Vector2(Graphics.Textures.GorgonLogo.Settings.Size),
				              Texture = Graphics.Textures.GorgonLogo,
				              TextureRegion = new RectangleF(Vector2.Zero, new Vector2(1)),
				              Color = Color.White,
				              Size = Graphics.Textures.GorgonLogo.Settings.Size
			              };
			DefaultCamera = new Gorgon2DOrthoCamera(this,
				"Gorgon.Camera.Default",
				new RectangleF(0, 0, _defaultTarget.Width, _defaultTarget.Height),
				0,
				1.0f)
			{
				AutoUpdate = true
			};

			Renderables = new GorgonRenderables(this, _cache);
			Drawing = new GorgonDrawing(this);

            // Perform further initialization.
            Initialize();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
                // Dump any pending rendering.
                _cache.Reset();

                if (_initialState != null)
                {
                    _initialState.Restore(true);
                    _initialState = null;
                }

                TrackedObjects.Clear();

                if (Effects != null)
                {
                    Effects.FreeEffects();
                    Effects = null;
                }

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
