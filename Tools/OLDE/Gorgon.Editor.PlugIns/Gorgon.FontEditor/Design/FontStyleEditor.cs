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
// Created: Sunday, May 06, 2012 11:14:29 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font style editor.
	/// </summary>
	class FontStyleEditor
		: UITypeEditor
	{
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
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			var editorSerivce = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			ListBoxFontStyle fontStyles = null;
			var descriptor = context.Instance as ContentTypeDescriptor;

            Debug.Assert(descriptor != null, "No descriptor!");

            var content = descriptor.Content as GorgonFontContent;

            Debug.Assert(content != null, "No content!!");

		    if (value == null)
		    {
		        value = FontStyle.Regular;
		    }

			try
			{
			    FontFamily family = FontFamily.Families.FirstOrDefault(item =>
			                                                           string.Equals(item.Name,
			                                                                         content.FontFamily,
			                                                                         StringComparison.OrdinalIgnoreCase));

			    if (family == null)
			    {
			        return value;
			    }

			    fontStyles = new ListBoxFontStyle(family, (FontStyle)value)
			                 {
			                     Service = editorSerivce,
			                     BackColor = DarkFormsRenderer.DarkBackground,
			                     ForeColor = DarkFormsRenderer.ForeColor,
			                     BorderStyle = BorderStyle.None
			                 };

			    editorSerivce.DropDownControl(fontStyles);

				return fontStyles.FontStyle != (FontStyle)value ? fontStyles.FontStyle : value;
			}
			finally
			{
			    if (fontStyles != null)
			    {
			        fontStyles.Dispose();
			    }
			}
		}

		/// <summary>
		/// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <returns>
		/// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"/> value that indicates the style of editor used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method. If the <see cref="T:System.Drawing.Design.UITypeEditor"/> does not support this method, then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> will return <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"/>.
		/// </returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
		#endregion
	}
}
