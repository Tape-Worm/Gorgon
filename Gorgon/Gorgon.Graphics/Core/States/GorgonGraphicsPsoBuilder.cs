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
// Created: June 15, 2026 1:09:18 PM
//

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Builds pipeline state objects for rendering.
/// </summary>
public sealed class GorgonGraphicsPsoBuilder
        : IGorgonFluentBuilder<GorgonGraphicsPsoBuilder, GorgonGraphicsPso, IGorgonAllocator<GorgonGraphicsPso>, string, GorgonShader>
{
    private readonly GorgonGraphicsPso _worker;
    private readonly DefaultAllocator _allocator;
    
    /// <summary>
    /// Default allocator object.
    /// </summary>
    /// <param name="graphics">The graphics object for the builder.</param>
    private class DefaultAllocator(GorgonGraphics graphics)
        : IGorgonAllocator<GorgonGraphicsPso>
    {
        private readonly GorgonGraphics _graphics = graphics;

        /// <inheritdoc/>
        public GorgonGraphicsPso Allocate(Action<GorgonGraphicsPso>? initializer = null)
        {
            GorgonGraphicsPso result = new(_graphics);
            initializer?.Invoke(result);
            return result;
        }
    }

    /// <summary>
    /// Property to return the graphics instance that is associated with this builder.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Function to copy the state from one pso object to another.
    /// </summary>
    /// <param name="source">The pso to copy from.</param>
    /// <param name="destination">The pso to copy into.</param>
    /// <exception cref="GorgonException">We should not see this happen.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(GorgonGraphicsPso source, GorgonGraphicsPso destination)
    {
        destination.D3DPso.Dispose();
        destination.Name = source.Name;
        destination.VertexShader = source.VertexShader;
        destination.OutputFormats = source.OutputFormats;
        
        destination.PrimitiveType = source.PrimitiveType;
        destination.IndexBufferStripCutIdentifier = source.IndexBufferStripCutIdentifier;        

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
    /// <para>Thrown if the <see cref="GorgonStencilOperation.ReadMask"/> or <see cref="GorgonStencilOperation.WriteMask"/> is different between the <see cref="GorgonDepthStencilState.FrontFaceStencilOperation"/> and <see cref="GorgonDepthStencilState.BackFaceStencilOperation"/> 
    /// and the adapter does not support independent reference masks between front and back stencil operations.</para>
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
    }

    /// <summary>
    /// Function to assign the format expected as the output format from the pixel shader.
    /// </summary>
    /// <param name="format">The expected output format.</param>
    /// <param name="renderTargetSlot">[Optional] The slot number corresponding to the render target that the pixel shader is rendering into.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="renderTargetSlot"/> is less than 0, or greater than or equal to <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/>.</para></exception>
    /// <remarks>
    /// <para>
    /// This provides the pipeline with the expected output format from the pixel shader on the specified render target slot. If this value is not matched correctly with the format of the current render 
    /// target view on the specified slot when rendering, then the call to the <see cref="GorgonCommandList.Draw(GorgonDrawCall)"/> method will fail.
    /// </para>
    /// <para>
    /// When the <paramref name="renderTargetSlot"/> is greater than 0, then it is expected that the previous target slots have valid format values (i.e. not <see cref="BufferFormat.Unknown"/>). Otherwise 
    /// rendering will fail.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Draw(GorgonDrawCall)"/>
    /// <seealso cref="GorgonVideoAdapterInfo"/>
    /// <seealso cref="BufferFormat"/>
    public GorgonGraphicsPsoBuilder OutputFormat(BufferFormat format, int renderTargetSlot = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(renderTargetSlot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(renderTargetSlot, GorgonVideoAdapterInfo.MaxRenderTargetCount);

        _worker.RWFormats[renderTargetSlot] = format;
        _worker.RWFormatCount = _worker.RWFormatCount.Max(renderTargetSlot + 1);
        return this;
    }

    /// <summary>
    /// Function to assign a series of formats expected as the output format from the pixel shader.
    /// </summary>
    /// <param name="formats">The list of formats for each render target slot.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This provides the pipeline with the expected output formats from the pixel shader on corresponding render target slots. If this value is not matched correctly with the formats of the current render 
    /// target views on the specified slots when rendering, then the call to the <see cref="GorgonCommandList.Draw(GorgonDrawCall)"/> method will fail.
    /// </para>
    /// <para>
    /// If the length of the <paramref name="formats"/> list is greater than <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/>, then only the values up to the 
    /// <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> will be copied, the rest will be ignored.
    /// </para>
    /// <para>
    /// Use the <see cref="OutputFormat(BufferFormat, int)"/> method if only a single output format is returned from the pixel shader.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Draw(GorgonDrawCall)"/>
    /// <seealso cref="GorgonVideoAdapterInfo"/>
    /// <seealso cref="BufferFormat"/>
    public GorgonGraphicsPsoBuilder OutputFormats(ReadOnlySpan<BufferFormat> formats)
    {
        _worker.OutputFormats = formats;
        return this;
    }

    /// <summary>
    /// Function to assign the multisampling values expected for the output.
    /// </summary>
    /// <param name="multisampleInfo">The multisample value to expect.</param>
    /// <param name="multisampleMask">[Optional] The mask used when performing multisampling.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// If the <paramref name="multisampleMask"/> is not specified, the value will not be changed and will remain the same as the previous call to this method. 
    /// </para>
    /// <para>
    /// The default values are <see cref="GorgonMultisampleInfo.NoMultisampling"/> and -1 (<c>0xFFFFFFFF</c> <langword>unsigned</langword>).
    /// </para>
    /// </remarks>
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
    /// Function to enable or disable independent blending.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable, <b>false</b> to disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This turns independent blending between render target slots on or off. Applications that enable this functionality can then proceed to assign a blend state for each render target assigned on the 
    /// pipeline via <see cref="BlendState(GorgonBlendState, int)"/>. Otherwise, when <paramref name="enabled"/> is <b>false</b>, then only the first render target slot's blend state is used.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonBlendState"/>
    public GorgonGraphicsPsoBuilder IndependentBlendingEnabled(bool enabled)
    {
        _worker.IsIndependentBlendingEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Function to set whether alpha to coverage is enabled or not.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable alpha to coverage, <b>false</b> disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public GorgonGraphicsPsoBuilder AlphaToCoverageEnabled(bool enabled)
    {
        _worker.IsAlphaToCoverageEnabled = enabled;
        return this;
    }


    /// <summary>
    /// Function to assign the primitive type to use when rendering with the pipeline state object.
    /// </summary>
    /// <param name="primType">The primitive type to assign.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="primType"/> is set to <see cref="PrimitiveType.None"/>.</exception>
    /// <remarks>
    /// <para>
    /// This value defaults to <see cref="PrimitiveType.TriangleList"/>.
    /// </para>
    /// </remarks>
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
    /// Function to assign the type of index buffer identifier used to restart strips of vertices.
    /// </summary>
    /// <param name="identifier">The identifier to apply.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This value applies only when the <see cref="Core.PrimitiveType"/> topology is a triangle or line strip. It will be ignored if it is any other type.
    /// </para>
    /// <para>
    /// Ensure that the value passed matches the size of an index in the buffer. For a 16-bit index, use the <see cref="IndexBufferStripCutIdentifier.StopWith16BitMax"/>, and for a 32-bit index use the 
    /// <see cref="IndexBufferStripCutIdentifier.StopWith32BitMax"/> value.
    /// </para>
    /// <para>
    /// This value defaults to <see cref="IndexBufferStripCutIdentifier.Disabled"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Core.PrimitiveType"/>
    public GorgonGraphicsPsoBuilder IndexBufferStripCutIdentifier(IndexBufferStripCutIdentifier identifier)
    {
        _worker.IndexBufferStripCutIdentifier = identifier;
        return this;
    }

    /// <summary>
    /// Function to assign a pixel shader to the pipeline state object.
    /// </summary>
    /// <param name="shader">The shader to assign, or <b>null</b> to disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// When specifying a pixel shader, ensure that the return value format is recorded using the <see cref="OutputFormat(BufferFormat, int)"/> or <see cref="OutputFormats(ReadOnlySpan{BufferFormat})"/> 
    /// method.
    /// </para>
    /// </remarks>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    public GorgonGraphicsPsoBuilder PixelShader(GorgonShader? shader)
    {
        _worker.PixelShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign a geometry shader to the pipeline state object.
    /// </summary>
    /// <param name="shader">The shader to assign, or <b>null</b> to disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    public GorgonGraphicsPsoBuilder GeometryShader(GorgonShader? shader)
    {
        _worker.GeometryShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign a domain shader to the pipeline state object.
    /// </summary>
    /// <param name="shader">The shader to assign, or <b>null</b> to disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    public GorgonGraphicsPsoBuilder DomainShader(GorgonShader? shader)
    {
        _worker.DomainShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign a hull shader to the pipeline state object.
    /// </summary>
    /// <param name="shader">The shader to assign, or <b>null</b> to disable.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <seealso cref="OutputFormat(BufferFormat, int)"/>
    /// <seealso cref="OutputFormats(ReadOnlySpan{BufferFormat})"/>
    public GorgonGraphicsPsoBuilder HullShader(GorgonShader? shader)
    {
        _worker.HullShader = shader;
        return this;
    }

    /// <summary>
    /// Function to assign a depth/stencil state to the pipeline state object.
    /// </summary>
    /// <param name="depthStencilState">The depth/stencil state to assign.</param>
    /// <param name="dsvFormat">The expected format of the depth/stencil buffer.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para>The <paramref name="depthStencilState"/> has the depth and/or stencil testing enabled, but the expected <paramref name="dsvFormat"/> is set to <see cref="BufferFormat.Unknown"/>.</para>
    /// <para>The <paramref name="depthStencilState"/> has depth testing enabled, but the <paramref name="dsvFormat"/> does not support depth data.</para>
    /// <para>The <paramref name="depthStencilState"/> has stencil testing enabled, but the <paramref name="dsvFormat"/> does not support stencil data.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This assigns a depth stencil state to the pipeline to configure how to perform depth and/or stencil testing. It also supplies the pipeline state with the expected format for depth/stencil testing 
    /// operations while the PSO is active. 
    /// </para>
    /// <para>
    /// If the <paramref name="dsvFormat"/> does not match the currently assigned <see cref="GorgonDepthStencilView"/> format, then the <see cref="GorgonDrawCall"/> will fail.
    /// </para>
    /// <para>
    /// Ensure that the format passed to the <paramref name="dsvFormat"/> has the correct depth and/or stencil format type (e.g. <see cref="BufferFormat.D32_Float"/>, 
    /// <see cref="BufferFormat.D24_UNorm_S8_UInt"/>, etc...), otherwise an exception will be thrown. An exception will also be thrown for passing <see cref="BufferFormat.Unknown"/> when the 
    /// <paramref name="depthStencilState"/> has depth and/or stencil testing enabled.
    /// </para>
    /// <para>
    /// It is important to note that the <see cref="GorgonDepthStencilState.IsDepthBoundsTestingEnabled"/> flag bypasses the <see cref="GorgonDepthStencilState.IsDepthEnabled"/> flag, and as such if it is set 
    /// to <b>true</b>, it will be treated as though depth testing is enabled.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonDepthStencilState.Default"/> (No depth/stencil testing).
    /// </para>
    /// </remarks>
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
    /// Function to assign a rasterizer state to the pipeline state object.
    /// </summary>
    /// <param name="rasterState">The rasterizer state to assign.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="GorgonRasterState.Default"/>.
    /// </para>
    /// </remarks>
    public GorgonGraphicsPsoBuilder RasterizerState(GorgonRasterState rasterState)
    {
        _worker.RasterizerState = rasterState;
        return this;
    }

    /// <summary>
    /// Function to assign a blend state to the pipeline state object.
    /// </summary>
    /// <param name="blendState">The blend state to assign.</param>
    /// <param name="renderTargetSlot">[Optional] The slot to use when <see cref="GorgonGraphicsPso.IsIndependentBlendingEnabled"/> is <b>true</b>.</param>
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="OutputFormat(BufferFormat, int)" path="/exception[@cref='T:System.ArgumentOutOfRangeException']"/></exception>
    /// <remarks>
    /// <para>
    /// This assigns a blending state for specified render target slot to the PSO. The <paramref name="renderTargetSlot"/> allows setting different blend states per render target view slot, however, the 
    /// <see cref="IndependentBlendingEnabled(bool)(bool)"/> method must be called and passed a value of <b>true</b> to enable this functionality, otherwise all render targets will receive the same blend state 
    /// as the first slot.
    /// </para>
    /// </remarks>
    /// <seealso cref="IndependentBlendingEnabled(bool)"/>
    public GorgonGraphicsPsoBuilder BlendState(GorgonBlendState blendState, int renderTargetSlot = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(renderTargetSlot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(renderTargetSlot, GorgonVideoAdapterInfo.MaxRenderTargetCount);

        _worker.RWBlendStates[renderTargetSlot] = blendState;

        return this;
    }

    /// <summary>
    /// Function to assign multiple blend states to the pipeline state object.
    /// </summary>
    /// <param name="blendStates">The blend states to assign.</param>    
    /// <inheritdoc cref="OutputFormat(BufferFormat, int)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This assigns blending states for the corresponding render target slots to the PSO. If the <see cref="IsIndependentBlendingEnabled(bool)"/> method has a value of <b>true</b>, then each blend state 
    /// will be used, otherwise all render targets will receive the same blend state as the first slot. So, passing multiple blend states without independent blending enabled will not work.
    /// </para>
    /// <para>
    /// If the length of the <paramref name="blendStates"/> list is greater than <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/>, then only the values up to the 
    /// <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/> will be copied, the rest will be ignored.
    /// </para>
    /// <para>
    /// To set a single blend state, call the <see cref="BlendState(GorgonBlendState, int)"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonVideoAdapterInfo"/>
    /// <seealso cref="IsIndependentBlendingEnabled(bool)"/>
    /// <seealso cref="BlendState(GorgonBlendState, int)"/>
    public GorgonGraphicsPsoBuilder BlendStates(ReadOnlySpan<GorgonBlendState> blendStates)
    {
        _worker.BlendStates = blendStates;
        return this;
    }

    /// <inheritdoc/>
    /// <param name="name">The name of the pipeline state object.</param>
    /// <param name="vertexShader">The vertex shader to assign to the pipeline state object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='allocator']"/></param>
    /// <exception cref="GorgonException"><inheritdoc cref="Validate" path="/exception[@cref='Gorgon.Core.GorgonException']"/></exception>
    /// <remarks>
    /// <inheritdoc path="/remarks/para"/>
    /// <para>
    /// The <paramref name="name"/> is required, if it is left empty, a name will be generated.
    /// </para>
    /// </remarks>
    public GorgonGraphicsPso Build(string name, GorgonShader vertexShader, IGorgonAllocator<GorgonGraphicsPso>? allocator = null)
    {
        allocator ??= _allocator;

        Debug.Assert(vertexShader != GorgonShader.NullShader, "A vertex shader is required for the PSO.");
                
        _worker.Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonGraphicsPso));
        _worker.VertexShader = vertexShader;

        Validate();

        GorgonGraphicsPso result = allocator.Allocate(pso =>
        {
            Copy(_worker, pso);
            pso.IndexBufferStripCutIdentifier = pso.PrimitiveType switch
            {
                Core.PrimitiveType.LineStrip or Core.PrimitiveType.TriangleStrip or Core.PrimitiveType.LineStripWithAdjacency or Core.PrimitiveType.TriangleStripWithAdjacency => _worker.IndexBufferStripCutIdentifier,
                _ => Core.IndexBufferStripCutIdentifier.Disabled
            };
        });

        return result;
    }

    /// <inheritdoc/>
    public GorgonGraphicsPsoBuilder Clear()
    {
        _worker.VertexShader = GorgonShader.NullShader;
        _worker.PixelShader = _worker.GeometryShader = _worker.DomainShader = _worker.HullShader = null;
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

    /// <inheritdoc/>
    public GorgonGraphicsPsoBuilder ResetTo(GorgonGraphicsPso builderObject)
    {
        Copy(builderObject, _worker);
        _worker.Name = string.Empty;
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsPsoBuilder"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object associated with this builder.</param>
    public GorgonGraphicsPsoBuilder(GorgonGraphics graphics)
    {        
        Graphics = graphics;
        _worker = new GorgonGraphicsPso(Graphics);
        _allocator = new DefaultAllocator(Graphics);
        _worker.UnregisterDisposable(Graphics);
    }
}
