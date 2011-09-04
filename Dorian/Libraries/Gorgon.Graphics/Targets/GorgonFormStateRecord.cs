#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, September 03, 2011 6:14:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Records the form state.
	/// </summary>
	public class FormStateRecord
	{
		#region Variables.
		private Point _location;					// Window location.
		private Size _size;							// Window size.
		private FormBorderStyle _border;			// Window border.
		private bool _topMost;						// Topmost flag.
		private bool _sysMenu;						// System menu flag.
		private bool _maximizeButton;				// Maxmimize button flag.
		private bool _minimizeButton;				// Minimize button flag.
		private bool _visible;						// Visible flag.
		private bool _enabled;						// Enabled flag.
		private Form _window;						// Window.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to restore the original window state.
		/// </summary>
		/// <param name="keepSize">TRUE to keep the size of the window, FALSE to restore it.</param>
		/// <param name="dontMove">TRUE to keep the window from moving, FALSE to restore the last location.</param>
		public void Restore(bool keepSize, bool dontMove)
		{
			if (_window == null)
				return;

			if (!dontMove)
				_window.DesktopLocation = _location;
			if (!keepSize)
				_window.Size = _size;
			_window.FormBorderStyle = _border;
			_window.TopMost = _topMost;
			_window.ControlBox = _sysMenu;
			_window.MaximizeBox = _maximizeButton;
			_window.MinimizeBox = _minimizeButton;
			_window.Enabled = _enabled;
			_window.Visible = _visible;
		}

		/// <summary>
		/// Function to update the form state.
		/// </summary>
		public void Update()
		{
			_location = _window.DesktopLocation;
			_size = _window.Size;
			_border = _window.FormBorderStyle;
			_topMost = _window.TopMost;
			_sysMenu = _window.ControlBox;
			_maximizeButton = _window.MaximizeBox;
			_minimizeButton = _window.MinimizeBox;
			_enabled = _window.Enabled;
			_visible = _window.Visible;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormStateRecord"/> struct.
		/// </summary>
		/// <param name="window">The window.</param>
		internal FormStateRecord(Form window)
		{
			_location = window.DesktopLocation;
			_size = window.Size;
			_border = window.FormBorderStyle;
			_topMost = window.TopMost;
			_sysMenu = window.ControlBox;
			_maximizeButton = window.MaximizeBox;
			_minimizeButton = window.MinimizeBox;
			_enabled = window.Enabled;
			_visible = window.Visible;
			_window = window;
		}
		#endregion
	}
}
