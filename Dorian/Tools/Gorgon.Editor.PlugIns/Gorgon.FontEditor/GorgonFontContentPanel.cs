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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Animation;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

// TODO: Add brush creation interface.
// TODO: Add texture clipping for glyphs.
// TODO: Add spacing/kerning interfaces for glyphs.

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Control for displaying font data.
    /// </summary>
    partial class GorgonFontContentPanel 
		: ContentPanel
    {
        #region Variables.
        private Vector2 _panelMidPoint = Vector2.Zero;
        private GorgonKeyboard _rawKeyboard;
		private bool _disposed;
	    private GorgonTexture2D _pattern;
	    private float _currentZoom = -1;
	    private int _currentTextureIndex;
	    private float _indexTransition;
	    private GorgonFontContent _content;
	    private GorgonText _text;
	    private GorgonSprite _patternSprite;
	    private Dictionary<GorgonGlyph, RectangleF> _glyphRegions;
	    private Vector2 _textureOffset = Vector2.Zero;
	    private RectangleF _textureRegion = RectangleF.Empty;
	    private GorgonGlyph _hoverGlyph;
	    private Vector2 _selectorBackGroundPos = Vector2.Zero;
	    private GorgonGlyph _selectedGlyph;
	    private GorgonTexture2D[] _textures;
	    private GorgonRenderTarget2D _editGlyph;
	    private GorgonAnimationController<GorgonSprite> _editGlyphAnimation;
		private GorgonAnimationController<GorgonSprite> _editBackgroundAnimation;
        private List<GorgonSprite> _textureSprites;
	    private GorgonSprite _glyphSprite;
	    private GorgonSprite _glyphBackgroundSprite;
        private IEnumerable<GorgonSprite> _sortedTextures;
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Load event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_Load(object sender, EventArgs e)
		{
			try
			{
				_rawKeyboard = RawInput.CreateKeyboard(panelTextures);
				_rawKeyboard.Enabled = true;
				_rawKeyboard.KeyUp += GorgonFontContentPanel_KeyUp;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
				_content.CloseContent();
			}
		}

		/// <summary>
		/// Handles the KeyUp event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_KeyUp(object sender, KeyboardEventArgs e)
		{
			Debug.Assert(ParentForm != null, "No parent form!");

			if ((ParentForm.ActiveControl != this) 
				|| (!panelTextures.Focused))
			{
				return;
			}

			switch (e.Key)
			{
				case KeyboardKeys.Left:
					buttonPrevTexture.PerformClick();
					break;
				case KeyboardKeys.Right:
					buttonNextTexture.PerformClick();
					break;
				case KeyboardKeys.Enter:
					if ((_selectedGlyph == null)
						|| (_content.CurrentState == DrawState.GlyphEdit)
						|| (_content.CurrentState == DrawState.ToGlyphEdit))
					{
						return;
					}

					buttonEditGlyph.PerformClick();
					break;
				case KeyboardKeys.Escape:
					if ((_selectedGlyph == null)
						|| ((_content.CurrentState != DrawState.GlyphEdit)
						&& (_content.CurrentState != DrawState.ToGlyphEdit)))
					{
						return;
					}

					buttonEditGlyph.PerformClick();
					break;
			}
		}

        /// <summary>
        /// Handles the Click event of the buttonRemoveCustomTexture control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonRemoveCustomTexture_Click(object sender, EventArgs e)
        {
            try
            {
                if (GorgonDialogs.ConfirmBox(ParentForm,
                                             string.Format(Resources.GORFNT_REMOVE_TEXTURE_PROMPT,
                                                           string.Format("{0} (0x{1})",
                                                                         _selectedGlyph.Character,
                                                                         ((ushort)_selectedGlyph.Character).FormatHex()),
                                                                         _selectedGlyph.Texture.Name))
                    == ConfirmationResult.No)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                // Ensure that the selected glyph actually exists in the glyph overrides.
                GorgonGlyph currentGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

                if (currentGlyph == null)
                {
                    return;
                }

                // Destroy the texture if it's not used by any other glyph.
                while (_content.Font.Settings.Glyphs.Any(item => item.Character == currentGlyph.Character))
                {
                    if (!_content.Font.Settings.Glyphs.Any(item =>
                                                           item.Texture == currentGlyph.Texture
                                                           && item != currentGlyph))
                    {
                        _selectedGlyph.Texture.Dispose();
                    }

                    _content.Font.Settings.Glyphs.Remove(currentGlyph);
                }

                _content.UpdateFont();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
		/// Handles the Click event of the buttonGlyphClip control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGlyphClip_Click(object sender, EventArgs e)
		{
		    try
		    {
		        imageFileBrowser.FileExtensions.Clear();
		        foreach (var extension in _content.ImageEditor.FileExtensions)
		        {
		            imageFileBrowser.FileExtensions[extension.Extension] = extension;
		        }
		        imageFileBrowser.FileExtensions["*"] = new GorgonFileExtension("*", Resources.GORFNT_DLG_ALL_FILES);

		        imageFileBrowser.FileView = GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView;
			    imageFileBrowser.DefaultExtension = GorgonFontEditorPlugIn.Settings.LastTextureExtension;

		        if (imageFileBrowser.ShowDialog(ParentForm) == DialogResult.OK)
		        {
		            using(var imageContent = _content.ImageEditor.ImportContent(imageFileBrowser.Files[0],
		                                                                        0,
		                                                                        0,
		                                                                        true,
		                                                                        BufferFormat.R8G8B8A8_UIntNormal))
		            {
		                // We can only use 2D content.
		                if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
		                {
		                    GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_IMAGE_NOT_2D, imageContent.Name));
		                    return;
		                }

		                // If the size is mismatched with the font textures then ask the user if they wish to resize the 
		                // texture.
		                if ((imageContent.Image.Settings.Width != _content.Font.Settings.TextureSize.Width)
		                    || (imageContent.Image.Settings.Height != _content.Font.Settings.TextureSize.Height))
		                {
		                    ConfirmationResult result = GorgonDialogs.ConfirmBox(ParentForm,
		                                                                         string.Format(
		                                                                                       Resources
		                                                                                           .GORFNT_IMAGE_SIZE_MISMATCH_MSG,
		                                                                                       imageContent.Image.Settings
		                                                                                                   .Width,
		                                                                                       imageContent.Image.Settings
		                                                                                                   .Height,
		                                                                                       _content.Font.Settings
		                                                                                               .TextureSize.Width,
		                                                                                       _content.Font.Settings
		                                                                                               .TextureSize.Height),
		                                                                         true,
		                                                                         false);

		                    if (result == ConfirmationResult.Cancel)
		                    {
		                        return;
		                    }

		                    // Resize or clip the image.
		                    imageContent.Image.Resize(_content.Font.Settings.TextureSize.Width,
		                                              _content.Font.Settings.TextureSize.Height,
		                                              result == ConfirmationResult.No,
		                                              ImageFilter.Point);
		                }


						// Remove any array indices from the texture, we don't need them for font glyphs.
			            var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
			            settings.ArrayCount = 1;

		                var texture = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.Name,
		                                                                                        imageContent.Image,
																								settings);

		                GorgonGlyph newGlyph =
		                    _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

		                if (newGlyph != null)
		                {
		                    _content.Font.Settings.Glyphs.Remove(newGlyph);

		                    // If this is the only glyph referencing this texture, then dump it.
		                    if (!_content.Font.Glyphs.Any(item => item.Texture == newGlyph.Texture && item != newGlyph))
		                    {
		                        newGlyph.Texture.Dispose();
		                    }
		                }

		                // Default to the full size of the texture.
		                newGlyph = new GorgonGlyph(_selectedGlyph.Character,
		                                           texture,
		                                           new Rectangle(0, 0, texture.Settings.Width, texture.Settings.Height),
		                                           Vector2.Zero,
		                                           Vector3.Zero);

		                _content.Font.Settings.Glyphs.Add(newGlyph);
		                _content.UpdateFont();
		            }
		        }

		        GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView = imageFileBrowser.FileView;
			    GorgonFontEditorPlugIn.Settings.LastTextureExtension = imageFileBrowser.DefaultExtension;
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
			Focus();
		}

		/// <summary>
		/// Handles the MouseWheel event of the PanelDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void PanelDisplay_MouseWheel(object sender, MouseEventArgs e)
		{
		    if ((e.Delta > 0)
		        && (buttonNextTexture.Enabled))
		    {
		        buttonNextTexture.PerformClick();
		    }

		    if ((e.Delta < 0)
		        && (buttonPrevTexture.Enabled))
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
				componentEdit = new formValueComponentEditDialog
				                {
					                ValueComponents = 2,
					                DecimalPlaces = 0,
					                MaxValue = 8,
					                MinValue = -8,
					                Value1 = GorgonFontEditorPlugIn.Settings.ShadowOffset.X,
					                Value2 = GorgonFontEditorPlugIn.Settings.ShadowOffset.Y,
					                Text = Resources.GORFNT_DLG_SHADOW_OFFSET_TEXT
				                };
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
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the componentEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void componentEdit_ValueChanged(object sender, EventArgs e)
		{
			var dialog = (formValueComponentEditDialog)sender;
						
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
			formAlphaPicker picker = null;

			try
			{
				picker = new formAlphaPicker
				         {
					         Text = Resources.GORFNT_DLG_SHADOW_ALPHA_TEXT,
					         SelectedAlphaValue = GorgonFontEditorPlugIn.Settings.ShadowOpacity
				         };
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
			if (_text == null)
			{
				return;
			}

			_text.ShadowEnabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled = itemPreviewShadowEnable.Checked;
			ValidateControls();
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
				picker = new ColorPickerDialog
				         {
					         Text = Resources.GORFNT_DLG_BACKGROUND_COLOR_TEXT,
					         AlphaEnabled = false,
					         OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor)
				         };

				if (picker.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				panelText.BackColor = picker.SelectedColor;
				GorgonFontEditorPlugIn.Settings.BackgroundColor = picker.SelectedColor.ToArgb();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
				{
					picker.Dispose();
				}
				ValidateControls();
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
				picker = new ColorPickerDialog
				         {
					         Text = Resources.GORFNT_DLG_FOREGROUND_COLOR_TEXT,
					         OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.TextColor)
				         };

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
				// If we can't use the image editor plug-in, then don't allow us to edit the glyph image dimensions.
				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _textures.Length)
				{
					_currentTextureIndex = _textures.Length - 1;
				}

	            buttonPrevTexture.Enabled = _currentTextureIndex > 0 &&
	                                        ((_content.CurrentState == DrawState.DrawFontTextures) ||
	                                         _content.CurrentState == DrawState.NextTexture ||
	                                         _content.CurrentState == DrawState.PrevTexture);
				buttonNextTexture.Enabled = _currentTextureIndex < _textures.Length - 1 && ((_content.CurrentState == DrawState.DrawFontTextures) ||
											 _content.CurrentState == DrawState.NextTexture ||
											 _content.CurrentState == DrawState.PrevTexture);

				labelTextureCount.Text = string.Format("{0}: {1}/{2}", Resources.GORFNT_LABEL_TEXTURE_COUNT, _currentTextureIndex + 1, _textures.Length);

				UpdateGlyphInfo();
            }
            else
            {
				buttonPrevTexture.Enabled = false;
				buttonNextTexture.Enabled = false;
                labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_LABEL_TEXTURE_COUNT);
            }

			itemShadowOffset.Enabled = itemShadowOpacity.Enabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
		}

        /// <summary>
        /// Function to update the glyph information.
        /// </summary>
        private void UpdateGlyphInfo()
        {
			buttonEditGlyph.Enabled = _selectedGlyph != null;
			
	        if ((!buttonEditGlyph.Enabled) && (buttonEditGlyph.Checked))
	        {
		        buttonEditGlyph.Checked = false;
	        }

	        buttonGlyphClip.Enabled =
		        buttonGlyphKern.Enabled =
		        buttonGlyphSizeSpace.Enabled = buttonEditGlyph.Enabled && _content.CurrentState == DrawState.GlyphEdit;

			buttonRemoveCustomTexture.Visible = buttonGlyphClip.Visible = _content.ImageEditor != null;
	        buttonRemoveCustomTexture.Enabled = (_content.ImageEditor != null) && (_selectedGlyph != null) &&
	                                            (_selectedGlyph.IsExternalTexture) &&
	                                            (_content.CurrentState == DrawState.GlyphEdit);

	        if ((_content.CurrentState == DrawState.ToGlyphEdit)
	            || (_content.CurrentState == DrawState.GlyphEdit))
	        {
		        buttonEditGlyph.Text = Resources.GORFNT_BUTTON_END_EDIT_GLYPH;
		        buttonEditGlyph.Image = Resources.stop_16x16;
	        }
	        else
	        {
				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.edit_16x16;
	        }

	        if (_selectedGlyph != null)
            {
	            if (_content.CurrentState == DrawState.DrawFontTextures)
	            {
		            labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2})",
																Resources.GORFNT_LABEL_SELECTED_GLYPH,
		                                                        _selectedGlyph.Character,
		                                                        ((ushort)_selectedGlyph.Character).FormatHex())
		                                                .Replace("&", "&&");
	            }
	            else
	            {
					labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7},{8} {9}: A={10}, B={11}, C={12}",
						Resources.GORFNT_LABEL_SELECTED_GLYPH,
						_selectedGlyph.Character,
						((ushort)_selectedGlyph.Character).FormatHex(),
						Resources.GORFNT_LABEL_GLYPH_LOCATION,
						_selectedGlyph.GlyphCoordinates.X,
						_selectedGlyph.GlyphCoordinates.Y,
						Resources.GORFNT_LABEL_GLYPH_SIZE,
						_selectedGlyph.GlyphCoordinates.Width,
						_selectedGlyph.GlyphCoordinates.Height,
						Resources.GORFNT_LABEL_ADVANCE,
						_selectedGlyph.Advance.X,
						_selectedGlyph.Advance.Y,
						_selectedGlyph.Advance.Z).Replace("&", "&&");
		            _hoverGlyph = null;
	            }
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

                labelHoverGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7},{8} {9}: A={10}, B={11}, C={12}",
					Resources.GORFNT_LABEL_HOVER_GLYPH,
                    _hoverGlyph.Character,
                    ((ushort)_hoverGlyph.Character).FormatHex(),
					Resources.GORFNT_LABEL_GLYPH_LOCATION,
                    _hoverGlyph.GlyphCoordinates.X,
                    _hoverGlyph.GlyphCoordinates.Y,
					Resources.GORFNT_LABEL_GLYPH_SIZE,
                    _hoverGlyph.GlyphCoordinates.Width,
                    _hoverGlyph.GlyphCoordinates.Height,
					Resources.GORFNT_LABEL_ADVANCE,
                    _hoverGlyph.Advance.X,
                    _hoverGlyph.Advance.Y,
					_hoverGlyph.Advance.Z).Replace("&", "&&");
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
			    _sortedTextures = null;
				_currentTextureIndex++;
				_content.CurrentState = DrawState.NextTexture;
				ActiveControl = panelTextures;
				UpdateGlyphRegions();
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
                _sortedTextures = null;
				_currentTextureIndex--;
				_content.CurrentState = DrawState.PrevTexture;
				ActiveControl = panelTextures;
				UpdateGlyphRegions();
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
			var selectedItem = (ToolStripMenuItem)sender;

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

				_currentZoom = float.Parse(selectedItem.Tag.ToString(),
				                           NumberStyles.Float,
				                           NumberFormatInfo.InvariantInfo);

				dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_MENU_ZOOM, selectedItem.Text);


			    if (_content.Font != null)
			    {

			        panelTextures.AutoScrollMinSize =
			            new Size((int)System.Math.Ceiling(_currentZoom * _textures[_currentTextureIndex].Settings.Width),
                                 (int)System.Math.Ceiling(_currentZoom * _textures[_currentTextureIndex].Settings.Height));
			    }

			    UpdateGlyphRegions();

                _sortedTextures = null;

				if (_content.CurrentState != DrawState.DrawFontTextures)
				{
					UpdateGlyphEditor();
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
        /// Handles the MouseDown event of the panelTextures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelTextures_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
	            switch (_content.CurrentState)
	            {
					case DrawState.NextTexture:
					case DrawState.PrevTexture:
					case DrawState.FromGlyphEdit:
					case DrawState.ToGlyphEdit:
			            break;
					case DrawState.GlyphEdit:
						break;
		            default:
						// If we're inside the texture, find the glyph that we're hovering over.
						_selectedGlyph = null;
						if (_textureRegion.Contains(e.Location))
						{
							foreach (var glyph in _glyphRegions)
							{
								var rect = glyph.Value;
								rect.X += _textureOffset.X;
								rect.Y += _textureOffset.Y;

								if (!rect.Contains(e.Location))
								{
									continue;
								}

								_selectedGlyph = glyph.Key;
								break;
							}
						}

			            break;
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
            // If we're inside the texture, find the glyph that we're hovering over.
			switch (_content.CurrentState)
			{
				case DrawState.FromGlyphEdit:
				case DrawState.ToGlyphEdit:
					break;
				case DrawState.GlyphEdit:
					break;
				default:
					_hoverGlyph = null;

					if (_textureRegion.Contains(e.Location))
					{
						foreach (var glyph in _glyphRegions)
						{
							var rect = glyph.Value;
							rect.X += _textureOffset.X;
							rect.Y += _textureOffset.Y;

							if (!rect.Contains(e.Location))
							{
								continue;
							}

							_hoverGlyph = glyph.Key;
							break;
						}
					}

					UpdateGlyphInfo();
					break;
			}
		}

		/// <summary>
		/// Function to update the glyph regions.
		/// </summary>
	    private void UpdateGlyphRegions()
	    {
			if (_currentTextureIndex < 0)
			{
				_currentTextureIndex = 0;
			}

			if (_currentTextureIndex >= _textures.Length)
			{
				_currentTextureIndex = _textures.Length - 1;

				// If for some reason we don't have textures, then leave.
				if (_currentTextureIndex < 0)
				{
					return;
				}
			}

			GorgonTexture2D currentTexture = _textures[_currentTextureIndex];

			// Get the current zoom level.
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
			_glyphRegions = new Dictionary<GorgonGlyph, RectangleF>();


			foreach (var glyph in glyphs)
			{
				var glyphRect = new RectangleF((glyph.GlyphCoordinates.Left - 1) * _currentZoom,
												(glyph.GlyphCoordinates.Top - 1) * _currentZoom,
												(glyph.GlyphCoordinates.Width + 1) * _currentZoom,
												(glyph.GlyphCoordinates.Height + 1) * _currentZoom);

				_glyphRegions[glyph] = glyphRect;
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
		        _sortedTextures = null;

                _panelMidPoint = new Vector2(panelTextures.ClientSize.Width / 2.0f, panelTextures.ClientSize.Height / 2.0f);

				if ((_content == null) || (_content.Font == null))
				{
					_textures = new GorgonTexture2D[0];
					return;
				}

				// Update the texture list.
				_textures = (from glyph in _content.Font.Glyphs
							 select glyph.Texture).Distinct().ToArray();

				// If we had a previously selected glyph, then try and find it in the updated glyphs from
				// the new font.  Otherwise, deselect it.
				if (_selectedGlyph != null)
				{
					_selectedGlyph = _content.Font.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

					// We found the glyph, let's see if we can't focus on the texture it was on.
					if (_selectedGlyph != null)
					{
						_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
					}
					else
					{
						// If we're editing a glyph that doesn't exist, then we need to stop editing immediately.
						if ((_editGlyph != null) && (_content.CurrentState != DrawState.DrawFontTextures))
						{
							_content.CurrentState = DrawState.DrawFontTextures;
						}
					}
				}

				// Disable any animations for the font textures.
				_indexTransition = _currentTextureIndex;

				UpdateGlyphRegions();
                InitializeTextureSprites();

				_text.Font = _content.Font;

				// Kill any transition.
				if (_editGlyphAnimation.CurrentAnimation != null)
				{
					_editGlyphAnimation.CurrentAnimation.Time = _editGlyphAnimation.CurrentAnimation.Speed > 0
						                                            ? _editGlyphAnimation.CurrentAnimation.Length
						                                            : 0;
					_editBackgroundAnimation.CurrentAnimation.Time = _editBackgroundAnimation.CurrentAnimation.Speed > 0
						                                                 ? _editBackgroundAnimation.CurrentAnimation.Length
						                                                 : 0;
					_editBackgroundAnimation.Update();
					_editGlyphAnimation.Update();
					_editBackgroundAnimation.Stop();
					_editGlyphAnimation.Stop();
				}

				if ((_content.CurrentState == DrawState.ToGlyphEdit)
				    || (_content.CurrentState == DrawState.GlyphEdit))
				{
					UpdateGlyphEditor();
				}
				else
				{
					if (_editGlyph != null)
					{
						_editGlyph.Dispose();
						_editGlyph = null;
					}
					_content.CurrentState = DrawState.DrawFontTextures;
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
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORFNT_TITLE;
			buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
			buttonGlyphSizeSpace.Text = Resources.GORFNT_BUTTON_EDIT_ADVANCE_TIP;
			buttonGlyphKern.Text = Resources.GORFNT_BUTTON_EDIT_KERNING_TIP;
			buttonGlyphClip.Text = Resources.GORFNT_BUTTON_CLIP_GLYPH_TIP;
			menuTextColor.Text = Resources.GORFNT_MENU_DISPLAY_COLORS;
			itemSampleTextForeground.Text = Resources.GORFNT_MENU_FOREGROUND_COLOR;
			itemSampleTextBackground.Text = Resources.GORFNT_MENU_BACKGROUND_COLOR;
			menuShadow.Text = Resources.GORFNT_MENU_SHADOW_SETTINGS;
			itemPreviewShadowEnable.Text = Resources.GORFNT_MENU_ENABLE_SHADOW;
			itemShadowOpacity.Text = Resources.GORFNT_MENU_SHADOW_OPACITY;
			itemShadowOffset.Text = Resources.GORFNT_MENU_SHADOW_OFFSET;
			toolStripLabel1.Text = string.Format("{0}:", Resources.GORFNT_LABEL_PREVIEW_TEXT);
			labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_LABEL_TEXTURE_COUNT);
			dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_MENU_ZOOM, Resources.GORFNT_MENU_TO_WINDOW);
			menuItemToWindow.Text = Resources.GORFNT_MENU_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			// Refresh the font display.
			GorgonFontContentPanel_Resize(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

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
			const float animationTime = 0.5f;		// Time for the animation.

			itemPreviewShadowEnable.Checked = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
			panelText.BackColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor);

			_text = _content.Renderer.Renderables.CreateText("Sample.Text", _content.Font, string.Empty);
			_text.Color = Color.Black;
			_text.WordWrap = true;
			_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
			_text.ShadowOpacity = GorgonFontEditorPlugIn.Settings.ShadowOpacity;
			_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
			_text.ShadowEnabled = itemPreviewShadowEnable.Checked;

            _pattern = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>("Background.Pattern", Resources.Pattern);

            _patternSprite = _content.Renderer.Renderables.CreateSprite("Pattern", new GorgonSpriteSettings
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

			// Create the glyph sprite.
			_glyphSprite = _content.Renderer.Renderables.CreateSprite("GlyphSprite",
																	  new GorgonSpriteSettings
																	  {
																		  InitialPosition = Vector2.Zero,
																		  Size = new Vector2(1),
																		  TextureRegion = new RectangleF(0, 0, 1, 1)
																	  });
			_glyphSprite.BlendingMode = BlendingMode.Modulate;

			_glyphBackgroundSprite = _content.Renderer.Renderables.CreateSprite("GlyphBackgroundSprite",
			                                                                    new GorgonSpriteSettings
			                                                                    {
				                                                                    InitialPosition = Vector2.Zero,
				                                                                    Size = new Vector2(1),
				                                                                    Color = GorgonColor.Transparent,
				                                                                    Texture = _pattern
			                                                                    });

			_glyphBackgroundSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_glyphBackgroundSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			// Create initial animations.
			_editGlyphAnimation = new GorgonAnimationController<GorgonSprite>();
			var animation = _editGlyphAnimation.Add("TransitionTo", animationTime);
			animation.Tracks["Position"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["ScaledSize"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(0, Vector2.Zero));
			animation.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(animation.Length, Vector2.Zero));
			animation.Tracks["ScaledSize"].KeyFrames.Add(new GorgonKeyVector2(0, new Vector2(1)));
			animation.Tracks["ScaledSize"].KeyFrames.Add(new GorgonKeyVector2(animation.Length, new Vector2(1)));


			_editBackgroundAnimation = new GorgonAnimationController<GorgonSprite>();
			animation = _editBackgroundAnimation.Add("GlyphBGColor", animationTime);
			animation.Tracks["Color"].InterpolationMode = TrackInterpolationMode.Linear;
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(0.0f, new GorgonColor(0.125f, 0.125f, 0, 0)));
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(animation.Length, new GorgonColor(0.125f, 0.125f, 0.7f, 1.0f)));

            _textureSprites = new List<GorgonSprite>();
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
		}

		/// <summary>
		/// Handles the Click event of the buttonEditGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditGlyph_Click(object sender, EventArgs e)
		{
			try
			{
				if ((_content.CurrentState != DrawState.GlyphEdit)
				    && ((_content.CurrentState != DrawState.ToGlyphEdit)))
				{
					InitializeGlyphEditTransition(true);
				}
				else
				{
					InitializeGlyphEditTransition(false);
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
		/// Handles the MouseDoubleClick event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (_content.CurrentState != DrawState.DrawFontTextures)
				{
					return;
				}

				if ((_textureRegion.Contains(e.Location))
					&& (_selectedGlyph != null))
				{
					buttonEditGlyph.Checked = true;
					InitializeGlyphEditTransition(true);
					return;
				}

				if ((_sortedTextures == null)
				    || (_textures.Length < 2))
				{
					return;
				}

				// Move to the font texture that we clicked on.
				foreach (GorgonSprite backSprite in _sortedTextures.Reverse())
				{
					var spriteSpace = new RectangleF(backSprite.Position - (Vector2.Modulate(backSprite.Anchor, backSprite.Scale)),
						                                backSprite.ScaledSize);

					if (!spriteSpace.Contains(e.Location))
					{
						continue;
					}

					int arrayIndex = Array.IndexOf(_textures, backSprite.Texture);

					if (arrayIndex == -1)
					{
						continue;
					}

					_sortedTextures = null;
					_content.CurrentState = arrayIndex < _currentTextureIndex ? DrawState.PrevTexture : DrawState.NextTexture;
					_currentTextureIndex = arrayIndex;
					UpdateGlyphRegions();

					return;
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
		/// Function to create the glyph texture.
		/// </summary>
		/// <param name="currentTexture">The currently selected texture.</param>
		/// <param name="textureSize">Size of the glyph texture.</param>
		/// <param name="glyphRegion">The glyph region on the source texture.</param>
	    private void CreateGlyphTexture(GorgonTexture2D currentTexture, Size textureSize, Rectangle glyphRegion)
	    {
			_editGlyph = _content.Graphics.Output.CreateRenderTarget("Glyph",
																	 new GorgonRenderTarget2DSettings
																	 {
																		 Width = textureSize.Width,
																		 Height = textureSize.Height,
																		 Format = BufferFormat.R8G8B8A8_UIntNormal
																	 });

			_content.Renderer.Target = _editGlyph;
			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.None;
			_content.Renderer.Clear(GorgonColor.Transparent);
			_content.Renderer.Drawing.Blit(currentTexture,
										   new RectangleF(Vector2.Zero, textureSize),
										   new RectangleF(currentTexture.ToTexel(glyphRegion.Location),
														  currentTexture.ToTexel(glyphRegion.Size)));

			_content.Renderer.Target = null;
	    }

		/// <summary>
		/// Function to update the selected texture region.
		/// </summary>
		/// <param name="currentTexture">The currently selected texture.</param>
	    private void UpdateTextureRegion(GorgonTexture2D currentTexture)
		{
		    var textureMidpoint = new Vector2((currentTexture.Settings.Width * _currentZoom / 2.0f),
		                                      (currentTexture.Settings.Height * _currentZoom) / 2.0f);

		    Vector2.Subtract(ref _panelMidPoint, ref textureMidpoint, out _textureOffset);

			if (panelTextures.HorizontalScroll.Visible)
			{
				_textureOffset.X = panelTextures.AutoScrollPosition.X;
			}

			if (panelTextures.VerticalScroll.Visible)
			{
				_textureOffset.Y = panelTextures.AutoScrollPosition.Y;
			}

			_textureRegion = new RectangleF(_textureOffset, ((Vector2)currentTexture.Settings.Size) * _currentZoom);
	    }

	    /// <summary>
	    /// Function to update the glyph editor area on the screen after a resize.
	    /// </summary>
	    private void UpdateGlyphEditor()
	    {
			GorgonTexture2D currentTexture = _selectedGlyph.Texture;

			UpdateTextureRegion(currentTexture);

			var textureRegion = new Rectangle((int)_textureRegion.X,
			                                  (int)_textureRegion.Y,
			                                  (int)_textureRegion.Width,
			                                  (int)_textureRegion.Height);
		    var glyphRegion = _selectedGlyph.GlyphCoordinates;
		    float aspect = (float)glyphRegion.Width / glyphRegion.Height;

		    if (aspect > 1.0f)
		    {
			    textureRegion.Height = (int)(textureRegion.Height / aspect);
			    textureRegion.Y = (int)(_panelMidPoint.Y - (textureRegion.Height * 0.5f));
		    }
		    else
		    {
			    textureRegion.Width = (int)(textureRegion.Width * aspect);
			    textureRegion.X = (int)(_panelMidPoint.X - (textureRegion.Width * 0.5f));
		    }

		    if (_editGlyph != null)
		    {
			    _editGlyph.Dispose();
			    _editGlyph = null;
		    }

		    CreateGlyphTexture(currentTexture, textureRegion.Size, glyphRegion);
		    _glyphSprite.Texture = _editGlyph;
		    if (_editGlyphAnimation.CurrentAnimation == null)
		    {
			    _glyphSprite.ScaledSize = Size.Round(textureRegion.Size);
			    _glyphBackgroundSprite.Size = Size.Round(textureRegion.Size);
			    _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(_glyphBackgroundSprite.Size));
		    }
		    _content.CurrentState = DrawState.GlyphEdit;
	    }

        /// <summary>
        /// Function to initialize the texture sprites.
        /// </summary>
        private void InitializeTextureSprites()
        {
            _textureSprites.Clear();

            // Create our sprites.
            var sprites = from texture in _textures
                          let spriteSettings = new GorgonSpriteSettings
                                               {
                                                   InitialPosition = Vector2.Zero,
                                                   Anchor = new Vector2(_content.Font.Settings.TextureSize.Width / 2.0f,
                                                                        _content.Font.Settings.TextureSize.Height / 2.0f),
                                                   Texture = texture,
                                                   TextureRegion = new RectangleF(0, 0, 1, 1),
                                                   Size = _content.Font.Settings.TextureSize
                                               }
                          select _content.Renderer.Renderables.CreateSprite(texture.Name, spriteSettings);

            _textureSprites.AddRange(sprites);
        }

	    /// <summary>
		/// Function to initialize the editor transition.
		/// </summary>
		/// <param name="editOn">TRUE when editing a glyph, FALSE when exiting the editor.</param>
	    private void InitializeGlyphEditTransition(bool editOn)
	    {
			// Move back to the selected texture index.
			if (_textures[_currentTextureIndex] != _selectedGlyph.Texture)
			{
				_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
				UpdateGlyphRegions();
			}

			GorgonTexture2D currentTexture = _selectedGlyph.Texture;
			var textureRegion = new Rectangle((int)_textureRegion.X,
			                                  (int)_textureRegion.Y,
			                                  (int)_textureRegion.Width,
			                                  (int)_textureRegion.Height);
			var glyphRegion = _selectedGlyph.GlyphCoordinates;
			var glyphScreenRegion = _glyphRegions[_selectedGlyph];

			glyphScreenRegion.X = glyphScreenRegion.X + textureRegion.X;
			glyphScreenRegion.Y = glyphScreenRegion.Y + textureRegion.Y;

			float aspect = (float)glyphRegion.Width / glyphRegion.Height;

			if (aspect > 1.0f)
			{
				textureRegion.Height = (int)(textureRegion.Height / aspect);

				if (!panelTextures.VerticalScroll.Visible)
				{
					textureRegion.Y = (int)(_panelMidPoint.Y - (textureRegion.Height * 0.5f));
				}
				else
				{
					textureRegion.Y = panelTextures.AutoScrollPosition.Y;
				}
			}
			else
			{
				textureRegion.Width = (int)(textureRegion.Width * aspect);
				if (!panelTextures.HorizontalScroll.Visible)
				{
					textureRegion.X = (int)(_panelMidPoint.X - (textureRegion.Width * 0.5f));
				}
				else
				{
					textureRegion.X = panelTextures.AutoScrollPosition.X;
				}
			}

			if ((_editGlyph != null) && (editOn))
			{
				_editGlyph.Dispose();
				_editGlyph = null;
			}

			// Create the target.
			if (_editGlyph == null)
			{
				CreateGlyphTexture(currentTexture, textureRegion.Size, glyphRegion);
			}

			_editBackgroundAnimation.Stop();
			_editGlyphAnimation.Stop();

			var transitionAnimation = _editGlyphAnimation["TransitionTo"];

		    transitionAnimation.Tracks["ScaledSize"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Size);
			transitionAnimation.Tracks["Position"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Location);
			transitionAnimation.Tracks["ScaledSize"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
			                                                                                textureRegion.Size);
			transitionAnimation.Tracks["Position"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
			                                                                            textureRegion.Location);

		    _glyphSprite.Texture = _editGlyph;

		    _glyphBackgroundSprite.Scale = new Vector2(1);
		    _glyphBackgroundSprite.Position = _textureRegion.Location;
			_glyphBackgroundSprite.Size = _textureRegion.Size;
		    _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(_textureRegion.Size));

			if (editOn)
			{
				_editBackgroundAnimation[0].Speed = 1.0f;
				transitionAnimation.Speed = 1.0f;

				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_END_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.stop_16x16;
			}
			else
			{
				_editBackgroundAnimation[0].Speed = -1.0f;
				transitionAnimation.Speed = -1.0f;
				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.edit_16x16;
			}

			_editGlyphAnimation.Play(_glyphSprite, "TransitionTo");
			_editBackgroundAnimation.Play(_glyphBackgroundSprite, "GlyphBGColor");
			_content.CurrentState = editOn ? DrawState.ToGlyphEdit : DrawState.FromGlyphEdit;
	    }

		/// <summary>
		/// Function to draw the glyph edit transition.
		/// </summary>
	    public void DrawGlyphEditTransition()
		{
			DrawFontTexture();

			_glyphBackgroundSprite.Draw();
			_glyphSprite.Draw();

			_editBackgroundAnimation.Update();
			_editGlyphAnimation.Update();

			if (_editBackgroundAnimation.CurrentAnimation != null)
			{
				return;
			}

			_content.CurrentState = _content.CurrentState == DrawState.ToGlyphEdit
				                        ? DrawState.GlyphEdit
				                        : DrawState.DrawFontTextures;
			ValidateControls();
		}

		/// <summary>
		/// Function to draw the glyph editor screen.
		/// </summary>
	    public void DrawGlyphEdit()
		{
			DrawFontTexture();
			
			var region = new RectangleF(panelTextures.Width / 2.0f - _editGlyph.Settings.Width / 2.0f,
			                            panelTextures.Height / 2.0f - _editGlyph.Settings.Height / 2.0f,
			                            _editGlyph.Settings.Width,
			                            _editGlyph.Settings.Height);

			if (panelTextures.HorizontalScroll.Visible)
			{
				region.X = panelTextures.AutoScrollPosition.X;
			}

			if (panelTextures.VerticalScroll.Visible)
			{
				region.Y = panelTextures.AutoScrollPosition.Y;
			}
			
			_glyphSprite.Position = region.Location;
			_glyphSprite.ScaledSize = region.Size;
			_glyphBackgroundSprite.Position = _textureRegion.Location;
			_glyphBackgroundSprite.ScaledSize = _textureRegion.Size;
			_glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(_textureRegion.Size));

			_glyphBackgroundSprite.Draw();
			// TODO: Make an option to show or hide the alpha.
			_glyphSprite.Draw();
		}

        /// <summary>
        /// Funciton to calculate the position and scale of the texture sprites.
        /// </summary>
        private void ArrangeTextureSprites()
        {
            for (int i = 0; i < _textureSprites.Count; ++i)
            {
                GorgonSprite sprite = _textureSprites[i];

                if ((i == _currentTextureIndex)
					&& (_content.CurrentState != DrawState.NextTexture)
					&& (_content.CurrentState != DrawState.PrevTexture))
                {
                    sprite.Scale = new Vector2(1);
                    continue;
                }
                
                float delta = (i - _indexTransition) / _textureSprites.Count;
                float deltaAbs = delta.Abs();
                
                sprite.Scale = new Vector2((1.0f - deltaAbs * 0.5f) * _currentZoom);
                sprite.Position = new Vector2(_panelMidPoint.X + delta * _content.FontTextureSize.Width * 2.0f * _currentZoom,
                                              _panelMidPoint.Y);

                sprite.Color = new GorgonColor(0.753f,
                                               0.753f,
                                               0.753f,
                                               (1.0f - deltaAbs));
                
                sprite.SmoothingMode = SmoothingMode.Smooth;
            }

	        if ((_content.CurrentState != DrawState.NextTexture)
	            && (_content.CurrentState != DrawState.PrevTexture))
	        {
		        _sortedTextures = _textureSprites.Where(item => item.Texture != _textures[_currentTextureIndex])
		                                         .OrderBy(item => item.Scale.X);
	        }
	        else
	        {
		        _sortedTextures = _textureSprites.OrderBy(item => item.Scale.X);
	        }
        }

        /// <summary>
        /// Function to draw the font texture(s).
        /// </summary>
        public void DrawFontTexture()
        {
            GorgonTexture2D currentTexture = _textures[_currentTextureIndex];
            
            if (_textureSprites.Count > 1)
            {
	            if (_indexTransition.EqualsEpsilon(_currentTextureIndex, 0.01f))
	            {
		            _content.CurrentState = DrawState.DrawFontTextures;
	            }

                if ((_sortedTextures == null)
					|| (_content.CurrentState == DrawState.NextTexture)
					|| (_content.CurrentState == DrawState.PrevTexture))
                {
                    ArrangeTextureSprites();

                    Debug.Assert(_sortedTextures != null, "No sorted textures.");
                }

                foreach (GorgonSprite backSprite in _sortedTextures)
                {
                    float color = backSprite.Color.Red * (1.0f - backSprite.Color.Alpha) * 0.5f;
                    _content.Renderer.Drawing.FilledRectangle(new RectangleF(backSprite.Position - (Vector2.Modulate(backSprite.Anchor, backSprite.Scale)), backSprite.ScaledSize),
                                                              new GorgonColor(color, color, color, backSprite.Opacity));
                    backSprite.Draw();
                }

				if ((_content.CurrentState == DrawState.NextTexture)
					|| (_content.CurrentState == DrawState.PrevTexture))
	            {
		            _indexTransition += (_currentTextureIndex - _indexTransition) * GorgonTiming.Delta * 4.0f;
					return;
	            }

		        _indexTransition = _currentTextureIndex;
            }

            UpdateTextureRegion(currentTexture);

            // Fill the background so we can clearly see the first page.
            _content.Renderer.Drawing.FilledRectangle(_textureRegion, Color.Black);

            if (_content.CurrentState != DrawState.GlyphEdit)
            {
                // Draw the borders around each glyph.
                foreach (var glyph in _glyphRegions)
                {
                    var glyphRect = glyph.Value;
                    glyphRect.X += _textureOffset.X;
                    glyphRect.Y += _textureOffset.Y;

                    _content.Renderer.Drawing.DrawRectangle(glyphRect, Color.Red);
                }
            }

            GorgonSprite sprite = _textureSprites[_currentTextureIndex];
            sprite.SmoothingMode = SmoothingMode.None;
            sprite.Color = GorgonColor.White;
            sprite.Scale = new Vector2(_currentZoom);

            // If the scroller is active, then position it based on that.
            if ((!panelTextures.HorizontalScroll.Visible)
                && (!panelTextures.VerticalScroll.Visible))
            {
                sprite.Position = _panelMidPoint;
            }
            else
            {
                sprite.Position = new Vector2(_textureRegion.Left + sprite.ScaledSize.X / 2.0f,
                                              _textureRegion.Top + sprite.ScaledSize.Y / 2.0f);
            }

            sprite.Draw();

			// If the editor is visible, then don't draw the selected texture.
			if (_content.CurrentState == DrawState.GlyphEdit)
			{
				return;
			}

			RectangleF rect;

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

			if (_hoverGlyph != null)
			{
				rect = _glyphRegions[_hoverGlyph];
				rect.X += _textureOffset.X;
				rect.Y += _textureOffset.Y;

				_patternSprite.Position = rect.Location;
				_patternSprite.Size = rect.Size;
				_patternSprite.Color = new GorgonColor(1, 0, 0, 0.4f);
				_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X,
							                                    _selectorBackGroundPos.Y,
							                                    rect.Width / _pattern.Settings.Width,
							                                    rect.Height / _pattern.Settings.Height);
				_patternSprite.Draw();
			}

			if ((_selectedGlyph == null) || (_selectedGlyph.Texture != currentTexture))
			{
				return;
			}

			rect = _glyphRegions[_selectedGlyph];
			rect.X += _textureOffset.X;
			rect.Y += _textureOffset.Y;

			_patternSprite.Position = rect.Location;
			_patternSprite.Size = rect.Size;
			_patternSprite.Color = new GorgonColor(0.25f, 0.25f, 1.0f, 0.4f);
			_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X,
			                                              _selectorBackGroundPos.Y,
			                                              rect.Width / _pattern.Settings.Width,
			                                              rect.Height / _pattern.Settings.Height);
			_patternSprite.Draw();
	    }
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentPanel"/> class.
        /// </summary>
        public GorgonFontContentPanel()
        {
            InitializeComponent();

			MouseWheel += PanelDisplay_MouseWheel;
	    }
        #endregion
	}
}
