﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 17, 2020 1:19:36 AM
// 

using Gorgon.UI;

namespace Gorgon.Editor.UI;

/// <summary>
/// Displays a wait panel on an application during long running operations
/// </summary>
public class WaitPanelDisplay
    : IDisposable
{

    // The form to display for the wait panel.
    private readonly GorgonWaitOverlay _waitForm;
    // The view model to hook into.
    private IViewModel _viewModel;
    // The form to parent the wait panel.
    private readonly Control _appForm;

    /// <summary>
    /// Function to unassign the events from the view model.
    /// </summary>
    private void UnassignEvents()
    {
        if (_viewModel is null)
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
        _appForm.Enabled = true;
        _waitForm.Hide();
    }

    /// <summary>Views the model wait panel activated.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void ViewModel_WaitPanelActivated(object sender, WaitPanelActivateArgs e)
    {
        if (!_waitForm.IsActive)
        {
            _appForm.Enabled = false;
            _waitForm.Show(_appForm, e.Title, e.Message);
        }
    }

    /// <summary>
    /// Function to assign the data context that will trigger the wait panel.
    /// </summary>
    /// <param name="dataContext">The data context to hook.</param>
    public void SetDataContext(IViewModel dataContext)
    {
        UnassignEvents();

        _viewModel = dataContext;

        if (_viewModel is null)
        {
            return;
        }

        _viewModel.WaitPanelActivated += ViewModel_WaitPanelActivated;
        _viewModel.WaitPanelDeactivated += ViewModel_WaitPanelDeactivated;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => UnassignEvents();

    /// <summary>Initializes a new instance of the <see cref="WaitPanelDisplay"/> class.</summary>
    /// <param name="appForm">The application form that will be the parent to the wait panel.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appForm"/> parameter is <b>null</b>.</exception>
    public WaitPanelDisplay(Control appForm)
    {
        _appForm = appForm ?? throw new ArgumentNullException(nameof(appForm));
        _waitForm = new GorgonWaitOverlay
        {
            OverlayColor = Graphics.GorgonColors.Black
        };
    }
}
