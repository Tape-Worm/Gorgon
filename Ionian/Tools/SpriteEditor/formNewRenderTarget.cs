#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, May 08, 2007 9:58:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface used to create a new render target.
	/// </summary>
	public partial class formNewRenderTarget 
		: Form
	{
		#region Variables.
		private bool _editMode;			// Flag to indicate that we're editing.
		#endregion

		#region Classes.
		/// <summary>
		/// Object representing the sorting method for the format combo.
		/// </summary>
		private class FormatSorter
			: IComparer<string>
		{
			#region IComparer<string> Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
			/// </returns>
			public int Compare(string x, string y)
			{
				return -string.Compare(x, y, true);
			}
			#endregion
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the render target name.
		/// </summary>
		public string RenderTargetName
		{
			get
			{
				return textName.Text;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the form controls.
		/// </summary>
		private void ValidateForm()
		{
			ImageBufferFormats format;		// Format.

			if (_editMode)
				textName.Enabled = false;

			if ((textName.Text == string.Empty) || (comboFormats.Text == string.Empty))
				buttonOK.Enabled = false;
			else
				buttonOK.Enabled = true;

			if (comboFormats.Text != string.Empty)
			{
				format = (ImageBufferFormats)Enum.Parse(typeof(ImageBufferFormats), comboFormats.Text);
				// Validate the check boxes.
				if (Gorgon.CurrentDriver.DepthBufferAvailable(format))
					checkUseDepthBuffer.Enabled = true;
				else
				{
					checkUseDepthBuffer.Checked = false;
					checkUseDepthBuffer.Enabled = false;
				}
			}
			else
			{
				checkUseStencilBuffer.Checked = false;
				checkUseStencilBuffer.Enabled = false;
				checkUseDepthBuffer.Checked = false;
				checkUseDepthBuffer.Enabled = false;
			}
		}

		/// <summary>
		/// Function to fill the format list.
		/// </summary>
		private void FillFormats()
		{
			string[] formats = null;						// List of formats.
			SortedList<string, string> formatList = null;	// Sorted format list.
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				comboFormats.Items.Clear();

				// Get format list.
				formats = Enum.GetNames(typeof(ImageBufferFormats));
				formatList = new SortedList<string, string>(new FormatSorter());

				// Add to the list.
				var validFormats = from formatEnum in formats
								   where (string.Compare(formatEnum, "bufferunknown", true) != 0) &&
										(Image.SupportsFormat((ImageBufferFormats)Enum.Parse(typeof(ImageBufferFormats), formatEnum), ImageType.RenderTarget))
								   select formatEnum;				
				foreach (string format in validFormats)
					comboFormats.Items.Add(format);

				// Get setting.
				Settings.Root = null;
				comboFormats.Text = Settings.GetSetting("LastRenderTargetFormat", string.Empty);

				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to retrieve render target format list.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFormats control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFormats_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			int width = 0;					// Width of the image.
			int height = 0;					// Height of the image.
			ImageBufferFormats format;		// Format of the image.
			RenderImage target = null;		// Render target.

			try
			{
				width = Convert.ToInt32(numericWidth.Value);
				height = Convert.ToInt32(numericHeight.Value);
				format = (ImageBufferFormats)Enum.Parse(typeof(ImageBufferFormats), comboFormats.Text);

				// Check for duplicate name.
				if (!_editMode)
				{
					if (RenderTargetCache.Targets.Contains(textName.Text))
					{
						UI.ErrorBox(this, "A render target with the name '" + textName.Text + "' already exists.");
						DialogResult = DialogResult.None;
						return;
					}

					// Create the render target.
					target = new RenderImage(textName.Text, width, height, format, checkUseDepthBuffer.Checked, checkUseStencilBuffer.Checked);
				}
				else
				{
					RenderImage image = null;		// Render target.

					image = RenderTargetCache.Targets[textName.Text] as RenderImage;

					// Update the target.
					if (image != null)
						image.SetDimensions(width, height, format);
				}


				Settings.Root = null;
				Settings.SetSetting("LastRenderTargetFormat", comboFormats.Text);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error creating the render target.", ex);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkUseDepthBuffer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkUseDepthBuffer_CheckedChanged(object sender, EventArgs e)
		{
			// Allow us to check the stencil buffer.
			if (checkUseDepthBuffer.Checked)
			{
				if (Gorgon.CurrentDriver.StencilBufferAvailable((ImageBufferFormats)Enum.Parse(typeof(ImageBufferFormats), comboFormats.Text)))
					checkUseStencilBuffer.Enabled = true;
				else
				{
					checkUseStencilBuffer.Checked = false;
					checkUseStencilBuffer.Enabled = false;
				}
			}
			else
			{
				checkUseStencilBuffer.Checked = false;
				checkUseStencilBuffer.Enabled = false;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the formNewRenderTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formNewRenderTarget_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Enter) && (buttonOK.Enabled))
			{
				buttonOK_Click(this, EventArgs.Empty);
				DialogResult = DialogResult.OK;
			}

			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Function to edit a render target.
		/// </summary>
		/// <param name="target">Target to edit.</param>
		public void EditTarget(string target)
		{
			RenderImage image = null;		// Render image.

			if (!RenderTargetCache.Targets.Contains(target)) 
			{
				UI.ErrorBox(this, "The render target '" + target + "' does not exist.");
				return;
			}

			// Check type.
			image = RenderTargetCache.Targets[target] as RenderImage;
			if (image == null)
			{
				UI.ErrorBox(this, "The render target '" + target + "' is not a render image.");
				return;
			}

			// Get data.
			textName.Text = image.Name;
			numericHeight.Value = image.Height;
			numericWidth.Value = image.Width;
			comboFormats.Text = image.Format.ToString();
			checkUseDepthBuffer.Checked = image.UseDepthBuffer;
			checkUseStencilBuffer.Checked = image.UseStencilBuffer;
			Text = "Edit render target - " + image.Name;
			_editMode = true;

			ValidateForm();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formNewRenderTarget()
		{
			InitializeComponent();

			FillFormats();
		}
		#endregion
	}
}