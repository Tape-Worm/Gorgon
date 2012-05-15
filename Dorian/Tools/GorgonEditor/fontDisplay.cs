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
// Created: Monday, May 14, 2012 8:30:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font editor.
	/// </summary>
	public partial class fontDisplay : UserControl
	{
		#region Variables.
		private GorgonFont _font = null;
		private float _zoom = -1;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current texture index.
		/// </summary>
		public int CurrentTexture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the zoom value.
		/// </summary>
		public float Zoom
		{
			get
			{
				if (_zoom == -1)
				{
					Vector2 zoomValue = Vector2.Zero;

					zoomValue = _font.Textures[CurrentTexture].Settings.Size;

					if (panelDisplay.ClientSize.Width != 0)
						zoomValue.X = panelDisplay.ClientSize.Width / zoomValue.X;
					else
						zoomValue.X = 1e-6f;
					if (panelDisplay.ClientSize.Height != 0)
						zoomValue.Y = panelDisplay.ClientSize.Height / zoomValue.Y;
					else
						zoomValue.Y = 1e-6f;

					return (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;
				}

				return _zoom;
			}
			set
			{
				_zoom = value;
			}
		}

		/// <summary>
		/// Property to set or return the font that we're editing.
		/// </summary>
		public GorgonFont CurrentFont
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
				ValidateCommands();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the itemZoom100 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemZoom100_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			float zoomFactor = 0;

			try
			{
				var menuItems = from toolItem in item.GetCurrentParent().Items.Cast<ToolStripItem>()
								let menuItem = toolItem as ToolStripMenuItem
								where menuItem != null && menuItem != item
								select menuItem;

				foreach (var menuItem in menuItems)
					menuItem.Checked = false;

				item.Checked = true;
				zoomFactor = float.Parse(item.Tag.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);
				if (zoomFactor > 0)
					Zoom = zoomFactor / 100.0f;
				else
					Zoom = -1;

				itemZoom.Text = "Zoom: " + item.Text;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonNext control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNext_Click(object sender, EventArgs e)
		{
			CurrentTexture++;
			ValidateCommands();
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevious control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPrevious_Click(object sender, EventArgs e)
		{
			CurrentTexture--;
			ValidateCommands();
		}

		/// <summary>
		/// Function to validate the command buttons.
		/// </summary>
		private void ValidateCommands()
		{
			if (CurrentTexture >= _font.Textures.Count)
				CurrentTexture = _font.Textures.Count - 1;
			if (CurrentTexture < 0)
				CurrentTexture = 0;

			buttonNext.Enabled = (CurrentTexture < _font.Textures.Count - 1);
			buttonPrevious.Enabled = (CurrentTexture > 0);

			labelTextureCounter.Text = "Texture: " + (CurrentTexture + 1).ToString() + "/" + _font.Textures.Count.ToString();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);
						
			stripFont.Renderer = new MetroDarkRenderer();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="fontDisplay"/> class.
		/// </summary>
		public fontDisplay()
		{
			InitializeComponent();
		}
		#endregion
	}
}
