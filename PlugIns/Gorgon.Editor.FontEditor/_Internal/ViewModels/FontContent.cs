#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: August 3, 2020 3:51:47 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Fonts;
using Gorgon.IO;
using Gorgon.Math;
using Drawing = System.Drawing;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model used to communicate with the font content control.
/// </summary>
internal class FontContent
    : ContentEditorViewModelBase<FontContentParameters>, IFontContent
{
    #region Variables.
    // The currently active panel.
    private IHostedPanelViewModel _currentPanel;
    // The service used for providing undo/redo functionality.
    private IUndoService _undoService;
    // The font service used to build and retrieve cached fonts.
    private FontService _fontService;
    // Information used to generate the font.
    private GorgonFontInfo _info;
    // Flag to indicate that the font should be bolded.
    private bool _isBold;
    // Flag to indicate that the font should be italicized.
    private bool _isItalic;
    // The selected font family.
    private Drawing.FontFamily _fontFamily;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the currently active hosted panel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property holds the view model for the currently active hosted panel which can be used for parameters for an operation on the content. Setting this value will bring the panel up on the UI and 
    /// setting it to <b>null</b> will remove it.
    /// </para>
    /// </remarks>
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
                _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                _currentPanel.IsActive = false;
            }

            OnPropertyChanging();
            _currentPanel = value;
            OnPropertyChanged();

            if (_currentPanel is not null)
            {
                _currentPanel.IsActive = true;
                _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
            }
        }
    }

    /// <summary>Property to return the command to undo an operation.</summary>
    public IEditorCommand<object> UndoCommand
    {
        get;
    }

    /// <summary>Property to return the command to redo an operation.</summary>
    public IEditorCommand<object> RedoCommand
    {
        get;
    }

    /// <summary>Property to return the type of content.</summary>
    public override string ContentType => CommonEditorContentTypes.FontType;

    /// <summary>
    /// Property to return the working font.
    /// </summary>
    public GorgonFont WorkingFont => _fontService.WorkerFont;

    /// <summary>
    /// Property to return the font family.
    /// </summary>
    public string FontFamily
    {
        get => _info.FontFamilyName;
        private set
        {
            if (string.Equals(_info.FontFamilyName, value, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            Drawing.FontFamily family = Interlocked.Exchange(ref _fontFamily, null);
            family?.Dispose();
            _fontFamily = new Drawing.FontFamily(value);
#if NET6_0_OR_GREATER
            _info = _info with
            {
                FontFamilyName = value,
                Name = $"{value} {_info.Size} {_info.FontHeightMode}"
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the font size.
    /// </summary>
    public float FontSize
    {
        get => _info.Size;
        private set
        {
            if (_info.Size.EqualsEpsilon(value))
            {
                return;
            }

            OnPropertyChanging();
#if NET6_0_OR_GREATER
            _info = _info with
            {
                Size = value,
                Name = $"{_info.FontFamilyName} {value} {_info.FontHeightMode}"
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the units for the font size.
    /// </summary>
    public FontHeightMode FontUnits
    {
        get => _info.FontHeightMode;
        private set
        {
            if (_info.FontHeightMode == value)
            {
                return;
            }

            OnPropertyChanging();
#if NET6_0_OR_GREATER
            _info = _info with
            {
                FontHeightMode = value,
                Name = $"{_info.FontFamilyName} {_info.Size} {value}"
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return whether the font is bold or not.
    /// </summary>
    public bool IsBold
    {
        get => _isBold;
        set
        {
            if (_isBold == value)
            {
                return;
            }

            OnPropertyChanging();
#if NET6_0_OR_GREATER
            _isBold = value;
            _info = _info with
            {
                FontStyle = GetFontStyle(value, IsItalic)
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return whether the font is italic or not.
    /// </summary>
    public bool IsItalic
    {
        get => _isItalic;
        set
        {
            if (_isItalic == value)
            {
                return;
            }

            OnPropertyChanging();
#if NET6_0_OR_GREATER
            _isItalic = value;
            _info = _info with
            {
                FontStyle = GetFontStyle(IsBold, value)
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return whether the sprite is antialiased or not.
    /// </summary>
    public bool IsAntiAliased
    {
        get => _info.AntiAliasingMode == FontAntiAliasMode.AntiAlias;
        set
        {
            if (((value) && (_info.AntiAliasingMode == FontAntiAliasMode.AntiAlias))
                || ((!value) && (_info.AntiAliasingMode == FontAntiAliasMode.None)))
            {
                return;
            }

            OnPropertyChanging();
#if NET6_0_OR_GREATER
            _info = _info with
            {
                AntiAliasingMode = value ? FontAntiAliasMode.AntiAlias : FontAntiAliasMode.None
            };
#endif
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the command used to set the font family.
    /// </summary>
    public IEditorAsyncCommand<string> SetFontFamilyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font size.
    /// </summary>
    public IEditorAsyncCommand<float> SetSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font style to Bold.
    /// </summary>
    public IEditorAsyncCommand<bool> SetBoldCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font style to Italic.
    /// </summary>
    public IEditorAsyncCommand<bool> SetItalicCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font glyphs to be antialiased or not.
    /// </summary>
    public IEditorAsyncCommand<bool> SetAntiAliasCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font measure units.
    /// </summary>
    public IEditorAsyncCommand<bool> SetFontUnitsCommand
    {
        get;
    }

    /// <summary>Property to return the outline interface view model.</summary>
    public IFontOutline FontOutline
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the font character selection view model.
    /// </summary>
    public IFontCharacterSelection FontCharacterSelection
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the texture editor context.
    /// </summary>
    public ITextureEditorContext TextureEditor
    {
        get;
        private set;
    }

    /// <summary>Property to return the command used to activate the outline interface.</summary>
    public IEditorCommand<bool> ActivateOutlineCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture context.
    /// </summary>
    public IEditorCommand<object> ActivateTextureContextCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the character selection interface.
    /// </summary>
    public IEditorCommand<object> ActivateCharacterSelectionCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new font derived from the current font.
    /// </summary>
    public IEditorAsyncCommand<object> NewFontCommand
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the FontUpdating event of the FontService control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void FontService_FontUpdating(object sender, EventArgs e) => NotifyPropertyChanging(nameof(WorkingFont));

    /// <summary>Handles the FontUpdated event of the FontService control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void FontService_FontUpdated(object sender, EventArgs e) => NotifyPropertyChanged(nameof(WorkingFont));

    /// <summary>
    /// Panels the wait panel deactivated.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Panel_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

    /// <summary>
    /// Panels the wait panel activated.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void Panel_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e);

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
                    CurrentPanel = null;
                }
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the TextureEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void TextureEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextureEditorContext.CurrentPanel):
                CurrentPanel = TextureEditor.CurrentPanel;
                break;
            case nameof(ITextureEditorContext.CurrentBrush):
#if NET6_0_OR_GREATER
                _info = _info with
                {
                    Brush = TextureEditor.CurrentBrush
                };
                ContentState = ContentState.Modified;
#endif
                break;
            case nameof(ITextureEditorContext.FontPadding):
#if NET6_0_OR_GREATER
                _info = _info with
                {
                    PackingSpacing = TextureEditor.FontPadding.Padding
                };
                ContentState = ContentState.Modified;
#endif
                break;
            case nameof(ITextureEditorContext.TextureSize):
#if NET6_0_OR_GREATER
                _info = _info with
                {
                    TextureHeight = TextureEditor.TextureSize.Height,
                    TextureWidth = TextureEditor.TextureSize.Width
                };
                ContentState = ContentState.Modified;
#endif
                break;
            case nameof(ITextureEditorContext.UsePremultipliedAlpha):
#if NET6_0_OR_GREATER
                _info = _info with
                {
                    UsePremultipliedTextures = TextureEditor.UsePremultipliedAlpha
                };
                ContentState = ContentState.Modified;
#endif
                break;
        }
    }

    /// <summary>
    /// Fonts the outline property changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private async void FontOutline_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontOutline.SelectedEndColor):
            case nameof(IFontOutline.SelectedStartColor):
            case nameof(IFontOutline.OutlineSize):
                await UpdateFontOutlineAsync();
                break;
        }
    }

    /// <summary>
    /// Function to perform an update of the font outline.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task UpdateFontOutlineAsync()
    {
#if NET6_0_OR_GREATER
        async Task<bool> UpdateFontAsync(GorgonFontInfo args)
        {
            try
            {
                ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_FONT_OUTLINE);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        if (((_info.OutlineSize == FontOutline.OutlineSize)
            && (_info.OutlineColor1.Equals(FontOutline.SelectedStartColor))
            && (_info.OutlineColor2.Equals(FontOutline.SelectedEndColor)))
            || (CurrentPanel != FontOutline))
        {
            return;
        }

        async Task Action(GorgonFontInfo args, CancellationToken _) => await UpdateFontAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = new(_info)
        {
            OutlineSize = FontOutline.OutlineSize,
            OutlineColor1 = FontOutline.SelectedStartColor,
            OutlineColor2 = FontOutline.SelectedEndColor
        };

        if (!(await UpdateFontAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_OUTLINE, Action, Action, undoArgs, redoArgs);
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Function to retrieve the font style from the style flags.
    /// </summary>
    /// <param name="isBold"><b>true</b> if the bold flag is set.</param>
    /// <param name="isItalic"><b>false</b> if the italic flag is set.</param>
    /// <returns>The font style.</returns>
    private FontStyle GetFontStyle(bool isBold, bool isItalic)
    {
        FontStyle result = FontStyle.Normal;

        if (isBold)
        {
            result = !isItalic ? FontStyle.Bold : FontStyle.BoldItalics;
        }

        if (isItalic)
        {
            result = !isBold ? FontStyle.Italics : FontStyle.BoldItalics;
        }

        return result;
    }
#endif

    /// <summary>
    /// Function to determine if the font can have its style set to italic.
    /// </summary>
    /// <param name="flag">The flag to evaluate.</param>
    /// <returns><b>true</b> if the command can execute, <b>false</b> if not.</returns>
    private bool CanSetItalic(bool flag)
    {
        if ((CurrentPanel is not null) || (_fontFamily is null))
        {
            return false;
        }

        return _fontFamily.IsStyleAvailable(Drawing.FontStyle.Italic);
    }

    /// <summary>
    /// Function to set the font italic style.
    /// </summary>
    /// <param name="flag">The state of the style flag.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetItalicAsync(bool flag)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetItalicAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                IsItalic = args.FontStyle is FontStyle.Italics or FontStyle.BoldItalics;
                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;

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

        if (flag == IsItalic)
        {
            return;
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetItalicAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            FontStyle = GetFontStyle(IsBold, flag)
        };

        if (!(await SetItalicAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_ITALIC, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the font can have its style set to bold.
    /// </summary>
    /// <param name="flag">The flag to evaluate.</param>
    /// <returns><b>true</b> if the command can execute, <b>false</b> if not.</returns>
    private bool CanSetBold(bool flag)
    {
        if ((CurrentPanel is not null) || (_fontFamily is null))
        {
            return false;
        }

        return _fontFamily.IsStyleAvailable(Drawing.FontStyle.Bold);
    }

    /// <summary>
    /// Function to set the font bold style.
    /// </summary>
    /// <param name="flag">The state of the style flag.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetBoldAsync(bool flag)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetBoldAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                IsBold = args.FontStyle is FontStyle.Bold or FontStyle.BoldItalics;
                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;

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

        if (flag == IsBold)
        {
            return;
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetBoldAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            FontStyle = GetFontStyle(flag, IsItalic)
        };

        if (!(await SetBoldAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_BOLD, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the font can have its style set to antiAlias.
    /// </summary>
    /// <param name="flag">The flag to evaluate.</param>
    /// <returns><b>true</b> if the command can execute, <b>false</b> if not.</returns>
    private bool CanSetAntiAlias(bool flag) => (CurrentPanel is null);

    /// <summary>
    /// Function to set the font anti alias style.
    /// </summary>
    /// <param name="flag">The state of the style flag.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetAntiAliasAsync(bool flag)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetAntiAliasAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                IsAntiAliased = args.AntiAliasingMode == FontAntiAliasMode.AntiAlias;
                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;

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

        if (flag == IsAntiAliased)
        {
            return;
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetAntiAliasAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            AntiAliasingMode = flag ? FontAntiAliasMode.AntiAlias : FontAntiAliasMode.None
        };

        if (!(await SetAntiAliasAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_ANTIALIAS, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the font can have its unit type set.
    /// </summary>
    /// <param name="points">True if the font unit is set to points, <b>false</b> if not.</param>
    /// <returns><b>true</b> if the command can execute, <b>false</b> if not.</returns>
    private bool CanSetFontUnits(bool points) => (CurrentPanel is null);

    /// <summary>
    /// Function to set the font units.
    /// </summary>
    /// <param name="points">True if the font unit is set to points, <b>false</b> if not.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetFontUnitsAsync(bool points)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetUnitsAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                FontUnits = args.FontHeightMode;
                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_UNITS);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }


        if (points == (FontUnits == FontHeightMode.Points))
        {
            return;
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetUnitsAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            FontHeightMode = points ? FontHeightMode.Points : FontHeightMode.Pixels
        };

        if (!(await SetUnitsAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(points ? Resources.GORFNT_UNDO_POINTS : Resources.GORFNT_UNDO_PIXEL, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the size of the font can be set.
    /// </summary>
    /// <param name="value">The new size.</param>
    /// <returns><b>true</b> if the size can be set, <b>false</b> if not.</returns>
    private bool CanSetFontSize(float value) => (CurrentPanel is null);

    /// <summary>
    /// Funtion to set the size of the font.
    /// </summary>
    /// <param name="value">The new size.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetSizeAsync(float value)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetSizeAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                FontSize = args.Size;
                _info = await _fontService.UpdateFontAsync(args);                    
                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_SIZE);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetSizeAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            Size = value
        };

        if (!(await SetSizeAsync(redoArgs)))
        {
            return;
        }

        _undoService.Record(Resources.GORFNT_UNDO_FONT_SIZE, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function called to determine if the font family can be set.
    /// </summary>
    /// <param name="fontFamily">The requested font family.</param>
    /// <returns><b>true</b> if the family can be set, <b>false</b> if not.</returns>
    private bool CanSetFontFamily(string fontFamily) => (CurrentPanel is null) && (!string.IsNullOrWhiteSpace(fontFamily));

    /// <summary>
    /// Function called to regenerate the font.
    /// </summary>
    /// <param name="fontFamily">The family to assign.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetFontFamilyAsync(string fontFamily)
    {
#if NET6_0_OR_GREATER
        async Task<bool> SetFamilyAsync(GorgonFontInfo args)
        {                
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                FontFamily = args.FontFamilyName;
                IsBold = args.FontStyle is FontStyle.Bold or FontStyle.BoldItalics;
                IsItalic = args.FontStyle is FontStyle.Italics or FontStyle.BoldItalics;
                _info = await _fontService.UpdateFontAsync(args);

                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_FONT_FAMILY);
                return false;
            }
            finally
            {                    
                HideWaitPanel();
            }
        }

        bool noRegular = false;
        var localFamily = new Drawing.FontFamily(fontFamily);
        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SetFamilyAsync(args);

        try
        {
            GorgonFontInfo undoArgs = new(_info);
            bool isBold = _info.FontStyle is FontStyle.Bold or FontStyle.BoldItalics;
            bool isItalic = _info.FontStyle is FontStyle.Italics or FontStyle.BoldItalics;


            if ((!localFamily.IsStyleAvailable(Drawing.FontStyle.Regular)) && (_info.FontStyle == FontStyle.Normal))
            {
                // Try to set the bold or italic version.
                isBold = true;
                isItalic = true;
                noRegular = true;
            }

            if ((isBold) && (!localFamily.IsStyleAvailable(Drawing.FontStyle.Bold)))
            {
                isBold = false;
            }

            if ((isItalic) && (!localFamily.IsStyleAvailable(Drawing.FontStyle.Italic)))
            {
                isItalic = false;
            }

            if ((noRegular) && (!isBold) && (!isItalic))
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GORFNT_ERR_FONT_FAMILY_NO_STYLE, fontFamily));
                HostServices.Log.Print($"ERROR: The font family '{fontFamily}' does not support a regular style, bold style, or italic style.", LoggingLevel.Simple);
                return;
            }

            GorgonFontInfo redoArgs = _info with
            {
                FontFamilyName = fontFamily,
                FontStyle = GetFontStyle(isBold, isItalic)
            };

            if (!(await SetFamilyAsync(redoArgs)))
            {
                return;
            }

            _undoService.Record(Resources.GORFNT_UNDO_FONT_FAMILY, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }
        finally
        {
            localFamily.Dispose();
        }
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if the current content can be saved.
    /// </summary>
    /// <param name="saveReason">The reason why the content is being saved.</param>
    /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
    private bool CanSaveFont(SaveReason saveReason) => (ContentState != ContentState.Unmodified)
                                                        && (((CurrentPanel is null) && (CommandContext is null)) || (saveReason != SaveReason.UserSave));

    /// <summary>
    /// Function to save the sprite back to the project file system.
    /// </summary>
    /// <param name="saveReason">The reason why the content is being saved.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSaveFontAsync(SaveReason saveReason)
    {
        Stream outStream = null;

        ShowWaitPanel(Resources.GORFNT_TEXT_SAVING);

        try
        {
            File.IsOpen = false;
            await _fontService.SaveAsync(File.Path);
            ContentState = ContentState.Unmodified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SAVING);
        }
        finally
        {
            File.IsOpen = true;
            outStream?.Dispose();

            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if the font outline interface can be activated.
    /// </summary>
    /// <param name="activateState">The current activation state.</param>
    /// <returns><b>true</b> if the outline interface can be activated, <b>false</b> if not.</returns>
    private bool CanActivateOutline(bool activateState) => ((CurrentPanel is null) || (CurrentPanel == FontOutline)) && (CommandContext is null);

    /// <summary>
    /// Function to activate or deactivate the outline interface.
    /// </summary>
    /// <param name="activateState">The current activation state.</param>
    private void DoActivateOutline(bool activateState)
    {
        try
        {
            if (!activateState)
            {
                CurrentPanel = null;
                return;
            }

            FontOutline.OutlineSize = _info.OutlineSize;
            FontOutline.SelectedStartColor = FontOutline.OriginalStartColor = _info.OutlineColor1;
            FontOutline.SelectedEndColor = FontOutline.OriginalEndColor = _info.OutlineColor2;
            CurrentPanel = FontOutline;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_FONT_OUTLINE);
        }
    }

    /// <summary>
    /// Function to determine if the texture editor context can be activated.
    /// </summary>
    /// <returns><b>true</b> if the context can be activated, <b>false</b> if not.</returns>
    private bool CanActivateTextureEditor() => (CurrentPanel is null) && ((CommandContext is null) || (CommandContext == TextureEditor));

    /// <summary>
    /// Function to activate the texture editor context.
    /// </summary>
    private void DoActivateTextureEditor()
    {
        try
        {
            // This is here to keep the file from being marked as changed due to the premultiplied setting below.
            TextureEditor.PropertyChanged -= TextureEditor_PropertyChanged;

            if (CommandContext == TextureEditor)
            {
                CommandContext = null;
                return;
            }

            CommandContext = TextureEditor;
            TextureEditor.UsePremultipliedAlpha = _info.UsePremultipliedTextures;
            TextureEditor.PropertyChanged += TextureEditor_PropertyChanged;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("Error: The texture editor context could not be activated.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the character selection interface can be activated.
    /// </summary>
    /// <returns><b>true</b> if it can be activated, <b>false</b> if not.</returns>
    private bool CanActivateCharacterSelection() => (CurrentPanel is null) && (CommandContext is null);

    /// <summary>
    /// Function to activate the character selection interface.
    /// </summary>
    private void DoActivateCharacterSelection()
    {
        try
        {
            CurrentPanel = FontCharacterSelection;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("Error: The texture character selector could not be activated.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if characters can be selected.
    /// </summary>
    /// <returns><b>true</b> if possible, <b>false</b> if not.</returns>
    private bool CanSelectCharacters() => (CurrentPanel == FontCharacterSelection) && (_info.Characters != FontCharacterSelection.Characters) && (!_info.Characters.SequenceEqual(FontCharacterSelection.Characters));

    /// <summary>
    /// Function to select characters.
    /// </summary>
    private async void DoSelectCharactersAsync()
    {
#if NET6_0_OR_GREATER
        async Task<bool> SelectCharsAsync(GorgonFontInfo args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_PLEASE_WAIT_GEN_FONT);

            try
            {
                _info = await _fontService.UpdateFontAsync(args);
                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_UNDO_SELECT_CHARS);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        Task UndoRedoActionAsync(GorgonFontInfo args, CancellationToken _) => SelectCharsAsync(args);

        GorgonFontInfo undoArgs = new(_info);
        GorgonFontInfo redoArgs = _info with
        {
            Characters = FontCharacterSelection.Characters
        };

        if (!(await SelectCharsAsync(redoArgs)))
        {
            return;
        }

        CurrentPanel = null;

        _undoService.Record(Resources.GORFNT_UNDO_SELECT_CHARS, UndoRedoActionAsync, UndoRedoActionAsync, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
#else
#pragma warning disable IDE0022 // Use expression body for methods
        await Task.CompletedTask;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Function to determine if a new font can be created.
    /// </summary>
    /// <returns><b>true</b> if a new font can be created, <b>false</b> if not.</returns>
    private bool CanCreateFont() => CurrentPanel is null;

    /// <summary>
    /// Function to create a new font based on the current font.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoCreateFontAsync()
    {
        try
        {
            // If this content is currently in a modified state, ask if we want to save first.
            if (ContentState != ContentState.Unmodified)
            {
                MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORFNT_CONFIRM_UNSAVED_CHANGES, File.Name), allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        await _fontService.SaveAsync(File.Path);
                        ContentState = ContentState.Unmodified;
                        break;
                    case MessageResponse.Cancel:
                        return;
                }
            }

            string fontDirectory = Path.GetDirectoryName(File.Path).FormatDirectory('/');
            if (string.IsNullOrWhiteSpace(fontDirectory))
            {
                fontDirectory = "/";
            }

            (string newName, GorgonFontInfo info) = _fontService.GetNewFontInfo(File.Name, fontDirectory, _info);

            if (info is null)
            {
                return;
            }

            string fontPath = fontDirectory + newName.FormatFileName();

            if (ContentFileManager.FileExists(fontPath))
            {
                IContentFile file = ContentFileManager.GetFile(fontPath);

                if (file.IsOpen)
                {
                    HostServices.MessageDisplay.ShowError(string.Format(Resources.GORFNT_ERR_FILE_OPEN_CANNOT_OVERWRITE, fontPath));
                    return;
                }

                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORFNT_CONFIRM_FILE_EXISTS, fontPath)) != MessageResponse.Yes)
                {
                    return;
                }
            }

            // Copy the current sprite state into a new file.
            ShowWaitPanel(Resources.GORFNT_TEXT_SAVING);

            NotifyPropertyChanging(nameof(WorkingFont));
            _info = await _fontService.UpdateFontAsync(info);

            // Write the updated data.                
            File.IsOpen = false;
            File = await _fontService.SaveAsync(fontPath);

            NotifyPropertyChanged(nameof(WorkingFont));
            // Remove all undo states since we're now working with a new file.
            ContentState = ContentState.Unmodified;
            _undoService.ClearStack();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_CREATE);
            // If we fail to create the new sprite, we need to shut the editor down because our state
            // could be corrupt.
            await CloseContentCommand.ExecuteAsync(new CloseContentArgs(false));
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if an undo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
    private bool CanUndo() => (_undoService.CanUndo) && (CurrentPanel is null);

    /// <summary>
    /// Function to determine if a redo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
    private bool CanRedo() => (_undoService.CanRedo) && (CurrentPanel is null);

    /// <summary>
    /// Function called when a redo operation is requested.
    /// </summary>
    private async void DoRedoAsync()
    {
        try
        {
            await _undoService.Redo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_REDO);
        }
    }

    /// <summary>
    /// Function called when an undo operation is requested.
    /// </summary>
    private async void DoUndoAsync()
    {
        try
        {
            await _undoService.Undo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_UNDO);
        }
    }

    /// <summary>Function to determine the action to take when this content is closing.</summary>
    /// <returns>
    ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>
    ///   <para>
    /// Content plug in developers should override this method so that users are given a chance to save their content data (if it has changed) prior to closing the content.
    /// </para>
    ///   <para>
    /// This is set up as an asynchronous method so that users may save their data asynchronously to keep the UI usable.
    /// </para>
    /// </remarks>
    protected override async Task<bool> OnCloseContentTaskAsync()
    {
        // When closing, we should check whether the content data is modified, this way we can prompt the user to save 
        // the data prior to closing it down.

        if (ContentState == ContentState.Unmodified)
        {
            return true;
        }

        MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORFNT_CONFIRM_UNSAVED_CHANGES, File.Name), allowCancel: true);

        switch (response)
        {
            case MessageResponse.Yes:
                await DoSaveFontAsync(SaveReason.ContentShutdown);
                return true;
            case MessageResponse.Cancel:
                return false;
            default:
                return true;
        }
    }

    /// <summary>Function to initialize the content.</summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    protected override void OnInitialize(FontContentParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        _undoService = injectionParameters.UndoService;
        _fontService = injectionParameters.FontService;
        FontOutline = injectionParameters.FontOutline;
        FontCharacterSelection = injectionParameters.FontCharacterSelection;
        TextureEditor = injectionParameters.TextureEditorContext;

        _info = new GorgonFontInfo(WorkingFont);
        _isBold = _info.FontStyle is FontStyle.Bold or FontStyle.BoldItalics;
        _isItalic = _info.FontStyle is FontStyle.Italics or FontStyle.BoldItalics;

        _fontFamily = new Drawing.FontFamily(_info.FontFamilyName);
    }

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        FontCharacterSelection.OkCommand = new EditorCommand<object>(DoSelectCharactersAsync, CanSelectCharacters);

        _fontService.FontUpdated += FontService_FontUpdated;
        _fontService.FontUpdating += FontService_FontUpdating;
        FontOutline.PropertyChanged += FontOutline_PropertyChanged;
        TextureEditor.PropertyChanged += TextureEditor_PropertyChanged;
        TextureEditor.WaitPanelActivated += Panel_WaitPanelActivated;
        TextureEditor.WaitPanelDeactivated += Panel_WaitPanelDeactivated;

        FontOutline?.Load();
        TextureEditor?.Load();
    }

    /// <summary>
    /// Function called when the associated view is unloaded.
    /// </summary>
    /// <seealso cref="ViewModelBase{T, Ths}.Load" />
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        FontCharacterSelection.OkCommand = null;

        FontCharacterSelection?.Unload();
        TextureEditor?.Unload();

        _fontService.FontUpdated -= FontService_FontUpdated;
        _fontService.FontUpdating -= FontService_FontUpdating;
        TextureEditor.PropertyChanged -= TextureEditor_PropertyChanged;
        FontOutline.PropertyChanged -= FontOutline_PropertyChanged;
        TextureEditor.WaitPanelActivated -= Panel_WaitPanelActivated;
        TextureEditor.WaitPanelDeactivated -= Panel_WaitPanelDeactivated;

        Drawing.FontFamily family = Interlocked.Exchange(ref _fontFamily, null);
        family?.Dispose();

        _fontService?.Dispose();

        base.OnUnload();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontContent"/> class.</summary>
    public FontContent()
    {
        UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
        RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
        SetFontFamilyCommand = new EditorAsyncCommand<string>(DoSetFontFamilyAsync, CanSetFontFamily);
        SetBoldCommand = new EditorAsyncCommand<bool>(DoSetBoldAsync, CanSetBold);
        SetItalicCommand = new EditorAsyncCommand<bool>(DoSetItalicAsync, CanSetItalic);
        SetSizeCommand = new EditorAsyncCommand<float>(DoSetSizeAsync, CanSetFontSize);
        SetAntiAliasCommand = new EditorAsyncCommand<bool>(DoSetAntiAliasAsync, CanSetAntiAlias);
        SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveFontAsync, CanSaveFont);
        SetFontUnitsCommand = new EditorAsyncCommand<bool>(DoSetFontUnitsAsync, CanSetFontUnits);
        ActivateOutlineCommand = new EditorCommand<bool>(DoActivateOutline, CanActivateOutline);
        ActivateTextureContextCommand = new EditorCommand<object>(DoActivateTextureEditor, CanActivateTextureEditor);
        ActivateCharacterSelectionCommand = new EditorCommand<object>(DoActivateCharacterSelection, CanActivateCharacterSelection);
        NewFontCommand = new EditorAsyncCommand<object>(DoCreateFontAsync, CanCreateFont);
    }
    #endregion
}
