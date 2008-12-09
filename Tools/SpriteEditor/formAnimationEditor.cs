#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
		private List<Animation> _playAnimations = null;					// List of animations to play.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the background image size.
		/// </summary>
		public Vector2D ImageSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the image location offset.
		/// </summary>
		public Vector2D ImageLocation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the image to use as a background.
		/// </summary>
		public Image ImageBackground
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the current drop-in.
		/// </summary>
		public AnimationDropIn DropIn
		{
			get
			{
				if (splitTrack.Panel1.Controls.Count < 1)
					return null;

				return splitTrack.Panel1.Controls[0] as AnimationDropIn;
			}
		}

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
		/// Property to return the current frame.
		/// </summary>
		public int CurrentFrame
		{
			get
			{
				return (int)numericTrackFrames.Value;
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
				UpdateTrackView();
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

			if ((!splitTrackView.Panel2Collapsed) && (DropIn != null))
			{
				TrackKeyDisplay display = panelTrackDisplay.Controls[DropIn.CurrentTrack.Name] as TrackKeyDisplay;
				if (display != null)
					display.Invalidate(true);
			}

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
			string lastSetting = string.Empty;		// Last track name.

			// Remove the control.
			if (splitTrack.Panel1.Controls.Count > 0)
				splitTrack.Panel1.Controls[0].Dispose();

			dropIn.PlayAnimations = _playAnimations;

			comboTrack.SelectedIndexChanged -= new EventHandler(comboTrack_SelectedIndexChanged);
			lastSetting = comboTrack.Text;
			FillTrackCombo();
			comboTrack.Text = lastSetting;
			if (comboTrack.Text == string.Empty)
				comboTrack.Text = "Image";
			comboTrack.SelectedIndexChanged += new EventHandler(comboTrack_SelectedIndexChanged);

			dropIn.CurrentTrack = track;
			splitTrack.Panel1.Controls.Add(dropIn);
			dropIn.Dock = DockStyle.Fill;
			dropIn.GetSettings();

			dropIn.ShowTrackKeys = !splitTrackView.Panel2Collapsed;
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
								
				UpdateTrackView();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error while trying to load track editor.", ex);
			}
		}

		/// <summary>
		/// Function to update the display track.
		/// </summary>
		private void TrackerUpdate()
		{
			Decimal maxFrames = 0;						// Frame count.
			int frequency = 0;							// Tick frequency.
			Decimal currentTime = 0;					// Current time.

			maxFrames = ((Decimal)_animation.Length / 1000.0M) * (Decimal)_animation.FrameRate;
			currentTime = (numericTrackFrames.Value / (Decimal)_animation.FrameRate) * 1000.0M;

			if (maxFrames >= 60)
				frequency = (int)(maxFrames / 20.0M);
			else
				frequency = 1;

			// Update the ranges for the controls.
			trackTrack.Maximum = (int)maxFrames - 1;
			trackTrack.TickFrequency = frequency;
			numericTrackFrames.Maximum = maxFrames - 1;
			numericTrackFrames.Value = currentTime;
			numericTrackFrames.Increment = (Decimal)frequency;
		}

		/// <summary>
		/// Handles the Click event of the checkShowAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkShowAll_Click(object sender, EventArgs e)
		{
			UpdateTrackView();
			Settings.Root = "AnimationEditor";
			Settings.SetSetting("ShowAllTracks", checkShowAll.Checked.ToString());
			Settings.Root = null;		
		}

		/// <summary>
		/// Function to update the track view.
		/// </summary>
		private void UpdateTrackView()
		{
			int height = 0;
			int maxWidth = panelTrackDisplay.Width - 2;
			int maxFrames = 0;
			int rows = 1;

			base.Refresh();

			if (_animation == null)
				return;

			try
			{
				if (_animation.Tracks.Count > 0)
				{

					bool hasImageKeys = _animation.Tracks["Image"].KeyCount > 0;

					panelTrackNames.ResetScroll();
					Point lastOffset = panelTrackDisplay.AutoScrollPosition;
					panelTrackDisplay.AutoScroll = false;
					panelTrackDisplay.AutoScrollMinSize = new Size(3, 3);
					panelTrackDisplay.AutoScrollOffset = new Point(0, 0);

					maxFrames = (int)((_animation.Length / 1000.0f) * _animation.FrameRate);
					if (maxFrames > 1024)
					{
						while ((maxFrames % 1024) != 0)
							maxFrames++;
					}
					rows = maxFrames / TrackKeyDisplay.MaxFramesPerRow;
					if (rows < 1)
						rows = 1;

					foreach (Track track in _animation.Tracks)
					{
						TrackKeyDisplay display = null;
						TrackNameButton button = null;

						if (!panelTrackDisplay.Controls.ContainsKey(track.Name))
						{
							display = new TrackKeyDisplay();
							display.Name = track.Name;
							panelTrackDisplay.Controls.Add(display);
							display.Location = new Point(0, height);
							display.Track = track;
							display.OwnerForm = this;
							display.MouseDown += new MouseEventHandler(display_MouseDown);
							display.Height = rows * 32;

							button = new TrackNameButton();
							button.Name = track.Name;
							panelTrackNames.Controls.Add(button);
							button.TrackName = track.Name;
							if (DropIn.CurrentTrack == track)
								button.Selected = true;
							button.Location = new Point(0, height + 2);
							button.MouseDown += new MouseEventHandler(button_MouseDown);
						}
						else
						{
							display = panelTrackDisplay.Controls[track.Name] as TrackKeyDisplay;
							display.Location = new Point(-panelTrackDisplay.AutoScrollOffset.X, height);
							display.Height = rows * 32;
							display.Refresh();

							button = panelTrackNames.Controls[track.Name] as TrackNameButton;
							button.Location = new Point(-panelTrackDisplay.AutoScrollOffset.X, height + 2);
							button.Refresh();
						}

						if ((track.KeyCount > 0) || (checkShowAll.Checked))
						{
							button.Visible = true;
							display.Visible = true;
							if (maxFrames > 1024)
								display.Width = 1024 * 32 + 8;
							else
								display.Width = maxFrames * 32 + 8;
							//display.Width - button.Width;
							maxWidth = MathUtility.Min(maxWidth, display.Width);
							height += display.Height;
						}
						else
						{
							display.Visible = false;
							button.Visible = false;
						}
					}

					panelTrackDisplay.AutoScroll = true;					
					panelTrackDisplay.AutoScrollMinSize = new Size(maxWidth, height);
					panelTrackDisplay.AutoScrollPosition = new Point(-lastOffset.X, -lastOffset.Y);
					panelTrackDisplay_Scroll(this, new ScrollEventArgs(ScrollEventType.First, 0));
				}
			}
			catch (Exception ex)
			{
				Dialogs.UI.ErrorBox(this, "Error refreshing track listing.", ex);
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the button control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void button_MouseDown(object sender, MouseEventArgs e)
		{
			TrackNameButton button = sender as TrackNameButton;

			if (button != null)
			{
				if (string.Compare(button.Name, comboTrack.Text, true) != 0)
					comboTrack.Text = button.Name;

				if (panelTrackNames.Controls.ContainsKey(comboTrack.Text))
				{
					button.Selected = true;
					foreach (TrackNameButton otherButtons in panelTrackNames.Controls)
					{
						if (otherButtons != button)
							otherButtons.Selected = false;							
					}
					panelTrackNames.ResetScroll();
					panelTrackDisplay_Scroll(this, new ScrollEventArgs(ScrollEventType.First, 0));
				}
			}			
		}

		/// <summary>
		/// Handles the Scroll event of the panelTrackDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
		private void panelTrackDisplay_Scroll(object sender, ScrollEventArgs e)
		{
			panelTrackDisplay.Invalidate(true);
			panelTrackNames.ScrollControls(new Point(0, -panelTrackDisplay.AutoScrollPosition.Y));
		}

		/// <summary>
		/// Handles the Resize event of the panelTrackNames control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void panelTrackNames_Resize(object sender, EventArgs e)
		{
			panelTrackNames.ScrollControls(new Point(0, -panelTrackDisplay.AutoScrollPosition.Y));
		}

		/// <summary>
		/// Handles the MouseDown event of the display control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void display_MouseDown(object sender, MouseEventArgs e)
		{
			TrackKeyDisplay display = sender as TrackKeyDisplay;

			if (display != null)
			{
				if (string.Compare(display.Name, comboTrack.Text, true) != 0)
					comboTrack.Text = display.Name;

				if (panelTrackNames.Controls.ContainsKey(comboTrack.Text))
				{
					foreach (TrackNameButton button in panelTrackNames.Controls)
					{
						if (button.TrackName != comboTrack.Text)
							button.Selected = false;
						else
							button.Selected = true;
					}
					panelTrackNames.ResetScroll();
					panelTrackDisplay_Scroll(this, new ScrollEventArgs(ScrollEventType.First, 0));
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			Sprite owner = _animation.Owner as Sprite;

			base.OnLoad(e);
			GetSettings();

			// Stop all animations except this one.
			foreach (Animation anim in owner.Animations)
			{
				if (_animation != anim)
				{
					anim.Reset();
					anim.AnimationState = AnimationState.Stopped;
				}
			}
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
		/// Function to show the track/key viewer.
		/// </summary>
		public void ShowViewer()
		{
			splitTrackView.Panel2Collapsed = !splitTrackView.Panel2Collapsed;
			Settings.Root = "AnimationEditor";
			Settings.SetSetting("TrackViewCollapsed", splitTrackView.Panel2Collapsed.ToString());
			Settings.Root = null;
		}

		/// <summary>
		/// Function to refresh the track viewer.
		/// </summary>
		public void RefreshTrackView()
		{
			UpdateTrackView();
		}

		/// <summary>
		/// Function to update the frame counter/tracker.
		/// </summary>
		/// <param name="time"></param>
		public void SetTime(float time)
		{
			Decimal frame = 0;			// Current frame.

			_noevent = true;

			frame = MathUtility.RoundInt((time / 1000.0f) * _animation.FrameRate);
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

			splitTrackView.Panel2Collapsed = Convert.ToBoolean(Settings.GetSetting("TrackViewCollapsed", "false"));
			checkShowAll.Checked = Convert.ToBoolean(Settings.GetSetting("ShowAllTracks", "true"));
			
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

			_playAnimations = new List<Animation>();
		}
		#endregion
	}
}