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
/// Parameters used to copy a buffer into a texture sub resource.
/// </summary>
public readonly ref struct GorgonCopyBufferToTexture
{
    /// <summary>
    /// Property to return the offset, in bytes, in the buffer to start copying from.
    /// </summary>
    public readonly long SourceOffset
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination array index.
    /// </summary>
    /// <remarks>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture3D"/>, then this value is ignored.
    /// </remarks>
    public readonly short DestinationArrayIndex
    {
        get;
        init;
    } = 0;


    /// <summary>
    /// Property to return the mip level on the destination texture to copy into.
    /// </summary>
    public readonly short DestinationMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination format plane index on the destination texture to copy into.
    /// </summary>
    public readonly byte DestinationPlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Function to increment/decrement the offset within the the source buffer.
    /// </summary>
    /// <param name="offsetBytes">The number of bytes to offset within the source buffer.</param>
    /// <returns>A new <see cref="GorgonCopyBufferToTexture"/> with the updated offset.</returns>
    public GorgonCopyBufferToTexture Offset(long offsetBytes) => new()
    {
        SourceOffset = SourceOffset + offsetBytes,
        DestinationArrayIndex = DestinationArrayIndex,
        DestinationMipLevel = DestinationMipLevel,
        DestinationPlane = DestinationPlane
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCopyBufferToTexture"/> value type.
    /// </summary>
    public GorgonCopyBufferToTexture()
    {
    }
}
