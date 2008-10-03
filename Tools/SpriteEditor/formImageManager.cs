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
// Created: Tuesday, May 08, 2007 2:00:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GorgonLibrary.Graphics.Tools.Controls;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for image previewing.
	/// </summary>
	public partial class formImageManager
		: Form
	{
		#region Variables.
		private ImageManager _imageManager = null;			// Image manager.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the image manager interface.
		/// </summary>
		public ImageManager ImageManager
		{
			get
			{
				return _imageManager;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formImageManager()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Reference to form main.</param>
		public formImageManager(formMain owner)
		{
			InitializeComponent();

			// Get main form.
			_imageManager = new ImageManager();
			_imageManager.Dock = DockStyle.Fill;
			Controls.Add(_imageManager);
			_imageManager.BindToMainForm(owner, null);
			_imageManager.RefreshList();
		}
		#endregion
	}
}