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
// Created: April 25, 2019 9:37:40 AM
// 

using System.Text.Json.Serialization;
using Gorgon.Graphics;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The settings for the plug-in
/// </summary>
internal class TextureAtlasSettings
{
    /// <summary>
    /// Property to set or return the last directory path used to output sprites into.
    /// </summary>
    [JsonInclude]
    public string LastOutputDir
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the window is in a maximized state or not.
    /// </summary>
    [JsonInclude]
    public bool IsMaximized
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the maximum texture size.
    /// </summary>
    [JsonInclude]
    public GorgonPoint MaxTextureSize
    {
        get;
        set;
    } = new GorgonPoint(1024, 1024);

    /// <summary>
    /// Property to set or return the maximum array count.
    /// </summary>
    [JsonInclude]
    public int MaxArrayCount
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Property to set or return the amount of padding around each sprite.
    /// </summary>
    [JsonInclude]
    public int Padding
    {
        get;
        set;
    } = 2;
}
