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
// Created: Tuesday, April 15, 2014 10:14:36 PM
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
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Preferences for plug-ins.
    /// </summary>
    partial class PanelPlugInPreferences 
        : PreferencePanel
    {
        #region Classes.
        /// <summary>
        /// A list item for the plug-in.
        /// </summary>
        private class PlugInListItem
        {
            #region Properties.
            /// <summary>
            /// Property to return the plug-in for the list item.
            /// </summary>
            public EditorPlugIn PlugIn
            {
                get;
                private set;
            }

            /// <summary>
            /// Property to return the settings interface for the plug-in.
            /// </summary>
            public IPlugInSettingsUI SettingsUI
            {
                get;
                private set;
            }
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
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="PlugInListItem"/> class.
            /// </summary>
            /// <param name="plugIn">The plug in to assign.</param>
            public PlugInListItem(EditorPlugIn plugIn)
            {
                PlugIn = plugIn;
                SettingsUI = (IPlugInSettingsUI)plugIn;
            }
            #endregion
        }
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform validation for the controls.
        /// </summary>
        private void ValidateControls()
        {
        }

        /// <summary>
        /// Function to localize the controls on the panel.
        /// </summary>
        private void LocalizeControls()
        {
            labelPlugIns.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_PLUGINS);
        }

        /// <summary>
        /// Function to fill the plug-in list.
        /// </summary>
        private void FillList()
        {
            listPlugIns.Items.Clear();

            var plugIns = from plugIn in PlugIns.ContentPlugIns
                          where !PlugIns.IsDisabled(plugIn.Value)
                                && plugIn.Value is IPlugInSettingsUI
                          orderby plugIn.Key
                          select plugIn;

            listPlugIns.BeginUpdate();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (KeyValuePair<string, ContentPlugIn> plugIn in plugIns)
                {
                    listPlugIns.Items.Add(new PlugInListItem(plugIn.Value));
                }

                if (listPlugIns.Items.Count > 0)
                {
                    listPlugIns.SelectedIndex = 0;
                }
            }
            finally
            {
                listPlugIns.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the listPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listPlugIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if ((listPlugIns.SelectedIndex < 0)
                    && (panelPlugInPrefs.Controls.Count > 0))
                {
                    panelPlugInPrefs.Controls.RemoveAt(0);
                    return;
                }

                var item = (PlugInListItem)listPlugIns.SelectedItem;

                PreferencePanel panel = item.SettingsUI.GetSettingsUI();

                if (panel == null)
                {
                    return;
                }

                // Add the plug-in settings interface.
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.BackColor = DarkFormsRenderer.DarkBackground;
                panel.ForeColor = DarkFormsRenderer.ForeColor;
                panel.Dock = DockStyle.Fill;
                panelPlugInPrefs.Controls.Add(panel);
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
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                LocalizeControls();

                FillList();
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
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelPlugInPreferences"/> class.
        /// </summary>
        public PanelPlugInPreferences()
        {
            InitializeComponent();
        }
        #endregion
    }
}
