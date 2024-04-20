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
using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, GorgonSamplerState value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (string propName in _propNames)
        {
            switch (propName)
            {
                case "borderColor":
                    writer.WriteNumber(propName, GorgonColor.ToARGB(value.BorderColor));
                    break;
                case "compareFunc":
                    writer.WriteNumber(propName, (int)value.ComparisonFunction);
                    break;
                case "filter":
                    writer.WriteNumber(propName, (int)value.Filter);
                    break;
                case "maxAnisotropy":
                    writer.WriteNumber(propName, value.MaxAnisotropy);
                    break;
                case "maxLod":
                    writer.WriteNumber(propName, value.MaximumLevelOfDetail);
                    break;
                case "minLod":
                    writer.WriteNumber(propName, value.MinimumLevelOfDetail);
                    break;
                case "mipLodBias":
                    writer.WriteNumber(propName, value.MipLevelOfDetailBias);
                    break;
                case "wrapU":
                    writer.WriteNumber(propName, (int)value.WrapU);
                    break;
                case "wrapV":
                    writer.WriteNumber(propName, (int)value.WrapV);
                    break;
                case "wrapW":
                    writer.WriteNumber(propName, (int)value.WrapW);
                    break;
                default:
                    throw new GorgonException(GorgonResult.CannotWrite, $@"Unknown property name {propName}.");
            }
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override GorgonSamplerState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ((reader.TokenType != JsonTokenType.StartObject)
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

        GorgonSamplerStateBuilder builder = new(_graphics);

        while ((reader.Read()) && (reader.TokenType != JsonTokenType.EndObject))
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string propName = reader.GetString();

            if (!reader.Read())
            {
                break;
            }

            switch (propName)
            {
                case "borderColor":
                    borderColor = GorgonColor.FromARGB(reader.GetInt32());
                    break;
                case "compareFunc":
                    compareFunction = (Comparison)reader.GetInt32();
                    break;
                case "filter":
                    filter = (SampleFilter)reader.GetInt32();
                    break;
                case "maxAnisotropy":
                    maxAnisotropy = reader.GetInt32();
                    break;
                case "maxLod":
                    maxLod = reader.GetSingle();
                    break;
                case "minLod":
                    minLod = reader.GetSingle();
                    break;
                case "mipLodBias":
                    mipLodBias = reader.GetSingle();
                    break;
                case "wrapU":
                    wrapU = (TextureWrap)reader.GetInt32();
                    break;
                case "wrapV":
                    wrapV = (TextureWrap)reader.GetInt32();
                    break;
                case "wrapW":
                    wrapW = (TextureWrap)reader.GetInt32();
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
