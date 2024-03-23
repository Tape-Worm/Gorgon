
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 26, 2018 11:51:46 PM
// 

using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// The view for a new project
/// </summary>
internal partial class StageNew
    : EditorBaseControl, IDataContext<INewProject>
{
    /// <summary>
    /// Property to return the data context assigned to this view.
    /// </summary>
    [Browsable(false)]
    public INewProject ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to reset the path input textbox to its original color.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void ResetTextBoxColor(INewProject dataContext)
    {
        PanelLocateText.BackColor = BackColor;
        TextProjectPath.BackColor = string.IsNullOrWhiteSpace(dataContext?.InvalidPathReason) ? DarkFormsRenderer.WindowBackground : Color.DarkRed;
        TextProjectPath.ForeColor = DarkFormsRenderer.ForeColor;
    }

    /// <summary>Handles the Click event of the ButtonSelect control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonSelect_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.SelectProjectWorkspaceCommand is null) || (!ViewModel.SelectProjectWorkspaceCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.SelectProjectWorkspaceCommand.Execute(null);
    }

    /// <summary>Handles the KeyDown event of the TextProjectPath control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    private void TextProjectPath_KeyDown(object sender, KeyEventArgs e)
    {

        if (e.KeyCode == Keys.Enter)
        {
            TextName_Leave(TextProjectPath, EventArgs.Empty);
            return;
        }

        if (e.KeyCode == Keys.Escape)
        {
            TextProjectPath.Text = string.Empty;
            TextName_Leave(TextProjectPath, EventArgs.Empty);
        }
    }

    /// <summary>Handles the MouseEnter event of the TextName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextName_MouseEnter(object sender, EventArgs e)
    {
        if (TextProjectPath.Focused)
        {
            return;
        }

        TextProjectPath.BackColor = Color.FromKnownColor(KnownColor.SteelBlue);
    }

    /// <summary>Handles the MouseLeave event of the TextName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextName_MouseLeave(object sender, EventArgs e)
    {
        if (TextProjectPath.Focused)
        {
            return;
        }

        ResetTextBoxColor(ViewModel);
    }

    /// <summary>Handles the Enter event of the TextName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextName_Enter(object sender, EventArgs e)
    {
        TextProjectPath.Parent.BackColor = TextProjectPath.BackColor = DarkFormsRenderer.BorderColor;
        TextProjectPath.ForeColor = DarkFormsRenderer.ForeColor;
        TextProjectPath.SelectAll();
        PanelLocateText.BackColor = DarkFormsRenderer.WindowBackground;
    }

    /// <summary>Handles the Leave event of the TextName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextName_Leave(object sender, EventArgs e)
    {
        try
        {
            // If the path is already set, then do nothing.
            if (string.Equals(ViewModel?.WorkspacePath?.FullName, TextProjectPath.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            if (ViewModel?.SetProjectWorkspaceCommand is null)
            {
                return;
            }

            SetProjectWorkspaceArgs args = new(TextProjectPath.Text);

            if (!ViewModel.SetProjectWorkspaceCommand.CanExecute(args))
            {
                return;
            }

            ViewModel.SetProjectWorkspaceCommand.Execute(args);
        }
        finally
        {
            ResetTextBoxColor(ViewModel);
            ValidateControls();
        }
    }

    /// <summary>
    /// Handles the Click event of the ButtonCreate control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonCreate_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.CreateProjectCommand is null) || (!ViewModel.CreateProjectCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.CreateProjectCommand.Execute(null);
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
            case nameof(INewProject.AvailableDriveSpace):
                LabelDriveSpace.Text = $"{ViewModel.AvailableDriveSpace.FormatMemory()} ({ViewModel.AvailableDriveSpace:###,##0} bytes)";
                break;
            case nameof(INewProject.Title):
                LabelProjectTitle.Text = ViewModel.Title ?? string.Empty;
                break;
            case nameof(INewProject.WorkspacePath):
                TextProjectPath.Text = ViewModel.WorkspacePath?.FullName ?? string.Empty;
                break;
            case nameof(INewProject.InvalidPathReason):
                ResetTextBoxColor(ViewModel);
                TipError.Show(ViewModel.InvalidPathReason, TextProjectPath, new Point(0, TextProjectPath.Bottom));
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
        switch (e.PropertyName)
        {
            case nameof(INewProject.Title):
            case nameof(INewProject.WorkspacePath):
            case nameof(INewProject.InvalidPathReason):
                TipError.Hide(TextProjectPath);
                break;
        }
    }

    /// <summary>
    /// Function to reset the control back to its original state when no data context is assigned.
    /// </summary>
    private void ResetDataContext()
    {
        LabelProjectTitle.Text = Resources.GOREDIT_NEW_PROJECT;
        TextProjectPath.Text = string.Empty;
        LabelRam.Text = 0.FormatMemory();
        LabelDriveSpace.Text = 0.FormatMemory();
        LabelActiveGpu.Text = Resources.GOREDIT_TEXT_UNKNOWN;
        ResetTextBoxColor(null);
    }

    /// <summary>
    /// Function to initialize the control from a data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(INewProject dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        TextProjectPath.Text = dataContext.WorkspacePath?.FullName ?? string.Empty;
        LabelProjectTitle.Text = dataContext.Title;
        LabelRam.Text = $"{dataContext.AvailableRam.FormatMemory()} ({dataContext.AvailableRam:###,##0} bytes)";
        LabelDriveSpace.Text = $"{dataContext.AvailableDriveSpace.FormatMemory()} ({dataContext.AvailableDriveSpace:###,##0} bytes)";
        LabelActiveGpu.Text = dataContext.GPUName;
        ResetTextBoxColor(dataContext);
    }

    /// <summary>
    /// Function unassign any data context events.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanging -= DataContext_PropertyChanging;
        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to validate control state.
    /// </summary>
    private void ValidateControls() => ButtonCreate.Enabled = (ViewModel?.CreateProjectCommand?.CanExecute(null) ?? false);

    /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ViewModel?.Load();
    }

    /// <summary>
    /// Function to assign a data context to the view.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    public void SetDataContext(INewProject dataContext)
    {
        try
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            ViewModel = dataContext;

            if ((IsDesignTime) || (ViewModel is null))
            {
                return;
            }

            ViewModel.PropertyChanging += DataContext_PropertyChanging;
            ViewModel.PropertyChanged += DataContext_PropertyChanged;
        }
        finally
        {
            ValidateControls();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StageNew"/> class.
    /// </summary>
    public StageNew() => InitializeComponent();

}
