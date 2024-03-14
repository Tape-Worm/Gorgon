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
// Created: August 3, 2020 3:45:05 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view for the font content.
/// </summary>
internal partial class FontContentView 
    : VisualContentBaseControl, IDataContext<IFontContent>
{
    #region Variables.
    // The renderer for our font data.
    private FontRenderer _renderer;
    // The marching ants selection rectangle.
    private IMarchingAnts _ants;
    // The clipper used to clip the texture.
    private IRectClipperService _clipper;
    // The form containing the ribbon used to merge with the application ribbon.
    private readonly FormRibbon _ribbonForm;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the font factory for our fonts.
    /// </summary>
    internal GorgonFontFactory FontFactory
    {
        get;
        set;
    }

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // <-- So we don't show up in the IDE.
    public IFontContent ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the controls on the view.
    /// </summary>
    private void ValidateButtons() => _ribbonForm.ValidateButtons();       

    /// <summary>
    /// Function to initialize the view from the data context.
    /// </summary>
    /// <param name="dataContext">The data context to use.</param>
    private void InitializeFromDataContext(IFontContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }
    }        

    /// <summary>Function called when a property is changed on the data context.</summary>
    /// <param name="propertyName">The name of the property that is updated.</param>
    /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(FontContent.CommandContext):
                SwitchRenderer(ViewModel.CommandContext?.Name ?? _renderer.Name, true);
                break;
            case nameof(FontContent.CurrentPanel):
                string rendererName = _renderer.Name;

                if ((ViewModel.CurrentPanel is not null) && (HasRenderer(ViewModel.CurrentPanel.GetType().FullName)))
                {
                    rendererName = ViewModel.CurrentPanel.GetType().FullName;
                }
                else if (ViewModel.CommandContext is not null)
                {
                    rendererName = ViewModel.CommandContext.Name;
                }

                if ((Renderer is null) || (!string.Equals(Renderer.Name, rendererName, StringComparison.OrdinalIgnoreCase)))
                {
                    SwitchRenderer(rendererName, true);
                }
                break;
        }
    }

    /// <summary>Function to handle a drag over event on the render control.</summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>Content editor developers can override this method to handle a drag over event when an item is dragged over the rendering area on the view.</remarks>
    protected override void OnRenderWindowDragOver(DragEventArgs e)
    {
        IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

        if ((contentData is null) || (contentData.FilePaths.Count != 1))
        {
            e.Effect = DragDropEffects.None;
            return;
        }

        var args = new SetTextureArgs(contentData.FilePaths[0]);

        if ((ViewModel?.TextureEditor?.TextureBrush?.LoadTextureCommand is null) || (!ViewModel.TextureEditor.TextureBrush.LoadTextureCommand.CanExecute(args)))
        {
            if (!args.Cancel)
            {
                base.OnRenderWindowDragOver(e);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
            return;
        }

        e.Effect = DragDropEffects.Move;
    }

    /// <summary>Function to handle a drag drop event on the render control.</summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>Content editor developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.</remarks>
    protected async override void OnRenderWindowDragDrop(DragEventArgs e)
    {
        IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

        var args = new SetTextureArgs(contentData.FilePaths[0]);

        if ((ViewModel?.TextureEditor?.TextureBrush?.LoadTextureCommand is not null) && (ViewModel.TextureEditor.TextureBrush.LoadTextureCommand.CanExecute(args)))
        {
            await ViewModel.TextureEditor.TextureBrush.LoadTextureCommand.ExecuteAsync(args);
        }
    }

    /// <summary>Function called when the renderer is switched.</summary>
    /// <param name="renderer">The current renderer.</param>
    /// <param name="resetZoom">
    ///   <b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
    protected override void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
    {
        // If we have a ribbon to merge into the editor ribbon, we should pass in the current renderer.
        _ribbonForm.ContentRenderer = renderer;

        if (renderer is null)
        {
            return;
        }

        // Calling this will reset the panning/zooming to the default for the currently active renderer. The DefaultZoom 
        // method is not defined on the base class for the renderer, so that if users don't require this can skip it.
        if (resetZoom)
        {
            _renderer.DefaultZoom();                
        }
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

        if (!_ribbonForm.IsHandleCreated)
        {
            _ribbonForm.CreateControl();
        }

        ViewModel?.Load();            
        RenderControl?.Select();

        ValidateButtons();
    }

    /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
    protected override void OnShutdown()
    {
        _ants?.Dispose();
        _clipper?.Dispose();

        ViewModel?.Unload();
        base.OnShutdown();
    }

    /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
    /// <param name="context">The graphics context from the host application.</param>
    /// <param name="swapChain">The current swap chain for the rendering panel.</param>
    protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
    {
        _ants = new MarchingAnts(context.Renderer2D);
        _clipper = new RectClipperService(context.Renderer2D, _ants);
        _renderer = new FontRenderer(context.Renderer2D, swapChain, ViewModel);
        TextureRenderer textureRenderer = new(context.Renderer2D, swapChain, ViewModel);
        TextureBrushRenderer brushRenderer = new(context.Renderer2D, swapChain, _clipper, ViewModel);

        // Create any necessary resources for the renderer here.
        _renderer.CreateResources();
        textureRenderer.CreateResources();
        brushRenderer.CreateResources();

        // Any renderers we create must be added to the internal renderer list so they can be managed by 
        // the content system.
        AddRenderer(_renderer.Name, _renderer);
        AddRenderer(textureRenderer.Name, textureRenderer);
        AddRenderer(brushRenderer.Name, brushRenderer);

        // We use this method to change renderers. In this case, we set the only renderer as the default.
        SwitchRenderer(_renderer.Name, true);
    }

    /// <summary>
    /// Function to assign a data context to the view as a view model.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>
    /// <para>
    /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
    /// </para>
    /// </remarks>
    public void SetDataContext(IFontContent dataContext)
    {
        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        _ribbonForm.SetDataContext(dataContext);
        FontOutline.SetDataContext(dataContext?.FontOutline);
        FontTextureSize.SetDataContext(dataContext?.TextureEditor.FontTextureSize);
        FontPadding.SetDataContext(dataContext?.TextureEditor.FontPadding);
        FontCharacterSelection.SetDataContext(dataContext?.FontCharacterSelection);
        FontSolidBrush.SetDataContext(dataContext?.TextureEditor.SolidBrush);
        FontPatternBrush.SetDataContext(dataContext?.TextureEditor.PatternBrush);
        FontGradientBrush.SetDataContext(dataContext?.TextureEditor.GradientBrush);
        FontTextureBrush.SetDataContext(dataContext?.TextureEditor.TextureBrush);
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontContentView"/> class.</summary>
    public FontContentView()
    {
        InitializeComponent();

        _ribbonForm = new FormRibbon();
        Ribbon = _ribbonForm.RibbonTextContent;

        RegisterChildPanel(typeof(FontOutline).FullName, FontOutline);
        RegisterChildPanel(typeof(FontTextureSize).FullName, FontTextureSize);
        RegisterChildPanel(typeof(FontPadding).FullName, FontPadding);
        RegisterChildPanel(typeof(FontCharacterSelection).FullName, FontCharacterSelection);
        RegisterChildPanel(typeof(FontSolidBrush).FullName, FontSolidBrush);
        RegisterChildPanel(typeof(FontPatternBrush).FullName, FontPatternBrush);
        RegisterChildPanel(typeof(FontGradientBrush).FullName, FontGradientBrush);
        RegisterChildPanel(typeof(FontTextureBrush).FullName, FontTextureBrush);
    }
    #endregion
}
