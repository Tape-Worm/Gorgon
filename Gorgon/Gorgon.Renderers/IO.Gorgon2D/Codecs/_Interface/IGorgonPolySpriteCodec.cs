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
// Created: August 11, 2018 3:36:34 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// An interface used to serialize and deserialize <see cref="GorgonPolySprite"/> objects.
/// </summary>
/// <seealso cref="GorgonPolySprite"/>
public interface IGorgonPolySpriteCodec
    : IGorgonGraphicsObject, IGorgonNamedObject
{
    #region Properties.
    /// <summary>
    /// Property to return the friendly description of the format.
    /// </summary>
    string CodecDescription
    {
        get;
    }

    /// <summary>
    /// Property to return the abbreviated name of the codec (e.g. PNG).
    /// </summary>
    string Codec
    {
        get;
    }

    /// <summary>
    /// Property to return the renderer used to create objects.
    /// </summary>
    Gorgon2D Renderer
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the codec can decode sprite data.
    /// </summary>
    bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the codec can encode sprite data.
    /// </summary>
    bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Property to return the version of sprite data that the codec supports.
    /// </summary>
    Version Version
    {
        get;
    }

    /// <summary>
    /// Property to return the common file extensions for a polygonal sprite.
    /// </summary>
    IReadOnlyList<GorgonFileExtension> FileExtensions
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve the name of the associated texture.
    /// </summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
    string GetAssociatedTextureName(Stream stream);

    /// <summary>
    /// Function to read the sprite data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the sprite.</param>
    /// <param name="overrideTexture">[Optional] The texture to use as an override for the sprite.</param>
    /// <param name="byteCount">[Optional] The number of bytes to read from the stream.</param>
    /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
    GorgonPolySprite FromStream(Stream stream, GorgonTexture2DView overrideTexture = null, int? byteCount = null);

    /// <summary>
    /// Function to read the sprite data from a file on the physical file system.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="overrideTexture">[Optional] The texture to use as an override for the sprite.</param>
    /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
    GorgonPolySprite FromFile(string filePath, GorgonTexture2DView overrideTexture = null);

    /// <summary>
    /// Function to save the sprite data to a stream.
    /// </summary>
    /// <param name="sprite">The sprite to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the sprite.</param>
    void Save(GorgonPolySprite sprite, Stream stream);

    /// <summary>
    /// Function to save the sprite data to a file on a physical file system.
    /// </summary>
    /// <param name="sprite">The sprite to serialize into the file.</param>
    /// <param name="filePath">The path to the file.</param>
    void Save(GorgonPolySprite sprite, string filePath);

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    bool IsReadable(Stream stream);
    #endregion
}
