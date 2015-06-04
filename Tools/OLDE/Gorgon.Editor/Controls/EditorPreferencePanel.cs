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
// Created: Thursday, April 11, 2013 9:10:43 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor
{
    /// <summary>
    /// Preferences for the editor.
    /// </summary>
    partial class EditorPreferencePanel
        : PreferencePanel
    {
        #region Value Types.
        /// <summary>
        /// An image editor drop down item.
        /// </summary>
        private struct ImageEditorDropDownItem 
        {
            #region Variables.
            /// <summary>
            /// The image editor plug-in.
            /// </summary>
            public readonly IImageEditorPlugIn PlugIn;
            #endregion

            #region Methods.
            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(PlugIn.Description) ? PlugIn.Name : PlugIn.Description;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(PlugIn);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is ImageEditorDropDownItem)
                {
                    return ((ImageEditorDropDownItem)obj).PlugIn == PlugIn;
                }

                return base.Equals(obj);
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="ImageEditorDropDownItem"/> struct.
            /// </summary>
            /// <param name="plugIn">The plug in.</param>
            public ImageEditorDropDownItem(IImageEditorPlugIn plugIn)
            {
                PlugIn = plugIn;
            }
            #endregion
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Click event of the checkAnimateLogo control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkAnimateLogo_Click(object sender, EventArgs e)
		{
			labelAnimateSpeed.Enabled = numericAnimateSpeed.Enabled = checkAnimateLogo.Checked;
		}

		/// <summary>
        /// Handles the Enter event of the textPlugInLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void textPlugInLocation_Enter(object sender, EventArgs e)
        {
            textPlugInLocation.Select(0, textPlugInLocation.Text.Length);
        }

        /// <summary>
        /// Handles the Enter event of the textScratchLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void textScratchLocation_Enter(object sender, EventArgs e)
        {
            textScratchLocation.Select(0, textScratchLocation.Text.Length);
        }

        /// <summary>
        /// Handles the Click event of the buttonPlugInLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonPlugInLocation_Click(object sender, EventArgs e)
        {
            try
            {
                if ((!string.IsNullOrWhiteSpace(Program.Settings.PlugInDirectory))
                    && (Directory.Exists(Program.Settings.PlugInDirectory)))
                {
                    dialogPlugInLocation.SelectedPath = Program.Settings.PlugInDirectory;
                }

	            if (dialogPlugInLocation.ShowDialog(ParentForm) != DialogResult.OK)
	            {
		            return;
	            }

	            textPlugInLocation.Text = dialogPlugInLocation.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar);
	            textPlugInLocation.Select(0, textPlugInLocation.Text.Length);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonScratch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonScratch_Click(object sender, EventArgs e)
        {
	        ScratchAccessibility result = ScratchArea.SetScratchLocation();

	        if (result != ScratchAccessibility.Accessible)
	        {
		        if (result == ScratchAccessibility.SystemArea)
		        {
					GorgonDialogs.ErrorBox(null, Resources.GOREDIT_ERR_CANNOT_USESYS_SCRATCH);
		        }

		        return;
	        }

	        textScratchLocation.Text = Program.Settings.ScratchPath;
	        textScratchLocation.Select(0, textScratchLocation.Text.Length);
        }

        /// <summary>
        /// Function to fill the image editor list.
        /// </summary>
        private void FillImageEditors()
        {
            comboImageEditor.Items.Clear();

            foreach (KeyValuePair<string, ContentPlugIn> plugIn in
                PlugIns.ContentPlugIns.Where(item => item.Value is IImageEditorPlugIn)
                .OrderBy(item => item.Value.Name))
            {
                var result = new ImageEditorDropDownItem((IImageEditorPlugIn)plugIn.Value);
                comboImageEditor.Items.Add(result);

                if (string.Equals(result.PlugIn.Name,
                                  Program.Settings.DefaultImageEditor,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    comboImageEditor.SelectedItem = result;
                }
            }

            if ((string.IsNullOrWhiteSpace(Program.Settings.DefaultImageEditor))
                && (comboImageEditor.Items.Count > 0))
            {
                comboImageEditor.SelectedIndex = 0;

                // Default to the first image editor.
                Program.Settings.DefaultImageEditor =
                    ((ImageEditorDropDownItem)comboImageEditor.SelectedItem).PlugIn.Name;
            }

            // Don't bother selecting if we've only got 1 editor or no editors at all.
            if (comboImageEditor.Items.Count < 2)
            {
                comboImageEditor.Enabled = false;
            }
        }

		/// <summary>
		/// Function to localize the text on the controls for the panel.
		/// </summary>
		/// <remarks>
		/// Override this method to supply localized text for any controls on the panel.
		/// </remarks>
	    protected internal override void LocalizeControls()
		{
			Text = Resources.GOREDIT_TEXT_EDITOR_PREFERENCES;
			labelPaths.Text = Resources.GOREDIT_TEXT_EDITOR_PATHS;
			labelOptions.Text = Resources.GOREDIT_TEXT_EDITOR_OPTIONS;
			labelScratchLocation.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_SCRATCH_LOCATION);
			labelPlugInLocation.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_PLUG_IN_LOCATION);
			labelImageEditor.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_IMAGE_EDITOR);
			checkAutoLoadFile.Text = Resources.GOREDIT_TEXT_LOAD_LAST_FILE;
		    checkAnimateLogo.Text = Resources.GOREDIT_TEXT_ANIMATE_LOGO;
		    labelAnimateSpeed.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_ANIMATE_SPEED);

			toolHelp.SetToolTip(imageScratchHelp, Resources.GOREDIT_TIP_SCRATCH_LOCATION);
			toolHelp.SetToolTip(imagePlugInHelp, Resources.GOREDIT_TIP_PLUG_IN_LOCATION);
			toolHelp.SetToolTip(imageImageEditorHelp, Resources.GOREDIT_TIP_IMAGE_EDITOR);
		}

	    /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            textScratchLocation.Text = Program.Settings.ScratchPath;
            textPlugInLocation.Text = Program.Settings.PlugInDirectory;
            checkAutoLoadFile.Checked = Program.Settings.AutoLoadLastFile;
		    checkAnimateLogo.Checked = Program.Settings.AnimateStartPageLogo;
		    var animSpeed = (decimal)Program.Settings.StartPageAnimationPulseRate;

		    if ((animSpeed >= numericAnimateSpeed.Minimum)
		        && (animSpeed <= numericAnimateSpeed.Maximum))
		    {
			    numericAnimateSpeed.Value = (decimal)Program.Settings.StartPageAnimationPulseRate;
		    }

		    FillImageEditors();

			checkAnimateLogo_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to commit any settings.
        /// </summary>
        public override void CommitSettings()
        {
            if (!string.IsNullOrWhiteSpace(textScratchLocation.Text))                
            {
                Program.Settings.ScratchPath = textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar);
            }

            if (!string.IsNullOrWhiteSpace(textPlugInLocation.Text))
            {
                Program.Settings.PlugInDirectory = textPlugInLocation.Text.FormatDirectory(Path.DirectorySeparatorChar);
            }

            Program.Settings.DefaultImageEditor = comboImageEditor.Text;

	        var speed = (float)numericAnimateSpeed.Value;
	        if ((ContentManagement.Current == null) && 
				((!Program.Settings.StartPageAnimationPulseRate.EqualsEpsilon(speed))
				|| (Program.Settings.AnimateStartPageLogo != checkAnimateLogo.Checked)))
	        {
		        Program.Settings.AnimateStartPageLogo = checkAnimateLogo.Checked;
		        Program.Settings.StartPageAnimationPulseRate = speed;
				ContentManagement.EditorSettingsUpdated();
	        }
			
            Program.Settings.AutoLoadLastFile = checkAutoLoadFile.Checked;
        }

        /// <summary>
        /// Function to validate any settings on this panel.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the settings are valid, <b>false</b> if not.
        /// </returns>
        public override bool ValidateSettings()
        {
			if (ScratchArea.CanAccessScratch(textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar)) != ScratchAccessibility.Accessible)
            {
                return false;
            }

	        if (!string.Equals(textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar), Program.Settings.ScratchPath, StringComparison.OrdinalIgnoreCase))
	        {
		        GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_DLG_SCRATCH_LOC_CHANGE);
	        }

            if ((string.Equals(Program.Settings.DefaultImageEditor,
                               comboImageEditor.Text,
                               StringComparison.OrdinalIgnoreCase))
                && (ContentManagement.Current != null))
            {
                GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_DLG_IMAGE_EDITOR_CHANGED);
            }

	        try
	        {
		        if (!Directory.Exists(textPlugInLocation.Text.FormatDirectory(Path.DirectorySeparatorChar)))
                {
                    return false;
                }

		        if (!string.Equals(textPlugInLocation.Text.FormatDirectory(Path.DirectorySeparatorChar), Program.Settings.PlugInDirectory, StringComparison.OrdinalIgnoreCase))
		        {
			        GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_DLG_PLUGIN_LOC_CHANGE);
		        }
	        }
	        catch
            {
                // We don't care about exceptions at this point.
                return false;
            }

            return true;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPreferencePanel"/> class.
        /// </summary>
        public EditorPreferencePanel()
        {
            InitializeComponent();
        }
        #endregion
    }
}
