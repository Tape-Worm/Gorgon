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
// Created: March 18, 2019 12:11:28 PM
// 
#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Functionality for handling an associated sprite texture.
    /// </summary>
    internal class SpriteTextureService
        : ISpriteTextureService
    {
        #region Variables.
        // The graphics interface for the application.
        private readonly GorgonGraphics _graphics;
        // The 2D renderer for the application.
        private readonly Gorgon2D _renderer;
        // The content file manager.
        private readonly IContentFileManager _fileManager;
        // The image codec used to read image file data.
        private readonly IGorgonImageCodec _codec;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to render the image data into an 32 bit RGBA pixel format render target and then return it as the properly formatted image data.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <returns>The converted image data for the texture.</returns>
        private IGorgonImage RenderImageData(GorgonTexture2DView texture)
        {
            GorgonRenderTargetView oldRtv = _graphics.RenderTargets[0];
            GorgonRenderTarget2DView convertTarget = null;
            GorgonTexture2DView rtvTexture = null;
            IGorgonImage tempImage = null;
            BufferFormat targetFormat = texture.FormatInformation.IsSRgb ? BufferFormat.R8G8B8A8_UNorm_SRgb : BufferFormat.R8G8B8A8_UNorm;

            try
            {
                IGorgonImage resultImage = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, targetFormat)
                {
                    Width = texture.Width,
                    Height = texture.Height,
                    ArrayCount = texture.ArrayCount
                });

                convertTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(texture, "Convert_rtv")
                {
                    ArrayCount = 1,
                    MipLevels = 1,
                    Format = targetFormat,
                    Binding = TextureBinding.ShaderResource
                });

                rtvTexture = convertTarget.GetShaderResourceView();

                for (int i = 0; i < texture.ArrayCount; ++i)
                {
                    convertTarget.Clear(GorgonColor.BlackTransparent);
                    _graphics.SetRenderTarget(convertTarget);
                    _renderer.Begin();
                    _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, texture.Width, texture.Height),
                        GorgonColor.White,
                        texture,
                        new DX.RectangleF(0, 0, 1, 1), i);
                    _renderer.End();

                    tempImage?.Dispose();
                    tempImage = convertTarget.Texture.ToImage();
                    tempImage.Buffers[0].CopyTo(resultImage.Buffers[0, i]);
                }

                return resultImage;
            }
            finally
            {
                tempImage?.Dispose();
                _graphics.SetRenderTarget(oldRtv);
                rtvTexture?.Dispose();
                convertTarget?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if the content file passed in is image content as supported by this editor.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns><b>true</b> if the file is an image, supported by this editor, or <b>false</b> if not.</returns>
        public bool IsContentImage(IContentFile file) => ((file != null)
                && (file.Metadata != null)
                && (file.Metadata.Attributes.TryGetValue(SpriteContent.ContentTypeAttr, out string contentType))
                && (string.Equals(contentType, "image", StringComparison.OrdinalIgnoreCase)));

        /// <summary>Function to retrieve the image data for a sprite texture as a 32 bit RGBA pixel data.</summary>
        /// <param name="texture">The texture to extract the data from.</param>
        /// <returns>The image data for the texture.</returns>
        public async Task<IGorgonImage> GetSpriteTextureImageDataAsync(GorgonTexture2DView texture)
        {
            IGorgonImage imageData = texture.Texture.ToImage();

            if ((imageData.Format == BufferFormat.R8G8B8A8_UNorm)
                || (imageData.Format == BufferFormat.R8G8B8A8_UNorm_SRgb))
            {
                return imageData;
            }                       

            if ((imageData.FormatInfo.IsSRgb)
                && (imageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm_SRgb)))
            {
                await Task.Run(() => imageData.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm_SRgb));
                return imageData;
            } else if (imageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
            {
                await Task.Run(() => imageData.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm));
                return imageData;
            }

            // OK, so this is going to take drastic measures.
            imageData.Dispose();
            return RenderImageData(texture);
        }

        /// <summary>Function to load an associated sprite texture for sprite content.</summary>
        /// <param name="spriteContent">The sprite content file to use.</param>
        /// <returns>The texture associated with the sprite, and the content file associated with that texture, or <b>null</b> if no sprite texture was found.</returns>
        public async Task<(GorgonTexture2DView, IContentFile)> LoadFromSpriteContentAsync(IContentFile spriteContent)
        {
            if ((spriteContent.Metadata == null)
                || (spriteContent.Metadata.Dependencies.Count == 0))
            {
                return (null, null);
            }

            (IGorgonImage imageData, IContentFile file) = await Task.Run(() =>
            {
                if (!spriteContent.Metadata.Dependencies.TryGetValue(SpriteEditorCommonConstants.ImageDependencyType, out string dependency))
                {
                    return (null, null);
                }

                IContentFile imageFile = _fileManager.GetFile(dependency);

                if (!IsContentImage(imageFile))
                {                    
                    return (null, null);
                }                    

                using (Stream stream = imageFile.OpenRead())
                {
                    return !_codec.IsReadable(stream) ? ((IGorgonImage, IContentFile imageFile))(null, null) : (_codec.LoadFromStream(stream), imageFile);
                }
            });
            
            if ((imageData == null) || (file == null))
            {
                return (null, null);
            }

            try
            {
                return (GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(file.Path)
                    {
                        Binding = TextureBinding.ShaderResource,
                        Usage = ResourceUsage.Default
                    }, imageData), file);
            }
            finally
            {
                imageData?.Dispose();
            }
        }

        /// <summary>
        /// Function to load a texture from the file system.
        /// </summary>
        /// <param name="file">The content file for the texture.</param>
        /// <returns>The texture from the file system.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file on the <paramref name="path"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown if the content file is not an image that can be read by using the default codec.</exception>
        public async Task<GorgonTexture2DView> LoadTextureAsync(IContentFile file)
        {
            if (!IsContentImage(file))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORSPR_ERR_NOT_SUPPORTED, file.Path));
            }

            IGorgonImage imageData = await Task.Run(() =>
            {
                using (Stream stream = file.OpenRead())
                {
                    if (!_codec.IsReadable(stream))
                    {
                        throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORSPR_ERR_TEXTURE_CANNOT_READ, file.Path));
                    }

                    return _codec.LoadFromStream(stream);
                }
            });

            try
            {
                return GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(file.Path)
                {
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Default
                }, imageData);
            }
            finally
            {
                imageData?.Dispose();
            }
        }

        /// <summary>
        /// Function to retrieve the image metadata for a content file.
        /// </summary>
        /// <param name="file">The file to retrieve metadata from.</param>
        /// <returns>The metadata for the file, or <b>null</b> if the file is not an image.</returns>        
        public IGorgonImageInfo GetImageMetadata(IContentFile file)
        {
            using (Stream stream = file.OpenRead())
            {
                return ((!_codec.IsReadable(stream)) || (!IsContentImage(file)))
                    ? null
                    : _codec.GetMetaData(stream);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteTextureService"/> class.</summary>
        /// <param name="graphicsContext">The graphics context for the application.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <param name="codec">The codec used to read image data.</param>
        public SpriteTextureService(IGraphicsContext graphicsContext, IContentFileManager fileManager, IGorgonImageCodec codec)
        {
            _graphics = graphicsContext.Graphics;
            _renderer = graphicsContext.Renderer2D;
            _fileManager = fileManager;
            _codec = codec;
        }
        #endregion
    }
}
