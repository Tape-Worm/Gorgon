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
// Created: Sunday, March 8, 2015 11:19:53 PM
// 
#endregion

using System.ComponentModel;
using System.Drawing;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Custom extensions to the flat form.
	/// </summary>
	public class EditorTheme
		: GorgonFlatFormTheme
	{
		#region Variables.
		// The background color of the property panel.
		private Color _propertyPanelBackgroundColor;
		// The background color of the property panel.
		private Color _brokenDependencyLinkForegroundColor;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the property panel background color.
		/// </summary>
		[Browsable(true)]
		public Color PropertyPanelBackgroundColor
		{
			get
			{
				return _propertyPanelBackgroundColor;
			}
			set
			{
				_propertyPanelBackgroundColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the broken dependency link foreground color.
		/// </summary>
		[Browsable(true)]
		public Color BrokenDependencyLinkForeColor
		{
			get
			{
				return _brokenDependencyLinkForegroundColor;
			}
			set
			{
				_brokenDependencyLinkForegroundColor = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorTheme"/> class.
		/// </summary>
		public EditorTheme()
		{
			_propertyPanelBackgroundColor = Color.FromArgb(58, 58, 58);
			_brokenDependencyLinkForegroundColor = Color.Red;
		}
		#endregion
	}
}
