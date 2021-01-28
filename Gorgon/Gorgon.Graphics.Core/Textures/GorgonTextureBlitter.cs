#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 6, 2017 10:47:10 PM
// 
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Geometry;
using DX = SharpDX;

namespace Gorgon.Graphics
{
    /// <summary>
    /// Provides functionality for blitting a texture to the currently active <see cref="GorgonGraphics.RenderTargets">render target</see>.
    /// </summary>
    public class GorgonTextureBlitter
        : IDisposable
    {
        #region Variables.
        // The graphics interface that owns this instance.
        private readonly GorgonGraphics _graphics;
        // The vertices used to blit the texture.
        private readonly GorgonVertexPosColorUv[] _vertices = new GorgonVertexPosColorUv[4];
        // The default vertex shader for blitting the texture.
        private GorgonVertexShader _blitVertexShader;
        // The default pixel shader for blitting the texture.
        private GorgonPixelShader _blitPixelShader;
        // The default vertex shader for blitting the texture using a full screen triangle.
        private GorgonVertexShader _fsVertexShader;
        // The default pixel shader for blitting the texture using a full screen triangle.
        private GorgonPixelShader _fsPixelShader;
        // The input layout for the blit vertices.
        private GorgonInputLayout _inputLayout;
        // The bindings for the vertex buffer.
        private GorgonVertexBufferBindings _vertexBufferBindings;
        // World/view/projection matrix.
        private GorgonConstantBufferView _wvpBuffer;
        // Flag used to determine if the blitter is initialized or not.
        private int _initializedFlag;
        // The draw call used to blit the texture.
        private GorgonDrawCall _blitDraw;
        // The draw call used to blit the full screen texture.
        private GorgonDrawCall _fullScreenDraw;
        // The builder used to create a draw call for blitting.
        private readonly GorgonDrawCallBuilder _blitBuilder = new GorgonDrawCallBuilder();
        // The builder used to create a draw call for full screen rendering.
        private readonly GorgonDrawCallBuilder _fsBuilder = new GorgonDrawCallBuilder();
        // The builder used to create a pipeline state object.
        private readonly GorgonPipelineStateBuilder _blitPsoBuilder;
        // The builder used to create a pipeline state object.
        private readonly GorgonPipelineStateBuilder _fsPsoBuilder;
        // The allocator used to create/recycle draw calls.
        private readonly GorgonDrawCallPoolAllocator<GorgonDrawCall> _drawAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawCall>(128);
        // The default pipeline state for blitting.
        private GorgonPipelineState _blitPso;
        // The default pipeline state for fullscreen blitting.
        private GorgonPipelineState _fsPso;
        // Constant buffers for the pixel shader
        private readonly GorgonConstantBuffers _emptyPsConstants = new GorgonConstantBuffers();
        // The default texture.
        private GorgonTexture2DView _defaultTexture;
        // The camera space for blitting the texture.
        private readonly GorgonOrthoCamera _camera;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the blitter.
        /// </summary>
        private void Initialize()
        {
            try
            {
                // We've been initialized, so leave.
                if ((_blitVertexShader != null) || (Interlocked.Increment(ref _initializedFlag) > 1))
                {
                    // Trap other threads until we're done initializing and then release them.
                    while ((_blitVertexShader == null) && (_initializedFlag > 0))
                    {
                        var wait = new SpinWait();
                        wait.SpinOnce();
                    }

                    return;
                }


                _blitVertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics,
                                                                                Resources.GraphicsShaders,
                                                                                "GorgonBltVertexShader",
                                                                                GorgonGraphics.IsDebugEnabled);
                _blitPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics,
                                                                              Resources.GraphicsShaders,
                                                                              "GorgonBltPixelShader",
                                                                              GorgonGraphics.IsDebugEnabled);

                _fsVertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics,
                                                                                  Resources.GraphicsShaders,
                                                                                  "GorgonFullScreenVertexShader",
                                                                                  GorgonGraphics.IsDebugEnabled);

                _fsPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics,
                                                                                Resources.GraphicsShaders,
                                                                                "GorgonFullScreenPixelShader",
                                                                                GorgonGraphics.IsDebugEnabled);

                _inputLayout = GorgonInputLayout.CreateUsingType<GorgonVertexPosColorUv>(_graphics, _blitVertexShader);

                _vertexBufferBindings = new GorgonVertexBufferBindings(_inputLayout)
                {
                    [0] = GorgonVertexBufferBinding.CreateVertexBuffer<GorgonVertexPosColorUv>(_graphics, new GorgonVertexBufferInfo("Gorgon Blitter Vertex Buffer")
                    {
                        Binding = VertexIndexBufferBinding.None,
                        SizeInBytes = GorgonVertexPosColorUv.SizeInBytes * 4,
                        Usage = ResourceUsage.Dynamic
                    })
                };

                _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                           new GorgonConstantBufferInfo("Gorgon Blitter WVP Buffer")
                                                                           {
                                                                               Usage = ResourceUsage.Dynamic,
                                                                               SizeInBytes = Unsafe.SizeOf<DX.Matrix>()
                                                                           });


                _fsPso = _fsPsoBuilder.PixelShader(_fsPixelShader)
                                      .VertexShader(_fsVertexShader)
                                      .BlendState(GorgonBlendState.NoBlending)
                                      .Build();

                _fsBuilder.PipelineState(_fsPso)
                          .VertexRange(0, 3)
                          .SamplerState(ShaderType.Pixel, GorgonSamplerState.PointFiltering);

                
                _blitPso = _blitPsoBuilder.VertexShader(_blitVertexShader)
                                          .BlendState(GorgonBlendState.NoBlending)
                                          .DepthStencilState(GorgonDepthStencilState.Default)
                                          .PrimitiveType(PrimitiveType.TriangleStrip)
                                          .RasterState(GorgonRasterState.Default)
                                          .PixelShader(_blitPixelShader)
                                          .Build();

                _blitBuilder.VertexBuffers(_inputLayout, _vertexBufferBindings)
                            .VertexRange(0, 4)
                            .SamplerState(ShaderType.Pixel, GorgonSamplerState.Default)
                            .PipelineState(_blitPso)
                            .ConstantBuffer(ShaderType.Vertex, _wvpBuffer);


                _defaultTexture = Resources.White_2x2.ToTexture2D(_graphics,
                                                                  new GorgonTexture2DLoadOptions
                                                                  {
                                                                      Name = "Gorgon_Default_White_Texture",
                                                                      Usage = ResourceUsage.Immutable,
                                                                      Binding = TextureBinding.ShaderResource
                                                                  }).GetShaderResourceView();

                _graphics.RenderTargetChanged += Graphics_RenderTargetChanged;
                _graphics.ViewportChanged += Graphics_RenderTargetChanged;
            }
            finally
            {
                Interlocked.Decrement(ref _initializedFlag);
            }
        }

        /// <summary>Handles the RenderTargetChanged event of the Graphics control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Graphics_RenderTargetChanged(object sender, EventArgs e) => UpdateProjection();

        /// <summary>
        /// Function to update the projection data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateProjection()
        {
            if (_graphics.RenderTargets[0] == null)            
            {
                return;
            }

            var targetSize = new DX.Size2F(_graphics.RenderTargets[0].Width, _graphics.RenderTargets[0].Height);

            if ((_camera.Changes == CameraChange.None) && (targetSize.Equals(_camera.ViewDimensions)))
            {
                return;
            }

            _camera.ViewDimensions = targetSize;
            ref DX.Matrix projectionMatrix = ref _camera.GetProjectionMatrix();
            _wvpBuffer.Buffer.SetData(ref projectionMatrix);

            // Since we only care about the projection matrix, we can disregard the view changes so we don't continuously repeat the projection calculations.
            _camera.DiscardChanges();            
        }

        /// <summary>
        /// Function to return the draw call used to render a full screen triangle.
        /// </summary>
        /// <param name="texture">The texture to render onto the triangle.</param>
        /// <param name="blendState">The blending state to apply.</param>
        /// <param name="samplerState">The sampler state to apply.</param>
        private void GetFullScreenDrawCall(GorgonTexture2DView texture, GorgonBlendState blendState, GorgonSamplerState samplerState)
        {
            if ((_fullScreenDraw != null) && (_fullScreenDraw.PixelShader.ShaderResources[0] == texture)
                && (blendState == _fsPso.BlendStates[0])
                && (samplerState == _fullScreenDraw.PixelShader.Samplers[0]))
            {
                return;
            }

            if (blendState != _fsPso.BlendStates[0])
            {
                _fsPso = _fsPsoBuilder.BlendState(blendState)
                                      .Build();
                _fsBuilder.PipelineState(_fsPso);
            }

            _fullScreenDraw = _fsBuilder.ShaderResource(ShaderType.Pixel, texture)
                                        .SamplerState(ShaderType.Pixel, samplerState)
                                        .Build(_drawAllocator);                      
        }

        /// <summary>
        /// Function to return the appropriate draw call based on the states provided.
        /// </summary>
        /// <param name="texture">The texture to display.</param>
        /// <param name="blendState">The blending state for the texture.</param>
        /// <param name="samplerState">The sampler state for the texture.</param>
        /// <param name="shader">The pixel shader to use.</param>
        /// <param name="constantBuffers">Constant buffers for the pixel shader, if required.</param>
	    private void GetDrawCall(GorgonTexture2DView texture, GorgonBlendState blendState, GorgonSamplerState samplerState, GorgonPixelShader shader, GorgonConstantBuffers constantBuffers)
        {
            if ((_blitDraw != null)
                && (shader == _blitPixelShader)
                && (_blitDraw.PixelShader.Samplers[0] == samplerState)
                && (_blitPso.BlendStates[0] == blendState)
                && (_blitDraw.PixelShader.ShaderResources[0] == texture)
                && ((constantBuffers == _emptyPsConstants)
                    || (_blitDraw.PixelShader.ConstantBuffers.DirtyEquals(constantBuffers))))
            {
                // This draw call hasn't changed, so return the previous one.
                return;
            }

            if (_blitPso.BlendStates[0] != blendState)
            {
                _blitPso = _blitPsoBuilder
                                 .BlendState(blendState)
                                 .Build();

                _blitBuilder.PipelineState(_blitPso);
            }

            _blitDraw = _blitBuilder.ConstantBuffers(ShaderType.Pixel, constantBuffers)
                                    .SamplerState(ShaderType.Pixel, samplerState)
                                    .ShaderResource(ShaderType.Pixel, texture)                                        
                                    .Build(_drawAllocator);
        }

        /// <summary>
        /// Function to blit a texture to the full size of the current view.
        /// </summary>
        /// <param name="texture">The texture to blit.</param>
        /// <param name="blendState">[Optional] The blending state to apply.</param>
        /// <param name="samplerState">[Optiona] The sampler state to apply.</param>
        /// <remarks>
        /// <para>
        /// This method blits the texture to the full size of the current view. It does this by rendering a single triangle that is large enough to encompass the entire view and calculating the texture 
        /// coordinates on the triangle so that the texture is displayed correctly. This is faster than blitting a quad because there's only a single triangle to render, thus the fill time is cut down.
        /// </para>
        /// <para>
        /// If the <paramref name="blendState"/> is not used, then the system will default to no blending.
        /// </para>
        /// <para>
        /// If the <paramref name="samplerState"/> is not used, then the system will default to point sampling.
        /// </para>
        /// </remarks>
        public void BlitFullScreen(GorgonTexture2DView texture, GorgonBlendState blendState = null, GorgonSamplerState samplerState = null)
        {
            // If we haven't initialized yet, then do so now.
            if (_initializedFlag == 0)
            {
                Initialize();
            }

            if (_graphics.RenderTargets[0] == null)
            {
                return;
            }

            if (texture == null)
            {
                texture = _defaultTexture;
            }

            if (blendState == null)
            {
                blendState = GorgonBlendState.NoBlending;
            }

            if (samplerState == null)
            {
                samplerState = GorgonSamplerState.PointFiltering;
            }

            GetFullScreenDrawCall(texture, blendState, samplerState);

            _graphics.Submit(_fullScreenDraw);
        }

        /// <summary>
        /// Function to draw a texture to the current render target.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="destination">The location on the target to draw into.</param>
        /// <param name="color">[Optional] The color to apply to the texture when drawing.</param>
        /// <param name="blendState">[Optional] The type of blending to perform.</param>
        /// <param name="samplerState">[Optional] The sampler state used to define how to sample the texture.</param>
        /// <param name="pixelShader">[Optional] A pixel shader used to apply effects to the texture.</param>
        /// <param name="psConstantBuffers">[Optional] A list of constant buffers for the pixel shader if they're required.</param>
        /// <remarks>
        /// <para>
        /// This is a utility method used to draw a (2D) texture to the current render target.  This is handy for quick testing to ensure things are working as they should. 
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This method, while quite handy, should not be used for performance sensitive work as it is not the most optimal means of displaying texture data.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2DView"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Blit(GorgonTexture2DView texture,
                                DX.Point destination,
                                in GorgonColor? color = null,
                                GorgonBlendState blendState = null,
                                GorgonSamplerState samplerState = null,
                                GorgonPixelShader pixelShader = null,
                                GorgonConstantBuffers psConstantBuffers = null)
            => Blit(texture,
                    new DX.Rectangle(destination.X, destination.Y, texture?.Width ?? 1, texture?.Height ?? 1),
                    null,
                    color,
                    blendState,
                    samplerState,
                    pixelShader,
                    psConstantBuffers);

        /// <summary>
        /// Function to blit the texture to the specified render target.
        /// </summary>
        /// <param name="texture">The texture that will be blitted to the render target.</param>
        /// <param name="destRect">The layout area to blit the texture into.</param>
        /// <param name="sourceRegion">The region on the texture to start blitting from.</param>
        /// <param name="color">The color used to tint the diffuse value of the texture.</param>
        /// <param name="blendState">The blending state to apply.</param>
        /// <param name="samplerState">The sampler state to apply.</param>
        /// <param name="pixelShader">The pixel shader used to override the default pixel shader.</param>
        /// <param name="pixelShaderConstants">The pixel shader constant buffers to use.</param>
        public void Blit(GorgonTexture2DView texture,
                         DX.Rectangle destRect,
                         in DX.Rectangle? sourceRegion = null,
                         in GorgonColor? color = null,
                         GorgonBlendState blendState = null,
                         GorgonSamplerState samplerState = null,
                         GorgonPixelShader pixelShader = null,
                         GorgonConstantBuffers pixelShaderConstants = null)
        {
            // If we haven't initialized yet, then do so now.
            if (_initializedFlag == 0)
            {
                Initialize();
            }

            GorgonColor actualColor = color ?? GorgonColor.White;

            if ((_graphics.RenderTargets[0] == null)
                || (actualColor.Alpha.EqualsEpsilon(0)))
            {
                return;
            }

            if (texture == null)
            {
                texture = _defaultTexture;
            }

            UpdateProjection();

            // Set to default states if not provided.
            if (blendState == null)
            {
                blendState = GorgonBlendState.NoBlending;
            }

            if (pixelShader == null)
            {
                pixelShader = _blitPixelShader;
            }

            if (samplerState == null)
            {
                samplerState = GorgonSamplerState.Default;
            }

            if (pixelShaderConstants == null)
            {
                pixelShaderConstants = _emptyPsConstants;
            }
            
            GetDrawCall(texture, blendState, samplerState, pixelShader, pixelShaderConstants);

            // Calculate position on the texture.
            DX.RectangleF region = texture.Texture.ToTexel(sourceRegion ?? new DX.Rectangle(0, 0, texture.Width, texture.Height));

            // Update the vertices.
            _vertices[0] = new GorgonVertexPosColorUv
            {
                Position = new DX.Vector4(destRect.X, destRect.Y, 0, 1.0f),
                UV = new DX.Vector2(region.Left, region.Top),
                Color = actualColor
            };
            _vertices[1] = new GorgonVertexPosColorUv
            {
                Position = new DX.Vector4(destRect.Right, destRect.Y, 0, 1.0f),
                UV = new DX.Vector2(region.Right, region.Top),
                Color = actualColor
            };
            _vertices[2] = new GorgonVertexPosColorUv
            {
                Position = new DX.Vector4(destRect.X, destRect.Bottom, 0, 1.0f),
                UV = new DX.Vector2(region.Left, region.Bottom),
                Color = actualColor
            };
            _vertices[3] = new GorgonVertexPosColorUv
            {
                Position = new DX.Vector4(destRect.Right, destRect.Bottom, 0, 1.0f),
                UV = new DX.Vector2(region.Right, region.Bottom),
                Color = actualColor
            };

            // Copy to the vertex buffer.
            _vertexBufferBindings[0].VertexBuffer.SetData<GorgonVertexPosColorUv>(_vertices);
            _graphics.Submit(_blitDraw);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _graphics.RenderTargetChanged -= Graphics_RenderTargetChanged;
            _graphics.ViewportChanged -= Graphics_RenderTargetChanged;

            _defaultTexture?.Texture?.Dispose();
            _wvpBuffer?.Dispose();
            _vertexBufferBindings?[0].VertexBuffer?.Dispose();
            _inputLayout?.Dispose();
            _fsPixelShader?.Dispose();
            _fsVertexShader?.Dispose();
            _blitVertexShader?.Dispose();
            _blitPixelShader?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTextureBlitter"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the required objects for blitting.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public GorgonTextureBlitter(GorgonGraphics graphics)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _blitPsoBuilder = new GorgonPipelineStateBuilder(_graphics);
            _fsPsoBuilder = new GorgonPipelineStateBuilder(_graphics);
            _camera = new GorgonOrthoCamera(_graphics, DX.Size2F.Empty, name: "Blitter camera");
        }
        #endregion
    }
}
