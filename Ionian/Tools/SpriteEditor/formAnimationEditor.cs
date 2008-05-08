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
// Created: Tuesday, July 10, 2007 11:59:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for the animation editor.
	/// </summary>
	public partial class formAnimationEditor 
		: Form
	{
		#region Variables.
		private SpriteDocumentList _documents = null;					// Sprite document list.
		private Animation _animation = null;							// Current animation.
		private bool _noevent = false;									// Flag to disable events.
		private AnimationDropIn _dropIn = null;							// Animation drop in panel.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the total number of frames for this track.
		/// </summary>
		public Decimal MaxFrames
		{
			get
			{
				return trackTrack.Maximum + 1;
			}
		}

		/// <summary>
		/// Property to set or return the currently loaded sprites.
		/// </summary>
		public SpriteDocumentList Sprites
		{
			get
			{
				return _documents;
			}
			set
			{
				_documents = value;
			}
		}

		/// <summary>
		/// Property to set or return the current animation.
		/// </summary>
		public Animation CurrentAnimation
		{
			get
			{
				return _animation;
			}
			set
			{
				Renderable animationOwner = null;

				if (value != null)
				{
					_animation = value;
					comboTrack.Items.Clear();
					animationOwner = _animation.Owner as Renderable;


					if (animationOwner != null)
					{
						foreach (Animation animation in animationOwner.Animations)
						{
							animation.Reset();
							if (animation.Owner is Renderable)
								((Renderable)animation.Owner).ApplyAnimations();
						}
					}
					else
					{
						UI.ErrorBox(this, "The object is not a renderable type.  Cannot edit its animation.");
						return;
					}

					// Get tracks.
					FillTrackCombo();
					string currentTrack = string.Empty;		// Current track.
					int lastCount = 0;						// Last key count.

					var tracks = _animation.Tracks.Where((track) => track.KeyCount > lastCount);
					foreach (Track track in tracks)
					{
						currentTrack = track.Name;
						lastCount = track.KeyCount;
					}

					if (currentTrack == string.Empty)
						comboTrack.SelectedItem = "Image";
					else
						comboTrack.SelectedItem = currentTrack;

					Text = "Animation Editor - " + _animation.Name;
				}
				else
					UI.ErrorBox(this, "Cannot set the current animation to NULL.");

				TrackerUpdate();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all the tracks from the animation.
		/// </summary>
		private void FillTrackCombo()
		{
			comboTrack.Items.Clear();
			if (_animation.Tracks.Count > 0)
			{
				foreach (Track track in _animation.Tracks)
					comboTrack.Items.Add(track.Name);
			}
		}

		/// <summary>
		/// Handles the Scroll event of the trackTrack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void trackTrack_Scroll(object sender, EventArgs e)
		{
			if (_noevent)
				return;
			numericTrackFrames.Value = (decimal)trackTrack.Value;
			UpdateDropIn();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericTrackTime control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void numericTrackTime_ValueChanged(object sender, EventArgs e)
		{
			if (_noevent)
				return;
			trackTrack.Value = (int)numericTrackFrames.Value;
			UpdateDropIn();
		}

		/// <summary>
		/// Function to update the drop-in.
		/// </summary>
		private void UpdateDropIn()
		{
			AnimationDropIn dropIn;			// Drop in.

			if (splitTrack.Panel1.Controls.Count > 0)
			{
				dropIn = splitTrack.Panel1.Controls[0] as AnimationDropIn;
				if (dropIn != null)  
					dropIn.CurrentTime = ((float)numericTrackFrames.Value / _animation.FrameRate) * 1000.0f;
			}
		}

		/// <summary>
		/// Function to add a drop in control to the panel.
		/// </summary>
		/// <param name="dropIn">Animation drop-in to add.</param>
		/// <param name="track">Track that is bound to this editor.</param>
		private void AddDropIn(AnimationDropIn dropIn, Track track)
		{	
			// Remove the control.
			if (splitTrack.Panel1.Controls.Count > 0)
				splitTrack.Panel1.Controls[0].Dispose();
			dropIn.CurrentTrack = track;
			splitTrack.Panel1.Controls.Add(dropIn);
			dropIn.Dock = DockStyle.Fill;
			dropIn.GetSettings();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboTrack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboTrack_SelectedIndexChanged(object sender, EventArgs e)
		{
			Renderable animObject = _animation.Owner as Renderable;		// Animated object.

			try
			{
				if ((animObject != null) && (_animation.Tracks.Contains(comboTrack.Text)))
				{
					switch (_animation.Tracks[comboTrack.Text].DataType.FullName.ToLower())
					{
						case "gorgonlibrary.graphics.image":
							AddDropIn(new ImageDropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						case "system.byte":
							AddDropIn(new ByteDropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						case "system.int32":
							AddDropIn(new Int32DropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						case "system.single":
							AddDropIn(new FloatDropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						case "system.drawing.color":
							AddDropIn(new ColorDropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						case "gorgonlibrary.vector2d":
							AddDropIn(new Vector2DDropIn(), _animation.Tracks[comboTrack.Text]);
							break;
						default:
							UI.ErrorBox(this, "I have no idea how to handle the type '" + _animation.Tracks[comboTrack.Text].DataType.FullName + "'.");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error while trying to load track editor.", ex);
			}
		}

		/// <summary>
		/// Property to drop in the editor.
		/// </summary>
		/// <param name="editor">Editor to drop in.</param>
		private void DropInEditor(AnimationDropIn editor)
		{
			float lastTime = 0.0f;		// Last time.
			
			// Remove the current control
			if (splitTrack.Panel1.Controls.Count != 0)
			{
				lastTime = ((AnimationDropIn)splitTrack.Panel1.Controls[0]).CurrentTime;		// Get 'last' time.

				// Don't remove the editor.
				((AnimationDropIn)splitTrack.Panel1.Controls[0]).CleanUp();
				splitTrack.Panel1.Controls.RemoveAt(0);
			}

			// Drop a frame editor in.
			splitTrack.Panel1.Controls.Add((Control)editor);
			_dropIn = editor;
		}

		/// <summary>
		/// Function to update the display track.
		/// </summary>
		private void TrackerUpdate()
		{
			Decimal maxFrames = 0;						// Frame count.
			int frequency = 0;							// Tick frequency.
			Decimal increment = 0;						// Increment.
			Decimal currentTime = 0;					// Current time.

			maxFrames = ((Decimal)_animation.Length / 1000.0M) * (Decimal)_animation.FrameRate;
			currentTime = (numericTrackFrames.Value / (Decimal)_animation.FrameRate) * 1000.0M;

			frequency = (int)(maxFrames / 20.0M);
			increment = (Decimal)frequency;

			// Clamp the increment.
			if (increment < 1.0M)
				increment = 1.0M;

			// Update the ranges for the controls.
			trackTrack.Maximum = (int)maxFrames - 1;
			trackTrack.TickFrequency = frequency;
			numericTrackFrames.Maximum = maxFrames - 1;
			numericTrackFrames.Value = currentTime;
			numericTrackFrames.Increment = (Decimal)frequency;

			if (_dropIn != null)
				_dropIn.CurrentTime = (float)currentTime;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			GetSettings();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				// Ensure that the animation is in an enabled state upon exit.
				if (_animation != null)
					_animation.Enabled = true;

				Settings.Root = "AnimationEditor";
				Settings.SetSetting("WindowState", WindowState.ToString());				

				// Set window dimensions.
				if (WindowState != FormWindowState.Maximized)
				{
					Settings.SetSetting("Left", Left.ToString());
					Settings.SetSetting("Top", Top.ToString());
					Settings.SetSetting("Width", Width.ToString());
					Settings.SetSetting("Height", Height.ToString());
				}
				Settings.Root = null;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the animation window settings.",ex);
			}
		}

		/// <summary>
		/// Function to update the frame counter/tracker.
		/// </summary>
		/// <param name="time"></param>
		public void SetTime(float time)
		{
			Decimal frame = 0;			// Current frame.

			_noevent = true;

			frame = (Decimal)((time / 1000.0f) * _animation.FrameRate);
			if (frame > numericTrackFrames.Maximum)
				frame = numericTrackFrames.Maximum;
			trackTrack.Value = (int)frame;
			numericTrackFrames.Value = frame;			
			UpdateDropIn();

			_noevent = false;
		}

		/// <summary>
		/// Function to retrieve animation editor settings.
		/// </summary>
		public void GetSettings()
		{
			Point defaultLocation = Point.Empty;		// Default location.
			Settings.Root = "AnimationEditor";
			WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), Settings.GetSetting("WindowState", "Normal"));

			// Set window dimensions.
			if (WindowState == FormWindowState.Normal)
			{
				defaultLocation = formMain.Me.Location;
				defaultLocation.X = (defaultLocation.X / 2) - 350;
				defaultLocation.Y = (defaultLocation.Y / 2) - 240;
				Left = Convert.ToInt32(Settings.GetSetting("Left", defaultLocation.X.ToString()));
				Top = Convert.ToInt32(Settings.GetSetting("Top", defaultLocation.Y.ToString()));
				Width = Convert.ToInt32(Settings.GetSetting("Width", "700"));
				Height = Convert.ToInt32(Settings.GetSetting("Height", "480"));
			}
			Settings.Root = null;
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formAnimationEditor()
		{
			InitializeComponent();
		}
		#endregion
	}
}