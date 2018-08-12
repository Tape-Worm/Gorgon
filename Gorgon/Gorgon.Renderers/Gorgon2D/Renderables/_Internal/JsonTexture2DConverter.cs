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
// Created: August 11, 2018 7:56:39 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A converter used to convert a texture to and from a string.
    /// </summary>
    internal class JsonTexture2DConverter
        : JsonConverter<GorgonTexture2DView>
    {
        // The graphics object to use for resource look up.
        private readonly GorgonGraphics _graphics;
        
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, GorgonTexture2DView value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(value.Texture.Name);
            writer.WritePropertyName("arrayStart");
            writer.WriteValue(value.ArrayIndex);
            writer.WritePropertyName("arrayCount");
            writer.WriteValue(value.ArrayCount);
            writer.WritePropertyName("mipStart");
            writer.WriteValue(value.MipSlice);
            writer.WritePropertyName("mipCount");
            writer.WriteValue(value.MipCount);
            writer.WritePropertyName("format");
            writer.WriteValue(value.Format);
            writer.WriteEndObject();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override GorgonTexture2DView ReadJson(JsonReader reader, Type objectType, GorgonTexture2DView existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if ((reader.TokenType != JsonToken.StartObject)
                || (_graphics == null)
                || (!reader.Read()))
            {
                return null;
            }
            
            string name = reader.ReadAsString();

            GorgonTexture2D texture = _graphics?.LocateResourcesByName<GorgonTexture2D>(name).FirstOrDefault();

            if (texture == null)
            {
                return null;
            }

            if (!reader.Read())
            {
                return null;
            }

            int arrayStart = reader.ReadAsInt32() ?? 0;
            if (!reader.Read())
            {
                return null;
            }
            int arrayCount = reader.ReadAsInt32() ?? 1;
            if (!reader.Read())
            {
                return null;
            }

            int mipStart = reader.ReadAsInt32() ?? 0;
            if (!reader.Read())
            {
                return null;
            }

            int mipCount = reader.ReadAsInt32() ?? 1;
            if (!reader.Read())
            {
                return null;
            }

            var format = (BufferFormat)(reader.ReadAsInt32() ?? (int)BufferFormat.R8G8B8A8_UNorm);
            reader.Read();

            return texture.GetShaderResourceView(format, mipStart, mipCount, arrayStart, arrayCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTexture2DConverter"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used for resource lookup.</param>
        public JsonTexture2DConverter(GorgonGraphics graphics)
        {
            _graphics = graphics;
        }
    }
}
