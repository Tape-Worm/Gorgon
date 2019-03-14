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
// Created: March 14, 2019 11:33:25 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DX = SharpDX;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Renderers;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The primary view for the sprite editor.
    /// </summary>
    internal partial class SpriteEditorView
        : ContentBaseControl, IDataContext<ISpriteContent>
    {
        #region Variables.
        // The default background image.
        private GorgonTexture2DView _defBgImage;
        // The graphics interface for the application.
        private GorgonGraphics _graphics;
        // The renderer for the application.
        private Gorgon2D _renderer;
        // The primary swap chain.
        private GorgonSwapChain _swapChain;
        // The patterned background to show behind the image.
        private GorgonTexture2DView _patternBg;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public ISpriteContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to handle idle time and rendering.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            Size clientSize = RenderControl.ClientSize;
            float newSize = clientSize.Width < clientSize.Height ? clientSize.Width : clientSize.Height;
            var size = new DX.Size2F(newSize.Min(_defBgImage.Width), newSize.Min(_defBgImage.Width));
            var halfClient = new DX.Size2F(clientSize.Width / 2.0f, clientSize.Height / 2.0f);
            
            GorgonTexture2DView texture = _defBgImage;
            
            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
            
            _swapChain.RenderTargetView.Clear(RenderControl.BackColor);

            _renderer.Begin();

            // If we do not have a texture, then display a message stating what we need to do.
            if (DataContext?.Texture != null)
            {
                size = new DX.Size2F(texture.Width, texture.Height);
                texture = DataContext.Texture;
            }

            var pos = new DX.Vector2((int)(halfClient.Width - size.Width / 2.0f), (int)(halfClient.Height - size.Height / 2.0f));
            var rect = new DX.RectangleF(pos.X, pos.Y, size.Width, size.Height);

            if (texture != _defBgImage)
            {
                _renderer.DrawFilledRectangle(rect, GorgonColor.White, _patternBg, new DX.RectangleF(0, 0, (rect.Width / _patternBg.Width), (rect.Height / _patternBg.Height)));
            }
            
            _renderer.DrawFilledRectangle(rect, new GorgonColor(GorgonColor.White, 0.5f), texture, new DX.RectangleF(0, 0, 1, 1));

            if (DataContext == null)
            {
                return true;
            }

            DX.Rectangle spriteRect = texture.ToPixel(DataContext.TextureCoordinates);

            _renderer.DrawFilledRectangle(new DX.RectangleF(spriteRect.X + pos.X, spriteRect.Y + pos.Y, spriteRect.Width, spriteRect.Height),
                GorgonColor.White, _patternBg, new DX.RectangleF(0, 0, (spriteRect.Width / (float)_patternBg.Width), (spriteRect.Height / (float)_patternBg.Height)));
            _renderer.DrawFilledRectangle(new DX.RectangleF(spriteRect.X + pos.X, spriteRect.Y + pos.Y, spriteRect.Width, spriteRect.Height), GorgonColor.White, texture, DataContext.TextureCoordinates);

            _renderer.End();

            _swapChain.Present(1);

            return true;
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.RenderControl"/>.</param>
        protected override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            _graphics = context.Graphics;
            _renderer = context.Renderer2D;
            _swapChain = swapChain;

            using (var stream = new MemoryStream(Resources.SpriteEditor_Bg_1024x1024))
            {
                _defBgImage = GorgonTexture2DView.FromStream(_graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "Default Sprite Background Image"
                });
            }

            _patternBg = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo("SpriteEditor_Bg_Pattern"), EditorCommonResources.CheckerBoardPatternImage);
        }

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        /// <remarks>Plug in developers do not need to clean up the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.SwapChain"/> as it will be returned to the swap chain pool automatically.</remarks>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();

            _patternBg?.Dispose();
            _defBgImage?.Dispose();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IdleMethod = Idle;

            DataContext?.OnLoad();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)            
        {
            base.SetDataContext(dataContext);

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor._Internal.Views.SpriteEditorView"/> class.</summary>
        public SpriteEditorView() => InitializeComponent();        
        #endregion
    }
}
