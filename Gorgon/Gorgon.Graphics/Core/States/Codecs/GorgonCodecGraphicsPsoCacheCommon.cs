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
// Created: July 20, 2026 6:37:57 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A graphics pipeline state object cache codec base class containing common functionality used to define codecs for storing and loading graphics pipeline state object cache data.
/// </summary>
/// <param name="graphics">The graphics interface that is associated with the codec.</param>
/// <remarks>
/// <para type="preamble">
/// Pipeline state object codecs allow for persisting pipeline states and their associated <see cref="GorgonShader"/> objects to a file to be loaded at a later time. This allows applications to build a set of 
/// <see cref="GorgonGraphicsPso"/> objects once, and reuse them as long as the GPU, its driver, and the version of Gorgon do not change.
/// </para>
/// <para type="common">
/// <h3>But why?</h3>
/// <para>
/// While making small projects, a set of pipeline state objects won't take long to build and can be built at runtime. However, a larger project with thousands of states can take a while to create, and is 
/// often the cause of stuttering while rendering. There are ways to combat this problem, such as building all your states up front before rendering begins, which can mean a long pause before the application 
/// starts showing anything. The other way, provided by this codec, is to load precompiled pipeline states from a file, which is much faster and will help minimize stutter at the cost of either more memory 
/// used (the cache binary data is in memory and deserialized), or a single bump when loading the file from its storage area. It is up to the user to decide which strategy suits their needs.
/// </para>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonShader"/>
public abstract class GorgonCodecGraphicsPsoCacheCommon(GorgonGraphics graphics)
{
    /// <summary>
    /// Property to return the builder used to define the state for the pipeline state objects read by the codec.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonGraphicsPso"/> cannot be created directly. Implementors will assign the state read from the stream to this builder, and then pass it to the 
    /// <see cref="BuildAndCachePso(string, byte[], GorgonShader, GorgonGraphicsPsoFactory)"/> method to create the <see cref="GorgonGraphicsPso"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    protected GorgonGraphicsPsoBuilder PsoBuilder
    {
        get;
    } = new(graphics);

    /// <summary>
    /// Property to return the graphics interface associated with the codec.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

    /// <summary>
    /// Property to return whether the codec supports decoding (reading) of <see cref="GorgonGraphicsPso"/> data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementors must return <b>true</b> if the codec can read the format implemented by the implementation codec, otherwise the codec is write only and this value should return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    public abstract bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether the codec supports encoding (writing) of <see cref="GorgonGraphicsPso"/> data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementors must return <b>true</b> if the codec can write the format implemented by the implementation codec, otherwise the codec is read only and this value should return <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPso"/>
    public abstract bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Function to build a <see cref="GorgonGraphicsPso"/> using cached data.
    /// </summary>
    /// <param name="name">The name of the PSO.</param>
    /// <param name="psoBlob">The compiled binary data for the PSO that was stored in the cache data, or an empty array if no compiled data is available.</param>
    /// <param name="vertexShader">The required vertex shader for the PSO.</param>
    /// <param name="cache">The cache object that will contain the newly created PSO, or the existing PSO in the cache if one with the same name exists.</param>
    /// <returns>The newly created <see cref="GorgonGraphicsPso"/>, or the existing <see cref="GorgonGraphicsPso"/> in the <paramref name="cache"/> with the same <paramref name="name"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="name"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the pipeline state defined in the <see cref="PsoBuilder"/> is not valid, or is not supported by the video adapter.</exception>
    /// <remarks>
    /// <para>
    /// Implementors <b>must</b> use this method to create the <see cref="GorgonGraphicsPso"/> after its state has been read from the stream and assigned to the <see cref="PsoBuilder"/>. A 
    /// <see cref="GorgonGraphicsPso"/> cannot be created directly, so this method must be used for Gorgon to populate the <see cref="GorgonGraphicsPso"/> with the correct information.
    /// </para>
    /// <para>
    /// If the <paramref name="psoBlob"/> cannot be used by the current video adapter or driver, then it is discarded, and the <see cref="GorgonGraphicsPso"/> is compiled from its state instead.
    /// </para>
    /// <para>
    /// If the <paramref name="cache"/> already contains a PSO with the same <paramref name="name"/>, then the existing PSO is returned, and the state in the <see cref="PsoBuilder"/> is ignored. If the 
    /// existing PSO has different settings than the <see cref="PsoBuilder"/>, then a warning will be written to the log.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected GorgonGraphicsPso BuildAndCachePso(string name, byte[] psoBlob, GorgonShader vertexShader, GorgonGraphicsPsoFactory cache) =>  cache.CreateOrGetPso(name, vertexShader, PsoBuilder, psoBlob);

    /// <summary>
    /// Function to retrieve the <see cref="GorgonShader"/> object from the cache that matches the shader blob loaded from the stream.
    /// </summary>
    /// <param name="cache">The cache to evaluate.</param>
    /// <param name="shaderBlobData">The binary data for the shader.</param>
    /// <returns>The <see cref="GorgonShader"/> if one was found matching the data, or <b>null</b> if no shader was in the cache.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to determine if a <see cref="GorgonShader"/> with the same <paramref name="shaderBlobData"/> already exists in the cache. This avoids loading the shader data again if the cache already 
    /// has the same shader.
    /// </para>
    /// <para>
    /// This method should be called prior to reading any shader data in the cache file. If the cache file does not contain shader data (i.e. it's stored externally), then the user can load in the binary data 
    /// for the shader from their preferred storage and pass it through the <paramref name="shaderBlobData"/> parameter. The <see cref="GorgonShader.ShaderData"/> property on the <see cref="GorgonShader"/> 
    /// contains the shader binary data in all cases.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    /// <seealso cref="GorgonShader"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static GorgonShader? GetShaderInCache(GorgonGraphicsPsoFactory cache, ReadOnlySpan<byte> shaderBlobData)
    {
        int hashCode = GorgonShader.CreateNewShaderHashCode(shaderBlobData);
        return cache.FindShaderWithSignature(hashCode, shaderBlobData);
    }

    /// <inheritdoc cref="GorgonShaderCodecCommon.CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])" path="/summary"/>
    /// <inheritdoc cref="GorgonShaderCodecCommon.CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])" path="/param"/>
    /// <returns>A new <see cref="GorgonShader"/> object.</returns>
    /// <remarks>
    /// <para>
    /// When implementors override the <see cref="OnDecodeFromStream(GorgonGraphicsPsoFactory, Stream, long)"/> method, they must use this method to construct a new <see cref="GorgonShader"/> object. How this 
    /// data is retrieved is up to the implementor of the codec, but the data must be returned in the same format as expected by Gorgon. The only other way to create a <see cref="GorgonShader"/> is with the 
    /// <see cref="GorgonShaderCompiler.Compile(string, string, ShaderType, ShaderModel, CompileFlags, IReadOnlyList{GorgonShaderMacro}?)"/> method in the <see cref="GorgonShaderCompiler"/> object.
    /// </para>
    /// <inheritdoc cref="GorgonShaderCodecCommon.CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])" path="/remarks/para[@type='params']"/>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    /// <seealso cref="GorgonShaderCompiler"/>
    protected GorgonShader CreateShader(ShaderType shaderType, ShaderModel shaderModel, byte[] shaderBinaryData, byte[]? optionalHashData = null, byte[]? optionalPdbData = null, string? optionalPdbName = null, byte[]? optionalReflectionData = null) =>
        new(Graphics, shaderBinaryData, optionalHashData, optionalPdbData, optionalPdbName, optionalReflectionData, shaderType, shaderModel);

    /// <summary>
    /// Function to read the pipeline state object cache data from a stream into a <see cref="GorgonGraphicsPsoFactory"/>.
    /// </summary>
    /// <param name="psoFactory">The factory that will receive the <see cref="GorgonGraphicsPso"/> objects, <see cref="GorgonShader"/> objects, and states read from the stream.</param>
    /// <param name="stream">The stream to read the data from.</param>
    /// <param name="size">The size, in bytes, of the data to read.</param>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to extract the data and any metadata from the stream, and add the pipeline state objects to the <paramref name="psoFactory"/> using the provided 
    /// <see cref="BuildAndCachePso(string, byte[], GorgonShader, GorgonGraphicsPsoFactory)"/>, <see cref="GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})"/>, and 
    /// <see cref="CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])"/> methods.
    /// </para>
    /// <para>
    /// The <paramref name="psoFactory"/> may already contain pipeline state objects, shaders, and states when this method is called.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonShader"/>
    /// <seealso cref="BuildAndCachePso(string, byte[], GorgonShader, GorgonGraphicsPsoFactory)"/>
    /// <seealso cref="GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})"/>
    /// <seealso cref="CreateShader(ShaderType, ShaderModel, byte[], byte[], byte[], string?, byte[])"/>
    protected abstract void OnDecodeFromStream(GorgonGraphicsPsoFactory psoFactory, Stream stream, long size);

    /// <summary>
    /// Function to persist the <see cref="GorgonGraphicsPsoFactory"/> data out to a stream.
    /// </summary>
    /// <param name="cache">The cache containing the data to persist.</param>
    /// <param name="stream">The stream to write the data into.</param>
    /// <remarks>
    /// <para>
    /// This will serialize the cached PSOs, shaders and states in the provided <paramref name="cache"/> into a binary stream. 
    /// </para>
    /// </remarks>
    protected abstract void OnEncodeToStream(GorgonGraphicsPsoFactory cache, Stream stream);

    /// <summary>
    /// Function to determine if the pipeline state object cache data at the current stream position can be read by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the pipeline state object cache data.</param>
    /// <returns>The <see cref="ReadStatus"/> value indicating if the data can be read or not.</returns>
    /// <remarks>
    /// <para>
    /// Implementors will use this method to read any header or metadata information to identify the pipeline state object cache data.
    /// </para>
    /// <para>
    /// Due to the nature of pipeline state objects, implementors <b>must</b> return <see cref="ReadStatus.DriverOrHardwareChanged"/> if the driver that was used to build a PSO cache has changed. The same is 
    /// true for hardware changes, or if the Gorgon version number changes. In these cases the user <b>must</b> rebuild their cache.
    /// </para>
    /// </remarks>
    protected abstract ReadStatus OnIsReadable(Stream stream);

    /// <summary>
    /// Function to persist the <see cref="GorgonGraphicsPsoFactory"/> binary data to the specified stream.
    /// </summary>
    /// <inheritdoc cref="OnEncodeToStream(GorgonGraphicsPsoFactory, Stream)" path="/param"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="cache"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will save all of the cached PSOs, and their associated shaders and rendering states, into the specified <paramref name="stream"/>.
    /// </para>
    /// </remarks>
    public void Save(GorgonGraphicsPsoFactory cache, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_READONLY, nameof(stream));
        }

        if (cache.PipelineStateObjects.Count == 0)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_NO_PSO_DATA_TO_WRITE);
        }

        OnEncodeToStream(cache, stream);
    }

    /// <summary>
    /// Function to persist the <see cref="GorgonGraphicsPsoFactory"/> binary data to a file on the file system.
    /// </summary>
    /// <param name="cache"><inheritdoc cref="OnEncodeToStream(GorgonGraphicsPsoFactory, Stream)" path="/param[@name='cache']"/></param>
    /// <param name="path">The path to the file on the file system.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> parameter is empty.</exception>
    /// <inheritdoc cref="Save(GorgonGraphicsPsoFactory, Stream)" path="/exception[@cref='T:Gorgon.Core.GorgonException']"/>
    /// <remarks>
    /// <para>
    /// This will save all of the cached PSOs, and their associated shaders and rendering states, into the file with the specified <paramref name="path"/>.
    /// </para>
    /// </remarks>
    public void Save(GorgonGraphicsPsoFactory cache, string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(cache, stream);
    }

    /// <summary>
    /// Function to read binary pipeline state object cache data from the current position within a stream into an existing <see cref="GorgonGraphicsPsoFactory"/>.
    /// </summary>
    /// <param name="psoFactory">The factory that will receive the <see cref="GorgonGraphicsPso"/> objects, <see cref="GorgonShader"/> objects, and states read from the stream.</param>
    /// <param name="stream">The stream containing the pipeline state object cache binary data.</param>
    /// <param name="size">[Optional] The size, in bytes, of the pipeline state object cache data to read.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only, or cannot perform seek operations.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the <paramref name="size"/> is less than 1, or the size of the pipeline state object cache data plus the current stream position exceeds the stream length.</exception>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the current GPU driver version does not match the driver information used to create the cache.
    /// </para>
    /// <para>Thrown if the GPU that generated the cache does not match the currently installed GPU in the computer.</para>
    /// <para>Thrown if the version of Gorgon that generated the cache does not match the current version of Gorgon.</para>
    /// </exception>
    /// <remarks>
    /// <para type="hwnote">
    /// <note type="important">
    /// <para>
    /// Loading a pipeline state object cache requires that the current system has the exact driver version, hardware, and version of Gorgon that was used to generate the data. If the driver version, hardware, 
    /// or Gorgon version information in the cache data does not match the current system, then an exception will be thrown. Therefore, it is important to check the results of the 
    /// <see cref="IsReadable(Stream)"/> or <see cref="IsReadable(string)"/> methods prior to loading a cache.
    /// </para>
    /// </note>
    /// </para>
    /// <para type="existing">
    /// The pipeline state objects read from the cache data are added to the <paramref name="psoFactory"/>, alongside any pipeline state objects that it already contains. This allows multiple caches to be 
    /// loaded into a single factory without creating duplicate pipeline state objects. If the <paramref name="psoFactory"/> already contains a pipeline state object with the same name as one in the cache 
    /// data, then the existing pipeline state object is kept. If the two have different settings, then a warning will be written to the log. Shaders and states that already exist in the 
    /// <paramref name="psoFactory"/> are reused.
    /// </para>
    /// <para type="size">
    /// If the <paramref name="size"/> is omitted, then the remaining length of the stream is used.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsReadable(Stream)"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    public void FromStream(GorgonGraphicsPsoFactory psoFactory, Stream stream, long? size = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_CANNOT_SEEK, nameof(stream));
        }

        size ??= stream.Length - stream.Position;

        if ((size < 1) || ((stream.Position + size) > stream.Length))
        {
            throw new EndOfStreamException();
        }

        OnDecodeFromStream(psoFactory, stream, size.Value);
    }

    /// <summary>
    /// Function to read binary pipeline state object cache data from the current position within a stream into a new <see cref="GorgonGraphicsPsoFactory"/>.
    /// </summary>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/param[@name!='psoFactory']"/>
    /// <returns>A new <see cref="GorgonGraphicsPsoFactory"/> containing the <see cref="GorgonGraphicsPso"/> objects, <see cref="GorgonShader"/> objects, and states read from the stream.</returns>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/remarks/para[@type='hwnote']"/>
    /// <para>
    /// To load the cache data into an existing <see cref="GorgonGraphicsPsoFactory"/>, use the <see cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)"/> method instead.
    /// </para>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/remarks/para[@type='size']"/>
    /// </remarks>
    /// <seealso cref="IsReadable(Stream)"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    public GorgonGraphicsPsoFactory FromStream(Stream stream, long? size = null)
    {
        GorgonGraphicsPsoFactory result = new(Graphics);

        try
        {
            FromStream(result, stream, size);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }        
    }

    /// <summary>
    /// Function to read binary pipeline state object cache data from a file on the file system into an existing <see cref="GorgonGraphicsPsoFactory"/>.
    /// </summary>
    /// <param name="psoFactory"><inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/param[@name='psoFactory']"/></param>
    /// <param name="path">The path to the file to read.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> is empty.</exception>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/exception[@cref='T:Gorgon.Core.GorgonException']"/>
    /// <remarks>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/remarks/para[@type='hwnote']"/>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/remarks/para[@type='existing']"/>
    /// </remarks>
    /// <seealso cref="IsReadable(string)"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    public void FromFile(GorgonGraphicsPsoFactory psoFactory, string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        FromStream(psoFactory, stream, stream.Length);
    }

    /// <summary>
    /// Function to read binary pipeline state object cache data from a file on the file system into a new <see cref="GorgonGraphicsPsoFactory"/>.
    /// </summary>
    /// <inheritdoc cref="FromFile(GorgonGraphicsPsoFactory, string)" path="/param[@name='path']"/>
    /// <returns>A new <see cref="GorgonGraphicsPsoFactory"/> containing the <see cref="GorgonGraphicsPso"/> objects, <see cref="GorgonShader"/> objects, and states read from the file.</returns>
    /// <inheritdoc cref="FromFile(GorgonGraphicsPsoFactory, string)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="FromStream(GorgonGraphicsPsoFactory, Stream, long?)" path="/remarks/para[@type='hwnote']"/>
    /// <para>
    /// To load the cache data into an existing <see cref="GorgonGraphicsPsoFactory"/>, use the <see cref="FromFile(GorgonGraphicsPsoFactory, string)"/> method instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsReadable(string)"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    public GorgonGraphicsPsoFactory FromFile(string path)
    {
        GorgonGraphicsPsoFactory psoFactory = new(Graphics);

        try
        {
            FromFile(psoFactory, path);
            return psoFactory;
        }
        catch
        {
            psoFactory.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Function to determine if the pipeline state object cache in the stream can be read by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the pipeline state object cache data.</param>
    /// <inheritdoc cref="OnIsReadable(Stream)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is write-only, or cannot seek.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current position of the <paramref name="stream"/> is at the end of the stream.</exception>
    /// <remarks>
    /// <para>
    /// This method will store the current stream position, and reset the stream back to it once it has completed.
    /// </para>
    /// <para type="common">
    /// Due to the nature of pipeline state objects, when a system saves a pipeline state cache, the current driver, GPU hardware, and Gorgon version information is recorded with the data. This data is used to 
    /// confirm that the current hardware, driver version, and Gorgon version are the same when loading a pipeline state cache object. If they are found to be different, a 
    /// <see cref="ReadStatus.DriverOrHardwareChanged"/> value is returned from this method. When this happens, users must rebuild their pipeline state caches.
    /// </para>
    /// </remarks>
    /// <seealso cref="ReadStatus"/>
    public ReadStatus IsReadable(Stream stream)
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
    /// Function to determine if the pipeline state object cache in the file can be read by this codec.
    /// </summary>
    /// <param name="path">Path to the pipeline state object cache file.</param>
    /// <inheritdoc cref="IsReadable(Stream)" path="/returns"/>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="path"/> is empty.</exception>
    /// <remarks>
    /// <inheritdoc cref="IsReadable(Stream)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="IsReadable(Stream)" path="/seealso"/>
    public ReadStatus IsReadable(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return IsReadable(stream);
    }
}
