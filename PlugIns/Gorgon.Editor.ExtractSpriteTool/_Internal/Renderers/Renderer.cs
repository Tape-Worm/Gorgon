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
// Created: April 24, 2019 10:14:49 PM
// 
#endregion

using System.Linq;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// The renderer used to draw the texture and sprites.
    /// </summary>
    internal class Renderer
        : IRenderer
    {
        #region Variables.
        // The graphics interface for the application.
        private readonly GorgonGraphics _graphics;
        // The 2D renderer for the application.
        private readonly Gorgon2D _renderer;
        // The swap chain used to render onto the window.
        private readonly GorgonSwapChain _swapChain;
        // Background texture.
        private GorgonTexture2DView _bgTexture;
        // The camera for viewing the scene.
        private IGorgon2DCamera _camera;
        // The sprite used to display the texture.
        private GorgonSprite _textureSprite;
        // Inverted rendering.
        private Gorgon2DBatchState _inverted;
        // The sprite to display for preview.
        private readonly GorgonSprite _previewSprite = new GorgonSprite();
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IExtract DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the render data from the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IExtract dataContext)
        {
            if (dataContext == null)
            {
                _previewSprite.Texture = null;
                _textureSprite = new GorgonSprite();
                return;
            }

            _previewSprite.Texture = dataContext.Texture;
            _textureSprite = new GorgonSprite
            {
                Texture = dataContext.Texture,
                TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                Size = new DX.Size2F(dataContext.Texture.Width, dataContext.Texture.Height),
                TextureSampler = GorgonSamplerState.PointFiltering
            };
        }

        /// <summary>
        /// Function to update the preview sprite.
        /// </summary>
        /// <param name="scale">The current scale factor.</param>
        private void UpdatePreviewSprite(float scale)
        {
            GorgonSprite currentSprite = DataContext.Sprites[DataContext.CurrentPreviewSprite];
            _previewSprite.Size = new DX.Size2F(currentSprite.Size.Width * scale, currentSprite.Size.Height * scale);
            _previewSprite.TextureRegion = currentSprite.TextureRegion;
            _previewSprite.TextureArrayIndex = currentSprite.TextureArrayIndex;
        }

        /// <summary>
        /// Function to calculate scaling to the specified size, bounded by the client area of the rendering control.
        /// </summary>
        /// <param name="size">The size of the area to zoom into.</param>
        /// <param name="windowSize">The size of the window.</param>
        /// <returns>The scaling factor to apply.</returns>
        private float CalcZoomToSize(DX.Size2F size, DX.Size2F windowSize)
        {
            var scaling = new DX.Vector2(windowSize.Width / size.Width, windowSize.Height / size.Height);

            return scaling.X.Min(scaling.Y);
        }

        /// <summary>
        /// Function to render the sprite preview.
        /// </summary>
        private void DrawPreview()
        {
            GorgonSprite sprite = DataContext.Sprites[DataContext.CurrentPreviewSprite];
            float scale = CalcZoomToSize(new DX.Size2F(sprite.Size.Width + 8, sprite.Size.Height + 8), new DX.Size2F(_swapChain.Width, _swapChain.Height));
            UpdatePreviewSprite(scale);

            _renderer.Begin(camera: _camera);
            _renderer.DrawFilledRectangle(new DX.RectangleF(-_swapChain.Width * 0.5f, -_swapChain.Height * 0.5f, _swapChain.Width, _swapChain.Height),
                GorgonColor.White,
                _bgTexture,
                new DX.RectangleF(0, 0, (float)_swapChain.Width / _bgTexture.Width, (float)_swapChain.Height / _bgTexture.Height));
            _renderer.DrawSprite(_previewSprite);
            _renderer.End();
        }

        /// <summary>
        /// Function to draw the wait panel for generating sprites.
        /// </summary>
        private void DrawWaitingPanel()
        {
            ref readonly ProgressData prog = ref DataContext.ExtractTaskProgress;
            string text = string.Format(Resources.GOREST_PROGRESS_SPR_GEN, prog.Current, prog.Total);

            DX.Size2F textSize = _renderer.DefaultFont.MeasureText(text, false);
            var pos = new DX.Vector2(_swapChain.Width * 0.5f - textSize.Width * 0.5f, _swapChain.Height * 0.5f - textSize.Height * 0.5f);

            float percent = (float)prog.Current / prog.Total;
            var barOutline = new DX.RectangleF(pos.X, _swapChain.Height * 0.5f - (textSize.Height + 4) * 0.5f,
                                            textSize.Width + 4, textSize.Height + 8);
            var bar = new DX.RectangleF(barOutline.X + 1, barOutline.Y + 1, ((barOutline.Width - 2) * percent), barOutline.Height - 2);

            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _swapChain.Width, _swapChain.Height),
                                        new GorgonColor(GorgonColor.White, 0.80f));
            _renderer.DrawString(text, pos, color: GorgonColor.Black);
            _renderer.DrawRectangle(barOutline, GorgonColor.Black, 2);
            _renderer.End();

            _renderer.Begin(_inverted);
            _renderer.DrawFilledRectangle(bar, GorgonColor.White);
            _renderer.End();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => _bgTexture?.Dispose();

        /// <summary>Function to perform the rendering.</summary>
        public void Render()
        {
            if (DataContext == null)
            {
                return;
            }

            _graphics.SetRenderTarget(_swapChain.RenderTargetView);

            if (DataContext.InSpritePreview)
            {
                DrawPreview();
                return;
            }

            float scale = CalcZoomToSize(new DX.Size2F(DataContext.Texture.Width + 8, DataContext.Texture.Height + 8), new DX.Size2F(_swapChain.Width, _swapChain.Height));
            _textureSprite.Size = new DX.Size2F(scale * DataContext.Texture.Width, scale * DataContext.Texture.Height).Truncate();
            _textureSprite.Position = new DX.Vector2(-_textureSprite.Size.Width * 0.5f, -_textureSprite.Size.Height * 0.5f).Truncate();

            _renderer.Begin(camera: _camera);
            _renderer.DrawFilledRectangle(new DX.RectangleF(-_swapChain.Width * 0.5f, -_swapChain.Height * 0.5f, _swapChain.Width, _swapChain.Height),
                GorgonColor.White,
                _bgTexture,
                new DX.RectangleF(0, 0, (float)_swapChain.Width / _bgTexture.Width, (float)_swapChain.Height / _bgTexture.Height));

            _textureSprite.TextureArrayIndex = DataContext.CurrentPreviewArrayIndex;
            _renderer.DrawSprite(_textureSprite);

            _renderer.End();

            _renderer.Begin(_inverted, _camera);
            var pos = new DX.Vector2(_textureSprite.Position.X + (DataContext.GridOffset.X * scale), _textureSprite.Position.Y + (DataContext.GridOffset.Y * scale));

            int maxWidth = (int)((DataContext.CellSize.Width * DataContext.GridSize.Width) * scale);
            int maxHeight = (int)((DataContext.CellSize.Height * DataContext.GridSize.Height) * scale);

            _renderer.DrawRectangle(new DX.RectangleF(pos.X - 1, pos.Y - 1, maxWidth + 2, maxHeight + 2),
                                    GorgonColor.DeepPink);

            for (int x = 1; x < DataContext.GridSize.Width; ++x)
            {
                var start = new DX.Vector2((int)((x * DataContext.CellSize.Width) * scale) + pos.X, pos.Y);
                var end = new DX.Vector2((int)((x * DataContext.CellSize.Width) * scale) + pos.X, pos.Y + maxHeight);

                _renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColor.DeepPink);
            }

            for (int y = 1; y < DataContext.GridSize.Height; ++y)
            {
                var start = new DX.Vector2(pos.X, (int)((y * DataContext.CellSize.Height) * scale) + pos.Y);
                var end = new DX.Vector2(pos.X + maxWidth, (int)((y * DataContext.CellSize.Height) * scale) + pos.Y);

                _renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColor.DeepPink);
            }

            _renderer.End();

            if (DataContext.SpriteGenerationTask != null)
            {
                DrawWaitingPanel();
            }
        }

        /// <summary>Function to perform setup on the renderer.</summary>
        public void Setup()
        {
            _bgTexture = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo("Extract BG Texture")
            {
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                Width = EditorCommonResources.CheckerBoardPatternImage.Width,
                Height = EditorCommonResources.CheckerBoardPatternImage.Height
            }, EditorCommonResources.CheckerBoardPatternImage);

            _camera = new Gorgon2DOrthoCamera(_renderer, new DX.Size2F(_swapChain.Width, _swapChain.Height))
            {
                Anchor = new DX.Vector2(0.5f, 0.5f)
            };

            var builder = new Gorgon2DBatchStateBuilder();
            _inverted = builder.BlendState(GorgonBlendState.Inverted)
                                .Build();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IExtract dataContext)
        {
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
        /// <param name="context">The application graphics context.</param>
        /// <param name="swapChain">The swap chain bound to the window.</param>
        public Renderer(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            _graphics = context.Graphics;
            _renderer = context.Renderer2D;
            _swapChain = swapChain;
            _previewSprite.Anchor = new DX.Vector2(0.5f, 0.5f);
            _previewSprite.TextureSampler = GorgonSamplerState.PointFiltering;
        }
        #endregion
    }
}
