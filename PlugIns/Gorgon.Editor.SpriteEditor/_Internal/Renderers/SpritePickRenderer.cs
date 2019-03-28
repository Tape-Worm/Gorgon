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
// Created: April 1, 2019 11:51:35 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.SpriteEditor.Properties;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer to use with the sprite picker tool.
    /// </summary>
    internal class SpritePickRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // Marching ants rectangle.
        private readonly IMarchingAnts _marchAnts;
        // The rectangle clipping service.
        private readonly IPickClipperService _picker;
        #endregion

        #region Properties.
        /// <summary>Property to return some information to display about the sprite given then current renderer context.</summary>
        public override string SpriteInfo
        {
            get
            {
                var spriteTextureBounds = _picker.Rectangle.ToRectangle();
                return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, spriteTextureBounds.Left,
                                                                   spriteTextureBounds.Top,
                                                                   spriteTextureBounds.Right,
                                                                   spriteTextureBounds.Bottom,
                                                                   spriteTextureBounds.Width,
                                                                   spriteTextureBounds.Height);
            }
        }

        /// <summary>Property to return the currently active cursor.</summary>
        /// <value>The current cursor.</value>
        public override Cursor CurrentCursor => Cursors.Hand;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to submit the updated texture coordinates to the sprite.
        /// </summary>
        private void SubmitTextureCoordinates()
        {
            (DX.RectangleF rect, int arrayIndex) args = (_picker.Rectangle, TextureArrayIndex);

            if ((SpriteContent?.SetTextureCoordinatesCommand == null) || (!SpriteContent.SetTextureCoordinatesCommand.CanExecute(args)))
            {
                return;
            }

            SpriteContent.SetTextureCoordinatesCommand.Execute(args);
        }

        /// <summary>Handles the PreviewKeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Window_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SpriteContent.CurrentTool = SpriteEditTool.None;
                    e.IsInputKey = true;
                    break;
                case Keys.Escape:
                    // Revert the changes, when we switch tools, we'll be committing our changes to the data context, and if they haven't changed, nothing will be 
                    // updated.
                    _picker.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
                    TextureArrayIndex = SpriteContent.ArrayIndex;
                    SpriteContent.CurrentTool = SpriteEditTool.None;
                    e.IsInputKey = true;
                    break;
            }
        }

        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Window_MouseUp(object sender, MouseEventArgs e) => _picker.MouseUp(new DX.Vector2(e.X, e.Y), e.Button, Control.ModifierKeys);

        /// <summary>Function called when the sprite is changing a property.</summary>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        protected override void OnSpriteChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentTool):
                    if (SpriteContent.CurrentTool == SpriteEditTool.SpritePick)
                    {
                        SubmitTextureCoordinates();
                    }
                    break;
            }
        }

        /// <summary>Function called when the sprite has a property change.</summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.ImageData):
                    _picker.ImageData = SpriteContent.ImageData;
                    break;
                case nameof(ISpriteContent.TextureCoordinates):
                case nameof(ISpriteContent.Texture):
                    if (SpriteContent.Texture != null)
                    {
                        _picker.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
                    }
                    else
                    {
                        _picker.Rectangle = DX.RectangleF.Empty;
                    }
                    break;
            }
        }


        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            DX.RectangleF spriteRegion = _picker.Rectangle;
            var imageRegion = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);

            RenderSpriteTextureWithoutSprite(imageRegion, spriteRegion);

            SwapChain.RenderTargetView.Clear(BackgroundColor);

            Renderer.Begin();

            // Draw the pattern layer.
            RenderRect(imageRegion, GorgonColor.White, BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
            // Draw the image buffer layer.
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height), new GorgonColor(GorgonColor.White, 0.5f), ImageBufferTexture, new DX.RectangleF(0, 0, 1, 1));
            // Draw the sprite layer.
            RenderRect(spriteRegion, GorgonColor.White, SpriteContent.Texture, SpriteContent.Texture.ToTexel(spriteRegion.ToRectangle()), TextureArrayIndex, GorgonSamplerState.PointFiltering);

            _marchAnts.Draw(ToClient(spriteRegion));

            Renderer.End();

            return 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            if (SpriteContent?.Texture == null)
            {
                _picker.ImageData = null;
                _picker.Rectangle = DX.RectangleF.Empty;
                return;
            }

            SwapChain.Window.MouseUp += Window_MouseUp;
            SwapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;

            _picker.ImageData = SpriteContent.ImageData;
            _picker.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SwapChain.Window.MouseUp -= Window_MouseUp;
            SwapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;

            base.OnUnload();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            _picker.PointFromClient = null;
            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="pickClipper">The sprite picker used to automatically clip sprite data.</param>
        /// <param name="ants">The marching ants rectangle used to draw selection rectangles.</param>
        /// <param name="initialZoom">The initial zoom value.</param>
        public SpritePickRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, IPickClipperService pickClipper, IMarchingAnts ants, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
            _marchAnts = ants;
            _picker = pickClipper;

            _picker.PointFromClient = p =>
            {
                DX.Vector2 pos = FromClient(p);
                return new DX.Vector2((int)pos.X, (int)pos.Y);
            };            
        }
        #endregion
    }
}
