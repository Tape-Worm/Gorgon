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
// Created: Wednesday, March 6, 2013 11:23:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
    /// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	public abstract class EditorContent
		: IDisposable, INamedObject
    {
		#region Variables.
        private string _name = "Content";									// Name of the content.
	    private bool _disposed;												// Flag to indicate that the object was disposed.
	    private bool _isOwned;												// Flag to indicate that this content is linked to another piece of content.
	    private int _renameLock;											// Lock flag for renaming.
		#endregion

        #region Properties.
	    /// <summary>
	    /// Property to return the control used to edit/display content.
	    /// </summary>
	    public ContentPanel ContentControl
	    {
		    get;
		    private set;
	    }

		/// <summary>
        /// Property to return whether this content has changed or not.
        /// </summary>
        [Browsable(false)]
        public bool HasChanges
        {
            get;
            internal set;
        }

		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
		[Browsable(false)]
		public GorgonGraphics Graphics
		{
			get;
			internal set;
		}

        /// <summary>
        /// Property to return whether the content has been initialized or not.
        /// </summary>
        [Browsable(false)]
        public bool IsInitialized
        {
            get
            {
                return ContentControl != null;
            }
        }

        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        [Browsable(false)]
        public abstract bool HasProperties
        {
            get;
        }

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
        [Browsable(false)]
		public abstract string ContentType
		{
			get;
		}

		/// <summary>
		/// Property to return whether the content object supports a renderer interface.
		/// </summary>
        [Browsable(false)]
		public abstract bool HasRenderer
		{
			get;
		}

		/// <summary>
		/// Property to return whether this content has a thumbnail or not.
		/// </summary>
		[Browsable(false)]
	    public bool HasThumbnail
	    {
		    get;
		    protected set;
	    }

		/// <summary>
		/// Property to return the name of the content object.
		/// </summary>
        [Browsable(true),
		LocalDisplayName(typeof(Resources), "GOREDIT_TEXT_NAME"),
		LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_NAME_DESC")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				try
				{
					// We're already in the middle of a rename operation, so do nothing.
					if (Interlocked.Increment(ref _renameLock) > 1)
					{
						return;
					}

					if (string.IsNullOrWhiteSpace(value))
					{
						return;
					}

					if (string.Equals(value, _name, StringComparison.OrdinalIgnoreCase))
					{
						return;
					}

					string newName = value.FormatFileName();

					if (string.IsNullOrWhiteSpace(newName))
					{
						return;
					}

					/*if ((HasProperties)
					    && (OnRenameContent != null))
					{
						OnRenameContent(this, newName);
					}*/

					_name = newName;

					// If we have a content UI, then tell it of the change to the property.
					if (ContentControl != null)
					{
						ContentControl.UpdateCaption();
					}
				}
				finally
				{
					Interlocked.Decrement(ref _renameLock);
				}
			}
		}

		/// <summary>
		/// Property to return whether this content has an owner or not.
		/// </summary>
        [Browsable(false)]
	    public bool HasOwner
	    {
			get
			{
				return _isOwned;
			}
			internal set
			{
				_isOwned = value;
				//TypeDescriptor["Name"].IsReadOnly = value;
			}
	    }
		#endregion

		#region Methods.
        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>A control to place in the primary interface window.</returns>
        protected abstract ContentPanel OnInitialize();
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorContent"/> class.
		/// </summary>
		protected EditorContent()
		{
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return Name;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if ((ContentControl != null)
				    && (!ContentControl.IsDisposed))
				{
					ContentControl.Dispose();
				}
			}

			_disposed = true;
            ContentControl = null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Calling this method will -not- call the Close method.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
    }
}
