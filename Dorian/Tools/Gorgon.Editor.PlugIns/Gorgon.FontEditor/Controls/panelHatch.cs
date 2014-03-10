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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
		private GorgonGlyphHatchBrush _brush;						// Current brush being edited.
		private GorgonColor _foregroundColor;						// Foreground hatch color.
		private GorgonColor _backgroundColor;						// Background hatch color.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current pattern brush.
		/// </summary>
		public GorgonGlyphHatchBrush Brush
		{
			get
			{
				return _brush;
			}
			set
			{
				_brush = value;

				if (_brush == null)
				{
					comboHatch.Style = HatchStyle.BackwardDiagonal;
					_foregroundColor = Color.Black;
					_backgroundColor = Color.White;
					return;
				}

				comboHatch.Style = _brush.HatchStyle;
				_foregroundColor = _brush.ForegroundColor;
				_backgroundColor = _brush.BackgroundColor;

				DrawPreview();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the preview window.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		private void DrawPreview(System.Drawing.Graphics graphics = null)
		{
			using (Brush brush = new HatchBrush(comboHatch.Style, _foregroundColor, _backgroundColor))
			{
				// TODO: Paint the brush.
			}

			System.Drawing.Graphics panelGraphics = graphics;

			try
			{
				if (panelGraphics == null)
				{
					panelGraphics = panelPreview.CreateGraphics();
				}

				// TODO: Draw to panel.
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
				comboHatch.RefreshPatterns();
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
