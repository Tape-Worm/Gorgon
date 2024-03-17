
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
// Created: August 11, 2018 7:56:31 PM
// 


using Newtonsoft.Json;
using DX = SharpDX;

namespace Gorgon.IO;

/// <summary>
/// A converter used to convert a rectangle to and from a string
/// </summary>
internal class JsonRectangleFConverter
    : JsonConverter<DX.RectangleF>
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, DX.RectangleF value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("l");
        writer.WriteValue(value.Left);
        writer.WritePropertyName("t");
        writer.WriteValue(value.Top);
        writer.WritePropertyName("r");
        writer.WriteValue(value.Right);
        writer.WritePropertyName("b");
        writer.WriteValue(value.Bottom);
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
    public override DX.RectangleF ReadJson(JsonReader reader, Type objectType, DX.RectangleF existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if ((reader.TokenType != JsonToken.StartObject)
            || (!reader.Read()))
        {
            return DX.RectangleF.Empty;
        }

        float l = (float)(reader.ReadAsDouble() ?? 0);

        if (!reader.Read())
        {
            return new DX.RectangleF(l, 0, 0, 0);
        }

        float t = (float)(reader.ReadAsDouble() ?? 0);

        if (!reader.Read())
        {
            return new DX.RectangleF(l, t, 0, 0);
        }

        float r = (float)(reader.ReadAsDouble() ?? 0);

        if (!reader.Read())
        {
            return new DX.RectangleF
            {
                Left = l,
                Top = t,
                Right = r
            };
        }

        float b = (float)(reader.ReadAsDouble() ?? 0);

        DX.RectangleF result = new()
        {
            Left = l,
            Top = t,
            Right = r,
            Bottom = b
        };
        reader.Read();

        return result;
    }
}
