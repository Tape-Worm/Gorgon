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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Plugins;
using Gorgon.Renderers;
using Gorgon.Math;

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

        // The image codec to use.
        private IGorgonImageCodec _ddsCodec;

        // The synchronization lock for threads.
        private readonly object _syncLock = new object();

        // The image used to display a broken image link to a sprite.
        private IGorgonImage _noImage;

        // Pattern for sprite background.
        private GorgonTexture2DView _bgPattern;

        // The render target for rendering the sprite.
        private GorgonRenderTarget2DView _rtv = null;

        // The settings for the plug in.
        private SpriteEditorSettings _settings = new SpriteEditorSettings();
        #endregion

        #region Properties.
        /// <summary>Property to return the name of the plug in.</summary>
        string IContentPluginMetadata.PluginName => Name;

        /// <summary>Property to return the description of the plugin.</summary>
        string IContentPluginMetadata.Description => Description;

        /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
        public override bool CanCreateContent => true;

        /// <summary>Property to return the ID of the small icon for this plug in.</summary>
        public Guid SmallIconID
        {
            get;
        }

        /// <summary>Property to return the ID of the new icon for this plug in.</summary>
        public Guid NewIconID
        {
            get;
        }

        /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
        public override string ContentTypeID => SpriteEditorCommonConstants.ContentType;

        /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
        public string ContentType => Resources.GORSPR_CONTENT_TYPE;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        private void LoadCodecPlugins()
        {
            if (_settings.CodecPluginPaths.Count == 0)
            {
                return;
            }

            Log.Print("Loading sprite codecs...", LoggingLevel.Intermediate);

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
            foreach (GorgonSpriteCodecPlugin plugin in plugins.GetPlugins<GorgonSpriteCodecPlugin>())
            {
                foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
                {
                    _codecList.Add(plugin.CreateCodec(desc.Name));
                }
            }
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
        /// Function to find the image associated with the sprite file.
        /// </summary>
        /// <param name="spriteFile">The sprite file to evaluate.</param>
        /// <param name="fileManager">The file manager used to handle content files.</param>
        /// <returns>The file representing the image associated with the sprite.</returns>
        private IContentFile FindImage(IContentFile spriteFile, IContentFileManager fileManager)
        {
            if ((spriteFile.Metadata.Dependencies.Count == 0)
                || (!spriteFile.Metadata.Dependencies.TryGetValue(SpriteEditorCommonConstants.ImageDependencyType, out string texturePath)))
            {
                return null;
            }

            IContentFile textureFile = fileManager.GetFile(texturePath);

            if (textureFile == null)
            {
                Log.Print($"[ERROR] Sprite '{spriteFile.Path}' has texture '{texturePath}', but the file was not found on the file system.", LoggingLevel.Verbose);
                return null;
            }

            string textureFileContentType = textureFile.Metadata.ContentMetadata?.ContentTypeID;

            if (string.IsNullOrWhiteSpace(textureFileContentType))
            {
                Log.Print($"[ERROR] Sprite texture '{texturePath}' was found but has no content type ID.", LoggingLevel.Verbose);
                return null;
            }

            if ((!textureFile.Metadata.Attributes.TryGetValue(SpriteContent.ContentTypeAttr, out string imageType))
                || (!string.Equals(imageType, textureFileContentType, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Print($"[ERROR] Sprite '{spriteFile.Path}' has texture '{texturePath}', but the texture has a content type ID of '{textureFileContentType}', and the sprite requires a content type ID of '{imageType}'.", LoggingLevel.Verbose);
                return null;
            }

            return textureFile;            
        }

        /// <summary>
        /// Function to render the sprite to a thumbnail image.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
        /// <returns>The image containing the rendered sprite.</returns>
        private IGorgonImage RenderThumbnail(GorgonSprite sprite)
        {
                      
            GorgonRenderTargetView prevRtv = null;

            try
            {
                sprite.Anchor = new DX.Vector2(0.5f);
                
                if (sprite.Size.Width > sprite.Size.Height)
                {
                    sprite.Scale = new DX.Vector2(256.0f / (sprite.Size.Width.Max(1)));
                }
                else
                {
                    sprite.Scale = new DX.Vector2(256.0f / (sprite.Size.Height.Max(1)));
                }

                sprite.Position = new DX.Vector2(128, 128);
                sprite.TextureSampler = GorgonSamplerState.PointFiltering;

                prevRtv = GraphicsContext.Graphics.RenderTargets[0];                
                GraphicsContext.Graphics.SetRenderTarget(_rtv);
                GraphicsContext.Renderer2D.Begin();
                GraphicsContext.Renderer2D.DrawFilledRectangle(new DX.RectangleF(0, 0, 256, 256), GorgonColor.White, _bgPattern, new DX.RectangleF(0, 0, 1, 1));
                GraphicsContext.Renderer2D.DrawSprite(sprite);
                GraphicsContext.Renderer2D.End();

                return _rtv.Texture.ToImage();
            }
            finally
            {
                if (prevRtv != null)
                {
                    GraphicsContext.Graphics.SetRenderTarget(prevRtv);
                }                
            }
        }

        /// <summary>
        /// Function to load the image to be used a thumbnail.
        /// </summary>
        /// <param name="thumbnailCodec">The codec for the thumbnail images.</param>
        /// <param name="thumbnailFile">The path to the thumbnail file.</param>
        /// <param name="content">The content being thumbnailed.</param>        
        /// <param name="fileManager">The file manager used to handle content files.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>The image, image content file and sprite, or just the thumbnail image if it was cached (sprite will be null).</returns>
        private (IGorgonImage image, IContentFile imageFile, GorgonSprite sprite) LoadThumbnailImage(IGorgonImageCodec thumbnailCodec, FileInfo thumbnailFile, IContentFile content, IContentFileManager fileManager, CancellationToken cancelToken)
        {
            IGorgonImage spriteImage;
            Stream inStream = null;
            Stream imgStream = null;

            try
            {
                // If we've already got the file, then leave.
                if (thumbnailFile.Exists)
                {
                    inStream = thumbnailFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    spriteImage = thumbnailCodec.LoadFromStream(inStream);

                    if (cancelToken.IsCancellationRequested)
                    {
                        return (null, null, null);
                    }

                    return (spriteImage, null, null);
                }

                IContentFile imageFile = FindImage(content, fileManager);
                if (imageFile == null)
                {
                    return (_noImage.Clone(), null, null);
                }

                imgStream = imageFile.OpenRead();
                inStream = content.OpenRead();

                if ((!_ddsCodec.IsReadable(imgStream))
                    || (!_defaultCodec.IsReadable(inStream)))
                {
                    return (_noImage.Clone(), null, null);
                }

                spriteImage = _ddsCodec.LoadFromStream(imgStream);
                GorgonSprite sprite = _defaultCodec.FromStream(inStream);

                return (spriteImage, imageFile, sprite);
            }
            catch (Exception ex)
            {
                Log.Print($"[ERROR] Cannot create thumbnail for '{content.Path}'", LoggingLevel.Intermediate);
                Log.LogException(ex);
                return (null, null, null);
            }
            finally
            {
                imgStream?.Dispose();
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

        /// <summary>
        /// Function to retrieve the sprite texture from the dependency metadata.
        /// </summary>
        /// <param name="dependencies">The dependencies for the sprite file.</param>
        /// <param name = "fileManager" > The file manager used to access other content files.</param>
        /// <returns>A tuple containing the sprite image, and the file for the sprite image, or <b>null</b> for both entries if no image was found in the dependency list.</returns>
        private (IGorgonImage spriteImage, IContentFile imageFile) GetSpriteImageFromMetadata(IList<string> dependencies, IContentFileManager fileManager)
        {
            if (dependencies.Count == 0)
            {
                return (null, null);
            }

            IGorgonImage depImage = null;

            // Find the sprite texture dependency.
            foreach (string dependencyPath in dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependencyPath))
                {
                    continue;
                }

                IContentFile dependency = fileManager.GetFile(dependencyPath);
                
                if ((!dependency.Metadata.Attributes.TryGetValue(SpriteContent.ContentTypeAttr, out string contentType))
                    || (!string.Equals(contentType, "image", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                using (Stream depStream = dependency.OpenRead())
                {
                    if (!_ddsCodec.IsReadable(depStream))
                    {
                        Log.Print("Sprite dependency '{dependency}' found. But is not DDS file.", LoggingLevel.Verbose);
                        continue;
                    }

                    depImage = _ddsCodec.LoadFromStream(depStream);
                    return (depImage, dependency);
                }
            }

            return (null, null);
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name = "fileManager" > The file manager used to access other content files.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
        /// <param name="undoService">The undo service for the plug in.</param>
        /// <returns>A new IEditorContent object.</returns>
        /// <remarks>
        /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
        /// application or plug in is shut down, and is not stored with the project.
        /// </remarks>
        protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IViewModelInjection injector, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
        {
            var content = new SpriteContent();
            GorgonTexture2DView spriteImage = null;
            IContentFile imageFile = null;
            GorgonSprite sprite = null;
            ISpriteTextureService textureService = null;
            Stream stream = null;

            try
            {                
                textureService = new SpriteTextureService(GraphicsContext, fileManager, _ddsCodec);
                                               
                // Load the sprite image.
                (spriteImage, imageFile) = await textureService.LoadFromSpriteContentAsync(file);

                // Load the sprite now. 
                stream = file.OpenRead();
                sprite = _defaultCodec.FromStream(stream, spriteImage);

                content.Initialize(new SpriteContentParameters(file, 
                    imageFile, 
                    fileManager, 
                    textureService,
                    sprite,
                    _defaultCodec,
                    undoService, 
                    scratchArea, 
                    injector.MessageDisplay, 
                    injector.BusyService));

                return content;
            }
            catch
            {
                spriteImage?.Dispose();
                throw;
            }
            finally
            {
                stream?.Dispose();
            }
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

            _rtv?.Dispose();
            _bgPattern?.Dispose();
            _noImage?.Dispose();
            _pluginCache?.Dispose();

            ViewFactory.Unregister<ISpriteContent>();

            base.OnShutdown();
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPluginService pluginService)
        {
            ViewFactory.Register<ISpriteContent>(() => new SpriteEditorView());

            _pluginService = pluginService;
            _pluginCache = new GorgonMefPluginCache(Log);

            // Get built-in codec list.
            _defaultCodec = new GorgonV3SpriteBinaryCodec(GraphicsContext.Renderer2D);
            _codecList.Add(_defaultCodec);
            _codecList.Add(new GorgonV3SpriteJsonCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV2SpriteCodec(GraphicsContext.Renderer2D));
            _codecList.Add(new GorgonV1SpriteBinaryCodec(GraphicsContext.Renderer2D));

            SpriteEditorSettings settings = pluginService.ReadContentSettings<SpriteEditorSettings>(this);

            if (settings != null)
            {
                _settings = settings;
            }

            // Load the additional plug ins.
            LoadCodecPlugins();

            _ddsCodec = new GorgonCodecDds();

            using (var noImageStream = new MemoryStream(Resources.NoImage_256x256))
            {
                _noImage = _ddsCodec.LoadFromStream(noImageStream);
            }

            _bgPattern = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo($"Sprite_Editor_Bg_Preview_{Guid.NewGuid():N}"), EditorCommonResources.CheckerBoardPatternImage);

            _rtv = GorgonRenderTarget2DView.CreateRenderTarget(GraphicsContext.Graphics, new GorgonTexture2DInfo($"SpriteEditor_Rtv_Preview_{Guid.NewGuid():N}")
            {
                Width = 256,
                Height = 256,
                Format = BufferFormat.R8G8B8A8_UNorm,
                Binding = TextureBinding.ShaderResource
            });
        }

        /// <summary>
        /// Function to update the dependencies for the sprite.
        /// </summary>
        /// <param name="fileStream">The stream for the file.</param>
        /// <param name="dependencyList">The list of dependency file paths.</param>
        /// <param name="fileManager">The content file management system.</param>
        private void UpdateDependencies(Stream fileStream, Dictionary<string, string> dependencyList, IContentFileManager fileManager)
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

            dependencyList[SpriteEditorCommonConstants.ImageDependencyType] = textureName;
        }

        /// <summary>Function to retrieve the default content for the plug in.</summary>
        /// <param name="name">The name of the content (if applicable).</param>
        /// <returns>A byte array containing the default content data.</returns>
        /// <remarks>
        ///   <para>
        /// This is used to generate default content data when creating new content.
        /// </para>
        ///   <para>
        /// This method will not be called if <see cref="P:Gorgon.Editor.Plugins.ContentPlugin.CanCreateContent"/> is <b>false</b>.
        /// </para>
        /// </remarks>
        public override Task<byte[]> GetDefaultContentAsync(string name)
        {
            var defaultSprite = new GorgonSprite();

            using (var stream = new MemoryStream())
            {
                _defaultCodec.Save(defaultSprite, stream);
                return Task.FromResult(stream.ToArray());
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

        /// <summary>Function to retrieve a thumbnail for the content.</summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <param name="outputFile">The output file for the thumbnail data.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="T:Gorgon.Graphics.Imaging.IGorgonImage"/> containing the thumbnail image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile" />, <paramref name="fileManager" /> or the <paramref name="outputFile" /> parameter is <b>null</b>.</exception>
        public async Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, IContentFileManager fileManager, FileInfo outputFile, CancellationToken cancelToken)
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

            // If the content is not a v3 sprite, then leave it.
            if ((!contentFile.Metadata.Attributes.TryGetValue(SpriteContent.CodecAttr, out string codecName))
                || (string.IsNullOrWhiteSpace(codecName))
                || (!string.Equals(codecName, _defaultCodec.GetType().FullName, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
                outputFile.Directory.Refresh();
            }

            IGorgonImageCodec pngCodec = new GorgonCodecPng();

            (IGorgonImage image, IContentFile imageFile, GorgonSprite sprite) = await Task.Run(() => LoadThumbnailImage(pngCodec, outputFile, contentFile, fileManager, cancelToken));

            if ((image == null) || (cancelToken.IsCancellationRequested))
            {
                return null;
            }

            // We loaded a cached thumbnail.
            if ((sprite == null) || (imageFile == null))
            {
                return image;
            }

            // We need to switch back to the main thread here to render the image, otherwise things will break.
            Cursor.Current = Cursors.WaitCursor;

            GorgonTexture2DView spriteTexture = null;
            IGorgonImage resultImage = null;

            try
            {                
                spriteTexture = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo(imageFile.Path)
                {
                    Width = image.Width,
                    Height = image.Height,
                    Format = image.Format,
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Default
                }, image);
                sprite.Texture = spriteTexture;

                resultImage = RenderThumbnail(sprite);

                await Task.Run(() => pngCodec.SaveToFile(resultImage, outputFile.FullName), cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                contentFile.Metadata.Attributes[CommonEditorConstants.ThumbnailAttr] = outputFile.Name;
                return resultImage;
            }
            catch (Exception ex)
            {
                Log.Print($"[ERROR] Cannot create thumbnail for '{contentFile.Path}'", LoggingLevel.Intermediate);
                Log.LogException(ex);
                return null;
            }
            finally
            {
                image?.Dispose();
                spriteTexture?.Dispose();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>Function to retrieve the icon used for new content creation.</summary>
        /// <returns>An image for the icon.</returns>
        /// <remarks>
        /// <para>
        /// This method is never called when <see cref="IContentPluginMetadata.CanCreateContent"/> is <b>false</b>.
        /// </para>
        /// </remarks>
        public Image GetNewIcon() => Resources.sprite_24x24;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the SpriteEditorPlugin class.</summary>
        public SpriteEditorPlugin()
            : base(Resources.GORSPR_DESC)
        {
            SmallIconID = Guid.NewGuid();
            NewIconID = Guid.NewGuid();
        }
        #endregion
    }
}
