
// 
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
// Created: August 11, 2018 7:56:31 PM
// 

using Gorgon.Core;
using Gorgon.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gorgon.Json;

/// <summary>
/// A converter used to convert a <see cref="GorgonRectangle"/> to and from JSON values.
/// </summary>
public class GorgonRectangleJsonConverter
    : JsonConverter<GorgonRectangle>
{
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonRectangle value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("width", value.Width);
        writer.WriteNumber("height", value.Height);
        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonRectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ((reader.TokenType == JsonTokenType.Null)
            || (!reader.Read()))
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new GorgonException(GorgonResult.CannotRead);
        }

        int x = 0;
        int y = 0;
        int w = 0;
        int h = 0;
        int? r = null;
        int? b = null;

        do
        {
            string propName = reader.GetString() ?? string.Empty;

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "left":
                case "l":
                case "x":
                    x = reader.GetInt32();
                    break;
                case "top":
                case "t":
                case "y":
                    y = reader.GetInt32();
                    break;
                case "right":
                case "r":
                    r = reader.GetInt32();
                    break;
                case "width":
                case "w":
                    w = reader.GetInt32();
                    break;
                case "bottom":
                case "b":
                    b = reader.GetInt32();
                    break;
                case "height":
                case "h":
                    h = reader.GetInt32();
                    break;
            }
        }
        while ((reader.Read()) && (reader.TokenType == JsonTokenType.PropertyName));

        if (r is not null)
        {
            w = r.Value - x;
        }

        if (b is not null)
        {
            h = b.Value - y;
        }

        return new GorgonRectangle
        {
            X = x,
            Y = y,
            Width = w,
            Height = h
        };
    }
}
