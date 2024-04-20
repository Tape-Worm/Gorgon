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
// Created: April 18, 2024 12:09:30 AM
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Renderers;
using Gorgon.Graphics;
using System.Numerics;
using Gorgon.Json;

namespace Gorgon.IO;

/// <summary>
/// A converter to convert a <see cref="GorgonPolySpriteVertex"/> object to and from JSON.
/// </summary>
internal class GorgonPolySpriteVertexJsonConverter
    : JsonConverter<GorgonPolySpriteVertex>
{
    // The converter for converting vector 2 values to and from JSON.
    private Vector2JsonConverter _vector2Converter = new();

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonPolySpriteVertex value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("position");
        _vector2Converter.Write(writer, value.Position, options);

        writer.WritePropertyName("texturecoordinate");
        _vector2Converter.Write(writer, value.TextureCoordinate, options);        

        writer.WriteNumber("color", GorgonColor.ToARGB(value.Color));

        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonPolySpriteVertex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Vector2? position = null;
        Vector2? uv = null;
        GorgonColor color = GorgonColors.White;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            string? property = reader.GetString();

            if (string.IsNullOrWhiteSpace(property))
            {
                return null;
            }

            if (!reader.Read())
            {
                break;
            }

            switch(property)
            {
                case "position":
                    position = _vector2Converter.Read(ref reader, typeof(Vector2), options);
                    break;
                case "color":
                    color = GorgonColor.FromABGR(reader.GetInt32());
                    break;
                case "texturecoordinate":
                    uv = _vector2Converter.Read(ref reader, typeof(Vector2), options);
                    break;
            }
        }

        if ((position is null) || (uv is null))
        {
            return null;
        }

        return new GorgonPolySpriteVertex(position.Value, color, uv.Value);
    }
}
