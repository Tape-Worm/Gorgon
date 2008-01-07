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
	/// Control representing the interface for transformation based animations.
	/// </summary>
	public partial class transformAnimation 
		: UserControl, IDropIn
	{
		#region Variables.
		private TrackTransform _currentTrack = null;			// Current track.
		private Sprite _owner = null;							// Sprite that owns this track.
		private Animation _animation = null;					// Sprite animation.
		private RenderWindow _animationDisplay = null;			// Display render window for the animation.
		private float _currentKeyTime = 0.0f;					// Current key frame time.
		private bool _isPlaying = false;						// Flag to indicate that an animation is playing.
		private formAnimationEditor _editorOwner = null;		// Animation editor.
		private Color _bgColor = Color.DarkBlue;				// Background color.
		private SpriteDocumentList _spriteList = null;			// Sprite document list.
		private int _keyIndex = 0;								// Key index.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current track.
		/// </summary>
		public TrackTransform CurrentTrack
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

				// Get base transforms.
				numericTranslateX.Value = (Decimal)_owner.Position.X;
				numericTranslateY.Value = (Decimal)_owner.Position.Y;
				numericScaleX.Value = (Decimal)_owner.Scale.X;
				numericScaleY.Value = (Decimal)_owner.Scale.Y;
				numericAngle.Value = (Decimal)_owner.Rotation;
				numericAxisX.Value = (Decimal)_owner.Axis.X;
				numericAxisY.Value = (Decimal)_owner.Axis.Y;
				numericImageX.Value = (Decimal)_owner.ImageOffset.X;
				numericImageY.Value = (Decimal)_owner.ImageOffset.Y;
				numericSpriteWidth.Value = (Decimal)_owner.Size.X;
				numericSpriteHeight.Value = (Decimal)_owner.Size.Y;

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
				KeyTransform transformKey = null;		// Transform key.

				_currentKeyTime = value;

				if ((_currentTrack != null) && (_currentTrack.KeyCount > 0))
				{
					// Get the current values at this key.
					transformKey = _currentTrack[value] as KeyTransform;

					// Get values.
					if (transformKey != null)
					{
						numericTranslateX.Value = (Decimal)transformKey.Position.X;
						numericTranslateY.Value = (Decimal)transformKey.Position.Y;
						numericScaleX.Value = (Decimal)transformKey.Scale.X;
						numericScaleY.Value = (Decimal)transformKey.Scale.Y;
						numericAngle.Value = (Decimal)transformKey.Rotation;
						numericAxisX.Value = (Decimal)transformKey.Axis.X;
						numericAxisY.Value = (Decimal)transformKey.Axis.Y;
						numericImageX.Value = (Decimal)transformKey.ImageOffset.X;
						numericImageY.Value = (Decimal)transformKey.ImageOffset.Y;
						numericSpriteWidth.Value = (Decimal)transformKey.Size.X;
						numericSpriteHeight.Value = (Decimal)transformKey.Size.Y;
					}
				}

				ValidateForm();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the current key to the specified sprite-frame.
		/// </summary>
		private void SetKeyFrame()
		{
			KeyTransform key = null;						// Key that we're editing/creating.
			Vector2D pos;									// Position.
			Vector2D scale;									// Scale.
			float rot;										// Rotation angle.
			Vector2D axis;									// Axis;
			Vector2D imageOrigin;							// Image origin.
			Vector2D imageSize;								// Sprite size.

			// Get values.
			pos = new Vector2D((float)numericTranslateX.Value, (float)numericTranslateY.Value);
			scale = new Vector2D((float)numericScaleX.Value, (float)numericScaleY.Value);
			rot = (float)numericAngle.Value;
			axis = new Vector2D((float)numericAxisX.Value, (float)numericAxisY.Value);
			imageOrigin = new Vector2D((float)numericImageX.Value, (float)numericImageY.Value);
			imageSize = new Vector2D((float)numericSpriteWidth.Value, (float)numericSpriteHeight.Value);

			// Now, check for an existing key.
			if (_currentTrack.Contains(_currentKeyTime))
			{
				key = _currentTrack[_currentKeyTime] as KeyTransform;
				if (key.Position != pos)
					key.Position = pos;
				if (key.Scale != scale)
					key.Scale = scale;
				if (!MathUtility.EqualFloat(rot, key.Rotation, 0.0001f))
					key.Rotation = rot;
				if (key.Axis != axis)
					key.Axis = axis;
				if (key.ImageOffset != imageOrigin)
					key.ImageOffset = imageOrigin;
				if (key.Size != imageSize)
					key.Size = imageSize;
			}
			else
			{
				key = _currentTrack.CreateKey(_currentKeyTime, pos, scale, rot);
				key.Axis = axis;
				key.ImageOffset = imageOrigin;
				key.Size = imageSize;
			}

			switch (comboInterpolation.Text.ToLower())
			{
				case "linear":
					key.InterpolationMode = InterpolationMode.Linear;
					break;
				case "spline":
					key.InterpolationMode = InterpolationMode.Spline;
					break;
				default:
					key.InterpolationMode = InterpolationMode.None;
					break;
			}

			_spriteList[_animation.Owner.Name].Changed = true;
			ValidateForm();
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
		/// Handles the Click event of the buttonFirstKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFirstKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = 0;
			_editorOwner.SetTime(_animation.TransformationTrack.GetKeyAtIndex(0).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonLastKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonLastKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = _animation.TransformationTrack.KeyCount - 1;
			_editorOwner.SetTime(_animation.TransformationTrack.GetKeyAtIndex(_keyIndex).Time);
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
				_keyIndex = _animation.TransformationTrack.KeyCount - 1;

			_editorOwner.SetTime(_animation.TransformationTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonNextKeyframe control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNextKeyframe_Click(object sender, EventArgs e)
		{
			_keyIndex++;

			if (_keyIndex >= _animation.TransformationTrack.KeyCount)
				_keyIndex = 0;
			_editorOwner.SetTime(_animation.TransformationTrack.GetKeyAtIndex(_keyIndex).Time);
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
				_animation.TransformationTrack.Remove(CurrentTime);
				ValidateForm();
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
				_animation.TransformationTrack.ClearKeys();
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the AnimationStopped event of the _animation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _animation_AnimationStopped(object sender, EventArgs e)
		{
			_keyIndex = 0;
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
				panelTransformControls.Enabled = false;				
				return;
			}
			else
				panelTransformControls.Enabled = true;

			// Ensure we have something to add.
			buttonSetKeyFrame.Enabled = true;

			if (_animation.TotalKeyCount > 0)
				buttonPlay.Enabled = true;

			if (_currentTrack.KeyCount > 0)
			{				
				if (_currentTrack.Contains(_currentKeyTime))
					buttonRemoveKeyFrame.Enabled = true;

				if (_currentKeyTime > _animation.TransformationTrack.GetKeyAtIndex(0).Time)
				{
					buttonFirstKeyFrame.Enabled = true;
					buttonPreviousKeyFrame.Enabled = true;
				}
				if (_currentKeyTime < _animation.TransformationTrack.GetKeyAtIndex(_animation.TransformationTrack.KeyCount - 1).Time)
				{
					buttonNextKeyframe.Enabled = true;
					buttonLastKeyFrame.Enabled = true;
				}
			}
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
			if (_currentTrack.KeyCount < 1)
			{
				// Reset.
				_owner.Rotation = 0;
				_owner.UniformScale = 1.0f;
				_owner.SetPosition(0, 0);				
			}
			
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

			_animationDisplay.Update();
			Gorgon.CurrentRenderTarget = null;
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

				comboInterpolation.Text = "Linear";
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

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteDocs">List of sprite documents.</param>
		public transformAnimation(SpriteDocumentList spriteDocs)
		{
			InitializeComponent();

			_spriteList = spriteDocs;
		}
		#endregion
	}
}
