#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 3, 2021 9:37:58 AM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view used to select characters for a font.
/// </summary>
internal partial class FontCharacterSelectionView
    : EditorSubPanelCommon, IDataContext<IFontCharacterSelection>
{
    #region Properties.
    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFontCharacterSelection ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the CharactersChanged event of the CharPicker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void CharPicker_CharactersChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Characters = CharPicker.Characters;
        ValidateOk();
    }

    /// <summary>
    /// Function to unassign the events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontCharacterSelection.Characters):
                CharPicker.Characters = ViewModel.Characters;
                break;
            case nameof(IFontCharacterSelection.CurrentFont):
                CharPicker.CurrentFont = ViewModel.CurrentFont;
                break;
        }

        ValidateOk();
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext() => UnassignEvents();

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IFontCharacterSelection dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        CharPicker.CurrentFont = dataContext.CurrentFont;
        CharPicker.Characters = dataContext.Characters;
    }

    /// <summary>
    /// Function to validate the state of the OK button.
    /// </summary>
    /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk() => (ViewModel?.OkCommand is not null) && (ViewModel.OkCommand.CanExecute(null));

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel()
    {
        if (ViewModel is not null)
        {
            ViewModel.IsActive = false;
        }
    }

    /// <summary>Function to submit the change.</summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if ((ViewModel?.OkCommand is null) || (!ViewModel.OkCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.OkCommand.Execute(null);
    }

    /// <summary>Raises the <see cref="System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        ViewModel?.Load();

        ValidateOk();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFontCharacterSelection dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            ValidateOk();
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
        ValidateOk();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontCharacterSelectionView"/> class.</summary>
    public FontCharacterSelectionView() => InitializeComponent();
    #endregion
}
