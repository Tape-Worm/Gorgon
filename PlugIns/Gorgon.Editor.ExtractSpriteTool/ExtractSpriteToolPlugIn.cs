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
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.PlugIns;
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
		// The project file manager.
        private IContentFileManager _fileManager;
		// The default image codec to use.
        private IGorgonImageCodec _defaultImageCodec;
		// Data used for extracting sprites.
        private readonly Lazy<SpriteExtractionData> _extractData;
        #endregion

        #region Properties.

        #endregion

        #region Methods.		
        /// <summary>
        /// Function to determine if the form can be shown.
        /// </summary>
        /// <returns><b>true</b> if the form can be shown, <b>false</b> if not.</returns>
        private bool CanShowForm()
        {
            if ((string.IsNullOrWhiteSpace(_fileManager?.CurrentDirectory))
				|| (_fileManager.SelectedFile == null))
            {
                return false;
            }

            if ((_fileManager.SelectedFile.Metadata == null)
                || (!_fileManager.SelectedFile.Metadata.Attributes.TryGetValue("Type", out string contentType))
                || (!string.Equals(contentType, "Image", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Stream fileStream = null;
            try
            {
                fileStream = _fileManager.SelectedFile.OpenRead();
                if (!_defaultImageCodec.IsReadable(fileStream))
                {
                    return false;
                }

                IGorgonImageInfo metaData = _defaultImageCodec.GetMetaData(fileStream);

                if ((metaData.ImageType != ImageType.Image2D) && (metaData.ImageType != ImageType.ImageCube))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {                
                CommonServices.Log.Print($"[ERROR] Cannot open the selected file {_fileManager.SelectedFile.Name}.", LoggingLevel.Simple);
                CommonServices.Log.LogException(ex);
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
            FormExtract _form = null;
            GorgonTexture2DView texture = null;
            Stream fileStream = null;
            ExtractSpriteToolSettings settings;
            IGorgonImage image = null;
            IContentFile textureFile;

            CommonServices.BusyService.SetBusy();

            try
            {
                settings = ToolPlugInService.ReadContentSettings<ExtractSpriteToolSettings>(typeof(ExtractSpriteToolPlugIn).FullName);

                if (settings == null)
                {
                    settings = new ExtractSpriteToolSettings();
                }

                textureFile = _fileManager.SelectedFile;
                fileStream = textureFile.OpenRead();

                texture = GorgonTexture2DView.FromStream(GraphicsContext.Graphics, fileStream, _defaultImageCodec, options: new GorgonTexture2DLoadOptions
                {
                    Name = "Extractor Texture Atlas",
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Default,
					IsTextureCube = false
                });

                image = texture.Texture.ToImage();

                fileStream.Dispose();

                if (string.IsNullOrWhiteSpace(settings.LastOutputDir))
                {
                    settings.LastOutputDir = Path.GetDirectoryName(textureFile.Path).FormatDirectory('/');
                }
                else
                {
                    if (!_fileManager.DirectoryExists(settings.LastOutputDir.FormatDirectory('/')))
                    {
						settings.LastOutputDir = Path.GetDirectoryName(textureFile.Path).FormatDirectory('/');
                    }
                }

                _extractData.Value.Texture = texture;
                _extractData.Value.SkipEmpty = settings.AllowEmptySpriteSkip;
                _extractData.Value.SkipColor = new GorgonColor(settings.SkipColor);

                var extractViewModel = new Extract();
                extractViewModel.Initialize(new ExtractParameters(settings, 
																_extractData.Value,
																textureFile,
																new ExtractorService(GraphicsContext.Renderer2D, _fileManager, 
																	new GorgonV3SpriteBinaryCodec(GraphicsContext.Renderer2D)), 
																new ColorPickerService(),
																FolderBrowser,
																CommonServices));

                _form = new FormExtract();
                _form.SetupGraphics(GraphicsContext);
                _form.SetDataContext(extractViewModel);
                _form.ShowDialog(GorgonApplication.MainForm);

                ToolPlugInService.WriteContentSettings(typeof(ExtractSpriteToolPlugIn).FullName, settings);
            }
            catch (Exception ex)
            {
                CommonServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_LAUNCH);
            }
            finally
            {
                _form?.Dispose();
                fileStream?.Dispose();
                image?.Dispose();
                texture?.Dispose();
                CommonServices.BusyService.SetIdle();                
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

            if (_button.CanExecute == null)
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
        public ExtractSpriteToolPlugIn()
            : base(Resources.GOREST_PLUGIN_DESC) => _extractData = new Lazy<SpriteExtractionData>(() => new SpriteExtractionData(), LazyThreadSafetyMode.ExecutionAndPublication);
        #endregion
    }
}
