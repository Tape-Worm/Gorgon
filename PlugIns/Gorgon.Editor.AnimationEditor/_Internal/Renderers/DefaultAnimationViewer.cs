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
using System.Numerics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;
using DX = SharpDX;

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
        /// <summary>Function called when the camera is panned.</summary>
        /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
        protected override void OnCameraMoved() => Clipper?.Refresh();

        /// <summary>Function called when the camera is zoomed.</summary>
        /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
        protected override void OnCameraZoomed() => Clipper?.Refresh();

        /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext" /> has been changed.</summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IAnimationContent.WorkingSprite):
                case nameof(IAnimationContent.PrimarySprite):
                    if (Clipper is not null)
                    {
                        Clipper.Rectangle = Renderer.MeasureSprite(Sprite).Truncate();
                        Clipper.Refresh();
                    }
                    break;
                case nameof(IAnimationContent.BackgroundImage):
                    if (Clipper is not null)
                    {
                        Clipper.Bounds = RenderRegion;
                    }
                    break;
            }
        }

        /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
        /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad" /> method. Failure to do so can cause memory leakage.</remarks>
        protected override void OnUnload()
        {
            if (Clipper is not null)
            {
                Clipper.RectChanged -= Clipper_RectChanged;
            }
        }

        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        /// <remarks>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
        /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
        /// </remarks>
        protected override void OnLoad()
        {
            base.OnLoad();

            _font = Fonts.GetFont(new GorgonFontInfo("Segoe UI", 18.0f, FontHeightMode.Points)
            {
                Name = "Segoe UI 18 pt",
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

            if (Clipper is not null)
            {
                Clipper.Camera = Camera;
                Clipper.AllowResize = false;
                Clipper.AllowManualInput = false;
                Clipper.AllowMove = true;
                Clipper.ClipAgainstBoundaries = false;
                Clipper.Bounds = RenderRegion;
                Clipper.Rectangle = Renderer.MeasureSprite(Sprite).Truncate();
                Clipper.RectChanged += Clipper_RectChanged;
            }
        }

        /// <summary>Handles the RectChanged event of the Clipper control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs">EventArgs</see> instance containing the event data.</param>
        private void Clipper_RectChanged(object sender, System.EventArgs e)
        {
            Vector2 offset = new Vector2(Sprite.Anchor.X * Clipper.Rectangle.Width, Sprite.Anchor.Y * Clipper.Rectangle.Height).Truncate();
            DataContext.PrimarySpriteStartPosition = new Vector2(Clipper.Rectangle.Left + offset.X, Clipper.Rectangle.Top + offset.Y);
        }

        /// <summary>Function called when the view has been resized.</summary>
        /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
        protected override void OnResizeEnd()
        {
            _instructions.LayoutArea = new DX.Size2F(ClientSize.Width, ClientSize.Height);

            _instructions.Text = Resources.GORANM_TEXT_TEXTURE_KEY_ASSIGN.WordWrap(_font, ClientSize.Width);
            _textSize = _instructions.Text.MeasureText(_font, true, wordWrapWidth: ClientSize.Width);

            Clipper?.Refresh();

            base.OnResizeEnd();
        }

        /// <summary>Function to handle a mouse move event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
        protected override void OnMouseMove(MouseArgs args)
        {
            Clipper?.MouseMove(args);
            base.OnMouseMove(args);
        }

        /// <summary>Function to handle a mouse down event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
        protected override void OnMouseDown(MouseArgs args)
        {
            Clipper?.MouseDown(args);
            base.OnMouseDown(args);
        }

        /// <summary>Function to handle a mouse up event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
        protected override void OnMouseUp(MouseArgs args) 
        {
            Clipper?.MouseUp(args);
            base.OnMouseUp(args);
        }

        /// <summary>Function to draw any gizmos for UI components.</summary>
        protected override void DrawGizmos()
        {
            base.DrawGizmos();

            if ((DataContext.CommandContext != DataContext.KeyEditor) || (SelectedTrackID != TrackSpriteProperty.Texture))
            {
                
                Renderer.Begin();
                Clipper.Render();
                Renderer.End();

                return;
            }

            _instructions.Position = new Vector2(0, 0);

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
        /// <param name="clipper">The service used to clip rectangular areas of an image.</param>
        public DefaultAnimationViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext, IRectClipperService clipper)
            : base(ViewerName, renderer, swapChain, dataContext, clipper, false)
        {            
        }        
        #endregion
    }
}
