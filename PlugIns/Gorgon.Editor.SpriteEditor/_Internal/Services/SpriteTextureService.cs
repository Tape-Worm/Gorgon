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

using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Functionality for handling an associated sprite texture.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteTextureService"/> class.</remarks>
/// <param name="graphicsContext">The graphics context for the application.</param>
/// <param name="fileManager">The content file manager.</param>
/// <param name="spriteCodec">The codec used to read sprite data.</param>
/// <param name="imageCodec">The codec used to read image data.</param>
/// <param name="log">The logging interface for debug logging.</param>
internal class SpriteTextureService(IGraphicsContext graphicsContext, IContentFileManager fileManager, IGorgonSpriteCodec spriteCodec, IGorgonImageCodec imageCodec, IGorgonLog log)
{
    #region Variables.
    // The graphics interface for the application.
    private readonly GorgonGraphics _graphics = graphicsContext.Graphics;
    // The 2D renderer for the application.
    private readonly Gorgon2D _renderer = graphicsContext.Renderer2D;
    // The content file manager.
    private readonly IContentFileManager _fileManager = fileManager;
    // The image codec used to read image file data.
    private readonly IGorgonImageCodec _imageCodec = imageCodec;
    // The codec used to read sprite data.
    private readonly IGorgonSpriteCodec _spriteCodec = spriteCodec;
    // The logging interface for debug logging.
    private readonly IGorgonLog _log = log;
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
        IGorgonImage tempImage = null;
        BufferFormat targetFormat = texture.FormatInformation.IsSRgb ? BufferFormat.R8G8B8A8_UNorm_SRgb : BufferFormat.R8G8B8A8_UNorm;

        try
        {
            IGorgonImage resultImage = new GorgonImage(new GorgonImageInfo(ImageDataType.Image2D, targetFormat)
            {
                Width = texture.Width,
                Height = texture.Height,
                ArrayCount = texture.ArrayCount
            });

            convertTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(texture)
            {
                Name = "Convert_rtv",
                ArrayCount = 1,
                MipLevels = 1,
                Format = targetFormat,
                Binding = TextureBinding.ShaderResource
            });

            _graphics.SetRenderTarget(convertTarget);

            for (int i = 0; i < texture.ArrayCount; ++i)
            {
                convertTarget.Clear(GorgonColor.BlackTransparent);

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
            convertTarget?.Dispose();
        }
    }

    /// <summary>
    /// Function to determine if the content file passed in is image content as supported by this editor.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <returns><b>true</b> if the file is an image, supported by this editor, or <b>false</b> if not.</returns>
    public bool IsContentImage(IContentFile file) => ((file is not null)
            && (file.Metadata is not null)
            && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
            && (string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Function to retrieve the image data for a sprite texture as a 32 bit RGBA pixel data.</summary>
    /// <param name="texture">The texture to extract the data from.</param>
    /// <returns>The image data for the texture.</returns>
    public async Task<IGorgonImage> GetSpriteTextureImageDataAsync(GorgonTexture2DView texture)
    {
        IGorgonImage imageData = texture.Texture.ToImage();

        if (imageData.Format is BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb)
        {
            return imageData;
        }

        if ((imageData.FormatInfo.IsSRgb)
            && (imageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm_SRgb)))
        {
            await Task.Run(() => imageData.BeginUpdate().ConvertToFormat(BufferFormat.R8G8B8A8_UNorm_SRgb).EndUpdate());
            return imageData;
        }
        else if (imageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
        {
            await Task.Run(() => imageData.BeginUpdate().ConvertToFormat(BufferFormat.R8G8B8A8_UNorm).EndUpdate());
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
        Debug.Assert(spriteContent.Metadata is not null, "No meta data for sprite content!");

        _log.Print($"Loading sprite texture for '{spriteContent.Path}'...", LoggingLevel.Verbose);
        (IGorgonImage imageData, IContentFile file) = await Task.Run(() =>
        {
            if ((!spriteContent.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> dependency))
                    || (dependency.Count == 0))
            {
                _log.Print("WARNING: No sprite texture dependency found, interrogating sprite data...", LoggingLevel.Verbose);
                // If there's no linkage, then see if the sprite has the path information embedded within its data.
                using Stream spriteStream = _fileManager.OpenStream(spriteContent.Path, FileMode.Open);
                string textureName = _spriteCodec.GetAssociatedTextureName(spriteStream);

                if ((string.IsNullOrWhiteSpace(textureName))
                    || (!_fileManager.FileExists(textureName)))
                {

                    return (null, null);
                }

                dependency = [textureName];
            }

            _log.Print($"Found sprite texture '{dependency[0]}'...", LoggingLevel.Verbose);
            IContentFile imageFile = _fileManager.GetFile(dependency[0]);

            if (!IsContentImage(imageFile))
            {
                _log.Print($"ERROR: '{dependency[0]}' not found in project or is not an image content file.", LoggingLevel.Simple);
                return (null, null);
            }

            using Stream stream = _fileManager.OpenStream(imageFile.Path, FileMode.Open);
            if (!_imageCodec.IsReadable(stream))
            {
                _log.Print($"ERROR: '{dependency[0]}' is not a {_imageCodec.Name} file.", LoggingLevel.Simple);
                return ((IGorgonImage, IContentFile imageFile))(null, null);
            }
            else
            {
                _log.Print($"Texture '{dependency[0]}' found and loaded.", LoggingLevel.Verbose);
                return (_imageCodec.FromStream(stream), imageFile);
            }
        });

        if ((imageData is null) || (file is null))
        {
            return (null, null);
        }

        try
        {
            return (GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(imageData.Width, imageData.Height, imageData.Format)
            {
                Name = file.Path,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default,
                IsCubeMap = false
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
    /// <exception cref="FileNotFoundException">Thrown if the <paramref name="file"/> was not found.</exception>
    /// <exception cref="GorgonException">Thrown if the content file is not an image that can be read by using the default codec.</exception>
    public async Task<GorgonTexture2DView> LoadTextureAsync(IContentFile file)
    {
        if (!IsContentImage(file))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORSPR_ERR_NOT_SUPPORTED, file.Path));
        }

        IGorgonImage LoadImage()
        {
            using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
            return !_imageCodec.IsReadable(stream)
                ? throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORSPR_ERR_TEXTURE_CANNOT_READ, file.Path))
                : _imageCodec.FromStream(stream);
        }

        IGorgonImage imageData = await Task.Run(LoadImage);

        try
        {
            return GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(imageData.Width, imageData.Height, imageData.Format)
            {
                Name = file.Path,
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
        using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
        return ((!_imageCodec.IsReadable(stream)) || (!IsContentImage(file)))
            ? null
            : _imageCodec.GetMetaData(stream);
    }

    #endregion
}
