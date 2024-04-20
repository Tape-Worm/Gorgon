
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
// Created: August 13, 2018 1:01:46 PM
// 

using System.Text.Json;
using System.Text.Json.Nodes;
using Gorgon.IO;
using Gorgon.Json;

namespace Gorgon.Renderers;

/// <summary>
/// Extension methods for the <see cref="GorgonSprite"/> object
/// </summary>
public static class GorgonSpriteExtensions
{
    /// <summary>
    /// The property name for the header value.
    /// </summary>
    internal const string JsonHeaderProp = "header";
    /// <summary>
    /// The property name for the header value.
    /// </summary>
    internal const string JsonVersionProp = "version";

    /// <summary>
    /// Function to convert a sprite into a JSON formatted string.
    /// </summary>
    /// <param name="sprite">The sprite to serialize.</param>
    /// <param name="prettyFormat"><b>true</b> to employ pretty formatting on the JSON string (increases size), <b>false</b> to compact the string.</param>
    /// <returns>The sprite data as a JSON formatted string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
    /// <seealso cref="GorgonSprite"/>
    public static string ToJson(this GorgonSprite sprite, bool prettyFormat = false)
    {
        if (sprite is null)
        {
            throw new ArgumentNullException(nameof(sprite));
        }

        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new Vector2JsonConverter(),
                new Vector3JsonConverter(),
                new GorgonColorJsonConverter(),
                new GorgonRectangleFJsonConverter(),
                new GorgonRangeFloatJsonConverter(),
                new JsonSamplerConverter(null),
                new JsonTexture2DConverter(null, null)
            },
            WriteIndented = prettyFormat
        };

        JsonObject? node = JsonSerializer.SerializeToNode(sprite, options).AsObject();

        if (node is null)
        {
            return string.Empty;
        }

        node[JsonHeaderProp] = GorgonSpriteCodecCommon.CurrentFileHeader;
        node[JsonVersionProp] = GorgonSpriteCodecCommon.CurrentVersion.ToString(2);

        return node.ToJsonString(options);
    }

    /// <summary>
    /// Function to convert a polygonal sprite into a JSON formatted string.
    /// </summary>
    /// <param name="sprite">The polygonal sprite to serialize.</param>
    /// <param name="prettyFormat"><b>true</b> to employ pretty formatting on the JSON string (increases size), <b>false</b> to compact the string.</param>
    /// <returns>The polygonal sprite data as a JSON formatted string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
    /// <seealso cref="GorgonPolySprite"/>
    public static string ToJson(this GorgonPolySprite sprite, bool prettyFormat = false)
    {
        if (sprite is null)
        {
            throw new ArgumentNullException(nameof(sprite));
        }

        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new Vector2JsonConverter(),
                new Vector3JsonConverter(),
                new GorgonColorJsonConverter(),
                new GorgonRectangleFJsonConverter(),
                new GorgonRangeFloatJsonConverter(),
                new JsonSamplerConverter(null),
                new JsonTexture2DConverter(null, null),
                new GorgonPolySpriteVertexJsonConverter()
            },
            WriteIndented = prettyFormat
        };

        JsonObject? node = JsonSerializer.SerializeToNode(sprite, options).AsObject();

        if (node is null)
        {
            return string.Empty;
        }

        node[JsonHeaderProp] = GorgonPolySpriteCodecCommon.CurrentFileHeader;
        node[JsonVersionProp] = GorgonPolySpriteCodecCommon.CurrentVersion.ToString(2);

        return node.ToJsonString(options);
    }
}
