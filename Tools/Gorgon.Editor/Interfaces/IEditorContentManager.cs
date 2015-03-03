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
// Created: Monday, March 2, 2015 11:19:56 PM
// 
#endregion

using System;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Manages the currently active content in the editor.
	/// </summary>
	interface IEditorContentManager
		: IDisposable
	{
		#region Events.
		/// <summary>
		/// Event called when the content is about to close.
		/// </summary>
		event EventHandler<ContentClosingEventArgs> ContentClosing;

		/// <summary>
		/// Event called when the content UI is created.
		/// </summary>
		event EventHandler<ContentCreatedEventArgs> ContentCreated;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the currently active content.
		/// </summary>
		/// <remarks>
		/// When there is no content created/loaded, then this property will return NULL (Nothing in VB.Net) even though 
		/// the "NoContent" content object is loaded in the display.
		/// </remarks>
		IContent CurrentContent
		{
			get;
		}

		/// <summary>
		/// Property to return the content panel.
		/// </summary>
		/// <remarks>
		/// This will return the UI for the content (if available) for use in the editor.
		/// </remarks>
		IContentPanel ContentPanel
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a content object for editing.
		/// </summary>
		void CreateContent();
		#endregion
	}
}