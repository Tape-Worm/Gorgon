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
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Control representing the interface for color based animations.
	/// </summary>
	public partial class colorAnimation 
		: UserControl, IDropIn
	{
		#region Variables.
		private TrackColor _currentTrack = null;				// Current track.
		private Sprite _owner = null;							// Sprite that owns this track.
		private Animation _animation = null;					// Sprite animation.
		private RenderWindow _animationDisplay = null;			// Display render window for the animation.
		private float _currentKeyTime = 0.0f;					// Current key frame time.
		private bool _isPlaying = false;						// Flag to indicate that an animation is playing.
		private formAnimationEditor _editorOwner = null;		// Animation editor.
		private Color _bgColor = Color.DarkBlue;				// Background color.
		private Color _currentColor = Color.White;				// Current color.
		private SpriteDocumentList _spriteList = null;			// Sprite document list.
		private ColorPicker _colorPicker = null;				// Color picker.
		private int _keyIndex = 0;								// Key index.
		private bool _noUpdateEvent = false;					// No update numeric color event.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current track.
		/// </summary>
		public TrackColor CurrentTrack
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

				// Get base color.
				_currentColor = _owner.Color;
				numericAlpha.Value = _owner.AlphaMaskValue;
				UpdateColorLabel();
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
				KeyColor colorKey = null;		// Color key.

				_currentKeyTime = value;

				if ((_currentTrack != null) && (_currentTrack.KeyCount > 0))
				{
					// Get the current values at this key.
					colorKey = _currentTrack[_currentKeyTime] as KeyColor;

					// Get values.
					if (colorKey != null)
					{
						_currentColor = colorKey.Color;
						numericAlpha.Value = colorKey.AlphaMaskValue;

						switch (colorKey.InterpolationMode)
						{
							case InterpolationMode.Linear:
								comboInterpolation.Text = "Linear";
								break;
							case InterpolationMode.Spline:
								comboInterpolation.Text = "Spline";
								break;
							default:
								comboInterpolation.Text = "None";
								break;
						}
					}
				}

				ValidateForm();
				UpdateColorLabel();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the color label.
		/// </summary>
		private void UpdateColorLabel()
		{
			_noUpdateEvent = true;
			numericA.Value = _currentColor.A;
			numericR.Value = _currentColor.R;
			numericG.Value = _currentColor.G;
			numericB.Value = _currentColor.B;
			pictureColor.Refresh();
			_noUpdateEvent = false;
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
				panelColorControls.Enabled = false;				
				return;
			}
			else
				panelColorControls.Enabled = true;

			// Ensure we have something to add.
			buttonSetKeyFrame.Enabled = true;

			if (_animation.TotalKeyCount > 0)
				buttonPlay.Enabled = true;

			if (_currentTrack.KeyCount > 0)
			{				
				if (_currentTrack.Contains(_currentKeyTime))
					buttonRemoveKeyFrame.Enabled = true;

				if (_currentKeyTime > _animation.ColorTrack.GetKeyAtIndex(0).Time)
				{
					buttonFirstKeyFrame.Enabled = true;
					buttonPreviousKeyFrame.Enabled = true;
				}
				if (_currentKeyTime < _animation.ColorTrack.GetKeyAtIndex(_animation.ColorTrack.KeyCount - 1).Time)
				{
					buttonNextKeyframe.Enabled = true;
					buttonLastKeyFrame.Enabled = true;
				}
			}

			pictureColor.Refresh();
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
			_owner.SetPosition((float)(_animationDisplay.Width / 2) - _owner.Width / 2.0f, (float)(_animationDisplay.Height / 2) - _owner.Height / 2.0f);
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
		public colorAnimation(SpriteDocumentList spriteDocs)
		{
			InitializeComponent();

			_spriteList = spriteDocs;
		}
		#endregion

		/// <summary>
		/// Function to set the current key to the specified sprite-frame.
		/// </summary>
		private void SetKeyFrame()
		{
			KeyColor key = null;						// Key that we're editing/creating.

			// Now, check for an existing key.
			if (_currentTrack.Contains(_currentKeyTime))
			{
				key = _currentTrack[_currentKeyTime] as KeyColor;
				if (key.Color != _currentColor)
					key.Color = _currentColor;
				if (key.AlphaMaskValue != (int)numericAlpha.Value)
					key.AlphaMaskValue = (int)numericAlpha.Value;
			}
			else
			{
				key = _currentTrack.CreateKey(_currentKeyTime, _currentColor);
				key.AlphaMaskValue = (int)numericAlpha.Value;
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
			_editorOwner.SetTime(_animation.ColorTrack.GetKeyAtIndex(0).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonLastKeyFrame control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonLastKeyFrame_Click(object sender, EventArgs e)
		{
			_keyIndex = _animation.ColorTrack.KeyCount - 1;
			_editorOwner.SetTime(_animation.ColorTrack.GetKeyAtIndex(_keyIndex).Time);
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
				_keyIndex = _animation.ColorTrack.KeyCount - 1;

			_editorOwner.SetTime(_animation.ColorTrack.GetKeyAtIndex(_keyIndex).Time);
		}

		/// <summary>
		/// Handles the Click event of the buttonNextKeyframe control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNextKeyframe_Click(object sender, EventArgs e)
		{
			_keyIndex++;

			if (_keyIndex >= _animation.ColorTrack.KeyCount)
				_keyIndex = 0;
			_editorOwner.SetTime(_animation.ColorTrack.GetKeyAtIndex(_keyIndex).Time);
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
				_animation.ColorTrack.Remove(CurrentTime);
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
				_animation.ColorTrack.ClearKeys();
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Paint event of the pictureColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void pictureColor_Paint(object sender, PaintEventArgs e)
		{
			SolidBrush brush = new SolidBrush(_currentColor);			
			e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
			e.Graphics.FillRectangle(brush, pictureColor.ClientRectangle);
			brush.Dispose();
		}

		/// <summary>
		/// Handles the DoubleClick event of the pictureColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void pictureColor_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				_colorPicker = new ColorPicker();
				_colorPicker.Color = _currentColor;
				_colorPicker.UseAlpha = true;

				if (_colorPicker.ShowDialog(_editorOwner) == DialogResult.OK)
				{
					_currentColor = _colorPicker.Color;
					UpdateColorLabel();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(ParentForm, "Error getting color value.", ex);
			}
			finally
			{
				if (_colorPicker != null)
					_colorPicker.Dispose();

				_colorPicker = null;
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericA control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void colorComponent_ValueChanged(object sender, EventArgs e)
		{
			if (!_noUpdateEvent)
			{
				_currentColor = Color.FromArgb((int)numericA.Value, (int)numericR.Value, (int)numericG.Value, (int)numericB.Value);
				pictureColor.Refresh();
			}
		}
	}
}
