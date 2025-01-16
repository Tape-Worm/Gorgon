
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

using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Animation;
using Gorgon.Graphics;

namespace Gorgon.IO;

/// <summary>
/// A JSON converter for a <see cref="GorgonKeyGorgonColor"/>
/// </summary>
class JsonGorgonColorKeyConverter
    : JsonConverter<GorgonKeyGorgonColor>
{
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonKeyGorgonColor value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("time", value.Time);
        writer.WriteNumber("argb", GorgonColor.ToARGB(value.Value));
        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonKeyGorgonColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int argb = 0;
        float time = 0;

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndObject))
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string propName = reader.GetString().ToUpperInvariant();

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "TIME":
                    time = reader.GetSingle();
                    break;
                case "ARGB":
                    argb = reader.GetInt32();
                    break;
            }
        }

        return new GorgonKeyGorgonColor(time, GorgonColor.FromARGB(argb));
    }
}
