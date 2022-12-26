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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Data;
using Gorgon.Renderers.Lights;
using Gorgon.Renderers.Properties;
using Gorgon.Renderers.Techniques;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// An effect used to render a scene with per-pixel lighting.
/// </summary>
/// <remarks>
/// <para>
/// This effect simulates lighting on 2D rendering by using a normal map (and specular map) to determine how to render a pixels shading. It does this by using a G-Buffer which contains a render target 
/// for the diffuse (unlit color), specular, and normal map data.
/// </para>
/// <para>
/// The effect renders the lighting data in 2 passes. The first renders the diffuse layer using a callback defined by the user to render their objects that they wish to have lit.  Then, the 2nd pass 
/// combines all the layers of the G-buffer together using additive blending for each <see cref="Lights">light</see> defined and returns the combined lighting data to a render target specified by the 
/// user. This output can then be combined with the scene using additive blending to produce the desired lighting effect. 
/// </para>
/// <para>
/// <note type="information">
/// <para>
/// Please note that this last compositing pass is not done by the effect and must be handled by the user. This is done in the interest of flexibility to allow the user to decide how to best handle 
/// the compositing of their scene.
/// </para>
/// </note>
/// </para>
/// <para>
/// In order for a sprite, or other 2D graphics object to be rendered using lighting, the backing texture must use a texture with an array count of 3 or higher. Each index of the array corresponds to a
/// layer used by the G-buffer to composite the lighting together. These array indices must be in the following order (while the word sprite is used, this can apply to anything that is textured):
/// <list type="number">
/// <item>
///		<description>The diffuse layer for the sprite - This is just the unlit colors for your sprite, basically the standard texture you'd normally use. Ideally, this texture should not contain 
///		any shading at all as this will be handled by the effect.</description>
/// </item>
/// <item>
///		<description>The specular layer for the sprite - This is the texture array index that controls which parts of the sprite are "shiny". Leaving this black will produce no specular hilighting at 
///		all, while pure white will make all parts of it shiny. This amount of specular is controlled by the <see cref="GorgonLightCommon.SpecularPower"/> property on a light.</description>
/// </item>
/// <item>
///		<description>The normal map layer for the sprite - This is the texture array index that provides normals for the lighting calculations. This layer must have data in it or else no lighting will 
///		be applied. Gorgon does not generate normal map for your texture, however there are a multitude of tools available online to help with this (e.g. CrazyBump, SpriteIlluminator, etc...).</description>
/// </item>
/// <item>
///		<description>The position buffer for the sprite - This is the texture array index that world position data for the lighting calculations. This layer may be omitted by passing -1 to a position 
///		index parameter. The <see cref="Gorgon2DGBuffer"/> will use always positional data and will generate world position data based on whatever you render.</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// The user must also supply at least a single light source to effectively view the lighting on the 2D object.
/// </para>
/// </remarks>
/// <seealso cref="Gorgon2DGBuffer"/>
public class Gorgon2DLightingEffect
    : Gorgon2DEffect
{
    #region Value Types.
    // Constant buffer data for global data.
    [StructLayout(LayoutKind.Sequential, Size = 64, Pack = 16)]
    private struct GlobalEffectData
    {
        /// <summary>
        /// The ambient color.
        /// </summary>
        public GorgonColor AmbientColor;
        /// <summary>
        /// Position for the light in screen space.
        /// </summary>
        public Vector4 CameraPosition;
        /// <summary>
        /// The texture array indices to use for the normal map, and specular map.
        /// </summary>
        public Vector4 ArrayIndices;
        /// <summary>
        /// The flag to check if the light is behind an object.
        /// </summary>
        public int ZCheck;
    }
    #endregion

    #region Variables.        
    // Our custom vertex shader for per pixel lighting.
    private GorgonVertexShader _vertexLitTransformShader;
    private Gorgon2DShaderState<GorgonVertexShader> _vertexLitShaderState;
    // A pixel shader used to render lights.
    private GorgonPixelShader _pixelLitTransformShader;
    private GorgonPixelShader _pixelLitArrayTransformShader;
    private Gorgon2DShaderState<GorgonPixelShader> _pixelLitShaderState;
    // The buffer that will hold the lighting data for an individual light.
    private GorgonConstantBufferView _lightBuffer;
    // The buffer that will hold the global effect data.
    private GorgonConstantBufferView _globalBuffer;
    // The batch render state for drawing with lights.
    private Gorgon2DBatchState _lightingState;
    // The data to pass to the effect.
    private GlobalEffectData _effectData = new()
    {
        AmbientColor = GorgonColor.Black,
        ZCheck = 1
    };
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the global ambient color.
    /// </summary>
    public GorgonColor AmbientColor
    {
        get => _effectData.AmbientColor;
        set => _effectData.AmbientColor = value;
    }

    /// <summary>
    /// Property to set or return whether to check if the light is behind an object or not.
    /// </summary>
    public bool CheckLightDepth
    {
        get => _effectData.ZCheck != 0;
        set => _effectData.ZCheck = value ? 1 : 0;
    }

    /// <summary>
    /// Property to return the list of point lights for rendering.
    /// </summary>
    public IList<GorgonLightCommon> Lights
    {
        get;
    } = new List<GorgonLightCommon>(1024);

    /// <summary>Property to return the number of passes required to render the effect.</summary>
    /// <remarks>This is merely for information, passes may or may not be exposed to the end user by the effect author.</remarks>
    public override int PassCount => Lights.Count + 1;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to perform the actual rendering of the effect.
    /// </summary>
    /// <param name="diffuse">The diffuse texture to render.</param>
    /// <param name="output">The final output target for the effect.</param>
    /// <param name="camera">The camera used to transform the lights to camera space.</param>
    private void OnRender(GorgonTexture2DView diffuse, GorgonRenderTargetView output, GorgonCameraCommon camera)
    {
        // Draw our diffuse pass first, so we can get our ambient.
        Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
        Renderer.Begin(Gorgon2DBatchState.NoBlend, camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                    AmbientColor,
                                    diffuse,
                                    new DX.RectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.Default);
        Renderer.End();

        BeginRender(output, null, null, null);

        for (int i = 0; i < Lights.Count; ++i)
        {
            if (BeginPass(i, output, camera) != PassContinuationState.Continue)
            {
                EndPass(i, output);
                break;
            }

            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                        GorgonColor.White,
                                        diffuse,
                                        new DX.RectangleF(0, 0, 1, 1),                                            
                                        textureSampler: GorgonSamplerState.Default);

            EndPass(i, output);
        }

        EndRender(output);
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
        
        GorgonConstantBufferView lightData = Interlocked.Exchange(ref _lightBuffer, null);
        GorgonConstantBufferView globalData = Interlocked.Exchange(ref _globalBuffer, null);

        GorgonVertexShader vertexShader = Interlocked.Exchange(ref _vertexLitTransformShader, null);
        vertexShader?.Dispose();
        vertexShader = Interlocked.Exchange(ref _vertexLitTransformShader, null);
        vertexShader?.Dispose();

        GorgonPixelShader lightShader = Interlocked.Exchange(ref _pixelLitArrayTransformShader, null);
        lightShader?.Dispose();
        lightShader = Interlocked.Exchange(ref _pixelLitTransformShader, null);
        lightShader?.Dispose();

        globalData?.Dispose();
        lightData?.Dispose();
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
        if (_vertexLitShaderState is null)
        {
            _vertexLitShaderState = builders.VertexShaderBuilder.Shader(_vertexLitTransformShader)
                                                                .Build(VertexShaderAllocator);
        }

        if ((statesChanged) || (_pixelLitShaderState is null))
        {
            builders.PixelShaderBuilder.Clear()                                           
                                       .ConstantBuffer(_lightBuffer, 1)
                                       .ConstantBuffer(_globalBuffer, 2)
                                       .SamplerState(GorgonSamplerState.Default, 0)
                                       .SamplerState(GorgonSamplerState.PointFilteringWrapping, 1)
                                       .SamplerState(GorgonSamplerState.Wrapping, 2);

            builders.PixelShaderBuilder.Shader(_pixelLitArrayTransformShader);

            _pixelLitShaderState = builders.PixelShaderBuilder.Build(PixelShaderAllocator);
            _lightingState = null;
        }

        if (_lightingState is null)
        {
            _lightingState = builders.BatchBuilder
                                     .BlendState(GorgonBlendState.Additive)
                                     .DepthStencilState(GorgonDepthStencilState.Default)
                                     .RasterState(GorgonRasterState.Default)
                                     .PixelShaderState(_pixelLitShaderState)
                                     .VertexShaderState(_vertexLitShaderState)
                                     .Build(BatchStateAllocator);
        }

        return _lightingState;
    }

    /// <summary>Function called prior to rendering.</summary>
    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
    /// <param name="sizeChanged">
    ///   <b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
    /// <remarks>
    /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
    /// targets (if applicable).
    /// </remarks>
    protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
    {
        if (Graphics.RenderTargets[0] != output)
        {
            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
        }
    }

    /// <summary>Function called prior to rendering a pass.</summary>
    /// <param name="passIndex">The index of the pass to render.</param>
    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
    /// <param name="camera">The currently active camera.</param>
    /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
    /// <remarks>
    ///   <para>
    /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
    /// </para>
    /// </remarks>
    /// <seealso cref="PassContinuationState"/>
    protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, GorgonCameraCommon camera)
    {
        var size = new DX.Size2F(output.Width * 0.5f, output.Height * 0.5f);
        float specularZ = (size.Width > size.Height ? size.Width * 0.5f : size.Height * 0.5f).Max(128).Min(640);
        var cameraPos = new Vector4(size.Width, size.Height, -specularZ, 0);

        // If no custom camera is in use, we need to pass in our default viewing information which is normally the output width, and height (by half), and an arbitrary Z value so 
        // the camera position isn't intersecting with the drawing plane (+ height information). Otherwise, our specular hilight will look really messed up.
        if (camera is not null)
        {
            cameraPos = new Vector4(camera.Position, 0);
        }

        _effectData.CameraPosition = cameraPos;

        _globalBuffer.Buffer.SetData(in _effectData);

        GorgonLightCommon light = Lights[passIndex];
        ref readonly GorgonGpuLightData gpuLight = ref light.GetGpuData();
        _lightBuffer.Buffer.SetData(in gpuLight);

        return PassContinuationState.Continue;
    }


    /// <summary>
    /// Function called to initialize the effect.
    /// </summary>
    /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
    protected override void OnInitialize()
    {
        if (_globalBuffer is not null)
        {
            return;
        }

        _globalBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                      new GorgonConstantBufferInfo(Unsafe.SizeOf<GlobalEffectData>())
                                                                      {
                                                                          Name = "Global light effect data.",                                                                              
                                                                          Usage = ResourceUsage.Dynamic
                                                                      });

        _lightBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                   new GorgonConstantBufferInfo(GorgonGpuLightData.SizeInBytes)
                                                                   {
                                                                       Name = "Deferred Lighting Light Data Buffer",                                                                           
                                                                       Usage = ResourceUsage.Dynamic
                                                                   });

        _vertexLitTransformShader = CompileShader<GorgonVertexShader>(Resources.Lighting, "GorgonVertexLitShader");
        _pixelLitArrayTransformShader = CompileShader<GorgonPixelShader>(Resources.Lighting, "GorgonPixelShaderLighting");
        _pixelLitTransformShader = CompileShader<GorgonPixelShader>(Resources.Lighting, "GorgonPixelShaderLighting");

        // If we ever need to change the indices, we can do so here.
        _effectData.ArrayIndices = new Vector4(1, 2, 3, 0);
    }

    /// <summary>
    /// Function to render the effect using the array indices of the diffuse texture.
    /// </summary>
    /// <param name="gbuffer">A Gbuffer containing the diffuse, normal and specular maps.</param>
    /// <param name="output">The final output target for the effect.</param>
    /// <param name="camera">[Optional] The camera used to transform the lights to camera space.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="gbuffer"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
    public void Render(IGorgonGBuffer gbuffer, GorgonRenderTargetView output, GorgonCameraCommon camera = null)
    {
        gbuffer.ValidateObject(nameof(gbuffer));
        output.ValidateObject(nameof(output));

        OnRender(gbuffer.GBufferTexture, output, camera);
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="Gorgon2DLightingEffect"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used to draw with the effect.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
    public Gorgon2DLightingEffect(Gorgon2D renderer)
        : base(renderer, Resources.GOR2D_EFFECT_LIGHTING, Resources.GOR2D_EFFECT_LIGHTING_DESC, 1)
    {
    }
    #endregion
}
