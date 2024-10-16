﻿
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: March 22, 2019 10:06:04 AM
// 

using System.Text.Json.Serialization;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The settings for the sprite importer plug-in
/// </summary>
internal class SpriteImportSettings
{
    /// <summary>
    /// Property to return the list of additional sprite codec plug-ins to load.
    /// </summary>
    [JsonInclude]
    public Dictionary<string, string> CodecPlugInPaths
    {
        get;
        private set;
    } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property to set or return the last codec plug-in path.
    /// </summary>
    [JsonInclude]
    public string LastCodecPlugInPath
    {
        get;
        set;
    }
}

/// <summary>
/// The settings for the sprite editor plug-in
/// </summary>
internal class SpriteEditorSettings
{
    /// <summary>
    /// Property to set or return the position of the manual rectangle editor window.
    /// </summary>
    [JsonInclude]
    public GorgonRectangle? ManualRectangleEditorBounds
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the position of the manual vertex editor window.
    /// </summary>
    [JsonInclude]
    public GorgonRectangle? ManualVertexEditorBounds
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to show a warning dialog for large images.
    /// </summary>
    [JsonInclude]
    public bool ShowImageSizeWarning
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the type of masking to perform when picking and clipping.
    /// </summary>
    [JsonInclude]
    public ClipMask ClipMaskType
    {
        get;
        set;
    } = ClipMask.Alpha;

    /// <summary>
    /// Property to set or return the sprite picker mask color.
    /// </summary>
    [JsonInclude]
    public int ClipMaskValue
    {
        get;
        set;
    } = GorgonColor.ToRGBA(new GorgonColor(1, 0, 1, 0));
}
