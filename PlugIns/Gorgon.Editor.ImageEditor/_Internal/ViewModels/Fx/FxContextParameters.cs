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
// Created: February 29, 2020 8:04:14 PM
// 
#endregion

using System;
using Gorgon.Editor.ImageEditor.Fx;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Parameters for the <see cref="IFxContext"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FxContextParameters"/> class.</remarks>
/// <param name="imageContent">The image content being edited.</param>
/// <param name="fxService">The service used to apply effects and generate previews for effects.</param>
/// <param name="blurSettings">The settings for the blur effect.</param>
/// <param name="sharpenSettings">The settings for the sharpen effect.</param>
/// <param name="embossSettings">The settings for the emboss effect.</param>
/// <param name="edgeDetectSettings">The settings for the edge detect effect.</param>
/// <param name="posterizeSettings">The settings for the posterize effect.</param>
/// <param name="oneBitSettings">The settings for the one bit effect.</param>
/// <param name="hostServices">The services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when the parameters are <b>null</b>.</exception>
internal class FxContextParameters(IImageContent imageContent,
                            IFxService fxService,
                            IFxBlur blurSettings,
                            IFxSharpen sharpenSettings,
                            IFxEmboss embossSettings,
                            IFxEdgeDetect edgeDetectSettings,
                            IFxPosterize posterizeSettings,
                            IFxOneBit oneBitSettings,
                            IHostContentServices hostServices)
        : ViewModelInjection<IHostContentServices>(hostServices)
{
    /// <summary>
    /// Property to return the image content for the currently being edited.
    /// </summary>
    public IImageContent ImageContent
    {
        get;
    } = imageContent ?? throw new ArgumentNullException(nameof(imageContent));

    /// <summary>
    /// Property to return the settings for the blur effect.
    /// </summary>
    public IFxBlur BlurSettings
    {
        get;
    } = blurSettings ?? throw new ArgumentNullException(nameof(blurSettings));

    /// <summary>
    /// Property to return the settings for the sharpen effect.
    /// </summary>
    public IFxSharpen SharpenSettings
    {
        get;
    } = sharpenSettings ?? throw new ArgumentNullException(nameof(sharpenSettings));

    /// <summary>
    /// Property to return the settings for the emboss effect.
    /// </summary>
    public IFxEmboss EmbossSettings
    {
        get;
    } = embossSettings ?? throw new ArgumentNullException(nameof(embossSettings));

    /// <summary>
    /// Property to return the settings for the edge detect effect.
    /// </summary>
    public IFxEdgeDetect EdgeDetectSettings
    {
        get;
    } = edgeDetectSettings ?? throw new ArgumentNullException(nameof(edgeDetectSettings));

    /// <summary>
    /// Property to return the settings for the posterize effect.
    /// </summary>
    public IFxPosterize PosterizeSettings
    {
        get;
    } = posterizeSettings ?? throw new ArgumentNullException(nameof(posterizeSettings));

    /// <summary>
    /// Property to return the settings for the one bit effect.
    /// </summary>
    public IFxOneBit OneBitSettings
    {
        get;
    } = oneBitSettings ?? throw new ArgumentNullException(nameof(oneBitSettings));

    /// <summary>
    /// Property to return the service used to apply effects and generate previews for effects.
    /// </summary>
    public IFxService FxService
    {
        get;
    } = fxService ?? throw new ArgumentNullException(nameof(fxService));
}
