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
// Created: Monday, August 07, 2006 2:10:03 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Form that edits the font backing image size.
	/// </summary>
	public partial class ImageSize : Form
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the size of the font image.
		/// </summary>
		public Size FontImageSize
		{
			get
			{
				return new Size((int)numericImageWidth.Value, (int)numericImageHeight.Value);
			}
			set
			{
				numericImageWidth.Value = (decimal)value.Width;
				numericImageHeight.Value = (decimal)value.Height;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the ImageSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void ImageSize_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				DialogResult = DialogResult.OK;
				e.Handled = true;
			}

			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				e.Handled = true;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageSize()
		{
			InitializeComponent();
		}
		#endregion
	}
}