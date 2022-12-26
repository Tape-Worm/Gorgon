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
// Created: May 9, 2019 7:50:58 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.ImageSplitTool.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.UI;

namespace Gorgon.Editor.ImageSplitTool;

/// <summary>
/// A dialog used for sprite selection.
/// </summary>
internal partial class FormImageSelector
    : EditorToolBaseForm, IDataContext<ISplit>
{
    #region Variables.
    // Flag to indicate that the form is being designed in the IDE.
    private readonly bool _isDesignTime;
    // The state of the close operation.
    private int _closeState;
    #endregion

    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISplit DataContext
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls() => ButtonSplit.Enabled = DataContext?.SplitImageCommand?.CanExecute(null) ?? false;

    /// <summary>Handles the Search event of the ContentFileExplorer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonSearchEventArgs"/> instance containing the event data.</param>
    private void ContentFileExplorer_Search(object sender, GorgonSearchEventArgs e)
    {
        if ((DataContext?.SearchCommand is null) || (!DataContext.SearchCommand.CanExecute(e.SearchText)))
        {
            return;
        }

        DataContext.SearchCommand.Execute(e.SearchText);
        ValidateControls();
    }


    /// <summary>Contents the file explorer file entries focused.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private async void ContentFileExplorer_FileEntriesFocused(object sender, ContentFileEntriesFocusedArgs e)
    {
        if ((DataContext?.RefreshSpritePreviewCommand is null) || (!DataContext.RefreshSpritePreviewCommand.CanExecute(e.FocusedFiles)))
        {
            return;
        }

        await DataContext.RefreshSpritePreviewCommand.ExecuteAsync(e.FocusedFiles);
        ValidateControls();
    }

    /// <summary>Handles the Click event of the ButtonCancel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonCancel_Click(object sender, EventArgs e)
    {
        var args = new CancelEventArgs();
        if ((DataContext?.CancelCommand is null) || (!DataContext.CancelCommand.CanExecute(args)))
        {
            Close();
            return;
        }

        await DataContext.CancelCommand.ExecuteAsync(args);

        if (args.Cancel)
        {
            return;
        }

        // Force a closure of the application.
        _closeState = 1;
        Close();
    }

    /// <summary>Handles the Click event of the ButtonLoad control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonSplit_Click(object sender, EventArgs e)
    {
        if ((DataContext?.SplitImageCommand is null) || (!DataContext.SplitImageCommand.CanExecute(null)))
        {
            return;
        }

        LabelProcessing.Text = string.Format(Resources.GORIST_TEXT_LOADING, string.Empty);
        LabelProcessing.Visible = true;
        ButtonSplit.Enabled = false;
        SplitFileSelector.Visible = false;
        TableOutput.Visible = false;

        await DataContext.SplitImageCommand.ExecuteAsync(null);

        SplitFileSelector.Visible = true;
        TableOutput.Visible = true;
        LabelProcessing.Visible = false;
        ValidateControls();
    }

    /// <summary>Function called when a property was changed on the data context.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method when detecting property changes on the data context instead of assigning their own event handlers.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISplit.Progress):
                LabelProcessing.Text = string.IsNullOrWhiteSpace(DataContext.Progress) ? Resources.GORIST_TEXT_LOADING : string.Format(Resources.GORIST_TEXT_PROCESSING, DataContext.Progress);
                break;
            case nameof(ISplit.OutputDirectory):
                TextOutputFolder.Text = DataContext.OutputDirectory;
                break;
        }

        ValidateControls();
    }

    /// <summary>Handles the Click event of the ButtonFolderBrowse control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFolderBrowse_Click(object sender, EventArgs e)
    {
        if ((DataContext?.SelectFolderCommand is null) || (!DataContext.SelectFolderCommand.CanExecute(null)))
        {
            return;
        }

        DataContext.SelectFolderCommand.Execute(null);
    }

    /// <summary>
    /// Function to reset the control back to its default state.
    /// </summary>
    private void ResetDataContext() => ContentFileExplorer.Entries = null;

    /// <summary>
    /// Function to initialize the form from the data context.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void InitializeFromDataContext(ISplit dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        TextOutputFolder.Text = dataContext.OutputDirectory;
        ContentFileExplorer.Entries = dataContext.SpriteFileEntries;
    }

    /// <summary>Function to perform custom graphics set up.</summary>
    /// <param name="graphicsContext">The graphics context for the application.</param>
    /// <param name="swapChain">The swap chain used to render into the UI.</param>
    /// <remarks>
    ///   <para>
    /// This method allows tool plug in implementors to setup additional functionality for custom graphics rendering.
    /// </para>
    ///   <para>
    /// Resources created by this method should be cleaned up in the <see cref="EditorToolBaseForm.OnShutdownGraphics"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="EditorToolBaseForm.OnShutdownGraphics" />
    protected override void OnSetupGraphics(IGraphicsContext graphicsContext, GorgonSwapChain swapChain)
    {
        base.OnSetupGraphics(graphicsContext, swapChain);

        var renderer = new Renderer(graphicsContext.Renderer2D, swapChain, DataContext);
        renderer.Initialize();

        AddRenderer("PreviewRenderer", renderer);
        SwitchRenderer("PreviewRenderer");
    }

    /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (_isDesignTime)
        {
            return;
        }

        if (_closeState != 0)
        {
            DataContext?.Unload();
            return;
        }

        if ((DataContext?.CancelCommand is null) || (!DataContext.CancelCommand.CanExecute(null)))
        {
            DataContext?.Unload();
            return;
        }

        e.Cancel = true;

        var args = new CancelEventArgs();
        await DataContext.CancelCommand.ExecuteAsync(args);

        if (args.Cancel)
        {
            _closeState = 0;
            return;
        }

        await Task.Yield();

        _closeState = 1;
        Close();
    }

    /// <summary>Raises the Load event.</summary>
    /// <param name="e">An EventArgs containing event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_isDesignTime)
        {
            return;
        }

        DataContext?.Load();

        ValidateControls();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISplit dataContext)
    {
        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);
        DataContext = dataContext;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FormImageSelector"/> class.</summary>
    public FormImageSelector()
    {
        _isDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        InitializeComponent();
    }
    #endregion
}
