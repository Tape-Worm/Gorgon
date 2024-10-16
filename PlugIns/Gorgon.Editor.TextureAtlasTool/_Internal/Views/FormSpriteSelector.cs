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
// Created: May 9, 2019 7:50:58 AM
// 

using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// A dialog used for sprite selection
/// </summary>
internal partial class FormSpriteSelector
    : Form, IDataContext<ISpriteFiles>
{
    // Flag to indicate that the form is being designed in the IDE.
    private readonly bool _isDesignTime;
    // The graphics context for the application.
    private IGraphicsContext _graphicsContext;
    // The swap chain for rendering the preview.
    private GorgonSwapChain _swapChain;
    // The image used for the preview.
    private GorgonTexture2DView _previewImage;
    // The previous idle function.
    private Func<bool> _oldIdle;

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISpriteFiles ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls()
    {
        ButtonLabelMultiSprite.Visible = ViewModel?.SelectedFiles.Count < 2;
        ButtonLoad.Enabled = ViewModel?.ConfirmLoadCommand?.CanExecute(null) ?? false;
    }

    /// <summary>Handles the Search event of the ContentFileExplorer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonSearchEventArgs"/> instance containing the event data.</param>
    private void ContentFileExplorer_Search(object sender, GorgonSearchEventArgs e)
    {
        if ((ViewModel?.SearchCommand is null) || (!ViewModel.SearchCommand.CanExecute(e.SearchText)))
        {
            return;
        }

        ViewModel.SearchCommand.Execute(e.SearchText);
        ValidateControls();
    }

    /// <summary>Contents the file explorer file entries focused.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private async void ContentFileExplorer_FileEntriesFocused(object sender, ContentFileEntriesFocusedArgs e)
    {
        if ((ViewModel?.RefreshSpritePreviewCommand is null) || (!ViewModel.RefreshSpritePreviewCommand.CanExecute(e.FocusedFiles)))
        {
            return;
        }

        await ViewModel.RefreshSpritePreviewCommand.ExecuteAsync(e.FocusedFiles);
        ValidateControls();
    }

    /// <summary>Handles the Click event of the ButtonCancel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonCancel_Click(object sender, EventArgs e) => Close();

    /// <summary>Handles the Click event of the ButtonLoad control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonLoad_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ConfirmLoadCommand is null) || (!ViewModel.ConfirmLoadCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ConfirmLoadCommand.Execute(null);
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteFiles.PreviewImage):
                UpdateRenderImage(ViewModel.PreviewImage);
                break;
        }

        ValidateControls();
    }

    /// <summary>
    /// Function to update the image to render for previewing.
    /// </summary>
    /// <param name="image">The image to update with.</param>
    private void UpdateRenderImage(IGorgonImage image)
    {
        _previewImage?.Dispose();

        if (image is null)
        {
            _previewImage = null;
            return;
        }

        _previewImage = GorgonTexture2DView.CreateTexture(_graphicsContext.Graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
        {
            Name = "Atlas_Sprite_Preview",
            ArrayCount = 1,
            Binding = TextureBinding.ShaderResource,
            Usage = ResourceUsage.Immutable,
            IsCubeMap = false,
            MipLevels = 1
        }, image);
    }

    /// <summary>
    /// Function to retrieve the rectangular region for rendering.
    /// </summary>
    /// <returns>The render area.</returns>
    private GorgonRectangle GetRenderRegion()
    {
        int size;

        if (_swapChain.Width < _swapChain.Height)
        {
            size = _swapChain.Width;
        }
        else
        {
            size = _swapChain.Height;
        }

        int top = (_swapChain.Height / 2) - (size / 2);
        int left = (_swapChain.Width / 2) - (size / 2);

        return new GorgonRectangle(left, top, size, size);
    }

    /// <summary>
    /// Function to update the preview during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> if not.</returns>
    private bool Idle()
    {
        _graphicsContext.Graphics.SetRenderTarget(_swapChain.RenderTargetView);
        _swapChain.RenderTargetView.Clear(PanelPreviewRender.BackColor);

        GorgonRectangleF renderRegion = GetRenderRegion();
        Vector2 halfClient = new(renderRegion.Width * 0.5f, renderRegion.Height * 0.5f);

        _graphicsContext.Renderer2D.Begin();
        _graphicsContext.Renderer2D.DrawFilledRectangle(renderRegion, DarkFormsRenderer.DarkBackground);

        // Render the sprite image.
        if (_previewImage is not null)
        {
            float scale = (renderRegion.Width / _previewImage.Width).Min(renderRegion.Height / _previewImage.Height);
            float width = _previewImage.Width * scale;
            float height = _previewImage.Height * scale;
            float x = renderRegion.X + halfClient.X - (width * 0.5f);
            float y = renderRegion.Y + halfClient.Y - (height * 0.5f);

            _graphicsContext.Renderer2D.DrawFilledRectangle(new GorgonRectangleF(x, y, width, height), GorgonColors.White, _previewImage, new GorgonRectangleF(0, 0, 1, 1));
        }
        else
        {
            Vector2 size = Resources.GORTAG_TEXT_SELECT_SPRITE.MeasureText(_graphicsContext.Renderer2D.DefaultFont, false);
            _graphicsContext.Renderer2D.DrawString(Resources.GORTAG_TEXT_SELECT_SPRITE,
                                                    new Vector2(renderRegion.X + halfClient.X - size.X * 0.5f, renderRegion.Y + halfClient.Y - size.Y * 0.5f),
                                                    color: GorgonColors.White);
        }
        _graphicsContext.Renderer2D.End();

        _swapChain.Present(1);

        if ((GorgonApplication.AllowBackground) && (_oldIdle is not null))
        {
            if (!_oldIdle())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Function to shut down the graphics context.
    /// </summary>
    private void ShutdownGraphics()
    {
        Func<bool> oldIdle = Interlocked.Exchange(ref _oldIdle, null);
        if (oldIdle is not null)
        {
            GorgonApplication.IdleMethod = oldIdle;
        }
        GorgonTexture2DView preview = Interlocked.Exchange(ref _previewImage, null);
        GorgonSwapChain swap = Interlocked.Exchange(ref _swapChain, null);

        preview?.Dispose();
        if (swap is not null)
        {
            _graphicsContext.ReturnSwapPresenter(ref swap);
        }
        _graphicsContext = null;
    }

    /// <summary>
    /// Function to unassign events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the control back to its default state.
    /// </summary>
    private void ResetDataContext() => ContentFileExplorer.Entries = null;

    /// <summary>
    /// Function to initialize the form from the data context.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void InitializeFromDataContext(ISpriteFiles dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        ContentFileExplorer.Entries = dataContext.SpriteFileEntries;
    }

    /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (_isDesignTime)
        {
            return;
        }

        ViewModel?.Unload();
    }

    /// <summary>Raises the Load event.</summary>
    /// <param name="e">An EventArgs containing event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_isDesignTime)
        {
            return;
        }

        ViewModel?.Load();

        ValidateControls();
    }

    /// <summary>
    /// Function to assign the graphics context for the preview.
    /// </summary>
    /// <param name="context">The graphics context for the application.</param>
    public void SetGraphicsContext(IGraphicsContext context)
    {
        ShutdownGraphics();

        _graphicsContext = context;

        if (context is null)
        {
            return;
        }

        _swapChain = context.LeaseSwapPresenter(PanelPreviewRender);
        _oldIdle = GorgonApplication.IdleMethod;
        GorgonApplication.IdleMethod = Idle;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISpriteFiles dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);
        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }

    /// <summary>Initializes a new instance of the <see cref="FormSpriteSelector"/> class.</summary>
    public FormSpriteSelector()
    {
        _isDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        InitializeComponent();
    }
}
