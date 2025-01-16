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
// Created: August 11, 2018 3:43:13 PM
// 

using System.Text;
using System.Text.Json;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Json;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// A codec that can read and write a JSON formatted version of Gorgon v3 sprite data
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV3SpriteJsonCodec"/> class
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
public class GorgonV3SpriteJsonCodec(Gorgon2D renderer)
        : GorgonSpriteCodecCommon(renderer, Resources.GOR2DIO_V3_JSON_CODEC, Resources.GOR2DIO_V3_JSON_CODEC_DESCRIPTION)
{
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

    /// <summary>
    /// Function to determine if the jason object has the data we need.
    /// </summary>
    /// <param name="document">The JSON document to evaluate.</param>
    /// <returns><b>true</b> if the data is for a sprite, <b>false</b> if not.</returns>
    private bool IsReadableJsonData(JsonDocument document)
    {
        if ((!document.RootElement.TryGetProperty(GorgonSpriteExtensions.JsonHeaderProp, out JsonElement headerElement))
            || (!document.RootElement.TryGetProperty(GorgonSpriteExtensions.JsonVersionProp, out JsonElement versionElement)))
        {
            return false;
        }

        return (headerElement.TryGetUInt64(out ulong header))
            && (Version.TryParse(versionElement.GetString(), out Version? version))
            && (header == CurrentFileHeader)
            && (version.Equals(Version));
    }

    /// <summary>
    /// Function to read the sprite data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the sprite.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <returns>A new <see cref="GorgonSprite"/>.</returns>
    protected override GorgonSprite OnReadFromStream(Stream stream, int byteCount, GorgonTexture2DView overrideTexture)
    {
        using GorgonSubStream wrappedStream = new(stream, stream.Position, byteCount, false);
        using StreamReader reader = new(wrappedStream, Encoding.UTF8, true, 80192, true);
        string jsonString = reader.ReadToEnd();
        return FromJson(Renderer, jsonString, overrideTexture);
    }

    /// <summary>
    /// Function to save the sprite data to a stream.
    /// </summary>
    /// <param name="sprite">The sprite to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the sprite.</param>
    protected override void OnSaveToStream(GorgonSprite sprite, Stream stream)
    {
        using StreamWriter writer = new(stream, Encoding.UTF8, 1024, true);
        writer.Write(sprite.ToJson());
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

    /// <summary>
    /// Function to retrieve the name of the associated texture.
    /// </summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
    /// <exception cref="GorgonException">Thrown if the string is not a valid sprite JSON.</exception>
    protected override string OnGetAssociatedTextureName(Stream stream)
    {
        if (stream.Length < 4)
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_SPRITE);
        }

        string json = stream.ReadString();

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using JsonDocument document = JsonDocument.Parse(json);

        if (!IsReadableJsonData(document))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_SPRITE);
        }

        if (!document.RootElement.TryGetProperty("Texture", out JsonElement textureElement))
        {
            return null;
        }

        if (!textureElement.TryGetProperty("name", out JsonElement textureNameElement))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_SPRITE);
        }

        return textureNameElement.GetString();
    }

    /// <summary>
    /// Function to convert a JSON string into a sprite object.
    /// </summary>
    /// <param name="renderer">The renderer for the sprite.</param>    
    /// <param name="json">The JSON string containing the sprite data.</param>
    /// <param name="overrideTexture">[Optional] The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <returns>A new <see cref="GorgonSprite"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the JSON string does not contain sprite data, or there is a version mismatch.</exception>
    public static GorgonSprite FromJson(Gorgon2D renderer, string json, GorgonTexture2DView? overrideTexture = null)
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

        // Set up serialization so we can convert our more complicated structures.
        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new Vector2JsonConverter(),
                new Vector3JsonConverter(),
                new GorgonColorJsonConverter(),
                new GorgonRectangleFJsonConverter(),
                new GorgonRangeFloatJsonConverter(),
                new JsonSamplerConverter(renderer.Graphics),
                new JsonTexture2DConverter(renderer.Graphics, overrideTexture)
            }
        };

        using JsonDocument document = JsonDocument.Parse(json);

        if ((!document.RootElement.TryGetProperty(GorgonSpriteExtensions.JsonHeaderProp, out JsonElement headerElement))
            || (!document.RootElement.TryGetProperty(GorgonSpriteExtensions.JsonVersionProp, out JsonElement versionElement))
            || (!headerElement.TryGetUInt64(out ulong id))
            || (id != CurrentFileHeader))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_SPRITE);
        }

        if ((!Version.TryParse(versionElement.GetString(), out Version? version)) || (!CurrentVersion.Equals(version)))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_SPRITE_VERSION_MISMATCH, CurrentVersion, version));
        }

        return document.Deserialize<GorgonSprite>(options);
    }
}
