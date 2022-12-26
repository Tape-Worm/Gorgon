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
// Created: November 7, 2018 1:33:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The settings for the image editor plug in.
/// </summary>
internal class ImageEditorSettings
{
    /// <summary>
    /// Property to return the list of additional image codec plug ins to load.
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> CodecPlugInPaths
    {
        get;
        private set;
    } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property to set or return the last codec plug in path.
    /// </summary>
    [JsonProperty]
    public string LastCodecPlugInPath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the to the directory that was last used for importing/exporting.
    /// </summary>
    [JsonProperty]
    public string LastImportExportPath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last used alpha value when setting an alpha channel.
    /// </summary>
    [JsonProperty]
    public int AlphaValue
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last used alpha value minimum when setting an alpha channel.
    /// </summary>
    [JsonProperty]
    public int AlphaRangeMin
    {
        get;
        set;
    } = 0;

    /// <summary>
    /// Property to set or return the last used alpha value maximum when setting an alpha channel.
    /// </summary>
    [JsonProperty]
    public int AlphaRangeMax
    {
        get;
        set;
    } = 255;

    /// <summary>Property to set or return the width of the picker window.</summary>
    [JsonProperty]
    public int PickerWidth
    {
        get;
        set;
    } = 900;

    /// <summary>Property to set or return the height of the picker window.</summary>
    [JsonProperty]
    public int PickerHeight
    {
        get;
        set;
    } = 600;

    /// <summary>Property to set or return the state of the picker window.</summary>
    [JsonProperty]
    public int PickerWindowState
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the path to the image editor to use when editing the texture.
    /// </summary>
    [JsonProperty]
    public string ImageEditorApplicationPath
    {
        get;
        set;
    }
}
