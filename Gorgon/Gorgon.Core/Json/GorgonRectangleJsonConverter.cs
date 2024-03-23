
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
/// A converter used to convert a <see cref="GorgonRectangle"/> to and from JSON values.
/// </summary>
public class GorgonRectangleJsonConverter
    : JsonConverter<GorgonRectangle?>
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, GorgonRectangle? value, JsonSerializer serializer)
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
        writer.WritePropertyName("width");
        writer.WriteValue(value.Value.Width);
        writer.WritePropertyName("height");
        writer.WriteValue(value.Value.Height);
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
    public override GorgonRectangle? ReadJson(JsonReader reader, Type objectType, GorgonRectangle? existingValue, bool hasExistingValue, JsonSerializer serializer)
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

        int x = 0;
        int y = 0;
        int w = 0;
        int h = 0;
        int? r = null;
        int? b = null;

        do
        {
            string propName = reader.Value?.ToString() ?? string.Empty;

            switch (propName)
            {
                case "left":
                case "l":
                case "x":
                    x = reader.ReadAsInt32() ?? 0;
                    break;
                case "top":
                case "t":
                case "y":
                    y = reader.ReadAsInt32() ?? 0;
                    break;
                case "right":
                case "r":
                    r = reader.ReadAsInt32() ?? 0;
                    break;
                case "width":
                case "w":
                    w = reader.ReadAsInt32() ?? 0;
                    break;
                case "bottom":
                case "b":
                    b = reader.ReadAsInt32() ?? 0;
                    break;
                case "height":
                case "h":
                    h = reader.ReadAsInt32() ?? 0;
                    break;
            }
        }
        while ((reader.Read()) && (reader.TokenType == JsonToken.PropertyName));

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
