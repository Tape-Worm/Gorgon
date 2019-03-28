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
// Created: April 1, 2019 9:28:03 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.SpriteEditor.Properties;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The renderer used for the sprite clipping tool.
    /// </summary>
    internal class SpriteClipRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // The rectangle clipping service.
        private readonly IRectClipperService _clipper;
        #endregion

        #region Properties.
        /// <summary>Property to return some information to display about the sprite given then current renderer context.</summary>
        public override string SpriteInfo
        {
            get
            {
                var spriteTextureBounds = _clipper.Rectangle.ToRectangle();
                return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, spriteTextureBounds.Left,
                                                                   spriteTextureBounds.Top,
                                                                   spriteTextureBounds.Right,
                                                                   spriteTextureBounds.Bottom,
                                                                   spriteTextureBounds.Width,
                                                                   spriteTextureBounds.Height);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Handles the RectChanged event of the Clipper control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void Clipper_RectChanged(object sender, EventArgs e) => SpriteContent.ManualInput.Rectangle = _clipper.Rectangle;

        /// <summary>Handles the KeyboardIconClicked event of the Clipper control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Clipper_KeyboardIconClicked(object sender, EventArgs e)
        {
            if ((SpriteContent?.ToggleManualInputCommand == null) || (!SpriteContent.ToggleManualInputCommand.CanExecute(null)))
            {
                return;
            }

            SpriteContent.ToggleManualInputCommand.Execute(null);
        }

        /// <summary>Handles the PreviewKeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SpriteContent.CurrentTool = SpriteEditTool.None;
                    if (SpriteContent.ManualInput.IsActive)
                    {
                        Clipper_KeyboardIconClicked(_clipper, EventArgs.Empty);
                    }
                    e.IsInputKey = true;
                    break;
                case Keys.Escape:
                    // Revert the changes, when we switch tools, we'll be committing our changes to the data context, and if they haven't changed, nothing will be 
                    // updated.
                    _clipper.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
                    TextureArrayIndex = SpriteContent.ArrayIndex;
                    SpriteContent.CurrentTool = SpriteEditTool.None;
                    if (SpriteContent.ManualInput.IsActive)
                    {
                        Clipper_KeyboardIconClicked(_clipper, EventArgs.Empty);
                    }
                    e.IsInputKey = true;
                    break;
                default:
                    e.IsInputKey = _clipper.KeyDown(e.KeyCode, e.Modifiers);
                    break;
            }
        }
        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            _clipper.MousePosition = new DX.Vector2(e.X, e.Y);
            _clipper.MouseUp(e.Button);
        }

        /// <summary>Handles the MouseDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            _clipper.MousePosition = new DX.Vector2(e.X, e.Y);
            _clipper.MouseDown(e.Button);
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e) 
        {
            _clipper.MousePosition = new DX.Vector2(e.X, e.Y);
            _clipper.MouseMove(e.Button);
        }

        /// <summary>Handles the PropertyChanged event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualInput_PropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            switch (e.PropertyName)
            {
                case nameof(IManualRectInputVm.Rectangle):
                    _clipper.Rectangle = SpriteContent.ManualInput.Rectangle;
                    break;
            }
        }

        /// <summary>
        /// Function to submit the updated texture coordinates to the sprite.
        /// </summary>
        private void SubmitTextureCoordinates()
        {
            (DX.RectangleF rect, int arrayIndex) args = (_clipper.Rectangle, TextureArrayIndex);

            if ((SpriteContent?.SetTextureCoordinatesCommand == null) || (!SpriteContent.SetTextureCoordinatesCommand.CanExecute(args)))
            {
                return;
            }

            SpriteContent.SetTextureCoordinatesCommand.Execute(args);
        }

        /// <summary>Function called when the <see cref="ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged() => _clipper.Refresh();

        /// <summary>Function called when the <see cref="ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged() => _clipper.Refresh();

        /// <summary>Function called when after the swap chain is resized.</summary>
        protected override void OnAfterSwapChainResized() => _clipper.Refresh();

        /// <summary>Function called when the sprite is changing a property.</summary>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        protected override void OnSpriteChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentTool):
                    if (SpriteContent.CurrentTool == SpriteEditTool.SpriteClip)
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
                case nameof(ISpriteContent.Texture):
                    if (SpriteContent.Texture != null)
                    {
                        _clipper.Bounds = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);
                        _clipper.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
                    }
                    else
                    {
                        _clipper.Bounds = DX.RectangleF.Empty;
                        _clipper.Rectangle = DX.RectangleF.Empty;
                    }
                    break;
                case nameof(ISpriteContent.TextureCoordinates):
                    if (SpriteContent.Texture == null)
                    {
                        return;
                    }

                    _clipper.Rectangle = ToClient(SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF()).Truncate();
                    break;
            }
        }       
        
        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            DX.RectangleF textureRegion = _clipper.Rectangle;
            DX.RectangleF spriteRegion = SpriteContent.Texture.ToTexel(textureRegion.ToRectangle());
            var imageRegion = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);

            RenderSpriteTextureWithoutSprite(imageRegion, spriteRegion);

            SwapChain.RenderTargetView.Clear(BackgroundColor);

            Renderer.Begin();

            // Draw the pattern layer.
            RenderRect(imageRegion, GorgonColor.White, BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));

            // Draw the image buffer layer.
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height), new GorgonColor(GorgonColor.White, 0.5f), ImageBufferTexture, new DX.RectangleF(0, 0, 1, 1));

            RenderRect(textureRegion, GorgonColor.White, SpriteContent.Texture, SpriteContent.Texture.ToTexel(textureRegion.ToRectangle()), TextureArrayIndex, GorgonSamplerState.PointFiltering);

            if ((SpriteContent.ManualInput != null) && (SpriteContent.ManualInput.IsActive))
            {
                Renderer.DrawFilledRectangle(new DX.RectangleF(SwapChain.Width - 182, 0, 182, 151), new GorgonColor(GorgonColor.BluePure, 0.4f));
                Renderer.DrawRectangle(new DX.RectangleF(SwapChain.Width - 181, 1, 182, 151), GorgonColor.BluePure, 2);
            }

            _clipper.Render();

            Renderer.End();

            // When we're dragging, update the swap presentation faster so we reduce the lag from the mouse input.
            return _clipper.IsDragging ? 0 : 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            if (SpriteContent.Texture != null)
            {
                _clipper.Bounds = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);
                _clipper.Rectangle = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            }
            else
            {
                _clipper.Bounds = DX.RectangleF.Empty;
                _clipper.Rectangle = DX.RectangleF.Empty;
            }

            SpriteContent.ManualInput.PropertyChanged += ManualInput_PropertyChanged;

            SwapChain.Window.MouseMove += Window_MouseMove;
            SwapChain.Window.MouseDown += Window_MouseDown;
            SwapChain.Window.MouseUp += Window_MouseUp;
            SwapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SpriteContent.ManualInput.PropertyChanged -= ManualInput_PropertyChanged;

            SwapChain.Window.MouseMove -= Window_MouseMove;
            SwapChain.Window.MouseDown -= Window_MouseDown;
            SwapChain.Window.MouseUp -= Window_MouseUp;
            SwapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;

            base.OnUnload();
        }        

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            // Unhook our transformation callbacks so we can avoid keeping things around longer than they should.
            _clipper.KeyboardIconClicked -= Clipper_KeyboardIconClicked;
            _clipper.RectChanged -= Clipper_RectChanged;
            _clipper.RectToClient = null;
            _clipper.PointFromClient = null;

            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="rectClipper">The rectangle clipper used to clip out sprite data.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        public SpriteClipRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, IRectClipperService rectClipper, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
            _clipper = rectClipper;

            _clipper.RectToClient = r => ToClient(r).Truncate();
            _clipper.PointFromClient = p => FromClient(p).Truncate();
            _clipper.KeyboardIconClicked += Clipper_KeyboardIconClicked;
            _clipper.RectChanged += Clipper_RectChanged;
        }
        #endregion
    }
}
