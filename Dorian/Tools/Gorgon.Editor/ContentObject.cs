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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	public abstract class ContentObject
		: IDisposable, INamedObject
	{
		#region Variables.
        private bool _hasChanged = false;           // Flag to indicate that the object was changed.
		private string _name = "Content";			// Name of the content.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the content has unsaved changes.
		/// </summary>
		protected internal bool HasChanges
		{
            get
            {
                return _hasChanged;
            }
            internal set
            {
                _hasChanged = value;
                OnHasChangesUpdated();
            }
		}

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
		public abstract string ContentType
		{
			get;
		}

		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get
			{
				return Program.Graphics;
			}
		}

		/// <summary>
		/// Property to return whether the content object supports a renderer interface.
		/// </summary>
		public abstract bool HasRenderer
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the content object.
		/// </summary>
		public virtual string Name
		{
			get
			{
				return _name;
			}
			protected set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				_name = value;
			}
		}

		/// <summary>
		/// Property to return the file system file that contains this content.
		/// </summary>
		public GorgonFileSystemFileEntry File
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function called when the HasChanges property is updated.
        /// </summary>
        protected virtual void OnHasChangesUpdated()
        {
        }

        /// <summary>
        /// Function to persist the content to the file system.
        /// </summary>
        protected abstract void OnPersist();

		/// <summary>
		/// Function called when the content window is closed.
		/// </summary>
		/// <returns>TRUE to continue closing the window, FALSE to cancel the close.</returns>
		protected abstract bool OnClose();

		/// <summary>
		/// Function to create new content.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not or canceled.</returns>
		protected internal virtual bool CreateNew()
		{
			return true;
		}

        /// <summary>
        /// Function to persist the content to a file in the file system.
        /// </summary>
        /// <param name="file">The file to persist the data into.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (Nothing in VB.Net).</exception>
        public void Persist(GorgonFileSystemFileEntry file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            File = file;

            if (HasChanges)
            {
                OnPersist();
                HasChanges = false;    
            }
        }

		/// <summary>
		/// Function to close the content window.
		/// </summary>
		/// <returns>TRUE to continue closing the window, FALSE to cancel the close.</returns>
		public bool Close()
		{
			return OnClose();
		}

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public virtual void Draw()
		{
		}

		/// <summary>
		/// Function to initialize the content editor.
		/// </summary>
		/// <returns>A control to place in the primary interface window.</returns>
		public abstract Control InitializeContent();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentObject"/> class.
		/// </summary>
		protected ContentObject()
		{
			HasChanges = false;			
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
			// Nothing in here to use.
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
