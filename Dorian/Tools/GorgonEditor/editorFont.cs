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
// Created: Thursday, May 03, 2012 9:18:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font editor.
	/// </summary>
	public partial class editorFont 
		: UserControl
	{
		#region Variables.
		private GorgonSwapChain _swapChain = null;
		private Gorgon2D _2D = null;
		#endregion

		#region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the renderer for the control.
		/// </summary>
		public void InitializeRendering()
		{

		}

		/// <summary>
		/// Function to end rendering.
		/// </summary>
		public void TerminateRendering()
		{

		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="editorFont"/> class.
		/// </summary>
		public editorFont()
		{
			InitializeComponent();
		}
		#endregion
	}
}
