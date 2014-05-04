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

using System.ComponentModel;
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
		/// Details for the files.
		/// </summary>
		Details = 0,
		/// <summary>
		/// Large icons for the files.
		/// </summary>
		Large = 1
	}

	/// <summary>
	/// A file browser interface to allow the selection of files inside of the currently open file system.
	/// </summary>
	public class EditorFileDialog
		: Component
	{
		#region Variables.
		private string _defaultExtension = string.Empty;
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
	    /// Property to set or return whether to allow multiple selections.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_BEHAVIOR")]
	    [LocalDescription(typeof(APIResources), "PROP_MULTISELECT_DESC")]
	    [DefaultValue(false)]
	    public bool MultipleSelection
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to set or return the default view for the dialog.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_APPEARANCE")]
	    [LocalDescription(typeof(APIResources), "PROP_FILEVIEW_DESC")]
	    [DefaultValue(typeof(FileViews), "Details")]
	    public FileViews FileView
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to set or return the starting directory for the dialog.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_DATA")]
	    [LocalDescription(typeof(APIResources), "PROP_STARTDIRECTORY_DESC")]
	    public string StartDirectory
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to set or return the default extension for the filter.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_BEHAVIOR")]
	    [LocalDescription(typeof(APIResources), "PROP_DEFAULTEXTENSION_DESC")]
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
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_APPEARANCE")]
	    [LocalDescription(typeof(APIResources), "PROP_TEXT_DESC")]
	    public string Text
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to return the selected file name.
	    /// </summary>
	    /// <remarks>If there are multiple file names selected, then this will return the first name on the <see cref="Files">file array</see>.</remarks>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_DATA")]
	    [LocalDescription(typeof(APIResources), "PROP_FILENAME_DESC")]
	    public string Filename
	    {
	        get;
	        set;
	    }

	    /// <summary>
        /// Property to return the array of selected file names.
        /// </summary>
        [Browsable(false)]
	    public GorgonFileSystemFileEntry[] Files
	    {
	        get;
            private set;
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
	        using(var selector = new FormEditorFileSelector(FileExtensions))
	        {
	            selector.Text = Text;
	            selector.DefaultExtension = DefaultExtension;
	            selector.CurrentView = FileView;
	            selector.StartDirectory = StartDirectory;
	            selector.AllowMultipleSelection = MultipleSelection;
	            selector.DefaultFilename = Filename;

	            DialogResult result = selector.ShowDialog(owner);

				FileView = selector.CurrentView;

		        if (result != DialogResult.OK)
		        {
			        return result;
		        }

		        DefaultExtension = selector.DefaultExtension;
		        Files = selector.GetFilenames();
		        Filename = Files.Length > 0 ? Files[0].FullPath : string.Empty;

		        return result;
	        }
	    }
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileDialog"/> class.
		/// </summary>
		public EditorFileDialog()
		{
			FileExtensions = new GorgonFileExtensionCollection();
			Text = APIResources.GOREDIT_TEXT_SELECT_FILE;
		}
		#endregion
	}
}
