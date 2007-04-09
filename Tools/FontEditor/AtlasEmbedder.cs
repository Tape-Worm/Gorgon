#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, August 14, 2006 1:32:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities.Mathematics;
using GorgonLibrary;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Form to place a font within an atlas.
	/// </summary>
	public partial class AtlasEmbedder : Form
	{
		#region Variables.
		private Drawing.Bitmap _fontBitmap;					// Font bitmap.
		private Drawing.Bitmap _atlas;						// Atlas bitmap.
		private Drawing.Bitmap _buffer;						// Buffer for double buffering.
		private Drawing.Bitmap _zoomImage;					// Image to zoom.
		private Drawing.Bitmap _zoomBuffer;					// Buffer for double buffering.
		private Drawing.Point _mouse;						// Mouse position.
		private bool _placed = false;						// True if font is placed, False if not.
		private Drawing.Point _placePosition;				// Font placement position.
		private Drawing.Font _positionFont;					// Font to use in drawing position info.
		private Drawing.SolidBrush _purpleBrush;			// Placed font brush.
		private Drawing.SolidBrush _blackBrush;				// Font bitmap background brush.
		private Drawing.Pen _redPen;						// Rectangle pen.
		private Drawing.Pen _bluePen;						// Line pen.
		private Drawing.Graphics _zoomImageGraphics = null;	// Graphics interface for the zoom image.
		private Drawing.Graphics _zoomPanelGraphics = null;	// Graphics interface for the zoom panel.
		private Drawing.Graphics _zoomBufferGraphics = null;// Graphics interface for the zoom buffer.
		private Drawing.Graphics _mainPanelGraphics = null;	// Main panel graphics interface.
		private Drawing.Graphics _bufferGraphics = null;	// Double buffer graphics interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the placement of the font.
		/// </summary>
		public Drawing.Point Placement
		{
			get
			{
				return _placePosition;
			}
			set
			{
				_placed = true;
				buttonOK.Enabled = true;
				_placePosition = value;
				scrollHorizontal.Value = _placePosition.X;
				scrollVertical.Value = _placePosition.Y;
			}
		}

		/// <summary>
		/// Property to set or return the font bitmap.
		/// </summary>
		public Drawing.Bitmap FontBitmap
		{
			get
			{
				return _fontBitmap;
			}
			set
			{
				_fontBitmap = value;
			}
		}

		/// <summary>
		/// Property to set or return the atlas bitmap.
		/// </summary>
		public Drawing.Bitmap AtlasBitmap
		{
			get
			{
				return _atlas;
			}
			set
			{
				_atlas = value;
				if (_buffer != null)
					_buffer.Dispose();
				_buffer = new Drawing.Bitmap(panelImage.ClientSize.Width, panelImage.ClientSize.Height, Drawing.Imaging.PixelFormat.Format32bppArgb);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ValueChanged event of the scrollVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void scrollVertical_ValueChanged(object sender, EventArgs e)
		{
			Draw();
		}

		/// <summary>
		/// Handles the ValueChanged event of the scrollHorizontal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void scrollHorizontal_ValueChanged(object sender, EventArgs e)
		{
			Draw();
		}

		/// <summary>
		/// Function to draw shadowed text.
		/// </summary>
		/// <param name="graphics">Graphics interface.</param>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Location of the text.</param>
		private void DrawText(Drawing.Graphics graphics, string text, Drawing.Point position)
		{
			graphics.DrawString(text, _positionFont, Drawing.Brushes.Black, position.X - scrollHorizontal.Value, position.Y - scrollVertical.Value);
			graphics.DrawString(text, _positionFont, Drawing.Brushes.White, position.X - scrollHorizontal.Value - 2, position.Y - scrollVertical.Value - 2);
		}

		/// <summary>
		/// Function to draw the font image in its placement and via the mouse.
		/// </summary>
		/// <param name="graphics">Graphics interface.</param>
		/// <param name="position">Position of the font 'cursor'.</param>
		private void DrawFont(Drawing.Graphics graphics, Drawing.Point position)
		{
			// Draw dropped font.
			if (_placed)
			{
				DrawText(graphics, _placePosition.X.ToString() + "x" + _placePosition.Y.ToString(), new Drawing.Point(_placePosition.X, _placePosition.Y - _positionFont.Height));
				graphics.FillRectangle(_purpleBrush, _placePosition.X - scrollHorizontal.Value, _placePosition.Y - scrollVertical.Value, _fontBitmap.Width, _fontBitmap.Height);
				graphics.DrawImage(_fontBitmap, _placePosition.X - scrollHorizontal.Value, _placePosition.Y - scrollVertical.Value);
			}

			// Draw the font 'cursor'.
			graphics.FillRectangle(_blackBrush, position.X, position.Y, _fontBitmap.Width, _fontBitmap.Height);
			graphics.DrawImage(_fontBitmap, position.X, position.Y);
			graphics.DrawRectangle(_redPen, position.X - 1, position.Y - 1, _fontBitmap.Width + 1, _fontBitmap.Height + 1);
			graphics.DrawLine(_bluePen, position.X, 0, position.X, panelImage.ClientSize.Height);
			graphics.DrawLine(_bluePen, 0, position.Y, panelImage.ClientSize.Width, position.Y);
		}

		/// <summary>
		/// Function to draw the zoomed cursor position.
		/// </summary>
		/// <param name="position">Position to zoom.</param>
		private void DrawZoom(Drawing.Point position)
		{
			_zoomImageGraphics.CompositingMode = Drawing.Drawing2D.CompositingMode.SourceCopy;
			_zoomImageGraphics.Clear(Drawing.Color.Black);
			_zoomImageGraphics.DrawImage(_buffer, new Drawing.Rectangle(0, 0, 20, 20), new Drawing.Rectangle(position.X - 10, position.Y - 10, 20, 20), Drawing.GraphicsUnit.Pixel);
			_zoomBufferGraphics.Clear(Drawing.Color.Gray);
			_zoomBufferGraphics.InterpolationMode = Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			_zoomBufferGraphics.DrawImage(_zoomImage, 0, 0, panelZoom.Width + 1, panelZoom.Height + 1);
		}

		/// <summary>
		/// Function to draw the display.
		/// </summary>
		private void Draw()
		{
			Drawing.Point mousePoint = Drawing.Point.Empty;		// Actual mouse cursor position.

			try
			{
				_mainPanelGraphics = Drawing.Graphics.FromHwnd(panelImage.Handle);
				_bufferGraphics = Drawing.Graphics.FromImage(_buffer);

				_bufferGraphics.Clear(Drawing.Color.FromArgb(255, 169, 153, 112));
				_bufferGraphics.DrawImage(_atlas, -scrollHorizontal.Value, -scrollVertical.Value);

				// Draw the font in its placement position.
				mousePoint.X = _mouse.X - scrollHorizontal.Value;
				mousePoint.Y = _mouse.Y - scrollVertical.Value;
				DrawFont(_bufferGraphics, mousePoint);

				// Draw zoom window.
				DrawZoom(mousePoint);

				// Draw to screen.
				_mainPanelGraphics.DrawImage(_buffer, 1, 1);
				_zoomPanelGraphics.DrawImage(_zoomBuffer, 0, 0);

				UpdateScrollBars(_atlas.Size);
			}
			finally
			{
				if (_mainPanelGraphics != null)
					_mainPanelGraphics.Dispose();
				if (_bufferGraphics != null)
					_bufferGraphics.Dispose();

				_bufferGraphics = null;
				_mainPanelGraphics = null;
			}
		}

		/// <summary>
		/// Handles the Paint event of the panelImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void panelImage_Paint(object sender, PaintEventArgs e)
		{
			Draw();
		}

		/// <summary>
		/// Handles the MouseMove event of the panelImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelImage_MouseMove(object sender, MouseEventArgs e)
		{
			_mouse.X = e.X - 1 + scrollHorizontal.Value;
			_mouse.Y = e.Y - 1 + scrollVertical.Value;

			labelPosition.Text = "Position: " + _mouse.X.ToString() + "x" + _mouse.Y.ToString();
			labelPosition.Owner.Refresh();
			labelPosition.Invalidate();
			Draw();
		}

		/// <summary>
		/// Handles the MouseDown event of the panelImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelImage_MouseDown(object sender, MouseEventArgs e)
		{
			_placed = true;
			_placePosition = _mouse;
			buttonOK.Enabled = true;
		}

		/// <summary>
		/// Handles the Click event of the buttonReset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonReset_Click(object sender, EventArgs e)
		{
			_placePosition = Drawing.Point.Empty;
			_placed = false;
			buttonOK.Enabled = false;
			Draw();
		}

		/// <summary>
		/// Function to update the scrollbars.
		/// </summary>
		/// <param name="size">Target size.</param>
		private void UpdateScrollBars(Drawing.Size size)
		{
			int maxHScroll = 100;		// Defaults.
			int maxVScroll = 100;

			// Set the scrollers.
			if ((size.Width > panelImage.ClientSize.Width) || ((scrollVertical.Visible) && (size.Width + scrollVertical.Width > panelImage.ClientSize.Width)))
			{
				scrollHorizontal.Visible = true;
				maxHScroll = (size.Width - panelImage.ClientSize.Width) + (scrollHorizontal.LargeChange);
			}
			else
			{
				scrollHorizontal.Value = 0;
				scrollHorizontal.Visible = false;
			}

			if ((size.Height > panelImage.ClientSize.Height) || ((scrollHorizontal.Visible) && (size.Height + scrollHorizontal.Height > panelImage.ClientSize.Height)))
			{
				scrollVertical.Visible = true;
				maxVScroll = (size.Height - panelImage.ClientSize.Height) + (scrollVertical.LargeChange);
			}
			else
			{
				scrollVertical.Value = 0;
				scrollVertical.Visible = false;
			}

			if (scrollHorizontal.Visible)
				maxVScroll += scrollHorizontal.Size.Height;

			if (scrollVertical.Visible)
				maxHScroll += scrollVertical.Size.Width;

			scrollHorizontal.Maximum = maxHScroll;
			scrollVertical.Maximum = maxVScroll;

			if ((scrollVertical.Visible) || (scrollHorizontal.Visible))
				labelScrollSpace.Visible = true;
			else
				labelScrollSpace.Visible = false;
		}
		
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_positionFont != null)
				_positionFont.Dispose();
			if (_zoomBuffer != null)
				_zoomBuffer.Dispose();
			if (_zoomImage != null)
				_zoomImage.Dispose();
			if (_buffer != null)
				_buffer.Dispose();

			if (_blackBrush != null)
				_blackBrush.Dispose();
			if (_purpleBrush != null)
				_purpleBrush.Dispose();
			if (_redPen != null)
				_redPen.Dispose();
			if (_bluePen != null)
				_bluePen.Dispose();

			// Remove any graphics interfaces.
			if (_zoomImageGraphics != null)
				_zoomImageGraphics.Dispose();
			if (_zoomPanelGraphics != null)
				_zoomPanelGraphics.Dispose();
			if (_zoomBufferGraphics != null)
				_zoomBufferGraphics.Dispose();
			if (_mainPanelGraphics != null)
				_mainPanelGraphics.Dispose();
			if (_bufferGraphics != null)
				_bufferGraphics.Dispose();

			_zoomImageGraphics = null;
			_zoomPanelGraphics = null;
			_zoomBufferGraphics = null;
			_mainPanelGraphics = null;
			_bufferGraphics = null;
			_redPen = null;
			_bluePen = null;
			_purpleBrush = null;
			_blackBrush = null;
			_positionFont = null;
			_zoomBuffer = null;
			_zoomImage = null;
			_buffer = null;
		}

		/// <summary>
		/// Raises the resize event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_buffer != null)
				_buffer.Dispose();

			_buffer = new Drawing.Bitmap(panelImage.ClientSize.Width, panelImage.ClientSize.Height);

			if (scrollHorizontal.Visible)
			{
				if (scrollHorizontal.Value >= scrollHorizontal.Maximum)
					scrollHorizontal.Value = scrollHorizontal.Maximum;
			}
			else
				scrollHorizontal.Value = 0;

			if (scrollVertical.Visible)
			{
				if (scrollVertical.Value >= scrollVertical.Maximum)
					scrollVertical.Value = scrollVertical.Maximum;
			}
			else
				scrollVertical.Value = 0;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public AtlasEmbedder()
		{
			InitializeComponent();

			_mouse = Drawing.Point.Empty;
			_zoomImage = new Drawing.Bitmap(20, 20, Drawing.Imaging.PixelFormat.Format32bppArgb);
			_zoomBuffer = new Drawing.Bitmap(panelZoom.Width, panelZoom.Height, Drawing.Imaging.PixelFormat.Format32bppArgb);
			_positionFont = new Drawing.Font("Arial", 12.0f, Drawing.FontStyle.Bold);			

			// Create brushes and pens.
			_redPen = new Drawing.Pen(Drawing.Color.FromArgb(128,255,0,0));
			_bluePen = new Drawing.Pen(Drawing.Color.FromArgb(128, 0, 0, 255));
			_purpleBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(128,255,0,255));
			_blackBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(128,0,0,0));

			// Create graphics interfaces.
			_zoomImageGraphics = Drawing.Graphics.FromImage(_zoomImage);
			_zoomPanelGraphics = Drawing.Graphics.FromHwnd(panelZoom.Handle);
			_zoomBufferGraphics = Drawing.Graphics.FromImage(_zoomBuffer);
			_mainPanelGraphics = null;
			_bufferGraphics = null;
		}
		#endregion
	}
}