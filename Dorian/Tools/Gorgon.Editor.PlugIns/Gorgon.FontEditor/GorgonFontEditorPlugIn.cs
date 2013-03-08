#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, March 07, 2013 8:17:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Font editor plug-in interface.
    /// </summary>
    public class GorgonFontEditorPlugIn
        : ContentPlugIn
	{
		#region Variables.
		private ToolStripMenuItem _createItem = null;
		private bool _disposed = false;
		#endregion

		#region Methods.
		/// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <param name="editorObjects">Editor information to pass to the interface.</param>
        /// <returns>
        /// A new content object interface.
        /// </returns>
        protected override ContentObject CreateContentObject(EditorPlugInData editorObjects)
        {
            return new GorgonFontContent();
        }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_createItem != null)
					{
						_createItem.Dispose();
					}
				}

				_createItem = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public override ToolStripMenuItem GetCreateMenuItem()
		{
			if (_createItem == null)
			{
				_createItem = new ToolStripMenuItem("Create new font...", Properties.Resources.font_document_16x16);
				_createItem.Name = "itemCreateGorgonFont";
			}

			return _createItem;
		}

		/// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>
		/// The 16x16 image for the content.
		/// </returns>
		public override System.Drawing.Image GetContentIcon()
		{
			return Properties.Resources.font_document_16x16;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontEditorPlugIn"/> class.
        /// </summary>
        public GorgonFontEditorPlugIn()
            : base("Gorgon Font Editor")
        {
			FileExtensions.Add(".gorfont", new Tuple<string, string>("gorFont", "Gorgon Font File (*.gorFont)"));
        }
        #endregion
    }
}
