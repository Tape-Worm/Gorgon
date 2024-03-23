
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

using Gorgon.Graphics;
using Newtonsoft.Json;

namespace Gorgon.Editor.ImageAtlasTool;

/// <summary>
/// The settings for the plug in
/// </summary>
internal class TextureAtlasSettings
{
    /// <summary>
    /// Property to set or return the last directory path used to output sprites into.
    /// </summary>
    [JsonProperty]
    public string LastOutputDir
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the window is in a maximized state or not.
    /// </summary>
    [JsonProperty]
    public bool IsMaximized
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the maximum texture size.
    /// </summary>
    [JsonProperty]
    public GorgonPoint MaxTextureSize
    {
        get;
        set;
    } = new GorgonPoint(1024, 1024);

    /// <summary>
    /// Property to set or return the maximum array count.
    /// </summary>
    [JsonProperty]
    public int MaxArrayCount
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Property to set or return the amount of padding around each sprite.
    /// </summary>
    [JsonProperty]
    public int Padding
    {
        get;
        set;
    } = 2;

    /// <summary>
    /// Property to set or return whether to generate sprites when generating the atlas.
    /// </summary>
    [JsonProperty]
    public bool GenerateSprites
    {
        get;
        set;
    } = true;
}
