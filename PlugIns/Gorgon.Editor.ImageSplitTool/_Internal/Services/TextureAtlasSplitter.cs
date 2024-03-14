
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 18, 2020 11:10:05 AM
// 


using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageSplitTool.Properties;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.ImageSplitTool;

/// <summary>
/// A service used to split an atlas texture into smaller images by using the associated sprite regions to define the image size
/// </summary>
internal class TextureAtlasSplitter
{

    // The core graphics interface from the editor.
    private readonly GorgonGraphics _graphics;
    // The renderer used for rendering the image data.
    private readonly Gorgon2D _renderer;
    // The application file manager.
    private readonly IContentFileManager _fileManager;
    // The default codec for loading the images.
    private readonly IGorgonImageCodec _imageCodec;
    // The default codec for loading the sprites.
    private readonly IGorgonSpriteCodec _spriteCodec;
    // The log for debug information.
    private readonly IGorgonLog _log;
    // The list of image and sprite files.
    private readonly IReadOnlyDictionary<IContentFile, IReadOnlyList<IContentFile>> _imagesAndSprites;







    /// <summary>
    /// Function to load the image data.
    /// </summary>
    /// <param name="imageFile">The content file for the image data.</param>
    /// <returns>The image data.</returns>
    private IGorgonImage LoadImage(IContentFile imageFile)
    {
        using Stream stream = _fileManager.OpenStream(imageFile.Path, FileMode.Open);
        return _imageCodec.FromStream(stream);
    }

    /// <summary>
    /// Function to load the sprite data.
    /// </summary>
    /// <param name="spriteFiles">The files containing sprite data.</param>
    /// <param name="texture">The texture atlas for the sprites.</param>
    /// <param name="cancelToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A list of sprites.</returns>
    private IReadOnlyList<(IContentFile file, GorgonSprite sprite)> LoadSprites(IReadOnlyList<IContentFile> spriteFiles, GorgonTexture2DView texture, CancellationToken cancelToken)
    {
        var result = new List<(IContentFile, GorgonSprite)>();

        foreach (IContentFile file in spriteFiles)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return Array.Empty<(IContentFile, GorgonSprite)>();
            }

            using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
            GorgonSprite sprite = _spriteCodec.FromStream(stream, texture);

            if (sprite.Texture != texture)
            {
                _log.Print($"WARNING: The sprite '{file.Path}' has texture '{sprite.Texture?.Texture.Name}' when it should have '{texture.Texture.Name}'. Skipping this sprite.", LoggingLevel.Intermediate);
                continue;
            }

            result.Add((file, sprite));
        }

        return result;
    }

    /// <summary>
    /// Function to retrieve a new name for a file.
    /// </summary>
    /// <param name="outputDirectory">The directory where the file will be stored.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="extension">The extension for the file name.</param>
    /// <returns>The new name for the file.</returns>
    private string GetNewPath(string outputDirectory, string fileName, string extension)
    {
        string newName = Path.GetFileNameWithoutExtension(fileName);

        if (!string.IsNullOrWhiteSpace(extension))
        {
            if (extension.StartsWith(".", StringComparison.Ordinal))
            {
                newName = $"{newName}{extension}";
            }
            else
            {
                newName = $"{newName}.{extension}";
            }
        }

        return $"{outputDirectory.FormatDirectory('/')}{newName}";
    }

    /// <summary>
    /// Function to check for the existence of a file.
    /// </summary>
    /// <param name="outputDirectory">The directory where the file will be stored.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="extension">The extension for the file name.</param>
    /// <param name="prevResult">The result from a previous call to this method.</param>
    /// <returns>The answer from the user when prompted with overwriting a file.</returns>
    private ConfirmationResult CheckFileExists(string outputDirectory, string fileName, string extension, ConfirmationResult prevResult)
    {
        if (((prevResult & ConfirmationResult.ToAll) == ConfirmationResult.ToAll) && ((prevResult & ConfirmationResult.Yes) == ConfirmationResult.Yes))
        {
            return prevResult;
        }

        ConfirmationResult response = prevResult & ~ConfirmationResult.ToAll;
        string outputPath = GetNewPath(outputDirectory, fileName, extension);

        IContentFile file = _fileManager.GetFile(outputPath);

        if (file is not null)
        {
            if (file.IsOpen)
            {
                GorgonDialogs.ErrorBox(GorgonApplication.MainForm, string.Format(Resources.GORIST_ERR_CANNOT_ACCESS_FILESYSTEM_LOCK, file.Path));
                return ConfirmationResult.Cancel;
            }

            if ((prevResult & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
            {
                response = GorgonDialogs.ConfirmBox(GorgonApplication.MainForm, string.Format(Resources.GORIST_CONFIRM_FILE_EXISTS, outputPath), allowCancel: true, allowToAll: true);
            }
        }

        return response == ConfirmationResult.None ? ConfirmationResult.Yes : response;
    }

    /// <summary>
    /// Function to create the split image from the sprite data.
    /// </summary>
    /// <param name="sprite">The sprite information used to clip the image.</param>
    /// <returns>The new image data.</returns>
    private IGorgonImage GetImage(GorgonSprite sprite)
    {
        // Check the format for support.
        BufferFormat format = BufferFormat.R8G8B8A8_UNorm;

        if ((_graphics.FormatSupport.TryGetValue(sprite.Texture.Format, out IGorgonFormatSupportInfo formatInfo))
            && (formatInfo.IsRenderTargetFormat))
        {
            format = sprite.Texture.Format;
        }

        using var target = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo((int)sprite.Size.Width,
                                                                                                                                   (int)sprite.Size.Height,
                                                                                                                                   format)
        {
            Binding = TextureBinding.RenderTarget
        });
        GorgonRenderTargetView currentTarget = _graphics.RenderTargets[0];

        _graphics.SetRenderTarget(target);
        target.Clear(GorgonColor.BlackTransparent);
        _renderer.Begin();
        _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, target.Width, target.Height), GorgonColor.White,
                                      sprite.Texture,
                                      sprite.TextureRegion,
                                      sprite.TextureArrayIndex,
                                      GorgonSamplerState.PointFiltering);
        _renderer.End();

        _graphics.SetRenderTarget(currentTarget);
        return target.Texture.ToImage();
    }

    /// <summary>
    /// Function to split the texture atlas into smaller images based on sprite coordinates.
    /// </summary>
    /// <param name="sprites">The list of sprites to evaluate.</param>
    /// <param name="outputDirectory">The directory to write into.</param>
    /// <returns>The list of updated images.</returns>
    private IReadOnlyList<(string newImagePath, IGorgonImage image, string newSpritePath, GorgonSprite sprite)> SplitImage(IReadOnlyList<(IContentFile file, GorgonSprite sprite)> sprites, string outputDirectory, ref ConfirmationResult response)
    {
        // The list of images from the sprites.
        var result = new List<(string newImagePath, IGorgonImage image, string newSpritePath, GorgonSprite sprite)>();
        string imageFileExtension = _imageCodec.CodecCommonExtensions.Count > 0 ? _imageCodec.CodecCommonExtensions[0] : string.Empty;
        string spriteFileExtension = _spriteCodec.FileExtensions.Count > 0 ? _spriteCodec.FileExtensions[0].Extension : string.Empty;

        for (int i = 0; i < sprites.Count; i++)
        {
            (IContentFile file, GorgonSprite sprite) = sprites[i];

            string imageFileName = Path.GetFileNameWithoutExtension(file.Name) + "_Texture";
            response = CheckFileExists(outputDirectory, imageFileName, imageFileExtension, response);

            if ((response & ConfirmationResult.No) == ConfirmationResult.No)
            {
                continue;
            }

            if (response == ConfirmationResult.Cancel)
            {
                return Array.Empty<(string, IGorgonImage, string, GorgonSprite)>();
            }

            response = CheckFileExists(outputDirectory, file.Name, spriteFileExtension, response);

            if ((response & ConfirmationResult.No) == ConfirmationResult.No)
            {
                continue;
            }

            if (response == ConfirmationResult.Cancel)
            {
                return Array.Empty<(string, IGorgonImage, string, GorgonSprite)>();
            }

            string newImageName = GetNewPath(outputDirectory, imageFileName, imageFileExtension);

            IGorgonImage newImage = GetImage(sprite);

            result.Add((newImageName, newImage, GetNewPath(outputDirectory, file.Name, spriteFileExtension), sprite));

            // Reassign a new texture to the sprite with our new image data (required when saving).
            sprite.Texture = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(newImage.Width, newImage.Height, newImage.Format)
            {
                Name = newImageName,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                ArrayCount = 1,
                MipLevels = 1,
                IsCubeMap = false
            }, newImage);

            sprite.TextureArrayIndex = 0;
            sprite.TextureRegion = new DX.RectangleF(0, 0, 1, 1);
        }

        return result;
    }

    /// <summary>
    /// Function to save the images and sprites.
    /// </summary>
    /// <param name="newImages">The list of updates images and sprites along with updated paths.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    private void SaveImages(IReadOnlyList<(string newImagePath, IGorgonImage image, string newSpritePath, GorgonSprite sprite)> newImages, CancellationToken cancelToken)
    {
        foreach ((string newImagePath, IGorgonImage image, string newSpritePath, GorgonSprite sprite) in newImages)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            // Write out the image data.
            using (Stream imageStream = _fileManager.OpenStream(newImagePath, FileMode.Create))
            {
                _imageCodec.Save(image, imageStream);
            }

            using Stream spriteStream = _fileManager.OpenStream(newSpritePath, FileMode.Create);
            _spriteCodec.Save(sprite, spriteStream);
        }
    }

    /// <summary>
    /// Function to split the texture atlas into smaller images.
    /// </summary>
    /// <param name="selectedImages">The images that were selected for processing.</param>
    /// <param name="outputDirectory">The directory that will receive the new images and updated sprites.</param>
    /// <param name="progress">A callback to show the current image file being processed.</param>
    /// <param name="cancelToken">A cancellation token for cancelling the operation.</param>
    /// <returns>A task for asynchronous operation.</returns>
    public async Task SplitAsync(IReadOnlyList<ContentFileExplorerFileEntry> selectedImages, string outputDirectory, Action<string> progress, CancellationToken cancelToken)
    {
        ConfirmationResult response = ConfirmationResult.None;

        foreach (ContentFileExplorerFileEntry fileEntry in selectedImages)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            IContentFile imageFile = _fileManager.GetFile(fileEntry.FullPath);

            if (imageFile is null)
            {
                _log.Print($"WARNING: The image file '{imageFile.Path}' is missing!", LoggingLevel.Intermediate);
                continue;
            }

            if (!_imagesAndSprites.TryGetValue(imageFile, out IReadOnlyList<IContentFile> spriteFiles))
            {
                _log.Print($"WARNING: The image file '{imageFile.Path}' is not a texture atlas!", LoggingLevel.Intermediate);
                continue;
            }

            _log.Print($"Reading image file '{imageFile.Path}'.", LoggingLevel.Verbose);

            progress?.Invoke(imageFile.Name);
            GorgonTexture2DView textureAtlas = null;
            IGorgonImage imageData = null;
            IReadOnlyList<(string newImagePath, IGorgonImage image, string newSpritePath, GorgonSprite sprite)> newImages = null;
            IReadOnlyList<(IContentFile file, GorgonSprite sprite)> sprites = null;
            try
            {
                imageData = await Task.Run(() => LoadImage(imageFile), cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                textureAtlas = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(imageData.Width, imageData.Height, imageData.Format)
                {
                    Name = imageFile.Name,
                    MipLevels = imageData.MipCount,
                    ArrayCount = imageData.ArrayCount
                }, imageData);

                // Retrieve any sprites associated with the texture atlas.
                sprites = await Task.Run(() => LoadSprites(spriteFiles, textureAtlas, cancelToken), cancelToken);

                if ((sprites.Count == 0) || (cancelToken.IsCancellationRequested))
                {
                    return;
                }

                // Begin the splitting operation.
                newImages = SplitImage(sprites, outputDirectory, ref response);

                if ((newImages.Count == 0) || (cancelToken.IsCancellationRequested))
                {
                    return;
                }

                await Task.Run(() => SaveImages(newImages, cancelToken), cancelToken);
            }
            finally
            {
                if (sprites is not null)
                {
                    foreach ((_, GorgonSprite sprite) in sprites)
                    {
                        sprite?.Texture?.Dispose();
                    }
                }

                if (newImages is not null)
                {
                    foreach ((_, IGorgonImage image, _, _) in newImages)
                    {
                        image?.Dispose();
                    }
                }
                imageData?.Dispose();
                textureAtlas?.Dispose();
            }
        }
    }



    /// <summary>Initializes a new instance of the <see cref="TextureAtlasSplitter"/> class.</summary>
    /// <param name="renderer">The renderer used to generate image data.</param>
    /// <param name="imagesAndSprites">The list of image and sprite dependency files</param>
    /// <param name="fileManager">The file manager used to manipulate the application file system.</param>
    /// <param name="imageCodec">The codec used for loading images.</param>
    /// <param name="spriteCodec">The codec used for loading sprites.</param>
    /// <param name="log">The log for debug messages.</param>
    public TextureAtlasSplitter(Gorgon2D renderer, IReadOnlyDictionary<IContentFile, IReadOnlyList<IContentFile>> imagesAndSprites, IContentFileManager fileManager, IGorgonImageCodec imageCodec, IGorgonSpriteCodec spriteCodec, IGorgonLog log)
    {
        _renderer = renderer;
        _imagesAndSprites = imagesAndSprites;
        _graphics = _renderer.Graphics;
        _fileManager = fileManager;
        _imageCodec = imageCodec;
        _spriteCodec = spriteCodec;
        _log = log;
    }
        
}
