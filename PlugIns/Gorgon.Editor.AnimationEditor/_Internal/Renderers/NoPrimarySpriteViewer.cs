#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 8, 2020 7:27:57 PM
// 
#endregion

using System.Linq;
using System.IO;
using DX = SharpDX;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Timing;
using Gorgon.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.UI;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A renderer to display on the UI when the animation does not have a primary sprite.
    /// </summary>
    internal class NoPrimarySpriteViewer
        : DefaultContentRenderer<IAnimationContent>
    {
        #region Constants.
        /// <summary>
        /// The name of the viewer.
        /// </summary>
        public const string ViewerName = "AnimationNoSpriteRenderer";
        #endregion

        #region Variables.
        // The texture to display when an animation does not contain a primary sprite.
        private GorgonTexture2DView _noSprite;
        // The horizontal position of the film strip background.
        private float _stripX;
        // The render target for our background.
        private GorgonRenderTarget2DView _rtv;
        // The texture for our background.
        private GorgonTexture2DView _srv;
        // The effect used to simulate old film.
        private Gorgon2DOldFilmEffect _oldFilm;
        // The factory used to generate fonts.
        private readonly GorgonFontFactory _fontFactory;
        // The font to render the text.
        private GorgonFont _font;
        // The text to display.
        private GorgonTextSprite _displayText;
        // The number of loops for the background animation.
        private int _stripAnimCount = 0;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to destroy the render target background.
        /// </summary>
        private void DestroyTarget()
        {
            _srv?.Dispose();
            _rtv?.Dispose();

            _srv = null;
            _rtv = null;
        }

        /// <summary>
        /// Function to create the background render target.
        /// </summary>
        private void CreateRenderTarget()
        {
            DestroyTarget();

            _rtv = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo("BG_Rtv")
            {
                Width = MainRenderTarget.Width,
                Height = MainRenderTarget.Height,
                Format = MainRenderTarget.Format,
                Usage = ResourceUsage.Default,
                Binding = TextureBinding.ShaderResource
            });

            _srv = _rtv.GetShaderResourceView();
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DestroyTarget();
                _noSprite?.Dispose();                
                _oldFilm?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>Function called when the view has been resized.</summary>
        /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
        protected override void OnResizeEnd()
        {
            base.OnResizeEnd();

            _displayText.LayoutArea = RenderRegion.Size;
            _displayText.Text = _font.WordWrap(Resources.GORANM_TEXT_NO_SPRITE, RenderRegion.Width);
            CreateRenderTarget();
        }

        /// <summary>Function to render the background.</summary>
        /// <remarks>Developers can override this method to render a custom background.</remarks>
        protected sealed override void OnRenderBackground()
        {
            if (DataContext.Settings.AnimateNoPrimarySpriteBackground)
            {
                _oldFilm.DirtRegion = null;
                _oldFilm.Time = GorgonTiming.SecondsSinceStart;
                _oldFilm.ShakeOffset = new DX.Vector2(GorgonRandom.RandomSingle(-2.0f, 2.0f), GorgonRandom.RandomSingle(-1.5f, 1.5f));

                if (_stripAnimCount < 30)
                {
                    _stripX -= 1.0f * GorgonTiming.Delta;

                    if (_stripX < -1.0f)
                    {
                        _stripX += 1.0f;
                        _stripAnimCount++;
                    }
                }
                else
                {
                    _stripX = 0.0f;
                }
            }
            else
            {
                _oldFilm.DirtRegion = DX.RectangleF.Empty;
                _stripX = 0;
            }

            Graphics.SetRenderTarget(_rtv);
                        
            Renderer.Begin();
            Renderer.DrawFilledRectangle(_rtv.Bounds.ToRectangleF(), GorgonColor.White, _noSprite, new DX.RectangleF(_stripX, 0, 1, 1));
            Renderer.DrawTextSprite(_displayText);
            Renderer.End();

            Graphics.SetRenderTarget(MainRenderTarget);

            _oldFilm.Render(_srv, MainRenderTarget);
        }

        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        /// <remarks>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
        /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
        /// </remarks>
        protected override void OnLoad()
        {
            base.OnLoad();

            _stripAnimCount = 0;
            _oldFilm.Time = 0;

            CreateRenderTarget();
        }

        /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
        /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.</remarks>
        protected override void OnUnload()
        {
            DestroyTarget();

            base.OnUnload();
        }

        /// <summary>Function called during resource creation.</summary>
        public void CreateResources()
        {
            using (var stream = new MemoryStream(Resources.filmstripbg))
            {
                _noSprite = GorgonTexture2DView.FromStream(Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "Animation Editor - No primary sprite texture",
                    Usage = ResourceUsage.Immutable
                });
            }

            _font = _fontFactory.GetFont(new GorgonFontInfo("Century", 64.0f, FontHeightMode.Points, "No Preview Sprite Font")
            {
                OutlineSize = 4,
                OutlineColor1 = GorgonColor.Black,
                OutlineColor2 = GorgonColor.Black,
                TextureHeight = 512,
                TextureWidth = 512,
                Characters = Resources.GORANM_TEXT_NO_SPRITE.Distinct(),
                DefaultCharacter = ' '
            });

            _oldFilm = new Gorgon2DOldFilmEffect(Renderer, 256)
            {
                ScrollSpeed = 0.05f
            };
            _oldFilm.Precache();

            _displayText = new GorgonTextSprite(_font, _font.WordWrap(Resources.GORANM_TEXT_NO_SPRITE, RenderRegion.Width))
            {
                Alignment = Alignment.Center,
                Color = GorgonColor.White,                
                LayoutArea = RenderRegion.Size,
                DrawMode = TextDrawMode.OutlinedGlyphs
            };
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="NoPrimarySpriteViewer"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="fontFactory">The factory used to generate fonts.</param>
        /// <param name="animation">The sprite used for rendering.</param>
        public NoPrimarySpriteViewer(Gorgon2D renderer, GorgonSwapChain swapChain, GorgonFontFactory fontFactory, IAnimationContent animation)
            : base(ViewerName, renderer, swapChain, animation)
        {
            CanPanHorizontally = false;
            CanPanHorizontally = false;
            CanZoom = false;

            _fontFactory = fontFactory;
        }
        #endregion
    }
}
