#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 28, 2019 12:15:14 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.UI;

namespace Gorgon.Editor
{
    /// <summary>
    /// A folder browser for the project file system.
    /// </summary>
    internal partial class FormFileSystemFolderBrowser
        : Form, IDataContext<IFileExplorer>
    {
        #region Variables.
        // The currently active directory.
        private IDirectory _currentDirectory;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the description to display on the browser.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description
        {
            get => FolderBrowser.Text;
            set => FolderBrowser.Text = value;
        }

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFileExplorer DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the currently selected directory.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentDirectory => FolderBrowser.CurrentDirectory;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the current directory as an <see cref="IDirectory"/> object.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>The <see cref="IDirectory"/>, or <b>null</b> if the directory was not found.</returns>
        private IDirectory GetDirectory(string path)
        {
            var args = new GetDirectoryArgs(path);
            if ((DataContext?.GetDirectoryCommand is null) || (!DataContext.GetDirectoryCommand.CanExecute(args)))
            {
                return null;
            }

            DataContext.GetDirectoryCommand.Execute(args);
            return args.Directory;
        }

        /// <summary>Fnuction called when deleting a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private async void FolderBrowser_FolderDeleting(object sender, FolderDeleteArgs e)
        {
            if (DataContext is null)
            {
                e.Cancel = true;
                return;
            }

            IDirectory dir = GetDirectory(e.DirectoryPath);

            if (dir is null)
            {
                e.Cancel = true;
                return;
            }

            var args = new DeleteArgs(dir.ID);

            if ((DataContext.DeleteDirectoryCommand is null) || (!DataContext.DeleteDirectoryCommand.CanExecute(args)))
            {
                return;
            }

            e.DeleteTask = DataContext.DeleteDirectoryCommand.ExecuteAsync(args);
            await e.DeleteTask;

            e.Cancel = !args.ItemsDeleted;

            // Ensure that the folder browser doesn't go any further since we've handled everything ourselves.
            e.SuppressPrompt = e.DeletionHandled = true;
        }

        /// <summary>Function called when adding a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FolderBrowser_FolderAdding(object sender, FolderAddArgs e)
        {
            if (DataContext is null)
            {
                e.Cancel = true;
                return;
            }

            var args = new CreateDirectoryArgs
            {
                Name = e.DirectoryName,
                ParentDirectory = _currentDirectory ?? DataContext.Root
            };

            if ((DataContext.CreateDirectoryCommand is null) || (!DataContext.CreateDirectoryCommand.CanExecute(args)))
            {
                return;
            }

            DataContext.CreateDirectoryCommand.Execute(args);

            if (args.Directory is null)
            {
                e.Cancel = true;
            }

            e.CreationHandled = true;
        }

        /// <summary>Function called when renaming a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FolderBrowser_FolderRenaming(object sender, FolderRenameArgs e)
        {
            if (DataContext?.RenameDirectoryCommand is null)
            {
                e.Cancel = true;
                return;
            }

            IDirectory prevDir = GetDirectory(e.OldDirectoryPath);

            if (prevDir is null)
            {
                e.Cancel = true;
                return;
            }

            var args = new RenameArgs(e.OldName, e.NewName)
            {
                ID = prevDir.ID
            };

            if (!DataContext.RenameDirectoryCommand.CanExecute(args))
            {
                e.Cancel = true;
                return;
            }
            
            DataContext.RenameDirectoryCommand.Execute(args);

            e.Cancel = args.Cancel;
            e.RenameHandled = true;
        }

        /// <summary>Folders the browser folder selected.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void FolderBrowser_FolderSelected(object sender, FolderSelectedArgs e) => ButtonOk.Enabled = !string.IsNullOrWhiteSpace(FolderBrowser.CurrentDirectory);

        /// <summary>Function called when a directory is selected or entered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void FolderBrowser_FolderEntered(object sender, FolderSelectedArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            ButtonOk.Enabled = !string.IsNullOrWhiteSpace(FolderBrowser.CurrentDirectory);
            _currentDirectory = GetDirectory(e.FolderPath);
        }

        /// <summary>
        /// Function to unassign events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }
        }

        /// <summary>
        /// Function to restore the control back to its original state.
        /// </summary>
        private void ResetDataContext()
        {
            _currentDirectory = null;
            FolderBrowser.DirectorySeparator = Path.DirectorySeparatorChar;
            FolderBrowser.RootFolder = null;
            FolderBrowser.AssignInitialDirectory(null);
        }

        /// <summary>
        /// Function to initialize the control from its data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IFileExplorer dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            FolderBrowser.DirectorySeparator = '/';
            FolderBrowser.RootFolder = new DirectoryInfo(dataContext.Root.PhysicalPath);
        }

        /// <summary>Raises the Load event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ButtonOk.Enabled = !string.IsNullOrWhiteSpace(FolderBrowser.CurrentDirectory);
        }

        /// <summary>
        /// Function to set the initial path for the folder selector.
        /// </summary>
        /// <param name="directory">The node to use as the initial path.</param>
        public void SetInitialPath(IDirectory directory)
        {
            if (directory is null)
            {
                return;
            }

            _currentDirectory = directory;

            FolderBrowser.AssignInitialDirectory(new DirectoryInfo(directory.PhysicalPath));
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFileExplorer dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormFileSystemFolderBrowser"/> class.</summary>
        public FormFileSystemFolderBrowser() => InitializeComponent();
        #endregion
    }
}
