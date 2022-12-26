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
// Created: April 4, 2018 9:39:05 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// A gallery for displaying the example images.
/// </summary>
class ImageGallery
    : IDisposable
{
    #region Variables.
    // The content font with bolding.
    private readonly Font _boldContentFont;
    // Our image list.
    private readonly IGorgonImage[] _images = new IGorgonImage[6];
    // Descriptions for each image.
    private readonly string[] _descriptions = {
                                                  "User Drawn Image",
                                                  "Direct Draw Surface (DDS) Image",
                                                  "16bpp TGA Image",
                                                  "Animated GIF",
                                                  "PNG Image (w/alpha)",
                                                  "BMP Image"
                                              };
    // The size of the image.
    private Size _imageSize;
    // The destination bitmap to draw on the screen.
    private Bitmap _destBitmap;
    // The device DPI.
    private readonly int _dpi;
    // The image frames for the animated gif.
    private Bitmap _gifBuffer;
    // Graphics context for the gif buffer.
    private System.Drawing.Graphics _gifGraphics;
    // GIF animator.
    private readonly GifAnimator _gifAnim;
    // GIF position.
    private Point _gifPosition;
    // Counter for drawing frames.
    private int _isDrawing;
    #endregion

    #region Properties.

    #endregion

    #region Methods.
    /// <summary>
    /// Function to provide a quick and dirty method of drawing outlined text.
    /// </summary>
    /// <param name="graphics">The graphics context that will receive the text.</param>
    /// <param name="x">The horizontal position of the text.</param>
    /// <param name="y">The vertical position of the text.</param>
    /// <param name="text">The text to render.</param>
    private void DrawOutlinedText(System.Drawing.Graphics graphics, int x, int y, string text)
    {
        for (int i = -2; i <= 2; ++i)
        {
            graphics.DrawString(text, _boldContentFont, Brushes.Black, x + i, y);
            graphics.DrawString(text, _boldContentFont, Brushes.Black, x, y + i);
            graphics.DrawString(text, _boldContentFont, Brushes.Black, x + i, y + i);
            graphics.DrawString(text, _boldContentFont, Brushes.Black, x - i, y + i);
        }

        graphics.DrawString(text, _boldContentFont, Brushes.White, x, y);
    }

    /// <summary>
    /// Function to draw some pretty pixels into a custom image for the example.
    /// </summary>
    private void CreateCustomImage()
    {
        // Create the image at the original size.
        var sourceImage = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
        {
            Width = 320,
            Height = 240
        });

        // Draw something pretty...
        float width = sourceImage.Width;
        float height = sourceImage.Height;
        float aspect = width / height;
        for (int x = 0; x < sourceImage.Width; ++x)
        {
            float rColorFade = ((x / width) * aspect).Cos();

            for (int y = 0; y < sourceImage.Height; ++y)
            {
                float bColorFade = (y / height).Sin();

                int pixelStride = sourceImage.Buffers[0].PitchInformation.RowPitch / sourceImage.Buffers[0].Width;
                // This is the position inside of the buffer, in bytes.
                int position = (y * sourceImage.Buffers[0].PitchInformation.RowPitch) + (x * pixelStride);
                var color = new GorgonColor(rColorFade, 1.0f - rColorFade, bColorFade, 1.0f);

                // Notice we're using AsRef here.  This allows us to read a value as another type.  In this case, we've chosen an 
                // int32 value to represent an ARGB pixel. Because this value is a reference to the location in memory, we can assign 
                // a new value to it. 
                //
                // Do note that the position is a byte address, and not an int address (i.e. position = 1 is 1 byte, not 1 int).
                sourceImage.Buffers[0].Data.AsRef<int>(position) = color.ToABGR();
            }
        }

        // If the DPI is standard, then do nothing else, otherwise scale up to the new size.
        if (_dpi == 96)
        {
            _images[0] = sourceImage;
            return;
        }

        // The methods to alter an image are built to be fluent, so you can chain calls together like:
        // newImage = image.Reize(...).Crop(...).ConvertFormat(...).GenerateMipMaps(...)
        //
        // In this case, we only need one operation, and that's a resize for the image to the new DPI.
        _images[0] = sourceImage.BeginUpdate()
                                .Resize(_imageSize.Width, _imageSize.Height, 1, ImageFilter.Fant)
                                .EndUpdate();
    }

    /// <summary>
    /// Function to load the animated GIF.
    /// </summary>
    /// <param name="path">The path to the file to load.</param>
    private void LoadAnimatedGif(string path)
    {
        // Note here that we're passing in codec specific options into the codec.
        // In this case, we're telling the codec to read all the frames from the animated GIF.
        var gifCodec = new GorgonCodecGif(decodingOptions: new GorgonGifDecodingOptions
        {
            ReadAllFrames = true
        });
        _images[3] = gifCodec.FromFile(path);

        // The GIF animation frames are stored in an image array, so in order to access them we need to index through 
        // the image array below.
        _gifBuffer = new Bitmap(_imageSize.Width, _imageSize.Height, PixelFormat.Format32bppArgb);
        _gifGraphics = System.Drawing.Graphics.FromImage(_gifBuffer);

        float imageAspect = _images[3].Height / (float)_images[3].Width;
        var newSize = new Size(_imageSize.Width, (int)(_imageSize.Width * imageAspect));

        // With this one, the source image isn't in the same aspect ratio as our thumbnail size.
        // So we need to expand the image boundaries to fit.  The expand method will do just that.
        _images[3] = _images[3].BeginUpdate()
                               .Resize(newSize.Width, newSize.Height, 1)
                               .Expand(_imageSize.Width, _imageSize.Height, 1)
                               .EndUpdate();

        // Pass in the frame delays to the animation interface. 
        // This is we can control the rate of animation frame change.
        _gifAnim.FrameTimes = gifCodec.GetFrameDelays(path);
    }

    /// <summary>
    /// Function to refresh the GIF animation.
    /// </summary>
    /// <param name="graphics">The graphics context that will receive the image data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    public void RefreshGif(System.Drawing.Graphics graphics)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (graphics.CompositingMode != CompositingMode.SourceOver)
        {
            graphics.CompositingMode = CompositingMode.SourceOver;
        }

        try
        {
            if (Interlocked.Increment(ref _isDrawing) > 1)
            {
                return;
            }

            _images[3].Buffers[0, _gifAnim.CurrentFrame].CopyTo(_gifBuffer);
            DrawOutlinedText(_gifGraphics, 0, 0, _descriptions[3]);
            graphics.DrawImage(_gifBuffer, _gifPosition.X, _gifPosition.Y);
        }
        finally
        {
            Interlocked.Decrement(ref _isDrawing);
        }
    }

    /// <summary>
    /// Function to load image data into memory for the gallery.
    /// </summary>
    /// <param name="path">The path to the directory where image files are loaded from.</param>
    public void LoadImages(string path)
    {
        // This will create a new image, and draw custom data to that image.
        CreateCustomImage();

        var imageDirectory = new DirectoryInfo(Path.Combine(path, "Images"));
        var ddsFileInfo = new FileInfo(Path.Combine(imageDirectory.FullName, "colleen.dds"));
        var tgaFileInfo = new FileInfo(Path.Combine(imageDirectory.FullName, "colleen2.tga"));
        var gifFileInfo = new FileInfo(Path.Combine(imageDirectory.FullName, "rain.gif"));
        var pngFileInfo = new FileInfo(Path.Combine(imageDirectory.FullName, "skull.png"));
        var bmpFileInfo = new FileInfo(Path.Combine(imageDirectory.FullName, "nebula1.bmp"));

        // Load each image and resize to the requested width & height.
        // Here we see how to use an image codec to read image data from a file.
        IGorgonImageCodec codec = new GorgonCodecDds();
        _images[1] = codec.FromFile(ddsFileInfo.FullName)
                          .BeginUpdate()
                          .Resize(_imageSize.Width, _imageSize.Height, filter: ImageFilter.Fant)
                          .EndUpdate();

        codec = new GorgonCodecTga();
        _images[2] = codec.FromFile(tgaFileInfo.FullName)
                          .BeginUpdate()
                          .Resize(_imageSize.Width, _imageSize.Height)
                          .ConvertToFormat(BufferFormat.B8G8R8A8_UNorm)
                          .EndUpdate();

        // Because the GIF is animated, we need to load it in a special way.
        LoadAnimatedGif(gifFileInfo.FullName);

        codec = new GorgonCodecPng();
        _images[4] = codec.FromFile(pngFileInfo.FullName)
                          .BeginUpdate()
                          .Resize(_imageSize.Width, _imageSize.Height)
                          .EndUpdate();

        codec = new GorgonCodecBmp();
        _images[5] = codec.FromFile(bmpFileInfo.FullName)
                          .BeginUpdate()
                          .Resize(_imageSize.Width, _imageSize.Height, 1, ImageFilter.Linear)
                          .EndUpdate();

        // This is going to be used to display the images on the screen.
        // We use GDI+ here simply because it's there.  
        _destBitmap = new Bitmap(_imageSize.Width, _imageSize.Height, PixelFormat.Format32bppArgb);
    }

    /// <summary>
    /// Function to draw the images to the screen.
    /// </summary>
    /// <param name="graphics">The graphics interface that will receive our images.</param>
    /// <param name="clientSize">The size of the area to draw into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    public void DrawGallery(System.Drawing.Graphics graphics, Size clientSize)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (graphics.CompositingMode != CompositingMode.SourceOver)
        {
            graphics.CompositingMode = CompositingMode.SourceOver;
        }

        try
        {
            Interlocked.Increment(ref _isDrawing);

            // Our position for the current image.
            var position = new Point(0, 0);

            for (int i = 0; i < _images.Length; ++i)
            {
                IGorgonImage image = _images[i];

                // Make a special case for the GIF file.
                if (i == 3)
                {
                    _gifPosition = position;
                    RefreshGif(graphics);
                }
                else
                {
                    // We can transfer our image data into an appropriately formatted GDI+ image like so:
                    image.Buffers[0].CopyTo(_destBitmap);

                    graphics.DrawImage(_destBitmap, position);
                    DrawOutlinedText(graphics, position.X, position.Y, _descriptions[i]);
                }

                position = new Point(position.X + image.Width, position.Y);

                if ((position.X + _imageSize.Width) >= clientSize.Width)
                {
                    position = new Point(0, position.Y + image.Height);
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _isDrawing);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // You must call Dispose for the images because each of them have native memory backing them and the GC 
        // cannot reclaim this for you.
        foreach (IGorgonImage image in _images)
        {
            image?.Dispose();
        }

        _gifGraphics.Dispose();
        _boldContentFont.Dispose();
        _gifBuffer.Dispose();
        _destBitmap?.Dispose();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGallery"/> class.
    /// </summary>
    /// <param name="baseFont">The base font.</param>
    /// <param name="deviceDpi">The DPI for the device.</param>
    /// <param name="animator">The animator used to animate a GIF file.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseFont"/>, or the <paramref name="animator"/> parameter is <b>null</b>.</exception>
    public ImageGallery(Font baseFont, int deviceDpi, GifAnimator animator)
    {
        if (baseFont is null)
        {
            throw new ArgumentNullException(nameof(baseFont));
        }

        _gifAnim = animator ?? throw new ArgumentNullException(nameof(animator));
        _boldContentFont = new Font(baseFont, FontStyle.Bold);
        _dpi = 96.Max(deviceDpi);
        float newScaleWidth = deviceDpi / 96.0f;
        var dpiScale = new SizeF(newScaleWidth, newScaleWidth);
        _imageSize = new Size((int)(dpiScale.Width * 320), (int)(dpiScale.Height * 240));
    }
    #endregion
}
