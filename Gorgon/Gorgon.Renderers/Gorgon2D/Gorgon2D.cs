#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 6, 2018 12:53:53 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides 2D rendering functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The 2D renderer allows applications to render sprites, text and primitives (lines, rectangles, ellipses, etc...) using a simplified interface.  
    /// </para>
    /// <para>
    /// This is a batching renderer, which means that items that need to be drawn are done as a group of items sharing a common global state during rendering (this includes pixel and vertex shaders). Which 
    /// global states/shaders are applied can be defined by the user via the <see cref="Gorgon2DBatchState"/> object which is passed to the <see cref="Begin"/> method.
    /// </para>
    /// <para>
    /// Because this is a batching renderer, applications must inform the renderer when to start rendering items via the <see cref="Begin"/> method, and when to end rendering using the <see cref="End"/> 
    /// method. 
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// While all drawing must be done between these calls. Changing the current render target, viewport and/or depth/stencil on the <see cref="GorgonGraphics"/> interface while rendering is not allowed 
    /// and will generate an exception if an attempt to change those items is made.  This means that applications must perform target changes, viewport changes, and/or depth/stencil changes prior to 
    /// calling <see cref="Begin"/>, or after <see cref="End"/>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// To render, an application must start the process by calling the <see cref="Begin"/> method, draw the desired items, and then call the <see cref="End"/> method. This render block segregates drawing 
    /// by global states.  So, for example, if the user wishes to change the blending mode, a call to <see cref="Begin"/> with the <see cref="Gorgon2DBatchState"/> set up for the appropriate blending mode 
    /// is made. When finished, the user will call the <see cref="End"/> method. These blocks batch all rendering commands until the <see cref="End"/> method is called, and this allows for high performance 
    /// 2D rendering.
    /// </para>
    /// <para>
    /// Because this renderer uses batching to achieve its performance, it is worth noting that calls to draw items will share the same global state via the <see cref="GorgonBlendState"/>,
    /// <see cref="GorgonDepthStencilState"/> and <see cref="GorgonRasterState"/> state objects. This includes pixel shaders and vertex shaders, and their associated resources.  And users can send custom
    /// states and shaders to the <see cref="Begin"/> method. However, when a new item is drawn with a different <see cref="GorgonTexture2DView"/>, or <see cref="GorgonSamplerState"/>, a state change will 
    /// be performaned on behalf of the user (for sake of convenience). This means that too many texture/sampler changes between sprites may cause a performance issue.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="Gorgon2DBatchState"/>
    public sealed class Gorgon2D
        : IGorgon2DFluent, IGorgon2DDrawingFluent
    {
        #region Constants.
        /// <summary>
        /// The renderer is not initialized.
        /// </summary>
        private const int Uninitialized = 0;

        /// <summary>
        /// The renderer is initializing.
        /// </summary>
        private const int Initializing = 1;

        /// <summary>
        /// The renderer is initialized.
        /// </summary>
        private const int Initialized = 2;
        #endregion

        #region Variables.
        // The flag to indicate that the renderer is initialized.
        private int _initialized = Uninitialized;
        // World matrix vertex shader.
        private GorgonVertexShader _polyTransformVertexShader;
        private readonly Gorgon2DShaderState<GorgonVertexShader> _polyTransformVertexState = new Gorgon2DShaderState<GorgonVertexShader>();
        // The default pixel shader used by the renderer.
        private GorgonPixelShader _polyPixelShader;
        private readonly Gorgon2DShaderState<GorgonPixelShader> _polyPixelState = new Gorgon2DShaderState<GorgonPixelShader>();
        // The default vertex shader used by the renderer.
        private GorgonVertexShader _defaultVertexShader;
        private readonly Gorgon2DShaderState<GorgonVertexShader> _defaultVertexState = new Gorgon2DShaderState<GorgonVertexShader>();
        // The default pixel shader used by the renderer.
        private GorgonPixelShader _defaultPixelShader;
        private readonly Gorgon2DShaderState<GorgonPixelShader> _defaultPixelState = new Gorgon2DShaderState<GorgonPixelShader>();
        // The layout used to define a vertex to the vertex shader.
        private GorgonInputLayout _vertexLayout;
        // The renderer used to draw batched renderable items.
        private BatchRenderer _batchRenderer;
        // The default texture to render.
        private GorgonTexture2DView _defaultTexture;
        // The default texture to render.
        private GorgonTexture2DView _blackTexture;
        // The buffer that holds the view and projection matrices.
        private CameraController _cameraController;
        // The buffer used to perform alpha testing.
        private GorgonConstantBufferView _alphaTest;
        // A factory used to create draw calls.
        private DrawCallFactory _drawCallFactory;
        // The currently active draw index call.
        private GorgonDrawIndexCall _currentDrawIndexCall;
        // The currently active draw call (no indexing).
        private GorgonDrawCall _currentDrawCall;
        // The previously assigned batch state.
        private readonly Gorgon2DBatchState _lastBatchState = new Gorgon2DBatchState();
        // The last sprite that was put into the system.
        private BatchRenderable _lastRenderable;
        // The current alpha test data.
        private AlphaTestData _alphaTestData;
        // Flag to indicate that the begin method has been called.
        private int _beginCalled;
        // A buffer used for text manipulation in the DrawText method.
        private readonly StringBuilder _textBuffer = new StringBuilder(256);
        // The default font.
        private Lazy<GorgonFontFactory> _defaultFontFactory;
        // The default text sprite for rendering strings.
        private GorgonTextSprite _defaultTextSprite;
        // The renderable for primitives (lines, rectangles, etc...)
        private readonly BatchRenderable _primitiveRenderable = new BatchRenderable
        {
            Vertices = new Gorgon2DVertex[4]
        };
        // The 2D camera used to render the data.
        private Gorgon2DOrthoCamera _defaultCamera;
        // The world matrix buffer for objects that use a world matrix.
        private GorgonConstantBufferView _polySpriteDataBuffer;
        // The transformer used for polygons.
        private readonly PolySpriteTransformer _polyTransformer = new PolySpriteTransformer();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return a black texture to pass to shaders when no texture is specified.
        /// </summary>
        internal GorgonTexture2DView EmptyBlackTexture => _blackTexture;

        /// <summary>
        /// Property to return the default font used for text rendering if the user did not specify a font with text drawing routines.
        /// </summary>
        public GorgonFont DefaultFont => _defaultFontFactory.Value.DefaultFont;

        /// <summary>
        /// Property to return the log used to log debug messages.
        /// </summary>
        public IGorgonLog Log => Graphics.Log;

        /// <summary>
        /// Property to return the currently active camera.
        /// </summary>
        public IGorgon2DCamera CurrentCamera
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the alpha testing range for primitive functions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value with a <see cref="GorgonRangeF"/> will exclude any alpha values within the range when rendering, this will improve performance. If this value is <b>null</b>, then alpha 
        /// testing is disabled and all pixel values will be rendered.
        /// </para>
        /// <para>
        /// Currently, the default is set to a minimum of 0 and a maximum of 0. This means that alpha values with a value of 0 will not be rendered.
        /// </para>
        /// <para>
        /// This applies to methods like <see cref="DrawFilledRectangle(DX.RectangleF, GorgonColor, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>, 
        /// <see cref="DrawRectangle(DX.RectangleF, GorgonColor, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>, etc... 
        /// <see cref="DrawSprite(GorgonSprite)"/> and <see cref="DrawTextSprite(GorgonTextSprite)"/> have their own alpha test ranges and are not affected by this property.
        /// </para>
        /// </remarks>
        /// <seealso cref="DrawLine(float, float, float, float, GorgonColor, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float, float)"/>
        /// <seealso cref="DrawRectangle(DX.RectangleF, GorgonColor, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        /// <seealso cref="DrawEllipse(DX.RectangleF, GorgonColor, float, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        /// <seealso cref="DrawArc(DX.RectangleF, GorgonColor, float, float, float, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>        
        /// <seealso cref="DrawTriangle(in GorgonTriangleVertex, in GorgonTriangleVertex, in GorgonTriangleVertex, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        /// <seealso cref="DrawFilledRectangle(DX.RectangleF, GorgonColor, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        /// <seealso cref="DrawFilledEllipse(DX.RectangleF, GorgonColor, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        /// <seealso cref="DrawFilledArc(DX.RectangleF, GorgonColor, float, float, float, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
        public GorgonRangeF? PrimitiveAlphaTestRange
        {
            get;
            set;
        } = new GorgonRangeF(0, 0);

        /// <summary>
        /// Property to return whether the renderer is currently rendering.
        /// </summary>
        /// <remarks>
        /// This value will return <b>true</b> if <see cref="Begin"/> was previously called, and <b>false</b> when <see cref="End"/>.
        /// </remarks>
        /// <seealso cref="Begin"/>
        /// <seealso cref="End"/>
        public bool IsRendering => _beginCalled != 0;

        /// <summary>
        /// Property to return the <see cref="GorgonGraphics"/> interface that owns this renderer.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the alpha testing values and render the active draw call.
        /// </summary>
        private void UpdateAlphaTestAndRender()
        {
            if ((_currentDrawCall == null) && (_currentDrawIndexCall == null))
            {
                return;
            }

            if (_lastRenderable != null)
            {
                UpdateAlphaTest(ref _lastRenderable.AlphaTestData);
            }

            if (_currentDrawIndexCall != null)
            {
                _batchRenderer.RenderBatches(_currentDrawIndexCall);
            }
            else if (_currentDrawCall != null)
            {
                _batchRenderer.RenderBatches(_currentDrawCall);
            }
        }

        /// <summary>
        /// Function to check for changes in the batch state with the current poly sprite, and render the previous batch if necessary.
        /// </summary>
        /// <param name="renderable"></param>
        private void RenderBatchOnChange(PolySpriteRenderable renderable)
        {
            // If the vertex buffer is different than the previous buffer
            if ((_currentDrawIndexCall != null)
                && (_currentDrawIndexCall.VertexBufferBindings[0].Equals(_batchRenderer.VertexBuffer))
                && (_currentDrawIndexCall.IndexBuffer == _batchRenderer.IndexBuffer))
            {
                UpdateAlphaTestAndRender();
                _lastRenderable = renderable;
                _lastRenderable.StateChanged = false;
                return;
            }

            RenderBatchOnChange(renderable, true, false);
        }

        /// <summary>
        /// Function to check for changes in the batch state, and render the previous batch if necessary.
        /// </summary>
        /// <param name="renderable">The renderable object that needs to be evaluated.</param>
        /// <param name="useIndices"><b>true</b> if the renderable requires indices, or <b>false</b> if not.</param>
        /// <param name="createDrawCall"><b>true</b> if a new draw call should be created, <b>false</b> if it should not.</param>
        /// <param name="flush"><b>true</b> to force a flush of the batch, or <b>false</b> to try to detect a batch flush.</param>
        private void RenderBatchOnChange(BatchRenderable renderable, bool useIndices, bool createDrawCall = true, bool flush = false)
        {
            // Check for alpha test, sampler[0], and texture[0] changes.  We only need a new draw call when those states change.
            if ((!flush)
                && (_lastRenderable != null)
                && (_batchRenderer.RenderableStateComparer.Equals(_lastRenderable, renderable))
                && (((useIndices) && (_currentDrawIndexCall != null))
                     || ((!useIndices) && (_currentDrawCall != null))))
            {
                return;
            }

            // Flush any pending draw calls.
            UpdateAlphaTestAndRender();

            if (createDrawCall)
            {
                if (useIndices)
                {
                    _currentDrawCall = null;
                    _currentDrawIndexCall = _drawCallFactory.GetDrawIndexCall(renderable, _lastBatchState, _batchRenderer);
                }
                else
                {
                    _currentDrawIndexCall = null;
                    _currentDrawCall = _drawCallFactory.GetDrawCall(renderable, _lastBatchState, _batchRenderer);
                }
            }
            else
            {
                _currentDrawCall = null;
                _currentDrawIndexCall = null;
            }

            _lastRenderable = renderable;
            // All states are reconciled, so reset the change flag.
            _lastRenderable.StateChanged = false;
        }

        /// <summary>
        /// Function called when a render target is changed on the main graphics interface.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void RenderTarget_Changed(object sender, EventArgs e)
        {
            // If we've not been initialized yet, do so now.
            if (_initialized != Initialized)
            {
                Initialize();
            }

            IGorgon2DCamera camera = CurrentCamera ?? _defaultCamera;
            _cameraController.UpdateCamera(camera);
        }

        /// <summary>
        /// Function called when a render target, depth/stencil or view port is changed on the primary graphics object.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void ValidateBeginEndCall(object sender, CancelEventArgs e)
        {
            // If we've already begun a Begin block, then we cannot allow it to continue or else the state will be incorrect when rendering.
            // To keep the code consistent, and to provide as little of a surprise as possible, we've opted to throw an error here instead 
            // of forcing a flush and restart of the pipeline.  This will force the developer to write their code in a more consistent manner.
            if (_beginCalled == 0)
            {
                return;
            }

            throw new InvalidOperationException(Resources.GOR2D_ERR_CANNOT_CHANGE_STATE_INSIDE_BEGIN);
        }

        /// <summary>
        /// Function to check the primitive (line, ellipse, etc...) renderable for state changes.
        /// </summary>
        /// <param name="texture">The texture to compare for changes.</param>
        /// <param name="textureSampler">The texture sampler to compare for changes.</param>
        private void CheckPrimitiveStateChange(GorgonTexture2DView texture, GorgonSamplerState textureSampler)
        {
            var alphaTestData = new AlphaTestData(false, GorgonRangeF.Empty);

            // The state has already been marked as changed, so we don't need to test further.
            if (_primitiveRenderable.StateChanged)
            {
                return;
            }

            _primitiveRenderable.StateChanged = (texture != _primitiveRenderable.Texture)
                                                || (textureSampler != _primitiveRenderable.TextureSampler)
                                                || (!AlphaTestData.Equals(in alphaTestData, in _primitiveRenderable.AlphaTestData));
        }

        /// <summary>
        /// Function to update the alpha test data.
        /// </summary>
        /// <param name="currentData">The data to write into the buffer.</param>
        private void UpdateAlphaTest(ref AlphaTestData currentData)
        {
            if (currentData.Equals(_alphaTestData))
            {
                return;
            }

            _alphaTest.Buffer.SetData(ref currentData);
            _alphaTestData = currentData;
        }

        /// <summary>
        /// Function to initialize the renderer.
        /// </summary>
        private void Initialize()
        {
            // Spin wait until we're fully initialized.
            var wait = new SpinWait();

            while (Interlocked.Exchange(ref _initialized, Initializing) == Initializing)
            {
                // If a thread is currently initialzing the renderer, make it wait until we've finalized initialization before continuing.
                wait.SpinOnce();
            }

            // We're already initialized, then we don't need to continue any further.
            if (_initialized == Initialized)
            {
                return;
            }

            try
            {
                _defaultVertexState.Shader = _defaultVertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.BasicSprite, "GorgonVertexShader", GorgonGraphics.IsDebugEnabled);
                _defaultPixelState.Shader = _defaultPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.BasicSprite, "GorgonPixelShaderTextured", GorgonGraphics.IsDebugEnabled);
                _polyTransformVertexState.Shader = _polyTransformVertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.BasicSprite, "GorgonVertexShaderPoly", GorgonGraphics.IsDebugEnabled);
                _polyPixelState.Shader = _polyPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.BasicSprite, "GorgonPixelShaderPoly", GorgonGraphics.IsDebugEnabled);

                _vertexLayout = GorgonInputLayout.CreateUsingType<Gorgon2DVertex>(Graphics, _defaultVertexShader);

                // We need to ensure that we have a default texture in case we decide not to send a texture in.
                GorgonTexture2D textureResource = Resources.White_2x2.ToTexture2D(Graphics,
                                                                                  new GorgonTexture2DLoadOptions
                                                                                  {
                                                                                      Name = "Default White 2x2 Texture",
                                                                                      Binding = TextureBinding.ShaderResource,
                                                                                      Usage = ResourceUsage.Immutable
                                                                                  });
                _defaultTexture = textureResource.GetShaderResourceView();

                // Set up an empty texture to use in place of NULL textures in a shader.
                textureResource = Resources.Black_2x2.ToTexture2D(Graphics,
                                                                                  new GorgonTexture2DLoadOptions
                                                                                  {
                                                                                      Name = "Empty black 2x2 Texture",
                                                                                      Binding = TextureBinding.ShaderResource,
                                                                                      Usage = ResourceUsage.Immutable
                                                                                  });

                _blackTexture = textureResource.GetShaderResourceView();

                _alphaTestData = new AlphaTestData(true, GorgonRangeF.Empty);
                _alphaTest = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _alphaTestData, "Alpha Test Buffer");

                _batchRenderer = new BatchRenderer(Graphics);
                _drawCallFactory = new DrawCallFactory(Graphics, _defaultTexture, _vertexLayout)
                {
                    AlphaTestBuffer = _alphaTest
                };

                // Set up the initial state.
                _lastBatchState.PixelShaderState = _defaultPixelState;
                _lastBatchState.VertexShaderState = _defaultVertexState;
                _lastBatchState.BlendState = GorgonBlendState.Default;
                _lastBatchState.RasterState = GorgonRasterState.Default;
                _lastBatchState.DepthStencilState = GorgonDepthStencilState.Default;

                _defaultCamera = new Gorgon2DOrthoCamera(this,
                                                         new DX.Size2F(Graphics.Viewports[0].Width, Graphics.Viewports[0].Height),
                                                         0,
                                                         1.0f,
                                                         "Gorgon2D.Default_Camera");
                _cameraController = new CameraController(Graphics);
                _cameraController.UpdateCamera(_defaultCamera);

                var polyData = new PolyVertexShaderData
                {
                    World = DX.Matrix.Identity,
                    Color = GorgonColor.White,
                    TextureTransform = new DX.Vector4(0, 0, 1, 1),
                    MiscInfo = new DX.Vector4(0, 0, 1, 0)
                };

                _polySpriteDataBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                                   ref polyData,
                                                                                   "[Gorgon2D] Polygon Sprite Data Buffer",
                                                                                   ResourceUsage.Dynamic);

                _defaultTextSprite = new GorgonTextSprite(_defaultFontFactory.Value.DefaultFont);

                _polyTransformVertexState.RwConstantBuffers[0] = _cameraController.CameraBuffer;
                _polyTransformVertexState.RwConstantBuffers[1] = _polySpriteDataBuffer;
                _polyPixelState.RwConstantBuffers[0] = _alphaTest;

                Graphics.RenderTargetChanging += ValidateBeginEndCall;
                Graphics.ViewportChanging += ValidateBeginEndCall;
                Graphics.DepthStencilChanging += ValidateBeginEndCall;
                Graphics.RenderTargetChanged += RenderTarget_Changed;
                Graphics.ViewportChanged += RenderTarget_Changed;

                Interlocked.Exchange(ref _initialized, 2);
            }
            catch (Exception ex)
            {
                // Get rid of any objects that we've already built.
                Dispose();

                Log.LogException(ex);
                throw;
            }
        }


        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="renderable">The renderable to interrogate.</param>
        /// <returns>The bounds with transformation applied.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        private DX.RectangleF GetTransformedBounds(BatchRenderable renderable)
        {
            float left = float.MaxValue;
            float top = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MinValue;

            for (int i = 0; i < renderable.ActualVertexCount; ++i)
            {
                ref readonly Gorgon2DVertex vertex = ref renderable.Vertices[i];

                left = vertex.Position.X.Min(left);
                top = vertex.Position.Y.Min(top);
                right = vertex.Position.X.Max(right);
                bottom = vertex.Position.Y.Max(bottom);
            }

            return new DX.RectangleF
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };
        }

        /// <summary>
        /// Function to begin rendering a batch.
        /// </summary>
        /// <param name="batchState">[Optional] Defines common state to use when rendering a batch of objects.</param>
        /// <param name="camera">[Optional] A camera to use when rendering.</param>
        /// <exception cref="GorgonException">Thrown if <see cref="Begin"/> is called more than once without calling <see cref=" End"/>.</exception>
        /// <remarks>
        /// <para>
        /// The 2D renderer uses batching for performance. This means that drawing items with common states (e.g. blending) can all be sent to the GPU at the same time. To faciliate this, applications
        /// must call this method prior to drawing.
        /// </para>
        /// <para>
        /// When batching occurs, all drawing that shares the same state and texture will be drawn in one deferred draw call to the GPU. However, if too many items are drawn (~10,000 sprites), or the item 
        /// being drawn has a different texture than the previous item, then the batch is broken up and the previous items  will be drawn to the GPU first.  So, best practice is to ensure that everything
        /// that is drawn shares the same texture.  This is typically achieved by using a sprite sheet where multiple sprite images are pack into a single texture.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// One exception to this is the <see cref="GorgonPolySprite"/> object, which is drawn immediately.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// Once rendering is done, the user must call <see cref="End"/> to finalize the rendering. Otherwise, items drawn in the batch will not appear.
        /// </para>
        /// <para>
        /// This method takes an optional <see cref="Gorgon2DBatchState"/> object which allows an application to override the blend state, depth/stencil state (if applicable), rasterization state, and
        /// pixel/vertex shaders and their associated resources. This means that if an application wants to, for example, change blending modes, then a separate call to this method is required after
        /// drawing items with the previous blend state.  
        /// </para>
        /// <para>
        /// The other optional parameter, <paramref name="camera"/>, allows an application to change the view in which the items are drawn for a batch. This takes a <see cref="IGorgon2DCamera"/> object
        /// that defines the projection and view of the scene being rendered. It is possible with this object to change the coordinate system, and to allow perspective rendering for a batch.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// There are a few things to be aware of when rendering:
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>Batches <b>cannot</b> be nested.  Attempting to do so will cause an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Applications <b>must</b> call this method prior to drawing anything. Failure to do so will result in an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Calls to <see cref="GorgonGraphics.SetRenderTarget"/>, <see cref="GorgonGraphics.SetRenderTargets"/>, <see cref="GorgonGraphics.SetDepthStencil"/>,
        /// <see cref="GorgonGraphics.SetViewport"/>, or <see cref="GorgonGraphics.SetViewports"/> while a batch is in progress is not allowed and will result in an exception if attempted.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="Gorgon2DBatchState"/>
        /// <seealso cref="IGorgon2DCamera"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonGraphics"/>
        public void Begin(Gorgon2DBatchState batchState = null, IGorgon2DCamera camera = null)
        {
            if (Interlocked.Exchange(ref _beginCalled, 1) == 1)
            {
                throw new GorgonException(GorgonResult.AlreadyInitialized, Resources.GOR2D_ERR_RENDER_ALREADY_STARTED);
            }

            // If we're not initialized, then do so now.
            if (_initialized != Initialized)
            {
                Initialize();
            }

            _cameraController.UpdateCamera(camera ?? _defaultCamera);
            CurrentCamera = camera;

            _lastRenderable = null;
            _lastBatchState.PixelShaderState = batchState?.PixelShaderState ?? _defaultPixelState;
            _lastBatchState.VertexShaderState = batchState?.VertexShaderState ?? _defaultVertexState;
            _lastBatchState.BlendState = batchState?.BlendState ?? GorgonBlendState.Default;
            _lastBatchState.RasterState = batchState?.RasterState ?? GorgonRasterState.Default;
            _lastBatchState.DepthStencilState = batchState?.DepthStencilState ?? GorgonDepthStencilState.Default;

            // If we didn't assign shaders, then use our defaults.
            if (_lastBatchState.PixelShaderState.Shader == null)
            {
                _lastBatchState.PixelShaderState.Shader = _defaultPixelShader;
            }

            if (_lastBatchState.PixelShaderState.RwConstantBuffers[0] == null)
            {
                _lastBatchState.PixelShaderState.RwConstantBuffers[0] = _alphaTest;
            }

            if (_lastBatchState.VertexShaderState.Shader == null)
            {
                _lastBatchState.VertexShaderState.Shader = _defaultVertexShader;
            }

            if (_lastBatchState.VertexShaderState.RwConstantBuffers[0] == null)
            {
                _lastBatchState.VertexShaderState.RwConstantBuffers[0] = _cameraController.CameraBuffer;
            }

            if (_lastBatchState.VertexShaderState.RwConstantBuffers[1] == null)
            {
                _lastBatchState.VertexShaderState.RwConstantBuffers[1] = _polySpriteDataBuffer;
            }
        }

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This finalizes rendering and flushes the current batch data to the GPU. Effectively, this is the method that performs the actual rendering for anything the user has drawn.
        /// </para>
        /// <para>
        /// The 2D renderer uses batching to achieve its performance. Because of this, we define a batch with a call to <see cref="Begin"/> and <c>End</c>. So, for optimal performance, it is best to draw
        /// as much drawing as possible within the Begin/End batch body.
        /// </para>
        /// <para>
        /// This method must be paired with a call to <see cref="Begin"/>, if it is not, it will do nothing. If this method is not called after a call to <see cref="Begin"/>, then nothing (in most cases) 
        /// will be drawn. If a previous call to <see cref="Begin"/> is made, and this method is not called, and another call to <see cref="Begin"/> is made, an exception is thrown.
        /// </para>
        /// </remarks>
        public void End()
        {
            if (Interlocked.Exchange(ref _beginCalled, 0) == 0)
            {
                return;
            }

            UpdateAlphaTestAndRender();

            _currentDrawCall = null;
            _currentDrawIndexCall = null;

            // Reset the last batch state so we can enter again with a clean setup.
            _lastBatchState.RasterState = null;
            _lastBatchState.BlendState = null;
            _lastBatchState.DepthStencilState = null;
            _lastBatchState.PixelShaderState = null;
            _lastBatchState.VertexShaderState = null;
        }

        /// <summary>
        /// Function to draw a polygonal sprite.
        /// </summary>
        /// <param name="sprite">The polygon sprite to draw.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method draws a sprite using a polygon as its surface. This is different from other sprite rendering in that:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The surface is not rectangular.</description>
        ///     </item>
        ///     <item>
        ///         <description>It is not batched with other drawing types, and is drawn immediately.  This may be a performance hit.</description>
        ///     </item>
        ///     <item>
        ///         <description>Unlike a <see cref="GorgonSprite"/>, which uses a rectangle, this type of sprite produces an irregular shape that can be used to match the texture being drawn.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// The method takes a <see cref="GorgonPolySprite"/> object which contains <see cref="GorgonPolySpriteVertex"/> objects to define the outer shape (hull) of the polygon. Gorgon will triangulate
        /// the hull into a mesh that can be rendered. 
        /// </para>
        /// <para>
        /// <see cref="GorgonPolySprite"/> objects cannot be created directly, but can be built using the <see cref="GorgonPolySpriteBuilder"/> object.  Please note that these objects implement
        /// <see cref="IDisposable"/>, so users should call the <c>Dispose</c> method when they are done with the objects.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonPolySpriteBuilder"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonPolySpriteVertex"/>
        /// <seealso cref="GorgonSprite"/>
        public void DrawPolygonSprite(GorgonPolySprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));

#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            PolySpriteRenderable renderable = sprite.Renderable;

            RenderBatchOnChange(renderable);

            UpdateAlphaTest(ref renderable.AlphaTestData);

            Gorgon2DShaderState<GorgonVertexShader> prevVShader = _lastBatchState.VertexShaderState;
            Gorgon2DShaderState<GorgonPixelShader> prevPShader = _lastBatchState.PixelShaderState;

            // This type is drawn immediately since it uses its own vertex/index buffer.  

            // Remember our previous vertex shader (assuming we haven't overridden it elsewhere).
            if ((_lastBatchState.VertexShaderState == null) || (_lastBatchState.VertexShaderState == _defaultVertexState))
            {
                _lastBatchState.VertexShaderState = _polyTransformVertexState;
            }

            if ((_lastBatchState.PixelShaderState == null) || (_lastBatchState.PixelShaderState == _defaultPixelState))
            {
                _lastBatchState.PixelShaderState = _polyPixelState;
            }

            _polyTransformer.Transform(renderable);
            var polyData = new PolyVertexShaderData
            {
                World = renderable.WorldMatrix,
                Color = renderable.LowerLeftColor,
                TextureTransform = renderable.TextureTransform,
                MiscInfo = new DX.Vector4(renderable.HorizontalFlip ? 1 : 0, renderable.VerticalFlip ? 1 : 0, renderable.AngleCos, renderable.AngleSin),
                TextureArrayIndex = renderable.TextureArrayIndex
            };

            _polySpriteDataBuffer.Buffer.SetData(ref polyData);

            if ((_currentDrawIndexCall == null)
                || (!_currentDrawIndexCall.VertexBufferBindings[0].Equals(in renderable.VertexBuffer))
                || (_currentDrawIndexCall.IndexBuffer != renderable.IndexBuffer))
            {
                _currentDrawIndexCall =
                    _drawCallFactory.GetDrawIndexCall(renderable, _lastBatchState, renderable.IndexBuffer, renderable.VertexBuffer, _vertexLayout);
                _currentDrawIndexCall.IndexStart = 0;
                _currentDrawIndexCall.IndexCount = renderable.IndexCount;
            }

            Graphics.Submit(_currentDrawIndexCall);

            if (prevVShader != null)
            {
                // Restore the shader.
                _lastBatchState.VertexShaderState = prevVShader;
            }

            if (prevPShader == null)
            {
                return;
            }

            _lastBatchState.PixelShaderState = prevPShader;
        }

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <returns>The bounds with transformation applied.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        public DX.RectangleF MeasureSprite(GorgonSprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));

            if (_initialized != Initialized)
            {
                Initialize();
            }

            if (sprite.IsUpdated)
            {
                _batchRenderer.SpriteTransformer.Transform(sprite.Renderable);
            }

            return GetTransformedBounds(sprite.Renderable);
        }

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <returns>The bounds with transformation applied.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        public DX.RectangleF MeasureSprite(GorgonTextSprite sprite)
        {
            // The number of characters evaluated.
            int charCount = 0;
            // The index into the vertex array for the sprite.
            int vertexOffset = 0;
            // The position of the current glyph.
            DX.Vector2 position = DX.Vector2.Zero;

            sprite.ValidateObject(nameof(sprite));

            if (_initialized != Initialized)
            {
                Initialize();
            }

            if ((sprite.Renderable.HasTransformChanges) || (sprite.Renderable.HasVertexChanges) || (sprite.Renderable.VertexCountChanged))
            {
                _textBuffer.Length = 0;
                int textLength = sprite.Text.Length;

                // If there's no text, then there's nothing to render.
                if (textLength == 0)
                {
                    return DX.RectangleF.Empty;
                }

                TextRenderable renderable = sprite.Renderable;

                _textBuffer.Append(sprite.Text);

                Alignment alignment = renderable.Alignment;
                GorgonFont font = renderable.Font;
                bool drawOutlines = ((renderable.DrawMode != TextDrawMode.GlyphsOnly) && (font.HasOutline));
                float fontHeight = font.FontHeight.FastFloor();
                bool hasKerning = (font.Info.UseKerningPairs) && (font.KerningPairs.Count > 0);
                IDictionary<GorgonKerningPair, int> kerningValues = font.KerningPairs;
                float lineSpaceMultiplier = renderable.LineSpaceMultiplier;

                renderable.IndexCount = 0;
                renderable.ActualVertexCount = 0;

                for (int line = 0; line < renderable.Lines.Length; ++line)
                {
                    string textLine = sprite.Lines[line];
                    textLength = textLine.Length;

                    DX.Size2F lineMeasure = DX.Size2F.Empty;

                    if (alignment != Alignment.UpperLeft)
                    {
                        lineMeasure = font.MeasureLine(textLine, drawOutlines, lineSpaceMultiplier);
                    }

                    position.X = 0;

                    for (int i = 0; i < textLength; ++i)
                    {
                        char character = textLine[i];
                        int kernAmount = 0;

                        if (!font.Glyphs.TryGetValue(character, out GorgonGlyph glyph))
                        {
                            if (!font.TryGetDefaultGlyph(out glyph))
                            {
                                // Only update when we're in non-outline.
                                ++charCount;
                                continue;
                            }
                        }

                        // Handle whitespace by just advancing our position, we don't need geometry for this.
                        if ((char.IsWhiteSpace(character))
                            || (glyph.TextureView == null))
                        {
                            if (character == '\t')
                            {
                                position.X += glyph.Advance * renderable.TabSpaceCount;
                            }
                            // We don't use carriage returns.
                            else if (character != '\r')
                            {
                                position.X += glyph.Advance;
                            }

                            // Only update when we're in non-outline.
                            ++charCount;
                            continue;
                        }

                        if ((hasKerning) && (i < textLength - 1))
                        {
                            var kernPair = new GorgonKerningPair(character, textLine[i + 1]);
                            kerningValues.TryGetValue(kernPair, out kernAmount);
                        }


                        _batchRenderer.TextSpriteTransformer.Transform(renderable,
                                                                        glyph,
                                                                        null,
                                                                        ref position,
                                                                        vertexOffset,
                                                                        drawOutlines,
                                                                        lineMeasure.Width);

                        vertexOffset += 4;
                        position.X += glyph.Advance + kernAmount;

                        // Only update when we're in non-outline.
                        ++charCount;

                        renderable.IndexCount += 6;
                        renderable.ActualVertexCount += 4;
                    }

                    // This is to account for the new line character.
                    ++charCount;
                    position.Y += fontHeight * lineSpaceMultiplier;
                }

                if (renderable.IndexCount == 0)
                {
                    return DX.RectangleF.Empty;
                }

                renderable.VertexCountChanged = false;
                renderable.HasTransformChanges = false;
                renderable.HasVertexChanges = false;
            }

            return GetTransformedBounds(sprite.Renderable);
        }

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <returns>The bounds with transformation applied.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        public DX.RectangleF MeasureSprite(GorgonPolySprite sprite)
        {
            if (_initialized != Initialized)
            {
                Initialize();
            }

            if (sprite.IsUpdated)
            {
                _polyTransformer.Transform(sprite.Renderable);
            }

            return GetTransformedBounds(sprite.Renderable);
        }

        /// <summary>
        /// Function to draw a sprite.
        /// </summary>
        /// <param name="sprite">The sprite object to draw.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method draws a regular rectangular <see cref="GorgonSprite"/> object. 
        /// </para>
        /// <para>
        /// A <see cref="GorgonSprite"/> is a data object that provides a means to rotate, scale and translate a texture region when rendering. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonSprite"/>
        public void DrawSprite(GorgonSprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));

#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            BatchRenderable renderable = sprite.Renderable;

            RenderBatchOnChange(renderable, true);

            if (sprite.IsUpdated)
            {
                _batchRenderer.SpriteTransformer.Transform(renderable);
            }

            _batchRenderer.QueueRenderable(renderable);
        }

        /// <summary>
        /// Function to retrieve the axis-aligned bounding box for a sprite.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the AABB from.</param>
        /// <returns>A rectangle containing the axis aligned bounding box.</returns>
        public DX.RectangleF GetAABB(GorgonSprite sprite)
        {
            GetAABB(sprite, out DX.RectangleF result);
            return result;
        }

        /// <summary>
        /// Function to retrieve the axis-aligned bounding box for a sprite.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the AABB from.</param>
        /// <param name="aabb">A rectangle containing the axis-aligned bounding box.</param>
        public void GetAABB(GorgonSprite sprite, out DX.RectangleF aabb)
        {
            if (_initialized == Uninitialized)
            {
                Initialize();
            }

            float x1 = float.MaxValue;
            float x2 = float.MinValue;
            float y1 = float.MaxValue;
            float y2 = float.MinValue;

            BatchRenderable renderable = sprite.Renderable;

            if (sprite.IsUpdated)
            {
                _batchRenderer.SpriteTransformer.Transform(renderable);
            }

            Gorgon2DVertex[] vertices = renderable.Vertices;

            for (int i = 0; i < vertices.Length; ++i)
            {
                ref readonly Gorgon2DVertex vertex = ref vertices[i];

                x1 = x1.Min(vertex.Position.X);
                y1 = y1.Min(vertex.Position.Y);
                x2 = x2.Max(vertex.Position.X);
                y2 = y2.Max(vertex.Position.Y);
            }

            aabb = new DX.RectangleF
            {
                Left = x1,
                Top = y1,
                Right = x2,
                Bottom = y2
            };
        }

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="font">[Optional] The font to use.</param>
        /// <param name="color">[Optional] The color of the text.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that allows an application to draw text directly to the currently assigned render target.  
        /// </para>
        /// <para>
        /// If the <paramref name="font"/> parameter is not specified, then the <see cref="DefaultFont"/> is used to render the text.
        /// </para>
        /// <para>
        /// If the <paramref name="color"/> parameter is not specified, then the <see cref="GorgonColor.White"/> color is used.
        /// </para>
        /// </remarks>
        public void DrawString(string text, DX.Vector2 position, GorgonFont font = null, GorgonColor? color = null)
        {
            // We have nothing to render.
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // Reset the font prior to doing anything.  If the font associated with the text sprite is disposed and we try to use it (e.g. by assigning text), 
            // then an error can occur because the font is no longer valid. By setting the font first, we are assured that we always have a valid font.
            _defaultTextSprite.Font = font ?? _defaultFontFactory.Value.DefaultFont;
            _defaultTextSprite.Text = text;
            _defaultTextSprite.Color = color ?? GorgonColor.White;
            _defaultTextSprite.Position = position;
            _defaultTextSprite.DrawMode = _defaultTextSprite.Font.HasOutline ? TextDrawMode.OutlinedGlyphs : TextDrawMode.GlyphsOnly;
            _defaultTextSprite.AllowColorCodes = (text.IndexOf("[c", StringComparison.CurrentCultureIgnoreCase) > -1)
                                                 && (text.IndexOf("[/c]", StringComparison.CurrentCultureIgnoreCase) > -1);

            DrawTextSprite(_defaultTextSprite);
        }

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="sprite">The text sprite to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method is used to draw a <see cref="GorgonTextSprite"/> to the current render target. A <see cref="GorgonTextSprite"/> is similar to a <see cref="GorgonSprite"/> in that it allows an
        /// application to take a block of text and translate, scale, and rotate the block of text.  
        /// </para>
        /// <para>
        /// Unlike the <see cref="DrawString"/> method, which just renders whatever text is sent to it, a <see cref="GorgonTextSprite"/> can also be used to align text to a boundary (e.g. center, left
        /// align, etc...). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTextSprite"/>
        /// <seealso cref="GorgonSprite"/>
        public void DrawTextSprite(GorgonTextSprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // The number of characters evaluated.
            int charCount = 0;
            // The index into the vertex array for the sprite.
            int vertexOffset = 0;
            // The position of the current glyph.
            DX.Vector2 position = DX.Vector2.Zero;

            sprite.ValidateObject(nameof(sprite));

            _textBuffer.Length = 0;
            int textLength = sprite.Text.Length;

            // If there's no text, then there's nothing to render.
            if (textLength == 0)
            {
                return;
            }

            TextRenderable renderable = sprite.Renderable;

            // Flush the previous batch if we have one that's different from the upcoming batch.
            RenderBatchOnChange(renderable, true);

            _textBuffer.Append(sprite.Text);

            Alignment alignment = renderable.Alignment;
            GorgonFont font = renderable.Font;
            bool drawOutlines = ((renderable.DrawMode != TextDrawMode.GlyphsOnly) && (font.HasOutline));
            int drawCount = ((drawOutlines) && (renderable.DrawMode == TextDrawMode.OutlinedGlyphs)) ? 2 : 1;
            float fontHeight = font.FontHeight.FastFloor();
            bool hasKerning = (font.Info.UseKerningPairs) && (font.KerningPairs.Count > 0);
            IDictionary<GorgonKerningPair, int> kerningValues = font.KerningPairs;
            float lineSpaceMultiplier = renderable.LineSpaceMultiplier;
            List<ColorBlock> colorBlocks = renderable.ColorBlocks;
            bool allowColorCodes = sprite.AllowColorCodes;
            bool isUpdated = sprite.IsUpdated;

            renderable.IndexCount = 0;
            renderable.ActualVertexCount = 0;

            for (int line = 0; line < renderable.Lines.Length; ++line)
            {
                string textLine = sprite.Lines[line];
                textLength = textLine.Length;

                for (int dc = 0; dc < drawCount; ++dc)
                {
                    bool isOutlinePass = (drawOutlines) && (dc == 0);

                    DX.Size2F lineMeasure = DX.Size2F.Empty;

                    if (alignment != Alignment.UpperLeft)
                    {
                        lineMeasure = font.MeasureLine(textLine, (drawOutlines) && (dc == 0), lineSpaceMultiplier);
                    }

                    position.X = 0;

                    for (int i = 0; i < textLength; ++i)
                    {
                        char character = textLine[i];
                        int kernAmount = 0;

                        // Find the color block for the text.
                        GorgonColor? blockColor = null;

                        if (!font.Glyphs.TryGetValue(character, out GorgonGlyph glyph))
                        {
                            if (!font.TryGetDefaultGlyph(out glyph))
                            {
                                // Only update when we're in non-outline.
                                if (!isOutlinePass)
                                {
                                    ++charCount;
                                }
                                continue;
                            }
                        }

                        // Handle whitespace by just advancing our position, we don't need geometry for this.
                        if ((char.IsWhiteSpace(character))
                            || (glyph.TextureView == null))
                        {
                            if (character == '\t')
                            {
                                position.X += glyph.Advance * renderable.TabSpaceCount;
                            }
                            // We don't use carriage returns.
                            else if (character != '\r')
                            {
                                position.X += glyph.Advance;
                            }

                            // Only update when we're in non-outline.
                            if (!isOutlinePass)
                            {
                                ++charCount;
                            }
                            continue;
                        }

                        // If we have a change of texture, then we need to let the renderer know that we need a flush.
                        if ((renderable.Texture != null) && (renderable.Texture != glyph.TextureView))
                        {
                            RenderBatchOnChange(renderable, true);
                            renderable.HasTextureChanges = true;
                        }

                        renderable.Texture = glyph.TextureView;

                        if (isUpdated)
                        {
                            if ((allowColorCodes) && (!isOutlinePass))
                            {
                                blockColor = _batchRenderer.TextSpriteTransformer.GetColorForCharacter(charCount, colorBlocks);
                            }

                            if ((blockColor != null) && (!renderable.HasVertexColorChanges))
                            {
                                renderable.HasVertexColorChanges = true;
                            }

                            if ((hasKerning) && (i < textLength - 1))
                            {
                                var kernPair = new GorgonKerningPair(character, textLine[i + 1]);
                                kerningValues.TryGetValue(kernPair, out kernAmount);
                            }


                            _batchRenderer.TextSpriteTransformer.Transform(renderable,
                                                                            glyph,
                                                                            blockColor,
                                                                            ref position,
                                                                            vertexOffset,
                                                                            isOutlinePass,
                                                                            lineMeasure.Width);

                            vertexOffset += 4;
                            position.X += glyph.Advance + kernAmount;
                        }

                        // Only update when we're in non-outline.
                        if (!isOutlinePass)
                        {
                            ++charCount;
                        }

                        renderable.IndexCount += 6;
                        renderable.ActualVertexCount += 4;
                    }

                }

                // This is to account for the new line character.
                ++charCount;
                position.Y += fontHeight * lineSpaceMultiplier;
            }

            if (renderable.IndexCount != 0)
            {
                _batchRenderer.QueueRenderable(renderable);
            }

            renderable.VertexCountChanged = false;
            renderable.HasTransformChanges = false;
            renderable.HasVertexChanges = false;
            renderable.HasTextureChanges = false;
            renderable.HasVertexColorChanges = false;
        }

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawFilledRectangle(DX.RectangleF region, GorgonColor color, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // If there's no width/height, then there's nothing to draw.
            if (region.IsEmpty)
            {
                return;
            }

            ref Gorgon2DVertex v0 = ref _primitiveRenderable.Vertices[0];
            ref Gorgon2DVertex v1 = ref _primitiveRenderable.Vertices[1];
            ref Gorgon2DVertex v2 = ref _primitiveRenderable.Vertices[2];
            ref Gorgon2DVertex v3 = ref _primitiveRenderable.Vertices[3];

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            if (texture != null)
            {
                textureArrayIndex = textureArrayIndex.Max(0);

                if (textureRegion == null)
                {
                    // Calculate the texture.
                    v0.UV = new DX.Vector3(region.Left / texture.Width, region.Top / texture.Height, textureArrayIndex);
                    v1.UV = new DX.Vector3(region.Right / texture.Width, region.Top / texture.Height, textureArrayIndex);
                    v2.UV = new DX.Vector3(region.Left / texture.Width, region.Bottom / texture.Height, textureArrayIndex);
                    v3.UV = new DX.Vector3(region.Right / texture.Width, region.Bottom / texture.Height, textureArrayIndex);
                }
                else
                {
                    v0.UV = new DX.Vector3(textureRegion.Value.TopLeft, textureArrayIndex);
                    v1.UV = new DX.Vector3(textureRegion.Value.TopRight, textureArrayIndex);
                    v2.UV = new DX.Vector3(textureRegion.Value.BottomLeft, textureArrayIndex);
                    v3.UV = new DX.Vector3(textureRegion.Value.BottomRight, textureArrayIndex);
                }
            }
            else
            {
                v0.UV = DX.Vector3.Zero;
                v1.UV = new DX.Vector3(1.0f, 0, 0);
                v2.UV = new DX.Vector3(0, 1.0f, 0);
                v3.UV = new DX.Vector3(1.0f, 1.0f, 0);

                texture = _defaultTexture;
            }

            v0.Color = color;
            v1.Color = color;
            v2.Color = color;
            v3.Color = color;

            v0.Position = new DX.Vector4(region.TopLeft, depth, 1.0f);
            v1.Position = new DX.Vector4(region.TopRight, depth, 1.0f);
            v2.Position = new DX.Vector4(region.BottomLeft, depth, 1.0f);
            v3.Position = new DX.Vector4(region.BottomRight, depth, 1.0f);

            CheckPrimitiveStateChange(texture, textureSampler);

            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleList;
            _primitiveRenderable.Bounds = region;
            _primitiveRenderable.ActualVertexCount = 4;
            _primitiveRenderable.IndexCount = 6;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);
            _primitiveRenderable.Texture = texture;
            _primitiveRenderable.TextureSampler = textureSampler;

            RenderBatchOnChange(_primitiveRenderable, true);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw a simple triangle.
        /// </summary>
        /// <param name="point1">The vertex for the first point in the triangle.</param>
        /// <param name="point2">The vertex for the second point in the triangle.</param>
        /// <param name="point3">The vertex for the third point in the triangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawTriangle(in GorgonTriangleVertex point1, in GorgonTriangleVertex point2, in GorgonTriangleVertex point3, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif
            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            CheckPrimitiveStateChange(texture, textureSampler);

            _primitiveRenderable.ActualVertexCount = 3;
            _primitiveRenderable.IndexCount = 0;
            _primitiveRenderable.Vertices[0] = new Gorgon2DVertex
            {
                Color = point1.Color,
                Position = new DX.Vector4(point1.Position, depth, 1.0f),
                UV = texture != null ? new DX.Vector3(point1.TextureCoordinate, point1.TextureArrayIndex) : DX.Vector3.Zero
            };
            _primitiveRenderable.Vertices[1] = new Gorgon2DVertex
            {
                Color = point2.Color,
                Position = new DX.Vector4(point2.Position, depth, 1.0f),
                UV = texture != null ? new DX.Vector3(point2.TextureCoordinate, point2.TextureArrayIndex) : DX.Vector3.Zero
            };
            _primitiveRenderable.Vertices[2] = new Gorgon2DVertex
            {
                Color = point3.Color,
                Position = new DX.Vector4(point3.Position, depth, 1.0f),
                UV = texture != null ? new DX.Vector3(point3.TextureCoordinate, point3.TextureArrayIndex) : DX.Vector3.Zero
            };
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);
            _primitiveRenderable.Texture = texture ?? _defaultTexture;
            _primitiveRenderable.TextureSampler = textureSampler;

            RenderBatchOnChange(_primitiveRenderable, false);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawRectangle(DX.RectangleF region,
                                  GorgonColor color,
                                  float thickness = 1.0f,
                                  GorgonTexture2DView texture = null,
                                  DX.RectangleF? textureRegion = null,
                                  int textureArrayIndex = 0,
                                  GorgonSamplerState textureSampler = null,
                                  float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // If there's no width/height or thickness, then there's nothing to draw.
            if ((region.IsEmpty) || (thickness <= 0.0f))
            {
                return;
            }

            //// Push borders to the outside.
            if (thickness > 1.0f)
            {
                region.Inflate(thickness / 2.0f, thickness / 2.0f);
            }

            DX.RectangleF? topAcross = null;
            DX.RectangleF? bottomAcross = null;
            DX.RectangleF? leftDown = null;
            DX.RectangleF? rightDown = null;

            // If we supply our own texture coordinates, then ensure that the individual lines are mapped appropriately.
            if ((textureRegion != null) && (texture != null))
            {
                var innerRect = new DX.RectangleF(thickness, thickness, region.Width - (thickness * 2), region.Height - (thickness * 2));

                innerRect.Left = ((innerRect.Left / region.Width) * textureRegion.Value.Width) + textureRegion.Value.Left;
                innerRect.Top = ((innerRect.Top / region.Height) * textureRegion.Value.Height) + textureRegion.Value.Top;
                innerRect.Right = (innerRect.Right / region.Width) * textureRegion.Value.Right;
                innerRect.Bottom = (innerRect.Bottom / region.Height) * textureRegion.Value.Bottom;

                topAcross = new DX.RectangleF
                {
                    Left = textureRegion.Value.Left,
                    Top = textureRegion.Value.Top,
                    Right = textureRegion.Value.Right,
                    Bottom = innerRect.Top
                };

                rightDown = new DX.RectangleF
                {
                    Left = innerRect.Right,
                    Top = innerRect.Top,
                    Right = textureRegion.Value.Right,
                    Bottom = innerRect.Bottom
                };

                bottomAcross = new DX.RectangleF
                {
                    Left = textureRegion.Value.Left,
                    Top = innerRect.Bottom,
                    Right = textureRegion.Value.Right,
                    Bottom = textureRegion.Value.Bottom
                };

                leftDown = new DX.RectangleF
                {
                    Left = textureRegion.Value.Left,
                    Top = innerRect.Top,
                    Right = innerRect.Left,
                    Bottom = innerRect.Bottom
                };
            }

            // Top Across.
            DrawFilledRectangle(new DX.RectangleF(region.X, region.Y, region.Width, thickness),
                                color,
                                texture,
                                topAcross,
                                textureArrayIndex,
                                textureSampler,
                                depth);

            // Right down.
            DrawFilledRectangle(new DX.RectangleF(region.Right - thickness, region.Y + thickness, thickness, region.Height - (thickness * 2)),
                                color,
                                texture,
                                rightDown,
                                textureArrayIndex,
                                textureSampler,
                                depth);

            // Bottom across.
            DrawFilledRectangle(new DX.RectangleF(region.X, region.Bottom - thickness, region.Width, thickness),
                                color,
                                texture,
                                bottomAcross,
                                textureArrayIndex,
                                textureSampler,
                                depth);

            // Left down.
            DrawFilledRectangle(new DX.RectangleF(region.X, region.Y + thickness, thickness, region.Height - (thickness * 2)),
                                color,
                                texture,
                                leftDown,
                                textureArrayIndex,
                                textureSampler,
                                depth);
        }

        /// <summary>
        /// Function to draw a line.
        /// </summary>
        /// <param name="x1">The starting horizontal position.</param>
        /// <param name="y1">The starting vertical position.</param>
        /// <param name="x2">The ending horizontal position.</param>
        /// <param name="y2">The ending vertical position.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the line.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="startDepth">[Optional] The depth value for the starting point of the line.</param>
        /// <param name="endDepth">[Optional] The depth value for the ending point of the line.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawLine(float x1, float y1, float x2, float y2, GorgonColor color, float thickness = 1.0f, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float startDepth = 0, float endDepth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // There's nothing to render.
            if (((x2 == x1) && (y2 == y1)) || (thickness <= 0.0f))
            {
                return;
            }

            ref Gorgon2DVertex v0 = ref _primitiveRenderable.Vertices[0];
            ref Gorgon2DVertex v1 = ref _primitiveRenderable.Vertices[1];
            ref Gorgon2DVertex v2 = ref _primitiveRenderable.Vertices[2];
            ref Gorgon2DVertex v3 = ref _primitiveRenderable.Vertices[3];

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            var bounds = new DX.RectangleF
            {
                Left = x1,
                Top = y1,
                Right = x2,
                Bottom = y2
            };

            // Get cross products of start and end points.
            var cross = new DX.Vector2(bounds.Height, -bounds.Width);
            cross.Normalize();

            DX.Vector2.Multiply(ref cross, thickness / 2.0f, out cross);

            var start1 = new DX.Vector2((x1 + cross.X).FastCeiling(), (y1 + cross.Y).FastCeiling());
            var end1 = new DX.Vector2((x2 + cross.X).FastCeiling(), (y2 + cross.Y).FastCeiling());
            var start2 = new DX.Vector2((x1 - cross.X).FastCeiling(), (y1 - cross.Y).FastCeiling());
            var end2 = new DX.Vector2((x2 - cross.X).FastCeiling(), (y2 - cross.Y).FastCeiling());

            v0.Position = new DX.Vector4(start1, startDepth, 1.0f);
            v1.Position = new DX.Vector4(end1, endDepth, 1.0f);
            v2.Position = new DX.Vector4(start2, startDepth, 1.0f);
            v3.Position = new DX.Vector4(end2, endDepth, 1.0f);

            if (texture != null)
            {
                textureArrayIndex = textureArrayIndex.Max(0);

                if (textureRegion == null)
                {
                    // Calculate the texture.
                    v0.UV = new DX.Vector3(start1.X / texture.Width, start1.Y / texture.Height, textureArrayIndex);
                    v1.UV = new DX.Vector3(end1.X / texture.Width, end1.Y / texture.Height, textureArrayIndex);
                    v2.UV = new DX.Vector3(start2.X / texture.Width, start2.Y / texture.Height, textureArrayIndex);
                    v3.UV = new DX.Vector3(end2.X / texture.Width, end2.Y / texture.Height, textureArrayIndex);
                }
                else
                {
                    // To perform the same kind of texture mapping on a line as we have on other primitives, we need to 
                    // find the min and max of the line vertices.
                    bounds = new DX.RectangleF
                    {
                        Left = float.MaxValue,
                        Top = float.MaxValue,
                        Right = float.MinValue,
                        Bottom = float.MinValue
                    };

                    for (int i = 0; i < 4; ++i)
                    {
                        bounds.Left = bounds.Left.Min(_primitiveRenderable.Vertices[i].Position.X);
                        bounds.Top = bounds.Top.Min(_primitiveRenderable.Vertices[i].Position.Y);
                        bounds.Right = bounds.Right.Max(_primitiveRenderable.Vertices[i].Position.X);
                        bounds.Bottom = bounds.Bottom.Max(_primitiveRenderable.Vertices[i].Position.Y);
                    }

                    v0.UV = new DX.Vector3((((start1.X - bounds.Left) / bounds.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                           (((start1.Y - bounds.Top) / bounds.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                           textureArrayIndex);
                    v1.UV = new DX.Vector3((((end1.X - bounds.Left) / bounds.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                           (((end1.Y - bounds.Top) / bounds.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                           textureArrayIndex);
                    v2.UV = new DX.Vector3((((start2.X - bounds.Left) / bounds.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                           (((start2.Y - bounds.Top) / bounds.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                           textureArrayIndex);
                    v3.UV = new DX.Vector3((((end2.X - bounds.Left) / bounds.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                           (((end2.Y - bounds.Top) / bounds.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                           textureArrayIndex);
                }
            }
            else
            {
                v0.UV = DX.Vector3.Zero;
                v1.UV = new DX.Vector3(1.0f, 0, 0);
                v2.UV = new DX.Vector3(0, 1.0f, 0);
                v3.UV = new DX.Vector3(1.0f, 1.0f, 0);

                texture = _defaultTexture;
            }


            v0.Color = color;
            v1.Color = color;
            v2.Color = color;
            v3.Color = color;

            CheckPrimitiveStateChange(texture, textureSampler);

            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleList;
            _primitiveRenderable.ActualVertexCount = 4;
            _primitiveRenderable.IndexCount = 6;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);
            _primitiveRenderable.Texture = texture;
            _primitiveRenderable.TextureSampler = textureSampler;

            RenderBatchOnChange(_primitiveRenderable, true);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawFilledEllipse(DX.RectangleF region, GorgonColor color, float smoothness = 1.0f, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            int quality = (int)(smoothness * 64.0f).FastCeiling().Max(8).Min(2048);

            // Nothing to draw.
            if (quality == 0)
            {
                return;
            }

            _primitiveRenderable.Bounds = region;
            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleStrip;
            _primitiveRenderable.IndexCount = 0;
            _primitiveRenderable.ActualVertexCount = (quality * 2) + 2;

            // Ensure the primitive batch object is large enough to hold our vertex list.
            if ((_primitiveRenderable.Vertices == null) || (_primitiveRenderable.Vertices.Length < _primitiveRenderable.ActualVertexCount))
            {
                _primitiveRenderable.Vertices = new Gorgon2DVertex[_primitiveRenderable.ActualVertexCount * 2];
            }

            DX.Vector2 centerPoint = region.Center;

            var radius = new DX.Vector2(region.Width * 0.5f, region.Height * 0.5f);

            DX.Vector3 uvCenter = DX.Vector3.Zero;

            if (texture != null)
            {
                uvCenter = textureRegion == null
                               ? new DX.Vector3(centerPoint.X / texture.Width, centerPoint.Y / texture.Height, textureArrayIndex)
                               : new DX.Vector3((((centerPoint.X - region.Left) / region.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                (((centerPoint.Y - region.Top) / region.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                textureArrayIndex);

            }

            int vertexIndex = 0;
            for (int i = 0; i <= quality; ++i)
            {
                float angle = (float)i / quality * 2.0f * (float)System.Math.PI;
                float sin = angle.FastSin();
                float cos = angle.FastCos();

                var point = new DX.Vector2((sin * radius.X) + centerPoint.X, (cos * radius.Y) + centerPoint.Y);

                DX.Vector3 uv = DX.Vector3.Zero;

                if (texture != null)
                {
                    uv = textureRegion == null
                             ? new DX.Vector3(point.X / texture.Width,
                                              point.Y / texture.Height,
                                              textureArrayIndex)
                             : new DX.Vector3((((point.X - region.Left) / region.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                              (((point.Y - region.Top) / region.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                              textureArrayIndex);
                }

                ref Gorgon2DVertex v = ref _primitiveRenderable.Vertices[vertexIndex++];
                ref Gorgon2DVertex c = ref _primitiveRenderable.Vertices[vertexIndex++];

                v.Position = new DX.Vector4(point, depth, 1.0f);
                v.Color = color;
                v.UV = uv;

                c.Position = new DX.Vector4(centerPoint, depth, 1.0f);
                c.Color = color;
                c.UV = uvCenter;
            }

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            CheckPrimitiveStateChange(texture, textureSampler);
            _primitiveRenderable.Texture = texture ?? _defaultTexture;
            _primitiveRenderable.TextureSampler = textureSampler;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);

            RenderBatchOnChange(_primitiveRenderable, false);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawArc(DX.RectangleF region, GorgonColor color, float startAngle, float endAngle, float smoothness = 1.0f, float thickness = 1.0f, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // Ensure we don't get overdraw by limiting the angle sizes.
            while (startAngle > 360.0f)
            {
                startAngle -= 360.0f;
            }

            while (endAngle > 360.0f)
            {
                endAngle -= 360.0f;
            }

            float wedgeAngle = (endAngle - startAngle).Abs();
            float wedgeRatio = wedgeAngle / 360.0f;
            int quality = (int)((smoothness * 64.0f) * wedgeRatio).FastCeiling().Max(0).Min(2048);

            // Nothing to draw.
            if ((quality == 0) || (thickness <= 0.0f))
            {
                return;
            }

            _primitiveRenderable.Bounds = region;
            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleStrip;
            _primitiveRenderable.IndexCount = 0;
            _primitiveRenderable.ActualVertexCount = (quality * 2) + 2;

            // Ensure the primitive batch object is large enough to hold our vertex list.
            if ((_primitiveRenderable.Vertices == null) || (_primitiveRenderable.Vertices.Length < _primitiveRenderable.ActualVertexCount))
            {
                _primitiveRenderable.Vertices = new Gorgon2DVertex[_primitiveRenderable.ActualVertexCount * 2];
            }

            DX.Vector2 centerPoint = region.Center;

            var outerRadius = new DX.Vector2((region.Width * 0.5f) + (thickness * 0.5f), (region.Height * 0.5f) + (thickness * 0.5f));
            var innerRadius = new DX.Vector2((region.Width * 0.5f) - (thickness * 0.5f), (region.Height * 0.5f) - (thickness * 0.5f));


            int vertexIndex = 0;
            for (int i = 0; i <= quality; ++i)
            {
                float angle = ((float)i / quality * wedgeRatio * 2.0f * (float)System.Math.PI) + startAngle.ToRadians();
                float sin = angle.FastSin();
                float cos = angle.FastCos();

                var innerPoint = new DX.Vector2((sin * innerRadius.X) + centerPoint.X, (cos * innerRadius.Y) + centerPoint.Y);
                var outerPoint = new DX.Vector2((sin * outerRadius.X) + centerPoint.X, (cos * outerRadius.Y) + centerPoint.Y);

                DX.Vector3 uvInner = DX.Vector3.Zero;
                DX.Vector3 uvOuter = DX.Vector3.Zero;

                if (texture != null)
                {
                    if (textureRegion == null)
                    {
                        uvOuter = new DX.Vector3(outerPoint.X / texture.Width,
                                                 outerPoint.Y / texture.Height,
                                                 textureArrayIndex);
                        uvInner = new DX.Vector3(innerPoint.X / texture.Width,
                                                 innerPoint.Y / texture.Height,
                                                 textureArrayIndex);
                    }
                    else
                    {
                        DX.RectangleF scaleRegion = region;
                        scaleRegion.Inflate(thickness * 0.5f, thickness * 0.5f);
                        uvOuter = new DX.Vector3((((outerPoint.X - scaleRegion.Left) / scaleRegion.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                 (((outerPoint.Y - scaleRegion.Top) / scaleRegion.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                 textureArrayIndex);
                        uvInner = new DX.Vector3((((innerPoint.X - scaleRegion.Left) / scaleRegion.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                 (((innerPoint.Y - scaleRegion.Top) / scaleRegion.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                 textureArrayIndex);
                    }
                }

                ref Gorgon2DVertex vOuter = ref _primitiveRenderable.Vertices[vertexIndex++];
                ref Gorgon2DVertex vInner = ref _primitiveRenderable.Vertices[vertexIndex++];

                vOuter.Position = new DX.Vector4(outerPoint, depth, 1.0f);
                vOuter.Color = color;
                vOuter.UV = uvOuter;

                vInner.Position = new DX.Vector4(innerPoint, depth, 1.0f);
                vInner.Color = color;
                vInner.UV = uvInner;
            }

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            CheckPrimitiveStateChange(texture, textureSampler);
            _primitiveRenderable.Texture = texture ?? _defaultTexture;
            _primitiveRenderable.TextureSampler = textureSampler;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);

            RenderBatchOnChange(_primitiveRenderable, false);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawFilledArc(DX.RectangleF region, GorgonColor color, float startAngle, float endAngle, float smoothness = 1.0f, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            // Ensure we don't get overdraw by limiting the angle sizes.
            while (startAngle > 360.0f)
            {
                startAngle -= 360.0f;
            }

            while (endAngle > 360.0f)
            {
                endAngle -= 360.0f;
            }

            float wedgeAngle = (endAngle - startAngle).Abs();
            float wedgeRatio = wedgeAngle / 360.0f;
            int quality = (int)((smoothness * 64.0f) * wedgeRatio).FastCeiling().Max(0).Min(2048);

            // Nothing to draw.
            if (quality == 0)
            {
                return;
            }

            _primitiveRenderable.Bounds = region;
            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleStrip;
            _primitiveRenderable.IndexCount = 0;
            _primitiveRenderable.ActualVertexCount = (quality * 2) + 2;

            // Ensure the primitive batch object is large enough to hold our vertex list.
            if ((_primitiveRenderable.Vertices == null) || (_primitiveRenderable.Vertices.Length < _primitiveRenderable.ActualVertexCount))
            {
                _primitiveRenderable.Vertices = new Gorgon2DVertex[_primitiveRenderable.ActualVertexCount * 2];
            }

            DX.Vector2 centerPoint = region.Center;

            var radius = new DX.Vector2(region.Width * 0.5f, region.Height * 0.5f);

            DX.Vector3 uvCenter = DX.Vector3.Zero;

            if (texture != null)
            {
                uvCenter = textureRegion == null
                               ? new DX.Vector3(centerPoint.X / texture.Width, centerPoint.Y / texture.Height, textureArrayIndex)
                               : new DX.Vector3((((centerPoint.X - region.Left) / region.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                (((centerPoint.Y - region.Top) / region.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                textureArrayIndex);

            }

            int vertexIndex = 0;
            for (int i = 0; i <= quality; ++i)
            {
                float angle = ((float)i / quality * wedgeRatio * 2.0f * (float)System.Math.PI) + startAngle.ToRadians();
                float sin = angle.FastSin();
                float cos = angle.FastCos();

                var point = new DX.Vector2((sin * radius.X) + centerPoint.X, (cos * radius.Y) + centerPoint.Y);

                DX.Vector3 uv = DX.Vector3.Zero;

                if (texture != null)
                {
                    uv = textureRegion == null
                             ? new DX.Vector3(point.X / texture.Width,
                                              point.Y / texture.Height,
                                              textureArrayIndex)
                             : new DX.Vector3((((point.X - region.Left) / region.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                              (((point.Y - region.Top) / region.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                              textureArrayIndex);
                }

                ref Gorgon2DVertex v = ref _primitiveRenderable.Vertices[vertexIndex++];
                ref Gorgon2DVertex c = ref _primitiveRenderable.Vertices[vertexIndex++];

                v.Position = new DX.Vector4(point, depth, 1.0f);
                v.Color = color;
                v.UV = uv;

                c.Position = new DX.Vector4(centerPoint, depth, 1.0f);
                c.Color = color;
                c.UV = uvCenter;
            }

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            CheckPrimitiveStateChange(texture, textureSampler);
            _primitiveRenderable.Texture = texture ?? _defaultTexture;
            _primitiveRenderable.TextureSampler = textureSampler;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);

            RenderBatchOnChange(_primitiveRenderable, false);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        public void DrawEllipse(DX.RectangleF region, GorgonColor color, float smoothness = 1.0f, float thickness = 1.0f, GorgonTexture2DView texture = null, DX.RectangleF? textureRegion = null, int textureArrayIndex = 0, GorgonSamplerState textureSampler = null, float depth = 0)
        {
#if DEBUG
            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }
#endif

            int quality = (int)(smoothness * 64.0f).FastCeiling().Max(8).Min(2048);

            // Nothing to draw.
            if ((quality == 0) || (thickness <= 0.0f))
            {
                return;
            }

            _primitiveRenderable.Bounds = region;
            _primitiveRenderable.PrimitiveType = PrimitiveType.TriangleStrip;
            _primitiveRenderable.IndexCount = 0;
            _primitiveRenderable.ActualVertexCount = (quality * 2) + 2;

            // Ensure the primitive batch object is large enough to hold our vertex list.
            if ((_primitiveRenderable.Vertices == null) || (_primitiveRenderable.Vertices.Length < _primitiveRenderable.ActualVertexCount))
            {
                _primitiveRenderable.Vertices = new Gorgon2DVertex[_primitiveRenderable.ActualVertexCount * 2];
            }

            DX.Vector2 centerPoint = region.Center;

            var outerRadius = new DX.Vector2((region.Width * 0.5f) + (thickness * 0.5f), (region.Height * 0.5f) + (thickness * 0.5f));
            var innerRadius = new DX.Vector2((region.Width * 0.5f) - (thickness * 0.5f), (region.Height * 0.5f) - (thickness * 0.5f));


            int vertexIndex = 0;
            for (int i = 0; i <= quality; ++i)
            {
                float angle = (float)i / quality * 2.0f * (float)System.Math.PI;
                float sin = angle.FastSin();
                float cos = angle.FastCos();

                var innerPoint = new DX.Vector2((sin * innerRadius.X) + centerPoint.X, (cos * innerRadius.Y) + centerPoint.Y);
                var outerPoint = new DX.Vector2((sin * outerRadius.X) + centerPoint.X, (cos * outerRadius.Y) + centerPoint.Y);

                DX.Vector3 uvInner = DX.Vector3.Zero;
                DX.Vector3 uvOuter = DX.Vector3.Zero;

                if (texture != null)
                {
                    if (textureRegion == null)
                    {
                        uvOuter = new DX.Vector3(outerPoint.X / texture.Width,
                                                 outerPoint.Y / texture.Height,
                                                 textureArrayIndex);
                        uvInner = new DX.Vector3(innerPoint.X / texture.Width,
                                                 innerPoint.Y / texture.Height,
                                                 textureArrayIndex);
                    }
                    else
                    {
                        DX.RectangleF scaleRegion = region;
                        scaleRegion.Inflate(thickness * 0.5f, thickness * 0.5f);
                        uvOuter = new DX.Vector3((((outerPoint.X - scaleRegion.Left) / scaleRegion.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                 (((outerPoint.Y - scaleRegion.Top) / scaleRegion.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                 textureArrayIndex);
                        uvInner = new DX.Vector3((((innerPoint.X - scaleRegion.Left) / scaleRegion.Width) * textureRegion.Value.Width) + textureRegion.Value.Left,
                                                 (((innerPoint.Y - scaleRegion.Top) / scaleRegion.Height) * textureRegion.Value.Height) + textureRegion.Value.Top,
                                                 textureArrayIndex);
                    }
                }

                ref Gorgon2DVertex vOuter = ref _primitiveRenderable.Vertices[vertexIndex++];
                ref Gorgon2DVertex vInner = ref _primitiveRenderable.Vertices[vertexIndex++];

                vOuter.Position = new DX.Vector4(outerPoint, depth, 1.0f);
                vOuter.Color = color;
                vOuter.UV = uvOuter;

                vInner.Position = new DX.Vector4(innerPoint, depth, 1.0f);
                vInner.Color = color;
                vInner.UV = uvInner;
            }

            if (textureSampler == null)
            {
                textureSampler = GorgonSamplerState.Wrapping;
            }

            CheckPrimitiveStateChange(texture, textureSampler);
            _primitiveRenderable.Texture = texture ?? _defaultTexture;
            _primitiveRenderable.TextureSampler = textureSampler;
            _primitiveRenderable.AlphaTestData = PrimitiveAlphaTestRange == null ? new AlphaTestData(false, GorgonRangeF.Empty) : new AlphaTestData(true, PrimitiveAlphaTestRange.Value);

            RenderBatchOnChange(_primitiveRenderable, false);

            _batchRenderer.QueueRenderable(_primitiveRenderable);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GorgonVertexShader worldShader = Interlocked.Exchange(ref _polyTransformVertexShader, null);
            GorgonVertexShader vertexShader = Interlocked.Exchange(ref _defaultVertexShader, null);
            GorgonPixelShader pixelShader = Interlocked.Exchange(ref _defaultPixelShader, null);
            GorgonPixelShader polyPShader = Interlocked.Exchange(ref _polyPixelShader, null);
            GorgonInputLayout layout = Interlocked.Exchange(ref _vertexLayout, null);
            BatchRenderer spriteRenderer = Interlocked.Exchange(ref _batchRenderer, null);
            GorgonTexture2DView whiteTexture = Interlocked.Exchange(ref _defaultTexture, null);
            GorgonTexture2DView blackTexture = Interlocked.Exchange(ref _blackTexture, null);
            CameraController camController = Interlocked.Exchange(ref _cameraController, null);
            GorgonConstantBufferView world = Interlocked.Exchange(ref _polySpriteDataBuffer, null);
            GorgonConstantBufferView alphaTest = Interlocked.Exchange(ref _alphaTest, null);
            Lazy<GorgonFontFactory> defaultFont = Interlocked.Exchange(ref _defaultFontFactory, null);

            Graphics.RenderTargetChanging -= ValidateBeginEndCall;
            Graphics.ViewportChanging -= ValidateBeginEndCall;
            Graphics.DepthStencilChanging -= ValidateBeginEndCall;
            Graphics.RenderTargetChanged -= RenderTarget_Changed;
            Graphics.ViewportChanged -= RenderTarget_Changed;

            if (defaultFont?.IsValueCreated ?? false)
            {
                defaultFont.Value.Dispose();
            }

            spriteRenderer?.Dispose();
            alphaTest?.Buffer?.Dispose();
            world?.Buffer?.Dispose();
            camController?.Dispose();
            whiteTexture?.Texture?.Dispose();
            blackTexture?.Texture?.Dispose();
            layout?.Dispose();
            polyPShader?.Dispose();
            worldShader?.Dispose();
            vertexShader?.Dispose();
            pixelShader?.Dispose();

            Interlocked.Exchange(ref _initialized, Uninitialized);
        }

        #region Fluent
        /// <summary>
        /// Function to begin rendering a batch.
        /// </summary>
        /// <param name="batchState">[Optional] Defines common state to use when rendering a batch of objects.</param>
        /// <param name="camera">[Optional] A camera to use when rendering.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <exception cref="GorgonException">Thrown if <see cref="IGorgon2DFluent.Begin"/> is called more than once without calling <see cref="IGorgon2DDrawingFluent.End"/>.</exception>
        /// <remarks>
        /// <para>
        /// The 2D renderer uses batching for performance. This means that drawing items with common states (e.g. blending) can all be sent to the GPU at the same time. To faciliate this, applications
        /// must call this method prior to drawing.
        /// </para>
        /// <para>
        /// When batching occurs, all drawing that shares the same state and texture will be drawn in one deferred draw call to the GPU. However, if too many items are drawn (~10,000 sprites), or the item 
        /// being drawn has a different texture than the previous item, then the batch is broken up and the previous items  will be drawn to the GPU first.  So, best practice is to ensure that everything
        /// that is drawn shares the same texture.  This is typically achieved by using a sprite sheet where multiple sprite images are pack into a single texture.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// One exception to this is the <see cref="GorgonPolySprite"/> object, which is drawn immediately.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// Once rendering is done, the user must call <see cref="IGorgon2DDrawingFluent.End"/> to finalize the rendering. Otherwise, items drawn in the batch will not appear.
        /// </para>
        /// <para>
        /// This method takes an optional <see cref="Gorgon2DBatchState"/> object which allows an application to override the blend state, depth/stencil state (if applicable), rasterization state, and
        /// pixel/vertex shaders and their associated resources. This means that if an application wants to, for example, change blending modes, then a separate call to this method is required after
        /// drawing items with the previous blend state.  
        /// </para>
        /// <para>
        /// The other optional parameter, <paramref name="camera"/>, allows an application to change the view in which the items are drawn for a batch. This takes a <see cref="IGorgon2DCamera"/> object
        /// that defines the projection and view of the scene being rendered. It is possible with this object to change the coordinate system, and to allow perspective rendering for a batch.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// There are a few things to be aware of when rendering:
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>Batches <b>cannot</b> be nested.  Attempting to do so will cause an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Applications <b>must</b> call this method prior to drawing anything. Failure to do so will result in an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Calls to <see cref="GorgonGraphics.SetRenderTarget"/>, <see cref="GorgonGraphics.SetRenderTargets"/>, <see cref="GorgonGraphics.SetDepthStencil"/>,
        /// <see cref="GorgonGraphics.SetViewport"/>, or <see cref="GorgonGraphics.SetViewports"/> while a batch is in progress is not allowed and will result in an exception if attempted.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="Gorgon2DBatchState"/>
        /// <seealso cref="IGorgon2DCamera"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonGraphics"/>
        IGorgon2DDrawingFluent IGorgon2DFluent.Begin(Gorgon2DBatchState batchState, IGorgon2DCamera camera)
        {
            Begin(batchState, camera);
            return this;
        }

        /// <summary>
        /// Function to draw a polygonal sprite.
        /// </summary>
        /// <param name="sprite">The polygon sprite to draw.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method draws a sprite using a polygon as its surface. This is different from other sprite rendering in that:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The surface is not rectangular.</description>
        ///     </item>
        ///     <item>
        ///         <description>It is not batched with other drawing types, and is drawn immediately.  This may be a performance hit.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// The method takes a <see cref="GorgonPolySprite"/> object which contains <see cref="GorgonPolySpriteVertex"/> objects to define the outer shape (hull) of the polygon. Gorgon will triangulate
        /// the hull into a mesh that can be rendered. 
        /// </para>
        /// <para>
        /// <see cref="GorgonPolySprite"/> objects cannot be created directly, but can be built using the <see cref="GorgonPolySpriteBuilder"/> object.  Please note that these objects implement
        /// <see cref="IDisposable"/>, so users should call the <c>Dispose</c> method when they are done with the objects.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonPolySpriteBuilder"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonPolySpriteVertex"/>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawPolygonSprite(GorgonPolySprite sprite)
        {
            DrawPolygonSprite(sprite);
            return this;
        }

        /// <summary>
        /// Function to draw a sprite.
        /// </summary>
        /// <param name="sprite">The sprite object to draw.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method draws a regular rectangular <see cref="GorgonSprite"/> object. 
        /// </para>
        /// <para>
        /// A <see cref="GorgonSprite"/> is a data object that provides a means to rotate, scale and translate a texture region when rendering. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonSprite"/>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawSprite(GorgonSprite sprite)
        {
            DrawSprite(sprite);
            return this;
        }

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="font">[Optional] The font to use.</param>
        /// <param name="color">[Optional] The color of the text.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that allows an application to draw text directly to the currently assigned render target.  
        /// </para>
        /// <para>
        /// If the <paramref name="font"/> parameter is not specified, then the <see cref="DefaultFont"/> is used to render the text.
        /// </para>
        /// <para>
        /// If the <paramref name="color"/> parameter is not specified, then the <see cref="GorgonColor.White"/> color is used.
        /// </para>
        /// </remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawString(string text, DX.Vector2 position, GorgonFont font, GorgonColor? color)
        {
            DrawString(text, position, font, color);
            return this;
        }

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="sprite">The text sprite to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="Begin"/> first.</exception>
        /// <remarks>
        /// <para>
        /// This method is used to draw a <see cref="GorgonTextSprite"/> to the current render target. A <see cref="GorgonTextSprite"/> is similar to a <see cref="GorgonSprite"/> in that it allows an
        /// application to take a block of text and translate, scale, and rotate the block of text.  
        /// </para>
        /// <para>
        /// Unlike the <see cref="IGorgon2DDrawingFluent.DrawString"/> method, which just renders whatever text is sent to it, a <see cref="GorgonTextSprite"/> can also be used to align text to a boundary (e.g. center, left
        /// align, etc...). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTextSprite"/>
        /// <seealso cref="GorgonSprite"/>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawTextSprite(GorgonTextSprite sprite)
        {
            DrawTextSprite(sprite);
            return this;
        }

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception> 
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawFilledRectangle(DX.RectangleF region, GorgonColor color, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawFilledRectangle(region, color, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw a simple triangle.
        /// </summary>
        /// <param name="point1">The vertex for the first point in the triangle.</param>
        /// <param name="point2">The vertex for the second point in the triangle.</param>
        /// <param name="point3">The vertex for the third point in the triangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawTriangle(in GorgonTriangleVertex point1, in GorgonTriangleVertex point2, in GorgonTriangleVertex point3, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawTriangle(point1, point2, point3, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawRectangle(DX.RectangleF region, GorgonColor color, float thickness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawRectangle(region, color, thickness, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw a line.
        /// </summary>
        /// <param name="x1">The starting horizontal position.</param>
        /// <param name="y1">The starting vertical position.</param>
        /// <param name="x2">The ending horizontal position.</param>
        /// <param name="y2">The ending vertical position.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the line.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="startDepth">[Optional] The depth value for the starting point of the line.</param>
        /// <param name="endDepth">[Optional] The depth value for the ending point of the line.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawLine(float x1, float y1, float x2, float y2, GorgonColor color, float thickness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float startDepth, float endDepth)
        {
            DrawLine(x1, y1, x2, y2, color, thickness, texture, textureRegion, textureArrayIndex, textureSampler, startDepth, endDepth);
            return this;
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawFilledEllipse(DX.RectangleF region, GorgonColor color, float smoothness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawFilledEllipse(region, color, smoothness, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawArc(DX.RectangleF region, GorgonColor color, float startAngle, float endAngle, float smoothness, float thickness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawArc(region, color, startAngle, endAngle, smoothness, thickness, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawFilledArc(DX.RectangleF region, GorgonColor color, float startAngle, float endAngle, float smoothness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawFilledArc(region, color, startAngle, endAngle, smoothness, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method was called without having called <see cref="IGorgon2DFluent.Begin"/> first.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawEllipse(DX.RectangleF region, GorgonColor color, float smoothness, float thickness, GorgonTexture2DView texture, DX.RectangleF? textureRegion, int textureArrayIndex, GorgonSamplerState textureSampler, float depth)
        {
            DrawEllipse(region, color, smoothness, thickness, texture, textureRegion, textureArrayIndex, textureSampler, depth);
            return this;
        }

        /// <summary>
        /// Function to perform an arbitrary update of any required logic while rendering.
        /// </summary>
        /// <param name="updateMethod">A method supplied by the user to perform some custom logic on objects that need to be rendered.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.Update(Action<IGorgon2DDrawingFluent> updateMethod)
        {
            updateMethod?.Invoke(this);
            return this;
        }

        /// <summary>
        /// Function to perform an arbitrary update of any required logic prior to rendering.
        /// </summary>
        /// <param name="updateMethod">A method supplied by the user to perform some custom logic on objects that need to be rendered.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        IGorgon2DFluent IGorgon2DFluent.Update(Action<GorgonGraphics> updateMethod)
        {
            updateMethod?.Invoke(Graphics);
            return this;
        }

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        /// <returns>The <see cref="IGorgon2DFluent"/> interface to allow continuation of rendering.</returns>
        /// <remarks>
        /// <para>
        /// This finalizes rendering and flushes the current batch data to the GPU. Effectively, this is the method that performs the actual rendering for anything the user has drawn.
        /// </para>
        /// <para>
        /// The 2D renderer uses batching to achieve its performance. Because of this, we define a batch with a call to <see cref="IGorgon2DFluent.Begin"/> and <c>End</c>. So, for optimal performance, it is best to draw
        /// as much drawing as possible within the Begin/End batch body.
        /// </para>
        /// <para>
        /// This method must be paired with a call to <see cref="IGorgon2DFluent.Begin"/>, if it is not, it will do nothing. If this method is not called after a call to <see cref="IGorgon2DFluent.Begin"/>, then nothing (in most cases) 
        /// will be drawn. If a previous call to <see cref="IGorgon2DFluent.Begin"/> is made, and this method is not called, and another call to <see cref="IGorgon2DFluent.Begin"/> is made, an exception is thrown.
        /// </para>
        /// </remarks>
        IGorgon2DFluent IGorgon2DDrawingFluent.End()
        {
            End();
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DFluent IGorgon2DFluent.MeasureSprite(GorgonPolySprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DFluent IGorgon2DFluent.MeasureSprite(GorgonTextSprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DFluent IGorgon2DFluent.MeasureSprite(GorgonSprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.MeasureSprite(GorgonPolySprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.MeasureSprite(GorgonTextSprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>Property to return the bounds of the sprite, with transformation applied.</summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.MeasureSprite(GorgonSprite sprite, out DX.RectangleF result)
        {
            result = MeasureSprite(sprite);
            return this;
        }

        /// <summary>
        /// Function to evaluate an expression and if the expression is <b>true</b>, then execute a series of drawing commands.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="drawCommands">The commands to execute.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="expression"/> parameter is <b>null</b>.</exception>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawIf(Func<bool> expression, Action<IGorgon2DDrawingFluent> drawCommands)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression())
            {
                drawCommands?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Function to execute a callback method for each item in an enumerable list of items.
        /// </summary>
        /// <param name="items">The list of items to enumerate through.</param>
        /// <param name="drawCommands">The callback method containing the drawing commands.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="items"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="drawCommands"/> parameter is a function that supplies the current iteration number of the loop, the fluent drawing interface and returns <b>true</b> to indicate that looping 
        /// should continue, or <b>false</b> to stop looping. 
        /// </para>
        /// </remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawEach<T>(IEnumerable<T> items, Func<T, IGorgon2DDrawingFluent, bool> drawCommands)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (drawCommands == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                if (!drawCommands(item, this))
                {
                    return this;
                }
            }

            return this;
        }

        /// <summary>
        /// Function to execute a callback method containing drawing commands for the supplied amount of times.
        /// </summary>
        /// <param name="count">The number of times to loop.</param>
        /// <param name="drawCommands">The callback method containing the drawing commands.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="drawCommands"/> parameter is a function that supplies the current iteration number of the loop, the fluent drawing interface and returns <b>true</b> to indicate that looping 
        /// should continue, or <b>false</b> to stop looping. 
        /// </para>
        /// </remarks>
        IGorgon2DDrawingFluent IGorgon2DDrawingFluent.DrawLoop(int count, Func<int, IGorgon2DDrawingFluent, bool> drawCommands)
        {
            if (drawCommands == null)
            {
                return this;
            }

            for (int i = 0; i < count; ++i)
            {
                if (!drawCommands(i, this))
                {
                    return this;
                }
            }

            return this;
        }
        #endregion
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2D"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use for rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public Gorgon2D(GorgonGraphics graphics)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _defaultFontFactory = new Lazy<GorgonFontFactory>(() => new GorgonFontFactory(Graphics), true);

            if (!GorgonShaderFactory.Includes.ContainsKey("Gorgon2DShaders"))
            {
                GorgonShaderFactory.Includes["Gorgon2DShaders"] = new GorgonShaderInclude("Gorgon2DShaders", Resources.BasicSprite);
            }
        }
        #endregion
    }
}
