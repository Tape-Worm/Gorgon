#region MIT.
// 
// Examples.
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using Dialogs;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private Random _rnd = new Random();					// Our handy dandy random number generator.
		private Font _6809Font = null;						// 6809 Font.
		private Font _arialFont = null;						// Arial font.
		private Font _betsyFont = null;						// Betsy font.
		private Font _wetPetFont = null;					// Wet pet font.
		private Font _courierFont = null;					// Courier font.
		private Font _papyFont = null;						// Papyrus font.
		private TextSprite _text = null;					// Text sprite.
		private TextSprite _scrollText = null;				// Scrolling text sprite.
		private Vector2D _scrollPosition;					// Scroller position.
		private Vector2D _scrollScale;						// Scroller scale.
		private float _clippedTextPos;						// Clipped text position.
		private bool _dir;									// Clipped text scroll direction.
		private float _rotator;								// Rotation angle.
		private Viewport _clippedView;						// Clipped text viewport.
		private Viewport _wrapView;							// Wrapped text viewport.
		private PreciseTimer _timer = null;					// Timer.
		private List<Font> _fontList = null;				// List of fonts.
		private int _fontIndex = 0;							// Index of current font.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			// Do nothing here.  When we need to update, we will.
			Gorgon.Screen.Clear();

			// Draw Arial font.
			_text.Bounds = null;
			_text.SetPosition(2, 2);
			_text.Font = _arialFont;
			_text.Color = Drawing.Color.Blue;
			_text.Text = "Arial 9pt. font.";			
			_text.Rotation = 0.0f;
			_text.Draw();

			// Draw wet pet font.
			_text.SetPosition(24, 82);
			_text.Font = _wetPetFont;
			_text.Color = Drawing.Color.Purple;
			_text.Text = "Wet pet 18pt. font.";
			_text.Rotation = 6.2f;
			_text.Draw();

			// Draw betsy.
			_text.SetPosition(89, 162);
			_text.Font = _betsyFont;
			_text.Color = Drawing.Color.Black;
			_text.Text = "Betsy 36pt. font.";
			_text.Rotation = 0.0f;
			_text.Draw();

			// Draw 6809 font.
			_text.SetPosition(220, 290);
			_text.Font = _6809Font;
			_text.Shadowed = true;
			_text.Color = Drawing.Color.DarkCyan;
			_text.Text = "6809 14pt. font w/shadowing.";
			_text.Rotation = _rotator;
			_text.Draw();
			_text.Shadowed = false;

			// Draw alphabet.
			_text.SetPosition(0, 210);
			_text.Font = _fontList[_fontIndex];

			if (_timer.Seconds > 10.0)
			{
				_fontIndex++;

				if (_fontIndex >= _fontList.Count)
					_fontIndex = 0;
				_timer.Reset();
			}
			
			_text.Color = Drawing.Color.Black;
			_text.Text = "~`!@#$%^&*()_+-={}[]|\\:\";'<>,.?/\n1234567890\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstuvwxyz\n" + _timer.Seconds.ToString("0.0") + " seconds.";
			_text.Rotation = 0.0f;
			_text.Draw();

			// Draw some arbitrary word-wrapped text.
			_text.Rotation = 0.0f;
			_text.SetPosition(300, 1);
			_text.Font = _courierFont;
			_text.Color = Drawing.Color.White;
			_text.Bounds = _wrapView;
			_text.WordWrap = true;
			_text.Text = "We can do stuff with text.  Like making this line that's really long turn into a word-wrapped line, which is limited by this purple border.";
			_text.Draw();

			// Draw border around wrapped area.
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.Rectangle(_wrapView.Left, _wrapView.Top, _wrapView.Width, _text.Height, Drawing.Color.Purple);
			Gorgon.Screen.EndDrawing();

			// Draw a background rectangle for the clipped view.
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.FilledRectangle(_clippedView.Left - 3.0f, _clippedView.Top - 3.0f, _clippedView.Width + 6.0f, 26, Drawing.Color.Blue);
			Gorgon.Screen.Rectangle(_clippedView.Left - 2.0f, _clippedView.Top - 2.0f, _clippedView.Width + 4.0f, 24, Drawing.Color.LightBlue);
			Gorgon.Screen.EndDrawing();

			Gorgon.CurrentClippingViewport = _clippedView;
			// Draw some arbitrary word-wrapped text.
			_text.SetPosition((int)_clippedTextPos, 370);
			_text.Color = Drawing.Color.Violet;
			_text.Bounds = null;
			_text.WordWrap = false;
			_text.Text = "This line of text will be clipped and we won't be able to see all of the text.";
			_text.Draw();
			Gorgon.CurrentClippingViewport = null;

			// Bounce the clipped message.
			if (!_dir)
				_clippedTextPos -= 50.0f * e.FrameDeltaTime;
			else
				_clippedTextPos += 50.0f * e.FrameDeltaTime;

			if ((_clippedTextPos <= 250.0f - (_text.Width - 300.0f)) || (_clippedTextPos >= 250.0f))
				_dir = !_dir;

			// Draw scroller text.
			_scrollText.Text = "X: " + _scrollText.Position.X.ToString("0.0") + "\nY: " + _scrollText.Position.Y.ToString("0.0");
			_scrollText.SetPosition((int)_scrollPosition.X, (int)_scrollPosition.Y);

			_scrollPosition.X += ((_scrollText.Scale.X * 35.0f) * e.FrameDeltaTime);

			// Resize the scroller.
			if (_scrollText.Position.X > (Gorgon.Screen.Width + 150))
			{
				_scrollScale.X = 64 / _scrollText.Width;
				_scrollScale.Y = (64 / _scrollText.Height) * (_scrollText.Height / _scrollText.Width);
                _scrollText.Scale = Vector2D.Multiply(_scrollScale, _rnd.Next(15) + 2.0f);
				_scrollText.Color = Drawing.Color.FromArgb(_rnd.Next(64, 255), _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
				_scrollPosition.Y = ((float)_rnd.NextDouble() * Gorgon.Screen.Height) - _scrollText.ScaledDimensions.Y;
				_scrollPosition.X = -_scrollText.Scale.X * _scrollText.Width;
			}
			
			_scrollText.Draw();

			// Adjust rotation.
			_rotator += (10.0f * e.FrameDeltaTime);
			if (_rotator > 360.0f)
				_rotator = 0.0f;
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		private void Initialize()
		{
			_timer = new PreciseTimer();
			_fontList = new List<Font>();

			// Smooth text.
			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

			// Load fonts.
			_6809Font = GorgonLibrary.Graphics.Font.FromFile(@"..\..\..\..\Resources\Fonts\FontsOPlenty\6809CHAR.TTF", 14.0f, true);
			_arialFont = new Font("Arial9pt", "Arial", 9.0f, true, true);
			_betsyFont = GorgonLibrary.Graphics.Font.FromFile(@"..\..\..\..\Resources\Fonts\FontsOPlenty\Betsy.TTF", 36.0f, true);
			_wetPetFont = GorgonLibrary.Graphics.Font.FromResource("WetPet", Properties.Resources.ResourceManager, 18.0f, true);
			_papyFont = new Font("Papyrus14pt", "Papyrus", 14.0f, true, true);
			_courierFont = new Font("CourierNew12pt", "Courier New", 12, true, true);
			_courierFont.OutlineWidth = 1.0f;

			// Build font list.
			_fontList.Add(_papyFont);
			_fontList.Add(_6809Font);
			_fontList.Add(_arialFont);
			_fontList.Add(_courierFont);
			_fontList.Add(_wetPetFont);
			_fontList.Add(_betsyFont);

			// Create first font.
			_text = new TextSprite("TextSprite", string.Empty, _arialFont);
			_scrollText = new TextSprite("Scroller", string.Empty, _courierFont);
			_scrollText.Color = Drawing.Color.Yellow;
			_scrollScale = Vector2D.Unit;

			// Set up clipped and wrapped areas.
			_clippedTextPos = 250.0f;
			_clippedView = new Viewport(250, 370, 300, 400);
			_wrapView = new Viewport(300, 0, 256, 128);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				Close();

			if (e.KeyCode == Keys.S)
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Perform clean up.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{			
				// Initialize Gorgon
				// Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
				Gorgon.Initialize(false, true);

				// Display the logo.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;

				// Set the video mode.
				ClientSize = new Drawing.Size(640, 480);
				Gorgon.SetMode(this);

				// Set an ugly background color.
				Gorgon.Screen.BackgroundColor = Drawing.Color.Ivory;

				// Initialize.
				Initialize();

				// Assign idle event.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);

				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Begin rendering.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
			}
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			Initialize();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion
	}
}