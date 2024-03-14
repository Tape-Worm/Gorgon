#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 22, 2019 10:49:16 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A registry for the image codecs used by the plug ins in this assembly.
/// </summary>
internal interface ICodecRegistry
{
    #region Properties.
    /// <summary>
    /// Property to return the codecs cross referenced with known file extension types.
    /// </summary>
    IList<(GorgonFileExtension extension, IGorgonImageCodec codec)> CodecFileTypes
    {
        get;
    }

    /// <summary>
    /// Property to return the list of image codec plug ins.
    /// </summary>
    IList<GorgonImageCodecPlugIn> CodecPlugIns
    {
        get;
    }

    /// <summary>
    /// Property to return the list of codecs.
    /// </summary>
    IList<IGorgonImageCodec> Codecs
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to add a codec to the registry.
    /// </summary>
    /// <param name="path">The path to the codec assembly.</param>
    /// <param name="errors">A list of errors if the plug in fails to load.</param>
    /// <returns>A list of codec plugs ins that were loaded.</returns>
    IReadOnlyList<GorgonImageCodecPlugIn> AddCodecPlugIn(string path, out IReadOnlyList<string> errors);

    /// <summary>
    /// Function to load the codecs from our settings data.
    /// </summary>
    /// <param name="settings">The settings containing the plug in paths.</param>
    void LoadFromSettings(ImageEditorSettings settings);

    /// <summary>
    /// Function to remove an image codec plug in from the registry.
    /// </summary>
    /// <param name="plugin">The plug in to remove.</param>
    void RemoveCodecPlugIn(GorgonImageCodecPlugIn plugin);
    #endregion
}
