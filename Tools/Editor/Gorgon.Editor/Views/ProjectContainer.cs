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
using System.Threading;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.Math;
using Krypton.Ribbon;

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
        private event EventHandler FileExplorerContextChangedEvent;

        // The file explorer context change event.
        private event EventHandler FileExplorerIsRenamingEvent;

        // The ribbon added event.
        private event EventHandler<ContentRibbonEventArgs> RibbonAddedEvent;

        // The ribbon removed event.
        private event EventHandler<ContentRibbonEventArgs> RibbonRemovedEvent;

        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonAdded
        {
            add
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        RibbonAddedEvent = value;
                        return;
                    }

                    RibbonAddedEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        return;
                    }

                    RibbonAddedEvent -= value;
                }
            }
        }

        /// <summary>
        /// Event triggered when a ribbon is added.
        /// </summary>
        public event EventHandler<ContentRibbonEventArgs> RibbonRemoved
        {
            add
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        RibbonRemovedEvent = value;
                        return;
                    }

                    RibbonRemovedEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        return;
                    }

                    RibbonRemovedEvent -= value;
                }
            }
        }

        /// <summary>
        /// Event triggered when the file explorer context changed.
        /// </summary>
        public event EventHandler FileExplorerContextChanged
        {
            add
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        FileExplorerContextChangedEvent = null;
                        return;
                    }

                    FileExplorerContextChangedEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        return;
                    }

                    FileExplorerContextChangedEvent -= value;
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
                    if (value is null)
                    {
                        FileExplorerIsRenamingEvent = null;
                        return;
                    }

                    FileExplorerIsRenamingEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value is null)
                    {
                        return;
                    }

                    FileExplorerIsRenamingEvent -= value;
                }
            }
        }
        #endregion

        #region Variables.
        // Synchronization lock for the file explorer context changed event.
        private readonly object _eventLock = new();
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
        /// Property to set or return the settings for the application.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EditorSettings Settings
        {
            get => FileExplorer.Settings;
            set => FileExplorer.Settings = value;
        }

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
        /// Function to set the splitter distance.
        /// </summary>
        /// <param name="distance">The distance of the splitter.</param>
        private void SetMainSplitDistance(int distance)
        {
            SplitMain.SplitterMoved -= SplitMain_SplitterMoved;
            try
            {
                SplitMain.SplitterDistance = (int)(((distance.Max(1).Min(99)) / 100.0M) * SplitMain.Width);
            }
            finally
            {
                SplitMain.SplitterMoved += SplitMain_SplitterMoved;
            }
        }

        /// <summary>
        /// Function to set the splitter distance.
        /// </summary>
        /// <param name="distance">The distance of the splitter.</param>
        private void SetFileSystemSplitDistance(int distance)
        {
            SplitFileSystem.SplitterMoved -= SplitFileSystem_SplitterMoved;
            try
            {
                SplitFileSystem.SplitterDistance = (int)(((distance.Max(1).Min(99)) / 100.0M) * SplitFileSystem.Height);
            }
            finally
            {
                SplitFileSystem.SplitterMoved += SplitFileSystem_SplitterMoved;
            }
        }

        /// <summary>
        /// Function to retrieve the content file contained in drag data.
        /// </summary>
        /// <param name="data">The drag/drop data.</param>
        /// <returns>The content file if dropping is allowed, or <b>null</b> if not.</returns>
        private string GetContentFileDragData(IDataObject data)
        {
            if (data is null)
            {
                return null;
            }

            Type dataType = typeof(GridRowsDragData);

            if (!data.GetDataPresent(dataType))
            {
                return null;
            }

            var rows = (GridRowsDragData)data.GetData(dataType);

            return rows.SourceFiles.Count != 1 ? null : rows.SourceFiles[0];
        }        

        /// <summary>Handles the IsRenamingChanged event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_IsRenamingChanged(object sender, EventArgs e)
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = FileExplorerIsRenamingEvent;
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
                handler = FileExplorerContextChangedEvent;
            }
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the DragDrop event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private async void PanelContent_DragDrop(object sender, DragEventArgs e)
        {
            string contentID = GetContentFileDragData(e.Data);

            if ((DataContext?.FileExplorer?.OpenContentFileCommand is null) || (!DataContext.FileExplorer.OpenContentFileCommand.CanExecute(contentID)))
            {
                return;
            }

            await DataContext.FileExplorer.OpenContentFileCommand.ExecuteAsync(contentID);
        }

        /// <summary>Handles the DragEnter event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelContent_DragEnter(object sender, DragEventArgs e)
        {
            string contentID = GetContentFileDragData(e.Data);

            if ((DataContext?.FileExplorer?.OpenContentFileCommand is null) || (!DataContext.FileExplorer.OpenContentFileCommand.CanExecute(contentID)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;
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
                case nameof(IProjectEditor.FileExplorer):
                    FileExplorer.SetDataContext(DataContext.FileExplorer);
                    break;
                case nameof(IProjectEditor.CurrentContent):
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
                case nameof(IProjectEditor.CurrentContent):
                    if (_contentControl is not null)
                    {
                        _contentControl.BubbleDragEnter -= ContentControl_BubbleDragEnter;
                        _contentControl.BubbleDragOver -= ContentControl_BubbleDragOver;
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
            SetupContent(null);
        }

        /// <summary>
        /// Function to set up the currently active content.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetupContent(IProjectEditor dataContext)
        {
            KryptonRibbon ribbon = Interlocked.Exchange(ref _currentContentRibbon, null);
            if (ribbon is not null)
            {
                EventHandler<ContentRibbonEventArgs> handler = null;

                lock (_eventLock)
                {                    
                    handler = RibbonRemovedEvent;                    
                }

                handler?.Invoke(this, new ContentRibbonEventArgs(ribbon));
            }

            // Remove all controls.
            while (PanelContent.Controls.Count > 0)
            {
                if (PanelContent.Controls[^1] is ContentBaseControl contentControl)
                {
                    contentControl.ContentClosed -= ContentClosed;
                }

                PanelContent.Controls[^1]?.Dispose();
            }

            // No content, so we can go now.
            if (dataContext?.CurrentContent is null)
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
            try
            {
                _contentControl.SetupGraphics(GraphicsContext);

                if (_contentControl.Ribbon is not null)
                {
                    if (Interlocked.Exchange(ref _currentContentRibbon, _contentControl.Ribbon) is null)
                    {
                        EventHandler<ContentRibbonEventArgs> handler = null;

                        lock (_eventLock)
                        {
                            handler = RibbonAddedEvent;
                        }

                        handler?.Invoke(this, new ContentRibbonEventArgs(_currentContentRibbon));
                    }
                }

                _contentControl.Start();

                _contentControl.ContentClosed += ContentClosed;
                _contentControl.BubbleDragEnter += ContentControl_BubbleDragEnter;
                _contentControl.BubbleDragOver += ContentControl_BubbleDragOver;
                _contentControl.BubbleDragDrop += ContentControl_BubbleDragDrop;
            }
            catch
            {
                if (_contentControl is not null)
                {
                    _contentControl.ContentClosed -= ContentClosed;
                    _contentControl.BubbleDragEnter -= ContentControl_BubbleDragEnter;
                    _contentControl.BubbleDragOver -= ContentControl_BubbleDragOver;
                    _contentControl.BubbleDragDrop -= ContentControl_BubbleDragDrop;

                    _contentControl.Stop();

                    PanelContent.Controls.Remove(_contentControl);
                    _contentControl.Dispose();
                }
                throw;
            }
        }

        /// <summary>Handles the SplitterMoved event of the SplitMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
        private void SplitMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            Settings.SplitMainDistance = ((int)((SplitMain.SplitterDistance / (decimal)SplitMain.Width) * 100.0M).Round()).Max(1).Min(99);
        }

        /// <summary>Handles the SplitterMoved event of the SplitFileSystem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
        private void SplitFileSystem_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            Settings.SplitPreviewDistance = ((int)((SplitFileSystem.SplitterDistance / (decimal)SplitFileSystem.Height) * 100.0M).Round()).Max(1).Min(99);
        }

        /// <summary>Handles the BubbleDragDrop event of the ContentControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ContentControl_BubbleDragDrop(object sender, DragEventArgs e) => PanelContent_DragDrop(sender, e);

        /// <summary>Handles the BubbleDragEnter event of the ContentControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ContentControl_BubbleDragEnter(object sender, DragEventArgs e) => PanelContent_DragEnter(sender, e);

        /// <summary>Handles the BubbleDragOver event of the ContentControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ContentControl_BubbleDragOver(object sender, DragEventArgs e) => PanelContent_DragEnter(sender, e);

        /// <summary>Function called when the currently active content is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ContentClosed(object sender, EventArgs e)
        {
            if ((DataContext?.ContentClosedCommand is null) || (!DataContext.ContentClosedCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ContentClosedCommand.Execute(null);
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IProjectEditor dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            FileExplorer.SetDataContext(dataContext.FileExplorer);
            Preview.SetDataContext(dataContext.ContentPreviewer);

            SplitMain.Panel2Collapsed = !Settings.ShowFileExplorer;
            SplitFileSystem.Panel2Collapsed = !Settings.ShowContentPreview;

            SplitMain.Panel1MinSize = (ClientSize.Width / 2).Max(320);
            SplitMain.Panel2MinSize = (ClientSize.Width / 6).Max(320);

            SplitFileSystem.Panel1MinSize = (ClientSize.Height / 2).Max(320);
            SplitFileSystem.Panel2MinSize = 256;

            SetMainSplitDistance(Settings.SplitMainDistance);
            SetFileSystemSplitDistance(Settings.SplitPreviewDistance);
        }

        /// <summary>
        /// Function to unassign the data context events.
        /// </summary>
        private void UnassignEvents()
        {
            ContentBaseControl currentContent = Interlocked.Exchange(ref _contentControl, null);

            if (currentContent is not null)
            {
                currentContent.ContentClosed -= ContentClosed;
                currentContent.BubbleDragOver -= ContentControl_BubbleDragOver;
                currentContent.BubbleDragEnter -= ContentControl_BubbleDragEnter;
                currentContent.BubbleDragDrop -= ContentControl_BubbleDragDrop;
                currentContent = null;
            }

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;

            DataContext.Unload();
        }

        /// <summary>
        /// Handles the Enter event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_Enter(object sender, EventArgs e)
        {
            if (DataContext is not null)
            {
                DataContext.ClipboardContext = DataContext.FileExplorer.Clipboard;
            }
        }


        /// <summary>Handles the Leave event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_Leave(object sender, EventArgs e)
        {
            if ((DataContext is not null) && (DataContext.FileExplorer.Clipboard == DataContext.ClipboardContext))
            {
                DataContext.ClipboardContext = null;
            }
        }

        /// <summary>Handles the Enter event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelContent_Enter(object sender, EventArgs e)
        {
            if (DataContext is not null)
            {
                DataContext.ClipboardContext = DataContext.CurrentContent.Clipboard;
            }
        }

        /// <summary>Handles the Leave event of the PanelContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelContent_Leave(object sender, EventArgs e)
        {
            if ((DataContext?.CurrentContent is not null) && (DataContext.CurrentContent.Clipboard == DataContext.ClipboardContext))
            {
                DataContext.ClipboardContext = null;
            }
        }

        /// <summary>Raises the <see cref="Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!IsHandleCreated)
            {
                return;
            }

            SplitMain.Panel1MinSize = (ClientSize.Width / 2).Max(320);
            SplitMain.Panel2MinSize = (ClientSize.Width / 6).Max(320);

            SplitFileSystem.Panel1MinSize = (ClientSize.Height / 2).Max(320);
            SplitFileSystem.Panel2MinSize = 256;

            SplitMain.PerformLayout();
            SplitFileSystem.PerformLayout();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((IsDesignTime) || (Disposing) || (!_deferDataContextLoad))
            {
                return;
            }

            DataContext?.Load();
            ActiveControl = null;
            FileExplorer.Select();
            ActiveControl = FileExplorer;
        }

        /// <summary>
        /// Function to make the file explorer visible or invisible.
        /// </summary>
        /// <param name="visible"><b>true</b> to make visible, <b>false</b> to make invisible.</param>
        public void SetFileExplorerVisibility(bool visible)
        {
            Settings.ShowFileExplorer = visible;
            SplitMain.Panel2Collapsed = !visible;
        }

        /// <summary>
        /// Function to make the preview visible or invisible.
        /// </summary>
        /// <param name="visible"><b>true</b> to make visible, <b>false</b> to make invisible.</param>
        public void SetPreviewVisibility(bool visible)
        {
            void RefreshContentPreviewer()
            {
                string filePath = DataContext.FileExplorer.SelectedFiles[0]?.FullPath;

                if ((DataContext.ContentPreviewer.RefreshPreviewCommand is not null) && (DataContext.ContentPreviewer.RefreshPreviewCommand.CanExecute(filePath)))
                {
                    DataContext.ContentPreviewer.RefreshPreviewCommand.Execute(filePath);
                }
            }

            void ResetContentPreviewer()
            {
                if ((DataContext.ContentPreviewer.ResetPreviewCommand is not null) && (DataContext.ContentPreviewer.ResetPreviewCommand.CanExecute(null)))
                {
                    DataContext.ContentPreviewer.ResetPreviewCommand.Execute(null);
                }
            }

            Settings.ShowContentPreview = visible;
            SplitFileSystem.Panel2Collapsed = !visible;

            if ((DataContext?.ContentPreviewer is null) || (DataContext?.FileExplorer is null))
            {
                return;
            }

            DataContext.ContentPreviewer.IsEnabled = visible;

            try
            {
                if ((FileExplorer is null) || (DataContext.FileExplorer.SelectedFiles.Count == 0) || (!visible))
                {
                    ResetContentPreviewer();
                    return;
                }

                RefreshContentPreviewer();
            }
            catch (Exception ex)
            {
                Debug.Print($"There was an error refreshing the file preview.\n\n{ex.Message}\n\n{ex.StackTrace}");
            }
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

            if (DataContext is null)
            {
                return;
            }

            _deferDataContextLoad = !IsHandleCreated;
            if (!_deferDataContextLoad)
            {
                DataContext.Load();
            }

            ActiveControl = null;
            FileExplorer.Select();
            ActiveControl = FileExplorer;

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
