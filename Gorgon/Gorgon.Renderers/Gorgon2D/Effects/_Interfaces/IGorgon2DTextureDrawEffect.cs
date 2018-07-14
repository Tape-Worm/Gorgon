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
// Created: July 14, 2018 11:31:44 AM
// 
#endregion

using DX = SharpDX;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An interface that defines an effect that draws a texture to the current render target as a post process effect.
    /// </summary>
    public interface IGorgon2DTextureDrawEffect
    {
        /// <summary>
        /// Function to render the effect.
        /// </summary>
        /// <param name="texture">The texture containing the image to burn or dodge.</param>
        /// <param name="region">[Optional] The region to draw the texture info.</param>
        /// <param name="textureCoordinates">[Optional] The texture coordinates, in texels, to use when drawing the texture.</param>
        /// <param name="samplerStateOverride">[Optional] An override for the current texture sampler.</param>
        /// <param name="blendStateOverride">[Optional] The blend state to use when rendering.</param>
        /// <param name="camera">[Optional] The camera used to render the image.</param>
        /// <remarks>
        /// <para>
        /// Renders the specified <paramref name="texture"/> using 1 bit color.
        /// </para>
        /// <para>
        /// If the <paramref name="region"/> parameter is omitted, then the texture will be rendered to the full size of the current render target.  If it is provided, then texture will be rendered to the
        /// location specified, and with the width and height specified.
        /// </para>
        /// <para>
        /// If the <paramref name="textureCoordinates"/> parameter is omitted, then the full size of the texture is rendered.
        /// </para>
        /// <para>
        /// If the <paramref name="samplerStateOverride"/> parameter is omitted, then the <see cref="GorgonSamplerState.Default"/> is used.  When provided, this will alter how the pixel shader samples our
        /// texture in slot 0.
        /// </para>
        /// <para>
        /// If the <paramref name="blendStateOverride"/>, parameter is omitted, then the <see cref="GorgonBlendState.Default"/> is used. 
        /// </para>
        /// <para>
        /// The <paramref name="camera"/> parameter is used to render the texture using a different view, and optionally, a different coordinate set.  
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        void RenderEffect(GorgonTexture2DView texture,
                                 DX.RectangleF? region = null,
                                 DX.RectangleF? textureCoordinates = null,
                                 GorgonSamplerState samplerStateOverride = null,
                                 GorgonBlendState blendStateOverride = null,
                                 Gorgon2DCamera camera = null);

    }
}
