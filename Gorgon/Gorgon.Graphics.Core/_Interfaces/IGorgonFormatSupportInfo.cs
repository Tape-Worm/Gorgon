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
// Created: April 7, 2018 11:35:23 PM
// 
#endregion

using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the support given to a specific <see cref="BufferFormat"/>.
/// </summary>
public interface IGorgonFormatSupportInfo
{
    /// <summary>
    /// Property to return the format that is being queried for support.
    /// </summary>
    BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the resource support for a format.
    /// </summary>
    BufferFormatSupport FormatSupport
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is suitable for use for presentation to the output device.
    /// </summary>
    bool IsDisplayFormat
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is suitable for use as a render target.
    /// </summary>
    bool IsRenderTargetFormat
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is suitable for use in a depth/stencil buffer.
    /// </summary>
    bool IsDepthBufferFormat
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is suitable for use in a vertex buffer.
    /// </summary>
    bool IsVertexBufferFormat
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is suitable for use in an index buffer.
    /// </summary>
    bool IsIndexBufferFormat
    {
        get;
    }

    /// <summary>
    /// Property to return the compute shader/uav support for a format.
    /// </summary>
    ComputeShaderFormatSupport ComputeSupport
    {
        get;
    }

    /// <summary>
    /// Property to return the maximum multisample count and quality level support for the format.
    /// </summary>
    GorgonMultisampleInfo MaxMultisampleCountQuality
    {
        get;
    }

    /// <summary>
    /// Function to determine if a format is suitable for the texture type specified by <see cref="ImageType"/>.
    /// </summary>
    /// <param name="imageType">The image type to evaluate.</param>
    /// <returns><b>true</b> if suitable, <b>false</b> if not.</returns>
    bool IsTextureFormat(ImageType imageType);
}