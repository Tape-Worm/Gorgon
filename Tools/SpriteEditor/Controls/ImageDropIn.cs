#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 03, 2008 1:57:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Image animation drop in component.
	/// </summary>
	public partial class ImageDropIn 
		: AnimationDropIn
	{
		#region Variables.
		private ToolStripButton buttonImportFrames = null;			// Frame importer.
		private Drawing.Image _current = null;						// Current cached image.
		private string _currentImageName = string.Empty;			// Current image name.
		private SpriteDocument _selectedSprite = null;				// Currently selected sprite.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SelectedIndexChanged event of the listFrames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listFrames_SelectedIndexChanged(object sender, EventArgs e)
		{
			string spriteName = string.Empty;		// Sprite name.

			_selectedSprite = null;
			if (listFrames.SelectedIndices.Count < 1)
				return;

			spriteName = listFrames.SelectedItems[0].Name;
			if (Editor.Sprites.Contains(spriteName))
				_selectedSprite = Editor.Sprites[spriteName];
			ValidateForm();
		}

		/// <summary>
		/// Handles the ItemDrag event of the listFrames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemDragEventArgs"/> instance containing the event data.</param>
		private void listFrames_ItemDrag(object sender, ItemDragEventArgs e)
		{
			listFrames.DoDragDrop(Editor.Sprites[((ListViewItem)e.Item).Name], DragDropEffects.All);
		}

		/// <summary>
		/// Handles the DragOver event of the panelRender control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void panelRender_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;
		}

		/// <summary>
		/// Handles the DragDrop event of the panelRender control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void panelRender_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				_selectedSprite = e.Data.GetData(typeof(SpriteDocument)) as SpriteDocument;	// Sprite being dragged.
				SetKeyFrame();
				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Unable to set this key frame.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the importFrames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonImportFrames_Click(object sender, EventArgs e)
		{
			formListAll allSprites = null;		// Sprite frame list.

			try
			{
				if (CurrentTrack.KeyCount > 0)
				{
					if (UI.ConfirmBox(ParentForm, "The track already has keys assigned to it.  Importing all the frames will remove these keys.\nContinue?") == ConfirmationResult.No)
						return;
				}

				allSprites = new formListAll();
				allSprites.SpriteOwner = Sprite.Sprite;
				allSprites.Interval = (decimal)(Animation.Length / imageSprites.Images.Count);
				allSprites.SpriteList = Editor.Sprites;
				allSprites.Frames = imageSprites;
				allSprites.CurrentTrack = CurrentTrack;
				allSprites.AnimationLength = Animation.Length;
				allSprites.RefreshList();

				// Begin import.
				if (allSprites.ShowDialog(ParentForm) == DialogResult.OK)
					Sprite.Changed = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "There was an error while trying to import the sprites.", ex);
			}
			finally
			{
				ValidateForm();
				if (allSprites != null)
					allSprites.Dispose();
				allSprites = null;
			}
		}

		/// <summary>
		/// Function to get the current sprite image.
		/// </summary>
		/// <param name="spriteImage">Image to convert.</param>
		private void GetGDIImage(Image spriteImage)
		{
			// Replace image with nothing if we don't have a sprite image.
			if ((spriteImage == null) || (spriteImage.RenderImage != null))
			{
				_currentImageName = string.Empty;
				if (_current != null)
					_current.Dispose();
				_current = null;
				return;
			}

			// Cache the sprite image.
			if ((_currentImageName != spriteImage.Name) || (_current == null))
			{
				if (_current != null)
					_current.Dispose();
				_current = spriteImage.SaveBitmap();
				_currentImageName = spriteImage.Name;
			}
		}

		/// <summary>
		/// Function to extract an image from the sprite passed.
		/// </summary>
		/// <param name="sprite">Sprite to extract.</param>
		/// <returns>A GDI+ image representing that sprite.</returns>
		private Drawing.Image GetSpriteImage(Sprite sprite)
		{
			Drawing.Image result = null;		// Resulting image.
			Drawing.Graphics graph = null;		// Graphics interface.
			Drawing.SolidBrush brush = null;	// Brush to fill image with.			

			try
			{
				// Resulting image.
				result = new Drawing.Bitmap(64, 64);
				graph = Drawing.Graphics.FromImage(result);

				// Fill with the sprite background color.
				brush = new Drawing.SolidBrush(sprite.Color);
				graph.FillRectangle(brush, 0, 0, 64, 64);

				GetGDIImage(sprite.Image);

				// We need to scale this image down to 64x64.
				graph.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighSpeed;
				graph.DrawImage(_current, new Drawing.RectangleF(0, 0, 64.0f, 64.0f), sprite.ImageRegion, Drawing.GraphicsUnit.Pixel);

				return result;
			}
			catch
			{
				if (result != null)
					result.Dispose();
				result = null;
				throw;
			}
			finally
			{
				if (brush != null)
					brush.Dispose();
				if (graph != null)
					graph.Dispose();

				brush = null;
				graph = null;
			}
		}

		/// <summary>
		/// Function to retrieve the list of sprite frames.
		/// </summary>
		private void GetSpriteFrames()
		{
			ListViewItem item = null;		// List view item.
			ListViewItem selected = null;	// Last selected item.

			Cursor.Current = Cursors.WaitCursor;
			listFrames.BeginUpdate();

			try
			{
				// If we haven't cached the sprite images yet, do so.
				if (imageSprites.Images.Count == 0)
				{
					foreach (SpriteDocument sprite in Editor.Sprites)
					{
						if ((sprite != Sprite) && (sprite.IncludeInAnimations) && (sprite.BoundImage != null) && (sprite.BoundImage.RenderImage == null))
							imageSprites.Images.Add(sprite.Name + ".@Image", GetSpriteImage(sprite.Sprite));
					}
				}

				if (listFrames.SelectedItems.Count > 0)
					selected = listFrames.SelectedItems[0];
				else
					selected = null;

				// Remove sprites.
				listFrames.Items.Clear();

				// Populate.
				foreach (SpriteDocument sprite in Editor.Sprites)
				{
					// Don't re-add the sprite being animated, the reasoning is that the
					// animated sprite constantly has its properties updated by the animation.
					// Thus if we were able to re-add ourselves, we'd end up with the last frame 
					// of animation we were on.  This obviously is not the effect we want.
					if ((Sprite != sprite) && (sprite.IncludeInAnimations) && (sprite.BoundImage != null) && (sprite.BoundImage.RenderImage == null))
					{
						item = listFrames.Items.Add(sprite.Name);
						item.Name = sprite.Name;
						item.ImageKey = sprite.Name + ".@Image";
					}
				}

				// Select the first frame.
				if (listFrames.Items.Count > 0)
				{
					if (selected != null)
						listFrames.Items[selected.Name].Selected = true;
					else
						listFrames.Items[0].Selected = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Error retrieving sprite list.", ex);
			}

			listFrames.EndUpdate();
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Function to set the current key.
		/// </summary>
		protected override void SetKeyFrameImpl()
		{
			string spriteName = string.Empty;			// Selected frame sprite name.
			KeyImage key = null;						// Key that we're editing/creating.

			base.SetKeyFrameImpl();

			if (_selectedSprite == null)
				return;
			
			// Now, check for an existing key.
			if (CurrentTrack.Contains(CurrentTime))
			{
				key = CurrentTrack[CurrentTime] as KeyImage;
				key.Image = _selectedSprite.BoundImage;
				key.ImageOffset = _selectedSprite.ImageLocation;
				key.ImageSize = _selectedSprite.Size;
			}
			else
			{
				key = new KeyImage(CurrentTime, _selectedSprite.BoundImage);
				key.ImageOffset = _selectedSprite.ImageLocation;
				key.ImageSize = _selectedSprite.Size;
				CurrentTrack.AddKey(key);
			}

			Sprite.Changed = true;			
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		protected override void ValidateForm()
		{
			buttonImportFrames.Enabled = false;
			base.ValidateForm();

			if ((listFrames.SelectedItems.Count > 0) && (!IsPlaying))
				buttonSetKeyFrame.Enabled = true;
			if ((imageSprites.Images.Count > 0) && (!IsPlaying))
				buttonImportFrames.Enabled = true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Retrieve the sprite frame list.
				GetSpriteFrames();
				
				// Automatically add the first key.
				//if (CurrentTrack.KeyCount == 0)
				//{
				//    KeyImage newKey = null;		// New key.
										
				//    newKey = new KeyImage(0.0f, Sprite.BoundImage);
				//    newKey.ImageOffset = Sprite.ImageLocation;
				//    newKey.ImageSize = Sprite.Size;
				//    CurrentTrack.AddKey(newKey);
				//    Sprite.Changed = true;
				//}

				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error installing the image animation component.", ex);
			}
		}

		/// <summary>
		/// Function to get the settings for the drop-in.
		/// </summary>
		public override void GetSettings()
		{
			Settings.Root = "AnimationDropInSettings";
			splitAnimation.SplitterDistance = Convert.ToInt32(Settings.GetSetting("ImageDropIn", "200"));
			Settings.Root = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public override void CleanUp()
		{

			if (_current != null)
				_current.Dispose();

			Settings.Root = "AnimationDropInSettings";
			Settings.SetSetting("ImageDropIn", splitAnimation.SplitterDistance.ToString());
			Settings.Root = null;
			
			base.CleanUp();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageDropIn"/> class.
		/// </summary>
		public ImageDropIn()
		{
			InitializeComponent();

			buttonImportFrames = new ToolStripButton();
			buttonImportFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			buttonImportFrames.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.import1;
			buttonImportFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
			buttonImportFrames.Name = "buttonImportFrames";
			buttonImportFrames.Size = new System.Drawing.Size(23, 22);
			buttonImportFrames.Text = "Use all sprites.";
			buttonImportFrames.Click += new EventHandler(buttonImportFrames_Click);

			stripAnimation.Items.Add(new ToolStripSeparator());
			stripAnimation.Items.Add(buttonImportFrames);			
		}
		#endregion

	}
}
