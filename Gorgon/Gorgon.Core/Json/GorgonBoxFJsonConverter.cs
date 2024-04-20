
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
/// A converter used to convert a <see cref="GorgonBoxF"/> to and from JSON values.
/// </summary>
public class GorgonBoxFJsonConverter
    : JsonConverter<GorgonBoxF>
{
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonBoxF value, JsonSerializerOptions options)
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
    public override GorgonBoxF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        float x = 0;
        float y = 0;
        float z = 0;
        float w = 0;
        float h = 0;
        float d = 0;

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
                    x = reader.GetSingle();
                    break;
                case "y":
                    y = reader.GetSingle();
                    break;
                case "z":
                    z = reader.GetSingle();
                    break;
                case "width":
                    w = reader.GetSingle();
                    break;
                case "height":
                    h = reader.GetSingle();
                    break;
                case "depth":
                    d = reader.GetSingle();
                    break;
            }
        } while ((reader.Read()) && (reader.TokenType == JsonTokenType.PropertyName));

        return new GorgonBoxF
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
