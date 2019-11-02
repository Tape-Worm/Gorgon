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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class GorgonV31AnimationBinaryCodec
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
        /// The single floating point value track data chunk ID.
        /// </summary>
        public static readonly ulong SingleData = "SNGLDATA".ChunkID();
        /// <summary>
        /// The 2D vector tracks data chunk ID.
        /// </summary>
        public static readonly ulong Vector2Data = "VEC2DATA".ChunkID();
        /// <summary>
        /// The 3D vector tracks data chunk ID.
        /// </summary>
        public static readonly ulong Vector3Data = "VEC3DATA".ChunkID();
        /// <summary>
        /// The 4D vector tracks data chunk ID.
        /// </summary>
        public static readonly ulong Vector4Data = "VEC4DATA".ChunkID();
        /// <summary>
        /// The color track data chunk ID.
        /// </summary>
        public static readonly ulong ColorData = "COLRDATA".ChunkID();
        /// <summary>
        /// The rectangle track data chunk ID.
        /// </summary>
        public static readonly ulong RectData = "RECTDATA".ChunkID();
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
        public override bool CanEncode => true;

        /// <summary>
        /// Property to return the version of animation data that the codec supports.
        /// </summary>
        public override Version Version => CurrentVersion;
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
            GorgonTexture2D texture = Renderer.Graphics.LocateResourcesByName<GorgonTexture2D>(textureName)
                                              .FirstOrDefault(item => item.Width == textureWidth
                                                                      && item.Height == textureHeight
                                                                      && item.Format == textureFormat
                                                                      && item.ArrayCount == textureArrayCount
                                                                      && item.MipLevels == textureMipCount);

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

        /// <summary>
        /// Function to write out the texture tracks.
        /// </summary>
        /// <param name="writer">The chunk file writer.</param>
        /// <param name="chunkID">The ID of the chunk for the track.</param>
        /// <param name="tracks">The list of tracks to write.</param>
        private void WriteTextureTrackValues(GorgonChunkFileWriter writer, ulong chunkID, IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> tracks)
        {
            if (tracks.Count == 0)
            {
                return;
            }

            GorgonBinaryWriter binWriter = writer.OpenChunk(chunkID);
            binWriter.Write(tracks.Count(item => item.Value.KeyFrames.Count > 0));

            foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> track in tracks.Where(item => item.Value.KeyFrames.Count > 0))
            {
                binWriter.Write(track.Key);
                binWriter.Write(track.Value.KeyFrames.Count);

                for (int i = 0; i < track.Value.KeyFrames.Count; ++i)
                {
                    GorgonKeyTexture2D key = track.Value.KeyFrames[i];

                    binWriter.Write(key.Time);

                    if (key.Value == null)
                    {
                        binWriter.WriteValue<byte>(0);
                    }
                    else
                    {
                        binWriter.WriteValue<byte>(1);
                        binWriter.Write(key.Value.Texture.Name);
                        binWriter.Write(key.Value.Texture.Width);
                        binWriter.Write(key.Value.Texture.Height);
                        binWriter.WriteValue(key.Value.Texture.Format);
                        binWriter.Write(key.Value.Texture.ArrayCount);
                        binWriter.Write(key.Value.Texture.MipLevels);
                        binWriter.Write(key.Value.ArrayIndex);
                        binWriter.Write(key.Value.ArrayCount);
                        binWriter.Write(key.Value.MipSlice);
                        binWriter.Write(key.Value.MipCount);
                        binWriter.WriteValue(key.Value.Format);
                    }

                    binWriter.WriteValue(ref key.TextureCoordinates);
                    binWriter.Write(key.TextureArrayIndex);
                }
            }
            writer.CloseChunk();
        }

        /// <summary>
        /// Function to write out the value tracks.
        /// </summary>
        /// <typeparam name="Tk">The type of key data.</typeparam>
        /// <typeparam name="Tkd">The type of data stored in the key.</typeparam>
        /// <param name="writer">The chunk file writer.</param>
        /// <param name="chunkID">The ID of the chunk for the track.</param>
        /// <param name="tracks">The list of tracks to write.</param>
        /// <param name="getValue">Callback used to extract the value from the concrete instance of the key type.</param>
        private void WriteTrackValues<Tk, Tkd>(GorgonChunkFileWriter writer, ulong chunkID, IReadOnlyDictionary<string, IGorgonAnimationTrack<Tk>> tracks, Func<Tk, Tkd> getValue)
            where Tk : IGorgonKeyFrame
            where Tkd : unmanaged
        {
            if (tracks.Count == 0)
            {
                return;
            }

            GorgonBinaryWriter binWriter = writer.OpenChunk(chunkID);
            binWriter.Write(tracks.Count(item => item.Value.KeyFrames.Count > 0));

            foreach (KeyValuePair<string, IGorgonAnimationTrack<Tk>> track in tracks.Where(item => item.Value.KeyFrames.Count > 0))
            {
                binWriter.Write(track.Key);
                binWriter.WriteValue(track.Value.InterpolationMode);
                binWriter.Write(track.Value.KeyFrames.Count);

                for (int i = 0; i < track.Value.KeyFrames.Count; ++i)
                {
                    Tk key = track.Value.KeyFrames[i];
                    Tkd value = getValue(key);
                    binWriter.Write(key.Time);
                    binWriter.WriteValue(ref value);
                }                
            }
            writer.CloseChunk();
        }

        /// <summary>
        /// Function to read in the texture tracks.
        /// </summary>
        /// <param name="reader">The chunk file reader.</param>
        /// <param name="chunkID">The ID of the chunk for the track.</param>
        /// <param name="builder">The builder used to generate the animation.</param>
        private void ReadTextureTrackValues(GorgonChunkFileReader reader, ulong chunkID, GorgonAnimationBuilder builder)
        {
            if (!reader.Chunks.Contains(chunkID))
            {
                return;
            }

            GorgonBinaryReader binReader = reader.OpenChunk(chunkID);
            int trackCount = binReader.ReadInt32();            

            for (int i = 0; i < trackCount; ++i)
            {
                string trackName = binReader.ReadString();

                IGorgonTrackKeyBuilder<GorgonKeyTexture2D> trackBuilder = builder.Edit2DTexture(trackName);

                int keyCount = binReader.ReadInt32();                

                for (int j = 0; j < keyCount; ++j)
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
                        trackBuilder.SetKey(new GorgonKeyTexture2D(time, textureName, binReader.ReadValue<DX.RectangleF>(), binReader.ReadInt32()));
                    }
                    else
                    {
                        trackBuilder.SetKey(new GorgonKeyTexture2D(time, texture, binReader.ReadValue<DX.RectangleF>(), binReader.ReadInt32()));
                    }
                }

                trackBuilder.EndEdit();
            }
            reader.CloseChunk();
        }

        /// <summary>
        /// Function to read the value tracks.
        /// </summary>
        /// <param name="reader">The chunk file reader.</param>
        /// <param name="chunkID">The ID of the chunk for the track.</param>                
        /// <param name="getBuilder">Callback to retrieve the appropriate builder for the track.</param>
        /// <param name="createKey">Callback used to create a key.</param>
        private void ReadTrackValues<Tk, Tkd>(GorgonChunkFileReader reader, ulong chunkID, Func<string, IGorgonTrackKeyBuilder<Tk>> getBuilder, Func<float, Tkd, Tk> createKey)
            where Tk : class, IGorgonKeyFrame
            where Tkd : unmanaged
        {
            if (!reader.Chunks.Contains(chunkID))
            {
                return;
            }

            GorgonBinaryReader binReader = reader.OpenChunk(chunkID);
            int trackCount = binReader.ReadInt32();

            if (trackCount == 0)
            {
                return;
            }
            
            for (int i = 0; i < trackCount; ++i)
            {                
                string trackName = binReader.ReadString();

                IGorgonTrackKeyBuilder<Tk> builder = getBuilder(trackName);

                builder.SetInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                int keyCount = binReader.ReadInt32();

                for (int j = 0; j < keyCount; ++j)
                {
                    builder.SetKey(createKey(binReader.ReadSingle(), binReader.ReadValue<Tkd>()));
                }

                builder.EndEdit();
            }
            reader.CloseChunk();
        }

        /// <summary>
        /// Function to save the animation data to a stream.
        /// </summary>
        /// <param name="animation">The animation to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the animation.</param>
        protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream)
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

                binWriter = writer.OpenChunk(AnimationData);
                binWriter.Write(animation.Name);
                binWriter.Write(animation.Length);
                binWriter.Write(animation.IsLooped);
                binWriter.Write(animation.LoopCount);
                writer.CloseChunk();

                // Write tracks with value type data.
                WriteTrackValues(writer, SingleData, animation.SingleTracks, k => k.Value);
                WriteTrackValues(writer, Vector2Data, animation.Vector2Tracks, k => k.Value);
                WriteTrackValues(writer, Vector3Data, animation.Vector3Tracks, k => k.Value);
                WriteTrackValues(writer, Vector4Data, animation.Vector4Tracks, k => k.Value);
                WriteTrackValues(writer, RectData, animation.RectangleTracks, k => k.Value);
                WriteTrackValues(writer, ColorData, animation.ColorTracks, k => k.Value);

                // Write out texture data.
                WriteTextureTrackValues(writer, TextureData, animation.Texture2DTracks);
            }
            finally
            {
                binWriter?.Dispose();
                writer.Close();
            }
        }

        /// <summary>
        /// Function to read the animation data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the animation.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        protected override IGorgonAnimation OnReadFromStream(Stream stream, int byteCount)
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
                string name = binReader.ReadString();
                float length = binReader.ReadSingle();
                bool isLooped = binReader.ReadBoolean();
                int loopCount = binReader.ReadInt32();
                reader.CloseChunk();

                ReadTrackValues<GorgonKeySingle, float>(reader, SingleData, builder.EditSingle, (t, v) => new GorgonKeySingle(t, v));
                ReadTrackValues<GorgonKeyVector2, DX.Vector2>(reader, Vector2Data, builder.EditVector2, (t, v) => new GorgonKeyVector2(t, v));
                ReadTrackValues<GorgonKeyVector3, DX.Vector3>(reader, Vector3Data, builder.EditVector3, (t, v) => new GorgonKeyVector3(t, v));
                ReadTrackValues<GorgonKeyVector4, DX.Vector4>(reader, Vector4Data, builder.EditVector4, (t, v) => new GorgonKeyVector4(t, v));
                ReadTrackValues<GorgonKeyRectangle, DX.RectangleF>(reader, RectData, builder.EditRectangle, (t, v) => new GorgonKeyRectangle(t, v));
                ReadTrackValues<GorgonKeyGorgonColor, GorgonColor>(reader, ColorData, builder.EditColor, (t, v) => new GorgonKeyGorgonColor(t, v));
                ReadTextureTrackValues(reader, TextureData, builder);
                
                IGorgonAnimation result;
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
                reader = new GorgonChunkFileReader(stream, new[] { CurrentFileHeader });
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
        /// Initializes a new instance of the <see cref="GorgonV31AnimationBinaryCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
        public GorgonV31AnimationBinaryCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_1_ANIM_BIN_CODEC, Resources.GOR2DIO_V3_1_ANIM_BIN_CODEC_DESCRIPTION)
        {
        }
        #endregion
    }
}
