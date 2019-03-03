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
// Created: March 2, 2019 1:30:05 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Plugins;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Gorgon sprite editor content plug-in interface.
    /// </summary>
    internal class SpriteEditorPlugin
        : ContentPlugin, IContentPluginMetadata
    {
        #region Variables.
        // The loaded image codecs.
        private List<IGorgonSpriteCodec> _codecList = new List<IGorgonSpriteCodec>();

        // The image editor settings.
        //private ImageEditorSettings _settings = new ImageEditorSettings();

        // The plug in cache for image codecs.
        private GorgonMefPluginCache _pluginCache;

        // The content plug in service that loaded this plug in.
        private IContentPluginService _pluginService;

        // This is the only codec supported by the image plug in.  Images will be converted when imported.
        private GorgonV3SpriteBinaryCodec _defaultCodec;

        // The synchronization lock for threads.
        private readonly object _syncLock = new object();
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
        /// Function to load external image codec plug ins.
        /// </summary>
        private void LoadCodecPlugins()
        {
            /*if (_settings.CodecPluginPaths.Count == 0)
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
            }*/
        }

        /// <summary>
        /// Function to retrieve the path to the texture converted used to convert compressed images.
        /// </summary>
        /// <returns>The file info for the texture converter file.</returns>
        private FileInfo GetTexConvExe()
        {
            FileInfo result;

            // The availability of texconv.exe determines whether or not we can use block compressed formats or not.
            Log.Print("Checking for texconv.exe...", LoggingLevel.Simple);
            var pluginDir = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
            result = new FileInfo(Path.Combine(pluginDir.FullName, "texconv.exe"));

            if (!result.Exists)
            {
                Log.Print($"WARNING: Texconv.exe was not found at {pluginDir.FullName}. Block compressed formats will be unavailable.", LoggingLevel.Simple);
            }
            else
            {
                Log.Print($"Found texconv.exe at '{result.FullName}'.", LoggingLevel.Simple);
            }

            return result;
        }

        /// <summary>
        /// Function to load the image to be used a thumbnail.
        /// </summary>
        /// <param name="thumbnailCodec">The codec for the thumbnail images.</param>
        /// <param name="thumbnailFile">The path to the thumbnail file.</param>
        /// <param name="content">The content being thumbnailed.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>The thumbnail image, and a flag to indicate whether the thumbnail needs conversion.</returns>
        private (IGorgonImage thumbnailImage, bool needsConversion) LoadThumbNailImage(IGorgonImageCodec thumbnailCodec, FileInfo thumbnailFile, IContentFile content, CancellationToken cancelToken)
        {
            IGorgonImage result;
            Stream inStream = null;

            try
            {
                // If we've already got the file, then leave.
                if (thumbnailFile.Exists)
                {
                    inStream = thumbnailFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    result = thumbnailCodec.LoadFromStream(inStream);

                    return cancelToken.IsCancellationRequested ? (null, false) : (result, false);
                }

                inStream = content.OpenRead();
                result = null;
                //result = _ddsCodec.LoadFromStream(inStream);

                return (result, true);
            }
            catch (Exception ex)
            {
                Log.Print($"[ERROR] Cannot create thumbnail for '{content.Path}'", LoggingLevel.Intermediate);
                Log.LogException(ex);
                return (null, false);
            }
            finally
            {
                inStream?.Dispose();
            }
        }

        /// <summary>
        /// Function to update the metadata for a file that is missing metadata attributes.
        /// </summary>
        /// <param name="attributes">The attributes to update.</param>        
        /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
        private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
        {
            bool needsRefresh = false;
            
            if ((attributes.TryGetValue(SpriteContent.CodecAttr, out string currentCodecType))
                && (!string.IsNullOrWhiteSpace(currentCodecType)))
            {
                attributes.Remove(SpriteContent.CodecAttr);
                needsRefresh = true;
            }

            if ((attributes.TryGetValue(SpriteContent.ContentTypeAttr, out string currentContentType))
                && (string.Equals(currentContentType, SpriteEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes.Remove(SpriteContent.ContentTypeAttr);
                needsRefresh = true;
            }

            string codecType = _defaultCodec.GetType().FullName;
            if ((!attributes.TryGetValue(SpriteContent.CodecAttr, out currentCodecType))
                || (!string.Equals(currentCodecType, codecType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[SpriteContent.CodecAttr] = codecType;
                needsRefresh = true;
            }
            
            if ((!attributes.TryGetValue(SpriteContent.ContentTypeAttr, out currentContentType))
                || (!string.Equals(currentContentType, SpriteEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[SpriteContent.ContentTypeAttr] = SpriteEditorCommonConstants.ContentType;
                needsRefresh = true;
            }

            return needsRefresh;
        }

        /// <summary>Function to register plug in specific search keywords with the system search.</summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="T:Gorgon.Core.IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService)
        {
            // Not needed yet.
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
        /// <param name="undoService">The undo service for the plug in.</param>
        /// <returns>A new IEditorContent object.</returns>
        /// <remarks>
        /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
        /// application or plug in is shut down, and is not stored with the project.
        /// </remarks>
        protected override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IViewModelInjection injector, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
        {
            return Task.FromResult<IEditorContent>(null);
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
                /*if (_settings != null)
                {
                    // Persist any settings.
                    _pluginService.WriteContentSettings(this, _settings);
                }*/
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                Log.LogException(ex);
            }

            _pluginCache?.Dispose();

            //ViewFactory.Unregister<IImageContent>();

            base.OnShutdown();
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPluginService pluginService)
        {
            //ViewFactory.Register<IImageContent>(() => new ImageEditorView());

            _pluginService = pluginService;
            //_pluginCache = new GorgonMefPluginCache(Log);

            // Get built-in codec list.
            _defaultCodec = new GorgonV3SpriteBinaryCodec(GraphicsContext.Renderer2D);
            _codecList.Add(_defaultCodec);
            _codecList.Add(new GorgonV3SpriteJsonCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV2SpriteCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV1SpriteBinaryCodec(GraphicsContext.Renderer2D));
            //_codecList.Add(new GorgonCodecJpeg());
            //_codecList.Add(new GorgonCodecDds());
            //_codecList.Add(new GorgonCodecTga());
            //_codecList.Add(new GorgonCodecBmp());
            //_codecList.Add(new GorgonCodecGif());

            //ImageEditorSettings settings = pluginService.ReadContentSettings<ImageEditorSettings>(this);

            //if (settings != null)
            //{
            //    _settings = settings;
            //}

            // Load the additional plug ins.
            LoadCodecPlugins();
        }

        /// <summary>
        /// Function to update the dependencies for the sprite.
        /// </summary>
        /// <param name="fileStream">The stream for the file.</param>
        /// <param name="dependencyList">The list of dependency file paths.</param>
        /// <param name="fileManager">The content file management system.</param>
        private void UpdateDependencies(Stream fileStream, List<string> dependencyList, IContentFileManager fileManager)
        {
            string textureName = _defaultCodec.GetAssociatedTextureName(fileStream);

            if (string.IsNullOrWhiteSpace(textureName))
            {
                return;
            }                       

            IContentFile textureFile = fileManager.GetFile(textureName);

            // If we lack the texture (e.g. it's been deleted or something), then reset the value from the metadata.
            if (textureFile == null)
            {
                dependencyList.Remove(textureName);
                return;
            }

            if (!dependencyList.Any(item => string.Equals(textureName, item, StringComparison.OrdinalIgnoreCase)))
            {
                dependencyList.Add(textureName);
            }
        }

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file" />, or the <paramref name="fileManager" /> parameter is <b>null</b>.</exception>
        public bool CanOpenContent(IContentFile file, IContentFileManager fileManager)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager));
            }

            using (Stream stream = file.OpenRead())
            {
                if (_defaultCodec.IsReadable(stream))
                {
                    UpdateFileMetadataAttributes(file.Metadata.Attributes);
                    UpdateDependencies(stream, file.Metadata.Dependencies, fileManager);
                    return true;
                }
                                
                return false;
            }
        }

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.sprite_16x16;

        /// <summary>
        /// Function to render the thumbnail into the image passed in.
        /// </summary>
        /// <param name="image">The image to render the thumbnail into.</param>
        /// <param name="scale">The scale of the image.</param>
        private void RenderThumbnail(ref IGorgonImage image, float scale)
        {
        }

        /// <summary>Function to retrieve a thumbnail for the content.</summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <param name="outputFile">The output file for the thumbnail data.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="T:Gorgon.Graphics.Imaging.IGorgonImage"/> containing the thumbnail image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile" />, <paramref name="fileManager" /> or the <paramref name="outputFile" /> parameter is <b>null</b>.</exception>
        public Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, IContentFileManager fileManager, FileInfo outputFile, CancellationToken cancelToken)
        {
            if (contentFile == null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager));
            }

            if (outputFile == null)
            {
                throw new ArgumentNullException(nameof(outputFile));
            }

            return Task.FromResult<IGorgonImage>(null);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the SpriteEditorPlugin class.</summary>
        public SpriteEditorPlugin()
            : base(Resources.GORSPR_DESC) => SmallIconID = Guid.NewGuid();
        #endregion
    }
}
