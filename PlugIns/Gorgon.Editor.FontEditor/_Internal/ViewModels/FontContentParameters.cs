#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 4, 2020 11:21:27 PM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Parameters to pass in to the <see cref="IFontContent"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FontContentParameters"/> class.</remarks>
/// <param name="fontService">The service used to generate fonts.</param>
/// <param name="settings">The settings panel view model.</param>
/// <param name="fontOutline">The font outline view model.</param>
/// <param name="textureEditor">The texture editor context view model.</param>
/// <param name="undoService">The service used to apply undo/redo operations.</param>
/// <param name="fileManager">The file manager for content files.</param>
/// <param name="file">The file that contains the content.</param>
/// <param name="commonServices">The common services for the application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class FontContentParameters(FontService fontService,
                             ISettings settings, IFontOutline fontOutline, ITextureEditorContext textureEditor, IFontCharacterSelection fontCharSelection,
                             IUndoService undoService, IContentFileManager fileManager, IContentFile file, IHostContentServices commonServices)
        : ContentViewModelInjection(fileManager, file, commonServices)
{
    /// <summary>
    /// Property to return the undo/redo service.
    /// </summary>
    public IUndoService UndoService
    {
        get;
    } = undoService ?? throw new ArgumentNullException(nameof(undoService));

    /// <summary>
    /// Property to return the settings view model.
    /// </summary>
    public ISettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the font service used to build fonts.
    /// </summary>
    public FontService FontService
    {
        get;
    } = fontService ?? throw new ArgumentNullException(nameof(fontService));

    /// <summary>
    /// Property to return the font outline view model.
    /// </summary>
    public IFontOutline FontOutline
    {
        get;
    } = fontOutline ?? throw new ArgumentNullException(nameof(fontOutline));

    /// <summary>
    /// Property to return the texture editor context view model.
    /// </summary>
    public ITextureEditorContext TextureEditorContext
    {
        get;
    } = textureEditor ?? throw new ArgumentNullException(nameof(textureEditor));

    public IFontCharacterSelection FontCharacterSelection
    {
        get;
    } = fontCharSelection ?? throw new ArgumentNullException(nameof(fontCharSelection));
}
