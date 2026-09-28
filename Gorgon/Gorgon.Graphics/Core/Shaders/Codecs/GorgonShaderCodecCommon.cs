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
/// Defines the optional blocks of data available in a shader file.
/// </summary>
[Flags]
public enum ShaderDataBlocks
{
    /// <summary>
    /// No extra blocks of data were returned.
    /// </summary>
    None = 0,
    /// <summary>
    /// Shader data contains the hash for the shader.
    /// </summary>
    HashData = 1,
    /// <summary>
    /// Shader data contains debugging information (PDB).
    /// </summary>
    DebugData = 2,
    /// <summary>
    /// Shader data contains reflection information.
    /// </summary>
    ReflectionData = 4    
}

/// <summary>
/// A shader metadata record returned by the <see cref="GorgonShaderCodecCommon.GetShaderMetadata(Stream)"/> method.
/// </summary>
/// <param name="ShaderType">The type of shader.</param>
/// <param name="ShaderModel">The shader model version for the shader.</param>
/// <param name="DataBlocks">The types of extra data in the shader.</param>
/// <seealso cref="GorgonShaderCodecCommon.GetShaderMetadata(Stream)"/>
public readonly record struct ShaderMetadata(ShaderType ShaderType, ShaderModel ShaderModel, ShaderDataBlocks DataBlocks);

/// <summary>
/// A shader codec base class containing common functionality used to define codecs for storing and loading shader data.
/// </summary>
/// <param name="graphics">The graphics interface associated with the codec.</param>
/// <remarks>
/// <para>
/// Shader codecs allow applications to take <see cref="GorgonShader"/> binary data and persist it to or read it from storage. The specific file format for a shader will use this base class to provide 
/// application specific formats.
/// </para>
/// </remarks>
/// <seealso cref="GorgonShader"/>
public abstract class GorgonShaderCodecCommon(GorgonGraphics graphics)
{
    /// <summary>
    /// Property to return the graphics interface associated with this codec.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

    /// <summary>
    /// Property to return whether this codec supports decoding (reading) of <see cref="GorgonShader"/> data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementors must return <b>true</b> if the codec can read the format implemented by the implementation codec, otherwise the codec is write only and this value should return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    public abstract bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether this codec supports encoding (writing) of <see cref="GorgonShader"/> data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementors must return <b>true</b> if the codec can write the format implemented by the implementation codec, otherwise the codec is read only and this value should return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    public abstract bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Function to read the binary shader data from the current position in the stream.
    /// </summary>
    /// <param name="stream">The stream to read the data from.</param>
    /// <param name="size">The size of the shader data, in bytes.</param>
    /// <returns>The shader object reconstituted from the stream data.</returns>
    /// <remarks>
    /// <para type="common">
    /// This will create a new <see cref="GorgonShader"/>, based on the data contained within the <paramref name="stream"/>.
    /// </para>
    /// <para>
    /// Implementors will use this method to extract the data and any metadata from the stream and reconstitute the <see cref="GorgonShader"/> using the provided <see cref="CreateShader"/> method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// <![CDATA[
    /// // How the developer will read the data is entirely up to them.
    /// protected override GorgonShader OnDecodeFromStream(Stream stream, long size)
    /// {    
    ///    ShaderType shaderType = // Read shader type...
    ///    ShaderModel shaderModel = // Read shader model...
    ///    byte[] shaderData = // Read shader data...
    ///    bool hasDebug = // Read debug flag...
    ///    
    ///    if (hasDebug)
    ///    {
    ///       byte[] pdbInfo = // Read pdb data...
    ///       return CreateShader(shaderType, shaderModel, shaderData, null, pdbInfo);
    ///    }
    ///
    ///    return CreateShader(shaderType, shaderModel, shaderData);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonShader"/>
    /// <seealso cref="CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])"/>
    protected abstract GorgonShader OnDecodeFromStream(Stream stream, long size);

    /// <summary>
    /// Function to write the shader binary data to the specified stream.
    /// </summary>
    /// <param name="shader">The shader containing the data to persist to the stream.</param>
    /// <param name="stream">The stream to write the data into.</param>
    /// <remarks>
    /// <para type="common">
    /// This method writes out the <see cref="GorgonShader"/> object contents into the specified <paramref name="stream"/>. 
    /// </para>
    /// <para>
    /// Implementors will use this method to write all the data, and any metadata to the stream in the format of their choosing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// <![CDATA[
    /// // How the developer will write the data is entirely up to them.
    /// protected override void OnEncodeToStream(GorgonShader shader, Stream stream)
    /// {
    ///    ShaderType shaderType = shader.ShaderType;
    ///    ShaderModel shaderModel = shader.ShaderModel;
    ///    ReadOnlySpan<byte> shaderData = shader.ShaderData;
    ///    bool hasDebug = shader.PdbData.Length != 0;
    ///    ReadOnlySpan<byte> pdbInfo = shader.PdbData;
    ///    
    ///    // Write shaderType...
    ///    // Write shaderModel...    
    ///    // Write shaderData...
    ///    // Write hasDebug...
    ///    
    ///    if (hasDebug)
    ///    {
    ///       // Write pdbInfo...
    ///    }    
    /// }
    /// ]]>
    /// </code>
    /// </example>
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
    /// <returns>A <see cref="ShaderMetadata"/> record containing information about the shader.</returns>
    /// <remarks>
    /// <para type="common">
    /// This method queries the shader data in the stream and returns a <see cref="ShaderMetadata"/> record that will tell the user what type of shader is in the stream, what shader model version was used, 
    /// and which optional blocks of data the shader contains as a <see cref="ShaderDataBlocks"/> bit mask. If the <see cref="ShaderMetadata.ShaderModel"/> is <see cref="ShaderModel.Unsupported"/>, then the
    /// shader cannot be read or used.
    /// </para>
    /// <para>
    /// Implementors will use this method to read shader metadata information from the stream.
    /// </para>
    /// </remarks>
    protected abstract ShaderMetadata OnGetShaderMetadata(Stream stream);

    /// <summary>
    /// Function to create a new shader object with the data loaded from the storage medium.
    /// </summary>
    /// <param name="shaderType">The type of shader.</param>
    /// <param name="shaderModel">The shader model that was used to compile the shader.</param>
    /// <param name="shaderBinaryData">The compiled binary data for the shader.</param>
    /// <param name="optionalHashData">[Optional] The shader hash data.</param>
    /// <param name="optionalPdbData">[Optional] The debug information (PDB) for the shader, if available.</param>
    /// <param name="optionalPdbName">[Optional] The name of the PDB file.</param>
    /// <param name="optionalReflectionData">[Optional] The reflection information for the shader, if available.</param>
    /// <returns>A new <see cref="GorgonShader"/> object.</returns>
    /// <remarks>
    /// <para>
    /// When implementors override the <see cref="OnDecodeFromStream(Stream, long)"/> method, they must use this method to construct a new <see cref="GorgonShader"/> object to return to the user. How this 
    /// data is retrieved is up to the implementor of the codec, but the data must be returned in the same format as expected by Gorgon. There is no other ability to directly create a 
    /// <see cref="GorgonShader"/> aside from the <see cref="GorgonShaderCompiler.Compile(string, string, ShaderType, ShaderModel, CompileFlags, IReadOnlyList{GorgonShaderMacro}?)"/> method in the 
    /// <see cref="GorgonShaderCompiler"/> object.
    /// </para>
    /// <para type="params">
    /// If the <paramref name="optionalPdbData"/> parameter is specified, it should contain the program database (PDB) for the shader debug information. If the shader does not have debug information, then 
    /// this parameter should be omitted.
    /// </para>
    /// <para type="params">
    /// If the <paramref name="optionalReflectionData"/> parameter is specified, it should contain binary data detailing reflection information for the shader. If the shader does not have reflection 
    /// information, then this parameter should be omitted.
    /// </para>
    /// </remarks>
    /// <example>
    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)"/>
    /// </example>
    /// <seealso cref="GorgonShader"/>
    /// <seealso cref="GorgonShaderCompiler"/>
    protected GorgonShader CreateShader(ShaderType shaderType, ShaderModel shaderModel, byte[] shaderBinaryData, byte[]? optionalHashData = null, byte[]? optionalPdbData = null, string? optionalPdbName = null, byte[]? optionalReflectionData = null) =>
        new(Graphics, shaderBinaryData, optionalHashData, optionalPdbData, optionalPdbName, optionalReflectionData, shaderType, shaderModel);

    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/summary"/>
    /// <param name="stream"><inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/param[@name='stream']"/></param>
    /// <param name="size">[Optional] <inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/param[@name='size']"/></param>
    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the <paramref name="size"/> is less than 1, or the <paramref name="size"/> is omitted and the current position of the <paramref name="stream"/> is at the end of the stream.</exception>
    /// <remarks>
    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/remarks/para[@type='common']"/>
    /// <para>
    /// If the <paramref name="size"/> is omitted, then the remaining length of the stream is used as the size.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    public GorgonShader FromStream(Stream stream, long? size = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        size ??= stream.Length - stream.Position;

        if (size <= 0)
        {
            throw new EndOfStreamException();
        }

        return OnDecodeFromStream(stream, size.Value);
    }

    /// <summary>
    /// Function to read binary shader data from a file path.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <inheritdoc cref="OnDecodeFromStream(Stream, long)" path="/returns"/>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> is empty.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the file is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will create a new shader, based on the data contained within the file in the specified <paramref name="path"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="FromStream(Stream, long?)" path="/seealso"/>
    public GorgonShader FromFile(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return FromStream(stream, stream.Length);
    }

    /// <summary>
    /// Function to read the metadata of a shader from the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the shader to read.</param>
    /// <inheritdoc cref="OnGetShaderMetadata(Stream)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current position of the <paramref name="stream"/> is at the end of the stream.</exception>
    /// <remarks>
    /// <inheritdoc cref="OnGetShaderMetadata(Stream)" path="/remarks/para[@type='common']"/>
    /// <para>
    /// This method will store the current stream position, and reset the stream back to it once it has completed. 
    /// </para>
    /// </remarks>
    /// <seealso cref="ShaderMetadata"/>
    public ShaderMetadata GetShaderMetadata(Stream stream)
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
    /// Function to read the metadata of a shader from the file on the specified path.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <inheritdoc cref="OnGetShaderMetadata(Stream)" path="/returns"/>
    /// <inheritdoc cref="FromFile(string)" path="/exception[@cref='T:Gorgon.Core.ArgumentEmptyException']"/>
    /// <remarks>
    /// <inheritdoc cref="OnGetShaderMetadata(Stream)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="GetShaderMetadata(Stream)" path="/seealso"/>
    public ShaderMetadata GetShaderMetadata(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return OnGetShaderMetadata(stream);
    }

    /// <inheritdoc cref="OnEncodeToStream(GorgonShader, Stream)" path="/summary"/>
    /// <inheritdoc cref="OnEncodeToStream(GorgonShader, Stream)" path="/param"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is read only.</exception>
    /// <remarks>
    /// <inheritdoc cref="OnEncodeToStream(GorgonShader, Stream)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="FromStream(Stream, long?)" path="/seealso"/>
    public void Save(GorgonShader shader, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_READONLY, nameof(stream));
        }

        OnEncodeToStream(shader, stream);
    }

    /// <summary>
    /// Function to persist the shader binary data to a file.
    /// </summary>
    /// <param name="shader"><inheritdoc cref="OnEncodeToStream(GorgonShader, Stream)" path="/param[@name='shader']"/></param>
    /// <param name="path">The path to the file on the file system.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method writes out the <see cref="GorgonShader"/> object contents into the file specified by the <paramref name="path"/>. 
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="FromStream(Stream, long?)" path="/seealso"/>
    public void Save(GorgonShader shader, string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        OnEncodeToStream(shader, stream);
    }

    /// <inheritdoc cref="OnIsReadable(Stream)" path="/summary"/>
    /// <inheritdoc cref="OnIsReadable(Stream)" path="/param"/>
    /// <inheritdoc cref="OnIsReadable(Stream)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only, or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current position of the <paramref name="stream"/> is at the end of the stream.</exception>
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

    /// <summary>
    /// Function to determine if the shader data in the file can be read by this codec.
    /// </summary>
    /// <inheritdoc cref="FromFile(string)" path="/param"/>
    /// <inheritdoc cref="OnIsReadable(Stream)" path="/returns"/>
    /// <inheritdoc cref="FromFile(string)" path="/exception[@cref='T:Gorgon.Core.ArgumentEmptyException']"/>
    public bool IsReadable(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return OnIsReadable(stream);
    }
}
