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
// Created: Tuesday, September 23, 2014 1:01:14 AM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Math;

namespace Gorgon.Examples
{
	/// <summary>
	/// Panel to display information abotu an example.
	/// </summary>
	public partial class ExamplePanel 
		: UserControl
	{
		#region Variables.
		// Example for the panel.
		private Example _example;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the example for the panel.
		/// </summary>
		internal Example Example
		{
			get
			{
				return _example;
			}
			set
			{
				_example = value;

				if (value == null)
				{
					pictureIcon.Image = Resources.Default_128x128;
					labelCaption.Text = string.Empty;
					labelText.Text = string.Empty;
					Enabled = false;
					return;
				}

				Enabled = true;

				pictureIcon.Image = value.Icon;
				labelCaption.Text = value.Name;
				labelText.Text = value.Text;

				Height = (labelText.Height + labelCaption.Height).Max(pictureIcon.Height);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ParentChanged event of the ExamplePanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ExamplePanel_ParentChanged(object sender, EventArgs e)
		{
			Parent.Resize += (o, args) =>
			{
				if (((ScrollableControl)Parent).VerticalScroll.Visible)
				{
					Width = Parent.ClientSize.Width + 1;
				}
				else
				{
					Width = Parent.ClientSize.Width + 2;
				}
			};
		}

		/// <summary>
		/// Handles the MouseDown event of the pictureIcon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void pictureIcon_MouseDown(object sender, MouseEventArgs e)
		{
			OnMouseDown(e);
		}

		/// <summary>
		/// Handles the MouseUp event of the pictureIcon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void pictureIcon_MouseUp(object sender, MouseEventArgs e)
		{
			OnMouseUp(e);
		}

		/// <summary>
		/// Handles the MouseEnter event of the panelText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelText_MouseEnter(object sender, EventArgs e)
		{
			OnMouseEnter(e);
		}

		/// <summary>
		/// Handles the MouseLeave event of the panelText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelText_MouseLeave(object sender, EventArgs e)
		{
			OnMouseLeave(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			pictureIcon.BackColor = Color.AliceBlue;
			labelCaption.BackColor = Color.Cornsilk;
			panel2.BackColor = panelText.BackColor = labelText.BackColor = Color.AliceBlue;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			pictureIcon.BackColor = Color.White;
			labelCaption.BackColor = Color.FromArgb(243, 243, 243);
			panel2.BackColor = panelText.BackColor = labelText.BackColor = Color.White;
		}

		/// <summary>
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			pictureIcon.BackColor = Color.Bisque;
			labelCaption.BackColor = Color.Cornsilk;
			panel2.BackColor = panelText.BackColor = labelText.BackColor = Color.Bisque;
		}

		/// <summary>
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			
			labelText.MaximumSize = new Size(panelText.ClientSize.Width, 0);
			Height = (labelText.Height + labelCaption.Height).Max(pictureIcon.Height);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (ParentForm == null)
			{
				return;
			}

			if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
			{
				OnMouseEnter(EventArgs.Empty);

				if (_example == null)
				{
					return;
				}

			    string exeDirectory = Path.GetDirectoryName(_example.ExePath);

			    if (string.IsNullOrEmpty(exeDirectory))
			    {
			        exeDirectory = Application.StartupPath;
			    }

				var processInfo = new ProcessStartInfo(_example.ExePath)
				                  {
				                      WorkingDirectory = exeDirectory
				                  };
			    Process process = Process.Start(processInfo);

				if (process == null)
				{
					return;
				}

				try
				{
					ParentForm.Visible = false;
					process.WaitForExit();
				}
				finally
				{
					process.Close();
					ParentForm.Visible = true;
				}

				return;
			}

			pictureIcon.BackColor = Color.White;
			labelCaption.BackColor = Color.FromArgb(243, 243, 243);
			panel2.BackColor = panelText.BackColor = labelText.BackColor = Color.White;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ExamplePanel"/> class.
		/// </summary>
		public ExamplePanel()
		{
			InitializeComponent();
		}
		#endregion
	}
}
