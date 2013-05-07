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
// Created: Sunday, April 7, 2013 5:26:29 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A form to handle picking an alpha value for a color.
	/// </summary>
	public partial class formAlphaPicker 
		: Form
	{
		#region Variables.
		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the currently selected alpha value.
		/// </summary>
		public float SelectedAlphaValue
		{
			get
			{
				return sliderAlpha.ValuePercentual;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				if (value > 1)
				{
					value = 1;
				}

				sliderAlpha.ValuePercentual = value;
				numericAlphaValue.Value = System.Math.Ceiling((decimal)(value * 255.0f));
				colorPreview.UpperColor = Color.FromArgb((int)numericAlphaValue.Value, Color.White);
				colorPreview.LowerColor = Color.FromArgb((int)numericAlphaValue.Value, Color.White);
				buttonOK.Enabled = true;
			}
		}
		#endregion

		#region Methods.		
		/// <summary>
		/// Handles the ValueChanged event of the numericAlphaValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericAlphaValue_ValueChanged(object sender, EventArgs e)
		{
			sliderAlpha.ValuePercentual = (float)(numericAlphaValue.Value) / 255.0f;
			colorPreview.LowerColor = Color.FromArgb(sliderAlpha.Value.A, Color.White);
		}

		/// <summary>
		/// Handles the ValueChanged event of the sliderAlpha control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void sliderAlpha_ValueChanged(object sender, EventArgs e)
		{
			numericAlphaValue.Value = sliderAlpha.Value.A;
			colorPreview.LowerColor = Color.FromArgb(sliderAlpha.Value.A, Color.White);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formAlphaPicker"/> class.
		/// </summary>
		public formAlphaPicker()
		{
			InitializeComponent();
		}
		#endregion
	}
}
