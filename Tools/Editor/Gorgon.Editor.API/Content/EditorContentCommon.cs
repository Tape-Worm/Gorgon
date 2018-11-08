#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: October 29, 2018 4:15:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.Content
{
    /// <summary>
    /// Common functionality for editor content.
    /// </summary>
    public abstract class EditorContentCommon
        : IEditorContent
    {
        #region Constants.
        /// <summary>
        /// The attribute name for the content type attribute.
        /// </summary>
        public const string ContentTypeAttr = "Type";
        #endregion

        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of content.
        /// </summary>
        public abstract string ContentType
        {
            get;
        }

        /// <summary>
        /// Property to return the content file.
        /// </summary>
        public IContentFile File
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Function to retrieve the view for the content.</summary>
        /// <returns>A UI for the content, must not be <b>null</b>.</returns>
        protected abstract ContentBaseControl OnGetView();

        /// <summary>Function to close the content.</summary>
        public abstract void Close();

        /// <summary>Function to retrieve the view for the content.</summary>
        /// <returns>A UI for the content, must not be <b>null</b>.</returns>
        /// <exception cref="GorgonException">Thrown if no view was found for the content.</exception>
        public ContentBaseControl GetView()
        {
            ContentBaseControl control = OnGetView();

            if (control == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_NO_CONTENT_VIEW);
            }

            return control;
        }
        
        /// <summary>
        /// Function to initialize the content.
        /// </summary>
        public abstract void Initialize();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the EditorContentCommon class.</summary>
        /// <param name="file">The file for the content.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public EditorContentCommon(IContentFile file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));

            if (!string.IsNullOrWhiteSpace(ContentType))
            {
                File.Metadata.Attributes[ContentTypeAttr] = ContentType;
            }            
        }
        #endregion
    }
}
