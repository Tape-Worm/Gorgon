
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
// Created: March 2, 2019 11:15:34 AM
// 

using Gorgon.Editor.Content;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers.Services;
using Gorgon.UI.OLDE;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// A plugin used to create a texture atlas
/// </summary>
/// <remarks>
/// <para>
/// This plugin varies from the Image atlas tool in that it uses sprites that are already defined and bound to separate images
/// </para>
/// </remarks>
internal class TextureAtlasToolPlugin
    : ToolPlugin
{

    // The cached button definition.
    private ToolPluginRibbonButton _button;
    // The default image codec to use.
    private IGorgonImageCodec _defaultImageCodec;
    // The default sprite codec to use.
    private IGorgonSpriteCodec _defaultSpriteCodec;
    // The view model for the sprite file browser.
    private SpriteFiles _fileVm;
    // The texture atlas view model.
    private TextureAtlas _textureAtlas;

    /// <summary>
    /// Function to retrieve the sprite file entries from the file system.
    /// </summary>
    /// <returns>The flattened list of entries used for searching and the file system entry hierarchy.</returns>
    private (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> fileSystemEntries) GetFileEntries()
    {
        List<IContentFileExplorerSearchEntry> searchEntries = [];
        List<ContentFileExplorerDirectoryEntry> fileSystemEntries = [];
        ContentFileExplorerDirectoryEntry dirEntry = null;
        List<ContentFileExplorerFileEntry> fileEntries = [];
        IEnumerable<string> dirs = ContentFileManager.EnumerateDirectories("/", "*", true);
        IEnumerable<IContentFile> spriteFiles = ContentFileManager.EnumerateContentFiles("/", "*")
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                            && (item.Metadata.ContentMetadata is not null)
                                            && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));
        IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();

        if (spriteFiles.Any())
        {
            dirEntry = new ContentFileExplorerDirectoryEntry("/", fileEntries);
            fileSystemEntries.Add(dirEntry);
            searchEntries.Add(dirEntry);

            foreach (IContentFile file in spriteFiles)
            {
                ContentFileExplorerFileEntry fileEntry = new(file, dirEntry);
                if (selectedFiles.Any(item => string.Equals(item, file.Path, StringComparison.OrdinalIgnoreCase)))
                {
                    fileEntry.IsSelected = true;
                }
                fileEntries.Add(fileEntry);
                searchEntries.Add(fileEntry);
            }
        }

        foreach (string subDir in dirs)
        {
            spriteFiles = ContentFileManager.EnumerateContentFiles(subDir, "*")
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                        && (item.Metadata.ContentMetadata is not null)
                                                        && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));
            if (!spriteFiles.Any())
            {
                continue;
            }

            fileEntries = [];
            dirEntry = new ContentFileExplorerDirectoryEntry(subDir, fileEntries);

            fileSystemEntries.Add(dirEntry);
            searchEntries.Add(dirEntry);

            foreach (IContentFile file in spriteFiles)
            {
                ContentFileExplorerFileEntry fileEntry = new(file, dirEntry);
                if (selectedFiles.Any(item => string.Equals(item, file.Path, StringComparison.OrdinalIgnoreCase)))
                {
                    fileEntry.IsSelected = true;
                }
                fileEntries.Add(fileEntry);
                searchEntries.Add(fileEntry);
            }
        }

        return (searchEntries, fileSystemEntries);
    }

    /// <summary>
    /// Function to show the tool form.
    /// </summary>
    private void ShowForm()
    {
        TextureAtlasSettings settings;
        FormAtlasGen form = null;

        HostToolServices.BusyService.SetBusy();

        try
        {
            settings = HostToolServices.ToolPluginService.ReadContentSettings<TextureAtlasSettings>(typeof(TextureAtlasToolPlugin).FullName);

            settings ??= new TextureAtlasSettings();

            (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> entries) = GetFileEntries();

            _fileVm ??= new SpriteFiles();

            _textureAtlas ??= new TextureAtlas();

            _fileVm.Initialize(new SpriteFilesParameters(entries, TemporaryFileSystem, new EditorContentSearchService(searchEntries), HostToolServices));
            _textureAtlas.Initialize(new TextureAtlasParameters(_fileVm,
                                                                settings,
                                                                new GorgonTextureAtlasService(HostToolServices.GraphicsContext.Renderer2D),
                                                                new FileIOService(ContentFileManager, _defaultImageCodec, _defaultSpriteCodec),
                                                                ContentFileManager,
                                                                HostToolServices));

            form = new FormAtlasGen(settings);
            form.SetDataContext(_textureAtlas);
            form.SetupGraphics(HostToolServices.GraphicsContext, true);

            HostToolServices.BusyService.SetIdle();
            form.ShowDialog(GorgonApplication.MainForm);

            HostToolServices.ToolPluginService.WriteContentSettings(typeof(TextureAtlasToolPlugin).FullName, settings);
        }
        catch (Exception ex)
        {
            HostToolServices.MessageDisplay.ShowError(ex, Resources.GORTAG_ERR_LAUNCH);
        }
        finally
        {
            HostToolServices.BusyService.SetIdle();
            form?.Dispose();
        }
    }

    /// <summary>Function to retrieve the ribbon button for the tool.</summary>
    /// <returns>A new tool ribbon button instance.</returns>
    /// <remarks>
    ///   <para>
    /// Tool plugin developers must override this method to return the button which is inserted on the application ribbon, under the "Tools" tab. If the method returns <b>null</b>, then the tool is
    /// ignored.
    /// </para>
    ///   <para>
    /// The resulting data structure will contain the means to handle the click event for the tool, and as such, is the only means of communication between the main UI and the plugin.
    /// </para>
    /// </remarks>
    protected override IToolPluginRibbonButton OnGetToolButton()
    {
        _button.ClickCallback ??= ShowForm;

        return _button;
    }

    /// <summary>Function to provide initialization for the Plugin.</summary>
    /// <remarks>This method is only called when the Plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        _defaultImageCodec = new GorgonCodecDds();
        _defaultSpriteCodec = new GorgonV3SpriteBinaryCodec(HostToolServices.GraphicsContext.Renderer2D);

        _button = new ToolPluginRibbonButton(Resources.GORTAG_TEXT_BUTTON, Resources.texture_atlas_48x48, Resources.texture_atlas_16x16, Resources.GORTAG_GROUP_BUTTON)
        {
            Description = Resources.GORTAG_DESC_BUTTON
        };
        _button.ValidateButton();
    }

    /// <summary>Function to provide clean up for the Plugin.</summary>
    protected override void OnShutdown()
    {
        // Disconnect from the button to ensure that we don't get this thing keeping us around longer than we should.
        if (_button is not null)
        {
            _button.ClickCallback = null;
            _button.Dispose();
        }

        base.OnShutdown();
    }

    /// <summary>Initializes a new instance of the <see cref="TextureAtlasToolPlugin"/> class.</summary>
    public TextureAtlasToolPlugin()
        : base(Resources.GORTAG_plugin_DESC)
    {
    }
}
