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

                // Write out position track.
                if (animation.PositionTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(PositionData);
                    binWriter.WriteValue(animation.PositionTrack.InterpolationMode);
                    binWriter.Write(animation.PositionTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.PositionTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyVector3 key = animation.PositionTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                // Write out position track.
                if (animation.ScaleTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(ScaleData);
                    binWriter.WriteValue(animation.ScaleTrack.InterpolationMode);
                    binWriter.Write(animation.ScaleTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.ScaleTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyVector3 key = animation.ScaleTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                // Write out rotation track.
                if (animation.RotationTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(RotationData);
                    binWriter.WriteValue(animation.RotationTrack.InterpolationMode);
                    binWriter.Write(animation.RotationTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.RotationTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyVector3 key = animation.RotationTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                // Write out size track.
                if (animation.SizeTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(SizeData);
                    binWriter.WriteValue(animation.SizeTrack.InterpolationMode);
                    binWriter.Write(animation.SizeTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.SizeTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyVector3 key = animation.SizeTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                // Write out bounds track.
                if (animation.RectBoundsTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(BoundsData);
                    binWriter.WriteValue(animation.RectBoundsTrack.InterpolationMode);
                    binWriter.Write(animation.RectBoundsTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.RectBoundsTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyRectangle key = animation.RectBoundsTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                // Write out colors track.
                if (animation.ColorTrack.KeyFrames.Count > 0)
                {
                    binWriter = writer.OpenChunk(ColorData);
                    binWriter.WriteValue(animation.ColorTrack.InterpolationMode);
                    binWriter.Write(animation.ColorTrack.KeyFrames.Count);

                    for (int i = 0; i < animation.ColorTrack.KeyFrames.Count; ++i)
                    {
                        GorgonKeyGorgonColor key = animation.ColorTrack.KeyFrames[i];
                        binWriter.Write(key.Time);
                        binWriter.WriteValue(ref key.Value);
                    }

                    writer.CloseChunk();
                }

                if (animation.Texture2DTrack.KeyFrames.Count == 0)
                {
                    return;
                }

                binWriter = writer.OpenChunk(TextureData);
                binWriter.Write(animation.Texture2DTrack.KeyFrames.Count);

                for (int i = 0; i < animation.Texture2DTrack.KeyFrames.Count; ++i)
                {
                    GorgonKeyTexture2D key = animation.Texture2DTrack.KeyFrames[i];

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

                writer.CloseChunk();
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

                int keyCount;

                if (reader.Chunks.Contains(PositionData))
                {
                    binReader = reader.OpenChunk(PositionData);
                    builder.PositionInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector3> track = builder.EditPositions();
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyVector3(binReader.ReadSingle(), binReader.ReadValue<DX.Vector3>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(ScaleData))
                {
                    binReader = reader.OpenChunk(ScaleData);
                    builder.ScaleInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector3> track = builder.EditScale();
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyVector3(binReader.ReadSingle(), binReader.ReadValue<DX.Vector3>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(RotationData))
                {
                    binReader = reader.OpenChunk(RotationData);
                    builder.RotationInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector3> track = builder.EditRotation();
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyVector3(binReader.ReadSingle(), binReader.ReadValue<DX.Vector3>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(SizeData))
                {
                    binReader = reader.OpenChunk(SizeData);
                    builder.SizeInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyVector3> track = builder.EditSize();
                    for (int i = 0; i < keyCount; ++i)
                    {
                        track.SetKey(new GorgonKeyVector3(binReader.ReadSingle(), binReader.ReadValue<DX.Vector3>()));
                    }
                    track.EndEdit();
                    reader.CloseChunk();
                }

                if (reader.Chunks.Contains(BoundsData))
                {
                    binReader = reader.OpenChunk(BoundsData);
                    builder.RotationInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyRectangle> track = builder.EditRectangularBounds();
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
                    builder.RotationInterpolationMode(binReader.ReadValue<TrackInterpolationMode>());
                    keyCount = binReader.ReadInt32();

                    IGorgonTrackKeyBuilder<GorgonKeyGorgonColor> track = builder.EditColors();
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

                IGorgonTrackKeyBuilder<GorgonKeyTexture2D> textureTrack = builder.Edit2DTexture();
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
