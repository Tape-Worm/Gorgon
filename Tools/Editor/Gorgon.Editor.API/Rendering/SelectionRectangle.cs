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
// Created: February 24, 2019 5:25:42 PM
// 
#endregion

using System;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// Used to render a selection rectangle on the UI.
    /// </summary>
    public class SelectionRectangle
        : IDisposable, ISelectionRectangle
    {
        #region Variables.
        // The renderer to use.
        private readonly Gorgon2D _renderer;
        // The texture to use for the selection overlay.
        private readonly Lazy<GorgonTexture2DView> _selectionTexture;
        // The offset of the texture within the selection rectangle.
        private Vector2 _selectionTextureOffset;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the color for the selection rectangle.
        /// </summary>
        public GorgonColor Color
        {
            get;
            set;
        } = new GorgonColor(GorgonColor.BluePure, 0.5f);

        /// <summary>
        /// Property to set or return the color for the selection rectangle border.
        /// </summary>
        public GorgonColor BorderColor
        {
            get;
            set;
        } = GorgonColor.BluePure;

        /// <summary>
        /// Property to set or return the speed of the overlay animation.
        /// </summary>
        public Vector2 Speed
        {
            get;
            set;
        } = new Vector2(16, 16);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create the selection texture.
        /// </summary>
        private GorgonTexture2DView CreateSelectionTexture()
        {
            GorgonTexture2DView result;

            using (var stream = new MemoryStream(Resources.selection_16x16))
            {
                result = GorgonTexture2DView.FromStream(_renderer.Graphics, stream, new GorgonCodecDds());
            }

            return result;
        }

        /// <summary>
        /// Function to draw the selection region.
        /// </summary>
        /// <param name="region">The region to draw.</param>
        public void Draw(DX.RectangleF region)
        {
            GorgonTexture2DView texture = _selectionTexture.Value;

            Debug.Assert(texture is not null, "No texture was found for the selection rectangle.");

            _renderer.DrawFilledRectangle(region,
                Color,
                texture,
                texture.ToTexel(new DX.Rectangle((int)_selectionTextureOffset.X,
                (int)_selectionTextureOffset.Y,
                (int)region.Width, (int)region.Height)));

            _renderer.DrawRectangle(region, BorderColor);

            _selectionTextureOffset = new Vector2(_selectionTextureOffset.X - (GorgonTiming.Delta * Speed.X), _selectionTextureOffset.Y - (GorgonTiming.Delta * Speed.Y));

            float regionMin = region.Width.Max(region.Height) * 2;

            if (_selectionTextureOffset.X < -regionMin)
            {
                _selectionTextureOffset = new Vector2(-(regionMin + _selectionTextureOffset.X), _selectionTextureOffset.Y);
            }

            if (_selectionTextureOffset.Y < -regionMin)
            {
                _selectionTextureOffset = new Vector2(_selectionTextureOffset.X, -(regionMin + _selectionTextureOffset.Y));
            }

            if (_selectionTextureOffset.X > regionMin)
            {
                _selectionTextureOffset = new Vector2((_selectionTextureOffset.X - regionMin), _selectionTextureOffset.Y);
            }

            if (_selectionTextureOffset.Y > regionMin)
            {
                _selectionTextureOffset = new Vector2(_selectionTextureOffset.X, (_selectionTextureOffset.Y - regionMin));
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_selectionTexture.IsValueCreated)
            {
                _selectionTexture.Value.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SelectionRectangle"/> class.</summary>
        /// <param name="graphics">The graphics context to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public SelectionRectangle(IGraphicsContext graphics)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            _renderer = graphics.Renderer2D;
            _selectionTexture = new Lazy<GorgonTexture2DView>(CreateSelectionTexture, LazyThreadSafetyMode.ExecutionAndPublication);
        }
        #endregion
    }
}
