#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: Thursday, January 7, 2016 9:36:20 PM
// 
#endregion

using System;
using D3D12 = SharpDX.Direct3D12;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Defines the resources that are available for a given <see cref="BufferFormat"/>.
	/// </summary>
	[Flags]
	public enum BufferFormatSupport
	{
		/// <summary>
		/// No support.
		/// </summary>
		None = D3D12.FormatSupport1.None,
		/// <summary>
		/// Format can be used in a generic buffer.
		/// </summary>
		Buffer = D3D12.FormatSupport1.Buffer,
		/// <summary>
		/// Format can be used in a vertex buffer.
		/// </summary>
		VertexBuffer = D3D12.FormatSupport1.InputAssemblyVertexBuffer,
		/// <summary>
		/// Format can be used in an index buffer.
		/// </summary>
		IndexBuffer = D3D12.FormatSupport1.InputAssemblyIndexBuffer,
		/// <summary>
		/// Format can be used in a streaming output buffer.
		/// </summary>
		StreamOutBuffer = D3D12.FormatSupport1.StreamOutputBuffer,
		/// <summary>
		/// Format can be used for a 1D texture.
		/// </summary>
		Texture1D = D3D12.FormatSupport1.Texture1D,
		/// <summary>
		/// Format can be used for a 2D texture.
		/// </summary>
		Texture2D = D3D12.FormatSupport1.Texture2D,
		/// <summary>
		/// Format can be used for a 3D texture.
		/// </summary>
		Texture3D = D3D12.FormatSupport1.Texture3D,
		/// <summary>
		/// Format can be used for a cube texture.
		/// </summary>
		TextureCube = D3D12.FormatSupport1.TextureCube,
		/// <summary>
		/// Format can be used with the HLSL <c>Load</c> function.
		/// </summary>
		ShaderLoad = D3D12.FormatSupport1.ShaderLoad,
		/// <summary>
		/// <para>
		/// Format can be used with the HLSL <c>Sample</c> function.
		/// </para>
		/// <para>
		/// <note type="note">
		/// If format is supported as texture resource (<see cref="Texture1D"/>, <see cref="Texture2D"/>, <see cref="Texture3D"/>, or <see cref="TextureCube"/>) but does not support this option, the resource 
		/// can still be used with the <c>Sample</c> function but must use a point filtering sampler to perform the sample.
		/// </note>
		/// </para>
		/// </summary>
		ShaderSample = D3D12.FormatSupport1.ShaderSample,
		/// <summary>
		/// Format can be used with the HLSL <c>SampleCmp</c> and <c>SampleCmpLevelZero</c> functions.
		/// </summary>
		ShaderSampleCompare = D3D12.FormatSupport1.ShaderSampleComparison,
		/// <summary>
		/// Reserved.
		/// </summary>
		ShaderSampleMonoText = D3D12.FormatSupport1.ShaderSampleMonoText,
		/// <summary>
		/// Format can be used with mip maps.
		/// </summary>
		MipMaps = D3D12.FormatSupport1.Mip,
		/// <summary>
		/// Format can be used for a render target.
		/// </summary>
		RenderTarget = D3D12.FormatSupport1.RenderTarget,
		/// <summary>
		/// Format can support blend operations.
		/// </summary>
		Blendable = D3D12.FormatSupport1.Blendable,
		/// <summary>
		/// Format can be used for a depth/stencil buffer.
		/// </summary>
		DepthStencil = D3D12.FormatSupport1.DepthStencil,
		/// <summary>
		/// Format can be used with multi-sample anti-aliasing (MSAA) resolve operations.
		/// </summary>
		MultiSampleResolve = D3D12.FormatSupport1.MultisampleResolve,
		/// <summary>
		/// Format can be used for display on the screen.
		/// </summary>
		Display = D3D12.FormatSupport1.Display,
		/// <summary>
		/// Format cannot be cast to another format.
		/// </summary>
		CastWithinBitLayout = D3D12.FormatSupport1.CastWithinBitLayout,
		/// <summary>
		/// Format can be used for a multi-sampled render target.
		/// </summary>
		MultiSampledRenderTarget = D3D12.FormatSupport1.MultisampleRenderTarget,
		/// <summary>
		/// Format can be used for a multi-sampled texture and read by the HLSL <c>Load</c> function.
		/// </summary>
		MultiSampledShaderLoad = D3D12.FormatSupport1.MultisampleLoad,
		/// <summary>
		/// Format can be used with the HLSL <c>Gather</c> method.
		/// </summary>
		ShaderGather = D3D12.FormatSupport1.ShaderGather,
		/// <summary>
		/// Format supports casting if the resource is a back buffer.
		/// </summary>
		BackBufferCast = D3D12.FormatSupport1.BackBufferCast,
		/// <summary>
		/// Format can be used with unordered access views (UAV).
		/// </summary>
		UnorderedAccess = D3D12.FormatSupport1.TypedUnorderedAccessView,
		/// <summary>
		/// Format can be used with the HLSL <c>Gather</c> with comparison method.
		/// </summary>
		ShaderGatherCompare = D3D12.FormatSupport1.ShaderGatherComparison,
		/// <summary>
		/// Format can be used with decoder output.
		/// </summary>
		DecoderOutput = D3D12.FormatSupport1.DecoderOutput,
		/// <summary>
		/// Format can be used with video processor output.
		/// </summary>
		VideoOutput = D3D12.FormatSupport1.VideoProcessorOutput,
		/// <summary>
		/// Format can be used with video processor input.
		/// </summary>
		VideoInput = D3D12.FormatSupport1.VideoProcessorInput,
		/// <summary>
		/// Format can be used in video encoding.
		/// </summary>
		VideoEncoder = D3D12.FormatSupport1.VideoEncoder
	}

	/// <summary>
	/// Defines the Unordered Access View operations that are available for a given <see cref="BufferFormat"/>.
	/// </summary>
	[Flags]
	public enum BufferFormatUavSupport
	{
		/// <summary>
		/// No unordered resource operations are supported for this format.
		/// </summary>
		None = D3D12.FormatSupport2.None,
		/// <summary>
		/// Format can be used with an atomic add.
		/// </summary>
		AtomicAdd = D3D12.FormatSupport2.UnorderedAccessViewAtomicAdd,
		/// <summary>
		/// Format can be used with atomic bitwise operations.
		/// </summary>
		AtomicBitwiseOperations = D3D12.FormatSupport2.UnorderedAccessViewAtomicBitwiseOperations,
		/// <summary>
		/// Format can be used with atomic compare with store or exchange.
		/// </summary>
		AtomicCompareStoreOrExchange = D3D12.FormatSupport2.UnorderedAccessViewAtomicCompareStoreOrCompareExchange,
		/// <summary>
		/// Format can be used with atomic exchange.
		/// </summary>
		AtomicExchange = D3D12.FormatSupport2.UnorderedAccessViewAtomicExchange,
		/// <summary>
		/// Format can be used with atomic signed minimum and maximum.
		/// </summary>
		AtomicSignedMinMax = D3D12.FormatSupport2.UnorderedAccessViewAtomicSignedMinimumOrMaximum,
		/// <summary>
		/// Format can be used with atomic unsigned minimum and maximum.
		/// </summary>
		AtomicUnsignedMinMax = D3D12.FormatSupport2.UnorderedAccessViewAtomicUnsignedMinimumOrMaximum,
		/// <summary>
		/// Format supports a typed load.
		/// </summary>
		TypedLoad = D3D12.FormatSupport2.UnorderedAccessViewTypedLoad,
		/// <summary>
		/// Format supports a typed store.
		/// </summary>
		TypedStore = D3D12.FormatSupport2.UnorderedAccessViewTypedStore,
		/// <summary>
		/// Format supports logic operations with blending.
		/// </summary>
		OutputMergerLogicOperation = D3D12.FormatSupport2.OutputMergerLogicOperation,
		/// <summary>
		/// Format supports tiled resources.
		/// </summary>
		Tiled = D3D12.FormatSupport2.Tiled,
		/// <summary>
		/// Format supports multi-plane overlays.
		/// </summary>
		MultiPlaneOverlay = D3D12.FormatSupport2.MultiplaneOverlay
	}
}