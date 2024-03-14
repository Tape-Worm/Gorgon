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
// Created: February 13, 2021 4:04:07 PM
// 
#endregion

using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Examples.Properties;
using Gorgon.Graphics.Imaging;
using Krypton.Toolkit;

namespace Gorgon.Examples;

/// <summary>
/// Provides a ribbon interface for the plug in view.
/// </summary>
/// <remarks>
/// We cannot provide a ribbon on the control directly because, for some reason, the Krypton components will only allow ribbons on forms. 
/// 
/// So, in order to provide ribbon functionality, we create a simple form and drop a Krypton ribbon control on it and set up our buttons for the 
/// view functions. We also have to ensure that the ribbon component is marked as internal/public so we can access it from the main view.
/// 
/// We expose the ribbon to the application by assigning the instance of the ribbon to the Ribbon property on the main view. The editor will 
/// pick up the Ribbon property and merge the ribbon on this form into the ribbon on the application window.
/// 
/// Typically we need to assign the view model for the main view to this form. This is so we can have the ribbon respond to the state of the 
/// view model, and execute commands on the view model in response to ribbon button presses. 
/// </remarks>
internal partial class FormRibbon
    : KryptonForm, IDataContext<ITextContent>
{
    #region Variables.
    // This allows us to set up a list of predefiend zoom values for zooming in and out on the content. It matches the zoom level we've picked 
    // with the menu item assigned to the zoom level.

    // The list of menu items associated with the zoom level.
    private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuZoomItems = [];

    // This is similar to the zoom items in that it's a lookup for the font values and the menu items for the fonts.

    // The list of menu items associated with font selection.
    private readonly Dictionary<FontFace, ToolStripMenuItem> _menuFontItems = [];

    // The current zoom level.
    private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
    // The renderer for the content.
    private IContentRenderer _contentRenderer;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the data context for the ribbon on the form.
    /// </summary>
    public ITextContent ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the currently active renderer.
    /// </summary>
    public IContentRenderer ContentRenderer
    {
        get => _contentRenderer;
        set
        {
            // This property is used so we can access the renderer for the content from the ribbon form should we 
            // need to. 
            //
            // In most cases this is used to detect whether the renderer camera zooms in/out on the content and 
            // adjust the zoom value (on the zoom drop down menu) to match.
            if (_contentRenderer == value)
            {
                return;
            }

            if (_contentRenderer is not null)
            {
                ContentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
            }

            _contentRenderer = value;

            if (_contentRenderer is not null)
            {
                ContentRenderer.ZoomScaleChanged += ContentRenderer_ZoomScale;
                _zoomLevel = _contentRenderer.ZoomLevel;
            }

            UpdateZoomMenu();
        }
    }
    #endregion

    #region Methods.
    /// <summary>Handles the ZoomScale event of the ContentRenderer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ZoomScaleEventArgs"/> instance containing the event data.</param>
    private void ContentRenderer_ZoomScale(object sender, ZoomScaleEventArgs e)
    {
        // This event callback is used to handle the case where the user zooms in/out on the content in the content 
        // renderer using CTRL + Middle Mouse Wheel.

        _zoomLevel = _contentRenderer.ZoomLevel;
        UpdateZoomMenu();
    }

    /// <summary>
    /// Function to update the zoom item menu to reflect the current selection.
    /// </summary>
    private void UpdateZoomMenu()
    {
        // This method updates the zoom drop down menu so that the correct zoom level is checked on the menu, and displayed 
        // in the menu text on the ribbon.

        // We use our look up to ensure we don't link to a menu item that doesn't exist.
        if (!_menuZoomItems.TryGetValue(_zoomLevel, out ToolStripMenuItem currentItem))
        {
            return;
        }

        // Because the built-in tool strip menus don't have radio button-like functionality, we can end up in a scenario 
        // where multiple menu items are checked when they should only have 1 item that is checked. This bit of code 
        // ensures that does not happen by unchecking all the items that are not the current item.
        foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
        {
            item.Checked = false;
        }

        if (!currentItem.Checked)
        {
            currentItem.Checked = true;
        }

        ButtonZoom.TextLine1 = string.Format(Resources.ZOOM_TEXT, _zoomLevel.GetName());
    }

    /// <summary>
    /// Function to update the font menu state based on our current font selection.
    /// </summary>
    private void UpdateFontMenu()
    {
        // For this example, we follow the same pattern as the zoom menu items.
        if (!_menuFontItems.TryGetValue(ViewModel.FontFace, out ToolStripMenuItem currentItem))
        {
            return;
        }

        foreach (ToolStripMenuItem item in MenuFonts.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
        {
            item.Checked = false;
        }

        if (!currentItem.Checked)
        {
            currentItem.Checked = true;
        }
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // As mentioned in the main view code behind, this method is called when a property on the view model 
        // is changed. With this, we detect a change in the font face and update the font menu accordingly.

        switch (e.PropertyName)
        {
            case nameof(ITextContent.FontFace):
                UpdateFontMenu();
                break;
        }

        // This method is used to enable/disable buttons depending on view model state.
        // We should call this regularly to ensure our UI is consistent with our state.
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the save button control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonSave_Click(object sender, EventArgs e)
    {
        // Here's how we execute a command on the view model. We first check to see if the command is assigned, 
        // and can actually execute given the current state of the view model.
        if ((ViewModel?.SaveContentCommand is null) || (!ViewModel.SaveContentCommand.CanExecute(SaveReason.UserSave)))
        {
            return;
        }

        // Notice that this command executes asynchronously, so we need to await it prior to continuing on.
        await ViewModel.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
        ValidateButtons();
    }

    /// <summary>
    /// Function to validate the state of the buttons.
    /// </summary>
    private void ValidateButtons()
    {
        if (ViewModel is null)
        {
            return;
        }

        // This method evaluates the state of the view model, and enables/disables buttons on the ribbon to reflect that 
        // state. This ensures that functionality that shouldn't be executed won't be executed at a given time and state.
        //
        // In most cases, this should be done by evaluating the CanExecute of the command for the button. This allows the 
        // view model to handle any complicated state evaluation logic and makes it such that the view stays unaware of 
        // the logic.

        ButtonFont.Enabled = ViewModel.CurrentPanel is null;
        ButtonChangeText.Enabled = ViewModel.ChangeTextCommand?.CanExecute(null) ?? false;
        ButtonTextColor.Enabled = ViewModel.ActivateTextColorCommand?.CanExecute(null) ?? false;
        ButtonSaveText.Enabled = ViewModel.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;
        ButtonTextUndo.Enabled = ViewModel.UndoCommand?.CanExecute(null) ?? false;
        ButtonTextRedo.Enabled = ViewModel.RedoCommand?.CanExecute(null) ?? false;

        ItemArial.Enabled = true;
        ItemPapyrus.Enabled = true;
        ItemTimesNewRoman.Enabled = true;
    }

    /// <summary>
    /// Function to unassign the events for the data context.
    /// </summary>
    private void UnassignEvents()
    {
        // Always unassign your view model events. Failure to do so can result in an event leak, causing the view to stay 
        // in memory for the lifetime of the application.

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the view when no data context is assigned.
    /// </summary>
    private void ResetDataContext()
    {
        // Whenever we have no view model, we use this to reset the ribbon back to its default state.

        RibbonTextContent.Enabled = false;
        UpdateZoomMenu();
        ItemZoomToWindow.Checked = true;
    }

    /// <summary>Handles the Click event of the Font control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Font_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        // This event callback method updates the FontFace property on the view model after the 
        // user clicks on the font face menu item. 
        //
        // We do this by assigning the correct font face value to the property on the view model 
        // by evaluating which menu item was clicked. 
        //
        // For this example, we do a simple property set on the view model. But for best practice 
        // we should be executing a command on the view model so that we can handle mutations 
        // and edge cases as needed.

        var menuitem = (ToolStripMenuItem)sender;

        FontFace fontFace = _menuFontItems.FirstOrDefault(item => item.Value == menuitem).Key;

        // Do not let us uncheck.
        // Yes, this is annoying. Blame Microsoft for a lousy implementation.
        if (ViewModel.FontFace == fontFace)
        {
            menuitem.Checked = true;
            return;
        }

        ViewModel.FontFace = fontFace;
        UpdateFontMenu();
    }

    /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [EventArgs] instance containing the event data.</param>
    private void ItemZoom_Click(object sender, EventArgs e)
    {
        var item = (ToolStripMenuItem)sender;

        if ((item.Tag is null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
        {
            item.Checked = false;
            return;
        }

        // Do not let us uncheck.
        // Yes, this is annoying. Blame Microsoft for a lousy implementation.
        if (_zoomLevel == zoom)
        {
            item.Checked = true;
            return;
        }

        // This updates the content renderer to zoom in at the desired zoom level that we've chosen 
        // on the zoom drop down menu, and updates the zoom drop down menu so that it is 
        // synchronized with the content renderer zoom value.
        //
        // We do not interact with the view model here since this is view-specific code, and has no 
        // bearing on our content data.

        _zoomLevel = zoom;
        UpdateZoomMenu();

        // We not only pass in the zoom value, but also an offset that centers on the view. 
        // This is done in "client size", meaning that 0x0 is the upper left of the control that we
        // are rendering our content into, and Width x Height is the size of that control.
        // 
        // To get the center, we multiply the width and height by 1/2.
        ContentRenderer?.MoveTo(new Vector2(ContentRenderer.ClientSize.Width * 0.5f, ContentRenderer.ClientSize.Height * 0.5f),
                                _zoomLevel.GetScale());
    }

    /// <summary>Handles the Click event of the ButtonChangeText control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonChangeText_Click(object sender, EventArgs e)
    {
        // Here's how we execute a command on the view model. We first check to see if the command is assigned, 
        // and can actually execute given the current state of the view model.
        if ((ViewModel?.ChangeTextCommand is null) || (!ViewModel.ChangeTextCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ChangeTextCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonTextColor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonTextColor_Click(object sender, EventArgs e)
    {
        // Here's how we execute a command on the view model. We first check to see if the command is assigned, 
        // and can actually execute given the current state of the view model.
        if ((ViewModel?.ActivateTextColorCommand is null) || (!ViewModel.ActivateTextColorCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ActivateTextColorCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonTextUndo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonTextUndo_Click(object sender, EventArgs e)
    {
        // Here's how we execute a command on the view model. We first check to see if the command is assigned, 
        // and can actually execute given the current state of the view model.
        if ((ViewModel?.UndoCommand is null) || (!ViewModel.UndoCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.UndoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonTextRedo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonTextRedo_Click(object sender, EventArgs e)
    {
        // Here's how we execute a command on the view model. We first check to see if the command is assigned, 
        // and can actually execute given the current state of the view model.
        if ((ViewModel?.RedoCommand is null) || (!ViewModel.RedoCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.RedoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>
    /// Function to initialize the view based on the data context.
    /// </summary>
    /// <param name="dataContext">The data context used to initialize.</param>
    private void InitializeFromDataContext(ITextContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        UpdateZoomMenu();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ITextContent dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;
        ValidateButtons();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the zoom back to the default.
    /// </summary>
    public void ResetZoom()
    {
        // Normally we'd set the zoom level to To Window, but in our case, we want it to go back to 100% (so the 
        // text renders nicely).
        _zoomLevel = ZoomLevels.Percent100;

        // We also need to notify the menu that we've selected this option. Due to how the context menu works, 
        // we have to do some manual house keeping to keep it up to date.
        UpdateZoomMenu();
    }
    #endregion

    #region Constructor.
    /// <summary>Initializes a new instance of the FormRibbon class.</summary>
    public FormRibbon()
    {
        InitializeComponent();

        ItemArial.Tag = ImageDataType.Image2D;
        ItemTimesNewRoman.Tag = ImageDataType.ImageCube;
        ItemPapyrus.Tag = ImageDataType.Image3D;

        foreach (ToolStripMenuItem menuItem in MenuZoom.Items.OfType<ToolStripMenuItem>())
        {
            if (!Enum.TryParse(menuItem.Tag.ToString(), out ZoomLevels level))
            {
                menuItem.Enabled = false;
                continue;
            }

            menuItem.Text = level.GetName();
            _menuZoomItems[level] = menuItem;
        }

        // Set up the look up for our fonts, this is so we can cross reference our 
        // font menu item with our enum.
        _menuFontItems[FontFace.Arial] = ItemArial;
        _menuFontItems[FontFace.TimesNewRoman] = ItemTimesNewRoman;
        _menuFontItems[FontFace.Papyrus] = ItemPapyrus;
    }
    #endregion
}
