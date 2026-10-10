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
// Created: June 14, 2026 4:31:28 PM
//

using System.Diagnostics.CodeAnalysis;
using Gorgon.Core;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pipeline state object for the graphics pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline state object (PSO) defines a state for the rendering pipeline used by a draw call. It sets up shaders, blending states, rasterizer state, depth/stencil state, etc... that are required for 
/// rendering data. For example, rendering a person, and a ghost of a person can be done by simply creating a PSO for the standard person, and a PSO that enables blending to make the character look like a 
/// ghost. 
/// </para>
/// <para>
/// Pipeline state objects perform final compilation of shaders and set up all the required state for rendering. Because of this, they can take a small amount of time to compile. This is no problem for a 
/// small application, but as the number of pipeline states grows, this can take a significant amount of time. The best practice for using pipeline state objects is similar to the previous version of Gorgon: 
/// Create the pipeline state objects up front instead of during rendering to minimize the cost. 
/// </para>
/// <para>
/// Pipeline state objects are immutable and can only be created by calling <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/> on the 
/// <see cref="GorgonGraphicsPsoFactory"/> interface and passing a <see cref="GorgonGraphicsPsoBuilder"/> to that method. 
/// </para>
/// <para>
/// The <see cref="GorgonGraphicsPsoFactory"/> controls the lifetime of the pipeline state object. It will remain usable until the factory cache is flushed, which destroys the underlying state for every 
/// object in the cache. If the application is still holding on to a pipeline state object after that happens, it can no longer be used to render, and its <see cref="IsCompiled"/> property will return 
/// <b>false</b>. 
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPsoFactory"/>
/// <seealso cref="GorgonGraphicsPsoBuilder"/>
public sealed class GorgonGraphicsPso
    : IGorgonNamedObject, IEquatable<GorgonGraphicsPso>
{    
    private ComPtr<ID3D12PipelineState> _d3dPso;

    private byte[] _psoBlob = [];

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
    internal ref ComPtr<ID3D12PipelineState> D3DPso => ref _d3dPso;    

    /// <summary>
    /// Property to return the PSO blob holding the cached compiled blob data.
    /// </summary>
    public ReadOnlySpan<byte> PsoCachedBlob 
    {
        get => _psoBlob;
        internal set => _psoBlob = (value.Length == 0 ? [] : value.ToArray());
    }

    /// <summary>
    /// Property to return the graphics interface associated with this object.
    /// </summary>    
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return whether the PSO has been compiled yet or not.
    /// </summary>
    public bool IsCompiled => !_d3dPso.IsNull;

    /// <summary>
    /// Property to return the format(s) for the values written by the pixel shader into the render targets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each index in this list corresponds to a render target slot, and that slot matches the 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target</a> semantic index for the value returned from the 
    /// <see cref="PixelShader"/>. For example, the format at index 0 is for the pixel shader output marked with 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target0</a>, the format at index 1 is for the output marked with 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target1</a>, and so on. This list can hold up to 
    /// <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> formats.
    /// </para>
    /// <para>
    /// When a draw call that uses this pipeline state object is executed, the formats of the render target views bound to the <see cref="GorgonCommandList"/> must match the formats in the corresponding slots 
    /// of this list. If they do not match, then the rendering results are undefined. When debugging is enabled, a warning will also be reported in the debug output.
    /// </para>
    /// <para>
    /// If this list is empty, then the pipeline state object will not write to any render target. This is useful when rendering only to a depth/stencil buffer. For example, a depth pass for a shadow map could 
    /// use a pipeline state object with no output formats, and a <see cref="DepthStencilFormat"/> of <see cref="BufferFormat.D32_Float"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="DepthStencilFormat"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonIndexedDrawCall"/>
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
    /// Property to return the format of the depth/stencil buffer expected by the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a draw call that uses this pipeline state object is executed, the format of the depth/stencil view bound to the <see cref="GorgonCommandList"/> must match this format. If they do not match, then 
    /// the rendering results are undefined. When debugging is enabled, a warning will also be reported in the debug output.
    /// </para>
    /// <para>
    /// The default value is <see cref="BufferFormat.Unknown"/>, which means the pipeline state object does not expect a depth/stencil buffer. In that case, depth testing, depth bounds testing, and stencil 
    /// testing are disabled in the <see cref="DepthStencilState"/>. When this value is set to any other format, the format will contain a depth component if depth testing or depth bounds testing is enabled, 
    /// and a stencil component if stencil testing is enabled.
    /// </para>
    /// </remarks>
    /// <seealso cref="DepthStencilState"/>
    /// <seealso cref="OutputFormats"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonIndexedDrawCall"/>
    public BufferFormat DepthStencilFormat
    {
        get;
        internal set;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the multisampling information expected for the render targets and depth/stencil buffer.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// This value defines the number of samples per pixel, and the quality level for those samples, that the pipeline state object expects when rendering. When a draw call that uses this pipeline state object 
    /// is executed, the multisample count and quality of the render target views and depth/stencil view bound to the <see cref="GorgonCommandList"/> must match this value. If they do not match, then the 
    /// rendering results are undefined. When debugging is enabled, a warning will also be reported in the debug output.
    /// </para>
    /// <para type="common">
    /// The count and quality must be supported by the formats in <see cref="OutputFormats"/> and <see cref="DepthStencilFormat"/>. Applications can determine the maximum supported values for a format by 
    /// looking up the format in <see cref="GorgonGraphics.FormatSupport"/> and reading the <see cref="GorgonBufferFormatSupport.MaxMultipleSampleValues"/> property.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonMultisampleInfo.NoMultisampling"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="MultisampleMask"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    /// <seealso cref="GorgonBufferFormatSupport"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonIndexedDrawCall"/>
    public GorgonMultisampleInfo Multisample
    {
        get;
        internal set;
    } = GorgonMultisampleInfo.NoMultisampling;

    /// <summary>
    /// Property to return the mask used to determine which samples are updated in the render targets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each bit in this value corresponds to a sample within a pixel. Bit 0 is for the first sample, bit 1 is for the second sample, and so on. When a bit is set, the corresponding sample will be updated in 
    /// all of the active render targets. When a bit is cleared, the corresponding sample will be left unchanged.
    /// </para>
    /// <para>
    /// This mask is always applied, even when the <see cref="Multisample"/> value is set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>. In that case, each pixel only has a single sample, so clearing 
    /// bit 0 will prevent anything from being written to the render targets.
    /// </para>
    /// <para>
    /// The default value is -1 (<c>0xFFFFFFFF</c>), which updates every sample.
    /// </para>
    /// </remarks>
    /// <seealso cref="Multisample"/>
    public int MultisampleMask
    {
        get;
        internal set;
    } = -1;

    /// <summary>
    /// Property to return whether alpha to coverage is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alpha to coverage is a multisampling technique that is most useful when there are several overlapping polygons that use transparency to define their edges (e.g. dense foliage). It can also be used to 
    /// define detailed silhouettes for sprites that would otherwise be opaque.
    /// </para>
    /// <para>
    /// When this value is set to <b>true</b>, the alpha component of the value returned from the <see cref="PixelShader"/> for 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target0</a> is converted into a coverage mask. This mask is 
    /// combined with the coverage of the primitive and the <see cref="MultisampleMask"/> to determine which samples are updated in all of the active render targets. The alpha value written to the first render 
    /// target is not changed by this process, and alpha to coverage works independently of whether blending is enabled in the <see cref="BlendStates"/>.
    /// </para>
    /// <para>
    /// An alpha value of 0 (or less) will produce no coverage, and an alpha value of 1 (or greater) will produce full coverage. How the values in between are converted into a coverage mask is determined by 
    /// the video hardware, and some hardware may dither the result. An alpha value of NaN will produce no coverage.
    /// </para>
    /// <para>
    /// If the pixel shader outputs a value using <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Coverage</a>, then alpha 
    /// to coverage is disabled.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Multisample"/>
    public bool IsAlphaToCoverageEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return whether independent blending is enabled for the render targets.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// When this value is set to <b>true</b>, each render target will use the blend state in the corresponding slot of the <see cref="BlendStates"/> list. When this value is set to <b>false</b>, only the 
    /// first blend state in the <see cref="BlendStates"/> list is used, and it is applied to all of the render targets. The remaining blend states are ignored.
    /// </para>
    /// <para type="common">
    /// If the first blend state has logic operations enabled (see <see cref="GorgonBlendState.IsLogicEnabled"/>), then this value must be <b>false</b>. Logic operations cannot be mixed with blending across 
    /// multiple render targets, and the same logic operation must be applied to all of the render targets.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonBlendState"/>
    public bool IsIndependentBlendingEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the state used to rasterize primitives.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// The rasterizer state controls how primitives are converted into pixels before they are sent to the pixel shader. This includes which faces are culled, whether triangles are filled or drawn as 
    /// wireframe, how depth bias is applied, whether depth clipping is enabled, and how lines are rasterized.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonRasterState.Default"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRasterState"/>
    public GorgonRasterState RasterizerState
    {
        get;
        internal set;
    } = GorgonRasterState.Default;

    /// <summary>
    /// Property to return the blend states for the render targets.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// Each index in this list corresponds to a render target slot, in the same order as the <see cref="OutputFormats"/>. The blend state at an index controls how the value returned from the pixel shader is 
    /// combined with the existing contents of the render target in that slot.
    /// </para>
    /// <para type="common">
    /// This list always contains <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> blend states, regardless of the number of <see cref="OutputFormats"/>. Any slot that was not assigned a blend state 
    /// will contain <see cref="GorgonBlendState.NoBlending"/>.
    /// </para>
    /// <para type="common">
    /// If <see cref="IsIndependentBlendingEnabled"/> is <b>false</b>, then only the first blend state in this list is used, and it is applied to all of the render targets.
    /// </para>
    /// <para>
    /// The default value for every slot is <see cref="GorgonBlendState.NoBlending"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonBlendState"/>
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
    /// Property to return the state used for depth/stencil testing.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// The depth/stencil state controls how the depth/stencil buffer is tested and updated when rendering. This includes whether depth testing is enabled and the comparison function it uses, whether depth 
    /// values are written, whether stencil testing is enabled and the stencil operations used for front and back faces, and whether depth bounds testing is enabled.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonDepthStencilState.Default"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="DepthStencilFormat"/>
    /// <seealso cref="GorgonDepthStencilState"/>
    public GorgonDepthStencilState DepthStencilState
    {
        get;
        internal set;
    } = GorgonDepthStencilState.Default;

    /// <summary>
    /// Property to return the name for this pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is used by the <see cref="GorgonGraphicsPsoFactory"/> as a unique identifier for caching purposes. This allows users to reuse PSOs without having to recreate them.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    public string Name
    {
        get;
        internal set;
    } = string.Empty;

    /// <summary>
    /// Property to return the vertex shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The vertex shader is executed for each vertex in a draw call. The values returned from the vertex shader are passed to the next active shader stage, or to the rasterizer if no other shader stages are 
    /// active.
    /// </para>
    /// <para>
    /// All pipeline state objects have a vertex shader, so this value will never be <b>null</b>.
    /// </para>
    /// </remarks>
    [NotNull]
    public GorgonShader? VertexShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the pixel shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The values returned from the pixel shader are written to the render targets described by the <see cref="OutputFormats"/>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// For each <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target</a> value returned from the pixel shader, the 
    /// <see cref="OutputFormats"/> should contain a format in the corresponding slot that is compatible with the type of the returned value. For example, a pixel shader that returns a <c>float4</c> requires a 
    /// floating-point or normalized format (e.g. <see cref="BufferFormat.R8G8B8A8_UNorm"/>), while a pixel shader that returns a <c>uint4</c> requires an unsigned integer format (e.g. 
    /// <see cref="BufferFormat.R8G8B8A8_UInt"/>).
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// A pixel shader is optional. When this value is <b>null</b>, no pixel shader is executed. This is useful when rendering only to a depth/stencil buffer (e.g. rendering a depth pass for a shadow map).
    /// </para>
    /// </remarks>
    public GorgonShader? PixelShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the geometry shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The geometry shader is executed for each primitive in a draw call, and can output zero or more primitives. It receives its input from the <see cref="VertexShader"/>, or the <see cref="DomainShader"/> 
    /// when tessellation is used. The values returned from the geometry shader are passed to the rasterizer.
    /// </para>
    /// <para>
    /// A geometry shader is optional. When this value is <b>null</b>, the primitives are passed to the rasterizer unchanged.
    /// </para>
    /// </remarks>
    public GorgonShader? GeometryShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the hull shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// The hull shader is the first stage of tessellation. It operates on each patch in a draw call, transforming the control points received from the <see cref="VertexShader"/>, and calculating the 
    /// tessellation factors used to subdivide the patch. The results are passed to the tessellator and the <see cref="DomainShader"/>.
    /// </para>
    /// <para>
    /// A hull shader is optional. When a hull shader is used, a <see cref="DomainShader"/> must also be used, and the <see cref="PrimitiveType"/> must be one of the patch list types (e.g. 
    /// <see cref="PrimitiveType.PatchListWith3ControlPoints"/>).
    /// </para>
    /// </remarks>
    /// <seealso cref="DomainShader"/>
    public GorgonShader? HullShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the domain shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// The domain shader is the last stage of tessellation. It is executed for each point generated by the tessellator, and uses the control points from the <see cref="HullShader"/> to calculate the final 
    /// position and attributes of the vertex at that point. The values returned from the domain shader are passed to the next active shader stage, or to the rasterizer if no other shader stages are active.
    /// </para>
    /// <para>
    /// A domain shader is optional. When a domain shader is used, a <see cref="HullShader"/> must also be used.
    /// </para>
    /// </remarks>
    /// <seealso cref="HullShader"/>
    public GorgonShader? DomainShader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the type of primitive rendered with the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para type="common">
    /// This value serves two purposes. It defines the type of primitive (e.g. triangle list, line strip, etc...) that is rendered by a draw call that uses this pipeline state object. It is also used to 
    /// determine the primitive topology type (point, line, triangle, or patch) for the pipeline state object itself.
    /// </para>
    /// <para type="common">
    /// When a <see cref="HullShader"/> and <see cref="DomainShader"/> are used, this value must be one of the patch list types (e.g. <see cref="PrimitiveType.PatchListWith3ControlPoints"/>).
    /// </para>
    /// <para>
    /// The default value is <see cref="PrimitiveType.TriangleList"/>.
    /// </para>
    /// </remarks>
    public PrimitiveType PrimitiveType
    {
        get;
        internal set;
    } = PrimitiveType.TriangleList;

    /// <summary>
    /// Property to return the index value used to restart a strip of primitives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the <see cref="PrimitiveType"/> is a strip type (e.g. <see cref="PrimitiveType.TriangleStrip"/> or <see cref="PrimitiveType.LineStrip"/>), and a <see cref="GorgonIndexedDrawCall"/> is used to 
    /// render, an index with this value will end the current strip and start a new one. This allows multiple strips to be rendered with a single draw call. For all other primitive types, this value is always 
    /// <see cref="IndexBufferStripCutIdentifier.Disabled"/>.
    /// </para>
    /// <para type="common">
    /// Use <see cref="IndexBufferStripCutIdentifier.StopWith16BitMax"/> when the index buffer contains 16-bit indices, and <see cref="IndexBufferStripCutIdentifier.StopWith32BitMax"/> when the index buffer 
    /// contains 32-bit indices. If this value does not match the size of the indices in the index buffer, then the results are undefined.
    /// </para>
    /// <para>
    /// The default value is <see cref="IndexBufferStripCutIdentifier.Disabled"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Core.IndexBufferStripCutIdentifier"/>
    /// <seealso cref="PrimitiveType"/>
    public IndexBufferStripCutIdentifier IndexBufferStripCutIdentifier
    {
        get;
        internal set;
    } = IndexBufferStripCutIdentifier.Disabled;

    /// <summary>
    /// Function to set the COM pointer for the pipeline state object.
    /// </summary>
    /// <param name="ptr">The COM pointer to the pipeline state object.</param>
    internal void SetComPtr(ComPtr<ID3D12PipelineState> ptr) => _d3dPso = ptr;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonGraphicsPso pso ? Equals(pso) : base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(Graphics);
        hash.Add(Name, StringComparer.OrdinalIgnoreCase);
        hash.Add(VertexShader);
        hash.Add(PixelShader);
        hash.Add(GeometryShader);
        hash.Add(DomainShader);
        hash.Add(HullShader);
        hash.Add(IndexBufferStripCutIdentifier);
        hash.Add(IsAlphaToCoverageEnabled);
        hash.Add(IsIndependentBlendingEnabled);
        hash.Add(Multisample);
        hash.Add(MultisampleMask);
        hash.Add(PrimitiveType);
        hash.Add(RasterizerState);
        hash.Add(DepthStencilFormat);
        hash.Add(DepthStencilState);

        for (int i = 0; i < BlendStates.Length; ++i)
        {
            hash.Add(BlendStates[i]);
        }

        for (int i = 0; i < OutputFormats.Length; ++i)
        {
            hash.Add(OutputFormats[i]);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(GorgonGraphicsPso? other) => (this == other) || (other is not null
        && other.Graphics == Graphics
        && string.Equals(other.Name, Name, StringComparison.OrdinalIgnoreCase)
        && other.PrimitiveType == PrimitiveType
        && other.OutputFormats.Length == OutputFormats.Length
        && other.DepthStencilFormat == DepthStencilFormat
        && other.VertexShader == VertexShader
        && other.PixelShader == PixelShader
        && other.GeometryShader == GeometryShader
        && other.DomainShader == DomainShader
        && other.HullShader == HullShader
        && other.IndexBufferStripCutIdentifier == IndexBufferStripCutIdentifier
        && other.IsAlphaToCoverageEnabled == IsAlphaToCoverageEnabled
        && other.IsIndependentBlendingEnabled == IsIndependentBlendingEnabled        
        && other.OutputFormats.SequenceEqual(OutputFormats)
        && other.Multisample.Equals(Multisample)
        && other.MultisampleMask == MultisampleMask        
        && other.RasterizerState.Equals(RasterizerState)
        && other.BlendStates.SequenceEqual(BlendStates)
        && other.DepthStencilState == DepthStencilState);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsPso"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object associated with this object.</param>
    internal GorgonGraphicsPso(GorgonGraphics graphics)
    {
        Array.Fill(RWFormats, BufferFormat.Unknown);
        Array.Fill(RWBlendStates, GorgonBlendState.NoBlending);
        Graphics = graphics;
    }
}
