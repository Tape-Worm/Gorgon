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
        /// Function called when the current content control is closing.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void ContentControl_ControlClosing(object sender, EventArgs e)
        {
            SetupContent(null);
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
            while (SplitProject.Panel1.Controls.Count > 0)
            {
                var contentControl = SplitProject.Panel1.Controls[SplitProject.Panel1.Controls.Count - 1] as ContentBaseControl;

                if (contentControl != null)
                {
                    contentControl.ControlClosing -= ContentControl_ControlClosing;
                }

                SplitProject.Panel1.Controls[SplitProject.Panel1.Controls.Count - 1].Dispose();
            }

            // No content, so we can go now.
            if (dataContext?.CurrentContent == null)
            {
                SplitProject.Panel1Collapsed = true;
                return;
            }

            // Get our view from the content.
            ContentBaseControl control = ViewFactory.CreateView<ContentBaseControl>(dataContext.CurrentContent);
            ViewFactory.AssignViewModel(dataContext.CurrentContent, control);
            control.Dock = DockStyle.Fill;
            SplitProject.Panel1.Controls.Add(control);
            SplitProject.Panel1Collapsed = false;

            // Set up the graphics context.
            control.SetupGraphics(GraphicsContext);

            if (control.Ribbon != null)
            {
                var args = new ContentRibbonEventArgs(control.Ribbon);
                EventHandler<ContentRibbonEventArgs> handler = RibbonAdded;
                handler?.Invoke(this, args);

                _currentContentRibbon = control.Ribbon;
            }

            control.ControlClosing += ContentControl_ControlClosing;

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
                if ((ActiveControl == this) || (ActiveControl == FileExplorer))
                {
                    DataContext.ClipboardContext = DataContext.FileExplorer as IClipboardHandler;
                }
                else
                {
                    DataContext.ClipboardContext = null;
                }
            }
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
