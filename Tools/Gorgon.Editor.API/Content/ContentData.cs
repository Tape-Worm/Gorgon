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
using System.ComponentModel;
using System.Threading;
using Gorgon.Core;
using Gorgon.Design;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
{
    /// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	/// <remarks>
	/// Content objects are wrappers for data stored in the file system used by the editor. When content is created, a new content data item should be 
	/// created by the content wrapper (e.g. a sprite, a font, text block, etc...). When the data object is created it will then expose the properties 
	/// that the editor should edit by providing properties on the content object that reflect those of the data object.
	/// <para>
	/// When persisting or restoring content data from the file system, a <see cref="IContentSerializer"/> object will be passed to the constructor of 
	/// the content object. This serializer object is responsible for sending and receiving data to and from the file system and (de)composing the data 
	/// into content data.
	/// </para>
	/// </remarks>
	public abstract class ContentData
		: IContentData
	{
		#region Variables.
		private string _name = "Content";									// Name of the content.
	    private bool _disposed;												// Flag to indicate that the object was disposed.
	    private bool _isOwned;												// Flag to indicate that this content is linked to another piece of content.
	    private int _renameLock;											// Lock flag for renaming.
	    private bool _hasChanges;											// Flag to indicate that we have changes.
		#endregion

        #region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the AfterSerializeEvent event of the Serializer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Serializer_AfterSerializeEvent(object sender, EventArgs eventArgs)
		{
			// Mark this object as being persisted.
			HasChanges = false;
		}

		/// <summary>
		/// Function called before content is renamed.
		/// </summary>
		/// <param name="e">Event parameters for the event.</param>
	    protected virtual void OnBeforeContentRenamed(BeforeContentRenamedArgs e)
	    {
			if (BeforeContentRenamed != null)
			{
				BeforeContentRenamed(this, e);
			}
	    }

		/// <summary>
		/// Function called after content is renamed.
		/// </summary>
		/// <param name="e">Event parameters for the event.</param>
	    protected virtual void OnAfterContentRenamed(AfterContentRenamedArgs e)
	    {
		    if (AfterContentRenamed != null)
		    {
			    AfterContentRenamed(this, e);
		    }
	    }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentData"/> class.
		/// </summary>
		/// <param name="serializer">The serializer used to persist or restore the data for the content.</param>
	    protected ContentData(IContentSerializer serializer)
	    {
		    Serializer = serializer;

			// Hook up the events for the serializer.
			if (Serializer != null)
			{
				Serializer.AfterSerialize += Serializer_AfterSerializeEvent;
			}
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
				if (Serializer != null)
				{
					Serializer.AfterSerialize -= Serializer_AfterSerializeEvent;
				}
			}

			_disposed = true;
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

		#region IContent Members
		#region Events.
		/// <summary>
		/// Event fired before content is renamed.
		/// </summary>
		public event EventHandler<BeforeContentRenamedArgs> BeforeContentRenamed;

		/// <summary>
		/// Event fired after content is renamed.
		/// </summary>
		public event EventHandler<AfterContentRenamedArgs> AfterContentRenamed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the serializer for this content.
		/// </summary>
		public IContentSerializer Serializer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the content has pending changes that need saving.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Serializer" /> property is NULL (Nothing in VB.Net), then this property will always return <c>false</c>.
		/// </remarks>
		[Browsable(false)]
		public bool HasChanges
		{
			get
			{
				// If we cannot serialize this content, then there's no point in 
				// advertising whether or not there's been changes.
				return Serializer != null && _hasChanges;
			}
			protected set
			{
				if (Serializer == null)
				{
					return;
				}

				_hasChanges = value;
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

					var beforeArgs = new BeforeContentRenamedArgs(Name, value);
					OnBeforeContentRenamed(beforeArgs);

					// We need a name for content.
					// So, either cancel if the user requests it, or cancel if the user passes back NULL or an empty string.
					if ((beforeArgs.Cancel)
						|| (string.IsNullOrWhiteSpace(beforeArgs.NewName)))
					{
						return;
					}

					// Ensure that the naming format is valid.  The editor requires that names 
					// be formatted like a file name.
					_name = beforeArgs.NewName.FormatFileName();

					var afterArgs = new AfterContentRenamedArgs(beforeArgs.OldName, _name);
					OnAfterContentRenamed(afterArgs);

					// This should be moved into a controller or something in response to the AfterContentRenamed event.
					/*// If we have a content UI, then tell it of the change to the property.
					if (ContentControl != null)
					{
						ContentControl.UpdateCaption();
					}*/
				}
				finally
				{
					Interlocked.Decrement(ref _renameLock);
				}
			}
		}

		/// <summary>
		/// Property to return whether the content data is read-only.
		/// </summary>
		/// <remarks>
		/// The data in the content may be read-only in cases where other content items is dependant upon the content.
		/// </remarks>
		[Browsable(false)]
		public bool ReadOnly
		{
			get
			{
				return _isOwned;
			}
			set
			{
				_isOwned = value;
				//TypeDescriptor["Name"].IsReadOnly = value;
			}
		}
		#endregion
		#endregion
	}
}
