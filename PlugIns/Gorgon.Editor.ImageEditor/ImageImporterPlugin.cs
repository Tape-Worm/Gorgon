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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.Services;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
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
        // The loaded image codecs.
        private readonly List<IGorgonImageCodec> _codecList = new List<IGorgonImageCodec>();

        // The list of available codecs matched by extension.
        private readonly List<(GorgonFileExtension extension, IGorgonImageCodec codec)> _codecs = new List<(GorgonFileExtension extension, IGorgonImageCodec codec)>();

        // The image editor settings.
        private ImageImporterSettings _settings = new ImageImporterSettings();

        // The plug in cache for image codecs.
        private GorgonMefPluginCache _pluginCache;

        // The content plug in service that loaded this plug in.
        private IContentImporterPluginService _pluginService;
        #endregion

        #region Properties.
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

                    result = _codecs.FirstOrDefault(item => item.extension == extension).codec;

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

        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        /// <param name="log">The logging interface used to log debug messages.</param>
        private void LoadCodecPlugins(IGorgonLog log)
        {
            if (_settings.CodecPluginPaths.Count == 0)
            {
                return;
            }

            log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            foreach (KeyValuePair<string, string> plugin in _settings.CodecPluginPaths)
            {
                log.Print($"Loading '{plugin.Key}' from '{plugin.Value}'...", LoggingLevel.Verbose);

                var file = new FileInfo(plugin.Value);

                if (!file.Exists)
                {
                    log.Print($"ERROR: Could not find the plug in assembly '{plugin.Value}' for plug in '{plugin.Key}'.", LoggingLevel.Simple);
                    continue;
                }

                _pluginCache.LoadPluginAssemblies(file.DirectoryName, file.Name);
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(_pluginCache, log);

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugin plugin in plugins.GetPlugins<GorgonImageCodecPlugin>())
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    _codecList.Add(plugin.CreateCodec(desc.Name));
                }
            }
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <param name="log">The logging interface for debug messages.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentImporterPluginService pluginService, IGorgonLog log)
        {
            _pluginService = pluginService;
            _pluginCache = new GorgonMefPluginCache(log);

            // Get built-in codec list.
            _codecList.Add(new GorgonCodecPng());
            _codecList.Add(new GorgonCodecJpeg());
            _codecList.Add(new GorgonCodecTga());
            _codecList.Add(new GorgonCodecBmp());
            _codecList.Add(new GorgonCodecGif());

            ImageImporterSettings settings = pluginService.ReadContentSettings<ImageImporterSettings>(this);

            if (settings != null)
            {
                _settings = settings;
            }

            // Load the additional plug ins.
            LoadCodecPlugins(log);

            foreach (IGorgonImageCodec codec in _codecList)
            {
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs.Add((new GorgonFileExtension(extension), codec));
                }
            }
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        /// <param name="log">The logging interface for debug messages.</param>
        protected override void OnShutdown(IGorgonLog log)
        {
            try
            {
                if (_settings != null)
                {
                    // Persist any settings.
                    _pluginService.WriteContentSettings(this, _settings);
                }
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                log.LogException(ex);
            }
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="sourceFile">The file being imported.</param>
        /// <param name="fileSystem">The file system containing the file being imported.</param>
        /// <param name="log">The logging interface to use.</param>
        /// <returns>A new <see cref="T:Gorgon.Editor.Services.IEditorContentImporter"/> object.</returns>
        protected override IEditorContentImporter OnCreateImporter(FileInfo sourceFile, IGorgonFileSystem fileSystem, IGorgonLog log) => new DdsImageImporter(sourceFile, GetCodec(sourceFile), log);

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        protected override bool OnCanOpenContent(FileInfo file) => GetCodec(file) != null;
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImageImporterPlugin"/> class.</summary>
        public ImageImporterPlugin()
            : base(Resources.GORIMG_IMPORT_DESC)
        {

        }
        #endregion
    }
}
