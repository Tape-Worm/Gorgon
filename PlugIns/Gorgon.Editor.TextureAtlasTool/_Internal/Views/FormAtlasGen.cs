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
// Created: April 24, 2019 6:55:01 PM
// 
#endregion

using System.ComponentModel;
using System.Diagnostics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The main view for the tool.
/// </summary>
internal partial class FormAtlasGen
    : EditorToolBaseForm, IDataContext<ITextureAtlas>
{
    #region Variables.
    // The file selector for sprites.
    private FormSpriteSelector _spriteSelector;
    // Flag to indicate that the dialog should close when the file selector closes.
    private bool _closeOnFileSelectionClose = true;
    #endregion

    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ITextureAtlas ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the settings for the plug in.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextureAtlasSettings Settings
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls()
    {
        ButtonGenerate.Enabled = ViewModel?.GenerateCommand?.CanExecute(null) ?? false;
        ButtonCalculateSize.Enabled = ViewModel?.CalculateSizesCommand?.CanExecute(null) ?? false;
        ButtonPrevArray.Visible = ViewModel.Atlas is not null;
        ButtonNextArray.Visible = ViewModel.Atlas is not null;
        ButtonPrevArray.Enabled = ViewModel?.PrevPreviewCommand?.CanExecute(null) ?? false;
        ButtonNextArray.Enabled = ViewModel?.NextPreviewCommand?.CanExecute(null) ?? false;
        ButtonOk.Enabled = ViewModel?.CommitAtlasCommand?.CanExecute(null) ?? false;
    }

    /// <summary>
    /// Function to update the text on the label that allows previewing of the atlas.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void UpdateLabelArrayText(ITextureAtlas dataContext)
    {
        if ((dataContext?.Atlas?.Textures is null) || (dataContext.Atlas.Textures.Count == 0))
        {
            LabelArray.Text = Resources.GORTAG_TEXT_NO_ATLAS;
            return;
        }

        GorgonTexture2D texture = dataContext.Atlas.Textures[dataContext.PreviewTextureIndex].Texture;

        LabelArray.Text = string.Format(Resources.GORTAG_TEXT_ARRAY_TEXTURE_COUNT,
                                        dataContext.PreviewArrayIndex + 1,
                                        texture.ArrayCount,
                                        dataContext.PreviewTextureIndex + 1,
                                        dataContext.Atlas.Textures.Count);
    }

    /// <summary>
    /// Function to display the file selector.
    /// </summary>
    private void ShowFileSelector()
    {
        if ((ViewModel?.SpriteFiles is null) || (_spriteSelector is not null))
        {
            return;
        }

        using (_spriteSelector = new FormSpriteSelector())
        {                
            _spriteSelector.SetDataContext(ViewModel.SpriteFiles);
            _spriteSelector.SetGraphicsContext(GraphicsContext);
            if (_spriteSelector.ShowDialog(this) == DialogResult.Cancel)
            {
                if (_closeOnFileSelectionClose)
                {
                    Close();
                }
            }
        }

        _closeOnFileSelectionClose = false;
        _spriteSelector = null;
    }

    /// <summary>Handles the Click event of the ButtonNextArray control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonNextArray_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.NextPreviewCommand is null) || (!ViewModel.NextPreviewCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.NextPreviewCommand.Execute(null);
    }

    /// <summary>Handles the Click event of the ButtonPrevArray control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonPrevArray_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.PrevPreviewCommand is null) || (!ViewModel.PrevPreviewCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.PrevPreviewCommand.Execute(null);
    }

    /// <summary>Handles the Click event of the ButtonGenerate control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonGenerate_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.GenerateCommand is null) || (!ViewModel.GenerateCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.GenerateCommand.Execute(null);
    }

    /// <summary>Handles the Click event of the ButtonLoadSprites control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonLoadSprites_Click(object sender, EventArgs e) => ShowFileSelector();

    /// <summary>Handles the ValueChanged event of the NumericTextureWidth control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericTextureWidth_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.MaxTextureSize = new DX.Size2((int)NumericTextureWidth.Value, ViewModel.MaxTextureSize.Height);
    }

    /// <summary>Handles the ValueChanged event of the NumericTextureHeight control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericTextureHeight_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.MaxTextureSize = new DX.Size2(ViewModel.MaxTextureSize.Width, (int)NumericTextureHeight.Value);
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

        ViewModel.MaxArrayCount = (int)NumericArrayIndex.Value;            
    }

    /// <summary>Handles the ValueChanged event of the NumericPadding control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericPadding_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Padding = (int)NumericPadding.Value;
    }

    /// <summary>Handles the Click event of the ButtonOk control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonOk_Click(object sender, EventArgs e)
    {
        var args = new CancelEventArgs(false);

        if ((ViewModel?.CommitAtlasCommand is null) || (!ViewModel.CommitAtlasCommand.CanExecute(args)))
        {
            return;
        }

        ViewModel.CommitAtlasCommand.Execute(args);

        if (!args.Cancel)
        {
            Close();
        }
    }

    /// <summary>Handles the Click event of the ButtonCalculateSize control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonCalculateSize_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.CalculateSizesCommand is null) || (!ViewModel.CalculateSizesCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.CalculateSizesCommand.Execute(null);
    }

    /// <summary>Handles the Click event of the ButtonFolderBrowse control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFolderBrowse_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.SelectFolderCommand is null) || (!ViewModel.SelectFolderCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.SelectFolderCommand.Execute(null);
    }

    /// <summary>Handles the TextChanged event of the TextBaseTextureName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextBaseTextureName_TextChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.BaseTextureName = TextBaseTextureName.Text;
    }

    /// <summary>Function called when a property was changed on the data context.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method when detecting property changes on the data context instead of assigning their own event handlers.</remarks>
    protected override void OnPropertyChanged(string propertyName) 
    {
        switch (propertyName)
        {
            case nameof(ITextureAtlas.PreviewTextureIndex):
            case nameof(ITextureAtlas.PreviewArrayIndex):
            case nameof(ITextureAtlas.Atlas):
                UpdateLabelArrayText(ViewModel);
                break;
            case nameof(ITextureAtlas.LoadedSpriteCount):
                LabelSpriteCount.Text = string.Format(ViewModel.LoadedSpriteCount == 0 ? Resources.GORTAG_TEXT_NO_SPRITES : Resources.GORTAG_TEXT_SPRITE_COUNT, ViewModel.LoadedSpriteCount);
                break;
            case nameof(ITextureAtlas.Padding):
                decimal padding = ViewModel.Padding.Min((int)NumericPadding.Maximum).Max((int)NumericPadding.Minimum);

                if (padding != NumericPadding.Value)
                {
                    NumericPadding.Value = padding;
                }
                break;
            case nameof(ITextureAtlas.MaxTextureSize):
                decimal w = ViewModel.MaxTextureSize.Width.Min((int)NumericTextureWidth.Maximum).Max((int)NumericTextureWidth.Minimum);
                decimal h = ViewModel.MaxTextureSize.Height.Min((int)NumericTextureHeight.Maximum).Max((int)NumericTextureHeight.Minimum);

                if (NumericTextureWidth.Value != w)
                {
                    NumericTextureWidth.Value = w;
                }

                if (NumericTextureHeight.Value != h)
                {
                    NumericTextureHeight.Value = h;
                }
                break;
            case nameof(ITextureAtlas.MaxArrayCount):
                decimal arrayCount = ViewModel.MaxArrayCount.Min((int)NumericArrayIndex.Maximum).Max((int)NumericArrayIndex.Minimum);

                if (NumericArrayIndex.Value != arrayCount)
                {
                    NumericArrayIndex.Value = arrayCount;
                }
                break;
            case nameof(ITextureAtlas.BaseTextureName):
                if (!string.Equals(TextBaseTextureName.Text, ViewModel.BaseTextureName, StringComparison.CurrentCulture))
                {
                    TextBaseTextureName.Text = ViewModel.BaseTextureName;
                }
                break;
            case nameof(ITextureAtlas.OutputPath):
                if (!string.Equals(TextOutputFolder.Text, ViewModel.OutputPath, StringComparison.CurrentCulture))
                {
                    TextOutputFolder.Text = ViewModel.OutputPath;
                }
                break;
        }

        ValidateControls();
    }

    /// <summary>
    /// Function to reset the control back to its default state.
    /// </summary>
    private void ResetDataContext()
    {
        ShowFileSelector();
        LabelSpriteCount.Text = Resources.GORTAG_TEXT_NO_SPRITES;
        NumericTextureHeight.Value = NumericTextureWidth.Value = NumericTextureWidth.Minimum;
        NumericArrayIndex.Value = NumericArrayIndex.Minimum;
        TextBaseTextureName.Text = TextOutputFolder.Text = string.Empty;
        ButtonFolderBrowse.Enabled = ButtonGenerate.Enabled = ButtonCalculateSize.Enabled = false;
        UpdateLabelArrayText(null);
    }

    /// <summary>
    /// Function to initialize the control based on the data context.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void InitializeFromDataContext(ITextureAtlas dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        LabelSpriteCount.Text = string.Format(dataContext.LoadedSpriteCount == 0 ? Resources.GORTAG_TEXT_NO_SPRITES : Resources.GORTAG_TEXT_SPRITE_COUNT, dataContext.LoadedSpriteCount);
        TextOutputFolder.Text = dataContext.OutputPath;
        NumericPadding.Value = dataContext.Padding.Min((int)NumericPadding.Maximum).Max((int)NumericPadding.Minimum);
        NumericTextureWidth.Value = dataContext.MaxTextureSize.Width.Min((int)NumericTextureWidth.Maximum).Max((int)NumericTextureWidth.Minimum);
        NumericTextureHeight.Value = dataContext.MaxTextureSize.Height.Min((int)NumericTextureHeight.Maximum).Max((int)NumericTextureHeight.Minimum);
        NumericArrayIndex.Value = dataContext.MaxArrayCount.Min((int)NumericArrayIndex.Maximum).Max((int)NumericArrayIndex.Minimum);
        TextBaseTextureName.Text = dataContext.BaseTextureName;
        UpdateLabelArrayText(dataContext);
    }

    /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        ViewModel?.Unload();
    }

    /// <summary>Raises the <see cref="Control.Resize"/> event.</summary>
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

        if (IsDesignTime)
        {
            return;
        }

        WindowState = Settings.IsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
        ShowFileSelector();
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

        ViewModel?.Load();

        ValidateControls();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ITextureAtlas dataContext)
    {
        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;
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

        Debug.Assert(ViewModel is not null, "No datacontext");

        NumericTextureWidth.Maximum = graphicsContext.VideoAdapter.MaxTextureWidth;
        NumericTextureHeight.Maximum = graphicsContext.VideoAdapter.MaxTextureHeight;
        NumericArrayIndex.Maximum = graphicsContext.VideoAdapter.MaxTextureArrayCount;

        var atlasRenderer = new Renderer(graphicsContext.Renderer2D, swapChain, ViewModel);
        AddRenderer(atlasRenderer.Name, atlasRenderer);
        SwitchRenderer(atlasRenderer.Name);
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FormAtlasGen"/> class.</summary>
    public FormAtlasGen() => InitializeComponent();

    /// <summary>Initializes a new instance of the <see cref="FormAtlasGen"/> class.</summary>
    /// <param name="settings">The settings for the plug in.</param>
    public FormAtlasGen(TextureAtlasSettings settings)
        : this() => Settings = settings;
    #endregion
}
