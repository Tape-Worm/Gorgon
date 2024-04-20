
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
// Created: August 11, 2018 7:56:39 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gorgon.IO;

/// <summary>
/// A converter used to convert a texture to and from a string
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonTexture2DConverter"/> class
/// </remarks>
/// <param name="graphics">The graphics interface used for resource lookup.</param>
/// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
internal class JsonTexture2DConverter(GorgonGraphics graphics, GorgonTexture2DView overrideTexture)
        : JsonConverter<GorgonTexture2DView>
{
    // The graphics object to use for resource look up.
    private readonly GorgonGraphics _graphics = graphics;
    // The override texture.
    private readonly GorgonTexture2DView _override = overrideTexture;
    // The list of properties for the type.
    private readonly HashSet<string> _propNames = new(StringComparer.Ordinal)
                                                  {
                                                      "name",
                                                      "texWidth",
                                                      "texHeight",
                                                      "texFormat",
                                                      "texArrayCount",
                                                      "texMipCount",
                                                      "arrayStart",
                                                      "arrayCount",
                                                      "mipStart",
                                                      "mipCount",
                                                      "format"
                                                  };

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonTexture2DView value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (string propName in _propNames)
        {
            switch (propName)
            {
                case "name":
                    writer.WriteString(propName, value.Texture.Name);
                    break;
                case "texWidth":
                    writer.WriteNumber(propName, value.Texture.Width);
                    break;
                case "texHeight":
                    writer.WriteNumber(propName, value.Texture.Height);
                    break;
                case "texFormat":
                    writer.WriteNumber(propName, (int)value.Texture.Format);
                    break;
                case "texArrayCount":
                    writer.WriteNumber(propName, value.Texture.ArrayCount);
                    break;
                case "texMipCount":
                    writer.WriteNumber(propName, value.Texture.MipLevels);
                    break;
                case "arrayStart":
                    writer.WriteNumber(propName, value.ArrayIndex);
                    break;
                case "arrayCount":
                    writer.WriteNumber(propName, value.ArrayCount);
                    break;
                case "mipStart":
                    writer.WriteNumber(propName, value.MipSlice);
                    break;
                case "mipCount":
                    writer.WriteNumber(propName, value.MipCount);
                    break;
                case "format":
                    writer.WriteNumber(propName, (int)value.Format);
                    break;
            }
        }
        writer.WriteEndObject();
    }

    /// <summary>
    /// Function to read a texture from the JSON data.
    /// </summary>
    /// <param name="reader">The reader used to parse the JSON data.</param>
    /// <param name="overrides">[Optional] A list of textures that can be used as a substitute for the texture in the JSON data.</param>
    /// <returns>A tuple containing the texture, and the name of the texture.</returns>
    public (GorgonTexture2DView Texture, string TextureName) ReadTexture(ref Utf8JsonReader reader, IEnumerable<GorgonTexture2DView> overrides = null)
    {
        string textureName = string.Empty;

        if ((reader.TokenType != JsonTokenType.StartObject)
            || (_graphics is null))
        {
            return (null, null);
        }

        int? texWidth = null;
        int? texHeight = null;
        int? texMipCount = null;
        int? texArrayCount = null;
        BufferFormat? texFormat = null;
        int? viewMipStart = null;
        int? viewMipCount = null;
        int? viewArrayStart = null;
        int? viewArrayCount = null;
        BufferFormat? viewFormat = null;

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndObject))
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string propName = reader.GetString();

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "name":
                    textureName = reader.GetString();
                    break;
                case "texWidth":
                    texWidth = reader.GetInt32();
                    break;
                case "texHeight":
                    texHeight = reader.GetInt32();
                    break;
                case "texFormat":
                    texFormat = (BufferFormat)reader.GetInt32();
                    break;
                case "texArrayCount":
                    texArrayCount = reader.GetInt32();
                    break;
                case "texMipCount":
                    texMipCount = reader.GetInt32();
                    break;
                case "arrayStart":
                    viewArrayStart = reader.GetInt32();
                    break;
                case "arrayCount":
                    viewArrayCount = reader.GetInt32();
                    break;
                case "mipStart":
                    viewMipStart = reader.GetInt32();
                    break;
                case "mipCount":
                    viewMipCount = reader.GetInt32();
                    break;
                case "format":
                    viewFormat = (BufferFormat)reader.GetInt32();
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(textureName))
        {
            _graphics?.Log.Print("Attempted to load a texture from JSON data, but texture was not in memory and name is unknown, or JSON texture data is not in the correct format.",
                                 LoggingLevel.Verbose);
            return (null, null);
        }

        // Check overrides list if one was supplied.
        if (overrides is not null)
        {
            GorgonTexture2DView overrideMatch = overrides.FirstOrDefault(item => string.Equals(item.Texture.Name, textureName, StringComparison.OrdinalIgnoreCase));

            if (overrideMatch is not null)
            {
                return (overrideMatch, null);
            }
        }

        if ((texWidth is null)
            || (texHeight is null)
            || (texFormat is null)
            || (texArrayCount is null)
            || (texMipCount is null)
            || (viewArrayStart is null)
            || (viewArrayCount is null)
            || (viewMipStart is null)
            || (viewMipCount is null)
            || (viewFormat is null))
        {
            return (null, textureName);
        }

        if (_override is not null)
        {
            return (_override, textureName);
        }

        GorgonTexture2D texture = _graphics?.Locate2DTextureByName(textureName, texWidth ?? 1, texHeight ?? 1, texFormat ?? BufferFormat.R8G8B8A8_UNorm, texArrayCount ?? 1, texMipCount ?? 1);

        return (texture?.GetShaderResourceView(viewFormat.Value, viewMipStart.Value, viewMipCount.Value, viewArrayStart.Value, viewArrayCount.Value), textureName);
    }

    /// <inheritdoc/>
    public override GorgonTexture2DView Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ReadTexture(ref reader).Texture;
}
