#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: May 29, 2018 8:51:01 AM
// 
#endregion

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the type of operation to perform while blending colors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum Blend
    {
        /// <summary>
        /// <para>
        /// The blend factor is (0, 0, 0, 0). No pre-blend operation.
        /// </para>
        /// </summary>
        Zero = SharpDX.Direct3D11.BlendOption.Zero,
        /// <summary>
        /// <para>
        /// The blend factor is (1, 1, 1, 1). No pre-blend operation.
        /// </para>
        /// </summary>
        One = SharpDX.Direct3D11.BlendOption.One,
        /// <summary>
        /// <para>
        /// The blend factor is (Rₛ, Gₛ, Bₛ, Aₛ), that is color data (RGB) from a pixel shader. No pre-blend operation.
        /// </para>
        /// </summary>
        SourceColor = SharpDX.Direct3D11.BlendOption.SourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - Rₛ, 1 - Gₛ, 1 - Bₛ, 1 - Aₛ), that is color data (RGB) from a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB.
        /// </para>
        /// </summary>
        InverseSourceColor = SharpDX.Direct3D11.BlendOption.InverseSourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is (Aₛ, Aₛ, Aₛ, Aₛ), that is alpha data (A) from a pixel shader. No pre-blend operation.
        /// </para>
        /// </summary>
        SourceAlpha = SharpDX.Direct3D11.BlendOption.SourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is ( 1 - Aₛ, 1 - Aₛ, 1 - Aₛ, 1 - Aₛ), that is alpha data (A) from a pixel shader. The pre-blend operation inverts the data, generating 1 - A.
        /// </para>
        /// </summary>
        InverseSourceAlpha = SharpDX.Direct3D11.BlendOption.InverseSourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (A A A A), that is alpha data from a render target. No pre-blend operation.
        /// </para>
        /// </summary>
        DestinationAlpha = SharpDX.Direct3D11.BlendOption.DestinationAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - A 1 - A 1 - A 1 - A), that is alpha data from a render target. The pre-blend operation inverts the data, generating 1 - A.
        /// </para>
        /// </summary>
        InverseDestinationAlpha = SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (R, G, B, A), that is color data from a render target. No pre-blend operation.
        /// </para>
        /// </summary>
        DestinationColor = SharpDX.Direct3D11.BlendOption.DestinationColor,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - R, 1 - G, 1 - B, 1 - A), that is color data from a render target. The pre-blend operation inverts the data, generating 1 - RGB.
        /// </para>
        /// </summary>
        InverseDestinationColor = SharpDX.Direct3D11.BlendOption.InverseDestinationColor,
        /// <summary>
        /// <para>
        /// The blend factor is (f, f, f, 1); where f = min(Aₛ, 1
        /// </para>
        /// <para>
        /// - A). The pre-blend operation clamps the data to 1 or less.
        /// </para>
        /// </summary>
        SourceAlphaSaturate = SharpDX.Direct3D11.BlendOption.SourceAlphaSaturate,
        /// <summary>
        /// <para>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. No pre-blend operation.
        /// </para>
        /// </summary>
        BlendFactor = SharpDX.Direct3D11.BlendOption.BlendFactor,
        /// <summary>
        /// <para>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
        /// </para>
        /// </summary>
        InverseBlendFactor = SharpDX.Direct3D11.BlendOption.InverseBlendFactor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources both as color data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        SecondarySourceColor = SharpDX.Direct3D11.BlendOption.SecondarySourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources both as color data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        InverseSecondarySourceColor = SharpDX.Direct3D11.BlendOption.InverseSecondarySourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources as alpha data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        SecondarySourceAlpha = SharpDX.Direct3D11.BlendOption.SecondarySourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is data sources as alpha data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - A. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        InverseSecondarySourceAlpha = SharpDX.Direct3D11.BlendOption.InverseSecondarySourceAlpha
    }
}