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
// Created: Tuesday, April 24, 2007 2:56:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form to add or edit comments.
	/// </summary>
	public partial class formComment : Form
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the comment.
		/// </summary>
		public string Comment
		{
			get
			{
				return textComment.Text;
			}
			set
			{
				textComment.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the textComment control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textComment_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Cancel)
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
		public formComment()
		{
			InitializeComponent();
		}
		#endregion
	}
}