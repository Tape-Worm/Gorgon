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
// Created: August 25, 2018 10:30:45 PM
// 
#endregion

using System;
using DX = SharpDX;
using Newtonsoft.Json;

namespace Gorgon.Editor
{
    /// <summary>
    /// A JSON converter for a SharpDX rectangle type.
    /// </summary>
    internal class JsonSharpDxRectConverter
        : JsonConverter<DX.Rectangle?>
    {
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DX.Rectangle? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("Left");
            writer.WriteValue(value.Value.Left);
            writer.WritePropertyName("Top");
            writer.WriteValue(value.Value.Top);
            writer.WritePropertyName("Right");
            writer.WriteValue(value.Value.Right);
            writer.WritePropertyName("Bottom");
            writer.WriteValue(value.Value.Bottom);
            writer.WriteEnd();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override DX.Rectangle? ReadJson(JsonReader reader, Type objectType, DX.Rectangle? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int left = 0;
            int top = 0;
            int right = 0;
            int bottom = 0;

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToLowerInvariant();

                switch (propName)
                {
                    case "left":
                        left = reader.ReadAsInt32() ?? 0;
                        break;
                    case "top":
                        top = reader.ReadAsInt32() ?? 0;
                        break;
                    case "right":
                        right = reader.ReadAsInt32() ?? 0;
                        break;
                    case "bottom":
                        bottom = reader.ReadAsInt32() ?? 0;
                        break;
                }
            }

            return new DX.Rectangle
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };
        }
    }
}
