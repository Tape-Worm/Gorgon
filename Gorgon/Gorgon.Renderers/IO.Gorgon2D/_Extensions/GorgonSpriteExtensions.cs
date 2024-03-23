
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

using Gorgon.IO;
using Gorgon.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        JsonSerializer serializer = new()
        {
            CheckAdditionalContent = false,
            Formatting = prettyFormat ? Formatting.Indented : Formatting.None
        };

        serializer.Converters.Add(new Vector2JsonConverter());
        serializer.Converters.Add(new Vector3JsonConverter());
        serializer.Converters.Add(new GorgonColorJsonConverter());
        serializer.Converters.Add(new GorgonRectangleFJsonConverter());
        serializer.Converters.Add(new JsonSamplerConverter(null));
        serializer.Converters.Add(new JsonTexture2DConverter(null, null));

        JObject jsonObj = JObject.FromObject(sprite, serializer);
        JToken firstProp = jsonObj.First;
        firstProp.AddBeforeSelf(new JProperty(JsonHeaderProp, GorgonSpriteCodecCommon.CurrentFileHeader));
        firstProp.AddBeforeSelf(new JProperty(JsonVersionProp, GorgonSpriteCodecCommon.CurrentVersion.ToString(2)));

        return jsonObj.ToString(prettyFormat ? Formatting.Indented : Formatting.None);
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

        JsonSerializer serializer = new()
        {
            CheckAdditionalContent = false,
            Formatting = prettyFormat ? Formatting.Indented : Formatting.None
        };

        serializer.Converters.Add(new Vector2JsonConverter());
        serializer.Converters.Add(new Vector3JsonConverter());
        serializer.Converters.Add(new GorgonColorJsonConverter());
        serializer.Converters.Add(new GorgonRectangleFJsonConverter());
        serializer.Converters.Add(new JsonSamplerConverter(null));
        serializer.Converters.Add(new JsonTexture2DConverter(null, null));

        JObject jsonObj = JObject.FromObject(sprite, serializer);
        JToken firstProp = jsonObj.First;
        firstProp.AddBeforeSelf(new JProperty(JsonHeaderProp, GorgonPolySpriteCodecCommon.CurrentFileHeader));
        firstProp.AddBeforeSelf(new JProperty(JsonVersionProp, GorgonPolySpriteCodecCommon.CurrentVersion.ToString(2)));

        return jsonObj.ToString(prettyFormat ? Formatting.Indented : Formatting.None);
    }
}
