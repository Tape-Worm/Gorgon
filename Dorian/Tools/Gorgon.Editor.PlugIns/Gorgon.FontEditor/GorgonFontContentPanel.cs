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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Control for displaying font data.
    /// </summary>
    partial class GorgonFontContentPanel 
		: ContentPanel
    {
        #region Variables.
	    private bool _disposed;
	    private GorgonTexture2D _pattern;
	    private float _currentZoom = -1;
	    private int _currentTextureIndex;
	    private GorgonFontContent _content;
	    private GorgonText _text;
	    private GorgonSprite _patternSprite;
	    private Dictionary<GorgonGlyph, RectangleF> _glyphRegions;
	    private Vector2 _textureOffset = Vector2.Zero;
	    private RectangleF _textureRegion = RectangleF.Empty;
	    private GorgonGlyph _hoverGlyph;
	    private Vector2 _selectorBackGroundPos = Vector2.Zero;
	    private GorgonGlyph _selectedGlyph;
        #endregion

        #region Properties.
		
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the TextChanged event of the textPreviewText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textPreviewText_TextChanged(object sender, EventArgs e)
		{
			string formattedText = textPreviewText.Text;

			if (_text == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(textPreviewText.Text))
			{
				textPreviewText.Text = Resources.GORFNT_DEFAULT_TEXT;
			    return;
			}

			GorgonFontEditorPlugIn.Settings.SampleText = formattedText;

			formattedText = formattedText.Replace("\\n", "\n");
			formattedText = formattedText.Replace("\\t", "\t");

			_text.Text = formattedText;
		}

		/// <summary>
		/// Handles the MouseClick event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_MouseClick(object sender, MouseEventArgs e)
		{
			this.Focus();
		}

		/// <summary>
		/// Handles the MouseWheel event of the PanelDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void PanelDisplay_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				buttonNextTexture.PerformClick();
			}

			if (e.Delta < 0)
			{
				buttonPrevTexture.PerformClick();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemShadowOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemShadowOffset_Click(object sender, EventArgs e)
		{
			formValueComponentEditDialog componentEdit = null;

			try
			{
				componentEdit = new formValueComponentEditDialog();
				componentEdit.ValueComponents = 2;
				componentEdit.DecimalPlaces = 0;
				componentEdit.MaxValue = 8;
				componentEdit.MinValue = -8;
				componentEdit.Value1 = GorgonFontEditorPlugIn.Settings.ShadowOffset.X;
				componentEdit.Value2 = GorgonFontEditorPlugIn.Settings.ShadowOffset.Y;
				componentEdit.Text = "Edit preview text shadow offset";
				componentEdit.ValueChanged += componentEdit_ValueChanged;

				if (componentEdit.ShowDialog() == DialogResult.OK)
				{
					_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset = new Point((int)componentEdit.Value1, (int)componentEdit.Value2);
				}
				else
				{
					_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (componentEdit != null)
				{
					componentEdit.ValueChanged -= componentEdit_ValueChanged;
					componentEdit.Dispose();
				}
				componentEdit = null;
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the componentEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void componentEdit_ValueChanged(object sender, EventArgs e)
		{
			var dialog = sender as formValueComponentEditDialog;
						
			_text.ShadowOffset = new Point((int)dialog.Value1, (int)dialog.Value2);
			DrawText();
			_content.Renderer.Render();
		}

		/// <summary>
		/// Handles the Click event of the itemShadowOpacity control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemShadowOpacity_Click(object sender, EventArgs e)
		{
			var opacity = (int)(GorgonFontEditorPlugIn.Settings.ShadowOpacity * 255.0f);
			formAlphaPicker picker = null;

			try
			{
				picker = new formAlphaPicker();
				picker.Text = "Select the opacity for the preview text shadow";
				picker.SelectedAlphaValue = GorgonFontEditorPlugIn.Settings.ShadowOpacity;
				if (picker.ShowDialog() == DialogResult.OK)
				{
					GorgonFontEditorPlugIn.Settings.ShadowOpacity = _text.ShadowOpacity = picker.SelectedAlphaValue;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemPreviewShadowEnable control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemPreviewShadowEnable_Click(object sender, EventArgs e)
		{
			if (_text != null)
			{
				_text.ShadowEnabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled = itemPreviewShadowEnable.Checked;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemSampleTextBackground control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSampleTextBackground_Click(object sender, EventArgs e)
		{
			ColorPickerDialog picker = null;

			try
			{
				picker = new ColorPickerDialog();
				picker.Text = "Select a background color for the preview text";
				picker.AlphaEnabled = false;
				picker.OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor);
				if (picker.ShowDialog() == DialogResult.OK)
				{
					panelText.BackColor = picker.SelectedColor;
					GorgonFontEditorPlugIn.Settings.BackgroundColor = picker.SelectedColor.ToArgb();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}	
		}

		/// <summary>
		/// Handles the Click event of the buttonTextColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonTextColor_Click(object sender, EventArgs e)
		{
			ColorPickerDialog picker = null;

			try
			{
				picker = new ColorPickerDialog();
				picker.Text = "Select a color for the preview text";
				picker.OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.TextColor);
				if (picker.ShowDialog() == DialogResult.OK)
				{
					GorgonFontEditorPlugIn.Settings.TextColor = picker.SelectedColor.ToArgb();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}

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

			itemShadowOffset.Enabled = itemShadowOpacity.Enabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled;

            UpdateGlyphInfo();
		}

        /// <summary>
        /// Function to update the glyph information.
        /// </summary>
        private void UpdateGlyphInfo()
        {
            if (_selectedGlyph != null)
            {
                labelSelectedGlyphInfo.Text = string.Format("Selected Glyph: {0} (U+{1})",
                    _selectedGlyph.Character,
                    ((ushort)_selectedGlyph.Character).FormatHex());
            }
            else
            {
                labelSelectedGlyphInfo.Text = string.Empty;
            }

            if (_hoverGlyph != null)
            {
                if (_selectedGlyph != null)
                {
                    separatorGlyphInfo.Visible = true;
                }

                labelHoverGlyphInfo.Text = string.Format("Glyph: {0} (U+{1}) Location: {2}, {3} Size: {4},{5} A:{6} B:{7}: C:{8}",
                    _hoverGlyph.Character,
                    ((ushort)_hoverGlyph.Character).FormatHex(),
                    _hoverGlyph.GlyphCoordinates.X,
                    _hoverGlyph.GlyphCoordinates.Y,
                    _hoverGlyph.GlyphCoordinates.Width,
                    _hoverGlyph.GlyphCoordinates.Height,
                    _hoverGlyph.Advance.X,
                    _hoverGlyph.Advance.Y,
                    _hoverGlyph.Advance.Z);
            }
            else
            {
                labelHoverGlyphInfo.Text = string.Empty;
                separatorGlyphInfo.Visible = false;
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
                GorgonFontContentPanel_Resize(this, e);
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
                GorgonFontContentPanel_Resize(this, e);
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
					dropDownZoom.Text = "Zoom: To Window";
				}
				else
				{
					dropDownZoom.Text = string.Format("Zoom: {0}%", _currentZoom * 100.0f);
				}

                GorgonFontContentPanel_Resize(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}

        /// <summary>
        /// Handles the MouseDown event of the panelTextures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelTextures_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // If we're inside the texture, find the glyph that we're hovering over.
                _selectedGlyph = null;
                if (_textureRegion.Contains(e.Location))
                {                    
                    foreach (var glyph in _glyphRegions)
                    {
                        var rect = glyph.Value;
                        rect.X += _textureOffset.X;
                        rect.Y += _textureOffset.Y;

                        if (rect.Contains(e.Location))
                        {
                            _selectedGlyph = glyph.Key;
                            break;
                        }
                    }
                }
            }
            finally
            {
                ValidateControls();
            }
        }

		/// <summary>
		/// Handles the MouseMove event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseMove(object sender, MouseEventArgs e)
		{
            _hoverGlyph = null;

            // If we're inside the texture, find the glyph that we're hovering over.
            if (_textureRegion.Contains(e.Location))
            {
                foreach (var glyph in _glyphRegions)
                {
                    var rect = glyph.Value;
                    rect.X += _textureOffset.X;
                    rect.Y += _textureOffset.Y;

                    if (rect.Contains(e.Location))
                    {
                        _hoverGlyph = glyph.Key;
                        break;
                    }
                }
            }

            UpdateGlyphInfo();
		}

		/// <summary>
		/// Handles the Resize event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_Resize(object sender, EventArgs e)
		{
            GorgonTexture2D currentTexture = null;
			try
			{
				if ((_content == null) || (_content.Font == null))
				{
					return;
				}

				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _content.Font.Textures.Count)
				{
					_currentTextureIndex = _content.Font.Textures.Count - 1;

					// If for some reason we don't have textures, then leave.
					if (_currentTextureIndex < 0)
					{
						return;
					}
				}

                currentTexture = _content.Font.Textures[_currentTextureIndex];

				if (menuItemToWindow.Checked)
				{
					ValidateControls();

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

					_currentZoom = (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;
				}

                // Recalculate 
                var glyphs = from GorgonGlyph fontGlyph in _content.Font.Glyphs
                             where fontGlyph.Texture == currentTexture && !char.IsWhiteSpace(fontGlyph.Character)
                             select fontGlyph;
                
                // Find the regions for the glyph.
				_hoverGlyph = null;
                _glyphRegions = new Dictionary<GorgonGlyph,RectangleF>();

			
                foreach (var glyph in glyphs)
                {
                    var glyphRect = new RectangleF((glyph.GlyphCoordinates.Left - 1) * _currentZoom, 
                                                    (glyph.GlyphCoordinates.Top - 1) * _currentZoom, 
                                                    (glyph.GlyphCoordinates.Width + 1) * _currentZoom, 
                                                    (glyph.GlyphCoordinates.Height + 1) * _currentZoom);

                    _glyphRegions[glyph] = glyphRect;
                }

				_text.Font = _content.Font;
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

			// If we had a previously selected glyph, then try and find it in the updated glyphs from
			// the new font.  Otherwise, deselect it.
			if (_selectedGlyph != null)
			{
				_selectedGlyph = _content.Font.Glyphs.Where((GorgonGlyph item) => item.Character == _selectedGlyph.Character).FirstOrDefault();

				// We found the glyph, let's see if we can't focus on the texture it was on.
				if (_selectedGlyph != null)
				{
					_currentTextureIndex = _content.Font.Textures.IndexOf(_selectedGlyph.Texture);
				}
			}

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
			itemPreviewShadowEnable.Checked = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
			panelText.BackColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor);

			_text = _content.Renderer.Renderables.CreateText("Sample.Text", _content.Font, string.Empty);
			_text.Color = Color.Black;
			_text.WordWrap = true;
			_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
			_text.ShadowOpacity = GorgonFontEditorPlugIn.Settings.ShadowOpacity;
			_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
			_text.ShadowEnabled = itemPreviewShadowEnable.Checked;

            _pattern = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>("Background.Pattern", Properties.Resources.Pattern);

            _patternSprite = _content.Renderer.Renderables.CreateSprite("Pattern", new GorgonSpriteSettings()
            {
                Color = new GorgonColor(1, 0, 0, 0.4f),
                Texture = _pattern,
                TextureRegion = new RectangleF(0, 0, 1, 1),
                Size = _pattern.Settings.Size
            });

            // Set the sprite up for wrapping the texture.
            _patternSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
            _patternSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			textPreviewText.Text = GorgonFontEditorPlugIn.Settings.SampleText;
			textPreviewText.Select();
			textPreviewText.Select(0, textPreviewText.Text.Length); 
		}

		/// <summary>
		/// Function to draw the sample text for the font.
		/// </summary>
		public void DrawText()
		{
			Vector2 textPosition = Vector2.Zero;			

			_content.Renderer.Clear(panelText.BackColor);

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

			_text.Color = new GorgonColor(GorgonFontEditorPlugIn.Settings.TextColor);		

			panelText.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_text.Size.X - (SystemInformation.VerticalScrollBarWidth * 1.5f)), (int)System.Math.Ceiling(_text.Size.Y));

			if (panelText.VerticalScroll.Visible)
			{
				textPosition.Y = panelText.AutoScrollPosition.Y;
			}

			_text.Position = textPosition;
			_text.Draw();

            System.Threading.Thread.Sleep(500);
		}

		/// <summary>
		/// Function to draw the font texture.
		/// </summary>
		public void DrawFontTexture()
		{
			GorgonTexture2D currentTexture = _content.Font.Textures[_currentTextureIndex];
			_content.Renderer.Clear(PanelDisplay.BackColor);

			/*_content.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

			float alpha = 0.0f;

			var texturePos = new Vector2(((panelTextures.ClientSize.Width / 2.0f) - (currentTexture.Settings.Width * _currentZoom) / 2.0f), 0);

			// Show the textures that come before the current texture.
			for (int i = _currentTextureIndex - 1; i >= 0; i--)
			{
				var displayTexture = _content.Font.Textures[i];
				float range = _currentTextureIndex;
				float index = (_currentTextureIndex - 1) - i;

				alpha = (range - index) / range;

				var textureSize = new Vector2((_currentZoom * 0.75f * alpha) * currentTexture.Settings.Width, (_currentZoom * 0.75f * alpha) * currentTexture.Settings.Height);

				texturePos.Y = panelTextures.ClientSize.Height / 2.0f - textureSize.Y / 2.0f;
				texturePos.X -= textureSize.X + 8.0f;				

				_content.Renderer.Drawing.FilledRectangle(new RectangleF(texturePos, textureSize), Color.FromArgb((int)(alpha * 128.0f), 192, 192, 192), displayTexture);
			}

			// Set to the middle.
			texturePos.X = ((panelTextures.ClientSize.Width / 2.0f) + (currentTexture.Settings.Width * _currentZoom) / 2.0f) + 8.0f;

			// Show the textures that come after the current texture.
			for (int i = _currentTextureIndex + 1; i < _content.Font.Textures.Count; i++)
			{
				var displayTexture = _content.Font.Textures[i];
				float range = _content.Font.Textures.Count - (_currentTextureIndex + 1);
				float index = i - (_currentTextureIndex + 1);

				alpha = (range - index) / range;

				var textureSize = new Vector2((_currentZoom * 0.75f * alpha) * currentTexture.Settings.Width, (_currentZoom * 0.75f * alpha) * currentTexture.Settings.Height);

				texturePos.Y = panelTextures.ClientSize.Height / 2.0f - textureSize.Y / 2.0f;

				_content.Renderer.Drawing.FilledRectangle(new RectangleF(texturePos, textureSize), Color.FromArgb((int)(alpha * 128.0f), 192, 192, 192), displayTexture);

				texturePos.X += textureSize.X + 8.0f;
			}

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.None;

            _selectorBackGroundPos.X += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Width;
            _selectorBackGroundPos.Y += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Height;

            if (_selectorBackGroundPos.X > 1.0f)
            {
                _selectorBackGroundPos.X = 0.0f;
            }

            if (_selectorBackGroundPos.Y > 1.0f)
            {
                _selectorBackGroundPos.Y = 0.0f;
            }

			panelTextures.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Width), (int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Height));

			_textureOffset = new Vector2(panelTextures.ClientSize.Width / 2.0f - (currentTexture.Settings.Width * _currentZoom) / 2.0f,
								         panelTextures.ClientSize.Height / 2.0f - (currentTexture.Settings.Height * _currentZoom) / 2.0f);

			if (panelTextures.HorizontalScroll.Visible)
			{
				_textureOffset.X = panelTextures.AutoScrollPosition.X;
			}

			if (panelTextures.VerticalScroll.Visible)
			{
                _textureOffset.Y = panelTextures.AutoScrollPosition.Y;
			}

            _textureRegion = new RectangleF(_textureOffset, ((Vector2)currentTexture.Settings.Size) * _currentZoom);

			// Fill the background so we can clearly see the first page.
			_content.Renderer.Drawing.FilledRectangle(_textureRegion, Color.Black);

			// Draw the borders around each glyph.
			foreach (var glyph in _glyphRegions)
			{
                var glyphRect = glyph.Value;
				glyphRect.X += _textureOffset.X;
                glyphRect.Y += _textureOffset.Y;

				_content.Renderer.Drawing.DrawRectangle(glyphRect, Color.Red);
			}

			// Draw the texture.
            _content.Renderer.Drawing.Blit(currentTexture, _textureRegion);

            if (_hoverGlyph != null)
            {
                var rect = _glyphRegions[_hoverGlyph];
                rect.X += _textureOffset.X;
                rect.Y += _textureOffset.Y;

                _patternSprite.Position = rect.Location;
                _patternSprite.Size = rect.Size;
                _patternSprite.Color = new GorgonColor(1, 0, 0, 0.4f);
				_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X, _selectorBackGroundPos.Y, rect.Width / _pattern.Settings.Width, rect.Height / _pattern.Settings.Height);
                _patternSprite.Draw();
            }

            if ((_selectedGlyph != null) && (_selectedGlyph.Texture == currentTexture))
            {
                var rect = _glyphRegions[_selectedGlyph];
                rect.X += _textureOffset.X;
                rect.Y += _textureOffset.Y;

                _patternSprite.Position = rect.Location;
                _patternSprite.Size = rect.Size;
                _patternSprite.Color = new GorgonColor(0.25f, 0.25f, 1.0f, 0.4f);
				_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X, _selectorBackGroundPos.Y, rect.Width / _pattern.Settings.Width, rect.Height / _pattern.Settings.Height);
                _patternSprite.Draw();
            }*/
		}
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentPanel"/> class.
        /// </summary>
        public GorgonFontContentPanel()
        {
            InitializeComponent();

			this.MouseWheel += PanelDisplay_MouseWheel;
        }
        #endregion
	}
}
