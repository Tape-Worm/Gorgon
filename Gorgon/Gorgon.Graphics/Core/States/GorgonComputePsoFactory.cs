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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 20, 2026 4:58:49 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A factory used to create, cache, and reuse <see cref="GorgonComputePso"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// Creating a pipeline state object is an expensive operation, because the compute shader is compiled into a form that the video adapter can use. To avoid paying that cost more than once, this factory 
/// stores each <see cref="GorgonComputePso"/> that it creates by name. When a pipeline state object is requested with a name that is already in the factory, the existing object is returned instead of 
/// creating a new one. The factory also keeps track of the compute shaders used by its pipeline state objects.
/// </para>
/// <para>
/// Pipeline state objects remain in the factory until the <see cref="GorgonPsoFactoryCommon{GorgonComputePso}.FlushCache"/> method is called, or the factory is disposed. Flushing the factory destroys 
/// the underlying state for every pipeline state object it contains. If the application is still holding on to a pipeline state object after that happens, it can no longer be used. If the application 
/// does not dispose of the factory, then it will be disposed when the <see cref="GorgonGraphics"/> interface it is associated with is disposed.
/// </para>
/// <para>
/// The contents of the factory can be saved to a stream or file with a codec (e.g. <see cref="Codecs.GorgonCodecComputePsoCache"/>), and loaded again at a later time. This avoids the cost of building the 
/// pipeline state objects again when the application restarts.
/// </para>
/// </remarks>
/// <seealso cref="GorgonComputePso"/>
/// <seealso cref="Codecs.GorgonCodecComputePsoCache"/>
public unsafe sealed class GorgonComputePsoFactory
    : GorgonPsoFactoryCommon<GorgonComputePso>
{
    /// <inheritdoc/>
    private protected override void OnCompile(GorgonComputePso pso)
    {
        ComPtr<ID3D12PipelineState> result = default;
        HRESULT err;

        Graphics.Log.Print($"Creating D3D PSO object for '{pso.Name}'...", LoggingLevel.Verbose);

        CD3DX12_PIPELINE_STATE_STREAM_ROOT_SIGNATURE rootSig = new(Graphics.D3DRootSignature.Get());

        ReadOnlySpan<byte> csBlob = pso.ComputeShader.ShaderData;

        fixed (byte* csPtr = csBlob, psoPtr = pso.PsoCachedBlob)
        {
            CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO cached = pso.PsoCachedBlob.Length == 0 ? new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO()
                                                                                   : new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO(new D3D12_CACHED_PIPELINE_STATE
                                                                                   {
                                                                                       CachedBlobSizeInBytes = (nuint)pso.PsoCachedBlob.Length,
                                                                                       pCachedBlob = psoPtr
                                                                                   });
            CD3DX12_PIPELINE_STATE_STREAM_CS csState = new(new D3D12_SHADER_BYTECODE(csPtr, (nuint)csBlob.Length));            

            CD3DX12_PIPELINE_STATE_STREAM6 stream = new()
            {
                pRootSignature = rootSig,
                CachedPSO = cached,
                CS = csState,
                // Unused
                PrimitiveTopologyType = new CD3DX12_PIPELINE_STATE_STREAM_PRIMITIVE_TOPOLOGY(),
                RTVFormats = new CD3DX12_PIPELINE_STATE_STREAM_RENDER_TARGET_FORMATS(),
                DSVFormat = new CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL_FORMAT(),
                RasterizerState = new CD3DX12_PIPELINE_STATE_STREAM_RASTERIZER2(),                
                BlendState = new CD3DX12_PIPELINE_STATE_STREAM_BLEND_DESC(),
                DepthStencilState = new CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL2(new D3D12_DEPTH_STENCIL_DESC2()
                {
                    DepthEnable = false,
                    StencilEnable = false,
                }),
                SerializedRootSignature = new CD3DX12_PIPELINE_STATE_STREAM_SERIALIZED_ROOT_SIGNATURE(),
                StreamOutput = new CD3DX12_PIPELINE_STATE_STREAM_STREAM_OUTPUT(),
                NodeMask = new CD3DX12_PIPELINE_STATE_STREAM_NODE_MASK(0),
                InputLayout = new CD3DX12_PIPELINE_STATE_STREAM_INPUT_LAYOUT(),
                ViewInstancingDesc = new CD3DX12_PIPELINE_STATE_STREAM_VIEW_INSTANCING(),
                Flags = new CD3DX12_PIPELINE_STATE_STREAM_FLAGS(),
                SampleDesc = new CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_DESC(),
                SampleMask = new CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_MASK(),
                IBStripCutValue = new CD3DX12_PIPELINE_STATE_STREAM_IB_STRIP_CUT_VALUE(),
                VS = new CD3DX12_PIPELINE_STATE_STREAM_VS(),
                PS = new CD3DX12_PIPELINE_STATE_STREAM_PS(),
                GS = new CD3DX12_PIPELINE_STATE_STREAM_GS(),
                DS = new CD3DX12_PIPELINE_STATE_STREAM_DS(),
                HS = new CD3DX12_PIPELINE_STATE_STREAM_HS(),
                AS = new CD3DX12_PIPELINE_STATE_STREAM_AS(),
                MS = new CD3DX12_PIPELINE_STATE_STREAM_MS(),
            };

            D3D12_PIPELINE_STATE_STREAM_DESC desc = new()
            {
                pPipelineStateSubobjectStream = &stream,
                SizeInBytes = (nuint)sizeof(CD3DX12_PIPELINE_STATE_STREAM6)
            };

            err = Graphics.D3DDevice.Get()->CreatePipelineState(&desc, Win32.__uuidof<ID3D12PipelineState>(), (void**)result.GetAddressOf());

            // If, for some reason, the hardware/driver data is no longer valid, then force a recompile from scratch.
            if ((err == D3D12.D3D12_ERROR_ADAPTER_NOT_FOUND) || (err == D3D12.D3D12_ERROR_DRIVER_VERSION_MISMATCH))
            {
                if (err == D3D12.D3D12_ERROR_DRIVER_VERSION_MISMATCH)
                {
                    Graphics.Log.PrintWarning($"The driver version for the pipeline state object '{pso.Name}' is incorrect, this object will be recompiled and the state cache updated. Please remove the old state cache.", LoggingLevel.Intermediate);
                }

                if (err == D3D12.D3D12_ERROR_ADAPTER_NOT_FOUND)
                {
                    Graphics.Log.PrintWarning($"The GPU used for the pipeline state object '{pso.Name}' is incorrect, this object will be recompiled and the state cache updated. Please remove the old state cache.", LoggingLevel.Intermediate);
                }

                // Try to rebuild without our cached PSO data.
                stream.CachedPSO = new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO();

                desc = new()
                {
                    pPipelineStateSubobjectStream = &stream,
                    SizeInBytes = (nuint)sizeof(CD3DX12_PIPELINE_STATE_STREAM6)
                };

                // If we still have a failure, then we're out of luck.
                pso.PsoCachedBlob = [];
                err = Graphics.D3DDevice.Get()->CreatePipelineState(&desc, Win32.__uuidof<ID3D12PipelineState>(), (void**)result.ReleaseAndGetAddressOf());
            }

            if (err.FAILED)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_ERROR_BUILDING_PSO, pso.Name));
            }

            result.SetD3DDebugName($"Gorgon D3D12 Compute PSO '{pso.Name}'");

            pso.SetComPtr(result);
        }

        // We already have a blob ready to go, no point in grabbing it again.
        if (pso.PsoCachedBlob.Length != 0)
        {
            return;
        }

        using ComPtr<ID3DBlob> blob = default;

        err = pso.D3DPso.Get()->GetCachedBlob(blob.GetAddressOf());

        if (err.FAILED)
        {
            // We want failure to retrieve the blob to be a failure condition.
            // So, reset the pointer on the PSO so we don't leak. 
            result.Dispose();
            pso.SetComPtr(default);
            err.ThrowIfFailed(GorgonResult.CannotCreate, Resources.GORGFX_ERR_ERROR_BUILDING_PSO, pso.Name);
        }

        GorgonPtr<byte> blobPtr = blob.ToGorgonPtr<byte>();
        pso.PsoCachedBlob = blobPtr.ToSpan();
    }

    /// <inheritdoc/>
    private protected override void OnAddShaderToCache(GorgonComputePso pso, HashSet<GorgonShader> shaderList) => shaderList.Add(pso.ComputeShader);

    /// <inheritdoc/>
    private protected override void OnUnload(GorgonComputePso pso)
    {
        if (!pso.IsCompiled)
        {
            return;
        }

        pso.D3DPso.Dispose();
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonComputePso"/>, or retrieve an existing one from the factory.
    /// </summary>
    /// <param name="name">The name of the pipeline state object.</param>
    /// <param name="computeShader">The compute shader to assign.</param>
    /// <param name="psoBlob">The compiled binary data for the pipeline state object, or an empty array if there is none. If the data cannot be used by the video adapter or driver, then it is discarded, and the pipeline state object is compiled from its state instead.</param>
    /// <returns>A new <see cref="GorgonComputePso"/> object if no other object exists with the same name, or the existing <see cref="GorgonComputePso"/> if the name already exists in the factory.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="name"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the pipeline state object could not be created by the video adapter.</exception>    
    /// <seealso cref="GorgonComputePso"/>
    internal GorgonComputePso CreateOrGetPso(string name, GorgonShader computeShader, byte[] psoBlob)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(name);

        if (TryGetCachedPso(name, out GorgonComputePso? result))
        {
            if (result.ComputeShader != computeShader)
            {
                Graphics.Log.PrintWarning($"A PSO with the name of '{name}' is already cached, but has a different compute shader. The cached PSO will be returned instead.", LoggingLevel.Intermediate);
            }

            if (!result.IsCompiled)
            {
                Graphics.Log.PrintWarning($"The PSO '{name}' is cached and will be used, however it is not compiled yet. It will be compiled now, and may impact performance.", LoggingLevel.Verbose);
                OnCompile(result);
            }

            return result;
        }

        result = new GorgonComputePso(Graphics, name, computeShader)
        {
            PsoCachedBlob = psoBlob
        };

        return Cache(result);
    }

    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, byte[])" path="/summary"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, byte[])" path="/param[@name!='psoBlob']"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, byte[])" path="/returns"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, byte[])" path="/exception"/>
    /// <remarks>
    /// <para>
    /// The <paramref name="name"/> is used as the key for the pipeline state object in the factory, and it is not case sensitive. If the factory already contains a pipeline state object with the same 
    /// <paramref name="name"/>, then that object is returned, and the <paramref name="computeShader"/> is ignored. If the existing pipeline state object has a different compute shader, then a warning 
    /// will be written to the log.
    /// </para>
    /// <para>
    /// If the <paramref name="name"/> is not in the factory, then a new pipeline state object is created from the <paramref name="computeShader"/>. The new pipeline state object is compiled immediately, 
    /// which can take a small amount of time. For this reason, it is best to create pipeline state objects up front instead of during rendering.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputePso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputePso CreateOrGetPso(string name, GorgonShader computeShader) => CreateOrGetPso(name, computeShader, []);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonComputePsoFactory"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this factory.</param>
    public GorgonComputePsoFactory(GorgonGraphics graphics)
        : base(graphics) => this.RegisterDisposable(graphics);
}
