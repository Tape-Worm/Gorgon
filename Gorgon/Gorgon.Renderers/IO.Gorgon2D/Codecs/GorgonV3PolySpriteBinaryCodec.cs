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
// Created: August 11, 2018 3:43:13 PM
// 
#endregion

using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// A codec that can read and write a binary formatted version of Gorgon v3 polygonal sprite data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV3PolySpriteBinaryCodec"/> class.
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
public class GorgonV3PolySpriteBinaryCodec(Gorgon2D renderer)
        : GorgonPolySpriteCodecCommon(renderer, Resources.GOR2DIO_V3_POLYSPRITE_BIN_CODEC, Resources.GOR2DIO_V3_POLYSPRITE_BIN_CODEC_DESCRIPTION)
{
    #region Properties.
    /// <summary>
    /// The sprite data chunk ID.
    /// </summary>
    public static readonly ulong SpriteData = "SPRTDATA".ChunkID();
    /// <summary>
    /// The sprite data chunk ID.
    /// </summary>
    public static readonly ulong VertexData = "HULLDATA".ChunkID();
    /// <summary>
    /// The sprite data chunk ID.
    /// </summary>
    public static readonly ulong IndexData = "INDXDATA".ChunkID();
    /// <summary>
    /// The texture data chunk ID.
    /// </summary>
    public static readonly ulong TextureData = "TXTRDATA".ChunkID();
    /// <summary>
    /// The texture sampler data chunk ID.
    /// </summary>
    public static readonly ulong TextureSamplerData = "TXTRSMPL".ChunkID();
    /// <summary>
    /// The version data chunk ID.
    /// </summary>
    public static readonly ulong VersionData = "VRSNDATA".ChunkID();

    /// <summary>
    /// Property to return whether or not the codec can decode sprite data.
    /// </summary>
    public override bool CanDecode => true;

    /// <summary>
    /// Property to return whether or not the codec can encode sprite data.
    /// </summary>
    public override bool CanEncode => true;

    /// <summary>
    /// Property to return the version of sprite data that the codec supports.
    /// </summary>
    public override Version Version => CurrentVersion;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to load the texture information.
    /// </summary>
    /// <param name="reader">The reader containing the texture information.</param>
    /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <param name="textureOffset">The texture transform offset.</param>
    /// <param name="textureScale">The texture transform scale.</param>
    /// <param name="textureArrayIndex">The texture array index.</param>
    /// <returns>The texture attached to the sprite.</returns>
    private GorgonTexture2DView LoadTexture(GorgonBinaryReader reader, GorgonTexture2DView overrideTexture, out Vector2 textureOffset, out Vector2 textureScale, out int textureArrayIndex)
    {
        // Write out as much info about the texture as we can so we can look it up based on these values when loading.
        string textureName = reader.ReadString();
        textureOffset = Vector2.Zero;
        textureScale = Vector2.One;
        textureArrayIndex = 0;

        if (string.IsNullOrWhiteSpace(textureName))
        {
            return null;
        }

        reader.ReadValue(out textureOffset);
        reader.ReadValue(out textureScale);
        reader.ReadValue(out textureArrayIndex);
        reader.ReadValue(out int textureWidth);
        reader.ReadValue(out int textureHeight);
        reader.ReadValue(out BufferFormat textureFormat);
        reader.ReadValue(out int textureArrayCount);
        reader.ReadValue(out int textureMipCount);

        GorgonTexture2D texture = null;

        // Locate the texture resource.
        if (overrideTexture is null)
        {
            texture = Renderer.Graphics.Locate2DTextureByName(textureName, textureWidth, textureHeight, textureFormat, textureArrayCount, textureMipCount);

            if (texture is null)
            {
                textureOffset = Vector2.Zero;
                textureScale = Vector2.One;
                textureArrayIndex = 0;
                return null;
            }
        }

        reader.ReadValue(out int viewArrayIndex);
        reader.ReadValue(out int viewArrayCount);
        reader.ReadValue(out int viewMipSlice);
        reader.ReadValue(out int viewMipCount);
        reader.ReadValue(out BufferFormat viewFormat);

        return overrideTexture ?? texture.GetShaderResourceView(viewFormat, viewMipSlice, viewMipCount, viewArrayIndex, viewArrayCount);
    }

    /// <summary>
    /// Function to read the sprite data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the sprite.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
    protected override GorgonPolySprite OnReadFromStream(Stream stream, int byteCount, GorgonTexture2DView overrideTexture)
    {
        var reader = new GorgonChunkFileReader(stream,
                                               [
                                                   CurrentFileHeader
                                               ]);
        GorgonBinaryReader binReader = null;
        GorgonPolySpriteVertex[] vertices;
        int[] indices = null;

        try
        {
            reader.Open();
            binReader = reader.OpenChunk(SpriteData);
            Vector2 anchor = binReader.ReadValue<Vector2>();

            // If we do not have alpha test information, then skip writing its data.
            GorgonRangeF? alphaRange = null;
            if (binReader.ReadBoolean())
            {
                alphaRange = binReader.ReadValue<GorgonRangeF>();
            }

            reader.CloseChunk();

            binReader = reader.OpenChunk(VertexData);

            vertices = new GorgonPolySpriteVertex[binReader.ReadInt32()];

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = new GorgonPolySpriteVertex(binReader.ReadValue<Vector2>(),
                                                            binReader.ReadValue<GorgonColor>(),
                                                            binReader.ReadValue<Vector2>());
            }

            reader.CloseChunk();

            if (reader.Chunks.Contains(IndexData))
            {
                binReader = reader.OpenChunk(IndexData);
                indices = new int[binReader.ReadInt32()];

                for (int i = 0; i < indices.Length; ++i)
                {
                    indices[i] = binReader.ReadInt32();
                }

                reader.CloseChunk();
            }

            GorgonTexture2DView texture = null;
            int textureArrayIndex = 0;
            Vector2 textureOffset = Vector2.Zero;
            Vector2 textureScale = Vector2.One;

            if (reader.Chunks.Contains(TextureData))
            {
                binReader = reader.OpenChunk(TextureData);
                texture = LoadTexture(binReader, overrideTexture, out textureOffset, out textureScale, out textureArrayIndex);
                reader.CloseChunk();
            }

            GorgonSamplerState samplerState = null;
            if (reader.Chunks.Contains(TextureSamplerData))
            {
                var builder = new GorgonSamplerStateBuilder(Renderer.Graphics);
                binReader = reader.OpenChunk(TextureSamplerData);
                binReader.ReadValue(out SampleFilter filter);
                binReader.ReadValue(out GorgonColor borderColor);
                binReader.ReadValue(out Comparison compareFunc);
                binReader.ReadValue(out TextureWrap wrapU);
                binReader.ReadValue(out TextureWrap wrapV);
                binReader.ReadValue(out TextureWrap wrapW);
                binReader.ReadValue(out int maxAnisotropy);
                binReader.ReadValue(out float minLod);
                binReader.ReadValue(out float maxLod);
                binReader.ReadValue(out float mipLodBias);
                reader.CloseChunk();

                samplerState = builder.Wrapping(wrapU, wrapV, wrapW, borderColor)
                                               .Filter(filter)
                                               .ComparisonFunction(compareFunc)
                                               .MaxAnisotropy(maxAnisotropy)
                                               .MipLevelOfDetail(minLod, maxLod, mipLodBias)
                                               .Build();
            }

            if ((indices is null) || (indices.Length == 0))
            {
                var builder = new GorgonPolySpriteBuilder(Renderer);
                return builder.AddVertices(vertices)
                              .Anchor(anchor)
                              .AlphaTest(alphaRange)
                              .Texture(texture)
                              .TextureArrayIndex(textureArrayIndex)
                              .TextureTransform(textureOffset, textureScale)
                              .TextureSampler(samplerState)
                              .Build();
            }

            var sprite = GorgonPolySprite.Create(Renderer, vertices, indices);
            sprite.Anchor = anchor;
            sprite.AlphaTest = alphaRange;
            sprite.Texture = texture;
            sprite.TextureArrayIndex = textureArrayIndex;
            sprite.TextureOffset = textureOffset;
            sprite.TextureScale = textureScale;
            sprite.TextureSampler = samplerState;

            return sprite;
        }
        finally
        {
            binReader?.Dispose();
            reader.Close();
        }
    }

    /// <summary>
    /// Function to save the sprite data to a stream.
    /// </summary>
    /// <param name="sprite">The sprite to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the sprite.</param>
    protected override void OnSaveToStream(GorgonPolySprite sprite, Stream stream)
    {
        var writer = new GorgonChunkFileWriter(stream, CurrentFileHeader);
        GorgonBinaryWriter binWriter = null;

        try
        {
            writer.Open();
            binWriter = writer.OpenChunk(VersionData);
            binWriter.Write((byte)Version.Major);
            binWriter.Write((byte)Version.Minor);
            writer.CloseChunk();

            binWriter = writer.OpenChunk(SpriteData);

            binWriter.WriteValue(sprite.Anchor);

            // If we do not have alpha test information, then skip writing its data.
            binWriter.Write(sprite.AlphaTest is not null);
            if (sprite.AlphaTest is not null)
            {
                binWriter.WriteValue(sprite.AlphaTest.Value);
            }

            writer.CloseChunk();

            binWriter = writer.OpenChunk(VertexData);

            binWriter.Write(sprite.Vertices.Count);

            foreach (GorgonPolySpriteVertex vertex in sprite.Vertices)
            {
                binWriter.WriteValue(vertex.Position);
                binWriter.WriteValue(vertex.Color);
                binWriter.WriteValue(vertex.TextureCoordinate);
            }

            writer.CloseChunk();

            if (sprite.Indices.Count > 0)
            {
                binWriter = writer.OpenChunk(IndexData);

                binWriter.Write(sprite.Indices.Count);

                for (int i = 0; i < sprite.Indices.Count; ++i)
                {
                    binWriter.Write(sprite.Indices[i]);
                }

                writer.CloseChunk();
            }

            // We have no texture data, so don't bother writing out that chunk.
            if (sprite.Texture is not null)
            {
                binWriter = writer.OpenChunk(TextureData);
                binWriter.Write(sprite.Texture.Texture.Name);
                binWriter.WriteValue(sprite.TextureOffset);
                binWriter.WriteValue(sprite.TextureScale);
                binWriter.WriteValue(sprite.TextureArrayIndex);
                // Write out as much info about the texture as we can so we can look it up based on these values when loading.
                binWriter.Write(sprite.Texture.Texture.Width);
                binWriter.Write(sprite.Texture.Texture.Height);
                binWriter.WriteValue(sprite.Texture.Texture.Format);
                binWriter.Write(sprite.Texture.Texture.ArrayCount);
                binWriter.Write(sprite.Texture.Texture.MipLevels);
                binWriter.Write(sprite.Texture.ArrayIndex);
                binWriter.Write(sprite.Texture.ArrayCount);
                binWriter.Write(sprite.Texture.MipSlice);
                binWriter.Write(sprite.Texture.MipCount);
                binWriter.WriteValue(sprite.Texture.Format);
                binWriter.Close();
            }

            if (sprite.TextureSampler is null)
            {
                return;
            }

            // Write out information about the texture sampler used by the sprite.
            binWriter = writer.OpenChunk(TextureSamplerData);
            binWriter.WriteValue(sprite.TextureSampler.Filter);
            binWriter.WriteValue(sprite.TextureSampler.BorderColor);
            binWriter.WriteValue(sprite.TextureSampler.ComparisonFunction);
            binWriter.WriteValue(sprite.TextureSampler.WrapU);
            binWriter.WriteValue(sprite.TextureSampler.WrapV);
            binWriter.WriteValue(sprite.TextureSampler.WrapW);
            binWriter.Write(sprite.TextureSampler.MaxAnisotropy);
            binWriter.Write(sprite.TextureSampler.MinimumLevelOfDetail);
            binWriter.Write(sprite.TextureSampler.MaximumLevelOfDetail);
            binWriter.Write(sprite.TextureSampler.MipLevelOfDetailBias);
            writer.CloseChunk();
        }
        finally
        {
            binWriter?.Dispose();
            writer.Close();
        }
    }

    /// <summary>
    /// Function to determine if the chunk file is a readable file.
    /// </summary>
    /// <param name="reader">The chunk file reader to use.</param>
    /// <returns><b>true</b> if the chunk file is readable, or <b>false</b> if not.</returns>
    private bool IsReadableChunkFile(GorgonChunkFileReader reader)
    {
        if ((!reader.Chunks.Contains(VersionData))
            || (!reader.Chunks.Contains(SpriteData))
            || (!reader.Chunks.Contains(VertexData)))
        {
            return false;
        }

        using GorgonBinaryReader binReader = reader.OpenChunk(VersionData);
        var fileVersion = new Version(binReader.ReadByte(), binReader.ReadByte());
        reader.CloseChunk();

        return Version.Equals(fileVersion);
    }

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    protected override bool OnIsReadable(Stream stream)
    {
        GorgonChunkFileReader reader = null;

        try
        {
            reader = new GorgonChunkFileReader(stream, [CurrentFileHeader]);
            reader.Open();
            return IsReadableChunkFile(reader);
        }
        catch
        {
            return false;
        }
        finally
        {
            reader?.Close();
        }
    }

    /// <summary>
    /// Function to retrieve the name of the associated texture.
    /// </summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
    protected override string OnGetAssociatedTextureName(Stream stream)
    {
        GorgonChunkFileReader reader = null;
        GorgonBinaryReader binReader = null;

        try
        {
            reader = new GorgonChunkFileReader(stream, [CurrentFileHeader]);
            reader.Open();
            if (!IsReadableChunkFile(reader))
            {
                return null;
            }

            // No texture data in this file.
            if (!reader.Chunks.Contains(TextureData))
            {
                return null;
            }

            binReader = reader.OpenChunk(TextureData);
            string result = binReader.ReadString();
            reader.CloseChunk();

            return result;
        }
        finally
        {
            binReader?.Dispose();
            reader?.Close();
        }
    }

    #endregion
}
