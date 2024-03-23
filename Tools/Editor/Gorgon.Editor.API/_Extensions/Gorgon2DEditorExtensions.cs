
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
// Created: March 27, 2019 10:41:00 PM
// 

using Gorgon.Core;
using Gorgon.Editor.ProjectData;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// Extension methods to load editor content
/// </summary>
/// <remarks>
/// <para>
/// A set of convenience methods to load data from an editor file system.  
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// These methods load the data using the layout and metadata information as provided by the default plug ins for the editor.  Custom plug ins for sprite data, etc... may not work with these methods 
/// unless those plug ins follow the same file layout as the default plug ins
/// </para>
/// </note>
/// </para>
/// </remarks>
public static class Gorgon2DEditorExtensions
{
    /// <summary>
    /// Function to create a loader object for reading content from an editor file system.
    /// </summary>
    /// <param name="fileSystem">The file system that holds the content to load.</param>
    /// <param name="renderer">The 2D renderer used to handle loading of additional resources.</param>
    /// <param name="textureCache">The cache used to host any texture dependencies for the content.</param>
    /// <returns>A new <see cref="IGorgonContentLoader"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="fileSystem"/> is not a Gorgon Editor file system, or is an unsupported version.</exception>
    public static IGorgonContentLoader CreateContentLoader(this IGorgonFileSystem fileSystem, Gorgon2D renderer, GorgonTextureCache<GorgonTexture2D> textureCache)
    {
        if (fileSystem is null)
        {
            throw new ArgumentNullException(nameof(fileSystem));
        }

        if (renderer is null)
        {
            throw new ArgumentNullException(nameof(renderer));

        }

        if (textureCache is null)
        {
            throw new ArgumentNullException(nameof(textureCache));
        }

        IProjectMetadata metadata = fileSystem.GetMetadata();

        return new ContentLoader2D(fileSystem, metadata, renderer, textureCache);
    }
}
