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
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Newtonsoft.Json;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A converter used to convert a texture sampler to and from a string.
    /// </summary>
    internal class JsonSamplerConverter
        : JsonConverter<GorgonSamplerState>
    {
        // The graphics object to use for resource look up.
        private readonly GorgonGraphics _graphics;
        
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, GorgonSamplerState value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("borderColor");
            writer.WriteValue(value.BorderColor.ToARGB());
            writer.WritePropertyName("compareFunc");
            writer.WriteValue(value.ComparisonFunction);
            writer.WritePropertyName("filter");
            writer.WriteValue(value.Filter);
            writer.WritePropertyName("maxAnisotropy");
            writer.WriteValue(value.MaxAnisotropy);
            writer.WritePropertyName("maxLod");
            writer.WriteValue(value.MaximumLevelOfDetail);
            writer.WritePropertyName("minLod");
            writer.WriteValue(value.MinimumLevelOfDetail);
            writer.WritePropertyName("mipLodBias");
            writer.WriteValue(value.MipLevelOfDetailBias);
            writer.WritePropertyName("wrapU");
            writer.WriteValue(value.WrapU);
            writer.WritePropertyName("wrapV");
            writer.WriteValue(value.WrapV);
            writer.WritePropertyName("wrapW");
            writer.WriteValue(value.WrapW);
            writer.WriteEndObject();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override GorgonSamplerState ReadJson(JsonReader reader, Type objectType, GorgonSamplerState existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if ((reader.TokenType != JsonToken.StartObject)
                || (_graphics == null)
                || (!reader.Read()))
            {
                return null;
            }

            var borderColor = new GorgonColor(reader.ReadAsInt32() ?? -1);
            if (!reader.Read())
            {
                return null;
            }
            var compareFunction = (Comparison)(reader.ReadAsInt32() ?? (int)Comparison.Always);
            if (!reader.Read())
            {
                return null;
            }
            var filter = (SampleFilter)(reader.ReadAsInt32() ?? (int)SampleFilter.MinMagMipLinear);
            if (!reader.Read())
            {
                return null;
            }
            int maxAnisotropy = reader.ReadAsInt32() ?? 0;
            if (!reader.Read())
            {
                return null;
            }
            var maxLod = (float)(reader.ReadAsDouble() ?? float.MaxValue);
            if (!reader.Read())
            {
                return null;
            }
            var minLod = (float)(reader.ReadAsDouble() ?? float.MinValue);
            if (!reader.Read())
            {
                return null;
            }
            var mipLodBias = (float)(reader.ReadAsDouble() ?? 0);
            if (!reader.Read())
            {
                return null;
            }
            var wrapU = (TextureWrap)(reader.ReadAsInt32() ?? (int)TextureWrap.Clamp);
            if (!reader.Read())
            {
                return null;
            }
            var wrapV = (TextureWrap)(reader.ReadAsInt32() ?? (int)TextureWrap.Clamp);
            if (!reader.Read())
            {
                return null;
            }
            var wrapW = (TextureWrap)(reader.ReadAsInt32() ?? (int)TextureWrap.Clamp);
            if (!reader.Read())
            {
                return null;
            }

            var builder = new GorgonSamplerStateBuilder(_graphics);
            return builder.Wrapping(wrapU, wrapV, wrapW, borderColor)
                          .MaxAnisotropy(maxAnisotropy)
                          .ComparisonFunction(compareFunction)
                          .Filter(filter)
                          .MipLevelOfDetail(minLod, maxLod, mipLodBias)
                          .Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSamplerConverter"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used for resource look up.</param>
        public JsonSamplerConverter(GorgonGraphics graphics)
        {
            _graphics = graphics;
        }
    }
}
