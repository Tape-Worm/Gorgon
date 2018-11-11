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
// Created: October 29, 2018 2:52:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Graphics.Imaging;
using Gorgon.Plugins;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Gorgon image editor content plug-in interface.
    /// </summary>
    internal class ImageEditorPlugin
        : ContentPlugin, IContentPluginMetadata
    {
        #region Variables.
        // The loaded image codecs.
        private List<IGorgonImageCodec> _codecList = new List<IGorgonImageCodec>();

        // The list of available codecs matched by extension.
        private readonly List<(GorgonFileExtension extension, IGorgonImageCodec codec)> _codecs = new List<(GorgonFileExtension extension, IGorgonImageCodec codec)>();

        // The image editor settings.
        private ImageEditorSettings _settings = new ImageEditorSettings();

        // The plug in cache for image codecs.
        private GorgonMefPluginCache _pluginCache;

        // The content plug in service that loaded this plug in.
        private IContentPluginService _pluginService;
        #endregion

        #region Properties.
        /// <summary>Property to return the name of the plug in.</summary>
        string IContentPluginMetadata.PluginName => Name;

        /// <summary>Property to return the description of the plugin.</summary>
        string IContentPluginMetadata.Description => Description;

        /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
        public override bool CanCreateContent => false;

        /// <summary>Property to return the ID of the small icon for this plug in.</summary>
        public Guid SmallIconID 
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the metadata for a file that is missing metadata attributes.
        /// </summary>
        /// <param name="attributes">The attributes to update.</param>
        /// <param name="codec">The codec for the image.</param>
        /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
        private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes, IGorgonImageCodec codec)
        {
            bool needsRefresh = false;
            string codecType = codec?.GetType().FullName;
            string currentCodecType;
            string currentContentType;

            if (codecType == null)
            {
                if ((attributes.TryGetValue(ImageContent.CodecAttr, out currentCodecType))
                    && (!string.IsNullOrWhiteSpace(currentCodecType)))
                {
                    attributes.Remove(ImageContent.CodecAttr);
                    needsRefresh = true;
                }

                if ((attributes.TryGetValue(ImageContent.ContentTypeAttr, out currentContentType))
                    && (string.Equals(currentContentType, ImageEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
                {
                    attributes.Remove(ImageContent.ContentTypeAttr);
                    needsRefresh = true;
                }
                
                return needsRefresh;
            }

            if ((!attributes.TryGetValue(ImageContent.CodecAttr, out currentCodecType))
                || (!string.Equals(currentCodecType, codecType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[ImageContent.CodecAttr] = codecType;
                needsRefresh = true;
            }

            if ((!attributes.TryGetValue(ImageContent.ContentTypeAttr, out currentContentType))
                || (!string.Equals(currentContentType, ImageEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[ImageContent.ContentTypeAttr] = ImageEditorCommonConstants.ContentType;
                needsRefresh = true;
            }

            return needsRefresh;
        }

        /// <summary>
        /// Function to retrieve the codec used by the image.
        /// </summary>
        /// <param name="file">The file containing the image content.</param>
        /// <returns>The codec used to read the file.</returns>
        private IGorgonImageCodec GetCodec(IContentFile file)
        {
            IGorgonImageCodec result = null;
            Stream stream = null;

            try
            {
                // First, check the attribute information.
                if (file.Metadata.Attributes.TryGetValue(ImageContent.CodecAttr, out string imageCodecType))
                {
                    // Locate the actual type in amongst our loaded codecs.
                    result = _codecList.FirstOrDefault(item => string.Equals(item.GetType().FullName, imageCodecType, StringComparison.Ordinal));

                    if (result != null)
                    {
                        return result;
                    }
                }

                // Second, locate the extension.
                if (!string.IsNullOrWhiteSpace(file.Extension))
                {
                    var extension = new GorgonFileExtension(file.Extension);

                    result = _codecs.FirstOrDefault(item => item.extension == extension).codec;

                    if (result != null)
                    {
                        return result;
                    }
                }

                // If that failed, then test to see if the file can be opened by the codec, this is more intensive because we have to attempt opening the file for each item.
                foreach (IGorgonImageCodec codec in _codecList)
                {
                    stream = file.OpenRead();

                    if (codec.IsReadable(stream))
                    {
                        result = codec;
                        return codec;
                    }

                    stream.Dispose();
                }
            }
            finally
            {
                // If metadata has been changed, we need to update.
                if (UpdateFileMetadataAttributes(file.Metadata.Attributes, result))
                {
                    file.RefreshMetadata();
                }
                stream?.Dispose();
            }

            return null;
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="log">The logging interface to use.</param>
        /// <returns>A new IEditorContent object.</returns>
        protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IViewModelInjection injector, IGorgonLog log)
        {
            IGorgonImageCodec codec = GetCodec(file);

            if (codec == null)
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_NO_CODEC, file.Name));
            }

            IGorgonImage image = await Task.Run(() => 
            {
                using (Stream stream = file.OpenRead())
                {
                    return codec.LoadFromStream(stream);
                }
            });

            var content = new ImageContent();
            content.Initialize(new ImageContentParameters(file, image, codec, _codecList, injector));

            return content;
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        /// <param name="log">The logging interface for debug messages.</param>
        protected override void OnShutdown(IGorgonLog log)
        {
            foreach (IDisposable codec in _codecList.OfType<IDisposable>())
            {   
                codec?.Dispose();
            }

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

            _pluginCache?.Dispose();

            ViewFactory.Unregister<IImageContent>();

            base.OnShutdown(log);
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
        protected override void OnInitialize(IContentPluginService pluginService, IGorgonLog log)
        {
            ViewFactory.Register<IImageContent>(() => new ImageEditorView());

            _pluginService = pluginService;
            _pluginCache = new GorgonMefPluginCache(log);

            // Get built-in codec list.
            _codecList.Add(new GorgonCodecPng());
            _codecList.Add(new GorgonCodecJpeg());
            _codecList.Add(new GorgonCodecDds());
            _codecList.Add(new GorgonCodecTga());
            _codecList.Add(new GorgonCodecBmp());
            _codecList.Add(new GorgonCodecGif());

            ImageEditorSettings settings = pluginService.ReadContentSettings<ImageEditorSettings>(this);

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

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file" /> parameter is <b>null</b>.</exception>
        public bool CanOpenContent(IContentFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            
            return GetCodec(file) != null;
        }

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.image_16x16;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorPlugin class.</summary>
        public ImageEditorPlugin()
            : base(Resources.GORIMG_DESC)
        {
            SmallIconID = Guid.NewGuid();
        }
        #endregion
    }
}
