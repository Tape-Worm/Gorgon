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
// Created: June 30, 2026 7:31:00 PM
//

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A codec for reading and writing a cache of <see cref="GorgonGraphicsPso"/> objects, held by a <see cref="GorgonGraphicsPsoFactory"/>, to a stream or file.
/// </summary>
/// <inheritdoc path="/param"/>
/// <remarks>
/// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon" path="/remarks/para[@type='preamble']"/>
/// <para>
/// This codec is used to read and write binary pipeline state object data using a custom format for Gorgon. The file format stores the pipeline states, their shaders, and the compiled form of each pipeline 
/// state object. It also records the version of Gorgon, the GPU driver version, and the GPU hardware that were used to create the data. This ensures that the data is not loaded on a system with different 
/// hardware, a different driver version, or a different version of Gorgon. If an attempt to load this data is made with mismatched versions, then the user must regenerate their PSO cache(s).
/// </para>
/// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon" path="/remarks/para[@type='common']"/>
/// </remarks>
public class GorgonCodecGraphicsPsoCache(GorgonGraphics graphics)
    : GorgonCodecGraphicsPsoCacheCommon(graphics)
{
    /// <summary>
    /// The header chunk for a Gorgon binary PSO cache file.
    /// </summary>
    public const string BinaryPsoFileHeader = "GORGPC10";

    /// <summary>
    /// The chunk ID for the Gorgon version, GPU hardware, and GPU driver information in the PSO cache file.
    /// </summary>
    public const string BinaryDriverInfo = "DRVRINFO";

    /// <summary>
    /// The chunk ID for the shader data.
    /// </summary>
    public const string BinaryShaderData = "SHDRDATA";

    /// <summary>
    /// The chunk ID for raster state data.
    /// </summary>
    public const string RasterStateData = "RSTRSTAT";

    /// <summary>
    /// The chunk ID for blend state data.
    /// </summary>
    public const string BlendStateData = "BLNDSTAT";

    /// <summary>
    /// The chunk ID for depth/stencil state data.
    /// </summary>
    public const string DepthStencilStateData = "DPSTSTAT";

    /// <summary>
    /// The chunk ID for the PSO data.
    /// </summary>
    public const string BinaryPsoData = "GPSODATA";

    /// <inheritdoc/>
    public override bool CanDecode => true;

    /// <inheritdoc/>
    public override bool CanEncode => true;

    /// <summary>
    /// Function to count the shaders used by a PSO.
    /// </summary>
    /// <param name="pso">The PSO to evaluate.</param>
    /// <returns>The number of shaders used by the PSO.</returns>
    private static int CountShaders(GorgonGraphicsPso pso)
    {
        // We always have a vertex shader, this is a minimum for a graphics PSO.
        int result = 1;

        if (pso.PixelShader is not null)
        {
            ++result;
        }

        if (pso.GeometryShader is not null)
        {
            ++result;
        }

        if (pso.HullShader is not null)
        {
            ++result;
        }

        if (pso.DomainShader is not null)
        {
            ++result;
        }

        return result;
    }

    /// <summary>
    /// Function to read a raster state block of data from the given chunk reader.
    /// </summary>
    /// <param name="cache">The cache that will contain the pso.</param>
    /// <param name="reader">The reader to use for deserialization.</param>
    /// <returns>A new raster state object.</returns>
    private static GorgonRasterState ReadRasterStateBlock(GorgonGraphicsPsoFactory cache, IGorgonChunkReader reader)
    {
        GorgonRasterState rasterState = new()
        {
            CullMode = (CullingMode)reader.ReadInt32(),
            IsFrontCounterClockwise = reader.ReadBool(),
            DepthBias = reader.ReadInt32(),
            DepthBiasClamp = reader.ReadSingle(),
            SlopeScaledDepthBias = reader.ReadSingle(),
            FillMode = (FillMode)reader.ReadInt32(),
            ForcedReadWriteViewSampleCount = reader.ReadInt32(),
            IsDepthClippingEnabled = reader.ReadBool(),
            LineRasterizationMode = (LineRasterizationMode)reader.ReadInt32(),
            UseConservativeRasterization = reader.ReadBool()
        };

        return cache.FindRasterState(rasterState) ?? rasterState;
    }

    /// <summary>
    /// Function to read a blend state block of data from the given chunk reader.
    /// </summary>
    /// <param name="cache">The cache that will contain the pso.</param>
    /// <param name="reader">The reader to use for deserialization.</param>
    /// <returns>A new blend state object.</returns>
    private static GorgonBlendState ReadBlendStateBlock(GorgonGraphicsPsoFactory cache, IGorgonChunkReader reader)
    {
        GorgonBlendState blendState = new()
        {
            AlphaBlendOperation = (BlendOperation)reader.ReadInt32(),
            ColorBlendOperation = (BlendOperation)reader.ReadInt32(),
            DestinationAlphaBlend = (Blend)reader.ReadInt32(),
            DestinationColorBlend = (Blend)reader.ReadInt32(),
            IsEnabled = reader.ReadBool(),
            IsLogicEnabled = reader.ReadBool(),
            LogicOperation = (LogicOperation)reader.ReadInt32(),
            SourceAlphaBlend = (Blend)reader.ReadInt32(),
            SourceColorBlend = (Blend)reader.ReadInt32(),
            WriteMask = (WriteMask)reader.ReadInt32()
        };

        return cache.FindBlendState(blendState) ?? blendState;
    }

    /// <summary>
    /// Function to read a depth/stencil state block of data from the given chunk reader.
    /// </summary>
    /// <param name="cache">The cache that will contain the pso.</param>
    /// <param name="reader">The reader to use for deserialization.</param>
    /// <returns>A new depth/stencil state object.</returns>
    private static GorgonDepthStencilState ReadDepthStencilStateBlock(GorgonGraphicsPsoFactory cache, IGorgonChunkReader reader)
    {
        GorgonDepthStencilState depthStencilState = new()
        {
            DepthFunction = (ComparisonFunction)reader.ReadInt32(),
            IsDepthEnabled = reader.ReadBool(),
            IsDepthWriteEnabled = reader.ReadBool(),
            IsDepthBoundsTestingEnabled = reader.ReadBool(),
            IsStencilEnabled = reader.ReadBool(),
            BackFaceStencilOperation = new GorgonStencilOperation()
            {
                StencilFunction = (ComparisonFunction)reader.ReadInt32(),
                PassOperation = (StencilOperation)reader.ReadInt32(),
                FailOperation = (StencilOperation)reader.ReadInt32(),
                DepthFailOperation = (StencilOperation)reader.ReadInt32(),
                ReadMask = reader.ReadByte(),
                WriteMask = reader.ReadByte(),
            },
            FrontFaceStencilOperation = new GorgonStencilOperation()
            {
                StencilFunction = (ComparisonFunction)reader.ReadInt32(),
                PassOperation = (StencilOperation)reader.ReadInt32(),
                FailOperation = (StencilOperation)reader.ReadInt32(),
                DepthFailOperation = (StencilOperation)reader.ReadInt32(),
                ReadMask = reader.ReadByte(),
                WriteMask = reader.ReadByte(),
            }
        };

        return cache.FindDepthStencilState(depthStencilState) ?? depthStencilState;
    }

    /// <summary>
    /// Function to read a shader block of data from the given chunk reader.
    /// </summary>
    /// <param name="cache">The cache that will contain the pso.</param>
    /// <param name="reader">The reader to use for deserialization.</param>
    /// <returns>A new shader object.</returns>
    private GorgonShader ReadShaderBlock(GorgonGraphicsPsoFactory cache, IGorgonChunkReader reader)
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
        byte flags;

        reader.ReadArray(data);

        result = GetShaderInCache(cache, data);

        byte[] pdb = [];
        byte[] reflection = [];
        byte[] hash = [];
        string pdbName = string.Empty;

        flags = reader.ReadByte();

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
    /// Function to write a raster state block into the given chunk writer.
    /// </summary>
    /// <param name="rasterState">The state to write.</param>
    /// <param name="writer">The writer to use for serialization.</param>
    private static void WriteRasterBlock(GorgonRasterState rasterState, IGorgonChunkWriter writer)
    {
        writer.WriteInt32((int)rasterState.CullMode);
        writer.WriteBool(rasterState.IsFrontCounterClockwise);
        writer.WriteInt32(rasterState.DepthBias);
        writer.WriteSingle(rasterState.DepthBiasClamp);
        writer.WriteSingle(rasterState.SlopeScaledDepthBias);
        writer.WriteInt32((int)rasterState.FillMode);
        writer.WriteInt32(rasterState.ForcedReadWriteViewSampleCount);
        writer.WriteBool(rasterState.IsDepthClippingEnabled);
        writer.WriteInt32((int)rasterState.LineRasterizationMode);
        writer.WriteBool(rasterState.UseConservativeRasterization);
    }

    /// <summary>
    /// Function to write a blend state block into the given chunk writer.
    /// </summary>
    /// <param name="blendState">The state to write.</param>
    /// <param name="writer">The writer to use for serialization.</param>
    private static void WriteBlendBlock(GorgonBlendState blendState, IGorgonChunkWriter writer)
    {
        writer.WriteInt32((int)blendState.AlphaBlendOperation);
        writer.WriteInt32((int)blendState.ColorBlendOperation);
        writer.WriteInt32((int)blendState.DestinationAlphaBlend);
        writer.WriteInt32((int)blendState.DestinationColorBlend);
        writer.WriteBool(blendState.IsEnabled);
        writer.WriteBool(blendState.IsLogicEnabled);
        writer.WriteInt32((int)blendState.LogicOperation);
        writer.WriteInt32((int)blendState.SourceAlphaBlend);
        writer.WriteInt32((int)blendState.SourceColorBlend);
        writer.WriteInt32((int)blendState.WriteMask);
    }

    /// <summary>
    /// Function to write a depth/stencil block into the given chunk writer.
    /// </summary>
    /// <param name="depthStencilState">The state to write.</param>
    /// <param name="writer">The writer to use for serialization.</param>
    private static void WriteDepthStencilBlock(GorgonDepthStencilState depthStencilState, IGorgonChunkWriter writer)
    {
        writer.WriteInt32((int)depthStencilState.DepthFunction);
        writer.WriteBool(depthStencilState.IsDepthEnabled);
        writer.WriteBool(depthStencilState.IsDepthWriteEnabled);
        writer.WriteBool(depthStencilState.IsDepthBoundsTestingEnabled);
        writer.WriteBool(depthStencilState.IsStencilEnabled);
        writer.WriteInt32((int)depthStencilState.BackFaceStencilOperation.StencilFunction);
        writer.WriteInt32((int)depthStencilState.BackFaceStencilOperation.PassOperation);
        writer.WriteInt32((int)depthStencilState.BackFaceStencilOperation.FailOperation);
        writer.WriteInt32((int)depthStencilState.BackFaceStencilOperation.DepthFailOperation);
        writer.WriteByte(depthStencilState.BackFaceStencilOperation.ReadMask);
        writer.WriteByte(depthStencilState.BackFaceStencilOperation.WriteMask);
        writer.WriteInt32((int)depthStencilState.FrontFaceStencilOperation.StencilFunction);
        writer.WriteInt32((int)depthStencilState.FrontFaceStencilOperation.PassOperation);
        writer.WriteInt32((int)depthStencilState.FrontFaceStencilOperation.FailOperation);
        writer.WriteInt32((int)depthStencilState.FrontFaceStencilOperation.DepthFailOperation);
        writer.WriteByte(depthStencilState.FrontFaceStencilOperation.ReadMask);
        writer.WriteByte(depthStencilState.FrontFaceStencilOperation.WriteMask);
    }

    /// <summary>
    /// Function to write a shader block of data into the given chunk writer.
    /// </summary>
    /// <param name="shader">The shader data to serialize.</param>
    /// <param name="writer">The writer to use for serialization.</param>
    private static void WriteShaderBlock(GorgonShader? shader, IGorgonChunkWriter writer)
    {
        if (shader is null)
        {
            return;
        }

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
    protected override void OnEncodeToStream(GorgonGraphicsPsoFactory cache, Stream stream)
    {
        Dictionary<GorgonShader, int> shaderTable = cache.GetShaders();
        Dictionary<GorgonRasterState, int> rasterTable = cache.GetRasterStates();
        Dictionary<GorgonBlendState, int> blendTable = cache.GetBlendStates();
        Dictionary<GorgonDepthStencilState, int> depthStencilTable = cache.GetDepthStencilStates();

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

        using (IGorgonChunkWriter rasterStateWriter = writer.OpenChunk(RasterStateData))
        {
            rasterStateWriter.WriteInt32(rasterTable.Count);

            foreach (KeyValuePair<GorgonRasterState, int> raster in rasterTable)
            {
                rasterStateWriter.WriteInt32(raster.Value);
                WriteRasterBlock(raster.Key, rasterStateWriter);
            }
        }

        using (IGorgonChunkWriter blendStateWriter = writer.OpenChunk(BlendStateData))
        {
            blendStateWriter.WriteInt32(blendTable.Count);

            foreach (KeyValuePair<GorgonBlendState, int> blend in blendTable)
            {
                blendStateWriter.WriteInt32(blend.Value);
                WriteBlendBlock(blend.Key, blendStateWriter);
            }
        }

        using (IGorgonChunkWriter depthStencilStateWriter = writer.OpenChunk(DepthStencilStateData))
        {
            depthStencilStateWriter.WriteInt32(depthStencilTable.Count);

            foreach (KeyValuePair<GorgonDepthStencilState, int> depthStencil in depthStencilTable)
            {
                depthStencilStateWriter.WriteInt32(depthStencil.Value);
                WriteDepthStencilBlock(depthStencil.Key, depthStencilStateWriter);
            }
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

        foreach (GorgonGraphicsPso pso in cache.PipelineStateObjects.Values)
        {
            Debug.Assert(pso.VertexShader is not null, $"Vertex shader for {pso.Name} is null.");

            psoWriter.WriteString(pso.Name);

            psoWriter.WriteInt32(pso.PsoCachedBlob.Length);
            psoWriter.WriteSpan(pso.PsoCachedBlob);

            psoWriter.WriteInt32(CountShaders(pso));

            psoWriter.WriteInt32(shaderTable[pso.VertexShader]);

            if (pso.PixelShader is not null)
            {
                psoWriter.WriteInt32(shaderTable[pso.PixelShader]);
            }            

            if (pso.GeometryShader is not null)
            {
                psoWriter.WriteInt32(shaderTable[pso.GeometryShader]);
            }            

            if (pso.HullShader is not null)
            {
                psoWriter.WriteInt32(shaderTable[pso.HullShader]);
            }            

            if (pso.DomainShader is not null)
            {
                psoWriter.WriteInt32(shaderTable[pso.DomainShader]);
            }            

            psoWriter.WriteInt32((int)pso.PrimitiveType);
            psoWriter.WriteInt32((int)pso.IndexBufferStripCutIdentifier);
            psoWriter.WriteInt32(pso.Multisample.Count);
            psoWriter.WriteInt32(pso.Multisample.Quality);
            psoWriter.WriteInt32(pso.MultisampleMask);

            // Write raster state index.
            if (rasterTable.TryGetValue(pso.RasterizerState, out int rasterIndex))
            {
                psoWriter.WriteInt32(rasterIndex);
            }
            else
            {
                psoWriter.WriteInt32(rasterTable[GorgonRasterState.Default]);
            }

            // Blend state.
            psoWriter.WriteBool(pso.IsAlphaToCoverageEnabled);
            psoWriter.WriteBool(pso.IsIndependentBlendingEnabled);

            for (int i = 0; i < pso.BlendStates.Length; ++i)
            {
                // Write blend state index.
                if (blendTable.TryGetValue(pso.BlendStates[i], out int blendIndex))
                {
                    psoWriter.WriteInt32(blendIndex);
                }
                else
                {
                    psoWriter.WriteInt32(blendTable[GorgonBlendState.Default]);
                }
            }

            psoWriter.WriteByte((byte)pso.OutputFormats.Length);
            for (int i = 0; i < pso.OutputFormats.Length; ++i)
            {
                psoWriter.WriteInt32((int)pso.OutputFormats[i]);
            }

            // Depth state.
            psoWriter.WriteInt32((int)pso.DepthStencilFormat);
            
            if (depthStencilTable.TryGetValue(pso.DepthStencilState, out int depthStencilIndex))
            {
                psoWriter.WriteInt32(depthStencilIndex);
            }
            else
            {
                psoWriter.WriteInt32(depthStencilTable[GorgonDepthStencilState.Default]);
            }
            
        }
    }

    /// <inheritdoc/>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the GPU driver version, the GPU, or the version of Gorgon does not match the information used to create the cache.
    /// </para>
    /// <para>Thrown if the cache data is corrupt, or contains a shader type or shader model that is not supported.</para>
    /// </exception>
    protected override void OnDecodeFromStream(GorgonGraphicsPsoFactory psoFactory, Stream stream, long size)
    {
        int psoCount;
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
        Dictionary<int, GorgonRasterState> rasterStates = [];
        Dictionary<int, GorgonBlendState> blendStates = [];
        Dictionary<int, GorgonDepthStencilState> depthStencilStates = [];

        using (IGorgonChunkReader rasterStateReader = reader.OpenChunk(RasterStateData))
        {
            int rasterStateCount = rasterStateReader.ReadInt32();

            for (int i = 0; i < rasterStateCount; i++)
            {
                int linkValue = rasterStateReader.ReadInt32();

                Debug.Assert(linkValue >= 0, "Link value is less than 0. This cache is corrupt!");

                GorgonRasterState state = ReadRasterStateBlock(psoFactory, rasterStateReader);
                rasterStates[linkValue] = state;
            }
        }

        using (IGorgonChunkReader blendStateReader = reader.OpenChunk(BlendStateData))
        {
            int blendStateCount = blendStateReader.ReadInt32();

            for (int i = 0; i < blendStateCount; i++)
            {
                int linkValue = blendStateReader.ReadInt32();

                Debug.Assert(linkValue >= 0, "Link value is less than 0. This cache is corrupt!");

                GorgonBlendState state = ReadBlendStateBlock(psoFactory, blendStateReader);
                blendStates[linkValue] = state;
            }
        }

        using (IGorgonChunkReader depthStencilStateReader = reader.OpenChunk(DepthStencilStateData))
        {
            int depthStencilStateCount = depthStencilStateReader.ReadInt32();

            for (int i = 0; i < depthStencilStateCount; i++)
            {
                int linkValue = depthStencilStateReader.ReadInt32();

                Debug.Assert(linkValue >= 0, "Link value is less than 0. This cache is corrupt!");

                GorgonDepthStencilState state = ReadDepthStencilStateBlock(psoFactory, depthStencilStateReader);
                depthStencilStates[linkValue] = state;
            }
        }

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

        IGorgonChunkReader psoReader = reader.OpenChunk(BinaryPsoData);

        try
        {
            psoCount = psoReader.ReadInt32();

            for (int i = 0; i < psoCount; ++i)
            {
                GorgonShader? vertexShader = null;

                PsoBuilder.Clear();

                name = psoReader.ReadString();

                int blobSize = psoReader.ReadInt32();
                byte[] blob = new byte[blobSize];
                psoReader.ReadArray(blob);

                int shaderCount = psoReader.ReadInt32();

                if (shaderCount == 0)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_NO_SHADERS, name));
                }

                GorgonShader? hullShader = null;
                GorgonShader? domainShader = null;

                for (int j = 0; j < shaderCount.Min(5); ++j)
                {
                    int key = psoReader.ReadInt32();

                    if (key == -1)
                    {
                        continue;
                    }

                    if (!shaders.TryGetValue(key, out GorgonShader? shader))
                    {
                        throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_CACHE_CORRUPT_CANNOT_FIND_SHADER_LINK, name, key));
                    }

                    switch (shader.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            vertexShader = shader;
                            break;
                        case ShaderType.PixelShader:
                            PsoBuilder.PixelShader(shader);
                            break;
                        case ShaderType.GeometryShader:
                            PsoBuilder.GeometryShader(shader);
                            break;
                        case ShaderType.HullShader:
                            hullShader = shader;
                            break;
                        case ShaderType.DomainShader:
                            domainShader = shader;
                            break;
                        default:
                            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_TYPE_NOT_SUPPORTED, shader.ShaderType));
                    }
                }

                if (vertexShader is null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_PSO_NO_VS, name));
                }

                if ((hullShader is not null) && (domainShader is not null))
                {
                    PsoBuilder.TessellationShaders(hullShader, domainShader);
                }

                PsoBuilder.PrimitiveType((PrimitiveType)psoReader.ReadInt32())
                          .IndexBufferStripCutIdentifier((IndexBufferStripCutIdentifier)psoReader.ReadInt32())
                          .Multisample(new GorgonMultisampleInfo(psoReader.ReadInt32(), psoReader.ReadInt32()), psoReader.ReadInt32());

                // Raster state.
                if (!rasterStates.TryGetValue(psoReader.ReadInt32(), out GorgonRasterState? rasterState))
                {
                    Graphics.Log.PrintWarning($"The rasterizer state for the PSO '{name}' is not in the PSO cache. Setting rasterizer state to default.", LoggingLevel.Intermediate);
                    rasterState = GorgonRasterState.Default;
                }

                PsoBuilder.RasterizerState(rasterState);

                // Blend state.
                PsoBuilder.AlphaToCoverageEnabled(psoReader.ReadBool())
                          .IndependentBlendingEnabled(psoReader.ReadBool());

                for (int r = 0; r < GorgonVideoAdapterInfo.MaxRenderTargetCount; ++r)
                {
                    if (!blendStates.TryGetValue(psoReader.ReadInt32(), out GorgonBlendState? blendState))
                    {
                        Graphics.Log.PrintWarning($"The blend state at index {r} for the PSO '{name}' is not in the PSO cache. Setting this blend state to default.", LoggingLevel.Intermediate);
                        blendState = GorgonBlendState.Default;
                    }

                    PsoBuilder.BlendState(blendState, r);
                }

                int outputCount = psoReader.ReadByte();

                for (int o = 0; o < outputCount; ++o)
                {
                    PsoBuilder.OutputFormat((BufferFormat)psoReader.ReadInt32(), o);
                }

                // Depth state.
                BufferFormat depthStencilFormat = (BufferFormat)psoReader.ReadInt32();

                if (!depthStencilStates.TryGetValue(psoReader.ReadInt32(), out GorgonDepthStencilState? depthStencilState))
                {
                    Graphics.Log.PrintWarning($"The depth stencil state for the PSO '{name}' is not in the PSO cache. Setting depth/stencil state to default.", LoggingLevel.Intermediate);
                    depthStencilState = GorgonDepthStencilState.Default;
                }

                PsoBuilder.DepthStencilState(depthStencilState, depthStencilFormat);

                BuildAndCachePso(name, blob, vertexShader, psoFactory);
            }
        }
        finally
        {
            psoReader.Dispose();
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

            IEnumerable<ulong> chunks = new[] { BinaryPsoFileHeader.ChunkID() };

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
        catch(Exception ex)
        {
            Graphics.Log.PrintError("There was an error retrieving the status for the pipeline state cache data.", LoggingLevel.Simple);
            Graphics.Log.PrintException(ex);
            return ReadStatus.InvalidFile;
        }
    }
}
