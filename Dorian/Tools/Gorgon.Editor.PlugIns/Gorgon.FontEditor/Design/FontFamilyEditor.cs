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
// Created: Sunday, May 06, 2012 11:15:27 PM
// 
#endregion

using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font family editor.
	/// </summary>
	class FontFamilyEditor
		: UITypeEditor
	{
		#region Properties.
		/// <summary>
		/// Gets a value indicating whether drop-down editors should be resizable by the user.
		/// </summary>
		/// <returns>true if drop-down editors are resizable; otherwise, false. </returns>
		public override bool IsDropDownResizable
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <param name="provider">An <see cref="T:System.IServiceProvider"/> that this editor can use to obtain services.</param>
		/// <param name="value">The object to edit.</param>
		/// <returns>
		/// The new value of the object. If the value of the object has not changed, this should return the same object it was passed.
		/// </returns>
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService editorSerivce = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			listBoxFont fonts = null;

			try
			{
				fonts = new listBoxFont();
				fonts.SelectedItem = value.ToString();
				fonts.Service = editorSerivce;

				editorSerivce.DropDownControl(fonts);

				if (string.Compare(fonts.SelectedItem.ToString(), value.ToString(), true) != 0)
					return fonts.SelectedItem.ToString();

				return value;
			}
			finally
			{
				if (fonts != null)
					fonts.Dispose();
			}
		}

		/// <summary>
		/// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <returns>
		/// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"/> value that indicates the style of editor used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method. If the <see cref="T:System.Drawing.Design.UITypeEditor"/> does not support this method, then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> will return <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"/>.
		/// </returns>
		public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FontFamilyEditor"/> class.
		/// </summary>
		public FontFamilyEditor()
		{
		}
		#endregion
	}
}
