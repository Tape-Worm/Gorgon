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
// Created: April 27, 2019 12:22:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used to retrieve sprite data from a texture atlas.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can use this to extract sprite information using a fixed size grid to retrieve texture coordinates from a texture passed to the service.
    /// </para>
    /// </remarks>
    public class SpriteExtractorService
        : ISpriteExtractorService
    {
        #region Variables.
        // The renderer for preparing a compatible texture.
        private readonly Gorgon2D _renderer;
        // The graphics interface.
        private readonly GorgonGraphics _graphics;
        // The file manager used to write the content files.
        private readonly IContentFileManager _fileManager;
        // The default sprite codec.
        private readonly IGorgonSpriteCodec _defaultCodec;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to detect if a sprite has no pixel data.
        /// </summary>
        /// <param name="bounds">The bounds of the sprite on the texture.</param>
        /// <param name="imageData">The system memory representation of the texture.</param>
        /// <param name="skipMask">The color used to mask which pixels are considered empty.</param>
        /// <returns><b>true</b> if sprite is empty, <b>false</b> if not.</returns>
        private bool IsEmpty(DX.Rectangle bounds, IGorgonImageBuffer imageData, GorgonColor skipMask)
        {
            int pixelSize = imageData.FormatInformation.SizeInBytes;
            int color = skipMask.ToABGR();

            for (int y = bounds.Y; y < bounds.Bottom; ++y)
            {
                for (int x = bounds.X; x < bounds.Right; ++x)
                {
                    // The last byte of the pixel should be the alpha (we force conversion to R8G8B8A8).
                    int value = imageData.Data.ReadAs<int>((y * imageData.PitchInformation.RowPitch) + (x * pixelSize));
                    if (color != value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Function to extract the rectangle that defines the sprite on the texture, in pixels.
        /// </summary>
        /// <param name="column">The current column.</param>
        /// <param name="row">The current row.</param>
        /// <param name="offset">The offset of the grid from the upper left corner of the texture.</param>
        /// <param name="cellSize">The size of the cell, in pixels.</param>
        /// <returns>The pixel coordinates for the sprite.</returns>
        private DX.Rectangle GetSpriteRect(int column, int row, DX.Point offset, DX.Size2 cellSize)
        {
            var upperLeft = new DX.Point(column * cellSize.Width + offset.X, row * cellSize.Height + offset.Y);
            return new DX.Rectangle(upperLeft.X, upperLeft.Y, cellSize.Width, cellSize.Height);
        }

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

                for (int i = 0; i < texture.ArrayCount; ++i)
                {
                    convertTarget.Clear(GorgonColor.BlackTransparent);
                    _graphics.SetRenderTarget(convertTarget);
                    _renderer.Begin();
                    _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, texture.Width, texture.Height),
                        GorgonColor.White,
                        texture,
                        new DX.RectangleF(0, 0, 1, 1),
                        i);
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
        /// Function to retrieve the image data for a sprite texture as a 32 bit RGBA pixel data.
        /// </summary>
        /// <param name="data">The data used for sprite extraction.</param>
        /// <returns>The image data for the texture.</returns>
        public async Task<IGorgonImage> GetSpriteTextureImageDataAsync(SpriteExtractionData data)
        {
            IGorgonImage imageData = data.Texture.Texture.ToImage();

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
            }
            else if (imageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
            {
                await Task.Run(() => imageData.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm));
                return imageData;
            }

            // OK, so this is going to take drastic measures.
            imageData.Dispose();
            return RenderImageData(data.Texture);
        }

        /// <summary>Function to retrieve the sprites from the texture atlas.</summary>
        /// <param name="data">The data used to extract the sprites.</param>
        /// <param name="imageData">The system memory version of the texture.</param>
        /// <param name="progressCallback">A callback method used to report on progress of the operation.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>A list of sprites generated by this method.</returns>
        public IReadOnlyList<GorgonSprite> ExtractSprites(SpriteExtractionData data, IGorgonImage imageData, Action<ProgressData> progressCallback, CancellationToken cancelToken)
        {
            var result = new List<GorgonSprite>();

            progressCallback?.Invoke(new ProgressData(0, data.SpriteCount));

            int count = 1;

            for (int array = 0; array < data.ArrayCount; ++array)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return Array.Empty<GorgonSprite>();
                }

                for (int y = 0; y < data.GridSize.Height; ++y)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return Array.Empty<GorgonSprite>();
                    }

                    for (int x = 0; x < data.GridSize.Width; ++x)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return Array.Empty<GorgonSprite>();
                        }

                        DX.Rectangle spriteRect = GetSpriteRect(x, y, data.GridOffset, data.CellSize);

                        if ((data.SkipEmpty) && (IsEmpty(spriteRect, imageData.Buffers[0, array + data.StartArrayIndex], data.SkipColor)))
                        {
                            continue;
                        }

                        result.Add(new GorgonSprite
                        {
                            TextureArrayIndex = array + data.StartArrayIndex,
                            Texture = data.Texture,
                            Anchor = DX.Vector2.Zero,
                            Size = new DX.Size2F(data.CellSize.Width, data.CellSize.Height),
                            TextureRegion = data.Texture.ToTexel(spriteRect),
                            TextureSampler = GorgonSamplerState.PointFiltering
                        });

                        progressCallback?.Invoke(new ProgressData(count++, data.SpriteCount));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Function to save the sprites to the file system.
        /// </summary>
        /// <param name="path">The path to save the sprites into.</param>
        /// <param name="baseFileName">The base file name.</param>
        /// <param name="sprites">The sprites to save.</param>
        /// <param name="textureFile">The texture file associated with the sprites.</param>
        /// <exception cref="IOException">Thrown when the file system is locked by another thread (after 10 seconds).</exception>
        public void SaveSprites(string path, string baseFileName, IEnumerable<GorgonSprite> sprites, IContentFile textureFile)
        {
            int spriteIndex = 1;

            string fileName = Path.GetFileNameWithoutExtension(baseFileName);
            string extension = Path.GetExtension(baseFileName);

            if ((!string.IsNullOrWhiteSpace(extension)) && (extension[0] != '.'))
            {
                extension = "." + extension;
            }

            foreach (GorgonSprite sprite in sprites)
            {
                string filePath = $"{path}{fileName} ({spriteIndex++}){(string.IsNullOrWhiteSpace(extension) ? string.Empty : extension)}";

                while (_fileManager.FileExists(filePath))
                {
                    filePath = $"{path}{fileName} ({spriteIndex++}){(string.IsNullOrWhiteSpace(extension) ? string.Empty : extension)}";
                }

                using (Stream stream = _fileManager.OpenStream(filePath, FileMode.Create))
                {
                    _defaultCodec.Save(sprite, stream);
                }

                IContentFile file = _fileManager.GetFile(filePath);

                Debug.Assert(file != null, $"Sprite file '{filePath}' was not created!");

                file.LinkContent(textureFile);
            }
        }
        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="SpriteExtractorService"/> class.</summary>
        /// <param name="renderer">The application 2D renderer.</param>
        /// <param name="fileManager">The file manager for the project files.</param>
        /// <param name="defaultCodec">The default sprite codec.</param>
        public SpriteExtractorService(Gorgon2D renderer, IContentFileManager fileManager, IGorgonSpriteCodec defaultCodec)
        {
            _renderer = renderer;
            _graphics = renderer.Graphics;
            _fileManager = fileManager;
            _defaultCodec = defaultCodec;
        }
        #endregion
    }
}
