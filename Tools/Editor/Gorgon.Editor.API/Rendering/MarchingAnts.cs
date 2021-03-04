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
// Created: March 15, 2019 4:03:29 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// Draws a marching ants effect for a rectangle.
    /// </summary>
    public class MarchingAnts
        : IDisposable, IMarchingAnts
    {
        #region Variables.
        // The texture used for the marching ants.
        private Lazy<GorgonTexture2DView> _marchAntsTexture;
        // The renderer to use.
        private readonly Gorgon2D _renderer;
        // The step for each movement of the ants.
        private float _step;
        #endregion

        #region Methods.
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Lazy<GorgonTexture2DView> ants = Interlocked.Exchange(ref _marchAntsTexture, null);

            if ((ants is null) || (!ants.IsValueCreated))
            {
                return;
            }

            ants.Value.Dispose();
        }

        /// <summary>
        /// Function to draw the marching ants rectangle.
        /// </summary>
        /// <param name="rect">The rectangular region to draw in.</param>
        public void Draw(DX.RectangleF rect)
        {
            _step += _marchAntsTexture.Value.Width * (GorgonTiming.Delta * 0.4f);

            if (_step > _marchAntsTexture.Value.Width)
            {
                _step -= _marchAntsTexture.Value.Width;
            }

            _renderer.DrawRectangle(rect, GorgonColor.White,
                texture: _marchAntsTexture.Value,
                textureRegion: _marchAntsTexture.Value.ToTexel(new DX.Rectangle((int)-_step, 0, (int)rect.Width, (int)rect.Height)),
                textureSampler: GorgonSamplerState.PointFilteringWrapping);
        }

        /// <summary>
        /// Function to build an instance of the marching ants texture.
        /// </summary>
        /// <returns>A new instance of the marching ants texture.</returns>
        private GorgonTexture2DView Build()
        {
            using (var image = new MemoryStream(Resources.march_ants_diag_32x32))
            {
                return GorgonTexture2DView.FromStream(_renderer.Graphics, image, new GorgonCodecDds(),
                    options: new GorgonTexture2DLoadOptions
                    {
                        Name = "MarchingAntsTexture",
                        Usage = ResourceUsage.Immutable
                    });
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Gorgon.Editor.Rendering.MarchingAnts"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        public MarchingAnts(Gorgon2D renderer)
        {
            _renderer = renderer;
            _marchAntsTexture = new Lazy<GorgonTexture2DView>(Build, true);
        }
        #endregion
    }
}
