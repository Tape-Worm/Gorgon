#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:30:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Configuration;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Configuration settings for the editor.
	/// </summary>
	public class GorgonEditorSettings
		: GorgonApplicationSettings
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the main form state.
		/// </summary>
		[GorgonApplicationSetting("FormState", FormWindowState.Maximized, typeof(FormWindowState), "MainApplication")]
		public FormWindowState FormState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		[GorgonApplicationSetting("WindowDimensions", typeof(Rectangle), "MainApplication")]
		public Rectangle WindowDimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default size type.
		/// </summary>
		[GorgonApplicationSetting("FontSizeType", Graphics.FontHeightMode.Points, typeof(Graphics.FontHeightMode), "FontEditor")]
		public Graphics.FontHeightMode FontSizeType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font anti-alias mode.
		/// </summary>
		[GorgonApplicationSetting("FontAntiAliasMode", Graphics.FontAntiAliasMode.AntiAliasHQ, typeof(Graphics.FontAntiAliasMode), "FontEditor")]
		public Graphics.FontAntiAliasMode FontAntiAliasMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font texture size.
		/// </summary>
		[GorgonApplicationSetting("FontTextureSize", typeof(Size), "FontEditor")]
		public Size FontTextureSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the initial path for projects.
		/// </summary>
		[GorgonApplicationSetting("InitialPath", typeof(string), "")]
		public string InitialPath
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorSettings"/> class.
		/// </summary>
		internal GorgonEditorSettings()
			: base("Gorgon Editor", new Version(1, 0))
		{
			Size baseSize = new Size(1280, 800);

			FormState = FormWindowState.Maximized;
			FontTextureSize = new Size(256, 256);

			// Set the default size, but ensure that it fits within the primary monitor.
			// Do not go larger than 1280x800 by default.
			if (Screen.PrimaryScreen.WorkingArea.Width < 1280)
				baseSize.Width = Screen.PrimaryScreen.WorkingArea.Width;
			if (Screen.PrimaryScreen.WorkingArea.Height < 800)
				baseSize.Height = Screen.PrimaryScreen.WorkingArea.Height;
			
			WindowDimensions = new Rectangle(Screen.PrimaryScreen.WorkingArea.Width / 2 - baseSize.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - baseSize.Height / 2, 1280, 800);
			InitialPath = GorgonComputerInfo.FolderPath(Environment.SpecialFolder.MyDocuments) + @"\Gorgon\Projects\";
		}
		#endregion
	}
}
