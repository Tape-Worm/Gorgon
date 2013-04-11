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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
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
        #region Variables.
        private ListViewItem[] _moveBuffer = null;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the scratch area is accessible.
        /// </summary>
        /// <param name="path">Path to the scratch area.</param>
        /// <returns>TRUE if accessible, FALSE if not.</returns>
        private bool CanAccessScratch()
        {
            string directoryName = textScratchLocation.Text.FormatDirectory(Path.DirectorySeparatorChar);

            // Ensure that the device exists or is ready.
            if (!Directory.Exists(Path.GetPathRoot(directoryName)))
            {
                GorgonDialogs.ErrorBox(ParentForm, "The location '" + Path.GetPathRoot(directoryName) + "' does not exist, or is not ready.");
                return false;
            }

            try
            {
                DirectoryInfo directoryInfo = null;

                // Do not allow access to a system location.
                if (Program.IsSystemLocation(directoryName))
                {
                    GorgonDialogs.ErrorBox(ParentForm, "Cannot set the scratch location to a system directory.");
                    return false;
                }

                directoryInfo = new DirectoryInfo(directoryName);
                if (!directoryInfo.Exists)
                {
                    // If we created the directory, then hide it.
                    directoryInfo.Create();
                    return true;
                }

                // Ensure that we have access to the directory.
                var acl = directoryInfo.GetAccessControl();

                // Ensure that we can actually write to this directory.
                FileInfo testWrite = new FileInfo(directoryName + "TestWrite.tst");
                using (Stream stream = testWrite.OpenWrite())
                {
                    stream.WriteByte(127);
                }
                testWrite.Delete();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
                return false;
            }

            return true;
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
        /// Handles the Click event of the buttonScratch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonScratch_Click(object sender, EventArgs e)
        {
            string scratchDirectory = Program.SetScratchLocation();

            if (!string.IsNullOrWhiteSpace(scratchDirectory))
            {
                textScratchLocation.Text = scratchDirectory;
                textScratchLocation.Select(0, scratchDirectory.Length);
            }
        }

        /// <summary>
        /// Function to validate the controls on the panel.
        /// </summary>
        private void ValidateControls()
        {
            buttonDeleteContentPlugIn.Enabled = listContentPlugIns.SelectedItems.Count > 0;
            buttonContentPlugInDown.Enabled = buttonContentPlugInUp.Enabled = (listContentPlugIns.SelectedItems.Count > 0) 
                                                                                && (listContentPlugIns.Items.Count > 0) 
                                                                                && (listContentPlugIns.SelectedItems.Count < listContentPlugIns.Items.Count);

            buttonContentPlugInUp.Enabled = buttonContentPlugInUp.Enabled 
                                            && (listContentPlugIns.SelectedItems[0].Index > 0);

            buttonContentPlugInDown.Enabled = buttonContentPlugInDown.Enabled
                                            && (listContentPlugIns.SelectedItems[listContentPlugIns.SelectedItems.Count - 1].Index < listContentPlugIns.Items.Count - 1);
        }

        /// <summary>
        /// Function to move selected list view items up or down.
        /// </summary>
        /// <param name="items">Items to move.</param>
        /// <param name="down">TRUE to move down, FALSE to move up.</param>
        private void MoveSelectedItems(ListView.SelectedListViewItemCollection items, bool down)
        {
            int moveCount = items.Count;

            if (items.Count > _moveBuffer.Length)
            {
                _moveBuffer = new ListViewItem[items.Count * 2];
            }

            items.CopyTo(_moveBuffer, 0);

            for (int i = 0; i < moveCount; i++)
            {
                ListViewItem item = _moveBuffer[i];
                int currentIndex = (down ? item.Index + 1 : item.Index - 1);

                listContentPlugIns.Items.Remove(item);
                listContentPlugIns.Items.Insert(currentIndex, item);
                item.Selected = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonContentPlugInDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonContentPlugInDown_Click(object sender, EventArgs e)
        {
            try
            {
                MoveSelectedItems(listContentPlugIns.SelectedItems, true);
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
        /// Handles the Click event of the buttonContentPlugInUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonContentPlugInUp_Click(object sender, EventArgs e)
        {
            try
            {
                MoveSelectedItems(listContentPlugIns.SelectedItems, false);
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
        /// Handles the SelectedIndexChanged event of the listContentPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listContentPlugIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateControls();
        }

        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            textScratchLocation.Text = Program.Settings.ScratchPath;

            listContentPlugIns.Items.Clear();

            listContentPlugIns.BeginUpdate();
            foreach (var plugIn in Program.ContentPlugIns)
            {
                ListViewItem item = new ListViewItem();

                item.Name = plugIn.Key;
                item.Text = plugIn.Value.Description;
                item.SubItems.Add(plugIn.Value.PlugInPath);

                listContentPlugIns.Items.Add(item);
            }

            listContentPlugIns.Items.Add("Dummy 1");
            listContentPlugIns.Items.Add("Dummy 2");
            listContentPlugIns.Items.Add("Dummy 3");
            listContentPlugIns.Items.Add("Dummy 4");
            listContentPlugIns.Items.Add("Dummy 5");

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            _moveBuffer = new ListViewItem[listContentPlugIns.Items.Count * 2];
        }

        /// <summary>
        /// Function to commit any settings.
        /// </summary>
        public override void CommitSettings()
        {
            Program.Settings.ScratchPath = textScratchLocation.Text;
        }

        /// <summary>
        /// Function to validate any settings on this panel.
        /// </summary>
        /// <returns>
        /// TRUE if the settings are valid, FALSE if not.
        /// </returns>
        public override bool ValidateSettings()
        {
            if (!CanAccessScratch())
            {
                return false;
            }
            else
            {
                if (string.Compare(textScratchLocation.Text, Program.Settings.ScratchPath, true) != 0)
                {
                    GorgonDialogs.InfoBox(ParentForm, "The scratch location has changed.  This will require that the application be restarted before this setting takes effect.");
                }
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
