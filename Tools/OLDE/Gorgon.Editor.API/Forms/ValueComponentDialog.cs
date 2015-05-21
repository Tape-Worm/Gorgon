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
// Created: Sunday, April 7, 2013 8:56:26 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Editor dialog for editing vector/point values.
	/// </summary>
	public partial class ValueComponentDialog 
		: FlatForm
	{
		#region Variables.
		private int _valueComponents = 1;
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when a value has changed.
		/// </summary>
		public event EventHandler ValueChanged;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the maximum value for the value components.
		/// </summary>
		public decimal MaxValue
		{
			get
			{
				return numericUpDown1.Maximum;
			}
			set
			{
				numericUpDown1.Maximum = numericUpDown2.Maximum = numericUpDown3.Maximum = numericUpDown4.Maximum = value;
			}
		}

		/// <summary>
		/// Property to set or return the minimum value for the value components.
		/// </summary>
		public decimal MinValue
		{
			get
			{
				return numericUpDown1.Minimum;
			}
			set
			{
				numericUpDown1.Minimum = numericUpDown2.Minimum = numericUpDown3.Minimum = numericUpDown4.Minimum = value;
			}
		}

		/// <summary>
		/// Property to set or return the number of decimal places for the value(s).
		/// </summary>
		public int DecimalPlaces
		{
			get
			{
				return numericUpDown1.DecimalPlaces;
			}
			set
			{
				numericUpDown1.DecimalPlaces = numericUpDown2.DecimalPlaces = numericUpDown3.DecimalPlaces = numericUpDown4.DecimalPlaces = value;
			}
		}

		/// <summary>
		/// Property to set or return the first value component value.
		/// </summary>
		public decimal Value1
		{
			get
			{
				return numericUpDown1.Value;
			}
			set
			{
				numericUpDown1.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the second value component value.
		/// </summary>
		public decimal Value2
		{
			get
			{
				return numericUpDown2.Value;
			}
			set
			{
				numericUpDown2.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the third value component value.
		/// </summary>
		public decimal Value3
		{
			get
			{
				return numericUpDown3.Value;
			}
			set
			{
				numericUpDown3.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the fourth value component value.
		/// </summary>
		public decimal Value4
		{
			get
			{
				return numericUpDown4.Value;
			}
			set
			{
				numericUpDown4.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the number of value components.
		/// </summary>
		public int ValueComponents
		{
			get
			{
				return _valueComponents;
			}
			set
			{
				if (value < 1)
				{
					value = 1;
				}
				if (value > 4)
				{
					value = 4;
				}

				numericUpDown1.Visible = true;

				labelComma1.Visible = numericUpDown2.Visible = (value > 1);
				labelComma2.Visible = numericUpDown3.Visible = (value > 2);
				labelComma3.Visible = numericUpDown4.Visible = (value > 3);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ValueChanged event of the numericUpDown1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, e);
			}
		}

		/// <summary>
		/// Handles the Enter event of the numericUpDown1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericUpDown1_Enter(object sender, EventArgs e)
		{
			var control = sender as NumericUpDown;

			if (control == null)
			{
				return;
			}

			control.Select(0, control.Text.Length);
		}

        /// <summary>
        /// Function to localize the form.
        /// </summary>
	    private void LocalizeControls()
        {
            labelValueCaption.Text = string.Format("{0}:", APIResources.GOREDIT_TEXT_VALUE);
            buttonOK.Text = APIResources.GOREDIT_ACC_TEXT_OK;
            buttonCancel.Text = APIResources.GOREDIT_ACC_TEXT_CANCEL;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	    protected override void OnLoad(EventArgs e)
	    {
	        base.OnLoad(e);

            LocalizeControls();
	    }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueComponentDialog"/> class.
		/// </summary>
		public ValueComponentDialog()
		{
			InitializeComponent();
		}
		#endregion
	}
}
