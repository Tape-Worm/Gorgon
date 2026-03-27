// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 26, 2025 9:52:31 PM
//

using Gorgon.Graphics.Imaging;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information used to build a swap chain.
/// </summary>
/// <param name="Width">The width, in pixels, of the swap chain buffers.</param>
/// <param name="Height">The height, in pixels, of the swap chain buffers.</param>
/// <param name="Format">The pixel format of the swap chain buffers.</param>
/// <remarks>
/// <para>
/// The <see cref="Format"/> property must support being used as a display format. Use the <see cref="GorgonGraphics.FormatSupport"/> property to determine if the format is suitable for display.
/// </para>
/// <para>
/// The <see cref="Width"/> and <see cref="Height"/> must be at least 1 pixel, otherwise the swap chain will fail upon creation.
/// </para>
/// </remarks>
public record class GorgonSwapChainInfo(int Width, int Height, BufferFormat Format)
{
    /// <summary>
    /// An empty version of the <see cref="GorgonSwapChainInfo"/> type.
    /// </summary>
    public static readonly GorgonSwapChainInfo Empty = new(0, 0, BufferFormat.Unknown);

    /// <summary>
    /// Property to return the number of texture resources for the swap chain.
    /// </summary>
    internal uint ResourceCount => TripleBuffer ? 3u : 2u;

    /// <summary>
    /// Function to convert this object to a DXGI swap chain description.
    /// </summary>
    /// <returns>The swap chain description.</returns>
    internal DXGI_SWAP_CHAIN_DESC1 ToDXGI() => new()
    {
        AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_IGNORE,
        BufferCount = ResourceCount,
        BufferUsage = DXGI.DXGI_USAGE_RENDER_TARGET_OUTPUT,
        Flags = (uint)(DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH | DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING | DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_FRAME_LATENCY_WAITABLE_OBJECT),
        Format = (DXGI_FORMAT)Format,
        SampleDesc = GorgonMultisampleInfo.NoMultisampling.ToDXGI(),
        Scaling = AllowScaling ? DXGI_SCALING.DXGI_SCALING_STRETCH : DXGI_SCALING.DXGI_SCALING_NONE,
        Stereo = false,
        SwapEffect = FlipDiscard ? DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD : DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
        Width = (uint)Width,
        Height = (uint)Height,
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// The default value is <b>true</b>.
    /// </remarks>
    public bool FlipDiscard
    {
        get;
        init;
    } = true;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// <inheritdoc/>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool TripleBuffer
    {
        get;
        init;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// The default value is <b>true</b>.
    /// </remarks>
    public bool AllowScaling
    {
        get;
        init;
    } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
    /// </summary>
    /// <param name="info">The settings to copy.</param>
    public GorgonSwapChainInfo(GorgonSwapChainInfo info)
    {
        Width = info.Width;
        Height = info.Height;
        Format = info.Format;
        FlipDiscard = info.FlipDiscard;
        TripleBuffer = info.TripleBuffer;
        AllowScaling = info.AllowScaling;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
    /// </summary>
    /// <param name="info">The settings to copy.</param>
    public GorgonSwapChainInfo(IGorgonSwapChainInfo info)
        : this(info.Width, info.Height, info.Format)
    {
        FlipDiscard = info.FlipDiscard;
        TripleBuffer = info.TripleBuffer; 
        AllowScaling = info.AllowScaling;
    }
}
