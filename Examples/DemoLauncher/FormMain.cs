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
// Created: Monday, August 26, 2013 10:57:50 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.UI;
using KRBTabControl;

namespace Gorgon.Examples
{
	/// <summary>
	/// Main interface.
	/// </summary>
	public partial class FormMain 
		: FlatForm
	{
		#region Variables.
		// List of categories.
		private CategoryCollection _categories;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a panel.
		/// </summary>
		/// <param name="parent">Parent panel.</param>
		/// <param name="example">Example data to display.</param>
		private static void AddPanel(Panel parent, Example example)
		{
			var testPanel = new ExamplePanel
			                {
								Example = example,
								Left = -1,
				                Width = parent.ClientSize.Width + 2,
				                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
				                Text = example.Text
			                };

			// Get last height.
			if (parent.Controls.Count > 0)
			{
				testPanel.Top = parent.Controls[parent.Controls.Count - 1].Bottom - 1;
			}
			else
			{
				testPanel.Top = -1;//(example.Index * (testPanel.Height - 1)) - 1;	
			}
			

			parent.Controls.Add(testPanel);
		}

		/// <summary>
		/// Function to add a category tab.
		/// </summary>
		/// <param name="category">Category to use.</param>
		/// <returns>The panel that will contain the example panels.</returns>
		private Panel AddCategory(Category category)
		{
			if ((category.Examples == null)
			    || (category.Examples.Count == 0))
			{
				return null;
			}

			var page = new TabPageEx(category.Name)
			           {
				           IsClosable = false,
						   Name = "page" + category.Name
			           };

			var panel = new Panel
			            {
				            Name = "panel" + category.Name,
				            BackColor = Color.White,
							Dock = DockStyle.Fill,
							BorderStyle = BorderStyle.FixedSingle,
							AutoScroll = true
			            };

			page.Controls.Add(panel);
			tabCategories.TabPages.Add(page);

			return panel;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_categories != null)
			{
				_categories.Dispose();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			//BorderColor = BackColor;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			//BorderColor = Color.Gray;
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
				_categories = CategoryCollection.Read();

				tabCategories.TabPages.Clear();

				// Add categories and examples.
				foreach (Category category in _categories)
				{
					Panel panel = AddCategory(category);

					if (panel == null)
					{
						continue;
					}

					foreach (Example example in category.Examples)
					{
						AddPanel(panel, example);
					}
				}
			}
			catch (Exception ex)
			{
				if (_categories != null)
				{
					_categories.Dispose();
				}

				GorgonDialogs.ErrorBox(this, ex);
				Application.Exit();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		public FormMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}
