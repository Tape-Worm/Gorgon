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
// Created: Tuesday, June 05, 2007 2:05:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	/// <summary>
	/// Interface for the image manager.
	/// </summary>
	public partial class ImageManager 
		: Manager
	{
		#region Variables.
		private Drawing.Image _image = null;				// GDI+ version of the image.
		private string _imageEditorName = string.Empty;		// Image editor name.
		private string _imageEditorPath = string.Empty;		// Image editor path.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the selected image.
		/// </summary>
		public Image SelectedImage
		{
			get
			{
				if (listImages.SelectedItems.Count == 0)
					return null;

				return ImageCache.Images[listImages.SelectedItems[0].Name];
			}
			set
			{
				if (value == null)
					return;

				listImages.SelectedItems.Clear();

				if (listImages.Items.ContainsKey(value.Name))
					listImages.Items[value.Name].Selected = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SplitterMoved event of the splitImageList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitImageList_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (Gorgon.IsInitialized)
				Gorgon.Go();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listImages control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void listImages_SelectedIndexChanged(object sender, EventArgs e)
		{
			GetSelectedImage();
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonEditImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonEditImage_Click(object sender, EventArgs e)
		{
			string imageName = string.Empty;		// Image file name.
			ProcessStartInfo startInfo;				// Process start info.
			Image editImage = null;					// Edit image.
			DateTime originalTime;                  // Original file time.
			DateTime updateTime;                    // Last update time.

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				// Get the image filename.
				editImage = ImageCache.Images[listImages.SelectedItems[0].Name];
				imageName = Path.GetFullPath(editImage.Filename);

				if (!File.Exists(_imageEditorPath))
				{
					UI.ErrorBox(MainForm, "Unable to locate the image editor at '" + _imageEditorPath + "'.");
					return;
				}

				if (!File.Exists(imageName))
				{
					UI.ErrorBox(MainForm, "Unable to locate the image at the specified location '" + imageName + "'.");
					return;
				}

				updateTime = originalTime = File.GetLastWriteTimeUtc(imageName);
				startInfo = new ProcessStartInfo("\"" + _imageEditorPath + "\"");
				startInfo.Arguments = "\"" + imageName + "\"";
				startInfo.WorkingDirectory = Path.GetDirectoryName(imageName);
				Process.Start(startInfo).WaitForExit();

				// Reload this image.
				if (editImage != null)
				{
					updateTime = File.GetLastWriteTimeUtc(imageName);
					if (updateTime > originalTime)
					{
						// Destroy the previous image.
						editImage.Dispose();

						// Re-load it.
						editImage = Image.FromFile(imageName);

						// Reassociate with the sprites.
						foreach (SpriteDocument sprite in MainForm.SpriteManager.Sprites)
						{
							if (string.Compare(sprite.Sprite.Image.Name, editImage.Name) == 0)
								sprite.BoundImage = editImage;
						}
					}
				}

				// Refresh the image.
				listImages.SelectedItems[0].SubItems[1].Text = editImage.Width + "x" + editImage.Height;
				listImages.SelectedItems[0].SubItems[2].Text = editImage.Format.ToString();
				GetSelectedImage();
			}
			catch (Win32Exception wEx)
			{
				UI.ErrorBox(MainForm, "Unable to edit the selected image '" + imageName + "'.", wEx);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to edit the selected image '" + imageName + "'.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRemoveImages control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonRemoveImages_Click(object sender, EventArgs e)
		{
			ConfirmationResult result;		// Confirmation result.

			try
			{
				if (UI.ConfirmBox(MainForm, "Are you sure you wish to remove the selected images?") == ConfirmationResult.No)
					return;

				Cursor.Current = Cursors.WaitCursor;

				foreach (ListViewItem item in listImages.SelectedItems)
				{
					result = ConfirmationResult.None;

					// Unbind image from any loaded sprites.
					foreach (SpriteDocument sprite in MainForm.SpriteManager.Sprites)
					{
						Image image = ImageCache.Images[item.Name];		// Current image.

						if (image == sprite.Sprite.Image)
						{
							if ((result & ConfirmationResult.ToAll) == 0)
								result = UI.ConfirmBox(MainForm, "The image '" + item.Name + "' is bound to the sprite '" + sprite.Name + "'.  It cannot be removed unless the sprite is unbound.\nRemove the binding to the sprite?", true, true);

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
						ImageCache.Images[item.Name].Dispose();

				}

				RefreshList();
				GetSelectedImage();

				// Update the status.
				MainForm.ProjectChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "There was an error attempting to remove the selected image(s).", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitImageList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterCancelEventArgs"/> instance containing the event data.</param>
		private void splitImageList_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			if (Gorgon.IsInitialized)
				Gorgon.Stop();
		}

		/// <summary>
		/// Handles the Click event of the buttonExtractSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonExtractSprites_Click(object sender, EventArgs e)
		{
			formSpriteExtractOptions extract = null;				// Sprite extraction interface.
			Drawing.RectangleF[] rectangles = null;					// Rectangle list.
			SpriteDocumentList spriteDocs = null;					// Sprite documents.
			SpriteDocument sprite = null;							// Sprite.
			int counter = 0;										// Sprite counter.
			string spriteName = string.Empty;						// Sprite name.
			ConfirmationResult result = ConfirmationResult.None;	// Dialog result.

			try
			{
				extract = new formSpriteExtractOptions();
				extract.GetSettings();

				// Get sprite documents.
				spriteDocs = MainForm.SpriteManager.Sprites;

				// Begin extraction.
				if (extract.ShowDialog(MainForm) == DialogResult.OK)
				{
					System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
					// Extract from all selected images.
					foreach (ListViewItem item in listImages.SelectedItems)
					{
						// Send the image to the finder.
						extract.Finder.GetGDIImage(ImageCache.Images[item.Name]);

						// Get sprite rectangles.
						rectangles = extract.Finder.GetRectangles();

						// Begin sprite creation.
						foreach (Drawing.RectangleF rectangle in rectangles)
						{
							spriteName = extract.Prefix + counter.ToString();

							// If a sprite exists with this name, then ask to overwrite it.
							while (spriteDocs.Contains(spriteName))
							{
								if ((result & ConfirmationResult.ToAll) == 0)
									result = UI.ConfirmBox(MainForm, "The sprite '" + spriteName + "' already exists.  Replace it?", false, true);

								if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
									spriteDocs.Remove(spriteName);
								else
									spriteName += "." + counter.ToString();
							}

							sprite = spriteDocs.Create(spriteName, ImageCache.Images[item.Name]);
							sprite.SetRegion(rectangle);
							counter++;
						}
					}

					MainForm.SpriteManager.RefreshList();
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
		/// Handles the Click event of the buttonRefresh control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			Image image = null;		// Image to refresh.

			try
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

				// Go through each image and re-load from disk.
				foreach (ListViewItem item in listImages.SelectedItems)
				{
					image = ImageCache.Images[item.Name];
					if ((image != null) && (File.Exists(image.Filename)))
					{
						// Re-load the image.
						image.Dispose();
						image = Image.FromFile(image.Filename);

						// Reassociate with the sprites.
						foreach (SpriteDocument sprite in MainForm.SpriteManager.Sprites)
						{
							if (string.Compare(sprite.Sprite.Image.Name, image.Name) == 0)
								sprite.BoundImage = image;
						}
					}
					else
						UI.ErrorBox(MainForm, "The image '" + item.Name + "' does not exist.");
				}

				GetSelectedImage();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to refresh the selected images.", ex);
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonGridExtract control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonGridExtract_Click(object sender, EventArgs e)
		{
			formSpriteGridExtractOptions extract = null;			// Sprite grid extraction interface.
			Drawing.RectangleF[] rectangles = null;					// Rectangle list.
			SpriteDocumentList spriteDocs = null;					// Sprite documents.
			SpriteDocument sprite = null;							// Sprite.
			int counter = 0;										// Sprite counter.
			string spriteName = string.Empty;						// Sprite name.
			ConfirmationResult result = ConfirmationResult.None;	// Dialog result.

			try
			{
				extract = new formSpriteGridExtractOptions();
				extract.GetSettings();

				// Get sprite documents.
				spriteDocs = MainForm.SpriteManager.Sprites;

				// Begin extraction.
				if (extract.ShowDialog(MainForm) == DialogResult.OK)
				{
					System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
					// Extract from all selected images.
					foreach (ListViewItem item in listImages.SelectedItems)
					{
						// Send the image to the finder.
						extract.Finder.GetGDIImage(ImageCache.Images[item.Name]);

						// Get sprite rectangles.
						rectangles = extract.Finder.GetGridRectangles();

						// Begin sprite creation.
						foreach (Drawing.RectangleF rectangle in rectangles)
						{
							spriteName = extract.Prefix + counter.ToString();

							// If a sprite exists with this name, then ask to overwrite it.
							while (spriteDocs.Contains(spriteName))
							{
								if ((result & ConfirmationResult.ToAll) == 0)
									result = UI.ConfirmBox(MainForm, "The sprite '" + spriteName + "' already exists.  Replace it?", false, true);

								if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
									spriteDocs.Remove(spriteName);
								else
									spriteName += "." + counter.ToString();
							}

							sprite = spriteDocs.Create(spriteName, ImageCache.Images[item.Name]);
							sprite.SetRegion(rectangle);
							counter++;
						}
					}

					MainForm.SpriteManager.RefreshList();
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
		/// Function to retrieve the selected image.
		/// </summary>
		private void GetSelectedImage()
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (_image != null)
					_image.Dispose();
				_image = null;

				picturePreview.Image = picturePreview.InitialImage;

				// If nothing's selected, then return.
				if (listImages.SelectedItems.Count == 0)
					return;

				// Try to load the image.
				if (!ImageCache.Images.Contains(listImages.SelectedItems[0].Name))
				{
					UI.ErrorBox(MainForm, "The image '" + listImages.SelectedItems[0].Name + "' is not loaded.");
					return;
				}

				// Save the image to the stream.
				_image = ImageCache.Images[listImages.SelectedItems[0].Name].SaveBitmap();

				// Set the image.				
				picturePreview.Image = _image;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOpenImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOpenImage_Click(object sender, EventArgs e)
		{
			string fileName = string.Empty;		// Filename.

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				Settings.Root = "Paths";
				dialogOpenImage.InitialDirectory = Settings.GetSetting("LastImageOpenPath", @".\");

				if (dialogOpenImage.ShowDialog(this) == DialogResult.OK)
				{
					// Get the image file.
					foreach (string file in dialogOpenImage.FileNames)
					{
						fileName = Path.GetFileNameWithoutExtension(file);
						Image.FromFile(file);
					}

					// Re-populate the list.
					RefreshList();

					// Select the newly loaded item.
					listImages.SelectedItems.Clear();
					listImages.Items[fileName].Selected = true;

					// Save the last open setting.
					Settings.SetSetting("LastImageOpenPath", Path.GetDirectoryName(dialogOpenImage.FileNames[0]) + @"\");

					// Update the status.
					MainForm.ProjectChanged = true;
				}
			}
			catch (Exception ex)
			{
				if (ImageCache.Images.Contains(fileName))
					ImageCache.Images[fileName].Dispose();
				GorgonException.Catch(ex, (error) => UI.ErrorBox(MainForm, "Could not load the image file '" + fileName + "'.\nIt may be corrupt or an unknown format.", error));
			}
			finally
			{
				Settings.Root = null;
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to validate the form interface.
		/// </summary>
		protected override void ValidateForm()
		{
			base.ValidateForm();


			// Hide this functionality if we're not a docking window.
			buttonGridExtract.Visible = buttonExtractSprites.Visible = toolStripSeparator2.Visible = (DockWindow != null);

			if (listImages.SelectedItems.Count == 0)
			{
				menuItemRemoveImage.Enabled = false;
				menuItemEdit.Enabled = false;
				buttonRemoveImages.Enabled = false;
				buttonEditImage.Enabled = false;
				buttonExtractSprites.Enabled = false;
				buttonGridExtract.Enabled = false;
				buttonRefresh.Enabled = false;
			}
			else
			{
				if (listImages.SelectedItems.Count == 1)
				{
					if (_imageEditorName != string.Empty)
					{
						menuItemEdit.Enabled = true;
						buttonEditImage.Enabled = true;
					}
					else
					{
						menuItemEdit.Enabled = false;
						buttonEditImage.Enabled = false;
					}
				}
				else
				{
					menuItemEdit.Enabled = false;
					buttonEditImage.Enabled = false;
				}

				buttonRefresh.Enabled = true;
				buttonRemoveImages.Enabled = true;
				menuItemRemoveImage.Enabled = true;
				buttonExtractSprites.Enabled = true;
				buttonGridExtract.Enabled = true;
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
				Settings.Root = "ImageManager";
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
		/// Function to retrieve settings for this control.
		/// </summary>
		public void GetSettings()
		{
			Settings.Root = "ImageManager";
			if (DockWindow != null)
			{
				splitImageList.Orientation = Orientation.Horizontal;				
				splitImageList.SplitterDistance = Convert.ToInt32(Settings.GetSetting("ImageSplitPosition", splitImageList.SplitterDistance.ToString()));				
			}
			else
			{
				splitImageList.Orientation = Orientation.Vertical;
				splitImageList.SplitterDistance = Convert.ToInt32(Settings.GetSetting("ImageNewSpriteSplitPosition", splitImageList.SplitterDistance.ToString()));				
			}

			// Get image editor.
			_imageEditorName = Settings.GetSetting("ImageEditorName", string.Empty);
			_imageEditorPath = Settings.GetSetting("ImageEditorPath", string.Empty);

			if (_imageEditorName != string.Empty)
			{
				menuItemEdit.Text = "Edit with '" + _imageEditorName + "'...";
				buttonEditImage.ToolTipText = buttonEditImage.Text = "Edit the image with '" + _imageEditorName + "'.";
			}
			else
			{
				menuItemEdit.Text = "Edit the image.";
				buttonEditImage.ToolTipText = buttonEditImage.Text = "Edit the image.";
			}


			Settings.Root = null;
		}

		/// <summary>
		/// Function to save the settings.
		/// </summary>
		public void SaveSettings()
		{
			Settings.Root = "ImageManager";
			if (DockWindow != null)
				Settings.SetSetting("ImageSplitPosition", splitImageList.SplitterDistance.ToString());
			else
				Settings.SetSetting("ImageNewSpriteSplitPosition", splitImageList.SplitterDistance.ToString());

			Settings.SetSetting("ImageEditorName", _imageEditorName);
			Settings.SetSetting("ImageEditorPath", _imageEditorPath);

			Settings.Root = null;
		}

		/// <summary>
		/// Function to remove all images.
		/// </summary>
		public void Clear()
		{
			// Remove each item.
			foreach (ListViewItem item in listImages.Items)
				ImageCache.Images[item.Name].Dispose();

			RefreshList();
		}

		/// <summary>
		/// Function to refresh the image list.
		/// </summary>
		public void RefreshList()
		{
			ListViewItem item = null;					// Sprite item.

			try
			{
				if (!Gorgon.IsInitialized)
					return;

				// Reset the preview image.
				picturePreview.Image = picturePreview.InitialImage;
				listImages.Items.Clear();
				listImages.BeginUpdate();

				// Add each sprite.
				foreach (Image image in ImageCache.Images)
				{
					// Don't add resource-based images.
					if (!image.IsResource)
					{
						item = new ListViewItem(image.Name);
						item.Name = image.Name;

						item.SubItems.Add(image.Width + "x" + image.Height);
						item.SubItems.Add(image.Format.ToString());

						listImages.Items.Add(item);
					}
				}

				listImages.EndUpdate();
				listImages.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

				// Select the first item.
				if (listImages.Items.Count > 0)
					listImages.Items[0].Selected = true;

				// Validate the form.				
				ValidateForm();

				// Get the selected image.
				if (listImages.SelectedItems.Count > 0)
					GetSelectedImage();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(MainForm, "Unable to enumerate the images.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageManager()
		{
			InitializeComponent();
		}
		#endregion
	}
}
