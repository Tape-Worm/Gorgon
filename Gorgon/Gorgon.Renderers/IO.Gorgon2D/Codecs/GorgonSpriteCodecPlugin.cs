
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
// Created: August 14, 2018 6:43:34 PM
// 

using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.Plugins;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// A plugin for allowing users to supply their own 3rd party sprite codecs
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonSpriteCodecPlugin"/> class
/// </remarks>
/// <param name="description">Optional description of the Plugin.</param>
/// <remarks>
/// <para>
/// Objects that implement this base class should pass in a hard coded description on the base constructor
/// </para>
/// </remarks>
public abstract class GorgonSpriteCodecPlugin(string description)
        : GorgonPlugin(description)
{
    /// <summary>
    /// Property to return the names of the available codecs for this plugin.
    /// </summary>
    /// <remarks>
    /// This returns a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the name of the plugin as its key, and an optional friendly description as its value.
    /// </remarks>
    public abstract IReadOnlyList<GorgonSpriteCodecDescription> Codecs
    {
        get;
    }

    /// <summary>
    /// Function to create a new <see cref="IGorgonSpriteCodec"/>.
    /// </summary>
    /// <param name="codec">The codec to retrieve from the plugin.</param>
    /// <param name="renderer">The renderer used to retrieve sprite textures.</param>
    /// <returns>A new <see cref="IGorgonSpriteCodec"/> object.</returns>
    /// <remarks>
    /// <para>
    /// Implementors must implement this method to return the codec from the plugin assembly.
    /// </para>
    /// </remarks>
    protected abstract IGorgonSpriteCodec OnCreateCodec(string codec, Gorgon2D renderer);

    /// <summary>
    /// Function to create a new image codec object.
    /// </summary>
    /// <param name="codec">The name of the codec to look up within the plugin.</param>
    /// <param name="renderer">The renderer used to retrieve sprite textures.</param>
    /// <returns>A new instance of a <see cref="IGorgonSpriteCodec"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/>, or the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="codec"/> was not found in this plugin.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="codec"/> is not found within the plugin, then an exception will be thrown. To determine whether the plugin has the desired <paramref name="codec"/>, check the 
    /// <see cref="Codecs"/> property on the plugin to locate the plugin name.
    /// </para>
    /// </remarks>
    public IGorgonSpriteCodec CreateCodec(string codec, Gorgon2D renderer)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));
        }

        if (!Codecs.Any(item => string.Equals(codec, item.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new KeyNotFoundException(string.Format(Resources.GOR2DIO_ERR_CODEC_NOT_IN_plugin, codec));
        }

        IGorgonSpriteCodec result = OnCreateCodec(codec, renderer);

        return result ?? throw new KeyNotFoundException(string.Format(Resources.GOR2DIO_ERR_CODEC_NOT_IN_plugin, codec));
    }
}
