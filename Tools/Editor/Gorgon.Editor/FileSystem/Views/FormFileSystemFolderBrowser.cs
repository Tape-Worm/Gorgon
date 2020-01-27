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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Editor.Properties;
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
        // A cache of file system directories.
        private readonly Dictionary<string, IDirectory> _directories = new Dictionary<string, IDirectory>(StringComparer.OrdinalIgnoreCase);
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

        /// <summary>Fnuction called when deleting a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private async void FolderBrowser_FolderDeleting(object sender, FolderDeleteArgs e)
        {
            if (DataContext == null)
            {
                e.Cancel = true;
                return;
            }

            IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, e.DirectoryPath, StringComparison.OrdinalIgnoreCase));

            if (dir == null)
            {
                e.Cancel = true;
                return;
            }

            var args = new DeleteArgs(dir.ID);

            if ((DataContext.DeleteDirectoryCommand == null) || (!DataContext.DeleteDirectoryCommand.CanExecute(args)))
            {
                return;
            }

            await DataContext.DeleteDirectoryCommand.ExecuteAsync(args);

            e.Cancel = !args.ItemsDeleted;

            if (!e.Cancel)
            {
                _directories.Remove(dir.ID);
            }

            e.SuppressPrompt = true;
            e.DeletionHandled = true;
        }

        /// <summary>Function called when adding a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FolderBrowser_FolderAdding(object sender, FolderAddArgs e)
        {
            if (DataContext == null)
            {
                e.Cancel = true;
                return;
            }

            IDirectory parentDir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, e.ParentPath, StringComparison.OrdinalIgnoreCase));

            if (parentDir == null)
            {
                parentDir = DataContext.Root;
            }

            var args = new CreateDirectoryArgs
            {
                Name = e.DirectoryPath,
                ParentDirectory = parentDir
            };

            if ((DataContext.CreateDirectoryCommand == null) || (!DataContext.CreateDirectoryCommand.CanExecute(args)))
            {
                return;
            }

            DataContext.CreateDirectoryCommand.Execute(args);

            if (args.Directory == null)
            {
                e.Cancel = true;
                e.CreationHandled = true;
                return;
            }

            _directories[args.Directory.ID] = args.Directory;
            e.CreationHandled = true;
        }

        /// <summary>Function called when renaming a folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FolderBrowser_FolderRenaming(object sender, FolderRenameArgs e)
        {
            if (DataContext?.RenameDirectoryCommand == null)
            {
                e.Cancel = true;
                return;
            }

            IDirectory prevDir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, e.OldDirectoryPath, StringComparison.OrdinalIgnoreCase));

            if (prevDir == null)
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

        /// <summary>Function called when a directory is selected or entered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void FolderBrowser_FolderEntered(object sender, FolderSelectedArgs e)
        {
            ButtonOk.Enabled = !string.IsNullOrWhiteSpace(FolderBrowser.CurrentDirectory);

            IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, FolderBrowser.CurrentDirectory, StringComparison.OrdinalIgnoreCase));
            _directories.Clear();

            if (dir == null)
            {
                return;
            }

            foreach (IDirectory subdir in dir.Directories)
            {
                _directories[subdir.ID] = subdir;
            }
        }

        /// <summary>
        /// Function to unassign events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }
        }

        /// <summary>
        /// Function to restore the control back to its original state.
        /// </summary>
        private void ResetDataContext()
        {
            _directories.Clear();
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
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            FolderBrowser.DirectorySeparator = '/';
            FolderBrowser.RootFolder = new DirectoryInfo(dataContext.Root.PhysicalPath);

            _directories.Clear();
            foreach (IDirectory dir in dataContext.Root.Directories)
            {
                _directories[dir.ID] = dir;
            }
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
            _directories.Clear();

            if (directory == null)
            {
                return;
            }

            FolderBrowser.AssignInitialDirectory(new DirectoryInfo(directory.PhysicalPath));
            
            foreach (IDirectory dir in directory.Directories)
            {
                _directories[dir.ID] = dir;
            }                
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
