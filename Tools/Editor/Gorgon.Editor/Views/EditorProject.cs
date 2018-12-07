#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: September 4, 2018 11:09:28 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ComponentFactory.Krypton.Docking;
using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Workspace;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.IO;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The view used for editing projects.
    /// </summary>
    internal partial class EditorProject 
        : EditorBaseControl, IDataContext<IProjectVm>
    {
        #region Constants.
        // The file system panel unique ID.
        private const string FileSystemID = "__FS_ID__QaIT-CQPZVwp4bV1zgyzp76KKy-et4Ft0sza971XT9IONmXAsTPQdqLk3OkCvCIB";
        // The content panel unique ID.
        private const string ContentID = "__CONTENT_ID__6jBgPRNH8W5BeUa3q-TCzJ8MSQiIZJq-dKMR2WMDLhOApFe3MbgPKMR9DtUTuspb";
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonAdded;

        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonRemoved;

        /// <summary>
        /// Event used to indicate that a rename operation has started.
        /// </summary>
        [Category("File system")]
        public event EventHandler RenameBegin
        {
            add
            {
                FileExplorer.RenameBegin += value;
            }
            remove
            {
                FileExplorer.RenameBegin -= value;
            }
        }

        /// <summary>
        /// Event used to indicate that a rename operation has started.
        /// </summary>
        [Category("File system")]
        public event EventHandler RenameEnd
        {
            add
            {
                FileExplorer.RenameEnd += value;
            }
            remove
            {
                FileExplorer.RenameEnd -= value;
            }
        }
        #endregion

        #region Variables.
        // Flag to indicate that the data context load should be deferred.
        private bool _deferDataContextLoad = true;
        // The current ribbon for the current content.
        private KryptonRibbon _currentContentRibbon;
        // The page used to host the file system explorer.
        private KryptonPage _fileSystemPage;
        // The page used to host the content.
        private KryptonPage _contentPage;
        #endregion
        
        #region Properties.
        /// <summary>
        /// Property to set or return the application graphics context.
        /// </summary>
        [Browsable(false)]
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IProjectVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the DockspaceAdding event of the Dock control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DockspaceEventArgs"/> instance containing the event data.</param>
        private void Dock_DockspaceAdding(object sender, DockspaceEventArgs e) => e.DockspaceControl.Width = 384;

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IProjectVm.FileExplorer):
                    FileExplorer.SetDataContext(DataContext.FileExplorer);
                    break;
                case nameof(IProjectVm.CurrentContent):
                    SetupContent(DataContext);
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanging event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            
        }

        /// <summary>
        /// Function to reset the view to its default state when the data context is reset.
        /// </summary>
        private void ResetDataContext() => FileExplorer.SetDataContext(null);

        /// <summary>
        /// Function to set up the currently active content.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetupContent(IProjectVm dataContext)
        {
            if (_currentContentRibbon != null)
            {                
                var args = new ContentRibbonEventArgs(_currentContentRibbon);
                _currentContentRibbon = null;

                EventHandler<ContentRibbonEventArgs> handler = RibbonRemoved;
                handler?.Invoke(this, args);
            }

            // Remove all controls.
            while (PanelContent.Controls.Count > 0)
            {
                // Leave this here for now, just in case we need it later.
                //var contentControl = SplitProject.Panel1.Controls[SplitProject.Panel1.Controls.Count - 1] as ContentBaseControl;
                PanelContent.Controls[PanelContent.Controls.Count - 1].Dispose();
            }

            // No content, so we can go now.
            if (dataContext?.CurrentContent == null)
            {                
                return;
            }

            // Get our view from the content.
            ContentBaseControl control = ViewFactory.CreateView<ContentBaseControl>(dataContext.CurrentContent);
            ViewFactory.AssignViewModel(dataContext.CurrentContent, control);
            control.Dock = DockStyle.Fill;
            PanelContent.Controls.Add(control);            

            // Set up the graphics context.
            control.SetupGraphics(GraphicsContext);

            if (control.Ribbon != null)
            {
                var args = new ContentRibbonEventArgs(control.Ribbon);
                EventHandler<ContentRibbonEventArgs> handler = RibbonAdded;
                handler?.Invoke(this, args);

                _currentContentRibbon = control.Ribbon;
            }

            control.Start();
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IProjectVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            FileExplorer.SetDataContext(dataContext.FileExplorer);
        }

        /// <summary>
        /// Function to unassign the data context events.
        /// </summary>
        private void UnassignEvents()
        {
            // Close the current content control.
            SetupContent(null);

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;

            SaveLayoutToDataContext();
            DataContext.OnUnload();
        }

        /// <summary>
        /// Handles the Enter event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_Enter(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.ClipboardContext = DataContext.FileExplorer as IClipboardHandler;
                DataContext.UndoContext = DataContext.FileExplorer as IUndoHandler;
            }
        }

        /// <summary>
        /// Function to save the layout to the data context.
        /// </summary>
        private void SaveLayoutToDataContext()
        {
            if (DataContext == null)
            {
                return;
            }

            try
            {
                // The guy who wrote this is useless.
                byte[] fuckedupData = DockManager.SaveConfigToArray(Encoding.UTF8);

                // Sanitize the array.
                byte[] cleanData = fuckedupData.Where(item => item != 0).ToArray();

                DataContext.Layout = cleanData;
            }
            catch(Exception)
            {
                // We don't care if this fails.
            }
        }

        /// <summary>
        /// Function to load the layout from the data context.
        /// </summary>
        private void LoadLayoutFromDataContext()
        {
            if (DataContext?.Layout == null)
            {
                return;
            }

            try
            {                

                DockManager.LoadConfigFromArray(DataContext.Layout);

                if ((DockManager.CellsWorkspace.Length == 0) || (!DockManager.CellsWorkspace[0].Contains(_contentPage)))
                {
                    if (!_contentPage.Controls.Contains(PanelContent))
                    {
                        _contentPage.Controls.Add(PanelContent);
                    }

                    DockManager.AddToWorkspace("Workspace", new[] { _contentPage });
                    // Update the main workspace area to hide the tab and buttons.
                    DockManager.CellsWorkspace[0].NavigatorMode = NavigatorMode.Panel;
                }
            }
            catch
            {
                // We don't care if this fails.
            }
        }

        /// <summary>
        /// Function to build the content page dock panel.
        /// </summary>
        private void CreateContentPageDockPanel()
        {

        }

        /// <summary>
        /// Function to build up the default docking scheme.
        /// </summary>
        private void CreateDefaultDockingScheme()
        {
            DockManager.ManageControl("Control", this, DockManager.ManageWorkspace("Workspace", DockSpace));
            DockManager.ManageFloating(ParentForm);
            DockManager.DefaultCloseRequest = DockingCloseRequest.None;

            // Setup content area.
            _contentPage = new KryptonPage
            {
                Name = ContentID,
                TextTitle = "N/A",
                UniqueName = ContentID,
                Visible = true
            };
            _contentPage.ClearFlags(KryptonPageFlags.All);

            Controls.Remove(PanelContent);
            _contentPage.Controls.Add(PanelContent);
            PanelContent.Dock = DockStyle.Fill;

            // Setup file explorer area.
            _fileSystemPage = new KryptonPage
            {
                Name = FileSystemID,
                TextTitle = Resources.GOREDIT_TEXT_FILESYSTEM,
                UniqueName = FileSystemID,
            };
            _fileSystemPage.ClearFlags(KryptonPageFlags.DockingAllowClose | KryptonPageFlags.DockingAllowDropDown | KryptonPageFlags.DockingAllowWorkspace);

            Controls.Remove(FileExplorer);
            _fileSystemPage.Controls.Add(FileExplorer);
            FileExplorer.Dock = DockStyle.Fill;

            // Link to dock manager.
            DockManager.AddDockspace("Control", DockingEdge.Right, new[] { _fileSystemPage });
            DockManager.AddToWorkspace("Workspace", new[] { _contentPage });           

            // Update the main workspace area to hide the tab and buttons.
            DockManager.CellsWorkspace[0].NavigatorMode = NavigatorMode.Panel;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((IsDesignTime)
                || (!_deferDataContextLoad))
            {
                return;
            }

            CreateDefaultDockingScheme();
            DataContext?.OnLoad();
            LoadLayoutFromDataContext();
        }

        /// <summary>
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
        /// </para>
        /// </remarks>
        public void SetDataContext(IProjectVm dataContext)
        {
            UnassignEvents();

            DataContext = null;
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }

            _deferDataContextLoad = !IsHandleCreated;
            if (!_deferDataContextLoad)
            {
                DataContext.OnLoad();
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorProject"/> class.
        /// </summary>
        public EditorProject() => InitializeComponent();
        #endregion
    }
}
