
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
// Created: May 25, 2020 8:54:53 PM
// 


using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// Parameters for the <see cref="IExtract"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ExtractParameters"/> class.</remarks>
/// <param name="settings">The plug in settings.</param>
/// <param name="extractData">The data used for extraction.</param>
/// <param name="extractor">The sprite extractor service used to create the sprites.</param>
/// <param name="textureFile">The file that contains the texture to extract from.</param>
/// <param name="fileManager">The file manager for the project file system.</param>
/// <param name="toolServices">The common tool services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class ExtractParameters(ExtractSpriteToolSettings settings, SpriteExtractionData extractData, ISpriteExtractorService extractor, IContentFile textureFile, IContentFileManager fileManager, IHostContentServices toolServices)
        : EditorToolViewModelInjection(fileManager, toolServices)
{
    /// <summary>
    /// Property to return the data used for extraction.
    /// </summary>
    public SpriteExtractionData ExtractionData
    {
        get;
    } = extractData ?? throw new ArgumentNullException(nameof(extractData));

    /// <summary>
    /// Property to return the settings for the plug in.
    /// </summary>
    public ExtractSpriteToolSettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the sprite extractor service used to create the sprites.
    /// </summary>
    public ISpriteExtractorService Extractor
    {
        get;
    } = extractor ?? throw new ArgumentNullException(nameof(extractor));

    /// <summary>
    /// Property to return the file that contains the texture to extract from.
    /// </summary>
    public IContentFile TextureFile
    {
        get;
    } = textureFile ?? throw new ArgumentNullException(nameof(textureFile));
}
