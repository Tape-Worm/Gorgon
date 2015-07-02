#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, January 18, 2015 11:07:06 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Interface used to create a new sprite.
	/// </summary>
	public partial class FormNewSprite 
		: GorgonFlatForm
	{
		#region Properties.
		/// <summary>
		/// Property to return the name of the sprite.
		/// </summary>
		public string SpriteName
		{
			get
			{
				return textName.Text.FormatFileName();
			}
		}

		/// <summary>
		/// Property to return the texture bound to the sprite.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the image editor plug-in to use.
		/// </summary>
		public IImageEditorPlugIn ImageEditor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the dependency for the texture bound to this sprite.
		/// </summary>
		public Dependency Dependency
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonSelectTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSelectTexture_Click(object sender, EventArgs e)
		{
			EditorFileDialog editorDialog = null;

			try
			{
				editorDialog = new EditorFileDialog
				               {
					               Text = Resources.GORSPR_DLG_SELECT_TEXTURE,
					               StartDirectory = GorgonSpriteEditorPlugIn.Settings.LastTexturePath,
					               FileView = FileViews.Large
				               };

				editorDialog.FileTypes.Add(ImageEditor.ContentType);

				if (editorDialog.ShowDialog(this) == DialogResult.Cancel)
				{
					return;
				}

				if (Texture != null)
				{
					Texture.Dispose();
				}

				// Read the new texture.
				using (Stream file = editorDialog.OpenFile())
				{
					using (IImageEditorContent image = ImageEditor.ImportContent(editorDialog.Files[0], file))
					{
						if (image.Image.Settings.ImageType != ImageType.Image2D)
						{
							throw new GorgonException(GorgonResult.CannotRead, Resources.GORSPR_ERR_2D_TEXTURE_ONLY);
						}

						Texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>(editorDialog.Filename, image.Image);

						// Create the new dependency to pass back to the initialization.
						Dependency = new Dependency(editorDialog.Files[0], GorgonSpriteContent.TextureDependencyType);
					}

					labelTexturePath.Text = editorDialog.Filename;
				}

			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (editorDialog != null)
				{
					editorDialog.Dispose();
				}

				Cursor = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			buttonOK.Enabled = !string.IsNullOrWhiteSpace(textName.Text) && !string.IsNullOrWhiteSpace(labelTexturePath.Text);
		}

		/// <summary>
		/// Function to localize the controls on the form.
		/// </summary>
		private void LocalizeControls()
		{
			Text = Resources.GORSPR_TEXT_NEW_SPRITE;

			labelName.Text = Resources.GORSPR_TEXT_NEW_SPRITE_NAME;
			labelTexture.Text = Resources.GORSPR_TEXT_NEW_SPRITE_TEXTURE;

			tipInfo.ToolTipTitle = Resources.GORSPR_TEXT_HELP;
			tipInfo.SetToolTip(buttonSelectTexture, Resources.GORSPR_TEXT_NEW_SPRITE_TIP_TEXTURE);
		}

		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			LocalizeControls();
			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormNewSprite"/> class.
		/// </summary>
		public FormNewSprite()
		{
			InitializeComponent();
		}
		#endregion
	}
}
