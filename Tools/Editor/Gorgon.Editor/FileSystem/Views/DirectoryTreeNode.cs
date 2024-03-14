#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: December 8, 2019 3:20:08 PM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// An tree node, representing a directory, for the <see cref="TreeEx"/> control.
/// </summary>
internal class DirectoryTreeNode
    : TreeNode, IDataContext<IDirectory>
{
    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    public IDirectory ViewModel
    {
        get;
        private set;
    }

    /// <summary>Gets or sets the foreground color of the tree node.</summary>
    public new Color ForeColor
    {
        get => (ViewModel is IExcludable excluded)
                && (!ViewModel.IsCut)
                && (excluded.IsExcluded)
                ? DarkFormsRenderer.ExcludedColor
                : base.ForeColor;
        set => base.ForeColor = value;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IDirectory.Name):
                if (!string.Equals(Text, ViewModel.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Text = ViewModel.Name;
                }
                break;
            case nameof(IDirectory.ID):
                if (!string.Equals(Name, ViewModel.ID, StringComparison.CurrentCultureIgnoreCase))
                {
                    Name = ViewModel.ID;
                }
                break;
            case nameof(IDirectory.ImageName):
                if (!string.Equals(ImageKey, ViewModel.ImageName, StringComparison.CurrentCultureIgnoreCase))
                {
                    ImageKey = ViewModel.ImageName;
                }
                break;
            case nameof(IExcludable.IsExcluded):
                TreeView.Invalidate();
                break;
        }
    }

    /// <summary>
    /// Function to unassign events on the current data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to set up events for the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign events on.</param>
    private void AssignEvents(IDirectory dataContext)
    {
        if (dataContext is null)
        {
            return;
        }

        dataContext.PropertyChanged += DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to revert the control back to the default state when the data context is not assigned.
    /// </summary>
    private void ResetDataContext()
    {
        if (ViewModel is null)
        {
            return;
        }

        UnassignEvents();
        ViewModel.Unload();
        ViewModel = null;

        ImageKey = string.Empty;
        Name = string.Empty;
        Text = string.Empty;
    }

    /// <summary>
    /// Function to initialize the control with the specified data context.
    /// </summary>
    /// <param name="dataContext">The data context to initialize from.</param>
    private void InitializeFromDataContext(IDirectory dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        ImageKey = dataContext.ImageName ?? string.Empty;
        Name = dataContext.ID;
        Text = dataContext.Name;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IDirectory dataContext)
    {
        InitializeFromDataContext(dataContext);

        AssignEvents(dataContext);

        ViewModel = dataContext;
    }
    #endregion
}
