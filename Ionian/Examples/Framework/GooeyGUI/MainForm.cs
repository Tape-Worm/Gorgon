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
// Created: Wednesday, October 01, 2008 9:56:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.GUI;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Variables.
		private Random _rnd = new Random();					// Random number generator.
		private GUISkin _skin = null;						// GUI skin.
		private Desktop _desktop = null;					// The root container for the GUI.
		private GUIWindow _window = null;					// A window in which to display our stuff.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			_desktop.Draw();
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();
		}

		/// <summary>
		/// Function called before Gorgon is shut down.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		/// <remarks>Users should override this function to perform clean up when the application closes.</remarks>
		protected override bool OnGorgonShutDown()
		{
			if (_desktop != null)
				_desktop.Dispose();

			if (_skin != null)
				_skin.Dispose();

			_desktop = null;
			_skin = null;

			return true;
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			GUIButton button;

			_skin = GUISkin.FromFile(FileSystems["GUISkin"]);
			_desktop = new Desktop(Input, _skin);
			_window = new GUIWindow("Window", Gorgon.Screen.Width / 4, Gorgon.Screen.Height / 4, Gorgon.Screen.Width - (Gorgon.Screen.Width / 4) * 2, Gorgon.Screen.Height - (Gorgon.Screen.Height / 4) * 2);
			button = new GUIButton("CloseButton");

			button.Owner = _window;
			button.Position = new Drawing.Point(_window.ClientArea.Width / 4, _window.ClientArea.Height - 40);
			button.Size = new Drawing.Size(_window.ClientArea.Width - (_window.ClientArea.Width / 4) * 2, 20);
			button.Text = "Click to close.";
			button.TextAlignment = Alignment.Center;

			_desktop.Windows.Add(_window);
			_desktop.ShowDesktopBackground = true;
			_desktop.BackgroundColor = Drawing.Color.Tan;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\GooeyGUI.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}