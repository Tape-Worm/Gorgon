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
using Gorgon.Design;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.IO;

namespace Gorgon.Editor
{
    /// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	public abstract class ContentObject
		: IDisposable, INamedObject
    {
        #region Constants.
        /// <summary>
        /// The type name for the raw input plug-in.
        /// </summary>
        public const string GorgonRawInputTypeName = "GorgonLibrary.Input.GorgonRawPlugIn";
        #endregion

		#region Variables.
		private GorgonInputFactory _input;									// Raw input interface.
        private string _name = "Content";									// Name of the content.
	    private bool _disposed;												// Flag to indicate that the object was disposed.
	    private bool _isOwned;												// Flag to indicate that this content is linked to another piece of content.
	    private int _renameLock;											// Lock flag for renaming.
	    private EditorFile _editorFile;										// The editor file for the content.
		#endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the action to use when closing the current content.
		/// </summary>
	    internal Action<ContentObject> OnCloseCurrent
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the action to use when content is renamed by changing the name property.
		/// </summary>
	    internal Action<ContentObject, string> OnRenameContent
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the action to use when refreshing properties on the property panel.
		/// </summary>
	    internal Action<ContentObject> OnPropertyRefreshed
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the action to use when committing this content.
		/// </summary>
	    internal Action<ContentObject> OnCommit
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the action to use when reloading this content.
		/// </summary>
	    internal Action<ContentObject> OnReload
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the action to use when the content changes.
		/// </summary>
	    internal Action OnChanged
	    {
		    get;
		    set;
	    }

	    /// <summary>
	    /// Property to return the control used to edit/display content.
	    /// </summary>
	    internal ContentPanel ContentControl
	    {
		    get;
		    private set;
	    }

		/// <summary>
		/// Property to return the type descriptor for this content.
		/// </summary>
		[Browsable(false)]
		public ContentTypeDescriptor TypeDescriptor
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
		/// Property to return the list of working dependencies for the content.
		/// </summary>
		/// <remarks>Whenever dependencies need to be updated for content, this is the collection that must be used.</remarks>
		[Browsable(false)]
	    public ContentDependencies Dependencies
	    {
		    get;
		    private set;
	    }

		/// <summary>
		/// Property to return the editor file associated with this content.
		/// </summary>
		[Browsable(false)]
	    public EditorFile EditorFile
	    {
			get
			{
				return _editorFile;
			}
			protected internal set
			{
				Dependencies.Clear();

				_editorFile = value;

				if (_editorFile == null)
				{
					return;
				}

				// Add existing dependencies.
				foreach (Dependency dependency in _editorFile.DependsOn)
				{
					Dependencies.Add(dependency.Clone());
				}
			}
	    }

		/// <summary>
		/// Property to return the default registered image editor.
		/// </summary>
		/// <remarks>Use this to load in image data for textures.</remarks>
		[Browsable(false)]
	    public IImageEditorPlugIn ImageEditor
	    {
		    get;
		    internal set;
	    }

		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
		[Browsable(false)]
		public static GorgonGraphics Graphics
		{
			get;
			internal set;
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
		LocalDisplayName(typeof(APIResources), "GOREDIT_TEXT_NAME"),
		LocalCategory(typeof(APIResources), "PROP_CATEGORY_DESIGN"), 
		LocalDescription(typeof(APIResources), "PROP_NAME_DESC")]
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

					if ((HasProperties)
					    && (OnRenameContent != null))
					{
						OnRenameContent(this, newName);
					}

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
				TypeDescriptor["Name"].IsReadOnly = value;
			}
	    }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to finalize the dependency list to the file.
        /// </summary>
        private void CommitDependencies()
        {
            if (EditorFile == null)
            {
                return;
            }

            EditorFile.DependsOn.Clear();

            foreach (Dependency dependency in Dependencies)
            {
                EditorFile.DependsOn[dependency.EditorFile, dependency.Type] = dependency;
            }
        }

        /// <summary>
        /// Function to return the raw input object from the editor.
        /// </summary>
        /// <returns>The raw input interface from the editor.</returns>
        protected GorgonInputFactory GetRawInput()
        {
            if (_input != null)
            {
                return _input;
            }

            if (!GorgonApplication.PlugIns.Contains(GorgonRawInputTypeName))
            {
                return null;
            }

            _input = GorgonInputFactory.CreateInputFactory(GorgonRawInputTypeName);

            return _input;
        }

        /// <summary>
        /// Function called after the content data has been updated.
        /// </summary>
        protected virtual void OnContentUpdated()
        {
        }

		/// <summary>
		/// Function to persist the content data to a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data.</param>
	    protected abstract void OnPersist(Stream stream);

		/// <summary>
		/// Function to read the content data from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
	    protected abstract void OnRead(Stream stream);

		/// <summary>
		/// Function to load a dependency file.
		/// </summary>
		/// <param name="dependency">The dependency to load.</param>
		/// <param name="stream">Stream containing the dependency file.</param>
		/// <returns>The result of the load operation.  If the dependency loaded correctly, then the developer should return a successful result.  If the dependency 
		/// is not vital to the content, then the developer can return a continue result, otherwise the developer should return fatal result and the content will 
		/// not continue loading.</returns>
	    protected virtual DependencyLoadResult OnLoadDependencyFile(Dependency dependency, Stream stream)
	    {
			return new DependencyLoadResult(DependencyLoadState.Successful, null);
	    }

        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>A control to place in the primary interface window.</returns>
        protected abstract ContentPanel OnInitialize();

        /// <summary>
        /// Function called when editor or plug-in settings are updated.
        /// </summary>
        protected internal virtual void OnEditorSettingsUpdated()
        {
        }

		/// <summary>
		/// Function to let the interface know that a property has been changed.
		/// </summary>
		/// <param name="property">Name of the property.</param>
		/// <param name="value">New value for the property.</param>
	    protected internal void NotifyPropertyChanged(string property, object value)
	    {
			// If we have a content UI, then tell it of the change to the property.
			if (ContentControl == null)
			{
				return;
			}

			HasChanges = !string.Equals(property, "revert", StringComparison.OrdinalIgnoreCase) 
							&& !string.Equals(property, "commit", StringComparison.OrdinalIgnoreCase);
			ContentControl.OnContentPropertyChanged(property, value);

			if (OnChanged != null)
			{
				OnChanged();
			}
		}

		/// <summary>
		/// Function to let the interface know that a property has been changed.
		/// </summary>
		/// <param name="propertyName">Name of the property that was updated.  This value is passed automatically.</param>
	    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	    {
			object value = null;

			if (TypeDescriptor.Contains(propertyName))
			{
				value = TypeDescriptor[propertyName].GetValue<object>();
			}

			NotifyPropertyChanged(propertyName, value);
	    }

        /// <summary>
        /// Function called when the content is reverted back to its original state.
        /// </summary>
        /// <returns><c>true</c> if reverted, <c>false</c> if not.</returns>
        protected virtual bool OnRevert()
        {
            return false;
        }

        /// <summary>
        /// Function to retrieve default values for properties with the DefaultValue attribute.
        /// </summary>
        internal void SetDefaults()
        {
            foreach (var descriptor in TypeDescriptor.Where(descriptor => descriptor.HasDefaultValue))
            {
                descriptor.DefaultValue = descriptor.GetValue<object>();
            }
        }

		/// <summary>
		/// Function to read a dependency file from a stream.
		/// </summary>
		/// <param name="dependency">The dependency to load.</param>
		/// <param name="stream">Stream containing the file to read.</param>
		/// <returns>The result of the load operation.</returns>
	    internal DependencyLoadResult LoadDependencyFile(Dependency dependency, Stream stream)
	    {
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException(APIResources.GOREDIT_ERR_STREAM_WRITE_ONLY);
			}

			if (stream.Position >= stream.Length)
			{
				throw new EndOfStreamException(APIResources.GOREDIT_ERR_STREAM_EOS);
			}

			return OnLoadDependencyFile(dependency, stream);
		}

		/// <summary>
		/// Function to read the content from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
	    internal void Read(Stream stream)
	    {
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException(APIResources.GOREDIT_ERR_STREAM_WRITE_ONLY);
			}

			if (stream.Position >= stream.Length)
			{
				throw new EndOfStreamException(APIResources.GOREDIT_ERR_STREAM_EOS);
			}

			OnRead(stream);

            HasChanges = false;

			SetDefaults();

            // Update the panel
			if (ContentControl != null)
			{
				ContentControl.RefreshContent();
			}
	    }

		/// <summary>
		/// Function to persist the content data into a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data for the content.</param>
		internal void Persist(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(APIResources.GOREDIT_ERR_STREAM_READ_ONLY);
			}

            CommitDependencies();

			OnPersist(stream);

            HasChanges = false;

			if (ContentControl != null)
			{
				ContentControl.ContentPersisted();
			}
		}

		/// <summary>
        /// Function to determine if a content property is available to the UI.
        /// </summary>
        /// <param name="propertyName">The name of the property to look up.</param>
        /// <returns><c>true</c> if found, <c>false</c> if not.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyName"/> is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="propertyName"/> is empty.</exception>
        public bool HasProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (string.IsNullOrWhiteSpace("propertyName"))
            {
                throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY);
            }

            return TypeDescriptor.Contains(propertyName);
        }

		/// <summary>
		/// Function to refresh a property in the property grid.
		/// </summary>
		/// <param name="propertyName">Name of the property to refresh.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="propertyName"/> is empty.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the <paramref name="propertyName"/> does not exist as a property for this content.</exception>
		public void RefreshProperty(string propertyName)
	    {
			if (propertyName == null)
			{
				throw new ArgumentNullException("propertyName");
			}

			if (string.IsNullOrWhiteSpace("propertyName"))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY);
			}

			if (!TypeDescriptor.Contains(propertyName))
			{
				throw new KeyNotFoundException(string.Format(APIResources.GOREDIT_ERR_PROPERTY_NOT_FOUND, propertyName));
			}

			if ((!HasProperties)
				|| (OnPropertyRefreshed == null))
			{
				return;
			}

			OnPropertyRefreshed(this);
	    }

        /// <summary>
        /// Function to set a property as disabled.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="disabled"><c>true</c> if disabled, <c>false</c> if not.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyName"/> is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="propertyName"/> is empty.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the <paramref name="propertyName"/> does not exist as a property for this content.</exception>
        public void DisableProperty(string propertyName, bool disabled)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (string.IsNullOrWhiteSpace("propertyName"))
            {
                throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY);
            }

            if (!TypeDescriptor.Contains(propertyName))
            {
                throw new KeyNotFoundException(string.Format(APIResources.GOREDIT_ERR_PROPERTY_NOT_FOUND, propertyName));
            }

	        if (!HasProperties)
	        {
		        return;
	        }

            TypeDescriptor[propertyName].IsReadOnly = disabled;

			RefreshProperty(propertyName);
        }

		/// <summary>
		/// Function to retrieve a thumbnail image for the content plug-in.
		/// </summary>
		/// <returns>The image for the thumbnail of the content.</returns>
		/// <remarks>The size of the thumbnail should be set to 128x128.</remarks>
		public virtual Image GetThumbNailImage()
		{
			return null;
		}

        /// <summary>
        /// Function called when a property is changed from the property grid.
        /// </summary>
        /// <param name="e">Event parameters called when a property is changed.</param>
        public virtual void OnPropertyChanged(PropertyValueChangedEventArgs e)
        {
            
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
        public ContentPanel InitializeContent()
		{
			ContentControl = OnInitialize();
			return ContentControl;
		}

		/// <summary>
		/// Function called when the content is fully loaded and ready for editing.
		/// </summary>
	    public virtual void OnContentReady()
	    {
	    }

		/// <summary>
		/// Function to close the content object.
		/// </summary>
		/// <remarks>Ensure that any changes to the content are persisted before calling this method, otherwise those changes will be lost.</remarks>
	    public void CloseContent()
		{
			if (OnCloseCurrent == null)
			{
				return;
			}

			OnCloseCurrent(this);
	    }

		/// <summary>
		/// Function to reload this content from its associated file on the file system.
		/// </summary>
	    public void Reload()
	    {
			if ((!HasChanges)
			    || (OnReload == null))
			{
				return;
			}

			OnReload(this);

			HasChanges = false;
			NotifyPropertyChanged("Reload", null);
	    }

		/// <summary>
		/// Function to commit this content back to the file system.
		/// </summary>
	    public void Commit()
	    {
			if ((!HasChanges)
			    || (OnCommit == null))
			{
				return;
			}

			OnCommit(this);

			HasChanges = false;
			NotifyPropertyChanged("Commit", null);
	    }

        /// <summary>
        /// Function to revert the content back to the original state.
        /// </summary>
        public void Revert()
        {
            if (!OnRevert())
            {
                return;
            }
			
            NotifyPropertyChanged("Revert", null);
        }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentObject"/> class.
		/// </summary>
		/// <param name="settings">The settings for the content.</param>
		protected ContentObject(ContentSettings settings)
		{
			TypeDescriptor = new ContentTypeDescriptor(this);
            TypeDescriptor.Enumerate(GetType());
			Dependencies = new ContentDependencies();

			if (settings == null)
			{
				return;
			}

			Name = settings.Name;
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
				Dependencies.Clear();

				if (_input != null)
				{
					_input.Dispose();
				}

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
