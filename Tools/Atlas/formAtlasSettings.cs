#region MIT.
// 
// Atlas.
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
// Created: Tuesday, November 13, 2007 1:33:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Atlas
{
	/// <summary>
	/// Interface to set the atlas settings.
	/// </summary>
	public partial class formAtlasSettings : Form
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the pixel padding around images.
		/// </summary>
		public int PixelPadding
		{
			get
			{
				return (int)numericPadding.Value;
			}
			set
			{
				numericPadding.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the atlas width.
		/// </summary>
		public int AtlasWidth
		{
			get
			{
				return (int)numericWidth.Value;
			}
			set
			{
				numericWidth.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the height of the atlas.
		/// </summary>
		public int AtlasHeight
		{
			get
			{
				return (int)numericHeight.Value;
			}
			set
			{
				numericHeight.Value = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AtlasSettings"/> class.
		/// </summary>
		public formAtlasSettings()
		{
			InitializeComponent();
		}
		#endregion
	}
}