#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 10, 2013 8:17:04 AM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Gorgon.Core;

namespace Gorgon.Examples
{
    /// <summary>
    /// This object is resposible for drawing the spray effect on the display surface.
    /// </summary>
    class Spray
        : IDisposable
    {
        #region Variables.
        private bool _disposed;                 // Flag to indicate that the object was disposed.
        private Graphics _graphics;             // Graphics interface.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the image used for the surface.
        /// </summary>
        public Image Surface
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to resize the drawing area.
        /// </summary>
        public void Resize(Size newSize)
        {
            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
            
            var newBuffer = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(newBuffer);

            if (Surface != null)
            {
                // Copy the old image to the new surface.
                _graphics.DrawImage(Surface, Point.Empty);
                Surface.Dispose();
                Surface = null;
            }

            // Set the new buffer as the current surface.
            Surface = newBuffer;
        }

        /// <summary>
        /// Function to randomly "spray" a point on the surface.
        /// </summary>
        /// <param name="point">Origin point for the spray.</param>
        public void SprayPoint(Point point)
        {
            var randomArea = new Point(GorgonRandom.RandomInt32(-10, 10), GorgonRandom.RandomInt32(-10, 10));
            Color randomColor = Color.FromArgb(GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255));

            using (var brush = new SolidBrush(randomColor))
            {                
                _graphics.FillEllipse(brush, new Rectangle(point.X + randomArea.X
                                                            , point.Y + randomArea.Y
                                                            , 10
                                                            , 10));
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Spray" /> class.
        /// </summary>
        /// <param name="size">Size of the drawing surface.</param>
        public Spray(Size size)
        {
            Surface = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(Surface);
        }
        #endregion

        #region IDisposable Implementation.
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
	        if (_disposed)
	        {
		        return;
	        }

	        if (disposing)
	        {
		        if (Surface != null)
		        {
			        Surface.Dispose();
			        Surface = null;
		        }

		        if (_graphics != null)
		        {
			        _graphics.Dispose();
			        _graphics = null;
		        }
	        }

	        _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
