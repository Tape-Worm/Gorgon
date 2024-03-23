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

using Gorgon.Core;
using Gorgon.Graphics;
using Newtonsoft.Json;

namespace Gorgon.Json;

/// <summary>
/// A converter used to convert a <see cref="GorgonColor"/> R, G, B and A values to and from a JSON value.
/// </summary>
public class GorgonColorComponentsJsonConverter
    : JsonConverter<GorgonColor?>
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, GorgonColor? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("r");
        writer.WriteValue((int)(value.Value.Red * 255.0f));
        writer.WritePropertyName("g");
        writer.WriteValue((int)(value.Value.Green * 255.0f));
        writer.WritePropertyName("b");
        writer.WriteValue((int)(value.Value.Blue * 255.0f));
        writer.WritePropertyName("a");
        writer.WriteValue((int)(value.Value.Alpha * 255.0f));
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
    public override GorgonColor? ReadJson(JsonReader reader, Type objectType, GorgonColor? existingValue, bool hasExistingValue, JsonSerializer serializer)
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

        float r = 0;
        float g = 0;
        float b = 0;
        float a = 0;

        do
        {
            string propName = reader.Value?.ToString() ?? string.Empty;

            switch (propName)
            {
                case "r":
                    r = (reader.ReadAsInt32() ?? 0) / 255.0f;
                    break;
                case "g":
                    g = (reader.ReadAsInt32() ?? 0) / 255.0f;
                    break;
                case "b":
                    b = (reader.ReadAsInt32() ?? 0) / 255.0f;
                    break;
                case "a":
                    a = (reader.ReadAsInt32() ?? 0) / 255.0f;
                    break;
            }
        } while ((reader.Read()) && (reader.TokenType == JsonToken.PropertyName));

        return new GorgonColor(r, g, b, a);
    }
}
