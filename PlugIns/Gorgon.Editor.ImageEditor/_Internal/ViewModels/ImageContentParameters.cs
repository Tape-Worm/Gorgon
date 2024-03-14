#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: November 10, 2018 11:23:11 PM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor.ViewModels;

/// <summary>
/// Parameters to pass to the <see cref="ImageContent"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the ImageContentVmParameters class.</remarks>
/// <param name="fileManager">The file manager for content files.</param>
/// <param name="file">The file for the image content.</param>
/// <param name="settings">The settings for the image editor.</param>
/// <param name="pluginSettings">The plug in settings for the image editor.</param>
/// <param name="imagePicker">The image picker used to import image data into the current image.</param>
/// <param name="cropResizeSettings">The crop/resize settings view model.</param>
/// <param name="dimensionSettings">The image dimensions settings view model.</param>
/// <param name="mipMapSettings">The mip map generation settings view model.</param>
/// <param name="alphaSettings">The set alpha value settings view model.</param>
/// <param name="fxContext">The context for the image effects.</param>
/// <param name="imageData">The image data and related information.</param>
/// <param name="videoAdapter">Information about the current video adapter.</param>
/// <param name="formatSupport">A list of <see cref="IGorgonFormatSupportInfo"/> objects for each pixel format.</param>
/// <param name="extEditorInfo">The external image editor information.</param>
/// <param name="userEditorInfo">The external image editor information.</param>
/// <param name="services">The services required by the image editor.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class ImageContentParameters(IContentFileManager fileManager,
    IContentFile file,
    ISettings settings,
    ISettingsPlugins pluginSettings,
    IImagePicker imagePicker,
    ICropResizeSettings cropResizeSettings,
    IDimensionSettings dimensionSettings,
    IMipMapSettings mipMapSettings,
    IAlphaSettings alphaSettings,
    IFxContext fxContext,
    (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) imageData,
    IGorgonVideoAdapterInfo videoAdapter,
    IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> formatSupport,
    ImageEditorServices services)
        : ContentViewModelInjection(fileManager, file, services.HostContentServices ?? throw new ArgumentNullException(nameof(services)))
{
    #region Properties.
    /// <summary>
    /// Property to return the image dimension editor view model.
    /// </summary>
    public IDimensionSettings DimensionSettings
    {
        get;
    } = dimensionSettings ?? throw new ArgumentNullException(nameof(dimensionSettings));

    /// <summary>
    /// Property to return the image picker view model.
    /// </summary>
    public IImagePicker ImagePicker
    {
        get;
    } = imagePicker ?? throw new ArgumentNullException(nameof(imagePicker));

    /// <summary>
    /// Property to return the crop/resize settings view model.
    /// </summary>        
    public ICropResizeSettings CropResizeSettings
    {
        get;
    } = cropResizeSettings ?? throw new ArgumentNullException(nameof(cropResizeSettings));

    /// <summary>
    /// Property to return the mip map generation view model.
    /// </summary>
    public IMipMapSettings MipMapSettings
    {
        get;
    } = mipMapSettings ?? throw new ArgumentNullException(nameof(mipMapSettings));

    /// <summary>
    /// Property to return the alpha settings view model.
    /// </summary>
    public IAlphaSettings AlphaSettings
    {
        get;
    } = alphaSettings ?? throw new ArgumentNullException(nameof(alphaSettings));

    /// <summary>
    /// Property to return the service used to load/save image data.
    /// </summary>
    public IImageIOService ImageIOService
    {
        get;
    } = services.ImageIO;

    /// <summary>
    /// Property to return the file used to storing working changes.
    /// </summary>
    public IGorgonVirtualFile WorkingFile
    {
        get;
    } = imageData.workingFile ?? throw new ArgumentNullException(nameof(imageData.workingFile));

    /// <summary>
    /// Property to return the image to edit.
    /// </summary>
    public IGorgonImage Image
    {
        get;
    } = imageData.image ?? throw new ArgumentNullException(nameof(imageData.image));

    /// <summary>
    /// Property to return the context for the image effects.
    /// </summary>
    public IFxContext FxContext
    {
        get;
    } = fxContext ?? throw new ArgumentNullException(nameof(fxContext));

    /// <summary>
    /// Property to return the format support information for the current video card.
    /// </summary>
    public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
    {
        get;
    } = formatSupport ?? throw new ArgumentNullException(nameof(formatSupport));

    /// <summary>
    /// Property to return the information about the currently active video adapter.
    /// </summary>
    public IGorgonVideoAdapterInfo VideoAdapterInfo
    {
        get;
    } = videoAdapter ?? throw new ArgumentNullException(nameof(videoAdapter));

    /// <summary>
    /// Property to return the settings for the image editor plugin.
    /// </summary>
    public ISettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the plug in settings for the image editor.
    /// </summary>
    public ISettingsPlugins PluginSettings
    {
        get;
    } = pluginSettings ?? throw new ArgumentNullException(nameof(pluginSettings));

    /// <summary>
    /// Property to return the original format for the image.
    /// </summary>
    public BufferFormat OriginalFormat
    {
        get;
    } = imageData.originalFormat;

    /// <summary>
    /// Property to return the service used to update the image.
    /// </summary>
    public IImageUpdaterService ImageUpdater
    {
        get;
    } = services.ImageUpdater;

    /// <summary>
    /// Property to return the undo service for the editor.
    /// </summary>
    public IUndoService UndoService
    {
        get;
    } = services.UndoService;

    /// <summary>
    /// Property to return the external editor service used to update an image.
    /// </summary>
    public IImageExternalEditService ExternalEditorService
    {
        get;
    } = services.ExternalEditorService;

    #endregion
}
