
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
// Created: April 2, 2019 11:23:30 PM
// 


using System.ComponentModel;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Krypton.Ribbon;
using Krypton.Toolkit;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Provides a ribbon interface for the plug in view
/// </summary>
/// <remarks>
/// We cannot provide a ribbon on the control directly. For some reason, the krypton components will only allow ribbons on forms
/// </remarks>
internal partial class FormRibbon
    : KryptonForm, IDataContext<IAnimationContent>
{

    // The list of menu items associated with the zoom level.
    private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuItems = [];
    // The buttons on the ribbon.
    private readonly List<WeakReference<KryptonRibbonGroupButton>> _ribbonButtons = [];
    // The numeric controls on the ribbon.
    private readonly List<WeakReference<KryptonRibbonGroupNumericUpDown>> _ribbonNumerics = [];
    // A list of buttons mapped to the tool structure.
    private readonly Dictionary<string, WeakReference<KryptonRibbonGroupButton>> _toolButtons = new(StringComparer.OrdinalIgnoreCase);
    // The currently selected zoom level
    private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
    // The renderer for the content.
    private IContentRenderer _contentRenderer;
    // The currently active undo/redo handler.
    private IUndoHandler _currentUndoHandler;



    /// <summary>
    /// Property to set or return the current graphics context.
    /// </summary>        
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AnimationEditorSettings Settings
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the currently active renderer.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IContentRenderer ContentRenderer
    {
        get => _contentRenderer;
        set
        {
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

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IAnimationContent ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the PropertyChanged event of the KeyEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void KeyEditor_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateButtons();

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IAnimationContent.CommandContext):
                SetToolStates(ViewModel);

                if (ViewModel.CommandContext is null)
                {
                    _currentUndoHandler = ViewModel;
                }
                else
                {
                    _currentUndoHandler = ViewModel.CommandContext as IUndoHandler;
                }
                break;
            case nameof(IAnimationContent.IsLooping):
                CheckAnimationLoop.Checked = ViewModel.IsLooping;
                break;
            case nameof(IAnimationContent.State):
                switch (ViewModel.State)
                {
                    case AnimationState.Stopped:
                        ButtonAnimStop.Visible = false;
                        ButtonAnimPlay.Visible = true;
                        break;
                    case AnimationState.Playing:
                        ButtonAnimStop.Visible = true;
                        ButtonAnimPlay.Visible = false;
                        break;
                }
                break;
        }
    }

    /// <summary>Handles the ZoomScale event of the ContentRenderer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ZoomScaleEventArgs"/> instance containing the event data.</param>
    private void ContentRenderer_ZoomScale(object sender, ZoomScaleEventArgs e)
    {
        _zoomLevel = _contentRenderer.ZoomLevel;
        UpdateZoomMenu();
    }

    /// <summary>Handles the Click event of the ButtonAnimStop control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimStop_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.StopAnimationCommand is null) || (!ViewModel.StopAnimationCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.StopAnimationCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimPlay control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimPlay_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.PlayAnimationCommand is null) || (!ViewModel.PlayAnimationCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.PlayAnimationCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAddTrack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAddTrack_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ShowAddTrackCommand is null) || (!ViewModel.ShowAddTrackCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ShowAddTrackCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonRemoveTrack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonRemoveTrack_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.DeleteTrackCommand is null) || (!ViewModel.DeleteTrackCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.DeleteTrackCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationClear control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationClear_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ClearAnimationCommand is null) || (!ViewModel.ClearAnimationCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ClearAnimationCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationUndo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationUndo_Click(object sender, EventArgs e)
    {
        if ((_currentUndoHandler?.UndoCommand is null) || (!_currentUndoHandler.UndoCommand.CanExecute(null)))
        {
            return;
        }

        _currentUndoHandler.UndoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationRedo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationRedo_Click(object sender, EventArgs e)
    {
        if ((_currentUndoHandler?.RedoCommand is null) || (!_currentUndoHandler.RedoCommand.CanExecute(null)))
        {
            return;
        }

        _currentUndoHandler.RedoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationLoadBack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonAnimationLoadBack_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.LoadBackgroundImageCommand is null) || (!ViewModel.LoadBackgroundImageCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.LoadBackgroundImageCommand.ExecuteAsync(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationClearBack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationClearBack_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ClearBackgroundImageCommand is null) || (!ViewModel.ClearBackgroundImageCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ClearBackgroundImageCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationSprite control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonAnimationSprite_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.LoadSpriteCommand is null) || (!ViewModel.LoadSpriteCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.LoadSpriteCommand.ExecuteAsync(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonPrevKey control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonPrevKey_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.PrevKeyCommand is null) || (!ViewModel.PrevKeyCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.PrevKeyCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonNextKey control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonNextKey_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.NextKeyCommand is null) || (!ViewModel.NextKeyCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.NextKeyCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonFirstKey control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFirstKey_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.FirstKeyCommand is null) && (ViewModel.FirstKeyCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.FirstKeyCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonLastKey control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonLastKey_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.LastKeyCommand is null) && (ViewModel.LastKeyCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.LastKeyCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationProperties control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationProperties_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ShowAnimationPropertiesCommand is null) || (!ViewModel.ShowAnimationPropertiesCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ShowAnimationPropertiesCommand.Execute(null);
        ValidateButtons();
    }


    /// <summary>Handles the Click event of the ButtonAnimationSetKeyframe control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonAnimationSetKeyframe_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.KeyEditor?.SetKeyCommand is null) || (!ViewModel.KeyEditor.SetKeyCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.KeyEditor.SetKeyCommand.ExecuteAsync(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationRemoveKeyframes control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationRemoveKeyframes_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.KeyEditor?.RemoveKeyCommand is null) || (!ViewModel.KeyEditor.RemoveKeyCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.KeyEditor.RemoveKeyCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationClearKeyframes control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationClearKeyframes_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.KeyEditor?.ClearKeysCommand is null) || (!ViewModel.KeyEditor.ClearKeysCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.KeyEditor.ClearKeysCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonSaveAnimation control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonSaveAnimation_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.SaveContentCommand is null) || (!ViewModel.SaveContentCommand.CanExecute(SaveReason.UserSave)))
        {
            return;
        }

        await ViewModel.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonNewAnimation control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonNewAnimation_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.NewAnimationCommand is null) || (!ViewModel.NewAnimationCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.NewAnimationCommand.ExecuteAsync(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonAnimationPaste control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonAnimationPaste_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.KeyEditor?.PasteDataCommand is null) || (!ViewModel.KeyEditor.PasteDataCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.KeyEditor.PasteDataCommand.ExecuteAsync(null);
    }

    /// <summary>Handles the Click event of the ButtonAnimationCopy control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationCopy_Click(object sender, EventArgs e)
    {
        if (ViewModel?.KeyEditor?.CopyDataCommand is null)
        {
            return;
        }

        KeyFrameCopyMoveData args = new()
        {
            KeyFrames = ViewModel.Selected,
            Operation = CopyMoveOperation.Copy
        };

        if (!ViewModel.KeyEditor.CopyDataCommand.CanExecute(args))
        {
            return;
        }

        ViewModel.KeyEditor.CopyDataCommand.Execute(args);
    }

    /// <summary>Handles the Click event of the ButtonAnimationCut control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimationCut_Click(object sender, EventArgs e)
    {
        if (ViewModel?.KeyEditor?.CopyDataCommand is null)
        {
            return;
        }

        KeyFrameCopyMoveData args = new()
        {
            KeyFrames = ViewModel.Selected,
            Operation = CopyMoveOperation.Move
        };

        if (!ViewModel.KeyEditor.CopyDataCommand.CanExecute(args))
        {
            return;
        }

        ViewModel.KeyEditor.CopyDataCommand.Execute(args);
    }

    /// <summary>Handles the Click event of the CheckAnimationEditTrack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckAnimationEditTrack_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ActivateKeyEditorCommand is null) || (!ViewModel.ActivateKeyEditorCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ActivateKeyEditorCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the CheckAnimationLoop control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckAnimationLoop_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.IsLooping = CheckAnimationLoop.Checked;
        ValidateButtons();
    }

    /// <summary>
    /// Function to ensure that visual states match the current tool setting.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void SetToolStates(IAnimationContent dataContext)
    {
        KryptonRibbonGroupButton button;

        if (dataContext.CommandContext is null)
        {
            foreach (KeyValuePair<string, WeakReference<KryptonRibbonGroupButton>> buttonItem in _toolButtons)
            {
                if (buttonItem.Value.TryGetTarget(out button))
                {
                    button.Checked = false;
                }
            }

            return;
        }

        if (!_toolButtons.TryGetValue(dataContext.CommandContext.Name, out WeakReference<KryptonRibbonGroupButton> buttonRef))
        {
            return;
        }

        if (!buttonRef.TryGetTarget(out button))
        {
            return;
        }

        button.Checked = true;
    }

    /// <summary>
    /// Function to update the zoom item menu to reflect the current selection.
    /// </summary>
    private void UpdateZoomMenu()
    {
        if (!_menuItems.TryGetValue(_zoomLevel, out ToolStripMenuItem currentItem))
        {
            return;
        }

        foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
        {
            item.Checked = false;
        }

        if (!currentItem.Checked)
        {
            currentItem.Checked = true;
        }

        ButtonZoomAnimation.TextLine1 = string.Format(Resources.GORANM_TEXT_ZOOM_BUTTON, _zoomLevel.GetName());
    }

    /// <summary>
    /// Function to build the list of available ribbon buttons for ease of access.
    /// </summary>
    private void GetRibbonButtons()
    {
        foreach (KryptonRibbonTab tab in RibbonAnimationContent.RibbonTabs)
        {
            foreach (KryptonRibbonGroup grp in tab.Groups)
            {
                // They really don't make it easy to get at the buttons do they?
                foreach (KryptonRibbonGroupLines container in grp.Items.OfType<KryptonRibbonGroupLines>())
                {
                    foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                    {
                        _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                    }

                    foreach (KryptonRibbonGroupNumericUpDown item in container.Items.OfType<KryptonRibbonGroupNumericUpDown>())
                    {
                        _ribbonNumerics.Add(new WeakReference<KryptonRibbonGroupNumericUpDown>(item));
                    }
                }

                foreach (KryptonRibbonGroupTriple container in grp.Items.OfType<KryptonRibbonGroupTriple>())
                {
                    foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                    {
                        _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                    }


                    foreach (KryptonRibbonGroupNumericUpDown item in container.Items.OfType<KryptonRibbonGroupNumericUpDown>())
                    {
                        _ribbonNumerics.Add(new WeakReference<KryptonRibbonGroupNumericUpDown>(item));
                    }
                }
            }
        }

        _toolButtons[KeyEditorContext.ContextName] = new WeakReference<KryptonRibbonGroupButton>(CheckAnimationEditTrack);
    }

    /// <summary>
    /// Function to disable or enable all of the ribbon buttons.
    /// </summary>
    /// <param name="enable"><b>true</b> to enable all buttons, <b>false</b> to disable.</param>
    private void EnableRibbon(bool enable)
    {
        foreach (WeakReference<KryptonRibbonGroupButton> item in _ribbonButtons)
        {
            if (!item.TryGetTarget(out KryptonRibbonGroupButton button))
            {
                continue;
            }

            button.Enabled = enable;
        }

        foreach (WeakReference<KryptonRibbonGroupNumericUpDown> item in _ribbonNumerics)
        {
            if (!item.TryGetTarget(out KryptonRibbonGroupNumericUpDown numeric))
            {
                continue;
            }

            numeric.Enabled = enable;
        }
    }

    /// <summary>
    /// Function to unassign the events for the data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.KeyEditor.PropertyChanged += KeyEditor_PropertyChanged;
        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the view when no data context is assigned.
    /// </summary>
    private void ResetDataContext()
    {
        UpdateZoomMenu();
        _currentUndoHandler = null;
        CheckAnimationEditTrack.Checked = false;
        CheckAnimationEditTrack.Checked = false;
        ItemZoomToWindow.Checked = true;
    }

    /// <summary>
    /// Function to initialize the view based on the data context.
    /// </summary>
    /// <param name="dataContext">The data context used to initialize.</param>
    private void InitializeFromDataContext(IAnimationContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        _currentUndoHandler = dataContext;
        CheckAnimationLoop.Checked = dataContext.IsLooping;
        UpdateZoomMenu();
    }

    /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [EventArgs] instance containing the event data.</param>
    private void ItemZoom_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem item = (ToolStripMenuItem)sender;

        if ((item.Tag is null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
        {
            item.Checked = false;
            return;
        }

        // Do not let us uncheck.
        if (_zoomLevel == zoom)
        {
            item.Checked = true;
            return;
        }

        _zoomLevel = zoom;
        UpdateZoomMenu();

        ContentRenderer?.MoveTo(new Vector2(ContentRenderer.ClientSize.Width * 0.5f, ContentRenderer.ClientSize.Height * 0.5f),
                                _zoomLevel.GetScale());
    }

    /// <summary>Raises the Load event.</summary>
    /// <param name="e">An EventArgs containing event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        ValidateButtons();
    }

    /// <summary>
    /// Function to validate the state of the buttons.
    /// </summary>
    public void ValidateButtons()
    {
        EnableRibbon(false);

        if (ViewModel is null)
        {
            return;
        }

        ButtonZoomAnimation.Enabled = true;
        CheckAnimationLoop.Enabled = ViewModel?.CurrentPanel is null;

        ButtonNewAnimation.Enabled = ViewModel?.NewAnimationCommand?.CanExecute(null) ?? false;
        ButtonSaveAnimation.Enabled = ViewModel?.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;
        ButtonAnimationLoadBack.Enabled = ViewModel?.LoadBackgroundImageCommand?.CanExecute(null) ?? false;
        ButtonAnimationClearBack.Enabled = ViewModel?.ClearBackgroundImageCommand?.CanExecute(null) ?? false;
        ButtonAnimationKeyUndo.Enabled = ButtonAnimationUndo.Enabled = _currentUndoHandler?.UndoCommand?.CanExecute(null) ?? false;
        ButtonAnimationKeyRedo.Enabled = ButtonAnimationRedo.Enabled = _currentUndoHandler?.RedoCommand?.CanExecute(null) ?? false;
        ButtonAnimationSprite.Enabled = ViewModel.LoadSpriteCommand?.CanExecute(null) ?? false;
        ButtonAnimPlay.Enabled = ViewModel.PlayAnimationCommand?.CanExecute(null) ?? false;
        ButtonAnimStop.Enabled = ViewModel.StopAnimationCommand?.CanExecute(null) ?? false;
        ButtonPrevKey.Enabled = ViewModel.PrevKeyCommand?.CanExecute(null) ?? false;
        ButtonNextKey.Enabled = ViewModel.NextKeyCommand?.CanExecute(null) ?? false;
        ButtonFirstKey.Enabled = ViewModel?.FirstKeyCommand?.CanExecute(null) ?? false;
        ButtonLastKey.Enabled = ViewModel?.LastKeyCommand?.CanExecute(null) ?? false;
        ButtonAnimationProperties.Enabled = ViewModel?.ShowAnimationPropertiesCommand?.CanExecute(null) ?? false;
        ButtonAnimationGoBack.Enabled = CheckAnimationEditTrack.Enabled = ViewModel?.ActivateKeyEditorCommand?.CanExecute(null) ?? false;
        ButtonAnimationSetKeyframe.Enabled = (ViewModel?.KeyEditor?.SetKeyCommand?.CanExecute(null) ?? false);
        ButtonAddTrack.Enabled = ViewModel.ShowAddTrackCommand?.CanExecute(null) ?? false;
        ButtonRemoveTrack.Enabled = ViewModel.DeleteTrackCommand?.CanExecute(null) ?? false;
        ButtonAnimationRemoveKeyframes.Enabled = ViewModel.KeyEditor?.RemoveKeyCommand?.CanExecute(null) ?? false;
        ButtonAnimationClearKeyframes.Enabled = ViewModel.KeyEditor?.ClearKeysCommand?.CanExecute(null) ?? false;
        ButtonAnimationClear.Enabled = ViewModel.ClearAnimationCommand?.CanExecute(null) ?? false;
        KeyFrameCopyMoveData copyArgs = new()
        {
            KeyFrames = ViewModel.Selected,
            Operation = CopyMoveOperation.Copy
        };
        ButtonAnimationCopy.Enabled = ViewModel?.KeyEditor?.CopyDataCommand?.CanExecute(copyArgs) ?? false;
        copyArgs.Operation = CopyMoveOperation.Move;
        ButtonAnimationCut.Enabled = ViewModel?.KeyEditor?.CopyDataCommand?.CanExecute(copyArgs) ?? false;
        ButtonAnimationPaste.Enabled = ViewModel?.KeyEditor?.PasteDataCommand?.CanExecute(copyArgs) ?? false;
    }

    /// <summary>
    /// Function to reset the zoom back to the default.
    /// </summary>
    public void ResetZoom()
    {
        _zoomLevel = ZoomLevels.ToWindow;
        UpdateZoomMenu();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IAnimationContent dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;
        ValidateButtons();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.KeyEditor.PropertyChanged += KeyEditor_PropertyChanged;
        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }



    /// <summary>Initializes a new instance of the FormRibbon class.</summary>
    public FormRibbon()
    {
        InitializeComponent();

        GetRibbonButtons();

        foreach (ToolStripMenuItem menuItem in MenuZoom.Items.OfType<ToolStripMenuItem>())
        {
            if (!Enum.TryParse(menuItem.Tag.ToString(), out ZoomLevels level))
            {
                menuItem.Enabled = false;
                continue;
            }

            menuItem.Text = level.GetName();
            _menuItems[level] = menuItem;
        }
    }

}
