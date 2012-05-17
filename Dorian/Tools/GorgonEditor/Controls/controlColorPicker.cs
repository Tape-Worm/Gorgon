#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, May 15, 2012 3:28:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Control to be displayed for picking colors on the property grid drop down.
	/// </summary>
	partial class ColorPickerControl : UserControl
	{
		/// <summary>
		/// Property to set or return the editor service.
		/// </summary>
		public System.Windows.Forms.Design.IWindowsFormsEditorService EditorService
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color.
		/// </summary>
		public Color CurrentColor
		{
			get
			{

				return Color.FromArgb((int)(System.Math.Floor(sliderAlpha.ValuePercentual * 255.0f)), ExtMethodsSystemDrawingColor.ColorFromHSV(sliderColor.ValuePercentual, panelColor.ValuePercentual.X, panelColor.ValuePercentual.Y));
			}
			set
			{
				sliderColor.ValuePercentual = value.GetHSVHue();
				sliderAlpha.ValuePercentual = value.A / 255.0f;
				panelColor.ValuePercentual = new PointF(value.GetHSVSaturation(), value.GetHSVBrightness());
			}
		}
		
		/// <summary>
		/// Function to return the 
		/// </summary>
		/// <param name="color">Color to retrieve from.</param>
		/// <returns>The brightness of the color</returns>
		private float GetBrightness(Color color)
		{
			return System.Math.Max(System.Math.Max(color.R, color.G), color.B) / 255.0f;
		}

		/// <summary>
		/// Function to retrieve the color saturation.
		/// </summary>
		/// <param name="color">Color to retrieve from.</param>
		/// <returns>The saturation of the color.</returns>
		private float GetSaturation(Color color)
		{
			int max = System.Math.Max(color.R, System.Math.Max(color.G, color.B));
			int min = System.Math.Min(color.R, System.Math.Min(color.G, color.B));

			return (max == 0) ? 0.0f : 1.0f - (1.0f * (float)min / (float)max);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorPickerControl"/> class.
		/// </summary>
		public ColorPickerControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the ValueChanged event of the sliderColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void sliderColor_ValueChanged(object sender, EventArgs e)
		{
			Color current = CurrentColor;

			panelColor.TopRightColor = Color.FromArgb(sliderAlpha.Value.A, sliderColor.Value);
			panelColor.TopLeftColor = Color.FromArgb(sliderAlpha.Value.A, Color.White);
			panelColor.BottomLeftColor = Color.FromArgb(sliderAlpha.Value.A, Color.Black);
			panelColor.BottomRightColor = Color.FromArgb(sliderAlpha.Value.A, Color.Black);

			labelColorInfo.Text = string.Format("R:{0}, G:{1}, B:{2}, A:{3}", current.R, current.G, current.B, current.A);
			labelColorInfo.Refresh();
		}

		/// <summary>
		/// Handles the Click event of the buttonExpando control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonExpando_Click(object sender, EventArgs e)
		{
			ColorPickerDialog picker = null;

			try
			{
				picker = new ColorPickerDialog();
				picker.OldColor = CurrentColor;
				if (picker.ShowDialog() == DialogResult.OK)
					CurrentColor = picker.SelectedColor;

				EditorService.CloseDropDown();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}
	}
}
