﻿
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
// Created: August 28, 2021 7:27:39 PM
// 

using System.Diagnostics;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Fonts.Codecs;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using Microsoft.IO;
using Drawing = System.Drawing;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The plugin used for editing/creating fonts
/// </summary>
internal class FontEditorPlugin
    : ContentPlugIn, IContentPlugInMetadata
{

    // The attribute key name for the animation codec attribute.
    private const string CodecAttr = "AnimationCodec";

    // No thumbnail image.
    private IGorgonImage _noThumbnail;
    // The global settings for the plug in.
    private FontEditorSettings _settings = new();
    // This the view model for the settings.
    private Settings _settingsViewModel;
    // This codec for the font.
    private GorgonFontCodec _defaultCodec;
    private GorgonFontCodec _previewerCodec;
    // The factory used to generate fonts.
    private GorgonFontFactory _fontFactory;
    private GorgonFontFactory _previewerFactory;

    /// <summary>
    /// The name of the settings file.
    /// </summary>
    public static readonly string SettingsName = typeof(FontEditorPlugin).FullName;

    /// <summary>
    /// Property to return the cached fonts for the font editor.
    /// </summary>
    internal static SortedDictionary<string, Font> CachedFonts
    {
        get;
        private set;
    }

    /// <summary>Property to return the name of the plug in.</summary>
    string IContentPlugInMetadata.PlugInName => Name;

    /// <summary>Property to return the description of the plugin.</summary>
    string IContentPlugInMetadata.Description => Description;

    /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
    public override bool CanCreateContent => true;

    /// <summary>Property to return the ID of the small icon for this plug in.</summary>
    public Guid SmallIconID
    {
        get;
    }

    /// <summary>Property to return the ID of the new icon for this plug in.</summary>
    public Guid NewIconID
    {
        get;
    }

    /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
    public override string ContentTypeID => CommonEditorContentTypes.FontType;

    /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
    public string ContentType => Resources.GORFNT_TEXT_CONTEXT_TYPE;

    /// <summary>
    /// Property to return the default file extension used by files generated by this content plug in.
    /// </summary>
    /// <remarks>
    /// Plug in developers can override this to default the file name extension for their content when creating new content with <see cref="GetDefaultContentAsync(string, HashSet{string})"/>.
    /// </remarks>
    protected override GorgonFileExtension DefaultFileExtension => new(_defaultCodec.DefaultFileExtension, Resources.GORFNT_TEXT_FILE_OPEN_DESC);

    /// <summary>
    /// Function to update the font cache.
    /// </summary>
    private static void UpdateCachedFonts()
    {
        // Clear the cached fonts.
        if (CachedFonts != null)
        {
            foreach (KeyValuePair<string, Font> font in CachedFonts)
            {
                font.Value.Dispose();
            }
        }

        SortedDictionary<string, Font> fonts = new(StringComparer.OrdinalIgnoreCase);

        // Get font families for previewing - Only up to 3000, after that we have to use the default font for whatever control is making use of this.
        foreach (FontFamily family in FontFamily.Families.Take(3000))
        {
            Font newFont = null;

            if (fonts.ContainsKey(family.Name))
            {
                continue;
            }

            if (family.IsStyleAvailable(FontStyle.Regular))
            {
                newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            }
            else if (family.IsStyleAvailable(FontStyle.Bold))
            {
                newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            }
            else if (family.IsStyleAvailable(FontStyle.Italic))
            {
                newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
            }

            // Only add if we could use the regular, bold or italic style.
            if (newFont != null)
            {
                fonts.Add(family.Name, newFont);
            }
        }

        CachedFonts = fonts;
    }

    /// <summary>
    /// Function to update the metadata for a file that is missing metadata attributes.
    /// </summary>
    /// <param name="attributes">The attributes to update.</param>
    /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
    private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
    {
        bool needsRefresh = false;

        if ((attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string currentContentType))
            && (string.Equals(currentContentType, ContentTypeID, StringComparison.OrdinalIgnoreCase)))
        {
            attributes.Remove(CommonEditorConstants.ContentTypeAttr);
            needsRefresh = true;
        }

        if ((!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out currentContentType))
            || (!string.Equals(currentContentType, CommonEditorContentTypes.FontType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes[CommonEditorConstants.ContentTypeAttr] = ContentTypeID;
            needsRefresh = true;
        }

        return needsRefresh;
    }

    /// <summary>Function to register plug in specific search keywords with the system search.</summary>
    /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="searchService">The search service to use for registration.</param>
    protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService)
    {
        // TODO: Maybe have font sizes, modifiers (bold/underline/etc...), and faces?
    }

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
    protected override ISettingsCategory OnGetSettings() => _settingsViewModel;

    /// <summary>Function to retrieve the default content name, and data.</summary>
    /// <param name="generatedName">A default name generated by the application.</param>
    /// <param name="metadata">Custom metadata for the content.</param>
    /// <returns>The default content name along with the content data serialized as a byte array. If either the name or data are <b>null</b>, then the user cancelled..</returns>
    /// <remarks>
    ///   <para>
    /// Plug in authors may override this method so a custom UI can be presented when creating new content, or return a default set of data and a default name, or whatever they wish.
    /// </para>
    ///   <para>
    /// If an empty string (or whitespace) is returned for the name, then the <paramref name="generatedName" /> will be used.
    /// </para>
    /// </remarks>
    protected override Task<(string name, RecyclableMemoryStream data)> OnGetDefaultContentAsync(string generatedName, ProjectItemMetadata metadata)
    {
        metadata.Attributes[CodecAttr] = _defaultCodec.GetType().FullName;
        metadata.Attributes[CommonEditorConstants.IsNewAttr] = bool.TrueString;

        string fontDirectory = ContentFileManager.CurrentDirectory.FormatDirectory('/');

        using FontService fontService = new(null, null, _defaultCodec, ContentFileManager);

        (string newName, GorgonFontInfo fontInfo) = fontService.GetNewFontInfo(generatedName, fontDirectory, null);

        if (fontInfo is null)
        {
            return Task.FromResult<(string, RecyclableMemoryStream)>((string.Empty, null));
        }

        using GorgonFont font = _fontFactory.GetFont(fontInfo);

        RecyclableMemoryStream stream = CommonEditorResources.MemoryStreamManager.GetStream() as RecyclableMemoryStream;
        _defaultCodec.Save(font, stream);

        return Task.FromResult<(string, RecyclableMemoryStream)>((newName, stream));
    }

    /// <summary>Function to open a content object from this plugin.</summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name = "fileManager" > The file manager used to access other content files.</param>
    /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
    /// <param name="undoService">The undo service for the plug in.</param>
    /// <returns>A new IEditorContent object.</returns>
    /// <remarks>
    /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
    /// application or plug in is shut down, and is not stored with the project.
    /// </remarks>
    protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
    {
        FontContent content = new();
        Stream stream = null;

        try
        {
            // Load the sprite now. 
            stream = ContentFileManager.OpenStream(file.Path, FileMode.Open);

            GorgonFont font = await _defaultCodec.FromStreamAsync(stream, file.Path);

            FontService fontService = new(_fontFactory, font, _defaultCodec, fileManager);

            Settings settings = new();
            settings.Initialize(new SettingsParameters(_settings, HostContentServices));

            FontOutline fontOutline = new();
            fontOutline.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontTextureSize fontTextureSize = new();
            fontTextureSize.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontPadding fontPadding = new();
            fontPadding.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontCharacterSelection fontCharSelection = new();
            fontCharSelection.Initialize(new FontCharacterSelectionParameters(fontService, HostContentServices));

            FontSolidBrush fontSolidBrush = new();
            fontSolidBrush.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontPatternBrush fontPatternBrush = new();
            fontPatternBrush.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontGradientBrush fontGradientBrush = new();
            fontGradientBrush.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            FontTextureBrush fontTextureBrush = new();
            fontTextureBrush.Initialize(new FontTextureBrushParameters(new ImageLoadService(new GorgonCodecDds(), fileManager), HostContentServices));

            TextureEditorContext textureEditor = new();
            textureEditor.Initialize(new TextureEditorContextParameters(fontService,
                                                                                        fontTextureSize,
                                                                                        fontPadding,
                                                                                        fontSolidBrush,
                                                                                        fontPatternBrush,
                                                                                        fontGradientBrush,
                                                                                        fontTextureBrush,
                                                                                        undoService,
                                                                                        HostContentServices));

            content.Initialize(new FontContentParameters(fontService,
                                                                        settings,
                                                                        fontOutline,
                                                                        textureEditor,
                                                                        fontCharSelection,
                                                                        undoService,
                                                                        fileManager,
                                                                        file,
                                                                        HostContentServices));

            return content;
        }
        finally
        {
            stream?.Dispose();
        }
    }

    /// <summary>Function to provide clean up for the plugin.</summary>
    protected override void OnShutdown()
    {
        _previewerFactory?.Dispose();
        _fontFactory?.Dispose();

        // This being an image resource, we should dispose of it right away.
        _noThumbnail?.Dispose();

        // If our application had settings, this is what we'd use to write the settings back to the disk.
        if (_settings is not null)
        {
            // Persist any settings.
            HostContentServices.ContentPlugInService.WriteContentSettings(SettingsName, _settings);
        }

        // And finally, always unregister the view model + view linkage.
        ViewFactory.Unregister<IFontContent>();
        //ViewFactory.Unregister<ITextContent>();

        base.OnShutdown();
    }

    /// <summary>Function to provide initialization for the plugin.</summary>
    /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        _fontFactory = new GorgonFontFactory(HostContentServices.GraphicsContext.Graphics);
        _previewerFactory = new GorgonFontFactory(HostContentServices.GraphicsContext.Graphics);
        _defaultCodec = new GorgonCodecGorFont(_fontFactory);
        _previewerCodec = new GorgonCodecGorFont(_previewerFactory);

        // This will contain our default configuration for the plug in and editor(s) contained within.
        FontEditorSettings settings = HostContentServices.ContentPlugInService.ReadContentSettings<FontEditorSettings>(SettingsName);
        if (settings is not null)
        {
            _settings = settings;
        }

        // Setup a view model for the plug in settings.
        // We can use this to adjust or retrieve the plug in settings for the plug in from within our view model(s).
        _settingsViewModel = new Settings();
        _settingsViewModel.Initialize(new SettingsParameters(_settings, HostContentServices));

        _noThumbnail = Resources.font_missing_96x96.ToGorgonImage();
        ViewFactory.Register<IFontContent>(() => new FontContentView
        {
            FontFactory = _fontFactory
        });
        //ViewFactory.Register<ISettings>(() => new TextContentSettingsPanel());
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
        if (!_defaultCodec.IsReadable(stream))
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
    public Image GetSmallIcon() => Resources.font_20x20;

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

        if (!ContentFileManager.FileExists(contentFile.Path))
        {
            return null;
        }

        using Stream fileStream = ContentFileManager.OpenStream(contentFile.Path, FileMode.Open);
        if (!_defaultCodec.IsReadable(fileStream))
        {
            return _noThumbnail.Clone();
        }

        fileStream.Position = 0;

        GorgonGraphics graphics = HostContentServices.GraphicsContext.Graphics;
        Gorgon2D renderer = HostContentServices.GraphicsContext.Renderer2D;

        using Drawing.Graphics gDpi = Drawing.Graphics.FromHwnd(GorgonApplication.MainForm.Handle);
        float dpiScale = gDpi.DpiX / 96.0f;
        GorgonPoint targetSize = new Vector2(256 * dpiScale, 256 * dpiScale).ToSize2();

        using GorgonFont font = await _previewerCodec.FromStreamAsync(fileStream, filePath);
        using GorgonRenderTarget2DView target = GorgonRenderTarget2DView.CreateRenderTarget(HostContentServices.GraphicsContext.Graphics, new GorgonTexture2DInfo(targetSize.X, targetSize.Y, BufferFormat.R8G8B8A8_UNorm)
        {
            Binding = TextureBinding.ShaderResource
        });

        target.Clear(GorgonColors.White);
        graphics.SetRenderTarget(target);
        renderer.Begin(font.UsePremultipliedTextures ? Gorgon2DBatchState.PremultipliedBlend : null);
        renderer.DrawRectangle(new GorgonRectangleF(0, 0, target.Width, target.Height), GorgonColors.Black, 2);
        if (!font.HasOutline)
        {
            renderer.DrawString(string.Format(Resources.GORFNT_TEXT_PREVIEW, contentFile.Name), new Vector2(3, 3), font, GorgonColors.Black);
        }
        else
        {
            renderer.DrawString(string.Format(Resources.GORFNT_TEXT_PREVIEW, contentFile.Name), new Vector2(3, 3), font, GorgonColors.White);
        }
        renderer.End();

        _previewerFactory.InvalidateCache();

        return target.Texture.ToImage();
    }

    /// <summary>Function to retrieve the icon used for new content creation.</summary>
    /// <returns>An image for the icon.</returns>
    public Image GetNewIcon() => Resources.font_24x24;

    // When we construct the plug in object, we'll need to send back a friendly description 
    // for display purposes.

    /// <summary>Initializes a new instance of the ImageEditorPlugIn class.</summary>
    public FontEditorPlugin()
        : base(Resources.GORFNT_TEXT_PLUGIN_DESC)
    {
        SmallIconID = Guid.NewGuid();
        NewIconID = Guid.NewGuid();
    }

    /// <summary>Initializes static members of the <see cref="FontEditorPlugin" /> class.</summary>
    static FontEditorPlugin() => UpdateCachedFonts();

}
