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
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using Gorgon.Editor.Content;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The view used for editing projects.
    /// </summary>
    internal partial class ProjectContainer
        : EditorBaseControl, IDataContext<IProjectEditor>
    {
        #region Events.
        // The file explorer context change event.
        private event EventHandler _fileExplorerContextChanged;

        // The file explorer context change event.
        private event EventHandler _fileExplorerIsRenaming;

        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonAdded;

        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonRemoved;

        /// <summary>
        /// Event triggered when the file explorer context changed.
        /// </summary>
        public event EventHandler FileExplorerContextChanged
        {
            add
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        _fileExplorerContextChanged = null;
                        return;
                    }

                    _fileExplorerContextChanged += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    _fileExplorerContextChanged -= value;
                }
            }
        }

        /// <summary>
        /// Event triggered when the file explorer starts or finishes a rename operation.
        /// </summary>
        public event EventHandler FileExplorerIsRenaming
        {
            add
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        _fileExplorerIsRenaming = null;
                        return;
                    }

                    _fileExplorerIsRenaming += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    _fileExplorerIsRenaming -= value;
                }
            }
        }
        #endregion

        #region Variables.
        // Synchronization lock for the file explorer context changed event.
        private readonly object _eventLock = new object();
        // Flag to indicate that the data context load should be deferred.
        private bool _deferDataContextLoad = true;
        // The current ribbon for the current content.
        private KryptonRibbon _currentContentRibbon;
        // The current content control.
        private ContentBaseControl _contentControl;
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
        public IProjectEditor DataContext
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
        private IContentFile GetContentFileDragData(IDataObject data)
        {
#warning FIX ME.
            /*Type dataType = typeof(TreeNodeDragData);

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

            return dragData?.Node == null ? null : dragData.Node as IContentFile;*/
            return null;
        }

        /// <summary>Handles the IsRenamingChanged event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_IsRenamingChanged(object sender, EventArgs e)
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = _fileExplorerIsRenaming;
            }
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the ControlContextChanged event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_ControlContextChanged(object sender, EventArgs e)
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = _fileExplorerContextChanged;
            }
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the DragDrop event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelContent_DragDrop(object sender, DragEventArgs e)
        {
#warning FIX ME.
            /*if (_dragDropHandler == null)
            {
                return;
            }

            IContentFile contentFile = GetContentFileDragData(e.Data);

            if (contentFile == null)
            {
                return;
            }

            _dragDropHandler.Drop(contentFile);*/
        }

        /// <summary>Handles the DragEnter event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelContent_DragEnter(object sender, DragEventArgs e)
        {
#warning FIX ME.
            /*
            if (_dragDropHandler == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            IContentFile contentFile = GetContentFileDragData(e.Data);

            if (!_dragDropHandler.CanDrop(contentFile))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
            */
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
                case nameof(IProjectEditor.ShowFileExplorer):
                    SplitMain.Panel2Collapsed = !DataContext.ShowFileExplorer;
                    break;
                case nameof(IProjectEditor.ShowContentPreview):
                    SplitFileSystem.Panel2Collapsed = !DataContext.ShowContentPreview;
                    break;
                case nameof(IProjectEditor.FileExplorer):
                    FileExplorer.SetDataContext(DataContext.FileExplorer);
                    break;
                case nameof(IProjectEditor.CurrentContent):
                    SetupContent(DataContext);
                    break;
                case nameof(IProjectEditor.FileExplorerDistance):
                    SplitMain.SplitterDistance = (int)(DataContext.FileExplorerDistance * SplitMain.ClientSize.Width);
                    break;
                case nameof(IProjectEditor.PreviewDistance):
                    SplitFileSystem.SplitterDistance = (int)(DataContext.PreviewDistance * SplitFileSystem.ClientSize.Height);
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
                case nameof(IProjectEditor.CurrentContent):
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
        }

        /// <summary>
        /// Function to set up the currently active content.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetupContent(IProjectEditor dataContext)
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
            _contentControl = ViewFactory.CreateView<ContentBaseControl>(dataContext.CurrentContent);
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

        /// <summary>Handles the SplitterMoved event of the SplitMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
        private void SplitMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.FileExplorerDistance = (double)SplitMain.SplitterDistance / SplitMain.ClientSize.Width;
        }

        /// <summary>Handles the SplitterMoved event of the SplitFileSystem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
        private void SplitFileSystem_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.PreviewDistance = (double)SplitFileSystem.SplitterDistance / SplitMain.ClientSize.Height;
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
        private void InitializeFromDataContext(IProjectEditor dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            FileExplorer.SetDataContext(dataContext.FileExplorer);
            Preview.SetDataContext(dataContext.ContentPreviewer);

            SplitMain.Panel2Collapsed = !dataContext.ShowFileExplorer;
            SplitFileSystem.Panel2Collapsed = !dataContext.ShowContentPreview;

            SplitMain.SplitterDistance = (int)(dataContext.FileExplorerDistance * SplitMain.ClientSize.Width);
            SplitFileSystem.SplitterDistance = (int)(dataContext.PreviewDistance * SplitFileSystem.ClientSize.Height);
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
                DataContext.ClipboardContext = DataContext.FileExplorer.Clipboard;
            }
        }


        /// <summary>Handles the Leave event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_Leave(object sender, EventArgs e)
        {
            if ((DataContext != null) && (DataContext.FileExplorer.Clipboard == DataContext.ClipboardContext))
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
                DataContext.ClipboardContext = DataContext.CurrentContent.Clipboard;
            }
        }

        /// <summary>Handles the Leave event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelContent_Leave(object sender, EventArgs e)
        {
            if ((DataContext?.CurrentContent != null) && (DataContext.CurrentContent.Clipboard == DataContext.ClipboardContext))
            {
                DataContext.ClipboardContext = null;
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((IsDesignTime) || (Disposing))
            {
                return;
            }

            if (_deferDataContextLoad)                
            {
                DataContext?.OnLoad();                
            }

            FileExplorer.Select();
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
        public void SetDataContext(IProjectEditor dataContext)
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
                FileExplorer.Select();
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectContainer"/> class.
        /// </summary>
        public ProjectContainer() => InitializeComponent();
        #endregion
    }
}
