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
// Created: Monday, April 21, 2014 11:25:08 PM
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
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.Controls
{
    /// <summary>
    /// Plug-in list interface.
    /// </summary>
    public partial class PanelPlugIns 
        : PreferencePanel
    {
        #region Variables.

        #endregion

        #region Properties.

        #endregion

        #region Methods.
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
        /// Function to localize the text on the controls for the panel.
        /// </summary>
        /// <remarks>
        /// Override this method to supply localized text for any controls on the panel.
        /// </remarks>
        protected internal override void LocalizeControls()
        {
            Text = Resources.GOREDIT_TEXT_PLUGINS;
        }

        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
            listContentPlugIns.BeginUpdate();
            listDisabledPlugIns.BeginUpdate();

            listDisabledPlugIns.Items.Clear();
            listContentPlugIns.Items.Clear();

            foreach (var plugIn in Gorgon.PlugIns)
            {
                if ((!(plugIn is EditorPlugIn))
                    && (!(plugIn is GorgonFileSystemProviderPlugIn)))
                {
                    continue;
                }

                var item = new ListViewItem();
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

                listContentPlugIns.Items.Add(item);
            }

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listDisabledPlugIns.EndUpdate();
            listDisabledPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelPlugIns"/> class.
        /// </summary>
        public PanelPlugIns()
        {
            InitializeComponent();
        }
        #endregion
    }
}
