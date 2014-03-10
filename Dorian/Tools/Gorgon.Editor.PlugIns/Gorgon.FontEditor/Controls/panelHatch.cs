#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, March 10, 2014 12:44:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	// ReSharper disable once InconsistentNaming
	/// <summary>
	/// A panel used to dislpay hatch patterns for font glyphs.
	/// </summary>
	public partial class panelHatch 
		: UserControl
	{
		#region Variables.
		private GorgonColor _foregroundColor;						// Foreground hatch color.
		private GorgonColor _backgroundColor;						// Background hatch color.
	    private Bitmap _previewBitmap;                              // Bitmap used to draw the preview.
	    private Bitmap _foreColorBitmap;                            // Bitmap used to draw the foreground color.
	    private Bitmap _backColorBitmap;                            // Bitmap used to draw the background color.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current pattern brush.
		/// </summary>
		public GorgonGlyphHatchBrush Brush
		{
			get;
			set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Handles the SelectedIndexChanged event of the comboHatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboHatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DrawPreview();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
        }


        /// <summary>
        /// Handles the Paint event of the panelForegroundColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelForegroundColor_Paint(object sender, PaintEventArgs e)
        {
            DrawForeColor(e.Graphics);
        }

        /// <summary>
        /// Handles the Paint event of the panelBackgroundColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelBackgroundColor_Paint(object sender, PaintEventArgs e)
        {
            DrawBackColor(e.Graphics);
        }

        /// <summary>
        /// Handles the Paint event of the panelPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            DrawPreview(e.Graphics);
        }

        /// <summary>
        /// Function to draw the foreground color.
        /// </summary>
        /// <param name="graphics">The graphics interface to use.</param>
	    private void DrawForeColor(System.Drawing.Graphics graphics = null)
	    {
            using (Brush brush = new SolidBrush(_foregroundColor),
                backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_foreColorBitmap))
                {
                    g.FillRectangle(backBrush, panelForegroundColor.ClientRectangle);
                    g.FillRectangle(brush, panelForegroundColor.ClientRectangle);
                }
            }

            System.Drawing.Graphics panelGraphics = graphics;

            try
            {
                if (panelGraphics == null)
                {
                    panelGraphics = panelForegroundColor.CreateGraphics();
                }

                panelGraphics.DrawImage(_foreColorBitmap, Point.Empty);
            }
            finally
            {
                if ((panelGraphics != null)
                    && (graphics == null))
                {
                    panelGraphics.Dispose();
                }
            }
	    }

        /// <summary>
        /// Function to draw the background color.
        /// </summary>
        /// <param name="graphics">The graphics interface to use.</param>
	    private void DrawBackColor(System.Drawing.Graphics graphics = null)
	    {
            using (Brush brush = new SolidBrush(_backgroundColor),
                backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_backColorBitmap))
                {
                    g.FillRectangle(backBrush, panelBackgroundColor.ClientRectangle);
                    g.FillRectangle(brush, panelBackgroundColor.ClientRectangle);
                }
            }

            System.Drawing.Graphics panelGraphics = graphics;

            try
            {
                if (panelGraphics == null)
                {
                    panelGraphics = panelBackgroundColor.CreateGraphics();
                }

                panelGraphics.DrawImage(_backColorBitmap, Point.Empty);
            }
            finally
            {
                if ((panelGraphics != null)
                    && (graphics == null))
                {
                    panelGraphics.Dispose();
                }
            }
        }

        /// <summary>
		/// Function to draw the preview window.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		private void DrawPreview(System.Drawing.Graphics graphics = null)
		{

			using (Brush brush = new HatchBrush(comboHatch.Style, _foregroundColor, _backgroundColor),
                backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile))
			{
			    using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_previewBitmap))
			    {
                    g.FillRectangle(backBrush, panelPreview.ClientRectangle);
                    g.FillRectangle(brush, panelPreview.ClientRectangle);
			    }
			}

			System.Drawing.Graphics panelGraphics = graphics;

			try
			{
				if (panelGraphics == null)
				{
					panelGraphics = panelPreview.CreateGraphics();
				}

				panelGraphics.DrawImage(_previewBitmap, Point.Empty);
			}
			finally
			{
				if ((panelGraphics != null)
					&& (graphics == null))
				{
					panelGraphics.Dispose();
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
			    _previewBitmap = new Bitmap(panelPreview.ClientSize.Width,
			                                panelPreview.ClientSize.Height,
			                                PixelFormat.Format32bppArgb);
			    _foreColorBitmap = new Bitmap(panelForegroundColor.ClientSize.Width,
			                                  panelForegroundColor.ClientSize.Height,
			                                  PixelFormat.Format32bppArgb);
			    _backColorBitmap = new Bitmap(panelBackgroundColor.ClientSize.Width,
			                                  panelBackgroundColor.ClientSize.Height,
			                                  PixelFormat.Format32bppArgb);

				comboHatch.RefreshPatterns();

			    if (Brush == null)
			    {
			        comboHatch.Style = HatchStyle.BackwardDiagonal;
			        _foregroundColor = Color.Black;
			        _backgroundColor = Color.White;
			    }
			    else
			    {
			        comboHatch.Style = Brush.HatchStyle;
			        _foregroundColor = Brush.ForegroundColor;
			        _backgroundColor = Brush.BackgroundColor;
			    }
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="panelHatch"/> class.
		/// </summary>
		public panelHatch()
		{
			InitializeComponent();
		}
		#endregion
	}
}
