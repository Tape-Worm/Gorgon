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
// Created: Thursday, March 07, 2013 9:51:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Control for displaying font data.
    /// </summary>
    partial class GorgonFontContentPanel 
		: ContentPanel
    {
        #region Variables.
		private float _currentZoom = -1;
		private int _currentTextureIndex = 0;
		private GorgonFontContent _content = null;        
        #endregion

        #region Properties.
		
        #endregion

        #region Methods.
		/// <summary>
		/// Function to perform validation of the form controls.
		/// </summary>
		private void ValidateControls()
		{
            if (_content.Font != null)
            {
                labelPrevTexture.Enabled = _currentTextureIndex > 0;
                labelNextTexture.Enabled = _currentTextureIndex < _content.Font.Textures.Count - 1;

                labelTextureCount.Text = string.Format("Texture: {0}/{1}", _currentTextureIndex + 1, _content.Font.Textures.Count);
            }
            else
            {
                labelPrevTexture.Enabled = false;
                labelNextTexture.Enabled = false;
                labelTextureCount.Text = "Texture: N/A";
            }
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelNextTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelNextTexture_MouseEnter(object sender, EventArgs e)
		{
			labelNextTexture.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseMove event of the labelNextTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelNextTexture_MouseMove(object sender, MouseEventArgs e)
		{
			labelNextTexture.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelNextTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelNextTexture_MouseLeave(object sender, EventArgs e)
		{
			labelNextTexture.ForeColor = Color.Silver;
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelPrevTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelPrevTexture_MouseEnter(object sender, EventArgs e)
		{
			labelPrevTexture.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseMove event of the labelPrevTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelPrevTexture_MouseMove(object sender, MouseEventArgs e)
		{
			labelPrevTexture.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelPrevTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelPrevTexture_MouseLeave(object sender, EventArgs e)
		{
			labelPrevTexture.ForeColor = Color.Silver;
		}

		/// <summary>
		/// Handles the MouseMove event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseMove(object sender, MouseEventArgs e)
		{
			var pos = panelToolbar.PointToClient(Cursor.Position);

			if (panelToolbar.DisplayRectangle.Contains(pos))
			{
				panelToolbar.BringToFront();
				panelToolbar.Enabled = true;
			}
			else
			{
				if (panelToolbar.Enabled)
				{
					panelToolbar.SendToBack();
					panelToolbar.Enabled = false;
				}
			}
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void OnContentChanged()
		{
			base.OnContentChanged();

			_content = Content as GorgonFontContent;

			ValidateControls();
		}

		/// <summary>
		/// Function to draw the font texture.
		/// </summary>
		public void DrawFontTexture()
		{
			GorgonTexture2D currentTexture = _content.Font.Textures[_currentTextureIndex];
			float scale = 1.0f;

			if (_currentZoom == -1.0f)
			{
				Vector2 zoomValue = currentTexture.Settings.Size;

				if (panelTextures.ClientSize.Width != 0)
				{
					zoomValue.X = panelTextures.ClientSize.Width / zoomValue.X;
				}
				else
				{
					zoomValue.X = 1e-6f;
				}

				if (panelTextures.ClientSize.Height != 0)
				{
					zoomValue.Y = panelTextures.ClientSize.Height / zoomValue.Y;
				}
				else
				{
					zoomValue.Y = 1e-6f;
				}

				scale = (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;
			}

			RectangleF region = new RectangleF(panelTextures.ClientSize.Width / 2.0f -(currentTexture.Settings.Width * scale) / 2.0f,
												panelTextures.ClientSize.Height / 2.0f - (currentTexture.Settings.Height * scale) / 2.0f,
												currentTexture.Settings.Width * scale, currentTexture.Settings.Height * scale);

			// Fill the background so we can clearly see the first page.
			_content.Renderer.Drawing.FilledRectangle(region, Color.Black);
			_content.Renderer.Drawing.Blit(currentTexture, region);
		}		
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentPanel"/> class.
        /// </summary>
        public GorgonFontContentPanel()
        {
            InitializeComponent();

			// Move the toolbar to the rear.
			panelToolbar.SendToBack();
        }
        #endregion
    }
}
