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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Preferences for the editor.
    /// </summary>
    public partial class EditorPreferencePanel
        : PreferencePanel
    {
        #region Properties.

        #endregion

        #region Methods.
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
	        if (ScratchArea.SetScratchLocation() != ScratchAccessibility.Accessible)
	        {
		        return;
	        }

	        textScratchLocation.Text = Program.Settings.ScratchPath;
	        textScratchLocation.Select(0, textScratchLocation.Text.Length);
        }

        /// <summary>
        /// Handles the DoubleClick event of the listDisabledPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listDisabledPlugIns_DoubleClick(object sender, EventArgs e)
        {
            Point cursorLocation = listDisabledPlugIns.PointToClient(Cursor.Position);
            ListViewHitTestInfo hitTest = listDisabledPlugIns.HitTest(cursorLocation);

            if (hitTest.Item != null)
            {
                GorgonDialogs.ErrorBox(ParentForm, "Plug-In '" + hitTest.Item.Text + "' failed to load.  See details for information.", null, hitTest.Item.SubItems[1].Text);
            }

        }
        
        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            textScratchLocation.Text = Program.Settings.ScratchPath;
            textPlugInLocation.Text = Program.Settings.PlugInDirectory;
            checkAutoLoadFile.Checked = Program.Settings.AutoLoadLastFile;

            listContentPlugIns.BeginUpdate();
            listDisabledPlugIns.BeginUpdate();

            listDisabledPlugIns.Items.Clear();
            listContentPlugIns.Items.Clear();

            foreach (var plugIn in Gorgon.PlugIns)
            {
                var item = new ListViewItem();

                if ((plugIn is EditorPlugIn) || (plugIn is GorgonFileSystemProviderPlugIn))
                {
                    var editorPlugIn = plugIn as EditorPlugIn;

                    item.Name = plugIn.Name;
                    item.Text = plugIn.Description;

                    if (plugIn is ContentPlugIn)
                    {
                        item.SubItems.Add(Resources.GOREDIT_PLUGIN_TYPE_CONTENT);
                    }

                    if (plugIn is FileWriterPlugIn)
                    {
                        item.SubItems.Add(Resources.GOREDIT_PLUGIN_TYPE_FILE_WRITER);
                    }

                    if (plugIn is GorgonFileSystemProviderPlugIn)
                    {
                        item.SubItems.Add(Resources.GOREDIT_PLUGIN_TYPE_FILE_READER);
                    }
                    
                    if ((editorPlugIn != null) && (PlugIns.IsDisabled(editorPlugIn)))
                    {
                        item.SubItems[1].Text = Resources.GOREDIT_PLUGIN_TYPE_DISABLED;
                        item.ForeColor = Color.FromKnownColor(KnownColor.DimGray);
                        item.Tag = null;

                        // We've got a disabled plug-in, add to the secondary list view
                        // to show why the plug-in was disabled.
                        var disabledItem = new ListViewItem
                        {
                            Name = plugIn.Name,
                            Text = plugIn.Description,
                            Tag = plugIn
                        };

                        disabledItem.SubItems.Add(PlugIns.GetDisabledReason(plugIn));
                        disabledItem.SubItems.Add(plugIn.PlugInPath);

                        listDisabledPlugIns.Items.Add(disabledItem);
                    }
                    else
                    {
                        item.Tag = plugIn;
                    }

                    item.SubItems.Add(plugIn.PlugInPath);
                }

                listContentPlugIns.Items.Add(item);                
            }

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listDisabledPlugIns.EndUpdate();
            listDisabledPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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

            Program.Settings.AutoLoadLastFile = checkAutoLoadFile.Checked;
        }

        /// <summary>
        /// Function to validate any settings on this panel.
        /// </summary>
        /// <returns>
        /// TRUE if the settings are valid, FALSE if not.
        /// </returns>
        public override bool ValidateSettings()
        {
			if (ScratchArea.CanAccessScratch(textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar)) != ScratchAccessibility.Accessible)
            {
                return false;
            }

	        if (!string.Equals(textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar), Program.Settings.ScratchPath, StringComparison.OrdinalIgnoreCase))
	        {
		        GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_SETTING_SCRATCH_LOC_CHANGE_MSG);
	        }

	        try
	        {
		        if (!Directory.Exists(textPlugInLocation.Text.FormatDirectory(Path.DirectorySeparatorChar)))
                {
                    return false;
                }

		        if (!string.Equals(textPlugInLocation.Text.FormatDirectory(Path.DirectorySeparatorChar), Program.Settings.PlugInDirectory, StringComparison.OrdinalIgnoreCase))
		        {
			        GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_SETTING_PLUGIN_LOC_CHANGE_MSG);
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
