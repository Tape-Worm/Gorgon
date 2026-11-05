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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: November 4, 2026 3:40:53 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A codec for reading and writing a cache of <see cref="GorgonComputePso"/> objects, held by a <see cref="GorgonComputePsoFactory"/>, to a stream or file.
/// </summary>
/// <inheritdoc path="/param"/>
/// <remarks>
/// <inheritdoc cref="GorgonCodecComputePsoCacheCommon" path="/remarks/para[@type='preamble']"/>
/// <para>
/// This codec is used to read and write binary pipeline state object data using a custom format for Gorgon. The file format stores the compute shaders, and the compiled form of each pipeline state object. 
/// It also records the version of Gorgon, the GPU driver version, and the GPU hardware that were used to create the data. This ensures that the data is not loaded on a system with different hardware, a 
/// different driver version, or a different version of Gorgon. If an attempt to load this data is made with mismatched versions, then the user must regenerate their PSO cache(s).
/// </para>
/// <inheritdoc cref="GorgonCodecPsoCacheCommon{TFactory}" path="/remarks/para[@type='common']"/>
/// </remarks>
/// <seealso cref="GorgonComputePso"/>
/// <seealso cref="GorgonComputePsoFactory"/>
public class GorgonCodecComputePsoCache(GorgonGraphics graphics)
    : GorgonCodecComputePsoCacheCommon(graphics)
{
    /// <summary>
    /// The header chunk for a Gorgon binary compute PSO cache file.
    /// </summary>
    public const string BinaryPsoFileHeader = "GORCPC10";

    /// <summary>
    /// The chunk ID for the Gorgon version, GPU hardware, and GPU driver information in the PSO cache file.
    /// </summary>
    public const string BinaryDriverInfo = "DRVRINFO";

    /// <summary>
    /// The chunk ID for the shader data.
    /// </summary>
    public const string BinaryShaderData = "SHDRDATA";

    /// <summary>
    /// The chunk ID for the PSO data.
    /// </summary>
    public const string BinaryPsoData = "CPSODATA";

    /// <inheritdoc/>
    public override bool CanDecode => true;

    /// <inheritdoc/>
    public override bool CanEncode => true;

    /// <summary>
    /// Function to read a shader block of data from the given chunk reader.
    /// </summary>
    /// <param name="cache">The cache that will contain the pso.</param>
    /// <param name="reader">The reader to use for deserialization.</param>
    /// <returns>A new shader object, or the existing shader in the <paramref name="cache"/> with the same data.</returns>
    private GorgonShader ReadShaderBlock(GorgonComputePsoFactory cache, IGorgonChunkReader reader)
    {
        GorgonShader? result;

        byte[] ReadDataBlock(int size, IGorgonChunkReader reader)
        {
            if (result is not null)
            {
                reader.Skip(size);
                return [];
            }

            byte[] buffer = new byte[size];
            reader.ReadArray(buffer);
            return buffer;
        }

        ShaderType shaderType = (ShaderType)reader.ReadInt32();
        ShaderModel shaderModel = (ShaderModel)reader.ReadInt32();

        if (shaderType != ShaderType.ComputeShader)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_TYPE_NOT_SUPPORTED, shaderType));
        }

        if (Graphics.Adapter.ShaderModelSupport < shaderModel)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_MODEL_NOT_SUPPORTED, shaderModel));
        }

        int dataSize = reader.ReadInt32();

        if (dataSize <= 0)
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_SHADER_FILE_CORRUPT);
        }

        byte[] data = new byte[dataSize];
        reader.ReadArray(data);

        result = GetShaderInCache(cache, data);

        byte[] pdb = [];
        byte[] reflection = [];
        byte[] hash = [];
        string pdbName = string.Empty;

        byte flags = reader.ReadByte();

        if (flags != 0)
        {
            if ((flags & 4) == 4)
            {
                pdbName = reader.ReadString();
                pdb = ReadDataBlock(reader.ReadInt32(), reader);
            }

            if ((flags & 2) == 2)
            {
                reflection = ReadDataBlock(reader.ReadInt32(), reader);
            }

            if ((flags & 1) == 1)
            {
                hash = ReadDataBlock(reader.ReadInt32(), reader);
            }
        }

        result ??= CreateShader(shaderType, shaderModel, data, hash, pdb, pdbName, reflection);
        return result;
    }

    /// <summary>
    /// Function to write a shader block of data into the given chunk writer.
    /// </summary>
    /// <param name="shader">The shader data to serialize.</param>
    /// <param name="writer">The writer to use for serialization.</param>
    private static void WriteShaderBlock(GorgonShader shader, IGorgonChunkWriter writer)
    {
        writer.WriteInt32((int)shader.ShaderType);
        writer.WriteInt32((int)shader.ShaderModel);
        writer.WriteInt32(shader.ShaderData.Length);
        writer.WriteSpan(shader.ShaderData);

        int hasPdb = shader.PdbData.IsEmpty ? 0 : 4;
        int hasReflection = shader.ReflectionData.IsEmpty ? 0 : 2;
        int hasHash = shader.Hash.IsEmpty ? 0 : 1;

        writer.WriteByte((byte)(hasPdb | hasReflection | hasHash));

        if (!shader.PdbData.IsEmpty)
        {
            writer.WriteString(shader.PdbName);
            writer.WriteInt32(shader.PdbData.Length);
            writer.WriteSpan(shader.PdbData);
        }

        if (!shader.ReflectionData.IsEmpty)
        {
            writer.WriteInt32(shader.ReflectionData.Length);
            writer.WriteSpan(shader.ReflectionData);
        }

        if (!shader.Hash.IsEmpty)
        {
            writer.WriteInt32(shader.Hash.Length);
            writer.WriteSpan(shader.Hash);
        }
    }

    /// <inheritdoc/>
    protected override void OnEncodeToStream(GorgonComputePsoFactory cache, Stream stream)
    {
        Dictionary<GorgonShader, int> shaderTable = cache.GetShaders();

        using GorgonChunkFileWriter writer = new(stream, BinaryPsoFileHeader.ChunkID());
        writer.Open();

        AssemblyInformationalVersionAttribute? attribute = Graphics.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        Debug.Assert(!string.IsNullOrWhiteSpace(attribute?.InformationalVersion), "No version number was found for the assembly.");

        long driverVersion = Graphics.GetDriverVersion();

        // This metadata is used to determine if the compiled data can be used on the current GPU with a specific driver version.
        // If any of this mismatches, we cannot use this PSO and the user will have to rebuild it.
        using (IGorgonChunkWriter driverWriter = writer.OpenChunk(BinaryDriverInfo))
        {
            driverWriter.WriteString(attribute.InformationalVersion, Encoding.ASCII);
            driverWriter.WriteInt32(Graphics.Adapter.PciInfo.VendorID);
            driverWriter.WriteInt32(Graphics.Adapter.PciInfo.SubSystemID);
            driverWriter.WriteInt32(Graphics.Adapter.PciInfo.DeviceID);
            driverWriter.WriteInt32(Graphics.Adapter.PciInfo.Revision);
            driverWriter.WriteInt64(driverVersion);
        }

        // Write all shader data up front so we can index it later.
        using (IGorgonChunkWriter shaderWriter = writer.OpenChunk(BinaryShaderData))
        {
            shaderWriter.WriteInt32(shaderTable.Count);

            foreach (KeyValuePair<GorgonShader, int> shaderData in shaderTable)
            {
                shaderWriter.WriteInt32(shaderData.Value);
                WriteShaderBlock(shaderData.Key, shaderWriter);
            }
        }

        using IGorgonChunkWriter psoWriter = writer.OpenChunk(BinaryPsoData);

        psoWriter.WriteInt32(cache.PipelineStateObjects.Count);

        foreach (GorgonComputePso pso in cache.PipelineStateObjects.Values)
        {
            psoWriter.WriteString(pso.Name);

            psoWriter.WriteInt32(pso.PsoCachedBlob.Length);
            psoWriter.WriteSpan(pso.PsoCachedBlob);

            psoWriter.WriteInt32(shaderTable[pso.ComputeShader]);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the GPU driver version, the GPU, or the version of Gorgon does not match the information used to create the cache.
    /// </para>
    /// <para>Thrown if the cache data is corrupt, or contains a shader type or shader model that is not supported.</para>
    /// </exception>
    protected override void OnDecodeFromStream(GorgonComputePsoFactory psoFactory, Stream stream, long size)
    {
        string name = stream is FileStream fs ? fs.Name : "Unknown";

        using GorgonChunkFileReader reader = new(stream, [BinaryPsoFileHeader.ChunkID()]);
        reader.Open();

        long driverVersion = Graphics.GetDriverVersion();

        AssemblyInformationalVersionAttribute? attribute = Graphics.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        Debug.Assert(!string.IsNullOrWhiteSpace(attribute?.InformationalVersion), "No version number was found for the assembly.");

        // This metadata is used to determine if the compiled data can be used on the current GPU with a specific driver version.
        // If any of this mismatches, we cannot use this PSO and the user will have to rebuild it.
        using (IGorgonChunkReader driverReader = reader.OpenChunk(BinaryDriverInfo))
        {
            string gorgonVersion = driverReader.ReadString(Encoding.ASCII);
            int vendorID = driverReader.ReadInt32();
            int subSysID = driverReader.ReadInt32();
            int devID = driverReader.ReadInt32();
            int rev = driverReader.ReadInt32();

            if (!string.Equals(gorgonVersion, attribute.InformationalVersion, StringComparison.OrdinalIgnoreCase))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_GORGON_MISMATCH, name, gorgonVersion, attribute.InformationalVersion));
            }

            if ((vendorID != Graphics.Adapter.PciInfo.VendorID)
                || (subSysID != Graphics.Adapter.PciInfo.SubSystemID)
                || (devID != Graphics.Adapter.PciInfo.DeviceID)
                || (rev != Graphics.Adapter.PciInfo.Revision))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_GPU_MISMATCH, vendorID, subSysID, devID, rev, Graphics.Adapter.PciInfo.VendorID, Graphics.Adapter.PciInfo.SubSystemID, Graphics.Adapter.PciInfo.DeviceID, Graphics.Adapter.PciInfo.Revision));
            }

            long originalDriverVersion = driverReader.ReadInt64();

            if (originalDriverVersion != driverVersion)
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_DRIVER_MISMATCH, name));
            }
        }

        Dictionary<int, GorgonShader> shaders = [];

        using (IGorgonChunkReader shaderReader = reader.OpenChunk(BinaryShaderData))
        {
            int shaderCount = shaderReader.ReadInt32();

            if (shaderCount < 1)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_PSO_CACHE_FILE_NO_SHADERS);
            }

            for (int i = 0; i < shaderCount; ++i)
            {
                int linkValue = shaderReader.ReadInt32();

                Debug.Assert(linkValue >= 0, "Link value is less than 0. This cache is corrupt!");

                GorgonShader shader = ReadShaderBlock(psoFactory, shaderReader);

                if (shaders.ContainsKey(linkValue))
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_CACHE_CORRUPT_SHADER_LINK_ID_DUPLICATE, linkValue, name, shader.ShaderType));
                }

                shaders[linkValue] = shader;
            }
        }

        using IGorgonChunkReader psoReader = reader.OpenChunk(BinaryPsoData);

        int psoCount = psoReader.ReadInt32();

        for (int i = 0; i < psoCount; ++i)
        {
            name = psoReader.ReadString();

            int blobSize = psoReader.ReadInt32();
            byte[] blob = new byte[blobSize];
            psoReader.ReadArray(blob);

            int key = psoReader.ReadInt32();

            if (!shaders.TryGetValue(key, out GorgonShader? computeShader))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_CACHE_CORRUPT_CANNOT_FIND_SHADER_LINK, name, key));
            }

            BuildAndCachePso(name, blob, computeShader, psoFactory);
        }
    }

    /// <inheritdoc/>
    protected override ReadStatus OnIsReadable(Stream stream)
    {
        try
        {
            long dataSize = stream.Length - stream.Position;

            if (dataSize < 64)
            {
                return ReadStatus.InvalidFile;
            }

            IEnumerable<ulong> chunks = [BinaryPsoFileHeader.ChunkID()];

            if (!GorgonChunkFileReader.IsReadable(stream, chunks))
            {
                return ReadStatus.InvalidFile;
            }

            AssemblyInformationalVersionAttribute? attribute = Graphics.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            Debug.Assert(!string.IsNullOrWhiteSpace(attribute?.InformationalVersion), "No version number was found for the assembly.");

            using GorgonChunkFileReader reader = new(stream, chunks);
            reader.Open();

            long driverVersion = Graphics.GetDriverVersion();

            // This metadata is used to determine if the compiled data can be used on the current GPU with a specific driver version.
            // If any of this mismatches, we cannot use this PSO and the user will have to rebuild it.
            using IGorgonChunkReader driverReader = reader.OpenChunk(BinaryDriverInfo);

            string gorgonVersion = driverReader.ReadString(Encoding.ASCII);
            int vendorID = driverReader.ReadInt32();
            int subSysID = driverReader.ReadInt32();
            int devID = driverReader.ReadInt32();
            int rev = driverReader.ReadInt32();

            if (!string.Equals(gorgonVersion, attribute.InformationalVersion, StringComparison.OrdinalIgnoreCase))
            {
                Graphics.Log.PrintWarning($"PSO Cache was generated by Gorgon {gorgonVersion}, the current version is {attribute.InformationalVersion}. The cache should be regenerated.", LoggingLevel.Intermediate);
                return ReadStatus.DriverOrHardwareChanged;
            }

            ReadStatus status = ReadStatus.CanRead;

            if ((vendorID != Graphics.Adapter.PciInfo.VendorID)
                || (subSysID != Graphics.Adapter.PciInfo.SubSystemID)
                || (devID != Graphics.Adapter.PciInfo.DeviceID)
                || (rev != Graphics.Adapter.PciInfo.Revision))
            {
                Graphics.Log.PrintWarning($"PSO Cache: The hardware information has been changed for '{Graphics.Adapter.Name}'.", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"File values expected:", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"VendorID: {vendorID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"SubSystemID: {subSysID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"DeviceID: {devID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"Revision: {rev}\n", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"Actual values:", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"VendorID: {Graphics.Adapter.PciInfo.VendorID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"SubSystemID: {Graphics.Adapter.PciInfo.SubSystemID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"DeviceID: {Graphics.Adapter.PciInfo.DeviceID}", LoggingLevel.Intermediate);
                Graphics.Log.PrintWarning($"Revision: {Graphics.Adapter.PciInfo.Revision}", LoggingLevel.Intermediate);
                status = ReadStatus.DriverOrHardwareChanged;
            }

            long originalDriverVersion = driverReader.ReadInt64();

            if (originalDriverVersion != driverVersion)
            {
                Graphics.Log.PrintWarning($"PSO Cache: The driver version has been changed for '{Graphics.Adapter.Name}'. Expected: {originalDriverVersion} Actual: {driverVersion}", LoggingLevel.Intermediate);
                status = ReadStatus.DriverOrHardwareChanged;
            }

            return status;
        }
        catch (Exception ex)
        {
            Graphics.Log.PrintError("There was an error retrieving the status for the pipeline state cache data.", LoggingLevel.Simple);
            Graphics.Log.PrintException(ex);
            return ReadStatus.InvalidFile;
        }
    }
}
