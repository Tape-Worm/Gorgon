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
// Created: April 20, 2019 2:19:56 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The panel used to display settings for image codec support.
/// </summary>
internal partial class ImageSettingsPanel
    : SettingsBaseControl, IDataContext<ISettings>
{
    #region Properties.
    /// <summary>Property to return the ID of the panel.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string PanelID => DataContext?.ID.ToString() ?? Guid.Empty.ToString();

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISettings DataContext
    {
        get;
        private set;
    }
    #endregion

    #region Methods.        
    /// <summary>Handles the Click event of the ButtonClear control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs">EventArgs</see> instance containing the event data.</param>
    private void ButtonClear_Click(object sender, EventArgs e) => TextPath.Text = string.Empty;

    /// <summary>Handles the Click event of the ButtonPath control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs">EventArgs</see> instance containing the event data.</param>
    private void ButtonPath_Click(object sender, EventArgs e)
    {
        try
        {
            DialogFileOpen.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            if (DialogFileOpen.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            TextPath.Text = DialogFileOpen.FileName;
        }
        catch (Exception ex)
        {
            GorgonDialogs.ErrorBox(ParentForm, ex);
        }
    }

    /// <summary>
    /// Function to validate the commands on the view.
    /// </summary>
    private void ValidateCommands() => ButtonClear.Enabled = !string.IsNullOrWhiteSpace(TextPath.Text);

    /// <summary>
    /// Function to restore the control to its default state.
    /// </summary>
    private void ResetDataContext() => TextPath.Text = string.Empty;

    /// <summary>Handles the TextChanged event of the TextPath control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs">EventArgs</see> instance containing the event data.</param>
    private void TextPath_TextChanged(object sender, EventArgs e)
    {
        if ((DataContext?.UpdatePathCommand is null) || (!DataContext.UpdatePathCommand.CanExecute(TextPath.Text)))
        {
            return;
        }

        DataContext.UpdatePathCommand.Execute(TextPath.Text);
        ValidateCommands();
    }

    /// <summary>
    /// Function to initialize the control from the specified data context.
    /// </summary>
    /// <param name="dataContext">The data context to apply.</param>
    private void InitializeFromDataContext(ISettings dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        TextPath.Text = dataContext?.ImageEditorApplicationPath ?? string.Empty;            
    }        

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISettings dataContext)
    {
        InitializeFromDataContext(dataContext);
        DataContext = dataContext;

        ValidateCommands();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ImageSettingsPanel"/> class.</summary>
    public ImageSettingsPanel() => InitializeComponent();
    #endregion
}
