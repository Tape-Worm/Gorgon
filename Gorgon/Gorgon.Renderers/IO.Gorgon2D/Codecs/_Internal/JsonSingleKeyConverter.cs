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
using Gorgon.Animation;
using Newtonsoft.Json;

namespace Gorgon.IO
{
    /// <summary>
    /// A JSON converter for a <see cref="GorgonKeySingle"/>
    /// </summary>
    class JsonSingleKeyConverter
        : JsonConverter<GorgonKeySingle>
    {
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, GorgonKeySingle value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("time");
            writer.WriteValue(value.Time);
            writer.WritePropertyName("value");
            writer.WriteValue(value.Value);
            writer.WriteEnd();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override GorgonKeySingle ReadJson(JsonReader reader, Type objectType, GorgonKeySingle existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float time = 0;
            float value = 0;

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
                    case "VALUE":
                        value = (float?)reader.ReadAsDecimal() ?? 0;
                        break;
                }
            }

            return new GorgonKeySingle(time, value);
        }
    }
}
