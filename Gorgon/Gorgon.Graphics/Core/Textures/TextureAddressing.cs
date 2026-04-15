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
// Created: April 14, 2026 10:56:37 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Values for texture addressing on a <see cref="GorgonSampler"/>.
/// </summary>
public enum TextureAddressing
{
    /// <summary>
    /// Repeats the texture on the axis this mode is applied to.
    /// </summary>
    Wrap = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_WRAP, 
    /// <summary>
    /// Mirrors the texture on the axis this mode is applied to.
    /// </summary>
    Mirror = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_MIRROR,
    /// <summary>
    /// Clamps the texture on the axis this mode is applied to.
    /// </summary>
    Clamp = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
    /// <summary>
    /// <para>
    /// Provides a border color on the axis this mode is applied to.
    /// </para>
    /// <para>
    /// The <see cref="GorgonSampler.BorderColor"/> value is used to provide the color.
    /// </para>
    /// </summary>
    Border = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_BORDER,
    /// <summary>
    /// Same as <see cref="Mirror"/>, except the mirroring only happens one time.
    /// </summary>
    MirrorOnce = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_MIRROR_ONCE,

}
