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
// Created: August 26, 2018 11:23:41 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The control to display recent documents.
    /// </summary>
    internal partial class StageRecent
        : EditorBaseControl, IDataContext<IRecentVm>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>        
        [Browsable(false)]
        public IRecentVm DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return whether or not the recent items list has any items in it.
        /// </summary>
        [Browsable(false)]
        public bool HasItems => DataContext?.Files.Count > 0;
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {

        }

        /// <summary>Handles the DeleteItem event of the RecentFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RecentItemDeleteEventArgs"/> instance containing the event data.</param>
        private void RecentFiles_DeleteItem(object sender, RecentItemDeleteEventArgs e)
        {
            if ((DataContext?.DeleteItemCommand == null) || (!DataContext.DeleteItemCommand.CanExecute(e)))
            {
                return;
            }

            DataContext.DeleteItemCommand.Execute(e);
        }

        /// <summary>Handles the RecentItemClick event of the RecentFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RecentItemClickEventArgs"/> instance containing the event data.</param>
        private void RecentFiles_RecentItemClick(object sender, RecentItemClickEventArgs e)
        {
            if ((DataContext?.OpenProjectCommand == null) || (e.Item == null))
            {
                return;
            }

            if (!DataContext.OpenProjectCommand.CanExecute(e.Item))
            {
                return;
            }

            DataContext.OpenProjectCommand.Execute(e.Item);
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

            DataContext.OnUnload();
            DataContext = null;
        }

        /// <summary>
        /// Function to initialize the control from a data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IRecentVm dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            RecentFiles.RecentItems = dataContext.Files;
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
        /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataContext?.OnLoad();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IRecentVm dataContext)
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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageRecent"/> class.
        /// </summary>
        public StageRecent() => InitializeComponent();
        #endregion
    }
}
