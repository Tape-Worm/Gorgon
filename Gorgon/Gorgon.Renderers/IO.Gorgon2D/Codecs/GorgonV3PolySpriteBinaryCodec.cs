﻿#region MIT
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

using System;
using System.IO;
using System.Linq;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

namespace Gorgon.IO
{
    /// <summary>
    /// A codec that can read and write a binary formatted version of Gorgon v3 polygonal sprite data.
    /// </summary>
    public class GorgonV3PolySpriteBinaryCodec
        : GorgonPolySpriteCodecCommon
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
        /// <param name="textureOffset">The texture transform offset.</param>
        /// <param name="textureScale">The texture transform scale.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        /// <returns>The texture attached to the sprite.</returns>
        private GorgonTexture2DView LoadTexture(GorgonBinaryReader reader, out DX.Vector2 textureOffset, out DX.Vector2 textureScale, out int textureArrayIndex)
        {
            // Write out as much info about the texture as we can so we can look it up based on these values when loading.
            string textureName = reader.ReadString();
            textureOffset = DX.Vector2.Zero;
            textureScale = DX.Vector2.One;
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

            // Locate the texture resource.
            GorgonTexture2D texture = Renderer.Graphics.LocateResourcesByName<GorgonTexture2D>(textureName)
                                              .FirstOrDefault(item => item.Width == textureWidth
                                                                      && item.Height == textureHeight
                                                                      && item.Format == textureFormat
                                                                      && item.ArrayCount == textureArrayCount
                                                                      && item.MipLevels == textureMipCount);

            if (texture == null)
            {
                textureOffset = DX.Vector2.Zero;
                textureScale = DX.Vector2.One;
                textureArrayIndex = 0;
                return null;
            }

            reader.ReadValue(out int viewArrayIndex);
            reader.ReadValue(out int viewArrayCount);
            reader.ReadValue(out int viewMipSlice);
            reader.ReadValue(out int viewMipCount);
            reader.ReadValue(out BufferFormat viewFormat);

            return texture.GetShaderResourceView(viewFormat, viewMipSlice, viewMipCount, viewArrayIndex, viewArrayCount);
        }

        /// <summary>
        /// Function to read the sprite data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the sprite.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
        protected override GorgonPolySprite OnReadFromStream(Stream stream, int byteCount)
        {
            var reader = new GorgonChunkFileReader(stream,
                                                   new[]
                                                   {
                                                       CurrentFileHeader
                                                   });
            GorgonBinaryReader binReader = null;
            var sprite = new GorgonPolySpriteBuilder(Renderer);

            try
            {
                reader.Open();
                binReader = reader.OpenChunk(SpriteData);
                sprite.Anchor(binReader.ReadValue<DX.Vector2>());
                
                // If we do not have alpha test information, then skip writing its data.
                if (binReader.ReadBoolean())
                {
                    sprite.AlphaTest(binReader.ReadValue<GorgonRangeF>());
                }
                
                reader.CloseChunk();

                binReader = reader.OpenChunk(VertexData);

                int vertexCount = binReader.ReadInt32();

                for (int i = 0; i < vertexCount; ++i)
                {
                    sprite.AddVertex(new GorgonPolySpriteVertex(binReader.ReadValue<DX.Vector2>(),
                                                                binReader.ReadValue<GorgonColor>(),
                                                                binReader.ReadValue<DX.Vector2>()));
                }

                reader.CloseChunk();

                if (reader.Chunks.Contains(TextureData))
                {
                    binReader = reader.OpenChunk(TextureData);
                    sprite.Texture(LoadTexture(binReader, out DX.Vector2 textureOffset, out DX.Vector2 textureScale, out int textureArrayIndex));
                    sprite.TextureArrayIndex(textureArrayIndex);
                    sprite.TextureTransform(textureOffset, textureScale);
                    reader.CloseChunk();
                }
                
                if (!reader.Chunks.Contains(TextureSamplerData))
                {
                    return sprite.Build();
                }

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

                sprite.TextureSampler(builder.Wrapping(wrapU, wrapV, wrapW, borderColor)
                                               .Filter(filter)
                                               .ComparisonFunction(compareFunc)
                                               .MaxAnisotropy(maxAnisotropy)
                                               .MipLevelOfDetail(minLod, maxLod, mipLodBias)
                                               .Build());

                return sprite.Build();
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
                binWriter.Write(sprite.AlphaTest != null);
                if (sprite.AlphaTest != null)
                {
                    binWriter.WriteValue(sprite.AlphaTest.Value);
                }
                
                writer.CloseChunk();

                binWriter = writer.OpenChunk(VertexData);

                binWriter.Write(sprite.Vertices.Count);

                foreach(GorgonPolySpriteVertex vertex in sprite.Vertices)
                {
                    binWriter.WriteValue(vertex.Position);
                    binWriter.WriteValue(vertex.Color);
                    binWriter.WriteValue(vertex.TextureCoordinate);
                }

                writer.CloseChunk();

                // We have no texture data, so don't bother writing out that chunk.
                if (sprite.Texture != null)
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

                if (sprite.TextureSampler == null)
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

            using (GorgonBinaryReader binReader = reader.OpenChunk(VersionData))
            {
                var fileVersion = new Version(binReader.ReadByte(), binReader.ReadByte());
                reader.CloseChunk();

                return Version.Equals(fileVersion);
            }
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
                reader = new GorgonChunkFileReader(stream, new [] { CurrentFileHeader });
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
                reader = new GorgonChunkFileReader(stream, new [] { CurrentFileHeader });
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

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV3PolySpriteBinaryCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public GorgonV3PolySpriteBinaryCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_POLYSPRITE_BIN_CODEC, Resources.GOR2DIO_V3_POLYSPRITE_BIN_CODEC_DESCRIPTION)
        {
            
        }
        #endregion
    }
}
