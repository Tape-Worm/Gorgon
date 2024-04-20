
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

using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Json;

/// <summary>
/// A converter used to convert a <see cref="GorgonBox"/> to and from JSON values.
/// </summary>
public class GorgonBoxJsonConverter
    : JsonConverter<GorgonBox>
{
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonBox value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("width", value.Width);
        writer.WriteNumber("height", value.Height);
        writer.WriteNumber("depth", value.Depth);
        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
        int z = 0;
        int w = 0;
        int h = 0;
        int d = 0;

        do
        {
            string propName = reader.GetString() ?? string.Empty;

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "x":
                    x = reader.GetInt32();
                    break;
                case "y":
                    y = reader.GetInt32();
                    break;
                case "z":
                    z = reader.GetInt32();
                    break;
                case "width":
                    w = reader.GetInt32();
                    break;
                case "height":
                    h = reader.GetInt32();
                    break;
                case "depth":
                    d = reader.GetInt32();
                    break;
            }
        } while ((reader.Read()) && (reader.TokenType == JsonTokenType.PropertyName));

        return new GorgonBox
        {
            X = x,
            Y = y,
            Z = z,
            Width = w,
            Height = h,
            Depth = d
        };
    }
}
