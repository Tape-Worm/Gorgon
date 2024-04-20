﻿
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

using System.Text;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using System.Text.Json;

namespace Gorgon.IO;

/// <summary>
/// A codec used to read/write animations as a JSON formatted string value
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV31AnimationJsonCodec"/> class
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
public class GorgonV31AnimationJsonCodec(Gorgon2D renderer)
        : GorgonAnimationCodecCommon(renderer, Resources.GOR2DIO_V3_1_ANIM_JSON_CODEC, Resources.GOR2DIO_V3_1_ANIM_JSON_CODEC_DESCRIPTION)
{
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
    /// Function to determine if the JSON object has the data we need.
    /// </summary>
    /// <param name="document">The JSON document to evaluate.</param>
    /// <returns><b>true</b> if the data is for a sprite, <b>false</b> if not.</returns>
    private bool IsReadableJsonData(JsonDocument document)
    {
        if ((!document.RootElement.TryGetProperty(JsonAnimationConverter.JsonHeaderProp, out JsonElement headerElement))
            || (!document.RootElement.TryGetProperty(JsonAnimationConverter.JsonVersionProp, out JsonElement versionElement)))
        {
            return false;
        }

        return (headerElement.TryGetUInt64(out ulong header))
            && (Version.TryParse(versionElement.GetString(), out Version? version))
            && (header == CurrentFileHeader)
            && (version.Equals(Version));
    }

    /*
    /// <summary>
    /// Function to read key data for a track.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <param name="trackProps">The rest of the track properties.</param>
    /// <returns>A list of 3D vector key frames.</returns>
    private static List<Tk> ReadKeyData<Tk, Tc>(JsonReader reader, Tc converter, out (TrackInterpolationMode interpolation, bool enabled, string trackName) trackProps)
           where Tk : class, IGorgonKeyFrame
           where Tc : JsonConverter<Tk>
    {
        List<Tk> ReadKeys()
        {
            List<Tk> keys = [];
            Type type = typeof(Tk);

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
            {
                if (reader.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                keys.Add(converter.ReadJson(reader, type, null, false, null));
            }

            return keys;
        }

        List<Tk> positions = null;
        TrackInterpolationMode interpolation = TrackInterpolationMode.None;
        string trackName = string.Empty;
        bool isEnabled = true;

        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            string propName = reader.Value.ToString().ToUpperInvariant();

            switch (propName)
            {
                case "INTERPOLATIONMODE":
                    interpolation = (TrackInterpolationMode)(reader.ReadAsInt32() ?? 0);
                    break;
                case "KEYFRAMES":
                    positions = ReadKeys();
                    break;
                case "ISENABLED":
                    isEnabled = reader.ReadAsBoolean() ?? true;
                    break;
                case "NAME":
                    trackName = reader.ReadAsString();
                    break;
            }
        }

        trackProps = (interpolation, isEnabled, trackName);

        return positions;
    }

    /// <summary>
    /// Function to read in a track containing single precision floating point key values.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter for key frame data.</param>
    /// <param name="track">The track list to update.</param>
    private void ReadTrack<Tk, Tc>(JsonTextReader reader, Tc converter, Dictionary<string, (List<Tk> keys, TrackInterpolationMode interpolation, bool isEnabled)> track)
        where Tk : class, IGorgonKeyFrame
        where Tc : JsonConverter<Tk>
    {
        if ((!reader.Read()) || (reader.TokenType != JsonToken.StartObject))
        {
            return;
        }

        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
        {
            List<Tk> keys = ReadKeyData<Tk, Tc>(reader, converter, out (TrackInterpolationMode interpolation, bool enabled, string trackName) trackProps);
            track[trackProps.trackName] = (keys, trackProps.interpolation, trackProps.enabled);
        }
    }*/

    /// <summary>Function to retrieve the names of the associated textures.</summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream)
    {
        string jsonString = null;

        List<string> result = [];

        /*// Loads the texture names.
        void LoadNames(JsonReader reader)
        {
            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "KEYFRAMES":
                        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                        {
                            if (reader.TokenType != JsonToken.PropertyName)
                            {
                                continue;
                            }

                            propName = reader.Value.ToString().ToUpperInvariant();

                            switch (propName)
                            {
                                case "UV":
                                    while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                                    {
                                        // Skip or we'll exit prematurely.
                                    }
                                    reader.Read();
                                    break;
                                case "TEXTURE":
                                    while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                                    {
                                        if (reader.TokenType != JsonToken.PropertyName)
                                        {
                                            continue;
                                        }

                                        switch (reader.Value.ToString())
                                        {
                                            case "name":
                                                string textureName = reader.ReadAsString();

                                                if ((!string.IsNullOrWhiteSpace(textureName)) && (!result.Contains(textureName)))
                                                {
                                                    result.Add(textureName);
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        using (GorgonSubStream wrappedStream = new(stream, stream.Position, stream.Length - stream.Position, false))
        using (StreamReader streamReader = new(wrappedStream, Encoding.UTF8, true, 80192, true))
        {
            jsonString = streamReader.ReadToEnd();
        }

        using (StringReader baseReader = new(jsonString))
        using (JsonTextReader reader = new(baseReader))
        {
            if (!IsReadableJObject(reader))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
            }

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "TEXTURES":
                        if ((!reader.Read()) || (reader.TokenType != JsonToken.StartObject))
                        {
                            return [];
                        }

                        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                        {
                            LoadNames(reader);
                        }
                        break;
                }
            }
        }*/

        return result;
    }
    
    /// <summary>
    /// Function to convert a JSON formatted string into a <see cref="IGorgonAnimation"/> object.
    /// </summary>
    /// <param name="renderer">The renderer for the animation.</param>
    /// <param name="json">The JSON string containing the animation data.</param>
    /// <param name="name">[Optional] The name of the animation.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the JSON string does not contain animation data, or there is a version mismatch.</exception>
    /// <remarks>
    /// <para>
    /// When passing in a list of <paramref name="textureOverrides"/>, the texture names should match the expected texture names in the key frame. For example, if the 
    /// <see cref="GorgonKeyTexture2D.TextureName"/> is <c>"WalkingFrames"</c>, then the <see cref="GorgonTexture2D.Name"/> should also be <c>"WalkingFrames"</c>. 
    /// </para>
    /// </remarks>
    public static IGorgonAnimation FromJson(Gorgon2D renderer, string json, string name = null, IEnumerable<GorgonTexture2DView> textureOverrides = null)
    {
        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (json is null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentEmptyException(nameof(json));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"GorgonAnimation_{Guid.NewGuid():N}";
        }

        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new JsonAnimationConverter(renderer, textureOverrides, name)
            }
        };

        using JsonDocument document = JsonDocument.Parse(json);

        if ((!document.RootElement.TryGetProperty(JsonAnimationConverter.JsonHeaderProp, out JsonElement headerElement))
            || (!document.RootElement.TryGetProperty(JsonAnimationConverter.JsonVersionProp, out JsonElement versionElement))
            || (!headerElement.TryGetUInt64(out ulong id))
            || (id != CurrentFileHeader))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
        }

        if ((!Version.TryParse(versionElement.GetString(), out Version? version)) || (!CurrentVersion.Equals(version)))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_ANIM_VERSION_MISMATCH, CurrentVersion, version));
        }

        return document.Deserialize<IGorgonAnimation>(options);

        /*
        float animLength = 0;
        float animFps = 0;
        int loopCount = 0;
        bool isLooped = false;

        JsonGorgonColorKeyConverter colorConvert = new();
        JsonTextureKeyConverter textureConvert = new(renderer.Graphics, textureOverrides);
        JsonSingleKeyConverter singleConverter = new();
        JsonVector2KeyConverter vec2Converter = new();
        JsonVector3KeyConverter vec3Converter = new();
        JsonVector4KeyConverter vec4Converter = new();
        JsonQuaternionKeyConverter quatConverter = new();
        JsonRectKeyConverter rectConverter = new();

        Dictionary<string, (List<GorgonKeyVector4>, TrackInterpolationMode, bool)> vec4 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyQuaternion>, TrackInterpolationMode, bool)> quat = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyVector3>, TrackInterpolationMode, bool)> vec3 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyVector2>, TrackInterpolationMode, bool)> vec2 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeySingle>, TrackInterpolationMode, bool)> singles = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyGorgonColor>, TrackInterpolationMode, bool)> colors = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyRectangle>, TrackInterpolationMode, bool)> rects = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyTexture2D>, TrackInterpolationMode, bool)> textures = new(StringComparer.OrdinalIgnoreCase);

        using (StringReader baseReader = new(json))
        using (JsonTextReader reader = new(baseReader))
        {
            if (!IsReadableJObject(reader))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
            }

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "SINGLETRACKS":
                        ReadTrack(reader, singleConverter, singles);
                        break;
                    case "VECTOR2TRACKS":
                        ReadTrack(reader, vec2Converter, vec2);
                        break;
                    case "VECTOR3TRACKS":
                        ReadTrack(reader, vec3Converter, vec3);
                        break;
                    case "VECTOR4TRACKS":
                        ReadTrack(reader, vec4Converter, vec4);
                        break;
                    case "QUATERNIONTRACKS":
                        ReadTrack(reader, quatConverter, quat);
                        break;
                    case "RECTTRACKS":
                        ReadTrack(reader, rectConverter, rects);
                        break;
                    case "COLORTRACKS":
                        ReadTrack(reader, colorConvert, colors);
                        break;
                    case "TEXTURES":
                        ReadTrack(reader, textureConvert, textures);
                        break;
                    case "NAME":
                        reader.ReadAsString();
                        break;
                    case "LENGTH":
                        animLength = (float)(reader.ReadAsDecimal() ?? 0);
                        break;
                    case "FPS":
                        animFps = (float)(reader.ReadAsDecimal() ?? 60);
                        break;
                    case "ISLOOPED":
                        isLooped = (reader.ReadAsBoolean() ?? false);
                        break;
                    case "LOOPCOUNT":
                        loopCount = (reader.ReadAsInt32() ?? 0);
                        break;
                }
            }
        }

        GorgonAnimationBuilder builder = new();

        if (singles.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeySingle> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in singles)
            {
                builder.EditSingle(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (vec2.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyVector2> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in vec2)
            {
                builder.EditVector2(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (vec3.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyVector3> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in vec3)
            {
                builder.EditVector3(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (vec4.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyVector4> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in vec4)
            {
                builder.EditVector4(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (quat.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyQuaternion> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in quat)
            {
                builder.EditQuaternion(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (rects.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyRectangle> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in rects)
            {
                builder.EditRectangle(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (colors.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyGorgonColor> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in colors)
            {
                builder.EditColor(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        if (textures.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeyTexture2D> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in textures)
            {
                builder.Edit2DTexture(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }

        IGorgonAnimation result = builder.Build(name, animFps, animLength);

        result.LoopCount = loopCount;
        result.IsLooped = isLooped;
        result.Speed = 1.0f;

        return result;
        */
    }

    /// <summary>
    /// Function to save the animation data to a stream.
    /// </summary>
    /// <param name="animation">The animation to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the animation.</param>
    protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream)
    {
        using StreamWriter writer = new(stream, Encoding.UTF8, 1024, true);
        writer.Write(animation.ToJson());
    }

    /// <summary>
    /// Function to read the animation data from a stream.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    protected override IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount, IEnumerable<GorgonTexture2DView> textureOverrides)
    {
        using GorgonSubStream wrappedStream = new(stream, stream.Position, byteCount, false);
        using StreamReader reader = new(wrappedStream, Encoding.UTF8, true, 80192, true);
        string jsonString = reader.ReadToEnd();
        return FromJson(Renderer, name, jsonString, textureOverrides);
    }

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    protected override bool OnIsReadable(Stream stream)
    {
        if (stream.Length < 4)
        {
            return false;
        }

        // Read the string from the stream.
        string json = stream.ReadString();

        // If we don't have a string, or we don't start with "{", then it's not valid JSON.
        if ((string.IsNullOrWhiteSpace(json)) || (!json.StartsWith("{", StringComparison.Ordinal)))
        {
            return false;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        return IsReadableJsonData(document);
    }
}
