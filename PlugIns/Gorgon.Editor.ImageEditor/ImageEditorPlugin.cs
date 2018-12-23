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
using System.Diagnostics;
using System.Threading;
using DX = SharpDX;
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
using Gorgon.Math;
using Gorgon.Graphics.Core;
using Gorgon.Graphics;

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

        // This is the only codec supported by the image plug in.  Images will be converted when imported.
        private GorgonCodecDds _ddsCodec = new GorgonCodecDds();
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
        /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
        private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
        {
            bool needsRefresh = false;
            string currentCodecType;
            string currentContentType;

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

            string codecType = _ddsCodec.GetType().FullName;
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

        /// <summary>Function to register plug in specific search keywords with the system search.</summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="T:Gorgon.Core.IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService)
        {
            searchService.MapKeywordToContentAttribute(Resources.GORIMG_SEARCH_KEYWORD_CODEC, ImageContent.CodecAttr);
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="scratchAreaDirectory">The directory for the temporary working directory.</param>
        /// <returns>A new IEditorContent object.</returns>
        protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IViewModelInjection injector, IGorgonFileSystemWriter<Stream> scratchArea)
        {
            IGorgonImage image = await Task.Run(() => 
            {
                using (Stream stream = file.OpenRead())
                {
                    return _ddsCodec.LoadFromStream(stream);
                }
            });
                        
            var content = new ImageContent();
            content.Initialize(new ImageContentParameters(file, image, _ddsCodec, _codecList, GraphicsContext.Graphics.FormatSupport, scratchArea, injector));

            return content;
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
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
                Log.LogException(ex);
            }            

            _pluginCache?.Dispose();

            ViewFactory.Unregister<IImageContent>();

            base.OnShutdown();
        }

        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        private void LoadCodecPlugins()
        {
            if (_settings.CodecPluginPaths.Count == 0)
            {
                return;
            }

            Log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            foreach (KeyValuePair<string, string> plugin in _settings.CodecPluginPaths)
            {
                Log.Print($"Loading '{plugin.Key}' from '{plugin.Value}'...", LoggingLevel.Verbose);

                var file = new FileInfo(plugin.Value);

                if (!file.Exists)
                {
                    Log.Print($"ERROR: Could not find the plug in assembly '{plugin.Value}' for plug in '{plugin.Key}'.", LoggingLevel.Simple);
                    continue;
                }

                _pluginCache.LoadPluginAssemblies(file.DirectoryName, file.Name);
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(_pluginCache, Log);

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
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPluginService pluginService)
        {
            ViewFactory.Register<IImageContent>(() => new ImageEditorView());

            _pluginService = pluginService;
            _pluginCache = new GorgonMefPluginCache(Log);

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
            LoadCodecPlugins();

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

            using (Stream stream = file.OpenRead())
            {
                if (_ddsCodec.IsReadable(stream))
                {
                    UpdateFileMetadataAttributes(file.Metadata.Attributes);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.image_16x16;

        /// <summary>
        /// Function to render the thumbnail into the image passed in.
        /// </summary>
        /// <param name="image">The image to render the thumbnail into.</param>
        /// <param name="scale">The scale of the image.</param>
        private void RenderThumbnail(ref IGorgonImage image, float scale)
        {
            using (GorgonTexture2D texture = image.ToTexture2D(GraphicsContext.Graphics, new GorgonTexture2DLoadOptions
            {
                Usage = ResourceUsage.Immutable
            }))            
            using (var rtv = GorgonRenderTarget2DView.CreateRenderTarget(GraphicsContext.Graphics, new GorgonTexture2DInfo
            {
                ArrayCount = 1,
                Binding = TextureBinding.ShaderResource,
                Format = BufferFormat.R8G8B8A8_UNorm,
                MipLevels = 1,
                Height = (int)(image.Height * scale),
                Width = (int)(image.Width * scale),
                Usage = ResourceUsage.Default
            }))
            {
                GorgonTexture2DView view = texture.GetShaderResourceView();
                rtv.Clear(GorgonColor.BlackTransparent);
                GraphicsContext.Graphics.SetRenderTarget(rtv);
                GraphicsContext.Graphics.DrawTexture(view, new DX.Rectangle(0, 0, rtv.Width, rtv.Height), blendState: GorgonBlendState.Default, samplerState: GorgonSamplerState.Default);
                GraphicsContext.Graphics.SetRenderTarget(null);

                image?.Dispose();
                image = rtv.Texture.ToImage();
            }            
        }

        /// <summary>Function to retrieve a thumbnail for the content.</summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="outputFile">The output file for the thumbnail data.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="T:Gorgon.Graphics.Imaging.IGorgonImage"/> containing the thumbnail image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/>, or the <paramref name="outputFile"/> parameter is <b>null</b>.</exception>
        public Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, FileInfo outputFile, CancellationToken cancelToken)
        {
            if (contentFile == null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            if (outputFile == null)
            {
                throw new ArgumentNullException(nameof(outputFile));
            }

            // If the content is not a DDS image, then leave it.
            if ((!contentFile.Metadata.Attributes.TryGetValue(ImageContent.CodecAttr, out string codecName))
                || (string.IsNullOrWhiteSpace(codecName))
                || (!string.Equals(codecName, _ddsCodec.GetType().FullName, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
                outputFile.Directory.Refresh();
            }

            return Task.Run(() =>
            {
                try
                {
                    IGorgonImage result;
                    IGorgonImageCodec pngCodec = new GorgonCodecPng();

                    // If we've already got the file, then leave.
                    if (outputFile.Exists)
                    {
                        using (Stream inStream = outputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            result = pngCodec.LoadFromStream(inStream);
                        }

                        return cancelToken.IsCancellationRequested ? null : result;
                    }

                    using (Stream inStream = contentFile.OpenRead())
                    {
                        result = _ddsCodec.LoadFromStream(inStream);
                    }

                    if (cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    const float maxSize = 256;
                    float scale = (result.Width / maxSize).Min(result.Height / maxSize);
                    RenderThumbnail(ref result, scale);

                    if (cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    pngCodec.SaveToFile(result, outputFile.FullName);

                    if (cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    contentFile.Metadata.Attributes[CommonEditorConstants.ThumbnailAttr] = outputFile.Name;

                    return result;
                }
                catch(Exception ex)
                {
                    Log.Print($"[ERROR] Cannot create thumbnail for '{contentFile.Path}'", LoggingLevel.Intermediate);
                    Log.LogException(ex);
                    return null;
                }
            });
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorPlugin class.</summary>
        public ImageEditorPlugin()
            : base(Resources.GORIMG_DESC) => SmallIconID = Guid.NewGuid();
        #endregion
    }
}
