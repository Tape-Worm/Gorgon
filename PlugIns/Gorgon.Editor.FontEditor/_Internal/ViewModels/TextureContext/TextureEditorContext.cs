using DX = SharpDX;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Diagnostics;
using System.ComponentModel;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A context editor for modifying the textures on the font.
/// </summary>
internal class TextureEditorContext
    : EditorContext<TextureEditorContextParameters>, ITextureEditorContext
{
    #region Constants.
    // The name of the context.
    private const string ContextName = "TextureEditor";
    #endregion

    #region Variables.
    // Flag to indicate that the font uses premultiplied alpha for its textures.
    private bool _premultipliedAlpha;
    // The service used to generate fonts.
    private FontService _fontService;
    // The undo service.
    private IUndoService _undoService;
    // The selected texture and array indices.
    private int _selectedTextureIndex;
    private int _selectedArrayIndex;
    // The currently hosted panel view model.
    private IHostedPanelViewModel _currentPanel;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the solid brush viewmodel.
    /// </summary>
    public IFontSolidBrush SolidBrush
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the pattern brush viewmodel.
    /// </summary>
    public IFontPatternBrush PatternBrush
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the gradient brush viewmodel.
    /// </summary>
    public IFontGradientBrush GradientBrush
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the texture brush view model.
    /// </summary>
    public IFontTextureBrush TextureBrush
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the font texture size viewmodel.
    /// </summary>
    public IFontTextureSize FontTextureSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the font padding viewmodel.
    /// </summary>
    public IFontPadding FontPadding
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the current brush.
    /// </summary>
    public GorgonGlyphBrush CurrentBrush => _fontService.WorkerFont?.Brush ?? FontSolidBrush.DefaultBrush;

    /// <summary>
    /// Property to set or return the current hosted panel view model.
    /// </summary>
    public IHostedPanelViewModel CurrentPanel
    {
        get => _currentPanel;
        set
        {
            if (_currentPanel == value)
            {
                return;
            }

            if (_currentPanel is not null)
            {
                _currentPanel.WaitPanelActivated -= CurrentPanel_WaitPanelActivated;
                _currentPanel.WaitPanelDeactivated -= CurrentPanel_WaitPanelDeactivated;
                _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;                                        
            }

            OnPropertyChanging();
            _currentPanel?.Unload();
            _currentPanel = value;
            _currentPanel?.Load();
            OnPropertyChanged();

            if (_currentPanel is null)
            {
                return;
            }

            _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
            _currentPanel.WaitPanelActivated += CurrentPanel_WaitPanelActivated;
            _currentPanel.WaitPanelDeactivated += CurrentPanel_WaitPanelDeactivated;
        }
    }

    /// <summary>
    /// Property to return the textures for the font.
    /// </summary>
    public IReadOnlyList<GorgonTexture2DView> Textures => _fontService?.Textures ?? [];

    /// <summary>
    /// Property to return the total number of textures, and array indices in the font.
    /// </summary>
    public int TotalTextureArrayCount => _fontService?.TotalTextureArrayCount ?? 0;

    /// <summary>Property to return the context name.</summary>
    /// <remarks>This value is used as a unique ID for the context.</remarks>
    public override string Name => ContextName;

    /// <summary>
    /// Property to return whether the textures use premultiplied alpha or not.
    /// </summary>
    public bool UsePremultipliedAlpha
    {
        get => _premultipliedAlpha;
        set
        {
            if (_premultipliedAlpha == value)
            {
                return;
            }

            OnPropertyChanging();
            _premultipliedAlpha = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the currently selected texture.</summary>
    public int SelectedTexture
    {
        get => _selectedTextureIndex.Max(0).Min(Textures.Count - 1);
        set
        {
            if (value == _selectedArrayIndex)
            {
                return;
            }

            OnPropertyChanging();
            _selectedTextureIndex = value.Max(0).Min(Textures.Count - 1);
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the currently selected array index for the <see cref="SelectedTexture" />.</summary>
    public int SelectedArrayIndex
    {
        get
        {
            if ((_selectedTextureIndex >= Textures.Count)
                || (_selectedTextureIndex < 0))
            {
                return 0;
            }

            GorgonTexture2DView texture = Textures[_selectedTextureIndex];

            return _selectedArrayIndex.Max(0).Min(texture.Texture.ArrayCount - 1);
        }
        set
        {
            if (value == _selectedArrayIndex)
            {
                return;
            }

            if ((_selectedTextureIndex >= Textures.Count)
                || (_selectedTextureIndex < 0))
            {
                return;
            }

            GorgonTexture2DView texture = Textures[_selectedTextureIndex];
            OnPropertyChanging();
            _selectedArrayIndex = value.Max(0).Min(texture.Texture.ArrayCount - 1);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the size of the font textures.
    /// </summary>
    public DX.Size2 TextureSize => new(_fontService?.WorkerFont?.TextureWidth ?? 0, _fontService?.WorkerFont?.TextureHeight ?? 0);

    /// <summary>
    /// Property to return the padding around the glyphs on the textures, in pixels.
    /// </summary>
    public int FontGlyphPadding => _fontService?.WorkerFont?.PackingSpacing ?? 0;

    /// <summary>
    /// Property to return the command used to change the premultiplied state on the font.
    /// </summary>
    public IEditorAsyncCommand<bool> SetPremultipliedCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the next font texture.
    /// </summary>
    public IEditorCommand<object> NextTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the previous font texture.
    /// </summary>
    public IEditorCommand<object> PrevTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the first font texture.
    /// </summary>
    public IEditorCommand<object> FirstTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the last font texture.
    /// </summary>
    public IEditorCommand<object> LastTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture size editor.
    /// </summary>
    public IEditorCommand<object> ActivateTextureSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the padding editor.
    /// </summary>
    public IEditorCommand<object> ActivatePaddingCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the solid brush editor.
    /// </summary>
    public IEditorCommand<object> ActivateSolidBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the pattern brush editor.
    /// </summary>
    public IEditorCommand<object> ActivatePatternBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the gradient brush editor.
    /// </summary>
    public IEditorCommand<object> ActivateGradientBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture brush editor.
    /// </summary>
    public IEditorCommand<object> ActivateTextureBrushCommand
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the WaitPanelDeactivated event of the CurrentPanel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void CurrentPanel_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

    /// <summary>Currents the panel wait panel activated.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void CurrentPanel_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e);

    /// <summary>Handles the PropertyChanged event of the CurrentPanel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void CurrentPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IHostedPanelViewModel.IsActive):
                if ((CurrentPanel is not null) && (!CurrentPanel.IsActive))
                {
                    CurrentPanel.Unload();
                    CurrentPanel = null;
                }
                break;
        }
    }

    /// <summary>
    /// Function to determine if the premultiplied alpha setting can be changed or not.
    /// </summary>
    /// <param name="value">The value for setting the state.</param>
    /// <returns><b>true</b> if the premultiplied state can be changed, <b>false</b> if not.</returns>
    private bool CanChangePremultiplied(bool value) => CurrentPanel is null;

    /// <summary>
    /// Function to change the font textures to/from premultiplied alpha.
    /// </summary>
    /// <param name="value"><b>true</b> to use premultiplied alpha, <b>false</b> to turn it off.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoChangePremultiplied(bool value)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetValueAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                UsePremultipliedAlpha = args.UsePremultipliedTextures;
                await _fontService.UpdateFontAsync(args);

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_STYLE);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        if (value == _fontService.WorkerFont.UsePremultipliedTextures)
        {
            return;
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetValueAsync(args);

        GorgonFontInfo undoArgs = new(_fontService.WorkerFont);
        GorgonFontInfo redoArgs = undoArgs with
        {
            UsePremultipliedTextures = value
        };

        if (!(await SetValueAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_SET_PREMULTIPLIED, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(ITextureEditorContext.Name));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the selected texture can be moved to the previous one.
    /// </summary>
    /// <returns></returns>
    private bool CanMovePrev() => (CurrentPanel is null) && ((_selectedTextureIndex > 0) || (_selectedArrayIndex > 0));

    /// <summary>
    /// Function to move to the previous texture selection.
    /// </summary>
    private void DoMovePrev()
    {
        try
        {
            if (SelectedArrayIndex == 0)
            {
                SelectedTexture--;
                SelectedArrayIndex = Textures[SelectedTexture].ArrayCount - 1;
                return;
            }

            SelectedArrayIndex--;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error moving to the previous texture.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the selected texture can be moved to the previous one.
    /// </summary>
    /// <returns></returns>
    private bool CanMoveNext()
    {
        if (CurrentPanel is not null)
        {
            return false;
        }

        if ((SelectedTexture < 0) || (SelectedTexture >= Textures.Count ))
        {
            return false;
        }

        return _selectedArrayIndex < Textures[SelectedTexture].ArrayCount - 1;
    }

    /// <summary>
    /// Function to move to the previous texture selection.
    /// </summary>
    private void DoMoveNext()
    {
        try
        {
            GorgonTexture2DView texture = Textures[SelectedTexture];

            if (SelectedArrayIndex == texture.ArrayCount - 1)
            {
                SelectedTexture++;
                SelectedArrayIndex = 0;
                return;
            }

            SelectedArrayIndex++;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error moving to the next texture.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the selected texture can be moved to the first one.
    /// </summary>
    /// <returns></returns>
    private bool CanMoveFirst() =>  (CurrentPanel is null) && ((_selectedTextureIndex != 0) || (_selectedArrayIndex != 0));

    /// <summary>
    /// Function to move to the first texture selection.
    /// </summary>
    private void DoMoveFirst()
    {
        try
        {
            SelectedArrayIndex = 0;
            SelectedTexture = 0;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error moving to the first texture.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the selected texture can be moved to the last one.
    /// </summary>
    /// <returns></returns>
    private bool CanMoveLast() => (CurrentPanel is null) && ((_selectedTextureIndex != Textures.Count - 1) || (_selectedArrayIndex != Textures[SelectedTexture].ArrayCount - 1));

    /// <summary>
    /// Function to move to the last texture selection.
    /// </summary>
    private void DoMoveLast()
    {
        try
        {
            SelectedTexture = Textures.Count - 1;
            SelectedArrayIndex = Textures[SelectedTexture].ArrayCount - 1;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error moving to the last texture.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the texture size editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivateTextureSize() => (CurrentPanel is null);

    /// <summary>
    /// Function to activate the texture size editor.
    /// </summary>
    private void DoActivateTextureSize()
    {
        try
        {
            FontTextureSize.TextureWidth = _fontService.WorkerFont.TextureWidth;
            FontTextureSize.TextureHeight = _fontService.WorkerFont.TextureHeight;
            CurrentPanel = FontTextureSize;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the texture size editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the font textures can be resized.
    /// </summary>
    private bool CanResizeTexture() => (_fontService.WorkerFont is not null) && ((FontTextureSize.TextureHeight != _fontService.WorkerFont.TextureHeight) || (FontTextureSize.TextureWidth != _fontService.WorkerFont.TextureWidth));

    /// <summary>
    /// Function to resize the font textures.
    /// </summary>
    private async void DoResizeTextureAsync()
    {
        async Task<bool> ResizeAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                NotifyPropertyChanging(nameof(TextureSize));

                await _fontService.UpdateFontAsync(args);

                NotifyPropertyChanged(nameof(TextureSize));

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        Task UndoRedo(GorgonFontInfo args, CancellationToken _) => ResizeAsync(args);

        GorgonFontInfo undoArgs = new(_fontService.WorkerFont);
        GorgonFontInfo redoArgs = new(_fontService.WorkerFont)
        {
            TextureWidth = FontTextureSize.TextureWidth,
            TextureHeight = FontTextureSize.TextureHeight
        };                       

        if (!(await ResizeAsync(redoArgs)))
        {
            return;
        }

        CurrentPanel = null;

        _undoService.Record(Resources.GORFNT_UNDO_TEXTURE_RESIZE, UndoRedo, UndoRedo, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(ITextureEditorContext.Name));
    }

    /// <summary>
    /// Function to determine if the padding interface can activate.
    /// </summary>
    /// <returns></returns>
    private bool CanActivatePadding() => _currentPanel is null;

    /// <summary>
    /// Function to activate the padding interface.
    /// </summary>
    private void DoActivatePadding()
    {
        try
        {
            CurrentPanel = FontPadding;
            FontPadding.Padding = _fontService.WorkerFont.PackingSpacing;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the glyph texture padding size editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the font textures can be padded.
    /// </summary>
    private bool CanPadTexture() => (_fontService.WorkerFont is not null) && (FontPadding.Padding != _fontService.WorkerFont.PackingSpacing);

    /// <summary>
    /// Function to pad the font textures.
    /// </summary>
    private async void DoPadTextureAsync()
    {
        async Task<bool> PadAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                NotifyPropertyChanging(nameof(TextureSize));

                await _fontService.UpdateFontAsync(args);

                NotifyPropertyChanged(nameof(TextureSize));

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        Task UndoRedo(GorgonFontInfo args, CancellationToken _) => PadAsync(args);

        GorgonFontInfo undoArgs = new(_fontService.WorkerFont);
        GorgonFontInfo redoArgs = new(_fontService.WorkerFont)
        {
            PackingSpacing = FontPadding.Padding
        };

        if (!(await PadAsync(redoArgs)))
        {
            return;
        }

        CurrentPanel = null;

        _undoService.Record(Resources.GORFNT_UNDO_PADDING, UndoRedo, UndoRedo, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(ITextureEditorContext.Name));
    }

    /// <summary>
    /// Function to determine if the solid brush editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivateSolidBrush() => (CurrentPanel is null);

    /// <summary>
    /// Function to activate the solid brush editor.
    /// </summary>
    private void DoActivateSolidBrush()
    {
        try
        {
            if (_fontService.WorkerFont.Brush is GorgonGlyphSolidBrush solidBrush)
            {
                SolidBrush.Brush = solidBrush;
                SolidBrush.OriginalColor = solidBrush.Color;
            }
            else
            {
                SolidBrush.Brush = FontSolidBrush.DefaultBrush;
                SolidBrush.OriginalColor = FontSolidBrush.DefaultBrush.Color;
            }

            CurrentPanel = SolidBrush;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the brush editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the pattern brush editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivatePatternBrush() => (CurrentPanel is null);

    /// <summary>
    /// Function to activate the pattern brush editor.
    /// </summary>
    private void DoActivatePatternBrush()
    {
        try
        {
            if (_fontService.WorkerFont.Brush is GorgonGlyphHatchBrush hatchBrush)
            {
                PatternBrush.Brush = hatchBrush;
                PatternBrush.OriginalColor = (hatchBrush.ForegroundColor, hatchBrush.BackgroundColor);
            }
            else
            {
                PatternBrush.Brush = FontPatternBrush.DefaultBrush;
                PatternBrush.OriginalColor = (FontPatternBrush.DefaultBrush.ForegroundColor, FontPatternBrush.DefaultBrush.BackgroundColor);
            }

            CurrentPanel = PatternBrush;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the brush editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the gradient brush editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivateGradientBrush() => (CurrentPanel is null);

    /// <summary>
    /// Function to activate the pattern brush editor.
    /// </summary>
    private void DoActivateGradientBrush()
    {
        try
        {
            if (_fontService.WorkerFont.Brush is GorgonGlyphLinearGradientBrush gradientBrush)
            {
                GradientBrush.Brush = gradientBrush;
            }
            else
            {
                GradientBrush.Brush = FontGradientBrush.DefaultBrush;
            }

            CurrentPanel = GradientBrush;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the brush editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the texture brush editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivateTextureBrush() => (CurrentPanel is null);

    /// <summary>
    /// Function to activate the texture brush editor.
    /// </summary>
    private void DoActivateTextureBrush()
    {
        try
        {
            if (_fontService.WorkerFont.Brush is GorgonGlyphTextureBrush textureBrush)
            {
                TextureBrush.Brush = textureBrush;
            }
            else
            {
                TextureBrush.Brush = FontTextureBrush.DefaultBrush;
            }

            CurrentPanel = TextureBrush;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("There was an error activating the brush editor.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the brush can be applied to the font.
    /// </summary>
    /// <returns><b>true</b> if it can be applied, <b>false</b> if not.</returns>
    private bool CanApplyBrush()
    {
        if ((CurrentPanel is not IFontBrush brush)
            || (_fontService.WorkerFont is null)
            || (brush.Brush == _fontService.WorkerFont.Brush))
        {
            return false;
        }


        return !brush.Brush.Equals(_fontService.WorkerFont.Brush);
    }

    /// <summary>
    /// Function to apply the brush to the font.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async void DoApplyBrushAsync()
    {
        async Task<bool> ApplyBrushAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                NotifyPropertyChanging(nameof(ITextureEditorContext.CurrentBrush));
                await _fontService.UpdateFontAsync(args);
                NotifyPropertyChanged(nameof(ITextureEditorContext.CurrentBrush));

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_APPLY_BRUSH);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        var brush = (IFontBrush)CurrentPanel;

        Task UndoRedoAction(GorgonFontInfo args, CancellationToken _) => ApplyBrushAsync(args);

        GorgonFontInfo undoArgs = new(_fontService.WorkerFont);
        GorgonFontInfo redoArgs = new(_fontService.WorkerFont)
        {
            Brush = brush?.Brush
        };

        if (!(await ApplyBrushAsync(redoArgs)))
        {
            return;
        }

        CurrentPanel = null;

        _undoService.Record(Resources.GORFNT_UNDO_SOLID_BRUSH, UndoRedoAction, UndoRedoAction, undoArgs, redoArgs);
    }        

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    ///   <para>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(TextureEditorContextParameters injectionParameters)
    {            
        _fontService = injectionParameters.FontService;
        _undoService = injectionParameters.UndoService;
        FontTextureSize = injectionParameters.FontTextureSize;
        FontPadding = injectionParameters.FontPadding;
        SolidBrush = injectionParameters.SolidBrush;
        PatternBrush = injectionParameters.PatternBrush;
        GradientBrush = injectionParameters.GradientBrush;
        TextureBrush = injectionParameters.TextureBrush;

        FontTextureSize.OkCommand = new EditorCommand<object>(DoResizeTextureAsync, CanResizeTexture);
        FontPadding.OkCommand = new EditorCommand<object>(DoPadTextureAsync, CanPadTexture);
        SolidBrush.OkCommand = new EditorCommand<object>(DoApplyBrushAsync, CanApplyBrush);
        PatternBrush.OkCommand = new EditorCommand<object>(DoApplyBrushAsync, CanApplyBrush);
        GradientBrush.OkCommand = new EditorCommand<object>(DoApplyBrushAsync, CanApplyBrush);
        TextureBrush.OkCommand = new EditorCommand<object>(DoApplyBrushAsync, CanApplyBrush);
    }

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        FontTextureSize?.Load();
        FontPadding?.Load();
        SolidBrush?.Load();
        PatternBrush?.Load();
        GradientBrush?.Load();
        TextureBrush?.Load();
    }        

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        GradientBrush.OkCommand = null;
        SolidBrush.OkCommand = null;
        PatternBrush.OkCommand = null;
        FontPadding.OkCommand = null;
        TextureBrush.OkCommand = null;
        FontTextureSize.OkCommand = null;

        FontTextureSize.Unload();
        FontPadding.Unload();
        SolidBrush.Unload();
        PatternBrush.Unload();
        GradientBrush.Unload();
        TextureBrush.Unload();

        CurrentPanel = null;

        base.OnUnload();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="TextureEditorContext" /> class.</summary>
    public TextureEditorContext()
    {
        SetPremultipliedCommand = new EditorAsyncCommand<bool>(DoChangePremultiplied, CanChangePremultiplied);
        PrevTextureCommand = new EditorCommand<object>(DoMovePrev, CanMovePrev);
        NextTextureCommand = new EditorCommand<object>(DoMoveNext, CanMoveNext);
        FirstTextureCommand = new EditorCommand<object>(DoMoveFirst, CanMoveFirst);
        LastTextureCommand = new EditorCommand<object>(DoMoveLast, CanMoveLast);
        ActivateTextureSizeCommand = new EditorCommand<object>(DoActivateTextureSize, CanActivateTextureSize);
        ActivatePaddingCommand = new EditorCommand<object>(DoActivatePadding, CanActivatePadding);
        ActivateSolidBrushCommand = new EditorCommand<object>(DoActivateSolidBrush, CanActivateSolidBrush);
        ActivatePatternBrushCommand = new EditorCommand<object>(DoActivatePatternBrush, CanActivatePatternBrush);
        ActivateGradientBrushCommand = new EditorCommand<object>(DoActivateGradientBrush, CanActivateGradientBrush);
        ActivateTextureBrushCommand = new EditorCommand<object>(DoActivateTextureBrush, CanActivateTextureBrush);
    }
    #endregion

}
