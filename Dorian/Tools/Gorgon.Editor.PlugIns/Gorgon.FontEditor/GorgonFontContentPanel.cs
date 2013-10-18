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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
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
		#region Classes
		/// <summary>
		/// Value type to hold the transition data.
		/// </summary>
	    class TransitionData
	    {
			#region Variables.
			private readonly float _time;						// Time for the transition.
			private readonly RectangleF _sourceRegion;			// The source region.
			private readonly RectangleF _destRegion;			// The destination region.
			private float _startTime;							// The starting time.
			private bool _hasEnded;								// Flag to indicate that the transition has started.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the current region.
			/// </summary>
			public RectangleF CurrentRegion
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the current time.
			/// </summary>
			public float CurrentTime
			{
				get;
				private set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to end the transition.
			/// </summary>
			public void EndTransition()
			{
				CurrentTime = 1.0f;
				UpdateTransition();
			}

			/// <summary>
			/// Function to update the transition.
			/// </summary>
			/// <returns>TRUE if the transition has ended, FALSE to continue.</returns>
			public bool UpdateTransition()
			{
				if (_hasEnded)
				{
					return true;
				}

				if (_startTime < 0)
				{
					_startTime = GorgonTiming.SecondsSinceStart;
				}

				CurrentTime = ((GorgonTiming.SecondsSinceStart - _startTime) / _time).Min(1.0f);

				CurrentRegion = new RectangleF(_sourceRegion.X + ((_destRegion.X - _sourceRegion.X) * CurrentTime),
				                               _sourceRegion.Y + ((_destRegion.Y - _sourceRegion.Y) * CurrentTime),
				                               _sourceRegion.Width + ((_destRegion.Width - _sourceRegion.Width) * CurrentTime),
				                               _sourceRegion.Height + ((_destRegion.Height - _sourceRegion.Height) * CurrentTime));

				_hasEnded = CurrentTime >= 1.0f;
				return _hasEnded;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="TransitionData"/> class.
			/// </summary>
			/// <param name="sourceRegion">The source region.</param>
			/// <param name="destRegion">The dest region.</param>
			/// <param name="time">The time.</param>
			public TransitionData(RectangleF sourceRegion, RectangleF destRegion, float time)
			{
				_time = time;
				CurrentRegion = _sourceRegion = sourceRegion;
				_destRegion = destRegion;
				_startTime = -1;
				CurrentTime = 0;
			}
			#endregion
	    }
		#endregion

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
	    private GorgonTexture2D[] _textures;
	    private TransitionData _animationTransition;
	    private GorgonRenderTarget2D _editGlyph;
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Click event of the buttonGlyphClip control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGlyphClip_Click(object sender, EventArgs e)
		{
			imageFileBrowser.FileExtensions.Clear();
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("dds", "Direct Draw Surface (*.dds)"));
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("png", "Portable Network Graphics (*.png)"));
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("tga", "Truevision Targa Files (*.tga)"));
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("bmp", "Windows Bitmap (*.bmp)"));
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("jpg", "Joint Photographics Experts Group (*.jpg)"));
			imageFileBrowser.FileExtensions.Add(new GorgonFileExtension("*", "All files (*.*)"));
			imageFileBrowser.ShowDialog(ParentForm);
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
				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _textures.Length)
				{
					_currentTextureIndex = _textures.Length - 1;
				}

				buttonPrevTexture.Enabled = _currentTextureIndex > 0 && (_content.CurrentState == DrawState.DrawFontTextures);
				buttonNextTexture.Enabled = _currentTextureIndex < _textures.Length - 1 && (_content.CurrentState == DrawState.DrawFontTextures);

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

	        buttonGlyphClip.Enabled =
		        buttonGlyphKern.Enabled =
		        buttonGlyphSizeSpace.Enabled = buttonEditGlyph.Enabled && _content.CurrentState == DrawState.GlyphEdit;

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
				_currentTextureIndex++;
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
				_currentTextureIndex--;
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

				UpdateGlyphRegions();

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

				UpdateGlyphRegions();

				_text.Font = _content.Font;

				// Kill any transition.
				if (_animationTransition != null)
				{
					_animationTransition.EndTransition();
					_animationTransition = null;
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
				if ((_selectedGlyph == null) || (_content.CurrentState != DrawState.DrawFontTextures))
				{
					return;
				}

				buttonEditGlyph.Checked = true;
				InitializeGlyphEditTransition(true);
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
			    textureRegion.Y = (int)((panelTextures.ClientSize.Height * 0.5f) - (textureRegion.Height * 0.5f));
		    }
		    else
		    {
			    textureRegion.Width = (int)(textureRegion.Width * aspect);
			    textureRegion.X = (int)((panelTextures.ClientSize.Width * 0.5f) - (textureRegion.Width * 0.5f));
		    }

		    if (_editGlyph != null)
		    {
			    _editGlyph.Dispose();
			    _editGlyph = null;
		    }

		    CreateGlyphTexture(currentTexture, textureRegion.Size, glyphRegion);
		    _content.CurrentState = DrawState.GlyphEdit;
	    }

	    /// <summary>
		/// Function to initialize the editor transition.
		/// </summary>
		/// <param name="editOn">TRUE when editing a glyph, FALSE when exiting the editor.</param>
	    private void InitializeGlyphEditTransition(bool editOn)
	    {
		    float animationTime = 0.5f;

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
				textureRegion.Y = (int)((panelTextures.ClientSize.Height * 0.5f) - (textureRegion.Height * 0.5f));
			}
			else
			{
				textureRegion.Width = (int)(textureRegion.Width * aspect);
				textureRegion.X = (int)((panelTextures.ClientSize.Width * 0.5f) - (textureRegion.Width * 0.5f));
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

			if (editOn)
			{
				if (_animationTransition != null)
				{
					glyphScreenRegion = _animationTransition.CurrentRegion;
					animationTime = _animationTransition.CurrentTime * animationTime;
				}

				// Calculate the destination rectangle.
				_animationTransition = new TransitionData(glyphScreenRegion,
					                                        textureRegion,
					                                        animationTime);
				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_END_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.stop_16x16;
			}
			else
			{
				if (_animationTransition != null)
				{
					textureRegion = Rectangle.Round(_animationTransition.CurrentRegion);
					animationTime = _animationTransition.CurrentTime * animationTime;
				}

				_animationTransition = new TransitionData(textureRegion, glyphScreenRegion, animationTime);
				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.edit_16x16;
			}

			_content.CurrentState = editOn ? DrawState.ToGlyphEdit : DrawState.FromGlyphEdit;
	    }

		/// <summary>
		/// Function to draw the glyph edit transition.
		/// </summary>
	    public void DrawGlyphEditTransition()
		{
			float color = _content.CurrentState == DrawState.ToGlyphEdit
				              ? _animationTransition.CurrentTime
				              : (1.0f - _animationTransition.CurrentTime);
			
			DrawFontTexture();

			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;

			_content.Renderer.Drawing.FilledRectangle(_textureRegion,
			                                          new GorgonColor(
				                                          0.125f,
				                                          0.125f,
				                                          color * 0.7f,
				                                          color),
			                                          _pattern,
			                                          new RectangleF(Vector2.Zero, _pattern.ToTexel(_textureRegion.Size)));

			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;


			// TODO: Make an option to show/hide the alpha.
			//_content.Renderer.Drawing.BlendingMode = BlendingMode.None;
			_content.Renderer.Drawing.Blit(_editGlyph, _animationTransition.CurrentRegion);
			//_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			if (!_animationTransition.UpdateTransition())
			{
				return;
			}

			_content.CurrentState = _content.CurrentState == DrawState.ToGlyphEdit
				                        ? DrawState.GlyphEdit
				                        : DrawState.DrawFontTextures;

			_animationTransition = null;
			ValidateControls();
		}

		/// <summary>
		/// Function to draw the glyph editor screen.
		/// </summary>
	    public void DrawGlyphEdit()
		{
			DrawFontTexture();

			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.FilledRectangle(_textureRegion,
			                                          new GorgonColor(0.125f, 0.125f, 0.7f, 1.0f),
			                                          _pattern,
			                                          new RectangleF(Vector2.Zero, _pattern.ToTexel(_textureRegion.Size)));

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

			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;

			// TODO: Make an option to show or hide the alpha.
			//_content.Renderer.Drawing.BlendingMode = BlendingMode.None;
			_content.Renderer.Drawing.Blit(_editGlyph, region);
			//_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
		}

		/// <summary>
		/// Function to draw the font texture(s).
		/// </summary>
		public void DrawFontTexture()
		{
			float alpha;
			GorgonTexture2D currentTexture = _textures[_currentTextureIndex];
			_content.Renderer.Clear(PanelDisplay.BackColor);

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

			var texturePos =
				new Vector2(((panelTextures.ClientSize.Width / 2.0f) - (currentTexture.Settings.Width * _currentZoom) / 2.0f), 0);

			int colorAdjust = _content.CurrentState == DrawState.DrawFontTextures ? 192 : (int)(192 * 0.63f);

			// Show the textures that come before the current texture.
			for (int i = _currentTextureIndex - 1; i >= 0; i--)
			{
				var displayTexture = _textures[i];
				float range = _currentTextureIndex;
				float index = (_currentTextureIndex - 1) - i;

				alpha = (range - index) / range;

				var textureSize = new Vector2((_currentZoom * 0.75f * alpha) * currentTexture.Settings.Width,
					                            (_currentZoom * 0.75f * alpha) * currentTexture.Settings.Height);

				texturePos.Y = panelTextures.ClientSize.Height / 2.0f - textureSize.Y / 2.0f;
				texturePos.X -= textureSize.X + 8.0f;

				_content.Renderer.Drawing.FilledRectangle(new RectangleF(texturePos, textureSize),
					                                        Color.FromArgb((int)(alpha * (colorAdjust * 0.6667f)), colorAdjust, colorAdjust, colorAdjust),
					                                        displayTexture);
			}

			// Set to the middle.
			texturePos.X = ((panelTextures.ClientSize.Width / 2.0f) + (currentTexture.Settings.Width * _currentZoom) / 2.0f) +
				            8.0f;

			// Show the textures that come after the current texture.
			for (int i = _currentTextureIndex + 1; i < _textures.Length; i++)
			{
				var displayTexture = _textures[i];
				float range = _textures.Length - (_currentTextureIndex + 1);
				float index = i - (_currentTextureIndex + 1);

				alpha = (range - index) / range;

				var textureSize = new Vector2((_currentZoom * 0.75f * alpha) * currentTexture.Settings.Width,
					                            (_currentZoom * 0.75f * alpha) * currentTexture.Settings.Height);

				texturePos.Y = panelTextures.ClientSize.Height / 2.0f - textureSize.Y / 2.0f;

				_content.Renderer.Drawing.FilledRectangle(new RectangleF(texturePos, textureSize),
															Color.FromArgb((int)(alpha * (colorAdjust * 0.6667f)), colorAdjust, colorAdjust, colorAdjust),
					                                        displayTexture);

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

			panelTextures.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Width),
				                                        (int)System.Math.Ceiling(_currentZoom * currentTexture.Settings.Height));

			UpdateTextureRegion(currentTexture);

			// If the editor is visible, then don't draw the selected texture.
			if (_content.CurrentState == DrawState.GlyphEdit)
			{
				return;
			}

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

			if (_content.CurrentState != DrawState.DrawFontTextures)
			{
				return;
			}

			RectangleF rect;

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
