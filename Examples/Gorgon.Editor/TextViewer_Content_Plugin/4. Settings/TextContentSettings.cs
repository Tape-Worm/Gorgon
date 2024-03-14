
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: August 5, 2020 8:30:25 PM
// 


using Newtonsoft.Json;

namespace Gorgon.Examples;

/// <summary>
/// The settings for the animation importer plug in
/// </summary>
/// <remarks>
/// This is our settings for the plug in. The settings should contain simple primitive types so they can be serialized into/deserialized from 
/// JSON data
/// </remarks>
internal class TextContentSettings
{
    /// <summary>
    /// Property to set or return the default font for the text.
    /// </summary>
    [JsonProperty]
    public FontFace DefaultFont
    {
        get;
        set;
    } = FontFace.Arial;
}
