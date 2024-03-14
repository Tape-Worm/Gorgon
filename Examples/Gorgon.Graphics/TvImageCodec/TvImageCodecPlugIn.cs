#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 5, 2017 10:00:01 PM
// 
#endregion
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Examples;

/// <summary>
/// Our entry point into the TV image codec.
/// </summary>
/// <remarks>
/// This plug in will encode/decode images as 1 pixel per channel.  This will give the image an appearance similar to the line patterns on a CRT TV screen.  Well, somewhat.
/// </remarks>
public class TvImageCodecPlugIn
    : GorgonImageCodecPlugIn
{
    #region Properties.
    /// <summary>
    /// Property to return the names of the available codecs for this plug in.
    /// </summary>
    /// <remarks>
    /// This returns a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the name of the plug in as its key, and an optional friendly description as its value.
    /// </remarks>
    public override IReadOnlyList<GorgonImageCodecDescription> Codecs
    {
        get;
    }
    #endregion

    #region Methods.
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
    protected override IGorgonImageCodec OnCreateCodec(string codec) => new TvImageCodec();

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
    protected override IGorgonImageCodecDecodingOptions OnCreateCodecDecodingOptions(string codec) => null;

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
    protected override IGorgonImageCodecEncodingOptions OnCreateCodecEncodingOptions(string codec) => null;
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="TvImageCodecPlugIn"/> class.
    /// </summary>
    public TvImageCodecPlugIn()
        : base("A TV image codec, used for example only.") => Codecs =
                 [
                     new GorgonImageCodecDescription(typeof(TvImageCodec))
                     {
                         Description = Description
                     }
                 ];
    #endregion
}
