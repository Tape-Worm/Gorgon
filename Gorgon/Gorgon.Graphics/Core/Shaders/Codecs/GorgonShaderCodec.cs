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
// Created: October 5, 2025 3:29:29 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// Defines the blocks of shaders available in a shader file.
/// </summary>
[Flags]
public enum ShaderDataBlocks
{
    /// <summary>
    /// No extra blocks of data were returned.
    /// </summary>
    None = 0,
    /// <summary>
    /// Shader data contains debugging information (PDB).
    /// </summary>
    DebugData = 1,
    /// <summary>
    /// Shader data contains reflection information.
    /// </summary>
    ReflectionData = 2
}

/// <summary>
/// A shader codec base class, used to define codecs for storing and loading shader data.
/// </summary>
/// <param name="graphics">The graphics interface used to validate shader data.</param>
/// <remarks>
/// <para>
/// Shader codecs allow applications to take shader binary data and persist it or read it from storage. The specific file format for a shader will use this base class to provide application specific formats.
/// </para>
/// </remarks>
public abstract class GorgonShaderCodec(GorgonGraphics graphics)
{
    /// <summary>
    /// Property to return the graphics interface used to assist in validating shader data.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

    /// <summary>
    /// Property to return whether this codec can decode shader data.
    /// </summary>
    public abstract bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether this codec can encode shader data.
    /// </summary>
    public abstract bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Function to decode the binary shader data from the stream.
    /// </summary>
    /// <param name="stream">The stream containing the binary shader data.</param>
    /// <param name="size">The size of the shader data, in bytes.</param>
    /// <returns>The shader object reconstituted from the stream data.</returns>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to extract the data and any metadata from the stream and reconstitute the <see cref="GorgonShader"/> using the provided <see cref="BuildShader"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="BuildShader"/>
    /// <seealso cref="GorgonShader"/>
    /// <example>
    /// <code language="csharp">
    /// 
    /// <![CDATA[
    /// protected override GorgonShader OnDecodeFromStream(Stream stream, long size)
    /// {
    ///    byte[] shaderData = ... Get your shader data ...    
    ///    ShaderType shaderType = ... Get your shader type ...
    ///    ShaderModel shaderModel = ... Get your shader model ...
    ///    bool hasDebug = ... Get flag to indicate debug info is present ...
    ///    
    ///    if (hasDebug)
    ///    {
    ///       byte[] pdbInfo = ... Get debug info if present ...
    ///       return BuildShader(shaderType, shaderModel, shaderData, pdbInfo);
    ///    }
    ///       
    ///    return BuildShader(shaderType, shaderModel, shaderData);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    protected abstract GorgonShader OnDecodeFromStream(Stream stream, long size);

    /// <summary>
    /// Function to decode the binary shader data from the stream.
    /// </summary>
    /// <param name="shader">The shader containing the data to persist to the stream.</param>
    /// <param name="stream">The stream containing the binary shader data.</param>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to write all the data, and any metadata to the stream in the format of their choosing.
    /// </para>
    /// </remarks>
    protected abstract void OnEncodeToStream(GorgonShader shader, Stream stream);

    /// <summary>
    /// Function to determine if the shader data at the current stream position can be read by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the shader data.</param>
    /// <returns><b>true</b> if the codec can read the shader in the stream, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to read any header or metadata information to identify the shader data.
    /// </para>
    /// </remarks>
    protected abstract bool OnIsReadable(Stream stream);

    /// <summary>
    /// Function to read the metadata of a shader from the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the shader to read.</param>
    /// <returns>A tuple containing the <see cref="ShaderType"/>, <see cref="ShaderModel"/> and flags via the <see cref="ShaderDataBlocks"/> enum that indicates any extra data in the shader file.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of the shader data plus the current stream position exceeds the stream length.</exception>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to read shader metadata information from the stream.
    /// </para>
    /// </remarks>
    protected abstract (ShaderType ShaderType, ShaderModel ShaderModel, ShaderDataBlocks DataBlocks) OnGetShaderMetadata(Stream stream);

    /// <summary>
    /// Function to create a new shader object with the data loaded from the storage medium.
    /// </summary>
    /// <param name="shaderType">The type of shader.</param>
    /// <param name="shaderModel">The shader model that was used to compile the shader.</param>
    /// <param name="shaderBinaryData">The compiled binary data for the shader.</param>
    /// <param name="shaderHashData">[Optional]The shader hash data.</param>
    /// <param name="optionalPdbData">[Optional] The debug information (PDB) for the shader, if available.</param>
    /// <param name="optionalPdbName">[Optional] The name of the PDB file.</param>
    /// <param name="optionalReflectionData">[Optional] The reflection information for the shader, if available.</param>
    /// <returns>A new <see cref="GorgonShader"/> object.</returns>
    /// <remarks>
    /// <para>
    /// When implementors override the <see cref="OnDecodeFromStream(Stream, long)"/> method, they must use this method to construct a new <see cref="GorgonShader"/> object to return to the user. How this 
    /// data is retrieved is up to the implementor of the codec, but the data must be returned in the same format as expected by Direct 3D 12.
    /// </para>
    /// <para>
    /// If the <paramref name="optionalPdbData"/> parameter is specified, it should contain the program database (PDB) for the shader debug information. If the shader does not have debug information, then 
    /// this parameter should be omitted.
    /// </para>
    /// <para>
    /// If the <paramref name="optionalReflectionData"/> parameter is specified, it should contain binary data detailing reflection information for the shader. If the shader does not have reflection 
    /// information, then this parameter should be omitted.
    /// </para>
    /// </remarks>
    /// <example>
    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)"/>
    /// </example>
    protected GorgonShader BuildShader(ShaderType shaderType, ShaderModel shaderModel, byte[] shaderBinaryData, byte[]? shaderHashData = null, byte[]? optionalPdbData = null, string? optionalPdbName = null, byte[]? optionalReflectionData = null) =>
        new(Graphics, shaderBinaryData, shaderHashData ?? [], optionalPdbData ?? [], optionalPdbName ?? string.Empty, optionalReflectionData ?? [], shaderType, shaderModel);

    /// <summary>
    /// Function to read binary shader data from the current position within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the shader binary data.</param>
    /// <param name="size">[Optional] The size, in bytes, of the shader data to read.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of the shader data plus the current stream position exceeds the stream length.</exception>
    /// <returns>The <see cref="GorgonShader"/> containing the binary shader data.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="size"/> is omitted, then the total length of the stream is used.
    /// </para>
    /// </remarks>
    public GorgonShader FromStream(Stream stream, long? size = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        size ??= stream.Length;

        if ((stream.Position + size) > stream.Length)
        {
            throw new EndOfStreamException();
        }

        return OnDecodeFromStream(stream, size.Value);
    }

    /// <summary>
    /// Function to read the metadata of a shader from the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the shader to read.</param>
    /// <returns>A tuple containing the <see cref="ShaderType"/>, <see cref="ShaderModel"/> and flags via the <see cref="ShaderDataBlocks"/> enum that indicates any extra data in the shader file.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of the shader data plus the current stream position exceeds the stream length.</exception>
    /// <remarks>
    /// <para>
    /// This method will store the current stream position, and reset the stream back to it once it has completed. 
    /// </para>
    /// </remarks>
    public (ShaderType ShaderType, ShaderModel ShaderModel, ShaderDataBlocks DataBlocks) GetShaderMetadata(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_CANNOT_SEEK, nameof(stream));
        }

        long currentPosition = stream.Position;

        if (currentPosition >= stream.Length)
        {
            throw new EndOfStreamException();
        }

        try
        {
            return OnGetShaderMetadata(stream);
        }
        finally
        {
            stream.Position = currentPosition;
        }
    }

    /// <summary>
    /// Function to read binary shader data from a file on the file system.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> is empty.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of the shader data plus the current stream position exceeds the stream length.</exception>
    /// <returns>The <see cref="GorgonShader"/> containing the binary shader data.</returns>
    public GorgonShader FromFile(string path)
    {
        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return FromStream(stream, stream.Length);
    }


    /// <summary>
    /// Function to persist the shader binary data to the specified stream.
    /// </summary>
    /// <param name="shader">The shader containing the data to persist.</param>
    /// <param name="stream">The stream to write the data into.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is read only.</exception>
    public void Save(GorgonShader shader, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_READONLY, nameof(stream));
        }

        OnEncodeToStream(shader, stream);
    }

    /// <summary>
    /// Function to persist the shader binary data to a file on the file system.
    /// </summary>
    /// <param name="shader">The shader containing the data to persist.</param>
    /// <param name="path">The path to the file on the file system.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> parameter is empty.</exception>
    public void Save(GorgonShader shader, string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(shader, stream);
    }

    /// <summary>
    /// Function to determine if the shader in the stream can be read by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the shader data.</param>
    /// <returns><b>true</b> if the codec can read the shader in the stream, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only, or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Throw if the current position of the <paramref name="stream"/> is at the end of the stream.</exception>
    /// <remarks>
    /// <para>
    /// This method will store the current stream position, and reset the stream back to it once it has completed. 
    /// </para>
    /// </remarks>
    public bool IsReadable(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_CANNOT_SEEK, nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        long streamPos = stream.Position;

        if (streamPos >= stream.Length)
        {
            throw new EndOfStreamException();
        }

        try
        {
            return OnIsReadable(stream);
        }
        finally
        {
            stream.Position = streamPos;
        }
    }
}
