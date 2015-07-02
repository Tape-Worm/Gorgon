#region MIT.
//  
// Gorgon
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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
// ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Created: 03/02/2015 9:45 PM
// 
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// A content panel to give content a UI for users to work with.
	/// </summary>
	public interface IContentPanel
		: IDisposable
	{
		#region Events.
		/// <summary>
		/// Event triggered when content is closing.
		/// </summary>
		/// <remarks>
		/// This event takes a cancel flag argument as an event parameter. If the user chooses to cancel closing the content, then the cancel flag will be true.
		/// </remarks>
		event EventHandler<GorgonCancelEventArgs> ContentClosing;

		/// <summary>
		/// Event triggered when the content is closed.
		/// </summary>
		event EventHandler ContentClosed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current theme for the application.
		/// </summary>
		GorgonFlatFormTheme CurrentTheme
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the caption for the content panel is visible or not.
		/// </summary>
		bool CaptionVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the dock style for the control.
		/// </summary>
		DockStyle Dock
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the padding around the control.
		/// </summary>
		Padding Padding
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the control uses an external renderer.
		/// </summary>
		bool UsesRenderer
		{
			get;
		}

		/// <summary>
		/// Property to return the renderer bound to this control.
		/// </summary>
		IEditorContentRenderer Renderer
		{
			get;
		}
		
		/// <summary>
		/// Property to set or return the control that will receive rendering.
		/// </summary>
		/// <remarks>When this property is set to NULL (<i>Nothing</i> in VB.Net), then the content area of this control will be used for rendering.</remarks>
		Control RenderControl
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to provide an action to perform when closing the content.
		/// </summary>
		/// <returns>The action to perform when closing the content.</returns>
		/// <remarks>
		/// The return value must one of the <see cref="ConfirmationResult"/> values. Return "yes" to ensure the content is saved, "no" to skip saving, 
		/// "cancel" to tell the application to stop the operation, or "none" to continue ("no" and "none" pretty much do the same thing).
		/// </remarks>
		ConfirmationResult GetCloseConfirmation();
		
		/// <summary>
		/// Function called when the close button is clicked or when the <see cref="Close"/> method is called.
		/// </summary>
		/// <remarks>
		/// This method will trigger the <see cref="ContentClosing"/> and <see cref="ContentClosed"/> events.
		/// </remarks>
		void Close();
		#endregion
	}
}