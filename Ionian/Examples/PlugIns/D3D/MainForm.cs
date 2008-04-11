#region LGPL.
// 
// Examples.
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
// Created: Monday, April 07, 2008 8:12:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
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

			_target.Smoothing = Smoothing.Smooth;
			_target.BlendingMode = BlendingModes.None;
            _target.AlphaMaskFunction = CompareFunctions.GreaterThanOrEqual;
            _target.AlphaMaskValue = 1;
            _target.Blit(32, 32, 64, 64);
            _target.Blit(32, 32, 128, 128);
            _target.Blit(32, 32, 256, 256);

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
            Gorgon.InvertFrameStatsTextColor = false;

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
