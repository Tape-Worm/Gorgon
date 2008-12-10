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
// Created: Friday, May 02, 2008 9:21:21 PM
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
using System.Linq;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Control representing the interface for frame based animations.
	/// </summary>
	public partial class AnimationDropIn
		: UserControl
	{
		#region Variables.
		private bool _isPlaying = false;						// Flag to indicate that an animation is playing.
		private Color _backgroundColor = Color.DarkBlue;		// Background color.
		private int _keyIndex = 0;								// Index of the key frames.
		private SpriteDocument _owner = null;					// Sprite that owns this animation.
		private Animation _animation = null;					// Animation that owns this track.
		private RenderWindow _display = null;					// Animation display.
		private float _currentTime = 0.0f;						// Current key time.
		private formAnimationEditor _editor = null;				// Animation editor.
		private Track _currentTrack = null;						// Current track.
		private KeyFrame _copiedFrame = null;					// Copied frame.
		private bool _showBoundingBox = false;					// Flag to show a bounding box around the sprite.
		private bool _showBoundingCircle = false;				// Flagt o show a bounding circle around the sprite.
		private Drawing.Color _axisColor = Drawing.Color.Red;	// Axis color.
		private Vector2D _originalSize = Vector2D.Zero;			// Original size for the sprite.
		private Vector2D _originalStart = Vector2D.Zero;		// Original image location start for the sprite.
		#endregion

		#region Properties.		
		/// <summary>
		/// Property to set or return whether there's a drag operation in progress.
		/// </summary>
		protected bool IsDragging
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the value when a drag operation is in progress.
		/// </summary>
		protected Vector2D DragValue
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the sprite that owns this animation.
		/// </summary>
		protected SpriteDocument Sprite
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the animation that owns this track.
		/// </summary>
		protected Animation Animation
		{
			get
			{
				return _animation;
			}
		}

		/// <summary>
		/// Property to return the display window.
		/// </summary>
		protected RenderWindow Display
		{
			get
			{
				return _display;
			}
		}

		/// <summary>
		/// Property to return the animation editor form.
		/// </summary>
		protected formAnimationEditor Editor
		{
			get
			{
				return _editor;
			}
		}

		/// <summary>
		/// Property to set or return the flag to indicate that we're playing.
		/// </summary>
		protected bool IsPlaying
		{
			get
			{
				return _isPlaying;
			}
			set
			{
				_isPlaying = value;
			}
		}

		/// <summary>
		/// Property to set or return the index of the key.
		/// </summary>
		protected int KeyIndex
		{
			get
			{
				return _keyIndex;
			}
			set
			{
				_keyIndex = value;
			}
		}

		/// <summary>
		/// Property to set or return the show track/keys button state.
		/// </summary>
		public bool ShowTrackKeys
		{
			get
			{
				return buttonViewTracks.Checked;
			}
			set
			{
				buttonViewTracks.Checked = value;
			}
		}

		/// <summary>
		/// Property to set or return the list of animations to play.
		/// </summary>
		public List<Animation> PlayAnimations
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the track is empty.
		/// </summary>
		public bool IsEmpty
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current track.
		/// </summary>
		public Track CurrentTrack
		{
			get
			{
				return _currentTrack;
			}
			set
			{
				_currentTrack = value;
				FixTimeErrors();
				ValidateForm();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonPlayOther control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPlayOther_Click(object sender, EventArgs e)
		{
			formAnimationSelector selector = null;

			try
			{
				selector = new formAnimationSelector();
				selector.UpdatePlaylist(PlayAnimations);
				selector.GetAnimations(_currentTrack.Owner, _owner.Sprite);
				if (selector.ShowDialog(ParentForm) == DialogResult.OK)
				{
					PlayAnimations = selector.CurrentPlaylist;

					foreach (Animation anim in _owner.Sprite.Animations.Where(animation => animation != _currentTrack.Owner))
					{
						anim.AnimationState = AnimationState.Stopped;
						anim.Reset();
					}

					foreach (Animation anim in PlayAnimations)
					{
						anim.Reset();
						anim.AnimationState = AnimationState.Playing;
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Unable to play other animations.", ex);
			}
			finally
			{
				if (selector != null)
					selector.Dispose();
				selector = null;
			}
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
				SetKeyFrame();
				ValidateForm();
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
			var anims = from anim in _owner.Sprite.Animations
						where !PlayAnimations.Contains(anim) && anim != _currentTrack.Owner
						select anim;

			foreach (Animation anim in anims)
			{
				anim.Reset();
				anim.AnimationState = AnimationState.Stopped;
			}

			foreach (Animation anim in PlayAnimations)
			{
				anim.Reset();
				anim.AnimationState = AnimationState.Playing;
			}

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
			_editor.SetTime(0);
			ValidateForm();

			foreach (Animation anim in PlayAnimations)
			{
				anim.Reset();
				anim.AnimationState = AnimationState.Stopped;
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
			_editor.SetTime(_currentTrack.GetKeyAtIndex(0).Time);
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonLastKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonLastKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = _currentTrack.KeyCount - 1;
			_editor.SetTime(_currentTrack.GetKeyAtIndex(_keyIndex).Time);
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonPreviousKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPreviousKeyFrame_Click(object sender, EventArgs e)
		{
			if ((_keyIndex == -1) && (CurrentTrack.KeyCount > 0))
			{
				KeyFrame prev = CurrentTrack.FindNearest(CurrentTime).PreviousKey;
				if (prev != null)
					_keyIndex = CurrentTrack.IndexOfKey(prev.Time);
				else
					_keyIndex = CurrentTrack.KeyCount - 1;
			}
			else
				_keyIndex--;

			if (_keyIndex < 0)
				_keyIndex = _currentTrack.KeyCount - 1;

			_editor.SetTime(_currentTrack.GetKeyAtIndex(_keyIndex).Time);			
			ValidateForm();
		}

		/// <summary>
		/// Handles the Click event of the buttonNextKeyframe control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNextKeyframe_Click(object sender, EventArgs e)
		{
			if ((_keyIndex == -1) && (CurrentTrack.KeyCount > 0))
			{
				KeyFrame next = CurrentTrack.FindNearest(CurrentTime).NextKey;
				if (next != null)
					_keyIndex = CurrentTrack.IndexOfKey(next.Time);
				else
					_keyIndex = 0;
			}
			else
				_keyIndex++;

			if (_keyIndex >= _currentTrack.KeyCount)
				_keyIndex = 0;
			_editor.SetTime(_currentTrack.GetKeyAtIndex(_keyIndex).Time);			
			ValidateForm();
		}

		/// <summary>
		/// Function to fix time related rounding errors.
		/// </summary>
		private void FixTimeErrors()
		{
			if (CurrentTrack == null)
				return;

			float currentTime = 0.0f;
			float adjustedTime = 0.0f;
			int frame = 0;
			KeyFrame newKey = null;

			for (int i = CurrentTrack.KeyCount - 1; i >= 0 ; i--)
			{
				currentTime = CurrentTrack.GetKeyAtIndex(i).Time;
				frame = MathUtility.RoundInt((currentTime / 1000.0f) * CurrentTrack.Owner.FrameRate);
				adjustedTime = ((float)frame / CurrentTrack.Owner.FrameRate) * 1000.0f;

				if (!MathUtility.EqualFloat(currentTime, adjustedTime, 0.001f))
				{
					newKey = CurrentTrack.GetKeyAtIndex(i).Clone();
					CurrentTrack.RemoveKeyAtIndex(i);
					newKey.Time = adjustedTime;
					CurrentTrack.AddKey(newKey);
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRemoveKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRemoveKeyFrame_Click(object sender, EventArgs e)
		{
			if (UI.ConfirmBox("This will remove the key frame at the current index.\nAre you sure?") == ConfirmationResult.Yes)
				RemoveKeyFrame();
		}

		/// <summary>
		/// Handles the Click event of the buttonClearKeys control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonClearKeys_Click(object sender, EventArgs e)
		{
			if (UI.ConfirmBox("This will clear the key frames for the animation.\nAre you sure?") == ConfirmationResult.Yes)
				ClearKeyFrames();
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Vector2D newPosition = Vector2D.Zero;		// New position.
			Vector2D axis = Vector2D.Zero;				// Axis.
			if (_display == null)
				return;

			// Set to our render window.
			Gorgon.CurrentRenderTarget = _display;

			_display.Clear(_backgroundColor);

			if (Editor.ImageBackground != null)
			{
				if (Editor.ImageSize == Vector2D.Zero)
					Editor.ImageBackground.Blit(Editor.ImageLocation.X, Editor.ImageLocation.Y);
				else
					Editor.ImageBackground.Blit(Editor.ImageLocation.X, Editor.ImageLocation.Y, Editor.ImageSize.X, Editor.ImageSize.Y);
			}

			// Only display if the key count is valid.
			Sprite.Sprite.Position = Vector2D.Subtract(Vector2D.Add(new Vector2D(_display.Width / 2.0f, _display.Height / 2.0f), Sprite.Sprite.Axis), new Vector2D(Sprite.Sprite.Width / 2.0f, Sprite.Sprite.Height / 2.0f));
			Sprite.Sprite.Draw();
			Display.BeginDrawing();

			if ((_showBoundingBox) || (_showBoundingCircle))
			{
				if (_showBoundingBox)
					Display.Rectangle(Sprite.Sprite.AABB.Left, Sprite.Sprite.AABB.Top, Sprite.Sprite.AABB.Width, Sprite.Sprite.AABB.Height, Drawing.Color.Red);
				if (_showBoundingCircle)
					Display.Circle(Sprite.Sprite.BoundingCircle.Center.X, Sprite.Sprite.BoundingCircle.Center.Y, Sprite.Sprite.BoundingCircle.Radius, Drawing.Color.Cyan);
			}

			newPosition = Sprite.Sprite.FinalPosition;
			newPosition.X = MathUtility.Round(newPosition.X);
			newPosition.Y = MathUtility.Round(newPosition.Y);
			if ((CurrentTrack.EditCanDragValues == GorgonLibrary.Internal.EditorDragType.Axis) && (IsDragging))
				newPosition = DragValue;

			if (_axisColor == Drawing.Color.Red)
				_axisColor = Drawing.Color.Yellow;
			else
				_axisColor = Drawing.Color.Red;
			Display.VerticalLine(newPosition.X, newPosition.Y - 5, 5, _axisColor);
			Display.VerticalLine(newPosition.X, newPosition.Y + 1, 5, _axisColor);
			Display.HorizontalLine(newPosition.X - 5, newPosition.Y, 5, _axisColor);
			Display.HorizontalLine(newPosition.X + 1, newPosition.Y, 5, _axisColor);

			Display.EndDrawing();

			if (_animation.HasKeys)
			{
				if (_isPlaying)
				{
					_animation.Advance(e.TimingData);
					foreach (Animation anim in PlayAnimations)
						anim.Advance(e.TimingData);
					_editor.SetTime(_animation.CurrentTime);
				}
				else
					_animation.CurrentTime = _currentTime;		// Jump to the specific time, if the time does not exist as a key frame (interpolated or not), then do nothing.
			}

			IdleEvent();

			_display.Update();
			Gorgon.CurrentRenderTarget = null;
		}

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
		/// Handles the Click event of the buttonPasteFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPasteFrame_Click(object sender, EventArgs e)
		{
			KeyFrame newFrame = null;		// Copy of the copy.

			try
			{
				if (_copiedFrame == null)
					return;

				if (CurrentTrack.Contains(CurrentTime))
				{
					if (UI.ConfirmBox("The current time has a " + CurrentTrack.Name + " key frame attached to it already.\nOverwrite this key frame?") == ConfirmationResult.No)
						return;
				}

				newFrame = _copiedFrame.Clone();
				newFrame.Time = CurrentTime;
				CurrentTrack.AddKey(newFrame);
				ValidateForm();
				if (Editor != null)
					Editor.RefreshTrackView();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error trying to paste the frame.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonCopyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCopyFrame_Click(object sender, EventArgs e)
		{
			try
			{
				_copiedFrame = CurrentTrack[CurrentTime].Clone();
				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error trying to copy the frame.", ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonCutFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCutFrame_Click(object sender, EventArgs e)
		{
			try
			{
				CutFrame();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error trying to cut the frame.", ex);
			}
		}

		/// <summary>
		/// Function to cut a frame.
		/// </summary>
		protected virtual void CutFrame()
		{
			_copiedFrame = CurrentTrack[CurrentTime].Clone();
			CurrentTrack.Remove(CurrentTime);
			ValidateForm();
			if (Editor != null)
				Editor.RefreshTrackView();
		}

		/// <summary>
		/// Function to allow rendering code from a child class.
		/// </summary>
		protected virtual void IdleEvent()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
		/// <returns>
		/// true if the key was processed by the control; otherwise, false.
		/// </returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			// Get all numeric fields.
			var controlCount = (from NumericUpDown control in splitAnimation.Panel2.Controls.OfType<NumericUpDown>()
						   where (control != null) && (control.Focused)
						   select control).Any();

			if ((controlCount) && ((keyData == Keys.Enter) || (keyData == (Keys.LButton | Keys.MButton | Keys.Back))) && (NumericFieldEnterPressed()))
				return true;

			return base.ProcessDialogKey(keyData);
		}

		/// <summary>
		/// Function to handle enter key presses in numeric up down fields.
		/// </summary>
		/// <returns>TRUE if handled, FALSE if not.</returns>
		protected virtual bool NumericFieldEnterPressed()
		{
			return false;
		}

		/// <summary>
		/// Handles the Click event of the menuitemOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemOffset_Click(object sender, EventArgs e)
		{
			formBackImageEdit edit = null;

			try
			{
				edit = new formBackImageEdit();
				edit.Text = "Update background offset.";
				edit.labelImageData.Text = "Offset:";
				edit.numericX.Value = (Decimal)Editor.ImageLocation.X;
				edit.numericY.Value = (Decimal)Editor.ImageLocation.Y;
				if (edit.ShowDialog(Editor) == DialogResult.OK)
					Editor.ImageLocation = new Vector2D((float)edit.numericX.Value, (float)edit.numericY.Value);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(Editor, ex);
			}
			finally
			{
				if (edit != null)
					edit.Dispose();
				edit = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemClearImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemClearImage_Click(object sender, EventArgs e)
		{
			Editor.ImageBackground = null;
		}

		/// <summary>
		/// Handles the Click event of the menuitemStretchImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemStretchImage_Click(object sender, EventArgs e)
		{
			formBackImageEdit edit = null;

			try
			{
				edit = new formBackImageEdit();
				edit.Text = "Update background size.";
				edit.labelImageData.Text = "Size:";
				if (Editor.ImageSize != Vector2D.Zero)
				{
					edit.numericX.Value = (Decimal)Editor.ImageSize.X;
					edit.numericY.Value = (Decimal)Editor.ImageSize.Y;
				}
				else
				{
					edit.numericX.Value = (Decimal)Editor.ImageBackground.Width;
					edit.numericY.Value = (Decimal)Editor.ImageBackground.Height;
				}
				if (edit.ShowDialog(Editor) == DialogResult.OK)
					Editor.ImageSize = new Vector2D((float)edit.numericX.Value, (float)edit.numericY.Value);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(Editor, ex);
			}
			finally
			{
				if (edit != null)
					edit.Dispose();
				edit = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonViewTracks control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonViewTracks_Click(object sender, EventArgs e)
		{
			Editor.ShowViewer();
		}

		/// <summary>
		/// Handles the Click event of the menuItemLoadImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemLoadImage_Click(object sender, EventArgs e)
		{
			formImageManager imageManager = null;

			try
			{
				imageManager = new formImageManager(formMain.Me);
				imageManager.StartPosition = FormStartPosition.CenterParent;
				imageManager.ShowDialog(Editor);
				formMain.Me.ImageManager.RefreshList();
				Editor.ImageBackground = imageManager.ImageManager.SelectedImage;
				Editor.ImageLocation = Vector2D.Zero;
				Editor.ImageSize = Vector2D.Zero;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(Editor, ex);
			}
			finally
			{
				if (imageManager != null)
					imageManager.Dispose();
				imageManager = null;
				ValidateForm();
			}
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		protected virtual void ValidateForm()
		{
			buttonFirstKeyFrame.Enabled = false;
			buttonLastKeyFrame.Enabled = false;
			buttonNextKeyframe.Enabled = false;
			buttonPreviousKeyFrame.Enabled = false;
			buttonRemoveKeyFrame.Enabled = false;
			buttonSetKeyFrame.Enabled = false;
			buttonPlay.Enabled = false;
			buttonStop.Enabled = false;
			buttonClearKeys.Enabled = false;
			buttonCopyFrame.Enabled = false;
			buttonCutFrame.Enabled = false;
			buttonPasteFrame.Enabled = false;
			buttonPlayOther.Enabled = false;
			buttonViewTracks.Enabled = false;
			menuitemClearImage.Enabled = false;
			menuitemOffset.Enabled = false;
			menuitemStretchImage.Enabled = false;

			if (_animation == null)
				return;

			if (_owner.Sprite.Animations.Count > 1)
				buttonPlayOther.Enabled = true;

			// We can only stop while playing.
			if (_isPlaying)
			{
				buttonStop.Enabled = true;
				return;
			}

			if (_animation.HasKeys)
				buttonPlay.Enabled = true;

			if (_copiedFrame != null)
				buttonPasteFrame.Enabled = true;

			if (_animation.HasKeys)
				buttonViewTracks.Enabled = true;

			if (_currentTrack.KeyCount > 0)
			{
				buttonClearKeys.Enabled = true;
				if (_currentTrack.Contains(_currentTime))
				{
					buttonRemoveKeyFrame.Enabled = true;
					if (Editor.MaxFrames > 1)
					{
						buttonCopyFrame.Enabled = true;
						buttonCutFrame.Enabled = true;
					}
				}

				if (_currentTime > _currentTrack.GetKeyAtIndex(0).Time)
				{
					buttonFirstKeyFrame.Enabled = true;
					buttonPreviousKeyFrame.Enabled = true;
				}
				if (_currentTime < _currentTrack.GetKeyAtIndex(_currentTrack.KeyCount - 1).Time)
				{
					buttonNextKeyframe.Enabled = true;
					buttonLastKeyFrame.Enabled = true;
				}
			}

			if (CurrentTrack.KeyCount > 0)
				_keyIndex = CurrentTrack.IndexOfKey(CurrentTime);

			if (Editor.ImageBackground != null)
			{
				menuitemClearImage.Enabled = true;
				menuitemOffset.Enabled = true;
				menuitemStretchImage.Enabled = true;
			}
		}

		/// <summary>
		/// Implementation function for SetKeyFrame.
		/// </summary>
		protected virtual void SetKeyFrameImpl()
		{
		}

		/// <summary>
		/// Function to set the current key.
		/// </summary>
		protected void SetKeyFrame()
		{
			IsEmpty = false;
			SetKeyFrameImpl();
			if (Editor != null)
				Editor.RefreshTrackView();
		}

		/// <summary>
		/// Function to remove a key frame.
		/// </summary>
		protected virtual void RemoveKeyFrame()
		{
			Sprite.Changed = true;
			_currentTrack.Remove(CurrentTime);
			ValidateForm();
			if (_currentTrack.KeyCount == 0)
				ResetSprite();
			if (Editor != null)
				Editor.RefreshTrackView();			
		}

		/// <summary>
		/// Function to clear all the key frames.
		/// </summary>
		protected virtual void ClearKeyFrames()
		{
			Sprite.Changed = true;
			_currentTrack.ClearKeys();
			ValidateForm();
			ResetSprite();
			if (Editor != null)
				Editor.RefreshTrackView();

		}

		/// <summary>
		/// Function to reset the sprite.
		/// </summary>
		protected virtual void ResetSprite()
		{
			switch (_currentTrack.Name.ToLower())
			{
				case "scaledwidth":
					Sprite.Sprite.ScaledWidth = Sprite.Sprite.Width;
					break;
				case "scaledheight":
					Sprite.Sprite.ScaledHeight = Sprite.Sprite.Height;
					break;
				case "scaleddimensions":
				case "scale":
				case "uniformscale":
					Sprite.Sprite.UniformScale = 1.0f;
					break;
				case "alphamaskvalue":
					Sprite.Sprite.AlphaMaskValue = 0;
					break;
				case "axis":
					Sprite.Sprite.Axis = Vector2D.Zero;
					break;
				case "imageoffset":
					if (_animation.Tracks["Image"].KeyCount == 0)
						Sprite.Sprite.ImageOffset = _originalStart;
					break;
				case "position":
					Sprite.Sprite.Position = Vector2D.Zero;
					break;
				case "rotation":
					Sprite.Sprite.Rotation = 0;
					break;
				case "color":
					Sprite.Sprite.Color = Color.White;
					break;
				case "opacity":
					Sprite.Sprite.Opacity = 255;
					break;
				case "size":
					if (_animation.Tracks["Image"].KeyCount == 0)
						Sprite.Sprite.Size = _originalSize;
					break;
				case "width":
					if (_animation.Tracks["Image"].KeyCount == 0)
						Sprite.Sprite.Width = _originalSize.X;
					break;
				case "height":
					if (_animation.Tracks["Image"].KeyCount == 0)
						Sprite.Sprite.Height = _originalSize.Y;
					break;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			Sprite sprite;			// Sprite owner.

			base.OnLoad(e);

			if (!DesignMode)
			{
				if (ParentForm != null)
					_editor = ParentForm as formAnimationEditor;

				// Get the bg color settings.
				Settings.Root = null;
				_backgroundColor = Color.FromArgb(255, Color.FromArgb(Convert.ToInt32(Settings.GetSetting("BGColor", "-16777077"))));
				_showBoundingBox = (string.Compare(Settings.GetSetting("ShowBoundingBox", "True"), "true", true) == 0);
				_showBoundingCircle = (string.Compare(Settings.GetSetting("ShowBoundingCircle", "True"), "true", true) == 0);

				// Get a reference to the render window display.
				_display = new RenderWindow("AnimationDisplay", panelRender, false);
				// Reset idle handler.
				Gorgon.Stop();
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
				Gorgon.Go();

				_animation = CurrentTrack.Owner;
				sprite = _animation.Owner as Sprite;
				_owner = _editor.Sprites[sprite.Name]; 
				_owner.Sprite.Position = Vector2D.Zero;
				_owner.Sprite.Rotation = 0.0f;
				_owner.Sprite.UniformScale = 1.0f;
				_originalSize = _owner.Size;
				_originalStart = _owner.ImageLocation;
				_keyIndex = 0;
				_animation.AnimationStopped += new EventHandler(_animation_AnimationStopped);
				_animation.AnimationState = AnimationState.Playing;

				ValidateForm();

				// If we have keys, we're not empty.
				if (CurrentTrack.KeyCount > 0)
					IsEmpty = false;
			}
		}

		/// <summary>
		/// Function to get the settings for the drop-in.
		/// </summary>
		public virtual void GetSettings()
		{
		}

		/// <summary>
		/// Property to set or return the current key frame time.
		/// </summary>
		/// <value></value>
		public virtual float CurrentTime
		{
			get
			{
				return _currentTime;
			}
			set
			{
				_currentTime = value;
				ValidateForm();
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public virtual void CleanUp()
		{
			if (_animation != null)
			{
				_animation.CurrentTime = 0.0f;
				_animation.AnimationState = AnimationState.Stopped;
				_animation.AnimationStopped -= new System.EventHandler(_animation_AnimationStopped);
				_animation.Reset();

				// If the track is empty, remove the default keys.
				if ((CurrentTrack != null) && (IsEmpty) && (CurrentTrack.KeyCount > 0))
					CurrentTrack.ClearKeys();					
			}

			Gorgon.Idle -= new FrameEventHandler(Gorgon_Idle);

			// Remove the idle handler.
			_keyIndex = 0;
			_isPlaying = false;
			_editor.SetTime(0);

			if (_display != null)
				_display.Dispose();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationDropIn"/> class.
		/// </summary>
		public AnimationDropIn()
		{
			InitializeComponent();

			// Default to empty.
			IsEmpty = true;
		}
		#endregion
	}
}
