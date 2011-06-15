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
using Dialogs;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

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
		private Sprite _exportMe = null;					// Sprite to export.
		private Image _spriteImage = null;					// Sprite image.
		private Font _font = null;							// Font object.
		private TextSprite _text = null;					// Text sprite.
		private SpriteExporter _pngExporter = null;			// Sprite PNG exporter.
		private SpriteExporter _bmpExporter = null;			// Sprite BMP exporter.
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

			// Draw to center of screen.
			_exportMe.SetPosition(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_exportMe.Draw();

			_text.Draw();
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		private void Initialize()
		{
			// Load the sprite image.
			_spriteImage = Image.FromFile(@"..\..\..\..\Resources\Images\Nebula.png");

			// Create sprite.
			_exportMe = new Sprite("ExportSprite", _spriteImage, new Vector2D(175, 164));
			_exportMe.SetAxis(_exportMe.Width / 2.0f, _exportMe.Height / 2.0f);			

			// Get the plug-ins.
			_pngExporter = SpriteExporter.LoadPlugIn(@".\PlugInODoomPNG.DLL");
			_bmpExporter = SpriteExporter.LoadPlugIn(@".\PlugInODoomBMP.DLL");

			// Create font and text sprite.
			_font = new Font("Arial12pt", "Arial", 12.0f);
			_text = new TextSprite("TextSprite", "Hit ESC to save the image and close the example.", _font, Drawing.Color.Black);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
			{
				if (dialogSaveSprite.ShowDialog(this) == DialogResult.OK)
				{
					if (dialogSaveSprite.FilterIndex == 2)
						_bmpExporter.Export(_exportMe, dialogSaveSprite.FileName);
					else
						_pngExporter.Export(_exportMe, dialogSaveSprite.FileName);
				}

				Close();
			}

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