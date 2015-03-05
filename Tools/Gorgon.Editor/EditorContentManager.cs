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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Event arguments for the content closing event.
	/// </summary>
	class ContentClosingEventArgs
		: GorgonCancelEventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the content object that's closing.
		/// </summary>
		public IContent Content
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentClosingEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content that's closing.</param>
		public ContentClosingEventArgs(IContent content)
			: base(false)
		{
			Content = content;
		}
		#endregion
	}

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
		/// Event called when the content is about to close.
		/// </summary>
		public event EventHandler<ContentClosingEventArgs> ContentClosing;
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
		/// Handles the CloseClickEvent event of the Content control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="gorgonCancelEventArgs">The <see cref="GorgonCancelEventArgs"/> instance containing the event data.</param>
		private void Content_CloseClickEvent(object sender, GorgonCancelEventArgs gorgonCancelEventArgs)
		{
			// If there's a disconnect (shouldn't be), then don't cause us any grief when trying to get rid of the panel.
			if (CurrentContent == null)
			{
				return;
			}

			CurrentContent.Close();
		}

		/// <summary>
		/// Function to disable any content panel events.
		/// </summary>
		/// <param name="panel">Panel to remove events from.</param>
		private void DisablePanelWiring(IContentPanel panel)
		{
			if (panel.CaptionVisible)
			{
				panel.CloseClick -= Content_CloseClickEvent;
			}
		}

		/// <summary>
		/// Function to enable any content panel events.
		/// </summary>
		/// <param name="panel"></param>
		private void EnablePanelWiring(IContentPanel panel)
		{
			if (panel.CaptionVisible)
			{
				panel.CloseClick += Content_CloseClickEvent;
			}
		}

		/// <summary>
		/// Function to create a content object for editing.
		/// </summary>
		public void CreateContent()
		{
			Tuple<IContent, IContentPanel> previousContent = _currentContent ?? _noContent;

			// Disable the current content
			_currentContent = null;

			if ((previousContent != null) && (previousContent.Item1 != null))
			{
				// We will not be removing the UI, the main form will handle that for us when 
				// we add the new UI.  But we must disable any events for the current panel.
				if (previousContent.Item2 != null)
				{
					DisablePanelWiring(previousContent.Item2);
				}

				var closeArgs = new ContentClosingEventArgs(previousContent.Item1);

				if (ContentClosing != null)
				{
					ContentClosing(this, closeArgs);

					// If we cancel, then do not create a new content object.
					if (closeArgs.Cancel)
					{
						return;
					}
				}

				previousContent.Item1.Dispose();
			}
			
			// TODO: Search for the appropriate content.
			// TODO: For now, just load default content.
			// if (param == null) then do the lines below and return, otherwise create the content as normal.
			// {
			IContent defaultContent = new NoContent();
			IContentPanel defaultContentUI = new ContentPanel(defaultContent, new NoContentRenderer(_graphicsProxy.Item, _settings, defaultContent))
			                                 {
				                                 CaptionVisible = false
			                                 };

			_noContent = new Tuple<IContent, IContentPanel>(defaultContent, defaultContentUI);

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
				if (ContentPanel != null)
				{
					DisablePanelWiring(ContentPanel);
					ContentPanel.Dispose();
				}

				if (CurrentContent != null)
				{
					CurrentContent.Dispose();	
				}

				if (_noContent != null)
				{
					if (_noContent.Item1 != null)
					{
						_noContent.Item1.Dispose();
					}

					if (_noContent.Item2 != null)
					{
						_noContent.Item2.Dispose();
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
