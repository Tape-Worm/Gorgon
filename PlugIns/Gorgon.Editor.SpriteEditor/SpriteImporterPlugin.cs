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
using System.Linq;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.SpriteEditor.Services;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A plugin used to build an importer for sprite data.
    /// </summary>
    internal class SpriteImporterPlugIn
        : ContentImportPlugIn
    {
        #region Variables.
        // The image editor settings.
        private IImporterPlugInSettings _settings;

		// The codecs registered with the plug in.
        private ICodecRegistry _codecs;

        // The plug in cache for image codecs.
        private GorgonMefPlugInCache _pluginCache;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the codec used by the image.
        /// </summary>
        /// <param name="file">The file containing the image content.</param>
        /// <returns>The codec used to read the file.</returns>
        private IGorgonSpriteCodec GetCodec(FileInfo file)
        {
            Stream stream = null;

            try
            {
                // Locate the file extension.
                if (!string.IsNullOrWhiteSpace(file.Extension))
                {
                    var extension = new GorgonFileExtension(file.Extension);

					// Since all Gorgon's sprite files use the same extension, we'll have to be a little more aggressive when determining type.
                    (GorgonFileExtension, IGorgonSpriteCodec codec)[] results = _codecs.CodecFileTypes.Where(item => item.extension == extension).ToArray();

                    if (results.Length == 0)
                    {
                        return null;
                    }

                    stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

                    foreach (IGorgonSpriteCodec codec in results.Select(item => item.codec))
                    {
                        if (codec.IsReadable(stream))
                        {
                            return codec;
                        }
                    }
                }
            }
            finally
            {
                stream?.Dispose();
            }

            return null;
        }

        /// <summary>Function to retrieve the settings interface for this plug in.</summary>
        /// <param name="injector">Objects to inject into the view model.</param>
        /// <returns>The settings interface view model.</returns>
        /// <remarks>
        ///   <para>
        /// Implementors who wish to supply customizable settings for their plug ins from the main "Settings" area in the application can override this method and return a new view model based on
        /// the base <see cref="ISettingsCategoryViewModel"/> type.
        /// </para>
        ///   <para>
        /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{System.Windows.Forms.Control})"/> method in the
        /// <see cref="OnInitialize()"/> method or the settings will not display.
        /// </para>
        /// </remarks>
        protected override ISettingsCategoryViewModel OnGetSettings() => _settings;

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize()
        {
            ViewFactory.Register<IImporterPlugInSettings>(() => new SpriteCodecSettingsPanel());
            
            _pluginCache = new GorgonMefPlugInCache(CommonServices.Log);

            SpriteImportSettings settings = ContentPlugInService.ReadContentSettings<SpriteImportSettings>(typeof(SpriteImporterPlugIn).FullName);

            if (settings == null)
            {
                settings = new SpriteImportSettings();
            }

            _codecs = new CodecRegistry(_pluginCache, GraphicsContext.Renderer2D, CommonServices.Log);
            _codecs.LoadFromSettings(settings);

            var settingsVm = new ImporterPlugInSettings();
            settingsVm.Initialize(new ImportPlugInSettingsParameters(settings, _codecs, new FileOpenDialogService(), ContentPlugInService, CommonServices));
            _settings = settingsVm;
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
        {
            try
            {
                if ((_settings?.WriteSettingsCommand != null) && (_settings.WriteSettingsCommand.CanExecute(null)))
                {
                    // Persist any settings.
                    _settings.WriteSettingsCommand.Execute(null);
                }

                ViewFactory.Unregister<IImporterPlugInSettings>();
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                CommonServices.Log.LogException(ex);
            }
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="sourceFile">The file being imported.</param>
        /// <param name="fileSystem">The file system containing the file being imported.</param>
        /// <returns>A new <see cref="T:Gorgon.Editor.Services.IEditorContentImporter"/> object.</returns>
        protected override IEditorContentImporter OnCreateImporter(FileInfo sourceFile, IGorgonFileSystem fileSystem) => 
            new GorgonSpriteImporter(sourceFile, GetCodec(sourceFile), GraphicsContext.Renderer2D, fileSystem, CommonServices.Log);

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        protected override bool OnCanOpenContent(FileInfo file) => GetCodec(file) != null;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteImporterPlugIn"/> class.</summary>
        public SpriteImporterPlugIn()
            : base(Resources.GORSPR_IMPORT_DESC)
        {
        }
        #endregion
    }
}
