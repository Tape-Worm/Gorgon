#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 17, 2020 1:19:36 AM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Displays a wait panel on an application during long running operations.
    /// </summary>
    public class WaitPanelDisplay
        : IDisposable
    {
        #region Variables.
        // The form to display for the wait panel.
        private FormWait _waitForm;   
        // The view model to hook into.
        private IViewModel _viewModel;
        // The form to parent the wait panel.
        private readonly Form _appForm;
        // The synchronization context for synchronizing to the main thread.
        private readonly SynchronizationContext _syncContext;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to unassign the events from the view model.
        /// </summary>
        private void UnassignEvents()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.WaitPanelActivated -= ViewModel_WaitPanelActivated;
            _viewModel.WaitPanelDeactivated -= ViewModel_WaitPanelDeactivated;
        }

        /// <summary>Handles the WaitPanelDeactivated event of the ViewModel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ViewModel_WaitPanelDeactivated(object sender, EventArgs e)
        {
            void HideForm(object _)
            {
                if (!_waitForm.Visible)
                {
                    return;
                }

                _appForm.Enabled = true;
                _waitForm.Hide();

                _appForm.BringToFront();
                _appForm.Activate();
            }

            if (_syncContext != SynchronizationContext.Current)
            {
                _syncContext.Send(HideForm, null);

            }
            else
            {
                HideForm(null);
            }
        }

        /// <summary>Views the model wait panel activated.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ViewModel_WaitPanelActivated(object sender, WaitPanelActivateArgs e)
        {
            void ShowSync(object _)
            {
                _waitForm.Wait.WaitMessage = e.Message;

                if (!string.IsNullOrWhiteSpace(e.Title))
                {
                    _waitForm.Wait.WaitTitle = e.Title;
                }

                if (_waitForm.Visible)
                {
                    return;
                }

                _appForm.Enabled = false;
                _waitForm.Show(_appForm);
                _waitForm.BringToFront();
                _waitForm.Invalidate();
            }

            if (_syncContext != SynchronizationContext.Current)
            {
                _syncContext.Post(ShowSync, null);
            }
            else
            {
                ShowSync(null);
            }
        }

        /// <summary>
        /// Function to bring the wait panel to the top of the z-order.
        /// </summary>
        public void BringToFront()
        {
            if (!_waitForm.Visible)
            {
                return;
            }

            _waitForm.BringToFront();
        }

        /// <summary>
        /// Function to assign the data context that will trigger the wait panel.
        /// </summary>
        /// <param name="dataContext">The data context to hook.</param>
        public void SetDataContext(IViewModel dataContext)
        {
            UnassignEvents();

            _viewModel = dataContext;

            if (_viewModel == null)
            {
                return;
            }

            _viewModel.WaitPanelActivated += ViewModel_WaitPanelActivated;
            _viewModel.WaitPanelDeactivated += ViewModel_WaitPanelDeactivated;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {            
            SetDataContext(null);
            FormWait wait =Interlocked.Exchange(ref _waitForm, null);            
            wait?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="WaitPanelDisplay"/> class.</summary>
        /// <param name="appForm">The application form that will be the parent to the wait panel.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appForm"/> parameter is <b>null</b>.</exception>
        public WaitPanelDisplay(Form appForm)
        {
            _appForm = appForm ?? throw new ArgumentNullException(nameof(appForm));
            _syncContext = SynchronizationContext.Current;
            _waitForm = new FormWait();
        }
        #endregion       
    }
}
