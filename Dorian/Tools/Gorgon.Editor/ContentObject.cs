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
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Content property changed event arguments.
    /// </summary>
    class ContentPropertyChangedEventArgs
        : EventArgs
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the name of the property that was updated.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the value for the property that was updated.
        /// </summary>
        public object Value
        {
            get;
            private set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public ContentPropertyChangedEventArgs(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }
        #endregion
    }

    /// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	public abstract class ContentObject
		: IDisposable, INamedObject
	{
		#region Variables.
        private Control _contentControl = null;     // Control used to edit/display the content.
        private bool _hasChanged = false;           // Flag to indicate that the object was changed.
		private string _name = "Content";			// Name of the content.
		private string _filePath = string.Empty;	// Path to the file for the content.
		#endregion

        #region Events.
        /// <summary>
        /// Event fired when the content has had a property change.
        /// </summary>
        [Browsable(false)]
        internal event EventHandler<ContentPropertyChangedEventArgs> ContentPropertyChanged;
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
        /// Property to return whether this is an internal content object only.
        /// </summary>
        internal virtual bool IsInternal
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return the type descriptor for this content.
        /// </summary>
        [Browsable(false)]
        internal ContentTypeDescriptor TypeDescriptor
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the plug-in that can create this content.
        /// </summary>
        [Browsable(false)]
        public ContentPlugIn PlugIn
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return whether the content has been initialized or not.
        /// </summary>
        [Browsable(false)]
        public bool IsInitialized
        {
            get
            {
                return _contentControl != null;
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
        /// Property to return whether the content can be exported.
        /// </summary>
        [Browsable(false)]
        public abstract bool CanExport
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
		/// Property to return the graphics interface for the application.
		/// </summary>
        [Browsable(false)]
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
        [Browsable(false)]
		public abstract bool HasRenderer
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the content object.
		/// </summary>
        [Browsable(true), Category("Design"), Description("Provides a name for the content.  The name should conform to a standard file name.")]
		public virtual string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

                if (string.Compare(value, _name, true) != 0)
                {
                    _name = ValidateName(value);
                    HasChanges = true;
                    OnContentPropertyChanged("Name", _name);
                }
			}
		}

		/// <summary>
		/// Property to return the file system file that contains this content.
		/// </summary>
        [Browsable(false)]
		public GorgonFileSystemFileEntry File
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_filePath))
				{
					return null;
				}

				return Program.ScratchFiles.GetFile(_filePath);
			}
			internal set
			{
				if ((value == null) && (!string.IsNullOrWhiteSpace(_filePath)))
				{
					_filePath = string.Empty;
                    _name = string.Empty;
					return;
				}

                if (string.Compare(value.FullPath, _filePath, true) != 0)
                {
                    _filePath = value.FullPath;
                    _name = File.Name;
                    HasChanges = true;
                }
			}
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
        /// Function to update the content.
        /// </summary>
        protected virtual void UpdateContent()
        {
            HasChanges = true;
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
		/// Function to open the content from the file system.
		/// </summary>
		protected internal virtual void OnOpenContent()
		{
		}

		/// <summary>
		/// Function to create new content.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not or canceled.</returns>
		protected internal virtual bool CreateNew()
		{
			return true;
		}

        /// <summary>
        /// Function called when the name is about to be changed.
        /// </summary>
        /// <param name="proposedName">The proposed name for the content.</param>
        /// <returns>A valid name for the content.</returns>
        protected virtual string ValidateName(string proposedName)
        {
            return proposedName;
        }

        /// <summary>
        /// Function to notify the application that a property on the content object was changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that was updated.</param>
        /// <param name="value">Value assigned to the property.</param>
        protected virtual void OnContentPropertyChanged(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("The parameter must not be NULL or empty.", "propertyName");
            }

            if (ContentPropertyChanged != null)
            {
                ContentPropertyChanged(this, new ContentPropertyChangedEventArgs(propertyName, value));
            }
        }

        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>A control to place in the primary interface window.</returns>
        protected abstract Control OnInitialize();

		/// <summary>
		/// Function called when the content is shown for the first time.
		/// </summary>
		public abstract void Activate();

        /// <summary>
        /// Function to export the content to an external file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        public virtual void Export(string filePath)
        {
        }

        /// <summary>
        /// Function to retrieve default values for properties with the DefaultValue attribute.
        /// </summary>
        internal void SetDefaults()
        {
            foreach (var descriptor in TypeDescriptor)
            {
                if (descriptor.HasDefaultValue)
                    descriptor.DefaultValue = descriptor.GetValue<object>();
            }
        }
        
        /// <summary>
		/// Function to open the content from the file system.
		/// </summary>
		/// <param name="file">File containing the content to open.</param>
		internal void OpenContent(GorgonFileSystemFileEntry file)
		{
            try
            {
                File = file;
                OnOpenContent();

                // Update the properties.
                SetDefaults();
            }
            finally
            {
                HasChanges = false;
            }
		}

        /// <summary>
        /// Function called when a property is changed from the property grid.
        /// </summary>
        /// <param name="e">Event parameters called when a property is changed.</param>
        public virtual void PropertyChanged(PropertyValueChangedEventArgs e)
        {
            
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
        public Control InitializeContent()
        {
            if (_contentControl == null)
            {
                _contentControl = OnInitialize();
            }

            return _contentControl;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentObject"/> class.
		/// </summary>
		protected ContentObject()
		{
			HasChanges = false;

            TypeDescriptor = new ContentTypeDescriptor(this);
            TypeDescriptor.Enumerate(GetType());
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
            _contentControl = null;
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
