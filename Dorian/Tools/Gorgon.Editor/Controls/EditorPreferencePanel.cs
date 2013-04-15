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
        /// Handles the DoubleClick event of the listDisabledPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listDisabledPlugIns_DoubleClick(object sender, EventArgs e)
        {
            Point cursorLocation = listDisabledPlugIns.PointToClient(Cursor.Position);
            ListViewHitTestInfo hitTest = listDisabledPlugIns.HitTest(cursorLocation);

            if ((hitTest != null) && (hitTest.Item != null))
            {
                GorgonDialogs.ErrorBox(ParentForm, "Plug-In '" + hitTest.Item.Text + "' failed to load.  See details for information.", hitTest.Item.SubItems[1].Text, true);
            }

        }

        /// <summary>
        /// Handles the ItemChecked event of the listContentPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckedEventArgs"/> instance containing the event data.</param>
        private void listContentPlugIns_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                if (e.Item == null)
                {
                    return;
                }

                // Don't allow the item to be checked if it's disabled.
                if (e.Item.Tag == null)
                {
                    if (e.Item.Checked)
                    {
                        e.Item.Checked = false;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
        }
        
        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            textScratchLocation.Text = Program.Settings.ScratchPath;

            listContentPlugIns.BeginUpdate();
            listContentPlugIns.Items.Clear();
            foreach (var plugIn in Gorgon.PlugIns)
            {
                ListViewItem item = new ListViewItem();

                if ((plugIn is EditorPlugIn) || (plugIn is GorgonFileSystemProviderPlugIn))
                {
                    EditorPlugIn editorPlugIn = plugIn as EditorPlugIn;

                    item.Name = plugIn.Name;
                    item.Text = plugIn.Description;

                    if (plugIn is ContentPlugIn)
                    {
                        item.SubItems.Add("Content");
                    }

                    if (plugIn is FileWriterPlugIn)
                    {
                        item.SubItems.Add("File Writer");
                    }

                    if (plugIn is GorgonFileSystemProviderPlugIn)
                    {
                        item.SubItems.Add("File Reader");
                    }

                    if ((editorPlugIn != null) && (Program.DisabledPlugIns.ContainsKey(editorPlugIn)))
                    {
                        item.SubItems[1].Text = "Disabled";
                        item.ForeColor = Color.FromKnownColor(KnownColor.DimGray);
                        item.Tag = null;
                        item.Checked = false;
                    }
                    else
                    {
                        item.Checked = true;
                        item.Tag = plugIn;
                    }

                    item.SubItems.Add(plugIn.PlugInPath);
                }

                listContentPlugIns.Items.Add(item);                
            }

            listContentPlugIns.ItemChecked += listContentPlugIns_ItemChecked;

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            listDisabledPlugIns.BeginUpdate();
            listDisabledPlugIns.Items.Clear();
            foreach (var plugIn in Program.DisabledPlugIns)
            {
                ListViewItem item = new ListViewItem();

                item.Name = plugIn.Key.Name;
                item.Text = plugIn.Key.Description;
                item.SubItems.Add(plugIn.Value);
                item.SubItems.Add(plugIn.Key.PlugInPath);
                item.Tag = plugIn;

                listDisabledPlugIns.Items.Add(item);
            }

            listDisabledPlugIns.EndUpdate();
            listDisabledPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

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
    }
}
