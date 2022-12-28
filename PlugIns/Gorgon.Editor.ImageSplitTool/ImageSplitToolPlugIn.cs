#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 2, 2019 11:15:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageSplitTool.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ImageSplitTool;

/// <summary>
/// A plug in used to split a texture atlas up by using the sprites associated with it.
/// </summary>
internal class ImageSplitToolPlugIn
    : ToolPlugIn
{
    #region Variables.
    // The cached button definition.
    private ToolPlugInRibbonButton _button;
    #endregion

    #region Methods.		
    /// <summary>
    /// Function to retrieve the sprite file entries from the file system.
    /// </summary>
    /// <returns>The flattened list of entries used for searching, the file system entry hierarchy and a list of image files and associated sprites.</returns>
    private (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> fileSystemEntries, IReadOnlyDictionary<IContentFile, IReadOnlyList<IContentFile>> imagesAndSprites) GetFileEntries()
    {
        var searchEntries = new List<IContentFileExplorerSearchEntry>();
        var fileSystemEntries = new List<ContentFileExplorerDirectoryEntry>();
        ContentFileExplorerDirectoryEntry dirEntry = null;
        var fileEntries = new List<ContentFileExplorerFileEntry>();
        IEnumerable<string> dirs = ContentFileManager.EnumerateDirectories("/", "*", true);
        IEnumerable<IContentFile> imageFiles = ContentFileManager.EnumerateContentFiles("/", "*")
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                        && (item.Metadata.ContentMetadata is not null)
                                                        && (string.Equals(fileType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));
        IEnumerable<IContentFile> spriteFiles = ContentFileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                        && (item.Metadata.ContentMetadata is not null)
                                                        && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));
        IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();
        var imagesAndSprites = new Dictionary<IContentFile, IReadOnlyList<IContentFile>>();

        // Function to gather all sprite dependencies for the image files.
        void GatherSpriteDependencies()
        {
            foreach (IContentFile imageFile in imageFiles)
            {
                List<IContentFile> spriteContentFiles;

                if (!imagesAndSprites.TryGetValue(imageFile, out IReadOnlyList<IContentFile> dependencyFiles))
                {
                    spriteContentFiles = new List<IContentFile>();
                    imagesAndSprites[imageFile] = spriteContentFiles;
                }
                else
                {
                    spriteContentFiles = (List<IContentFile>)dependencyFiles;
                }

                foreach (IContentFile spriteFile in spriteFiles)
                {
                    if ((spriteFile.Metadata.DependsOn.Count == 0)
                        || (!spriteFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> images))
                        || (!images.Any(item => string.Equals(imageFile.Path, item, StringComparison.OrdinalIgnoreCase))))
                    {
                        continue;
                    }

                    spriteContentFiles.Add(spriteFile);
                }
            }
        }

        GatherSpriteDependencies();

        dirEntry = new ContentFileExplorerDirectoryEntry("/", fileEntries);
        fileSystemEntries.Add(dirEntry);
        searchEntries.Add(dirEntry);

        foreach (IContentFile file in imageFiles)
        {
            if ((!imagesAndSprites.TryGetValue(file, out IReadOnlyList<IContentFile> spriteDepends))
                || (spriteDepends.Count < 2))
            {
                continue;
            }                

            var fileEntry = new ContentFileExplorerFileEntry(file, dirEntry);
            if (selectedFiles.Any(item => string.Equals(item, file.Path, StringComparison.OrdinalIgnoreCase)))
            {
                fileEntry.IsSelected = true;
            }
            fileEntries.Add(fileEntry);
            searchEntries.Add(fileEntry);
        }

        foreach (string subDir in dirs)
        {
            imageFiles = ContentFileManager.EnumerateContentFiles(subDir, "*")
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                        && (item.Metadata.ContentMetadata is not null)
                                                        && (string.Equals(fileType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));

            if (!imageFiles.Any())
            {
                continue;
            }

            GatherSpriteDependencies();

            fileEntries = new List<ContentFileExplorerFileEntry>();
            dirEntry = new ContentFileExplorerDirectoryEntry(subDir, fileEntries);

            foreach (IContentFile file in imageFiles)
            {
                if ((!imagesAndSprites.TryGetValue(file, out IReadOnlyList<IContentFile> spriteDepends))
                    || (spriteDepends.Count < 2))
                {
                    continue;
                }

                var fileEntry = new ContentFileExplorerFileEntry(file, dirEntry);
                if (selectedFiles.Any(item => string.Equals(item, file.Path, StringComparison.OrdinalIgnoreCase)))
                {
                    fileEntry.IsSelected = true;
                }
                fileEntries.Add(fileEntry);
                searchEntries.Add(fileEntry);
            }

            if (fileEntries.Count > 0)
            {
                fileSystemEntries.Add(dirEntry);
                searchEntries.Add(dirEntry);
            }
        }

        return (searchEntries, fileSystemEntries, imagesAndSprites);
    }

    /// <summary>
    /// Function to determine if the form can be shown.
    /// </summary>
    /// <returns><b>true</b> if the form can be shown, <b>false</b> if not.</returns>
    private bool CanShowForm()
    {
        IEnumerable<IContentFile> imageFiles = ContentFileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                            && (!item.IsOpen)
                                                            && (string.Equals(fileType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));
        IEnumerable<IContentFile> spriteFiles = ContentFileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                            && (!item.IsOpen)
                                                            && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));

        return (spriteFiles.Any()) && (imageFiles.Any());
    }

    /// <summary>
    /// Function to show the tool form.
    /// </summary>
    private void ShowForm()
    {
        ImageSplitToolSettings settings;
        FormImageSelector form = null;
        Stream fileStream = null;

        HostToolServices.BusyService.SetBusy();

        try
        {
            settings = HostToolServices.ToolPlugInService.ReadContentSettings<ImageSplitToolSettings>(typeof(ImageSplitToolPlugIn).FullName);

            settings ??= new ImageSplitToolSettings();

            if ((string.IsNullOrWhiteSpace(settings.LastOutputDir)) || (!ContentFileManager.DirectoryExists(settings.LastOutputDir)))
            {
                settings.LastOutputDir = ContentFileManager.CurrentDirectory;
            }

            (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> entries, IReadOnlyDictionary<IContentFile, IReadOnlyList<IContentFile>> imagesAndSprites) = GetFileEntries();

            var splitter = new ImageSelection();

            var textureSplitterService = new TextureAtlasSplitter(HostToolServices.GraphicsContext.Renderer2D, 
                                                                  imagesAndSprites,
                                                                  ContentFileManager,  
                                                                  new GorgonCodecDds(), 
                                                                  new GorgonV3SpriteBinaryCodec(HostToolServices.GraphicsContext.Renderer2D), HostToolServices.Log);

            splitter.Initialize(new SplitParameters(entries,                                                          
                                                     new EditorContentSearchService(searchEntries), 
                                                     ContentFileManager, 
                                                     TemporaryFileSystem,
                                                     settings, 
                                                     textureSplitterService,
                                                     HostToolServices));

            form = new FormImageSelector();
            form.SetDataContext(splitter);
            form.SetupGraphics(HostToolServices.GraphicsContext);

            HostToolServices.BusyService.SetIdle();
            form.ShowDialog(GorgonApplication.MainForm);

            HostToolServices.ToolPlugInService.WriteContentSettings(typeof(ImageSplitToolPlugIn).FullName, settings);
        }
        catch (Exception ex)
        {                
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIST_ERR_LAUNCH);
        }
        finally
        {
            form?.Dispose();
            fileStream?.Dispose();
            HostToolServices.BusyService.SetIdle();
        }
    }

    /// <summary>Function to retrieve the ribbon button for the tool.</summary>
    /// <param name="fileManager">The project file manager.</param>
    /// <returns>A new tool ribbon button instance.</returns>
    /// <remarks>
    ///   <para>
    /// Tool plug in developers must override this method to return the button which is inserted on the application ribbon, under the "Tools" tab. If the method returns <b>null</b>, then the tool is
    /// ignored.
    /// </para>
    ///   <para>
    /// The resulting data structure will contain the means to handle the click event for the tool, and as such, is the only means of communication between the main UI and the plug in.
    /// </para>
    ///   <para>
    /// The <paramref name="fileManager" /> will allow plug ins to enumerate files in the project file system, create files/directories, and delete files/directories. This allows the plug in a means
    /// to persist any data generated.
    /// </para>
    /// </remarks>
    protected override IToolPlugInRibbonButton OnGetToolButton()
    {
        _button.ClickCallback ??= ShowForm;
        
        _button.CanExecute ??= CanShowForm;

        return _button;
    }

    /// <summary>Function to provide initialization for the plugin.</summary>
    /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        _button = new ToolPlugInRibbonButton(Resources.GORIST_TEXT_BUTTON, Resources.image_split_48x48, Resources.image_split_16x16, Resources.GORIST_GROUP_BUTTON)
        {
            Description = Resources.GORIST_DESC_BUTTON
        };
        _button.ValidateButton();
    }

    /// <summary>Function to provide clean up for the plugin.</summary>
    protected override void OnShutdown()
    {
        // Disconnect from the button to ensure that we don't get this thing keeping us around longer than we should.
        if (_button is not null)
        {
            _button.CanExecute = null;
            _button.ClickCallback = null;
            _button.Dispose();
        }

        base.OnShutdown();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ImageSplitToolPlugIn"/> class.</summary>
    public ImageSplitToolPlugIn()
        : base(Resources.GORIST_PLUGIN_DESC) 
    {        
    }
    #endregion
}
