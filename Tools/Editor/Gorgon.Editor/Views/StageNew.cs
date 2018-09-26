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
// Created: August 26, 2018 11:51:46 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.UI;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The view for a new project.
    /// </summary>
    internal partial class StageNew
        : EditorBaseControl, IDataContext<IStageNewVm>
    {
        #region Events.
        /// <summary>
        /// Event triggered when a project has been created.
        /// </summary>
        public event EventHandler<ProjectCreateArgs> ProjectCreated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IStageNewVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the ButtonCreate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonCreate_Click(object sender, EventArgs e)
        {
            if (DataContext?.CreateProjectCommand == null)
            {
                return;
            }

            var args = new ProjectCreateArgs();

            if (!DataContext.CreateProjectCommand.CanExecute(args))
            {
                return;
            }

            await DataContext.CreateProjectCommand.ExecuteAsync(args);

            EventHandler<ProjectCreateArgs> handler = ProjectCreated;
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Handles the TextChanged event of the TextName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (DataContext == null)
                {
                    return;
                }

                DataContext.Title = TextName.Text;
            }
            finally
            {
                ValidateControls();
            }
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
                case nameof(IStageNewVm.Title):
                    TextName.Text = DataContext.Title;
                    break;
                case nameof(IStageNewVm.WorkspacePath):
                    LabelWorkspaceLocation.Text = DataContext.WorkspacePath?.FullName ?? Resources.GOREDIT_ERR_ERROR;
                    break;
            }

            ValidateControls();
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
        /// Function to reset the control back to its original state when no data context is assigned.
        /// </summary>
        private void ResetDataContext()
        {
            TextName.Text = Resources.GOREDIT_NEW_PROJECT;
            LabelWorkspaceLocation.Text = string.Empty;
            LabelRam.Text = $"0 bytes";
            LabelDriveSpace.Text = $"0 bytes";
            LabelActiveGpu.Text = Resources.GOREDIT_TEXT_UNKNOWN;
        }

        /// <summary>
        /// Function to initialize the control from a data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IStageNewVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            TextName.Text = dataContext.Title ?? string.Empty;
            LabelWorkspaceLocation.Text = dataContext.WorkspacePath?.FullName ?? Resources.GOREDIT_ERR_ERROR;
            LabelRam.Text = $"{dataContext.AvailableRam.FormatMemory()} ({dataContext.AvailableRam:###,##0} bytes)";
            LabelDriveSpace.Text = $"{dataContext.AvailableDriveSpace.FormatMemory()} ({dataContext.AvailableDriveSpace:###,##0} bytes)";
            LabelActiveGpu.Text = dataContext.GPUName;
        }

        /// <summary>
        /// Function unassign any data context events.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to validate control state.
        /// </summary>
        private void ValidateControls() => ButtonCreate.Enabled = (DataContext?.CreateProjectCommand?.CanExecute(null) ?? false);

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataContext?.OnLoad();
        }

        /// <summary>
        /// Function to assign a data context to the view.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        public void SetDataContext(IStageNewVm dataContext)
        {
            try
            {
                UnassignEvents();

                InitializeFromDataContext(dataContext);
                DataContext = dataContext;

                if ((IsDesignTime) || (DataContext == null))
                {
                    return;
                }

                DataContext.PropertyChanging += DataContext_PropertyChanging;
                DataContext.PropertyChanged += DataContext_PropertyChanged;
            }
            finally
            {
                ValidateControls();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageNew"/> class.
        /// </summary>
        public StageNew() => InitializeComponent();
        #endregion
    }
}
