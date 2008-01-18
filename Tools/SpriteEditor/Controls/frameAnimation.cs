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
// Created: Thursday, July 26, 2007 11:11:08 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Data;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Control representing the interface for frame based animations.
	/// </summary>
	public partial class frameAnimation 
		: UserControl, IDropIn
	{
		#region Variables.
		private SpriteDocumentList _spriteList = null;			// List of sprite documents.
		private Drawing.Image _current = null;					// Current cached image.
		private string _currentImageName = string.Empty;		// Current image name.
		private TrackFrame _currentTrack = null;				// Current track.
		private SortedList<float, Drawing.Image> _frames = null;// Frame list.
		private Sprite _owner = null;							// Sprite that owns this track.
		private Animation _animation = null;					// Sprite animation.
		private RenderWindow _animationDisplay = null;			// Display render window for the animation.
		private float _currentKeyTime = 0.0f;					// Current key frame time.
		private bool _isPlaying = false;						// Flag to indicate that an animation is playing.
		private formAnimationEditor _editorOwner = null;		// Animation editor.
		private Color _bgColor = Color.DarkBlue;				// Background color.
		private int _keyIndex = 0;										// Key index.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current track.
		/// </summary>
		public TrackFrame CurrentTrack
		{
			get
			{
				return _currentTrack;
			}
			set
			{
				_currentTrack = value;

				// Reset the idle handler.
				Gorgon.Stop();

				// Create a new render window.
				if (!RenderTargetCache.Targets.Contains("AnimationDisplay"))
					_animationDisplay = new RenderWindow("AnimationDisplay", panelRender, false);
				else
					_animationDisplay = RenderTargetCache.Targets["AnimationDisplay"] as RenderWindow;

				// Reset idle handler.
				Gorgon.Idle -= new FrameEventHandler(Gorgon_Idle);
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);

				Gorgon.Go();

				if (_currentTrack != null)
				{
					_animation = _currentTrack.Owner;
					_owner = _animation.Owner as Sprite;					
					// Set the owner position to the axis.
					_owner.Position = _owner.Axis;					

					// Start rendering.
					_animation.Enabled = true;
					_animation.AnimationStopped -= new EventHandler(_animation_AnimationStopped);
					_animation.AnimationStopped += new EventHandler(_animation_AnimationStopped);
					_animation.AnimationState = AnimationState.Playing;
				}

				ValidateForm();
				GetSprites();
				_keyIndex = 0;
			}
		}

		/// <summary>
		/// Property to set or return the current frame time.
		/// </summary>
		public float CurrentTime
		{
			get
			{
				return _currentKeyTime;
			}
			set
			{
				_currentKeyTime = value;
				ValidateForm();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the AnimationStopped event of the _animation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _animation_AnimationStopped(object sender, EventArgs e)
		{
			_animation.Reset();
			_animation.AnimationState = AnimationState.Playing;
			_isPlaying = false;
			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		private void ValidateForm()
		{
			buttonFirstKeyFrame.Enabled = false;
			buttonLastKeyFrame.Enabled = false;
			buttonNextKeyframe.Enabled = false;
			buttonPreviousKeyFrame.Enabled = false;
			buttonRemoveKeyFrame.Enabled = false;
			buttonSetKeyFrame.Enabled = false;
			buttonPlay.Enabled = false;
			buttonStop.Enabled = false;

			if (_currentTrack == null)
				return;			

			// We can only stop while playing.
			if (_isPlaying)
			{
				buttonStop.Enabled = true;
				listFrames.Enabled = false;				
				return;
			}
			else
				listFrames.Enabled = true;

			// Ensure we have something to add.
			if (listFrames.SelectedItems.Count > 0)
				buttonSetKeyFrame.Enabled = true;

			if (_animation.TotalKeyCount > 0)
				buttonPlay.Enabled = true;

			if (_currentTrack.KeyCount > 0)
			{				
				if (_currentTrack.Contains(_currentKeyTime))
					buttonRemoveKeyFrame.Enabled = true;

				if (_currentKeyTime > _animation.FrameTrack.GetKeyAtIndex(0).Time)
				{
					buttonFirstKeyFrame.Enabled = true;
					buttonPreviousKeyFrame.Enabled = true;
				}
				if (_currentKeyTime < _animation.FrameTrack.GetKeyAtIndex(_animation.FrameTrack.KeyCount - 1).Time)
				{
					buttonNextKeyframe.Enabled = true;
					buttonLastKeyFrame.Enabled = true;
				}
			}

			if (imagesSprites.Images.Count > 0)
				buttonGetAll.Enabled = true;
			else
				buttonGetAll.Enabled = false;
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			if (_animationDisplay == null)
				return;

			// Set to our render window.
			Gorgon.CurrentRenderTarget = _animationDisplay;

			_animationDisplay.Clear(_bgColor);

			// Only display if the key count is valid.
			if (_animation.TotalKeyCount > 0)
			{
				_owner.Draw();
				if (_isPlaying)
				{
					_animation.Advance(e.TimingData);
					_editorOwner.SetTime(_animation.CurrentTime);
				}
				else
				{
					// Jump to the specific time, if the time does not exist as a key frame (interpolated or not), then do nothing.
					_animation.CurrentTime = _currentKeyTime;
				}
			}

			_animationDisplay.Update();
			Gorgon.CurrentRenderTarget = null;
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

				if ((sprite.Image != null) && (sprite.Image.RenderImage == null))
				{					
					// We need to scale this image down to 64x64.
					graph.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighSpeed;
					graph.DrawImage(_current, new Drawing.RectangleF(0, 0, 64.0f, 64.0f), sprite.ImageRegion, Drawing.GraphicsUnit.Pixel);
				}
				else
					graph.DrawString("RT", this.Font, Drawing.Brushes.Yellow, 0, 0); 

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
		/// Function to retrieve the currently loaded sprites.
		/// </summary>
		private void GetSprites()
		{
			ListViewItem item = null;		// List view item.
			ListViewItem selected = null;	// Last selected item.

			Cursor.Current = Cursors.WaitCursor;
			listFrames.BeginUpdate();

			try
			{
				// If we haven't cached the sprite images yet, do so.
				if (imagesSprites.Images.Count == 0)
				{
					foreach (SpriteDocument sprite in _spriteList)
					{
						if (sprite.Sprite != _owner)
							imagesSprites.Images.Add(sprite.Name + ".@Image", GetSpriteImage(sprite.Sprite));
					}
				}

				if (listFrames.SelectedItems.Count > 0)
					selected = listFrames.SelectedItems[0];
				else
					selected = null;

				// Remove sprites.
				listFrames.Items.Clear();

				// Populate.
				foreach (SpriteDocument sprite in _spriteList)
				{
					// Don't re-add the sprite being animated, the reasoning is that the
					// animated sprite constantly has its properties updated by the animation.
					// Thus if we were able to re-add ourselves, we'd end up with the last frame 
					// of animation we were on.  This obviously is not the effect we want.
					if (_owner != sprite.Sprite)
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
			catch (SharpException sEx)
			{
				UI.ErrorBox(ParentForm, "Error retrieving sprite list.", sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Error retrieving sprite list.", ex);
			}

			listFrames.EndUpdate();
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				if (ParentForm != null)
					_editorOwner = ParentForm as formAnimationEditor;

				// Get the bg color settings.
				Settings.Root = null;
				_bgColor = Color.FromArgb(255, Color.FromArgb(Convert.ToInt32(Settings.GetSetting("BGColor", "-16777077"))));
			}
		}

		/// <summary>
		/// Function to clean up.
		/// </summary>
		public void CleanUp()
		{
			// Remove the idle handler.
			_keyIndex = 0;
			_isPlaying = false;
			_animation.Reset();
			_editorOwner.SetTime(0);

			Gorgon.Idle -= new FrameEventHandler(Gorgon_Idle);
			RenderTargetCache.Targets[_animationDisplay.Name].Dispose();
			_animationDisplay = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public frameAnimation(SpriteDocumentList documents)
		{
			InitializeComponent();

			_spriteList = documents;
			_frames = new SortedList<float, System.Drawing.Image>();			
		}
		#endregion

		/// <summary>
		/// Function to set the current key to the specified sprite-frame.
		/// </summary>
		/// <param name="sprite">Sprite to set.</param>
		private void SetKeyFrame(SpriteDocument sprite)
		{
			string spriteName = string.Empty;			// Selected frame sprite name.
			KeyFrame key = null;						// Key that we're editing/creating.

			// Now, check for an existing key.
			if (_currentTrack.Contains(_currentKeyTime))
			{
				key = _currentTrack[_currentKeyTime] as KeyFrame;
				key.Frame = new Frame(sprite.Sprite.Image, sprite.Sprite.ImageOffset, sprite.Sprite.Size);
			}
			else
				_currentTrack.CreateKey(_currentKeyTime, sprite.Sprite.Image, sprite.Sprite.ImageOffset, sprite.Sprite.Size);

			_spriteList[_animation.Owner.Name].Changed = true;
		}

		/// <summary>
		/// Handles the Click event of the buttonSetKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSetKeyFrame_Click(object sender, EventArgs e)
		{
			try
			{
				SetKeyFrame(_spriteList[listFrames.SelectedItems[0].Name]);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Unable to set this key frame.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPlay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPlay_Click(object sender, EventArgs e)
		{
			_isPlaying = true;
			_keyIndex = 0;
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonStop control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonStop_Click(object sender, EventArgs e)
		{
			_keyIndex = 0;
			_isPlaying = false;
			_animation.Reset();
			_editorOwner.SetTime(0);
			ValidateForm();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listFrames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listFrames_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Handles the ItemDrag event of the listFrames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemDragEventArgs"/> instance containing the event data.</param>
		private void listFrames_ItemDrag(object sender, ItemDragEventArgs e)
		{
			listFrames.DoDragDrop(_spriteList[((ListViewItem)e.Item).Name], DragDropEffects.All);
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
				SpriteDocument sprite = e.Data.GetData(typeof(SpriteDocument)) as SpriteDocument;	// Sprite being dragged.
				SetKeyFrame(sprite);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Unable to set this key frame.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonFirstKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFirstKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = 0;
			_editorOwner.SetTime(_animation.FrameTrack.GetKeyAtIndex(0).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonLastKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonLastKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = _animation.FrameTrack.KeyCount - 1;
			_editorOwner.SetTime(_animation.FrameTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonPreviousKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPreviousKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex--;

			if (_keyIndex < 0)
				_keyIndex = _animation.FrameTrack.KeyCount - 1;

			_editorOwner.SetTime(_animation.FrameTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonNextKeyframe control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNextKeyframe_Click(object sender, EventArgs e)
		{
			_keyIndex++;

			if (_keyIndex >= _animation.FrameTrack.KeyCount)
				_keyIndex = 0;
			_editorOwner.SetTime(_animation.FrameTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonRemoveKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRemoveKeyFrame_Click(object sender, EventArgs e)
		{
			if (UI.ConfirmBox("This will remove the key frame at the current index.\nAre you sure?") == ConfirmationResult.Yes)
			{
				_spriteList[_animation.Owner.Name].Changed = true;		
				_animation.FrameTrack.Remove(CurrentTime);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonClearKeys control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonClearKeys_Click(object sender, EventArgs e)
		{
			if (UI.ConfirmBox("This will clear the key frames for the animation.\nAre you sure?") == ConfirmationResult.Yes)
			{
				_spriteList[_animation.Owner.Name].Changed = true;
				_animation.FrameTrack.ClearKeys();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonGetAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonGetAll_Click(object sender, EventArgs e)
		{
			formListAll allSprites = null;		// Sprite frame list.

			try
			{
				if (_currentTrack.KeyCount > 0)
				{
					if (UI.ConfirmBox(ParentForm, "The track already has keys assigned to it.  Importing all the frames will remove these keys.\nContinue?") == ConfirmationResult.No)
						return;
				}

				allSprites = new formListAll();
				allSprites.SpriteOwner = _owner;
				allSprites.Interval = (decimal)(_animation.Length / imagesSprites.Images.Count);
				allSprites.SpriteList = _spriteList;
				allSprites.Frames = imagesSprites;
				allSprites.CurrentTrack = _currentTrack;
				allSprites.AnimationLength = _animation.Length;
				allSprites.RefreshList();

				// Begin import.
				allSprites.ShowDialog(ParentForm);
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
	}
}
