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
using Gorgon.Graphics.Core;
using Newtonsoft.Json;
using DX = SharpDX;

namespace Gorgon.IO
{
    /// <summary>
    /// A JSON converter for a <see cref="GorgonKeyTexture2D"/>
    /// </summary>
    class JsonTextureKeyConverter
        : JsonConverter<GorgonKeyTexture2D>
    {
        // The texture converter to serialize the texture.
        private readonly JsonTexture2DConverter _textureConverter;
        // The texture converter to serialize a rectangle.
        private readonly JsonRectangleFConverter _rectConverter = new JsonRectangleFConverter();

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, GorgonKeyTexture2D value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("time");
            writer.WriteValue(value.Time);
            writer.WritePropertyName("texture");
            _textureConverter.WriteJson(writer, value.Value, serializer);
            writer.WritePropertyName("arrayindex");
            writer.WriteValue(value.TextureArrayIndex);
            writer.WritePropertyName("uv");
            _rectConverter.WriteJson(writer, value.TextureCoordinates, serializer);
            writer.WriteEnd();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override GorgonKeyTexture2D ReadJson(JsonReader reader, Type objectType, GorgonKeyTexture2D existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float time = 0;
            int arrayIndex = 0;
            DX.RectangleF uv = DX.RectangleF.Empty;
            GorgonTexture2DView texture = null;
            string textureName = string.Empty;

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
                        time = (float)(reader.ReadAsDecimal() ?? 0);
                        break;
                    case "ARRAYINDEX":
                        arrayIndex = reader.ReadAsInt32() ?? 0;
                        break;
                    case "UV":
                        reader.Read();
                        uv = _rectConverter.ReadJson(reader, typeof(DX.RectangleF), DX.RectangleF.Empty, false, serializer);
                        break;
                    case "TEXTURE":
                        reader.Read();
                        texture = _textureConverter.ReadTexture(reader, out textureName);

                        if ((texture is null) && (string.IsNullOrWhiteSpace(textureName)))
                        {
                            return null;
                        }

                        break;
                }
            }

            return texture != null ? new GorgonKeyTexture2D(time, texture, uv, arrayIndex) : new GorgonKeyTexture2D(time, textureName, uv, arrayIndex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextureKeyConverter"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use for texture lookup.</param>
        public JsonTextureKeyConverter(GorgonGraphics graphics) => _textureConverter = new JsonTexture2DConverter(graphics, null);
    }
}
