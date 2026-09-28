
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: February 7, 2021 12:31:53 AM
// 

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Gorgon.Graphics;
using Gorgon.Json;

namespace Gorgon.Examples;

/// <summary>
/// Configuration information for the example application
/// </summary>]
public class ExampleConfig
{
    // The default instance.
    private static ExampleConfig _default;
    // Options used to deserialize a JSON string.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new GorgonPointJsonConverter()
        }
    };

    /// <summary>
    /// Function to load the configuration.
    /// </summary>
    private static void LoadConfig()
    {
        using StreamReader reader = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
        string configJson = reader.ReadToEnd();

        JsonObject? result = JsonSerializer.Deserialize<JsonObject>(configJson, _jsonOptions);

        if ((result is null) || (result.Count != 1))
        {
            _default = new ExampleConfig();
            return;
        }

        _default = result[0].Deserialize<ExampleConfig>(_jsonOptions) ?? new ExampleConfig();
    }

    /// <summary>
    /// Property to return the default settings.
    /// </summary>
    public static ExampleConfig Default
    {
        get
        {
            if (_default is null)
            {
                LoadConfig();
            }

            return _default;
        }
    }

    /// <summary>
    /// Property to set or return the path to the resources for the example.
    /// </summary>
    [JsonInclude]
    public string ResourceLocation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the path to the location for example plugins.
    /// </summary>
    [JsonInclude]
    public string PluginLocation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the desired window resolution.
    /// </summary>
    [JsonInclude]
    public GorgonPoint Resolution
    {
        get;
        set;
    } = GorgonPoint.Zero;

    /// <summary>
    /// Property to set or return whether the example runs in windowed mode, or full screen mode.
    /// </summary>
    [JsonInclude]
    public bool IsWindowed
    {
        get;
        set;
    } = true;
}
