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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Control to be displayed for picking colors on the property grid drop down.
	/// </summary>
	partial class ControlColorPicker : UserControl
	{
		/// <summary>
		/// Property to set or return the editor service.
		/// </summary>
		public IWindowsFormsEditorService EditorService
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to edit the only the alpha channel.
		/// </summary>
		public bool AlphaOnly
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
		/// Initializes a new instance of the <see cref="ControlColorPicker"/> class.
		/// </summary>
		public ControlColorPicker()
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

			if (!AlphaOnly)
				labelColorInfo.Text = string.Format("R:{0}, G:{1}, B:{2}, A:{3}", current.R, current.G, current.B, current.A);
			else
				labelColorInfo.Text = string.Format("Alpha: {0}", current.A);
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
				picker = new ColorPickerDialog
				         {
				             OldColor = CurrentColor
				         };
			    if (picker.ShowDialog() == DialogResult.OK)
			    {
			        CurrentColor = picker.SelectedColor;
			    }

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

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				if (AlphaOnly)
				{
					panelColor.Visible = false;
					sliderColor.Visible = false;
					buttonExpando.Visible = false;
					sliderAlpha.Location = panelColor.Location;
					sliderAlpha.Width = 64;
					sliderAlpha.Height = panelColor.Height;					
					Width = 64 + (panelColor.Left * 2);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}
	}
}
