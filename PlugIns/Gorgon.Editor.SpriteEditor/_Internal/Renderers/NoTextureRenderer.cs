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
// Created: March 31, 2019 10:25:11 PM
// 
#endregion

using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer used to render into the sprite area when there is no sprite active.
    /// </summary>
    internal class NoTextureRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // The texture to display when a sprite lacks a texture association.
        private GorgonTexture2DView _noImage;
        #endregion        

        #region Methods.
        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            var clientSize = new DX.Size2F(SwapChain.Width, SwapChain.Height);
            float newSize = clientSize.Width < clientSize.Height ? clientSize.Width : clientSize.Height;
            var size = new DX.Size2F(newSize.Min(_noImage.Width), newSize.Min(_noImage.Width));
            var halfClient = new DX.Size2F(clientSize.Width / 2.0f, clientSize.Height / 2.0f);
            var pos = new DX.Vector2((int)(halfClient.Width - size.Width / 2.0f), (int)(halfClient.Height - size.Height / 2.0f));

            Graphics.SetRenderTarget(SwapChain.RenderTargetView);
            SwapChain.RenderTargetView.Clear(BackgroundColor);

            Renderer.Begin();
            Renderer.DrawFilledRectangle(new DX.RectangleF(pos.X, pos.Y, size.Width, size.Height), GorgonColor.White, _noImage, new DX.RectangleF(0, 0, 1, 1));
            Renderer.End();

            return 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            if (_noImage != null)
            {
                return;
            }

            using (var stream = new MemoryStream(Resources.SpriteEditor_Bg_1024x1024))
            {
                _noImage = GorgonTexture2DView.FromStream(Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "Sprite Editor - No texture default texture",
                    Usage = ResourceUsage.Immutable
                });
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            GorgonTexture2DView noImage = Interlocked.Exchange(ref _noImage, null);
            noImage?.Dispose();

            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        public NoTextureRenderer(GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer)
            : base(null, graphics, swapChain, renderer, 1.0f)
        {
        }
        #endregion
    }
}
