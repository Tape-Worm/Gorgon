#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 11, 2018 7:59:58 PM
// 
#endregion

using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Gorgon.Renderers;

/// <summary>
/// A converter used to convert a texture to and from a string.
/// </summary>
internal class JsonVector3Converter
    : JsonConverter
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var v3 = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(v3.X);
        writer.WritePropertyName("y");
        writer.WriteValue(v3.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(v3.Z);
        writer.WriteEndObject();
    }

    /// <summary>Reads the JSON representation of the object.</summary>
    /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if ((reader.TokenType != JsonToken.StartObject)
            || (!reader.Read()))
        {
            return Vector3.Zero;
        }

        float x = (float)(reader.ReadAsDouble() ?? 0);
        if (!reader.Read())
        {
            return new Vector3(x, 0, 0);
        }

        float y = (float)(reader.ReadAsDouble() ?? 0);

        if (!reader.Read())
        {
            return new Vector3(x, y, 0);
        }

        float z = (float)(reader.ReadAsDouble() ?? 0);
        reader.Read();

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType) => (objectType == typeof(Vector3));
}
