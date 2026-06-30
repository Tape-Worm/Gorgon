// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: June 24, 2026 11:37:34 PM
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides common resources used by various pieces of functionality in Gorgon.
/// </summary>
internal sealed class SharedResources
    : IDisposable
{
    #region Shaders
    // The shader code used to blit our texture.
    private const string BlitterShader = @"
            struct GorgonBlitterVertex
            {
                float2 position;
                float2 uv;
            };

            struct GorgonBlitterPixelShaderInput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct GorgonBlitterRenderData
            {
                float4x4 wvp;
                int vbHandle;
                int textureHandle;
                int samplerHandle;
            };
           
            SamplerState _gorgonBlitterSampler : register(s0);
            ConstantBuffer<GorgonBlitterRenderData> _gorgonBlitterRenderData : register(b0);
            
            GorgonBlitterPixelShaderInput GorgonBlitterVS(uint vertexID : SV_VertexID)
            {
                GorgonBlitterPixelShaderInput result;

                StructuredBuffer<GorgonBlitterVertex> vertexBuffer = ResourceDescriptorHeap[_gorgonBlitterRenderData.vbHandle];

                GorgonBlitterVertex vertex = vertexBuffer[vertexID];

                result.position = mul(_gorgonBlitterRenderData.wvp, float4(vertex.position, 0.7f, 1));
                result.uv = vertex.uv;

                return result;
            }

            float4 GorgonBlitterPS(GorgonBlitterPixelShaderInput input) : SV_TARGET
            {
                SamplerState sampler = SamplerDescriptorHeap[_gorgonBlitterRenderData.samplerHandle];
                Texture2D texture = ResourceDescriptorHeap[_gorgonBlitterRenderData.textureHandle];                
                float4 result = texture.Sample(sampler, input.uv);

                return result;
            }            
        ";
    #endregion

    /// <summary>
    /// The name of the include for attching the Gorgon blitter shaders to a custom shader.
    /// </summary>
    public const string BlitterShadersName = "__GORGON__BLITTER__SHADERS__";

    /// <summary>
    /// Property to return the global shader factory for building shaders.
    /// </summary>
    public GorgonShaderFactory ShaderFactory
    {
        get;
    }

    /// <summary>
    /// Property to return the vertex shader for the blitter command.
    /// </summary>
    public GorgonShader BlitterVertexShader
    {
        get;
    }

    /// <summary>
    /// Property to return the pixel shader for the blitter command.
    /// </summary>
    public GorgonShader BlitterPixelShader
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            BlitterPixelShader.Dispose();
            BlitterVertexShader.Dispose();
            ShaderFactory.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedResources"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this instance.</param>
    public SharedResources(GorgonGraphics graphics)
    {
        ShaderFactory = new GorgonShaderFactory(graphics);
        ShaderFactory.Includes[BlitterShadersName] = new GorgonShaderInclude(BlitterShadersName, BlitterShader);

        CompileFlags flags = graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;

        BlitterVertexShader = ShaderFactory.Compile(BlitterShader, "GorgonBlitterVS", ShaderType.VertexShader, flags: flags);
        BlitterPixelShader = ShaderFactory.Compile(BlitterShader, "GorgonBlitterPS", ShaderType.PixelShader, flags: flags);
    }
}
