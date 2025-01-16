
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: November 20, 2017 11:51:24 PM
// 

// ReSharper disable InconsistentNaming
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how a texel is filtered when rendering
/// </summary>
public enum SampleFilter
{
    /// <summary>
    /// <para>
    /// Use point sampling for minification, magnification, and mip-level sampling.
    /// </para>
    /// </summary>
    MinMagMipPoint = D3D11.Filter.MinMagMipPoint,
    /// <summary>
    /// <para>
    /// Use point sampling for minification and magnification; use linear interpolation for mip-level sampling.
    /// </para>
    /// </summary>
    MinMagPointMipLinear = D3D11.Filter.MinMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling.
    /// </para>
    /// </summary>
    MinPointMagLinearMipPoint = D3D11.Filter.MinPointMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling.
    /// </para>
    /// </summary>
    MinPointMagMipLinear = D3D11.Filter.MinPointMagMipLinear,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling.
    /// </para>
    /// </summary>
    MinLinearMagMipPoint = D3D11.Filter.MinLinearMagMipPoint,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling.
    /// </para>
    /// </summary>
    MinLinearMagPointMipLinear = D3D11.Filter.MinLinearMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling.
    /// </para>
    /// </summary>
    MinMagLinearMipPoint = D3D11.Filter.MinMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification, magnification, and mip-level sampling.
    /// </para>
    /// </summary>
    MinMagMipLinear = D3D11.Filter.MinMagMipLinear,
    /// <summary>
    /// <para>
    /// Use anisotropic interpolation for minification, magnification, and mip-level sampling.
    /// </para>
    /// </summary>
    Anisotropic = D3D11.Filter.Anisotropic,
    /// <summary>
    /// <para>
    /// Use point sampling for minification, magnification, and mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinMagMipPoint = D3D11.Filter.ComparisonMinMagMipPoint,
    /// <summary>
    /// <para>
    /// Use point sampling for minification and magnification; use linear interpolation for mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinMagPointMipLinear = D3D11.Filter.ComparisonMinMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinPointMagLinearMipPoint = D3D11.Filter.ComparisonMinPointMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinPointMagMipLinear = D3D11.Filter.ComparisonMinPointMagMipLinear,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinLinearMagMipPoint = D3D11.Filter.ComparisonMinLinearMagMipPoint,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinLinearMagPointMipLinear = D3D11.Filter.ComparisonMinLinearMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinMagLinearMipPoint = D3D11.Filter.ComparisonMinMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Use linear interpolation for minification, magnification, and mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonMinMagMipLinear = D3D11.Filter.ComparisonMinMagMipLinear,
    /// <summary>
    /// <para>
    /// Use anisotropic interpolation for minification, magnification, and mip-level sampling. Compare the result to the comparison value.
    /// </para>
    /// </summary>
    ComparisonAnisotropic = D3D11.Filter.ComparisonAnisotropic,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagMipPoint and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinMagMipPoint = D3D11.Filter.MinimumMinMagMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagPointMipLinear and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinMagPointMipLinear = D3D11.Filter.MinimumMinMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinPointMagLinearMipPoint and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinPointMagLinearMipPoint = D3D11.Filter.MinimumMinPointMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinPointMagMipLinear and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinPointMagMipLinear = D3D11.Filter.MinimumMinPointMagMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinLinearMagMipPoint and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinLinearMagMipPoint = D3D11.Filter.MinimumMinLinearMagMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinLinearMagPointMipLinear and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinLinearMagPointMipLinear = D3D11.Filter.MinimumMinLinearMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagLinearMipPoint and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinMagLinearMipPoint = D3D11.Filter.MinimumMinMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagMipLinear and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumMinMagMipLinear = D3D11.Filter.MinimumMinMagMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.Anisotropic and instead of filtering them return the minimum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the minimum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MinimumAnisotropic = D3D11.Filter.MinimumAnisotropic,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagMipPoint and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinMagMipPoint = D3D11.Filter.MaximumMinMagMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagPointMipLinear and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinMagPointMipLinear = D3D11.Filter.MaximumMinMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinPointMagLinearMipPoint and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinPointMagLinearMipPoint = D3D11.Filter.MaximumMinPointMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinPointMagMipLinear and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinPointMagMipLinear = D3D11.Filter.MaximumMinPointMagMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinLinearMagMipPoint and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinLinearMagMipPoint = D3D11.Filter.MaximumMinLinearMagMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinLinearMagPointMipLinear and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinLinearMagPointMipLinear = D3D11.Filter.MaximumMinLinearMagPointMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagLinearMipPoint and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinMagLinearMipPoint = D3D11.Filter.MaximumMinMagLinearMipPoint,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.MinMagMipLinear and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumMinMagMipLinear = D3D11.Filter.MaximumMinMagMipLinear,
    /// <summary>
    /// <para>
    /// Fetch the same set of texels as SampleFilter.Anisotropic and instead of filtering them return the maximum of the texels.  Texels that are weighted 0 during filtering aren't counted towards the maximum.  You can query support for this filter type from the  member in the D3D11_FEATURE_DATA_D3D11_OPTIONS1 structure.
    /// </para>
    /// </summary>
    MaximumAnisotropic = D3D11.Filter.MaximumAnisotropic
}
