
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 25, 2018 2:43:32 PM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Animation;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// A codec used to read/write animations as binary formatted data
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV31AnimationBinaryCodec"/> class
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
public class GorgonV31AnimationBinaryCodec(Gorgon2D renderer)
        : GorgonAnimationCodecCommon(renderer, Resources.GOR2DIO_V3_1_ANIM_BIN_CODEC, Resources.GOR2DIO_V3_1_ANIM_BIN_CODEC_DESCRIPTION)
{
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
    /// The Quaternion tracks data chunk ID.
    /// </summary>
    public static readonly ulong QuaternionData = "QUATDATA".ChunkID();
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

    /// <summary>
    /// Function to load the texture information.
    /// </summary>
    /// <param name="reader">The reader containing the texture information.</param>
    /// <param name="textureOverrides">Overrides for the texture keys.</param>
    /// <returns>The texture attached to the sprite.</returns>
    private (GorgonTexture2DView Texture, string TextureName) LoadTexture(IGorgonChunkReader reader, IEnumerable<GorgonTexture2DView> textureOverrides)
    {
        // Write out as much info about the texture as we can so we can look it up based on these values when loading.
        string textureName = reader.ReadString();

        if (string.IsNullOrWhiteSpace(textureName))
        {
            return (null, null);
        }

        reader.ReadValue(out int textureWidth);
        reader.ReadValue(out int textureHeight);
        reader.ReadValue(out BufferFormat textureFormat);
        reader.ReadValue(out int textureArrayCount);
        reader.ReadValue(out int textureMipCount);

        // Locate the texture resource.
        GorgonTexture2DView overrideMatch = textureOverrides?.FirstOrDefault(item => string.Equals(item.Texture.Name, textureName, StringComparison.OrdinalIgnoreCase));

        // We need to read these even if we don't use them.
        reader.ReadValue(out int viewArrayIndex);
        reader.ReadValue(out int viewArrayCount);
        reader.ReadValue(out int viewMipSlice);
        reader.ReadValue(out int viewMipCount);
        reader.ReadValue(out BufferFormat viewFormat);

        if (overrideMatch is not null)
        {
            return (overrideMatch, textureName);
        }

        GorgonTexture2D texture = Renderer.Graphics.Locate2DTextureByName(textureName, textureWidth, textureHeight, textureFormat, textureArrayCount, textureMipCount);

        if (texture is null)
        {
            return (null, textureName);
        }

        if (viewArrayCount == -1)
        {
            viewArrayCount = texture.ArrayCount;
        }

        if (viewMipCount == -1)
        {
            viewMipCount = texture.MipLevels;
        }

        return (texture.GetShaderResourceView(viewFormat, viewMipSlice.Max(0).Min(texture.MipLevels - 1),
                                                          viewMipCount.Max(1).Min(texture.MipLevels),
                                                          viewArrayIndex.Max(0).Min(texture.ArrayCount - 1),
                                                          viewArrayCount.Max(1).Min(texture.ArrayCount)), textureName);
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

        using IGorgonChunkReader binReader = reader.OpenChunk(VersionData);
        Version fileVersion = new(binReader.ReadByte(), binReader.ReadByte());

        return Version.Equals(fileVersion);
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

        using IGorgonChunkWriter binWriter = writer.OpenChunk(chunkID);
        binWriter.WriteInt32(tracks.Count(item => item.Value.KeyFrames.Count > 0));

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> track in tracks.Where(item => item.Value.KeyFrames.Count > 0))
        {
            binWriter.WriteString(track.Key);
            binWriter.WriteBool(track.Value.IsEnabled);
            binWriter.WriteInt32(track.Value.KeyFrames.Count);

            for (int i = 0; i < track.Value.KeyFrames.Count; ++i)
            {
                GorgonKeyTexture2D key = track.Value.KeyFrames[i];

                binWriter.WriteSingle(key.Time);

                if ((key.Value is null) && (string.IsNullOrWhiteSpace(key.TextureName)))
                {
                    binWriter.WriteValue<byte>(0);
                }
                else
                {
                    binWriter.WriteValue<byte>(1);

                    if (key.Value is not null)
                    {
                        binWriter.WriteString(key.Value.Texture.Name);
                        binWriter.WriteInt32(key.Value.Texture.Width);
                        binWriter.WriteInt32(key.Value.Texture.Height);
                        binWriter.WriteValue(key.Value.Texture.Format);
                        binWriter.WriteInt32(key.Value.Texture.ArrayCount);
                        binWriter.WriteInt32(key.Value.Texture.MipLevels);
                        binWriter.WriteInt32(key.Value.ArrayIndex);
                        binWriter.WriteInt32(key.Value.ArrayCount);
                        binWriter.WriteInt32(key.Value.MipSlice);
                        binWriter.WriteInt32(key.Value.MipCount);
                        binWriter.WriteValue(key.Value.Format);
                    }
                    else
                    {
                        binWriter.WriteString(key.TextureName);
                        // If we don't have any texture reference, write out default values.
                        binWriter.WriteInt32(0);
                        binWriter.WriteInt32(0);
                        binWriter.WriteValue(BufferFormat.Unknown);
                        binWriter.WriteInt32(-1);
                        binWriter.WriteInt32(-1);
                        binWriter.WriteInt32(0);
                        binWriter.WriteInt32(-1);
                        binWriter.WriteInt32(0);
                        binWriter.WriteInt32(-1);
                        binWriter.WriteValue(BufferFormat.Unknown);
                    }
                }

                // SharpDX rectangle data was stored as LTRB, so we need to convert it to keep compatibility.
                GorgonRectangleF tempRect = new(key.TextureCoordinates.Left, key.TextureCoordinates.Top, key.TextureCoordinates.Right, key.TextureCoordinates.Bottom);
                binWriter.WriteValue(in key.TextureCoordinates);
                binWriter.WriteInt32(key.TextureArrayIndex);
            }
        }
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

        using IGorgonChunkWriter binWriter = writer.OpenChunk(chunkID);
        binWriter.WriteInt32(tracks.Count(item => item.Value.KeyFrames.Count > 0));

        foreach (KeyValuePair<string, IGorgonAnimationTrack<Tk>> track in tracks.Where(item => item.Value.KeyFrames.Count > 0))
        {
            binWriter.WriteString(track.Key);
            binWriter.WriteValue(track.Value.InterpolationMode);
            binWriter.WriteBool(track.Value.IsEnabled);
            binWriter.WriteInt32(track.Value.KeyFrames.Count);

            for (int i = 0; i < track.Value.KeyFrames.Count; ++i)
            {
                Tk key = track.Value.KeyFrames[i];
                Tkd value = getValue(key);
                binWriter.WriteSingle(key.Time);
                binWriter.WriteValue(in value);
            }
        }
    }

    /// <summary>
    /// Function to read in the texture tracks.
    /// </summary>
    /// <param name="reader">The chunk file reader.</param>
    /// <param name="chunkID">The ID of the chunk for the track.</param>
    /// <param name="builder">The builder used to generate the animation.</param>
    /// <param name="overrides">The texture overrides.</param>
    private void ReadTextureTrackValues(GorgonChunkFileReader reader, ulong chunkID, GorgonAnimationBuilder builder, IEnumerable<GorgonTexture2DView> overrides)
    {
        if (!reader.Chunks.Contains(chunkID))
        {
            return;
        }

        using IGorgonChunkReader binReader = reader.OpenChunk(chunkID);
        int trackCount = binReader.ReadInt32();

        for (int i = 0; i < trackCount; ++i)
        {
            string trackName = binReader.ReadString();

            IGorgonTrackKeyBuilder<GorgonKeyTexture2D> trackBuilder = builder.Edit2DTexture(trackName);

            trackBuilder.Enabled(binReader.ReadBool());

            int keyCount = binReader.ReadInt32();

            for (int j = 0; j < keyCount; ++j)
            {
                float time = binReader.ReadSingle();
                byte hasTexture = binReader.ReadByte();
                GorgonTexture2DView texture = null;
                string textureName = string.Empty;

                if (hasTexture != 0)
                {
                    (texture, textureName) = LoadTexture(binReader, overrides);

                    if ((texture is null) && (string.IsNullOrWhiteSpace(textureName)))
                    {
                        Renderer.Log.Print("Attempted to load a texture from the data, but the texture was not in memory and the name is unknown.",
                                             LoggingLevel.Verbose);
                    }
                }

                // SharpDX rectangle data was stored as LTRB, so we need to convert it to keep compatibility.
                GorgonRectangleF tempRect = binReader.ReadValue<GorgonRectangleF>();
                if ((texture is null) && (hasTexture is not 0))
                {
                    trackBuilder.SetKey(new GorgonKeyTexture2D(time, textureName, GorgonRectangleF.FromLTRB(tempRect.X, tempRect.Y, tempRect.Width, tempRect.Height), binReader.ReadInt32()));
                }
                else
                {
                    trackBuilder.SetKey(new GorgonKeyTexture2D(time, texture, GorgonRectangleF.FromLTRB(tempRect.X, tempRect.Y, tempRect.Width, tempRect.Height), binReader.ReadInt32()));
                }
            }

            trackBuilder.EndEdit();
        }
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

        using IGorgonChunkReader binReader = reader.OpenChunk(chunkID);
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
            builder.Enabled(binReader.ReadBool());
            int keyCount = binReader.ReadInt32();

            for (int j = 0; j < keyCount; ++j)
            {
                builder.SetKey(createKey(binReader.ReadSingle(), binReader.ReadValue<Tkd>()));
            }

            builder.EndEdit();
        }
    }

    /// <summary>
    /// Function to save the animation data to a stream.
    /// </summary>
    /// <param name="animation">The animation to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the animation.</param>
    protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream)
    {
        GorgonChunkFileWriter writer = new(stream, CurrentFileHeader);
        IGorgonChunkWriter binWriter = null;

        try
        {
            writer.Open();
            binWriter = writer.OpenChunk(VersionData);
            binWriter.WriteByte((byte)Version.Major);
            binWriter.WriteByte((byte)Version.Minor);
            writer.Close();

            binWriter = writer.OpenChunk(AnimationData);
            binWriter.WriteString("NA");
            binWriter.WriteSingle(animation.Length);
            binWriter.WriteSingle(animation.Fps);
            binWriter.WriteBool(animation.IsLooped);
            binWriter.WriteInt32(animation.LoopCount);
            writer.Close();

            // Write tracks with value type data.
            WriteTrackValues(writer, SingleData, animation.SingleTracks, k => k.Value);
            WriteTrackValues(writer, Vector2Data, animation.Vector2Tracks, k => k.Value);
            WriteTrackValues(writer, Vector3Data, animation.Vector3Tracks, k => k.Value);
            WriteTrackValues(writer, Vector4Data, animation.Vector4Tracks, k => k.Value);
            WriteTrackValues(writer, QuaternionData, animation.QuaternionTracks, k => k.Value);
            // SharpDX rectangle data was stored as LTRB, so we need to convert it to keep compatibility.
            WriteTrackValues(writer, RectData, animation.RectangleTracks, k => new GorgonRectangleF(k.Value.Left, k.Value.Top, k.Value.Right, k.Value.Bottom));
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

    /// <summary>Function to retrieve the names of the associated textures.</summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream)
    {
        List<string> result = [];

        using GorgonChunkFileReader reader = new(stream, [CurrentFileHeader]);
        reader.Open();

        if (!reader.Chunks.Contains(TextureData))
        {
            return [];
        }

        using IGorgonChunkReader binReader = reader.OpenChunk(TextureData);

        int trackCount = binReader.ReadInt32();

        for (int i = 0; i < trackCount; ++i)
        {
            binReader.ReadString();
            binReader.ReadBool();
            int keyCount = binReader.ReadInt32();

            for (int j = 0; j < keyCount; ++j)
            {
                binReader.ReadSingle();
                byte hasTexture = binReader.ReadByte();
                string textureName = string.Empty;

                if (hasTexture != 0)
                {
                    textureName = binReader.ReadString();

                    if ((!string.IsNullOrWhiteSpace(textureName))
                        && (!result.Contains(textureName)))
                    {
                        result.Add(textureName);
                    }

                    binReader.Skip((sizeof(int) * 8) + (sizeof(BufferFormat) * 2));
                }

                binReader.Skip(Unsafe.SizeOf<GorgonRectangleF>() + sizeof(int));
            }
        }

        return result;
    }

    /// <summary>Function to read the animation data from a stream.</summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation" />.</returns>
    /// <remarks>
    /// Implementors should handle the <paramref name="textureOverrides" /> parameter by matching the textures by name, and, if the texture is not found in the override list, fall back to whatever scheme
    /// is used to retrieve the texture for codec.
    /// </remarks>
    protected override IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount, IEnumerable<GorgonTexture2DView> textureOverrides)
    {
        GorgonAnimationBuilder builder = new();

        GorgonChunkFileReader reader = new(stream,
                                               [
                                                   CurrentFileHeader
                                               ]);
        IGorgonChunkReader binReader = null;

        try
        {
            reader.Open();
            binReader = reader.OpenChunk(AnimationData);
            // We don't use the stored name anymore.
            binReader.ReadString();
            float length = binReader.ReadSingle();
            float fps = binReader.ReadSingle();
            bool isLooped = binReader.ReadBool();
            int loopCount = binReader.ReadInt32();
            binReader.Close();

            ReadTrackValues<GorgonKeySingle, float>(reader, SingleData, builder.EditSingle, (t, v) => new GorgonKeySingle(t, v));
            ReadTrackValues<GorgonKeyVector2, Vector2>(reader, Vector2Data, builder.EditVector2, (t, v) => new GorgonKeyVector2(t, v));
            ReadTrackValues<GorgonKeyVector3, Vector3>(reader, Vector3Data, builder.EditVector3, (t, v) => new GorgonKeyVector3(t, v));
            ReadTrackValues<GorgonKeyVector4, Vector4>(reader, Vector4Data, builder.EditVector4, (t, v) => new GorgonKeyVector4(t, v));
            ReadTrackValues<GorgonKeyQuaternion, Quaternion>(reader, QuaternionData, builder.EditQuaternion, (t, v) => new GorgonKeyQuaternion(t, v));
            ReadTrackValues<GorgonKeyRectangle, GorgonRectangleF>(reader, RectData, builder.EditRectangle, (t, v) => new GorgonKeyRectangle(t, v));
            ReadTrackValues<GorgonKeyGorgonColor, GorgonColor>(reader, ColorData, builder.EditColor, (t, v) => new GorgonKeyGorgonColor(t, v));
            ReadTextureTrackValues(reader, TextureData, builder, textureOverrides);

            IGorgonAnimation result;
            result = builder.Build(name, fps, length);
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
            ulong[] appIDs = [CurrentFileHeader];

            if (!GorgonChunkFileReader.IsReadable(stream, appIDs))
            {
                return false;
            }

            reader = new GorgonChunkFileReader(stream, appIDs);
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
}
