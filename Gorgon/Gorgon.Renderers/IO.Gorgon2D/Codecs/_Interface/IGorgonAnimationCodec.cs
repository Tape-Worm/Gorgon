
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 25, 2018 2:35:12 PM
// 

using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// An interface used to serialize or deserialize a <see cref="IGorgonAnimation"/>
/// </summary>
/// <seealso cref="IGorgonAnimation"/>
public interface IGorgonAnimationCodec
    : IGorgonNamedObject
{
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
    /// Property to return whether or not the codec can decode animation data.
    /// </summary>
    bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the codec can encode animation data.
    /// </summary>
    bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Property to return the version of animation data that the codec supports.
    /// </summary>
    Version Version
    {
        get;
    }

    /// <summary>
    /// Property to return the common file extensions for an animation.
    /// </summary>
    IReadOnlyList<GorgonFileExtension> FileExtensions
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the names of the textures associated with the animation.
    /// </summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    IReadOnlyList<string> GetAssociatedTextureNames(Stream stream);

    /// <summary>
    /// Function to read the animation data from a stream.
    /// </summary>        
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="byteCount">[Optional] The number of bytes to read from the stream.</param>
    /// <param name="name">[Optional] The name of the animation.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    /// <remarks>
    /// <para>
    /// When passing in a list of <paramref name="textureOverrides"/>, the texture names should match the expected texture names in the key frame. For example, if the 
    /// <see cref="GorgonKeyTexture2D.TextureName"/> is <c>"WalkingFrames"</c>, then the <see cref="GorgonTexture2D.Name"/> should also be <c>"WalkingFrames"</c>. 
    /// </para>
    /// </remarks>
    IGorgonAnimation FromStream(Stream stream, int? byteCount = null, string name = null, IEnumerable<GorgonTexture2DView> textureOverrides = null);

    /// <summary>
    /// Function to read the animation data from a file on the physical file system.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="name">[Optional] The name of the animation.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    /// <remarks>
    /// <para>
    /// When passing in a list of <paramref name="textureOverrides"/>, the texture names should match the expected texture names in the key frame. For example, if the 
    /// <see cref="GorgonKeyTexture2D.TextureName"/> is <c>"WalkingFrames"</c>, then the <see cref="GorgonTexture2D.Name"/> should also be <c>"WalkingFrames"</c>. 
    /// </para>
    /// </remarks>
    IGorgonAnimation FromFile(string filePath, string name = null, IEnumerable<GorgonTexture2DView> textureOverrides = null);

    /// <summary>
    /// Function to save the animation data to a stream.
    /// </summary>
    /// <param name="animation">The animation to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the animation.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/>, or the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the stream is read only.</exception>
    /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
    void Save(IGorgonAnimation animation, Stream stream);

    /// <summary>
    /// Function to save the animation data to a file on a physical file system.
    /// </summary>
    /// <param name="animation">The animation to serialize into the file.</param>
    /// <param name="filePath">The path to the file to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath" /> parameter is empty.</exception>
    /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
    void Save(IGorgonAnimation animation, string filePath);

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
    /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
    bool IsReadable(Stream stream);

}
