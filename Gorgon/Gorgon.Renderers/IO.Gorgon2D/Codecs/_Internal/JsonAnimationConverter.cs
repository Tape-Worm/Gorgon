// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: April 17, 2024 10:20:22 PM
//
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Animation;
using Gorgon.Renderers;
using Gorgon.Graphics.Core;
using System.Drawing.Printing;

namespace Gorgon.IO;

/// <summary>
/// A converter to convert a <see cref="IGorgonAnimation"/> object to and from JSON.
/// </summary>
internal class JsonAnimationConverter
    : JsonConverter<IGorgonAnimation>
{
    /// <summary>
    /// The property name for the header value.
    /// </summary>
    public const string JsonHeaderProp = "header";
    /// <summary>
    /// The property name for the header value.
    /// </summary>
    public const string JsonVersionProp = "version";

    // The builder used to create the animation object.
    private readonly GorgonAnimationBuilder _builder = new();
    // The name for the animation.
    private readonly string _name;
    // Converters for data types.
    private readonly JsonTextureKeyConverter _textureConverter;
    private readonly JsonSingleKeyConverter _singleKeyConverter;
    private readonly JsonVector2KeyConverter _vector2KeyConverter;
    private readonly JsonVector3KeyConverter _vector3KeyConverter;
    private readonly JsonVector4KeyConverter _vector4KeyConverter;
    private readonly JsonGorgonColorKeyConverter _colorKeyConverter;
    private readonly JsonQuaternionKeyConverter _quatConverter;
    private readonly JsonRectKeyConverter _rectKeyConverter;

    /// <summary>
    /// Function to read the keys for a given track.
    /// </summary>
    /// <typeparam name="T">The type of key in the track.</typeparam>
    /// <typeparam name="Tc">The type of converter required to convert key frames from JSON.</typeparam>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter for the key values.</param>
    /// <param name="options">The options for the serializer.</param>
    private List<T> ReadKeys<T, Tc>(ref Utf8JsonReader reader, Tc converter, JsonSerializerOptions options)
        where T : IGorgonKeyFrame
        where Tc : JsonConverter<T>
    {
        List<T> keys = [];

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return keys;
        }

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndArray))
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                continue;
            }

            keys.Add(converter.Read(ref reader, typeof(T), options));
        }

        return keys;
    }

    /// <summary>
    /// Function to deserialize animation track data from JSON.
    /// </summary>
    /// <typeparam name="T">The type of key in the track.</typeparam>
    /// <typeparam name="Tc">The type of converter required to convert key frames from JSON.</typeparam>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the key frame data from JSON.</param>
    /// <param name="options">The options for the serializer.</param>
    /// <returns>A tuple containing the metadata for the track, and a list of keys for that track.</returns>
    private (TrackInterpolationMode Interpolation, bool Enabled, string TrackName, List<T>? Keys) ReadTrack<T, Tc>(ref Utf8JsonReader reader, Tc converter, JsonSerializerOptions options)
        where T : IGorgonKeyFrame
        where Tc : JsonConverter<T>
    {
        List<T>? keys = null;
        TrackInterpolationMode interpolation = TrackInterpolationMode.None;
        string trackName = string.Empty;
        bool isEnabled = true;

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndObject))
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string? propertyName = reader.GetString()?.ToUpperInvariant();

            if ((string.IsNullOrWhiteSpace(propertyName)) || (!reader.Read()))
            {
                break;
            }

            switch (propertyName)
            {
                case "INTERPOLATIONMODE":
                    interpolation = (TrackInterpolationMode)reader.GetInt32();
                    break;
                case "KEYFRAMES":
                    keys = ReadKeys<T, Tc>(ref reader, converter, options);
                    break;
                case "ISENABLED":
                    isEnabled = reader.GetBoolean();
                    break;
                case "NAME":
                    trackName = reader.GetString();
                    break;
            }
        }

        return (interpolation, isEnabled, trackName, keys);
    }

    /// <summary>
    /// Function to read in a track containing single precision floating point key values.
    /// </summary>
    /// <typeparam name="T">The type of key in the track.</typeparam>
    /// <typeparam name="Tc">The type of converter required to convert key frames from JSON.</typeparam>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter for key frame data.</param>
    /// <param name="track">The track list to update.</param>
    /// <param name="options">The options for the serializer.</param>
    private void ReadTrack<T, Tc>(ref Utf8JsonReader reader, Tc converter, Dictionary<string, (List<T> keys, TrackInterpolationMode interpolation, bool isEnabled)> track, JsonSerializerOptions options)
        where T : class, IGorgonKeyFrame
        where Tc : JsonConverter<T>
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return;
        }

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndArray))
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                continue;
            }

            (TrackInterpolationMode interpolation, bool enabled, string trackName, List<T>? keys) = ReadTrack<T, Tc>(ref reader, converter, options);
            track[trackName] = (keys, interpolation, enabled);
        }
    }

    /// <summary>
    /// Function to serialize animation track data into JSON.
    /// </summary>
    /// <typeparam name="T">The type of key in the track.</typeparam>
    /// <typeparam name="Tc">The type of converter required to convert key frames into JSON.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="name">The name of the track.</param>
    /// <param name="tracks">The tracks to serialize.</param>
    /// <param name="converter">The converter used to convert the key frame data to JSON.</param>
    /// <param name="options">The options for the serializer.</param>
    private void WriteTracks<T, Tc>(Utf8JsonWriter writer, string name, IReadOnlyDictionary<string, IGorgonAnimationTrack<T>> tracks, Tc converter, JsonSerializerOptions options)
        where T : IGorgonKeyFrame
        where Tc : JsonConverter<T>
    {
        writer.WriteStartArray(name);

        foreach (KeyValuePair<string, IGorgonAnimationTrack<T>> track in tracks)
        {
            writer.WriteStartObject();
            writer.WriteString("name", track.Key);
            writer.WriteNumber("interpolationmode", (int)track.Value.InterpolationMode);
            writer.WriteBoolean("isenabled", track.Value.IsEnabled);

            writer.WriteStartArray("keyframes");
            foreach (T key in track.Value.KeyFrames)
            {
                converter.Write(writer, key, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IGorgonAnimation value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(JsonHeaderProp, GorgonAnimationCodecCommon.CurrentFileHeader);
        writer.WriteString(JsonVersionProp, GorgonAnimationCodecCommon.CurrentVersion.ToString(2));
        writer.WriteNumber("length", value.Length);
        writer.WriteNumber("fps", value.Fps);
        writer.WriteBoolean("islooped", value.IsLooped);
        writer.WriteNumber("loopcount", value.LoopCount);

        if (value.SingleTracks.Count > 0)
        {
            WriteTracks(writer, "singletracks", value.SingleTracks, _singleKeyConverter, options);
        }

        if (value.Vector2Tracks.Count > 0)
        {
            WriteTracks(writer, "vector2tracks", value.Vector2Tracks, _vector2KeyConverter, options);
        }

        if (value.Vector3Tracks.Count > 0)
        {
            WriteTracks(writer, "vector3tracks", value.Vector3Tracks, _vector3KeyConverter, options);
        }

        if (value.Vector4Tracks.Count > 0)
        {
            WriteTracks(writer, "vector4tracks", value.Vector4Tracks, _vector4KeyConverter, options);
        }

        if (value.QuaternionTracks.Count > 0)
        {
            WriteTracks(writer, "quaterniontracks", value.QuaternionTracks, _quatConverter, options);
        }

        if (value.ColorTracks.Count > 0)
        {
            WriteTracks(writer, "colortracks", value.ColorTracks, _colorKeyConverter, options);
        }

        if (value.Texture2DTracks.Count > 0)
        {
            WriteTracks(writer, "textures", value.Texture2DTracks, _textureConverter, options);
        }

        if (value.RectangleTracks.Count > 0)
        {
            WriteTracks(writer, "rectangletracks", value.RectangleTracks, _rectKeyConverter, options);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override IGorgonAnimation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float? animLength = null;
        float animFps = 0;
        int loopCount = 0;
        bool isLooped = false;

        // Track data.
        Dictionary<string, (List<GorgonKeyVector4>, TrackInterpolationMode, bool)> vec4 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyQuaternion>, TrackInterpolationMode, bool)> quat = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyVector3>, TrackInterpolationMode, bool)> vec3 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyVector2>, TrackInterpolationMode, bool)> vec2 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeySingle>, TrackInterpolationMode, bool)> singles = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyGorgonColor>, TrackInterpolationMode, bool)> colors = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyRectangle>, TrackInterpolationMode, bool)> rects = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, (List<GorgonKeyTexture2D>, TrackInterpolationMode, bool)> textures = new(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string? propertyName = reader.GetString()?.ToUpperInvariant();

            if ((!reader.Read()) || (string.IsNullOrWhiteSpace(propertyName)))
            {
                break;
            }

            switch(propertyName)
            {
                case "LENGTH":
                    animLength = reader.GetSingle();
                    break;
                case "FPS":
                    animFps = reader.GetSingle();
                    break;
                case "ISLOOPED":
                    isLooped = reader.GetBoolean();
                    break;
                case "LOOPCOUNT":
                    loopCount = reader.GetInt32();
                    break;
                case "SINGLETRACKS":
                    ReadTrack(ref reader, _singleKeyConverter, singles, options);
                    break;
                case "VECTOR2TRACKS":
                    ReadTrack(ref reader, _vector2KeyConverter, vec2, options);
                    break;
                case "VECTOR3TRACKS":
                    ReadTrack(ref reader, _vector3KeyConverter, vec3, options);
                    break;
                case "VECTOR4TRACKS":
                    ReadTrack(ref reader, _vector4KeyConverter, vec4, options);
                    break;
                case "QUATERNIONTRACKS":
                    ReadTrack(ref reader, _quatConverter, quat, options);
                    break;
                case "RECTTRACKS":
                    ReadTrack(ref reader, _rectKeyConverter, rects, options);
                    break;
                case "COLORTRACKS":
                    ReadTrack(ref reader, _colorKeyConverter, colors, options);
                    break;
                case "TEXTURES":
                    ReadTrack(ref reader, _textureConverter, textures, options);
                    break;
            }
        }

        if (animLength is null)
        {
            return null;
        }

        _builder.Clear();

        if (singles.Count > 0)
        {
            foreach (KeyValuePair<string, (List<GorgonKeySingle> keys, TrackInterpolationMode interpolation, bool isEnabled)> track in singles)
            {
                _builder.EditSingle(track.Key)
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
                _builder.EditVector2(track.Key)
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
                _builder.EditVector3(track.Key)
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
                _builder.EditVector4(track.Key)
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
                _builder.EditQuaternion(track.Key)
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
                _builder.EditRectangle(track.Key)
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
                _builder.EditColor(track.Key)
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
                _builder.Edit2DTexture(track.Key)
                       .SetInterpolationMode(track.Value.interpolation)
                       .Enabled(track.Value.isEnabled)
                       .SetKeys(track.Value.keys)
                       .EndEdit();
            }
        }


        IGorgonAnimation result = _builder.Build(_name, animFps, animLength);
        result.IsLooped = isLooped;
        result.LoopCount = loopCount;
        result.Speed = 1.0f;

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAnimationConverter"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used to locate texture data.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <param name="name">The name for the animation.</param>
    public JsonAnimationConverter(Gorgon2D? renderer, IEnumerable<GorgonTexture2DView>? textureOverrides, string? name)
    {
        _name = string.IsNullOrWhiteSpace(name) ? $"GorgonAnimation_{Guid.NewGuid():N}" : name;
        _textureConverter = new JsonTextureKeyConverter(renderer?.Graphics, textureOverrides);
        _singleKeyConverter = new JsonSingleKeyConverter();
        _vector2KeyConverter = new JsonVector2KeyConverter();
        _vector3KeyConverter = new JsonVector3KeyConverter();
        _vector4KeyConverter = new JsonVector4KeyConverter();
        _colorKeyConverter = new JsonGorgonColorKeyConverter();
        _rectKeyConverter = new JsonRectKeyConverter();
        _quatConverter = new JsonQuaternionKeyConverter();
    }
}
