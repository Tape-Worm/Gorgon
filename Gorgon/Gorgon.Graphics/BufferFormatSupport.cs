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
// Created: January 21, 2016 8:19:37 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

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
		None = D3D11.FormatSupport.None,
		/// <summary>
		/// Format can be used in a generic buffer.
		/// </summary>
		Buffer = D3D11.FormatSupport.Buffer,
		/// <summary>
		/// Format can be used in a vertex buffer.
		/// </summary>
		VertexBuffer = D3D11.FormatSupport.InputAssemblyVertexBuffer,
		/// <summary>
		/// Format can be used in an index buffer.
		/// </summary>
		IndexBuffer = D3D11.FormatSupport.InputAssemblyIndexBuffer,
		/// <summary>
		/// Format can be used in a streaming output buffer.
		/// </summary>
		StreamOutBuffer = D3D11.FormatSupport.StreamOutputBuffer,
		/// <summary>
		/// Format can be used for a 1D texture.
		/// </summary>
		Texture1D = D3D11.FormatSupport.Texture1D,
		/// <summary>
		/// Format can be used for a 2D texture.
		/// </summary>
		Texture2D = D3D11.FormatSupport.Texture2D,
		/// <summary>
		/// Format can be used for a 3D texture.
		/// </summary>
		Texture3D = D3D11.FormatSupport.Texture3D,
		/// <summary>
		/// Format can be used for a cube texture.
		/// </summary>
		TextureCube = D3D11.FormatSupport.TextureCube,
		/// <summary>
		/// Format can be used with the HLSL <c>Load</c> function.
		/// </summary>
		ShaderLoad = D3D11.FormatSupport.ShaderLoad,
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
		ShaderSample = D3D11.FormatSupport.ShaderSample,
		/// <summary>
		/// Format can be used with the HLSL <c>SampleCmp</c> and <c>SampleCmpLevelZero</c> functions.
		/// </summary>
		ShaderSampleCompare = D3D11.FormatSupport.ShaderSampleComparison,
		/// <summary>
		/// Reserved.
		/// </summary>
		ShaderSampleMonoText = D3D11.FormatSupport.ShaderSampleMonoText,
		/// <summary>
		/// Format can be used with mip maps.
		/// </summary>
		MipMaps = D3D11.FormatSupport.Mip,
		/// <summary>
		/// Format can be used with automatic generation of mip maps.
		/// </summary>
		MipMapAutoGen = D3D11.FormatSupport.Mip,
		/// <summary>
		/// Format can be used for a render target.
		/// </summary>
		RenderTarget = D3D11.FormatSupport.RenderTarget,
		/// <summary>
		/// Format can support blend operations.
		/// </summary>
		Blendable = D3D11.FormatSupport.Blendable,
		/// <summary>
		/// Format can be used for a depth/stencil buffer.
		/// </summary>
		DepthStencil = D3D11.FormatSupport.DepthStencil,
		/// <summary>
		/// Format can be used when locking for access by the CPU.
		/// </summary>
		CpuLockable = D3D11.FormatSupport.CpuLockable,
		/// <summary>
		/// Format can be used with multi-sample anti-aliasing (MSAA) resolve operations.
		/// </summary>
		MultiSampleResolve = D3D11.FormatSupport.MultisampleResolve,
		/// <summary>
		/// Format can be used for display on the screen.
		/// </summary>
		Display = D3D11.FormatSupport.Display,
		/// <summary>
		/// Format cannot be cast to another format.
		/// </summary>
		CastWithinBitLayout = D3D11.FormatSupport.CastWithinBitLayout,
		/// <summary>
		/// Format can be used for a multi-sampled render target.
		/// </summary>
		MultiSampledRenderTarget = D3D11.FormatSupport.MultisampleRenderTarget,
		/// <summary>
		/// Format can be used for a multi-sampled texture and read by the HLSL <c>Load</c> function.
		/// </summary>
		MultiSampledShaderLoad = D3D11.FormatSupport.MultisampleLoad,
		/// <summary>
		/// Format can be used with the HLSL <c>Gather</c> method.
		/// </summary>
		ShaderGather = D3D11.FormatSupport.ShaderGather,
		/// <summary>
		/// Format supports casting if the resource is a back buffer.
		/// </summary>
		BackBufferCast = D3D11.FormatSupport.BackBufferCast,
		/// <summary>
		/// Format can be used with unordered access views (UAV).
		/// </summary>
		UnorderedAccess = D3D11.FormatSupport.TypedUnorderedAccessView,
		/// <summary>
		/// Format can be used with the HLSL <c>Gather</c> with comparison method.
		/// </summary>
		ShaderGatherCompare = D3D11.FormatSupport.ShaderGatherComparison
	}

	/// <summary>
	/// Defines the Unordered Access functionality for a compute shader resource that are available for a given <see cref="BufferFormat"/>.
	/// </summary>
	[Flags]
	public enum BufferFormatComputeSupport
	{
		/// <summary>
		/// No unordered resource operations are supported for this format.
		/// </summary>
		None = D3D11.ComputeShaderFormatSupport.None,
		/// <summary>
		/// Format can be used with an atomic add.
		/// </summary>
		AtomicAdd = D3D11.ComputeShaderFormatSupport.AtomicAdd,
		/// <summary>
		/// Format can be used with atomic bitwise operations.
		/// </summary>
		AtomicBitwiseOperations = D3D11.ComputeShaderFormatSupport.AtomicBitwiseOperations,
		/// <summary>
		/// Format can be used with atomic compare with store or exchange.
		/// </summary>
		AtomicCompareStoreOrExchange = D3D11.ComputeShaderFormatSupport.AtomicCompareStoreOrCompareExchange,
		/// <summary>
		/// Format can be used with atomic exchange.
		/// </summary>
		AtomicExchange = D3D11.ComputeShaderFormatSupport.AtomicExchange,
		/// <summary>
		/// Format can be used with atomic signed minimum and maximum.
		/// </summary>
		AtomicSignedMinMax = D3D11.ComputeShaderFormatSupport.AtomicSignedMinimumOrMaximum,
		/// <summary>
		/// Format can be used with atomic unsigned minimum and maximum.
		/// </summary>
		AtomicUnsignedMinMax = D3D11.ComputeShaderFormatSupport.AtomicUnsignedMinimumOrMaximum,
		/// <summary>
		/// Format supports a typed load.
		/// </summary>
		TypedLoad = D3D11.ComputeShaderFormatSupport.TypedLoad,
		/// <summary>
		/// Format supports a typed store.
		/// </summary>
		TypedStore = D3D11.ComputeShaderFormatSupport.TypedStore
	}
}
