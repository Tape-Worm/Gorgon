#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, June 10, 2007 3:33:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	/// <summary>
	/// Interface for the render target manager.
	/// </summary>
	public partial class RenderTargetManager 
		: Manager
	{
		#region Properties.
		/// <summary>
		/// Property to return the selected render target.
		/// </summary>
		public RenderImage SelectedTarget
		{
			get
			{
				if (listTargets.SelectedItems.Count == 0)
					return null;

				return RenderTargetCache.Targets[listTargets.SelectedItems[0].Name] as RenderImage;
			}
			set
			{
				if (value == null)
					return;

				listTargets.SelectedItems.Clear();

				if (listTargets.Items.ContainsKey(value.Name))
					listTargets.Items[value.Name].Selected = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the form interface.
		/// </summary>
		protected override void ValidateForm()
		{
			base.ValidateForm();

			if (listTargets.SelectedItems.Count == 0)
			{
				menuPopupItemDeleteTarget.Enabled = false;
				buttonDeleteTarget.Enabled = false;
				menuPopupItemUpdateTarget.Enabled = false;
				buttonEditTarget.Enabled = false;
			}
			else
			{
				if (listTargets.SelectedItems.Count == 1)
				{
					menuPopupItemUpdateTarget.Enabled = true;
					buttonEditTarget.Enabled = true;
				}
				else
				{
					menuPopupItemUpdateTarget.Enabled = false;
					buttonEditTarget.Enabled = false;
				}

				menuPopupItemDeleteTarget.Enabled = true;
				buttonDeleteTarget.Enabled = true;
			}
		}

		/// <summary>
		/// Function called when the dock window is docking to its host container.
		/// </summary>
		protected override void OnDockWindowDocking()
		{
			base.OnDockWindowDocking();

			if (Toolitem != null)
				((ToolStripMenuItem)Toolitem).Checked = true;
		}

		/// <summary>
		/// Function called when the undocked dock window is closed.
		/// </summary>
		protected override void OnDockWindowClosing()
		{
			base.OnDockWindowClosing();

			if (Toolitem != null)
				((ToolStripMenuItem)Toolitem).Checked = false;
		}

		/// <summary>
		/// Function called when the bound menu item is clicked.
		/// </summary>
		/// <remarks>This function needs to be overridden.</remarks>
		protected override void OnMenuItemClick()
		{
			base.OnMenuItemClick();

			if (MainForm != null)
			{
				MainForm.ValidateForm();
				Settings.Root = "TargetManager";
				Settings.SetSetting("Visible", ((ToolStripMenuItem)Toolitem).Checked.ToString());
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:VisibleChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Toolitem != null)
				((ToolStripMenuItem)Toolitem).Checked = Visible;

			base.OnVisibleChanged(e);
		}

		/// <summary>
		/// Function to bind this control to the main form and a specific toolstrip item.
		/// </summary>
		/// <param name="mainForm">Main form to bind with.</param>
		/// <param name="toolItem">Toolstrip item to bind with.</param>
		public override void BindToMainForm(formMain mainForm, ToolStripItem toolItem)
		{
			base.BindToMainForm(mainForm, toolItem);

			RefreshList();
		}

		/// <summary>
		/// Function to remove all images.
		/// </summary>
		public void Clear()
		{
			// Remove each item.
			foreach (ListViewItem item in listTargets.Items)
				RenderTargetCache.Targets[item.Name].Dispose();

			RefreshList();
		}

		/// <summary>
		/// Function to refresh the image list.
		/// </summary>
		public void RefreshList()
		{
			ListViewItem item = null;		// List view item.
			RenderImage image = null;		// Render target image.

			try
			{
				if (!Gorgon.IsInitialized)
					return;

				Cursor.Current = Cursors.WaitCursor;

				// Fill in the list.
				listTargets.Items.Clear();

				listTargets.BeginUpdate();
				foreach (RenderTarget target in RenderTargetCache.Targets)
				{
					if (MainForm.ValidRenderTarget(target.Name))
					{
						image = target as RenderImage;

						// Add the item.
						item = new ListViewItem(image.Name);
						item.Name = image.Name;
						item.SubItems.Add(image.Width.ToString() + "x" + image.Height.ToString());
						item.SubItems.Add(image.Format.ToString());
						item.SubItems.Add(image.UseDepthBuffer.ToString());
						item.SubItems.Add(image.UseStencilBuffer.ToString());

						// Add the item.
						listTargets.Items.Add(item);
					}
				}
				listTargets.EndUpdate();

				listTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to populate render target list.", ex);
			}
			finally
			{
				ValidateForm();
				Cursor.Current = Cursors.Default;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTargetManager()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the Click event of the buttonNewTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonNewTarget_Click(object sender, EventArgs e)
		{
			formNewRenderTarget newTarget = null;		// New target form.
			ListViewItem item = null;					// Item.
			RenderImage image = null;					// Render image.

			try
			{
				newTarget = new formNewRenderTarget();

				if (newTarget.ShowDialog(this) == DialogResult.OK)
				{
					// Get the image.
					image = RenderTargetCache.Targets[newTarget.RenderTargetName] as RenderImage;

					// Add item.
					item = new ListViewItem(image.Name);
					item.Name = image.Name;
					item.SubItems.Add(image.Width.ToString() + "x" + image.Height.ToString());
					item.SubItems.Add(image.Format.ToString());
					item.SubItems.Add(image.UseDepthBuffer.ToString());
					item.SubItems.Add(image.UseStencilBuffer.ToString());

					listTargets.Items.Add(item);
					listTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

					// Select this target.
					listTargets.Items[item.Name].Selected = true;

					MainForm.ProjectChanged = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, ex);
			}
			finally
			{
				if (newTarget != null)
					newTarget.Dispose();
				newTarget = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonEditTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonEditTarget_Click(object sender, EventArgs e)
		{
			formNewRenderTarget newTarget = null;		// New target form.
			ListViewItem item = null;					// Item.
			RenderImage image = null;					// Render image.

			try
			{
				newTarget = new formNewRenderTarget();
				item = listTargets.SelectedItems[0];
				newTarget.EditTarget(item.Text);

				if (newTarget.ShowDialog(this) == DialogResult.OK)
				{
					// Get the image.
					image = RenderTargetCache.Targets[newTarget.RenderTargetName] as RenderImage;

					// Add item.
					item.SubItems.Clear();
					item.Text = image.Name;
					item.Name = image.Name;
					item.SubItems.Add(image.Width.ToString() + "x" + image.Height.ToString());
					item.SubItems.Add(image.Format.ToString());
					item.SubItems.Add(image.UseDepthBuffer.ToString());
					item.SubItems.Add(image.UseStencilBuffer.ToString());

					listTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

					// Update any bound sprites.
					MainForm.SpriteManager.Sprites.Refresh();

					MainForm.ProjectChanged = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, ex);
			}
			finally
			{
				if (newTarget != null)
					newTarget.Dispose();
				newTarget = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonDeleteTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonDeleteTarget_Click(object sender, EventArgs e)
		{
			ConfirmationResult result;		// Confirmation result.

			try
			{
				if (UI.ConfirmBox(MainForm, "Are you sure you wish to delete the selected render targets?") == ConfirmationResult.No)
					return;

				Cursor.Current = Cursors.WaitCursor;

				// Remove targets.
				foreach (ListViewItem item in listTargets.SelectedItems)
				{
					RenderImage image = null;		// Render target.

					result = ConfirmationResult.None;

					// Unbind image from any loaded sprites.
					foreach (SpriteDocument sprite in MainForm.SpriteManager.Sprites)
					{
						image = RenderTargetCache.Targets[item.Name] as RenderImage;
						if ((image != null) && (image.Image == sprite.Sprite.Image))
						{
							if ((result & ConfirmationResult.ToAll) == 0)
								result = UI.ConfirmBox(MainForm, "The render target '" + item.Name + "' is bound to the sprite '" + sprite.Name + "'.  It cannot be removed unless the sprite is unbound.\nRemove the binding to the sprite?", true, true);

							if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
							{
								sprite.Bind(null);
								MainForm.SpriteManager.RefreshList();
							}

							// Exit.
							if (result == ConfirmationResult.Cancel)
								return;
						}
					}

					if (((result & ConfirmationResult.Yes) == ConfirmationResult.Yes) || (result == ConfirmationResult.None))
						RenderTargetCache.Targets[item.Name].Dispose();
				}

				RefreshList();
				MainForm.ProjectChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to delete the render targets.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listTargets control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void listTargets_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the listTargets control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.LabelEditEventArgs"/> instance containing the event data.</param>
		private void listTargets_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			RenderImage image = null;			// Selected image.
			RenderImage newImage = null;		// New render image.
			string oldName = string.Empty;		// Previous name.

			try
			{
				if ((string.IsNullOrEmpty(e.Label)) || (e.Label == listTargets.Items[e.Item].Name))
				{
					e.CancelEdit = true;
					return;
				}

				// Don't allow duplicates.
				if (RenderTargetCache.Targets.Contains(e.Label))
				{
					UI.ErrorBox(MainForm, "There is already a render target with the name '" + e.Label + "'.");
					e.CancelEdit = true;
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				// Get previous name.
				oldName = listTargets.Items[e.Item].Name;
				listTargets.Items[e.Item].Name = e.Label;

				image = RenderTargetCache.Targets[oldName] as RenderImage;

				// This should NEVER happen, if it does, we have big problems.
				if (image == null)
				{
					UI.ErrorBox(MainForm, "Unable to find the selected render target.\nSomething is seriously un-right.");
					e.CancelEdit = true;
					return;
				}

				// Create with the new name.
				newImage = new RenderImage(e.Label, image.Width, image.Height, image.Format);

				// Bind to the new image.
				MainForm.SpriteManager.Sprites.ReplaceImage(image.Image, newImage.Image);

				// Destroy the previous render target.
				RenderTargetCache.Targets[image.Name].Dispose();

				MainForm.ProjectChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to rename this render target.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the listTargets control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void listTargets_KeyDown(object sender, KeyEventArgs e)
		{
			if ((listTargets.SelectedItems.Count == 1) && (e.KeyCode == Keys.F2))
				listTargets.SelectedItems[0].BeginEdit();
		}
	}
}
