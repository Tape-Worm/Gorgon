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
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.Services;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A plugin used to build an importer for image data.
    /// </summary>
    internal class ImageImporterPlugIn
        : ContentImportPlugIn
    {
        #region Variables.
        // The image editor settings.
        private ISettings _settings;

        // The codec registry.
        private ICodecRegistry _codecs;
        #endregion

        #region Methods.
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
            ViewFactory.Register<ISettings>(() => new ImageCodecSettingsPanel());

            // Retrieve the shared settings.
            (_codecs, _settings) = SharedDataFactory.GetSharedData(HostContentServices);
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
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                HostContentServices.Log.LogException(ex);
            }
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <returns>A new <see cref="IEditorContentImporter"/> object.</returns>
        /// <remarks>This method creates an instance of the custom content importer. The application will use the object returned to perform the actual import process.</remarks>
        protected override IEditorContentImporter OnCreateImporter() => new DdsImageImporter(TemporaryFileSystem, _codecs, HostContentServices.Log);

        /// <summary>
        /// Function to determine if the content plugin can open the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file to evaluate.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to determine if the file specified by the <paramref name="filePath"/> passed to the method can be opened by this plug in. If the method returns <b>true</b>, then the host 
        /// application will convert the file using the importer produced by this plug in. Otherwise, if the method returns <b>false</b>, then the file is skipped.
        /// </para>
        /// <para>
        /// The <paramref name="filePath"/> is a path to the file on the project virtual file system.
        /// </para>
        /// <para>
        /// Implementors may use whatever method they desire to determine if the file can be opened (e.g. checking file extensions, examining file headers, etc...).
        /// </para>
        /// </remarks>
        protected override bool OnCanOpenContent(string filePath) => GetCodec(filePath, _codecs) != null;

        /// <summary>
        /// Function to retrieve the codec used by the image.
        /// </summary>
        /// <param name="filePath">The path to the file to evaluate.</param>
        /// <param name="codecs">The codecs to evaluate.</param>
        /// <returns>The codec used to read the file.</returns>
        public static IGorgonImageCodec GetCodec(string filePath, ICodecRegistry codecs)
        {
            string fileExtension = Path.GetExtension(filePath);

            // Locate the file extension.
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                return null;
            }
            var extension = new GorgonFileExtension(fileExtension);

            return codecs.CodecFileTypes.FirstOrDefault(item => item.extension == extension).codec;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ImageImporterPlugIn"/> class.</summary>
        public ImageImporterPlugIn()
            : base(Resources.GORIMG_IMPORT_DESC)
        {

        }
        #endregion
    }
}
