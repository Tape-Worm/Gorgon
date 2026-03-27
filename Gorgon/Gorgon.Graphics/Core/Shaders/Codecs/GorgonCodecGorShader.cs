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
// Created: October 5, 2025 4:22:39 PM
//

using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A codec for reading and writing Gorgon binary shader data.
/// </summary>
/// <param name="graphics"><inheritdoc/></param>
/// <remarks>
/// <para>
/// This codec is used to read and write binary shader data using a custom format for Gorgon. This file format includes metadata about shader type, shader model, and other validation information.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// This file format is not backwards compatible with shader files from previous versions of Gorgon. Files will need to be recompiled and saved with this codec before they can be loaded.
/// </para>
/// </note>
/// </para>
/// </remarks>
public sealed class GorgonCodecGorShader(GorgonGraphics graphics)
    : GorgonShaderCodec(graphics)
{
    /// <summary>
    /// The header chunk for a Gorgon binary shader file.
    /// </summary>
    public const string BinaryShaderFileHeader = "GORSHD30";

    /// <summary>
    /// The chunk ID for the chunk that contains metadata for the shader.
    /// </summary>
    public const string BinaryShaderMetaData = "METADATA";

    /// <summary>
    /// The chunk ID for the chunk that contains the binary shader bytecode.
    /// </summary>
    public const string BinaryShaderByteCode = "BYTECODE";

    /// <summary>
    /// The chunk ID for the optional chunk that contains the the hash data for the shader.
    /// </summary>
    public const string HashData = "HASHDATA";

    /// <summary>
    /// The chunk ID for the optional chunk that contains the debugging data for the shader.
    /// </summary>
    public const string PdbData = "PDB_DATA";

    /// <summary>
    /// The chunk ID for the optional chunk that contains the reflection data for the shader.
    /// </summary>
    public const string ReflectionData = "REFLECTN";

    /// <inheritdoc/>
    public override bool CanDecode => true;

    /// <inheritdoc/>
    public override bool CanEncode => true;

    /// <inheritdoc/>
    protected override (ShaderType ShaderType, ShaderModel ShaderModel, ShaderDataBlocks DataBlocks) OnGetShaderMetadata(Stream stream)
    {
        ShaderType shaderType;
        ShaderModel shaderModel;


        using GorgonChunkFileReader reader = new(stream, [
            BinaryShaderFileHeader.ChunkID()
            ]);

        reader.Open();

        if (!reader.Chunks.Contains(BinaryShaderMetaData))
        {
            return (ShaderType.None, ShaderModel.Unsupported, ShaderDataBlocks.None);
        }

        using (IGorgonChunkReader metadataReader = reader.OpenChunk(BinaryShaderMetaData))
        {
            shaderType = (ShaderType)metadataReader.ReadInt32();
            shaderModel = (ShaderModel)metadataReader.ReadInt32();
        }

        ShaderDataBlocks dataBlocks = ShaderDataBlocks.None;

        if (reader.Chunks.Contains(PdbData))
        {
            dataBlocks |= ShaderDataBlocks.DebugData;
        }

        if (reader.Chunks.Contains(ReflectionData))
        {
            dataBlocks |= ShaderDataBlocks.ReflectionData;
        }

        return (shaderType, shaderModel, dataBlocks);
    }

    /// <inheritdoc/>
    protected unsafe override GorgonShader OnDecodeFromStream(Stream stream, long size)
    {
        using GorgonChunkFileReader reader = new(stream, [
            BinaryShaderFileHeader.ChunkID()
            ]);

        reader.Open();

        uint dataSize = 0;
        ShaderType shaderType;
        ShaderModel shaderModel;

        using (IGorgonChunkReader metadataReader = reader.OpenChunk(BinaryShaderMetaData))
        {
            shaderType = (ShaderType)metadataReader.ReadInt32();
            shaderModel = (ShaderModel)metadataReader.ReadInt32();
            dataSize = metadataReader.ReadUInt32();

            if (dataSize == 0)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_SHADER_FILE_CORRUPT);
            }
        }

        if (shaderType == ShaderType.None)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_TYPE_NOT_SUPPORTED, shaderType));
        }

        byte[] shaderBinaryData;
        byte[] hashData = [];
        byte[] pdbData = [];
        byte[] reflectionData = [];
        string pdbName = string.Empty;

        using (IGorgonChunkReader dataReader = reader.OpenChunk(BinaryShaderByteCode))
        {
            using ComPtr<ID3DBlob> blob = default;

            DX.D3DCreateBlob(dataSize, blob.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotRead, () => Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER);

            shaderBinaryData = new byte[dataSize];

            dataReader.ReadArray(shaderBinaryData);
        }

        if (reader.Chunks.Contains(HashData))
        {
            using IGorgonChunkReader hashReader = reader.OpenChunk(HashData);
            int hashSize = 0;

            hashSize = hashReader.ReadInt32();
            if (hashSize != 0)
            {
                hashData = new byte[hashSize];
                hashReader.ReadArray(hashData);
            }
        }

        if (reader.Chunks.Contains(PdbData))
        {
            using IGorgonChunkReader debugReader = reader.OpenChunk(PdbData);
            uint debugSize = 0;

            pdbName = debugReader.ReadString();

            debugSize = debugReader.ReadUInt32();

            if (debugSize != 0)
            {
                pdbData = new byte[debugSize];
                debugReader.ReadArray(pdbData);
            }
        }

        if (reader.Chunks.Contains(ReflectionData))
        {
            using IGorgonChunkReader reflectReader = reader.OpenChunk(ReflectionData);
            uint reflectSize = 0;
            reflectSize = reflectReader.ReadUInt32();

            if (reflectSize != 0)
            {
                reflectionData = new byte[reflectSize];
                reflectReader.ReadArray(reflectionData);
            }
        }

        return BuildShader(shaderType, shaderModel, shaderBinaryData, hashData, pdbData, pdbName, reflectionData);
    }

    /// <inheritdoc/>
    protected override void OnEncodeToStream(GorgonShader shader, Stream stream)
    {
        using GorgonChunkFileWriter writer = new(stream, BinaryShaderFileHeader.ChunkID());

        writer.Open();

        using (IGorgonChunkWriter metadataWriter = writer.OpenChunk(BinaryShaderMetaData))
        {
            metadataWriter.WriteInt32((int)shader.ShaderType);
            metadataWriter.WriteInt32((int)shader.ShaderModel);
            metadataWriter.WriteInt32(shader.ShaderData.Length);
        }

        using (IGorgonChunkWriter dataWriter = writer.OpenChunk(BinaryShaderByteCode))
        {
            dataWriter.WriteSpan(shader.ShaderData);
        }

        if (shader.Hash.Length != 0)
        {
            using IGorgonChunkWriter hashWriter = writer.OpenChunk(HashData);

            hashWriter.WriteInt32(shader.Hash.Length);
            hashWriter.WriteSpan(shader.Hash);
        }

        if (shader.PdbData.Length != 0)
        {
            using IGorgonChunkWriter debugWriter = writer.OpenChunk(PdbData);

            debugWriter.WriteString(shader.PdbName);
            debugWriter.WriteInt32(shader.PdbData.Length);
            debugWriter.WriteSpan(shader.PdbData);
        }

        if (shader.ReflectionData.Length == 0)
        {
            return;
        }

        using IGorgonChunkWriter reflectionWriter = writer.OpenChunk(ReflectionData);

        reflectionWriter.WriteInt32(shader.ReflectionData.Length);
        reflectionWriter.WriteSpan(shader.ReflectionData);
    }

    /// <inheritdoc/>
    protected override bool OnIsReadable(Stream stream)
    {
        using BinaryReader headerReader = new(stream, Encoding.UTF8, true);

        return (headerReader.ReadInt64() == GorgonChunkFile.FileFormatHeaderIDv0100)
            && (headerReader.ReadUInt64() == BinaryShaderFileHeader.ChunkID());
    }
}
