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
// Created: Saturday, April 19, 2008 12:48:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Tools;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing a color picker.
	/// </summary>
	/// <remarks>This control uses the color picker created by Ken Getz.  
	/// You can check out the article here: http://msdn.microsoft.com/msdnmag/issues/03/07/GDIColorPicker/default.aspx
	/// As such, the documentation/formatting/commenting of the code is entirely different than the rest of the library.
	/// </remarks>
	public class ColorPicker
		: IDisposable 
	{
		#region Variables.
		private ColorChooser _colorChooser = null;		// Color chooser object.
		private Icon _dialogIcon = null;				// Icon for the dialog.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the color for the picker.
		/// </summary>
		public Color Color
		{
			get
			{
				return _colorChooser.Color;
			}
			set
			{
				_colorChooser.Color = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to use alpha blending or not.
		/// </summary>
		public bool UseAlpha
		{
			get
			{
				return _colorChooser.UseAlpha;
			}
			set
			{
				_colorChooser.UseAlpha = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to show the window as a dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <returns>A dialog result.</returns>
		public DialogResult ShowDialog(Form owner)
		{
			if (_dialogIcon != null)
				_dialogIcon.Dispose();
			_dialogIcon = null;

			// Copy the icon.
			if (owner != null)
			{
				_dialogIcon = new Icon(owner.Icon,owner.Icon.Width,owner.Icon.Height);
				_colorChooser.Icon = _dialogIcon;
			}
			else
				_colorChooser.Icon = null;
			
			return _colorChooser.ShowDialog(owner);
		}		

		/// <summary>
		/// Function to show the window as a dialog.
		/// </summary>
		/// <returns>A dialog result.</returns>
		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ColorPicker()
		{
			// Create the color chooser.
			_colorChooser = new ColorChooser();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~ColorPicker()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_colorChooser != null)
					_colorChooser.Dispose();
				if (_dialogIcon != null)
					_dialogIcon.Dispose();
			}

			// Do unmanaged clean up.
			_colorChooser = null;
			_dialogIcon = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
