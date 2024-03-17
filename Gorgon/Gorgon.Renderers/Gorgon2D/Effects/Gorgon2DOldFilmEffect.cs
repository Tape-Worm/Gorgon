
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, August 22, 2013 11:54:51 PM
// 


using System.Numerics;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// A post process effect to give an old scratched film effect
/// </summary>
/// <remarks>
/// <para>
/// This effect animates an image to appear as though it is being displayed on old film. 
/// </para>
/// </remarks>
public class Gorgon2DOldFilmEffect
    : Gorgon2DEffect, IGorgon2DCompositorEffect
{
    /// <summary>
    /// The name of the shader include for Gorgon's <see cref="Gorgon2DOldFilmEffect"/>.
    /// </summary>
    public const string Gorgon2DFilmGrainIncludeName = "FilmGrainShaders";

    /// <summary>
    /// Settings for scratches.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
    private struct ScratchSettings
    {
        /// <summary>
        /// Speed of a scratch.
        /// </summary>
        public float ScratchVisibleTime;
        /// <summary>
        /// Speed of a scratch scroll.
        /// </summary>
        public float ScratchScrollSpeed;
        /// <summary>
        /// Intensity of a scratch.
        /// </summary>
        public float ScratchIntensity;
        /// <summary>
        /// Width of a scratch.
        /// </summary>
        public float ScratchWidth;
    }

    /// <summary>
    /// Settings for sepia tone.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 48)]
    private struct SepiaSettings
    {
        /// <summary>
        /// Lightest value for sepia.
        /// </summary>
        public GorgonColor SepiaLightColor;
        /// <summary>
        /// Darkest value for sepa.
        /// </summary>
        public GorgonColor SepiaDarkColor;

        /// <summary>
        /// Desaturation of color.
        /// </summary>
        public float SepiaDesaturationAmount;
        /// <summary>
        /// Tone for sepia color.
        /// </summary>
        public float SepiaToneAmount;
    }

    // Texture used to hold random noise for the shader.
    private GorgonTexture2DView _randomTexture;
    // Flag to indicate whether the effect has updated parameters or not.
    private bool _isScratchUpdated = true;
    // Flag to indicate whether the effect has updated parameters or not.
    private bool _isSepiaUpdated = true;
    // Settings for film scratches.
    private ScratchSettings _scratchSettings;
    // Settings for sepia tone.
    private SepiaSettings _sepiaSettings;
    // Constant buffer for scratch settings.
    private GorgonConstantBufferView _scratchBuffer;
    // Constant buffer for sepia settings.
    private GorgonConstantBufferView _sepiaBuffer;
    // Noise frequency.
    private float _noiseFrequency = 42.0f;
    // The shader used for the film effect.
    private GorgonPixelShader _filmShader;
    private Gorgon2DShaderState<GorgonPixelShader> _filmState;
    // The batch state to use when rendering.
    private Gorgon2DBatchState _batchState;

    /// <summary>
    /// Property to set or return the noise frequency used in generating scratches in the film.
    /// </summary>
    /// <remarks>It is not recommended to update this value every frame, doing so may have a significant performance hit.</remarks>
    public float NoiseFrequency
    {
        get => _noiseFrequency;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_noiseFrequency == value)
            {
                return;
            }

            if (value < 1)
            {
                value = 1.0f;
            }

            _noiseFrequency = value;

            // Prepare the random data texture for update.
            if (_randomTexture is null)
            {
                return;
            }

            _randomTexture.Dispose();
            _randomTexture = null;
        }
    }

    /// <summary>
    /// Property to set or return the speed of updates to the current set of scratches.
    /// </summary>
    /// <remarks>Smaller values keep the current scratches on the screen longer.</remarks>
    public float UpdateSpeed
    {
        get => _scratchSettings.ScratchVisibleTime;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_scratchSettings.ScratchVisibleTime == value)
            {
                return;
            }

            _scratchSettings.ScratchVisibleTime = value;
            _isScratchUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the speed for scratches to move horizontally.
    /// </summary>
    public float ScrollSpeed
    {
        get => _scratchSettings.ScratchScrollSpeed;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_scratchSettings.ScratchVisibleTime == value)
            {
                return;
            }

            _scratchSettings.ScratchVisibleTime = value;
            _isScratchUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the itensity multiplier for scratches.
    /// </summary>
    public float Intensity
    {
        get => _scratchSettings.ScratchIntensity;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_scratchSettings.ScratchIntensity == value)
            {
                return;
            }

            _scratchSettings.ScratchIntensity = value;
            _isScratchUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the width multiplier for scratches.
    /// </summary>
    public float ScratchWidth
    {
        get => _scratchSettings.ScratchWidth;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_scratchSettings.ScratchWidth == value)
            {
                return;
            }

            _scratchSettings.ScratchWidth = value;
            _isScratchUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the amount of desaturation for the current scene.
    /// </summary>
    public float DesaturationAmount
    {
        get => _sepiaSettings.SepiaDesaturationAmount;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_sepiaSettings.SepiaDesaturationAmount == value)
            {
                return;
            }

            _sepiaSettings.SepiaDesaturationAmount = value;
            _isSepiaUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the amount of sepia tone for the current scene.
    /// </summary>
    public float ToneAmount
    {
        get => _sepiaSettings.SepiaToneAmount;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_sepiaSettings.SepiaToneAmount == value)
            {
                return;
            }

            _sepiaSettings.SepiaToneAmount = value;
            _isSepiaUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the lightest color for sepia tone mapping.
    /// </summary>
    public GorgonColor SepiaLightColor
    {
        get => _sepiaSettings.SepiaLightColor;
        set
        {
            if (GorgonColor.Equals(in _sepiaSettings.SepiaLightColor, in value))
            {
                return;
            }

            _sepiaSettings.SepiaLightColor = value;
            _isSepiaUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the darkest color for sepia tone mapping.
    /// </summary>
    public GorgonColor SepiaDarkColor
    {
        get => _sepiaSettings.SepiaDarkColor;
        set
        {
            if (GorgonColor.Equals(in _sepiaSettings.SepiaDarkColor, in value))
            {
                return;
            }

            _sepiaSettings.SepiaDarkColor = value;
            _isSepiaUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the percentage that random hair will appear.
    /// </summary>
    public int HairPercent
    {
        get;
        set;
    } = 70;

    /// <summary>
    /// Property to set or return the amount of dirt and dust that will appear.
    /// </summary>
    public int DirtAmount
    {
        get;
        set;
    } = 25;

    /// <summary>
    /// Property to return the noise texture width and height.
    /// </summary>
    /// <remarks>
    /// This determines how often and how many vertical scratch lines appear on the effect. A larger value will yield more scratch lines, while a smaller value will yield less.
    /// </remarks>
    public int NoiseTextureSize
    {
        get;
    }

    /// <summary>
    /// Property to set or return the dirt/hair region to draw in on the output render target.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then the entire output render target region is used to draw the dirt/hair.
    /// </remarks>
    public DX.RectangleF? DirtRegion
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the offset that can be used to simulate shaking.
    /// </summary>
    public Vector2 ShakeOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Funciton to generate random noise for the effect.
    /// </summary>
    private void GenerateRandomNoise()
    {
        int textureSize = NoiseTextureSize.Min(128).Max(16);

        using GorgonImage image = new(new GorgonImageInfo(ImageDataType.Image2D, BufferFormat.R8_UNorm)
        {
            Width = textureSize,
            Height = textureSize
        });
        IGorgonImageBuffer imageBuffer = image.Buffers[0];

        for (int y = 0; y < textureSize; ++y)
        {
            for (int x = 0; x < textureSize; ++x)
            {
                float simplexNoise = GorgonRandom.SimplexNoise(new Vector2(x * (1.0f / _noiseFrequency), y * (1.0f / _noiseFrequency)));

                if (simplexNoise < -0.75f)
                {
                    simplexNoise *= -1;
                }
                else
                {
                    simplexNoise *= 0.95f;
                }

                if (simplexNoise < 0.125f)
                {
                    simplexNoise = 0.0f;
                }


                image.Buffers[0].Data[(y * imageBuffer.PitchInformation.RowPitch) + x] = (byte)(simplexNoise * 255.0f);
            }
        }

        _randomTexture = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo(textureSize, textureSize, BufferFormat.R8_UNorm)
        {
            Name = "Gorgon2D Old Film Effect Random Noise Texture",
            Usage = ResourceUsage.Immutable,
            Binding = TextureBinding.ShaderResource
        }, image);
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
        GenerateRandomNoise();

        _scratchBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, in _scratchSettings, "Gorgon 2D Old Film Effect - Scratch settings");
        _sepiaBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, in _sepiaSettings, "Gorgon 2D Old Film Effect - Sepia settings");

        // Create pixel shader.
        _filmShader = CompileShader<GorgonPixelShader>(Resources.FilmGrain, "GorgonPixelShaderFilmGrain");
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
        if (Graphics.RenderTargets[0] != output)
        {
            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
        }

        if (_isScratchUpdated)
        {
            _scratchBuffer.Buffer.SetData(in _scratchSettings);
            _isScratchUpdated = false;
        }

        if (!_isSepiaUpdated)
        {
            return;
        }

        _sepiaBuffer.Buffer.SetData(in _sepiaSettings);
        _isSepiaUpdated = false;
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
        if (_batchState is null)
        {
            _filmState = builders.PixelShaderBuilder
                          .ConstantBuffer(_scratchBuffer, 1)
                          .ConstantBuffer(_sepiaBuffer, 2)
                          .ShaderResource(_randomTexture, 1)
                          .SamplerState(GorgonSamplerState.Wrapping, 1)
                          .Shader(_filmShader)
                          .Build();

            // Build our state.
            _batchState = builders.BatchBuilder
                          .BlendState(GorgonBlendState.NoBlending)
                          .PixelShaderState(_filmState)
                          .Build();

        }

        return _batchState;
    }

    /// <summary>
    /// Function called after rendering is complete.
    /// </summary>
    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
    /// <remarks>
    /// <para>
    /// Applications can use this to clean up and/or restore any states when rendering is finished. This is an ideal method to copy any rendering imagery to the final output render target.
    /// </para>
    /// </remarks>
    protected override void OnAfterRender(GorgonRenderTargetView output)
    {
        if (Graphics.RenderTargets[0] != output)
        {
            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
        }

        DX.RectangleF region = DirtRegion ?? new DX.RectangleF(0, 0, output.Width, output.Height);

        // If we've specified no region, then don't draw anything.
        if ((region.Width.EqualsEpsilon(0))
            || (region.Height.EqualsEpsilon(0)))
        {
            return;
        }

        Renderer.Begin();

        for (int i = 0; i < DirtAmount; ++i)
        {
            float grayDust = GorgonRandom.RandomSingle(0.1f, 0.25f);
            GorgonColor dustColor = new(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));

            // Render dust points.
            Renderer.DrawFilledRectangle(new DX.RectangleF(GorgonRandom.RandomSingle(region.Left, region.Right),
                                                           GorgonRandom.RandomSingle(region.Top, region.Bottom),
                                                           1,
                                                           1),
                                         dustColor);

            if (GorgonRandom.RandomInt32(100) > HairPercent)
            {
                continue;
            }

            // Render dirt/hair lines.
            Vector2 dirtStart = new(GorgonRandom.RandomSingle(region.Left, region.Right),
                                           GorgonRandom.RandomSingle(region.Top, region.Bottom));

            float dirtWidth = GorgonRandom.RandomSingle(1.0f, 3.0f);
            bool isHair = GorgonRandom.RandomInt32(100) > 50;
            bool isHairVertical = (isHair) && (GorgonRandom.RandomInt32(100) > 50);

            grayDust = GorgonRandom.RandomSingle(0.1f, 0.15f);
            dustColor = new GorgonColor(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));

            for (int j = 0; j < GorgonRandom.RandomInt32(4, (int)(region.Width * 0.10f).Min(4)); j++)
            {
                DX.Size2F size = isHair ? new DX.Size2F(1, 1) : new DX.Size2F(dirtWidth, dirtWidth);
                Renderer.DrawFilledRectangle(new DX.RectangleF(dirtStart.X, dirtStart.Y, size.Width, size.Height), dustColor);

                if ((!isHair) || (isHairVertical))
                {
                    if (GorgonRandom.RandomInt32(100) > 50)
                    {
                        dirtStart.X++;
                    }
                    else
                    {
                        dirtStart.X--;
                    }
                }
                else
                {
                    if (grayDust < 0.25f)
                    {
                        dirtStart.X++;
                    }
                    else
                    {
                        dirtStart.X--;
                    }
                }

                if ((!isHair) || (!isHairVertical))
                {
                    if (GorgonRandom.RandomInt32(100) > 50)
                    {
                        dirtStart.Y++;
                    }
                    else
                    {
                        dirtStart.Y--;
                    }
                }
                else
                {
                    if (dirtWidth < 1.5f)
                    {
                        dirtStart.Y++;
                    }
                    else
                    {
                        dirtStart.Y--;
                    }
                }
            }
        }
        Renderer.End();
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        GorgonPixelShader shader = Interlocked.Exchange(ref _filmShader, null);
        GorgonTexture2DView texture = Interlocked.Exchange(ref _randomTexture, null);
        GorgonConstantBufferView buffer1 = Interlocked.Exchange(ref _scratchBuffer, null);
        GorgonConstantBufferView buffer2 = Interlocked.Exchange(ref _sepiaBuffer, null);

        shader?.Dispose();
        texture?.Dispose();
        buffer1?.Dispose();
        buffer2?.Dispose();
    }

    /// <summary>
    /// Function to render the effect.
    /// </summary>
    /// <param name="texture">The texture to blur and render to the output.</param>
    /// <param name="output">The output render target.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// <note type="important">
    /// <para>
    /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void Render(GorgonTexture2DView texture, GorgonRenderTargetView output)
    {
        BeginRender(output);

        switch (BeginPass(0, output))
        {
            case PassContinuationState.Continue:
                Renderer.DrawFilledRectangle(new DX.RectangleF(ShakeOffset.X, ShakeOffset.Y, output.Width, output.Height), GorgonColor.White, texture, new DX.RectangleF(0, 0, 1, 1));
                break;
            default:
                EndRender(null);
                return;
        }

        EndPass(0, output);

        EndRender(output);
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="Gorgon2DOldFilmEffect"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used to draw with this effect.</param>
    /// <param name="noiseTextureSize">[Optional] The size (width and height) of the texture used for the random noise for scratch line generation</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="noiseTextureSize"/> will determine how often and how many vertical scratch lines appear on the effect. A larger value will yield more scratch lines, while a smaller value
    /// will yield less.
    /// </para>
    /// </remarks>
    public Gorgon2DOldFilmEffect(Gorgon2D renderer, int noiseTextureSize = 64)
        : base(renderer, Resources.GOR2D_EFFECT_FILM, Resources.GOR2D_EFFECT_FILM_DESC, 1)
    {
        NoiseTextureSize = noiseTextureSize;

        _scratchSettings = new ScratchSettings
        {
            ScratchIntensity = 0.49f,
            ScratchScrollSpeed = 0.01f,
            ScratchVisibleTime = 0.003f,
            ScratchWidth = 0.01f
        };

        _sepiaSettings = new SepiaSettings
        {
            SepiaDesaturationAmount = 0.0f,
            SepiaToneAmount = 0.5f,
            SepiaLightColor = new GorgonColor(1, 0.9f, 0.65f, 1.0f),
            SepiaDarkColor = new GorgonColor(0.2f, 0.102f, 0, 1.0f)
        };
    }

}
