﻿#region MIT.
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
// Created: Thursday, January 10, 2013 8:33:49 AM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Gorgon.Examples
{
    /// <summary>
    /// This will display our mouse cursor on a double buffered surface.
    /// </summary>
    class MouseCursor
        : IDisposable
    {
        #region Variables.
        private bool _disposed;                                 // Flag to indicate that the object was disposed.
        private BufferedGraphicsContext _graphicsContext;		// Buffered graphics context.
        private BufferedGraphics _buffer;						// Buffered graphics page.
        private Image _mouseImage;								// Image to use for double buffering our mouse.
        private Graphics _imageGraphics;						// Graphics interface for the mouse double buffer image.
        private Graphics _graphics;								// GDI+ graphics interface.
        private Color _clearColor = Color.White;                // Color to clear the surface with.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the cursor hot spot.
		/// </summary>
		public Point Hotspot
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
        /// Function to clean up any objects that are allocating memory.
        /// </summary>
        private void CleanUp()
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }

            if (_graphicsContext != null)
            {
                _graphicsContext.Dispose();
                _graphicsContext = null;
            }

            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }

            if (_imageGraphics != null)
            {
                _imageGraphics.Dispose();
                _imageGraphics = null;
            }

			if (_mouseImage == null)
			{
				return;
			}

			_mouseImage.Dispose();
			_mouseImage = null;
        }

        /// <summary>
        /// Function to create the double buffer surface resources.
        /// </summary>
        /// <param name="displayControl">The control that will be used as the display.</param>
        private void CreateDoubleBufferSurface(Control displayControl)
        {
            _graphics = Graphics.FromHwnd(displayControl.Handle);

            _mouseImage = new Bitmap(displayControl.ClientSize.Width, displayControl.ClientSize.Height, PixelFormat.Format32bppArgb);
            _imageGraphics = Graphics.FromImage(_mouseImage);

            _graphicsContext = BufferedGraphicsManager.Current;
            _buffer = _graphicsContext.Allocate(_imageGraphics, new Rectangle(0, 0, _mouseImage.Width, _mouseImage.Height));
        }

        /// <summary>
        /// Handles the Resize event of the displayControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void displayControl_Resize(object sender, EventArgs e)
        {
            var displayControl = (Control)sender;
            Form ownerForm = displayControl.FindForm();
            
            CleanUp();

            if ((ownerForm != null) && (ownerForm.WindowState != FormWindowState.Minimized))
            {
                CreateDoubleBufferSurface(displayControl);
            }
        }

        /// <summary>
        /// Function to draw the mouse cursor.
        /// </summary>
        /// <param name="position">Position of the mouse cursor.</param>
        /// <param name="cursor">The image used for the mouse cursor.</param>
        /// <param name="additionalBuffer">Image to copy into the buffer before the cursor is displayed.</param>
        public void DrawMouseCursor(Point position, Image cursor, Image additionalBuffer)
        {
            _buffer.Graphics.Clear(_clearColor);
            if (additionalBuffer != null)
            {
                _buffer.Graphics.DrawImage(additionalBuffer, Point.Empty);
            }

            _buffer.Graphics.DrawImage(cursor, new Point(position.X + Hotspot.X, position.Y + Hotspot.Y));
            _buffer.Render();
            _buffer.Render(_graphics);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseCursor" /> class.
        /// </summary>
        /// <param name="displayControl">The control that acts as our display area.</param>
        public MouseCursor(Control displayControl)
        {
            CreateDoubleBufferSurface(displayControl);

            _clearColor = displayControl.BackColor;
            displayControl.Resize += displayControl_Resize;            
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
		        CleanUp();
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
