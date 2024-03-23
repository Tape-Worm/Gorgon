
// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
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
// Created: August 16, 2016 3:30:55 PM
// 

using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.PlugIns;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// A plug in to allow for loading of custom image codecs
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonImageCodecPlugIn"/> class
/// </remarks>
/// <param name="description">Optional description of the plug in.</param>
/// <remarks>
/// <para>
/// Objects that implement this base class should pass in a hard coded description on the base constructor
/// </para>
/// </remarks>
public abstract class GorgonImageCodecPlugIn(string description)
        : GorgonPlugIn(description)
{
    /// <summary>
    /// Property to return the names of the available codecs for this plug in.
    /// </summary>
    /// <remarks>
    /// This returns a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the name of the plug in as its key, and an optional friendly description as its value.
    /// </remarks>
    public abstract IReadOnlyList<GorgonImageCodecDescription> Codecs
    {
        get;
    }

    /// <summary>
    /// Function to create a new <see cref="IGorgonImageCodec"/>.
    /// </summary>
    /// <param name="codec">The codec to retrieve from the plug in.</param>
    /// <returns>A new <see cref="IGorgonImageCodec"/> object.</returns>
    /// <remarks>
    /// <para>
    /// Implementors must implement this method to return the codec from the plug in assembly.
    /// </para>
    /// </remarks>
    protected abstract IGorgonImageCodec OnCreateCodec(string codec);

    /// <summary>
    /// Function to create image encoding options for the codec.
    /// </summary>
    /// <param name="codec">The name of the codec to which the options will apply.</param>
    /// <returns>A new <see cref="IGorgonImageCodecEncodingOptions"/> object.</returns>
    /// <remarks>
    /// <para>
    /// Implementors must implement this method to return any optional encoding options for the codec to use when encoding data. If no encoding options apply to the codec, then this method should 
    /// return <b>null</b>.
    /// </para>
    /// </remarks>
    protected abstract IGorgonImageCodecEncodingOptions OnCreateCodecEncodingOptions(string codec);

    /// <summary>
    /// Function to create image decoding options for the codec.
    /// </summary>
    /// <param name="codec">The name of the codec to which the options will apply.</param>
    /// <returns>A new <see cref="IGorgonImageCodecDecodingOptions"/> object.</returns>
    /// <remarks>
    /// <para>
    /// Implementors must implement this method to return any optional decoding options for the codec to use when decoding data. If no decoding options apply to the codec, then this method should 
    /// return <b>null</b>.
    /// </para>
    /// </remarks>
    protected abstract IGorgonImageCodecDecodingOptions OnCreateCodecDecodingOptions(string codec);

    /// <summary>
    /// Function to create a new codec encoding options object for the specified codec.
    /// </summary>
    /// <param name="codec">The name of the codec to which the options will apply.</param>
    /// <returns>A new instance of a <see cref="IGorgonImageCodecEncodingOptions"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="codec"/> was not found in this plug in.</exception>
    /// <remarks>
    /// <para>
    /// If the requested <paramref name="codec"/> has no encoding options, then this method will return <b>null</b>.
    /// </para>
    /// </remarks>
    public IGorgonImageCodecEncodingOptions CreateEncodingOptions(string codec)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        return !Codecs.Any(item => string.Equals(codec, item.Name, StringComparison.OrdinalIgnoreCase))
            ? throw new KeyNotFoundException(string.Format(Resources.GORIMG_ERR_CODEC_NOT_IN_PLUGIN, codec))
            : OnCreateCodecEncodingOptions(codec);

    }

    /// <summary>
    /// Function to create a new codec decoding options object for the specified codec.
    /// </summary>
    /// <param name="codec">The name of the codec to which the options will apply.</param>
    /// <returns>A new instance of a <see cref="IGorgonImageCodecDecodingOptions"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="codec"/> was not found in this plug in.</exception>
    /// <remarks>
    /// <para>
    /// If the requested <paramref name="codec"/> has no decoding options, then this method will return <b>null</b>.
    /// </para>
    /// </remarks>
    public IGorgonImageCodecDecodingOptions CreateDecodingOptions(string codec)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        return !Codecs.Any(item => string.Equals(codec, item.Name, StringComparison.OrdinalIgnoreCase))
            ? throw new KeyNotFoundException(string.Format(Resources.GORIMG_ERR_CODEC_NOT_IN_PLUGIN, codec))
            : OnCreateCodecDecodingOptions(codec);

    }

    /// <summary>
    /// Function to create a new image codec object.
    /// </summary>
    /// <param name="codec">The name of the codec to look up within the plug in.</param>
    /// <returns>A new instance of a <see cref="IGorgonImageCodec"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="codec"/> was not found in this plug in.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="codec"/> is not found within the plug in, then an exception will be thrown. To determine whether the plug in has the desired <paramref name="codec"/>, check the 
    /// <see cref="Codecs"/> property on the plug in to locate the plug in name.
    /// </para>
    /// </remarks>
    public IGorgonImageCodec CreateCodec(string codec)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        if (!Codecs.Any(item => string.Equals(codec, item.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new KeyNotFoundException(string.Format(Resources.GORIMG_ERR_CODEC_NOT_IN_PLUGIN, codec));
        }

        IGorgonImageCodec result = OnCreateCodec(codec);

        return result ?? throw new KeyNotFoundException(string.Format(Resources.GORIMG_ERR_CODEC_NOT_IN_PLUGIN, codec));
    }
}
