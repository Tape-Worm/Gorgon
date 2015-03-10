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
// Created: Monday, March 2, 2015 9:26:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Event arguments for the content panel created event.
	/// </summary>
	class ContentCreatedEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the content object that was created.
		/// </summary>
		public IContent Content
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the panel that's been created.
		/// </summary>
		public IContentPanel Panel
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content that was created.</param>
		/// <param name="panel">The UI for the content.</param>
		public ContentCreatedEventArgs(IContent content, IContentPanel panel)
		{
			Panel = panel;
			Content = content;
		}
		#endregion
	}

	/// <summary>
	/// Manages the currently active content in the editor.
	/// </summary>
	sealed class EditorContentManager 
		: IEditorContentManager
	{
		#region Variables.
		// Application log file.
		private GorgonLogFile _log;
		// The settings object for the default content.
		private readonly IEditorSettings _settings;
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Currently displayed content.
		private Tuple<IContent, IContentPanel> _currentContent;
		// Object and UI for the default content.
		private Tuple<IContent, IContentPanel> _noContent;
		// The proxy graphics interface for content.
		private readonly IProxyObject<GorgonGraphics> _graphicsProxy;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorContentManager"/> class.
		/// </summary>
		/// <param name="log">The application log file.</param>
		/// <param name="settings">The application settings to pass to the default content.</param>
		/// <param name="graphicsProxy">The proxy object for the graphics interface.</param>
		public EditorContentManager(GorgonLogFile log, IEditorSettings settings, IProxyObject<GorgonGraphics> graphicsProxy)
		{
			_log = log;
			_graphicsProxy = graphicsProxy;
			_settings = settings;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="EditorContentManager"/> class.
		/// </summary>
		~EditorContentManager()
		{
			Dispose(false);
		}
		#endregion

		#region IContentManager
		#region Events.
		/// <summary>
		/// Event called when the content UI is created.
		/// </summary>
		public event EventHandler<ContentCreatedEventArgs> ContentCreated;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the currently active content.
		/// </summary>
		/// <remarks>
		/// When there is no content created/loaded, then this property will return NULL (Nothing in VB.Net) even though 
		/// the "NoContent" content object is loaded in the display.
		/// </remarks>
		public IContent CurrentContent
		{
			get
			{
				return _currentContent == null ? null : _currentContent.Item1;
			}
		}

		/// <summary>
		/// Property to return the content panel.
		/// </summary>
		/// <remarks>
		/// This will return the UI for the content (if available) for use in the editor.
		/// </remarks>
		public IContentPanel ContentPanel
		{
			get
			{
				return _currentContent == null ? null : _currentContent.Item2;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ClosedEvent event of the Content control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Content_ClosedEvent(object sender, EventArgs e)
		{
			IContent content = CurrentContent ?? _noContent.Item1;
			IContentPanel contentUI = sender as IContentPanel;

			// Ensure that we unsubscribe from the events for this UI.
			if (contentUI != null)
			{
				contentUI.ContentClosing -= Content_ClosingEvent;
				contentUI.ContentClosed -= Content_ClosedEvent;
			}

			if (content != null)
			{
				_log.Print("ContentService: User destroying content '{0}'.", LoggingLevel.Verbose, content.Name);
				content.Dispose();
			}

			_noContent = null;
			_currentContent = null;

			// Create the default content.
			CreateContent();
		}

		/// <summary>
		/// Handles the ClosingEvent event of the Content control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ContentClosingEventArgs"/> instance containing the event data.</param>
		private void Content_ClosingEvent(object sender, ContentClosingEventArgs e)
		{
			IContent content = CurrentContent ?? _noContent.Item1;

			_log.Print("ContentService: User destroying content panel for content '{0}'", LoggingLevel.Verbose, content.Name);

			if (e.Action != ConfirmationResult.Yes)
			{
				return;
			}

			// TODO: Do saving here.
		}

		/// <summary>
		/// Function to close and clean up any current content to allow for new content to be loaded.
		/// </summary>
		/// <returns>
		/// TRUE if the content was closed, FALSE if cancelled.
		/// </returns>
		public bool CloseContent()
		{
			Tuple<IContent, IContentPanel> content = _currentContent ?? _noContent;

			if ((content == null) || (content.Item1 == null))
			{
				return true;
			}
			
			if (content.Item2 != null)
			{
				// Turn off the events, we don't need them here.
				content.Item2.ContentClosing -= Content_ClosingEvent;
				content.Item2.ContentClosed -= Content_ClosedEvent;

				// If we have changes, then ensure that we save them first.
				if (content.Item1.HasChanges)
				{
					ConfirmationResult result = content.Item2.GetCloseConfirmation();

					if (result == ConfirmationResult.Cancel)
					{
						return false;
					}

					if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
					{
						// TODO: Perform saving.
					}
				}

				_log.Print("ContentService: Destroying content panel for content '{0}'", LoggingLevel.Verbose, content.Item1.Name);
				content.Item2.Close();
			}

			_log.Print("ContentService: Destroying content '{0}'.", LoggingLevel.Verbose, content.Item1.Name);
			content.Item1.Dispose();

			// Disable the current content.
			_currentContent = null;
			_noContent = null;

			return true;
		}

		/// <summary>
		/// Function to create a content object for editing.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when attempting to create a content object when content already exists in the editor.</exception>
		public void CreateContent()
		{
			if ((CurrentContent != null) || (_noContent != null))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_ALREADY_HAVE_CONTENT);	
			}

			// TODO: Search for the appropriate content.
			// TODO: For now, just load default content.
			// if (param == null) then do the lines below and return, otherwise create the content as normal.
			// {
			IContent content = new NoContent
			                   {
				                   Name = "DefaultContent"
			                   };
			IContentPanel contentUI = new ContentPanel(content, new NoContentRenderer(_graphicsProxy.Item, _settings, content))
			                          {
				                          CaptionVisible = false,
										  Name = "DefaultContentPanel"
			                          };
			_log.Print("ContentService: Loading content '{0}'.", LoggingLevel.Verbose, content.Name);

			contentUI.ContentClosing += Content_ClosingEvent;
			contentUI.ContentClosed += Content_ClosedEvent;

			_noContent = new Tuple<IContent, IContentPanel>(content, contentUI);

			var panelCreatedArgs = new ContentCreatedEventArgs(_noContent.Item1, _noContent.Item2);
			if (ContentCreated != null)
			{
				ContentCreated(this, panelCreatedArgs);

				 _noContent.Item2.Renderer.StartRendering();
			}
			// return;
			// }
		}
		#endregion
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				Tuple<IContent, IContentPanel> content = _currentContent ?? _noContent;

				if (content != null)
				{
					if (content.Item2 != null)
					{
						_noContent.Item2.ContentClosing -= Content_ClosingEvent;
						_noContent.Item2.ContentClosed -= Content_ClosedEvent;
						_noContent.Item2.Dispose();
					}

					if (content.Item1 != null)
					{
						_noContent.Item1.Dispose();
					}
				}
			}

			_noContent = null;
			_currentContent = null;
			_disposed = true;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);	
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
