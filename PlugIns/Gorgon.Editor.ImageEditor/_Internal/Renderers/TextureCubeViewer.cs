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
// Created: January 9, 2019 1:43:36 PM
// 
#endregion

using System.Threading;
using System.Windows.Forms;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A viewer for a 2D cube texture.
    /// </summary>
    internal class TextureCubeViewer
        : TextureViewer
    {
        #region Variables.
        // The texture to display.
        private GorgonTexture2D _texture;
        // The view for the texture.
        private GorgonTexture2DView _textureView;
        // Bounds for each cube image.
        private readonly DX.RectangleF[] _cubeImageBounds = new DX.RectangleF[6];
        private readonly DX.RectangleF[] _cubeScreenBounds = new DX.RectangleF[6];
        // The font used to print each axis.
        private GorgonFont _axisFont;
        // The selection rectangle.
        private IMarchingAnts _selectionRect;
        // The font factory used to generate the font for the glyphs.
        private readonly GorgonFontFactory _fontFactory;
        #endregion

        #region Methods.
        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GorgonFont font = Interlocked.Exchange(ref _axisFont, null);
                IMarchingAnts ants = Interlocked.Exchange(ref _selectionRect, null);

                ants?.Dispose();
                font?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>Function to destroy the texture when done with it.</summary>
        protected override void DestroyTexture()
        {
            _texture?.Dispose();
            _texture = null;
            _textureView = null;
        }

        /// <summary>Function to create the texture for display.</summary>
        protected override void CreateTexture()
        {
            if ((DataContext?.ImageData == null) || (DataContext.ImageType != ImageType.ImageCube))
            {
                RenderRegion = DX.RectangleF.Empty;
                return;
            }

            RenderRegion = new DX.RectangleF(0, 0, DataContext.ImageData.Width * 4, DataContext.ImageData.Height * 3);

            _texture = DataContext.ImageData.ToTexture2D(Graphics, new GorgonTexture2DLoadOptions
            {
                Name = DataContext.File.Name,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                IsTextureCube = false
            });

            ShaderView = _textureView = _texture.GetShaderResourceView();
            TextureUpdated();
        }

        /// <summary>Function to handle a mouse up event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
        protected override void OnMouseUp(MouseArgs args)
        {
            base.OnMouseUp(args);

            if (args.MouseButtons != MouseButtons.Left)
            {
                return;
            }

            int cubeGroup = (DataContext.CurrentArrayIndex / 6) * 6;

            for (int i = 0; i < _cubeImageBounds.Length; ++i)
            {                
                if (_cubeImageBounds[i].Contains(args.CameraSpacePosition))
                {
                    DataContext.CurrentArrayIndex = cubeGroup + i;
                    return;
                }
            }

            args.Handled = true;
        }

        /// <summary>Function to draw the texture.</summary>
        protected override void DrawTexture()
        {
            var tileSize = new DX.Vector2(DataContext.Width, DataContext.Height);
            var bounds = new DX.RectangleF(-tileSize.X, -tileSize.Y * 0.5f, tileSize.X, tileSize.Y);
            DX.Vector3 tileClient = Camera.Unproject((DX.Vector3)bounds.Location);
            var clientBounds = new DX.RectangleF(tileClient.X, tileClient.Y, tileSize.X * Camera.Zoom.X, tileSize.Y * Camera.Zoom.X);
            int cubeGroup = DataContext.CurrentArrayIndex / 6;

            _cubeImageBounds[0] = new DX.RectangleF(bounds.Left + bounds.Width, bounds.Top, bounds.Width, bounds.Height);
            _cubeImageBounds[1] = new DX.RectangleF(bounds.Left - bounds.Width, bounds.Top, bounds.Width, bounds.Height);
            _cubeImageBounds[2] = new DX.RectangleF(bounds.Left, bounds.Top - bounds.Height, bounds.Width, bounds.Height);
            _cubeImageBounds[3] = new DX.RectangleF(bounds.Left, bounds.Top + bounds.Height, bounds.Width, bounds.Height);
            _cubeImageBounds[4] = bounds;
            _cubeImageBounds[5] = new DX.RectangleF(bounds.Left + (bounds.Width * 2), bounds.Top, bounds.Width, bounds.Height);

            _cubeScreenBounds[0] = new DX.RectangleF(clientBounds.Left + clientBounds.Width, clientBounds.Top, clientBounds.Width, clientBounds.Height);
            _cubeScreenBounds[1] = new DX.RectangleF(clientBounds.Left - clientBounds.Width, clientBounds.Top, clientBounds.Width, clientBounds.Height);
            _cubeScreenBounds[2] = new DX.RectangleF(clientBounds.Left, clientBounds.Top - clientBounds.Height, clientBounds.Width, clientBounds.Height);
            _cubeScreenBounds[3] = new DX.RectangleF(clientBounds.Left, clientBounds.Top + clientBounds.Height, clientBounds.Width, clientBounds.Height);
            _cubeScreenBounds[4] = clientBounds;
            _cubeScreenBounds[5] = new DX.RectangleF(clientBounds.Left + (clientBounds.Width * 2), clientBounds.Top, clientBounds.Width, clientBounds.Height);

            int selectedImage = DataContext.CurrentArrayIndex % 6;

            Renderer.Begin(BatchState, Camera);

            for (int i = 0; i < 6; ++i)
            {
                Renderer.DrawFilledRectangle(_cubeImageBounds[i],
                    new GorgonColor(GorgonColor.White, Opacity),
                    _textureView,
                    new DX.RectangleF(0, 0, 1, 1),
                    (cubeGroup * 6) + i,
                    textureSampler: GorgonSamplerState.PointFiltering);
            }

            Renderer.End();

            Renderer.Begin(BatchState);

            for (int i = 0; i < _cubeScreenBounds.Length; ++i)
            {
                Renderer.DrawRectangle(_cubeScreenBounds[i], new GorgonColor(GorgonColor.Black, Opacity * 0.88f), 1);
            }
                        
            _selectionRect.Draw(_cubeScreenBounds[selectedImage]);

            var offset = new DX.Vector2(0, _axisFont.MeasureLine("+X", true).Height);

            Renderer.DrawString("+X", _cubeScreenBounds[0].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));
            Renderer.DrawString("-X", _cubeScreenBounds[1].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));
            Renderer.DrawString("+Y", _cubeScreenBounds[2].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));
            Renderer.DrawString("-Y", _cubeScreenBounds[3].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));
            Renderer.DrawString("+Z", _cubeScreenBounds[4].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));
            Renderer.DrawString("-Z", _cubeScreenBounds[5].BottomLeft - offset, _axisFont, new GorgonColor(GorgonColor.White, Opacity));

            Renderer.End();
        }

        /// <summary>Function called during resource creation.</summary>
        protected override void OnCreateResources()
        {
            _axisFont = _fontFactory.GetFont(new GorgonFontInfo("Segoe UI", 12, FontHeightMode.Points, "Segoe UI Bold 12pt - Axis Font")
            {
                OutlineColor1 = GorgonColor.Black,
                OutlineColor2 = GorgonColor.Black,
                OutlineSize = 3,
                FontStyle = FontStyle.Bold
            });

            _selectionRect = new MarchingAnts(Renderer);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="TextureCubeViewer"/> class.</summary>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="fontFactory">The font factory used to generate the font for the glyphs.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>
        public TextureCubeViewer(Gorgon2D renderer, GorgonSwapChain swapChain, GorgonFontFactory fontFactory, IImageContent dataContext)
            : base(ImageType.ImageCube.ToString(), "Gorgon2DTextureArrayView", 0, renderer, swapChain, dataContext) => _fontFactory = fontFactory;
        #endregion
    }
}
