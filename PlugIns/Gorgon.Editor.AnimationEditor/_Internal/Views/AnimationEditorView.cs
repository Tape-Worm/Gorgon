
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
// Created: March 14, 2019 11:33:25 AM
// 


using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The primary view for the animation editor
/// </summary>
internal partial class AnimationEditorView
    : VisualContentBaseControl, IDataContext<IAnimationContent>
{

    // The form for the ribbon.
    private readonly FormRibbon _ribbonForm;
    // The list of tracks in the animation.
    private readonly List<ITrack> _tracks = [];
    // The loader used to load sprites into the animation.
    private ISpriteLoader _spriteLoader;
    // The marching ants interface.
    private IMarchingAnts _marchingAnts;
    // The clipping service.
    private IRectClipperService _clipper;
    // The texture for the anchor icon.
    private GorgonTexture2DView _anchorTexture;
    // The anchor editing service.
    private IAnchorEditService _anchorService;
    // The service to facilitate vertex editing for an animation.
    private VertexEditService _vertexEditorService;



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

    /// <summary>Handles the CollectionChanged event of the Tracks control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ITrack newTrack = (ITrack)e.NewItems[0];
                _tracks.Add(newTrack);
                newTrack.PropertyChanged += Track_PropertyChanged;
                break;
            case NotifyCollectionChangedAction.Remove:
                ITrack oldTrack = (ITrack)e.OldItems[0];
                _tracks.Remove(oldTrack);
                oldTrack.PropertyChanged -= Track_PropertyChanged;
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (ITrack track in _tracks)
                {
                    track.PropertyChanged -= Track_PropertyChanged;
                }
                _tracks.Clear();
                break;
        }

        ValidateButtons();
    }

    /// <summary>Handles the PropertyChanged event of the Track control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    /// <exception cref="NotImplementedException"></exception>
    private void Track_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateButtons();

    /// <summary>Handles the SplitterMoved event of the SplitTrack control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
    private void SplitTrack_SplitterMoved(object sender, SplitterEventArgs e)
    {
        if (ViewModel?.Settings is null)
        {
            return;
        }

        ViewModel.Settings.SplitterOffset = SplitTrack.SplitPosition;
    }

    /// <summary>
    /// Function to validate the controls on the view.
    /// </summary>
    private void ValidateButtons()
    {
        _ribbonForm.ValidateButtons();
        _ribbonForm.ButtonZoomAnimation.Enabled = (Renderer?.CanZoom ?? false);
    }

    /// <summary>
    /// Function to initialize the view from the data context.
    /// </summary>
    /// <param name="dataContext">The data context to use.</param>
    private void InitializeFromDataContext(IAnimationContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        _spriteLoader = dataContext;

        _tracks.Clear();
        _ribbonForm.SetDataContext(dataContext);

        foreach (ITrack track in dataContext.Tracks)
        {
            _tracks.Add(track);
            track.PropertyChanged += Track_PropertyChanged;
        }
    }

    /// <summary>Function called when the view should be reset by a <b>null</b> data context.</summary>
    protected override void ResetDataContext()
    {
        base.ResetDataContext();

        _spriteLoader = null;
        _tracks.Clear();
        Tracks.SetDataContext(null);
    }

    /// <summary>Raises the <see cref="Control.Resize"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if ((IsDesignTime) || (SplitTrack is null))
        {
            return;
        }

        SplitTrack.MinExtra = ClientSize.Height / 2;
        SplitTrack.MinSize = ClientSize.Height / 4;

        if (ViewModel?.Settings is null)
        {
            return;
        }

        SplitTrack.SplitPosition = ViewModel.Settings.SplitterOffset;
    }

    /// <summary>Function to handle a drag enter event on the render control.</summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>Content editor developers can override this method to handle a drag enter event when an item is dragged into the rendering area on the view.</remarks>
    protected override void OnRenderWindowDragOver(DragEventArgs e)
    {
        IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

        e.Effect = DragDropEffects.None;

        if (contentData is null)
        {
            return;
        }

        if ((_spriteLoader?.LoadSpriteCommand is not null) && (_spriteLoader.LoadSpriteCommand.CanExecute(contentData.FilePaths)))
        {
            e.Effect = DragDropEffects.Move;
        }
    }

    /// <summary>Function to handle a drag drop event on the render control.</summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>Content editor developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.</remarks>
    protected override async void OnRenderWindowDragDrop(DragEventArgs e)
    {
        IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

        if ((_spriteLoader?.LoadSpriteCommand is not null) && (_spriteLoader.LoadSpriteCommand.CanExecute(contentData.FilePaths)))
        {
            await _spriteLoader.LoadSpriteCommand.ExecuteAsync(contentData.FilePaths);
        }
    }

    /// <summary>Function called when a property is changing on the data context.</summary>
    /// <param name="propertyName">The name of the property that is updating.</param>
    /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
    protected override void OnPropertyChanging(string propertyName)
    {
        base.OnPropertyChanging(propertyName);

        switch (propertyName)
        {
            case nameof(IAnimationContent.CommandContext):
                ViewModel.CommandContext?.Unload();
                RenderControl.Cursor = Cursor.Current = Cursors.Default;
                ValidateButtons();
                break;
        }
    }

    /// <summary>Function called when a property is changed on the data context.</summary>        
    /// <param name="propertyName">The name of the property that is updated.</param>
    /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        string rendererName;

        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(IAnimationContent.PrimarySprite):
            case nameof(IAnimationContent.CurrentPanel):
            case nameof(IAnimationContent.Selected):
                if ((ViewModel.PrimarySprite is null)
                    || (ViewModel.CommandContext != ViewModel.KeyEditor)
                    || (ViewModel.Selected.Count == 0)
                    || (!HasRenderer(ViewModel.Selected[0].Track.KeyType.ToString())))
                {
                    rendererName = ViewModel.PrimarySprite is not null ? DefaultAnimationViewer.ViewerName : NoPrimarySpriteViewer.ViewerName;

                    if ((!HasRenderer(rendererName)) || (string.Equals(Renderer?.Name, rendererName, StringComparison.OrdinalIgnoreCase)))
                    {
                        break;
                    }
                }
                else
                {
                    rendererName = ViewModel.Selected[0].Track.KeyType.ToString();
                }

                // Only switch if we're not on the same renderer.
                if (!string.Equals(Renderer?.Name, rendererName, StringComparison.OrdinalIgnoreCase))
                {
                    SwitchRenderer(rendererName, false);
                }
                break;
            case nameof(IAnimationContent.CommandContext):
                if (ViewModel.CommandContext is null)
                {
                    SwitchRenderer(ViewModel.PrimarySprite is not null ? DefaultAnimationViewer.ViewerName : NoPrimarySpriteViewer.ViewerName, false);
                    _spriteLoader = ViewModel;
                }
                else
                {
                    ViewModel.CommandContext.Load();
                    _spriteLoader = ViewModel.CommandContext as ISpriteLoader;
                }
                break;
        }
        ValidateButtons();
    }

    /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
    protected override void OnShutdown()
    {
        ViewModel?.Unload();

        _vertexEditorService?.Dispose();
        _anchorTexture?.Dispose();
        _clipper?.Dispose();
        _marchingAnts?.Dispose();

        base.OnShutdown();
    }

    /// <summary>Function called when the renderer is switched.</summary>
    /// <param name="renderer">The current renderer.</param>
    /// <param name="resetZoom"><b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
    protected override void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
    {
        base.OnSwitchRenderer(renderer, resetZoom);
        AnimationViewer viewer = renderer as AnimationViewer;
        _ribbonForm.ContentRenderer = viewer;

        if (viewer is null)
        {
            return;
        }

        viewer.DefaultZoom();

        if (resetZoom)
        {
            _ribbonForm.ResetZoom();
        }
    }

    /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
    /// <param name="context">The graphics context from the host application.</param>
    /// <param name="swapChain">The current swap chain for the rendering panel.</param>
    protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
    {
        base.OnSetupContentRenderer(context, swapChain);

        using MemoryStream stream = CommonEditorResources.MemoryStreamManager.GetStream(Resources.anchor_24x24);
        _anchorTexture = GorgonTexture2DView.FromStream(context.Graphics, stream, new GorgonCodecDds(),
            options: new GorgonTexture2DLoadOptions
            {
                Name = "Animation Editor Anchor Sprite",
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            });

        _marchingAnts = new MarchingAnts(context.Renderer2D);
        _clipper = new RectClipperService(context.Renderer2D, _marchingAnts);
        _anchorService = new AnchorEditService(context.Renderer2D, new GorgonSprite
        {
            Texture = _anchorTexture,
            Size = new DX.Size2F(_anchorTexture.Width, _anchorTexture.Height),
            // Place the hotspot on rope hole at the top of the handle.
            Anchor = new Vector2(0.5f, 0.125f)
        }, new DX.Rectangle
        {
            Left = -context.Graphics.VideoAdapter.MaxTextureWidth / 2,
            Top = -context.Graphics.VideoAdapter.MaxTextureHeight / 2,
            Right = context.Graphics.VideoAdapter.MaxTextureWidth / 2 - 1,
            Bottom = context.Graphics.VideoAdapter.MaxTextureHeight / 2 - 1,
        });
        _vertexEditorService = new VertexEditService(context.Renderer2D);

        NoPrimarySpriteViewer noSprite = new(context.Renderer2D, swapChain, context.FontFactory, ViewModel);
        DefaultAnimationViewer defaultView = new(context.Renderer2D, swapChain, ViewModel, _clipper);
        SingleAnimationViewer singleEditorView = new(context.Renderer2D, swapChain, ViewModel);
        Vector2AnimationViewer vec2EditorView = new(context.Renderer2D, swapChain, ViewModel, _clipper, _anchorService, _vertexEditorService);

        noSprite.CreateResources();
        defaultView.CreateResources();
        singleEditorView.CreateResources();
        vec2EditorView.CreateResources();

        AddRenderer(noSprite.Name, noSprite);
        AddRenderer(defaultView.Name, defaultView);
        AddRenderer(singleEditorView.Name, singleEditorView);
        AddRenderer(vec2EditorView.Name, vec2EditorView);

        SwitchRenderer(ViewModel.PrimarySprite is null ? noSprite.Name : defaultView.Name, true);

        ValidateButtons();
    }

    /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
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
        ShowFocusState(true);
        RenderControl?.Select();

        ValidateButtons();
    }

    /// <summary>Function to unassign events for the data context.</summary>
    protected override void UnassignEvents()
    {
        base.UnassignEvents();

        if (ViewModel is null)
        {
            return;
        }

        foreach (ITrack track in ViewModel.Tracks)
        {
            track.PropertyChanged -= Track_PropertyChanged;
        }

        ViewModel.Tracks.CollectionChanged -= Tracks_CollectionChanged;
        ViewModel.KeyEditor.PropertyChanged -= KeyEditor_PropertyChanged;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IAnimationContent dataContext)
    {
        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);

        Tracks.SetDataContext(dataContext);
        AddTracks.SetDataContext(dataContext?.AddTrack);
        AnimationProperties.SetDataContext(dataContext?.Properties);
        FloatValuesEditor.SetDataContext(dataContext?.KeyEditor?.FloatKeysEditor);
        ColorValuesEditor.SetDataContext(dataContext?.KeyEditor?.ColorKeysEditor);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        foreach (ITrack track in ViewModel.Tracks)
        {
            track.PropertyChanged += Track_PropertyChanged;
        }

        ViewModel.Tracks.CollectionChanged += Tracks_CollectionChanged;
        ViewModel.KeyEditor.PropertyChanged += KeyEditor_PropertyChanged;
    }



    /// <summary>Initializes a new instance of the <see cref="AnimationEditorView"/> class.</summary>
    public AnimationEditorView(AnimationEditorSettings settings)
        : this() => _ribbonForm.Settings = settings;

    /// <summary>Initializes a new instance of the <see cref="AnimationEditorView"/> class.</summary>
    public AnimationEditorView()
    {
        InitializeComponent();

        _ribbonForm = new FormRibbon();
        Ribbon = _ribbonForm.RibbonAnimationContent;

        RegisterChildPanel(typeof(AddTrack).FullName, AddTracks);
        RegisterChildPanel(typeof(AnimProperties).FullName, AnimationProperties);
        RegisterChildPanel(typeof(KeyValueEditor).FullName, FloatValuesEditor);
        RegisterChildPanel(typeof(ColorValueEditor).FullName, ColorValuesEditor);
    }

}
