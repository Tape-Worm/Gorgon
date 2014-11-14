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
// Created: Tuesday, April 15, 2014 10:10:21 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// UI for the font editor preferences.
    /// </summary>
    public partial class PanelFontPreferences 
        : PreferencePanel
    {
        #region Variables.
        // The shadow opacity.
        private int _shadowOpacity;
        // Preview text color.
        private Color _textColor;
        // Preview background color.
        private Color _backColor;
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the MouseDoubleClick event of the panelShadowOpacity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelShadowOpacity_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GetShadowAlpha();
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the panelTextColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelTextColor_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GetColor(ref _textColor);
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the panelBackColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelBackColor_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GetColor(ref _backColor);
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the panelShadowOpacity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void panelShadowOpacity_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if ((e.KeyCode != Keys.Enter)
                && (e.KeyCode != Keys.Space))
            {
                return;
            }

            GetShadowAlpha();
            e.IsInputKey = true;
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the panelTextColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void panelTextColor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if ((e.KeyCode != Keys.Enter)
                && (e.KeyCode != Keys.Space))
            {
                return;
            }

            GetColor(ref _textColor);
            e.IsInputKey = true;
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the panelBackColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void panelBackColor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if ((e.KeyCode != Keys.Enter)
                && (e.KeyCode != Keys.Space))
            {
                return;
            }

            GetColor(ref _backColor);
            e.IsInputKey = true;
        }

        /// <summary>
        /// Handles the TextChanged event of the textPreviewText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void textPreviewText_TextChanged(object sender, EventArgs e)
        {
            if (textPreviewText.TextLength == 0)
            {
                textPreviewText.Text = Resources.GORFNT_DEFAULT_PREVIEW_TEXT;
            }

            ValidateControls();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the checkShadowEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void checkShadowEnabled_CheckedChanged(object sender, EventArgs e)
        {
            ValidateControls();
        }

        /// <summary>
        /// Handles the Paint event of the panelShadowOpacity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelShadowOpacity_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            using(var brush = new SolidBrush(Color.FromArgb(_shadowOpacity, Color.Black)))
            {
                e.Graphics.FillRectangle(brush, panelShadowOpacity.ClientRectangle);   
            }
        }

        /// <summary>
        /// Handles the Paint event of the panelTextColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelTextColor_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            using (var brush = new SolidBrush(_textColor))
            {
                e.Graphics.FillRectangle(brush, panelTextColor.ClientRectangle);
            }
        }

        /// <summary>
        /// Handles the Paint event of the panelBackColor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void panelBackColor_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            using (var brush = new SolidBrush(_backColor))
            {
                e.Graphics.FillRectangle(brush, panelBackColor.ClientRectangle);
            }
        }

        /// <summary>
        /// Function to get the preview text color.
        /// </summary>
        /// <param name="color">Color to update.</param>
        private void GetColor(ref Color color)
        {
            ColorPickerDialog colorDialog = null;

            try
            {
                colorDialog = new ColorPickerDialog
                {
                    OldColor = color
                };

                if (colorDialog.ShowDialog(ParentForm) != DialogResult.OK)
                {
                    return;
                }

                color = colorDialog.SelectedColor;
                panelTextColor.Refresh();
                panelBackColor.Refresh();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                if (colorDialog != null)
                {
                    colorDialog.Dispose();
                }
            }
        }

        /// <summary>
        /// Function to retrieve a new shadow alpha color.
        /// </summary>
        private void GetShadowAlpha()
        {
            AlphaChannelDialog picker = null;

            try
            {
                picker = new AlphaChannelDialog
                {
                    Text = Resources.GORFNT_DLG_SHADOW_ALPHA_CAPTION,
                    SelectedAlphaValue = _shadowOpacity / 255.0f
                };

                if (picker.ShowDialog(ParentForm) != DialogResult.OK)
                {
                    return;
                }

                _shadowOpacity = (int)(picker.SelectedAlphaValue * 255);
                panelShadowOpacity.Refresh();
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
        /// Function to validate the controls on the form.
        /// </summary>
        private void ValidateControls()
        {
            numericShadowOffsetX.Enabled =
                numericShadowOffsetY.Enabled =
                labelSep.Enabled =
                labelShadowOffset.Enabled =
                labelShadowOpacity.Enabled = panelShadowOpacity.Enabled = checkShadowEnabled.Checked;
        }

        /// <summary>
        /// Function to localize the controls on the form.
        /// </summary>
        protected override void LocalizeControls()
        {
	        labelFontEditorSettings.Text = Resources.GORFNT_TEXT_FONT_EDITOR_SETTINGS;
	        checkShowAnimations.Text = Resources.GORFNT_TEXT_SHOW_ANIMATIONS;
            labelGlyphEdit.Text = Resources.GORFNT_TEXT_GLYPH_EDIT_SETTINGS;
            labelPreview.Text = Resources.GORFNT_TEXT_FONT_PREVIEW_SETTINGS;
            checkZoomSnap.Text = Resources.GORFNT_TEXT_SNAP_ZOOM;
            labelZoomWindowSize.Text = Resources.GORFNT_TEXT_ZOOM_WINSIZE;
            labelZoomAmount.Text = Resources.GORFNT_TEXT_ZOOM_AMOUNT;
            checkShadowEnabled.Text = Resources.GORFNT_TEXT_ENABLE_SHADOW;
            labelShadowOffset.Text = string.Format("{0}:", Resources.GORFNT_TEXT_SHADOW_OFFSET);
            labelShadowOpacity.Text = string.Format("{0}:", Resources.GORFNT_TEXT_SHADOW_OPACITY);
            labelTextColor.Text = string.Format("{0}:", Resources.GORFNT_TEXT_FOREGROUND_COLOR);
            labelBackgroundColor.Text = string.Format("{0}:", Resources.GORFNT_TEXT_BACKGROUND_COLOR);
            labelPreviewText.Text = string.Format("{0}:", Resources.GORFNT_TEXT_PREVIEW_TEXT);
	        labelBlendMode.Text = Resources.GORFNT_TEXT_BLEND_MODE;
	        comboBlendMode.Items.Add(Resources.GORFNT_TEXT_BLEND_MOD);
			comboBlendMode.Items.Add(Resources.GORFNT_TEXT_BLEND_ADD);
        }

        /// <summary>
        /// Function to commit any settings.
        /// </summary>
        public override void CommitSettings()
        {
            GorgonFontPlugInSettings settings = GorgonFontEditorPlugIn.Settings;
            float shadowOpacity = _shadowOpacity / 255.0f;

	        settings.ShowAnimations = checkShowAnimations.Checked;

            settings.ZoomWindowSnap = checkZoomSnap.Checked;
            settings.ZoomWindowSize = (int)numericZoomWindowSize.Value;
            settings.ZoomWindowScaleFactor = (float)numericZoomAmount.Value;

            settings.ShadowEnabled = checkShadowEnabled.Checked;
            if (settings.ShadowEnabled)
            {
                settings.ShadowOffset = new Point((int)numericShadowOffsetX.Value, (int)numericShadowOffsetY.Value);
                settings.ShadowOpacity = shadowOpacity;
            }

            settings.TextColor = _textColor.ToArgb();
            settings.BackgroundColor = _backColor.ToArgb();

            settings.SampleText = textPreviewText.Text;

	        settings.BlendMode = comboBlendMode.Text;

            // Persist to the plug-in settings file.
            settings.Save();
        }

        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            base.InitializeSettings();

            try
            {
	            checkShowAnimations.Checked = GorgonFontEditorPlugIn.Settings.ShowAnimations;
                checkZoomSnap.Checked = GorgonFontEditorPlugIn.Settings.ZoomWindowSnap;
                numericZoomAmount.Value = (decimal)GorgonFontEditorPlugIn.Settings.ZoomWindowScaleFactor;
                numericZoomWindowSize.Value = GorgonFontEditorPlugIn.Settings.ZoomWindowSize;
                checkShadowEnabled.Checked = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
                numericShadowOffsetX.Value = GorgonFontEditorPlugIn.Settings.ShadowOffset.X;
                numericShadowOffsetY.Value = GorgonFontEditorPlugIn.Settings.ShadowOffset.Y;
                _shadowOpacity = (int)(GorgonFontEditorPlugIn.Settings.ShadowOpacity * 255);
                _textColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.TextColor);
                _backColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor);
                textPreviewText.Text = GorgonFontEditorPlugIn.Settings.SampleText;

	            comboBlendMode.Text = comboBlendMode.Items.Contains(GorgonFontEditorPlugIn.Settings.BlendMode)
		                                  ? GorgonFontEditorPlugIn.Settings.BlendMode
		                                  : comboBlendMode.Items[0].ToString();


                panelShadowOpacity.Refresh();
                panelTextColor.Refresh();
                panelBackColor.Refresh();
            }
            catch (GorgonException ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();   
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelFontPreferences"/> class.
        /// </summary>
        public PanelFontPreferences()
        {
            InitializeComponent();
        }
        #endregion
    }
}
