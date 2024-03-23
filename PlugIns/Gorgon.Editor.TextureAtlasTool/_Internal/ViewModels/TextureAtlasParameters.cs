
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
// Created: May 7, 2019 12:12:47 AM
// 

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The parameters for the <see cref="ITextureAtlas"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="TextureAtlasParameters"/> class.</remarks>
/// <param name="spriteFiles">The view model for the sprite loader UI.</param>
/// <param name="settings">The settings for the texture atlas plug in.</param>
/// <param name="atlasGenerator">The service used to generate texture atlases.</param>
/// <param name="fileIO">The service used to manage the atlas files.</param>
/// <param name="fileManager">The file manager for the project file system.</param>
/// <param name="toolServices">The common tool services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class TextureAtlasParameters(ISpriteFiles spriteFiles,
                              TextureAtlasSettings settings,
                              IGorgonTextureAtlasService atlasGenerator,
                              FileIOService fileIO,
                              IContentFileManager fileManager,
                              IHostContentServices toolServices)
        : EditorToolViewModelInjection(fileManager, toolServices)
{
    /// <summary>
    /// Property to return the view model for the sprite loader UI.
    /// </summary>
    public ISpriteFiles SpriteFiles
    {
        get;
    } = spriteFiles ?? throw new ArgumentNullException(nameof(spriteFiles));

    /// <summary>
    /// Property to return the settings for the plug in.
    /// </summary>
    public TextureAtlasSettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the atlas generation service.
    /// </summary>
    public IGorgonTextureAtlasService AtlasGenerator
    {
        get;
    } = atlasGenerator ?? throw new ArgumentNullException(nameof(atlasGenerator));

    /// <summary>
    /// Property to return the service used to manage atlas files.
    /// </summary>
    public FileIOService FileIO
    {
        get;
    } = fileIO ?? throw new ArgumentNullException(nameof(fileIO));
}
