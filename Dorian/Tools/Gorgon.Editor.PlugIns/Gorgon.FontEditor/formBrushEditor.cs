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
// Created: Wednesday, November 13, 2013 8:58:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// An editor that will create a brush for a font.
    /// </summary>
    partial class formBrushEditor 
        : ZuneForm
    {
        #region Variables.
	    private GorgonGlyphTextureBrush _textureBrush;
	    private GorgonGlyphTextureBrush _defaultTextureBrush;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the current brush type.
		/// </summary>
	    public GlyphBrushType BrushType
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current solid brush.
		/// </summary>
	    public GorgonGlyphSolidBrush SolidBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current hatch pattern brush.
		/// </summary>
	    public GorgonGlyphHatchBrush PatternBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current gradient brush.
		/// </summary>
	    public GorgonGlyphLinearGradientBrush GradientBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current texture brush.
		/// </summary>
	    public GorgonGlyphTextureBrush TextureBrush
	    {
			get
			{
				return _textureBrush ?? _defaultTextureBrush;
			}
			set
			{
				if (value == null)
				{
					value = _defaultTextureBrush;
				}
				_textureBrush = value;
			}
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to localize the form.
		/// </summary>
	    private void Localize()
	    {
		    comboBrushType.Items.Clear();
			comboBrushType.Items.Add(Resources.GORFNT_PROP_VALUE_SOLID_BRUSH);
			comboBrushType.Items.Add(Resources.GORFNT_PROP_VALUE_GRADIENT_BRUSH);
			comboBrushType.Items.Add(Resources.GORFNT_PROP_VALUE_PATTERN_BRUSH);
			comboBrushType.Items.Add(Resources.GORFNT_PROP_VALUE_TEXTURE_BRUSH);
	    }

		/// <summary>
		/// Function to update the solid brush color settings.
		/// </summary>
	    private void DrawSolid()
		{
			try
			{
				numericRed.ValueChanged -= NumericValueUpdated;
				numericGreen.ValueChanged -= NumericValueUpdated;
				numericBlue.ValueChanged -= NumericValueUpdated;
				numericAlpha.ValueChanged -= NumericValueUpdated;
				panelColor.ValueChanged -= panelColor_ValueChanged;
				sliderColor.ValueChanged -= panelColor_ValueChanged;
				sliderAlpha.ValueChanged -= panelColor_ValueChanged;

				Color color = Color.FromArgb((int)(sliderAlpha.ValuePercentual * 255.0f).FastFloor(),
											 ExtMethodsSystemDrawingColor.ColorFromHSV(sliderColor.ValuePercentual,
																					   panelColor.ValuePercentual.X,
																					   panelColor.ValuePercentual.Y));

				boxCurrentColor.LowerColor = color;
				panelColor.TopRightColor = Color.FromArgb(sliderAlpha.Value.A, sliderColor.Value);
				panelColor.TopLeftColor = Color.FromArgb(sliderAlpha.Value.A, Color.White);
				panelColor.BottomLeftColor = Color.FromArgb(sliderAlpha.Value.A, Color.Black);
				panelColor.BottomRightColor = Color.FromArgb(sliderAlpha.Value.A, Color.Black);
				panelColor.ValuePercentual = new PointF(color.GetHSVSaturation(), color.GetHSVBrightness());


				numericRed.Value = color.R;
				numericGreen.Value = color.G;
				numericBlue.Value = color.B;
				numericAlpha.Value = color.A;
			}
			finally
			{
				numericRed.ValueChanged += NumericValueUpdated;
				numericGreen.ValueChanged += NumericValueUpdated;
				numericBlue.ValueChanged += NumericValueUpdated;
				numericAlpha.ValueChanged += NumericValueUpdated;
				panelColor.ValueChanged += panelColor_ValueChanged;
				sliderColor.ValueChanged += panelColor_ValueChanged;
				sliderAlpha.ValueChanged += panelColor_ValueChanged;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the tabBrushEditor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void tabBrushEditor_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (tabBrushEditor.SelectedTab == pageSolid)
				{
					numericRed.ValueChanged -= NumericValueUpdated;
					numericGreen.ValueChanged -= NumericValueUpdated;
					numericBlue.ValueChanged -= NumericValueUpdated;
					numericAlpha.ValueChanged -= NumericValueUpdated;
					panelColor.ValueChanged -= panelColor_ValueChanged;
					sliderColor.ValueChanged -= panelColor_ValueChanged;
					sliderAlpha.ValueChanged -= panelColor_ValueChanged;

					DrawSolid();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
	    protected override void OnFormClosing(FormClosingEventArgs e)
	    {
		    base.OnFormClosing(e);

			numericRed.ValueChanged -= NumericValueUpdated;
			numericGreen.ValueChanged -= NumericValueUpdated;
			numericBlue.ValueChanged -= NumericValueUpdated;
			numericAlpha.ValueChanged -= NumericValueUpdated;
			panelColor.ValueChanged -= panelColor_ValueChanged;
			sliderColor.ValueChanged -= panelColor_ValueChanged;
			sliderAlpha.ValueChanged -= panelColor_ValueChanged;

			if (_defaultTextureBrush == null)
			{
				return;
			}

			_defaultTextureBrush.Dispose();
			_defaultTextureBrush = null;
	    }

		/// <summary>
		/// Handles the ValueChanged event of the panelColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelColor_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				DrawSolid();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function called when the numeric value is updated for the solid color selector.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
	    private void NumericValueUpdated(object sender, EventArgs e)
		{
			try
			{

				Color newColor = Color.FromArgb((int)numericAlpha.Value,
				                                (int)numericRed.Value,
				                                (int)numericGreen.Value,
				                                (int)numericBlue.Value);

				panelColor.ValuePercentual = new PointF(newColor.GetHSVSaturation(), newColor.GetHSVBrightness());
				sliderAlpha.ValuePercentual = newColor.A / 255.0f;
				sliderColor.ValuePercentual = newColor.GetHSVHue();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

	    /// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	    protected override void OnLoad(EventArgs e)
	    {
		    base.OnLoad(e);

			Localize();

		    switch (BrushType)
		    {
				case GlyphBrushType.Solid:
				    Color brushColor = SolidBrush.Color;

				    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_SOLID_BRUSH;
				    tabBrushEditor.SelectedTab = pageSolid;
					boxCurrentColor.LowerColor = boxCurrentColor.UpperColor = SolidBrush.Color;
				    sliderAlpha.ValuePercentual = SolidBrush.Color.Alpha;
				    sliderColor.ValuePercentual = brushColor.GetHSVHue();
					panelColor.ValuePercentual = new PointF(brushColor.GetHSVSaturation(), brushColor.GetHSVBrightness());
					DrawSolid();
				    break;
				case GlyphBrushType.LinearGradient:
					comboBrushType.Text = Resources.GORFNT_PROP_VALUE_GRADIENT_BRUSH;
				    break;
				case GlyphBrushType.Hatched:
				    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_PATTERN_BRUSH;
				    break;
				case GlyphBrushType.Texture:
				    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_TEXTURE_BRUSH;
					tabBrushEditor.SelectedTab = pageTexture;
				    break;
		    }
	    }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="formBrushEditor"/> class.
        /// </summary>
        public formBrushEditor()
        {
            InitializeComponent();
			_defaultTextureBrush = new GorgonGlyphTextureBrush(Resources.Pattern);
			SolidBrush = new GorgonGlyphSolidBrush();
	        GradientBrush = new GorgonGlyphLinearGradientBrush
	                        {
		                        Angle = 0.0f,
		                        StartColor = GorgonColor.Black,
		                        EndColor = GorgonColor.White,
		                        GammaCorrection = false,
		                        GradientRegion = new RectangleF(0, 0, 64, 64)
	                        };
	        PatternBrush = new GorgonGlyphHatchBrush
	                       {
		                       ForegroundColor = GorgonColor.White,
		                       BackgroundColor = GorgonColor.Black,
		                       HatchStyle = HatchStyle.BackwardDiagonal
	                       };
        }
        #endregion
    }
}
