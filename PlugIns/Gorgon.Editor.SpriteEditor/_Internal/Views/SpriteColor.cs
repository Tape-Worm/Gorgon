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
// Created: March 28, 2019 9:48:28 AM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A color selection control for the sprite.
/// </summary>
internal partial class SpriteColor
    : EditorSubPanelCommon, IDataContext<ISpriteColorEdit>
{
    #region Variables.
    // The list of vertices that were selected.
    private readonly GorgonColor[] _selectedColors = new GorgonColor[4];
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISpriteColorEdit ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the ColorChanged event of the Picker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> instance containing the event data.</param>
    private void Picker_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        for (int i = 0; i < ViewModel.SelectedVertices.Count; ++i)
        {
            if (ViewModel.SelectedVertices[i])
            {
                _selectedColors[i] = e.Color;
            }
            else
            {
                _selectedColors[i] = ViewModel.SpriteColor[i];
            }
        }

        ViewModel.SpriteColor = _selectedColors;
        ViewModel.SelectedColor = e.Color;
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
        int firstSelected = 0;

        switch (e.PropertyName)
        {
            case nameof(ISpriteColorEdit.SelectedColor):
                Picker.SelectedColor = ViewModel.SelectedColor;
                break;
            case nameof(ISpriteColorEdit.OriginalSpriteColor):
                for (int i = 0; i < ViewModel.SelectedVertices.Count; ++i)
                {
                    if (ViewModel.SelectedVertices[i])
                    {
                        firstSelected = i;
                        break;
                    }
                }

                Picker.OriginalColor = ViewModel.OriginalSpriteColor[firstSelected];
                break;
            case nameof(ISpriteColorEdit.SelectedVertices):
            case nameof(ISpriteColorEdit.SpriteColor):
                Picker.ColorChanged -= Picker_ColorChanged;
                try
                {
                    for (int i = 0; i < ViewModel.SelectedVertices.Count; ++i)
                    {
                        if (ViewModel.SelectedVertices[i])
                        {
                            firstSelected = i;
                            break;
                        }
                    }
                    Picker.SelectedColor = ViewModel.SpriteColor[firstSelected];
                    Picker.OriginalColor = ViewModel.OriginalSpriteColor[firstSelected];
                }
                finally
                {
                    Picker.ColorChanged += Picker_ColorChanged;
                }
                break;
        }
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext()
    {
        UnassignEvents();
        Picker.OriginalColor = Picker.SelectedColor = GorgonColor.BlackTransparent;
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(ISpriteColorEdit dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        int firstSelected = 0;
        for (int i = 0; i < dataContext.SelectedVertices.Count; ++i)
        {
            if (dataContext.SelectedVertices[i])
            {
                firstSelected = i;
                break;
            }
        }

        Picker.OriginalColor = dataContext.OriginalSpriteColor[firstSelected];
        Picker.SelectedColor = dataContext.SpriteColor[firstSelected];
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

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel()
    {
        base.OnCancel();

        if ((ViewModel?.CancelCommand is null) || (!ViewModel.CancelCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.CancelCommand.Execute(null);
    }

    /// <summary>
    /// Function to validate the state of the OK button.
    /// </summary>
    /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk() => (ViewModel?.OkCommand is not null) && (ViewModel.OkCommand.CanExecute(null));

    /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        ViewModel?.Load();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISpriteColorEdit dataContext)
    {
        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="SpriteColor"/> class.</summary>
    public SpriteColor() => InitializeComponent();
    #endregion
}
