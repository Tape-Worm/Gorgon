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
		private GorgonText _text = null;                            
		private StringBuilder _sampleText = new StringBuilder(256); 		
        #endregion

        #region Properties.
		
        #endregion

        #region Methods.
		/// <summary>
		/// Function to perform validation of the form controls.
		/// </summary>
		private void ValidateControls()
		{
            if ((_content != null) && (_content.Font != null))
            {
				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _content.Font.Textures.Count)
				{
					_currentTextureIndex = _content.Font.Textures.Count - 1;
				}

				buttonPrevTexture.Enabled = _currentTextureIndex > 0;
				buttonNextTexture.Enabled = _currentTextureIndex < _content.Font.Textures.Count - 1;

                labelTextureCount.Text = string.Format("Texture: {0}/{1}", _currentTextureIndex + 1, _content.Font.Textures.Count);
            }
            else
            {
				buttonPrevTexture.Enabled = false;
				buttonNextTexture.Enabled = false;
                labelTextureCount.Text = "Texture: N/A";
            }
		}

		/// <summary>
		/// Handles the Resize event of the panelText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelText_Resize(object sender, EventArgs e)
		{
			if (_text != null)
			{
				_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonNextTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextTexture_Click(object sender, EventArgs e)
		{
			try
			{
				_currentTextureIndex++;
				ActiveControl = panelTextures;
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevTexture_Click(object sender, EventArgs e)
		{
			try
			{
				_currentTextureIndex--;
				ActiveControl = panelTextures;
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the zoomItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void zoomItem_Click(object sender, EventArgs e)
		{
			try
			{
				var items = dropDownZoom.DropDownItems.Cast<ToolStripItem>().Where(item => item is ToolStripMenuItem);
					
				foreach (ToolStripMenuItem control in items)
				{
					if (control != sender)
					{
						control.Checked = false;
					}
				}

				_currentZoom = float.Parse(((ToolStripMenuItem)sender).Tag.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);

				if (_currentZoom == -1)
				{
					Vector2 zoomValue = _content.Font.Textures[_currentTextureIndex].Settings.Size;

					// Calculate actual zoom based on window size.
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

					_currentZoom = (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;

					dropDownZoom.Text = "Zoom: To Window";
				}
				else
				{
					dropDownZoom.Text = string.Format("Zoom: {0}%", _currentZoom * 100.0f);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
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
		/// Handles the Resize event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_Resize(object sender, EventArgs e)
		{
			try
			{
				if ((_content == null) || (_content.Font == null))
				{
					return;
				}

				if (menuItemToWindow.Checked)
				{
					ValidateControls();

					Vector2 zoomValue = _content.Font.Textures[_currentTextureIndex].Settings.Size;

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

					_currentZoom = (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			// Refresh the scale.
			GorgonFontContentPanel_Resize(this, EventArgs.Empty);

			ValidateControls();
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void OnContentChanged()
		{
			base.OnContentChanged();

			_content = Content as GorgonFontContent;

			if (_content == null)
			{
				throw new InvalidCastException("The content is not font content.");
			}

			GorgonFontContentPanel_Resize(this, EventArgs.Empty);

			ValidateControls();
		}

		/// <summary>
		/// Function to create the resources for the content.
		/// </summary>
		public void CreateResources()
		{
			_sampleText.Length = 0;
			_sampleText.Append(GorgonFontEditorPlugIn.Settings.SampleText);

			_text = _content.Renderer.Renderables.CreateText("Sample.Text", _content.Font, _sampleText.ToString());
			_text.Color = Color.Black;
			_text.WordWrap = true;
			_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
		}

		/// <summary>
		/// Function to draw the sample text for the font.
		/// </summary>
		public void DrawText()
		{
			Vector2 textPosition = Vector2.Zero;			

			_content.Renderer.Clear(panelText.BackColor);

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

			// If the font object has changed, then reassign it.
			if (_text.Font != _content.Font)
			{
				_text.Font = _content.Font;
			}

			_text.Text = _sampleText.ToString();

			panelText.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_text.Size.X - (SystemInformation.VerticalScrollBarWidth * 1.5f)), (int)System.Math.Ceiling(_text.Size.Y));

			if (panelText.VerticalScroll.Visible)
			{
				textPosition.Y = panelText.AutoScrollPosition.Y;
			}

			_text.Position = textPosition;
			_text.Draw();
		}

		/// <summary>
		/// Function to draw the font texture.
		/// </summary>
		public void DrawFontTexture()
		{
			GorgonTexture2D currentTexture = _content.Font.Textures[_currentTextureIndex];
			_content.Renderer.Clear(PanelDisplay.BackColor);

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.None;


			panelTextures.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Width), (int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Height));

			Vector2 textureLocation = new Vector2(panelTextures.ClientSize.Width / 2.0f - (currentTexture.Settings.Width * _currentZoom) / 2.0f,
													panelTextures.ClientSize.Height / 2.0f - (currentTexture.Settings.Height * _currentZoom) / 2.0f);

			if (panelTextures.HorizontalScroll.Visible)
			{
				textureLocation.X = panelTextures.AutoScrollPosition.X;
			}

			if (panelTextures.VerticalScroll.Visible)
			{
				textureLocation.Y = panelTextures.AutoScrollPosition.Y;
			}

			RectangleF region = new RectangleF(textureLocation.X,
												textureLocation.Y,
												currentTexture.Settings.Width * _currentZoom, currentTexture.Settings.Height * _currentZoom);

			// Fill the background so we can clearly see the first page.
			_content.Renderer.Drawing.FilledRectangle(region, Color.Black);

			// Draw the borders around each glyph.
			foreach (var glyph in _content.Font.Glyphs.Where((GorgonGlyph item) => item.Texture == currentTexture && !char.IsWhiteSpace(item.Character)))
			{
				var glyphRect = new RectangleF(glyph.GlyphCoordinates.Left - 1, glyph.GlyphCoordinates.Top - 1, glyph.GlyphCoordinates.Width + 1, glyph.GlyphCoordinates.Height + 1);
				glyphRect.X = glyphRect.X * _currentZoom + textureLocation.X;
				glyphRect.Y = glyphRect.Y * _currentZoom + textureLocation.Y;
				glyphRect.Width *= _currentZoom;
				glyphRect.Height *= _currentZoom;

				_content.Renderer.Drawing.DrawRectangle(glyphRect, Color.Red);
			}

			// Draw the texture.
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
