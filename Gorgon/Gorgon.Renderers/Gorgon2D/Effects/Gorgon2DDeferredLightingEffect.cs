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
// Created: July 25, 2018 7:36:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect used to render a scene with deferred per-pixel lighting.
    /// </summary>
    public class Gorgon2DDeferredLightingEffect
        : Gorgon2DEffect
    {
        #region Value Types.
        // Constant buffer data for a point light.
        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct PointLightData
        {
            /// <summary>
            /// Position for the light in screen space.
            /// </summary>
            public DX.Vector4 Position;
            /// <summary>
            /// Color of the light.
            /// </summary>
            public GorgonColor Color;
            /// <summary>
            /// The direction for the light.
            /// </summary>
            public DX.Vector4 LightDirection;
            /// <summary>
            /// Various attributes (x = Specular enabled, y = Specular power, z = Attenuation/Intensity).
            /// </summary>
            public DX.Vector2 LightAttribs;
            /// <summary>
            /// Specularity enabled.
            /// </summary>
            public int SpecularEnabled;
            /// <summary>
            /// The light type.
            /// </summary>
            public int LightType;
        }
        #endregion

        #region Constants.
        /// <summary>
        /// The maximum number of lights that can be used at one time.
        /// </summary>
        public const int MaxLightCount = 256;
        #endregion

        #region Variables.
        // Our custom vertex shader so we can calculate the tangent and bi-tangent.
        private Gorgon2DShader<GorgonVertexShader> _vertexShader;
        // A pixel shader used to render the scene to multiple render targets.
        private Gorgon2DShader<GorgonPixelShader> _deferredShader;
        // A pixel shader used to render point lights.
        private Gorgon2DShader<GorgonPixelShader> _lightShader;
        // The constant buffer that will send the screen size on update.
        private GorgonConstantBufferView _screenSizeData;
        // The buffer that will hold the lighting data.
        private GorgonConstantBufferView _lightData;
        // The action used to render the scene data to the various buffers.
        private Action _renderAction;
        // The gbuffer data.
        private GorgonTexture2D _gbuffer;
        // The final output target.
        private GorgonRenderTargetView _finalOutput;
        // The render target for the gbuffer channels.
        private GorgonRenderTargetView[] _gbufferTargets = new GorgonRenderTargetView[3];
        // The texture used to draw the gbuffer data.
        private GorgonTexture2DView _gbufferTexture;
        // The batch render state for drawing to the deferred targets.
        private Gorgon2DBatchState _deferredState;
        // The batch render state for drawing with lights.
        private Gorgon2DBatchState _lightingState;
        // The color used to clear the normal map render target.
        private readonly GorgonColor _normalClearColor = new GorgonColor(127.0f / 255.0f, 127.0f / 255.0f, 254.0f / 255.0f, 1.0f);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of point lights for rendering.
        /// </summary>
        public IList<Gorgon2DLight> Lights
        {
            get;
        } = new List<Gorgon2DLight>(MaxLightCount);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the render targets.
        /// </summary>
        /// <param name="width">The width of the render targets.</param>
        /// <param name="height">The height of the render targets.</param>
        private void BuildRenderTargets(int width, int height)
        {
            _gbufferTexture?.Dispose();

            for (int i = 0; i < _gbufferTargets.Length; ++i)
            {
                _gbufferTargets[i]?.Dispose();
            }

            _gbuffer?.Dispose();

            // For the lighting effect, we use a deferred rendering technique where we have 3 render targets for diffuse, specularity, and normal mapping.
            // These targets are sub resources of the same texture resource (array indices).
            _gbuffer = new GorgonTexture2D(Graphics, new GorgonTexture2DInfo("Gorgon 2D GBuffer")
                                                     {
                                                         Width = width,
                                                         Height = height,
                                                         Format = BufferFormat.R8G8B8A8_UNorm,
                                                         Binding = TextureBinding.ShaderResource | TextureBinding.RenderTarget,
                                                         ArrayCount = 3
                                                     });
            // Diffuse.
            _gbufferTargets[0] = _gbuffer.GetRenderTargetView(arrayIndex: 0, arrayCount: 1);
            // Specular.
            _gbufferTargets[1] = _gbuffer.GetRenderTargetView(arrayIndex: 1, arrayCount: 1);
            // Normals.
            _gbufferTargets[2] = _gbuffer.GetRenderTargetView(arrayIndex: 2, arrayCount: 1);

            _gbufferTexture = _gbuffer.GetShaderResourceView();
        }

        /// <summary>
        /// Function to adjust the targets and screen size buffer when the render target changes size.
        /// </summary>
        /// <param name="width">The width of the output render target.</param>
        /// <param name="height">The height of the output render target.</param>
        private void CheckForOutputSizeChange(int width, int height)
        {
            // The size is the same, we don't need to do anything.
            if ((_finalOutput != null) && (_finalOutput.Width == width) && (_finalOutput.Height == height))
            {
                return;
            }

            // We need to rebuild the render targets to match our output size.
            BuildRenderTargets(width, height);

            var screenSize = new DX.Vector4(width, height, 0, 0);
            _screenSizeData.Buffer.SetData(ref screenSize);
        }

        /// <summary>
        /// Function to update the light data in the lighting shader.
        /// </summary>
        /// <param name="lightIndex">The index of the light to update.</param>
        private void UpdateLight(int lightIndex)
        {
            Gorgon2DLight light = Lights[lightIndex];

            if (light == null)
            {
                return;
            }

            var lightData = new PointLightData
                            {
                                Position = new DX.Vector4(light.Position, 1.0f),
                                Color = light.Color,
                                LightDirection = new DX.Vector4(light.LightDirection, 0),
                                LightAttribs = new DX.Vector2(light.SpecularPower, light.Attenuation),
                                SpecularEnabled = light.SpecularEnabled ? 1 : 0,
                                LightType = (int)light.LightType
                            };

            _lightData.Buffer.SetData(ref lightData);
        }

        /// <summary>
        /// Function to send the gbuffer to the final output target and perform the lighting calculations.
        /// </summary>
        /// <param name="lightIndex">The index of the light to render.</param>
        private void RenderToOutput(int lightIndex)
        {
            UpdateLight(lightIndex);

            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _finalOutput.Width, _finalOutput.Height),
                                         GorgonColor.White,
                                         _gbufferTexture,
                                         new DX.RectangleF(0, 0, 1, 1));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Interlocked.Exchange(ref _finalOutput, null);
            Gorgon2DShader<GorgonVertexShader> vertexShader = Interlocked.Exchange(ref _vertexShader, null);
            Gorgon2DShader<GorgonPixelShader> deferredShader = Interlocked.Exchange(ref _deferredShader, null);
            Gorgon2DShader<GorgonPixelShader> lightShader = Interlocked.Exchange(ref _lightShader, null);
            GorgonConstantBufferView screenSizeData = Interlocked.Exchange(ref _screenSizeData, null);
            GorgonConstantBufferView lightData = Interlocked.Exchange(ref _lightData, null);
            GorgonTexture2D texture = Interlocked.Exchange(ref _gbuffer, null);
            GorgonRenderTargetView[] targets = Interlocked.Exchange(ref _gbufferTargets, null);
            GorgonTexture2DView textureView = Interlocked.Exchange(ref _gbufferTexture, null);
            
            textureView?.Dispose();

            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i]?.Dispose();
            }

            texture?.Dispose();
            lightData?.Dispose();
            screenSizeData?.Dispose();
            lightShader?.Dispose();
            deferredShader?.Dispose();
            vertexShader?.Dispose();
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
        {
            switch (passIndex)
            {
                case 0:
                    return _deferredState;
                default:
                    return _lightingState;
            }
        }

        /// <summary>
        /// Function called prior to rendering a pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <returns>A <see cref="T:Gorgon.Renderers.PassContinuationState" /> to instruct the effect on how to proceed.</returns>
        /// <seealso cref="T:Gorgon.Renderers.PassContinuationState" />
        /// <remarks>Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.</remarks>
        protected override PassContinuationState OnBeforeRenderPass(int passIndex)
        {
            Debug.Assert(passIndex >= 0, "Illegal pass index.");

            int lightIndex = passIndex - 1;
            if ((Lights.Count == 0) || (lightIndex >= Lights.Count))
            {
                return PassContinuationState.Stop;
            }

            switch (passIndex)
            {
                case 0:
                    _gbufferTargets[0].Clear(GorgonColor.BlackTransparent);
                    _gbufferTargets[1].Clear(GorgonColor.BlackTransparent);
                    _gbufferTargets[2].Clear(_normalClearColor);
                    Graphics.SetRenderTargets(_gbufferTargets, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
                case 1:
                    Graphics.SetRenderTarget(_finalOutput, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
                default:
                    return Lights[lightIndex ] != null ? PassContinuationState.Continue : PassContinuationState.Skip;
            }
        }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="batchState">The current batch state for the pass.</param>
        /// <param name="camera">The current camera to use when rendering.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
        {
            Renderer.Begin(batchState, camera);

            if (passIndex == 0)
            {
                _renderAction();
            }
            else
            {
                // The actual lighting will be done in this method.
                RenderToOutput(passIndex - 1);
            }

            Renderer.End();
        }

        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
            _screenSizeData = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                            new GorgonConstantBufferInfo("Deferred Lighting Screen Size Buffer")
                                                                            {
                                                                                SizeInBytes = DX.Vector4.SizeInBytes
                                                                            });
            _lightData = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                       new GorgonConstantBufferInfo("Deferred Lighting Light Data Buffer")
                                                                       {
                                                                           SizeInBytes = Unsafe.SizeOf<PointLightData>()
                                                                       });

            _vertexShader = VertexShaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics,
                                                                                                       Resources.Lighting,
                                                                                                       "GorgonVertexLightingShader",
                                                                                                       GorgonGraphics.IsDebugEnabled))
                                               .Build();

            Macros.Add(new GorgonShaderMacro("DEFERRED_LIGHTING"));
            _deferredShader = PixelShaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                                       Resources.Lighting,
                                                                                                       "GorgonPixelShaderDeferred",
                                                                                                       GorgonGraphics.IsDebugEnabled,
                                                                                                       Macros))
                                                .Build();

            Macros.Clear();
            Macros.Add(new GorgonShaderMacro("LIGHTS"));

            _lightShader = PixelShaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                                    Resources.Lighting,
                                                                                                    "GorgonPixelShaderLighting",
                                                                                                    GorgonGraphics.IsDebugEnabled,
                                                                                                    Macros))
                                             .ConstantBuffer(_screenSizeData, 1)
                                             .ConstantBuffer(_lightData, 2)
                                             .Build();

            // Rebuild our states for the new pixel shaders.
            _lightingState = BatchStateBuilder.PixelShader(_lightShader)
                                                .BlendState(GorgonBlendState.Additive)
                                                .Build();

            var blendBuilder = new GorgonBlendStateBuilder();

            _deferredState = BatchStateBuilder.PixelShader(_deferredShader)
                                              .VertexShader(_vertexShader)
                                              .BlendState(blendBuilder.DestinationBlend(alpha: Blend.InverseSourceAlpha))
                                              .Build();
        }

        /// <summary>
        /// Function to render the effect.
        /// </summary>
        /// <param name="renderAction">The action used to render the scene.</param>
        /// <param name="output">The render target view that will receive the final rendered output.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderAction"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="renderAction"/> is a method that users can define to draw whatever objects that need to be lit.  When this method is called, the <see cref="Gorgon2D.Begin"/> and
        /// <see cref="Gorgon2D.End"/> methods are already taken care of by the effect and will not need to be called during the callback.
        /// </para>
        /// <para>
        /// When rendering, the texture used by the objects being rendered must be a 3 level array texture, where the first layer contains the diffuse texture, the second contains the specular texture and
        /// the third contains the normal map texture.  By doing this, only a single texture, instead of 3, needs to be bound to the GPU. This will provide a performance increase. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void RenderEffect(Action renderAction, GorgonRenderTargetView output, Gorgon2DCamera camera = null)
        {
            renderAction.ValidateObject(nameof(renderAction));
            output.ValidateObject(nameof(output));

            // If the output has changed size since last render, then we need to adjust our render target(s) and shader data.
            CheckForOutputSizeChange(output.Width, output.Height);

            _finalOutput = output;
            _renderAction = renderAction;

            Render(camera: camera);

            _renderAction = null;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DDeferredLightingEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with the effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public Gorgon2DDeferredLightingEffect(Gorgon2D renderer)
            : base(renderer, "Deferred Lighting", "Renders a scene with deferred per-pixel lighting.", MaxLightCount + 1)
        {

        }
        #endregion
    }
}
