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
// Created: Wednesday, October 16, 2013 4:20:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Views available to the file view.
	/// </summary>
	public enum FileViews
	{
		/// <summary>
		/// List of files.
		/// </summary>
		List = 0,
		/// <summary>
		/// Details for the files.
		/// </summary>
		Details = 1,
		/// <summary>
		/// Large icons for the files.
		/// </summary>
		Large = 2
	}

	/// <summary>
	/// A file browser interface to allow the selection of files inside of the currently open file system.
	/// </summary>
	public class EditorFileBrowser
		: Component
	{
		#region Variables.
		private string _defaultExtension;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of file name extensions to filter on.
		/// </summary>
		/// <remarks>Clear this list to retrieve all files.</remarks>
		[Browsable(false)]
		public GorgonFileExtensionCollection FileExtensions
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the default extension for the filter.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_BEHAVIOR"), LocalDescription(typeof(Resources), "PROP_DEFAULTEXTENSION_DESC")]
		public string DefaultExtension
		{
			get
			{
				return _defaultExtension;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}

				if (value.StartsWith("."))
				{
					value = value.Substring(1);
				}

				_defaultExtension = value;
			}
		}

		/// <summary>
		/// Property to set or return the caption text for the dialog.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_TEXT_DESC")]
		public string Text
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to show the dialog on the screen.
		/// </summary>
		/// <param name="owner">[Optional] The window that owns this window.</param>
		/// <returns>A dialog result from the window.</returns>
		/// <remarks>Depending on which button was clicked, either DialogResult.OK or DialogResult.Cancel will be returned from this method.</remarks>
		public DialogResult ShowDialog(IWin32Window owner = null)
		{
			formEditorFileSelector selector = null;

			try
			{
				// TODO: Initialization.
				selector = new formEditorFileSelector(FileExtensions)
				           {
					           Text = Text,
							   DefaultExtension = DefaultExtension
				           };

				return selector.ShowDialog(owner);
			}
			finally
			{
				if (selector != null)
				{
					selector.Dispose();
				}
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileBrowser"/> class.
		/// </summary>
		public EditorFileBrowser()
		{
			FileExtensions = new GorgonFileExtensionCollection();
		}
		#endregion
	}
}
