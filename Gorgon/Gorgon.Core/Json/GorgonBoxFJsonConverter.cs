﻿
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
using Newtonsoft.Json;

namespace Gorgon.Json;

/// <summary>
/// A converter used to convert a <see cref="GorgonBoxF"/> to and from JSON values.
/// </summary>
public class GorgonBoxFJsonConverter
    : JsonConverter<GorgonBoxF?>
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, GorgonBoxF? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.Value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Value.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.Value.Z);
        writer.WritePropertyName("width");
        writer.WriteValue(value.Value.Width);
        writer.WritePropertyName("height");
        writer.WriteValue(value.Value.Height);
        writer.WritePropertyName("depth");
        writer.WriteValue(value.Value.Depth);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override GorgonBoxF? ReadJson(JsonReader reader, Type objectType, GorgonBoxF? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if ((reader.TokenType == JsonToken.Null)
            || (!reader.Read()))
        {
            return hasExistingValue ? existingValue : null;
        }

        if (reader.TokenType != JsonToken.PropertyName)
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
            string propName = reader.Value?.ToString() ?? string.Empty;

            switch (propName)
            {
                case "x":
                    x = (float)(reader.ReadAsDouble() ?? 0);
                    break;
                case "y":
                    y = (float)(reader.ReadAsDouble() ?? 0);
                    break;
                case "z":
                    z = (float)(reader.ReadAsDouble() ?? 0);
                    break;
                case "width":
                    w = (float)(reader.ReadAsDouble() ?? 0);
                    break;
                case "height":
                    h = (float)(reader.ReadAsDouble() ?? 0);
                    break;
                case "depth":
                    d = (float)(reader.ReadAsDouble() ?? 0);
                    break;
            }
        } while ((reader.Read()) && (reader.TokenType == JsonToken.PropertyName));

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