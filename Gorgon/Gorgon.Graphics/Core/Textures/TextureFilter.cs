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
// Created: April 14, 2026 11:15:08 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how a texture should be filtered when sampled by a <see cref="GorgonSampler"/>.
/// </summary>
/// <remarks>
/// <para>
/// If you use different filter types for min versus mag filter, undefined behavior occurs in certain cases where the choice between whether magnification or minification happens is ambiguous.To prevent this 
/// undefined behavior, use filter modes that use similar filter operations for both min and mag (or use anisotropic filtering, which avoids the issue as well).
/// </para>
/// </remarks>
public enum TextureFilter
{
    /// <summary>
    /// Use point sampling for minification, magnification, and mip-level sampling.
    /// </summary>
    PointMinMagMip = D3D12_FILTER.D3D12_FILTER_MIN_MAG_MIP_POINT,
    /// <summary>
    /// Use point sampling for minification and magnification; use linear interpolation for mip-level sampling.
    /// </summary>
    PointMinMagLinearMip = D3D12_FILTER.D3D12_FILTER_MIN_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling.
    /// </summary>
    PointMinLinearMagPointMip = D3D12_FILTER.D3D12_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling.
    /// </summary>
    PointMinLinearMagMip = D3D12_FILTER.D3D12_FILTER_MIN_POINT_MAG_MIP_LINEAR,
    /// <summary>
    /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling.
    /// </summary>
    LinearMinPointMagMip = D3D12_FILTER.D3D12_FILTER_MIN_LINEAR_MAG_MIP_POINT,
    /// <summary>
    /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling.
    /// </summary>
    LinearMinPointMagLinearMip = D3D12_FILTER.D3D12_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling.
    /// </summary>
    LinearMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MIN_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Use linear interpolation for minification, magnification, and mip-level sampling.
    /// </summary>
    LinearMinMagMip = D3D12_FILTER.D3D12_FILTER_MIN_MAG_MIP_LINEAR,
    /// <summary>
    /// Use anisotropic interpolation for minification and magnification. Point filtering for mip-level sampling.
    /// </summary>
    AnisotropicMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MIN_MAG_ANISOTROPIC_MIP_POINT,
    /// <summary>
    /// Use anisotropic sampling for minification, magnification, and mip-level sampling.
    /// </summary>
    AnisotropicMinMagMip = D3D12_FILTER.D3D12_FILTER_ANISOTROPIC,
    /// <summary>
    /// Use point sampling for minification and magnification; use linear interpolation for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    ComparePointMinMagPointMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_MAG_MIP_POINT,
    /// <summary>
    /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    ComparePointMinMagLinearMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    ComparePointMinLinearMagPointMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    ComparePointMinLinearMagMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_POINT_MAG_MIP_LINEAR,
    /// <summary>
    /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareLinearMinPointMagMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_LINEAR_MAG_MIP_POINT,
    /// <summary>
    /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareLinearMinPointMagLinearMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareLinearMinMagPointMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Use linear interpolation for minification, magnification, and mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareLinearMinMagMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR,
    /// <summary>
    /// Use anisotropic interpolation for minification, magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareAnisotropicMinMagPointMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_MIN_MAG_ANISOTROPIC_MIP_POINT,
    /// <summary>
    /// Use anisotropic interpolation for minification, magnification, and mip-level sampling. Compare the result to the comparison value.
    /// </summary>
    CompareAnisotropicMinMagMip = D3D12_FILTER.D3D12_FILTER_COMPARISON_ANISOTROPIC,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinMagMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards the 
    /// minimum.
    /// </summary>
    MinimumPointMinMagMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_MAG_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinMagLinearMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the minimum. 
    /// </summary>
    MinimumPointMinMagLinearMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinLinearMagPointMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the minimum.
    /// </summary>
    MinimumPointMinLinearMagPointMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinLinearMagMip"/> instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards the 
    /// minimum.
    /// </summary>
    MinimumPointLinearMagMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_POINT_MAG_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinPointMagMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the minimum. 
    /// </summary>
    MinimumLinearMinLinearMagPointMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_LINEAR_MAG_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinPointMagLinearMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the minimum. 
    /// </summary>
    MinimumLinearMinPointMagLinearMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinMagPointMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the minimum.
    /// </summary>
    MinimumLinearMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinMagMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards the 
    /// minimum.
    /// </summary>
    MinimumLinearMinMagMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_MAG_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="AnisotropicMinMagPointMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the minimum.
    /// </summary>
    MinimumAnisotropicMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_MIN_MAG_ANISOTROPIC_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="AnisotropicMinMagMip"/> and instead of filtering them return the minimum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the minimum.
    /// </summary>
    MinimumAnisotropicMinMagMip = D3D12_FILTER.D3D12_FILTER_MINIMUM_ANISOTROPIC,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinMagMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards the 
    /// maximum. 
    /// </summary>
    MaximumPointMinMagMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_MAG_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinMagLinearMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the maximum. 
    /// </summary>
    MaximumPointMinMagLinearMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinLinearMagPointMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the maximum.
    /// </summary>
    MaximumPointMinLinearMagPointMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="PointMinLinearMagMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the maximum.
    /// </summary>
    MaximumPointMinLinearMagMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinPointMagMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the maximum. 
    /// </summary>
    MaximumLinearMinPointMagMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinPointMagLinearMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the maximum. 
    /// </summary>
    MaximumLinearMinPointMagLinearMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinMagPointMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the maximum. 
    /// </summary>
    MaximumLinearMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="LinearMinMagMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards the 
    /// maximum. 
    /// </summary>
    MaximumLinearMinMagMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_MAG_MIP_LINEAR,
    /// <summary>
    /// Fetch the same set of texels as <see cref="AnisotropicMinMagPointMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted 
    /// towards the maximum.
    /// </summary>
    MaximumAnisotropicMinMagPointMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_MIN_MAG_ANISOTROPIC_MIP_POINT,
    /// <summary>
    /// Fetch the same set of texels as <see cref="AnisotropicMinMagMip"/> and instead of filtering them return the maximum of the texels. Texels that are weighted 0 during filtering aren't counted towards 
    /// the maximum.
    /// </summary>
    MaximumAnisotropicMinMagMip = D3D12_FILTER.D3D12_FILTER_MAXIMUM_ANISOTROPIC
}
