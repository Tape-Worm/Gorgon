
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 20, 2019 2:19:56 PM
// 


using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Examples;

/// <summary>
/// The panel used to settings for the text content plug in
/// </summary>
/// <remarks>
/// As mentioned in the plug in class, we create a panel control for presenting our plug in settings to the user. 
/// 
/// To create a settings panel, the class must inherit from the SettingsBaseControl and implement the IDataContext interface
/// </remarks>
internal partial class TextContentSettingsPanel
    : SettingsBaseControl, IDataContext<ISettings>
{

    /// <summary>Property to return the ID of the panel.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string PanelID => ViewModel?.ID.ToString() ?? Guid.Empty.ToString();

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISettings ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the CheckedChanged event of the Radio controls.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Radio_CheckedChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (RadioArial.Checked)
        {
            ViewModel.DefaultFont = FontFace.Arial;
        }
        else if (RadioTimesNewRoman.Checked)
        {
            ViewModel.DefaultFont = FontFace.TimesNewRoman;
        }
        else if (RadioPapyrus.Checked)
        {
            ViewModel.DefaultFont = FontFace.Papyrus;
        }
    }

    /// <summary>
    /// Function to restore the control to its default state.
    /// </summary>
    private void ResetDataContext() => RadioArial.Checked = true;

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

        // Here we transfer the setting values from the view model.
        switch (dataContext.DefaultFont)
        {
            case FontFace.Arial:
                RadioArial.Checked = true;
                break;
            case FontFace.TimesNewRoman:
                RadioTimesNewRoman.Checked = true;
                break;
            case FontFace.Papyrus:
                RadioPapyrus.Checked = true;
                break;
        }
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISettings dataContext)
    {
        InitializeFromDataContext(dataContext);
        ViewModel = dataContext;
    }



    /// <summary>Initializes a new instance of the <see cref="TextContentSettingsPanel"/> class.</summary>
    public TextContentSettingsPanel() => InitializeComponent();

}
