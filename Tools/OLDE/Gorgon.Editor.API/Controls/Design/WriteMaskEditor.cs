#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, November 20, 2014 12:39:42 AM
// 
#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor.Design
{
	/// <summary>
	/// List box for write masking flags.
	/// </summary>
	public class WriteMaskEditor
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
		/// Property to return write mask flags.
		/// </summary>
		public ColorWriteMaskFlags WriteMask
		{
			get
			{
				const ColorWriteMaskFlags result = ColorWriteMaskFlags.None;

			    if (CheckedItems.Count == 0)
			    {
				    return ColorWriteMaskFlags.None;
			    }

				return CheckedItems.Count == 4
					       ? ColorWriteMaskFlags.All
					       : CheckedItems.Cast<ColorWriteMaskFlags>().Aggregate(result, (current, item) => current | item);
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
		/// Initializes a new instance of the <see cref="WriteMaskEditor"/> class.
		/// </summary>
		public WriteMaskEditor()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WriteMaskEditor"/> class.
		/// </summary>
		/// <param name="writeMask">The current write mask flags.</param>
		public WriteMaskEditor(ColorWriteMaskFlags writeMask)
			: this()
		{
			var maskBits =
				((ColorWriteMaskFlags[])Enum.GetValues(typeof(ColorWriteMaskFlags))).Where(item => item != ColorWriteMaskFlags.All && item != ColorWriteMaskFlags.None)
				                                                                    .ToArray();

			Items.Clear();

			foreach (ColorWriteMaskFlags flag in maskBits)
			{
			    Items.Add(flag, (writeMask & flag) == flag);
			}


		    CheckOnClick = true;
		}
		#endregion
	}
}
