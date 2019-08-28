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
// Created: May 8, 2019 8:40:05 PM
// 
#endregion

using System.Linq;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.TextureAtlasTool
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
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public ITextureAtlas DataContext
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
        private void InitializeFromDataContext(ITextureAtlas dataContext)
        {
            if (dataContext?.Atlas == null)
            {
                _textureSprite = new GorgonSprite();
                return;
            }

            _textureSprite = new GorgonSprite
            {
                Texture = dataContext.Atlas.Textures[0],
                TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                Size = new DX.Size2F(dataContext.Atlas.Textures[0].Width, dataContext.Atlas.Textures[0].Height),
                TextureSampler = GorgonSamplerState.PointFiltering
            };
        }

        /// <summary>
        /// Function to draw a message on the screen when no atlas is loaded.
        /// </summary>
        private void DrawMessage()
        {
            DX.Size2F textSize = _renderer.DefaultFont.MeasureText(Resources.GORTAG_TEXT_NO_ATLAS_MESSAGE, false);

            _renderer.Begin(camera: _camera);
            _renderer.DrawFilledRectangle(new DX.RectangleF(-_swapChain.Width * 0.5f, -_swapChain.Height * 0.5f, _swapChain.Width, _swapChain.Height),
                GorgonColor.White,
                _bgTexture,
                new DX.RectangleF(0, 0, (float)_swapChain.Width / _bgTexture.Width, (float)_swapChain.Height / _bgTexture.Height));

            _renderer.DrawFilledRectangle(new DX.RectangleF(-_swapChain.Width * 0.5f, -_swapChain.Height * 0.5f, _swapChain.Width, _swapChain.Height), new GorgonColor(GorgonColor.White, 0.75f));
            _renderer.DrawString(Resources.GORTAG_TEXT_NO_ATLAS_MESSAGE, new DX.Vector2((int)(-textSize.Width * 0.5f), (int)(-textSize.Height * 0.5f)), color: GorgonColor.Black);

            _renderer.End();

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

            if (DataContext.Atlas == null)
            {
                DrawMessage();
                return;
            }

            GorgonTexture2DView texture = _textureSprite.Texture = DataContext.Atlas.Textures[DataContext.PreviewTextureIndex]; // Change to use index.

            float scale = CalcZoomToSize(new DX.Size2F(texture.Width + 8, texture.Height + 8), new DX.Size2F(_swapChain.Width, _swapChain.Height));
            _textureSprite.Size = new DX.Size2F(scale * texture.Width, scale * texture.Height).Truncate();
            _textureSprite.Position = new DX.Vector2(-_textureSprite.Size.Width * 0.5f, -_textureSprite.Size.Height * 0.5f).Truncate();

            _renderer.Begin(camera: _camera);
            _renderer.DrawFilledRectangle(new DX.RectangleF(-_swapChain.Width * 0.5f, -_swapChain.Height * 0.5f, _swapChain.Width, _swapChain.Height),
                GorgonColor.White,
                _bgTexture,
                new DX.RectangleF(0, 0, (float)_swapChain.Width / _bgTexture.Width, (float)_swapChain.Height / _bgTexture.Height));

            _textureSprite.TextureArrayIndex = DataContext.PreviewArrayIndex; // Change to use index.
            _renderer.DrawSprite(_textureSprite);

            _renderer.End();
        }

        /// <summary>Function to perform setup on the renderer.</summary>
        public void Setup()
        {
            _bgTexture = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo("Texture Atlas BG Texture")
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
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ITextureAtlas dataContext)
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
        }
        #endregion
    }
}
