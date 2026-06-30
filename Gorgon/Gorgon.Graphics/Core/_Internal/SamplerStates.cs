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
// Created: April 15, 2026 1:24:03 AM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of pre-defined sampler states for convenience.
/// </summary>
internal class SamplerStates
    : IDisposable
{
    /// <summary>
    /// Property to return the default sampler state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies <see cref="TextureFilter.PointMinMagMip">point filtering</see> for minified, magnified and mip map levels, with addressing of <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public GorgonSampler Default
    {
        get;
    }

    /// <summary>
    /// Property to return a wrapping sampler state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies <see cref="TextureFilter.PointMinMagMip">point filtering</see> for minified, magnified and mip map levels, with addressing of <see cref="TextureAddressing.Wrap"/>.
    /// </para>
    /// </remarks>
    public GorgonSampler Wrapping
    {
        get;
    }

    /// <summary>
    /// Property to return the linear sampler state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies <see cref="TextureFilter.LinearMinMagMip">linear filtering</see> for minified, magnified and mip map levels, with addressing of <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public GorgonSampler Linear
    {
        get;
    }

    /// <summary>
    /// Property to return a linear wrapping sampler state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies <see cref="TextureFilter.LinearMinMagMip">linear filtering</see> for minified, magnified and mip map levels, with addressing of <see cref="TextureAddressing.Wrap"/>.
    /// </para>
    /// </remarks>
    public GorgonSampler LinearWrapping
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Default.Dispose();
            Wrapping.Dispose();
            Linear.Dispose();
            LinearWrapping.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplerStates"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns these samplers.</param>
    public SamplerStates(GorgonGraphics graphics)
    {
        Default = new GorgonSampler(graphics, nameof(Default));
        Wrapping = new GorgonSampler(graphics, nameof(Wrapping))
        {
            UAddressing = TextureAddressing.Wrap,
            VAddressing = TextureAddressing.Wrap,
            WAddressing = TextureAddressing.Wrap
        };
        Linear = new GorgonSampler(graphics, nameof(Linear))
        {
            Filter = TextureFilter.LinearMinMagMip
        };
        LinearWrapping = new GorgonSampler(graphics, nameof(LinearWrapping))
        {
            Filter = TextureFilter.LinearMinMagMip,
            UAddressing = TextureAddressing.Wrap,
            VAddressing = TextureAddressing.Wrap,
            WAddressing = TextureAddressing.Wrap
        };

        Default.UnregisterDisposable(graphics);
        Wrapping.UnregisterDisposable(graphics);
        Linear.UnregisterDisposable(graphics);
        LinearWrapping.UnregisterDisposable(graphics);

        // Pre-allocate the handles.
        Default.GetViewHandle();
        Wrapping.GetViewHandle();
        Linear.GetViewHandle();
        LinearWrapping.GetViewHandle();
    }
}
