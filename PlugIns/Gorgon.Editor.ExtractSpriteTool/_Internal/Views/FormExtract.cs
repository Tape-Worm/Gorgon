﻿
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
// Created: April 24, 2019 6:55:01 PM
// 

using System.ComponentModel;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// The main view for the tool
/// </summary>
internal partial class FormExtract
    : EditorToolBaseForm, IDataContext<IExtract>
{

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    /// <summary>Property to return the data context assigned to this view.</summary>
    public IExtract ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the settings for the plug-in.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ExtractSpriteToolSettings Settings
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls()
    {
        if (ViewModel?.Texture is null)
        {
            foreach (Control ctl in Controls)
            {
                ctl.Enabled = false;
            }
            return;
        }

        TableSpritePreview.Visible = ViewModel.IsInSpritePreview;
        ButtonNextSprite.Enabled = ViewModel.NextPreviewSpriteCommand?.CanExecute(null) ?? false;
        ButtonPrevSprite.Enabled = ViewModel.PrevPreviewSpriteCommand?.CanExecute(null) ?? false;
        CheckPreviewSprites.Enabled = (ViewModel.Sprites is not null) && (ViewModel.Sprites.Count > 0);
        TableSkipMaskColor.Enabled = (!ViewModel.IsGenerating) && (ViewModel.SetEmptySpriteMaskColorCommand?.CanExecute(null) ?? false);

        ButtonOk.Enabled = ViewModel.SaveSpritesCommand?.CanExecute(null) ?? false;
        LabelArrayRange.Visible = LabelArrayCount.Visible = LabelArrayStart.Visible = NumericArrayIndex.Visible = NumericArrayCount.Visible = ViewModel.Texture?.Texture.ArrayCount > 1;
    }

    /// <summary>Handles the Click event of the SkipColor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void SkipColor_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.SetEmptySpriteMaskColorCommand is null) || (!ViewModel.SetEmptySpriteMaskColorCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.SetEmptySpriteMaskColorCommand.Execute(null);
    }

    /// <summary>Handles the ValueChanged event of the NumericCellWidth control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericCellWidth_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.CellSize = new GorgonPoint((int)NumericCellWidth.Value, ViewModel.CellSize.Y);
    }

    /// <summary>Handles the ValueChanged event of the NumericCellHeight control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericCellHeight_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.CellSize = new GorgonPoint(ViewModel.CellSize.X, (int)NumericCellHeight.Value);
    }

    /// <summary>Handles the ValueChanged event of the NumericOffsetX control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericOffsetX_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.GridOffset = new GorgonPoint((int)NumericOffsetX.Value, ViewModel.GridOffset.Y);
    }

    /// <summary>Handles the ValueChanged event of the NumericOffsetY control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericOffsetY_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.GridOffset = new GorgonPoint(ViewModel.GridOffset.X, (int)NumericOffsetY.Value);
    }

    /// <summary>Handles the ValueChanged event of the NumericColumnCount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericColumnCount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.GridSize = new GorgonPoint((int)NumericColumnCount.Value, ViewModel.GridSize.Y);
    }

    /// <summary>Handles the ValueChanged event of the NumericRowCount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericRowCount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.GridSize = new GorgonPoint(ViewModel.GridSize.X, (int)NumericRowCount.Value);
    }

    /// <summary>Handles the ValueChanged event of the NumericArrayIndex control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericArrayIndex_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.StartArrayIndex = (int)NumericArrayIndex.Value;
    }

    /// <summary>Handles the ValueChanged event of the NumericArrayCount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericArrayCount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ArrayCount = (int)NumericArrayCount.Value;
    }

    /// <summary>Handles the Click event of the ButtonGenerate control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonGenerate_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.GenerateSpritesCommand is null) || (!ViewModel.GenerateSpritesCommand.CanExecute(null)))
        {
            return;
        }

        // Turn off all controls so we don't get any interference.
        PanelSkip.Enabled = TableExtractParams.Enabled = TableSpritePreview.Enabled = TableSkipMaskColor.Enabled =
        ButtonOk.Enabled = ButtonGenerate.Enabled = false;
        try
        {
            await ViewModel.GenerateSpritesCommand.ExecuteAsync(null);
        }
        finally
        {
            // Turn on all controls.
            PanelSkip.Enabled = TableExtractParams.Enabled = TableSpritePreview.Enabled = TableSkipMaskColor.Enabled =
            ButtonOk.Enabled = ButtonGenerate.Enabled = true;
            ValidateControls();
        }
    }

    /// <summary>Handles the Click event of the ButtonOk control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonOk_Click(object sender, EventArgs e)
    {
        SaveSpritesArgs args = new();

        if ((ViewModel?.SaveSpritesCommand is null) || (!ViewModel.SaveSpritesCommand.CanExecute(args)))
        {
            return;
        }

        ViewModel.SaveSpritesCommand.Execute(args);

        if (args.Cancel)
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }

    /// <summary>Handles the Click event of the ButtonNextSprite control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonNextSprite_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.NextPreviewSpriteCommand is null) || (!ViewModel.NextPreviewSpriteCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.NextPreviewSpriteCommand.Execute(null);
        ValidateControls();
    }

    /// <summary>Handles the Click event of the ButtonPrevSprite control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonPrevSprite_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.PrevPreviewSpriteCommand is null) || (!ViewModel.PrevPreviewSpriteCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.PrevPreviewSpriteCommand.Execute(null);
        ValidateControls();
    }

    /// <summary>Handles the CheckedChanged event of the CheckPreviewSprites control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckPreviewSprites_CheckedChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.IsInSpritePreview = CheckPreviewSprites.Checked;
        ValidateControls();
    }

    /// <summary>Handles the Click event of the CheckSkipEmpty control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckSkipEmpty_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.SkipEmpty = CheckSkipEmpty.Checked;
        ValidateControls();
    }

    /// <summary>Processes a command key.</summary>
    /// <param name="msg">A <see cref="Message"/>, passed by reference, that represents the Win32 message to process.</param>
    /// <param name="keyData">One of the <see cref="Keys"/> values that represents the key to process.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the keystroke was processed and consumed by the control; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span> to allow further processing.
    /// </returns>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if ((keyData == Keys.Escape) && (ViewModel?.CancelSpriteGenerationCommand is not null) && (ViewModel.CancelSpriteGenerationCommand.CanExecute(null)))
        {
            ViewModel.CancelSpriteGenerationCommand.Execute(null);
            ValidateControls();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>Handles the PreviewKeyDown event of the PanelRender control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
    private void PanelRender_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.S:
                if (CheckPreviewSprites.Enabled)
                {
                    CheckPreviewSprites.Checked = !CheckPreviewSprites.Checked;
                }
                break;
            case Keys.G:
                ButtonGenerate.PerformClick();
                break;
            case Keys.Left:
            case Keys.Oemcomma:
                if (ViewModel.IsInSpritePreview)
                {
                    ButtonPrevSprite.PerformClick();
                }
                else
                {
                    ViewModel.StartArrayIndex--;
                }
                e.IsInputKey = true;
                break;
            case Keys.Right:
            case Keys.OemPeriod:
                if (ViewModel.IsInSpritePreview)
                {
                    ButtonNextSprite.PerformClick();
                }
                else
                {
                    ViewModel.StartArrayIndex++;
                }
                e.IsInputKey = true;
                break;
        }
        ValidateControls();
    }

    /// <summary>Handles the MouseWheel event of the PanelRender control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private void PanelRender_MouseWheel(object sender, MouseEventArgs e)
    {
        if ((ViewModel is null) || (ViewModel.IsGenerating))
        {
            return;
        }

        if (ViewModel.IsInSpritePreview)
        {
            if (e.Delta < 0)
            {
                ButtonPrevSprite.PerformClick();
            }
            else if (e.Delta > 0)
            {
                ButtonNextSprite.PerformClick();
            }
            return;
        }

        if (e.Delta < 0)
        {
            NumericArrayIndex.Value--;
        }
        else if (e.Delta > 0)
        {
            NumericArrayIndex.Value++;
        }
    }

    /// <summary>
    /// Function to update the sprite preview label.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void UpdateSpritePreviewLabel(IExtract dataContext)
    {
        if (dataContext is null)
        {
            LabelSprite.Text = string.Format(Resources.GOREST_TEXT_SPRITE_PREVIEW, 0, 0);
            return;
        }

        LabelSprite.Text = string.Format(Resources.GOREST_TEXT_SPRITE_PREVIEW, dataContext.CurrentPreviewSprite + 1, dataContext.SpritePreviewCount);
    }

    /// <summary>Function called when a property was changed on the data context.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method when detecting property changes on the data context instead of assigning their own event handlers.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(IExtract.StartArrayIndex):
                int currentIndex = (int)NumericArrayIndex.Value;
                if (currentIndex != ViewModel.StartArrayIndex)
                {
                    NumericArrayIndex.Value = ViewModel.StartArrayIndex;
                }
                break;
            case nameof(IExtract.SpritePreviewCount):
            case nameof(IExtract.CurrentPreviewSprite):
                UpdateSpritePreviewLabel(ViewModel);
                break;
            case nameof(IExtract.IsInSpritePreview):
                CheckPreviewSprites.Checked = ViewModel.IsInSpritePreview;
                break;
            case nameof(IExtract.GridOffset):
                NumericCellWidth.Maximum = ViewModel.Texture.Width - ViewModel.GridOffset.X;
                NumericCellHeight.Maximum = ViewModel.Texture.Height - ViewModel.GridOffset.Y;
                break;
            case nameof(IExtract.CellSize):
                NumericOffsetX.Maximum = ViewModel.Texture.Width - ViewModel.CellSize.X;
                NumericOffsetY.Maximum = ViewModel.Texture.Height - ViewModel.CellSize.Y;
                break;
            case nameof(IExtract.MaxGridSize):
                NumericColumnCount.Maximum = ViewModel.MaxGridSize.X;
                NumericRowCount.Maximum = ViewModel.MaxGridSize.Y;
                break;
            case nameof(IExtract.MaxArrayCount):
                NumericArrayCount.Maximum = ViewModel.MaxArrayCount;
                break;
            case nameof(IExtract.MaxArrayIndex):
                NumericArrayIndex.Maximum = ViewModel.MaxArrayIndex;
                break;
            case nameof(IExtract.SkipMaskColor):
                SkipColor.Color = ViewModel.SkipMaskColor;
                break;
        }

        ValidateControls();
    }

    /// <summary>
    /// Function to initialize the form based on its data context.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void InitializeFromDataContext(IExtract dataContext)
    {
        if (dataContext is null)
        {
            UpdateSpritePreviewLabel(null);
            return;
        }

        SkipColor.Color = dataContext.SkipMaskColor;
        CheckSkipEmpty.Checked = dataContext.SkipEmpty;
        NumericOffsetX.Maximum = (dataContext.Texture.Width - dataContext.CellSize.X).Max(0);
        NumericOffsetY.Maximum = (dataContext.Texture.Height - dataContext.CellSize.Y).Max(0);
        NumericCellWidth.Maximum = (dataContext.Texture.Width - dataContext.GridOffset.X).Max(1);
        NumericCellHeight.Maximum = (dataContext.Texture.Height - dataContext.GridOffset.Y).Max(1);
        NumericArrayCount.Maximum = dataContext.MaxArrayCount.Max(1);
        NumericArrayIndex.Maximum = dataContext.MaxArrayIndex.Max(0);
        NumericColumnCount.Maximum = dataContext.MaxGridSize.X;
        NumericRowCount.Maximum = dataContext.MaxGridSize.Y;

        NumericCellWidth.Value = dataContext.CellSize.X.Min((int)NumericCellWidth.Maximum).Max(1);
        NumericCellHeight.Value = dataContext.CellSize.Y.Min((int)NumericCellHeight.Maximum).Max(1);
        NumericColumnCount.Value = dataContext.GridSize.X.Min((int)NumericColumnCount.Maximum).Max(1);
        NumericRowCount.Value = dataContext.GridSize.Y.Min((int)NumericRowCount.Maximum).Max(1);
        NumericOffsetX.Value = dataContext.GridOffset.X.Min((int)NumericOffsetX.Maximum).Max(0);
        NumericOffsetY.Value = dataContext.GridOffset.Y.Min((int)NumericOffsetY.Maximum).Max(0);
        NumericArrayCount.Value = dataContext.ArrayCount.Min((int)NumericArrayCount.Maximum).Max(1);
        NumericArrayIndex.Value = dataContext.StartArrayIndex.Min((int)NumericArrayIndex.Maximum).Max(0);

        UpdateSpritePreviewLabel(dataContext);
    }

    /// <summary>Raises the <see cref="Form.Resize"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (Settings is null)
        {
            return;
        }

        Settings.IsMaximized = WindowState == FormWindowState.Maximized;
    }

    /// <summary>Raises the Shown event.</summary>
    /// <param name="e">An EventArgs containing event data.</param>
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if ((Settings is not null) && (Settings.IsMaximized))
        {
            WindowState = FormWindowState.Maximized;
        }
    }

    /// <summary>Function to perform custom graphics set up.</summary>
    /// <param name="graphicsContext">The graphics context for the application.</param>
    /// <param name="swapChain">The swap chain used to render into the UI.</param>
    /// <remarks>
    ///   <para>
    /// This method allows tool plug-in implementors to setup additional functionality for custom graphics rendering.
    /// </para>
    ///   <para>
    /// Resources created by this method should be cleaned up in the <see cref="EditorToolBaseForm.OnShutdownGraphics"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="M:Gorgon.Editor.UI.Views.EditorToolBaseForm.OnShutdownGraphics" />
    protected override void OnSetupGraphics(IGraphicsContext graphicsContext, GorgonSwapChain swapChain)
    {
        base.OnSetupGraphics(graphicsContext, swapChain);

        Renderer extractRenderer = new(graphicsContext.Renderer2D, swapChain, ViewModel);
        AddRenderer(extractRenderer.Name, extractRenderer);
        SwitchRenderer(extractRenderer.Name);
    }

    /// <summary>Raises the Load event.</summary>
    /// <param name="e">An EventArgs containing event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        ValidateControls();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IExtract dataContext)
    {
        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;
    }

    /// <summary>Initializes a new instance of the <see cref="FormExtract"/> class.</summary>
    /// <param name="settings">The settings.</param>
    public FormExtract(ExtractSpriteToolSettings settings)
        : this()
    {
        Settings = settings;
        PanelRender.MouseWheel += PanelRender_MouseWheel;
    }

    /// <summary>Initializes a new instance of the <see cref="FormExtract"/> class.</summary>
    public FormExtract() => InitializeComponent();

}
