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
// Created: June 14, 2026 4:31:28 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pipeline state object.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline state object (PSO) defines the current state of the rendering pipeline. It sets up shaders that are required for the scene, blending states, rasterizer state, depth/stencil state, etc... 
/// </para>
/// </remarks>
public sealed unsafe class GorgonGraphicsPso
    : IGorgonNamedObject, IDisposable
{    
    private ComPtr<ID3D12PipelineState1> _d3dPso;

    private bool _disposed;

    /// <summary>
    /// The read/write version of the output format list.
    /// </summary>
    internal readonly BufferFormat[] RWFormats = new BufferFormat[GorgonVideoAdapterInfo.MaxRenderTargetCount];
    /// <summary>
    /// The read/write version of the output format count.
    /// </summary>
    internal int RWFormatCount;
    /// <summary>
    /// The read/write version of the blend states.
    /// </summary>
    internal readonly GorgonBlendState[] RWBlendStates = new GorgonBlendState[GorgonVideoAdapterInfo.MaxRenderTargetCount];

    /// <summary>
    /// Property to return the underlying Direct3D PSO object.
    /// </summary>
    internal ref readonly ComPtr<ID3D12PipelineState1> D3DPso => ref _d3dPso;

    /// <summary>
    /// Property to return the graphics interface associated with this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the format(s) expected to be output from the pixel shader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This informs the PSO that the <see cref="PixelShader"/> will output with this format into the render targets on the given indices/slots. 
    /// </para>
    /// <para>
    /// If the render target view formats bound to the <see cref="GorgonCommandList"/> do not match the formats in the specified slots on this property, then the <see cref="GorgonDrawCall"/> that uses this 
    /// PSO will fail.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDrawCall"/>
    public ReadOnlySpan<BufferFormat> OutputFormats 
    {
        get => RWFormats.AsSpan(0, RWFormatCount);
        internal set
        {
            if (value.Length == 0)
            {
                RWFormatCount = 0;
                Array.Fill(RWFormats, BufferFormat.Unknown);
                return;
            }

            RWFormatCount = value.Length.Min(GorgonVideoAdapterInfo.MaxRenderTargetCount);

            if (RWFormatCount < RWFormats.Length)
            {
                Array.Fill(RWFormats, BufferFormat.Unknown, RWFormatCount, RWFormats.Length - RWFormatCount); 
            }

            value[..RWFormatCount].CopyTo(RWFormats);
        }
    }

    /// <summary>
    /// Property to return the expected depth/stencil format for depth/stencil testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This informs the PSO that this is the format that is expected when reading or writing depth data during depth/stencil testing.
    /// </para>
    /// <para>
    /// If the depth/stencil format bound to the <see cref="GorgonCommandList"/> does not match the depth/stencil format on this property, then the <see cref="GorgonDrawCall"/> that uses this PSO will fail.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDrawCall"/>
    public BufferFormat DepthStencilFormat
    {
        get;
        internal set;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the depth/stencil state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tells the application how to interact with the depth/stencil buffer, if one is present, or allows the application to disable depth/stencil testing.
    /// </para>
    /// <para>
    /// The default value is set to <see cref="GorgonDepthStencilState.Default"/> (No depth/stencil testing).
    /// </para>
    /// </remarks>
    public GorgonDepthStencilState DepthStencilState
    {
        get;
        internal set;
    } = GorgonDepthStencilState.Default;

    /// <summary>
    /// Property to return the multisample mask.
    /// </summary>
    /// <remarks>
    /// This value is only used when the <see cref="Multisample"/> value is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>.
    /// </remarks>
    public int MultisampleMask
    {
        get;
        internal set;
    } = -1;

    /// <summary>
    /// Property to return the multisampling state.
    /// </summary>
    public GorgonMultisampleInfo Multisample
    {
        get;
        internal set;
    } = GorgonMultisampleInfo.NoMultisampling;

    /// <summary>
    /// Property to return whether alpha to coverage is enabled or not for blending.
    /// </summary>
    /// <remarks>
    /// This will use alpha to coverage as a multisampling technique when writing a pixel to a render target. Alpha to coverage is useful in situations where there are multiple overlapping polygons 
    /// that use transparency to define edges.
    /// </remarks>
    public bool IsAlphaToCoverageEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return whether independent render target blending is enabled or not.
    /// </summary>
    /// <remarks>
    /// This will specify whether to use different blending states for each render target. When this value is set to <b>true</b>, each render target blend state will be independent of other render 
    /// target blend states. When this value is set to <b>false</b>, then only the blend state of the first render target is used.
    /// </remarks>
    public bool IsIndependentBlendingEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the rasterization stage state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tells the application how to rasterize geometry while rendering.
    /// </para>
    /// <para>
    /// The default value is set to <see cref="GorgonRasterState.Default"/> (Back face culling, solid fill and default line rasterization).
    /// </para>
    /// </remarks>
    public GorgonRasterState RasterizerState
    {
        get;
        internal set;
    } = GorgonRasterState.Default;

    /// <summary>
    /// Property to return the blend states for the render targets.
    /// </summary>
    public ReadOnlySpan<GorgonBlendState> BlendStates
    {
        get => RWBlendStates;
        internal set
        {
            int count = value.Length.Min(RWBlendStates.Length);

            if (count < RWBlendStates.Length)
            {
                Array.Fill(RWBlendStates, GorgonBlendState.NoBlending, count, RWBlendStates.Length - count);
            }

            value[..count].CopyTo(RWBlendStates);
        }
    }

    /// <summary>
    /// Property to return the name for this pipeline state object.
    /// </summary>
    /// <remarks>
    /// Pipeline state objects should be uniquely named so they can be cached for efficient processing.
    /// </remarks>
    public string Name
    {
        get;
        internal set;
    } = string.Empty;

    /// <summary>
    /// Property to return the vertex shader for the pipeline state object.
    /// </summary>
    public GorgonShader VertexShader
    {
        get;
        internal set;
    } = GorgonShader.NullShader;

    /// <summary>
    /// Property to return the pixel shader for the pipeline state object.
    /// </summary>
    public GorgonShader? PixelShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the geometry shader for the pipeline state object.
    /// </summary>
    public GorgonShader? GeometryShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the hull shader for the pipeline state object.
    /// </summary>
    public GorgonShader? HullShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the domain shader for the pipeline state object.
    /// </summary>
    public GorgonShader? DomainShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the primitive type expected by the PSO.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="PrimitiveType.TriangleList"/>.
    /// </remarks>
    public PrimitiveType PrimitiveType
    {
        get;
        internal set;
    } = PrimitiveType.TriangleList;

    /// <summary>
    /// Property to return the type of index buffer identifier used to restart strips of vertices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value applies only when the <see cref="PrimitiveType"/> topology is a triangle or line strip. It will be ignored if it is any other type.
    /// </para>
    /// <para>
    /// This value defaults to <see cref="IndexBufferStripCutIdentifier.Disabled"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Core.PrimitiveType"/>
    public IndexBufferStripCutIdentifier IndexBufferStripCutIdentifier
    {
        get;
        internal set;
    } = IndexBufferStripCutIdentifier.Disabled;

    /// <summary>
    /// Function to retrieve the pointer to the shader blob data.
    /// </summary>
    /// <param name="shader">The shader to evaluate.</param>
    /// <returns>The pointer to the shader blob data, if available, otherwise <b>null</b>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static D3D12_SHADER_BYTECODE GetShaderPtr(GorgonShader? shader)
    {
        void* ptr = (shader is null) || (shader.ShaderData.Equals(GorgonPtr<byte>.NullPtr)) ? null : (void*)shader.ShaderData;
        nuint size = (nuint)(shader?.ShaderData.Length ?? 0);
        return (ptr is not null && size > 0) ? new(ptr, size) : new(null, 0);
    }

    /// <summary>
    /// Function to retrieve the output formats for the PSO.
    /// </summary>
    /// <returns>The render target format streaming sub object.</returns>
    private CD3DX12_PIPELINE_STATE_STREAM_RENDER_TARGET_FORMATS GetOutputFormats()
    {
        if (RWFormatCount == 0)
        {
            return new();
        }

        DXGI_FORMAT* rtvFormats = stackalloc DXGI_FORMAT[RWFormatCount];

        for (int i = 0; i < RWFormatCount; ++i)
        {
            rtvFormats[i] = (DXGI_FORMAT)RWFormats[i];
        }        

        return new(new D3D12_RT_FORMAT_ARRAY(rtvFormats, (uint)RWFormatCount));
    }

    /// <summary>
    /// Function to build a D3D pipeline state from our PSO data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This rebuilds the internal COM pointer object for the PSO.
    /// </para>
    /// </remarks>
    internal void UpdateD3DPso()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(GorgonGraphicsPso));

        Graphics.Log.Print($"Creating D3D PSO object for '{Name}'...", LoggingLevel.Verbose);

        D3D12_SHADER_BYTECODE vsByteCode = GetShaderPtr(VertexShader);
        D3D12_SHADER_BYTECODE psByteCode = GetShaderPtr(PixelShader);
        D3D12_SHADER_BYTECODE gsByteCode = GetShaderPtr(GeometryShader);
        D3D12_SHADER_BYTECODE dsByteCode = GetShaderPtr(DomainShader);
        D3D12_SHADER_BYTECODE hsByteCode = GetShaderPtr(HullShader);

        D3D12_DEPTH_STENCIL_DESC2 dsStateDesc = DepthStencilState.GetDesc();
        D3D12_RASTERIZER_DESC2 rasterStateDesc = RasterizerState.GetDesc();
        D3D12_BLEND_DESC blendDesc = D3D12_BLEND_DESC.DEFAULT;
        blendDesc.AlphaToCoverageEnable = IsAlphaToCoverageEnabled;
        blendDesc.IndependentBlendEnable = IsIndependentBlendingEnabled;

        for (int i = 0; i < RWBlendStates.Length; ++i)
        {
            blendDesc.RenderTarget[i] = RWBlendStates[i].GetDesc();
        }

        CD3DX12_PIPELINE_STATE_STREAM_ROOT_SIGNATURE rootSig = new(Graphics.D3DRootSignature.Get());
        CD3DX12_PIPELINE_STATE_STREAM_PRIMITIVE_TOPOLOGY primTop = new(PrimitiveType.ToTopologyType());
        CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL2 dssState = new(in dsStateDesc);
        CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL_FORMAT dssFmtState = new((DXGI_FORMAT)DepthStencilFormat);
        CD3DX12_PIPELINE_STATE_STREAM_RASTERIZER2 rState = new(in rasterStateDesc);
        CD3DX12_PIPELINE_STATE_STREAM_BLEND_DESC blendState = new(in blendDesc);
        CD3DX12_PIPELINE_STATE_STREAM_RENDER_TARGET_FORMATS rtvFormats = GetOutputFormats();
        CD3DX12_PIPELINE_STATE_STREAM_VS vsState = new(in vsByteCode);
        CD3DX12_PIPELINE_STATE_STREAM_PS psState = new(in psByteCode);
        CD3DX12_PIPELINE_STATE_STREAM_GS gsState = new(in gsByteCode);
        CD3DX12_PIPELINE_STATE_STREAM_HS hsState = new(in hsByteCode);
        CD3DX12_PIPELINE_STATE_STREAM_DS dsState = new(in dsByteCode);
        CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_DESC sampler = new(Multisample.ToDXGI());
        CD3DX12_PIPELINE_STATE_STREAM_SAMPLE_MASK sampleMask = new((uint)MultisampleMask);
        CD3DX12_PIPELINE_STATE_STREAM_STREAM_OUTPUT streamOut = new();

        CD3DX12_PIPELINE_STATE_STREAM6 stream = new()
        {
            pRootSignature = rootSig,
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
            StreamOutput = streamOut,
            IBStripCutValue = new CD3DX12_PIPELINE_STATE_STREAM_IB_STRIP_CUT_VALUE((D3D12_INDEX_BUFFER_STRIP_CUT_VALUE)IndexBufferStripCutIdentifier),
            // Unused
            SerializedRootSignature = new CD3DX12_PIPELINE_STATE_STREAM_SERIALIZED_ROOT_SIGNATURE(),
            NodeMask = new CD3DX12_PIPELINE_STATE_STREAM_NODE_MASK(0),            
            InputLayout = new CD3DX12_PIPELINE_STATE_STREAM_INPUT_LAYOUT(),            
            ViewInstancingDesc = new CD3DX12_PIPELINE_STATE_STREAM_VIEW_INSTANCING(),
            CachedPSO = new CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO(),
            Flags = new CD3DX12_PIPELINE_STATE_STREAM_FLAGS(),
            CS = new CD3DX12_PIPELINE_STATE_STREAM_CS(),
            AS = new CD3DX12_PIPELINE_STATE_STREAM_AS(),
            MS = new CD3DX12_PIPELINE_STATE_STREAM_MS(),
        };

        ComPtr<ID3D12PipelineState1> pso = default;
        D3D12_PIPELINE_STATE_STREAM_DESC desc = new()
        {
            pPipelineStateSubobjectStream = &stream,
            SizeInBytes = (nuint)sizeof(CD3DX12_PIPELINE_STATE_STREAM6)
        };        

        Graphics.D3DDevice.Get()->CreatePipelineState(&desc, Win32.__uuidof<ID3D12PipelineState>(), (void**)pso.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_ERROR_BUILDING_PSO, Name));

        pso.SetD3DDebugName($"Gorgon D3D12 Graphics PSO '{Name}'");

        _d3dPso.Attach(pso);
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)        
        {
            this.UnregisterDisposable(Graphics);
            Graphics.Log.Print($"Destroying D3D PSO object for '{Name}'...", LoggingLevel.Verbose);
            _disposed = true;
        }

        _d3dPso.Dispose();        
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonGraphicsPso() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsPso"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object associated with this object.</param>
    internal GorgonGraphicsPso(GorgonGraphics graphics)
    {
        this.RegisterDisposable(graphics);
        Array.Fill(RWFormats, BufferFormat.Unknown);
        Array.Fill(RWBlendStates, GorgonBlendState.NoBlending);
        Graphics = graphics;
    }
}
