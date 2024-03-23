
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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

using SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of operation to perform while blending colors
/// </summary>
public enum Blend
{
    /// <summary>
    /// <para>
    /// The blend factor is (0, 0, 0, 0). No pre-blend operation.
    /// </para>
    /// </summary>
    Zero = BlendOption.Zero,
    /// <summary>
    /// <para>
    /// The blend factor is (1, 1, 1, 1). No pre-blend operation.
    /// </para>
    /// </summary>
    One = BlendOption.One,
    /// <summary>
    /// <para>
    /// The blend factor is (Rₛ, Gₛ, Bₛ, Aₛ), that is color data (RGB) from a pixel shader. No pre-blend operation.
    /// </para>
    /// </summary>
    SourceColor = BlendOption.SourceColor,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - Rₛ, 1 - Gₛ, 1 - Bₛ, 1 - Aₛ), that is color data (RGB) from a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB.
    /// </para>
    /// </summary>
    InverseSourceColor = BlendOption.InverseSourceColor,
    /// <summary>
    /// <para>
    /// The blend factor is (Aₛ, Aₛ, Aₛ, Aₛ), that is alpha data (A) from a pixel shader. No pre-blend operation.
    /// </para>
    /// </summary>
    SourceAlpha = BlendOption.SourceAlpha,
    /// <summary>
    /// <para>
    /// The blend factor is ( 1 - Aₛ, 1 - Aₛ, 1 - Aₛ, 1 - Aₛ), that is alpha data (A) from a pixel shader. The pre-blend operation inverts the data, generating 1 - A.
    /// </para>
    /// </summary>
    InverseSourceAlpha = BlendOption.InverseSourceAlpha,
    /// <summary>
    /// <para>
    /// The blend factor is (A A A A), that is alpha data from a render target. No pre-blend operation.
    /// </para>
    /// </summary>
    DestinationAlpha = BlendOption.DestinationAlpha,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - A 1 - A 1 - A 1 - A), that is alpha data from a render target. The pre-blend operation inverts the data, generating 1 - A.
    /// </para>
    /// </summary>
    InverseDestinationAlpha = BlendOption.InverseDestinationAlpha,
    /// <summary>
    /// <para>
    /// The blend factor is (R, G, B, A), that is color data from a render target. No pre-blend operation.
    /// </para>
    /// </summary>
    DestinationColor = BlendOption.DestinationColor,
    /// <summary>
    /// <para>
    /// The blend factor is (1 - R, 1 - G, 1 - B, 1 - A), that is color data from a render target. The pre-blend operation inverts the data, generating 1 - RGB.
    /// </para>
    /// </summary>
    InverseDestinationColor = BlendOption.InverseDestinationColor,
    /// <summary>
    /// <para>
    /// The blend factor is (f, f, f, 1); where f = min(Aₛ, 1
    /// </para>
    /// <para>
    /// - A). The pre-blend operation clamps the data to 1 or less.
    /// </para>
    /// </summary>
    SourceAlphaSaturate = BlendOption.SourceAlphaSaturate,
    /// <summary>
    /// <para>
    /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. No pre-blend operation.
    /// </para>
    /// </summary>
    BlendFactor = BlendOption.BlendFactor,
    /// <summary>
    /// <para>
    /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
    /// </para>
    /// </summary>
    InverseBlendFactor = BlendOption.InverseBlendFactor,
    /// <summary>
    /// <para>
    /// The blend factor is data sources both as color data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    SecondarySourceColor = BlendOption.SecondarySourceColor,
    /// <summary>
    /// <para>
    /// The blend factor is data sources both as color data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    InverseSecondarySourceColor = BlendOption.InverseSecondarySourceColor,
    /// <summary>
    /// <para>
    /// The blend factor is data sources as alpha data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    SecondarySourceAlpha = BlendOption.SecondarySourceAlpha,
    /// <summary>
    /// <para>
    /// The blend factor is data sources as alpha data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - A. This blend factor supports dual-source color blending.
    /// </para>
    /// </summary>
    InverseSecondarySourceAlpha = BlendOption.InverseSecondarySourceAlpha
}
