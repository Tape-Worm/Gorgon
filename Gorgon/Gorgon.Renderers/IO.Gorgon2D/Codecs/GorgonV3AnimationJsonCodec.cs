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

using System.Numerics;
using System.Text;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using Newtonsoft.Json;

namespace Gorgon.IO;

/// <summary>
/// A codec used to read/write animations as a JSON formatted string value.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV3AnimationJsonCodec"/> class.
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
public class GorgonV3AnimationJsonCodec(Gorgon2D renderer)
        : GorgonAnimationCodecCommon(renderer, Resources.GOR2DIO_V3_ANIM_JSON_CODEC, Resources.GOR2DIO_V3_ANIM_JSON_CODEC_DESCRIPTION)
{
    #region Constants.
    /// <summary>
    /// The ID for the file header for the previous version of the animation format.
    /// </summary>
    internal static readonly ulong FileHeader30 = "GORANM30".ChunkID();

    /// <summary>
    /// The previous version for animation serialization.
    /// </summary>
    internal static readonly Version Version30 = new(3, 0);
    #endregion

    #region Properties.
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
    public override Version Version => Version30;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve the stream as JSON.Net object.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns>The data as a JSON.Net object.</returns>
    private static JsonReader GetJsonReader(Stream stream)
    {
        var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
        var jsonReader = new JsonTextReader(reader)
        {
            CloseInput = true
        };
        return jsonReader;
    }

    /// <summary>
    /// Function to determine if the jason object has the data we need.
    /// </summary>
    /// <param name="reader">The reader for the JSON data.</param>
    /// <returns><b>true</b> if the data is for a sprite, <b>false</b> if not.</returns>
    private bool IsReadableJObject(JsonReader reader)
    {
        // Find the header node.
        while (reader.Read())
        {
            if ((string.Equals(reader.Path, "header", StringComparison.Ordinal))
                && (reader.TokenType == JsonToken.PropertyName))
            {
                ulong? id = (ulong?)reader.ReadAsDecimal();

                if ((id is null) || (id != FileHeader30))
                {
                    return false;
                }

                if (!reader.Read())
                {
                    return false;
                }
            }

            // These must come right after each other.
            if ((!string.Equals(reader.Path, "version", StringComparison.Ordinal))
                || (reader.TokenType != JsonToken.PropertyName))
            {
                continue;
            }

            return reader.Read()
&& (Version.TryParse(reader.Value.ToString(), out Version version))
                   && (version.Equals(Version));
        }

        return false;
    }

    /// <summary>
    /// Function to read a track containing rectangle data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <param name="interpolation">The interpolation mode for the track.</param>
    /// <returns>A list of rectangle key frames.</returns>
    private static List<GorgonKeyRectangle> ReadRects(JsonReader reader, JsonRectKeyConverter converter, out TrackInterpolationMode interpolation)
    {
        List<GorgonKeyRectangle> ReadKeys()
        {
            var keys = new List<GorgonKeyRectangle>();
            Type type = typeof(GorgonKeyRectangle);

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

        List<GorgonKeyRectangle> bounds = null;
        interpolation = TrackInterpolationMode.None;

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
                    bounds = ReadKeys();
                    break;
            }
        }

        return bounds;
    }

    /// <summary>
    /// Function to read a track containing <see cref="GorgonColor"/> data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <param name="interpolation">The interpolation mode for the track.</param>
    /// <returns>A list of <see cref="GorgonColor"/> key frames.</returns>
    private static List<GorgonKeyGorgonColor> ReadColors(JsonReader reader, JsonGorgonColorKeyConverter converter, out TrackInterpolationMode interpolation)
    {
        List<GorgonKeyGorgonColor> ReadKeys()
        {
            var keys = new List<GorgonKeyGorgonColor>();
            Type type = typeof(GorgonKeyGorgonColor);

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

        List<GorgonKeyGorgonColor> colors = null;
        interpolation = TrackInterpolationMode.None;

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
                    colors = ReadKeys();
                    break;
            }
        }

        return colors;
    }

    /// <summary>
    /// Function to read a track containing texture data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <returns>A list of texture key frames.</returns>
    private static List<GorgonKeyTexture2D> ReadTextures(JsonReader reader, JsonTextureKeyConverter converter)
    {
        List<GorgonKeyTexture2D> ReadKeys()
        {
            var keys = new List<GorgonKeyTexture2D>();
            Type type = typeof(GorgonKeyTexture2D);

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
            {
                if (reader.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                GorgonKeyTexture2D key = converter.ReadJson(reader, type, null, false, null);

                if (key is not null)
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        List<GorgonKeyTexture2D> textures = null;

        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            string propName = reader.Value.ToString();

            if (string.Equals(propName, "KEYFRAMES", StringComparison.InvariantCultureIgnoreCase))
            {
                textures = ReadKeys();
            }
        }

        return textures;
    }

    /// <summary>
    /// Function to read a track containing 3D vector data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <param name="interpolation">The interpolation mode for the track.</param>
    /// <returns>A list of 3D vector key frames.</returns>
    private static List<GorgonKeySingle> ReadFloat(JsonReader reader, JsonVector3KeyConverter converter, out TrackInterpolationMode interpolation)
    {
        List<GorgonKeySingle> ReadSingleKeys()
        {
            var keys = new List<GorgonKeySingle>();
            Type vec3Type = typeof(GorgonKeyVector3);

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
            {
                if (reader.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                GorgonKeyVector3 vec3 = converter.ReadJson(reader, vec3Type, null, false, null);
                keys.Add(new GorgonKeySingle(vec3.Time, vec3.Value.Z));
            }

            return keys;
        }

        List<GorgonKeySingle> positions = null;
        interpolation = TrackInterpolationMode.None;

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
                    positions = ReadSingleKeys();
                    break;
            }
        }

        return positions;
    }

    /// <summary>
    /// Function to read a track containing 3D vector data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="converter">The converter used to convert the data from JSON.</param>
    /// <param name="interpolation">The interpolation mode for the track.</param>
    /// <returns>A list of 3D vector key frames.</returns>
    private static List<GorgonKeyVector2> ReadVector2(JsonReader reader, JsonVector3KeyConverter converter, out TrackInterpolationMode interpolation)
    {
        List<GorgonKeyVector2> ReadVec3Keys()
        {
            var keys = new List<GorgonKeyVector2>();
            Type vec3Type = typeof(GorgonKeyVector3);

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
            {
                if (reader.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                GorgonKeyVector3 vec3 = converter.ReadJson(reader, vec3Type, null, false, null);
                keys.Add(new GorgonKeyVector2(vec3.Time, new Vector2(vec3.Value.X, vec3.Value.Y)));
            }

            return keys;
        }

        List<GorgonKeyVector2> positions = null;
        interpolation = TrackInterpolationMode.None;

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
                    positions = ReadVec3Keys();
                    break;
            }
        }

        return positions;
    }


    /// <summary>Function to retrieve the names of the associated textures.</summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream)
    {
        static IReadOnlyList<string> ReadTextureNames(JsonReader reader)
        {
            var names = new List<string>();

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
            {
                if (reader.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        continue;
                    }

                    string propName = reader.Value.ToString().ToUpperInvariant();

                    switch (propName)
                    {
                        case "TEXTURE":
                            reader.Read();

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

                                        if ((!string.IsNullOrWhiteSpace(textureName)) && (!names.Contains(textureName)))
                                        {
                                            names.Add(string.IsNullOrEmpty(textureName) ? null : textureName);
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            return names;
        }

        using (JsonReader reader = GetJsonReader(stream))
        {
            if (!IsReadableJObject(reader))
            {
                return [];
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
                        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
                        {
                            if (reader.TokenType != JsonToken.PropertyName)
                            {
                                continue;
                            }

                            propName = reader.Value.ToString();

                            if (string.Equals(propName, "KEYFRAMES", StringComparison.InvariantCultureIgnoreCase))
                            {
                                return ReadTextureNames(reader);
                            }
                        }
                        break;
                }
            }
        }

        return [];
    }

    /// <summary>
    /// Function to convert a JSON formatted string into a <see cref="IGorgonAnimation"/> object.
    /// </summary>
    /// <param name="renderer">The renderer for the animation.</param>
    /// <param name="json">The JSON string containing the animation data.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the JSON string does not contain animation data, or there is a version mismatch.</exception>
    public IGorgonAnimation FromJson(Gorgon2D renderer, string json)
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

        string animName = string.Empty;
        float animLength = 0;
        int loopCount = 0;
        bool isLooped = false;

        var colorConvert = new JsonGorgonColorKeyConverter();
        var textureConvert = new JsonTextureKeyConverter(renderer.Graphics);
        var vec3Converter = new JsonVector3KeyConverter();
        var rectConverter = new JsonRectKeyConverter();

        TrackInterpolationMode posInterp = TrackInterpolationMode.None;
        TrackInterpolationMode scaleInterp = TrackInterpolationMode.None;
        TrackInterpolationMode rotInterp = TrackInterpolationMode.None;
        TrackInterpolationMode sizeInterp = TrackInterpolationMode.None;
        TrackInterpolationMode colorInterp = TrackInterpolationMode.None;
        TrackInterpolationMode boundInterp = TrackInterpolationMode.None;

        List<GorgonKeyVector2> positions = null;
        List<GorgonKeySingle> rotations = null;
        List<GorgonKeyVector2> scales = null;
        List<GorgonKeyVector2> sizes = null;
        List<GorgonKeyGorgonColor> colors = null;
        List<GorgonKeyRectangle> bounds = null;
        List<GorgonKeyTexture2D> textures = null;

        using (var baseReader = new StringReader(json))
        using (var reader = new JsonTextReader(baseReader))
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
                    case "POSITIONS":
                        positions = ReadVector2(reader, vec3Converter, out posInterp);
                        break;
                    case "SCALES":
                        scales = ReadVector2(reader, vec3Converter, out scaleInterp);
                        break;
                    case "ROTATIONS":
                        rotations = ReadFloat(reader, vec3Converter, out rotInterp);
                        break;
                    case "SIZE":
                        sizes = ReadVector2(reader, vec3Converter, out sizeInterp);
                        break;
                    case "BOUNDS":
                        bounds = ReadRects(reader, rectConverter, out boundInterp);
                        break;
                    case "COLORS":
                        colors = ReadColors(reader, colorConvert, out colorInterp);
                        break;
                    case "TEXTURES":
                        textures = ReadTextures(reader, textureConvert);
                        break;
                    case "NAME":
                        animName = reader.ReadAsString();
                        break;
                    case "LENGTH":
                        animLength = (float)(reader.ReadAsDecimal() ?? 0);
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

        // There's no name, so it's a broken JSON.
        if (string.IsNullOrWhiteSpace(animName))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
        }

        var builder = new GorgonAnimationBuilder();

        if ((positions is not null) && (positions.Count > 0))
        {
            builder.EditVector2("Position")
                   .SetInterpolationMode(posInterp)
                   .SetKeys(positions)
                   .EndEdit();
        }

        if ((scales is not null) && (scales.Count > 0))
        {
            builder.EditVector2("Scale")
                   .SetInterpolationMode(scaleInterp)
                   .SetKeys(scales)
                   .EndEdit();
        }

        if ((rotations is not null) && (rotations.Count > 0))
        {
            builder.EditSingle("Angle")
                   .SetInterpolationMode(rotInterp)
                   .SetKeys(rotations)
                   .EndEdit();
        }

        if ((sizes is not null) && (sizes.Count > 0))
        {
            builder.EditVector2("Size")
                   .SetInterpolationMode(sizeInterp)
                   .SetKeys(sizes)
                   .EndEdit();
        }

        if ((colors is not null) && (colors.Count > 0))
        {
            builder.EditColor("Color")
                   .SetInterpolationMode(colorInterp)
                   .SetKeys(colors)
                   .EndEdit();
        }

        if ((bounds is not null) && (bounds.Count > 0))
        {
            builder.EditRectangle("Bounds")
                   .SetInterpolationMode(boundInterp)
                   .SetKeys(bounds)
                   .EndEdit();
        }

        if ((textures is not null) && (textures.Count > 0))
        {
            builder.Edit2DTexture("Texture")
                   .SetKeys(textures)
                   .EndEdit();
        }

        IGorgonAnimation result = builder.Build(animName, animLength);

        result.LoopCount = loopCount;
        result.IsLooped = isLooped;
        result.Speed = 1.0f;

        return result;
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
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    protected override IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount, IEnumerable<GorgonTexture2DView> textureOverrides)
    {
        if ((textureOverrides is not null) && (textureOverrides.Any()))
        {
            Graphics.Log.Print("WARNING: The texture overrides parameter is not supported for version 3 files. Textures will not be overridden.", Diagnostics.LoggingLevel.Intermediate);
        }

        using var wrappedStream = new GorgonStreamWrapper(stream, stream.Position, byteCount, false);
        using var reader = new StreamReader(wrappedStream, Encoding.UTF8, true, 80192, true);
        string jsonString = reader.ReadToEnd();
        return FromJson(Renderer, jsonString);
    }

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    protected override bool OnIsReadable(Stream stream)
    {
        JsonReader reader = null;

        long currentPosition = stream.Position;
        try
        {
            reader = GetJsonReader(stream);
            return IsReadableJObject(reader);
        }
        catch (JsonException)
        {
            return false;
        }
        finally
        {
            stream.Position = currentPosition;
            reader?.Close();
        }
    }

    #endregion
}
