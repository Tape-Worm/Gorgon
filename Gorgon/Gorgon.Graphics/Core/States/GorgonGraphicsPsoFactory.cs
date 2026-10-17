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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A factory used to create, cache, and reuse <see cref="GorgonGraphicsPso"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// Creating a pipeline state object is an expensive operation, because the shaders and states are compiled into a form that the video adapter can use. To avoid paying that cost more than once, this factory 
/// stores each <see cref="GorgonGraphicsPso"/> that it creates by name. When a pipeline state object is requested with a name that is already in the factory, the existing object is returned instead of 
/// creating a new one. The factory also keeps track of the shaders, and the rasterizer, blend, and depth/stencil states used by its pipeline state objects.
/// </para>
/// <para>
/// Pipeline state objects remain in the factory until the <see cref="FlushCache"/> method is called, or the factory is disposed. Flushing the factory destroys the underlying state for every pipeline state 
/// object it contains. If the application is still holding on to a pipeline state object after that happens, it can no longer be used to render. If the application does not dispose of the factory, then it 
/// will be disposed when the <see cref="GorgonGraphics"/> interface it is associated with is disposed.
/// </para>
/// <para>
/// The contents of the factory can be saved to a stream or file with a codec (e.g. <see cref="Codecs.GorgonCodecGraphicsPsoCache"/>), and loaded again at a later time. This avoids the cost of building the 
/// pipeline state objects again when the application restarts.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonGraphicsPsoBuilder"/>
/// <seealso cref="Codecs.GorgonCodecGraphicsPsoCache"/>
public unsafe sealed class GorgonGraphicsPsoFactory
    : IDisposable
{
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<string, GorgonGraphicsPso> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<GorgonShader> _shaderCache = [];
    private readonly HashSet<GorgonRasterState> _rasterStates = [];
    private readonly HashSet<GorgonBlendState> _blendStates = [];
    private readonly HashSet<GorgonDepthStencilState> _depthStencilStates = [];

    /// <summary>
    /// Property to return the pipeline state objects that are stored in the factory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pipeline state objects are keyed by name, and the names are not case sensitive.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The dictionary returned is not thread safe. Do not create PSOs, or flush the factory, in other threads while iterating through the dictionary.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    public IReadOnlyDictionary<string, GorgonGraphicsPso> PipelineStateObjects => _cache;

    /// <summary>
    /// Property to return the graphics interface associated with this cache.
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
            this.UnregisterDisposable(Graphics);
            FlushCache();
        }
    }

    /// <summary>
    /// Function to retrieve the output formats for the PSO.
    /// </summary>
    /// <param name="pso">The PSO being compiled.</param>
    /// <returns>The render target format streaming sub object.</returns>
    private CD3DX12_PIPELINE_STATE_STREAM_RENDER_TARGET_FORMATS GetOutputFormats(GorgonGraphicsPso pso)
    {
        int formatCount = pso.OutputFormats.Length;

        if (formatCount == 0)
        {
            return new();
        }

        DXGI_FORMAT* rtvFormats = stackalloc DXGI_FORMAT[formatCount];

        for (int i = 0; i < formatCount; ++i)
        {
            rtvFormats[i] = (DXGI_FORMAT)pso.OutputFormats[i];
        }

        return new(new D3D12_RT_FORMAT_ARRAY(rtvFormats, (uint)formatCount));
    }

    /// <summary>
    /// Function to compile the pipeline state object.
    /// </summary>
    /// <param name="pso">The PSO to build from.</param>
    /// <remarks>
    /// <para>
    /// This will rebuild the pipeline state object on the GPU and prepare it for use.
    /// </para>
    /// </remarks>
    private void Compile(GorgonGraphicsPso pso)
    {
        ComPtr<ID3D12PipelineState> result = default;
        HRESULT err;

        Graphics.Log.Print($"Creating D3D PSO object for '{pso.Name}'...", LoggingLevel.Verbose);

        D3D12_DEPTH_STENCIL_DESC2 dsStateDesc = pso.DepthStencilState.GetDesc();
        D3D12_RASTERIZER_DESC2 rasterStateDesc = pso.RasterizerState.GetDesc();
        D3D12_BLEND_DESC blendDesc = D3D12_BLEND_DESC.DEFAULT;
        blendDesc.AlphaToCoverageEnable = pso.IsAlphaToCoverageEnabled;
        blendDesc.IndependentBlendEnable = pso.IsIndependentBlendingEnabled;

        for (int i = 0; i < pso.BlendStates.Length; ++i)
        {
            blendDesc.RenderTarget[i] = pso.BlendStates[i].GetDesc();
        }

        CD3DX12_PIPELINE_STATE_STREAM_ROOT_SIGNATURE rootSig = new(Graphics.D3DRootSignature.Get());
        CD3DX12_PIPELINE_STATE_STREAM_PRIMITIVE_TOPOLOGY primTop = new(pso.PrimitiveType.ToTopologyType());
        CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL2 dssState = new(in dsStateDesc);
        CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL_FORMAT dssFmtState = new((DXGI_FORMAT)pso.DepthStencilFormat);
        CD3DX12_PIPELINE_STATE_STREAM_RASTERIZER2 rState = new(in rasterStateDesc);
        CD3DX12_PIPELINE_STATE_STREAM_BLEND_DESC blendState = new(in blendDesc);
        CD3DX12_PIPELINE_STATE_STREAM_RENDER_TARGET_FORMATS rtvFormats = GetOutputFormats(pso);
        CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_DESC sampler = new(pso.Multisample.ToDXGI());
        CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_MASK sampleMask = new((uint)pso.MultisampleMask);

        ReadOnlySpan<byte> vsBlob = pso.VertexShader is null ? [] : pso.VertexShader.ShaderData;
        ReadOnlySpan<byte> psBlob = pso.PixelShader is null ? [] : pso.PixelShader.ShaderData;
        ReadOnlySpan<byte> gsBlob = pso.GeometryShader is null ? [] : pso.GeometryShader.ShaderData;
        ReadOnlySpan<byte> hsBlob = pso.HullShader is null ? [] : pso.HullShader.ShaderData;
        ReadOnlySpan<byte> dsBlob = pso.DomainShader is null ? [] : pso.DomainShader.ShaderData;

        fixed (byte* vsPtr = vsBlob, psPtr = psBlob, gsPtr = gsBlob, hsPtr = hsBlob, dsPtr = dsBlob, psoPtr = pso.PsoCachedBlob)
        {
            CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO cached = pso.PsoCachedBlob.Length == 0 ? new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO()
                                                                                   : new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO(new D3D12_CACHED_PIPELINE_STATE
                                                                                   {
                                                                                       CachedBlobSizeInBytes = (nuint)pso.PsoCachedBlob.Length,
                                                                                       pCachedBlob = psoPtr
                                                                                   });
            CD3DX12_PIPELINE_STATE_STREAM_VS vsState = new(new D3D12_SHADER_BYTECODE(vsPtr, (nuint)vsBlob.Length));
            CD3DX12_PIPELINE_STATE_STREAM_PS psState = new(new D3D12_SHADER_BYTECODE(psPtr, (nuint)psBlob.Length));
            CD3DX12_PIPELINE_STATE_STREAM_GS gsState = new(new D3D12_SHADER_BYTECODE(gsPtr, (nuint)gsBlob.Length));
            CD3DX12_PIPELINE_STATE_STREAM_HS hsState = new(new D3D12_SHADER_BYTECODE(hsPtr, (nuint)hsBlob.Length));
            CD3DX12_PIPELINE_STATE_STREAM_DS dsState = new(new D3D12_SHADER_BYTECODE(dsPtr, (nuint)dsBlob.Length));

            CD3DX12_PIPELINE_STATE_STREAM6 stream = new()
            {
                pRootSignature = rootSig,
                CachedPSO = cached,
                PrimitiveTopologyType = primTop,
                VS = vsState,
                PS = psState,
                GS = gsState,
                HS = hsState,
                DS = dsState,
                RasterizerState = rState,
                DepthStencilState = dssState,
                BlendState = blendState,
                RTVFormats = rtvFormats,
                DSVFormat = dssFmtState,
                SampleDesc = sampler,
                SampleMask = sampleMask,
                IBStripCutValue = new CD3DX12_PIPELINE_STATE_STREAM_IB_STRIP_CUT_VALUE((D3D12_INDEX_BUFFER_STRIP_CUT_VALUE)pso.IndexBufferStripCutIdentifier),
                // Unused
                SerializedRootSignature = new CD3DX12_PIPELINE_STATE_STREAM_SERIALIZED_ROOT_SIGNATURE(),
                StreamOutput = new CD3DX12_PIPELINE_STATE_STREAM_STREAM_OUTPUT(),
                NodeMask = new CD3DX12_PIPELINE_STATE_STREAM_NODE_MASK(0),
                InputLayout = new CD3DX12_PIPELINE_STATE_STREAM_INPUT_LAYOUT(),
                ViewInstancingDesc = new CD3DX12_PIPELINE_STATE_STREAM_VIEW_INSTANCING(),
                Flags = new CD3DX12_PIPELINE_STATE_STREAM_FLAGS(),
                CS = new CD3DX12_PIPELINE_STATE_STREAM_CS(),
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

            result.SetD3DDebugName($"Gorgon D3D12 Graphics PSO '{pso.Name}'");

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
            err.ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_ERROR_BUILDING_PSO, pso.Name));
        }

        GorgonPtr<byte> blobPtr = blob.ToGorgonPtr<byte>();
        pso.PsoCachedBlob = blobPtr.ToSpan();
    }

    /// <summary>
    /// Function to add a shader to the shader cache list.
    /// </summary>
    /// <param name="shader">The shader to add.</param>
    private void AddToShaderList(GorgonShader? shader)
    {
        if (shader is null)
        {
            return;
        }

        _shaderCache.Add(shader);
    }

    /// <summary>
    /// Function to add a PSO and its shaders to the cache.
    /// </summary>
    /// <param name="pso">The PSO with the shaders to add.</param>
    /// <returns>The cache object.</returns>
    private GorgonGraphicsPso Cache(GorgonGraphicsPso pso)
    {
        Debug.Assert(pso.VertexShader is not null, $"No vertex shader for pso {pso.Name}");

        Compile(pso);

        using (_cacheLock.EnterScope())
        {
            // Check to ensure that a thread hasn't already created our PSO.
            if (_cache.TryGetValue(pso.Name, out GorgonGraphicsPso? existing))
            {
                pso.D3DPso.Dispose();
                return existing;
            }

            _cache[pso.Name] = pso;

            AddToShaderList(pso.VertexShader);
            AddToShaderList(pso.PixelShader);
            AddToShaderList(pso.GeometryShader);
            AddToShaderList(pso.HullShader);
            AddToShaderList(pso.DomainShader);

            _rasterStates.Add(pso.RasterizerState);

            for (int i = 0; i < pso.BlendStates.Length; ++i)
            {
                _blendStates.Add(pso.BlendStates[i]);
            }

            _depthStencilStates.Add(pso.DepthStencilState);

            return pso;
        }
    }

    /// <summary>
    /// Function to locate a cached shader by using its hash code and binary blob.
    /// </summary>
    /// <param name="hashCode">The hashcode of the shader to look up.</param>
    /// <param name="blob">The binary blob to compare.</param>
    /// <returns>A <see cref="GorgonShader"/> matching the hashcode and blob data, or <b>null</b> if no shader was found in the cache.</returns>
    internal GorgonShader? FindShaderWithSignature(int hashCode, ReadOnlySpan<byte> blob)
    {
        using (_cacheLock.EnterScope())
        {
            if (_shaderCache.Count == 0)
            {
                return null;
            }

            foreach (GorgonShader cachedShader in _shaderCache)
            {
                if (cachedShader.CheckShaderBlob(hashCode, blob))
                {
                    return cachedShader;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Function to locate a cached raster state.
    /// </summary>
    /// <param name="state">The state to look up.</param>
    /// <returns>The state, if found, or <b>null</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GorgonRasterState? FindRasterState(GorgonRasterState state)
    {
        using (_cacheLock.EnterScope())
        {
            if (_rasterStates.TryGetValue(state, out GorgonRasterState? cached))
            {
                return cached;
            }

            return null;
        }
    }

    /// <summary>
    /// Function to locate a cached blend state.
    /// </summary>
    /// <param name="state">The state to look up.</param>
    /// <returns>The state, if found, or <b>null</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GorgonBlendState? FindBlendState(GorgonBlendState state)
    {
        using (_cacheLock.EnterScope())
        {
            if (_blendStates.TryGetValue(state, out GorgonBlendState? cached))
            {
                return cached;
            }

            return null;
        }
    }

    /// <summary>
    /// Function to locate a cached depth/stencil state.
    /// </summary>
    /// <param name="state">The state to look up.</param>
    /// <returns>The state, if found, or <b>null</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GorgonDepthStencilState? FindDepthStencilState(GorgonDepthStencilState state)
    {
        using (_cacheLock.EnterScope())
        {
            if (_depthStencilStates.TryGetValue(state, out GorgonDepthStencilState? cached))
            {
                return cached;
            }

            return null;
        }
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonGraphicsPso"/>, or retrieve an existing one from the factory.
    /// </summary>
    /// <param name="name">The name of the pipeline state object.</param>
    /// <param name="vertexShader">The vertex shader for the pipeline state object.</param>
    /// <param name="builder">The pipeline state object builder used to create the object, if necessary.</param>
    /// <param name="psoBlob">The compiled binary data for the pipeline state object, or an empty array if there is none. If the data cannot be used by the video adapter or driver, then it is discarded, and the pipeline state object is compiled from its state instead.</param>
    /// <returns>A new <see cref="GorgonGraphicsPso"/> object if no other object exists with the same name, or the existing <see cref="GorgonGraphicsPso"/> if the name already exists in the factory.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="name"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the pipeline state object could not be created by the video adapter.</exception>
    /// <inheritdoc cref="GorgonGraphicsPsoBuilder.Build(string, GorgonShader)" path="/exception"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    internal GorgonGraphicsPso CreateOrGetPso(string name, GorgonShader vertexShader, GorgonGraphicsPsoBuilder builder, byte[] psoBlob)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(name);

        GorgonGraphicsPso? result;

        using (_cacheLock.EnterScope())
        {
            if (_cache.TryGetValue(name, out result))
            {
                if (!builder.IsPsoDataSame(result, vertexShader))
                {
                    Graphics.Log.PrintWarning($"A PSO with the name of '{name}' is already cached, but has different settings than the builder. The cached PSO will be returned instead.", LoggingLevel.Intermediate);
                }

                if (!result.IsCompiled)
                {
                    Graphics.Log.PrintWarning($"The PSO '{name}' is cached and will be used, however it is not compiled yet. It will be compiled now, and may impact performance.", LoggingLevel.Verbose);
                    Compile(result);
                }

                return result;
            }
        }

        result = builder.Build(name, vertexShader);
        result.PsoCachedBlob = psoBlob;

        return Cache(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to flush the factory, and remove all of the stored <see cref="GorgonGraphicsPso"/> objects, <see cref="GorgonShader"/> objects, and states.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will wait for the GPU to finish its work, and then destroy the underlying state for every <see cref="GorgonGraphicsPso"/> in the factory. If the application is still holding on to a 
    /// <see cref="GorgonGraphicsPso"/> after this method is called, it can no longer be used to render, and its <see cref="GorgonGraphicsPso.IsCompiled"/> property will return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonShader"/>
    public void FlushCache()
    {
        Graphics.Log.Print("Flushing the Pipeline State Object cache and Shader cache.", LoggingLevel.Simple);

        Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);

        using (_cacheLock.EnterScope())
        {
            foreach (GorgonGraphicsPso pso in _cache.Values)
            {
                ref ComPtr<ID3D12PipelineState> psoPtr = ref pso.D3DPso;
                psoPtr.Dispose();
            }

            _cache.Clear();
            _shaderCache.Clear();
            _rasterStates.Clear();
            _blendStates.Clear();
            _depthStencilStates.Clear();

            _rasterStates.Add(GorgonRasterState.Default);
            _blendStates.Add(GorgonBlendState.Default);
            _depthStencilStates.Add(GorgonDepthStencilState.Default);
        }
    }

    /// <summary>
    /// Function to determine if the requested <see cref="GorgonGraphicsPso"/> exists in the factory.
    /// </summary>
    /// <param name="psoName">The name of the PSO to look up.</param>
    /// <returns><b>true</b> if the PSO is already cached, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="psoName"/> is not case sensitive. If the <paramref name="psoName"/> is empty, then this method will return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasPso(string psoName)
    {
        if (string.IsNullOrWhiteSpace(psoName))
        {
            return false;
        }

        using (_cacheLock.EnterScope())
        {
            return _cache.ContainsKey(psoName);
        }
    }

    /// <summary>
    /// Function to attempt to return a <see cref="GorgonGraphicsPso"/> from the factory.
    /// </summary>
    /// <param name="psoName">The name of the pipeline state object to return.</param>
    /// <param name="pso">The resulting <see cref="GorgonGraphicsPso"/> if found, or <b>null</b> if not.</param>
    /// <returns><b>true</b> if the <see cref="GorgonGraphicsPso"/> was found, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="psoName"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="psoName"/> is not case sensitive.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetPso(string psoName, [NotNullWhen(true)] out GorgonGraphicsPso? pso) 
    {        
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(psoName);

        using (_cacheLock.EnterScope())
        {
            return _cache.TryGetValue(psoName, out pso);
        }
    }

    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder, byte[])" path="/summary"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder, byte[])" path="/param[@name!='psoBlob']"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder, byte[])" path="/returns"/>
    /// <inheritdoc cref="CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder, byte[])" path="/exception"/>
    /// <remarks>
    /// <para>
    /// The <paramref name="name"/> is used as the key for the pipeline state object in the factory, and it is not case sensitive. If the factory already contains a pipeline state object with the same 
    /// <paramref name="name"/>, then that object is returned, and the <paramref name="vertexShader"/> and <paramref name="builder"/> are ignored. If the existing pipeline state object has different settings 
    /// than the <paramref name="builder"/>, then a warning will be written to the log.
    /// </para>
    /// <para>
    /// If the <paramref name="name"/> is not in the factory, then a new pipeline state object is created from the state in the <paramref name="builder"/>, and the <paramref name="vertexShader"/>. The new 
    /// pipeline state object is compiled immediately, which can take a small amount of time. For this reason, it is best to create pipeline state objects up front instead of during rendering.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonGraphicsPsoBuilder"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonGraphicsPso CreateOrGetPso(string name, GorgonShader vertexShader, GorgonGraphicsPsoBuilder builder) => CreateOrGetPso(name, vertexShader, builder, []);

    /// <summary>
    /// Function to iterate through the <see cref="GorgonGraphicsPso"/> objects in the factory, and find one based on the predicate filter passed.
    /// </summary>
    /// <param name="predicate">The predicate filter used to locate the pipeline state object.</param>
    /// <returns>The first <see cref="GorgonGraphicsPso"/> object that matches the <paramref name="predicate"/>, or <b>null</b> if no match was found.</returns>
    public GorgonGraphicsPso? FindPso(Func<GorgonGraphicsPso, bool> predicate)
    {
        using (_cacheLock.EnterScope())
        {
            if (_cache.Count == 0)
            {
                return null;
            }

            foreach (GorgonGraphicsPso pso in _cache.Values)
            {
                if (predicate(pso))
                {
                    return pso;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Function to retrieve a copy of all of the raster states in the factory.
    /// </summary>
    /// <returns>A dictionary containing each raster state, and an index ID for that raster state.</returns>
    /// <remarks>
    /// <para>
    /// The index IDs are sequential, start at 0, and are only valid for the returned dictionary.
    /// </para>
    /// </remarks>
    public Dictionary<GorgonRasterState, int> GetRasterStates()
    {
        using (_cacheLock.EnterScope())
        {
            Dictionary<GorgonRasterState, int> result = [];

            int i = 0;
            foreach (GorgonRasterState state in _rasterStates)
            {
                result[state] = i++;
            }

            return result;
        }
    }

    /// <summary>
    /// Function to retrieve a copy of all of the blend states in the factory.
    /// </summary>
    /// <returns>A dictionary containing each blend state, and an index ID for that blend state.</returns>
    /// <inheritdoc cref="GetRasterStates" path="/remarks"/>
    public Dictionary<GorgonBlendState, int> GetBlendStates()
    {
        using (_cacheLock.EnterScope())
        {
            Dictionary<GorgonBlendState, int> result = [];

            int i = 0;
            foreach (GorgonBlendState state in _blendStates)
            {
                result[state] = i++;
            }

            return result;
        }
    }

    /// <summary>
    /// Function to retrieve a copy of all of the depth/stencil states in the factory.
    /// </summary>
    /// <returns>A dictionary containing each depth/stencil state, and an index ID for that depth/stencil state.</returns>
    /// <inheritdoc cref="GetRasterStates" path="/remarks"/>
    public Dictionary<GorgonDepthStencilState, int> GetDepthStencilStates()
    {
        using (_cacheLock.EnterScope())
        {
            Dictionary<GorgonDepthStencilState, int> result = [];

            int i = 0;
            foreach (GorgonDepthStencilState state in _depthStencilStates)
            {
                result[state] = i++;
            }

            return result;
        }
    }

    /// <summary>
    /// Function to retrieve a copy of the unique shaders used by all of the <see cref="GorgonGraphicsPso"/> objects in the factory.
    /// </summary>
    /// <returns>A dictionary containing each shader, and an index ID for that shader.</returns>
    /// <inheritdoc cref="GetRasterStates" path="/remarks"/>
    public Dictionary<GorgonShader, int> GetShaders()
    {
        using (_cacheLock.EnterScope())
        {
            Dictionary<GorgonShader, int> result = [];

            int i = 0;
            foreach (GorgonShader shader in _shaderCache)
            {
                result[shader] = i++;
            }

            return result;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsPsoFactory"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this factory.</param>
    public GorgonGraphicsPsoFactory(GorgonGraphics graphics)
    { 
        Graphics = graphics;
        this.RegisterDisposable(graphics);

        _rasterStates.Add(GorgonRasterState.Default);
        _blendStates.Add(GorgonBlendState.Default);
        _depthStencilStates.Add(GorgonDepthStencilState.Default);
    }
}
