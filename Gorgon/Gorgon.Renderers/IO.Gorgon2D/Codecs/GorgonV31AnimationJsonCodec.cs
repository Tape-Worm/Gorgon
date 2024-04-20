
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
using System.Text.Json;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

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

    /// <summary>Function to retrieve the names of the associated textures.</summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream)
    {
        List<string> result = [];
        string jsonString = stream.ReadString();

        using JsonDocument document = JsonDocument.Parse(jsonString);

        if ((!IsReadableJsonData(document)) || (!document.RootElement.TryGetProperty("textures", out JsonElement textureTrack)))
        {
            return result;
        }

        foreach (JsonElement track in textureTrack.EnumerateArray())
        {
            if (!track.TryGetProperty("keyframes", out JsonElement keyFrames))
            {
                continue;
            }

            foreach (JsonElement keyFrame in keyFrames.EnumerateArray())
            {
                if ((!keyFrame.TryGetProperty("texture", out JsonElement texture))
                    || (!texture.TryGetProperty("name", out JsonElement textureName)))
                {
                    continue;
                }

                string? name = textureName.GetString();

                if ((!string.IsNullOrWhiteSpace(name)) && (!result.Contains(name)))
                {
                    result.Add(name);
                }
            }
        }

        return result;
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
