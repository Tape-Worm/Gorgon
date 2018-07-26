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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Graphics;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;

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
            /// Various attributes (x = Specular enabled, y = Specular power, z = Attenuation/Intensity).
            /// </summary>
            public DX.Vector4 LightAttribs;
            /// <summary>
            /// The direction for the light.
            /// </summary>
            public DX.Vector3 LightDirection;
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
        private Gorgon2DShader<GorgonPixelShader> _pointLightShader;
        // The actual pixel shader for a point light.
        private GorgonPixelShader _pointLightPixelShader;
        // The constant buffer that will send the screen size on update.
        private GorgonConstantBufferView _screenSizeData;
        // The buffer that will hold the lighting data.
        private GorgonConstantBufferView _lightData;
        // The last known screen size.
        private DX.Vector4 _screenSize;
        // The action used to render the scene data to the various buffers.
        private Action _renderAction;
        // The final output target.
        private GorgonRenderTargetView _finalOutput;
        // The render targets for the deferred channels.
        private GorgonRenderTargetView[] _targets = new GorgonRenderTargetView[3];
        // The texture used to draw the normal map render target.
        private GorgonTexture2DView _normalTexture;
        // The texture used to draw the specular highlighting.
        private GorgonTexture2DView _specularTexture;
        // The texture used to draw the diffuse data.
        private GorgonTexture2DView _diffuseTexture;
        // The batch render state for drawing to the deferred targets.
        private Gorgon2DBatchState _deferredState;
        // The batch render state for drawing with point lights.
        private Gorgon2DBatchState _pointLightState;
        // The color used to clear the normal map render target.
        private readonly GorgonColor _normalClearColor = new GorgonColor(127.0f / 255.0f, 127.0f / 255.0f, 254.0f / 255.0f, 1.0f);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of point lights for rendering.
        /// </summary>
        public Gorgon2DPointLight[] PointLights
        {
            get;
        } = new Gorgon2DPointLight[MaxLightCount];
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the render targets.
        /// </summary>
        private void BuildRenderTargets()
        {
            GorgonRenderTarget2DView target;

            _diffuseTexture?.Dispose();
            _specularTexture?.Dispose();
            _normalTexture?.Dispose();

            _targets[0] = target = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo("Gorgon 2D Deferred Lighting Diffuse Target")
                                                                                {
                                                                                    Width = _finalOutput.Width,
                                                                                    Height = _finalOutput.Height,
                                                                                    Format = BufferFormat.R8G8B8A8_UNorm,
                                                                                    Binding = TextureBinding.ShaderResource
                                                                                });
            _diffuseTexture = target.Texture.GetShaderResourceView();
            _targets[1] = target = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(target, "Gorgon 2D Deferred Lighting Specular Target"));
            _specularTexture = target.Texture.GetShaderResourceView();
            _targets[2] = target = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(target, "Gorgon 2D Deferred Lighting Normal Target"));
            _normalTexture = target.Texture.GetShaderResourceView();

            
            _pointLightShader = PixelShaderBuilder.Shader(_pointLightPixelShader)
                                                  .ConstantBuffer(_screenSizeData, 1)
                                                  .ConstantBuffer(_lightData, 2)
                                                  .ShaderResource(_normalTexture, 1)
                                                  .ShaderResource(_specularTexture, 2)
                                                  .Build();

            _pointLightState = BatchStateBuilder.PixelShader(_pointLightShader)
                                                .BlendState(GorgonBlendState.Additive)
                                                .Build();
        }

        /// <summary>
        /// Function to adjust the targets and screen size buffer when the render target changes size.
        /// </summary>
        private void AdjustForScreenSizeChange()
        {
            // The size is the same, we don't need to do anything.
            if ((_screenSize.X.EqualsEpsilon(_finalOutput.Width))
                && (_screenSize.Y.EqualsEpsilon(_finalOutput.Height)))
            {
                return;
            }

            BuildRenderTargets();

            _screenSize = new DX.Vector4(_finalOutput.Width, _finalOutput.Height, 0, 0);
            _screenSizeData.Buffer.SetData(ref _screenSize);
        }

        private void UpdateLights(int lightIndex)
        {
            Gorgon2DPointLight light = PointLights[lightIndex];

            if (light == null)
            {
                return;
            }

            var lightData = new PointLightData
                            {
                                Position = new DX.Vector4(light.Position, 1.0f),
                                Color = light.Color,
                                LightDirection = light.LightDirection,
                                LightAttribs = new DX.Vector4(light.SpecularEnabled ? -1.0f : 0.0f, light.SpecularPower, light.Attenuation, 0)
                            };

            _lightData.Buffer.SetData(ref lightData);
        }

        /// <summary>
        /// Function to send the gbuffer to the final output target.
        /// </summary>
        /// <param name="lightIndex">The index of the light to render.</param>
        private void RenderToOutput(int lightIndex)
        {
            UpdateLights(lightIndex);

            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _finalOutput.Width, _finalOutput.Height),
                                         GorgonColor.White,
                                         _diffuseTexture,
                                         new DX.RectangleF(0, 0, 1, 1));
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Gorgon2DShader<GorgonVertexShader> vertexShader = Interlocked.Exchange(ref _vertexShader, null);
            Gorgon2DShader<GorgonPixelShader> deferredShader = Interlocked.Exchange(ref _deferredShader, null);
            Gorgon2DShader<GorgonPixelShader> pointLightShader = Interlocked.Exchange(ref _pointLightShader, null);
            GorgonConstantBufferView screenSizeData = Interlocked.Exchange(ref _screenSizeData, null);
            GorgonConstantBufferView lightData = Interlocked.Exchange(ref _lightData, null);
            GorgonRenderTargetView[] targets = Interlocked.Exchange(ref _targets, null);
            GorgonTexture2DView normalTexture = Interlocked.Exchange(ref _normalTexture, null);
            GorgonTexture2DView specularTexture = Interlocked.Exchange(ref _specularTexture, null);
            GorgonTexture2DView diffuseTexture = Interlocked.Exchange(ref _diffuseTexture, null);

            diffuseTexture?.Dispose();
            specularTexture?.Dispose();
            normalTexture?.Dispose();

            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i]?.Dispose();
            }

            lightData?.Dispose();
            screenSizeData?.Dispose();
            pointLightShader?.Dispose();
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
                    return _pointLightState;
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
            if (PointLights[0] == null)
            {
                return PassContinuationState.Stop;
            }

            switch (passIndex)
            {
                case 0:
                    _targets[0].Clear(GorgonColor.BlackTransparent);
                    _targets[1].Clear(GorgonColor.BlackTransparent);
                    _targets[2].Clear(_normalClearColor);
                    Graphics.SetRenderTargets(_targets, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
                case 1:
                    Graphics.SetRenderTarget(_finalOutput, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
                default:
                    return PointLights[passIndex - 1] != null ? PassContinuationState.Continue : PassContinuationState.Stop;
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
            _screenSizeData = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Deferred Lighting Screen Size Buffer")
                                                                                      {
                                                                                          SizeInBytes = DX.Vector4.SizeInBytes
                                                                                      });
            _lightData = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Deferred Lighting Light Data Buffer")
                                                                                 {
                                                                                     SizeInBytes = Unsafe.SizeOf<PointLightData>()
                                                                                 });

            _vertexShader = VertexShaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics,
                                                                                       Resources.Lighting,
                                                                                       "GorgonVertexLightingShader",
                                                                                       GorgonGraphics.IsDebugEnabled))
                                               .Build();

            GorgonShaderMacro[] macro = {
                                            new GorgonShaderMacro("DEFERRED_LIGHTING"),
                                        };
            _deferredShader = PixelShaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                                       Resources.Lighting,
                                                                                                       "GorgonPixelShaderDeferred",
                                                                                                       GorgonGraphics.IsDebugEnabled,
                                                                                                       macro))
                                                .Build();

            macro[0] = new GorgonShaderMacro("POINT_LIGHT");
            
            _pointLightPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                    Resources.Lighting,
                                                                                    "GorgonPixelShaderPointLight",
                                                                                    GorgonGraphics.IsDebugEnabled,
                                                                                    macro);

            _deferredState = BatchStateBuilder.PixelShader(_deferredShader)
                                               .VertexShader(_vertexShader)
                                               .Build();
        }

        public void RenderEffect(Action renderAction, GorgonRenderTargetView output)
        {
            _finalOutput = output;
            _renderAction = renderAction;

            AdjustForScreenSizeChange();

            Render();

            _finalOutput = null;
            _renderAction = null;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DDeferredLightingEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with the effect.</param>
        public Gorgon2DDeferredLightingEffect(Gorgon2D renderer)
            : base(renderer, "Deferred Lighting", "Renders a scene with deferred per-pixel lighting.", MaxLightCount + 1)
        {

        }
        #endregion
    }
}
