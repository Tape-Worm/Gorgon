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
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A content panel to give content a UI for users to work with.
	/// </summary>
	public interface IContentPanel
		: IDisposable
	{
		#region Events.
		/// <summary>
		/// Event fired when the close button is clicked.
		/// </summary>
		event EventHandler<GorgonCancelEventArgs> CloseClick;		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current theme for the application.
		/// </summary>
		FlatFormTheme CurrentTheme
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
		/// <remarks>When this property is set to NULL (Nothing in VB.Net), then the content area of this control will be used for rendering.</remarks>
		Control RenderControl
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		
		#endregion
	}
}