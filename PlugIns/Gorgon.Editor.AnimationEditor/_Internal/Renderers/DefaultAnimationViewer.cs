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
// Created: June 10, 2020 6:38:49 AM
// 
#endregion

using System.Linq;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Editor.AnimationEditor.Properties;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The default viewer for an aniamtion.
    /// </summary>
    internal class DefaultAnimationViewer
        : AnimationViewer
    {
        #region Constants.
        /// <summary>
        /// The name of the viewer.
        /// </summary>
        public const string ViewerName = "AnimationDefaultRenderer";
        #endregion

        #region Variables.
        // The font used to rendering instructional text.
        private GorgonFont _font;
        // The set key frame instructions.
        private GorgonTextSprite _instructions;
        // The size of the text sprite.
        private DX.Size2F _textSize;
        #endregion

        #region Methods.
        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        /// <remarks>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
        /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
        /// </remarks>
        protected override void OnLoad()
        {
            base.OnLoad();

            _font = Fonts.GetFont(new GorgonFontInfo("Segoe UI", 18.0f, FontHeightMode.Points, "Segoe UI 18 pt")
            {
                FontStyle = FontStyle.Bold,
                Characters = Resources.GORANM_TEXT_TEXTURE_KEY_ASSIGN.Distinct(),
                OutlineColor1 = GorgonColor.Black,
                OutlineColor2 = GorgonColor.Black,
                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                OutlineSize = 3
            });

            _instructions = new GorgonTextSprite(_font)
            {
                Alignment = Gorgon.UI.Alignment.LowerCenter,
                DrawMode = TextDrawMode.OutlinedGlyphs,
                LayoutArea = new DX.Size2F(ClientSize.Width, ClientSize.Height),                
            };

            _instructions.Text = Resources.GORANM_TEXT_TEXTURE_KEY_ASSIGN.WordWrap(_font, ClientSize.Width);
            _textSize = _instructions.Text.MeasureText(_font, true, wordWrapWidth: ClientSize.Width);
        }

        /// <summary>Function called when the view has been resized.</summary>
        /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
        protected override void OnResizeEnd()
        {
            _instructions.LayoutArea = new DX.Size2F(ClientSize.Width, ClientSize.Height);

            _instructions.Text = Resources.GORANM_TEXT_TEXTURE_KEY_ASSIGN.WordWrap(_font, ClientSize.Width);
            _textSize = _instructions.Text.MeasureText(_font, true, wordWrapWidth: ClientSize.Width);

            base.OnResizeEnd();
        }

        /// <summary>Function to draw any gizmos for UI components.</summary>
        protected override void DrawGizmos()
        {
            base.DrawGizmos();

            if ((DataContext.CommandContext != DataContext.KeyEditor) || (SelectedTrackID != TrackSpriteProperty.Texture))
            {
                return;
            }

            _instructions.Position = new DX.Vector2(0, 0);

            Renderer.Begin();
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, ClientSize.Height - _textSize.Height, ClientSize.Width, _textSize.Height), new GorgonColor(GorgonColor.Black, 0.65f));
            Renderer.DrawTextSprite(_instructions);
            Renderer.End();
        }

        /// <summary>Function to draw the animation.</summary>
        protected override void DrawAnimation()
        {
            Renderer.Begin();                       
            Renderer.DrawSprite(Sprite);
            Renderer.End();

            if ((DataContext?.UpdateAnimationPreviewCommand is null) || (!DataContext.UpdateAnimationPreviewCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UpdateAnimationPreviewCommand.Execute(null);
        }
        #endregion

        #region Methods.
        /// <summary>Function to set the default zoom/offset for the viewer.</summary>
        public override void DefaultZoom()
        {
            if (Sprite is null)
            {
                return;
            }

            ZoomToSprite(Sprite);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="DefaultAnimationViewer"/> class.</summary>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>        
        public DefaultAnimationViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext)
            : base(ViewerName, renderer, swapChain, dataContext, false)
        {            
        }        
        #endregion
    }
}
