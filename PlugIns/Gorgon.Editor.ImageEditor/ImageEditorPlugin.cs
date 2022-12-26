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
// Created: October 29, 2018 2:52:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Drawing = System.Drawing;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Fx;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Editor.ImageEditor.Native;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Gorgon image editor content plug in interface.
/// </summary>
internal class ImageEditorPlugIn
    : ContentPlugIn, IContentPlugInMetadata
{
    #region Variables.
    // This is the only codec supported by the image plug in.  Images will be converted when imported.
    private readonly GorgonCodecDds _ddsCodec = new();

    // The synchronization lock for threads.
    private readonly object _syncLock = new();

    // The codec registry.
    private ICodecRegistry _codecs;

    // The plug in settings.
    private ISettings _settings;
    private ISettingsPlugins _pluginSettings;

    // No thumbnail image.
    private IGorgonImage _noThumbnail;

    // The effects services for the image editor.
    private FxService _fxServices;

    /// <summary>
    /// The name of the settings file.
    /// </summary>
    public static readonly string SettingsName = typeof(ImageEditorPlugIn).FullName;
    #endregion

    #region Properties.
    /// <summary>Property to return the name of the plug in.</summary>
    string IContentPlugInMetadata.PlugInName => Name;

    /// <summary>Property to return the description of the plugin.</summary>
    string IContentPlugInMetadata.Description => Description;

    /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
    public override bool CanCreateContent => false;

    /// <summary>Property to return the ID of the small icon for this plug in.</summary>
    public Guid SmallIconID
    {
        get;
    }

    /// <summary>Property to return the ID of the new icon for this plug in.</summary>
    public Guid NewIconID => Guid.Empty;

    /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
    public override string ContentTypeID => CommonEditorContentTypes.ImageType;

    /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
    public string ContentType => Resources.GORIMG_CONTENT_TYPE;        
    #endregion

    #region Methods.
    /// <summary>
    /// Function to render the thumbnail into the image passed in.
    /// </summary>
    /// <param name="image">The image to render the thumbnail into.</param>
    /// <param name="scale">The scale of the image.</param>
    private void RenderThumbnail(ref IGorgonImage image, float scale)
    {
        lock (_syncLock)
        {
            using GorgonTexture2D texture = image.ToTexture2D(HostContentServices.GraphicsContext.Graphics, new GorgonTexture2DLoadOptions
            {
                Usage = ResourceUsage.Immutable,
                IsTextureCube = false
            });
            using var rtv = GorgonRenderTarget2DView.CreateRenderTarget(HostContentServices.GraphicsContext.Graphics, new GorgonTexture2DInfo((int)(image.Width * scale),
                                                                                                                                                                       (int)(image.Height * scale),
                                                                                                                                                                       BufferFormat.R8G8B8A8_UNorm)
            {
                ArrayCount = 1,
                Binding = TextureBinding.ShaderResource,
                MipLevels = 1,
                Usage = ResourceUsage.Default
            });
            GorgonTexture2DView view = texture.GetShaderResourceView(mipCount: 1, arrayCount: 1);
            rtv.Clear(GorgonColor.BlackTransparent);
            HostContentServices.GraphicsContext.Graphics.SetRenderTarget(rtv);
            HostContentServices.GraphicsContext.Blitter.Blit(view, new DX.Rectangle(0, 0, rtv.Width, rtv.Height), blendState: GorgonBlendState.Default, samplerState: GorgonSamplerState.Default);
            HostContentServices.GraphicsContext.Graphics.SetRenderTarget(null);

            image?.Dispose();
            image = rtv.Texture.ToImage();
        }
    }

    /// <summary>
    /// Function to retrieve the path to the texture converted used to convert compressed images.
    /// </summary>
    /// <returns>The file info for the texture converter file.</returns>
    private FileInfo GetTexConvExe()
    {
        FileInfo result;

        // The availability of texconv.exe determines whether or not we can use block compressed formats or not.
        HostContentServices.Log.Print("Checking for texconv.exe...", LoggingLevel.Simple);
        var pluginDir = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
        result = new FileInfo(Path.Combine(pluginDir.FullName, "texconv.exe"));

        if (!result.Exists)
        {
            HostContentServices.Log.Print($"WARNING: Texconv.exe was not found at {pluginDir.FullName}. Block compressed formats will be unavailable.", LoggingLevel.Simple);
        }
        else
        {
            HostContentServices.Log.Print($"Found texconv.exe at '{result.FullName}'.", LoggingLevel.Simple);
        }

        return result;
    }

    /// <summary>
    /// Function to load the image to be used a thumbnail.
    /// </summary>
    /// <param name="thumbnailCodec">The codec for the thumbnail images.</param>
    /// <param name="thumbnailFile">The path to the thumbnail file.</param>
    /// <param name="content">The content being thumbnailed.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    /// <returns>The thumbnail image, and a flag to indicate whether the thumbnail needs conversion.</returns>
    private (IGorgonImage thumbnailImage, bool needsConversion) LoadThumbNailImage(IGorgonImageCodec thumbnailCodec, string thumbnailFile, IContentFile content, CancellationToken cancelToken)
    {
        IGorgonImage result;
        Stream inStream = null;

        try
        {
            IGorgonVirtualFile file = TemporaryFileSystem.FileSystem.GetFile(thumbnailFile);

            // If we've already got the file, then leave.
            if (file is not null)
            {
                inStream = file.OpenStream();
                result = thumbnailCodec.FromStream(inStream);

                return cancelToken.IsCancellationRequested ? (null, false) : (result, false);
            }

            inStream = ContentFileManager.OpenStream(content.Path, FileMode.Open);
            result = _ddsCodec.FromStream(inStream);

            return (result, true);
        }
        catch (Exception ex)
        {
            HostContentServices.Log.Print($"ERROR: Cannot create thumbnail for '{content.Path}'", LoggingLevel.Intermediate);
            HostContentServices.Log.LogException(ex);
            return (null, false);
        }
        finally
        {
            inStream?.Dispose();
        }
    }

    /// <summary>
    /// Function to update the metadata for a file that is missing metadata attributes.
    /// </summary>
    /// <param name="attributes">The attributes to update.</param>
    /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
    private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
    {
        bool needsRefresh = false;

        if ((attributes.TryGetValue(ImageContent.CodecAttr, out string currentCodecType))
            && (!string.IsNullOrWhiteSpace(currentCodecType)))
        {
            attributes.Remove(ImageContent.CodecAttr);
            needsRefresh = true;
        }

        if ((attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string currentContentType))
            && (string.Equals(currentContentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes.Remove(CommonEditorConstants.ContentTypeAttr);
            needsRefresh = true;
        }

        string codecType = _ddsCodec.GetType().FullName;
        if ((!attributes.TryGetValue(ImageContent.CodecAttr, out currentCodecType))
            || (!string.Equals(currentCodecType, codecType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes[ImageContent.CodecAttr] = codecType;
            needsRefresh = true;
        }

        if ((!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out currentContentType))
            || (!string.Equals(currentContentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes[CommonEditorConstants.ContentTypeAttr] = CommonEditorContentTypes.ImageType;
            needsRefresh = true;
        }

        return needsRefresh;
    }

    /// <summary>Function to register plug in specific search keywords with the system search.</summary>
    /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="searchService">The search service to use for registration.</param>
    protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService) => searchService.MapKeywordToContentAttribute(Resources.GORIMG_SEARCH_KEYWORD_CODEC, ImageContent.CodecAttr);


    /// <summary>Function to retrieve the settings interface for this plug in.</summary>
    /// <returns>The settings interface view model.</returns>
    /// <remarks>
    ///   <para>
    /// Implementors who wish to supply customizable settings for their plug ins from the main "Settings" area in the application can override this method and return a new view model based on
    /// the base <see cref="ISettingsCategory"/> type. Returning <b>null</b> will mean that the plug in does not have settings that can be managed externally.
    /// </para>
    ///   <para>
    /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{Control})"/> method when the plug in first loaded,
    /// or else the panel will not show in the main settings area.
    /// </para>
    /// </remarks>
    protected override ISettingsCategory OnGetSettings() => _settings;

    /// <summary>Function to open a content object from this plugin.</summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name = "fileManager" > The file manager used to access other content files.</param>
    /// <param name="injector">Parameters for injecting dependency objects.</param>
    /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
    /// <param name="undoService">The undo service for the plug in.</param>
    /// <returns>A new IEditorContent object.</returns>
    /// <remarks>
    /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
    /// application or plug in is shut down, and is not stored with the project.
    /// </remarks>
    protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
    {
        FileInfo texConvExe = GetTexConvExe();
        TexConvCompressor compressor = null;

        if (texConvExe.Exists)
        {
            compressor = new TexConvCompressor(texConvExe, scratchArea, _ddsCodec);
        }

        var imageIO = new ImageIOService(_ddsCodec,
            _codecs,
            new ExportImageDialogService(_settings),
            new ImportImageDialogService(_settings, _codecs),
            _noThumbnail,
            HostContentServices.BusyService,
            scratchArea,
            compressor,
            HostContentServices.Log);

        var imageData = await Task.Run(() =>
        {
            using Stream inStream = ContentFileManager.OpenStream(file.Path, FileMode.Open);
            return imageIO.LoadImageFile(inStream, file.Name);
        });

        var services = new ImageEditorServices
        {                
            HostContentServices = HostContentServices,
            ImageIO = imageIO,
            UndoService = undoService,
            ImageUpdater = new ImageUpdaterService(),
            ExternalEditorService = new ImageExternalEditService(HostContentServices.Log)
        };

        var imagePicker = new ImagePicker();
        var sourceImagePicker = new SourceImagePicker();
        var cropResizeSettings = new CropResizeSettings();
        var dimensionSettings = new DimensionSettings();
        var mipSettings = new MipMapSettings();
        var alphaSettings = new AlphaSettings
        {
            AlphaValue = _settings.LastAlphaValue,
            UpdateRange = _settings.LastAlphaRange
        };
        var blurSettings = new FxBlur();
        var sharpenSettings = new FxSharpen();
        var embossSettings = new FxEmboss();
        var edgeDetectSettings = new FxEdgeDetect();
        var posterizeSettings = new FxPosterize();
        var oneBitSettings = new FxOneBit();

        var injector = new HostedPanelViewModelParameters(HostContentServices);
        
        cropResizeSettings.Initialize(injector);
        dimensionSettings.Initialize(new DimensionSettingsParameters(HostContentServices));
        mipSettings.Initialize(injector);
        sourceImagePicker.Initialize(new SourceImagePickerParameters(HostContentServices));            
        blurSettings.Initialize(injector);
        sharpenSettings.Initialize(injector);
        embossSettings.Initialize(injector);
        edgeDetectSettings.Initialize(injector);
        posterizeSettings.Initialize(injector);

        
        imagePicker.Initialize(new ImagePickerParameters(fileManager, file, HostContentServices)
        {
            ImageServices = services,
            CropResizeSettings = cropResizeSettings,
            Settings = _settings,
            SourceImagePicker = sourceImagePicker
        });

        var content = new ImageContent();
        var fxContext = new FxContext();

        fxContext.Initialize(new FxContextParameters(content, _fxServices, blurSettings, sharpenSettings, embossSettings, edgeDetectSettings, posterizeSettings, oneBitSettings, HostContentServices));
        content.Initialize(new ImageContentParameters(fileManager,
            file,
            _settings,
            _pluginSettings,
            imagePicker,
            cropResizeSettings,
            dimensionSettings,
            mipSettings,
            alphaSettings,
            fxContext,
            imageData,
            HostContentServices.GraphicsContext.Graphics.VideoAdapter,
            HostContentServices.GraphicsContext.Graphics.FormatSupport,
            services));

        return content;
    }

    /// <summary>Function to provide clean up for the plugin.</summary>
    protected override void OnShutdown()
    {
        _fxServices?.Dispose();
        _noThumbnail?.Dispose();

        HostContentServices.ContentPlugInService.WriteContentSettings(typeof(ImageEditorPlugIn).FullName, _settings);

        ViewFactory.Unregister<IImageContent>();
        ViewFactory.Unregister<ISettings>();

        base.OnShutdown();
    }

    /// <summary>Function to provide initialization for the plugin.</summary>
    /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        ViewFactory.Register<IImageContent>(() => new ImageEditorView());
        (_codecs, _settings, _pluginSettings) = SharedDataFactory.GetSharedData(HostContentServices);

        _fxServices = new FxService(HostContentServices.GraphicsContext);
        _fxServices.LoadResources();

        _noThumbnail = Resources.no_thumb_64x64.ToGorgonImage();

        ViewFactory.Register<ISettings>(() => new ImageSettingsPanel());
    }

    /// <summary>Function to determine if the content plugin can open the specified file.</summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>
    ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    public bool CanOpenContent(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        IContentFile file = ContentFileManager.GetFile(filePath);

        Debug.Assert(file is not null, $"File '{filePath}' doesn't exist, but it should!");

        using Stream stream = ContentFileManager.OpenStream(filePath, FileMode.Open);
        if (!_ddsCodec.IsReadable(stream))
        {
            return false;
        }

        IGorgonImageInfo metadata = _ddsCodec.GetMetaData(stream);

        // We won't be supporting 1D images in this editor.
        if (metadata.ImageType is ImageType.Image1D or ImageType.Unknown)
        {
            return false;
        }

        UpdateFileMetadataAttributes(file.Metadata.Attributes);
        return true;
    }

    /// <summary>
    /// Function to retrieve the small icon for the content plug in.
    /// </summary>
    /// <returns>An image for the small icon.</returns>
    public Drawing.Image GetSmallIcon() => Resources.image_20x20;

    /// <summary>Function to retrieve a thumbnail for the content.</summary>
    /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
    /// <param name="filePath">The path to the thumbnail file to write into.</param>
    /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the thumbnail image data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile" />, or the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
    public async Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, string filePath, CancellationToken cancelToken)
    {
        if (contentFile is null)
        {
            throw new ArgumentNullException(nameof(contentFile));
        }

        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        // If the content is not a DDS image, then leave it.
        if ((!contentFile.Metadata.Attributes.TryGetValue(ImageContent.CodecAttr, out string codecName))
            || (string.IsNullOrWhiteSpace(codecName))
            || (!string.Equals(codecName, _ddsCodec.GetType().FullName, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        string fileDirectoryPath = Path.GetDirectoryName(filePath).FormatDirectory('/');
        IGorgonVirtualDirectory directory = TemporaryFileSystem.FileSystem.GetDirectory(fileDirectoryPath);

        if (directory is null)
        {
            directory = TemporaryFileSystem.CreateDirectory(fileDirectoryPath);
        }

        IGorgonImageCodec pngCodec = new GorgonCodecPng();

        (IGorgonImage thumbImage, bool needsConversion) = await Task.Run(() => LoadThumbNailImage(pngCodec, filePath, contentFile, cancelToken));

        if ((thumbImage is null) || (cancelToken.IsCancellationRequested))
        {
            return null;
        }

        if (!needsConversion)
        {
            return thumbImage;
        }

        // We need to switch back to the main thread here to render the image, otherwise things will break.
        Cursor.Current = Cursors.WaitCursor;

        try
        {
            const float maxSize = 256;
            float scale = (maxSize / thumbImage.Width).Min(maxSize / thumbImage.Height);
            RenderThumbnail(ref thumbImage, scale);

            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            // We're done on the main thread, we can switch to another thread to write the image.
            Cursor.Current = Cursors.Default;

            await Task.Run(() => {
                using Stream stream = TemporaryFileSystem.OpenStream(filePath, FileMode.Create);
                pngCodec.Save(thumbImage, stream);
            }, cancelToken);

            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            contentFile.Metadata.Thumbnail = Path.GetFileName(filePath);
            return thumbImage;
        }
        catch (Exception ex)
        {
            HostContentServices.Log.Print($"ERROR: Cannot create thumbnail for '{contentFile.Path}'", LoggingLevel.Intermediate);
            HostContentServices.Log.LogException(ex);
            return null;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>Function to retrieve the icon used for new content creation.</summary>
    /// <returns>An image for the icon.</returns>
    public Drawing.Image GetNewIcon() => null;
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the ImageEditorPlugIn class.</summary>
    public ImageEditorPlugIn()
        : base(Resources.GORIMG_DESC) => SmallIconID = Guid.NewGuid();
    #endregion
}
