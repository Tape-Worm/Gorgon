
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
// Created: May 29, 2018 8:51:01 AM
// 

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of blending function to perform while blending colors and alpha channels.
/// </summary>
public enum Blend
{
    /// <summary>
    /// <para>
    /// The blend factor is (0, 0, 0, 0). No pre-blend operation.
    /// </para>
    /// </summary>
    Zero = D3D12_BLEND.D3D12_BLEND_ZERO,
    /// <summary>
    /// <para>
    /// The blend factor is (1, 1, 1, 1). No pre-blend operation.
    /// </para>
    /// </summary>
    One = D3D12_BLEND.D3D12_BLEND_ONE,
    /// <summary>
    /// <para>
    /// The blend factor is (Rₛ, Gₛ, Bₛ, Aₛ), that is color data (RGB) from a pixel shader. No pre-blend operation.
    /// </para>
    /// </summary>
    SourceColor = D3D12_BLEND.D3D12_BLEND_SRC_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - Rₛ, 1 - Gₛ, 1 - Bₛ, 1 - Aₛ), that is color data (RGB) from a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB.
    /// </para>
    /// </summary>
    InverseSourceColor = D3D12_BLEND.D3D12_BLEND_INV_SRC_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is (Aₛ, Aₛ, Aₛ, Aₛ), that is alpha data (A) from a pixel shader. No pre-blend operation.
    /// </para>
    /// </summary>
    SourceAlpha = D3D12_BLEND.D3D12_BLEND_SRC_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is ( 1 - Aₛ, 1 - Aₛ, 1 - Aₛ, 1 - Aₛ), that is alpha data (A) from a pixel shader. The pre-blend operation inverts the data, generating 1 - A.
    /// </para>
    /// </summary>
    InverseSourceAlpha = D3D12_BLEND.D3D12_BLEND_INV_SRC_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is (A, A, A, A), that is alpha data from a render target. No pre-blend operation.
    /// </para>
    /// </summary>
    DestinationAlpha = D3D12_BLEND.D3D12_BLEND_DEST_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - A, 1 - A, 1 - A, 1 - A), that is alpha data from a render target. The pre-blend operation inverts the data, generating 1 - A.
    /// </para>
    /// </summary>
    InverseDestinationAlpha = D3D12_BLEND.D3D12_BLEND_INV_DEST_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is (R, G, B, A), that is color data from a render target. No pre-blend operation.
    /// </para>
    /// </summary>
    DestinationColor = D3D12_BLEND.D3D12_BLEND_DEST_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - R, 1 - G, 1 - B, 1 - A), that is color data from a render target. The pre-blend operation inverts the data, generating 1 - RGB.
    /// </para>
    /// </summary>
    InverseDestinationColor = D3D12_BLEND.D3D12_BLEND_INV_DEST_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is (f, f, f, 1); where f = min(Aₛ, 1- A). The pre-blend operation clamps the data to 1 or less.
    /// </para>
    /// </summary>
    SourceAlphaSaturate = D3D12_BLEND.D3D12_BLEND_SRC_ALPHA_SAT,
    /// <summary>
    /// <para>
    /// The blend factor is set on the pipeline state. No pre-blend operation.
    /// </para>
    /// </summary>
    BlendFactor = D3D12_BLEND.D3D12_BLEND_BLEND_FACTOR,
    /// <summary>
    /// <para>
    /// The blend factor is set on the pipeline state. The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
    /// </para>
    /// </summary>
    InverseBlendFactor = D3D12_BLEND.D3D12_BLEND_INV_BLEND_FACTOR,
    /// <summary>
    /// <para>
    /// The blend factor is data sources both as color data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    SecondarySourceColor = D3D12_BLEND.D3D12_BLEND_SRC1_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is data sources both as color data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    InverseSecondarySourceColor = D3D12_BLEND.D3D12_BLEND_INV_SRC1_COLOR,
    /// <summary>
    /// <para>
    /// The blend factor is data sources as alpha data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    SecondarySourceAlpha = D3D12_BLEND.D3D12_BLEND_SRC1_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is data sources as alpha data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - A. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    InverseSecondarySourceAlpha = D3D12_BLEND.D3D12_BLEND_INV_SRC1_ALPHA,
    /// <summary>
    /// <para>
    /// The blend factor is (A, A, A, A), which is set with the <see cref="GorgonDrawCall.BlendFactor"/> property.
    /// </para>
    /// <para>
    /// This blending type will only work if the video adapter has its <see cref="GorgonVideoAdapterInfo.SupportsAlphaBlendFactor"/> value set to <b>true</b>.
    /// </para>
    /// </summary>
    AlphaFactor = D3D12_BLEND.D3D12_BLEND_ALPHA_FACTOR,
    /// <summary>
    /// <para>
    /// The blend factor is (1 – A, 1 – A, 1 – A, 1 – A), which is set with the <see cref="GorgonDrawCall.BlendFactor"/> property.
    /// </para>
    /// <para>
    /// This blending type will only work if the video adapter has its <see cref="GorgonVideoAdapterInfo.SupportsAlphaBlendFactor"/> value set to <b>true</b>.
    /// </para>
    /// </summary>
    InverseAlphaFactor = D3D12_BLEND.D3D12_BLEND_INV_ALPHA_FACTOR
}
