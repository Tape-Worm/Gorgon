
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 28, 2016 11:49:51 PM
// 

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Describes how depth and stencil testing are performed against a depth/stencil buffer.
/// </summary>
/// <remarks>
/// <para>
/// This defines how rasterized primitive data is tested against a depth/stencil buffer. Depth testing, depth writing, depth bounds testing, and stencil operations are affected by this state.
/// </para>
/// <para>
/// The default constructor sets up the <see cref="GorgonDepthStencilState"/> with the same parameters as the <see cref="Default"/> property.
/// </para>
/// </remarks>
public sealed record class GorgonDepthStencilState()
    : IPsoState<GorgonDepthStencilState>
{
    /// <summary>
    /// The default depth/stencil state.
    /// </summary>
    public static readonly GorgonDepthStencilState Default = new();

    /// <summary>
    /// Depth/stencil enabled.
    /// </summary>
    public static readonly GorgonDepthStencilState DepthStencilEnabled = new()
    {
        IsDepthEnabled = true,
        IsStencilEnabled = true,
        IsDepthWriteEnabled = true
    };

    /// <summary>
    /// Depth/stencil enabled, depth write disabled.
    /// </summary>
    public static readonly GorgonDepthStencilState DepthStencilEnabledNoWrite = new()
    {
        IsDepthEnabled = true,
        IsStencilEnabled = true,
        IsDepthWriteEnabled = false
    };

    /// <summary>
    /// Depth only enabled.
    /// </summary>
    public static readonly GorgonDepthStencilState DepthEnabled = new()
    {
        IsDepthEnabled = true,
        IsDepthWriteEnabled = true
    };

    /// <summary>
    /// Depth only enabled, depth write disabled.
    /// </summary>
    public static readonly GorgonDepthStencilState DepthEnabledNoWrite = new()
    {
        IsDepthEnabled = true,
        IsDepthWriteEnabled = false
    };

    /// <summary>
    /// Depth/stencil enabled. With a comparison of less than or equal for the depth buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
    /// </para>
    /// </remarks>
    public static readonly GorgonDepthStencilState DepthLessEqualStencilEnabled = new()
    {
        IsDepthEnabled = true,
        IsStencilEnabled = true,
        IsDepthWriteEnabled = true,
        DepthFunction = ComparisonFunction.LessEqual
    };

    /// <summary>
    /// Depth/stencil enabled, depth write disabled. With a comparison of less than or equal for the depth buffer.
    /// </summary>
    /// <inheritdoc cref="DepthLessEqualStencilEnabled" path="/remarks"/>
    public static readonly GorgonDepthStencilState DepthLessEqualStencilEnabledNoWrite = new()
    {
        IsDepthEnabled = true,
        IsStencilEnabled = true,
        IsDepthWriteEnabled = false,
        DepthFunction = ComparisonFunction.LessEqual
    };

    /// <summary>
    /// Depth only enabled. With a comparison of less than or equal for the depth buffer.
    /// </summary>
    /// <inheritdoc cref="DepthLessEqualStencilEnabled" path="/remarks"/>
    public static readonly GorgonDepthStencilState DepthLessEqualEnabled = new()
    {
        IsDepthEnabled = true,
        IsDepthWriteEnabled = true,
        DepthFunction = ComparisonFunction.LessEqual
    };

    /// <summary>
    /// Depth only enabled, depth write disabled. With a comparison of less than or equal for the depth buffer.
    /// </summary>
    /// <inheritdoc cref="DepthLessEqualStencilEnabled" path="/remarks"/>
    public static readonly GorgonDepthStencilState DepthLessEqualEnabledNoWrite = new()
    {
        IsDepthEnabled = true,
        IsDepthWriteEnabled = false,
        DepthFunction = ComparisonFunction.LessEqual
    };

    /// <summary>
    /// Stencil only enabled.
    /// </summary>
    public static readonly GorgonDepthStencilState StencilEnabled = new()
    {
        IsStencilEnabled = true,
        IsDepthWriteEnabled = true
    };

    /// <summary>
    /// Depth/stencil enabled, with a greater than or equal comparer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is useful for reversing a depth distribution across the depth buffer to provide better accuracy. See 
    /// <a href="https://developer.nvidia.com/content/depth-precision-visualized">https://developer.nvidia.com/content/depth-precision-visualized</a> for more information.
    /// </para>
    /// </remarks>
    public static readonly GorgonDepthStencilState DepthStencilEnabledGreaterEqual = new()
    {
        IsDepthWriteEnabled = true,
        IsDepthEnabled = true,
        IsStencilEnabled = true,
        DepthFunction = ComparisonFunction.GreaterEqual
    };

    /// <summary>
    /// Property to return the depth comparison function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function compares the depth value of the incoming pixel against the existing value in the depth buffer. If the comparison evaluates to <b>true</b>, then the pixel passes the depth test. This value 
    /// is only used when <see cref="IsDepthEnabled"/> is <b>true</b>.
    /// </para>
    /// <para>
    /// The default value is <see cref="ComparisonFunction.Less"/>.
    /// </para>
    /// </remarks>
    public ComparisonFunction DepthFunction
    {
        get;
        init;
    } = ComparisonFunction.Less;

    /// <summary>
    /// Property to return whether to enable writing to the depth buffer or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the depth value of a pixel that passes the depth test is written to the depth buffer. This value is only used when <see cref="IsDepthEnabled"/> is <b>true</b>.
    /// </para>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool IsDepthWriteEnabled
    {
        get;
        init;
    } = true;

    /// <summary>
    /// Property to return whether depth testing is enabled or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the pipeline state object must be given a depth/stencil format that contains a depth component (e.g. <see cref="BufferFormat.D32_Float"/>).
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder.DepthStencilState(GorgonDepthStencilState, BufferFormat)"/>
    public bool IsDepthEnabled
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return whether stencil testing is enabled or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the pipeline state object must be given a depth/stencil format that contains a stencil component (e.g. <see cref="BufferFormat.D24_UNorm_S8_UInt"/>).
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder.DepthStencilState(GorgonDepthStencilState, BufferFormat)"/>
    public bool IsStencilEnabled
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the setup information for stencil operations on front-facing polygons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="GorgonStencilOperation.ReadMask"/> or <see cref="GorgonStencilOperation.WriteMask"/> values are different between the <see cref="FrontFaceStencilOperation"/> and the 
    /// <see cref="BackFaceStencilOperation"/>, and the video adapter does not support it (see <see cref="GorgonVideoAdapterInfo.SupportsIndependentFrontAndBackStencilRef"/>), then an exception will be thrown 
    /// when the pipeline state object is created by <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonStencilOperation"/>
    public GorgonStencilOperation FrontFaceStencilOperation
    {
        get;
        init;
    } = new GorgonStencilOperation();

    /// <summary>
    /// Property to return the setup information for stencil operations on back-facing polygons.
    /// </summary>
    /// <inheritdoc cref="FrontFaceStencilOperation" path="/remarks"/>
    /// <seealso cref="GorgonStencilOperation"/>
    public GorgonStencilOperation BackFaceStencilOperation
    {
        get;
        init;
    } = new GorgonStencilOperation();

    /// <summary>
    /// Property to return whether depth boundary testing is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to determine if a pixel/sample passes if a depth buffer value is within the range specified on <see cref="GorgonDrawCallCommon.DepthBoundsTestRange"/>.
    /// </para>
    /// <para>
    /// When this value is <b>true</b>, the pipeline state object must be given a depth/stencil format that contains a depth component. If the video adapter does not support depth bounds testing (see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsDepthBoundsTest"/>), then an exception will be thrown when the pipeline state object is created by 
    /// <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDrawCallCommon.DepthBoundsTestRange"/>
    public bool IsDepthBoundsTestingEnabled
    {
        get;
        init;
    } = false;

    /// <summary>
    /// Function to build the Direct3D 12 depth/stencil state description.
    /// </summary>
    /// <returns>The D3D12 depth/stencil state description.</returns>
    internal D3D12_DEPTH_STENCIL_DESC2 GetDesc() => new()
    {
        DepthEnable = IsDepthEnabled,
        StencilEnable = IsStencilEnabled,
        DepthWriteMask = IsDepthWriteEnabled ? D3D12_DEPTH_WRITE_MASK.D3D12_DEPTH_WRITE_MASK_ALL : D3D12_DEPTH_WRITE_MASK.D3D12_DEPTH_WRITE_MASK_ZERO,
        DepthFunc = (D3D12_COMPARISON_FUNC)DepthFunction,
        BackFace = BackFaceStencilOperation.GetDesc(),
        FrontFace = FrontFaceStencilOperation.GetDesc(),
        DepthBoundsTestEnable = IsDepthBoundsTestingEnabled,
    };

    /// <inheritdoc/>
    public static GorgonDepthStencilState GetDefinedState(GorgonDepthStencilState state)
    {
        if (state == Default)
        {
            return Default;
        }

        if (state == DepthStencilEnabled)
        {
            return DepthStencilEnabled;
        }

        if (state == DepthStencilEnabledNoWrite)
        {
            return DepthStencilEnabledNoWrite;
        }

        if (state == DepthEnabled)
        {
            return DepthEnabled;
        }

        if (state == DepthEnabledNoWrite)
        {
            return DepthEnabledNoWrite;
        }

        if (state == DepthLessEqualStencilEnabled)
        {
            return DepthLessEqualStencilEnabled;
        }

        if (state == DepthLessEqualStencilEnabledNoWrite)
        {
            return DepthLessEqualStencilEnabledNoWrite;
        }

        if (state == DepthLessEqualEnabled)
        {
            return DepthLessEqualEnabled;
        }

        if (state == DepthLessEqualEnabledNoWrite)
        {
            return DepthLessEqualEnabledNoWrite;
        }

        if (state == StencilEnabled)
        {
            return StencilEnabled;
        }

        if (state == DepthStencilEnabledGreaterEqual)
        {
            return DepthStencilEnabledGreaterEqual;
        }

        return state;
    }
}