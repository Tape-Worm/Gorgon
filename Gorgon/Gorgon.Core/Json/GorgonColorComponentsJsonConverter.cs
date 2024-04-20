// Gorgon.
// Copyright (C) 2023 Michael Winsor
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
// Created: December 17, 2023 10:59:47 PM
//

using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Json;

/// <summary>
/// A converter used to convert a <see cref="GorgonColor"/> R, G, B and A values to and from a JSON value.
/// </summary>
public class GorgonColorComponentsJsonConverter
    : JsonConverter<GorgonColor>
{
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonColor value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("r", (int)(value.Red * 255.0f));
        writer.WriteNumber("g", (int)(value.Green * 255.0f));
        writer.WriteNumber("b", (int)(value.Blue * 255.0f));
        writer.WriteNumber("a", (int)(value.Alpha * 255.0f));
        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        float r = 0;
        float g = 0;
        float b = 0;
        float a = 0;

        do
        {
            string propName = reader.GetString() ?? string.Empty;

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "r":
                    r = reader.GetInt32() / 255.0f;
                    break;
                case "g":
                    g = reader.GetInt32() / 255.0f;
                    break;
                case "b":
                    b = reader.GetInt32() / 255.0f;
                    break;
                case "a":
                    a = reader.GetInt32() / 255.0f;
                    break;
            }
        } while ((reader.Read()) && (reader.TokenType == JsonTokenType.PropertyName));

        return new GorgonColor(r, g, b, a);
    }
}
