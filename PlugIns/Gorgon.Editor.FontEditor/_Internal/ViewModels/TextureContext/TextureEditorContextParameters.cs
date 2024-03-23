
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: September 1, 2021 2:37:04 AM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The parameters for the <see cref="ITextureEditorContext"/> context view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="TextureEditorContextParameters"/> class.</remarks>
/// <param name="fontService">The service used to generate fonts.</param>
/// <param name="fontTextureSize">The font texture size view model.</param>
/// <param name="fontPadding">The font padding view model.</param>
/// <param name="solidBrush">The view model for a solid color brush.</param>
/// <param name="patternBrush">The view model for a pattern brush.</param>
/// <param name="gradientBrush">The view model for a gradient brush.</param>
/// <param name="textureBrush">The view model for a texture brush.</param>
/// <param name="undoService">The undo service.</param>
/// <param name="hostServices">The services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class TextureEditorContextParameters(FontService fontService, IFontTextureSize fontTextureSize, IFontPadding fontPadding, IFontSolidBrush solidBrush, IFontPatternBrush patternBrush, IFontGradientBrush gradientBrush, IFontTextureBrush textureBrush, IUndoService undoService, IHostContentServices hostServices)
        : ViewModelInjection<IHostContentServices>(hostServices)
{
    /// <summary>
    /// Property to return the undo service.
    /// </summary>
    public IUndoService UndoService
    {
        get;
    } = undoService ?? throw new ArgumentNullException(nameof(fontService));

    /// <summary>
    /// Property to return the service used to generate the font.
    /// </summary>
    public FontService FontService
    {
        get;
    } = fontService ?? throw new ArgumentNullException(nameof(undoService));

    /// <summary>
    /// Property to return the font padding view model.
    /// </summary>
    public IFontPadding FontPadding
    {
        get;
    } = fontPadding ?? throw new ArgumentNullException(nameof(fontPadding));

    /// <summary>
    /// Property to return the font texture size view model.
    /// </summary>
    public IFontTextureSize FontTextureSize
    {
        get;
    } = fontTextureSize ?? throw new ArgumentNullException(nameof(fontTextureSize));

    /// <summary>
    /// Property to return the solid color brush view model.
    /// </summary>
    public IFontSolidBrush SolidBrush
    {
        get;
    } = solidBrush ?? throw new ArgumentNullException(nameof(solidBrush));

    /// <summary>
    /// Property to return the pattern brush view model.
    /// </summary>
    public IFontPatternBrush PatternBrush
    {
        get;
    } = patternBrush ?? throw new ArgumentNullException(nameof(patternBrush));

    /// <summary>
    /// Property to return the gradient brush view model.
    /// </summary>
    public IFontGradientBrush GradientBrush
    {
        get;
    } = gradientBrush ?? throw new ArgumentNullException(nameof(gradientBrush));

    /// <summary>
    /// Property to return the texture brush view model.
    /// </summary>
    public IFontTextureBrush TextureBrush
    {
        get;
    } = textureBrush ?? throw new ArgumentNullException(nameof(textureBrush));
}
