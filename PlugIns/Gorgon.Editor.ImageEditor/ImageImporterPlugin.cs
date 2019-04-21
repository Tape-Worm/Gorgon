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
// Created: December 17, 2018 10:00:39 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.Services;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Plugins;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A plugin used to build an importer for image data.
    /// </summary>
    internal class ImageImporterPlugin
        : ContentImportPlugin
    {
        #region Variables.
        // The image editor settings.
        private ISettings _settings;

        // The plug in cache for image codecs.
        private GorgonMefPluginCache _pluginCache;

		// The codec registry.
        private CodecRegistry _codecs;
        #endregion

        #region Properties.
		/// <summary>
        /// Property to return the common codec registry factory for all image plug ins in this assembly.
        /// </summary>
		public static Lazy<CodecRegistry> CodecRegistryFactory
        {
            get;
            private set;
        }

		/// <summary>
        /// Property to return the factory used to create the shared settings.
        /// </summary>
		public static Lazy<ISettings> SettingsFactory
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the codec used by the image.
        /// </summary>
        /// <param name="file">The file containing the image content.</param>
        /// <returns>The codec used to read the file.</returns>
        private IGorgonImageCodec GetCodec(FileInfo file)
        {
            IGorgonImageCodec result = null;
            Stream stream = null;

            try
            {
                // Locate the file extension.
                if (!string.IsNullOrWhiteSpace(file.Extension))
                {
                    var extension = new GorgonFileExtension(file.Extension);

                    result = _codecs.CodecFileTypes.FirstOrDefault(item => item.extension == extension).codec;

                    if (result != null)
                    {
                        return result;
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
        /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register``1(System.Func{System.Windows.Forms.Control})"/> method in the
        /// <see cref="OnInitialize(IContentPluginService)"/> method or the settings will not display.
        /// </para>
        /// </remarks>
        protected override ISettingsCategoryViewModel OnGetSettings() => SettingsFactory.Value;

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPluginService pluginService)
        {
            ViewFactory.Register<ISettings>(() => new ImageCodecSettingsPanel());
			            
            _pluginCache = new GorgonMefPluginCache(CommonServices.Log);

            if (SettingsFactory == null)
            {
                SettingsFactory = new Lazy<ISettings>(() =>
                {
                    ImageEditorSettings settings = pluginService.ReadContentSettings<ImageEditorSettings>(ImageEditorPlugin.SettingsName);

                    if (settings == null)
                    {
                        settings = new ImageEditorSettings();
                    }

                    var settingsVm = new Settings();
                    settingsVm.Initialize(new SettingsParameters(settings, pluginService, CommonServices));
					
                    return settingsVm;
                }, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            _settings = SettingsFactory.Value;

            if (CodecRegistryFactory == null)
            {
                CodecRegistryFactory = new Lazy<CodecRegistry>(() => new CodecRegistry(_settings, _pluginCache, CommonServices.Log), LazyThreadSafetyMode.ExecutionAndPublication);
            }

            _codecs = CodecRegistryFactory.Value;
            _codecs.LoadFromSettings();
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

                ViewFactory.Unregister<ISettings>();

                foreach (IDisposable codec in _codecs.Codecs.OfType<IDisposable>())
                {
                    codec.Dispose();
                }

                _pluginCache.Dispose();
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
        /// <param name="log">The logging interface to use.</param>
        /// <returns>A new <see cref="IEditorContentImporter"/> object.</returns>
        protected override IEditorContentImporter OnCreateImporter(FileInfo sourceFile, IGorgonFileSystem fileSystem) => new DdsImageImporter(sourceFile, GetCodec(sourceFile), CommonServices.Log);

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        protected override bool OnCanOpenContent(FileInfo file) => GetCodec(file) != null;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ImageImporterPlugin"/> class.</summary>
        public ImageImporterPlugin()
            : base(Resources.GORIMG_IMPORT_DESC)
        {

        }	
        #endregion
    }
}
