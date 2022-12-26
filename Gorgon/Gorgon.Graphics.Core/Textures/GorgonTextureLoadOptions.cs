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
// Created: April 12, 2018 12:40:39 AM
// 
#endregion

namespace Gorgon.Graphics.Core;

/// <summary>
/// Options to pass when loading a texture from a stream or the file system.
/// </summary>
public class GorgonTextureLoadOptions
{
    /// <summary>
    /// Property to set or return the name of the image.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the intended usage for the texture.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="ResourceUsage.Default"/>.
    /// </remarks>
    public ResourceUsage Usage
    {
        get;
        set;
    } = ResourceUsage.Default;


    /// <summary>
    /// Property to set or return the allowed bindings for the texture.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="TextureBinding.ShaderResource"/>.
    /// </remarks>
    public TextureBinding Binding
    {
        get;
        set;
    } = TextureBinding.ShaderResource;

    /// <summary>
    /// Property to set or return whether to convert the texture data to premultiplied alpha when loading.
    /// </summary>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool ConvertToPremultipliedAlpha
    {
        get;
        set;
    }
}
