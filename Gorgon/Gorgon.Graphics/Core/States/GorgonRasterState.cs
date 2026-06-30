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
// Created: June 29, 2026 5:08:58 PM
//

using System;
using System.Collections.Generic;
using System.Text;
using Gorgon.Math;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Describes a state for defining how to rasterize primitives when rendering.
/// </summary>
/// <remarks>
/// <para>
/// This is the part of the <see cref="GorgonGraphicsPso"/> object that defines the raster state during a draw call.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
public class GorgonRasterState
    : IEquatable<GorgonRasterState>
{
    /// <summary>
    /// The default raster state.
    /// </summary>
    /// <remarks>
    /// Provides back face culling, and a solid fill mode.
    /// </remarks>
    public static readonly GorgonRasterState Default = new();

    /// <summary>
    /// Wireframe, with no culling.
    /// </summary>
    public static readonly GorgonRasterState WireFrameNoCulling = new()
    {
        CullMode = CullingMode.None,
        FillMode = FillMode.Wireframe
    };

    /// <summary>
    /// Front face culling.
    /// </summary>
    public static readonly GorgonRasterState CullFrontFace = new()
    {
        CullMode = CullingMode.Front
    };

    /// <summary>
    /// No culling.
    /// </summary>
    public static readonly GorgonRasterState NoCulling = new()
    {
        CullMode = CullingMode.None
    };

    /// <summary>
    /// Property to return the current culling mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to determine if a triangle is drawn or not by the direction it's facing.
    /// </para>
    /// <para>
    /// The default value is <see cref="CullingMode.Back"/>.
    /// </para>
    /// </remarks>
    public CullingMode CullMode
    {
        get;
        internal set;
    } = CullingMode.Back;

    /// <summary>
    /// Property to return the triangle fill mode.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="FillMode.Solid"/>.
    /// </remarks>
    public FillMode FillMode
    {
        get;
        internal set;
    } = FillMode.Solid;

    /// <summary>
    /// Property to return whether conservative rasterization should be used or not.
    /// </summary>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool UseConservativeRasterization
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return whether a triangle is front or back facing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value determines if a triangle is front or back facing by using the winding order of its vertices.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsFrontCounterClockwise
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the value to be added to the depth of a pixel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to help z-fighting for co-planar polygons. This is often caused by a lack of precision for the depth in the view volume and the depth/stencil buffer. By adding a small offset 
    /// via this property, it can make a polygon appear to be in front of or behind another polygon even though they actually share the same (or very nearly the same depth value).
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int DepthBias
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the scalar used for a slope of a pixel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used in conjunction with the <see cref="DepthBias"/> to help overcome issues (typically "acne" for shadow maps) when rendering coplanar polygons. It is used to adjust the depth value 
    /// based on a slope.
    /// </para>
    /// <para>
    /// The default value is 0.0f.
    /// </para>
    /// </remarks>
    public float SlopeScaledDepthBias
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the clamping value for the <see cref="DepthBias"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The maximum <see cref="DepthBias"/> for a pixel.
    /// </para>
    /// <para>
    /// The default value is 0.0f.
    /// </para>
    /// </remarks>
    /// <seealso cref="DepthBias"/>
    public float DepthBiasClamp
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return whether depth/clipping is enabled or not.
    /// </summary>
    /// <remarks>
    /// The default value is <b>true</b>.
    /// </remarks>
    public bool IsDepthClippingEnabled
    {
        get;
        internal set;
    } = true;

    /// <summary>
    /// Property to return the type of rasterization performed on line primitves.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="LineRasterizationMode.QuadrilateralNarrow"/> and <see cref="LineRasterizationMode.QuadrilateralWide"/> require MSAA to be enabled on the currently bound render target(s).
    /// </para>
    /// <para>
    /// The default value is <see cref="LineRasterizationMode.Default"/>.
    /// </para>
    /// </remarks>
    public LineRasterizationMode LineRasterizationMode
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the number of samples to use when unordered access view rendering or rasterizing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This forces the number of samples to use when rendering unordered access view data. The valid values are 0, 1, 4, 8 and 16. A value of 0 indicates that the sample count is not forced.
    /// </para>
    /// <para>
    /// <note type="note">
    /// <para>
    /// If you want to render with sample count set to 1 or greater, you must follow these guidelines:
    /// <list type="bullet">
    ///		<item> 
    ///			<description>Don't bind depth-stencil views.</description>
    ///		</item>
    ///		<item>
    ///			<description>Disable depth testing.</description>
    ///		</item>
    ///		<item>
    ///			<description>Ensure the shader doesn't output depth.</description>
    ///		</item>
    ///		<item>
    ///			<description>If you have any render-target views bound and this value is greater than 1, ensure that every render target has only a single sample.</description>
    ///		</item>
    ///		<item>
    ///			<description>Don't operate the shader at sample frequency.</description>
    ///		</item> 
    /// </list>
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int ForcedReadWriteViewSampleCount
    {
        get;
        internal set;
    }

    /// <summary>
    /// Function to return the D3D rasterization description.
    /// </summary>
    /// <returns>The D3D 12 rasterization description.</returns>
    internal D3D12_RASTERIZER_DESC2 GetDesc() =>
        new()
        {
            CullMode = (D3D12_CULL_MODE)CullMode,
            FillMode = (D3D12_FILL_MODE)FillMode,
            DepthBias = DepthBias,
            DepthBiasClamp = DepthBiasClamp,
            DepthClipEnable = IsDepthClippingEnabled,
            FrontCounterClockwise = IsFrontCounterClockwise,
            SlopeScaledDepthBias = SlopeScaledDepthBias,
            ForcedSampleCount = (uint)ForcedReadWriteViewSampleCount,
            ConservativeRaster = UseConservativeRasterization ? D3D12_CONSERVATIVE_RASTERIZATION_MODE.D3D12_CONSERVATIVE_RASTERIZATION_MODE_ON : D3D12_CONSERVATIVE_RASTERIZATION_MODE.D3D12_CONSERVATIVE_RASTERIZATION_MODE_OFF,
            LineRasterizationMode = (D3D12_LINE_RASTERIZATION_MODE)LineRasterizationMode
        };

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <returns>true if the current object is equal to the <paramref name="state" /> parameter; otherwise, false.</returns>
    /// <param name="state">An object to compare with this object.</param>
    public bool Equals(GorgonRasterState? state) => (this == state) || ((state is not null)
                                   && (CullMode == state.CullMode)
                                   && (DepthBias == state.DepthBias)
                                   && (DepthBiasClamp.EqualsEpsilon(state.DepthBiasClamp))
                                   && (IsDepthClippingEnabled == state.IsDepthClippingEnabled)
                                   && (FillMode == state.FillMode)
                                   && (ForcedReadWriteViewSampleCount == state.ForcedReadWriteViewSampleCount)
                                   && (IsFrontCounterClockwise == state.IsFrontCounterClockwise)
                                   && (LineRasterizationMode == state.LineRasterizationMode)
                                   && (SlopeScaledDepthBias.EqualsEpsilon(state.SlopeScaledDepthBias))
                                   && (UseConservativeRasterization == state.UseConservativeRasterization));

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj) => obj is GorgonRasterState rs ?  Equals(rs) : base.Equals(obj);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(CullMode);
        hashCode.Add(DepthBias);
        hashCode.Add(DepthBiasClamp);
        hashCode.Add(IsDepthClippingEnabled);
        hashCode.Add(FillMode);
        hashCode.Add(ForcedReadWriteViewSampleCount);
        hashCode.Add(IsFrontCounterClockwise);
        hashCode.Add(LineRasterizationMode);
        hashCode.Add(SlopeScaledDepthBias);
        hashCode.Add(UseConservativeRasterization);
        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRasterState"/> class.
    /// </summary>
    internal GorgonRasterState()
    {
    }
}
