#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Friday, May 18, 2007 2:48:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for image binding.
	/// </summary>
	public partial class formBindImage 
		: Form
	{
		#region Variables.
		private ListView.SelectedListViewItemCollection _selectedSprites = null;	// Selected sprites.
		private formMain _owner = null;												// Owner form.
		private bool _isPropBagEditor = false;										// Flag to indicate that this is a property bag editor.
		private Image _propBagImage = null;											// Property bag image.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether this is a property bag editor.
		/// </summary>
		public bool IsPropBagEditor
		{
			get
			{
				return _isPropBagEditor;
			}
			set
			{
				_isPropBagEditor = value;
			}
		}

		/// <summary>
		/// Property to set or return the selected image for a property bag.
		/// </summary>
		public Image PropBagSelectedImage
		{
			get
			{
				return _propBagImage;
			}
			set
			{
				_propBagImage = value;

				// Refresh the image list.
				FillCombos();

				if (_propBagImage != null)
				{
					if (_propBagImage.RenderImage != null)
					{
						checkRenderTarget.Checked = true;
						// Update interface.
						checkRenderTarget_Click(this, EventArgs.Empty);
						comboImages.Text = _propBagImage.RenderImage.Name;
					}
					else
					{
						checkRenderTarget.Checked = false;
						// Update interface.
						checkRenderTarget_Click(this, EventArgs.Empty);
						comboImages.Text = _propBagImage.Name;
					}

				}

				ValidateForm();
			}
		}

		/// <summary>
		/// Property to set or return the current sprite.
		/// </summary>
		public ListView.SelectedListViewItemCollection SelectedSprites
		{
			get
			{
				return _selectedSprites;
			}
			set
			{
				bool sameImage = true;		// Flag to indicate that the images are the same.
				Image lastImage = null;		// Last image.
				
				_selectedSprites = value;

				// Find out if the items are the same.
				lastImage = _owner.SpriteManager.Sprites[_selectedSprites[0].Text].Sprite.Image;
				comboImages.Text = string.Empty;

				// Determine if the bound images are the same.
				if (_selectedSprites.Count > 1)
				{
					foreach (ListViewItem item in _selectedSprites)
					{
						if (!_owner.SpriteManager.Sprites.Contains(item.Name))
							UI.ErrorBox(this, "Unable to find the sprite '" + item.Name + "'.");
						else
						{
							if (_owner.SpriteManager.Sprites[item.Name].Sprite.Image != lastImage)
							{
								sameImage = false;
								break;
							}
							else
								lastImage = _owner.SpriteManager.Sprites[item.Name].Sprite.Image;
						}
					}
				}

				// If it's not the same image, remove the name.
				if (sameImage)
				{
					// If the same, get the name and if they're all render targets, then check the box.
					if (lastImage != null)
					{
						if (lastImage.ImageType == ImageType.RenderTarget)
						{
							checkRenderTarget.Checked = true;
							checkRenderTarget_Click(this, EventArgs.Empty);
							comboImages.Text = lastImage.RenderImage.Name;
						}
						else
							comboImages.Text = lastImage.Name;
					}
				}

				ValidateForm();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateForm()
		{
			// No images?  Disable.
			if (comboImages.Items.Count == 0)
			{
				labelImageName.Enabled = false;
				comboImages.Enabled = false;
			}
			else
			{
				labelImageName.Enabled = true;
				comboImages.Enabled = true;
			}

			// Validate OK.
			if (comboImages.Text != string.Empty)
				buttonOK.Enabled = true;
			else
				buttonOK.Enabled = false;
		}

		/// <summary>
		/// Function to fill in combo boxes.
		/// </summary>
		private void FillCombos()
		{			
			comboImages.Items.Clear();

			// Add images.
			if (!checkRenderTarget.Checked)
			{
				foreach (Image image in ImageCache.Images)
				{
					// Ignore resource images.
					if ((!image.IsResource) && (image.ImageType == ImageType.Normal))
						comboImages.Items.Add(image.Name);
				}
			}
			else
			{
				foreach (RenderTarget image in RenderTargetCache.Targets)
				{
					// Only add images that are not rendertargets unless we have render target checked.
					if (_owner.ValidRenderTarget(image.Name))
						comboImages.Items.Add(image.Name);
				}
			}

			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonImageManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonImageManager_Click(object sender, EventArgs e)
		{
			formImageManager imageManager = null;			// Image manager.
			formRenderTargetManager targetManager = null;	// Render target manager.

			try
			{
				if (!checkRenderTarget.Checked)
				{
					// Open the image manager.					
					imageManager = new formImageManager(_owner);
					imageManager.StartPosition = FormStartPosition.CenterParent;
					imageManager.ImageManager.GetSettings();
					if (ImageCache.Images.Contains(comboImages.Text))
						imageManager.ImageManager.SelectedImage = ImageCache.Images[comboImages.Text];
					else
						imageManager.ImageManager.SelectedImage = null;
					imageManager.ShowDialog(this);

					// Re-fill the combo and select the image name.
					FillCombos();
					if (imageManager.ImageManager.SelectedImage != null)
						comboImages.Text = imageManager.ImageManager.SelectedImage.Name;
				}
				else
				{
					targetManager = new formRenderTargetManager(_owner);
					targetManager.StartPosition = FormStartPosition.CenterParent;
					if (RenderTargetCache.Targets.Contains(comboImages.Text))
						targetManager.TargetManager.SelectedTarget = RenderTargetCache.Targets[comboImages.Text] as RenderImage;
					else
						targetManager.TargetManager.SelectedTarget = null;
					targetManager.ShowDialog(this);

					// Re-fill the combo and select the image name.
					FillCombos();
					if (targetManager.TargetManager.SelectedTarget != null)
						comboImages.Text = targetManager.TargetManager.SelectedTarget.Name;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateForm();

				if (targetManager != null)
				{
					_owner.RenderTargetManager.RefreshList();
					_owner.RenderTargetManager.SelectedTarget = targetManager.TargetManager.SelectedTarget;
					targetManager.Dispose();
				}

				if (imageManager != null)
				{
					_owner.ImageManager.RefreshList();
					_owner.ImageManager.SelectedImage = imageManager.ImageManager.SelectedImage;
					imageManager.Dispose();
				}

				targetManager = null;
				imageManager = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			try
			{
				// Just get the selected image.
				if (_isPropBagEditor)
				{
					_propBagImage = null;
					if (!checkRenderTarget.Checked)
						_propBagImage = ImageCache.Images[comboImages.Text];
					else
						_propBagImage = ((RenderImage)RenderTargetCache.Targets[comboImages.Text]).Image;

					return;
				}

				// Update each sprite.
				foreach (ListViewItem item in _selectedSprites)
				{
					if (checkRenderTarget.Checked)
						_owner.SpriteManager.Sprites[item.Name].Bind(((RenderImage)RenderTargetCache.Targets[comboImages.Text]).Image);
					else
						_owner.SpriteManager.Sprites[item.Name].Bind(ImageCache.Images[comboImages.Text]);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to re-bind the sprite.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the checkRenderTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkRenderTarget_Click(object sender, EventArgs e)
		{
			if (!checkRenderTarget.Checked)
			{
				buttonImageManager.Image = Properties.Resources.photo_scenery;
				tips.SetToolTip(buttonImageManager, "Open the image manager.");
			}
			else
			{
				buttonImageManager.Image = Properties.Resources.target;
				tips.SetToolTip(buttonImageManager, "Open the render target manager.");
			}

			FillCombos();
			ValidateForm();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboImages control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboImages_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formBindImage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public formBindImage(formMain owner)
			: this()
		{
			_owner = owner;

			FillCombos();
		}
		#endregion
	}
}