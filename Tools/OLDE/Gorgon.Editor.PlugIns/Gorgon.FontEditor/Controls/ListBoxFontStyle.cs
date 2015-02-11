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
// Created: Sunday, May 06, 2012 11:15:54 PM
// 
#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Listbox for font styles.
	/// </summary>
	class ListBoxFontStyle
		: CheckedListBox
	{
		#region Variables.
		private StringFormat _format;					// String format.
		private bool _disposed;							// Flag to indicate that we've disposed the object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the editor service to use.
		/// </summary>
		public IWindowsFormsEditorService Service
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the font style.
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				var result = FontStyle.Regular;

			    if (CheckedItems.Count == 0)
			    {
			        return FontStyle.Regular;
			    }

			    foreach (var item in CheckedItems)
				{
					switch ((FontStyle)item)
					{
						case FontStyle.Bold:
							result |= FontStyle.Bold;
							break;
						case FontStyle.Italic:
							result |= FontStyle.Italic;
							break;
						case FontStyle.Underline:
							result |= FontStyle.Underline;
							break;
						case FontStyle.Strikeout:
							result |= FontStyle.Strikeout;
							break;
					}
				}

				return result;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				    if (_format != null)
				    {
				        _format.Dispose();
				    }
				}

				_format = null;
				_disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.CheckedListBox.ItemCheck"/> event.
		/// </summary>
		/// <param name="ice">An <see cref="T:System.Windows.Forms.ItemCheckEventArgs"/> that contains the event data.</param>
		protected override void OnItemCheck(ItemCheckEventArgs ice)
		{
			base.OnItemCheck(ice);

		    if (Items.Count < 2)
		    {
		        ice.NewValue = CheckState.Checked;
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ListBoxFontStyle"/> class.
		/// </summary>
		public ListBoxFontStyle()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListBoxFontStyle"/> class.
		/// </summary>
		/// <param name="fontFamily">Font family.</param>
		/// <param name="currentStyles">Currently selected styles.</param>
		public ListBoxFontStyle(FontFamily fontFamily, FontStyle currentStyles)
			: this()
		{
			var fontStyles = ((FontStyle[])Enum.GetValues(typeof(FontStyle))).Where(item => item != FontStyle.Regular).ToArray();

			Items.Clear();

			foreach (FontStyle style in fontStyles.Where(fontFamily.IsStyleAvailable))
			{
			    Items.Add(style, (currentStyles & style) == style);
			}

		    CheckOnClick = true;
		}
		#endregion
	}
}
