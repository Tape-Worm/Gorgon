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
// Created: November 16, 2017 12:17:15 PM
// 
#endregion

// ReSharper disable InconsistentNaming
using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Resource support for <see cref="BufferFormat"/> values.
/// </summary>
[Flags]
public enum BufferFormatSupport
{
    /// <summary>
    /// No support.
    /// </summary>
    None = D3D11.FormatSupport.None,
    /// <summary>
    /// <para>
    /// Buffer resources supported.
    /// </para>
    /// </summary>
    Buffer = D3D11.FormatSupport.Buffer,
    /// <summary>
    /// <para>
    /// Vertex buffers supported.
    /// </para>
    /// </summary>
    VertexBuffer = D3D11.FormatSupport.InputAssemblyVertexBuffer,
    /// <summary>
    /// <para>
    /// Index buffers supported.
    /// </para>
    /// </summary>
    IndexBuffer = D3D11.FormatSupport.InputAssemblyIndexBuffer,
    /// <summary>
    /// <para>
    /// Streaming output buffers supported.
    /// </para>
    /// </summary>
    StreamOutBuffer = D3D11.FormatSupport.StreamOutputBuffer,
    /// <summary>
    /// <para>
    /// 1D texture resources supported.
    /// </para>
    /// </summary>
    Texture1D = D3D11.FormatSupport.Texture1D,
    /// <summary>
    /// <para>
    /// 2D texture resources supported.
    /// </para>
    /// </summary>
    Texture2D = D3D11.FormatSupport.Texture2D,
    /// <summary>
    /// <para>
    /// 3D texture resources supported.
    /// </para>
    /// </summary>
    Texture3D = D3D11.FormatSupport.Texture3D,
    /// <summary>
    /// <para>
    /// Cube texture resources supported.
    /// </para>
    /// </summary>
    TextureCube = D3D11.FormatSupport.TextureCube,
    /// <summary>
    /// <para>
    /// The HLSL Load function for texture objects is supported.
    /// </para>
    /// </summary>
    ShaderLoad = D3D11.FormatSupport.ShaderLoad,
    /// <summary>
    /// <para>
    /// The HLSL Sample function for texture objects is supported.
    /// </para>
    /// </summary>
    ShaderSample = D3D11.FormatSupport.ShaderSample,
    /// <summary>
    /// <para>
    /// The HLSL SampleCmp and SampleCmpLevelZero functions for texture objects are supported.
    /// </para>
    /// </summary>
    ShaderSampleComparison = D3D11.FormatSupport.ShaderSampleComparison,
    /// <summary>
    /// <para>
    /// Reserved.
    /// </para>
    /// </summary>
    Reserved = D3D11.FormatSupport.ShaderSampleMonoText,
    /// <summary>
    /// <para>
    /// Mipmaps are supported.
    /// </para>
    /// </summary>
    Mip = D3D11.FormatSupport.Mip,
    /// <summary>
    /// <para>
    /// Automatic generation of mipmaps is supported.
    /// </para>
    /// </summary>
    MipAutogen = D3D11.FormatSupport.MipAutogen,
    /// <summary>
    /// <para>
    /// Render targets are supported.
    /// </para>
    /// </summary>
    RenderTarget = D3D11.FormatSupport.RenderTarget,
    /// <summary>
    /// <para>
    /// Blend operations supported.
    /// </para>
    /// </summary>
    Blendable = D3D11.FormatSupport.Blendable,
    /// <summary>
    /// <para>
    /// Depth stencils supported.
    /// </para>
    /// </summary>
    DepthStencil = D3D11.FormatSupport.DepthStencil,
    /// <summary>
    /// <para>
    /// CPU locking supported.
    /// </para>
    /// </summary>
    CpuLockable = D3D11.FormatSupport.CpuLockable,
    /// <summary>
    /// <para>
    /// Multisample antialiasing (MSAA) resolve operations are supported. For more info, see ID3D11DeviceContex::ResolveSubresource. 
    /// </para>
    /// </summary>
    MultisampleResolve = D3D11.FormatSupport.MultisampleResolve,
    /// <summary>
    /// <para>
    /// Format can be displayed on screen.
    /// </para>
    /// </summary>
    Display = D3D11.FormatSupport.Display,
    /// <summary>
    /// <para>
    /// Format cannot be cast to another format.
    /// </para>
    /// </summary>
    CastWithinBitLayout = D3D11.FormatSupport.CastWithinBitLayout,
    /// <summary>
    /// <para>
    /// Format can be used as a multisampled rendertarget.
    /// </para>
    /// </summary>
    MultisampleRenderTarget = D3D11.FormatSupport.MultisampleRenderTarget,
    /// <summary>
    /// <para>
    /// Format can be used as a multisampled texture and read into a shader with the HLSL load function.
    /// </para>
    /// </summary>
    MultisampleLoad = D3D11.FormatSupport.MultisampleLoad,
    /// <summary>
    /// <para>
    /// Format can be used with the HLSL gather function. This value is available in DirectX 10.1 or higher.
    /// </para>
    /// </summary>
    ShaderGather = D3D11.FormatSupport.ShaderGather,
    /// <summary>
    /// <para>
    /// Format supports casting when the resource is a back buffer.
    /// </para>
    /// </summary>
    BackBufferCast = D3D11.FormatSupport.BackBufferCast,
    /// <summary>
    /// <para>
    /// Format can be used for an unordered access view.
    /// </para>
    /// </summary>
    TypedUnorderedAccessView = D3D11.FormatSupport.TypedUnorderedAccessView,
    /// <summary>
    /// <para>
    /// Format can be used with the HLSL gather with comparison function.
    /// </para>
    /// </summary>
    ShaderGatherComparison = D3D11.FormatSupport.ShaderGatherComparison,
    /// <summary>
    /// <para>
    /// Format can be used with the decoder output.
    /// </para>
    /// </summary>
    DecoderOutput = D3D11.FormatSupport.DecoderOutput,
    /// <summary>
    /// <para>
    /// Format can be used with the video processor output.
    /// </para>
    /// </summary>
    VideoProcessorOutput = D3D11.FormatSupport.VideoProcessorOutput,
    /// <summary>
    /// <para>
    /// Format can be used with the video processor input.
    /// </para>
    /// </summary>
    VideoProcessorInput = D3D11.FormatSupport.VideoProcessorInput,
    /// <summary>
    /// <para>
    /// Format can be used with the video encoder.
    /// </para>
    /// </summary>
    VideoEncoder = D3D11.FormatSupport.VideoEncoder
}