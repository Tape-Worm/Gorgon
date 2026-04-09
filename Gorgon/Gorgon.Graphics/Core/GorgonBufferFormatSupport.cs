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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 24, 2025 11:08:34 PM
//

using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of supported values for a given format.
/// </summary>
/// <param name="Format">The format.</param>
/// <param name="IsBufferFormat">Buffer resources supported.</param>
/// <param name="IsVertexBufferFormat">Vertex buffers supported.</param>
/// <param name="IsIndexBufferFormat">Index buffers supported.</param>
/// <param name="IsStreamOutFormat">Streaming output buffers supported.</param>
/// <param name="Is1DTextureFormat">1D texture resources supported.</param>
/// <param name="Is2DTextureFormat">2D texture resources supported.</param>
/// <param name="Is3DTextureFormat">3D texture resources supported.</param>
/// <param name="IsCubeTextureFormat">Cube texture resources supported.</param>
/// <param name="SupportsShaderLoadInstruction">The HLSL Load function for texture objects is supported.</param>
/// <param name="SupportsShaderSampleInstruction">The HLSL Sample function for texture objects is supported.</param>
/// <param name="SupportsShaderSampleCmpInstruction">The HLSL SampleCmp and SampleCmpLevelZero functions for texture objects are supported.</param>
/// <param name="SupportsMipMaps">Mipmaps are supported.</param>
/// <param name="IsRenderTargetFormat">Render targets are supported.</param>
/// <param name="CanBlend">Blend operations supported.</param>
/// <param name="IsDepthStencilFormat">Depth stencils supported.</param>
/// <param name="CanResolveMultisample">Multisample antialiasing (MSAA) resolve operations are supported.</param>
/// <param name="IsDisplayFormat">Format can be displayed on screen.</param>
/// <param name="CanCastWithinBitLayout">Format can be cast to another format.</param>
/// <param name="IsMultisampleRenderTargetFormat">Format can be used as a multi-sampled render target.</param>
/// <param name="SupportsMultisampleLoadInstruction">Format can be used as a multi-sampled texture and read into a shader with the HLSL Load function.</param>
/// <param name="SupportsGatherInstruction">Format can be used with the HLSL gather function.</param>
/// <param name="CanCastBackBufferResource">Format supports casting when the resource is a back buffer.</param>
/// <param name="IsTypedReadWriteViewFormat">Format can be used for an read/write access view.</param>
/// <param name="SupportsShaderGatherCmpInstruction">Format can be used with the HLSL gather with comparison function.</param>
/// <param name="IsDecoderFormat">Format can be used with the decoder output.</param>
/// <param name="IsEncoderFormat">Format can be used with the video encoder.</param>
/// <param name="IsVideoProcessorOutputFormat">Format can be used with the video processor output.</param>
/// <param name="IsVideoProcessorInputFormat">Format can be used with the video processor input.</param>
/// <param name="SupportsReadWriteViewAtomicAdd">Format supports atomic add.</param>
/// <param name="SupportsReadWriteViewAtomicBitwiseOperations">Format supports atomic bitwise operations.</param>
/// <param name="SupportsReadWriteViewAtomicCompareStoreOrExchange">Format supports atomic compare with store or exchange.</param>
/// <param name="SupportsReadWriteViewAtomicExchange">Format supports atomic exchange.</param>
/// <param name="SupportsReadWriteViewAtomicSignedMinOrMax">Format supports atomic min and max.</param>
/// <param name="SupportsReadWriteViewAtomicUnsignedMinOrMax">Format supports atomic unsigned min and max.</param>
/// <param name="SupportsReadWriteViewTypedLoad">Format supports a typed load.</param>
/// <param name="SupportsReadWriteViewTypedStore">Format supports a typed store.</param>
/// <param name="SupportsBlendingLogicOperators">Format supports logic operations in blend state.</param>
/// <param name="IsTiledFormat">Format supports tiled resources.</param>
/// <param name="SupportsMultiplaneOverlay">Format supports multi-plane overlays.</param>
/// <param name="PlaneCount">The number of planes supported by the format.</param>
/// <param name="MaxMultipleSampleValues">The maximum count and quality for this format.</param>
/// <remarks>
/// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
/// </remarks>
public record class GorgonBufferFormatSupport(BufferFormat Format, bool IsBufferFormat,
    bool IsVertexBufferFormat,
    bool IsIndexBufferFormat,
    bool IsStreamOutFormat,
    bool Is1DTextureFormat,
    bool Is2DTextureFormat,
    bool Is3DTextureFormat,
    bool IsCubeTextureFormat,
    bool SupportsShaderLoadInstruction,
    bool SupportsShaderSampleInstruction,
    bool SupportsShaderSampleCmpInstruction,
    bool SupportsMipMaps,
    bool IsRenderTargetFormat,
    bool CanBlend,
    bool IsDepthStencilFormat,
    bool CanResolveMultisample,
    bool IsDisplayFormat,
    bool CanCastWithinBitLayout,
    bool IsMultisampleRenderTargetFormat,
    bool SupportsMultisampleLoadInstruction,
    bool SupportsGatherInstruction,
    bool CanCastBackBufferResource,
    bool IsTypedReadWriteViewFormat,
    bool SupportsShaderGatherCmpInstruction,
    bool IsDecoderFormat,
    bool IsEncoderFormat,
    bool IsVideoProcessorOutputFormat,
    bool IsVideoProcessorInputFormat,
    bool SupportsReadWriteViewAtomicAdd,
    bool SupportsReadWriteViewAtomicBitwiseOperations,
    bool SupportsReadWriteViewAtomicCompareStoreOrExchange,
    bool SupportsReadWriteViewAtomicExchange,
    bool SupportsReadWriteViewAtomicSignedMinOrMax,
    bool SupportsReadWriteViewAtomicUnsignedMinOrMax,
    bool SupportsReadWriteViewTypedLoad,
    bool SupportsReadWriteViewTypedStore,
    bool SupportsBlendingLogicOperators,
    bool IsTiledFormat,
    bool SupportsMultiplaneOverlay,
    byte PlaneCount,
    GorgonMultisampleInfo MaxMultipleSampleValues);
