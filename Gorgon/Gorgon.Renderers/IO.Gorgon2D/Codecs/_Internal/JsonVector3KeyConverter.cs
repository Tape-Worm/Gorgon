
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
// Created: August 25, 2018 10:30:45 PM
// 

using System.Numerics;
using Gorgon.Animation;
using Newtonsoft.Json;

namespace Gorgon.IO;

/// <summary>
/// A JSON converter for a <see cref="GorgonKeyVector3"/>
/// </summary>
class JsonVector3KeyConverter
    : JsonConverter<GorgonKeyVector3>
{
    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, GorgonKeyVector3 value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("time");
        writer.WriteValue(value.Time);
        writer.WritePropertyName("x");
        writer.WriteValue(value.Value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Value.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.Value.Z);
        writer.WriteEnd();
    }

    /// <summary>Reads the JSON representation of the object.</summary>
    /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override GorgonKeyVector3 ReadJson(JsonReader reader, Type objectType, GorgonKeyVector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float time = 0;
        float x = 0;
        float y = 0;
        float z = 0;

        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            string propName = reader.Value.ToString().ToUpperInvariant();

            switch (propName)
            {
                case "TIME":
                    time = (float?)reader.ReadAsDecimal() ?? 0;
                    break;
                case "X":
                    x = (float?)reader.ReadAsDecimal() ?? 0;
                    break;
                case "Y":
                    y = (float?)reader.ReadAsDecimal() ?? 0;
                    break;
                case "Z":
                    z = (float?)reader.ReadAsDecimal() ?? 0;
                    break;
            }
        }

        return new GorgonKeyVector3(time, new Vector3(x, y, z));
    }
}
