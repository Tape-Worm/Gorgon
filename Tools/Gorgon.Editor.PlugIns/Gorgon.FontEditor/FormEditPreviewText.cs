﻿#region MIT.
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
// Created: Sunday, April 6, 2014 9:43:26 PM
// 
#endregion

using System;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Text editor window for the preview text.
	/// </summary>
	public partial class FormEditPreviewText 
		: FlatForm
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the preview text.
		/// </summary>
		public string PreviewText
		{
			get
			{
				return textPreview.Text;
			}
			set
			{
				textPreview.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the TextChanged event of the textPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textPreview_TextChanged(object sender, EventArgs e)
		{
			buttonOK.Enabled = textPreview.TextLength > 0;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormEditPreviewText"/> class.
		/// </summary>
		public FormEditPreviewText()
		{
			InitializeComponent();
		}
		#endregion
	}
}
