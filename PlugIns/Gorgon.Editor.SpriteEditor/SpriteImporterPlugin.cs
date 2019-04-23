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
using Gorgon.Diagnostics;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.SpriteEditor.Services;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A plugin used to build an importer for sprite data.
    /// </summary>
    internal class SpriteImporterPlugIn
        : ContentImportPlugIn
    {
        #region Variables.
        // The loaded image codecs.
        private readonly List<IGorgonSpriteCodec> _codecList = new List<IGorgonSpriteCodec>();

        // The list of available codecs matched by extension.
        private readonly List<(GorgonFileExtension extension, IGorgonSpriteCodec codec)> _codecs = new List<(GorgonFileExtension extension, IGorgonSpriteCodec codec)>();

        // The image editor settings.
        private SpriteEditorSettings _settings = new SpriteEditorSettings();

        // The plug in cache for image codecs.
        private GorgonMefPlugInCache _pluginCache;

        // The content plug in service that loaded this plug in.
        private IContentPlugInService _pluginService;
        #endregion

        #region Properties.
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

                    (GorgonFileExtension, IGorgonSpriteCodec codec)[] results = _codecs.Where(item => item.extension == extension).ToArray();

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

        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        private void LoadCodecPlugIns()
        {
            if (_settings.CodecPlugInPaths.Count == 0)
            {
                return;
            }

            CommonServices.Log.Print("Loading sprite codecs...", LoggingLevel.Intermediate);
            _pluginCache.ValidateAndLoadAssemblies(_settings.CodecPlugInPaths.Select(item => new FileInfo(item.Value)), CommonServices.Log);
            IGorgonPlugInService plugins = new GorgonMefPlugInService(_pluginCache, CommonServices.Log);

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonSpriteCodecPlugIn plugin in plugins.GetPlugIns<GorgonSpriteCodecPlugIn>())
            {
                foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
                {
                    _codecList.Add(plugin.CreateCodec(desc.Name));
                }
            }
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPlugInService pluginService)
        {
            _pluginService = pluginService;
            _pluginCache = new GorgonMefPlugInCache(CommonServices.Log);

            // Get built-in codec list.
            _codecList.Add(new GorgonV3SpriteBinaryCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV3SpriteJsonCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV2SpriteCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV1SpriteBinaryCodec(GraphicsContext.Renderer2D));

            SpriteEditorSettings settings = pluginService.ReadContentSettings<SpriteEditorSettings>(SpriteEditorPlugIn.SettingsName);

            if (settings != null)
            {
                _settings = settings;
            }

            // Load the additional plug ins.
            LoadCodecPlugIns();

            foreach (IGorgonSpriteCodec codec in _codecList)
            {
                foreach (GorgonFileExtension extension in codec.FileExtensions)
                {
                    _codecs.Add((extension, codec));
                }
            }
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
        {
            try
            {
                if (_settings != null)
                {
                    // Persist any settings.
                    _pluginService.WriteContentSettings(SpriteEditorPlugIn.SettingsName, _settings);
                }
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

        #region Methods.

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
