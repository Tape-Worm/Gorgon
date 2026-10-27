
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
// Created: April 22, 2019 10:49:16 PM
// 

using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A registry for the image codecs used by the plugins in this assembly
/// </summary>
internal interface ICodecRegistry
{
    /// <summary>
    /// Property to return the codecs cross referenced with known file extension types.
    /// </summary>
    IList<(GorgonFileExtension extension, IGorgonImageCodec codec)> CodecFileTypes
    {
        get;
    }

    /// <summary>
    /// Property to return the list of image codec plugins.
    /// </summary>
    IList<GorgonImageCodecPlugin> CodecPlugins
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

    /// <summary>
    /// Function to add a codec to the registry.
    /// </summary>
    /// <param name="path">The path to the codec assembly.</param>
    /// <param name="errors">A list of errors if the plugin fails to load.</param>
    /// <returns>A list of codec plugs ins that were loaded.</returns>
    IReadOnlyList<GorgonImageCodecPlugin> AddCodecPlugin(string path, out IReadOnlyList<string> errors);

    /// <summary>
    /// Function to load the codecs from our settings data.
    /// </summary>
    /// <param name="settings">The settings containing the plugin paths.</param>
    void LoadFromSettings(ImageEditorSettings settings);

    /// <summary>
    /// Function to remove an image codec plugin from the registry.
    /// </summary>
    /// <param name="plugin">The plugin to remove.</param>
    void RemoveCodecPlugin(GorgonImageCodecPlugin plugin);

}
