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
// Created: January 16, 2026 2:28:28 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Parameters used to copy a texture sub resource to a buffer.
/// </summary>
public readonly ref struct GorgonCopyTextureToBuffer
{
    /// <summary>
    /// An empty texture sub resource parameter.
    /// </summary>
    public static GorgonCopyTextureToBuffer Empty => default;

    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceMipLevel == 0)
                && (SourceArrayIndex == 0) && (SourcePlane == 0) && (DestinationOffset == 0);

    /// <summary>
    /// Property to return source mip level to copy.
    /// </summary>
    public readonly short SourceMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the source array index to copy from.
    /// </summary>
    /// <remarks>
    /// If the texture is a <see cref="TextureType.Texture3D"/>, then this value will be ignored, otherwise it will be the source array index for a 
    /// <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
    /// </remarks>
    public readonly short SourceArrayIndex
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the source format plane index on the texture to copy from.
    /// </summary>
    public readonly byte SourcePlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the offset, in bytes, within the buffer to start copying into.
    /// </summary>
    public readonly long DestinationOffset
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCopyTextureToBuffer"/> value type.
    /// </summary>
    public GorgonCopyTextureToBuffer()
    {
    }
}
