
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, April 05, 2012 8:23:51 AM
// 


using System.Numerics;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// An effect that renders the edges of an image with Sobel edge detection
/// </summary>
public class Gorgon2DSobelEdgeDetectEffect
    : Gorgon2DEffect, IGorgon2DCompositorEffect
{

    /// <summary>
    /// Settings for the effect shader.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Settings"/> struct.
    /// </remarks>
    /// <param name="linecolor">The linecolor.</param>
    /// <param name="texelSize">Size of the texel.</param>
    /// <param name="threshold">The threshold.</param>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    private readonly struct Settings(GorgonColor linecolor, Vector2 texelSize, float threshold)
    {
        // Texel size and threshold.
        private readonly Vector4 _texelThreshold = new(texelSize, threshold, 0);

        /// <summary>
        /// Line color.
        /// </summary>
        public readonly GorgonColor LineColor = linecolor;

        /// <summary>
        /// Property to return the size of a texel.
        /// </summary>
        public Vector2 TexelSize => new(_texelThreshold.X, _texelThreshold.Y);

        /// <summary>
        /// Property to return the threshold for the effect.
        /// </summary>
        public float Threshold => _texelThreshold.Z;
    }



    // Buffer for the sobel edge detection.
    private GorgonConstantBufferView _sobelBuffer;
    // The pixel shader for the effect.
    private GorgonPixelShader _sobelShader;
    private Gorgon2DShaderState<GorgonPixelShader> _sobelState;
    // The batch state used for the effect.
    private Gorgon2DBatchState _batchState;
    // Settings for the effect.
    private Settings _settings;
    // Flag to indicate that the parameters have been updated.
    private bool _isUpdated = true;
    // The thickness of the lines.
    private float _lineThickness = 1.0f;
    // The texture size used to calculate the line thickness.
    private DX.Size2F _textureSize = new(512.0f, 512.0f);



    /// <summary>
    /// Property to set or return the offset of the shapren/embossing edges.
    /// </summary>
    public DX.Size2F TextureSize
    {
        get => _textureSize;
        set
        {
            if (_textureSize == value)
            {
                return;
            }

            _textureSize = value;
            _isUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the relative thickness of the line.
    /// </summary>
    public float LineThickness
    {
        get => _lineThickness;
        set
        {
            if (_lineThickness == value)
            {
                return;
            }

            _settings = new Settings(_settings.LineColor,
                                     new Vector2(_settings.TexelSize.X * value, _settings.TexelSize.Y * value),
                                     _settings.Threshold);
            _lineThickness = value;
            _isUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the threshold value for the edges.
    /// </summary>
    public float EdgeThreshold
    {
        get => _settings.Threshold;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_settings.Threshold == value)
            {
                return;
            }

            if (value < 0)
            {
                value = 0.0f;
            }
            if (value > 1.0f)
            {
                value = 1.0f;
            }

            _settings = new Settings(_settings.LineColor, _settings.TexelSize, value);
            _isUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the color for the edges.
    /// </summary>
    public GorgonColor LineColor
    {
        get => _settings.LineColor;
        set
        {
            if (_settings.LineColor.Equals(value))
            {
                return;
            }

            _settings = new Settings(value, _settings.TexelSize, _settings.Threshold);
            _isUpdated = true;
        }
    }



    /// <summary>
    /// Function called when the effect is being initialized.
    /// </summary>
    /// <remarks>
    /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
    /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
    /// </remarks>
    protected override void OnInitialize()
    {
        _sobelBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, in _settings, "Gogron 2D Sobel Edge Detect Filter Effect Constant Buffer");
        _sobelShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSobelEdge");
    }

    /// <summary>
    /// Function called to build a new (or return an existing) 2D batch state.
    /// </summary>
    /// <param name="passIndex">The index of the current rendering pass.</param>
    /// <param name="builders">The builder types that will manage the state of the effect.</param>
    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
    /// <returns>The 2D batch state.</returns>
    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
    {
        if ((_batchState is null) || (statesChanged))
        {
            _sobelState ??= builders.PixelShaderBuilder
                          .ConstantBuffer(_sobelBuffer, 1)
                          .Shader(_sobelShader)
                          .Build();

            _batchState = builders.BatchBuilder
                          .PixelShaderState(_sobelState)
                          .Build(BatchStateAllocator);
        }

        return _batchState;
    }

    /// <summary>
    /// Function called prior to rendering.
    /// </summary>
    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
    /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
    /// <remarks>
    /// <para>
    /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
    /// targets (if applicable).
    /// </para>
    /// </remarks>
    protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
    {
        if (!_isUpdated)
        {
            return;
        }

        _settings = new Settings(_settings.LineColor, new Vector2((1.0f / _textureSize.Width) * LineThickness, (1.0f / _textureSize.Height) * LineThickness), _settings.Threshold);
        _sobelBuffer.Buffer.SetData(in _settings);
        _isUpdated = false;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _sobelBuffer, null);
        GorgonPixelShader shader = Interlocked.Exchange(ref _sobelShader, null);

        buffer?.Dispose();
        shader?.Dispose();
    }

    /// <summary>
    /// Function to begin rendering the effect.
    /// </summary>
    /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
    /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
    /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
    /// <param name="camera">[Optional] The camera to use when rendering.</param>
    public void Begin(GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, GorgonCameraCommon camera = null)
    {
        GorgonRenderTargetView target = Graphics.RenderTargets[0];

        if (target is null)
        {
            return;
        }

        BeginRender(target, blendState, depthStencilState, rasterState);
        BeginPass(0, target, camera);
    }

    /// <summary>
    /// Function to end the effect rendering.
    /// </summary>
    public void End()
    {
        GorgonRenderTargetView target = Graphics.RenderTargets[0];

        if (target is null)
        {
            return;
        }

        EndPass(0, target);
        EndRender(target);
    }

    /// <summary>
    /// Function to render an effect under the <see cref="Gorgon2DCompositor"/>.
    /// </summary>
    /// <param name="texture">The texture to render into the next target.</param>
    /// <param name="output">The render target that will receive the final output.</param>
    public void Render(GorgonTexture2DView texture, GorgonRenderTargetView output)
    {
        if ((texture is null) || (output is null))
        {
            return;
        }

        Graphics.SetRenderTarget(output);

        Begin(GorgonBlendState.Default, GorgonDepthStencilState.Default, GorgonRasterState.Default, null);

        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                        GorgonColors.White,
                                        texture,
                                        new DX.RectangleF(0, 0, 1, 1));
        End();
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="Gorgon2DSobelEdgeDetectEffect"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used to render this effect.</param>
    public Gorgon2DSobelEdgeDetectEffect(Gorgon2D renderer)
        : base(renderer, Resources.GOR2D_EFFECT_SOBEL, Resources.GOR2D_EFFECT_SOBEL_DESC, 1)
    {
        _settings = new Settings(Color.Black, Vector2.Zero, 0.75f);
        Macros.Add(new GorgonShaderMacro("SOBEL_EDGE_EFFECT"));
    }

}
