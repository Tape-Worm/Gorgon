#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
		#endregion

		#region Properties.
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
				ValidateForm();
			}
		}
		#endregion

		#region Methods.		
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
				_keyIndex = _currentTrack.KeyCount - 1;

			_editor.SetTime(_currentTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonNextKeyframe control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNextKeyframe_Click(object sender, EventArgs e)
		{
			_keyIndex++;

			if (_keyIndex >= _currentTrack.KeyCount)
				_keyIndex = 0;
			_editor.SetTime(_currentTrack.GetKeyAtIndex(_keyIndex).Time);
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
			if (_display == null)
				return;

			// Set to our render window.
			Gorgon.CurrentRenderTarget = _display;

			_display.Clear(_backgroundColor);

			// Only display if the key count is valid.
			if (_animation.HasKeys)
			{
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
				if (_axisColor == Drawing.Color.Red)
					_axisColor = Drawing.Color.Yellow;
				else
					_axisColor = Drawing.Color.Red;
				Display.VerticalLine(newPosition.X, newPosition.Y - 5, 5, _axisColor);
				Display.VerticalLine(newPosition.X, newPosition.Y + 1, 5, _axisColor);
				Display.HorizontalLine(newPosition.X - 5, newPosition.Y, 5, _axisColor);
				Display.HorizontalLine(newPosition.X + 1, newPosition.Y, 5, _axisColor);

				Display.EndDrawing();
				
				if (_isPlaying)
				{
					_animation.Advance(e.TimingData);
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
				_copiedFrame = CurrentTrack[CurrentTime].Clone();
				CurrentTrack.Remove(CurrentTime);
				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error trying to cut the frame.", ex);
			}
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
			foreach (Control control in splitAnimation.Panel2.Controls)
			{
				if (control is NumericUpDown)
				{
					if (((NumericUpDown)control).Focused)
					{
						if ((keyData == Keys.Enter) || (keyData == (Keys.LButton | Keys.MButton | Keys.Back)))
						{
							if (NumericFieldEnterPressed())
								return true;
						}
					}
				}
			}

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

			if (_animation == null)
				return;

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
		}

		/// <summary>
		/// Function to set the current key.
		/// </summary>
		protected virtual void SetKeyFrame()
		{
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
		}

		/// <summary>
		/// Function to reset the sprite.
		/// </summary>
		protected virtual void ResetSprite()
		{
			switch (_currentTrack.Name.ToLower())
			{
				case "scale":
					Sprite.Sprite.Scale = Vector2D.Unit;
					break;
				case "uniformscale":
					Sprite.Sprite.UniformScale = 1.0f;
					break;
				case "position":
					Sprite.Sprite.Position = Vector2D.Zero;
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
				_owner.Sprite.Position = _owner.Axis;
				_keyIndex = 0;
				_animation.Enabled = true;
				_animation.AnimationStopped += new EventHandler(_animation_AnimationStopped);
				_animation.AnimationState = AnimationState.Playing;

				ValidateForm();
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
		}
		#endregion
	}
}
