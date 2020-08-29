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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Newtonsoft.Json;

namespace Gorgon.IO
{
    /// <summary>
    /// A converter used to convert a texture to and from a string.
    /// </summary>
    internal class JsonTexture2DConverter
        : JsonConverter<GorgonTexture2DView>
    {
        // The graphics object to use for resource look up.
        private readonly GorgonGraphics _graphics;
        // The override texture.
        private readonly GorgonTexture2DView _override;
        // The list of properties for the type.
        private readonly HashSet<string> _propNames = new HashSet<string>(StringComparer.Ordinal)
                                                      {
                                                          "name",
                                                          "texWidth",
                                                          "texHeight",
                                                          "texFormat",
                                                          "texArrayCount",
                                                          "texMipCount",
                                                          "arrayStart",
                                                          "arrayCount",
                                                          "mipStart",
                                                          "mipCount",
                                                          "format"
                                                      };

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
            foreach (string propName in _propNames)
            {
                writer.WritePropertyName(propName);
                switch (propName)
                {
                    case "name":
                        writer.WriteValue(value.Texture.Name);
                        break;
                    case "texWidth":
                        writer.WriteValue(value.Texture.Width);
                        break;
                    case "texHeight":
                        writer.WriteValue(value.Texture.Height);
                        break;
                    case "texFormat":
                        writer.WriteValue(value.Texture.Format);
                        break;
                    case "texArrayCount":
                        writer.WriteValue(value.Texture.ArrayCount);
                        break;
                    case "texMipCount":
                        writer.WriteValue(value.Texture.MipLevels);
                        break;
                    case "arrayStart":
                        writer.WriteValue(value.ArrayIndex);
                        break;
                    case "arrayCount":
                        writer.WriteValue(value.ArrayCount);
                        break;
                    case "mipStart":
                        writer.WriteValue(value.MipSlice);
                        break;
                    case "mipCount":
                        writer.WriteValue(value.MipCount);
                        break;
                    case "format":
                        writer.WriteValue(value.Format);
                        break;
                }
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Function to read a texture from the JSON data.
        /// </summary>
        /// <param name="reader">The JSON reader to use.</param>
        /// <param name="textureName">The name of the texture.</param>
        /// <returns></returns>
        public GorgonTexture2DView ReadTexture(JsonReader reader, out string textureName)
        {
            textureName = string.Empty;

            if ((reader.TokenType != JsonToken.StartObject)
                || (_graphics == null))
            {
                return null;
            }

            int? texWidth = null;
            int? texHeight = null;
            int? texMipCount = null;
            int? texArrayCount = null;
            BufferFormat? texFormat = null;
            int? viewMipStart = null;
            int? viewMipCount = null;
            int? viewArrayStart = null;
            int? viewArrayCount = null;
            BufferFormat? viewFormat = null;

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                switch (reader.Value.ToString())
                {
                    case "name":
                        textureName = reader.ReadAsString();
                        break;
                    case "texWidth":
                        texWidth = reader.ReadAsInt32();
                        break;
                    case "texHeight":
                        texHeight = reader.ReadAsInt32();
                        break;
                    case "texFormat":
                        texFormat = (BufferFormat?)reader.ReadAsInt32();
                        break;
                    case "texArrayCount":
                        texArrayCount = reader.ReadAsInt32();
                        break;
                    case "texMipCount":
                        texMipCount = reader.ReadAsInt32();
                        break;
                    case "arrayStart":
                        viewArrayStart = reader.ReadAsInt32();
                        break;
                    case "arrayCount":
                        viewArrayCount = reader.ReadAsInt32();
                        break;
                    case "mipStart":
                        viewMipStart = reader.ReadAsInt32();
                        break;
                    case "mipCount":
                        viewMipCount = reader.ReadAsInt32();
                        break;
                    case "format":
                        viewFormat = (BufferFormat?)reader.ReadAsInt32();
                        break;
                }
            }

            if ((string.IsNullOrWhiteSpace(textureName))
                || (texWidth == null)
                || (texHeight == null)
                || (texFormat == null)
                || (texArrayCount == null)
                || (texMipCount == null)
                || (viewArrayStart == null)
                || (viewArrayCount == null)
                || (viewMipStart == null)
                || (viewMipCount == null)
                || (viewFormat == null))
            {
                if (string.IsNullOrWhiteSpace(textureName))
                {
                    _graphics?.Log.Print("Attempted to load a texture from JSON data, but texture was not in memory and name is unknown, or JSON texture data is not in the correct format.",
                                         LoggingLevel.Verbose);
                }

                return null;
            }

            if (_override != null)
            {
                return _override;
            }

            GorgonTexture2D texture = _graphics?.Locate2DTextureByName(textureName, texWidth ?? 1, texHeight ?? 1, texFormat ?? BufferFormat.R8G8B8A8_UNorm, texArrayCount ?? 1, texMipCount ?? 1);

            return texture?.GetShaderResourceView(viewFormat.Value, viewMipStart.Value, viewMipCount.Value, viewArrayStart.Value, viewArrayCount.Value);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override GorgonTexture2DView ReadJson(JsonReader reader, Type objectType, GorgonTexture2DView existingValue, bool hasExistingValue, JsonSerializer serializer) => ReadTexture(reader, out string _);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTexture2DConverter"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used for resource lookup.</param>
        /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
        public JsonTexture2DConverter(GorgonGraphics graphics, GorgonTexture2DView overrideTexture)
        {
            _graphics = graphics;
            _override = overrideTexture;
        }
    }
}
