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
// Created: April 8, 2018 9:08:33 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how to handle texture coordinates that are outside of the bounds of a texture.
/// </summary>
[Flags]
public enum TextureWrap
{
    /// <summary>
    /// <para>
    /// Tile the texture at every (u,v) integer junction. For example, for u values between 0 and 3, the texture is repeated three times.
    /// </para>
    /// </summary>
    Wrap = D3D11.TextureAddressMode.Wrap,
    /// <summary>
    /// <para>
    /// Flip the texture at every (u,v) integer junction. For u values between 0 and 1, for example, the texture is addressed normally; between 1 and 2, the texture is flipped (mirrored); between 2 and 3, the texture is normal again; and so on. 
    /// </para>
    /// </summary>
    Mirror = D3D11.TextureAddressMode.Mirror,
    /// <summary>
    /// <para>
    /// Texture coordinates outside the range [0.0, 1.0] are set to the texture color at 0.0 or 1.0, respectively.
    /// </para>
    /// </summary>
    Clamp = D3D11.TextureAddressMode.Clamp,
    /// <summary>
    /// <para>
    /// Texture coordinates outside the range [0.0, 1.0] are set to the border color specified in <see cref="GorgonSamplerState"/> or HLSL code.
    /// </para>
    /// </summary>
    Border = D3D11.TextureAddressMode.Border,
    /// <summary>
    /// <para>
    /// Similar to TextureWrap.Mirror and TextureWrap.Clamp. Takes the absolute value of the texture coordinate (thus, mirroring around 0), and then clamps to the maximum value.
    /// </para>
    /// </summary>
    MirrorOnce = D3D11.TextureAddressMode.MirrorOnce
}
