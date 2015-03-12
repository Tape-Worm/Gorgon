#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, March 11, 2015 11:53:20 PM
// 
#endregion

using System;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// An abstract implementation of the <see cref="IContentModel"/> interface.
	/// </summary>
	/// <remarks>
	/// Developers of content plug-ins must override this object and return the necessary items for their content so the interface can properly consume the view and other 
	/// related items.
	/// </remarks>
	public abstract class ContentModel
		: IContentModel
	{
		#region Variables.
		// The content interface.
		private IContentData _content;
		// The content view interface.
		private IContentPanel _contentView;
		// Flag to indicate that the content was disposed.
		private bool _disposed;
		// Synchronization object for threads.
		private readonly static object _syncLock = new object();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the content object.
		/// </summary>
		/// <returns>The new content object.</returns>
		protected abstract IContentData CreateContent();

		/// <summary>
		/// Function to create the content view.
		/// </summary>
		/// <returns>The content view.</returns>
		protected abstract IContentPanel CreateView();
		#endregion

		#region IContentService Members
		#region Properties.
		/// <summary>
		/// Property to return whether the content provides a view or not.
		/// </summary>
		public abstract bool HasView
		{
			get;
		}

		/// <summary>
		/// Property to return the content interface.
		/// </summary>
		public IContentData Content
		{
			get
			{
				if (_content != null)
				{
					return _content;
				}

				lock (_syncLock)
				{
					_content = CreateContent();
				}

				return _content;
			}
		}

		/// <summary>
		/// Property to return the content view.
		/// </summary>
		/// <remarks>
		/// If the content does not support views, then this value will be NULL (Nothing in VB.Net).
		/// </remarks>
		public IContentPanel View
		{
			get
			{
				if (_contentView != null)
				{
					return _contentView;
				}

				lock (_syncLock)
				{
					_contentView = CreateView();
				}

				return _contentView;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load all content items at once.
		/// </summary>
		public void Load()
		{
			lock (_syncLock)
			{
				CreateContent();
				CreateView();
			}
		}

		/// <summary>
		/// Function used to unload the content and any other related items.
		/// </summary>
		public void Unload()
		{
			if (_disposed)
			{
				return;
			}

			if (_contentView != null)
			{
				_contentView.Dispose();
			}

			if (_content != null)
			{
				_content.Dispose();
			}

			_content = null;
			_contentView = null;
		}
		#endregion
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
				Unload();
			}

			_content = null;
			_contentView = null;
			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
