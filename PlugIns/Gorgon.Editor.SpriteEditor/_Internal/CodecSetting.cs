
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
// Created: April 21, 2019 1:39:30 AM
// 

using Gorgon.Core;
using Gorgon.IO;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A setting used to display a loaded codec
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="CodecSetting"/> class.</remarks>
/// <param name="description">The friendly description of the codec.</param>
/// <param name="plugin">The plugin for the codec.</param>
/// <param name="desc">The description of the codec.</param>
internal class CodecSetting(string description, GorgonSpriteCodecPlugIn plugin, GorgonSpriteCodecDescription desc)
        : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the description for the codec.
    /// </summary>
    public string Description
    {
        get;
    } = description;

    /// <summary>
    /// Property to return the plug-in that contains the codec.
    /// </summary>
    public GorgonSpriteCodecPlugIn PlugIn
    {
        get;
    } = plugin;

    /// <summary>
    /// Property to return the formal name of the codec.
    /// </summary>
    public string Name
    {
        get;
    } = desc.Name;
}
