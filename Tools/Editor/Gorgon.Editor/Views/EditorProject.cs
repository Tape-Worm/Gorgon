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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Docking;
using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Workspace;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

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
        // The preview panel unique ID.
        private const string PreviewID = "__PREVIEW_ID__ZjCHftPl-E1bNRQRc64B3hIFkQXDuZNiIeWbDsP6PpzOl87LIzgrMStLNUtizZZW";
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
        // The page used to host the preview window.
        private KryptonPage _previewPage;
        // The drag drop handler to use for handling content files dropped on to the project surface.
        private Olde_IDragDropHandler<OLDE_IContentFile> _dragDropHandler;
        // The current content control.
        private OLDE_ContentBaseControl _contentControl;
        // The graphics context for the application.
        private IGraphicsContext _graphicsContext;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the application graphics context.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGraphicsContext GraphicsContext
        {
            get => _graphicsContext;
            set => _graphicsContext = Preview.GraphicsContext = value;
        }

        /// <summary>
        /// Property to set or return the main ribbon interface on the parent control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KryptonRibbon MainRibbon
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProjectVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the content file contained in drag data.
        /// </summary>
        /// <param name="data">The drag/drop data.</param>
        /// <returns>The content file if dropping is allowed, or <b>null</b> if not.</returns>
        private OLDE_IContentFile GetContentFileDragData(IDataObject data)
        {
            Type dataType = typeof(TreeNodeDragData);

            if ((_dragDropHandler == null)
                || (data == null))
            {
                return null;
            }

            if (!data.GetDataPresent(dataType))
            {
                dataType = typeof(ListViewItemDragData);

                if (!data.GetDataPresent(dataType))
                {
                    return null;
                }
            }

            var dragData = (IFileExplorerNodeDragData)data.GetData(dataType);

            return dragData?.Node == null ? null : dragData.Node as OLDE_IContentFile;

        }

        /// <summary>Handles the DragDrop event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelContent_DragDrop(object sender, DragEventArgs e)
        {
            if (_dragDropHandler == null)
            {
                return;
            }

            OLDE_IContentFile contentFile = GetContentFileDragData(e.Data);

            if (contentFile == null)
            {
                return;
            }

            _dragDropHandler.Drop(contentFile);
        }

        /// <summary>Handles the DragEnter event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelContent_DragEnter(object sender, DragEventArgs e)
        {
            if (_dragDropHandler == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            OLDE_IContentFile contentFile = GetContentFileDragData(e.Data);

            if (!_dragDropHandler.CanDrop(contentFile))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>Handles the DockspaceAdding event of the Dock control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DockspaceEventArgs"/> instance containing the event data.</param>
        private void Dock_DockspaceAdding(object sender, DockspaceEventArgs e)
        {
            e.DockspaceControl.Width = 384;
            e.DockspaceControl.Height = 384;
        }

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
            switch (e.PropertyName)
            {
                case nameof(IProjectVm.CurrentContent):
                    if (_contentControl != null)
                    {
                        _contentControl.BubbleDragEnter -= ContentControl_BubbleDragEnter;
                        _contentControl.BubbleDragDrop -= ContentControl_BubbleDragDrop;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to reset the view to its default state when the data context is reset.
        /// </summary>
        private void ResetDataContext()
        {
            FileExplorer.SetDataContext(null);
            Preview.SetDataContext(null);
            _dragDropHandler = null;
        }

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
                _contentControl = null;
                return;
            }

            // Get our view from the content.
            _contentControl = ViewFactory.CreateView<OLDE_ContentBaseControl>(dataContext.CurrentContent);
            ViewFactory.AssignViewModel(dataContext.CurrentContent, _contentControl);
            _contentControl.Dock = DockStyle.Fill;
            PanelContent.Controls.Add(_contentControl);

            // Set up the graphics context.
            _contentControl.SetupGraphics(GraphicsContext);

            if (_contentControl.Ribbon != null)
            {
                var args = new ContentRibbonEventArgs(_contentControl.Ribbon);
                EventHandler<ContentRibbonEventArgs> handler = RibbonAdded;
                handler?.Invoke(this, args);

                _currentContentRibbon = _contentControl.Ribbon;
            }

            _contentControl.Start();

            _contentControl.BubbleDragEnter += ContentControl_BubbleDragEnter;
            _contentControl.BubbleDragDrop += ContentControl_BubbleDragDrop;
        }

        /// <summary>Handles the BubbleDragDrop event of the ContentControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ContentControl_BubbleDragDrop(object sender, DragEventArgs e) => PanelContent_DragDrop(sender, e);

        /// <summary>Handles the BubbleDragEnter event of the ContentControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ContentControl_BubbleDragEnter(object sender, DragEventArgs e) => PanelContent_DragEnter(sender, e);

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
            Preview.SetDataContext(dataContext.ContentPreviewer);
            _dragDropHandler = dataContext as Olde_IDragDropHandler<OLDE_IContentFile>;
        }

        /// <summary>
        /// Function to unassign the data context events.
        /// </summary>
        private void UnassignEvents()
        {
            if (_contentControl != null)
            {
                _contentControl.BubbleDragEnter -= ContentControl_BubbleDragEnter;
                _contentControl.BubbleDragDrop -= ContentControl_BubbleDragDrop;
            }

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
                DataContext.ClipboardContext = DataContext.FileExplorer as Olde_IClipboardHandler;
            }
        }


        /// <summary>Handles the Leave event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_Leave(object sender, EventArgs e)
        {
            if ((DataContext != null) && (DataContext.FileExplorer == DataContext.ClipboardContext))
            {
                DataContext.ClipboardContext = null;
            }
        }

        /// <summary>Handles the Enter event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelContent_Enter(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.ClipboardContext = DataContext.CurrentContent as Olde_IClipboardHandler;
            }
        }

        /// <summary>Handles the Leave event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelContent_Leave(object sender, EventArgs e)
        {
            if ((DataContext?.CurrentContent is Olde_IClipboardHandler currentHandler) && (currentHandler == DataContext.ClipboardContext))
            {
                DataContext.ClipboardContext = null;
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
            catch (Exception)
            {
                // We don't care if this fails.
            }
        }

        /// <summary>
        /// Function to assign the default controls to the specified pages.
        /// </summary>
        private void AssignControlsToPages()
        {

            if (Controls.Contains(PanelContent))
            {
                Controls.Remove(PanelContent);
            }
            if (Controls.Contains(FileExplorer))
            {
                Controls.Remove(FileExplorer);
            }

            _contentPage.Controls.Add(PanelContent);
            PanelContent.Dock = DockStyle.Fill;
            _fileSystemPage.Controls.Add(FileExplorer);
            FileExplorer.Dock = DockStyle.Fill;
            _previewPage.Controls.Add(Preview);
            Preview.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Function to create the default pages.
        /// </summary>
        private void CreateDefaultPages()
        {
            _contentPage?.Dispose();
            _fileSystemPage?.Dispose();
            _previewPage?.Dispose();

            // Setup content area.
            _contentPage = new KryptonPage
            {
                Name = ContentID,
                Text = "Content Panel",
                TextTitle = "Content Panel",
                UniqueName = ContentID
            };
            _contentPage.ClearFlags(KryptonPageFlags.DockingAllowAutoHidden |
                KryptonPageFlags.DockingAllowDocked |
                KryptonPageFlags.AllowPageDrag |
                KryptonPageFlags.AllowPageReorder |
                KryptonPageFlags.DockingAllowDropDown |
                KryptonPageFlags.DockingAllowFloating |
                KryptonPageFlags.DockingAllowNavigator);

            // Setup file explorer area.
            _fileSystemPage = new KryptonPage
            {
                Name = FileSystemID,
                Text = Resources.GOREDIT_TEXT_FILESYSTEM,
                TextTitle = Resources.GOREDIT_TEXT_FILESYSTEM,
                UniqueName = FileSystemID
            };

            _previewPage = new KryptonPage
            {
                Name = PreviewID,
                Text = Resources.GOREDIT_TEXT_PREVIEW,
                TextTitle = Resources.GOREDIT_TEXT_PREVIEW,
                UniqueName = PreviewID,
            };

            _fileSystemPage.ClearFlags(KryptonPageFlags.DockingAllowClose | KryptonPageFlags.DockingAllowDropDown | KryptonPageFlags.DockingAllowWorkspace);
            _previewPage.ClearFlags(KryptonPageFlags.DockingAllowClose | KryptonPageFlags.DockingAllowDropDown | KryptonPageFlags.DockingAllowWorkspace);
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
                DockManager.CellsWorkspace[0].NavigatorMode = NavigatorMode.Panel;
            }
            catch
            {
                if (_contentPage.Controls.Contains(PanelContent))
                {
                    _contentPage.Controls.Remove(PanelContent);
                }

                if (_fileSystemPage.Controls.Contains(FileExplorer))
                {
                    _fileSystemPage.Controls.Remove(FileExplorer);
                }

                if (_previewPage.Controls.Contains(Preview))
                {
                    _previewPage.Controls.Remove(Preview);
                }

                DockManager.RemoveAllPages(false);

                _contentPage?.Dispose();
                _fileSystemPage?.Dispose();
                _previewPage?.Dispose();

                // We don't care if this fails.
                CreateDefaultDockingScheme();
            }
        }

        /// <summary>
        /// Function to build up the default docking scheme.
        /// </summary>
        private void CreateDefaultDockingScheme()
        {
            CreateDefaultPages();

            AssignControlsToPages();

            // Link to dock manager.
            DockManager.AddToWorkspace("MainWorkspace", new[] { _contentPage });
            KryptonDockingDockspace dockSpace = DockManager.AddDockspace("Control", DockingEdge.Right, new[] { _fileSystemPage });
            var previewCell = new KryptonWorkspaceCell("1*,384");
            previewCell.Pages.Add(_previewPage);
            dockSpace.DockspaceControl.Root.Orientation = Orientation.Vertical;
            dockSpace.DockspaceControl.Root.Children.Add(previewCell);

            // Update the main workspace area to hide the tab and buttons.
            DockManager.CellsWorkspace[0].NavigatorMode = NavigatorMode.Panel;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((IsDesignTime)
                || (!_deferDataContextLoad)
                || (Disposing))
            {
                return;
            }

            KryptonDockingWorkspace mainWorkspace = DockManager.ManageWorkspace("MainWorkspace", DockSpace);
            DockManager.ManageControl("Control", this, mainWorkspace);
            DockManager.ManageFloating(ParentForm);
            DockManager.DefaultCloseRequest = DockingCloseRequest.None;

            CreateDefaultDockingScheme();

            DataContext?.OnLoad();
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

            if ((DataContext.Layout != null) && (DataContext.Layout.Length != 0))
            {
                LoadLayoutFromDataContext();
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
