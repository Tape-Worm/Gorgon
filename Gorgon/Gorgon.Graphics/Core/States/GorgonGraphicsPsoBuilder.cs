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
// Created: June 15, 2026 1:09:18 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Patterns;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to set up the state for a <see cref="GorgonGraphicsPso"/>.
/// </summary>
/// <param name="graphics">The graphics interface that is associated with the pipeline state objects created from this builder.</param>
/// <remarks>
/// <para>
/// This builder is used to define the state for a graphics pipeline state object. The state includes the render target output formats, the depth/stencil format, multisampling, the primitive type, the shaders 
/// (e.g. pixel, geometry, etc...), and the blend, rasterizer, and depth/stencil states. Once the state is set up, the builder is passed to the 
/// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/> method, along with a name and a vertex shader, to create the <see cref="GorgonGraphicsPso"/>.
/// </para>
/// <para>
/// The pipeline state object receives a copy of the state held by this builder. Changing the builder after a pipeline state object is created will not change that pipeline state object. This allows a single 
/// builder to be reused to create many pipeline state objects. To start from a clean slate, call <see cref="Clear"/>, or to start from the state of an existing pipeline state object, call 
/// <see cref="ResetTo(GorgonGraphicsPso)"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonGraphicsPsoFactory"/>
public sealed class GorgonGraphicsPsoBuilder(GorgonGraphics graphics)
{
    private readonly GorgonGraphicsPso _worker = new(graphics);

    /// <summary>
    /// Property to return the graphics instance that is associated with this builder.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;
    
    /// <summary>
    /// Function to copy the state from one pso object to another.
    /// </summary>
    /// <param name="source">The pso to copy from.</param>
    /// <param name="destination">The pso to copy into.</param>
    private static void Copy(GorgonGraphicsPso source, GorgonGraphicsPso destination)
    {
        destination.Name = source.Name;
        destination.OutputFormats = source.OutputFormats;        
        
        destination.PrimitiveType = source.PrimitiveType;
        destination.IndexBufferStripCutIdentifier = source.IndexBufferStripCutIdentifier;

        destination.VertexShader = source.VertexShader;
        destination.PixelShader = source.PixelShader;
        destination.GeometryShader = source.GeometryShader;
        destination.DomainShader = source.DomainShader;
        destination.HullShader = source.HullShader;
        
        destination.DepthStencilState = source.DepthStencilState;
        destination.DepthStencilFormat = source.DepthStencilFormat;

        destination.RasterizerState = source.RasterizerState;

        destination.IsAlphaToCoverageEnabled = source.IsAlphaToCoverageEnabled;        
        destination.IsIndependentBlendingEnabled = source.IsIndependentBlendingEnabled;
        destination.BlendStates = source.BlendStates;

        destination.Multisample = source.Multisample;
        destination.MultisampleMask = source.MultisampleMask;
    }

    /// <summary>
    /// Function to validate the various pieces of the pipeline state object before building.
    /// </summary>
    /// <exception cref="GorgonException">
    /// <para>Thrown if the <see cref="GorgonRasterState.LineRasterizationMode"/> is set to <see cref="LineRasterizationMode.QuadrilateralNarrow"/> and the adapter does not support it.</para>
    /// <para>Thrown if the <see cref="GorgonDepthStencilState.IsDepthBoundsTestingEnabled"/> is <b>true</b> and the adapter does not support depth bounds testing.</para>
    /// <para>Thrown if the <see cref="GorgonStencilOperation.ReadMask"/> or <see cref="GorgonStencilOperation.WriteMask"/> is different between the <see cref="GorgonDepthStencilState.FrontFaceStencilOperation"/> and <see cref="GorgonDepthStencilState.BackFaceStencilOperation"/>, and the adapter does not support independent reference masks between front and back stencil operations.</para>
    /// <para>Thrown if the <see cref="IndependentBlendingEnabled(bool)"/> has been set to <b>true</b>, and the first render target blending slot has its <see cref="GorgonBlendState.IsLogicEnabled"/> flag set to <b>true</b>.</para>
    /// <para>Thrown if a hull shader is assigned without a domain shader, or a domain shader is assigned without a hull shader.</para>
    /// <para>Thrown if a hull and domain shader are assigned, and the primitive type is not one of the patch list types.</para>
    /// <para>Thrown if the index buffer strip cut value is not <see cref="IndexBufferStripCutIdentifier.Disabled"/>, and the primitive type is not a strip type.</para>
    /// </exception>
    private void Validate()
    {
        if ((!Graphics.Adapter.SupportsNarrowQuadrilateralLines) && (_worker.RasterizerState.LineRasterizationMode == LineRasterizationMode.QuadrilateralNarrow))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_NO_NARROW_QUAD_SUPPORT, Graphics.Adapter.Name));
        }

        if ((!Graphics.Adapter.SupportsDepthBoundsTest) && (_worker.DepthStencilState.IsDepthBoundsTestingEnabled))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_NO_DEPTH_BOUNDS_TEST_SUPPORT, Graphics.Adapter.Name));
        }

        if ((!Graphics.Adapter.SupportsIndependentFrontAndBackStencilRef) && (_worker.DepthStencilState.IsStencilEnabled) 
            && ((_worker.DepthStencilState.FrontFaceStencilOperation.ReadMask != _worker.DepthStencilState.BackFaceStencilOperation.ReadMask)
             || (_worker.DepthStencilState.FrontFaceStencilOperation.WriteMask != _worker.DepthStencilState.BackFaceStencilOperation.WriteMask)))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_NO_INDEPENDENT_STENCIL_REF_SUPPORT, Graphics.Adapter.Name));
        }

        if ((_worker.IsIndependentBlendingEnabled) && (_worker.RWBlendStates[0].IsLogicEnabled))
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_PSO_INDEPENDENT_BLEND_LOGIC_ENABLED);
        }

        if ((_worker.HullShader is not null) && (_worker.PrimitiveType.ToTopologyType() != D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_PATCH))
        {        
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_PSO_HULL_DOMAIN_PRIMTYPE_NOT_PATCH, _worker.PrimitiveType));
        }

        if ((_worker.IndexBufferStripCutIdentifier is not Core.IndexBufferStripCutIdentifier.Disabled)
            && (_worker.PrimitiveType is not Core.PrimitiveType.LineStrip and not Core.PrimitiveType.TriangleStrip and not Core.PrimitiveType.LineStripWithAdjacency and not Core.PrimitiveType.TriangleStripWithAdjacency))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INDEX_CUT_NON_STRIP_PRIMITIVE, _worker.PrimitiveType));
        }
    }

    /// <summary>
    /// Function to build a new <see cref="GorgonGraphicsPso"/> from the state in this builder.
    /// </summary>
    /// <param name="name">The name of the pipeline state object.</param>
    /// <param name="vertexShader">The vertex shader to assign to the pipeline state object.</param>
    /// <returns>A new <see cref="GorgonGraphicsPso"/> containing a copy of the state in this builder.</returns>
    /// <inheritdoc cref="Validate" path="/exception"/>
    internal GorgonGraphicsPso Build(string name, GorgonShader vertexShader)
    {
        _worker.Name = name;
        _worker.VertexShader = vertexShader;

        Validate();

        GorgonGraphicsPso result = new(Graphics);

        Copy(_worker, result);

        return result;
    }

    /// <summary>
    /// Function to determine whether the given PSO has the same settings as the PSO settings in this builder.
    /// </summary>
    /// <param name="pso">The PSO to evaluate.</param>
    /// <param name="vertexShader">The vertex shader</param>
    /// <returns><b>true</b> if the settings are the same, <b>false</b> if not.</returns>
    internal bool IsPsoDataSame(GorgonGraphicsPso pso, GorgonShader vertexShader) => pso.Graphics == Graphics
                && pso.PrimitiveType == _worker.PrimitiveType
                && pso.OutputFormats.Length == _worker.OutputFormats.Length
                && pso.DepthStencilFormat == _worker.DepthStencilFormat
                && pso.VertexShader == vertexShader
                && pso.PixelShader == _worker.PixelShader
                && pso.GeometryShader == _worker.GeometryShader
                && pso.DomainShader == _worker.DomainShader
                && pso.HullShader == _worker.HullShader
                && pso.IndexBufferStripCutIdentifier == _worker.IndexBufferStripCutIdentifier
                && pso.IsAlphaToCoverageEnabled == _worker.IsAlphaToCoverageEnabled
                && pso.IsIndependentBlendingEnabled == _worker.IsIndependentBlendingEnabled
                && pso.OutputFormats.SequenceEqual(_worker.OutputFormats)
                && pso.Multisample.Equals(_worker.Multisample)
                && pso.MultisampleMask == _worker.MultisampleMask
                && pso.RasterizerState.Equals(_worker.RasterizerState)
                && pso.BlendStates.SequenceEqual(_worker.BlendStates)
                && pso.DepthStencilState == _worker.DepthStencilState;

    /// <summary>
    /// Function to assign the format for the values written by the pixel shader into a single render target slot.
    /// </summary>
    /// <param name="format">The format for the render target slot.</param>
    /// <param name="renderTargetSlot">[Optional] The render target slot that will receive the format.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="renderTargetSlot"/> is less than 0, or greater than or equal to <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// This assigns the <paramref name="format"/> to the <paramref name="renderTargetSlot"/>, and leaves the formats in the other slots unchanged. The <paramref name="renderTargetSlot"/> matches the 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target</a> semantic index for the value returned from the pixel 
    /// shader. For example, slot 0 is for the pixel shader output marked with 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target0</a>, slot 1 is for the output marked with 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target1</a>, and so on.
    /// </para>
    /// <para>
    /// Assigning a format to a slot will also include every slot before it in the <see cref="GorgonGraphicsPso.OutputFormats"/>. Any of those slots that have not been assigned a format will contain 
    /// <see cref="BufferFormat.Unknown"/>.
    /// </para>
    /// <para>
    /// When a draw call that uses the pipeline state object is executed, the formats of the render target views bound to the <see cref="GorgonCommandList"/> must match the formats in the corresponding slots. 
    /// If they do not match, then the rendering results are undefined. When debugging is enabled, a warning will also be reported in the debug output.
    /// </para>
    /// <para>
    /// To assign the formats for several render target slots at once, use the <see cref="OutputFormats(ReadOnlySpan{BufferFormat})"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    /// <seealso cref="GorgonGraphicsPso.OutputFormats"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonIndexedDrawCall"/>
    public GorgonGraphicsPsoBuilder OutputFormat(BufferFormat format, int renderTargetSlot = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(renderTargetSlot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(renderTargetSlot, GorgonVideoAdapterInfo.MaxRenderTargetCount);

        _worker.RWFormats[renderTargetSlot] = format;
        _worker.RWFormatCount = _worker.RWFormatCount.Max(renderTargetSlot + 1);
        return this;
    }

    /// <summary>
    /// Function to assign the formats for the values written by the pixel shader into the render targets.
    /// </summary>
    /// <param name="formats">The formats for each render target slot.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.OutputFormats" path="/remarks/para"/>
    /// <para>
    /// This replaces all of the formats that were previously assigned. If the <paramref name="formats"/> list contains more than <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> formats, then only 
    /// the first <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> formats are used, and the rest are ignored. Passing an empty list will remove all of the formats.
    /// </para>
    /// <para>
    /// To assign the format for a single render target slot, use the <see cref="OutputFormat(BufferFormat, int)"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="GorgonGraphicsPso.OutputFormats"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonIndexedDrawCall"/>
    /// <seealso cref="BufferFormat"/>
    public GorgonGraphicsPsoBuilder OutputFormats(ReadOnlySpan<BufferFormat> formats)
    {
        _worker.OutputFormats = formats;
        return this;
    }

    /// <summary>
    /// Function to assign the multisampling information expected for the render targets and depth/stencil buffer.
    /// </summary>
    /// <param name="multisampleInfo">The number of samples per pixel, and the quality level for those samples.</param>
    /// <param name="multisampleMask">[Optional] The mask used to determine which samples are updated in the render targets.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.Multisample" path="/remarks/para[@type='common']"/>
    /// <para>
    /// Each bit in the <paramref name="multisampleMask"/> corresponds to a sample within a pixel, and only the samples with their bit set will be updated in the render targets. If the 
    /// <paramref name="multisampleMask"/> is omitted, then the mask that is currently assigned is kept. See <see cref="GorgonGraphicsPso.MultisampleMask"/> for more information about how the mask is applied.
    /// </para>
    /// <para>
    /// The default values are <see cref="GorgonMultisampleInfo.NoMultisampling"/> for the <paramref name="multisampleInfo"/>, and -1 (<c>0xFFFFFFFF</c>) for the <paramref name="multisampleMask"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso.Multisample"/>
    /// <seealso cref="GorgonGraphicsPso.MultisampleMask"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    /// <seealso cref="GorgonBufferFormatSupport"/>
    public GorgonGraphicsPsoBuilder Multisample(GorgonMultisampleInfo multisampleInfo, int? multisampleMask = null)
    {
        _worker.Multisample = multisampleInfo;

        if (multisampleMask is not null)
        {
            _worker.MultisampleMask = multisampleMask.Value;
        }

        return this;
    }

    /// <summary>
    /// Function to enable or disable independent blending for the render targets.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable independent blending, <b>false</b> to disable it.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.IsIndependentBlendingEnabled" path="/remarks/para[@type='common']"/>
    /// <para>
    /// If independent blending is enabled while the blend state in the first slot has logic operations enabled, then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendState(GorgonBlendState, int)"/>
    /// <seealso cref="BlendStates(ReadOnlySpan{GorgonBlendState})"/>
    /// <seealso cref="GorgonBlendState"/>
    public GorgonGraphicsPsoBuilder IndependentBlendingEnabled(bool enabled)
    {
        _worker.IsIndependentBlendingEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Function to enable or disable alpha to coverage.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable alpha to coverage, <b>false</b> to disable it.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <inheritdoc cref="GorgonGraphicsPso.IsAlphaToCoverageEnabled" path="/remarks"/>
    /// <seealso cref="Multisample(GorgonMultisampleInfo, int?)"/>
    public GorgonGraphicsPsoBuilder AlphaToCoverageEnabled(bool enabled)
    {
        _worker.IsAlphaToCoverageEnabled = enabled;
        return this;
    }


    /// <summary>
    /// Function to assign the type of primitive rendered with the pipeline state object.
    /// </summary>
    /// <param name="primType">The type of primitive to render.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="primType"/> is <see cref="PrimitiveType.None"/>.</exception>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.PrimitiveType" path="/remarks/para[@type='common']"/>
    /// <para>
    /// If a hull shader and domain shader are assigned, and the <paramref name="primType"/> is not one of the patch list types, then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>. The same is true if an index buffer strip cut value other than 
    /// <see cref="IndexBufferStripCutIdentifier.Disabled"/> is assigned, and the <paramref name="primType"/> is not a strip type.
    /// </para>
    /// <para>
    /// The default value is <see cref="PrimitiveType.TriangleList"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Core.PrimitiveType"/>
    /// <seealso cref="TessellationShaders(GorgonShader?, GorgonShader?)"/>
    /// <seealso cref="IndexBufferStripCutIdentifier(Core.IndexBufferStripCutIdentifier)"/>
    public GorgonGraphicsPsoBuilder PrimitiveType(PrimitiveType primType)
    {
        if (primType == Core.PrimitiveType.None)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_PSO_INVALID_PRIMITIVE_TYPE, primType), nameof(primType));
        }

        _worker.PrimitiveType = primType;
        return this;
    }

    /// <summary>
    /// Function to assign the index value used to restart a strip of primitives.
    /// </summary>
    /// <param name="identifier">The index value used to restart a strip.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// When the primitive type is a strip type (e.g. <see cref="PrimitiveType.TriangleStrip"/> or <see cref="PrimitiveType.LineStrip"/>), and a <see cref="GorgonIndexedDrawCall"/> is used to render, an index 
    /// with this value will end the current strip and start a new one. This allows multiple strips to be rendered with a single draw call.
    /// </para>
    /// <inheritdoc cref="GorgonGraphicsPso.IndexBufferStripCutIdentifier" path="/remarks/para[@type='common']"/>
    /// <para>
    /// If the <paramref name="identifier"/> is not <see cref="IndexBufferStripCutIdentifier.Disabled"/>, and the primitive type is not a strip type, then an exception will be thrown when the pipeline state 
    /// object is created by <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default value is <see cref="IndexBufferStripCutIdentifier.Disabled"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Core.IndexBufferStripCutIdentifier"/>
    /// <seealso cref="PrimitiveType(Core.PrimitiveType)"/>
    public GorgonGraphicsPsoBuilder IndexBufferStripCutIdentifier(IndexBufferStripCutIdentifier identifier)
    {
        _worker.IndexBufferStripCutIdentifier = identifier;
        return this;
    }

    /// <summary>
    /// Function to assign the pixel shader for the pipeline state object.
    /// </summary>
    /// <param name="shader">The pixel shader to assign, or <b>null</b> to remove the pixel shader.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <inheritdoc cref="GorgonGraphicsPso.PixelShader" path="/remarks"/>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    public GorgonGraphicsPsoBuilder PixelShader(GorgonShader? shader)
    {
        _worker.PixelShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign the geometry shader for the pipeline state object.
    /// </summary>
    /// <param name="shader">The geometry shader to assign, or <b>null</b> to remove the geometry shader.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <inheritdoc cref="GorgonGraphicsPso.GeometryShader" path="/remarks"/>
    public GorgonGraphicsPsoBuilder GeometryShader(GorgonShader? shader)
    {
        _worker.GeometryShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign the hull and domain shaders used for tessellation.
    /// </summary>
    /// <param name="hullShader">The hull shader to assign, or <b>null</b> to disable tessellation.</param>
    /// <param name="domainShader">The domain shader to assign, or <b>null</b> to disable tessellation.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if only one of the <paramref name="hullShader"/> or <paramref name="domainShader"/> parameters is <b>null</b>.</exception>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.HullShader" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="GorgonGraphicsPso.DomainShader" path="/remarks/para[@type='common']"/>
    /// <para>
    /// Tessellation is optional. The hull shader and domain shader are always assigned together, so to disable tessellation, pass <b>null</b> for both the <paramref name="hullShader"/> and the 
    /// <paramref name="domainShader"/>.
    /// </para>
    /// <para>
    /// When tessellation is used, the primitive type must be one of the patch list types (e.g. <see cref="PrimitiveType.PatchListWith3ControlPoints"/>). If it is not, then an exception will be thrown when the 
    /// pipeline state object is created by <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="PrimitiveType(Core.PrimitiveType)"/>
    /// <seealso cref="GorgonGraphicsPso.HullShader"/>
    /// <seealso cref="GorgonGraphicsPso.DomainShader"/>
    public GorgonGraphicsPsoBuilder TessellationShaders(GorgonShader? hullShader, GorgonShader? domainShader)
    {
        if (((hullShader is not null) && (domainShader is null))
            || ((hullShader is null) && (domainShader is not null)))
        {
            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_PSO_HULL_AND_DOMAIN_REQUIRED);
        }

        _worker.HullShader = hullShader;
        _worker.DomainShader = domainShader;
        return this;
    }

    /// <summary>
    /// Function to assign the state used for depth/stencil testing, and the format of the depth/stencil buffer.
    /// </summary>
    /// <param name="depthStencilState">The depth/stencil state to assign.</param>
    /// <param name="dsvFormat">The format of the depth/stencil buffer expected by the pipeline state object.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para>Thrown if the <paramref name="depthStencilState"/> has depth testing, depth bounds testing, or stencil testing enabled, and the <paramref name="dsvFormat"/> is <see cref="BufferFormat.Unknown"/>.</para>
    /// <para>Thrown if the <paramref name="depthStencilState"/> has depth testing or depth bounds testing enabled, and the <paramref name="dsvFormat"/> does not contain a depth component.</para>
    /// <para>Thrown if the <paramref name="depthStencilState"/> has stencil testing enabled, and the <paramref name="dsvFormat"/> does not contain a stencil component.</para>
    /// </exception>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.DepthStencilState" path="/remarks/para[@type='common']"/>
    /// <para>
    /// When a draw call that uses the pipeline state object is executed, the format of the depth/stencil view bound to the <see cref="GorgonCommandList"/> must match the <paramref name="dsvFormat"/>. If they 
    /// do not match, then the rendering results are undefined. When debugging is enabled, a warning will also be reported in the debug output.
    /// </para>
    /// <para>
    /// If the <paramref name="depthStencilState"/> has depth testing or depth bounds testing enabled, then the <paramref name="dsvFormat"/> must contain a depth component (e.g. 
    /// <see cref="BufferFormat.D32_Float"/>). If it has stencil testing enabled, then the <paramref name="dsvFormat"/> must contain a stencil component (e.g. <see cref="BufferFormat.D24_UNorm_S8_UInt"/>). 
    /// Pass <see cref="BufferFormat.Unknown"/> when no depth/stencil buffer is used, and ensure that depth testing, depth bounds testing, and stencil testing are all disabled.
    /// </para>
    /// <para>
    /// If depth bounds testing is enabled and the video adapter does not support it (see <see cref="GorgonVideoAdapterInfo.SupportsDepthBoundsTest"/>), or stencil testing is enabled with different read or 
    /// write masks for the front and back faces and the video adapter does not support it (see <see cref="GorgonVideoAdapterInfo.SupportsIndependentFrontAndBackStencilRef"/>), then an exception will be thrown 
    /// when the pipeline state object is created by <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default values are <see cref="GorgonDepthStencilState.Default"/> for the <paramref name="depthStencilState"/>, and <see cref="BufferFormat.Unknown"/> for the <paramref name="dsvFormat"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDepthStencilState"/>
    /// <seealso cref="GorgonGraphicsPso.DepthStencilState"/>
    /// <seealso cref="GorgonGraphicsPso.DepthStencilFormat"/>
    public GorgonGraphicsPsoBuilder DepthStencilState(GorgonDepthStencilState depthStencilState, BufferFormat dsvFormat)
    {
        if (dsvFormat == BufferFormat.Unknown)
        {
            if ((depthStencilState.IsDepthEnabled) || (depthStencilState.IsDepthBoundsTestingEnabled) || (depthStencilState.IsStencilEnabled))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, Resources.GORGFX_ERR_PSO_DEPTH_UNKNOWN_ENABLED);
            }
        }
        else
        {
            GorgonFormatInfo formatInfo = new(dsvFormat);

            if ((!formatInfo.HasDepth) && ((depthStencilState.IsDepthEnabled) || (depthStencilState.IsDepthBoundsTestingEnabled)))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_ERR_PSO_DEPTH_ENABLED_FORMAT_NO_DEPTH, dsvFormat));
            }

            if ((!formatInfo.HasStencil) && (depthStencilState.IsStencilEnabled))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_ERR_PSO_STENCIL_ENABLED_FORMAT_NO_STENCIL, dsvFormat));
            }
        }

        _worker.DepthStencilState = depthStencilState;
        _worker.DepthStencilFormat = dsvFormat;
        return this;
    }

    /// <summary>
    /// Function to assign the state used to rasterize primitives.
    /// </summary>
    /// <param name="rasterState">The rasterizer state to assign.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.RasterizerState" path="/remarks/para[@type='common']"/>
    /// <para>
    /// If the <see cref="GorgonRasterState.LineRasterizationMode"/> is set to <see cref="LineRasterizationMode.QuadrilateralNarrow"/>, and the video adapter does not support it (see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsNarrowQuadrilateralLines"/>), then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonRasterState.Default"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRasterState"/>
    public GorgonGraphicsPsoBuilder RasterizerState(GorgonRasterState rasterState)
    {
        _worker.RasterizerState = rasterState;
        return this;
    }

    /// <summary>
    /// Function to assign the blend state for a single render target slot.
    /// </summary>
    /// <param name="blendState">The blend state to assign.</param>
    /// <param name="renderTargetSlot">[Optional] The render target slot that will receive the blend state.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/exception"/>
    /// <exception cref="GorgonException">Thrown if the <paramref name="blendState"/> has both blending (<see cref="GorgonBlendState.IsEnabled"/>) and logic operations (<see cref="GorgonBlendState.IsLogicEnabled"/>) enabled.</exception>
    /// <remarks>
    /// <para>
    /// This assigns the <paramref name="blendState"/> to the <paramref name="renderTargetSlot"/>, and leaves the blend states in the other slots unchanged. The blend state controls how the value returned from 
    /// the pixel shader is combined with the existing contents of the render target in that slot.
    /// </para>
    /// <para>
    /// The blend states in slots other than the first slot are only used when independent blending is enabled with the <see cref="IndependentBlendingEnabled(bool)"/> method. Otherwise, the blend state in the 
    /// first slot is applied to all of the render targets.
    /// </para>
    /// <para>
    /// If the blend state in the first slot has logic operations enabled, and independent blending is enabled, then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// To assign the blend states for several render target slots at once, use the <see cref="BlendStates(ReadOnlySpan{GorgonBlendState})"/> method.
    /// </para>
    /// <para>
    /// The default value for every slot is <see cref="GorgonBlendState.NoBlending"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendStates(ReadOnlySpan{GorgonBlendState})"/>
    /// <seealso cref="IndependentBlendingEnabled(bool)"/>
    /// <seealso cref="GorgonBlendState"/>
    public GorgonGraphicsPsoBuilder BlendState(GorgonBlendState blendState, int renderTargetSlot = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(renderTargetSlot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(renderTargetSlot, GorgonVideoAdapterInfo.MaxRenderTargetCount);

        if ((blendState.IsEnabled) && (blendState.IsLogicEnabled))
        {
            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_BLEND_STATE_ENABLED_LOGIC_ENABLED, renderTargetSlot));
        }

        _worker.RWBlendStates[renderTargetSlot] = blendState;

        return this;
    }

    /// <summary>
    /// Function to assign the blend states for the render targets.
    /// </summary>
    /// <param name="blendStates">The blend states for each render target slot.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if any of the <paramref name="blendStates"/> has both blending (<see cref="GorgonBlendState.IsEnabled"/>) and logic operations (<see cref="GorgonBlendState.IsLogicEnabled"/>) enabled.</exception>
    /// <remarks>
    /// <inheritdoc cref="GorgonGraphicsPso.BlendStates" path="/remarks/para[@type='common']"/>
    /// <para>
    /// This replaces all of the blend states that were previously assigned. If the <paramref name="blendStates"/> list contains more than <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> blend 
    /// states, then only the first <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> blend states are used, and the rest are ignored.
    /// </para>
    /// <para>
    /// If the first blend state has logic operations enabled, and independent blending is enabled, then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// To assign the blend state for a single render target slot, use the <see cref="BlendState(GorgonBlendState, int)"/> method.
    /// </para>
    /// <para>
    /// The default value for every slot is <see cref="GorgonBlendState.NoBlending"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendState(GorgonBlendState, int)"/>
    /// <seealso cref="IndependentBlendingEnabled(bool)"/>
    /// <seealso cref="GorgonBlendState"/>
    public GorgonGraphicsPsoBuilder BlendStates(ReadOnlySpan<GorgonBlendState> blendStates)
    {
        for (int i = 0; i < blendStates.Length; ++i)
        {
            if ((blendStates[i].IsEnabled) && (blendStates[i].IsLogicEnabled))
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_BLEND_STATE_ENABLED_LOGIC_ENABLED, i));
            }
        }

        _worker.BlendStates = blendStates;
        return this;
    }

    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo}.Clear()"/>
    public GorgonGraphicsPsoBuilder Clear()
    {
        _worker.VertexShader = _worker.PixelShader = _worker.GeometryShader = _worker.DomainShader = _worker.HullShader = null;
        _worker.RWFormatCount = 0;
        _worker.Name = string.Empty;
        _worker.DepthStencilFormat = BufferFormat.Unknown;        
        _worker.IsAlphaToCoverageEnabled = false;
        _worker.IsIndependentBlendingEnabled = false;
        _worker.Multisample = GorgonMultisampleInfo.NoMultisampling;
        _worker.MultisampleMask = -1;
        _worker.PrimitiveType = Core.PrimitiveType.TriangleList;
        _worker.IndexBufferStripCutIdentifier = Core.IndexBufferStripCutIdentifier.Disabled;
        _worker.DepthStencilState = GorgonDepthStencilState.Default;
        _worker.RasterizerState = GorgonRasterState.Default;        

        Array.Fill(_worker.RWBlendStates, GorgonBlendState.NoBlending);
        Array.Fill(_worker.RWFormats, BufferFormat.Unknown);

        return this;
    }

    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo}.ResetTo(TBo)"/>
    public GorgonGraphicsPsoBuilder ResetTo(GorgonGraphicsPso builderObject)
    {
        Copy(builderObject, _worker);        
        _worker.Name = string.Empty;
        return this;
    }
}
