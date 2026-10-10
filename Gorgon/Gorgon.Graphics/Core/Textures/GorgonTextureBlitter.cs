// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 20, 2025 5:57:46 PM
//

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Memory;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A utility used to render textures to the render target bound to a <see cref="GorgonCommandList"/>.
/// </summary>
/// <remarks>
/// <para>
/// The texture blitter provides a quick way to display the contents of a texture on a render target without having to set up pipeline state objects, buffers, and shaders. This is useful for testing, 
/// tools, and applications that only need to display a texture. It is not a 2D renderer, and it is not intended for performance sensitive work.
/// </para>
/// <para>
/// A texture can be rendered into a rectangular area on the render target with the <see cref="Blit"/> method, or rendered over the entire viewport with the <see cref="BlitFullScreen"/> method. Custom pixel 
/// shaders can be used with the <see cref="Blit"/> method to apply effects to the texture.
/// </para>
/// <para>
/// <note type="information">
/// <para>
/// The blitter uses the last constant slot (<see cref="GorgonGraphics.MaxRootCbvCount"/> - 1) to send its data to its shaders. Applications should not use this slot for their own data if they are 
/// using the blitter.
/// </para>
/// </note>
/// </para>
/// <para type="creation">
/// <note type="important">
/// <para>
/// Creating a blitter waits for the GPU to finish its current work. Therefore, a blitter should be created when the application is initialized, and not while rendering.
/// </para>
/// </note>
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// This type is not thread safe. When recording commands on multiple threads, use a separate blitter for each thread.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList"/>
public unsafe class GorgonTextureBlitter
    : IDisposable
{
    /// <summary>
    /// The shader source code used to blit textures with the <see cref="GorgonTextureBlitter"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This source code contains the vertex and pixel shaders used by the blitter, along with the structures and constant data that they use. It is provided so that custom pixel shaders for the blitter 
    /// can be written against the same structures and constant data. See the <see cref="GorgonTextureBlitterShadersName"/> field for information on how to include this source code in a custom pixel shader.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTextureBlitterShadersName"/>
    /// <seealso cref="Blit"/>
    public static readonly string GorgonTextureBlitterShader = $$"""
            // Our default blitting vertex.
            struct GorgonBlitterVertex
            {
                float2 position;
                float2 uv;
                float4 color;
            };
        
            // The pixel shader value for rendering a standard blit.
            struct GorgonBlitterPixelShaderInput
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            // The pixel shader value for rendering a full screen triangle blit.
            struct GorgonBlitterFSPixelShaderInput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
        
            // Data required for standard blitter rendering.
            struct GorgonBlitterRenderData
            {
                float4x4 wvp;
                int vbHandle;
                int textureHandle;
                int samplerHandle;
            };

            // Data required for full screen triangle rendering.
            struct GorgonFullScreenBlitterRenderData
            {
                int textureHandle;
                int samplerHandle;
                int noBlending;
            };
                   
            // The constant buffer holding the blitter render data.
            ConstantBuffer<GorgonBlitterRenderData> _gorgonBlitterRenderData : register(b{{(GorgonGraphics.MaxRootCbvCount - 1)}});
            // The constant buffer holding the full screen triangle render data.
            ConstantBuffer<GorgonFullScreenBlitterRenderData> _gorgonFullScreenBlitterRenderData : register(b{{(GorgonGraphics.MaxRootCbvCount - 1)}});
                    
            // The vertex shader for the standard blitter.
            GorgonBlitterPixelShaderInput GorgonBlitterVS(uint vertexID : SV_VertexID)
            {
                GorgonBlitterPixelShaderInput result;

                StructuredBuffer<GorgonBlitterVertex> vertexBuffer = ResourceDescriptorHeap[_gorgonBlitterRenderData.vbHandle];
                GorgonBlitterVertex vertex = vertexBuffer[vertexID];

                result.position = mul(_gorgonBlitterRenderData.wvp, float4(vertex.position, 0.7f, 1));
                result.uv = vertex.uv;
                result.color = vertex.color;

                return result;
            }

            // The pixel shader for the standard blitter.
            float4 GorgonBlitterPS(GorgonBlitterPixelShaderInput input) : SV_TARGET
            {
                SamplerState sampler = SamplerDescriptorHeap[_gorgonBlitterRenderData.samplerHandle];
                Texture2D texture = ResourceDescriptorHeap[_gorgonBlitterRenderData.textureHandle];                
                float4 result = texture.Sample(sampler, input.uv) * input.color;

                clip(result.a <= 0 ? -1 : 1);

                return result;
            }            

            // The vertex shader for full screen triangle blitting.
            GorgonBlitterFSPixelShaderInput GorgonFullScreenVertexShader(uint vertexID : SV_VertexID)
            {
                GorgonBlitterFSPixelShaderInput result;

                result.uv = float2((vertexID << 1) & 2, vertexID & 2);
                result.position = float4(result.uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);

                return result;
            }

            // The pixel shader for full screen triangle blitting.
            float4 GorgonFullScreenPixelShader(GorgonBlitterFSPixelShaderInput input) : SV_Target
            {
                SamplerState sampler = SamplerDescriptorHeap[_gorgonFullScreenBlitterRenderData.samplerHandle];
                Texture2D texture = ResourceDescriptorHeap[_gorgonFullScreenBlitterRenderData.textureHandle];                
                float4 result = texture.Sample(sampler, input.uv);
                
                clip(_gorgonFullScreenBlitterRenderData.noBlending == 0 && result.a <= 0 ? -1 : 1);

        	    return result;
            }        
        """;    

    /// <summary>
    /// The name of the include for attaching the Gorgon blitter shaders to a custom shader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A custom pixel shader passed to the <see cref="Blit"/> method must use the same inputs and constant data as the blitter's own pixel shader. To get these, include the 
    /// <see cref="GorgonTextureBlitterShader"/> source code in the custom shader with <c>#GorgonInclude "__GORGON__BLITTER__SHADERS__"</c>. This should be the first include in the custom shader, 
    /// before any other <c>#GorgonInclude</c> or <c>#include</c>. This include is available to every 
    /// <see cref="GorgonShaderCompiler"/>, so no other setup is required.
    /// </para>
    /// <para>
    /// The included source code provides the following for a custom pixel shader:
    /// <list type="bullet">
    ///     <item>
    ///         <term>GorgonBlitterPixelShaderInput</term>
    ///         <description>The input to the pixel shader. This contains the position, the tint color, and the texture coordinates for the pixel.</description>
    ///     </item>
    ///     <item>
    ///         <term>_gorgonBlitterRenderData</term>
    ///         <description>The constant data for the blit. The <c>textureHandle</c> and <c>samplerHandle</c> members are the handles for the texture and sampler passed to the <see cref="Blit"/> method.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The blitter's constant data uses the last constant slot. Custom pixel shaders can receive their own data in the other slots by calling <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> 
    /// before calling <see cref="Blit"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTextureBlitterShader"/>
    /// <seealso cref="Blit"/>
    public const string GorgonTextureBlitterShadersName = "__GORGON__BLITTER__SHADERS__";

    /// <summary>
    /// The layout of a vertex for the blitter.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        /// <summary>
        /// The size, in bytes, of the structure.
        /// </summary>
        public static readonly int SizeInBytes = sizeof(Vertex);

        /// <summary>
        /// The 2D position of the vertex.
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The texture coordinate for the vertex.
        /// </summary>
        public Vector2 UV;
        /// <summary>
        /// The color for the blit rectangle.
        /// </summary>
        public GorgonColor Color;
    }

    /// <summary>
    /// The data to send to the shader for rendering.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The world/view/projection matrix used to transform the blit.
        /// </summary>
        public Matrix4x4 Wvp;
        /// <summary>
        /// The handle for the vertex buffer resource.
        /// </summary>
        public int VertexBufferHandle;
        /// <summary>
        /// The handle for the texture resource.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// The handle for the texture sampler.
        /// </summary>
        public int SamplerHandle;
    }

    /// <summary>
    /// The data to send to the shader for full screen triangle rendering.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct FullScreenRenderData
    {
        /// <summary>
        /// The handle for the texture resource.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// The handle for the texture sampler.
        /// </summary>
        public int SamplerHandle;
        /// <summary>
        /// A flag to indicate whether blending is being used. Used to control alpha clipping in the shader.
        /// </summary>
        public int NoBlending;
    }

    /// <summary>
    /// A key used to look up preexisting PSO states.
    /// </summary>
    /// <param name="BlendState">The blending state to use.</param>
    /// <param name="MultisampleInfo">The multi sample information for the current render target in slot 0.</param>
    /// <param name="FormatList">An encoded list of each render target format.</param>
    /// <param name="PixelShader">The pixel shader used.</param>
    private readonly record struct PsoKey(GorgonBlendState BlendState, GorgonMultisampleInfo MultisampleInfo, ulong FormatList, GorgonShader PixelShader);

    private Vector2 _viewDimensions;
    private GorgonIndexedDrawCall? _drawCall;
    private GorgonDrawCall? _fullScreenDrawCall;
    private readonly GorgonIndexBuffer _indexBuffer;
    private readonly GorgonStructuredBufferView _vertexBuffer;
    private Matrix4x4 _projection;
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    private readonly Dictionary<PsoKey, GorgonGraphicsPso> _cachedPsos = [];
    private readonly Dictionary<PsoKey, GorgonGraphicsPso> _cachedFullScreenPsos = [];
    private (PsoKey Key, GorgonGraphicsPso Pso) _currentPso;
    private (PsoKey Key, GorgonGraphicsPso Pso) _currentFullScreenPso;
    private readonly GorgonShader _defaultPixelShader;

    /// <summary>
    /// Property to return the graphics instance associated with the blitter.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _psoFactory.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }

    /// <summary>
    /// Function to build the pipeline state object for the blitter.
    /// </summary>
    /// <param name="commandList">The command list to use for rendering.</param>
    /// <param name="blendState">The requested blend state.</param>
    /// <param name="pixelShader">The user defined or default pixel shader to apply.</param>
    /// <param name="key">The key used to cache the PSO.</param>
    private void BuildPSO(GorgonCommandList commandList, GorgonBlendState blendState, GorgonShader pixelShader, ref readonly PsoKey key)
    {
        GorgonGraphicsPso pso;
        Span<BufferFormat> outputFormats = stackalloc BufferFormat[commandList.RenderTargets.Length];        

        if (commandList.RenderTargets.Length > 1)
        {
            for (int i = 0; i < outputFormats.Length; ++i)
            {
                outputFormats[i] = commandList.RenderTargets[i].Format;
            }
        }
        else
        {
            outputFormats[0] = commandList.RenderTargets[0].Format;
        }               

        GorgonGraphicsPsoBuilder psoFactory = new(Graphics);
        psoFactory.PixelShader(pixelShader)
                  .OutputFormats(outputFormats)
                  .Multisample(commandList.RenderTargets[0].Texture.MultisampleInfo)
                  .BlendState(blendState);

        _cachedPsos[key] = pso = _psoFactory.CreateOrGetPso($"Gorgon Blitter PSO - {commandList.Name}:{Guid.NewGuid():N}", Graphics.SharedResources.BlitterVertexShader, psoFactory);        
        _currentPso = (key, pso);
    }

    /// <summary>
    /// Function to build the pipeline state object for the full screen blitter.
    /// </summary>
    /// <param name="commandList"><inheritdoc cref="BuildPSO" path="/param[@name='commandList']"/></param>
    /// <param name="blendState">The requested blend state.</param>
    /// <param name="key"><inheritdoc cref="BuildPSO" path="/param[@name='key']"/></param>
    private void BuildFullScreenPSO(GorgonCommandList commandList, GorgonBlendState blendState, ref readonly PsoKey key)
    {
        GorgonGraphicsPso pso;
        Span<BufferFormat> outputFormats = stackalloc BufferFormat[commandList.RenderTargets.Length];

        if (commandList.RenderTargets.Length > 1)
        {
            for (int i = 0; i < outputFormats.Length; ++i)
            {
                outputFormats[i] = commandList.RenderTargets[i].Format;
            }
        }
        else
        {
            outputFormats[0] = commandList.RenderTargets[0].Format;
        }

        GorgonGraphicsPsoBuilder psoFactory = new(Graphics);
        psoFactory.PixelShader(Graphics.SharedResources.FullScreenBlitterPixelShader)
                  .OutputFormats(outputFormats)
                  .Multisample(commandList.RenderTargets[0].Texture.MultisampleInfo)
                  .BlendState(blendState);

        _cachedFullScreenPsos[key] = pso = _psoFactory.CreateOrGetPso($"Gorgon Full Screen Triangle Blitter PSO - {commandList.Name}:{Guid.NewGuid():N}", Graphics.SharedResources.FullScreenBlitterVertexShader, psoFactory);
        _currentFullScreenPso = (key, pso);
    }

    /// <summary>Function to update the projection matrix.</summary>
    /// <param name="viewPort">The current viewport for target slot 0.</param>
    /// <param name="projectionMatrix">The instance of the matrix to update.</param>
    private void UpdateProjectionMatrix(ref readonly GorgonViewport viewPort, ref Matrix4x4 projectionMatrix)
    {
        if ((viewPort.Width == _viewDimensions.X) && (viewPort.Height == _viewDimensions.Y))
        {
            return;
        }

        const float zRange = 1.0f;
        const float left = 0;
        const float top = 0;
        float right = viewPort.Width;
        float bottom = viewPort.Height;

        projectionMatrix = Matrix4x4.Identity;
        projectionMatrix.M11 = 2.0f / (right - left);
        projectionMatrix.M22 = 2.0f / (top - bottom);
        projectionMatrix.M33 = zRange;
        projectionMatrix.M41 = (left + right) / (left - right);
        projectionMatrix.M42 = (top + bottom) / (bottom - top);

        _viewDimensions = new Vector2(viewPort.Width, viewPort.Height);
    }

    /// <summary>
    /// Function to build a new PSO cache key.
    /// </summary>
    /// <param name="blendState">The blend state for the PSO at target slot 0.</param>
    /// <param name="rtvs">The list of bound render target views on the command list.</param>
    /// <param name="pixelShader">The currently active pixel shader.</param>
    /// <param name="key">The new key.</param>
    private static void BuildPsoKey(GorgonBlendState blendState, ReadOnlySpan<GorgonRenderTargetView> rtvs, GorgonShader pixelShader, out PsoKey key)
    {
        ulong formatKey = 0;

        for (int i = 0; i < rtvs.Length; ++i)
        {
            formatKey |= (((ulong)rtvs[i].Format) & 0xff) << (i * 8);
        }

        key = new PsoKey(blendState, rtvs[0].Texture.MultisampleInfo, formatKey, pixelShader);
    }

    /// <summary>
    /// Function to update the draw call used to draw the rectangle.
    /// </summary>
    /// <param name="texture">The current texture being applied.</param>
    [MemberNotNull(nameof(_drawCall))]
    private void UpdateDrawCall(IGorgonTextureView<GorgonTextureCommon> texture)
    {
        if (_drawCall is null)
        {            
            _drawCall = new GorgonIndexedDrawCall(6, _indexBuffer, _currentPso.Pso);
            _drawCall.AssignBuffer(new GorgonUsedBuffer(_vertexBuffer, ShaderStage.Vertex, BufferUsage.VertexBuffer));
        }
        else if (_drawCall.Pso != _currentPso.Pso)
        {
            _drawCall.Pso = _currentPso.Pso;
        }

        if ((_drawCall.UsedTextures.Count == 0) || (_drawCall.UsedTextures[0].Texture != texture.Texture))
        {
            _drawCall.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
        }
    }

    /// <summary>
    /// Function to update the draw call used to draw the rectangle.
    /// </summary>
    /// <param name="texture">The current texture being applied.</param>
    [MemberNotNull(nameof(_fullScreenDrawCall))]
    private void UpdateFullScreenDrawCall(IGorgonTextureView<GorgonTextureCommon> texture)
    {
        if (_fullScreenDrawCall is null)
        {
            _fullScreenDrawCall = new GorgonDrawCall(3, _currentFullScreenPso.Pso);
        }
        else if (_fullScreenDrawCall.Pso != _currentFullScreenPso.Pso)
        {
            _fullScreenDrawCall.Pso = _currentFullScreenPso.Pso;
        }

        if ((_fullScreenDrawCall.UsedTextures.Count == 0) || (_fullScreenDrawCall.UsedTextures[0].Texture != texture.Texture))
        {
            _fullScreenDrawCall.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to render a texture into a rectangular area on the render target bound to a <see cref="GorgonCommandList"/>.
    /// </summary>
    /// <param name="commandList">The command list that will record the commands to render the texture.</param>
    /// <param name="texture">The view of the texture to render.</param>
    /// <param name="destination">The area on the render target to render into, in pixels.</param>
    /// <param name="color">[Optional] The color used to tint the texture.</param>
    /// <param name="textureCoordinates">[Optional] The region of the texture to render, in normalized texture coordinates (0.0 - 1.0).</param>
    /// <param name="sampler">[Optional] The sampler used to sample the texture.</param>
    /// <param name="blendState">[Optional] The blending state to apply when rendering the texture.</param>
    /// <param name="pixelShader">[Optional] A custom pixel shader used to render the texture.</param>
    /// <remarks>
    /// <para>
    /// This method renders the <paramref name="texture"/> into the <paramref name="destination"/> area on the render target in slot 0 of the <paramref name="commandList"/>. The 
    /// <paramref name="destination"/> is relative to the upper left corner of the first viewport assigned to the <paramref name="commandList"/>.
    /// </para>
    /// <para>
    /// If the <paramref name="color"/> is omitted, then the texture is rendered without a tint. If the alpha value of the <paramref name="color"/> is 0, then nothing is rendered. When using the default pixel 
    /// shader, pixels with an alpha value of 0 (after the tint is applied) are not rendered.
    /// </para>
    /// <para>
    /// If the <paramref name="textureCoordinates"/> are omitted, then the entire texture is rendered. If the <paramref name="sampler"/> is omitted, then <see cref="GorgonSampler.Default(GorgonGraphics)"/> 
    /// is used. If the <paramref name="blendState"/> is omitted, then <see cref="GorgonBlendState.NoBlending"/> is used.
    /// </para>
    /// <para>
    /// A custom <paramref name="pixelShader"/> can be used to apply an effect to the texture as it is rendered. The shader must use the inputs and constant data from the blitter's shader source code, 
    /// see the <see cref="GorgonTextureBlitterShadersName"/> field for more information. If the <paramref name="pixelShader"/> is not a pixel shader, then a warning is written to the log, and the default 
    /// pixel shader is used instead.
    /// </para>
    /// <para type="state">
    /// The <paramref name="commandList"/> must have at least one viewport, one scissor rectangle, and one render target assigned. If any of these are missing, or if the <paramref name="texture"/> is 
    /// multisampled, then a warning is written to the log, and nothing is rendered. Multisampled textures must be resolved into a non-multisampled texture before they can be rendered by the blitter.
    /// </para>
    /// <para type="constants">
    /// The blitter writes its data into the last constant slot (<see cref="GorgonGraphics.MaxRootCbvCount"/> - 1) on the <paramref name="commandList"/>. Any data previously written to that slot by 
    /// the application is replaced.
    /// </para>
    /// <para type="pso">
    /// The first time the blitter is used with a new combination of render target formats, multisampling, blending state, or pixel shader, it builds a new pipeline state object. This is a small, one 
    /// time cost.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following code shows how to render textures using the blitter.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Create the blitter once, when the application is initialized.
    /// GorgonTextureBlitter blitter = new(graphics);
    ///
    /// // ... Later, when recording commands.
    /// // The command list must have a render target, viewport, and scissor rectangle assigned.
    /// commandList.SetRenderTarget(renderTarget)
    ///            .SetViewport(viewport)
    ///            .SetScissorRectangle(scissor);
    ///
    /// // Render the texture at 32, 32 with a size of 256x256 pixels.
    /// blitter.Blit(commandList, textureView, new GorgonRectangleF(32, 32, 256, 256));
    ///
    /// // Render the upper left quarter of the texture at 50% opacity, using alpha blending.
    /// blitter.Blit(commandList, textureView, new GorgonRectangleF(320, 32, 256, 256),
    ///              color: new GorgonColor(GorgonColors.White, 0.5f),
    ///              textureCoordinates: new GorgonRectangleF(0, 0, 0.5f, 0.5f),
    ///              blendState: GorgonBlendState.Default);
    /// ]]>
    /// </code>
    /// <para>
    /// The following code shows how to render a texture with a custom pixel shader that converts the texture to grayscale.
    /// </para>
    /// The HLSL code:
    /// <code>
    /// <![CDATA[
    /// #GorgonInclude "__GORGON__BLITTER__SHADERS__"
    ///
    /// float4 GrayscalePS(GorgonBlitterPixelShaderInput input) : SV_Target
    /// {
    ///     SamplerState sampler = SamplerDescriptorHeap[_gorgonBlitterRenderData.samplerHandle];
    ///     Texture2D texture = ResourceDescriptorHeap[_gorgonBlitterRenderData.textureHandle];
    ///     float4 color = texture.Sample(sampler, input.uv) * input.color;
    ///     float gray = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    ///
    ///     return float4(gray, gray, gray, color.a);
    /// }
    /// ]]>
    /// </code>
    /// The C# code:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Compile our shader. The blitter include is available to every compiler.
    /// GorgonShader grayscale = compiler.Compile(shaderSource, "GrayscalePS", ShaderType.PixelShader);
    ///
    /// // ... Later, when recording commands.
    /// blitter.Blit(commandList, textureView, new GorgonRectangleF(32, 32, 256, 256), pixelShader: grayscale);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="BlitFullScreen"/>
    /// <seealso cref="GorgonTextureBlitterShadersName"/>
    public void Blit(GorgonCommandList commandList, IGorgonTextureView<GorgonTextureCommon> texture, GorgonRectangleF destination, GorgonColor? color = null, GorgonRectangleF? textureCoordinates = null, GorgonSampler? sampler = null, GorgonBlendState? blendState = null, GorgonShader? pixelShader = null)
    {
        if ((pixelShader is not null) && (pixelShader.ShaderType != ShaderType.PixelShader))
        {
            Graphics.Log.PrintWarning($"The shader passed to the blitter is not a pixel shader. Using the default shader instead.", LoggingLevel.Intermediate);
            pixelShader = _defaultPixelShader;
        }

        if (commandList.Viewports.IsEmpty)
        {
            Graphics.Log.PrintWarning($"There are no viewports set on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (commandList.ScissorRectangles.IsEmpty)
        {
            Graphics.Log.PrintWarning($"There are no scissor rectangles set on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (commandList.RenderTargets.Length == 0)
        {
            Graphics.Log.PrintWarning($"There are no render targets bound on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (!texture.Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            Graphics.Log.PrintWarning($"The texture '{texture.Texture.Name}' is a multisampled texture. The blitter cannot blit these until they are resolved into a non-multisampled texture. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;            
        }

        GorgonColor actualColor = color ?? GorgonColors.White;

        // If we have 0 alpha, then there's nothing to render.
        // We'll use the exact 0 value here just to be safe, in case the epsilon provides a tinty value.
        if (actualColor.Alpha == 0.0f)
        {
            return;
        }

        GorgonRectangleF uv = textureCoordinates is null ? new GorgonRectangleF(0, 0, 1, 1) : textureCoordinates.Value;
        sampler ??= GorgonSampler.Default(Graphics);
        blendState ??= GorgonBlendState.NoBlending;
        pixelShader ??= _defaultPixelShader;

        BuildPsoKey(blendState, commandList.RenderTargets, pixelShader, out PsoKey key);

        if (!key.Equals(_currentPso.Key))
        {
            if (!_cachedPsos.TryGetValue(key, out GorgonGraphicsPso? value))
            {
                BuildPSO(commandList, blendState, pixelShader, in key);
            }
            else
            {
                _currentPso = (key, value);
            }
        }

        UpdateDrawCall(texture);
        UpdateProjectionMatrix(in commandList.Viewports[0], ref _projection);

        RenderData data = new()
        {
            Wvp = _projection,
            VertexBufferHandle = _vertexBuffer.GetViewHandle(),
            TextureHandle = texture.GetViewHandle(),
            SamplerHandle = sampler.GetViewHandle()
        };

        Span<Vertex> vertices = [
            new Vertex
                {
                    Position = destination.TopLeft,
                    UV = uv.TopLeft,
                    Color = actualColor
                },
                new Vertex
                {
                    Position = destination.TopRight,
                    UV = uv.TopRight,
                    Color = actualColor
                },
                new Vertex
                {
                    Position = destination.BottomRight,
                    UV = uv.BottomRight,
                    Color = actualColor
                },
                new Vertex
                {
                    Position = destination.BottomLeft,
                    UV = uv.BottomLeft,
                    Color = actualColor
                }
        ];

        GorgonGpuUploadMemory uploader = new(Graphics, _vertexBuffer.Buffer.SizeInBytes);
        uploader.WriteRange(vertices);

        commandList.UploadGpuMemoryToBuffer(in uploader, _vertexBuffer.Buffer)
                   .WriteConstant(GorgonGraphics.MaxRootCbvCount - 1, in data)
                   .Draw(_drawCall);
    }

    /// <summary>
    /// Function to render a texture over the entire area of the first viewport assigned to a <see cref="GorgonCommandList"/>.
    /// </summary>
    /// <param name="commandList">The command list that will record the commands to render the texture.</param>
    /// <param name="texture">The view of the texture to render.</param>
    /// <param name="sampler">[Optional] The sampler used to sample the texture.</param>
    /// <param name="blendState">[Optional] The blending state to apply when rendering the texture.</param>
    /// <remarks>
    /// <para>
    /// This method stretches the <paramref name="texture"/> over the entire area of the first viewport on the render target in slot 0 of the <paramref name="commandList"/>. It renders a single triangle that 
    /// covers the viewport, and does not upload any vertex data, so it is faster than calling <see cref="Blit"/> with a rectangle that covers the viewport. However, it does not support tinting, rendering a 
    /// region of the texture, or custom pixel shaders.
    /// </para>
    /// <para>
    /// If the <paramref name="sampler"/> is omitted, then <see cref="GorgonSampler.Default(GorgonGraphics)"/> is used. If the <paramref name="blendState"/> is omitted, then 
    /// <see cref="GorgonBlendState.NoBlending"/> is used.
    /// </para>
    /// <para>
    /// When the <paramref name="blendState"/> is equal to <see cref="GorgonBlendState.NoBlending"/>, every pixel of the texture is written to the render target, including pixels with an alpha value of 0. 
    /// For any other blending state, pixels with an alpha value of 0 are not rendered.
    /// </para>
    /// <inheritdoc cref="Blit(GorgonCommandList, IGorgonTextureView{GorgonTextureCommon}, GorgonRectangleF, GorgonColor?, GorgonRectangleF?, GorgonSampler?, GorgonBlendState?, GorgonShader?)" path="/remarks/para[@type='state']"/>
    /// <inheritdoc cref="Blit(GorgonCommandList, IGorgonTextureView{GorgonTextureCommon}, GorgonRectangleF, GorgonColor?, GorgonRectangleF?, GorgonSampler?, GorgonBlendState?, GorgonShader?)" path="/remarks/para[@type='constants']"/>
    /// <inheritdoc cref="Blit(GorgonCommandList, IGorgonTextureView{GorgonTextureCommon}, GorgonRectangleF, GorgonColor?, GorgonRectangleF?, GorgonSampler?, GorgonBlendState?, GorgonShader?)" path="/remarks/para[@type='pso']"/>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following code shows how to render textures over the entire viewport using the blitter.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Create the blitter once, when the application is initialized.
    /// GorgonTextureBlitter blitter = new(graphics);
    ///
    /// // ... Later, when recording commands.
    /// // The command list must have a render target, viewport, and scissor rectangle assigned.
    /// commandList.SetRenderTarget(renderTarget)
    ///            .SetViewport(viewport)
    ///            .SetScissorRectangle(scissor);
    ///
    /// // Render a background image over the entire viewport.
    /// blitter.BlitFullScreen(commandList, backgroundView);
    ///
    /// // Render an overlay on top of the background, using linear filtering and alpha blending.
    /// blitter.BlitFullScreen(commandList, overlayView, GorgonSampler.Linear(commandList.Graphics), GorgonBlendState.Default);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="Blit"/>
    public void BlitFullScreen(GorgonCommandList commandList, IGorgonTextureView<GorgonTextureCommon> texture, GorgonSampler? sampler = null, GorgonBlendState ? blendState = null)
    {
        if (commandList.Viewports.IsEmpty)
        {
            Graphics.Log.PrintWarning($"There are no viewports set on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (commandList.ScissorRectangles.IsEmpty)
        {
            Graphics.Log.PrintWarning($"There are no scissor rectangles set on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (commandList.RenderTargets.Length == 0)
        {
            Graphics.Log.PrintWarning($"There are no render targets bound on the command list '{commandList.Name}'. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        if (!texture.Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            Graphics.Log.PrintWarning($"The texture '{texture.Texture.Name}' is a multisampled texture. The blitter cannot blit these until they are resolved into a non-multisampled texture. Nothing will be drawn.", LoggingLevel.Intermediate);
            return;
        }

        sampler ??= GorgonSampler.Default(Graphics);
        blendState ??= GorgonBlendState.NoBlending;        

        BuildPsoKey(blendState, commandList.RenderTargets, Graphics.SharedResources.FullScreenBlitterPixelShader, out PsoKey key);

        if (!key.Equals(_currentFullScreenPso.Key))
        {
            if (!_cachedFullScreenPsos.TryGetValue(key, out GorgonGraphicsPso? value))
            {
                BuildFullScreenPSO(commandList, blendState, in key);
            }
            else
            {
                _currentFullScreenPso = (key, value);
            }
        }

        FullScreenRenderData data = new()
        {
            TextureHandle = texture.GetViewHandle(),
            SamplerHandle = sampler.GetViewHandle(),
            NoBlending = (blendState == GorgonBlendState.NoBlending) ? 1 : 0
        };

        UpdateFullScreenDrawCall(texture);

        commandList.WriteConstant(GorgonGraphics.MaxRootCbvCount - 1, in data)
                   .Draw(_fullScreenDrawCall);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureBlitter"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with the blitter.</param>
    /// <remarks>
    /// <inheritdoc cref="GorgonTextureBlitter" path="/remarks/para[@type='creation']"/>
    /// </remarks>
    public GorgonTextureBlitter(GorgonGraphics graphics)
    {
        Graphics = graphics;

        _indexBuffer = new GorgonIndexBuffer(Graphics, "Gorgon Blitter Index Buffer", new GorgonIndexBufferInfo(sizeof(short) * 6, false));
        _vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<Vertex>(Graphics, "Gorgon Blitter Vertex Buffer", 4);
        _psoFactory = new GorgonGraphicsPsoFactory(Graphics);
        _defaultPixelShader = Graphics.SharedResources.BlitterPixelShader;

        Span<short> indices = [
            0, 1, 2, 2, 3, 0
        ];

        GorgonCommandList initData = Graphics.GetCommandList("Gorgon Blitter Data Initialization Command List")
                                             .CopyRange(indices, _indexBuffer);
        Graphics.Submit(initData);
        Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);
    }
}

