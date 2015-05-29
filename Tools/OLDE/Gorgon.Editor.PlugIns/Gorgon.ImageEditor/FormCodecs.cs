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
// Created: Thursday, November 6, 2014 9:01:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditorPlugIn
{
	/// <summary>
	/// Main UI for the codec loader.
	/// </summary>
	partial class FormCodecs 
		: FlatForm
	{
		#region Variables.
		
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to fill the list with the codec plug-ins.
		/// </summary>
		private void FillList()
		{
			listCodecs.BeginUpdate();

			try
			{
				listCodecs.Items.Clear();

				IEnumerable<GorgonCodecPlugIn> plugIns = GorgonApplication.PlugIns.OfType<GorgonCodecPlugIn>().OrderBy(item => item.Name);

				foreach (GorgonCodecPlugIn codecPlugIn in plugIns)
				{
					var codecItem = new ListViewItem
					                {
						                Text = codecPlugIn.Name,
						                Name = codecPlugIn.Name,
										Tag = codecPlugIn
					                };

					codecItem.SubItems.Add(codecPlugIn.Description);
					codecItem.SubItems.Add(codecPlugIn.PlugInPath);

					listCodecs.Items.Add(codecItem);
				}
			}
			finally
			{
				listCodecs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				listCodecs.EndUpdate();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillList();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				Close();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormCodecs"/> class.
		/// </summary>
		public FormCodecs()
		{
			InitializeComponent();
		}
		#endregion
	}
}
