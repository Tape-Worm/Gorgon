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
// Created: Wednesday, May 30, 2007 3:30:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Dialogs;
using ControlExtenders;
using Flobbster.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	/// <summary>
	/// Control that will represent the sprite manager.
	/// </summary>
	public partial class SpriteManager 
		: Manager
	{
		#region Variables.
		private SpriteDocument _current = null;				// Currently active sprite.
		private Animation _currentAnimation = null;			// Current animation.
		private SpriteDocumentList _spriteDocs = null;		// Sprite document list.
		private bool _queuePropertyUpdate = false;			// Flag to indicate that the property bag update should be queued.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the sprite document list.
		/// </summary>
		[Browsable(false)]
		public SpriteDocumentList Sprites
		{
			get
			{
				return _spriteDocs;
			}
		}

		/// <summary>
		/// Property to return the selected items.
		/// </summary>
		[Browsable(false)]
		public ListView.SelectedListViewItemCollection Selected
		{
			get
			{
				return listSprites.SelectedItems;
			}
		}

		/// <summary>
		/// Property to return the sprite list.
		/// </summary>
		[Browsable(false)]
		public ListView.ListViewItemCollection Items
		{
			get
			{
				return listSprites.Items;
			}
		}

		/// <summary>
		/// Property to set or return the currently active sprite.
		/// </summary>
		[Browsable(false)]
		public SpriteDocument CurrentSprite
		{
			get
			{
				return _current;
			}
			set
			{
				listSprites.SelectedItems.Clear();

				if ((value == null) || (!_spriteDocs.Contains(value.Name)))
					_current = null;
				else
				{
					_current = value;
					listSprites.Items[value.Name].Selected = true;
                    UpdatePropertyGrid();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonSaveSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSaveSprite_Click(object sender, EventArgs e)
		{
			SpriteDocument doc = null;		// Sprite document.

			try
			{
				// Save each document.
				foreach (ListViewItem item in listSprites.SelectedItems)
				{
					// Retrieve document.
					doc = _spriteDocs[item.Name];

					if (doc.Sprite.Filename == string.Empty)
						MainForm.SaveSpriteAsDialog(doc, dialogSave);
					else
					{
						doc.Save(doc.Sprite.Filename, doc.IsXML);
						doc.Changed = false;
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "There was an error attempting to save the selected sprite(s).", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				Settings.Root = null;

				MainForm.SpriteManager.RefreshList();
				MainForm.ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOpenSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOpenSprite_Click(object sender, EventArgs e)
		{
			SpriteDocument lastDocument = null;		// Last document loaded.
			try
			{
				Settings.Root = "Paths";
				dialogOpen.InitialDirectory = Settings.GetSetting("LastSpriteOpenPath", @".\");

				if (dialogOpen.ShowDialog(this) == DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;

					// Load all selected sprites.
					foreach (string filename in dialogOpen.FileNames)
					{
						lastDocument = _spriteDocs.Load(filename);
						// Bind to the sprite changed event.
						lastDocument.SpriteChanged += new EventHandler(sprite_SpriteChanged);
					}

					// Select the first item.
					Settings.SetSetting("LastSpriteOpenPath", Path.GetDirectoryName(dialogOpen.FileName));

					// Update everything.
					MainForm.RefreshAll();

					if (lastDocument != null)
					{
						MainForm.SpriteManager.Selected.Clear();
						MainForm.SpriteManager.Items[lastDocument.Name].Selected = true;
                        MainForm.SpriteManager.UpdatePropertyGrid();
					}

					MainForm.ProjectChanged = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "There was an error attempting to open the sprite(s).", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Handles the Opening event of the popupPropGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupPropGrid_Opening(object sender, CancelEventArgs e)
		{
			if ((gridSpriteProperties.SelectedObject == null) || (gridSpriteProperties.SelectedGridItem == null))
			{
				e.Cancel = true;
				return;
			}

			// Disable the reset item if necessary.
			if (!gridSpriteProperties.SelectedGridItem.PropertyDescriptor.CanResetValue(gridSpriteProperties.SelectedObject))
				menuReset.Enabled = false;
			else
				menuReset.Enabled = true;
		}

		/// <summary>
		/// Handles the Click event of the menuReset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuReset_Click(object sender, EventArgs e)
		{
			if ((gridSpriteProperties.SelectedObject != null) && (gridSpriteProperties.SelectedGridItem != null))
				gridSpriteProperties.ResetSelectedProperty();
		}

		/// <summary>
		/// Handles the SplitterMoved event of the splitSpriteList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitSpriteList_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (Gorgon.IsInitialized)
				Gorgon.Go();
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitSpriteList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterCancelEventArgs"/> instance containing the event data.</param>
		private void splitSpriteList_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			if (Gorgon.IsInitialized)
				Gorgon.Stop();
		}

		/// <summary>
		/// Handles the KeyDown event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void listSprites_KeyDown(object sender, KeyEventArgs e)
		{
			if ((listSprites.SelectedItems.Count == 1) && (e.KeyCode == Keys.F2))
				listSprites.SelectedItems[0].BeginEdit();
		}

		/// <summary>
		/// Handles the Click event of the menuItemClone control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemClone_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				// Go through each item and clone it.
				foreach (ListViewItem item in listSprites.SelectedItems)
				{
					SpriteDocument clone = null;	// Clone of the sprite.

					clone = (SpriteDocument)_spriteDocs[item.Name].Clone();

					// Add to list.
					_spriteDocs.Add(clone);
				}

				RefreshList();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to clone the selected sprite(s).", ex);
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemHorizontal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemHorizontal_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				// Go through each item and clone it.
				foreach (ListViewItem item in listSprites.SelectedItems)
				{
					_spriteDocs[item.Name].Sprite.HorizontalFlip = !_spriteDocs[item.Name].Sprite.HorizontalFlip;
					_spriteDocs[item.Name].Changed = true;
				}

				RefreshList();
				RefreshPropertyGrid();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to flip the selected sprite(s).", ex);
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemVertical_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				// Go through each item and clone it.
				foreach (ListViewItem item in listSprites.SelectedItems)
				{
					_spriteDocs[item.Name].Sprite.VerticalFlip = !_spriteDocs[item.Name].Sprite.VerticalFlip;
					_spriteDocs[item.Name].Changed = true;
				}

				RefreshList();
				RefreshPropertyGrid();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to flip the selected sprite(s).", ex);
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemAutoResize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAutoResize_Click(object sender, EventArgs e)
		{
			formSpriteExtractOptions extract = null;				// Sprite extraction interface.
			SpriteDocument sprite = null;							// Sprite.

			try
			{
				extract = new formSpriteExtractOptions();
				extract.GetSettings();

				// Begin extraction.
				if (extract.ShowDialog(MainForm) == DialogResult.OK)
				{
					System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

					// Get all the selected sprite rectangles.
					foreach (ListViewItem item in listSprites.SelectedItems)
					{
						sprite = _spriteDocs[item.Name];
						if (sprite.BoundImage != null)
						{
							// If the image is a render target, just resize to the size of the target.
							if (sprite.BoundImage.RenderImage == null)
								extract.Finder.UpdateRectangle(sprite.Sprite);
							else
								sprite.SetRegion(new RectangleF(0, 0, sprite.BoundImage.Width, sprite.BoundImage.Height));

							sprite.Changed = true;
						}
					}

					RefreshList();
					RefreshPropertyGrid();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to extract the sprites from the selected image(s).", ex);
			}
			finally
			{
				if (extract != null)
					extract.Dispose();

				extract = null;
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Selected event of the tabSpriteExtra control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TabControlEventArgs"/> instance containing the event data.</param>
		private void tabSpriteExtra_Selected(object sender, TabControlEventArgs e)
		{
			// Update the animation list.
			if (e.TabPage == pageAnimations)
				RefreshAnimationList();
		}

		/// <summary>
		/// Handles the Click event of the buttonNewAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNewAnimation_Click(object sender, EventArgs e)
		{
			formNewAnimation newAnim = null;		// New animation.

			try
			{
				newAnim = new formNewAnimation();
				newAnim.CurrentSprite = CurrentSprite.Sprite;

				if ((sender == menuItemUpdateAnimation) || (sender == buttonEditAnimation))
					newAnim.EditAnimation(listAnimations.SelectedItems[0].Name);

				if (newAnim.ShowDialog(this) == DialogResult.OK)
				{
					// Mark as changed.
					CurrentSprite.Changed = true;

					RefreshAnimationList();

					// Select the first animation.
					if (listAnimations.Items.Count > 0)
						listAnimations.Items[listAnimations.Items.Count - 1].Selected = true;

					ValidateForm();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to add the animation.", ex);
			}
			finally
			{
				if (newAnim != null)
					newAnim.Dispose();
				newAnim = null;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listAnimations_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the Opening event of the popupAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupAnimations_Opening(object sender, CancelEventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the listAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.LabelEditEventArgs"/> instance containing the event data.</param>
		private void listAnimations_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			string oldName = string.Empty;		// Old name.
			if ((string.IsNullOrEmpty(e.Label)) || (e.Label == listSprites.Items[e.Item].Name))
			{
				e.CancelEdit = true;
				return;
			}

			if (_current == null)
			{
				e.CancelEdit = true;
				return;
			}

			if (_current.Sprite.Animations.Contains(e.Label))
			{
				UI.ErrorBox(MainForm, "The animation '" + e.Label + "' already exists.");
				e.CancelEdit = true;
				return;
			}

			try
			{
				// Rename the animation.
				oldName = listAnimations.Items[e.Item].Name;
				listAnimations.Items[e.Item].Name = e.Label;
				_current.Sprite.Animations.Rename(oldName, e.Label);
				_current.Changed = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to rename the animation.", ex);
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the listAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void listAnimations_KeyDown(object sender, KeyEventArgs e)
		{
			if ((listAnimations.SelectedItems.Count == 1) && (e.KeyCode == Keys.F2))
				listAnimations.SelectedItems[0].BeginEdit();
		}

		/// <summary>
		/// Handles the Click event of the buttonDeleteAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonDeleteAnimation_Click(object sender, EventArgs e)
		{
			ConfirmationResult result = ConfirmationResult.None;			// Confirmation dialog result.

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				// Remove the items.
				foreach (ListViewItem item in listAnimations.SelectedItems)
				{
					// Keep asking until we cancel or check 'to all'.
					if ((result & ConfirmationResult.ToAll) == 0)
						result = UI.ConfirmBox(MainForm, "Are you sure you wish to remove the animation '" + item.Text + "'?", true, true);

					if (result == ConfirmationResult.Cancel)
						return;

					if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
					{
						_current.Sprite.Animations.Remove(item.Name);
						_current.SetAnimReadOnly(false);
						_current.Changed = true;
					}
				}

				// Select the first item.
				if (listAnimations.Items.Count > 0)
					listAnimations.Items[0].Selected = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to delete the selected animations.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;

				RefreshAnimationList();
				MainForm.ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemClearAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemClearAnimations_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (UI.ConfirmBox(MainForm, "Are you sure you wish to remove all the animations for '" + _current.Name + "'?") == ConfirmationResult.Yes)
				{
					_current.Sprite.Animations.Clear();
					_current.Changed = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to delete the animations.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;

				RefreshAnimationList();
				MainForm.ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonAnimationEditor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonAnimationEditor_Click(object sender, EventArgs e)
		{
			formAnimationEditor editor = null;			// Animation editor.

			try
			{
				formMain.Me.StopRendering = true;
				editor = new formAnimationEditor();
				editor.GetSettings();
				editor.Sprites = _spriteDocs;
				editor.CurrentAnimation = _current.Sprite.Animations[listAnimations.SelectedItems[0].Name];
				editor.ShowDialog(MainForm);
				RefreshAnimationList();
				_current.RefreshProperties();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to edit the animation.", ex);
			}
			finally
			{
				if (editor != null)
					editor.Dispose();
				editor = null;

				formMain.Me.StopRendering = false;
			}
		}

		/// <summary>
		/// Handles the MouseUp event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void listSprites_MouseUp(object sender, MouseEventArgs e)
		{
			// Update the property bags if we've queued the action.
			if (_queuePropertyUpdate)
			{
				UpdatePropertyGrid();
				_queuePropertyUpdate = false;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemUpdate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonEditSprite_Click(object sender, EventArgs e)
		{
			// Enter the clipping mode.
			MainForm.Clipper.UpdateRectangle = CurrentSprite.Sprite.ImageRegion;
			MainForm.EnterClipping();
		}

		/// <summary>
		/// Handles the Click event of the menuItemBind control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemBind_Click(object sender, EventArgs e)
		{
			formBindImage binder = null;			// Image binder.

			try
			{
				Gorgon.Stop();

				binder = new formBindImage(MainForm);
				binder.SelectedSprites = listSprites.SelectedItems;

				if (binder.ShowDialog(MainForm) == DialogResult.OK)
				{
					// Update the sprite list.
					RefreshList();

					// Force the main window to have focus.
					Focus();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to update sprite image binding(s).", ex);
			}
			finally
			{
				Gorgon.Go();
				if (binder != null)
					binder.Dispose();
				binder = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemUnbind control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemUnbind_Click(object sender, EventArgs e)
		{
			try
			{
				if (UI.ConfirmBox(MainForm, "Are you sure you wish to unbind the selected sprites from their images?") == ConfirmationResult.No)
					return;

				Cursor.Current = Cursors.WaitCursor;

				// Unbind each image.
				foreach (ListViewItem item in listSprites.SelectedItems)
					_spriteDocs[item.Text].Bind(null);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to unbind images from the selected sprites.", ex);
			}
			finally
			{
				RefreshList();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemClearAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemClearAll_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				Gorgon.Stop();

				_spriteDocs.Clear(true);
				listSprites.Items.Clear();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to delete the selected sprites.", ex);
			}
			finally
			{
				CurrentSprite = null;
				ValidateForm();
				Cursor.Current = Cursors.Default;
				Gorgon.Go();
			}
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.LabelEditEventArgs"/> instance containing the event data.</param>
		private void listSprites_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			string oldName = string.Empty;		// Old name.
			if ((string.IsNullOrEmpty(e.Label)) || (e.Label == listSprites.Items[e.Item].Name))
			{
				e.CancelEdit = true;
				return;
			}

			if (_spriteDocs.Contains(e.Label))
			{
				UI.ErrorBox(MainForm, "The sprite '" + e.Label + "' already exists.");
				e.CancelEdit = true;
				return;
			}

			try
			{
				// Rename the sprite.
				oldName = listSprites.Items[e.Item].Name;
				listSprites.Items[e.Item].Name = e.Label;
				_spriteDocs.Rename(_spriteDocs[oldName], e.Label);

				RefreshPropertyGrid();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to rename the sprite.", ex);
			}
		}

		/// <summary>
		/// Handles the Opening event of the popupSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupSprites_Opening(object sender, CancelEventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonCreateSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCreateSprite_Click(object sender, EventArgs e)
		{
			formNewSprite newSprite = null;		// New sprite dialog.
			SpriteDocument sprite = null;		// New sprite.

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				Gorgon.Stop();

				newSprite = new formNewSprite();

				if (newSprite.ShowDialog(MainForm) == DialogResult.OK)
				{
					// Create the sprite.
					sprite = _spriteDocs.Create(newSprite.SpriteName, newSprite.Image);

					if (sprite == null)
						return;

					// Begin sprite clipping.
					RefreshList();
					listSprites.SelectedItems.Clear();
					listSprites.Items[sprite.Name].Selected = true;

                    // Ensure the property grid has updated.
                    UpdatePropertyGrid();

					// Bind to the sprite changed event.
					sprite.SpriteChanged += new EventHandler(sprite_SpriteChanged);

					MainForm.Clipper.UpdateRectangle = RectangleF.Empty;
					MainForm.EnterClipping();
					MainForm.Focus();
					MainForm.ValidateForm();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, ex);
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

				Gorgon.Go();

				if (newSprite != null)
					newSprite.Dispose();
				newSprite = null;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listSprites_SelectedIndexChanged(object sender, EventArgs e)
		{
			if ((listSprites.SelectedItems.Count == 0) || (listSprites.Items.Count == 0))
				_current = null;
			else
			{
				if (_spriteDocs.Contains(listSprites.SelectedItems[0].Name))
					_current = _spriteDocs[listSprites.SelectedItems[0].Name];
				else
					_current = null;
			}

			_queuePropertyUpdate = true;
			ValidateForm();

			// Update animation list.
			if (tabSpriteExtra.SelectedTab == pageAnimations)
				RefreshAnimationList();
		}

		/// <summary>
		/// Handles the Click event of the buttonRemoveSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRemoveSprites_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				Gorgon.Stop();

				if (listSprites.SelectedItems.Count > 0)
					_spriteDocs.Remove(listSprites.SelectedItems);

				// Select the first item.
                if (listSprites.Items.Count > 0)
                {
                    listSprites.Items[0].Selected = true;
                    UpdatePropertyGrid();
                }

				MainForm.ProjectChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to delete the selected sprites.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				Gorgon.Go();

				RefreshList();
				MainForm.ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemNvidiaImport control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemNvidiaImport_Click(object sender, EventArgs e)
		{
			NVidiaAtlasImport nvImport = null;						// NV texture atlas import.
			ConfirmationResult result = ConfirmationResult.None;	// Confirmation result.
			string spriteName = string.Empty;						// Name of the sprite.
			int counter = 0;										// Sprite counter.
			SpriteDocument spriteDoc = null;						// Sprite document.

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				nvImport = new NVidiaAtlasImport();

				Settings.Root = "Paths";
				dialogOpen.Title = "Import nvidia texture atlas...";
				dialogOpen.InitialDirectory = Settings.GetSetting("LastImportOpenPath", @".\");
				dialogOpen.Filter = "Nvidia texture atlas (*.tai)|*.tai";
				dialogOpen.Multiselect = false;

				// Import.
				if (dialogOpen.ShowDialog(ParentForm) == DialogResult.OK)
				{
					nvImport.Import(dialogOpen.FileName);

					// Create sprites.
					foreach (KeyValuePair<string, NVidiaAtlasImport.SpriteData> sprite in nvImport.Sprites)
					{
						spriteName = sprite.Key;

						// If a sprite exists with this name, then ask to overwrite it.
						while (_spriteDocs.Contains(spriteName))
						{
							if ((result & ConfirmationResult.ToAll) == 0)
								result = UI.ConfirmBox(ParentForm, "The sprite '" + spriteName + "' already exists.  Replace it?", false, true);

							if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
								_spriteDocs.Remove(spriteName);
							else
								spriteName += "." + counter.ToString();
						}

						spriteDoc = _spriteDocs.Create(spriteName, sprite.Value.Image);
						spriteDoc.SetRegion(sprite.Value.SpriteRect);
						counter++;
					}

					MainForm.ProjectChanged = true;
					ValidateForm();
					RefreshList();
					MainForm.ImageManager.RefreshList();
					MainForm.ValidateForm();

					Settings.SetSetting("LastImportOpenPath", Path.GetDirectoryName(dialogOpen.FileName));
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Error trying to import the texture atlas file(s).", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to validate the form controls.
		/// </summary>
		protected override void ValidateForm()
		{
			MainForm.ValidateForm();

			int keyCount = 0;		// Transform key count.

			if (_current != null)
			{
				foreach (Animation anim in _current.Sprite.Animations)
				{
					foreach (Track track in anim.Tracks)
						keyCount += track.KeyCount;
				}
			}

			if (listSprites.SelectedItems.Count == 0)
			{
				buttonRemoveSprites.Enabled = false;
				buttonEditSprite.Enabled = false;
				menuItemBind.Enabled = false;
				menuItemUnbind.Enabled = false;
				menuItemUpdate.Enabled = false;
				menuItemDeleteSprites.Enabled = false;
				menuItemUpdate.Enabled = false;
				buttonSaveSprite.Enabled = false;
				menuFlip.Enabled = false;
				menuItemClone.Enabled = false;
				menuItemAutoResize.Enabled = false;
				splitSpriteList.Panel2Collapsed = true;
				panelAnimation.Enabled = false;
				stripAnimation.Enabled = false;
				popupAnimations.Enabled = false;
			}
			else
			{
				if (listSprites.SelectedItems.Count == 1)
				{
					if (keyCount == 0)
					{
						buttonEditSprite.Enabled = true;
						menuItemUpdate.Enabled = true;
					}
					else
					{
						buttonEditSprite.Enabled = false;
						menuItemUpdate.Enabled = false;
					}
					panelAnimation.Enabled = true;
					stripAnimation.Enabled = true;
					popupAnimations.Enabled = true;
				}
				else
				{
					buttonEditSprite.Enabled = false;
					menuItemUpdate.Enabled = false;
					panelAnimation.Enabled = false;
					stripAnimation.Enabled = false;
					popupAnimations.Enabled = false;
				}

				if (keyCount == 0)
					menuItemAutoResize.Enabled = true;
				else
					menuItemAutoResize.Enabled = false;
				menuFlip.Enabled = true;
				menuItemClone.Enabled = true;
				buttonSaveSprite.Enabled = true;
				menuItemBind.Enabled = true;
				menuItemUnbind.Enabled = true;
				menuItemDeleteSprites.Enabled = true;
				buttonRemoveSprites.Enabled = true;
				splitSpriteList.Panel2Collapsed = false;
			}

			if (listSprites.Items.Count > 0)
				menuItemClearAll.Enabled = true;
			else
				menuItemClearAll.Enabled = false;

			if (listAnimations.Items.Count > 0)
				menuItemClearAnimations.Enabled = true;
			else
				menuItemClearAnimations.Enabled = false;

			if (listAnimations.SelectedItems.Count == 0)
			{
				buttonEditAnimation.Enabled = false;
				buttonDeleteAnimation.Enabled = false;
				menuItemUpdateAnimation.Enabled = false;
				menuItemDeleteAnimation.Enabled = false;
				buttonAnimationEditor.Enabled = false;
				menuItemAnimationEditor.Enabled = false;
			}
			else
			{
				if (listAnimations.SelectedItems.Count == 1)
				{
					buttonAnimationEditor.Enabled = true;
					menuItemAnimationEditor.Enabled = true;
					buttonEditAnimation.Enabled = true;
					menuItemUpdateAnimation.Enabled = true;
				}
				else
				{
					buttonAnimationEditor.Enabled = false;
					menuItemAnimationEditor.Enabled = false;
					buttonEditAnimation.Enabled = false;
					menuItemUpdateAnimation.Enabled = false;
				}

				buttonDeleteAnimation.Enabled = true;
				menuItemDeleteAnimation.Enabled = true;
			}

			if (!_queuePropertyUpdate)
				UpdatePropertyGrid();
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
				Settings.Root = "SpriteManager";
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
			if (!DesignMode)
			{
				if (Toolitem != null)
					((ToolStripMenuItem)Toolitem).Checked = Visible;

				base.OnVisibleChanged(e);
			}
		}

		/// <summary>
		/// Function to retrieve settings for this control.
		/// </summary>
		public void GetSettings()
		{
			Settings.Root = "SpriteManager";
			splitSpriteList.SplitterDistance = Convert.ToInt32(Settings.GetSetting("PropertySplitPosition", splitSpriteList.SplitterDistance.ToString()));
			Settings.Root = null;
		}

		/// <summary>
		/// Function to save settings for this control.
		/// </summary>
		public void SaveSettings()
		{
			Settings.Root = "SpriteManager";
			Settings.SetSetting("PropertySplitPosition", splitSpriteList.SplitterDistance.ToString());
			Settings.Root = null;
		}

		/// <summary>
		/// Function to update the property grid.
		/// </summary>
		public void UpdatePropertyGrid()
		{
			// Get the selected items. (How can you not love teh LINQ?)
			gridSpriteProperties.SelectedObjects = (from docs in _spriteDocs
													join ListViewItem items in listSprites.SelectedItems on docs.Name equals items.Name
													select docs.PropertyBag).ToArray();

			// Update the grid.
			gridSpriteProperties.Refresh();
		}

		/// <summary>
		/// Function to refresh the property grid.
		/// </summary>
		public void RefreshPropertyGrid()
		{
			gridSpriteProperties.Refresh();
		}

		/// <summary>
		/// Handles the SpriteChanged event of the sprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public void sprite_SpriteChanged(object sender, EventArgs e)
		{
			SpriteDocument sprite = sender as SpriteDocument;	// Sprite document that sent the event.
			ListViewItem item = null;							// List view item.
			ListViewItem.ListViewSubItem sub1 = null;			// Sub item.

			// Find out which item this sprite belongs to and update it.
			if (listSprites.Items.ContainsKey(sprite.Name))
			{
				listSprites.BeginUpdate();
				item = listSprites.Items[sprite.Name];
				item.SubItems.Clear();
				item.Text = sprite.Name;
				item.Name = sprite.Name;
				if (sprite.Changed)
					item.SubItems.Add("Yes");
				else
					item.SubItems.Add("No");
				item.SubItems.Add(sprite.Sprite.Width + "x" + sprite.Sprite.Height);

				// Assume none.
				sub1 = item.SubItems.Add("n/a");

				// Update binding info.
				if (sprite.Sprite.Image != null)
				{
					if (sprite.Sprite.Image.ImageType == ImageType.RenderTarget)
					{
						sub1.Text = sprite.Sprite.Image.RenderImage.Name;
						item.SubItems.Add("Yes");
					}
					else
					{
						sub1.Text = sprite.Sprite.Image.Name;
						item.SubItems.Add("No");
					}
				}
				else
					item.SubItems.Add("No");

				listSprites.EndUpdate();
				listSprites.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

				MainForm.ValidateForm();
				sprite.Sprite.UpdateAABB();
			}

			RefreshPropertyGrid();
		}

		/// <summary>
		/// Function to remove all images.
		/// </summary>
		public void Clear()
		{
			// Remove each item.
			_spriteDocs.Clear(false);
			RefreshList();
		}

		/// <summary>
		/// Function to bind this control to the main form and a specific menu item.
		/// </summary>
		/// <param name="owner">Main form that owns the control.</param>
		/// <param name="item">Menu item that is bound to the control.</param>
		public override void BindToMainForm(formMain owner, ToolStripItem item)
		{
			base.BindToMainForm(owner, item);

			if (_spriteDocs == null)
				_spriteDocs = new SpriteDocumentList(MainForm);
			RefreshList();
		}

		/// <summary>
		/// Function to refresh the sprite animation list.
		/// </summary>
		public void RefreshAnimationList()
		{
			ListViewItem item = null;					// Sprite item.
			string currentAnimation = string.Empty;		// Remember selection.

			try
			{
				// No sprite?  Do nothing.
				if (_current == null)
				{
					listAnimations.Items.Clear();
					return;
				}

				if (_currentAnimation != null)
					currentAnimation = _currentAnimation.Name;
				_currentAnimation = null;

				listAnimations.Items.Clear();
				listAnimations.BeginUpdate();

				// Add each sprite.
				foreach (Animation animation in _current.Sprite.Animations)
				{
					item = new ListViewItem(animation.Name);

					item.SubItems.Add(animation.Length.ToString("0.0") + " ms.");
					if (animation.Looped)
						item.SubItems.Add("Yes");
					else
						item.SubItems.Add("No");
					item.Name = animation.Name;

					listAnimations.Items.Add(item);

					// Select the sprite.
					if (currentAnimation == animation.Name)
						item.Selected = true;
				}

				listAnimations.EndUpdate();
				listAnimations.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

				// Validate the form.
				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to enumerate the sprite animations.", ex);
			}
		}

		/// <summary>
		/// Function to refresh the sprite list.
		/// </summary>
		public void RefreshList()
		{
			ListViewItem item = null;					// Sprite item.
			ListViewItem.ListViewSubItem sub1 = null;	// List view sub item.
			string currentSprite = string.Empty;		// Remember selection.

			try
			{
				if (_current != null)
					currentSprite = _current.Name;
				_current = null;

				listSprites.Items.Clear();
				listSprites.BeginUpdate();

				// Add each sprite.
				foreach (SpriteDocument sprite in _spriteDocs)
				{
					item = new ListViewItem(sprite.Name);
					if (sprite.Changed)
						item.SubItems.Add("Yes");
					else
						item.SubItems.Add("No");
					item.SubItems.Add(sprite.Sprite.Width.ToString("0.0") + " x " + sprite.Sprite.Height.ToString("0.0"));
					item.Name = sprite.Name;

					// Assume none.
					sub1 = item.SubItems.Add("n/a");

					// Update binding info.					
					if (sprite.Sprite.Image != null)
					{
						if (sprite.Sprite.Image.ImageType == ImageType.RenderTarget)
						{
							sub1.Text = sprite.Sprite.Image.RenderImage.Name;
							item.SubItems.Add("Yes");
						}
						else
						{
							sub1.Text = sprite.Sprite.Image.Name;
							item.SubItems.Add("No");
						}
					}
					else
						item.SubItems.Add("No");

					listSprites.Items.Add(item);

					// Select the sprite.
					if (currentSprite == sprite.Name)
						item.Selected = true;
				}

				listSprites.EndUpdate();
				listSprites.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

				// Validate the form.
				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to enumerate the sprites.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public SpriteManager()
		{
			InitializeComponent();			
		}
		#endregion
	}
}
