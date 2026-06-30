
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
/// Describes how rasterized primitive data is clipped against a depth/stencil buffer
/// </summary>
/// <remarks>
/// <para>
/// This will define how rasterized primitive data is clipped against a depth/stencil buffer. Depth reading, writing, and stencil operations are affected by this state
/// </para>
/// <para>
/// The depth/stencil state contains several common depth/stencil states used by applications as static members of the class. 
/// </para>
/// <para>
/// A depth/stencil state is an immutable object, and as such can only be created by using a <see cref="GorgonDepthStencilStateBuilder"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
public sealed class GorgonDepthStencilState    
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
    /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
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
    /// <remarks>
    /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
    /// </remarks>
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
    /// <remarks>
    /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
    /// </remarks>
    public static readonly GorgonDepthStencilState DepthLessEqualEnabled = new()
    {
        IsDepthEnabled = true,
        IsDepthWriteEnabled = true,
        DepthFunction = ComparisonFunction.LessEqual
    };

    /// <summary>
    /// Depth only enabled, depth write disabled. With a comparison of less than or equal for the depth buffer.
    /// </summary>
    /// <remarks>
    /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
    /// </remarks>
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
    /// Property to set or return the depth comparison function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this property to determine whether a depth value will be written into the buffer if the function specified evaluates to true using the data being written and existing data.
    /// </para>
    /// <para>
    /// The default value is <see cref="ComparisonFunction.Less"/>.
    /// </para>
    /// </remarks>
    public ComparisonFunction DepthFunction
    {
        get;
        internal set;
    } = ComparisonFunction.Less;

    /// <summary>
    /// Property to set or return whether to enable writing to the depth buffer or not.
    /// </summary>
    /// <remarks>
    /// The default value is <c>true</c>.
    /// </remarks>
    public bool IsDepthWriteEnabled
    {
        get;
        internal set;
    } = true;

    /// <summary>
    /// Property to set or return whether the depth buffer is enabled or not.
    /// </summary>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsDepthEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return whether the stencil buffer is enabled or not.
    /// </summary>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsStencilEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the setup information for stencil operations on front facing polygons.
    /// </summary>
    public GorgonStencilOperation FrontFaceStencilOperation
    {
        get;
    } = new GorgonStencilOperation();

    /// <summary>
    /// Property to return the setup information for stencil operations on back facing polygons.
    /// </summary>
    public GorgonStencilOperation BackFaceStencilOperation
    {
        get;
    } = new GorgonStencilOperation();

    /// <summary>
    /// Property to return whether depth boundary testing is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to determine if a pixel/sample passes if a depth buffer value is within the range specified on <see cref="GorgonDrawCall.DepthBoundsTestRange"/>.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDrawCall.DepthBoundsTestRange"/>
    public bool IsDepthBoundsTestingEnabled
    {
        get;
        internal set;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
    /// </summary>
    internal GorgonDepthStencilState()
    {
    }
}
