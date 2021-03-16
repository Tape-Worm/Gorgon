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
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// A plug in used to extract sprites from a texture atlas by using an adjustable grid.
    /// </summary>
    internal class ExtractSpriteToolPlugIn
        : ToolPlugIn
    {
        #region Variables.
        // The cached button definition.
        private ToolPlugInRibbonButton _button;
        // The default image codec to use.
        private IGorgonImageCodec _defaultImageCodec;
        // Data used for extracting sprites.
        private readonly SpriteExtractionData _extractData = new();
        #endregion

        #region Methods.		
        /// <summary>
        /// Function to determine if the form can be shown.
        /// </summary>
        /// <returns><b>true</b> if the form can be shown, <b>false</b> if not.</returns>
        private bool CanShowForm()
        {
            if (string.IsNullOrWhiteSpace(ContentFileManager.CurrentDirectory))
            {
                return false;
            }

            IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();

            if ((selectedFiles.Count != 1)
                || (!ContentFileManager.FileExists(selectedFiles[0])))
            {
                return false;
            }

            IContentFile file = ContentFileManager.GetFile(selectedFiles[0]);

            if ((file?.Metadata is null)
                || (!file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Stream fileStream = null;
            try
            {
                fileStream = ContentFileManager.OpenStream(file.Path, FileMode.Open);
                if (!_defaultImageCodec.IsReadable(fileStream))
                {
                    return false;
                }

                IGorgonImageInfo metaData = _defaultImageCodec.GetMetaData(fileStream);

                if (metaData.ImageType is not ImageType.Image2D and not ImageType.ImageCube)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HostToolServices.Log.Print($"ERROR: Cannot open the selected file {file?.Name ?? string.Empty}.", LoggingLevel.Simple);
                HostToolServices.Log.LogException(ex);
            }
            finally
            {
                fileStream?.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Function to show the tool form.
        /// </summary>
        private void ShowForm()
        {
            ExtractSpriteToolSettings settings;
            FormExtract form = null;
            string textureFile;
            GorgonTexture2DView texture = null;
            Stream fileStream = null;

            HostToolServices.BusyService.SetBusy();

            try
            {
                settings = HostToolServices.ToolPlugInService.ReadContentSettings<ExtractSpriteToolSettings>(typeof(ExtractSpriteToolPlugIn).FullName);

                if (settings is null)
                {
                    settings = new ExtractSpriteToolSettings();
                }

                textureFile = ContentFileManager.GetSelectedFiles()[0];

                if (string.IsNullOrWhiteSpace(settings.LastOutputDir))
                {
                    settings.LastOutputDir = Path.GetDirectoryName(textureFile).FormatDirectory('/');
                }
                else
                {
                    if (!ContentFileManager.DirectoryExists(settings.LastOutputDir.FormatDirectory('/')))
                    {
                        settings.LastOutputDir = Path.GetDirectoryName(textureFile).FormatDirectory('/');
                    }
                }

                fileStream = ContentFileManager.OpenStream(textureFile, FileMode.Open);

                texture = GorgonTexture2DView.FromStream(HostToolServices.GraphicsContext.Graphics, fileStream, _defaultImageCodec, options: new GorgonTexture2DLoadOptions
                {
                    Name = "Extractor Texture Atlas",
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Default,
                    IsTextureCube = false
                });

                fileStream.Dispose();

                _extractData.Texture = texture;
                _extractData.SkipEmpty = settings.AllowEmptySpriteSkip;
                _extractData.SkipColor = new GorgonColor(settings.SkipColor);
                _extractData.CellSize = settings.GridCellSize;

                var extractViewModel = new Extract();
                extractViewModel.Initialize(new ExtractParameters(settings, 
                                                                  _extractData, 
                                                                  new SpriteExtractorService(HostToolServices.GraphicsContext.Renderer2D,
                                                                                             ContentFileManager,
                                                                                             new GorgonV3SpriteBinaryCodec(HostToolServices.GraphicsContext.Renderer2D)),
                                                                  ContentFileManager.GetFile(textureFile),
                                                                  ContentFileManager, 
                                                                  HostToolServices));

                form = new FormExtract(settings);
                form.SetDataContext(extractViewModel);
                form.SetupGraphics(HostToolServices.GraphicsContext);

                HostToolServices.BusyService.SetIdle();

                form.ShowDialog(GorgonApplication.MainForm);

                HostToolServices.BusyService.SetBusy();
                HostToolServices.ToolPlugInService.WriteContentSettings(typeof(ExtractSpriteToolPlugIn).FullName, settings);
            }
            catch (Exception ex)
            {                
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_LAUNCH);
            }
            finally
            {
                _extractData.Texture = null;

                form?.Dispose();
                fileStream?.Dispose();
                texture?.Dispose();
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
            if (_button.ClickCallback is null)
            {
                _button.ClickCallback = ShowForm;
            }
            
            if (_button.CanExecute is null)
            {
                _button.CanExecute = CanShowForm;
            }

            return _button;
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize()
        {
            _defaultImageCodec = new GorgonCodecDds();
            
            _button = new ToolPlugInRibbonButton(Resources.GOREST_TEXT_BUTTON, Resources.extract_grid_48x48, Resources.extract_grid_16x16, Resources.GOREST_GROUP_BUTTON)
            {
                Description = Resources.GOREST_DESC_BUTTON
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
        /// <summary>Initializes a new instance of the <see cref="ExtractSpriteToolPlugIn"/> class.</summary>
        public ExtractSpriteToolPlugIn()
            : base(Resources.GOREST_PLUGIN_DESC) 
        {        
        }
        #endregion
    }
}
