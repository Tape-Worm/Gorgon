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
// Created: Wednesday, November 12, 2014 12:02:02 AM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	/// <summary>
	/// Main UI for sprite editing.
	/// </summary>
	partial class PanelSpriteEditor 
		: ContentPanel
	{
		#region Variables.
		// The sprite being edited.
		private GorgonSpriteContent _content;
		// The plug-in that created the content.
		private GorgonSpriteEditorPlugIn _plugIn;
		#endregion

		#region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the panel.
		/// </summary>
		private void ValidateControls()
		{
			buttonSave.Enabled = buttonRevert.Enabled = _content.HasChanges;
		}

		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_content.Commit();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRevert control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonRevert_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORSPR_DLG_REVERT_CONFIRM) == ConfirmationResult.No)
				{
					return;
				}

				_content.Revert();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to perform localization on the control text properties.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORSPR_CONTENT_TYPE;

			buttonSave.Text = Resources.GORSPR_TEXT_SAVE;
			buttonRevert.Text = Resources.GORSPR_TEXT_REVERT;

			labelZoom.Text = Resources.GORSPR_TEXT_ZOOM;
		}

		/// <summary>
		/// Function to draw the current sprite.
		/// </summary>
		private void DrawSprite()
		{
			_content.Sprite.Scale = new Vector2(4);
			var halfSprite = (Point)(new Vector2(_content.Sprite.ScaledSize.X / 2.0f, _content.Sprite.ScaledSize.Y / 2.0f));
			var halfScreen = (Point)(new Vector2(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f));
			var spritePosition = new Vector2(halfScreen.X - halfSprite.X, halfScreen.Y - halfSprite.Y);

			var imageScale = new Vector2(_content.Sprite.ScaledSize.X / _content.TextureRegion.Width, _content.Sprite.ScaledSize.Y / _content.TextureRegion.Height);
			var imagePosition = new Vector2(spritePosition.X - _content.TextureRegion.X * imageScale.X, spritePosition.Y - _content.TextureRegion.Y * imageScale.Y);

			var spriteRect = new RectangleF(spritePosition, _content.Sprite.ScaledSize);

			_content.TextureSprite.ScaledSize = (Point)Vector2.Modulate(imageScale, _content.TextureSprite.Size);
			_content.TextureSprite.Position = (Point)imagePosition;
			_content.TextureSprite.SmoothingMode = SmoothingMode.Smooth;
			_content.Sprite.Position = spritePosition;
			
			_content.TextureSprite.Draw();

			// Clear the area behind the sprite so we don't get the texture blending with itself.
			_content.Renderer.Drawing.BlendingMode = BlendingMode.None;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.Blit(_content.BackgroundTexture, spriteRect, _content.BackgroundTexture.ToTexel(spriteRect));

			_content.Sprite.Draw();

			_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			_content.Renderer.Drawing.DrawRectangle(spriteRect, new GorgonColor(0, 0, 1.0f, 0.5f));
			_content.Sprite.Scale = new Vector2(1);
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			ValidateControls();
		}

		/// <summary>
		/// Function to draw the
		/// </summary>
		public void Draw()
		{
			DrawSprite();
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			if (_content == null)
			{
				throw new InvalidCastException(string.Format(Resources.GORSPR_ERR_CONTENT_NOT_SPRITE, Content.Name));
			}

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelSpriteEditor"/> class.
		/// </summary>
		public PanelSpriteEditor(GorgonSpriteContent spriteContent, GorgonInputFactory input)
			: base(spriteContent, input)
		{
			_content = spriteContent;
			_plugIn = (GorgonSpriteEditorPlugIn)_content.PlugIn;

			InitializeComponent();
		}
		#endregion
	}
}
