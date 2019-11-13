#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: November 14, 2019 10:53:27 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A GBuffer containing targets for rendering effects that have need of multiple render targets.
    /// </summary>
    public class Gorgon2DGBuffer
        : Gorgon2DEffect
    {
        #region Variables.
        // The graphics interface used for target creation.
        private GorgonGraphics _graphics;
        // The renderer used to render to the GBuffer.
        private Gorgon2D _renderer;
        // The default batch render state for drawing normals.
        private Gorgon2DBatchState _gbufferState;
        // Our custom vertex shader so we can calculate the tangent and bi-tangent.
        private GorgonVertexShader _vertexShader;
        private Gorgon2DShaderState<GorgonVertexShader> _vertexShaderState;
        // A pixel shader used to render the normals.
        private GorgonPixelShader _pixelShader;
        private Gorgon2DShaderState<GorgonPixelShader> _pixelShaderState;
        // The texture that holds the GBuffer data.
        private GorgonTexture2D _gbuffer;
        // The render targets for the gbuffer.
        private readonly GorgonRenderTarget2DView[] _target = new GorgonRenderTarget2DView[3];
        // Flag to indicate that array indices should be used to render.
        private bool _useArray;
        // Parameters for the gbuffer shader.
        private GorgonConstantBufferView _params;
        // The current normal/specular map textures when using separate textures.
        private GorgonTexture2DView _normalTexture;
        private GorgonTexture2DView _specularTexture;
        // The original render target view.
        private GorgonRenderTargetView _originalTarget;
        // Flag to indicate whether the effect has been initialized or not.
        private bool _initialized;

        // The texture information for the main GBuffer targets.
        private readonly GorgonTexture2DInfo _mainInfo = new GorgonTexture2DInfo("GBuffer")
        {
            ArrayCount = 3,
            Binding = TextureBinding.ShaderResource | TextureBinding.RenderTarget,
            Format = BufferFormat.R8G8B8A8_UNorm,
            Usage = ResourceUsage.Default
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the entire gbuffer texture (all array indices).
        /// </summary>
        public GorgonTexture2DView GBufferTexture
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the diffuse texture for the gbuffer.
        /// </summary>
        public GorgonTexture2DView Diffuse
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the diffuse render target for the gbuffer.
        /// </summary>
        public GorgonRenderTarget2DView DiffuseTarget => _target[0];

        /// <summary>
        /// Property to return the specular texture for the gbuffer.
        /// </summary>
        public GorgonTexture2DView Specular
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the specular render target for the gbuffer.
        /// </summary>
        public GorgonRenderTarget2DView SpecularTarget => _target[2];

        /// <summary>
        /// Property to return the normal map texture for the gbuffer.
        /// </summary>
        public GorgonTexture2DView Normal
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the normal map render target for the gbuffer.
        /// </summary>
        public GorgonRenderTarget2DView NormalTarget => _target[1];
        #endregion

        #region Methods.
        /// <summary>
        /// Function to dispose of the gbuffer.
        /// </summary>
        private void UnloadGBuffer()
        {
            Diffuse?.Dispose();
            Normal?.Dispose();
            Specular?.Dispose();
            GBufferTexture?.Dispose();

            for (int i = 0; i < _target.Length; ++i)
            {
                _target[i]?.Dispose();
            }

            GBufferTexture = Specular = Normal = Diffuse = null;            
            Array.Clear(_target, 0, _target.Length);
            
            _gbuffer?.Dispose();
        }

        /// <summary>
        /// Function to begin actual rendering of the effect.
        /// </summary>
        /// <param name="blendState">A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">The camera used to transform the lights to camera space.</param>
        private void OnBeginRender(GorgonBlendState blendState, GorgonDepthStencilState depthStencilState, GorgonRasterState rasterState, IGorgon2DCamera camera)
        {
            _originalTarget = Graphics.RenderTargets[0];

            BeginRender(_target[0], blendState, depthStencilState, rasterState);
            BeginPass(0, _target[0], camera);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            UnloadGBuffer();
            
            _params?.Dispose();
            _pixelShader?.Dispose();
        }

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="defaultStatesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        /// <remarks>
        ///   <para>
        /// This method is responsible for initializing and setting up a rendering state for a pass. State changes (e.g. blend states change, additonal textures needed, etc...) are also handled
        /// by this method. Note that the <paramref name="defaultStatesChanged" /> parameter indicates that the user has changed the default effect states when initially rendering (i.e. not per pass).
        /// </para>
        ///   <para>
        /// Developers must take care with this method when creating state objects. Constant discarding and creating of states can get expensive as the garbage collector needs to kick in and release
        /// the memory occupied by the states. To help alleviate constant state changes between passes, of the allocator properties in this class may be used to reuse state objects. The rule of thumb
        /// however is to create a state once, and then just return that state and only recreate when absolutely necessary. Sometimes that's never, other times it's when the swap chain resizes, and
        /// other times it may be on every pass. These conditions depend on the requirements of the effect.
        /// </para>
        /// </remarks>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool defaultStatesChanged)
        {
            if (_vertexShaderState == null)
            {                
                _vertexShaderState = builders.VertexShaderBuilder.Shader(_vertexShader)
                                                                 .Build();
            }

            if (_pixelShader == null)
            {
                Macros.Clear();
                if (_useArray)
                {
                    Macros.Add(new GorgonShaderMacro("USE_ARRAY"));
                }

                _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.GBuffer, "GorgonPixelShaderGBuffer", GorgonGraphics.IsDebugEnabled, Macros);
                _pixelShaderState = null;
            }

            if (_pixelShaderState == null)
            {
                builders.PixelShaderBuilder.Clear()
                                           .Shader(_pixelShader);
                if (!_useArray)
                {
                    builders.PixelShaderBuilder.ShaderResource(_normalTexture ?? Renderer.EmptyNormalMapTexture, 1)
                                               .ShaderResource(_specularTexture ?? Renderer.EmptyBlackTexture, 2)
                                               .SamplerState(GorgonSamplerState.PointFiltering, 1)
                                               .SamplerState(GorgonSamplerState.Default, 2);
                }
                else
                {
                    builders.PixelShaderBuilder.ConstantBuffer(_params, 1);
                }

                _pixelShaderState = builders.PixelShaderBuilder.Build(PixelShaderAllocator);
                _gbufferState = null;
            }

            if ((_gbufferState == null) || (defaultStatesChanged))
            {
                _gbufferState = builders.BatchBuilder.PixelShaderState(_pixelShaderState)
                                                     .VertexShaderState(_vertexShaderState)
                                                     .Build(BatchStateAllocator);
            }

            return _gbufferState;
        }

        /// <summary>Function called prior to rendering.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="sizeChanged">
        ///   <b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        ///   <para>
        /// This method is called prior to rendering the effect (before any passes). It allows an effect to do any required effect set up that's not specific to a pass. For example, this method could
        /// be used to send a view matrix to a shader via a constant buffer since the view matrix will unlikely change between passes.
        /// </para>
        ///   <para>
        /// Developers should override this method to perform any required setup prior to rendering a pass.
        /// </para>
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
        {
            if (sizeChanged)
            {
                Resize(output.Width, output.Height);
            }

            Graphics.SetRenderTargets(_target, Graphics.DepthStencilView);
        }

        /// <summary>Function called to initialize the effect.</summary>
        /// <remarks>
        /// This method is called to allow the effect to initialize. Developers must override this method to compile shaders, constant buffers, etc... and set up any initial values for the effect.
        /// </remarks>
        protected override void OnInitialize()
        {
            if (_initialized)
            {
                return;
            }

            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.GBuffer, "GorgonVertexShaderGBuffer", GorgonGraphics.IsDebugEnabled);
            _params = GorgonConstantBufferView.CreateConstantBuffer(_graphics, new GorgonConstantBufferInfo("GBuffer Parameters")
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = DX.Vector4.SizeInBytes
            });
        }

        /// <summary>
        /// Function to clear the GBuffer.
        /// </summary>
        public void ClearGBuffer()
        {
            const float normChanValue = 127.0f / 255.0f;
            _target[0].Clear(GorgonColor.BlackTransparent);
            _target[1].Clear(new GorgonColor(normChanValue, normChanValue, 1.0f, 0.0f));
            _target[2].Clear(GorgonColor.BlackTransparent);            
        }

        /// <summary>
        /// Function to update the gbuffer to a new width and height.
        /// </summary>
        /// <param name="width">The width of the gbuffer texture.</param>
        /// <param name="height">The height of the gbuffer texture.</param>
        public void Resize(int width, int height)
        {
            // Unassign the render targets if they're assigned.
            if (_graphics.RenderTargets[0] == _target[0])
            {
                _graphics.SetRenderTarget(null);
            }

            UnloadGBuffer();

            _mainInfo.Width = width;
            _mainInfo.Height = height;
            _mainInfo.ArrayCount = _target.Length;
            _gbuffer = new GorgonTexture2D(_graphics, _mainInfo);

            for (int i = 0; i < _mainInfo.ArrayCount; ++i)
            {
                _target[i] = _gbuffer.GetRenderTargetView(arrayIndex: i, arrayCount: 1);
            }

            GBufferTexture = _gbuffer.GetShaderResourceView(arrayCount: _mainInfo.ArrayCount);
            Diffuse = _gbuffer.GetShaderResourceView(arrayIndex: 0, arrayCount: 1);
            Normal = _gbuffer.GetShaderResourceView(arrayIndex: 1, arrayCount: 1);
            Specular = _gbuffer.GetShaderResourceView(arrayIndex: 2, arrayCount: 1);
        }

        /// <summary>
        /// Function to begin rendering using separate texture views for the normal/specular map.
        /// </summary>
        /// <param name="normal">[Optional] The normal map texture to render.</param>
        /// <param name="specular">[Optional] The specular map texture to render.</param>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        public void Begin(GorgonTexture2DView normal = null, GorgonTexture2DView specular = null, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            if ((_normalTexture != normal) || (_specularTexture != specular))
            {
                _normalTexture = normal;
                _specularTexture = specular;
                _pixelShaderState = null;
            }

            if (_useArray)
            {
                _gbufferState = null;
                _pixelShader?.Dispose();
                _pixelShader = null;
                _useArray = false;
            }

            OnBeginRender(blendState, depthStencilState, rasterState, camera);
        }

        /// <summary>
        /// Function to begin rendering using array indices of the texture being rendered for the normal/specular map.
        /// </summary>
        /// <param name="normalMapIndex">The array index of the texture being rendered that contains the normal map.</param>
        /// <param name="specularMapIndex">The array index of the texture being rendered that contains the specular map.</param>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <remarks>
        /// <para>
        /// This method takes the texture of whatever is currently being rendered and uses an array index to index into the texture and retrieve the normal and specular map values.
        /// </para>
        /// </remarks>
        public void Begin(int normalMapIndex, int specularMapIndex, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            if (!_initialized)
            {
                OnInitialize();
                _initialized = true;
            }

            if (!_useArray)
            {
                var gbufferParams = new DX.Vector4(normalMapIndex, specularMapIndex, 0, 0);
                _params.Buffer.SetData(ref gbufferParams);
                _pixelShader?.Dispose();
                _pixelShader = null;
                _useArray = true;
            }

            OnBeginRender(blendState, depthStencilState, rasterState, camera);
        }

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        public void End()
        {
            EndPass(0, _target[0]);
            EndRender(_target[0]);
                        
            Graphics.SetRenderTarget(_originalTarget, Graphics.DepthStencilView);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Gorgon2DGBuffer"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="width">The initial width of the gbuffer.</param>
        /// <param name="height">The initial height of the gbuffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer" /> parameter is <strong>null</strong>.</exception>
        public Gorgon2DGBuffer(Gorgon2D renderer, int width, int height)
            : base(renderer, Resources.GOR2D_EFFECT_GBUFFER, Resources.GOR2D_EFFECT_GBUFFER_DESC, 1)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _graphics = renderer.Graphics;
            Resize(width, height);
        }
        #endregion
    }
}
