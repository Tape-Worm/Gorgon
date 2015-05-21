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
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.UI;

namespace Gorgon.Editor.Controls
{
    /// <summary>
    /// Plug-in list interface.
    /// </summary>
    partial class PanelPlugIns 
        : PreferencePanel
    {
        #region Variables.
		// List of pending disabled plug-ins.
		private HashSet<string> _pendingDisabled;
		// System disabled plug-ins.
	    private HashSet<string> _sysDisabled;
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Opening event of the popupPlugIns control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
		private void popupPlugIns_Opening(object sender, CancelEventArgs e)
		{
			try
			{
				if (listContentPlugIns.SelectedItems.Count == 0)
				{
					e.Cancel = true;
					return;
				}

				var plugIn = (GorgonPlugIn)listContentPlugIns.SelectedItems[0].Tag;

				if (!_pendingDisabled.Contains(plugIn.Name))
				{
					itemEnablePlugIn.Visible = false;
					itemDisablePlugIn.Visible = true;
					if (listContentPlugIns.SelectedItems.Count == 1)
					{
						itemDisablePlugIn.Text = string.Format("{0} '{1}'...",
															   Resources.GOREDIT_ACC_TEXT_DISABLE_PLUGIN,
															   listContentPlugIns.SelectedItems[0].Text);
					}
					else
					{
						itemDisablePlugIn.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DISABLE_PLUGIN);
					}

					return;
				}

				itemDisablePlugIn.Visible = false;
				itemEnablePlugIn.Visible = true;
				if (listContentPlugIns.SelectedItems.Count == 1)
				{
					itemEnablePlugIn.Text = string.Format("{0} '{1}'...",
														   Resources.GOREDIT_ACC_TEXT_ENABLE_PLUGIN,
														   listContentPlugIns.SelectedItems[0].Text);
				}
				else
				{
					itemEnablePlugIn.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_ENABLE_PLUGIN);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
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

            if (hitTest.Item != null)
            {
                GorgonDialogs.InfoBox(ParentForm, string.Format("{0}:\n{1}", string.Format(Resources.GOREDIT_DLG_PLUG_IN_FAIL_REASON, hitTest.Item.Text), hitTest.Item.SubItems[1].Text));
            }
        }

		/// <summary>
		/// Handles the Click event of the itemEnablePlugIn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemEnablePlugIn_Click(object sender, EventArgs e)
		{
			try
			{
				var listView = (ListView)popupPlugIns.SourceControl;

				foreach (var plugIn in
					listView.SelectedItems.Cast<ListViewItem>()
					        .Select(item => (GorgonPlugIn)item.Tag)
					        .Where(plugIn => _pendingDisabled.Contains(plugIn.Name)))
				{
					_pendingDisabled.Remove(plugIn.Name);
				}

				InitializeSettings();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemDisablePlugIn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemDisablePlugIn_Click(object sender, EventArgs e)
		{
			try
			{
				var result = ConfirmationResult.None;

				foreach (ListViewItem item in listContentPlugIns.SelectedItems)
				{
					var plugIn = (GorgonPlugIn)item.Tag;

					if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
					{
						result = GorgonDialogs.ConfirmBox(ParentForm,
						                                  string.Format(Resources.GOREDIT_DLG_DISABLE_PLUGIN, item.Text),
						                                  string.Empty,
						                                  listContentPlugIns.SelectedItems.Count > 1,
						                                  listContentPlugIns.SelectedItems.Count > 1);
					}

					if (result == ConfirmationResult.Cancel)
					{
						return;
					}

					if ((result & ConfirmationResult.No) == ConfirmationResult.No)
					{
						continue;
					}

					if (_pendingDisabled.Contains(plugIn.Name))
					{
						continue;
					}

					_pendingDisabled.Add(plugIn.Name);
				}

				InitializeSettings();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
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
	        itemEnablePlugIn.Text = Resources.GOREDIT_ACC_TEXT_ENABLE_PLUGIN;
	        itemDisablePlugIn.Text = Resources.GOREDIT_ACC_TEXT_DISABLE_PLUGIN;
	        pagePlugIns.Text = Resources.GOREDIT_TEXT_AVAILABLE_PLUGINS;
	        pageDisabled.Text = Resources.GOREDIT_TEXT_FAILED_PLUGINS;
	        columnDisabledDescription.Text = columnDesc.Text = Resources.GOREDIT_TEXT_DESCRIPTION;
	        columnDisablePath.Text = columnPath.Text = Resources.GOREDIT_TEXT_PATH;
	        columnType.Text = Resources.GOREDIT_TEXT_TYPE;
	        columnDisabledReason.Text = Resources.GOREDIT_TEXT_REASON;

			toolHelp.SetToolTip(imagePlugInHelp, Resources.GOREDIT_TIP_PLUGINS);
        }

        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public override void InitializeSettings()
        {
	        if (_pendingDisabled == null)
	        {
		        _pendingDisabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				_sysDisabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				// Get user plug-in disabled names.
		        foreach (var plugInName in Program.Settings.DisabledPlugIns)
		        {
			        if (_pendingDisabled.Contains(plugInName))
			        {
				        continue;
			        }

			        _pendingDisabled.Add(plugInName);
		        }

				// Get system disabled plug-ins.
		        foreach (var plugIn in GorgonApplication.PlugIns.Where(item => (item is GorgonFileSystemProviderPlugIn || item is EditorPlugIn)
			                                                   && (!_pendingDisabled.Contains(item.Name))
			                                                   && (PlugIns.IsDisabled(item))))
		        {
			        if (_sysDisabled.Contains(plugIn.Name))
			        {
				        continue;
			        }

			        _sysDisabled.Add(plugIn.Name);
		        }
	        }

            listContentPlugIns.BeginUpdate();
            listDisabledPlugIns.BeginUpdate();

            listDisabledPlugIns.Items.Clear();
            listContentPlugIns.Items.Clear();

            foreach (var plugIn in GorgonApplication.PlugIns)
            {
                if ((!(plugIn is EditorPlugIn))
                    && (!(plugIn is GorgonFileSystemProviderPlugIn)))
                {
                    continue;
                }

                var item = new ListViewItem
                           {
	                           Name = plugIn.Name,
	                           Text = plugIn.Description,
							   Tag = plugIn
                           };

	            if (plugIn is ContentPlugIn)
                {
                    item.SubItems.Add(Resources.GOREDIT_TEXT_CONTENT);
                }

                if (plugIn is FileWriterPlugIn)
                {
                    item.SubItems.Add(Resources.GOREDIT_TEXT_FILE_WRITER);
                }

                if (plugIn is GorgonFileSystemProviderPlugIn)
                {
                    item.SubItems.Add(Resources.GOREDIT_TEXT_FILE_READER);
                }

	            if (_pendingDisabled.Contains(plugIn.Name))
	            {
			        item.SubItems[1].Text = Resources.GOREDIT_TEXT_DISABLED;
		            item.ForeColor = Color.FromKnownColor(KnownColor.DimGray);
	            }
				else if (_sysDisabled.Contains(plugIn.Name))
	            {
			        item.SubItems[1].Text = Resources.GOREDIT_TEXT_ERROR;
			        item.ForeColor = Color.DarkRed;

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

	            item.SubItems.Add(plugIn.PlugInPath);

                listContentPlugIns.Items.Add(item);
            }

            listContentPlugIns.EndUpdate();
            listContentPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listDisabledPlugIns.EndUpdate();
            listDisabledPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

		/// <summary>
		/// Function to validate any settings on this panel.
		/// </summary>
		/// <returns>
		/// TRUE if the settings are valid, FALSE if not.
		/// </returns>
	    public override bool ValidateSettings()
	    {
		    if (!_pendingDisabled.SetEquals(PlugIns.UserDisabledPlugIns))
		    {
				GorgonDialogs.InfoBox(ParentForm, Resources.GOREDIT_DLG_APP_NEEDS_RESTART);    
		    }

		    return base.ValidateSettings();
	    }

	    /// <summary>
		/// Function to commit any settings.
		/// </summary>
	    public override void CommitSettings()
	    {
		    base.CommitSettings();

	        Program.Settings.DisabledPlugIns.Clear();

	        foreach (string pending in _pendingDisabled)
	        {
	            Program.Settings.DisabledPlugIns.Add(pending);
	        }
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
