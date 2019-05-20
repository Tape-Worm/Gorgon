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
using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.UI;
using System.Collections.Generic;
using Gorgon.Editor.UI.Controls;
using System.Linq;
using Gorgon.Editor.Services;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// A plug in used to extract sprites from a texture atlas by using an adjustable grid.
    /// </summary>
    internal class TextureAtlasToolPlugIn
        : ToolPlugIn
    {
        #region Variables.
		// The cached button definition.
        private ToolPlugInRibbonButton _button;
		// The project file manager.
        private IContentFileManager _fileManager;
		// The default image codec to use.
        private IGorgonImageCodec _defaultImageCodec;
        // The default sprite codec to use.
        private IGorgonSpriteCodec _defaultSpriteCodec;
		// The view model for the sprite file browser.
        private SpriteFiles _fileVm;
        // The texture atlas view model.
        private TextureAtlas _textureAtlas;
        #endregion

        #region Methods.		
        /// <summary>
        /// Function to retrieve the sprite file entries from the file system.
        /// </summary>
        /// <param name="searchEntries">The flattened list of entries used for searching.</param>
        /// <param name="fileSystemEntries">The file system entry hierarchy.</param>
        private (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> fileSystemEntries) GetFileEntries()
        {
            var searchEntries = new List<IContentFileExplorerSearchEntry>();
            var fileSystemEntries = new List<ContentFileExplorerDirectoryEntry>();
            ContentFileExplorerDirectoryEntry dirEntry = null;
            var fileEntries = new List<ContentFileExplorerFileEntry>();
            IEnumerable<string> dirs = _fileManager.EnumerateDirectories("/", "*", true);
            IEnumerable<IContentFile> spriteFiles = _fileManager.EnumerateContentFiles("/", "*")
                                                .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));

            if (spriteFiles.Any())
            {
                dirEntry = new ContentFileExplorerDirectoryEntry("/", fileEntries);
                fileSystemEntries.Add(dirEntry);
                searchEntries.Add(dirEntry);

                foreach (IContentFile file in spriteFiles)
                {
                    var fileEntry = new ContentFileExplorerFileEntry(file, dirEntry);
                    fileEntries.Add(fileEntry);
                    searchEntries.Add(fileEntry);
                }
            }

            foreach (string subDir in dirs)
            {
                spriteFiles = _fileManager.EnumerateContentFiles(subDir, "*")
                                                .Where(item => (item.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string fileType))
                                                && (string.Equals(fileType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));
                if (!spriteFiles.Any())
                {
                    continue;
                }

                fileEntries = new List<ContentFileExplorerFileEntry>();
                dirEntry = new ContentFileExplorerDirectoryEntry(subDir, fileEntries);

                fileSystemEntries.Add(dirEntry);
                searchEntries.Add(dirEntry);

                foreach (IContentFile file in spriteFiles)
                {
                    var fileEntry = new ContentFileExplorerFileEntry(file, dirEntry);
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

            CommonServices.BusyService.SetBusy();

            try
            {
                settings = ToolPlugInService.ReadContentSettings<TextureAtlasSettings>(typeof(TextureAtlasToolPlugIn).FullName);

                if (settings == null)
                {
                    settings = new TextureAtlasSettings();
                }

                (List<IContentFileExplorerSearchEntry> searchEntries, List<ContentFileExplorerDirectoryEntry> entries) = GetFileEntries();

                if (_fileVm == null)
                {
                    _fileVm = new SpriteFiles();
                }

                if (_textureAtlas == null)
                {
                    _textureAtlas = new TextureAtlas();
                }

                var fileIO = new FileIOService(_fileManager, _defaultImageCodec, _defaultSpriteCodec);

                _fileVm.Initialize(new SpriteFilesParameters(entries, new EditorContentSearchService(searchEntries), CommonServices));
                _textureAtlas.Initialize(new TextureAtlasParameters(settings, 
                                        _fileVm, 
                                        new GorgonTextureAtlasService(GraphicsContext.Renderer2D), 
                                        fileIO, 
                                        FolderBrowser, 
                                        CommonServices));	

                form = new FormAtlasGen();
                form.SetupGraphics(GraphicsContext);
                form.SetDataContext(_textureAtlas);
                
                CommonServices.BusyService.SetIdle();
                form.ShowDialog(GorgonApplication.MainForm);

                ToolPlugInService.WriteContentSettings(typeof(TextureAtlasToolPlugIn).FullName, settings);
            }
            catch (Exception ex)
            {
                CommonServices.MessageDisplay.ShowError(ex, Resources.GORTAG_ERR_LAUNCH);
            }
            finally
            {
                CommonServices.BusyService.SetIdle();
                form?.Dispose();
            }
        }

        /// <summary>Function to retrieve the ribbon button for the tool.</summary>
        /// <param name="fileManager">The project file manager.</param>
        /// <param name="scratchArea">The scratch area for writing temporary data.</param>
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
        ///   <para>
        /// The <paramref name="scratchArea" /> is used to write temporary data to the project temporary area, which is useful for handling transitory states. Because this is <b>temporary</b>, any data
        /// written to this area will be deleted on application shut down. So do not rely on this data being there on the next start up.
        /// </para>
        /// </remarks>
        protected override IToolPlugInRibbonButton OnGetToolButton(IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea)
        {
            _fileManager = fileManager;

            if (_button.ClickCallback == null)
            {
                _button.ClickCallback = ShowForm;
            }

            return _button;
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize()
        {            
            _defaultImageCodec = new GorgonCodecDds();
            _defaultSpriteCodec = new GorgonV3SpriteBinaryCodec(GraphicsContext.Renderer2D);

            _button = new ToolPlugInRibbonButton(Resources.GORTAG_TEXT_BUTTON, Resources.texture_atlas_48x48, Resources.texture_atlas_16x16, Resources.GORTAG_GROUP_BUTTON)
            {
                Description = Resources.GORTAG_DESC_BUTTON
            };
            _button.ValidateButton();
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
        {
            _fileManager = null;

            // Disconnect from the button to ensure that we don't get this thing keeping us around longer than we should.
            if (_button != null)
            {
                _button.CanExecute = null;
                _button.ClickCallback = null;
                _button.Dispose();
            }

            base.OnShutdown();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="DummySpriteCodecPlugIn"/> class.</summary>
        public TextureAtlasToolPlugIn()
            : base(Resources.GORTAG_PLUGIN_DESC)
        {
        }
        #endregion
    }
}
