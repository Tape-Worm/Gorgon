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
// Created: Thursday, March 07, 2013 9:30:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// The main content interface.
    /// </summary>
    class GorgonFontContent
        : ContentObject
    {
        #region Variables.
		private ContentPlugIn _plugIn = null;						// The plug-in that interfaces the content with the main system.
        private GorgonFontContentPanel _panel = null;               // Interface for editing the font.
        private bool _disposed = false;                             // Flag to indicate that the object was disposed.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of content.
        /// </summary>
        public override string ContentType
        {
            get 
            {
                return "Gorgon Font";
            }
        }		

        /// <summary>
        /// Property to return whether the content object supports a renderer interface.
        /// </summary>
        public override bool HasRenderer
        {
            get 
            {
                return true;
            }
        }
        #endregion

        #region Methods.
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
					_panel.Dispose();
					_panel = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Function called when the content window is closed.
        /// </summary>
        /// <returns>
        /// TRUE to continue closing the window, FALSE to cancel the close.
        /// </returns>
        protected override bool OnClose()
        {
            if (HasChanges)
            {
                // TODO: Perform save functionality here.
            }

            return true;
        }

		/// <summary>
		/// Function to create new content.
		/// </summary>
		/// <returns>
		/// TRUE if successful, FALSE if not or canceled.
		/// </returns>
		protected override bool CreateNew()
		{
			formNewFont newFont = null;

			try
			{
				newFont = new formNewFont();
				newFont.Content = this;

				if (newFont.ShowDialog() == DialogResult.OK)
				{
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return false;
			}
			finally
			{
				if (newFont != null)
				{
					newFont.Dispose();
					newFont = null;
				}
			}
		}

        /// <summary>
        /// Function to initialize the content editor.
        /// </summary>
        /// <returns>
        /// A control to place in the primary interface window.
        /// </returns>        
        public override System.Windows.Forms.Control InitializeContent()
        {
            _panel = new GorgonFontContentPanel();
			_panel.Content = this;

            return _panel;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContent"/> class.
        /// </summary>
		/// <param name="plugIn">The plug-in that creates this object.</param>
        public GorgonFontContent(ContentPlugIn plugIn)
            : base()
        {
			_plugIn = plugIn;
        }
        #endregion
    }
}
