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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	/// <summary>
	/// Mode for the sprite editor.
	/// </summary>
	public enum SpriteEditorMode
	{
		/// <summary>
		/// In view mode.
		/// </summary>
		View = 0,
		/// <summary>
		/// In edit mode.
		/// </summary>
		Edit = 1
	}

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
		// The amount of zoom on the display.
		private float _zoom = 1.0f;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the sprite editor mode.
		/// </summary>
		public SpriteEditorMode SpriteEditorMode
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Resize event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelSprite_Resize(object sender, EventArgs e)
		{
			if (menuItemToWindow.Checked)
			{
				CalculateToWindow();
			}
			else
			{
				SetUpScrolling();
			}
		}

		/// <summary>
		/// Function to set up scrolling.
		/// </summary>
		private void SetUpScrolling()
		{
			Size panelSize = panelOuter.ClientSize;

			// Turn off the resizing event so we don't get recursive calls.
			panelSprite.Resize -= panelSprite_Resize;
			scrollHorizontal.Scroll -= OnScroll;
			scrollVertical.Scroll -= OnScroll;

			try
			{
				// Only use scrolling with regular textures and not cube maps.
				if ((_content == null)
					|| (_content.Sprite == null)
					|| ((_content.Texture == null) && (SpriteEditorMode == SpriteEditorMode.Edit)))
				{
					panelHScroll.Visible = panelVScroll.Visible = false;
					panelSprite.ClientSize = panelSprite.ClientSize;
					return;
				}

				Size spriteSize;

				switch (SpriteEditorMode)
				{
					case SpriteEditorMode.Edit:
						Debug.Assert(_content.Texture != null, "Texture should not be NULL");

						spriteSize = _content.Texture.Settings.Size;
						break;
					default:
						spriteSize = (Size)(_content.Sprite.Size * _zoom);
						break;
				}

				if (spriteSize.Width > panelSprite.ClientSize.Width)
				{
					panelHScroll.Visible = true;
					panelSize.Height = panelSize.Height - panelHScroll.ClientSize.Height;
				}
				else
				{
					scrollHorizontal.Value = 0;
					panelHScroll.Visible = false;
				}

				if (spriteSize.Height > panelSprite.ClientSize.Height)
				{
					panelVScroll.Visible = true;
					panelSize.Width = panelSize.Width - panelVScroll.ClientSize.Width;
				}
				else
				{
					scrollVertical.Value = 0;
					panelVScroll.Visible = false;
				}

				if (panelHScroll.Visible)
				{
					scrollHorizontal.Maximum = ((spriteSize.Width - panelSize.Width) + (scrollHorizontal.LargeChange)).Max(0) - 1;
					scrollHorizontal.Scroll += OnScroll;
				}

				if (panelVScroll.Visible)
				{
					scrollVertical.Maximum = ((spriteSize.Height - panelSize.Height) + (scrollVertical.LargeChange)).Max(0) - 1;
					scrollVertical.Scroll += OnScroll;
				}

				panelSprite.ClientSize = panelSize;
			}
			finally
			{
				// Re-enable resizing.
				panelSprite.Resize += panelSprite_Resize;
			}
		}

		/// <summary>
		/// Function called when a scrollbar is moved.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
		private void OnScroll(object sender, ScrollEventArgs e)
		{
			var scroller = (ScrollBar)sender;

			if (scroller == scrollVertical)
			{
				scrollVertical.Value = e.NewValue;
			}

			if (scroller == scrollHorizontal)
			{
				scrollHorizontal.Value = e.NewValue;
			}

			_content.Draw();
		}

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
		/// Handles the Click event of the zoomItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void zoomItem_Click(object sender, EventArgs e)
		{
			var selectedItem = (ToolStripMenuItem)sender;

			try
			{
				var items = dropDownZoom.DropDownItems.Cast<ToolStripItem>().Where(item => item is ToolStripMenuItem);

				// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
				foreach (ToolStripMenuItem control in items)
				{
					if (control != sender)
					{
						control.Checked = false;
					}
				}

				_zoom = float.Parse(selectedItem.Tag.ToString(),
				                    NumberStyles.Float,
				                    NumberFormatInfo.InvariantInfo);

				dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORSPR_TEXT_ZOOM, selectedItem.Text);

				if (menuItemToWindow.Checked)
				{
					CalculateToWindow();
				}
				else
				{
					SetUpScrolling();

					if (scrollHorizontal.Visible)
					{
						scrollHorizontal.Value = scrollHorizontal.Maximum / 2;
					}

					if (scrollVertical.Visible)
					{
						scrollVertical.Value = scrollVertical.Maximum / 2;
					}
				}

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

		/// <summary>
		/// Function to perform localization on the control text properties.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORSPR_CONTENT_TYPE;

			buttonSave.Text = Resources.GORSPR_TEXT_SAVE;
			buttonRevert.Text = Resources.GORSPR_TEXT_REVERT;

			menuItemToWindow.Text = Resources.GORSPR_TEXT_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);

			dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORSPR_TEXT_ZOOM, menuItem100.Text);
		}

		/// <summary>
		/// Function to calculate the zoom required to scale the sprite to the window.
		/// </summary>
		private void CalculateToWindow()
		{
			var windowScale = new Vector2(panelSprite.ClientSize.Width / _content.Sprite.Size.X, panelSprite.ClientSize.Height / _content.Sprite.Size.Y);

			_zoom = windowScale.Y < windowScale.X ? windowScale.Y : windowScale.X;

			SetUpScrolling();
		}

		/// <summary>
		/// Function to draw the current sprite.
		/// </summary>
		private void DrawSprite()
		{
			_content.Sprite.Scale = new Vector2(_zoom);

			var halfSprite = (Point)(new Vector2(_content.Sprite.ScaledSize.X / 2.0f, _content.Sprite.ScaledSize.Y / 2.0f));
			var halfScreen = (Point)(new Vector2(panelSprite.ClientSize.Width / 2.0f, panelSprite.ClientSize.Height / 2.0f));

			if (_content.Sprite.ScaledSize.Y > panelSprite.ClientSize.Height)
			{
				halfSprite.Y = halfScreen.Y = 0;
			}

			if (_content.Sprite.ScaledSize.X > panelSprite.ClientSize.Width)
			{
				halfSprite.X = halfScreen.X = 0;
			}

			var spritePosition = new Vector2(halfScreen.X - halfSprite.X - scrollHorizontal.Value, halfScreen.Y - halfSprite.Y - scrollVertical.Value);
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

			SetUpScrolling();
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

			SetUpScrolling();
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
