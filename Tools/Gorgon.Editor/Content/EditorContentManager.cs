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
using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Editor
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
		public IContentData Content
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
		public ContentCreatedEventArgs(IContentData content, IContentPanel panel)
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
		private readonly GorgonLogFile _log;
		// The settings object for the default content.
		private readonly IEditorSettings _settings;
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Currently displayed content.
		private IContentModel _currentContent;
		// Object and UI for the default content.
		private IContentModel _noContentService;
		// The graphics service interface for content.
		private readonly IGraphicsService _graphicsService;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorContentManager"/> class.
		/// </summary>
		/// <param name="log">The application log file.</param>
		/// <param name="settings">The application settings to pass to the default content.</param>
		/// <param name="graphicsService">The graphics service interface used to retrieve the application graphics object instance.</param>
		/// <param name="noContentService">The content service used to create objects for use when no actual content is loaded.</param>
		public EditorContentManager(GorgonLogFile log, IEditorSettings settings, IGraphicsService graphicsService, IContentModel noContentService)
		{
			_log = log;
			_graphicsService = graphicsService;
			_settings = settings;
			_noContentService = noContentService;
		}
		#endregion

		#region IContentManager
		#region Methods.
		/*/// <summary>
		/// Handles the ClosedEvent event of the Content control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Content_ClosedEvent(object sender, EventArgs e)
		{
			IContentData content = CurrentContent ?? _noContent.Item1;
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
		/// <param name="e">The <see cref="GorgonCancelEventArgs"/> instance containing the event data.</param>
		private void Content_ClosingEvent(object sender, GorgonCancelEventArgs e)
		{
			Tuple<IContentData, IContentPanel> content = _currentContent ?? _noContent;

			_log.Print("ContentService: User destroying content panel for content '{0}'", LoggingLevel.Verbose, content.Item1.Name);

			var result = ConfirmationResult.None;
			
			if (content.Item1.HasChanges)
			{
				result = content.Item2.GetCloseConfirmation();

				if (result == ConfirmationResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}
			
			if (result == ConfirmationResult.Yes)
			{
				return;
			}

			// TODO: Do saving here.
		}*/

		/// <summary>
		/// Function to create a content object for editing.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when attempting to create a content object when content already exists in the editor.</exception>
		public IContentModel CreateContent()
		{
			// TODO: Search for the appropriate content.
			// TODO: For now, just load default content.
			// if (param == null) then do the lines below and return, otherwise create the content as normal.
			// {
			_currentContent = _noContentService;
			// }

			_log.Print("ContentService: Loading content '{0}'.", LoggingLevel.Verbose, _currentContent.Content.Name);
	
			return _currentContent;
		}
		#endregion
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_noContentService != null)
				{
					_noContentService.Dispose();
				}

				_noContentService = null;
			}

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
