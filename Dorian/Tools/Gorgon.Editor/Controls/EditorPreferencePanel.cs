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
using System.Drawing;
using GorgonLibrary.IO;
using GorgonLibrary.FileSystem;
using GorgonLibrary.PlugIns;
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
            // Content plug-in.
            buttonDeleteContentPlugIn.Enabled = listContentPlugIns.SelectedItems.Count > 0;
            buttonContentPlugInDown.Enabled = buttonContentPlugInUp.Enabled = (listContentPlugIns.SelectedItems.Count > 0) 
                                                                                && (listContentPlugIns.Items.Count > 0) 
                                                                                && (listContentPlugIns.SelectedItems.Count < listContentPlugIns.Items.Count);

            buttonContentPlugInUp.Enabled = buttonContentPlugInUp.Enabled 
                                            && (listContentPlugIns.SelectedItems[0].Index > 0);

            buttonContentPlugInDown.Enabled = buttonContentPlugInDown.Enabled
                                            && (listContentPlugIns.SelectedItems[listContentPlugIns.SelectedItems.Count - 1].Index < listContentPlugIns.Items.Count - 1);


            // File writer plug-in.
            buttonDeleteWriterPlugIn.Enabled = listWriterPlugIns.SelectedItems.Count > 0;
            buttonWriterPlugInDown.Enabled = buttonWriterPlugInUp.Enabled = (listWriterPlugIns.SelectedItems.Count > 0)
                                                                                && (listWriterPlugIns.Items.Count > 0)
                                                                                && (listWriterPlugIns.SelectedItems.Count < listWriterPlugIns.Items.Count);

            buttonWriterPlugInUp.Enabled = buttonWriterPlugInUp.Enabled
                                            && (listWriterPlugIns.SelectedItems[0].Index > 0);

            buttonWriterPlugInDown.Enabled = buttonWriterPlugInDown.Enabled
                                            && (listWriterPlugIns.SelectedItems[listWriterPlugIns.SelectedItems.Count - 1].Index < listWriterPlugIns.Items.Count - 1);

            // File reader plug-in.
            buttonDeleteReaderPlugIn.Enabled = listReaderPlugIns.SelectedItems.Count > 0;
            buttonReaderPlugInDown.Enabled = buttonReaderPlugInUp.Enabled = (listReaderPlugIns.SelectedItems.Count > 0)
                                                                                && (listReaderPlugIns.Items.Count > 0)
                                                                                && (listReaderPlugIns.SelectedItems.Count < listReaderPlugIns.Items.Count);

            buttonReaderPlugInUp.Enabled = buttonReaderPlugInUp.Enabled
                                            && (listReaderPlugIns.SelectedItems[0].Index > 0);

            buttonReaderPlugInDown.Enabled = buttonReaderPlugInDown.Enabled
                                            && (listReaderPlugIns.SelectedItems[listReaderPlugIns.SelectedItems.Count - 1].Index < listReaderPlugIns.Items.Count - 1);
        }

        /// <summary>
        /// Function to move selected list view items up or down.
        /// </summary>
        /// <param name="items">Items to move.</param>
        /// <param name="down">TRUE to move down, FALSE to move up.</param>
        private void MoveSelectedItems(ListView.SelectedListViewItemCollection items, bool down)
        {
            int moveCount = items.Count;
            int currentIndex = -1;

            if (items.Count > _moveBuffer.Length)
            {
                _moveBuffer = new ListViewItem[items.Count * 2];
            }

            items.CopyTo(_moveBuffer, 0);

            if (!down)
            {
                for (int i = 0; i < moveCount; i++)
                {
                    ListViewItem item = _moveBuffer[i];
                    currentIndex = item.Index - 1;

                    listContentPlugIns.Items.Remove(item);
                    listContentPlugIns.Items.Insert(currentIndex, item);
                    item.Selected = true;
                }
            }
            else
            {
                for (int i = moveCount - 1; i >= 0; i--)
                {
                    ListViewItem item = _moveBuffer[i];
                    currentIndex = item.Index + 1;

                    listContentPlugIns.Items.Remove(item);
                    listContentPlugIns.Items.Insert(currentIndex, item);
                    item.Selected = true;
                }
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
                MoveSelectedItems(((ListView)sender).SelectedItems, true);
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
                MoveSelectedItems(((ListView)sender).SelectedItems, false);
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

            listContentPlugIns.BeginUpdate();
            listContentPlugIns.Items.Clear();
            foreach (var plugIn in Program.ContentPlugIns)
            {
                ListViewItem item = new ListViewItem();

                item.Name = plugIn.Key;
                item.Text = plugIn.Value.Description;
                item.SubItems.Add(plugIn.Value.PlugInPath);
                item.Tag = plugIn;

                if (Program.DisabledPlugIns.ContainsKey(plugIn.Value))
                {                    
                    item.ForeColor = Color.FromKnownColor(KnownColor.DimGray);
                }

                listContentPlugIns.Items.Add(item);
            }

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            listWriterPlugIns.BeginUpdate();
            listWriterPlugIns.Items.Clear();
            foreach (var plugIn in Program.WriterPlugIns)
            {
                ListViewItem item = new ListViewItem();

                item.Name = plugIn.Key;
                item.Text = plugIn.Value.Description;
                item.SubItems.Add(plugIn.Value.PlugInPath);
                item.Tag = plugIn;

                if (Program.DisabledPlugIns.ContainsKey(plugIn.Value))
                {
                    item.ForeColor = Color.FromKnownColor(KnownColor.DimGray);
                }

                listWriterPlugIns.Items.Add(item);
            }

            listWriterPlugIns.EndUpdate();
            listWriterPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            listReaderPlugIns.BeginUpdate();
            listReaderPlugIns.Items.Clear();
            foreach (var plugIn in Gorgon.PlugIns.Where(item => item is GorgonFileSystemProviderPlugIn))
            {
                ListViewItem item = new ListViewItem();

                item.Name = plugIn.Name;
                item.Text = plugIn.Description;
                item.SubItems.Add(Gorgon.PlugIns[plugIn.Name].PlugInPath);
                item.Tag = plugIn;

                listReaderPlugIns.Items.Add(item);
            }

            listReaderPlugIns.EndUpdate();
            listReaderPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            _moveBuffer = new ListViewItem[64];
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

        /// <summary>
        /// Handles the MouseMove event of the listContentPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void listContentPlugIns_MouseMove(object sender, MouseEventArgs e)
        {
            ListView listView = (ListView)sender;
            ListViewHitTestInfo hitTest = listView.HitTest(e.Location);

            // If we're over a disabled plug-in, show the tool tip with the reason why it's disabled.
            if ((hitTest.Item != null) && (hitTest.Item.ForeColor == Color.FromKnownColor(KnownColor.DimGray)))
            {
                var plugIn = (EditorPlugIn)hitTest.Item.Tag;

                if (Program.DisabledPlugIns.ContainsKey(plugIn))
                {
                    tipError.Show(Program.DisabledPlugIns[plugIn], listView, e.Location);
                }
            }
        }

        /// <summary>
        /// Handles the MouseLeave event of the listContentPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listContentPlugIns_MouseLeave(object sender, EventArgs e)
        {
            tipError.Hide((IWin32Window)sender);
        }
    }
}
