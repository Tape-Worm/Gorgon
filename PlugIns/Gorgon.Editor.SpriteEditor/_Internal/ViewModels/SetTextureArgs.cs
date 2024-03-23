
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
// Created: April 10, 2020 11:58:11 AM
// 

using System.ComponentModel;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The arguments for the <see cref="ISpriteContent.SetTextureCommand"/>
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SetTextureArgs"/> class.</remarks>
/// <param name="textureFilePath">The path to the file containing the texture data.</param>
internal class SetTextureArgs(string textureFilePath)
        : CancelEventArgs
{
    /// <summary>
    /// Property to return the file containing the texture data.
    /// </summary>
    public string TextureFilePath
    {
        get;
    } = textureFilePath;
}
