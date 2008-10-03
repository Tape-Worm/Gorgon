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
// Created: TOBEREPLACED
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
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Main form.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Variables.
        private Random _rnd = new Random();			// Our handy dandy random number generator.
        private IRenderer _renderer = null;         // Renderer interface.
        private RenderImage _target = null;         // Render target.
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
            Gorgon.Screen.Clear(Drawing.Color.White);

            Gorgon.CurrentRenderTarget = _target;
            _target.Clear(Drawing.Color.Transparent);
            _target.BlendingMode = Gorgon.GlobalStateSettings.GlobalBlending;
            _renderer.Begin();
            _renderer.Render(e.FrameDeltaTime);
            _renderer.End();

            Gorgon.CurrentRenderTarget = null;

			_target.BlendingMode = BlendingModes.None;
            _target.Smoothing = Gorgon.GlobalStateSettings.GlobalSmoothing;
            _target.AlphaMaskFunction = CompareFunctions.GreaterThanOrEqual;
            _target.AlphaMaskValue = 1;
            _target.Blit(32, Gorgon.Screen.Height - 70, 64, 64, Drawing.Color.Red, BlitterSizeMode.Scale);
            _target.Blit(32, Gorgon.Screen.Height - 134, 128, 128, Drawing.Color.Green, BlitterSizeMode.Scale);
            _target.Blit(32, Gorgon.Screen.Height - 262, 256, 256, Drawing.Color.Blue, BlitterSizeMode.Scale);

			Gorgon.CurrentRenderTarget.BlendingMode = BlendingModes.Modulated;
			_renderer.Begin();
			_renderer.Render(e.FrameDeltaTime);
			_renderer.End();
		}

        /// <summary>
        /// Function to provide initialization for our example.
        /// </summary>
        private void Initialize()
        {
            // Set smoothing mode to all the sprites.
            Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

            if (_target != null)
                _target.Dispose();
            _target = new RenderImage("MyTarget", 320, 240, ImageBufferFormats.BufferRGB888A8);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
                Close();

            if (e.KeyCode == Keys.F)
            {
                if (Gorgon.GlobalStateSettings.GlobalSmoothing == Smoothing.None)
                    Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;
                else
                    Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.None;
            }

            if (e.KeyCode == Keys.S)
                Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (_renderer != null)
                _renderer.Dispose();

            // Perform clean up.
            Gorgon.Terminate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
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
                ClientSize = new Drawing.Size(640, 400);
                Gorgon.SetMode(this);

                // Set an ugly background color.
                Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(0, 0, 16);

                // Initialize the app.
                Initialize();
                
                // Create our renderer.
                // Load our plug-in.
                _renderer = RendererPlugIn.LoadRenderer(@".\Renderer.dll");
                _renderer.Initialize();

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
