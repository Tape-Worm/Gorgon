#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, February 10, 2015 11:14:19 PM
// 
#endregion

using System;
using System.Windows.Forms;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Primary application window.
	/// </summary>
	public partial class FormMain 
		: FlatForm
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply the theme to the primary window.
		/// </summary>
		private void SetupTheme()
		{
			/*// Set up our forms renderer.
			if (!(ToolStripManager.Renderer is GorgonEditorRenderer))
			{
				ToolStripManager.Renderer = new GorgonEditorRenderer();
			}*/

			/*this.BackColor = GorgonEditorRenderer.WindowBackground;
			this.ForeColor = GorgonEditorRenderer.WindowCaptionForeColor;
			this.BorderColor = GorgonEditorRenderer.WindowActiveBorderColor;
			this.InactiveBorderColor = GorgonEditorRenderer.WindowInactiveBorderColor;
			this.WindowIconHilightColor = GorgonEditorRenderer.WindowIconHilightColor;*/
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetupTheme();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		public FormMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}
