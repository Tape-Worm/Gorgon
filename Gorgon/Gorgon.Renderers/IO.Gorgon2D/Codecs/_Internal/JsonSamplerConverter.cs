
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
// Created: August 11, 2018 7:56:39 PM
// 


using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Newtonsoft.Json;

namespace Gorgon.IO;

/// <summary>
/// A converter used to convert a texture sampler to and from a string
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonSamplerConverter"/> class
/// </remarks>
/// <param name="graphics">The graphics interface used for resource look up.</param>
internal class JsonSamplerConverter(GorgonGraphics graphics)
        : JsonConverter<GorgonSamplerState>
{
    // The graphics object to use for resource look up.
    private readonly GorgonGraphics _graphics = graphics;
    // The property names for the object.
    private readonly HashSet<string> _propNames = new(StringComparer.Ordinal)
                                                  {
                                                      "borderColor",
                                                      "compareFunc",
                                                      "filter",
                                                      "maxAnisotropy",
                                                      "maxLod",
                                                      "minLod",
                                                      "mipLodBias",
                                                      "wrapU",
                                                      "wrapV",
                                                      "wrapW"
                                                  };

    /// <summary>Writes the JSON representation of the object.</summary>
    /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, GorgonSamplerState value, JsonSerializer serializer)
    {
        if (value is null)
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
                case "borderColor":
                    writer.WriteValue(value.BorderColor.ToARGB());
                    break;
                case "compareFunc":
                    writer.WriteValue(value.ComparisonFunction);
                    break;
                case "filter":
                    writer.WriteValue(value.Filter);
                    break;
                case "maxAnisotropy":
                    writer.WriteValue(value.MaxAnisotropy);
                    break;
                case "maxLod":
                    writer.WriteValue(value.MaximumLevelOfDetail);
                    break;
                case "minLod":
                    writer.WriteValue(value.MinimumLevelOfDetail);
                    break;
                case "mipLodBias":
                    writer.WriteValue(value.MipLevelOfDetailBias);
                    break;
                case "wrapU":
                    writer.WriteValue(value.WrapU);
                    break;
                case "wrapV":
                    writer.WriteValue(value.WrapV);
                    break;
                case "wrapW":
                    writer.WriteValue(value.WrapW);
                    break;
                default:
                    throw new GorgonException(GorgonResult.CannotWrite, $@"Unknown property name {propName}.");
            }
        }

        writer.WriteEndObject();
    }

    /// <summary>Reads the JSON representation of the object.</summary>
    /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override GorgonSamplerState ReadJson(JsonReader reader, Type objectType, GorgonSamplerState existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if ((reader.TokenType != JsonToken.StartObject)
            || (_graphics is null))
        {
            return null;
        }

        GorgonColor? borderColor = null;
        Comparison? compareFunction = null;
        SampleFilter? filter = null;
        int? maxAnisotropy = null;
        float? maxLod = null;
        float? minLod = null;
        float? mipLodBias = null;
        TextureWrap? wrapU = null;
        TextureWrap? wrapV = null;
        TextureWrap? wrapW = null;

        var builder = new GorgonSamplerStateBuilder(_graphics);

        while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            switch (reader.Value.ToString())
            {
                case "borderColor":
                    borderColor = reader.ReadAsInt32();
                    break;
                case "compareFunc":
                    compareFunction = (Comparison?)reader.ReadAsInt32();
                    break;
                case "filter":
                    filter = (SampleFilter?)reader.ReadAsInt32();
                    break;
                case "maxAnisotropy":
                    maxAnisotropy = reader.ReadAsInt32();
                    break;
                case "maxLod":
                    maxLod = (float?)reader.ReadAsDouble();
                    break;
                case "minLod":
                    minLod = (float?)reader.ReadAsDouble();
                    break;
                case "mipLodBias":
                    mipLodBias = (float?)reader.ReadAsDouble();
                    break;
                case "wrapU":
                    wrapU = (TextureWrap?)reader.ReadAsInt32();
                    break;
                case "wrapV":
                    wrapV = (TextureWrap?)reader.ReadAsInt32();
                    break;
                case "wrapW":
                    wrapW = (TextureWrap?)reader.ReadAsInt32();
                    break;
            }
        }

        return builder.Wrapping(wrapU, wrapV, wrapW, borderColor)
                      .MaxAnisotropy(maxAnisotropy ?? 1)
                      .ComparisonFunction(compareFunction ?? Comparison.Never)
                      .Filter(filter ?? SampleFilter.MinMagMipLinear)
                      .MipLevelOfDetail(minLod ?? float.MinValue, maxLod ?? float.MaxValue, mipLodBias ?? 0)
                      .Build();
    }
}
