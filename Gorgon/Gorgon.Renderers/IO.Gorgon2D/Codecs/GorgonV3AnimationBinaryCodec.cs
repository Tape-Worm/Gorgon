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
// Created: August 25, 2018 2:43:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Animation;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.IO
{
    /// <summary>
    /// A codec used to read/write animations as binary formatted data.
    /// </summary>
    public class GorgonV3AnimationBinaryCodec
        : GorgonAnimationCodecCommon
    {
        #region Properties.
        /// <summary>
        /// The version data chunk ID.
        /// </summary>
        public static readonly ulong VersionData = "VRSNDATA".ChunkID();
        /// <summary>
        /// The animation track data chunk ID.
        /// </summary>
        public static readonly ulong AnimationData = "ANIMDATA".ChunkID();
        /// <summary>
        /// The position track data chunk ID.
        /// </summary>
        public static readonly ulong PositionData = "POSNDATA".ChunkID();
        /// <summary>
        /// The scale track data chunk ID.
        /// </summary>
        public static readonly ulong ScaleData = "SCLEDATA".ChunkID();
        /// <summary>
        /// The rotation track data chunk ID.
        /// </summary>
        public static readonly ulong RotationData = "ROTNDATA".ChunkID();
        /// <summary>
        /// The color track data chunk ID.
        /// </summary>
        public static readonly ulong ColorData = "COLRDATA".ChunkID();
        /// <summary>
        /// The bounds track data chunk ID.
        /// </summary>
        public static readonly ulong BoundsData = "BNDSDATA".ChunkID();
        /// <summary>
        /// The bounds track data chunk ID.
        /// </summary>
        public static readonly ulong SizeData = "SIZEDATA".ChunkID();
        /// <summary>
        /// The texture track data chunk ID.
        /// </summary>
        public static readonly ulong TextureData = "TXTRDATA".ChunkID();

        /// <summary>
        /// Property to return whether or not the codec can decode animation data.
        /// </summary>
        public override bool CanDecode => true;

        /// <summary>
        /// Property to return whether or not the codec can encode animation data.
        /// </summary>
        public override bool CanEncode => false;

        /// <summary>
        /// Property to return the version of animation data that the codec supports.
        /// </summary>
        public override Version Version => GorgonV3AnimationJsonCodec.Version30;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load the texture information.
        /// </summary>
        /// <param name="reader">The reader containing the texture information.</param>
        /// <param name="textureName">The name of the texture.</param>
        /// <returns>The texture attached to the sprite.</returns>
        private GorgonTexture2DView LoadTexture(GorgonBinaryReader reader, out string textureName)
        {
            // Write out as much info about the texture as we can so we can look it up based on these values when loading.
            textureName = reader.ReadString();

            if (string.IsNullOrWhiteSpace(textureName))
            {
                return null;
            }

            reader.ReadValue(out int textureWidth);
            reader.ReadValue(out int textureHeight);
            reader.ReadValue(out BufferFormat textureFormat);
            reader.ReadValue(out int textureArrayCount);
            reader.ReadValue(out int textureMipCount);

            // Locate the texture resource.
            GorgonTexture2D texture = Renderer.Graphics.Locate2DTextureByName(textureName, textureWidth, textureHeight, textureFormat, textureArrayCount, textureMipCount);

            if (texture == null)
            {
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
        /// Function to determine if the chunk file is a readable file.
        /// </summary>
        /// <param name="reader">The chunk file reader to use.</param>
        /// <returns><b>true</b> if the chunk file is readable, or <b>false</b> if not.</returns>
        private bool IsReadableChunkFile(GorgonChunkFileReader reader)
        {
            if ((!reader.Chunks.Contains(VersionData))
                || (!reader.Chunks.Contains(AnimationData)))
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

        /// <summary>Function to retrieve the names of the associated textures.</summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
        protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream)
        {
            GorgonChunkFileReader reader = null;
            GorgonBinaryReader binReader = null;

            try
            {
                reader = new GorgonChunkFileReader(stream, new[] { CurrentFileHeader });
                reader.Open();
                if (!IsReadableChunkFile(reader))
                {
                    return Array.Empty<string>();
                }

                // No texture data in this file.
                if (!reader.Chunks.Contains(TextureData))
                {
                    return Array.Empty<string>();
                }

                binReader = reader.OpenChunk(TextureData);
                int keyCount = binReader.ReadInt32();
                var result = new List<string>();
                
                for (int i = 0; i < keyCount; ++i)
                {
                    binReader.ReadSingle();
                    byte hasTexture = binReader.ReadByte();

                    if (hasTexture == 0)
                    {
                        continue;
                    }

                    string textureName = binReader.ReadString();

                    if ((string.IsNullOrWhiteSpace(textureName))
                        || (result.Contains(textureName)))
                    {
                        continue;
                    }

                    result.Add(textureName);
                }

                reader.CloseChunk();

                return result;
            }
            finally
            {
                binReader?.Dispose();
                reader?.Close();
            }
        }

        /// <summary>
        /// Function to save the animation data to a stream.
        /// </summary>
        /// <param name="animation">The animation to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the animation.</param>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream) => throw new NotSupportedException();

        /// <summary>
        /// Function to read the animation data from a stream.
        /// </summary>
        /// <param name="name">Not used.</param>
        /// <param name="stream">The stream containing the animation.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        protected override IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount)
        {
            var builder = new GorgonAnimationBuilder();

            var reader = new GorgonChunkFileReader(stream,
                                                   new[]
                                                   {
                                                       CurrentFileHeader
                                                   });
            GorgonBinaryReader binReader = null;

            try
            {
                reader.Open();
                binReader = reader.OpenChunk(AnimationData);
                name = binReader.ReadString();
                float length = binReader.ReadSingle();
                bool isLooped = binReader.ReadBoolean();
                int loopCount = binReader.ReadInt32();
                reader.CloseChunk();

                int keyCount;

                if (reader.Chunks.Contains(PositionData))
                {
                    binReader = reader.OpenChunk(PositionData);
                    TrackInterpolationMode interpolation =  binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector2> track = builder.EditVector2("Position")
                                                                            .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        DX.Vector3 val = binReader.ReadValue<DX.Vector3>();
                        track.SetKey(new GorgonKeyVector2(binReader.ReadSingle(), new DX.Vector2(val.X, val.Y)));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(ScaleData))
                {
                    binReader = reader.OpenChunk(ScaleData);
                    TrackInterpolationMode interpolation = binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector2> track = builder.EditVector2("Scale")
                                                                            .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        DX.Vector3 val = binReader.ReadValue<DX.Vector3>();
                        track.SetKey(new GorgonKeyVector2(binReader.ReadSingle(), new DX.Vector2(val.X, val.Y)));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(RotationData))
                {
                    binReader = reader.OpenChunk(RotationData);
                    TrackInterpolationMode interpolation = binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeySingle> track = builder.EditSingle("Angle")
                                                                           .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeySingle(binReader.ReadSingle(), binReader.ReadValue<DX.Vector3>().Z));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(SizeData))
                {
                    binReader = reader.OpenChunk(SizeData);
                    TrackInterpolationMode interpolation = binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector2> track = builder.EditVector2("Size")
                                                                            .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        DX.Vector3 val = binReader.ReadValue<DX.Vector3>();
                        track.SetKey(new GorgonKeyVector2(binReader.ReadSingle(), new DX.Vector2(val.X, val.Y)));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(BoundsData))
                {
                    binReader = reader.OpenChunk(BoundsData);
                    TrackInterpolationMode interpolation = binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyRectangle> track = builder.EditRectangle("Bounds")
                                                                              .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyRectangle(binReader.ReadSingle(), binReader.ReadValue<DX.RectangleF>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(ColorData))
                {
                    binReader = reader.OpenChunk(ColorData);
                    TrackInterpolationMode interpolation = binReader.ReadValue<TrackInterpolationMode>();
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyGorgonColor> track = builder.EditColor("Color")
                                                                                .SetInterpolationMode(interpolation);
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyGorgonColor(binReader.ReadSingle(), binReader.ReadValue<GorgonColor>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                IGorgonAnimation result;
                if (!reader.Chunks.Contains(TextureData))
                {
                    result = builder.Build(name, length);
                    result.IsLooped = isLooped;
                    result.LoopCount = loopCount;
                    return result;
                }

                binReader = reader.OpenChunk(TextureData);
                keyCount = binReader.ReadInt32();

                IGorgonTrackKeyBuilder<GorgonKeyTexture2D> textureTrack = builder.Edit2DTexture("Texture");
                for (int i = 0; i < keyCount; ++i)
                {
                    float time = binReader.ReadSingle();
                    byte hasTexture = binReader.ReadByte();
                    GorgonTexture2DView texture = null;
                    string textureName = string.Empty;

                    if (hasTexture != 0)
                    {
                        texture = LoadTexture(binReader, out textureName);

                        if ((texture == null) && (string.IsNullOrWhiteSpace(textureName)))
                        {
                            Renderer.Log.Print("Attempted to load a texture from the data, but the texture was not in memory and the name is unknown.",
                                                 LoggingLevel.Verbose);
                            continue;
                        }
                    }

                    if ((texture == null) && (hasTexture != 0))
                    {
                        textureTrack.SetKey(new GorgonKeyTexture2D(time, textureName, binReader.ReadValue<DX.RectangleF>(), binReader.ReadInt32()));
                    }
                    else
                    {
                        textureTrack.SetKey(new GorgonKeyTexture2D(time, texture, binReader.ReadValue<DX.RectangleF>(), binReader.ReadInt32()));
                    }
                }
                textureTrack.EndEdit();
                reader.CloseChunk();

                result = builder.Build(name, length);
                result.IsLooped = isLooped;
                result.LoopCount = loopCount;
                return result;
            }
            finally
            {
                binReader?.Dispose();
                reader.Close();
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
                reader = new GorgonChunkFileReader(stream, new[] { GorgonV3AnimationJsonCodec.FileHeader30 });
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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV3AnimationBinaryCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
        public GorgonV3AnimationBinaryCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_ANIM_BIN_CODEC, Resources.GOR2DIO_V3_ANIM_BIN_CODEC_DESCRIPTION)
        {
        }
        #endregion
    }
}
