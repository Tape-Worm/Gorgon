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
// Created: February 17, 2020 1:56:46 AM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Displays a progress panel on an application during long running operations.
    /// </summary>
    public class ProgressPanelDisplay
        : IDisposable
    {
        #region Variables.
        // The current action to use when cancelling an operation in progress.
        private Action _progressCancelAction;
        // The form to display for the progress panel.
        private FormProgress _progressForm;
        // The application form that will be the parent of the panel.
        private readonly Form _appForm;
        // The view model to hook into.
        private IViewModel _viewModel;
        // The synchronization context for synchronizing to the main thread.
        private readonly SynchronizationContext _syncContext;
        // The timer used to control the rate of updates to the progress panel.
        private IGorgonTimer _progressTimer;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to unassign the events from the view model.
        /// </summary>
        private void UnassignEvents()
        {
            _progressCancelAction?.Invoke();

            if (_viewModel == null)
            {
                return;
            }

            _viewModel.ProgressDeactivated -= ViewModel_ProgressDeactivated;
            _viewModel.ProgressUpdated -= ViewModel_ProgressUpdated;
        }

        /// <summary>Handles the OperationCancelled event of the Progress control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Progress_OperationCancelled(object sender, EventArgs e) => _progressCancelAction?.Invoke();

        /// <summary>Handles the ProgressDeactivated event of the ViewModel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ViewModel_ProgressDeactivated(object sender, EventArgs e)
        {
            void HideForm(object _)
            {
                _appForm.Enabled = true;

                _progressCancelAction = null;
                _progressForm.Hide();
                _progressForm.Progress.OperationCancelled -= Progress_OperationCancelled;
                _progressTimer = null;

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

        /// <summary>Views the model progress updated.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ViewModel_ProgressUpdated(object sender, ProgressPanelUpdateArgs e)
        {
            // Drop the message if it's been less than 10 milliseconds since our last call.
            // This will keep us from flooding the window message queue with very fast operations.            
            if ((_progressTimer != null) && (_progressTimer.Milliseconds < 10) && (e.PercentageComplete < 1) && (e.PercentageComplete > 0))
            {
                return;
            }

            // The actual method to execute on the main thread.
            void UpdateProgressPanel(object ctx)
            {
                var args = (ProgressPanelUpdateArgs)ctx;
                _progressForm.Progress.ProgressTitle = args.Title ?? string.Empty;
                _progressForm.Progress.ProgressMessage = args.Message ?? string.Empty;
                _progressForm.Progress.CurrentValue = args.PercentageComplete;
                _progressForm.Progress.AllowCancellation = args.CancelAction != null;
                _progressCancelAction = args.CancelAction;

                bool isMarquee = _progressForm.Progress.MeterStyle == ProgressBarStyle.Marquee;

                if (_progressForm.Progress.Visible)
                {
                    if (isMarquee == e.IsMarquee)
                    {
                        _progressTimer?.Reset();
                        return;
                    }

                    // If we change to a marquee format, then hide the previous panel.
                    ViewModel_ProgressDeactivated(this, EventArgs.Empty);
                }

                _progressTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerQpc() : new GorgonTimerMultimedia();
                _progressForm.Progress.OperationCancelled += Progress_OperationCancelled;
                _progressForm.Progress.MeterStyle = args.IsMarquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                _appForm.Enabled = false;
                _progressForm.Show(_appForm);
                _progressForm.Progress.BringToFront();
                _progressForm.Progress.Invalidate();
            }

            if (_syncContext != SynchronizationContext.Current)
            {
                _syncContext.Send(UpdateProgressPanel, e);
            }
            else
            {
                UpdateProgressPanel(e);
            }
        }

        /// <summary>
        /// Function to bring the wait panel to the top of the z-order.
        /// </summary>
        public void BringToFront()
        {
            if (!_progressForm.Visible)
            {
                return;
            }

            _progressForm.BringToFront();
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

            _viewModel.ProgressUpdated += ViewModel_ProgressUpdated;
            _viewModel.ProgressDeactivated += ViewModel_ProgressDeactivated;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {            
            SetDataContext(null);
            FormProgress progress = Interlocked.Exchange(ref _progressForm, null);
            progress?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ProgressPanelDisplay"/> class.</summary>
        /// <param name="appForm">The application form that will be the parent to the wait panel.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appForm"/> parameter is <b>null</b>.</exception>
        public ProgressPanelDisplay(Form appForm)
        {
            _appForm = appForm ?? throw new ArgumentNullException(nameof(appForm));
            _syncContext = SynchronizationContext.Current;
            _progressForm = new FormProgress();
        }        
        #endregion

    }
}
