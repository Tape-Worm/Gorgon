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
// Created: June 29, 2026 5:08:58 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Describes a state for defining how to rasterize primitives when rendering.
/// </summary>
/// <remarks>
/// <para>
/// This defines how rasterization is performed when rendering primitives. This state can provide anti-aliasing for lines, tell the GPU what to cull by the vertex order of a triangle, among other properties.
/// </para>
/// <para>
/// The raster state contains several common raster states used by applications as static members of the class. Use these instead of defining your own so there's less garbage for the GC to pick up.
/// </para>
/// <para>
/// The default constructor sets up the <see cref="GorgonRasterState"/> with the same parameters as the <see cref="Default"/> property.
/// </para>
/// </remarks>
public sealed record class GorgonRasterState()
    : IPsoState<GorgonRasterState>
{
    /// <summary>
    /// The default raster state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides back face culling, and a solid fill mode.
    /// </para>
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
        init;
    } = CullingMode.Back;

    /// <summary>
    /// Property to return the triangle fill mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="FillMode.Solid"/>.
    /// </para>
    /// </remarks>
    public FillMode FillMode
    {
        get;
        init;
    } = FillMode.Solid;

    /// <summary>
    /// Property to return whether conservative rasterization should be used or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When conservative rasterization is used, a pixel is rasterized if any part of the primitive covers any part of the pixel, instead of only when the primitive covers the center of the pixel.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool UseConservativeRasterization
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return whether triangles with a counter-clockwise winding order are front-facing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, a triangle is considered front-facing if its vertices are counter-clockwise on the render target, and back-facing if they are clockwise. When this value is <b>false</b>, 
    /// the opposite is true.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CullMode"/>
    public bool IsFrontCounterClockwise
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the value to be added to the depth of a pixel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to help reduce z-fighting for co-planar polygons. This is often caused by a lack of precision for the depth in the view volume and the depth/stencil buffer. By adding a small offset 
    /// via this property, it can make a polygon appear to be in front of or behind another polygon, even though they actually share the same depth value (or very nearly the same depth value).
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int DepthBias
    {
        get;
        init;
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
        init;
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
        init;
    }

    /// <summary>
    /// Property to return whether depth clipping is enabled or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, primitives are clipped against the near and far planes of the view volume. When this value is <b>false</b>, the depth clipping is skipped, and improper depth ordering at 
    /// the pixel level may result.
    /// </para>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool IsDepthClippingEnabled
    {
        get;
        init;
    } = true;

    /// <summary>
    /// Property to return the type of rasterization performed on line primitives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="LineRasterizationMode.QuadrilateralNarrow"/> and <see cref="LineRasterizationMode.QuadrilateralWide"/> require MSAA to be enabled on the currently bound render target(s).
    /// </para>
    /// <para>
    /// If this value is set to <see cref="LineRasterizationMode.QuadrilateralNarrow"/>, and the video adapter does not support it (see <see cref="GorgonVideoAdapterInfo.SupportsNarrowQuadrilateralLines"/>), 
    /// then an exception will be thrown when the pipeline state object is created by <see cref="GorgonGraphicsPsoFactory.CreateOrGetPso(string, GorgonShader, GorgonGraphicsPsoBuilder)"/>.
    /// </para>
    /// <para>
    /// The default value is <see cref="LineRasterizationMode.Default"/>.
    /// </para>
    /// </remarks>
    public LineRasterizationMode LineRasterizationMode
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the number of samples to use when unordered access view rendering or rasterizing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This forces the number of samples to use when rendering unordered access view data. The valid values are 0, 1, 4, 8, and optionally 16. A value of 0 indicates that the sample count is not forced.
    /// </para>
    /// <para>
    /// <note type="note">
    /// <para>
    /// If you want to render with sample count set to 1 or greater, you must follow these guidelines:
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>Don't bind depth-stencil views.</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>Disable depth testing.</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>Ensure the shader doesn't output depth.</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>If you have any render-target views bound and this value is greater than 1, ensure that every render target has only a single sample.</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>Don't operate the shader at sample frequency.</description>
    /// 	</item>
    /// </list>
    /// Otherwise, the rendering results are undefined.
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
        init;
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

    /// <inheritdoc/>
    public static GorgonRasterState GetDefinedState(GorgonRasterState state)
    {
        if (state == Default)
        {
            return Default;
        }
        
        if (state == NoCulling)
        {
            return NoCulling;
        }
        
        if (state == CullFrontFace)
        {
            return CullFrontFace;
        }
        
        if (state == WireFrameNoCulling)
        {
            return WireFrameNoCulling;
        }

        return state;
    }
}
