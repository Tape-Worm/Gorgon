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
// Created: Monday, May 07, 2007 11:30:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using GorgonLibrary;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface to create a new sprite.
	/// </summary>
	public partial class formNewSprite 
		: Form
	{
		#region Variables.
		private bool _imageManagerVisible;			// Flag to indicate that the image manager is visible.
		private bool _targetManagerVisible;			// Flag to indicate that the image manager is visible.
		private formMain _owner;					// Owning window.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether we're binding to a render target or not.
		/// </summary>
		public bool IsRenderTarget
		{
			get
			{
				return checkRenderTarget.Checked;
			}
		}

		/// <summary>
		/// Property to return the selected image.
		/// </summary>
		public Image Image
		{
			get
			{
				if (checkRenderTarget.Checked)
					return ((RenderImage)RenderTargetCache.Targets[comboImages.Text]).Image;
				
				return ImageCache.Images[comboImages.Text];
			}
		}

		/// <summary>
		/// Property to return the sprite name.
		/// </summary>
		public string SpriteName
		{
			get
			{
				return textName.Text;
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
			if ((textName.Text != string.Empty) && (comboImages.Text != string.Empty))
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
				var images = ImageCache.Images.Where(image => (!image.IsResource) && (image.ImageType == ImageType.Normal));
				foreach (var image in images)
					comboImages.Items.Add(image.Name);

				if (_owner.ImageManager.SelectedImage != null)
					comboImages.Text = _owner.ImageManager.SelectedImage.Name;
				else
					comboImages.Text = string.Empty;
			}
			else
			{
				var targets = RenderTargetCache.Targets.Where(target => (_owner.ValidRenderTarget(target.Name)));
				foreach (var image in targets)
					comboImages.Items.Add(image.Name);

				if (_owner.RenderTargetManager.SelectedTarget != null)
					comboImages.Text = _owner.RenderTargetManager.SelectedTarget.Name;
				else
					comboImages.Text = string.Empty;
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkRenderTarget control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkRenderTarget_CheckedChanged(object sender, EventArgs e)
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
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
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

		/// <summary>
		/// Handles the Click event of the buttonBrowse control.
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

				Focus();
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFormat control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (_owner.SpriteManager.Sprites.Contains(textName.Text))
			{
				UI.ErrorBox(this, "The sprite '" + textName.Text + "' already exists.");
				DialogResult = DialogResult.None;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the formNewSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formNewSprite_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Enter) && (buttonOK.Enabled))
			{
				buttonOK_Click(this, EventArgs.Empty);
				DialogResult = DialogResult.OK;
			}

			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Re-display managers if necessary.
			_owner.ImageManager.Visible = _imageManagerVisible ;
			_owner.RenderTargetManager.Visible = _targetManagerVisible;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_owner = Owner as formMain;

			FillCombos();			

			// Get last visible state.
			_imageManagerVisible = _owner.ImageManager.Visible;
			_targetManagerVisible = _owner.RenderTargetManager.Visible;

			// Update interface.
			ValidateForm();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formNewSprite()
		{
			InitializeComponent();
		}
		#endregion
	}
}